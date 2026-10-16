<#
.SYNOPSIS
    Epic-scope command and path legs of the Codex preimplementation gate (issue #707).

.DESCRIPTION
    Dot-sourced by enforce-orchestration-preimplementation-gate.ps1. The Codex counterpart
    of the issue #663 Claude sibling, holding two groups of functions:

    - The two per-mode read seams of issue #554 (Get-EpicCheckpointContent and
      Get-ParallelCheckpointContent), relocated verbatim from the gate file so the gate
      stays inside the 500-line cap. Their names and behaviour are unchanged, so tests
      that mock or shadow them by name are unaffected.
    - The epic-scope decision for the command, apply_patch, and path legs. A call is epic
      scope when the effective worktree's HEAD (the -C selector worktree when present,
      otherwise the session root) equals the integration_branch of
      artifacts/orchestration/epic-orchestrator-state.json. In epic scope the call is
      decided by the command-leg readiness predicate, and under issue #663 decision D2 an
      implementation-classified operand is allowed only while a merge is in progress in
      that worktree. Outside epic scope the decision function returns $null and the gate's
      single-feature path runs unchanged.

.NOTES
    PowerShell 7+. Functions only: no StrictMode and no preference assignment, because a
    dot-sourced file changes its caller's scope. Call-time dependencies:
    Split-OrchestrationCommandLine and ConvertTo-OrchestrationCommandToken (helpers file),
    Get-OrchestrationDelegationCheckpointPath (modes file), and the gate's allow and block
    decision constructors Get-OrchestrationPreimplementationGateAllowDecision and
    Get-OrchestrationPreimplementationGateBlockDecision. The resolver and its seams come
    from the resolution sibling dot-sourced below. The per-segment target resolver comes
    from the targets file dot-sourced below (issue #738), which calls
    Read-CommandLineSegment, Get-CommandLineGlobalOption, and Get-CommandLineInvocation,
    loaded by the gate's dot-source of hook-command-invocation.ps1.
    Mirrored byte-identically under extensions/drm-copilot/resources/codex-and-agents-customizations/.
    AUTHORITY: PowerShell-authoritative. The epic readiness predicate has no reference implementation in another language, and this hook starts no interpreter process (issue #707, D15).
#>

. (Join-Path $PSScriptRoot 'enforce-orchestration-preimplementation-gate-epic-resolution.ps1')
. (Join-Path $PSScriptRoot 'enforce-orchestration-preimplementation-gate-targets.ps1')

# An epic manifest lives under the epics tree; the pattern is tested after backslash
# normalisation so either separator spelling is accepted.
$script:EpicScopeManifestPattern = '(^|/)docs/features/epics/'

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

function Get-OrchestrationEpicScopeSelector {
    <#
    .SYNOPSIS
        Returns the value of a leading `git -C <value>` selector, or $null.
    .DESCRIPTION
        Pure. Reads only the first segment of the command line, split and tokenized with
        the helpers-file parser so quoting is handled exactly as the staging exemption
        handles it. An unbalanced command line has no decidable selector and returns $null.
    .PARAMETER Command
        The full command line.
    .OUTPUTS
        System.String
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param([Parameter(Mandatory)][AllowEmptyString()][string] $Command)

    $split = Split-OrchestrationCommandLine -CommandText $Command
    if (-not $split.Balanced -or @($split.Segments).Count -eq 0) {
        return $null
    }
    $token = @(ConvertTo-OrchestrationCommandToken -Segment ([string]@($split.Segments)[0]).Trim())
    if ($token.Count -ge 3 -and $token[0] -ceq 'git' -and $token[1] -ceq '-C' -and $token[2]) {
        return $token[2]
    }
    return $null
}

function Get-EpicCommandLegReadinessFailure {
    <#
    .SYNOPSIS
        Name the first failed conjunct of the epic command-leg readiness shape, or ''.
    .DESCRIPTION
        Conjuncts, in order: checkpoint-absent; route_id; epic_feature_folder (empty);
        epic_manifest_path (empty, or not under docs/features/epics/ after backslash
        normalisation); integration_branch (empty); features (absent or empty);
        merge-in-progress (no merge in progress in the effective worktree, D2). Returns
        the empty string when every conjunct holds, as the issue #663 predicate does.
    .PARAMETER Checkpoint
        The parsed epic checkpoint object, or $null.
    .PARAMETER MergeInProgress
        Whether MERGE_HEAD exists in the effective worktree's git directory.
    .OUTPUTS
        System.String
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [object] $Checkpoint,

        [Parameter(Mandatory = $true)]
        [bool] $MergeInProgress
    )

    if ($null -eq $Checkpoint) {
        return 'checkpoint-absent'
    }
    if ([string](Get-EpicScopePropertyValue -InputObject $Checkpoint -Name 'route_id') -cne $script:EpicScopeRouteId) {
        return 'route_id'
    }
    if ([string]::IsNullOrWhiteSpace([string](Get-EpicScopePropertyValue -InputObject $Checkpoint -Name 'epic_feature_folder'))) {
        return 'epic_feature_folder'
    }
    $manifestPath = ([string](Get-EpicScopePropertyValue -InputObject $Checkpoint -Name 'epic_manifest_path')) -replace '\\', '/'
    if ([string]::IsNullOrWhiteSpace($manifestPath) -or $manifestPath -notmatch $script:EpicScopeManifestPattern) {
        return 'epic_manifest_path'
    }
    if ([string]::IsNullOrWhiteSpace([string](Get-EpicScopePropertyValue -InputObject $Checkpoint -Name 'integration_branch'))) {
        return 'integration_branch'
    }
    # An empty features array unrolls to $null through the property read.
    $features = Get-EpicScopePropertyValue -InputObject $Checkpoint -Name 'features'
    if ($null -eq $features -or @($features).Count -eq 0) {
        return 'features'
    }
    if (-not $MergeInProgress) {
        return 'merge-in-progress'
    }
    return ''
}

function Get-OrchestrationEpicScopeDecision {
    <#
    .SYNOPSIS
        Returns the epic-scope decision for an implementation-classified command or path, or $null.
    .DESCRIPTION
        Resolves the target of every command segment, apply_patch file marker, and path
        (issue #738), then the epic scope of the session root and of each target. A command
        whose text is an apply_patch body is decided as a path leg over its marker paths.
        When neither is epic scope, returns $null so the caller's single-feature path runs
        unchanged. In epic scope, denies an unresolvable, ambiguous, or out-of-scope target,
        and otherwise evaluates the epic command-leg readiness predicate against every
        target: the first failure denies (the session-root wording is unchanged), and no
        failure allows.
    .PARAMETER Command
        The command line of a command or apply_patch leg; empty for a path leg.
    .PARAMETER FilePath
        The file path of a path leg; empty for a command leg.
    .OUTPUTS
        System.Collections.Specialized.OrderedDictionary, or $null outside epic scope.
    #>
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param(
        [AllowNull()][AllowEmptyString()][string] $Command = '',
        [AllowNull()][AllowEmptyString()][string] $FilePath = ''
    )

    # A call carrying neither a command nor a path has no leg to decide.
    if (-not $Command -and -not $FilePath) {
        return $null
    }
    $epicScopeSessionRoot = (Get-Location).Path
    $legCommand = [string]$Command
    $legPaths = @($FilePath)
    $markerPaths = @(Get-OrchestrationPatchMarkerPath -PatchText $legCommand)
    if ($markerPaths.Count -gt 0) {
        $legCommand = ''
        $legPaths = $markerPaths
    }
    $targetResult = Get-OrchestrationCommandTarget -Command $legCommand -FilePath $legPaths -SessionRoot $epicScopeSessionRoot
    $scopeResolver = {
        param([string] $Selector)
        Resolve-EpicScopeCheckpoint -SessionRoot $epicScopeSessionRoot -WorktreeSelector $Selector
    }
    $verdict = Resolve-OrchestrationEpicTargetVerdict -SessionRoot $epicScopeSessionRoot -TargetResult $targetResult -ScopeResolver $scopeResolver
    $outcome = Resolve-OrchestrationEpicTargetOutcome -Verdict $verdict -ReadinessFailure {
        param($Scope)
        Get-EpicCommandLegReadinessFailure -Checkpoint $Scope.Checkpoint -MergeInProgress $Scope.MergeInProgress
    }
    if ($outcome.Outcome -eq 'none') {
        return $null
    }
    if ($outcome.Outcome -eq 'deny') {
        return Get-OrchestrationPreimplementationGateBlockDecision -Reason $outcome.Reason
    }
    return Get-OrchestrationPreimplementationGateAllowDecision
}
