<#
.SYNOPSIS
    Portable-identity resolution of the item a gated call acts on (issue #673).

.DESCRIPTION
    Selects the worktree a call pertains to from portable identity alone: the
    canonical issue number in the call text, matched against the issue-num of each
    live worktree's orchestrator checkpoint, and a branch signal. A feature-folder
    path and a file path are never read as worktree selectors, because a merged
    feature folder exists in every checkout branched from main and so places a call
    in many worktrees at once; it names what the work is, not where it lives.

    A worktree is live when it is registered in the repository's worktree
    administrative layout and its root still carries a worktree marker, so a deleted
    directory is excluded. No identity, or one matching no live worktree, resolves
    NoTarget; several live matches or disagreeing identities resolve Ambiguous. A
    branch signal is the only tie-breaker, because git checks a branch out in at most
    one worktree; no timestamp or lifecycle step is ever read to break a tie.

.NOTES
    PowerShell 7+. Re-uses its siblings for enumeration, the marker test, the ascent,
    the branch reader, normalisation, the join, the result constructor, and both
    reason-code accessors, re-implementing none. Neither reason-code literal appears
    here. Its only filesystem read is the checkpoint-text seam. Mirrored byte-identically.
    CONVENTION: this module fails fast at module scope and imports its siblings with -ErrorAction Stop.
#>

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

Import-Module (Join-Path $PSScriptRoot 'WorktreeResolution.psm1') -ErrorAction Stop
Import-Module (Join-Path $PSScriptRoot 'WorktreeTargetResolution.psm1') -ErrorAction Stop

# The one definition of the checkpoint location relative to a worktree root; every
# consumer composes an absolute path from it and a resolved root.
$script:CheckpointRelativePath = 'artifacts/orchestration/orchestrator-state.json'

# The canonical issue-number line an orchestrator writes into a delegation prompt.
# Case-sensitive on purpose: a lowercase near-miss is not the contract sentence.
$script:IssueSignalPattern = 'Canonical issue number for this feature is #?(?<number>\d+)'

# An issue number as a caller may spell it: optional hash, then digits only.
$script:IssueNumberPattern = '^#?(?<digits>\d+)$'

function Get-WorktreeItemCheckpointRelativePath {
    <#
    .SYNOPSIS
        Return the repository-relative path of the orchestrator checkpoint.
    .DESCRIPTION
        The single definition of that literal; an absolute path comes from Get-WorktreeItemCheckpointPath.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param()

    return $script:CheckpointRelativePath
}

function Get-WorktreeItemCheckpointPath {
    <#
    .SYNOPSIS
        Compose the absolute checkpoint path under a worktree root.
    .DESCRIPTION
        Returns a forward-slash absolute path, and throws when the root is not absolute:
        a relative root would reintroduce the process-directory binding this removes.
    .PARAMETER WorktreeRoot
        The absolute worktree root, in either separator style.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory = $true)] [string] $WorktreeRoot
    )

    return (Join-WorktreeResolutionPath -WorktreeRoot $WorktreeRoot -RepoRelativePath (Get-WorktreeItemCheckpointRelativePath))
}

function ConvertTo-WorktreeItemIssueNumber {
    <#
    .SYNOPSIS
        Normalise a value to a positive issue number as a digit string, or $null.
    .DESCRIPTION
        Pure. Accepts an integer, or a string of optional hash plus digits with
        whitespace trimmed, and returns the number without leading zeros. Returns $null
        for anything else, including zero, a negative value, none, and an empty value.
    .PARAMETER Value
        The candidate value, of any type.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory = $true)] [AllowNull()] [AllowEmptyString()] [object] $Value
    )

    if ($null -eq $Value) { return $null }
    $match = [regex]::Match(([string] $Value).Trim(), $script:IssueNumberPattern)
    if (-not $match.Success) { return $null }
    # A digit run wider than the largest integer is rejected rather than wrapped.
    $number = [long] 0
    if (-not [long]::TryParse($match.Groups['digits'].Value, [ref] $number)) { return $null }
    if ($number -le 0) { return $null }
    return $number.ToString([System.Globalization.CultureInfo]::InvariantCulture)
}

function Find-WorktreeItemIssueSignal {
    <#
    .SYNOPSIS
        Return the distinct issue numbers named by canonical issue lines in a text.
    .DESCRIPTION
        Scans case-sensitively for the canonical issue-number sentence, returning each
        distinct number in first-appearance order as one object a caller assigns first.
    .PARAMETER Text
        The prompt or command text to scan.
    #>
    [CmdletBinding()]
    [OutputType([string[]], [object[]])]
    param(
        [Parameter(Mandatory = $true)] [AllowNull()] [AllowEmptyString()] [string] $Text
    )

    $found = [System.Collections.Generic.List[string]]::new()
    # Collect each canonical line's number in first-appearance order, discarding a
    # repeat. Order matters: the ambiguity detail lists them as the caller wrote them.
    foreach ($match in [regex]::Matches([string] $Text, $script:IssueSignalPattern)) {
        $number = ConvertTo-WorktreeItemIssueNumber -Value $match.Groups['number'].Value
        if ($null -ne $number -and -not $found.Contains($number)) { $found.Add($number) }
    }
    return , [string[]] $found.ToArray()
}

function Get-WorktreeItemCheckpointText {
    <#
    .SYNOPSIS
        Return the raw text of a file, or $null when it is absent or unreadable.
    .DESCRIPTION
        The module's only filesystem read, isolated as the seam a test mocks.
    .PARAMETER Path
        The absolute path to read.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory = $true)] [AllowNull()] [AllowEmptyString()] [string] $Path
    )

    if ([string]::IsNullOrWhiteSpace($Path)) { return $null }
    if (-not (Test-Path -LiteralPath $Path -PathType Leaf -ErrorAction SilentlyContinue)) { return $null }
    try { return [System.IO.File]::ReadAllText($Path) } catch { return $null }
}

function Get-WorktreeItemCheckpointIssue {
    <#
    .SYNOPSIS
        Return the issue number recorded by a worktree's checkpoint, or $null.
    .DESCRIPTION
        Reads only the canonical checkpoint path beneath the given root, so an archived
        checkpoint is never matched. Returns $null when the file is absent, empty,
        unparseable, not an object, or carries no issue-num key.
    .PARAMETER WorktreeRoot
        The absolute worktree root whose checkpoint is read.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory = $true)] [string] $WorktreeRoot
    )

    $text = Get-WorktreeItemCheckpointText -Path (Get-WorktreeItemCheckpointPath -WorktreeRoot $WorktreeRoot)
    if ([string]::IsNullOrWhiteSpace($text)) { return $null }
    $payload = $null
    try { $payload = $text | ConvertFrom-Json } catch { return $null }
    # A parse that yielded nothing, one that yielded a scalar or array, and a missing
    # key all mean the same to a caller: this worktree records no identifiable issue.
    if ($null -eq $payload -or $payload -isnot [pscustomobject]) { return $null }
    if ($payload.PSObject.Properties.Name -notcontains 'issue-num') { return $null }
    return (ConvertTo-WorktreeItemIssueNumber -Value $payload.'issue-num')
}

function Get-WorktreeItemLiveRoot {
    <#
    .SYNOPSIS
        Return the live worktree roots reachable from a session path.
    .DESCRIPTION
        Seam. Ascends from the session path to its enclosing worktree root, enumerates
        the registered roots, and keeps those still carrying a worktree marker. Empty
        when the path lies inside no worktree, and returned as one object.
    .PARAMETER SessionRoot
        The calling process's path, used only to find the repository.
    .PARAMETER Branch
        Optional. Keep only the roots that have this branch checked out.
    #>
    [CmdletBinding()]
    [OutputType([string[]], [object[]])]
    param(
        [Parameter(Mandatory = $true)] [AllowEmptyString()] [string] $SessionRoot,
        [string] $Branch
    )

    $ascended = Find-WorktreeResolutionRoot -Path $SessionRoot
    if ($null -eq $ascended) { return , [string[]] @() }
    # The enumerator applies the branch filter, being the only code reading each HEAD.
    $registered = if ([string]::IsNullOrWhiteSpace($Branch)) {
        Get-WorktreeResolutionWorktreeRoot -SessionRoot $ascended
    }
    else {
        Get-WorktreeResolutionWorktreeRoot -SessionRoot $ascended -Branch $Branch
    }
    # Drop a registration whose directory is gone; it can hold no checkpoint to match.
    $live = [System.Collections.Generic.List[string]]::new()
    foreach ($root in @($registered)) {
        if (Test-WorktreeResolutionRootMarker -Path $root) { $live.Add($root) }
    }
    return , [string[]] $live.ToArray()
}

# Private: convert a resolved root and session path into a result, labelling it
# SessionRoot or OtherWorktree so that test is written once, as the sibling normalisation
# constructor does. The ConvertTo verb is deliberate: a New- verb is state-changing to
# PSScriptAnalyzer and this function changes nothing.
function ConvertTo-WorktreeItemResolvedResult {
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [Parameter(Mandatory = $true)] [string] $WorktreeRoot,
        [Parameter(Mandatory = $true)] [string] $SessionRoot,
        [Parameter(Mandatory = $true)] [string] $Detail,
        [string] $Branch
    )

    $root = ConvertTo-WorktreeResolutionNormalizedPath -Path $WorktreeRoot
    # The root is the session's own when it equals the session path or the worktree that
    # path sits inside. The ascent runs only on failure, because it reaches the disk.
    $isSession = ($null -ne $root -and $root -eq (ConvertTo-WorktreeResolutionNormalizedPath -Path $SessionRoot))
    if (-not $isSession) {
        $ascended = Find-WorktreeResolutionRoot -Path $SessionRoot
        $isSession = ($null -ne $ascended -and $root -eq $ascended)
    }
    $status = if ($isSession) { 'SessionRoot' } else { 'OtherWorktree' }
    $signal = if ([string]::IsNullOrWhiteSpace($Branch)) { '' } else { 'Branch' }
    return (New-WorktreeResolutionTargetResult -Status $status -SessionRoot $SessionRoot `
            -WorktreeRoot $WorktreeRoot -Signal $signal -SignalValue $Branch -Candidate @($WorktreeRoot) -Detail $Detail)
}

# Private: decide the target when the call carries a branch signal. A branch is checked
# out in at most one worktree, so two live matches is repository state, not caller error.
function Resolve-WorktreeItemTargetByBranch {
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [Parameter(Mandatory = $true)] [string] $Session,
        [Parameter(Mandatory = $true)] [string] $Branch,
        [Parameter(Mandatory = $true)] [AllowEmptyCollection()] [string[]] $Issue
    )

    # Assigned before wrapping, because the seam returns its array as one object;
    # wrapping the call would yield a one-element array holding the array.
    $branchSignalRoots = Get-WorktreeItemLiveRoot -SessionRoot $Session -Branch $Branch
    $branchRoots = @($branchSignalRoots)
    # Zero live matches names no place to act on; more than one is a double checkout.
    if ($branchRoots.Count -eq 0) {
        return (New-WorktreeResolutionTargetResult -Status 'NoTarget' -SessionRoot $Session -Detail (
                "branch '{0}' is checked out in no live worktree" -f $Branch))
    }
    if ($branchRoots.Count -gt 1) {
        return (New-WorktreeResolutionTargetResult -Status 'Ambiguous' -SessionRoot $Session `
                -Signal 'Branch' -SignalValue $Branch -Candidate $branchRoots -Detail (
                "branch '{0}' is checked out in {1} live worktrees" -f $Branch, $branchRoots.Count))
    }

    $target = $branchRoots[0]
    $branchOnly = "branch '{0}' resolves to the live worktree that has it checked out" -f $Branch
    if ($Issue.Count -eq 0) {
        return (ConvertTo-WorktreeItemResolvedResult -WorktreeRoot $target -SessionRoot $Session -Branch $Branch -Detail $branchOnly)
    }

    $number = $Issue[0]
    $recorded = Get-WorktreeItemCheckpointIssue -WorktreeRoot $target
    # The branch worktree agrees with the issue, disagrees, or records none. Agreement
    # breaks a stale-attempt tie; disagreement must not be guessed past.
    if ($null -ne $recorded) {
        if ($recorded -eq $number) {
            return (ConvertTo-WorktreeItemResolvedResult -WorktreeRoot $target -SessionRoot $Session -Branch $Branch -Detail (
                    "branch '{0}' and issue {1} resolve to the same live worktree" -f $Branch, $number))
        }
        return (New-WorktreeResolutionTargetResult -Status 'Ambiguous' -SessionRoot $Session `
                -Signal 'Branch' -SignalValue $Branch -Candidate @($target) -Detail (
                "branch '{0}' places the call in a worktree whose checkpoint records issue {1}, not issue {2}" -f $Branch, $recorded, $number))
    }

    # The branch worktree records no issue, so the number is checked against the other
    # live worktrees; a match there means the two signals disagree about placement.
    $elsewhere = [System.Collections.Generic.List[string]]::new()
    $liveRoots = Get-WorktreeItemLiveRoot -SessionRoot $Session
    foreach ($root in @($liveRoots)) {
        if ($root -ne $target -and (Get-WorktreeItemCheckpointIssue -WorktreeRoot $root) -eq $number) { $elsewhere.Add($root) }
    }
    if ($elsewhere.Count -gt 0) {
        return (New-WorktreeResolutionTargetResult -Status 'Ambiguous' -SessionRoot $Session `
                -Signal 'Branch' -SignalValue $Branch -Candidate (@($target) + $elsewhere.ToArray()) -Detail (
                "branch '{0}' and issue {1} place the call in different worktrees" -f $Branch, $number))
    }
    return (ConvertTo-WorktreeItemResolvedResult -WorktreeRoot $target -SessionRoot $Session -Branch $Branch -Detail $branchOnly)
}

# Private: decide the target when the call carries an issue number and no branch. Two
# live worktrees recording one issue is stale-attempt, and its detail names both remedies.
function Resolve-WorktreeItemTargetByIssue {
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [Parameter(Mandatory = $true)] [string] $Session,
        [Parameter(Mandatory = $true)] [string] $Issue
    )

    # Keep every live worktree whose own checkpoint records this issue. An absent or
    # unreadable checkpoint records nothing, which becomes a deny, not a silent pick.
    $matched = [System.Collections.Generic.List[string]]::new()
    $liveRoots = Get-WorktreeItemLiveRoot -SessionRoot $Session
    foreach ($root in @($liveRoots)) {
        if ((Get-WorktreeItemCheckpointIssue -WorktreeRoot $root) -eq $Issue) { $matched.Add($root) }
    }

    # Zero matches denies for want of a place, one resolves, more is stale-attempt.
    if ($matched.Count -eq 0) {
        return (New-WorktreeResolutionTargetResult -Status 'NoTarget' -SessionRoot $Session -Detail (
                "issue {0} is recorded in the orchestrator checkpoint of no live worktree" -f $Issue))
    }
    if ($matched.Count -eq 1) {
        return (ConvertTo-WorktreeItemResolvedResult -WorktreeRoot $matched[0] -SessionRoot $Session -Detail (
                "issue {0} resolves to the live worktree recording it" -f $Issue))
    }
    $detail = ("issue {0} is recorded in the orchestrator checkpoints of {1} live worktrees; " -f $Issue, $matched.Count) +
    'move the stale checkpoint to artifacts/orchestration/handoff/ or add a branch: label naming the item''s branch'
    return (New-WorktreeResolutionTargetResult -Status 'Ambiguous' -SessionRoot $Session -Candidate $matched.ToArray() -Detail $detail)
}

function Resolve-WorktreeItemTarget {
    <#
    .SYNOPSIS
        Resolve the worktree a gated call acts on, from portable identity only.
    .DESCRIPTION
        Reads the canonical issue number and a branch signal from the call text and
        decides one of the four target states. No path signal is read. The session path
        only finds the repository and labels a resolved result SessionRoot.
    .PARAMETER Text
        The prompt or command text to read identity from.
    .PARAMETER SessionRoot
        Optional. The calling process's path. Defaults to the current location.
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [Parameter(Mandatory = $true)] [AllowNull()] [AllowEmptyString()] [string] $Text,
        [string] $SessionRoot
    )

    $session = if ([string]::IsNullOrWhiteSpace($SessionRoot)) { (Get-Location).ProviderPath } else { $SessionRoot }
    # Assigned before it is wrapped, for the reason recorded in the branch resolver.
    $issueSignal = Find-WorktreeItemIssueSignal -Text $Text
    $issues = @($issueSignal)
    $branch = Find-WorktreeResolutionBranchSignal -Text $Text
    $hasBranch = -not [string]::IsNullOrWhiteSpace($branch)

    # Two identity-shape verdicts precede any enumeration or file read: several issue
    # numbers cannot name one item, and no identity cannot name any.
    if ($issues.Count -gt 1) {
        return (New-WorktreeResolutionTargetResult -Status 'Ambiguous' -SessionRoot $session -Detail (
                "the call names {0} different issue numbers ({1}), and an item has exactly one" -f $issues.Count, ($issues -join ', ')))
    }
    if ($issues.Count -eq 0 -and -not $hasBranch) {
        return (New-WorktreeResolutionTargetResult -Status 'NoTarget' -SessionRoot $session -Detail (
                'the call carries neither a canonical issue number line nor a branch signal ' +
                '(--head, --branch, or branch:), so the item it acts on cannot be identified'))
    }

    # A branch signal is authoritative about placement and can break a stale-attempt
    # tie, so it selects the branch-present rows; otherwise the issue number does.
    if ($hasBranch) {
        return (Resolve-WorktreeItemTargetByBranch -Session $session -Branch $branch -Issue $issues)
    }
    return (Resolve-WorktreeItemTargetByIssue -Session $session -Issue $issues[0])
}

Export-ModuleMember -Function `
    Get-WorktreeItemCheckpointRelativePath, `
    Get-WorktreeItemCheckpointPath, `
    Find-WorktreeItemIssueSignal, `
    ConvertTo-WorktreeItemIssueNumber, `
    Get-WorktreeItemCheckpointText, `
    Get-WorktreeItemCheckpointIssue, `
    Get-WorktreeItemLiveRoot, `
    Resolve-WorktreeItemTarget
