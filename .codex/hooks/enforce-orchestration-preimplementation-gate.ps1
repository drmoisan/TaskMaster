
<#
.SYNOPSIS
    Blocks implementation operations before orchestration readiness exists.
#>
[CmdletBinding()]
param()

# Shared Codex PreToolUse transport: stdin payload parsing and tool_input-to-file
# mapping for every tool name the ^(apply_patch|Edit|Write)$ matcher admits.
. (Join-Path $PSScriptRoot 'codex-pretooluse-file-mapping.ps1')

# Pure pathspec classifier for the issue #539 orchestration-bookkeeping staging exemption.
# Extracted to a dot-sourced sibling so this file stays inside the 500-line cap, following
# the headroom-split precedent already used on this side by enforce-completion-helpers.ps1.
. (Join-Path $PSScriptRoot 'enforce-orchestration-preimplementation-gate-helpers.ps1')

# Pure mode dispatch and per-mode readiness predicates for issue #554. A new sibling
# rather than an addition to the helpers file above, whose header declares a different
# normative contract and which lacks headroom; leaving that file byte-untouched is the
# proof the issue #539 exemption is behaviourally unchanged.
. (Join-Path $PSScriptRoot 'enforce-orchestration-preimplementation-gate-modes.ps1')
# Shared command-line parser (issue #545): per-segment scan text and structural matching.
. (Join-Path $PSScriptRoot 'hook-command-scanner.ps1')
. (Join-Path $PSScriptRoot 'hook-command-invocation.ps1')

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
    #
    # Idempotence for the apply_patch call site: (^|/) matches at ^, so a
    # repo-relative path harvested from a file marker by Test-ImplementationCommand
    # classifies exactly as it does today.
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

    foreach ($match in [regex]::Matches($normalizedCommand, '(?m)^\*\*\* (?:Add|Update|Delete) File:\s*(?<path>.+?)\s*$')) {
        $path = (([string]$match.Groups['path'].Value).Trim()) -replace '\\', '/'
        if (Test-ImplementationPath -NormalizedPath $path) {
            return $true
        }
    }
    foreach ($match in [regex]::Matches($normalizedCommand, '(?m)^\*\*\* Move to:\s*(?<path>.+?)\s*$')) {
        $path = (([string]$match.Groups['path'].Value).Trim()) -replace '\\', '/'
        if (Test-ImplementationPath -NormalizedPath $path) {
            return $true
        }
    }

    $implementationCommandPatterns = @(
        '(^|\s)git\s+(add|commit)\b',
        '(^|\s)(poetry\s+run\s+)?(black|ruff|pyright|pytest)\b',
        '(^|\s)npm\s+.*\s+(prettier|lint|typecheck|test:unit)\b',
        '(^|\s)npx\s+(prettier|eslint|tsc|jest)\b',
        '(^|\s)pwsh\s+.*(Invoke-Pester|tests/scripts/)'
    )
    $scanText = @(Read-CommandLineSegment -CommandText $normalizedCommand).ScanText
    $isStaging = @('add', 'commit').Where({ Test-CommandLineInvocation -CommandText $normalizedCommand -CommandWord 'git' -SubcommandPath @($_) }).Count -gt 0

    for ($index = 0; $index -lt $implementationCommandPatterns.Count; $index++) {
        if (-not (($scanText | Where-Object { $_ -match $implementationCommandPatterns[$index] }) -or ($index -eq 0 -and $isStaging))) {
            continue
        }
        # Allow-side only (issue #539). Index 0 is the git staging trigger, whose pattern
        # text is unchanged. It is the sole leg the orchestration-bookkeeping exemption may
        # clear, and only when no other implementation pattern matches the same line: the
        # loop continues rather than returning, so a chained line carrying any non-git
        # implementation segment still classifies as implementation. The apply_patch marker
        # legs above are upstream of this loop and are unmodified.
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
        both preparation markers. The prompt is read as a named field via this file's
        own Get-StringProperty helper rather than from the serialized payload, so that
        marker text planted in an unrelated field cannot exempt an implementation
        delegation.
    .OUTPUTS
        System.Boolean
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param([Parameter(Mandatory)][AllowNull()] $ToolInput)

    if ($null -eq $ToolInput) {
        return $false
    }

    $subagentType = Get-StringProperty -Value $ToolInput -Name 'subagent_type'
    if ($subagentType -ne 'orchestrator') {
        return $false
    }

    $prompt = Get-StringProperty -Value $ToolInput -Name 'prompt'
    foreach ($marker in $script:PreparationModeMarkers) {
        if (-not $prompt.Contains($marker)) {
            return $false
        }
    }
    return $true
}

# Classifies an Agent delegation as implementation by STRUCTURE (issue #554). Both
# reads are field-scoped through this file's own Get-StringProperty, and the
# whole-payload serialization scan this function used to perform is removed: it let any
# field, and two ordinary English words, decide the outcome, so marker text planted
# outside 'prompt' changed the classification and rewording a prompt changed the
# decision. Neither can happen now. An allow-listed subagent_type is implementation
# whatever the prompt says; any other non-orchestrator subagent_type is not; an
# orchestrator whose resolved mode is preparation is not, and every other one is.
#
# Get-StringProperty TRIMS where the Claude-side reader does not. Every marker test
# introduced here is a containment test over the prompt, which is insensitive to
# leading and trailing whitespace, so that pre-existing divergence between the two
# surfaces cannot change any decision this change introduces.
function Test-ImplementationDelegation {
    [CmdletBinding()]
    [OutputType([bool])]
    param([Parameter(Mandatory)][AllowNull()] $ToolInput)

    if ($null -eq $ToolInput) {
        return $false
    }

    $subagentType = Get-StringProperty -Value $ToolInput -Name 'subagent_type'
    if (Test-OrchestrationImplementationAgent -SubagentType $subagentType) {
        return $true
    }
    if ($subagentType -ne 'orchestrator') {
        return $false
    }

    $prompt = Get-StringProperty -Value $ToolInput -Name 'prompt'
    return ((Resolve-OrchestrationDelegationMode -Prompt $prompt) -ne 'preparation')
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

# The two per-mode read seams (issue #554). Each takes its path from the fixed mode
# table and never from a delegation's own text; an absent file returns an empty
# string, which the readiness predicate then treats as a deny.
function Get-EpicCheckpointContent {
    [CmdletBinding()]
    [OutputType([string])]
    param()

    $path = Get-OrchestrationDelegationCheckpointPath -Mode 'epic'
    if (-not (Test-Path -LiteralPath $path)) {
        return ''
    }
    return Get-Content -Raw -LiteralPath $path
}

function Get-ParallelCheckpointContent {
    [CmdletBinding()]
    [OutputType([string])]
    param()

    $path = Get-OrchestrationDelegationCheckpointPath -Mode 'parallel'
    if (-not (Test-Path -LiteralPath $path)) {
        return ''
    }
    return Get-Content -Raw -LiteralPath $path
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

# Builds a mode-specific deny reason naming the checkpoint actually consulted and the
# predicate that failed, behind the unchanged PREIMPLEMENTATION_GATE_BLOCKED prefix
# that downstream reason-matching reads.
function Get-OrchestrationModeDenyReason {
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory)][string] $Mode,
        [Parameter(Mandatory)][string] $Failure
    )

    $path = Get-OrchestrationDelegationCheckpointPath -Mode $Mode
    return ("PREIMPLEMENTATION_GATE_BLOCKED: this $Mode-mode delegation was evaluated against " +
        "$path, and the failed readiness predicate is '$Failure'. Implementation operations " +
        'require that checkpoint to satisfy every readiness predicate before implementation begins.')
}

function Invoke-OrchestrationPreimplementationGateDecision {
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param(
        [string] $ToolInputRaw,
        [string] $CheckpointRaw,

        # The two per-mode injection parameters (issue #554, decision D2). Each
        # overrides its read seam whenever the caller BINDS it, decided with
        # ContainsKey and never with a truthiness test, so an explicitly supplied
        # empty string suppresses the seam instead of falling through to disk. The
        # two parameters above keep their existing names, positions, attributes,
        # and truthiness-based fall-through exactly.
        [AllowNull()]
        [AllowEmptyString()]
        [string] $EpicCheckpointRaw,

        [AllowNull()]
        [AllowEmptyString()]
        [string] $ParallelCheckpointRaw
    )

    if (-not $ToolInputRaw) {
        return Get-OrchestrationPreimplementationGateAllowDecision
    }
    try {
        $toolInput = $ToolInputRaw | ConvertFrom-Json -ErrorAction Stop
    } catch {
        throw "enforce-orchestration-preimplementation-gate received malformed mapped tool_input JSON: $_"
    }

    $requiresReadyCheckpoint = $false
    # The path and command legs are single-feature by construction; only the
    # delegation leg carries a mode marker, so only it can move the mode off the
    # default. Both other legs therefore keep the default readiness source and the
    # default wording they have today.
    $mode = $script:OrchestrationDelegationDefaultMode
    $prompt = ''
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
            if ($requiresReadyCheckpoint) {
                $prompt = Get-StringProperty -Value $toolInput -Name 'prompt'
                $mode = Resolve-OrchestrationDelegationMode -Prompt $prompt
            }
        }
    }

    if (-not $requiresReadyCheckpoint) {
        return Get-OrchestrationPreimplementationGateAllowDecision
    }

    # A prompt-declared checkpoint path is a cross-check operand only and never
    # selects a source: a delegation that named its own readiness file would choose
    # its own gate. Disagreement with the mode's canonical path is a deny.
    if (-not (Test-OrchestrationDelegationDeclaredCheckpointPath -Prompt $prompt -Mode $mode)) {
        return Get-OrchestrationPreimplementationGateBlockDecision -Reason (
            Get-OrchestrationModeDenyReason -Mode $mode -Failure 'declared-checkpoint-path')
    }

    if ($mode -eq 'epic' -or $mode -eq 'parallel') {
        $isEpic = ($mode -eq 'epic')
        $injected = if ($isEpic) { 'EpicCheckpointRaw' } else { 'ParallelCheckpointRaw' }
        $modeRaw = if ($PSBoundParameters.ContainsKey($injected)) {
            [string]$PSBoundParameters[$injected]
        } elseif ($isEpic) { Get-EpicCheckpointContent } else { Get-ParallelCheckpointContent }
        try {
            $modeCheckpoint = ConvertFrom-CheckpointJson -Json ([string]$modeRaw)
        } catch { $modeCheckpoint = $null }
        $folder = Find-OrchestrationDelegationTargetFolder -Prompt $prompt
        $issue = Find-OrchestrationDelegationIssueNumber -Prompt $prompt
        $failure = if ($isEpic) {
            Get-EpicOrchestrationReadinessFailure -Checkpoint $modeCheckpoint -TargetFolder $folder -IssueNumber $issue
        } else {
            Get-ParallelOrchestrationReadinessFailure -Checkpoint $modeCheckpoint -TargetFolder $folder -IssueNumber $issue
        }
        if (-not $failure) { return Get-OrchestrationPreimplementationGateAllowDecision }
        return Get-OrchestrationPreimplementationGateBlockDecision -Reason (
            Get-OrchestrationModeDenyReason -Mode $mode -Failure $failure)
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

if ($MyInvocation.InvocationName -eq '.') {
    return
}

try {
    $payload = ConvertFrom-CodexPreToolUsePayload -PayloadRaw ([Console]::In.ReadToEnd()) -HookName 'enforce-orchestration-preimplementation-gate'
    $toolName = [string]$payload.tool_name

    # Bash and apply_patch take the pre-fix path unchanged: the raw tool_input is
    # serialized and evaluated by the untouched decision function, so every
    # allow/deny outcome those two tool names produce today is preserved exactly.
    if (@('Bash', 'apply_patch') -contains $toolName) {
        $decision = Invoke-OrchestrationPreimplementationGateDecision -ToolInputRaw ($payload.tool_input | ConvertTo-Json -Compress -Depth 20)
        if ($decision.hookSpecificOutput.permissionDecision -eq 'deny') {
            $decision | ConvertTo-Json -Compress -Depth 5 | Write-Output
        }
        exit 0
    }

    # Edit and Write map to file paths through the shared module; each mapped
    # path enters the same untouched Test-ImplementationPath decision flow. Any
    # other well-formed tool name maps to no records, so the hook allows.
    foreach ($toolInput in @(ConvertTo-CodexFileEditInput -Payload $payload)) {
        $mappedRaw = @{ file_path = [string]$toolInput.file_path } | ConvertTo-Json -Compress
        $decision = Invoke-OrchestrationPreimplementationGateDecision -ToolInputRaw $mappedRaw
        if ($decision.hookSpecificOutput.permissionDecision -eq 'deny') {
            $decision | ConvertTo-Json -Compress -Depth 5 | Write-Output
            exit 0
        }
    }
    exit 0
} catch {
    [Console]::Error.WriteLine([string]$_)
    exit 2
}
