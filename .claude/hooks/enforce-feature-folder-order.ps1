<#
.SYNOPSIS
    Pre-tool-use hook that blocks writes to a feature folder's plan.md when issue.md,
    spec.md, or user-story.md are not yet present in that same folder.

.DESCRIPTION
    Invoked by the Claude Code PreToolUse hook on Write or Edit operations. Acquires
    the hook payload through the shared reader and reads file_path from the envelope's
    nested tool_input. When the
    target file_path matches a feature-folder plan.md path under
    docs/features/(active|archive)/<folder>/plan.md, the script verifies that
    each of issue.md, spec.md, and user-story.md exists in the same folder.

    If any of the three sibling files is missing, the script emits a PreToolUse
    JSON response with hookSpecificOutput.permissionDecision='deny' and exits 0 so
    Claude Code surfaces the reason. All other paths pass through with
    permissionDecision='allow'.

    Filesystem reads go through Get-FeatureFolderFileExistence so tests can
    inject a fake without touching disk.

.NOTES
    Compatible with PowerShell 7+. Read-only validation gate; no state mutation.
#>
[CmdletBinding()]
param()


Import-Module (Join-Path $PSScriptRoot '../lib/hook-payload/HookPayload.psm1') -Force
function Get-FeatureFolderFileExistence {
    <#
    .SYNOPSIS
        Wrapper around Test-Path for sibling-file existence checks. Tests mock this.
    .PARAMETER Path
        Absolute or workspace-relative file path to check.
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [Parameter(Mandatory)]
        [string] $Path
    )

    return [bool](Test-Path -LiteralPath $Path -PathType Leaf)
}

function Get-FeatureFolderMissingFile {
    <#
    .SYNOPSIS
        Returns the list of required sibling files missing alongside plan.md.
    .PARAMETER PlanFilePath
        Normalized (forward-slash) path to the plan.md target.
    #>
    [CmdletBinding()]
    [OutputType([string[]])]
    param(
        [Parameter(Mandatory)]
        [string] $PlanFilePath
    )

    $folder = $PlanFilePath -replace '/plan\.md$', ''
    $required = @('issue.md', 'spec.md', 'user-story.md')
    [System.Collections.Generic.List[string]] $missing = [System.Collections.Generic.List[string]]::new()

    foreach ($name in $required) {
        $siblingPath = "$folder/$name"
        if (-not (Get-FeatureFolderFileExistence -Path $siblingPath)) {
            $missing.Add($name)
        }
    }

    return [string[]] $missing.ToArray()
}

function Test-IsFeaturePlanPath {
    <#
    .SYNOPSIS
        Returns $true if the normalized path targets a feature-folder plan.md.
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [Parameter(Mandatory)]
        [string] $NormalizedPath
    )

    return $NormalizedPath -match '(^|/)docs/features/(active|archive)/[^/]+/plan\.md$'
}

function Invoke-FeatureFolderOrderDecision {
    <#
    .SYNOPSIS
        Parses the PreToolUse envelope and produces an allow-or-block decision.
    .PARAMETER ToolInputRaw
        The raw JSON hook payload acquired by Read-ClaudeHookRawPayload. An envelope
        anomaly fails closed as a deny; a well-formed tool_input carrying no file_path
        remains an allow.
    #>
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param(
        [AllowNull()]
        [AllowEmptyString()]
        [string] $ToolInputRaw
    )

    $payload = Resolve-ClaudeHookToolInput -Raw $ToolInputRaw
    if (-not $payload.IsValid) {
        return [ordered]@{
            hookSpecificOutput = [ordered]@{
                hookEventName            = 'PreToolUse'
                permissionDecision       = 'deny'
                permissionDecisionReason = 'FEATURE_FOLDER_ORDER_BLOCKED: payload anomaly - ' +
                (Get-ClaudeHookPayloadAnomalyReason -Anomaly $payload.Anomaly) +
                '. The gate fails closed on an envelope it cannot read.'
            }
        }
    }

    $filePath = Get-ClaudeHookToolInputString -ToolInput $payload.Value -Name 'file_path'
    if (-not $filePath) {
        return [ordered]@{ hookSpecificOutput = [ordered]@{ hookEventName = 'PreToolUse'; permissionDecision = 'allow' } }
    }

    $normalized = $filePath -replace '\\', '/'

    if (-not (Test-IsFeaturePlanPath -NormalizedPath $normalized)) {
        return [ordered]@{ hookSpecificOutput = [ordered]@{ hookEventName = 'PreToolUse'; permissionDecision = 'allow' } }
    }

    $missing = Get-FeatureFolderMissingFile -PlanFilePath $normalized
    if ($missing.Count -eq 0) {
        return [ordered]@{ hookSpecificOutput = [ordered]@{ hookEventName = 'PreToolUse'; permissionDecision = 'allow' } }
    }

    $list = ($missing -join ', ')
    return [ordered]@{
        hookSpecificOutput = [ordered]@{
            hookEventName            = 'PreToolUse'
            permissionDecision       = 'deny'
            permissionDecisionReason = "FEATURE_FOLDER_ORDER_BLOCKED: cannot write plan.md before producing prerequisite documents. Missing in feature folder: $list. Invoke the prd-feature subagent to generate the missing file(s) before authoring plan.md."
        }
    }
}

function Invoke-FeatureFolderOrderEntryPoint {
    <#
    .SYNOPSIS
        Runs the hook decision and returns the process exit code.
    .DESCRIPTION
        Acquires the payload through the shared reader unless the caller supplies
        one, emits the compact decision JSON, and returns 0. It never returns 1:
        exit 1 is non-blocking for PreToolUse, so every anomaly is already a deny
        decision by the time control reaches here. The function does not call exit;
        the thin tail converts the returned code into a process exit.
    .PARAMETER ToolInputRaw
        Optional pre-acquired payload text. When omitted the ReadPayload seam runs.
    .PARAMETER ReadPayload
        Seam for payload acquisition, so tests can drive the empty-on-all-transports
        case without touching a console.
    .OUTPUTS
        System.Int32
    #>
    [CmdletBinding()]
    [OutputType([int])]
    param(
        [AllowNull()]
        [AllowEmptyString()]
        [string] $ToolInputRaw,

        [scriptblock] $ReadPayload = { Read-ClaudeHookRawPayload }
    )

    if (-not $PSBoundParameters.ContainsKey('ToolInputRaw')) {
        $ToolInputRaw = [string](& $ReadPayload)
    }

    $decision = Invoke-FeatureFolderOrderDecision -ToolInputRaw $ToolInputRaw
    $decision | ConvertTo-Json -Compress -Depth 5 | Write-Output

    return 0
}

# Guard allows dot-sourcing in tests without executing the entrypoint.
if ($MyInvocation.InvocationName -eq '.') {
    return
}

# The entry point returns its [int] exit code as the last pipeline element and the
# decision JSON before it. `exit (<call>)` would capture BOTH into the exit
# expression and emit nothing, so the decision is written explicitly here first.
$entryPointResult = @(Invoke-FeatureFolderOrderEntryPoint)
if ($entryPointResult.Count -gt 1) {
    $entryPointResult[0..($entryPointResult.Count - 2)] | Write-Output
}

exit ([int]$entryPointResult[-1])