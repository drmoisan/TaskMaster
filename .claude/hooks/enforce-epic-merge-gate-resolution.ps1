<#
.SYNOPSIS
    Checkpoint read seams and run-target resolution for the epic merge gate (issue #690).

.DESCRIPTION
    Dot-sourced by enforce-epic-merge-gate.ps1 immediately after the authorization sibling.
    Holds:

    - The import guards for WorktreeRunResolution.psm1 and, after it,
      WorktreeItemResolution.psm1 (issue #850). A failed import is recorded in
      $script:HookDependencyFailures, and the gate's call to
      Get-HookDependencyFailureDecision turns it into a deny, so a missing module
      cannot make the gate exit non-zero and fail open.
    - The three checkpoint read seams, relocated from the gate file with their names kept.
      Each takes a mandatory absolute path composed beneath a resolved worktree root.
    - The session worktree root, used for a bare command.
    - The resolution seam Resolve-EpicMergeGateRunTarget, which locates the epic or
      parallel checkpoint that records the command's pull request number.
    - The item seam Resolve-EpicMergeGateItemTarget (issue #850), which locates the live
      worktree whose per-feature checkpoint records the command's pull request number in
      pr_gate.pr_number or a standalone_merge_authorizations entry.
    - Test-ChildCheckpointPrGateBinding, which binds the command's pull request number to
      the per-feature checkpoint: pr_gate.pr_number must equal it when recorded, and
      otherwise a positive-integer standalone record must name it (issue #788).
    - Get-EpicMergeGateUnresolvedReason, the reason prefix used when neither the item
      target nor either run branch resolves.
    - Test-EpicMergeGateNoRecordingTarget, the precondition of the docs-only merge branch:
      the item, epic, and parallel targets all resolve NoTarget (issue #927).

.NOTES
    PowerShell 7+. Depends on Get-EpicMergeGateBlockDecision and
    Test-StandalonePositiveJsonInteger from the authorization sibling, dot-sourced by the
    gate before this file. Mirrored byte-identically under
    extensions/drm-copilot/resources/claude-customizations/.
#>

try { Import-Module (Join-Path $PSScriptRoot '../lib/worktree-resolution/WorktreeRunResolution.psm1') -Force -ErrorAction Stop } catch { Add-HookDependencyFailure -Name 'WorktreeRunResolution.psm1' -ErrorRecord $_ }
try { Import-Module (Join-Path $PSScriptRoot '../lib/worktree-resolution/WorktreeItemResolution.psm1') -Force -ErrorAction Stop } catch { Add-HookDependencyFailure -Name 'WorktreeItemResolution.psm1' -ErrorRecord $_ }

function Get-ChildOrchestratorCheckpointContent {
    <#
    .SYNOPSIS
        Read the raw JSON text of the per-feature orchestrator checkpoint. Tests mock
        this function (read seam).
    .PARAMETER Path
        The absolute checkpoint path composed beneath the item or session worktree root.
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
        Return the worktree root the session runs in (the bare-command root).
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

function Resolve-EpicMergeGateItemTarget {
    <#
    .SYNOPSIS
        Resolve the worktree whose per-feature checkpoint records a pull request number (issue #850 seam).
    .PARAMETER PrNumber
        The explicit pull request number the command names.
    .OUTPUTS
        System.Management.Automation.PSCustomObject (the worktree-resolution target result).
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [Parameter(Mandatory)][int] $PrNumber
    )

    return Resolve-WorktreeItemTargetByPrNumber -PrNumber $PrNumber -SessionRoot (Get-Location).Path
}

function Test-ChildCheckpointPrGateBinding {
    <#
    .SYNOPSIS
        True when the per-feature checkpoint binds the command's pull request number (issue #788).
    .DESCRIPTION
        Pure. A bare command is unbound and returns $true. An explicit number requires a
        checkpoint: when it records pr_gate.pr_number, that value must parse as an integer
        equal to the number; otherwise a standalone_merge_authorizations entry whose
        pr_number is a positive JSON integer equal to the number must exist. Every other
        case returns $false, so a checkpoint that names neither field cannot authorize an
        explicit-number merge through the child branch.
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

    if ($null -eq $CommandPrNumber) {
        return $true
    }
    if ($null -eq $Checkpoint) {
        return $false
    }
    $props = @($Checkpoint.PSObject.Properties.Name)
    if ($props -contains 'pr_gate' -and $null -ne $Checkpoint.pr_gate -and
        @($Checkpoint.pr_gate.PSObject.Properties.Name) -contains 'pr_number' -and $null -ne $Checkpoint.pr_gate.pr_number) {
        $recorded = 0
        if (-not [int]::TryParse([string]$Checkpoint.pr_gate.pr_number, [ref] $recorded)) {
            return $false
        }
        return ($recorded -eq $CommandPrNumber)
    }
    if ($props -notcontains 'standalone_merge_authorizations') {
        return $false
    }
    foreach ($entry in @($Checkpoint.standalone_merge_authorizations)) {
        if ($null -eq $entry -or @($entry.PSObject.Properties.Name) -notcontains 'pr_number') {
            continue
        }
        if ((Test-StandalonePositiveJsonInteger -Value $entry.pr_number) -and [long]$entry.pr_number -eq $CommandPrNumber) {
            return $true
        }
    }
    return $false
}

function Get-EpicMergeGateUnresolvedReason {
    <#
    .SYNOPSIS
        Returns '<code>: <item detail>; <epic detail>; <parallel detail>' when no target resolved, or $null.
    .DESCRIPTION
        The ambiguity code of the first Ambiguous target among item, epic, and parallel
        wins; otherwise the item target's code is used. Returns $null when any target is
        absent (a bare command) or any target resolved.
    .PARAMETER ItemTarget
        The item target, or $null.
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
        [AllowNull()] $ItemTarget,
        [AllowNull()] $EpicTarget,
        [AllowNull()] $ParallelTarget
    )

    if ($null -eq $ItemTarget -or $null -eq $EpicTarget -or $null -eq $ParallelTarget) {
        return $null
    }
    $targets = @($ItemTarget, $EpicTarget, $ParallelTarget)
    $unresolved = @('NoTarget', 'Ambiguous')
    foreach ($target in $targets) {
        if ($unresolved -notcontains $target.Status) {
            return $null
        }
    }
    $ambiguous = @($targets | Where-Object { $_.Status -eq 'Ambiguous' })
    $code = if ($ambiguous.Count -gt 0) { $ambiguous[0].ReasonCode } else { $ItemTarget.ReasonCode }
    return ('{0}: {1}; {2}; {3}' -f $code, $ItemTarget.Detail, $EpicTarget.Detail, $ParallelTarget.Detail)
}

function Test-EpicMergeGateNoRecordingTarget {
    <#
    .SYNOPSIS
        True only when the item, epic, and parallel targets all have status NoTarget (issue #927).
    .DESCRIPTION
        Pure. That combination means no checkpoint records the command's pull request number,
        which is the only case the docs-only merge branch evaluates. A $null target (a bare
        command), an Ambiguous target, and any resolved target return $false.
    .PARAMETER ItemTarget
        The item target, or $null.
    .PARAMETER EpicTarget
        The epic run target, or $null.
    .PARAMETER ParallelTarget
        The parallel run target, or $null.
    .OUTPUTS
        System.Boolean
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [AllowNull()] $ItemTarget,
        [AllowNull()] $EpicTarget,
        [AllowNull()] $ParallelTarget
    )

    foreach ($target in @($ItemTarget, $EpicTarget, $ParallelTarget)) {
        if ($null -eq $target -or [string]$target.Status -cne 'NoTarget') {
            return $false
        }
    }
    return $true
}
