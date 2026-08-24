<#
.SYNOPSIS
    Blocks implementation operations before orchestration readiness exists.
#>
[CmdletBinding()]
param()


Import-Module (Join-Path $PSScriptRoot '../lib/hook-payload/HookPayload.psm1') -Force
$script:CheckpointPath = 'artifacts/orchestration/orchestrator-state.json'

function ConvertFrom-CheckpointJson {
    [CmdletBinding()]
    param([Parameter(Mandatory)][string] $Json)

    return $Json | ConvertFrom-Json -ErrorAction Stop
}

function Get-StringProperty {
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory)][AllowNull()] $Value,
        [Parameter(Mandatory)][string] $Name
    )

    if ($null -eq $Value -or -not ($Value.PSObject.Properties.Name -contains $Name)) {
        return ''
    }
    return ([string]$Value.$Name).Trim()
}

function Test-FeatureDocumentationOrEvidencePath {
    [CmdletBinding()]
    [OutputType([bool])]
    param([Parameter(Mandatory)][string] $NormalizedPath)

    return $NormalizedPath.StartsWith('docs/features/active/')
}

function Test-ImplementationPath {
    [CmdletBinding()]
    [OutputType([bool])]
    param([Parameter(Mandatory)][string] $NormalizedPath)

    if (Test-FeatureDocumentationOrEvidencePath -NormalizedPath $NormalizedPath) {
        return $false
    }
    if ($NormalizedPath -eq $script:CheckpointPath) {
        return $false
    }
    return $NormalizedPath -match '\.(py|ps1|psm1|ts|tsx|js|jsx|cs|json|yml|yaml)$'
}

function Test-ImplementationCommand {
    [CmdletBinding()]
    [OutputType([bool])]
    param([Parameter(Mandatory)][string] $Command)

    $normalizedCommand = $Command.Trim()
    if (-not $normalizedCommand) {
        return $false
    }

    $implementationCommandPatterns = @(
        '(^|\s)git\s+(add|commit)\b',
        '(^|\s)(poetry\s+run\s+)?(black|ruff|pyright|pytest)\b',
        '(^|\s)npm\s+.*\s+(prettier|lint|typecheck|test:unit)\b',
        '(^|\s)npx\s+(prettier|eslint|tsc|jest)\b',
        '(^|\s)pwsh\s+.*(Invoke-Pester|tests/scripts/)'
    )

    foreach ($pattern in $implementationCommandPatterns) {
        if ($normalizedCommand -match $pattern) {
            return $true
        }
    }
    return $false
}

function Test-ImplementationDelegation {
    [CmdletBinding()]
    [OutputType([bool])]
    param([Parameter(Mandatory)][AllowNull()] $ToolInput)

    if ($null -eq $ToolInput) {
        return $false
    }

    $payloadText = ($ToolInput | ConvertTo-Json -Depth 20 -Compress)
    return $payloadText -match '(python-typed-engineer|powershell-typed-engineer|typescript-engineer|csharp-typed-engineer|atomic-executor|implementation|execute)'
}

function Test-OrchestrationReady {
    [CmdletBinding()]
    [OutputType([bool])]
    param([Parameter(Mandatory)][AllowNull()] $Payload)

    if ($null -eq $Payload) {
        return $false
    }
    $issueNum = Get-StringProperty -Value $Payload -Name 'issue-num'
    $featureFolder = Get-StringProperty -Value $Payload -Name 'feature-folder'
    $routeId = Get-StringProperty -Value $Payload -Name 'route_id'
    if (-not $routeId) {
        $routeId = Get-StringProperty -Value $Payload -Name 'path_selected'
    }
    $lifecycleReady = $false
    if ($Payload.PSObject.Properties.Name -contains 'lifecycle_ready') {
        $lifecycleReady = [bool]$Payload.lifecycle_ready
    }

    if (-not $issueNum -or -not $featureFolder -or -not $routeId -or -not $lifecycleReady) {
        return $false
    }

    return (
        $featureFolder.StartsWith('docs/features/active/') -and
        $routeId -and
        $lifecycleReady
    )
}

function Get-CheckpointContent {
    [CmdletBinding()]
    [OutputType([string])]
    param()

    if (-not (Test-Path -LiteralPath $script:CheckpointPath)) {
        return ''
    }
    return Get-Content -Raw -LiteralPath $script:CheckpointPath
}

function Get-OrchestrationPreimplementationGateAllowDecision {
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param()

    return [ordered]@{
        hookSpecificOutput = [ordered]@{
            hookEventName      = 'PreToolUse'
            permissionDecision = 'allow'
        }
    }
}

function Get-OrchestrationPreimplementationGateBlockDecision {
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

function Invoke-OrchestrationPreimplementationGateDecision {
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param(
        [AllowNull()]
        [AllowEmptyString()]
        [string] $ToolInputRaw,

        [string] $CheckpointRaw
    )

    $payload = Resolve-ClaudeHookToolInput -Raw $ToolInputRaw
    if (-not $payload.IsValid) {
        return Get-OrchestrationPreimplementationGateBlockDecision -Reason (
            'PREIMPLEMENTATION_GATE_BLOCKED: payload anomaly - ' +
            (Get-ClaudeHookPayloadAnomalyReason -Anomaly $payload.Anomaly) +
            '. The gate fails closed on an envelope it cannot read.')
    }

    $toolInput = $payload.Value

    $requiresReadyCheckpoint = $false
    $filePath = Get-StringProperty -Value $toolInput -Name 'file_path'
    if ($filePath) {
        $normalized = ([string]$filePath) -replace '\\', '/'
        $requiresReadyCheckpoint = Test-ImplementationPath -NormalizedPath $normalized
    } else {
        $command = Get-StringProperty -Value $toolInput -Name 'command'
        if ($command) {
            $requiresReadyCheckpoint = Test-ImplementationCommand -Command $command
        } else {
            $requiresReadyCheckpoint = Test-ImplementationDelegation -ToolInput $toolInput
        }
    }

    if (-not $requiresReadyCheckpoint) {
        return Get-OrchestrationPreimplementationGateAllowDecision
    }

    if (-not $CheckpointRaw) {
        $CheckpointRaw = Get-CheckpointContent
    }
    try {
        $checkpoint = ConvertFrom-CheckpointJson -Json $CheckpointRaw
    } catch {
        $checkpoint = $null
    }

    if (Test-OrchestrationReady -Payload $checkpoint) {
        return Get-OrchestrationPreimplementationGateAllowDecision
    }
    return Get-OrchestrationPreimplementationGateBlockDecision -Reason 'PREIMPLEMENTATION_GATE_BLOCKED: Implementation operations require artifacts/orchestration/orchestrator-state.json to contain issue number, feature folder, route metadata, lifecycle readiness, and checkpoint state before implementation begins.'
}

function Invoke-OrchestrationPreimplementationGateEntryPoint {
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

    $decision = Invoke-OrchestrationPreimplementationGateDecision -ToolInputRaw $ToolInputRaw
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
$entryPointResult = @(Invoke-OrchestrationPreimplementationGateEntryPoint)
if ($entryPointResult.Count -gt 1) {
    $entryPointResult[0..($entryPointResult.Count - 2)] | Write-Output
}

exit ([int]$entryPointResult[-1])