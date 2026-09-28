<#
.SYNOPSIS
    Worktree locator and path normaliser for target-worktree resolution (issue #669).

.DESCRIPTION
    Locates the worktree that contains a path by an upward walk for a .git entry,
    enumerates the worktrees reachable from a session root through the repository
    administrative directory, and converts paths between absolute and repo-relative
    form without truncating them. It also publishes the single ambiguity reason
    code that downstream gates concatenate into a deny reason.

    A level is a worktree root when its .git child is a directory (the main
    checkout) or a file whose first line is a gitdir: line (a linked worktree). A
    linked worktree may live outside the main checkout, so containment is never
    decided by comparing a path against one known root.

.NOTES
    Compatible with PowerShell 7+. No external module dependencies, no subprocess,
    no network, no clock read, and no environment read. All filesystem contact
    passes through three exported seams so tests model any worktree topology
    without writing a temporary file. Mirrored byte-identically under
    extensions/drm-copilot/resources/claude-customizations/.
    CONVENTION: this module fails fast at module scope and imports its siblings with -ErrorAction Stop.
#>

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# The name of the entry that marks a worktree root. Only this one name is probed,
# because the repository layout never uses a separate administrative directory.
$script:GitEntryName = '.git'

# The first-line prefix of a linked worktree's .git file, and the pattern that
# captures its target. Anything else in a .git file is not a root marker.
$script:GitDirPrefix = 'gitdir:'
$script:GitDirLinePattern = '^' + [regex]::Escape($script:GitDirPrefix) + '\s*(.+)$'

# The per-worktree administrative files read during enumeration. commondir leads
# from a linked worktree's administrative directory back to the main .git
# directory; worktrees holds one administrative directory per linked worktree.
$script:CommonDirFileName = 'commondir'
$script:WorktreeAdminDirectoryName = 'worktrees'
$script:WorktreeGitDirFileName = 'gitdir'
$script:HeadFileName = 'HEAD'

# The only HEAD form that names a branch. A detached HEAD names no branch, so it
# never matches a branch filter.
$script:BranchReferencePrefix = 'ref: refs/heads/'

# Upper bound on the upward walk, so a malformed input cannot loop.
$script:MaximumAscentDepth = 64

# The single cause code emitted when the correct target cannot be identified. It
# carries no _BLOCKED suffix because this module owns no gate.
$script:AmbiguityReasonCode = 'TARGET_WORKTREE_AMBIGUOUS'
# The no-target reason code (issue #687). Without it a NoTarget result carried nothing a gate
# could emit, leaving only wrong options: substitute the session root, which in a parallel or
# epic topology is a sibling item's state, or deny without naming why.
$script:NoTargetReasonCode = 'TARGET_WORKTREE_NOT_DERIVABLE'

function Get-WorktreeResolutionGitEntryKind {
    <#
    .SYNOPSIS
        Classify a path as Directory, File, or None. The module's only existence probe (seam).
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
        Read a file's raw text, or $null when unreadable. The module's only content read (seam).
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

function Get-WorktreeResolutionDirectoryChildName {
    <#
    .SYNOPSIS
        List child directory names as an array (empty when absent). The module's only directory enumeration (seam).
    .PARAMETER Path
        The absolute path of the directory to list.
    #>
    [CmdletBinding()]
    [OutputType([string[]], [object[]])]
    param(
        [Parameter(Mandatory = $true)]
        [string] $Path
    )

    return , [string[]] @(Get-ChildItem -LiteralPath $Path -Directory -Name -ErrorAction SilentlyContinue)
}

function ConvertTo-WorktreeResolutionNormalizedPath {
    <#
    .SYNOPSIS
        Normalise a path to forward slashes, no repeated separator, no trailing slash, no leading ./.
    .DESCRIPTION
        Pure. Returns $null for a null, empty, or whitespace input. A UNC share prefix
        and the filesystem root / are preserved.
    .PARAMETER Path
        The path to normalise, in either separator style.
    #>
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
    # Pure: true for a drive-rooted (C:/...) or slash-rooted normalised path.
    [OutputType([bool])]
    param([string] $Path)

    return ($Path -match '^([A-Za-z]:(/|$)|/)')
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
    # drive root. The parent of C:/a is C:, C: has no parent, and the parent of /a is /.
    [OutputType([string])]
    param([string] $Path)

    $index = $Path.LastIndexOf('/')
    if ($index -lt 0 -or $Path -eq '/') {
        return $null
    }
    return $Path.Substring(0, [math]::Max($index, 1))
}

function Resolve-WorktreeResolutionPathAgainst {
    # Pure: resolve a path relative to a normalised absolute base, collapsing . and
    # .. segments. An absolute path is normalised and collapsed without the base.
    [OutputType([string])]
    param([string] $BasePath, [string] $Path)

    $normalized = ConvertTo-WorktreeResolutionNormalizedPath -Path $Path
    if ($null -eq $normalized) {
        return $null
    }
    if (-not (Test-WorktreeResolutionAbsolutePath -Path $normalized)) {
        $normalized = Join-WorktreeResolutionSegment -BasePath $BasePath -ChildPath $normalized
    }
    $kept = [System.Collections.Generic.List[string]]::new()
    foreach ($segment in $normalized.Split('/')) {
        if ($segment -eq '..') {
            if ($kept.Count -gt 1) {
                $kept.RemoveAt($kept.Count - 1)
            }
        } elseif ($segment -ne '.') {
            $kept.Add($segment)
        }
    }
    $joined = $kept -join '/'
    if ($joined.Length -eq 0) {
        return '/'
    }
    return $joined
}

function Get-WorktreeResolutionGitDirTarget {
    # Pure: the target named by the first line of .git file text when that line is a
    # well-formed gitdir: line, otherwise $null.
    [OutputType([string])]
    param([AllowNull()][string] $Text)

    $firstLine = Get-WorktreeResolutionFirstLine -Text $Text
    if ($null -ne $firstLine -and $firstLine -match $script:GitDirLinePattern) {
        return $Matches[1].Trim()
    }
    return $null
}

function Test-WorktreeResolutionRootMarker {
    <#
    .SYNOPSIS
        Test whether a directory level is a worktree root.
    .DESCRIPTION
        True when the level's .git child is a directory, or a file whose first line is
        a well-formed gitdir: line. Reaches the filesystem only through the seams.
    .PARAMETER Path
        The absolute directory level to test.
    #>
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
    $gitEntry = Join-WorktreeResolutionSegment -BasePath $level -ChildPath $script:GitEntryName
    $kind = Get-WorktreeResolutionGitEntryKind -Path $gitEntry
    if ($kind -ne 'File') {
        return ($kind -eq 'Directory')
    }
    $target = Get-WorktreeResolutionGitDirTarget -Text (Get-WorktreeResolutionGitFileText -Path $gitEntry)
    return ($null -ne $target)
}

function Find-WorktreeResolutionRoot {
    <#
    .SYNOPSIS
        Ascend from a path to the first level that is a worktree root.
    .DESCRIPTION
        Returns the normalised root, or $null when the input is not absolute (it would
        depend on the current directory), when the ascent reaches the filesystem or
        drive root without a marker, or when MaximumDepth levels have been tested.
    .PARAMETER Path
        The absolute path to start from; the path itself is level 0.
    .PARAMETER MaximumDepth
        The highest level above the input that is tested.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string] $Path,

        [ValidateRange(0, 4096)]
        [int] $MaximumDepth = $script:MaximumAscentDepth
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

function Get-WorktreeResolutionFirstLine {
    # Pure: the trimmed first line of a text, or $null when there is none.
    [OutputType([string])]
    param([AllowNull()][string] $Text)

    if ([string]::IsNullOrWhiteSpace($Text)) {
        return $null
    }
    return ($Text -split "`r?`n", 2)[0].Trim()
}

function Get-WorktreeResolutionCommonGitDirectory {
    # The main .git directory reachable from a worktree root, or $null. A .git
    # directory is the main checkout's own. A .git file's gitdir: target is the
    # per-worktree admin directory, whose commondir file leads back to main .git.
    [OutputType([string])]
    param([string] $WorktreeRoot)

    $gitEntry = Join-WorktreeResolutionSegment -BasePath $WorktreeRoot -ChildPath $script:GitEntryName
    $kind = Get-WorktreeResolutionGitEntryKind -Path $gitEntry
    if ($kind -eq 'Directory') {
        return $gitEntry
    }
    if ($kind -ne 'File') {
        return $null
    }
    $adminTarget = Get-WorktreeResolutionGitDirTarget -Text (Get-WorktreeResolutionGitFileText -Path $gitEntry)
    if ($null -eq $adminTarget) {
        return $null
    }
    $adminDirectory = Resolve-WorktreeResolutionPathAgainst -BasePath $WorktreeRoot -Path $adminTarget
    $commonDirPath = Join-WorktreeResolutionSegment -BasePath $adminDirectory -ChildPath $script:CommonDirFileName
    $commonTarget = Get-WorktreeResolutionFirstLine -Text (Get-WorktreeResolutionGitFileText -Path $commonDirPath)
    if ($null -eq $commonTarget) {
        return $null
    }
    return (Resolve-WorktreeResolutionPathAgainst -BasePath $adminDirectory -Path $commonTarget)
}

function Get-WorktreeResolutionWorktreeRoot {
    <#
    .SYNOPSIS
        Enumerate the candidate worktree roots reachable from a session root.
    .DESCRIPTION
        Reads the administrative layout through the seams and starts no process. The
        main checkout is always a candidate. Each worktrees/<name>/gitdir file names a
        linked worktree's own .git file, whose parent directory is that worktree's
        root and may lie outside the main checkout. Always returns an array.
    .PARAMETER SessionRoot
        The worktree root the invoking process runs in.
    .PARAMETER Branch
        Optional. Keep only candidates whose HEAD names this branch.
    .PARAMETER RepoRelativePath
        Optional. Keep only candidates under which this repo-relative path exists.
    #>
    [CmdletBinding()]
    [OutputType([string[]], [object[]])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string] $SessionRoot,
        [string] $Branch,
        [string] $RepoRelativePath
    )

    $sessionLevel = ConvertTo-WorktreeResolutionNormalizedPath -Path $SessionRoot
    $commonGitDirectory = if ($null -ne $sessionLevel) { Get-WorktreeResolutionCommonGitDirectory -WorktreeRoot $sessionLevel }
    if ($null -eq $commonGitDirectory) {
        return , [string[]] @()
    }

    # Deduplication below is case-insensitive, matching the Windows filesystem.
    $entries = [System.Collections.Generic.List[hashtable]]::new()
    if ($commonGitDirectory.EndsWith('/' + $script:GitEntryName)) {
        $mainHead = Join-WorktreeResolutionSegment -BasePath $commonGitDirectory -ChildPath $script:HeadFileName
        $entries.Add(@{ Root = (Get-WorktreeResolutionParentPath -Path $commonGitDirectory); Head = $mainHead })
    }
    $adminRoot = Join-WorktreeResolutionSegment -BasePath $commonGitDirectory -ChildPath $script:WorktreeAdminDirectoryName
    # Assigned before enumeration: the seam emits its array as one object.
    $adminNames = Get-WorktreeResolutionDirectoryChildName -Path $adminRoot
    foreach ($name in @($adminNames)) {
        $adminDirectory = Join-WorktreeResolutionSegment -BasePath $adminRoot -ChildPath $name
        $gitDirFile = Join-WorktreeResolutionSegment -BasePath $adminDirectory -ChildPath $script:WorktreeGitDirFileName
        $gitFileTarget = Get-WorktreeResolutionFirstLine -Text (Get-WorktreeResolutionGitFileText -Path $gitDirFile)
        if ($null -eq $gitFileTarget) {
            continue
        }
        $gitFilePath = Resolve-WorktreeResolutionPathAgainst -BasePath $adminDirectory -Path $gitFileTarget
        $linkedHead = Join-WorktreeResolutionSegment -BasePath $adminDirectory -ChildPath $script:HeadFileName
        $entries.Add(@{ Root = (Get-WorktreeResolutionParentPath -Path $gitFilePath); Head = $linkedHead })
    }

    $relative = ConvertTo-WorktreeResolutionNormalizedPath -Path $RepoRelativePath
    $wantedHead = if ([string]::IsNullOrWhiteSpace($Branch)) { $null } else { $script:BranchReferencePrefix + $Branch.Trim() }
    $seen = [System.Collections.Generic.HashSet[string]]::new([System.StringComparer]::OrdinalIgnoreCase)
    $roots = [System.Collections.Generic.List[string]]::new()
    foreach ($entry in $entries) {
        if ($null -eq $entry.Root -or -not $seen.Add($entry.Root)) {
            continue
        }
        if ($null -ne $wantedHead -and (Get-WorktreeResolutionFirstLine -Text (Get-WorktreeResolutionGitFileText -Path $entry.Head)) -cne $wantedHead) {
            continue
        }
        if ($null -ne $relative -and (Get-WorktreeResolutionGitEntryKind -Path (Join-WorktreeResolutionSegment -BasePath $entry.Root -ChildPath $relative)) -eq 'None') {
            continue
        }
        $roots.Add($entry.Root)
    }
    return , [string[]] $roots.ToArray()
}

function ConvertTo-WorktreeResolutionNormalizationResult {
    # Pure: the single constructor of the normalisation result. ReasonCode is the
    # ambiguity literal exactly when the path was not normalised.
    [OutputType([pscustomobject])]
    param([string] $RepoRelativePath, [string] $WorktreeRoot, [bool] $IsNormalized, [string] $Detail)

    return [pscustomobject]@{
        IsNormalized     = $IsNormalized
        RepoRelativePath = if ($IsNormalized) { $RepoRelativePath } else { $null }
        WorktreeRoot     = if ($IsNormalized) { $WorktreeRoot } else { $null }
        ReasonCode       = if ($IsNormalized) { $null } else { $script:AmbiguityReasonCode }
        Detail           = $Detail
    }
}

function ConvertTo-WorktreeResolutionRepoRelativePath {
    <#
    .SYNOPSIS
        Convert a path to its repo-relative form by locating its containing worktree.
    .DESCRIPTION
        Always returns an object. An absolute path is placed by the upward walk and
        keeps its full remainder below the located root; no segment is dropped. A
        relative path is normalised only against an explicit WorktreeRoot, and is
        otherwise refused (Ruling A) because it would depend on the current directory.
    .PARAMETER Path
        The path to convert, relative or absolute, in either separator style.
    .PARAMETER WorktreeRoot
        Optional. The worktree root a relative path is normalised against.
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string] $Path,
        [string] $WorktreeRoot
    )

    $normalized = ConvertTo-WorktreeResolutionNormalizedPath -Path $Path
    if ($null -eq $normalized) {
        return (ConvertTo-WorktreeResolutionNormalizationResult -IsNormalized $false -Detail 'the path to normalise is empty, so no containing worktree can be located')
    }
    if (Test-WorktreeResolutionAbsolutePath -Path $normalized) {
        $root = Find-WorktreeResolutionRoot -Path $normalized
        if ($null -eq $root) {
            return (ConvertTo-WorktreeResolutionNormalizationResult -IsNormalized $false -Detail "no worktree root was found on the upward walk from '$normalized'")
        }
        $remainder = $normalized.Substring($root.Length).TrimStart('/')
        return (ConvertTo-WorktreeResolutionNormalizationResult -IsNormalized $true -WorktreeRoot $root -RepoRelativePath $remainder -Detail "path '$normalized' lies in worktree '$root'")
    }
    $suppliedRoot = ConvertTo-WorktreeResolutionNormalizedPath -Path $WorktreeRoot
    if ($null -eq $suppliedRoot) {
        return (ConvertTo-WorktreeResolutionNormalizationResult -IsNormalized $false -Detail "relative path '$normalized' was supplied without a worktree root, so its containing worktree cannot be identified")
    }
    return (ConvertTo-WorktreeResolutionNormalizationResult -IsNormalized $true -WorktreeRoot $suppliedRoot -RepoRelativePath $normalized -Detail "relative path '$normalized' was normalised against the supplied worktree '$suppliedRoot'")
}

function Get-WorktreeResolutionAmbiguityReasonCode {
    <#
    .SYNOPSIS
        Return the ambiguity reason code, TARGET_WORKTREE_AMBIGUOUS, so callers never restate it.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param()

    return $script:AmbiguityReasonCode
}

function Get-WorktreeResolutionNoTargetReasonCode {
    <#
    .SYNOPSIS
        Return the no-target reason code, TARGET_WORKTREE_NOT_DERIVABLE, so callers never restate it.
    .DESCRIPTION
        Distinct from the ambiguity code: that means signals disagreed, this means the
        payload carried no signal at all, and a gate reports a different remedy for each.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param()

    return $script:NoTargetReasonCode
}

Export-ModuleMember -Function `
    Get-WorktreeResolutionGitEntryKind, `
    Get-WorktreeResolutionGitFileText, `
    Get-WorktreeResolutionDirectoryChildName, `
    ConvertTo-WorktreeResolutionNormalizedPath, `
    Test-WorktreeResolutionRootMarker, `
    Find-WorktreeResolutionRoot, `
    Get-WorktreeResolutionWorktreeRoot, `
    ConvertTo-WorktreeResolutionRepoRelativePath, `
    Get-WorktreeResolutionAmbiguityReasonCode, `
    Get-WorktreeResolutionNoTargetReasonCode
