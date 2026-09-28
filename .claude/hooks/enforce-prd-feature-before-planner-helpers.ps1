<#
.SYNOPSIS
    Prompt-side feature-folder resolution for the prd-feature gate (issue #672).
.DESCRIPTION
    This file is dot-sourced by its parent hook enforce-prd-feature-before-planner.ps1,
    following the headroom-split precedent set by
    enforce-orchestration-preimplementation-gate-helpers.ps1. The parent keeps the
    decision entrypoint and the seams the existing suites mock; this file carries the
    resolution and prerequisite-mapping logic, so neither file approaches the 500-line
    cap as target resolution grows.

    The file declares no file-scope parameter block, no requires directive, and no
    entrypoint. Apart from its function declarations,
    its only file-scope statement is the resolution-module import
    that follows this block, which brings in issue #669's path normalisation and
    ambiguity reason code. That import is deliberately unguarded, for the same
    reason the parent hook's is: a resolution module that cannot be loaded is
    itself the target-not-resolvable state, so letting the failure surface fails
    the gate closed rather than degrading it to a permissive path.
.NOTES
    Compatible with PowerShell 7+. Read-only resolution logic.
#>

# Issue #669 owns path normalisation and the ambiguity reason code. The import is
# unguarded for the same reason the parent's is: a resolution module that cannot be
# loaded is itself the target-not-resolvable state.
Import-Module (Join-Path $PSScriptRoot '../lib/worktree-resolution/WorktreeResolution.psm1') -Force

function Resolve-PrdFeatureWorkMode {
    <#
    .SYNOPSIS
        Parses the persisted `- Work Mode: ...` marker out of issue.md content
        and returns the canonical mode, or $null when the marker is absent,
        unreadable, or unrecognized.
    .DESCRIPTION
        Recognizes minor-audit, full-feature, full-bug, and the legacy full
        marker (normalized to full-feature), mirroring the regex convention
        used by scripts/dev_tools/prompt_mode_contract.py so both runtimes
        agree on what counts as a valid marker line.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [AllowNull()]
        [string] $IssueContent
    )

    if ([string]::IsNullOrWhiteSpace($IssueContent)) {
        return $null
    }

    $match = [regex]::Match($IssueContent, '(?im)^-\s*Work Mode:\s*(minor-audit|full-feature|full-bug|full)\s*$')
    if (-not $match.Success) {
        return $null
    }

    $rawMode = $match.Groups[1].Value
    if ($rawMode -eq 'full') {
        return 'full-feature'
    }
    return $rawMode
}

function Get-PrdFeatureRequiredFile {
    <#
    .SYNOPSIS
        Maps a resolved work mode to the set of prd-feature output files the
        target folder must contain before an atomic-planner delegation is
        allowed.
    .DESCRIPTION
        full-feature requires spec.md and user-story.md; full-bug requires
        spec.md only; minor-audit requires neither.

        The default arm returns spec.md alone. It is not reached from the
        decision path for an undeterminable mode, which denies on its own branch
        without probing at all; the arm exists so a direct caller passing a $null
        or unrecognized mode never receives a permissive empty set, and it must
        not name user-story.md, because that document is required to be ABSENT
        for full-bug and minor-audit work.
    #>
    [CmdletBinding()]
    [OutputType([string[]])]
    param(
        [AllowNull()]
        [string] $WorkMode
    )

    # Route on the canonical mode; anything outside the three known values
    # (including $null) falls through to the fail-closed default case.
    switch ($WorkMode) {
        'full-feature' { return [string[]]@('spec.md', 'user-story.md') }
        'full-bug' { return [string[]]@('spec.md') }
        'minor-audit' { return [string[]]@() }
        default { return [string[]]@('spec.md') }
    }
}

function ConvertTo-PrdFeatureFolderToken {
    <#
    .SYNOPSIS
        Reduces one cited path token to its four-segment repo-relative feature
        folder, or $null when the token does not name one.
    .DESCRIPTION
        The prefix half is issue #669's path normalisation, not a local
        re-implementation: ConvertTo-WorktreeResolutionRepoRelativePath places an
        absolute token by locating its containing worktree and returns the full
        remainder below that root, so no segment is dropped and no prefix is
        discarded by pattern shape. A token that does not normalise is already
        repo-relative (or names no worktree), and the repo-relative remainder is
        then read from the token itself.

        The depth-collapsing half below is carried byte-unmodified from the fix
        for issue #518. It is depth normalisation, not the segment-count
        truncation the epic prohibits: it maps a folder path, a spec.md path, a
        research/ artifact path, and an evidence/ artifact path onto the same
        folder.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory)]
        [AllowEmptyString()]
        [string] $Path
    )

    if (-not $Path) {
        return $null
    }

    $placed = ConvertTo-WorktreeResolutionRepoRelativePath -Path $Path
    $normalized = if ($placed.IsNormalized) {
        $placed.RepoRelativePath
    }
    else {
        ($Path -replace '\\', '/')
    }
    $normalized = $normalized.TrimEnd('/')

    # Read the feature-folder remainder from wherever it starts in the normalised
    # token. A normalised token starts at it already; a token that named no
    # worktree keeps whatever prefix it was cited with.
    $start = $normalized.IndexOf('docs/features/active/')
    if ($start -lt 0) {
        return $null
    }
    $normalized = $normalized.Substring($start)

    # Truncate to exactly two segments past the docs/features/active/ prefix,
    # that is, to the four segments docs, features, active, and the feature
    # folder name. Truncation is depth-insensitive, so a folder path, a
    # spec.md path, a research/ artifact path, and an evidence/ artifact path
    # all reduce to the same value. A '.' component is a path no-op and is
    # discarded first, so a degenerate token such as docs/features/active/.
    # yields three segments and is rejected rather than resolved.
    $segments = @($normalized -split '/' | Where-Object { $_ -ne '' -and $_ -ne '.' })
    if ($segments.Count -lt 4) {
        return $null
    }

    return ($segments[0..3] -join '/')
}

function Find-PrdFeatureFolderCandidate {
    <#
    .SYNOPSIS
        Returns the distinct feature folders a prompt cites, in first-occurrence
        order.
    .DESCRIPTION
        The scan pattern keeps any prefix the token was cited with, so an absolute
        citation reaches the normalisation in ConvertTo-PrdFeatureFolderToken
        rather than being discarded by the pattern itself.
    #>
    [CmdletBinding()]
    [OutputType([string[]])]
    param(
        [Parameter(Mandatory)]
        [AllowEmptyString()]
        [string] $Prompt
    )

    if (-not $Prompt) {
        return [string[]]@()
    }

    # Allow forward or backslash separators inside the matched path token, and
    # keep any prefix the token carries.
    $pattern = '(?:[^\s"''`]*[\\/])?docs[\\/]+features[\\/]+active[\\/]+[^\s"''`]+'
    $matchList = [regex]::Matches($Prompt, $pattern)
    if ($matchList.Count -eq 0) {
        return [string[]]@()
    }

    # Deduplicate preserving FIRST-OCCURRENCE order. A [hashtable] must not be
    # used here: PowerShell hashtable key enumeration order is unspecified, so a
    # first-occurrence selection rule fed by a hashtable is not deterministic.
    [System.Collections.Generic.List[string]] $candidates = [System.Collections.Generic.List[string]]::new()
    foreach ($m in $matchList) {
        $truncated = ConvertTo-PrdFeatureFolderToken -Path $m.Value
        if (-not $truncated) {
            continue
        }

        if (-not $candidates.Contains($truncated)) {
            $candidates.Add($truncated)
        }
    }

    return [string[]] $candidates.ToArray()
}

function Select-PrdFeatureFolderByCheckpoint {
    <#
    .SYNOPSIS
        Returns the candidate the resolved worktree's checkpoint names, or $null when
        the checkpoint names none of them.
    .DESCRIPTION
        The disambiguator is the orchestrator checkpoint of the worktree identity
        resolution selected, which is the item's own. Neither prompt position nor a
        foreign session's record takes that role: position would make selection an
        accident of wording, and a checkpoint belonging to some other session would
        choose between two cited folders on the strength of that session's record of
        its own unrelated work.

        Before issue #673 this function read a signal value off a call target the gate
        had derived from a path token. That is gone: a path no longer selects a
        worktree, so there is no derived signal to read.
    .PARAMETER Candidate
        The distinct feature folders cited in the prompt.
    .PARAMETER CheckpointFolder
        The feature-folder value read from the resolved worktree's checkpoint.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory)]
        [AllowEmptyCollection()]
        [string[]] $Candidate,

        [AllowNull()]
        [string] $CheckpointFolder
    )

    $folder = ConvertTo-PrdFeatureFolderToken -Path $CheckpointFolder
    if ($folder -and ($Candidate -contains $folder)) {
        return $folder
    }

    return $null
}

function Find-PrdFeatureFolderFromPrompt {
    <#
    .SYNOPSIS
        Scans a prompt string for docs/features/active/<...> path tokens,
        truncates each to exactly four path segments, and returns the selected
        feature folder. Returns $null when no token truncates to four segments
        and when a multi-candidate tie cannot be resolved against the resolved
        worktree's checkpoint.
    .DESCRIPTION
        Truncation to four segments -- docs, features, active, and the
        feature-folder name -- is two segments past the docs/features/active/
        prefix, so the depth at which an artifact is cited cannot change which
        folder is resolved. Candidates are deduplicated preserving
        first-occurrence order; selection among two or more distinct candidates
        is made against the checkpoint of the worktree identity resolution
        selected, which is the item's own, and an unresolved tie returns $null so
        the caller can deny with the ambiguity code rather than guess.

        The return value is a repo-relative path normalized to forward slashes,
        or $null. The function reads no file except through issue #669's
        normalisation, and it is deterministic for a given prompt and checkpoint
        value.
    .PARAMETER Prompt
        The delegation prompt to scan.
    .PARAMETER CheckpointFolder
        Optional. The feature-folder value read from the resolved worktree's
        checkpoint, used only to break a multi-candidate tie.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory)]
        [AllowEmptyString()]
        [string] $Prompt,

        [AllowNull()]
        [string] $CheckpointFolder
    )

    $candidates = @(Find-PrdFeatureFolderCandidate -Prompt $Prompt)
    if ($candidates.Count -eq 0) {
        return $null
    }

    # One distinct candidate is used directly, so the common case never consults
    # the checkpoint at all.
    if ($candidates.Count -eq 1) {
        return $candidates[0]
    }

    # More than one distinct feature folder was cited. The resolved worktree's own
    # checkpoint is the only disambiguator; an unresolved tie is ambiguous and is
    # reported as such by returning $null, rather than resolved positionally.
    return (Select-PrdFeatureFolderByCheckpoint -Candidate $candidates -CheckpointFolder $CheckpointFolder)
}

function Get-PrdFeatureMissingFile {
    <#
    .SYNOPSIS
        Returns the subset of $RequiredFile that is missing from the target
        folder.
    #>
    [CmdletBinding()]
    [OutputType([string[]])]
    param(
        [Parameter(Mandatory)]
        [string] $FeatureFolder,

        [Parameter(Mandatory)]
        [AllowEmptyCollection()]
        [string[]] $RequiredFile
    )

    [System.Collections.Generic.List[string]] $missing = [System.Collections.Generic.List[string]]::new()
    foreach ($name in $RequiredFile) {
        $candidate = "$FeatureFolder/$name"
        if (-not (Get-PrdFeatureFileExistence -Path $candidate)) {
            $missing.Add($name)
        }
    }
    return [string[]] $missing.ToArray()
}
