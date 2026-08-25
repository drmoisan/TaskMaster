<#
.SYNOPSIS
    Blocks implementation operations before orchestration readiness exists.
#>
[CmdletBinding()]
param()


Import-Module (Join-Path $PSScriptRoot '../lib/hook-payload/HookPayload.psm1') -Force

# Pure pathspec classifier for the issue #539 orchestration-bookkeeping staging exemption.
# Extracted to a dot-sourced sibling so this file stays inside the 500-line cap, following
# the enforce-pr-author-skill.ps1 headroom-split precedent.
. (Join-Path $PSScriptRoot 'enforce-orchestration-preimplementation-gate-helpers.ps1')

# The readiness checkpoint this gate reads and names in its block message.
$script:CheckpointPath = 'artifacts/orchestration/orchestrator-state.json'

# Every orchestration checkpoint a planner or orchestrator surface writes. Writing one
# of these is orchestration bookkeeping, not implementation, so the gate must not
# require a ready checkpoint before the checkpoint itself can be created. The set is a
# list of repo-relative literals behind a single membership check: no directory prefix,
# no glob, and no absolute-path entry.
$script:CheckpointPaths = @(
    'artifacts/orchestration/orchestrator-state.json'
    'artifacts/orchestration/parallel-planner-state.json'
    'artifacts/orchestration/parallel-orchestrator-state.json'
    'artifacts/orchestration/epic-planner-state.json'
    'artifacts/orchestration/epic-orchestrator-state.json'
    'artifacts/orchestration/powershell-orchestrator-state.json'
    'artifacts/orchestration/csharp-orchestrator-state.json'
)

# Both markers must appear in the field-scoped prompt for a delegation to qualify as a
# preparation-mode kickoff. The literals are reused verbatim from
# .claude/skills/parallel-plan/SKILL.md and .claude/skills/epic-plan/SKILL.md.
$script:PreparationModeMarkers = @(
    'Preparation mode: true.'
    'route_id: preparation.'
)

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

    # The segment anchor (^|/) admits both the repo-relative spelling and an
    # absolute spelling of the same feature document, which the Write tool
    # supplies by contract. -cmatch is deliberate and must not be normalized into
    # -match: String.StartsWith is case-sensitive, so the case-sensitive operator
    # is what preserves the previous semantics exactly. PowerShell -match is
    # case-insensitive and would widen this predicate.
    return $NormalizedPath -cmatch '(^|/)docs/features/active/'
}

function Test-ImplementationPath {
    [CmdletBinding()]
    [OutputType([bool])]
    param([Parameter(Mandatory)][string] $NormalizedPath)

    if (Test-FeatureDocumentationOrEvidencePath -NormalizedPath $NormalizedPath) {
        return $false
    }
    # Segment-anchored and end-anchored, so an absolute spelling of a checkpoint is
    # exempt exactly as its repo-relative spelling already was. -match is
    # deliberate here and must not be narrowed into -cmatch: -contains was
    # case-insensitive, so the case-insensitive operator is what preserves the
    # previous semantics exactly.
    #
    # Accepted widening: this also exempts a path OUTSIDE the workspace whose tail
    # is an artifacts/orchestration/ segment followed by one of the seven names.
    # Measured exposure in this repository is one matching file, the real
    # checkpoint; there is no nested or vendored second copy. The same widening is
    # already accepted for the identical literal in four other hooks. Resolving a
    # workspace root instead would reintroduce every root-resolution failure mode
    # (8.3 short names, drive-letter case, symlinks, linked worktrees), and a strip
    # that failed to match would leave the path absolute and deny, reinstating the
    # reported defect in a subtler form.
    #
    # Known deliberate miss: a path reaching a checkpoint name only through a '..'
    # hop stays denied. The Write tool does not emit '..' segments, and a
    # canonicalizer would reintroduce filesystem dependence for no measured gain.
    foreach ($checkpoint in $script:CheckpointPaths) {
        if ($NormalizedPath -match ('(^|/)' + [regex]::Escape($checkpoint) + '$')) {
            return $false
        }
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

    for ($index = 0; $index -lt $implementationCommandPatterns.Count; $index++) {
        if ($normalizedCommand -notmatch $implementationCommandPatterns[$index]) {
            continue
        }
        # Allow-side only (issue #539). Index 0 is the git staging trigger, whose pattern
        # text is unchanged. It is the sole leg the orchestration-bookkeeping exemption may
        # clear, and only when no other implementation pattern matches the same line: the
        # loop continues rather than returning, so a chained line carrying any non-git
        # implementation segment still classifies as implementation.
        if ($index -eq 0 -and (Test-ExemptOrchestrationStagingCommand -CommandText $normalizedCommand)) {
            continue
        }
        return $true
    }
    return $false
}

function Test-PreparationModeDelegation {
    <#
    .SYNOPSIS
        Identifies an orchestrator delegation that is a preparation-mode kickoff.
    .DESCRIPTION
        Returns true only when all three conjuncts hold: the payload is present, the
        delegated agent is exactly 'orchestrator', and the field-scoped prompt carries
        both preparation markers. The prompt is read as a named field rather than from
        the serialized payload so that marker text planted in an unrelated field cannot
        exempt an implementation delegation.
    .OUTPUTS
        System.Boolean
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param([Parameter(Mandatory)][AllowNull()] $ToolInput)

    if ($null -eq $ToolInput) {
        return $false
    }

    $subagentType = Get-ClaudeHookToolInputString -ToolInput $ToolInput -Name 'subagent_type'
    if ($subagentType -ne 'orchestrator') {
        return $false
    }

    $prompt = Get-ClaudeHookToolInputString -ToolInput $ToolInput -Name 'prompt'
    foreach ($marker in $script:PreparationModeMarkers) {
        if (-not $prompt.Contains($marker)) {
            return $false
        }
    }
    return $true
}

function Test-ImplementationDelegation {
    [CmdletBinding()]
    [OutputType([bool])]
    param([Parameter(Mandatory)][AllowNull()] $ToolInput)

    if ($null -eq $ToolInput) {
        return $false
    }

    try {
        if (Test-PreparationModeDelegation -ToolInput $ToolInput) {
            return $false
        }
    } catch {
        # An envelope the field reader cannot probe falls through to the unchanged
        # whole-payload regex below. An extraction failure must never become an
        # exemption, so the gate stays closed on the stricter classifier.
        Write-Debug "Preparation-mode probe failed: $($_.Exception.Message)"
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