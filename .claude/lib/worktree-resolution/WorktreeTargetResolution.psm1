<#
.SYNOPSIS
    Call-target derivation for target-worktree resolution (issue #669).

.DESCRIPTION
    Reads target signals from a tool-call payload (a feature-folder path, a file
    path, and a branch name) and derives the worktree the call pertains to as one
    of four states: SessionRoot, OtherWorktree, NoTarget, or Ambiguous.

    The presence of a signal, not the success of resolving it, separates NoTarget
    from Ambiguous. Signals that resolve to different worktree roots are
    Ambiguous. The precedence order FeatureFolderPath, FilePath, Branch only
    selects which agreeing signal is reported; no run state is ever read to break
    a disagreement.

.NOTES
    Compatible with PowerShell 7+. Pure given its sibling WorktreeResolution.psm1,
    which owns every filesystem read. No subprocess, no network, no clock read, and
    no environment read. Mirrored byte-identically under
    extensions/drm-copilot/resources/claude-customizations/.
    CONVENTION: this module fails fast at module scope and imports its siblings with -ErrorAction Stop.
#>

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

Import-Module (Join-Path $PSScriptRoot 'WorktreeResolution.psm1') -ErrorAction Stop

# The four states of a derived target. The set is closed so a caller writes exactly
# one branch per state; a fifth value would silently fall through every caller.
$script:StatusSessionRoot = 'SessionRoot'
$script:StatusOtherWorktree = 'OtherWorktree'
$script:StatusNoTarget = 'NoTarget'
$script:StatusAmbiguous = 'Ambiguous'

# The signal kinds a result may report. SessionRoot names no payload signal; it is
# reserved for a caller that resolves against the session root deliberately.
$script:SignalFeatureFolderPath = 'FeatureFolderPath'
$script:SignalFilePath = 'FilePath'
$script:SignalBranch = 'Branch'
$script:SignalSessionRoot = 'SessionRoot'

# The prose label used for each payload signal in a Detail clause.
$script:SignalLabel = @{
    FeatureFolderPath = 'feature folder token'
    FilePath          = 'file path'
    Branch            = 'branch'
}

# A feature-folder token: an optional prefix ending in a separator, then
# docs/features/active/<folder>. The prefix is kept rather than discarded, because
# an absolute prefix is the only unambiguous placement of the token.
$script:FeatureFolderPattern = '(?<![^\s"''`(])(?:[^\s"''`]*?[\\/])?docs[\\/]+features[\\/]+active[\\/]+[A-Za-z0-9][A-Za-z0-9._-]*'

# An absolute file path with an extension. Only the absolute form is read from free
# text, because a relative file path names a file present in every worktree.
$script:FilePathPattern = '(?<![^\s"''`(])(?:[A-Za-z]:[\\/]|/)[^\s"''`]*[\\/][^\s"''`\\/]+\.[A-Za-z0-9]+'

# A branch named by a --head or --branch option, or by a "branch:" label. Narrow on
# purpose: a bare word is never read as a branch.
$script:BranchPattern = '(?i)(?:(?<![\w-])--(?:head|branch)(?:=|\s+)|\bbranch:\s*)(?<name>[A-Za-z0-9][A-Za-z0-9._/-]*)'

# A drive-rooted or slash-rooted normalised path.
$script:AbsolutePathPattern = '^([A-Za-z]:(/|$)|/)'

function New-WorktreeResolutionTargetResult {
    <#
    .SYNOPSIS
        Build a target result object. The single constructor of that shape.
    .DESCRIPTION
        Enforces the field invariants: SessionRoot and Detail are always non-empty,
        Candidates is always an array, WorktreeRoot is populated only for the two
        resolved states, Signal and SignalValue are null for NoTarget, and
        ReasonCode is the ambiguity code when Status is Ambiguous and the no-target
        code when Status is NoTarget (issue #687); it is null for the two resolved
        states.
    .PARAMETER Status
        One of SessionRoot, OtherWorktree, NoTarget, or Ambiguous.
    .PARAMETER SessionRoot
        The worktree root the invoking process runs in.
    .PARAMETER WorktreeRoot
        The resolved target root; required for the two resolved states.
    .PARAMETER Signal
        The signal kind that produced or failed to produce the target.
    .PARAMETER SignalValue
        The raw token the signal was read from.
    .PARAMETER Candidate
        The distinct candidate worktree roots considered.
    .PARAMETER Detail
        A prose clause safe to append to a gate's deny reason.
    #>
    [Diagnostics.CodeAnalysis.SuppressMessageAttribute('PSUseShouldProcessForStateChangingFunctions', '', Justification = 'Pure in-memory factory that changes no system state; the exported name is fixed by the issue #669 contract.')]
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [Parameter(Mandatory = $true)]
        [ValidateSet('SessionRoot', 'OtherWorktree', 'NoTarget', 'Ambiguous')]
        [string] $Status,
        [Parameter(Mandatory = $true)]
        [ValidatePattern('\S')]
        [string] $SessionRoot,
        [string] $WorktreeRoot,
        [ValidateSet('', 'FeatureFolderPath', 'FilePath', 'Branch', 'SessionRoot')]
        [string] $Signal,
        [string] $SignalValue,
        [AllowEmptyCollection()]
        [string[]] $Candidate = @(),
        [Parameter(Mandatory = $true)]
        [ValidatePattern('\S')]
        [string] $Detail
    )

    $isResolved = $Status -in @($script:StatusSessionRoot, $script:StatusOtherWorktree)
    $root = ConvertTo-WorktreeResolutionNormalizedPath -Path $WorktreeRoot
    if ($isResolved -and $null -eq $root) {
        throw "A target result with Status '$Status' requires a WorktreeRoot."
    }
    $hasSignal = ($Status -ne $script:StatusNoTarget) -and -not [string]::IsNullOrEmpty($Signal)
    $kept = if ($Status -eq $script:StatusNoTarget) { @() } else { $Candidate }
    $candidates = [string[]] @($kept | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
    return [pscustomobject]@{
        Status       = $Status
        WorktreeRoot = if ($isResolved) { $root } else { $null }
        SessionRoot  = ConvertTo-WorktreeResolutionNormalizedPath -Path $SessionRoot
        Signal       = if ($hasSignal) { $Signal } else { $null }
        SignalValue  = if ($hasSignal) { $SignalValue } else { $null }
        Candidates   = $candidates
        ReasonCode   = switch ($Status) {
            $script:StatusAmbiguous { Get-WorktreeResolutionAmbiguityReasonCode; break }
            $script:StatusNoTarget { Get-WorktreeResolutionNoTargetReasonCode; break }
            default { $null }
        }
        Detail       = $Detail
    }
}

function Find-WorktreeResolutionFeatureFolderSignal {
    <#
    .SYNOPSIS
        Return the first feature-folder token in a text, with any absolute prefix preserved, or $null.
    .DESCRIPTION
        The match does not start at the literal docs segment: a drive or worktree
        prefix written before it stays part of the token, so the token can be
        placed in exactly one worktree. The token ends at the feature-folder name.
    .PARAMETER Text
        The prompt or payload text to scan.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [AllowEmptyString()]
        [string] $Text
    )

    $match = [regex]::Match([string] $Text, $script:FeatureFolderPattern)
    if (-not $match.Success) {
        return $null
    }
    return $match.Value.TrimEnd('.')
}

function Find-WorktreeResolutionBranchSignal {
    <#
    .SYNOPSIS
        Return the first branch named by --head, --branch, or a "branch:" label, or $null.
    .PARAMETER Text
        The prompt or payload text to scan.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [AllowEmptyString()]
        [string] $Text
    )

    $match = [regex]::Match([string] $Text, $script:BranchPattern)
    if (-not $match.Success) {
        return $null
    }
    return $match.Groups['name'].Value.TrimEnd('.', '/')
}

function Find-WorktreeResolutionFilePathSignal {
    <#
    .SYNOPSIS
        Return the first absolute file path (with an extension) in a text, or $null.
    .PARAMETER Text
        The prompt or payload text to scan.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowNull()]
        [AllowEmptyString()]
        [string] $Text
    )

    $match = [regex]::Match([string] $Text, $script:FilePathPattern)
    if (-not $match.Success) {
        return $null
    }
    return $match.Value
}

function Get-WorktreeResolutionSignalCandidate {
    # The distinct worktree roots one present signal resolves to. An absolute path
    # is placed by the upward walk; a relative path is looked up in every worktree
    # reachable from the session root; a branch is matched against each HEAD.
    [OutputType([string[]])]
    param([hashtable] $Signal, [string] $SessionRoot)

    if ($Signal.Kind -eq $script:SignalBranch) {
        $byBranch = Get-WorktreeResolutionWorktreeRoot -SessionRoot $SessionRoot -Branch $Signal.Value
        return , [string[]] @($byBranch)
    }
    $normalized = ConvertTo-WorktreeResolutionNormalizedPath -Path $Signal.Value
    if ($normalized -match $script:AbsolutePathPattern) {
        $placed = ConvertTo-WorktreeResolutionRepoRelativePath -Path $normalized
        if (-not $placed.IsNormalized) {
            return , [string[]] @()
        }
        return , [string[]] @($placed.WorktreeRoot)
    }
    $byPath = Get-WorktreeResolutionWorktreeRoot -SessionRoot $SessionRoot -RepoRelativePath $normalized
    return , [string[]] @($byPath)
}

function Resolve-WorktreeCallTarget {
    <#
    .SYNOPSIS
        Derive the worktree a tool call pertains to, as one of four states.
    .DESCRIPTION
        Always returns a target result object. A call naming no signal is NoTarget.
        A present signal that places in no worktree or in several, or two present
        signals that place in different worktrees, is Ambiguous. Otherwise the one
        agreed root is SessionRoot when it is the session root and OtherWorktree
        when it is not. Signals are reported in the precedence FeatureFolderPath,
        FilePath, Branch, which never suppresses a disagreement.
    .PARAMETER Text
        Optional prompt or payload text scanned for all three signal kinds.
    .PARAMETER Branch
        Optional explicit branch; takes the place of a branch read from Text.
    .PARAMETER FilePath
        Optional explicit file path, relative or absolute; takes the place of a
        file path read from Text.
    .PARAMETER SessionRoot
        Optional. Defaults to the worktree containing the current location.
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [AllowNull()]
        [AllowEmptyString()]
        [string] $Text,
        [string] $Branch,
        [string] $FilePath,
        [string] $SessionRoot
    )

    $session = ConvertTo-WorktreeResolutionNormalizedPath -Path $SessionRoot
    if ($null -eq $session) {
        $current = (Get-Location).ProviderPath
        $session = Find-WorktreeResolutionRoot -Path $current
        if ($null -eq $session) {
            $session = ConvertTo-WorktreeResolutionNormalizedPath -Path $current
        }
    }

    $filePathValue = if ([string]::IsNullOrWhiteSpace($FilePath)) { Find-WorktreeResolutionFilePathSignal -Text $Text } else { $FilePath.Trim() }
    $branchValue = if ([string]::IsNullOrWhiteSpace($Branch)) { Find-WorktreeResolutionBranchSignal -Text $Text } else { $Branch.Trim() }
    $present = @(
        @{ Kind = $script:SignalFeatureFolderPath; Value = (Find-WorktreeResolutionFeatureFolderSignal -Text $Text) }
        @{ Kind = $script:SignalFilePath; Value = $filePathValue }
        @{ Kind = $script:SignalBranch; Value = $branchValue }
    ) | Where-Object { -not [string]::IsNullOrWhiteSpace($_.Value) }
    $present = @($present)
    if ($present.Count -eq 0) {
        return (New-WorktreeResolutionTargetResult -Status $script:StatusNoTarget -SessionRoot $session -Detail "the call names no feature folder, file path, or branch, so it has no target and the session root '$session' applies")
    }

    $agreed = [System.Collections.Generic.List[string]]::new()
    foreach ($signal in $present) {
        $label = $script:SignalLabel[$signal.Kind]
        $candidates = Get-WorktreeResolutionSignalCandidate -Signal $signal -SessionRoot $session
        if ($candidates.Count -ne 1) {
            $detail = "$label '$($signal.Value)' matched $($candidates.Count) candidate worktrees; supply an absolute path inside exactly one worktree to disambiguate"
            return (New-WorktreeResolutionTargetResult -Status $script:StatusAmbiguous -SessionRoot $session -Signal $signal.Kind -SignalValue $signal.Value -Candidate $candidates -Detail $detail)
        }
        if (-not ($agreed -contains $candidates[0])) {
            $agreed.Add($candidates[0])
        }
    }

    $reported = $present[0]
    $reportedLabel = $script:SignalLabel[$reported.Kind]
    if ($agreed.Count -gt 1) {
        $detail = "the call's signals resolve to $($agreed.Count) different worktrees ($($agreed -join ', ')); precedence does not choose between disagreeing signals"
        return (New-WorktreeResolutionTargetResult -Status $script:StatusAmbiguous -SessionRoot $session -Signal $reported.Kind -SignalValue $reported.Value -Candidate $agreed.ToArray() -Detail $detail)
    }
    $root = $agreed[0]
    $status = if ($root -eq $session) { $script:StatusSessionRoot } else { $script:StatusOtherWorktree }
    $detail = "$reportedLabel '$($reported.Value)' resolves to worktree '$root'"
    return (New-WorktreeResolutionTargetResult -Status $status -SessionRoot $session -WorktreeRoot $root -Signal $reported.Kind -SignalValue $reported.Value -Candidate @($root) -Detail $detail)
}

function Join-WorktreeResolutionPath {
    <#
    .SYNOPSIS
        Compose an absolute forward-slash path from a worktree root and a repo-relative path.
    .PARAMETER WorktreeRoot
        The absolute worktree root, in either separator style.
    .PARAMETER RepoRelativePath
        The path below the root, in either separator style.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory = $true)]
        [string] $WorktreeRoot,
        [Parameter(Mandatory = $true)]
        [string] $RepoRelativePath
    )

    $root = ConvertTo-WorktreeResolutionNormalizedPath -Path $WorktreeRoot
    if ($root -notmatch $script:AbsolutePathPattern) {
        throw "WorktreeRoot must be an absolute path: '$WorktreeRoot'."
    }
    $relative = ConvertTo-WorktreeResolutionNormalizedPath -Path $RepoRelativePath
    if ($null -eq $relative) {
        return $root
    }
    $separator = if ($root.EndsWith('/')) { '' } else { '/' }
    return $root + $separator + $relative.TrimStart('/')
}

Export-ModuleMember -Function `
    New-WorktreeResolutionTargetResult, `
    Find-WorktreeResolutionFeatureFolderSignal, `
    Find-WorktreeResolutionBranchSignal, `
    Find-WorktreeResolutionFilePathSignal, `
    Resolve-WorktreeCallTarget, `
    Join-WorktreeResolutionPath
