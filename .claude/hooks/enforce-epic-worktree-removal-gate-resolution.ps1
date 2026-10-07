<#
.SYNOPSIS
    Checkpoint read seams and run-target resolution for the epic worktree-removal gate (issue #690).

.DESCRIPTION
    Dot-sourced by enforce-epic-worktree-removal-gate.ps1 after its command-parser
    dot-sources. Holds:

    - The import guard for WorktreeRunResolution.psm1. A failed import is recorded in
      $script:EpicWorktreeGateResolutionImportFailure, and
      Get-EpicWorktreeGateImportFailureDecision turns it into a deny, so a missing module
      cannot make the gate exit non-zero and fail open.
    - The two checkpoint read seams, relocated from the gate file with their names kept.
      Each takes a mandatory absolute path composed beneath a resolved worktree root.
    - The resolution seam Resolve-EpicWorktreeGateRunTarget, which locates the epic or
      parallel checkpoint that records the removal target under worktree_path.
    - Read-EpicWorktreeGateRunCheckpoint, which returns the target and the parsed
      checkpoint beneath it ($null when the target is unresolved).

.NOTES
    PowerShell 7+. Depends on Get-EpicWorktreeGateBlockDecision and
    ConvertFrom-EpicWorktreeGateJson from the gate file, resolved at call time. Mirrored
    byte-identically under extensions/drm-copilot/resources/claude-customizations/.
#>

# Import guard (issue #690): a failed import denies instead of failing open.
$script:EpicWorktreeGateResolutionImportFailure = $null
try {
    Import-Module (Join-Path $PSScriptRoot '../lib/worktree-resolution/WorktreeRunResolution.psm1') -Force -ErrorAction Stop
}
catch {
    $script:EpicWorktreeGateResolutionImportFailure = 'WorktreeRunResolution.psm1'
}

function Get-EpicWorktreeGateImportFailureDecision {
    <#
    .SYNOPSIS
        Returns the deny for a failed worktree-resolution import, or $null.
    .OUTPUTS
        System.Collections.Specialized.OrderedDictionary, or $null when the import succeeded.
    #>
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param()

    if (-not $script:EpicWorktreeGateResolutionImportFailure) {
        return $null
    }
    return Get-EpicWorktreeGateBlockDecision -Reason (
        "EPIC_WORKTREE_REMOVAL_BLOCKED: the worktree-resolution module '$($script:EpicWorktreeGateResolutionImportFailure)' " +
        'failed to import, so the run checkpoint that governs this removal cannot be located; the gate fails closed.')
}

function Get-EpicWorktreeGateCheckpointContent {
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

function Get-EpicWorktreeGateParallelCheckpointContent {
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

function Resolve-EpicWorktreeGateRunTarget {
    <#
    .SYNOPSIS
        Resolve the worktree whose run checkpoint records a worktree path (issue #690 seam).
    .PARAMETER Kind
        epic or parallel.
    .PARAMETER WorktreePath
        The removal target the command names.
    .OUTPUTS
        System.Management.Automation.PSCustomObject (the worktree-resolution target result).
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [Parameter(Mandatory)][ValidateSet('epic', 'parallel')][string] $Kind,
        [AllowNull()][AllowEmptyString()][string] $WorktreePath
    )

    return Resolve-WorktreeRunTargetByRecord -Kind $Kind -RecordField worktree_path -Value $WorktreePath -SessionRoot (Get-Location).Path
}

function Read-EpicWorktreeGateRunCheckpoint {
    <#
    .SYNOPSIS
        Resolve one run kind and read its checkpoint beneath the resolved root.
    .PARAMETER Kind
        epic or parallel.
    .PARAMETER WorktreePath
        The removal target the command names.
    .OUTPUTS
        System.Management.Automation.PSCustomObject with Target and Checkpoint ($null when unresolved).
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [Parameter(Mandatory)][ValidateSet('epic', 'parallel')][string] $Kind,
        [AllowNull()][AllowEmptyString()][string] $WorktreePath
    )

    $target = Resolve-EpicWorktreeGateRunTarget -Kind $Kind -WorktreePath $WorktreePath
    $checkpoint = $null
    if ($target.Status -eq 'SessionRoot' -or $target.Status -eq 'OtherWorktree') {
        $path = Get-WorktreeRunCheckpointPath -Kind $Kind -WorktreeRoot $target.WorktreeRoot
        $raw = if ($Kind -eq 'epic') { Get-EpicWorktreeGateCheckpointContent -Path $path } else { Get-EpicWorktreeGateParallelCheckpointContent -Path $path }
        $checkpoint = ConvertFrom-EpicWorktreeGateJson -Raw $raw
    }
    return [pscustomobject]@{
        Target     = $target
        Checkpoint = $checkpoint
    }
}
