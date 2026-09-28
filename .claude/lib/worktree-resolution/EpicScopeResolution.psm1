<#
.SYNOPSIS
    Epic-scope resolution: decides whether a gated call is an epic-level operation (issue #663).

.DESCRIPTION
    An epic run keeps its state in artifacts/orchestration/epic-orchestrator-state.json at
    the coordinating worktree root, not in a per-feature checkpoint. This module decides,
    for one gated call, whether the call is an operation of that epic and, if so, returns
    the absolute path and parsed content of the governing epic checkpoint.

    A call is epic scope only when the session root's epic checkpoint parses, carries
    route_id "epic" and a non-empty integration_branch, and that branch equals either the
    call's branch signal (--head, --branch, or a branch: label) or, for a command or path
    leg, the HEAD branch of the effective worktree (the -C selector worktree when present,
    otherwise the session root). A command or path leg ignores any text branch signal, so
    its merge probe inspects the worktree it operates on. Anything else is not epic scope,
    so the caller's existing per-feature resolution runs unchanged.

    The checkpoint path is always composed from the resolved session root and is never
    read from command or prompt text (the issue #554 posture and the issue #673
    invariant). Exactly one checkpoint read happens per resolution.

.NOTES
    PowerShell 7+. Every filesystem contact passes through the exported seams of the
    sibling WorktreeResolution.psm1, so tests model any topology by mocking those seams
    inside this module and create no file. No subprocess, no network, no clock read, and
    no environment read. Mirrored byte-identically under
    extensions/drm-copilot/resources/claude-customizations/.
    AUTHORITY: PowerShell-authoritative. The epic readiness predicates have no Python reference implementation, and no enforcement hook invokes Python (issue #663, D5).
    CONVENTION: this module fails fast at module scope and imports its siblings with -ErrorAction Stop.
#>

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

Import-Module (Join-Path $PSScriptRoot 'WorktreeResolution.psm1') -ErrorAction Stop
Import-Module (Join-Path $PSScriptRoot 'WorktreeTargetResolution.psm1') -ErrorAction Stop

# The one definition of the epic checkpoint location relative to a worktree root.
$script:EpicCheckpointRelativePath = 'artifacts/orchestration/epic-orchestrator-state.json'

# The only route id that makes an epic checkpoint scope-defining. Compared case-sensitively.
$script:EpicRouteId = 'epic'

# Git layout names read from a worktree: the root marker, the linked-worktree pointer
# line, the HEAD file, the branch reference prefix, and the in-progress merge marker.
$script:GitEntryName = '.git'
$script:GitDirLinePattern = '^gitdir:\s*(?<target>.+)$'
$script:HeadFileName = 'HEAD'
$script:BranchReferencePrefix = 'ref: refs/heads/'
$script:MergeHeadFileName = 'MERGE_HEAD'

# A drive-rooted or slash-rooted normalised path.
$script:AbsolutePathPattern = '^([A-Za-z]:(/|$)|/)'

function Get-EpicScopePropertyValue {
    # Pure: the named property of an object, or $null when the object or the property
    # is absent. StrictMode forbids reading a missing property directly.
    [OutputType([object])]
    param([AllowNull()][object] $InputObject, [string] $Name)

    if ($null -eq $InputObject) {
        return $null
    }
    $property = $InputObject.PSObject.Properties[$Name]
    if ($null -eq $property) {
        return $null
    }
    return $property.Value
}

function Get-EpicScopeFirstLine {
    # Pure: the trimmed first line of a text, or $null when there is none.
    [OutputType([string])]
    param([AllowNull()][string] $Text)

    if ([string]::IsNullOrWhiteSpace($Text)) {
        return $null
    }
    return ($Text -split "`r?`n", 2)[0].Trim()
}

function New-EpicScopeResult {
    # Pure: the single constructor of the resolution result shape.
    [Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSUseShouldProcessForStateChangingFunctions', '', Justification = 'Pure in-memory factory that changes no system state.')]
    [OutputType([pscustomobject])]
    param(
        [bool] $IsEpicScope,
        [AllowNull()][string] $CheckpointPath,
        [AllowNull()][object] $Checkpoint,
        [AllowNull()][string] $WorktreeRoot,
        [AllowNull()][string] $Branch,
        [bool] $MergeInProgress,
        [string] $Reason
    )

    return [pscustomobject]@{
        IsEpicScope     = $IsEpicScope
        CheckpointPath  = $CheckpointPath
        Checkpoint      = $Checkpoint
        WorktreeRoot    = $WorktreeRoot
        Branch          = $Branch
        MergeInProgress = $MergeInProgress
        Reason          = $Reason
    }
}

function Get-EpicScopeCheckpointRelativePath {
    <#
    .SYNOPSIS
        Return the repository-relative path of the epic checkpoint.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param()

    return $script:EpicCheckpointRelativePath
}

function Get-EpicScopeCheckpointText {
    <#
    .SYNOPSIS
        Read the epic checkpoint text, or $null when no such file exists (seam).
    .PARAMETER Path
        The absolute path of the epic checkpoint.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory = $true)]
        [string] $Path
    )

    if ((Get-WorktreeResolutionGitEntryKind -Path $Path) -ne 'File') {
        return $null
    }
    return (Get-WorktreeResolutionGitFileText -Path $Path)
}

function ConvertFrom-EpicScopeCheckpointText {
    <#
    .SYNOPSIS
        Parse epic checkpoint text into an object, or $null. Never throws.
    .DESCRIPTION
        Pure. Null, empty, unparseable, and non-object JSON (an array or a scalar) all
        return $null, so an unreadable checkpoint is treated as absent.
    .PARAMETER Text
        The raw checkpoint text.
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [AllowEmptyString()]
        [string] $Text
    )

    if ([string]::IsNullOrWhiteSpace($Text)) {
        return $null
    }
    try {
        $parsed = ConvertFrom-Json -InputObject $Text -ErrorAction Stop
    } catch {
        # Unparseable text is a fail-closed "not epic scope", never a hook error.
        Write-Debug "EPIC_SCOPE_CHECKPOINT_UNPARSEABLE: $($_.Exception.Message)"
        return $null
    }
    if ($parsed -isnot [System.Management.Automation.PSCustomObject]) {
        return $null
    }
    return $parsed
}

function Get-EpicScopeWorktreeGitDirectory {
    <#
    .SYNOPSIS
        Return a worktree's git directory, or $null.
    .DESCRIPTION
        For a main checkout the git entry is a directory and is returned as-is. For a
        linked worktree the git entry is a file whose first line is a gitdir line; its
        target is returned, joined to the worktree root when it is relative. Anything
        else returns $null.
    .PARAMETER WorktreeRoot
        The absolute worktree root.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string] $WorktreeRoot
    )

    $root = ConvertTo-WorktreeResolutionNormalizedPath -Path $WorktreeRoot
    if ($null -eq $root -or $root -notmatch $script:AbsolutePathPattern) {
        return $null
    }
    $gitEntry = Join-WorktreeResolutionPath -WorktreeRoot $root -RepoRelativePath $script:GitEntryName
    $kind = Get-WorktreeResolutionGitEntryKind -Path $gitEntry
    # Decide by entry kind: a directory is the git directory itself, a file points at it,
    # and a missing entry means the root carries no git state.
    if ($kind -eq 'Directory') {
        return $gitEntry
    } elseif ($kind -ne 'File') {
        return $null
    }
    $firstLine = Get-EpicScopeFirstLine -Text (Get-WorktreeResolutionGitFileText -Path $gitEntry)
    if ($null -eq $firstLine -or $firstLine -notmatch $script:GitDirLinePattern) {
        return $null
    }
    $target = ConvertTo-WorktreeResolutionNormalizedPath -Path $Matches['target']
    if ($target -match $script:AbsolutePathPattern) {
        return $target
    }
    return (Join-WorktreeResolutionPath -WorktreeRoot $root -RepoRelativePath $target)
}

function Get-EpicScopeWorktreeHeadBranch {
    <#
    .SYNOPSIS
        Return the branch checked out in a worktree, or $null for a detached or unreadable HEAD (seam).
    .PARAMETER WorktreeRoot
        The absolute worktree root.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string] $WorktreeRoot
    )

    $gitDirectory = Get-EpicScopeWorktreeGitDirectory -WorktreeRoot $WorktreeRoot
    if ($null -eq $gitDirectory) {
        return $null
    }
    $headPath = Join-WorktreeResolutionPath -WorktreeRoot $gitDirectory -RepoRelativePath $script:HeadFileName
    $firstLine = Get-EpicScopeFirstLine -Text (Get-WorktreeResolutionGitFileText -Path $headPath)
    if ($null -eq $firstLine -or -not $firstLine.StartsWith($script:BranchReferencePrefix)) {
        return $null
    }
    $branch = $firstLine.Substring($script:BranchReferencePrefix.Length).Trim()
    if ($branch.Length -eq 0) {
        return $null
    }
    return $branch
}

function Test-EpicScopeMergeInProgress {
    <#
    .SYNOPSIS
        Report whether a merge is in progress in a worktree (MERGE_HEAD exists) (seam).
    .PARAMETER WorktreeRoot
        The absolute worktree root.
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string] $WorktreeRoot
    )

    $gitDirectory = Get-EpicScopeWorktreeGitDirectory -WorktreeRoot $WorktreeRoot
    if ($null -eq $gitDirectory) {
        return $false
    }
    $mergeHeadPath = Join-WorktreeResolutionPath -WorktreeRoot $gitDirectory -RepoRelativePath $script:MergeHeadFileName
    return ((Get-WorktreeResolutionGitEntryKind -Path $mergeHeadPath) -eq 'File')
}

function Resolve-EpicScopeCheckpoint {
    <#
    .SYNOPSIS
        Decide whether a gated call is an epic-level operation and return the governing epic checkpoint.
    .DESCRIPTION
        Returns an object with IsEpicScope, CheckpointPath, Checkpoint, WorktreeRoot,
        Branch, MergeInProgress, and Reason. The first matching step decides: no branch
        signal without head matching (no-branch-signal, and no checkpoint read); an
        unresolvable session root (session-root-unresolved); an absent or unparseable epic
        checkpoint (epic-checkpoint-absent-or-unparseable); a route_id other than epic
        (route_id); an empty integration_branch (integration_branch); an unresolvable
        selector (selector-unresolved); a branch that does not match (branch-mismatch).
    .PARAMETER Text
        The command or prompt text; only its branch signal is read.
    .PARAMETER SessionRoot
        The session directory; defaults to the current location.
    .PARAMETER WorktreeSelector
        The -C selector value of a command leg, when present.
    .PARAMETER MatchWorktreeHead
        Match the effective worktree's HEAD branch, ignoring any text branch signal, and
        probe that worktree for a merge in progress.
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [AllowEmptyString()]
        [string] $Text,

        [AllowNull()]
        [AllowEmptyString()]
        [string] $SessionRoot = (Get-Location).ProviderPath,

        [AllowNull()]
        [AllowEmptyString()]
        [string] $WorktreeSelector,

        [switch] $MatchWorktreeHead
    )

    $branch = Find-WorktreeResolutionBranchSignal -Text $Text
    if ($null -eq $branch -and -not $MatchWorktreeHead) {
        return (New-EpicScopeResult -IsEpicScope $false -Reason 'no-branch-signal')
    }
    $root = Find-WorktreeResolutionRoot -Path $SessionRoot
    if ($null -eq $root) {
        return (New-EpicScopeResult -IsEpicScope $false -Reason 'session-root-unresolved')
    }
    $path = Join-WorktreeResolutionPath -WorktreeRoot $root -RepoRelativePath (Get-EpicScopeCheckpointRelativePath)
    $checkpoint = ConvertFrom-EpicScopeCheckpointText -Text (Get-EpicScopeCheckpointText -Path $path)
    if ($null -eq $checkpoint) {
        return (New-EpicScopeResult -IsEpicScope $false -CheckpointPath $path -Reason 'epic-checkpoint-absent-or-unparseable')
    }
    if ([string](Get-EpicScopePropertyValue -InputObject $checkpoint -Name 'route_id') -cne $script:EpicRouteId) {
        return (New-EpicScopeResult -IsEpicScope $false -CheckpointPath $path -Reason 'route_id')
    }
    $integrationBranch = [string](Get-EpicScopePropertyValue -InputObject $checkpoint -Name 'integration_branch')
    if ([string]::IsNullOrWhiteSpace($integrationBranch)) {
        return (New-EpicScopeResult -IsEpicScope $false -CheckpointPath $path -Reason 'integration_branch')
    }

    # Decide the matched branch. A command or path leg (-MatchWorktreeHead) matches the
    # effective worktree's HEAD, the selector worktree taking precedence, and ignores any
    # text branch signal so the merge probe inspects the worktree the call operates on
    # (issue #663 remediation CR-2); otherwise the explicit branch signal decides.
    $effectiveRoot = $root
    if (-not $MatchWorktreeHead) {
        $candidate = $branch
    } else {
        if (-not [string]::IsNullOrWhiteSpace($WorktreeSelector)) {
            $effectiveRoot = Find-WorktreeResolutionRoot -Path $WorktreeSelector
        }
        if ($null -eq $effectiveRoot) {
            return (New-EpicScopeResult -IsEpicScope $false -CheckpointPath $path -Reason 'selector-unresolved')
        }
        $candidate = Get-EpicScopeWorktreeHeadBranch -WorktreeRoot $effectiveRoot
    }
    if ($null -eq $candidate -or $candidate -cne $integrationBranch) {
        return (New-EpicScopeResult -IsEpicScope $false -CheckpointPath $path -Reason 'branch-mismatch')
    }

    $mergeInProgress = $false
    if ($MatchWorktreeHead) {
        $mergeInProgress = [bool](Test-EpicScopeMergeInProgress -WorktreeRoot $effectiveRoot)
    }
    return (New-EpicScopeResult -IsEpicScope $true -CheckpointPath $path -Checkpoint $checkpoint -WorktreeRoot $effectiveRoot -Branch $candidate -MergeInProgress $mergeInProgress -Reason 'epic-scope')
}

Export-ModuleMember -Function `
    Get-EpicScopeCheckpointRelativePath, `
    Get-EpicScopeCheckpointText, `
    ConvertFrom-EpicScopeCheckpointText, `
    Get-EpicScopeWorktreeGitDirectory, `
    Get-EpicScopeWorktreeHeadBranch, `
    Test-EpicScopeMergeInProgress, `
    Resolve-EpicScopeCheckpoint
