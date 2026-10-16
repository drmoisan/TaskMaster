<#
.SYNOPSIS
    Pre-tool-use hook that blocks writes to a feature folder's plan file until the
    prerequisite documents its persisted work mode requires exist in that same folder.

.DESCRIPTION
    Invoked by the Claude Code PreToolUse hook on Write or Edit operations. Acquires
    the hook payload through the shared reader and reads file_path from the envelope's
    nested tool_input. The gate applies to a feature-folder plan file under
    docs/features/(active|archive)/<folder>/: a literal plan.md or a timestamped
    plan.<yyyy-MM-ddTHH-mm>.md (issue #568). Other paths pass through with
    permissionDecision='allow'.

    For a plan file, the script reads the folder's issue.md through
    Get-FeatureFolderIssueContent, resolves the persisted '- Work Mode:' marker with the
    shared Resolve-FeatureFolderWorkMode, and takes the required set from the shared
    Get-FeatureFolderPlanPrerequisite:
      - minor-audit                  -> issue.md
      - full-bug                     -> issue.md, spec.md
      - full-feature and legacy full -> issue.md, spec.md, user-story.md
    A missing, empty, malformed, or unrecognized marker, or an unreadable issue.md,
    fails closed to the full-feature set. Because issue.md is in every set, an absent
    issue.md always denies.

    If any required file is missing, the script emits a PreToolUse JSON response with
    hookSpecificOutput.permissionDecision='deny' whose reason names the plan file, the
    resolved work mode, and the missing files, and exits 0 so Claude Code surfaces it.

    Filesystem reads go through Get-FeatureFolderFileExistence and
    Get-FeatureFolderIssueContent so tests can inject fakes without touching disk.

.NOTES
    Compatible with PowerShell 7+. Read-only validation gate; no state mutation. The pure
    sibling feature-folder-resolution.ps1 (issue #565) is dot-sourced inside a guard: when
    it cannot be loaded, every plan-file write denies and every other path stays allowed.
#>
[CmdletBinding()]
param()
$script:HookDependencyGuardLoadFailed = $false; try { . (Join-Path $PSScriptRoot 'hook-dependency-guard.ps1') } catch { $script:HookDependencyGuardLoadFailed = $true }


try { Import-Module (Join-Path $PSScriptRoot '../lib/hook-payload/HookPayload.psm1') -Force -ErrorAction Stop } catch { Add-HookDependencyFailure -Name 'HookPayload.psm1' -ErrorRecord $_ }

# Shared work-mode parser and prerequisite map (issue #565). Guarded so a failed
# dot-source denies plan writes rather than failing open.
$script:FeatureFolderOrderResolutionImportFailure = $null
try {
    . (Join-Path $PSScriptRoot 'feature-folder-resolution.ps1')
}
catch {
    $script:FeatureFolderOrderResolutionImportFailure = 'feature-folder-resolution.ps1'
}

# The plan leaf a gated path ends in: plan.md or plan.<yyyy-MM-ddTHH-mm>.md (issue #568).
$script:FeaturePlanLeafPattern = '/plan(\.\d{4}-\d{2}-\d{2}T\d{2}-\d{2})?\.md$'

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

function Get-FeatureFolderIssueContent {
    <#
    .SYNOPSIS
        Reads a feature folder's issue.md, or returns $null when the file is absent or
        cannot be read. Tests mock this seam; a $null result fails closed to the
        full-feature prerequisite set.
    .PARAMETER FeatureFolder
        Normalized (forward-slash) feature-folder path.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory)]
        [string] $FeatureFolder
    )

    $issuePath = "$FeatureFolder/issue.md"
    if (-not (Test-Path -LiteralPath $issuePath -PathType Leaf)) {
        return $null
    }

    try {
        return Get-Content -LiteralPath $issuePath -Raw -ErrorAction Stop
    }
    catch {
        return $null
    }
}

function Get-FeatureFolderMissingFile {
    <#
    .SYNOPSIS
        Returns the list of required sibling files missing alongside the plan file.
    .PARAMETER PlanFilePath
        Normalized (forward-slash) path to the plan.md or plan.<timestamp>.md target.
    .PARAMETER RequiredFile
        The prerequisite file names to probe. Defaults to the full-feature set.
    #>
    [CmdletBinding()]
    [OutputType([string[]])]
    param(
        [Parameter(Mandatory)]
        [string] $PlanFilePath,

        [string[]] $RequiredFile = @('issue.md', 'spec.md', 'user-story.md')
    )

    $folder = $PlanFilePath -replace $script:FeaturePlanLeafPattern, ''
    [System.Collections.Generic.List[string]] $missing = [System.Collections.Generic.List[string]]::new()

    foreach ($name in $RequiredFile) {
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
        Returns $true if the normalized path targets a feature-folder plan.md or a
        timestamped plan.<yyyy-MM-ddTHH-mm>.md directly inside an active or archive
        feature folder.
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [Parameter(Mandatory)]
        [string] $NormalizedPath
    )

    return $NormalizedPath -match '(^|/)docs/features/(active|archive)/[^/]+/plan(\.\d{4}-\d{2}-\d{2}T\d{2}-\d{2})?\.md$'
}

function Get-FeatureFolderOrderDenyDecision {
    <#
    .SYNOPSIS
        Builds the PreToolUse deny decision carrying the given reason.
    .PARAMETER Reason
        The deny reason, beginning with FEATURE_FOLDER_ORDER_BLOCKED:.
    #>
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param(
        [Parameter(Mandatory)]
        [string] $Reason
    )

    return [ordered]@{
        hookSpecificOutput = [ordered]@{
            hookEventName            = 'PreToolUse'
            permissionDecision       = 'deny'
            permissionDecisionReason = $Reason
        }
    }
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
    $dependencyDecision = Get-HookDependencyFailureDecision -HookEvent PreToolUse -ReasonPrefix 'FEATURE_FOLDER_ORDER_BLOCKED:'
    if ($null -ne $dependencyDecision) { return $dependencyDecision }

    $payload = Resolve-ClaudeHookToolInput -Raw $ToolInputRaw
    if (-not $payload.IsValid) {
        return Get-FeatureFolderOrderDenyDecision -Reason (
            'FEATURE_FOLDER_ORDER_BLOCKED: payload anomaly - ' +
            (Get-ClaudeHookPayloadAnomalyReason -Anomaly $payload.Anomaly) +
            '. The gate fails closed on an envelope it cannot read.')
    }

    $filePath = Get-ClaudeHookToolInputString -ToolInput $payload.Value -Name 'file_path'
    if (-not $filePath) {
        return [ordered]@{ hookSpecificOutput = [ordered]@{ hookEventName = 'PreToolUse'; permissionDecision = 'allow' } }
    }

    $normalized = $filePath -replace '\\', '/'

    if (-not (Test-IsFeaturePlanPath -NormalizedPath $normalized)) {
        return [ordered]@{ hookSpecificOutput = [ordered]@{ hookEventName = 'PreToolUse'; permissionDecision = 'allow' } }
    }

    $planLeaf = ($normalized -split '/')[-1]
    if ($script:FeatureFolderOrderResolutionImportFailure) {
        return Get-FeatureFolderOrderDenyDecision -Reason (
            "FEATURE_FOLDER_ORDER_BLOCKED: the shared resolver '$($script:FeatureFolderOrderResolutionImportFailure)' " +
            "failed to load, so the prerequisite documents for $planLeaf cannot be determined; the gate fails closed.")
    }

    $folder = $normalized -replace $script:FeaturePlanLeafPattern, ''
    $workMode = Resolve-FeatureFolderWorkMode -IssueContent (Get-FeatureFolderIssueContent -FeatureFolder $folder)
    $required = @(Get-FeatureFolderPlanPrerequisite -WorkMode $workMode)
    $missing = @(Get-FeatureFolderMissingFile -PlanFilePath $normalized -RequiredFile $required)
    if ($missing.Count -eq 0) {
        return [ordered]@{ hookSpecificOutput = [ordered]@{ hookEventName = 'PreToolUse'; permissionDecision = 'allow' } }
    }

    $list = ($missing -join ', ')
    return Get-FeatureFolderOrderDenyDecision -Reason (
        "FEATURE_FOLDER_ORDER_BLOCKED: cannot write $planLeaf before producing prerequisite documents for work mode '$workMode'. " +
        "Missing in feature folder: $list. Invoke the prd-feature subagent to generate the missing file(s) before authoring the plan.")
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
if ($script:HookDependencyGuardLoadFailed) { [Console]::Error.WriteLine('FEATURE_FOLDER_ORDER_BLOCKED: hook-dependency-guard.ps1 failed to load; the gate fails closed.'); exit 2 }
$dependencyDecision = Get-HookDependencyFailureDecision -HookEvent PreToolUse -ReasonPrefix 'FEATURE_FOLDER_ORDER_BLOCKED:'
if ($null -ne $dependencyDecision) { $dependencyDecision | ConvertTo-Json -Compress -Depth 5 | Write-Output; exit 0 }

# The entry point returns its [int] exit code as the last pipeline element and the
# decision JSON before it. `exit (<call>)` would capture BOTH into the exit
# expression and emit nothing, so the decision is written explicitly here first.
$entryPointResult = @(Invoke-FeatureFolderOrderEntryPoint)
if ($entryPointResult.Count -gt 1) {
    $entryPointResult[0..($entryPointResult.Count - 2)] | Write-Output
}

exit ([int]$entryPointResult[-1])