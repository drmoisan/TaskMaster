<#
.SYNOPSIS
    Checkpoint read seams and run-target resolution for the epic merge gate (issue #690).

.DESCRIPTION
    Dot-sourced by enforce-epic-merge-gate.ps1 immediately after the authorization sibling.
    Holds:

    - The import guard for WorktreeRunResolution.psm1. A failed import is recorded in
      $script:EpicMergeGateResolutionImportFailure, and
      Get-EpicMergeGateImportFailureDecision turns it into a deny, so a missing module
      cannot make the gate exit non-zero and fail open.
    - The three checkpoint read seams, relocated from the gate file with their names kept.
      Each takes a mandatory absolute path composed beneath a resolved worktree root.
    - The session worktree root, used for the child branch and for a bare command.
    - The resolution seam Resolve-EpicMergeGateRunTarget, which locates the epic or
      parallel checkpoint that records the command's pull request number.
    - Test-ChildCheckpointPrGateBinding, which binds the command's pull request number to
      the per-feature checkpoint's pr_gate.pr_number when that field is recorded.
    - Get-EpicMergeGateUnresolvedReason, the reason prefix used when neither run branch
      resolves.

.NOTES
    PowerShell 7+. Depends on Get-EpicMergeGateBlockDecision from the authorization
    sibling, dot-sourced by the gate before this file. Mirrored byte-identically under
    extensions/drm-copilot/resources/claude-customizations/.
#>

# Import guard (issue #690): a failed import denies instead of failing open.
$script:EpicMergeGateResolutionImportFailure = $null
try {
    Import-Module (Join-Path $PSScriptRoot '../lib/worktree-resolution/WorktreeRunResolution.psm1') -Force -ErrorAction Stop
}
catch {
    $script:EpicMergeGateResolutionImportFailure = 'WorktreeRunResolution.psm1'
}

function Get-EpicMergeGateImportFailureDecision {
    <#
    .SYNOPSIS
        Returns the deny for a failed worktree-resolution import, or $null.
    .OUTPUTS
        System.Collections.Specialized.OrderedDictionary, or $null when the import succeeded.
    #>
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param()

    if (-not $script:EpicMergeGateResolutionImportFailure) {
        return $null
    }
    return Get-EpicMergeGateBlockDecision -Reason (
        "EPIC_MERGE_GATE_BLOCKED: the worktree-resolution module '$($script:EpicMergeGateResolutionImportFailure)' " +
        'failed to import, so the run checkpoint that governs this merge cannot be located; the gate fails closed.')
}

function Get-ChildOrchestratorCheckpointContent {
    <#
    .SYNOPSIS
        Read the raw JSON text of the per-feature orchestrator checkpoint. Tests mock
        this function (read seam).
    .PARAMETER Path
        The absolute checkpoint path composed beneath the session worktree root.
    .OUTPUTS
        System.String or $null
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param([Parameter(Mandatory)][ValidatePattern('^([A-Za-z]:[\\/]|/)')][string] $Path)

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        return $null
    }
    return (Get-Content -LiteralPath $Path -Raw)
}

function Get-EpicOrchestratorCheckpointContent {
    <#
    .SYNOPSIS
        Read the raw JSON text of the epic checkpoint. Tests mock this function
        (read seam).
    .PARAMETER Path
        The absolute checkpoint path composed beneath the resolved worktree root.
    .OUTPUTS
        System.String or $null
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param([Parameter(Mandatory)][ValidatePattern('^([A-Za-z]:[\\/]|/)')][string] $Path)

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        return $null
    }
    return (Get-Content -LiteralPath $Path -Raw)
}

function Get-ParallelOrchestratorCheckpointContent {
    <#
    .SYNOPSIS
        Read the raw JSON text of the parallel-orchestrator checkpoint. Tests mock
        this function (read seam).
    .PARAMETER Path
        The absolute checkpoint path composed beneath the resolved worktree root.
    .OUTPUTS
        System.String or $null
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param([Parameter(Mandatory)][ValidatePattern('^([A-Za-z]:[\\/]|/)')][string] $Path)

    if (-not (Test-Path -LiteralPath $Path -PathType Leaf)) {
        return $null
    }
    return (Get-Content -LiteralPath $Path -Raw)
}

function Get-EpicMergeGateSessionWorktreeRoot {
    <#
    .SYNOPSIS
        Return the worktree root the session runs in (the child branch and bare-command root).
    .OUTPUTS
        System.String
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param()

    return (Resolve-WorktreeOperandTarget -Path '' -SessionRoot (Get-Location).Path).WorktreeRoot
}

function Resolve-EpicMergeGateRunTarget {
    <#
    .SYNOPSIS
        Resolve the worktree whose run checkpoint records a pull request number (issue #690 seam).
    .PARAMETER Kind
        epic or parallel.
    .PARAMETER PrNumber
        The explicit pull request number the command names.
    .OUTPUTS
        System.Management.Automation.PSCustomObject (the worktree-resolution target result).
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [Parameter(Mandatory)][ValidateSet('epic', 'parallel')][string] $Kind,
        [Parameter(Mandatory)][int] $PrNumber
    )

    return Resolve-WorktreeRunTargetByRecord -Kind $Kind -RecordField pr_number -Value ([string]$PrNumber) -SessionRoot (Get-Location).Path
}

function Test-ChildCheckpointPrGateBinding {
    <#
    .SYNOPSIS
        True unless the per-feature checkpoint records a pr_gate.pr_number that differs from the command's.
    .DESCRIPTION
        A bare command, an absent checkpoint, and a checkpoint without pr_gate.pr_number
        keep today's unbound child-branch decision. When both numbers are present they must
        parse as integers and be equal.
    .PARAMETER Checkpoint
        Parsed per-feature checkpoint, or $null.
    .PARAMETER CommandPrNumber
        The explicit pull request number the command names, or $null.
    .OUTPUTS
        System.Boolean
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [AllowNull()] $Checkpoint,
        [AllowNull()][Nullable[int]] $CommandPrNumber
    )

    if ($null -eq $CommandPrNumber -or $null -eq $Checkpoint) {
        return $true
    }
    if (@($Checkpoint.PSObject.Properties.Name) -notcontains 'pr_gate' -or $null -eq $Checkpoint.pr_gate) {
        return $true
    }
    $prGate = $Checkpoint.pr_gate
    if (@($prGate.PSObject.Properties.Name) -notcontains 'pr_number' -or $null -eq $prGate.pr_number) {
        return $true
    }
    $recorded = 0
    if (-not [int]::TryParse([string]$prGate.pr_number, [ref] $recorded)) {
        return $false
    }
    return ($recorded -eq $CommandPrNumber)
}

function Get-EpicMergeGateUnresolvedReason {
    <#
    .SYNOPSIS
        Returns '<code>: <epic detail>; <parallel detail>' when neither run branch resolved, or $null.
    .DESCRIPTION
        The ambiguity code wins when either target is Ambiguous. Returns $null when either
        target is absent (a bare command) or either target resolved.
    .PARAMETER EpicTarget
        The epic run target, or $null.
    .PARAMETER ParallelTarget
        The parallel run target, or $null.
    .OUTPUTS
        System.String or $null
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [AllowNull()] $EpicTarget,
        [AllowNull()] $ParallelTarget
    )

    if ($null -eq $EpicTarget -or $null -eq $ParallelTarget) {
        return $null
    }
    $unresolved = @('NoTarget', 'Ambiguous')
    if ($unresolved -notcontains $EpicTarget.Status -or $unresolved -notcontains $ParallelTarget.Status) {
        return $null
    }
    $code = if ($EpicTarget.Status -eq 'Ambiguous') { $EpicTarget.ReasonCode } elseif ($ParallelTarget.Status -eq 'Ambiguous') { $ParallelTarget.ReasonCode } else { $EpicTarget.ReasonCode }
    return ('{0}: {1}; {2}' -f $code, $EpicTarget.Detail, $ParallelTarget.Detail)
}
