<#
.SYNOPSIS
    Epic-scope legs, checkpoint read seams, and target resolution of the preimplementation gate.

.DESCRIPTION
    Dot-sourced by enforce-orchestration-preimplementation-gate.ps1. Holds four groups of
    functions:

    - The import guards for the worktree-resolution modules (issue #690). A failed import
      is recorded in $script:HookDependencyFailures, and the gate's call to
      Get-HookDependencyFailureDecision turns that record into a deny, so a
      missing module cannot make the gate exit non-zero and fail open.
    - The three checkpoint read seams, Get-CheckpointContent, Get-EpicCheckpointContent,
      and Get-ParallelCheckpointContent. Each takes a mandatory absolute path, so no
      read binds to the calling process's directory. Get-CheckpointContent and
      Get-OrchestrationModeDenyReason were relocated here from the gate file by issue #690
      to keep the gate inside the 500-line cap.
    - The target resolution of issue #690. Resolve-OrchestrationGateTarget selects the
      worktree whose checkpoint governs the call from portable identity only: the
      integration_branch: value for an epic delegation, the parallel_slug: value for a
      parallel delegation, the file_path or git -C operand for the path and command legs,
      and the canonical issue-number line and branch: label for a single-feature
      delegation. Read-OrchestrationGateCheckpoint reads the checkpoint beneath the
      resolved root, or returns the deny for an unresolved target.
    - The epic-scope decision for the command and path legs (issue #663). A staging
      command or a Write/Edit call is epic scope when the effective worktree's HEAD equals
      the integration branch of the epic checkpoint that Resolve-EpicScopeCheckpoint
      locates. Outside epic scope the decision function returns $null and the gate's
      single-feature path runs.

.NOTES
    PowerShell 7+. Depends on functions the gate defines or dot-sources before any call:
    Split-OrchestrationCommandLine and ConvertTo-OrchestrationCommandToken (helpers file),
    Get-OrchestrationDelegationCheckpointPath (modes file), the gate's allow and block
    decision constructors, and, through the targets file this file dot-sources (issue #738),
    Read-CommandLineSegment, Get-CommandLineGlobalOption, and Get-CommandLineInvocation
    (loaded by the gate's dot-source of hook-command-invocation.ps1). Imports
    WorktreeItemResolution.psm1 and WorktreeRunResolution.psm1 before
    EpicScopeResolution.psm1, so the nested import inside EpicScopeResolution reuses the
    same module instance. Mirrored byte-identically under
    extensions/drm-copilot/resources/claude-customizations/.
#>

try { Import-Module (Join-Path $PSScriptRoot '../lib/worktree-resolution/WorktreeItemResolution.psm1') -Force -ErrorAction Stop } catch { Add-HookDependencyFailure -Name 'WorktreeItemResolution.psm1' -ErrorRecord $_ }
try { Import-Module (Join-Path $PSScriptRoot '../lib/worktree-resolution/WorktreeRunResolution.psm1') -Force -ErrorAction Stop } catch { Add-HookDependencyFailure -Name 'WorktreeRunResolution.psm1' -ErrorRecord $_ }
try { Import-Module (Join-Path $PSScriptRoot '../lib/worktree-resolution/EpicScopeResolution.psm1') -Force -ErrorAction Stop } catch { Add-HookDependencyFailure -Name 'EpicScopeResolution.psm1' -ErrorRecord $_ }
try { Import-Module (Join-Path $PSScriptRoot '../lib/worktree-resolution/EpicScopeReadiness.psm1') -Force -ErrorAction Stop } catch { Add-HookDependencyFailure -Name 'EpicScopeReadiness.psm1' -ErrorRecord $_ }
# Per-segment target resolution for the epic-scope decision (issue #738).
. (Join-Path $PSScriptRoot 'enforce-orchestration-preimplementation-gate-targets.ps1')

# The three checkpoint read seams. Each takes the absolute path composed beneath a
# resolved worktree root; an absent file returns an empty string, which the readiness
# predicate then treats as a deny.
function Get-CheckpointContent {
    [CmdletBinding()]
    [OutputType([string])]
    param([Parameter(Mandatory)][ValidatePattern('^([A-Za-z]:[\\/]|/)')][string] $Path)

    if (-not (Test-Path -LiteralPath $Path)) {
        return ''
    }
    return Get-Content -Raw -LiteralPath $Path
}

function Get-EpicCheckpointContent {
    [CmdletBinding()]
    [OutputType([string])]
    param([Parameter(Mandatory)][ValidatePattern('^([A-Za-z]:[\\/]|/)')][string] $Path)

    if (-not (Test-Path -LiteralPath $Path)) {
        return ''
    }
    return Get-Content -Raw -LiteralPath $Path
}

function Get-ParallelCheckpointContent {
    [CmdletBinding()]
    [OutputType([string])]
    param([Parameter(Mandatory)][ValidatePattern('^([A-Za-z]:[\\/]|/)')][string] $Path)

    if (-not (Test-Path -LiteralPath $Path)) {
        return ''
    }
    return Get-Content -Raw -LiteralPath $Path
}

# Builds a mode-specific deny reason naming the checkpoint actually consulted and the
# predicate that failed, behind the unchanged PREIMPLEMENTATION_GATE_BLOCKED prefix
# that downstream reason-matching reads. A resolved absolute path is named when known.
function Get-OrchestrationModeDenyReason {
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory)][string] $Mode,
        [Parameter(Mandatory)][string] $Failure,
        [AllowNull()][AllowEmptyString()][string] $CheckpointPath = ''
    )

    $path = if ($CheckpointPath) { $CheckpointPath } else { Get-OrchestrationDelegationCheckpointPath -Mode $Mode }
    return ("PREIMPLEMENTATION_GATE_BLOCKED: this $Mode-mode delegation was evaluated against " +
        "$path, and the failed readiness predicate is '$Failure'. Implementation operations " +
        'require that checkpoint to satisfy every readiness predicate before implementation begins.')
}

function Resolve-OrchestrationGateTarget {
    <#
    .SYNOPSIS
        Resolves the worktree whose checkpoint governs a gated call (issue #690 seam).
    .DESCRIPTION
        An epic delegation is keyed on integration_branch: (with epic_feature_folder: as a
        cross-check), a parallel delegation on parallel_slug:, a path leg on its file_path
        operand, a command leg on its git -C selector (none means the session root), and a
        single-feature delegation on its canonical issue-number line and branch: label.
    .OUTPUTS
        System.Management.Automation.PSCustomObject (the worktree-resolution target result).
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [Parameter(Mandatory)][string] $Mode,
        [AllowNull()][AllowEmptyString()][string] $Prompt = '',
        [AllowNull()][AllowEmptyString()][string] $FilePath = '',
        [AllowNull()][AllowEmptyString()][string] $Command = ''
    )

    $sessionRoot = (Get-Location).Path
    if ($Mode -eq 'epic' -or $Mode -eq 'parallel') {
        $signal = Find-WorktreeRunIdentitySignal -Text $Prompt
        if ($Mode -eq 'epic') {
            return Resolve-WorktreeEpicTarget -IntegrationBranch $signal.IntegrationBranch -EpicSlug $signal.EpicSlug -SessionRoot $sessionRoot
        }
        return Resolve-WorktreeParallelTarget -ParallelSlug $signal.ParallelSlug -SessionRoot $sessionRoot
    }
    if ($FilePath) {
        return Resolve-WorktreeOperandTarget -Path $FilePath -SessionRoot $sessionRoot
    }
    if ($Command) {
        return Resolve-WorktreeOperandTarget -Path (Get-OrchestrationEpicScopeSelector -Command $Command) -SessionRoot $sessionRoot
    }
    return Resolve-WorktreeItemTarget -Text $Prompt -SessionRoot $sessionRoot
}

function Read-OrchestrationGateCheckpoint {
    <#
    .SYNOPSIS
        Reads the checkpoint beneath the resolved target worktree, or returns the deny reason.
    .DESCRIPTION
        Resolved is $false, with DenyReason set, when the target is NoTarget or Ambiguous.
        Otherwise Raw holds the checkpoint text (empty when absent) and Path its absolute
        path. Only Status, WorktreeRoot, ReasonCode, and Detail are read from the target.
    .OUTPUTS
        System.Management.Automation.PSCustomObject
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [Parameter(Mandatory)][string] $Mode,
        [AllowNull()][AllowEmptyString()][string] $Prompt = '',
        [AllowNull()][AllowEmptyString()][string] $FilePath = '',
        [AllowNull()][AllowEmptyString()][string] $Command = ''
    )

    $target = Resolve-OrchestrationGateTarget -Mode $Mode -Prompt $Prompt -FilePath $FilePath -Command $Command
    if ($target.Status -eq 'NoTarget' -or $target.Status -eq 'Ambiguous') {
        return [pscustomobject]@{
            Resolved   = $false
            Raw        = ''
            Path       = $null
            DenyReason = ("PREIMPLEMENTATION_GATE_BLOCKED: $($target.ReasonCode): the target worktree of this $Mode call " +
                "could not be resolved: $($target.Detail). Implementation operations require an identifiable target " +
                'worktree whose checkpoint is ready.')
        }
    }

    $kind = if ($Mode -eq 'epic' -or $Mode -eq 'parallel') { $Mode } else { 'item' }
    $path = Get-WorktreeRunCheckpointPath -Kind $kind -WorktreeRoot $target.WorktreeRoot
    $raw = switch ($kind) {
        'epic' { Get-EpicCheckpointContent -Path $path }
        'parallel' { Get-ParallelCheckpointContent -Path $path }
        default { Get-CheckpointContent -Path $path }
    }
    return [pscustomobject]@{
        Resolved   = $true
        Raw        = [string]$raw
        Path       = $path
        DenyReason = $null
    }
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

function Get-OrchestrationEpicScopeDecision {
    <#
    .SYNOPSIS
        Returns the epic-scope decision for an implementation-classified command or path, or $null.
    .DESCRIPTION
        Resolves the target of every command segment and path (issue #738) with
        Get-OrchestrationCommandTarget, then the epic scope of the session root and of each
        target with worktree-HEAD matching. When neither is epic scope, returns $null so the
        caller's single-feature path runs unchanged. In epic scope, denies an unresolvable,
        ambiguous, or out-of-scope target, and otherwise evaluates the epic command-leg
        readiness predicate against every target: the first failure denies (the session-root
        wording is unchanged), and no failure allows.
    .PARAMETER Command
        The Bash command line; empty for a path leg.
    .PARAMETER FilePath
        The Write/Edit file path; empty for a command leg.
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
    $epicScopeText = [string]$Command
    $targetResult = Get-OrchestrationCommandTarget -Command $epicScopeText -FilePath @($FilePath) -SessionRoot $epicScopeSessionRoot
    $scopeResolver = {
        param([string] $Selector)
        Resolve-EpicScopeCheckpoint -Text $epicScopeText -SessionRoot $epicScopeSessionRoot -WorktreeSelector $Selector -MatchWorktreeHead
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
