<#
.SYNOPSIS
    Epic-scope command and path legs of the preimplementation gate (issue #663).

.DESCRIPTION
    Dot-sourced by enforce-orchestration-preimplementation-gate.ps1. Holds two groups of
    functions:

    - The two per-mode read seams of issue #554 (Get-EpicCheckpointContent and
      Get-ParallelCheckpointContent), relocated verbatim from the gate file by issue #663
      so the gate stays inside the 500-line cap. Their names and behaviour are unchanged,
      so tests that mock or shadow them by name are unaffected.
    - The epic-scope decision for the command and path legs. A staging command or a
      Write/Edit call is epic scope when the effective worktree's HEAD (the -C selector
      worktree when present, otherwise the session root) equals the integration_branch of
      artifacts/orchestration/epic-orchestrator-state.json. In epic scope the call is
      decided by the epic command-leg readiness predicate, and under decision D2 an
      implementation-classified operand is allowed only while a merge is in progress in
      that worktree. Outside epic scope the decision function returns $null and the gate's
      single-feature path runs unchanged.

.NOTES
    PowerShell 7+. Depends on functions the gate defines or dot-sources before any call:
    Split-OrchestrationCommandLine and ConvertTo-OrchestrationCommandToken (helpers file),
    Get-OrchestrationDelegationCheckpointPath (modes file), and the gate's allow and block
    decision constructors. Mirrored byte-identically under
    extensions/drm-copilot/resources/claude-customizations/.
#>

Import-Module (Join-Path $PSScriptRoot '../lib/worktree-resolution/EpicScopeResolution.psm1') -Force -ErrorAction Stop
Import-Module (Join-Path $PSScriptRoot '../lib/worktree-resolution/EpicScopeReadiness.psm1') -Force -ErrorAction Stop

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

function Get-OrchestrationEpicScopeDecision {
    <#
    .SYNOPSIS
        Returns the epic-scope decision for an implementation-classified command or path, or $null.
    .DESCRIPTION
        Resolves epic scope with worktree-HEAD matching and the command's -C selector. When
        the call is not epic scope, returns $null so the caller's single-feature path runs
        unchanged. In epic scope, returns an allow decision when the epic command-leg
        readiness predicate passes, and otherwise a deny naming the epic checkpoint and the
        failed conjunct.
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
    $selector = if ($Command) { Get-OrchestrationEpicScopeSelector -Command $Command } else { $null }
    $scope = Resolve-EpicScopeCheckpoint -Text ([string]$Command) -SessionRoot (Get-Location).Path -WorktreeSelector $selector -MatchWorktreeHead
    if (-not $scope.IsEpicScope) {
        return $null
    }

    $failure = Get-EpicCommandLegReadinessFailure -Checkpoint $scope.Checkpoint -MergeInProgress $scope.MergeInProgress
    if (-not $failure) {
        return Get-OrchestrationPreimplementationGateAllowDecision
    }
    return Get-OrchestrationPreimplementationGateBlockDecision -Reason ("PREIMPLEMENTATION_GATE_BLOCKED: this epic-scope operation was evaluated against $($scope.CheckpointPath), and the failed readiness predicate is '$failure'. Implementation operations in epic scope require that checkpoint to satisfy every readiness predicate, and a production path may be staged or edited only while a merge is in progress.")
}
