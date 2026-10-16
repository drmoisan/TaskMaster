<#
.SYNOPSIS
    Artifact-root resolution and the Check 1 body-path binding for enforce-pr-author-skill.ps1 (issue #850).

.DESCRIPTION
    Dot-sourced by enforce-pr-author-skill-helpers.ps1 after its module imports. Holds:

    - Resolve-PrAuthorArtifactRoot, which resolves once per call the worktree whose
      artifacts/ tree the gate reads: the epic checkpoint's worktree under epic scope, or
      the item worktree identity resolution selects. The PR context summary, the body, and
      the receipt are composed beneath that root, so a gate running in the session worktree
      no longer reads another item's artifacts from its own process directory.
    - Get-PrAuthorBodyPathBindingReason, the Check 1 binding: a relative canonical
      --body-file value is accepted only when the resolved worktree is the session worktree,
      because gh reads a relative path from its own process directory; an absolute value is
      accepted only when it equals the canonical path beneath the resolved root.
    - Test-PrAuthorBodyPathEqual and Get-PrAuthorEpicArtifactRoot, the pure path helpers.
    - ConvertTo-PrAuthorBodyNumber, the strict body-number parse (issue #901, #850 note L19).

.NOTES
    PowerShell 7+. Depends, at call time, on Get-PrAuthorBodyFileValue and
    Get-PrAuthorTargetCheckpointResolution from the helpers file and on the worktree-resolution
    modules it imports. It reads no file. Mirrored byte-identically under
    extensions/drm-copilot/resources/claude-customizations/.
#>
[CmdletBinding()]
param()

function Test-PrAuthorBodyPathEqual {
    <#
    .SYNOPSIS
        Compare two paths: case-insensitive for drive-letter and UNC paths, case-sensitive otherwise.
    .PARAMETER Left
        The first path, in either separator style.
    .PARAMETER Right
        The second path, in either separator style.
    .OUTPUTS
        System.Boolean
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [AllowNull()][AllowEmptyString()][string] $Left,
        [AllowNull()][AllowEmptyString()][string] $Right
    )

    $a = ConvertTo-WorktreeResolutionNormalizedPath -Path $Left
    $b = ConvertTo-WorktreeResolutionNormalizedPath -Path $Right
    if ($null -eq $a -or $null -eq $b) {
        return $false
    }
    $comparison = if ($a -match '^([A-Za-z]:|//)' -or $b -match '^([A-Za-z]:|//)') {
        [System.StringComparison]::OrdinalIgnoreCase
    }
    else {
        [System.StringComparison]::Ordinal
    }
    return [string]::Equals($a, $b, $comparison)
}

function Get-PrAuthorEpicArtifactRoot {
    <#
    .SYNOPSIS
        Return the worktree root that holds an epic checkpoint path, or $null.
    .PARAMETER CheckpointPath
        The absolute epic checkpoint path the epic-scope resolver composed.
    .OUTPUTS
        System.String or $null
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [AllowNull()][AllowEmptyString()][string] $CheckpointPath
    )

    $normalized = ConvertTo-WorktreeResolutionNormalizedPath -Path $CheckpointPath
    $suffix = '/' + (Get-EpicScopeCheckpointRelativePath)
    if ($null -eq $normalized -or -not $normalized.EndsWith($suffix, [System.StringComparison]::Ordinal)) {
        return $null
    }
    $root = $normalized.Substring(0, $normalized.Length - $suffix.Length)
    if ($root.Length -eq 0) {
        return $null
    }
    return $root
}

function Resolve-PrAuthorArtifactRoot {
    <#
    .SYNOPSIS
        Resolve, once per call, the worktree whose artifacts the gate reads (issue #850).
    .DESCRIPTION
        Epic scope: the root is the worktree holding the epic checkpoint, and a relative body
        path is allowed only when that root is the session worktree. Otherwise the item target
        is resolved by identity; an unresolved target returns its resolution reason unchanged,
        and a relative body path is allowed only for a SessionRoot target.
    .PARAMETER CommandText
        The Bash command text under evaluation.
    .OUTPUTS
        System.Collections.Specialized.OrderedDictionary with Reason, ArtifactRoot,
        CheckpointPath, EpicScope, Status, and RelativeBodyAllowed.
    #>
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param(
        [Parameter(Mandatory)]
        [string] $CommandText
    )

    $epicScope = Resolve-EpicScopeCheckpoint -Text $CommandText -SessionRoot (Get-Location).Path
    if ($null -ne $epicScope -and $epicScope.IsEpicScope) {
        $root = Get-PrAuthorEpicArtifactRoot -CheckpointPath $epicScope.CheckpointPath
        $reason = $null
        $relativeAllowed = $false
        if ($null -eq $root) {
            $reason = "ORCHESTRATOR_STATE_PREFLIGHT_FAILED: the epic checkpoint path '$($epicScope.CheckpointPath)' does not end with '$(Get-EpicScopeCheckpointRelativePath)', so the worktree that holds it cannot be derived."
        }
        else {
            $relativeAllowed = Test-PrAuthorBodyPathEqual -Left $root -Right $epicScope.WorktreeRoot
        }
        return [ordered]@{
            Reason              = $reason
            ArtifactRoot        = $root
            CheckpointPath      = $epicScope.CheckpointPath
            EpicScope           = $epicScope
            Status              = 'Epic'
            RelativeBodyAllowed = $relativeAllowed
        }
    }

    $resolution = Get-PrAuthorTargetCheckpointResolution -CommandText $CommandText
    $resolved = -not $resolution.Reason
    return [ordered]@{
        Reason              = $resolution.Reason
        ArtifactRoot        = if ($resolved) { ConvertTo-WorktreeResolutionNormalizedPath -Path $resolution.WorktreeRoot } else { $null }
        CheckpointPath      = $resolution.CheckpointPath
        EpicScope           = $null
        Status              = $resolution.Status
        RelativeBodyAllowed = ($resolved -and $resolution.Status -eq 'SessionRoot')
    }
}

function ConvertTo-PrAuthorBodyNumber {
    <#
    .SYNOPSIS
        Parse the <N> of artifacts/pr_body_<N>.md, or return $null.
    .DESCRIPTION
        Accepts only ASCII digits that parse as an Int32 and whose canonical invariant spelling
        equals the input, so a value above Int32.MaxValue, a leading zero, and a non-ASCII
        decimal digit all return $null instead of throwing or naming a different file.
    .PARAMETER Digits
        The digit run captured from the body-file name.
    .OUTPUTS
        System.Int32 or $null
    #>
    [CmdletBinding()]
    [OutputType([int])]
    param(
        [AllowNull()][AllowEmptyString()][string] $Digits
    )

    $number = 0
    $parsed = [int]::TryParse($Digits, [System.Globalization.NumberStyles]::None, [System.Globalization.CultureInfo]::InvariantCulture, [ref] $number)
    if (-not $parsed -or $number.ToString([System.Globalization.CultureInfo]::InvariantCulture) -cne $Digits) {
        return $null
    }
    return $number
}

function Get-PrAuthorBodyPathBindingReason {
    <#
    .SYNOPSIS
        Check 1: bind the --body-file value to the canonical body path beneath the artifact root.
    .DESCRIPTION
        Reads the value once through Get-PrAuthorBodyFileValue. A relative
        artifacts/pr_body_<N>.md (case-sensitive) is accepted only when RelativeBodyAllowed is
        true; otherwise the deny names the absolute canonical path to pass instead. An absolute
        value is accepted only when it equals that canonical path under Test-PrAuthorBodyPathEqual.
        Any other value, including no readable value, denies with the generic text.
    .PARAMETER CommandText
        The Bash command text under evaluation.
    .PARAMETER ArtifactRoot
        The resolved worktree root the artifacts are read beneath.
    .PARAMETER RelativeBodyAllowed
        Whether a relative body path is read from the resolved worktree.
    .OUTPUTS
        System.Collections.Specialized.OrderedDictionary with Reason and BodyNumber.
    #>
    [CmdletBinding()]
    [OutputType([System.Collections.Specialized.OrderedDictionary])]
    param(
        [Parameter(Mandatory)]
        [string] $CommandText,

        [Parameter(Mandatory)]
        [string] $ArtifactRoot,

        [bool] $RelativeBodyAllowed
    )

    $generic = "PR_BODY_PATH_NONCANONICAL: ``--body-file`` must reference the canonical ``artifacts/pr_body_<N>.md`` file produced by the pr-author skill beneath the target worktree '$ArtifactRoot' (relative only when that worktree is the session worktree). The path supplied does not match."
    $value = ConvertTo-WorktreeResolutionNormalizedPath -Path (Get-PrAuthorBodyFileValue -CommandText $CommandText)
    if ($null -eq $value) {
        return [ordered]@{ Reason = $generic; BodyNumber = $null }
    }

    if ($value -cmatch '^artifacts/pr_body_(\d+)\.md$') {
        $number = ConvertTo-PrAuthorBodyNumber -Digits $Matches[1]
        if ($null -eq $number) {
            return [ordered]@{ Reason = $generic; BodyNumber = $null }
        }
        if (-not $RelativeBodyAllowed) {
            $canonical = Join-WorktreeResolutionPath -WorktreeRoot $ArtifactRoot -RepoRelativePath "artifacts/pr_body_$number.md"
            return [ordered]@{
                Reason     = "PR_BODY_PATH_NONCANONICAL: the target worktree '$ArtifactRoot' is not the session worktree, so gh would read a relative --body-file from the session worktree. Pass the absolute canonical path '$canonical'."
                BodyNumber = $null
            }
        }
        return [ordered]@{ Reason = $null; BodyNumber = $number }
    }

    if ($value -match '/artifacts/pr_body_(\d+)\.md$') {
        $number = ConvertTo-PrAuthorBodyNumber -Digits $Matches[1]
        if ($null -ne $number) {
            $canonical = Join-WorktreeResolutionPath -WorktreeRoot $ArtifactRoot -RepoRelativePath "artifacts/pr_body_$number.md"
            if (Test-PrAuthorBodyPathEqual -Left $value -Right $canonical) {
                return [ordered]@{ Reason = $null; BodyNumber = $number }
            }
        }
    }

    return [ordered]@{ Reason = $generic; BodyNumber = $null }
}
