<#
.SYNOPSIS
    Epic-scope resolution for the Codex preimplementation gate (issue #707).

.DESCRIPTION
    A Codex-local port of the issue #663 resolver (D1). Scope is decided only by the
    session-root epic checkpoint artifacts/orchestration/epic-orchestrator-state.json
    plus an effective-worktree HEAD match with its integration_branch (D3, D4). The
    effective worktree is the -C selector worktree when supplied, otherwise the session
    root. The checkpoint path is composed from the resolved session root, never read from
    command text, and read once per resolution.

    It performs no subprocess, network, clock, or environment read. Every filesystem
    contact passes through Get-WorktreeResolutionGitEntryKind (the only existence probe)
    and Get-WorktreeResolutionGitFileText (the only content read), so tests mock those.

.NOTES
    PowerShell 7+. Functions only, with no StrictMode and no preference assignment.
    Dot-sourced by enforce-orchestration-preimplementation-gate-epic-scope.ps1.
    Mirrored byte-identically under extensions/drm-copilot/resources/codex-and-agents-customizations/.
    AUTHORITY: PowerShell-authoritative. The epic readiness predicate has no reference implementation in another language, and this hook starts no interpreter process (issue #707, D15).
#>

# The one definition of the epic checkpoint location relative to a worktree root.
$script:EpicScopeCheckpointRelativePath = 'artifacts/orchestration/epic-orchestrator-state.json'

# The only route id that makes an epic checkpoint scope-defining. Compared case-sensitively.
$script:EpicScopeRouteId = 'epic'

# The in-progress merge marker probed inside a worktree's git directory.
$script:EpicScopeMergeHeadFileName = 'MERGE_HEAD'

# Git layout names read from a worktree: the root marker, the linked-worktree pointer
# line, the HEAD file, and the branch reference prefix.
$script:WorktreeResolutionGitEntryName = '.git'
$script:WorktreeResolutionGitDirLinePattern = '^gitdir:\s*(?<target>.+)$'
$script:WorktreeResolutionHeadFileName = 'HEAD'
$script:WorktreeResolutionBranchReferencePrefix = 'ref: refs/heads/'

# A drive-rooted or slash-rooted normalised path.
$script:WorktreeResolutionAbsolutePathPattern = '^([A-Za-z]:(/|$)|/)'

# Upper bound on the upward walk, so a malformed input cannot loop.
$script:WorktreeResolutionMaximumAscentDepth = 64

function Get-WorktreeResolutionGitEntryKind {
    <#
    .SYNOPSIS
        Classify a path as Directory, File, or None. The only existence probe (seam).
    .PARAMETER Path
        The absolute path to classify.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory = $true)]
        [string] $Path
    )

    if (Test-Path -LiteralPath $Path -PathType Container) {
        return 'Directory'
    }
    if (Test-Path -LiteralPath $Path -PathType Leaf) {
        return 'File'
    }
    return 'None'
}

function Get-WorktreeResolutionGitFileText {
    <#
    .SYNOPSIS
        Read a file's raw text, or $null when unreadable. The only content read (seam).
    .PARAMETER Path
        The absolute path of the file to read.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory = $true)]
        [string] $Path
    )

    return (Get-Content -LiteralPath $Path -Raw -ErrorAction SilentlyContinue)
}

function ConvertTo-WorktreeResolutionNormalizedPath {
    # Pure: forward slashes, no repeated separator, no trailing slash, no leading ./;
    # $null for a null, empty, or whitespace input. A UNC prefix and / are preserved.
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [AllowEmptyString()]
        [string] $Path
    )

    if ([string]::IsNullOrWhiteSpace($Path)) {
        return $null
    }
    $normalized = $Path.Trim() -replace '\\', '/'
    $uncPrefix = if ($normalized.StartsWith('//')) { '/' } else { '' }
    $normalized = $uncPrefix + ($normalized -replace '/{2,}', '/')
    while ($normalized.StartsWith('./')) {
        $normalized = $normalized.Substring(2)
    }
    if ($normalized.Length -gt 1) {
        $normalized = $normalized.TrimEnd('/')
    }
    if ($normalized.Length -eq 0) {
        return $null
    }
    return $normalized
}

function Test-WorktreeResolutionAbsolutePath {
    # Pure: true for a drive-rooted (<drive>:/...) or slash-rooted normalised path.
    [OutputType([bool])]
    param([string] $Path)

    return ($Path -match $script:WorktreeResolutionAbsolutePathPattern)
}

function Join-WorktreeResolutionSegment {
    # Pure: append a child to a normalised base without doubling the separator.
    [OutputType([string])]
    param([string] $BasePath, [string] $ChildPath)

    $separator = if ($BasePath.EndsWith('/')) { '' } else { '/' }
    return $BasePath + $separator + $ChildPath
}

function Get-WorktreeResolutionParentPath {
    # Pure: the parent of a normalised absolute path, or $null at the filesystem or
    # drive root. The parent of <drive>:/a is <drive>:, <drive>: has no parent, and the
    # parent of /a is /.
    [OutputType([string])]
    param([string] $Path)

    $index = $Path.LastIndexOf('/')
    if ($index -lt 0 -or $Path -eq '/') {
        return $null
    }
    return $Path.Substring(0, [math]::Max($index, 1))
}

function Get-WorktreeResolutionFirstLine {
    # Pure: the trimmed first line of a text, or $null when there is none.
    [OutputType([string])]
    param([AllowNull()][string] $Text)

    if ([string]::IsNullOrWhiteSpace($Text)) {
        return $null
    }
    return ($Text -split "`r?`n", 2)[0].Trim()
}

function Get-WorktreeResolutionGitDirTarget {
    # Pure: the target named by the first line of .git file text when that line is a
    # well-formed gitdir: line, otherwise $null.
    [OutputType([string])]
    param([AllowNull()][string] $Text)

    $firstLine = Get-WorktreeResolutionFirstLine -Text $Text
    if ($null -ne $firstLine -and $firstLine -match $script:WorktreeResolutionGitDirLinePattern) {
        return $Matches['target'].Trim()
    }
    return $null
}

function Test-WorktreeResolutionRootMarker {
    # True when the level's .git child is a directory, or a file whose first line is a
    # well-formed gitdir: line. Reaches the filesystem only through the seams.
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [Parameter(Mandatory = $true)]
        [string] $Path
    )

    $level = ConvertTo-WorktreeResolutionNormalizedPath -Path $Path
    if ($null -eq $level) {
        return $false
    }
    $gitEntry = Join-WorktreeResolutionSegment -BasePath $level -ChildPath $script:WorktreeResolutionGitEntryName
    $kind = Get-WorktreeResolutionGitEntryKind -Path $gitEntry
    if ($kind -ne 'File') {
        return ($kind -eq 'Directory')
    }
    $target = Get-WorktreeResolutionGitDirTarget -Text (Get-WorktreeResolutionGitFileText -Path $gitEntry)
    return ($null -ne $target)
}

function Find-WorktreeResolutionRoot {
    # Ascend from an absolute path to the first worktree-root level; $null for a relative
    # input, an ascent past the filesystem or drive root, or MaximumDepth levels tested.
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string] $Path,

        [ValidateRange(0, 4096)]
        [int] $MaximumDepth = $script:WorktreeResolutionMaximumAscentDepth
    )

    $current = ConvertTo-WorktreeResolutionNormalizedPath -Path $Path
    if ($null -eq $current -or -not (Test-WorktreeResolutionAbsolutePath -Path $current)) {
        return $null
    }
    for ($depth = 0; ($null -ne $current) -and ($depth -le $MaximumDepth); $depth++) {
        if (Test-WorktreeResolutionRootMarker -Path $current) {
            return $current
        }
        $current = Get-WorktreeResolutionParentPath -Path $current
    }
    return $null
}

function Join-WorktreeResolutionPath {
    # Compose an absolute forward-slash path from a worktree root and a repo-relative
    # path; throws when the root is not absolute.
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory = $true)]
        [string] $WorktreeRoot,
        [Parameter(Mandatory = $true)]
        [string] $RepoRelativePath
    )

    $root = ConvertTo-WorktreeResolutionNormalizedPath -Path $WorktreeRoot
    if ($root -notmatch $script:WorktreeResolutionAbsolutePathPattern) {
        throw "WorktreeRoot must be an absolute path: '$WorktreeRoot'."
    }
    $relative = ConvertTo-WorktreeResolutionNormalizedPath -Path $RepoRelativePath
    if ($null -eq $relative) {
        return $root
    }
    $separator = if ($root.EndsWith('/')) { '' } else { '/' }
    return $root + $separator + $relative.TrimStart('/')
}

function Get-EpicScopePropertyValue {
    # Pure: the named property of an object, or $null when the object or the property
    # is absent.
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
    # Return the repository-relative path of the epic checkpoint.
    [CmdletBinding()]
    [OutputType([string])]
    param()

    return $script:EpicScopeCheckpointRelativePath
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
    # Pure: parse checkpoint text into an object, or $null for null, empty, unparseable,
    # or non-object JSON (an array or a scalar). Never throws.
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
    # A worktree's git directory: the .git directory of a main checkout, or the gitdir
    # target of a linked worktree joined to the root when relative; otherwise $null.
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string] $WorktreeRoot
    )

    $root = ConvertTo-WorktreeResolutionNormalizedPath -Path $WorktreeRoot
    if ($null -eq $root -or $root -notmatch $script:WorktreeResolutionAbsolutePathPattern) {
        return $null
    }
    $gitEntry = Join-WorktreeResolutionPath -WorktreeRoot $root -RepoRelativePath $script:WorktreeResolutionGitEntryName
    $kind = Get-WorktreeResolutionGitEntryKind -Path $gitEntry
    # Decide by entry kind: a directory is the git directory itself, a file points at it,
    # and a missing entry means the root carries no git state.
    if ($kind -eq 'Directory') {
        return $gitEntry
    } elseif ($kind -ne 'File') {
        return $null
    }
    $firstLine = Get-WorktreeResolutionFirstLine -Text (Get-WorktreeResolutionGitFileText -Path $gitEntry)
    if ($null -eq $firstLine -or $firstLine -notmatch $script:WorktreeResolutionGitDirLinePattern) {
        return $null
    }
    $target = ConvertTo-WorktreeResolutionNormalizedPath -Path $Matches['target']
    if ($target -match $script:WorktreeResolutionAbsolutePathPattern) {
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
    $headPath = Join-WorktreeResolutionPath -WorktreeRoot $gitDirectory -RepoRelativePath $script:WorktreeResolutionHeadFileName
    $firstLine = Get-WorktreeResolutionFirstLine -Text (Get-WorktreeResolutionGitFileText -Path $headPath)
    if ($null -eq $firstLine -or -not $firstLine.StartsWith($script:WorktreeResolutionBranchReferencePrefix)) {
        return $null
    }
    $branch = $firstLine.Substring($script:WorktreeResolutionBranchReferencePrefix.Length).Trim()
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
    $mergeHeadPath = Join-WorktreeResolutionPath -WorktreeRoot $gitDirectory -RepoRelativePath $script:EpicScopeMergeHeadFileName
    return ((Get-WorktreeResolutionGitEntryKind -Path $mergeHeadPath) -eq 'File')
}

function Resolve-EpicScopeCheckpoint {
    <#
    .SYNOPSIS
        Decide whether a gated command or path leg is an epic-level operation and return the governing epic checkpoint.
    .DESCRIPTION
        Never throws; fixed head matching (D3). Returns IsEpicScope, CheckpointPath,
        Checkpoint, WorktreeRoot, Branch, MergeInProgress, and Reason. The first matching
        step decides: session-root-unresolved; epic-checkpoint-absent-or-unparseable;
        route_id (not epic); integration_branch (empty); selector-unresolved;
        branch-mismatch (effective HEAD detached or different). Otherwise the reason is
        epic-scope and the effective worktree is probed for a merge in progress.
    .PARAMETER SessionRoot
        The session directory; defaults to the current location.
    .PARAMETER WorktreeSelector
        The -C selector value of a command leg; its worktree's HEAD then decides.
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [AllowNull()]
        [AllowEmptyString()]
        [string] $SessionRoot = (Get-Location).ProviderPath,

        [AllowNull()]
        [AllowEmptyString()]
        [string] $WorktreeSelector
    )

    $root = Find-WorktreeResolutionRoot -Path $SessionRoot
    if ($null -eq $root) {
        return (New-EpicScopeResult -IsEpicScope $false -Reason 'session-root-unresolved')
    }
    $path = Join-WorktreeResolutionPath -WorktreeRoot $root -RepoRelativePath (Get-EpicScopeCheckpointRelativePath)
    $checkpoint = ConvertFrom-EpicScopeCheckpointText -Text (Get-EpicScopeCheckpointText -Path $path)
    if ($null -eq $checkpoint) {
        return (New-EpicScopeResult -IsEpicScope $false -CheckpointPath $path -Reason 'epic-checkpoint-absent-or-unparseable')
    }
    if ([string](Get-EpicScopePropertyValue -InputObject $checkpoint -Name 'route_id') -cne $script:EpicScopeRouteId) {
        return (New-EpicScopeResult -IsEpicScope $false -CheckpointPath $path -Reason 'route_id')
    }
    $integrationBranch = [string](Get-EpicScopePropertyValue -InputObject $checkpoint -Name 'integration_branch')
    if ([string]::IsNullOrWhiteSpace($integrationBranch)) {
        return (New-EpicScopeResult -IsEpicScope $false -CheckpointPath $path -Reason 'integration_branch')
    }

    # The effective worktree's HEAD decides, the selector worktree taking precedence, so the
    # merge probe inspects the worktree the call operates on.
    $effectiveRoot = $root
    if (-not [string]::IsNullOrWhiteSpace($WorktreeSelector)) {
        $effectiveRoot = Find-WorktreeResolutionRoot -Path $WorktreeSelector
    }
    if ($null -eq $effectiveRoot) {
        return (New-EpicScopeResult -IsEpicScope $false -CheckpointPath $path -Reason 'selector-unresolved')
    }
    $candidate = Get-EpicScopeWorktreeHeadBranch -WorktreeRoot $effectiveRoot
    if ($null -eq $candidate -or $candidate -cne $integrationBranch) {
        return (New-EpicScopeResult -IsEpicScope $false -CheckpointPath $path -Reason 'branch-mismatch')
    }

    $mergeInProgress = [bool](Test-EpicScopeMergeInProgress -WorktreeRoot $effectiveRoot)
    return (New-EpicScopeResult -IsEpicScope $true -CheckpointPath $path -Checkpoint $checkpoint -WorktreeRoot $effectiveRoot -Branch $candidate -MergeInProgress $mergeInProgress -Reason 'epic-scope')
}
