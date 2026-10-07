<#
.SYNOPSIS
    Portable-identity resolution of the epic or parallel run a gated call belongs to (issue #690).

.DESCRIPTION
    Selects the worktree whose run checkpoint governs a call from portable identity
    alone: the integration_branch:, epic_feature_folder:, and parallel_slug: literals of
    a kickoff prompt, or a pull request number or worktree path a command names. A
    checkpoint path is always composed beneath a resolved root, so no read binds to the
    calling process's directory.

    A live worktree matches an epic run when its epic checkpoint records route_id epic
    and the integration branch; several matches are narrowed to the worktree that has
    that branch checked out, because git checks a branch out in at most one worktree. A
    parallel run has no branch, so several matches are ambiguous. A record match keeps
    the live worktrees whose run checkpoint records the pull request number or worktree
    path. There is no session-root-first shortcut: a stale copy at the session root never
    wins over the worktree that owns the run.

.NOTES
    PowerShell 7+. Re-uses its siblings for enumeration, the ascent, normalisation, the
    join, the result constructor, the SessionRoot/OtherWorktree labelling, and both
    reason-code accessors, re-implementing none. Its only filesystem read is the
    checkpoint-text seam. Mirrored byte-identically.
    CONVENTION: this module fails fast at module scope and imports its siblings with -ErrorAction Stop.
#>

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

Import-Module (Join-Path $PSScriptRoot 'WorktreeResolution.psm1') -ErrorAction Stop
Import-Module (Join-Path $PSScriptRoot 'WorktreeTargetResolution.psm1') -ErrorAction Stop
Import-Module (Join-Path $PSScriptRoot 'WorktreeItemResolution.psm1') -ErrorAction Stop

# The run checkpoint locations relative to a worktree root.
$script:EpicCheckpointRelativePath = 'artifacts/orchestration/epic-orchestrator-state.json'
$script:ParallelCheckpointRelativePath = 'artifacts/orchestration/parallel-orchestrator-state.json'

# The route_id each run checkpoint must record.
$script:EpicRouteId = 'epic'
$script:ParallelRouteId = 'parallel'

# Where a stale checkpoint copy is moved so it stops competing with the owning worktree.
$script:HandoffRemedy = 'artifacts/orchestration/handoff/'

# The kickoff literals, matched case-sensitively and never as the tail of a longer word.
$script:IntegrationBranchPattern = '(?<![A-Za-z0-9_])integration_branch:[ \t]*(?<value>\S+)'
$script:EpicSlugPattern = '(?<![A-Za-z0-9_])epic_feature_folder:[ \t]*(?<value>\S+)'
$script:ParallelSlugPattern = '(?<![A-Za-z0-9_])parallel_slug:[ \t]*(?<value>\S+)'

# A drive-rooted or slash-rooted path; anything else is relative to some directory.
$script:AbsolutePathPattern = '^([A-Za-z]:(/|$)|/)'

# Private: the single value a pattern names in a text, or $null when it names none or
# more than one distinct value (a prompt that names two runs identifies neither).
function Get-WorktreeRunSignalValue {
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory = $true)] [AllowEmptyString()] [string] $Text,
        [Parameter(Mandatory = $true)] [string] $Pattern
    )

    $values = [System.Collections.Generic.List[string]]::new()
    foreach ($match in [regex]::Matches($Text, $Pattern)) {
        $value = $match.Groups['value'].Value
        # The kickoff sentence ends the value with a full stop; one is removed.
        if ($value.EndsWith('.')) { $value = $value.Substring(0, $value.Length - 1) }
        if ($value.Length -gt 0 -and -not $values.Contains($value)) { $values.Add($value) }
    }
    if ($values.Count -eq 1) { return $values[0] }
    return $null
}

function Find-WorktreeRunIdentitySignal {
    <#
    .SYNOPSIS
        Read the run identity literals from a prompt.
    .DESCRIPTION
        Pure. Returns an object with IntegrationBranch, EpicSlug, and ParallelSlug, each
        the single distinct value the prompt names for that literal, or $null.
    .PARAMETER Text
        The prompt text to scan.
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [Parameter(Mandatory = $true)] [AllowNull()] [AllowEmptyString()] [string] $Text
    )

    $source = [string] $Text
    return [pscustomobject]@{
        IntegrationBranch = Get-WorktreeRunSignalValue -Text $source -Pattern $script:IntegrationBranchPattern
        EpicSlug          = Get-WorktreeRunSignalValue -Text $source -Pattern $script:EpicSlugPattern
        ParallelSlug      = Get-WorktreeRunSignalValue -Text $source -Pattern $script:ParallelSlugPattern
    }
}

function Get-WorktreeRunCheckpointText {
    <#
    .SYNOPSIS
        Return the raw text of a file, or $null when it is absent or unreadable; an unreadable file also writes a diagnostic to stderr.
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
    # An unreadable file is reported on stderr and treated as absent, so resolution stays fail-closed and the hook's stdout stays JSON-only.
    try { return [System.IO.File]::ReadAllText($Path) }
    catch { [Console]::Error.WriteLine(("WORKTREE_RUN_CHECKPOINT_UNREADABLE: '{0}': {1}" -f $Path, $_.Exception.Message)); return $null }
}

function Get-WorktreeRunCheckpointPath {
    <#
    .SYNOPSIS
        Compose the absolute path of a run or item checkpoint beneath a worktree root.
    .DESCRIPTION
        The single composition of an absolute checkpoint path for the gates. Throws when
        the root is not absolute, through the shared join.
    .PARAMETER Kind
        epic, parallel, or item.
    .PARAMETER WorktreeRoot
        The absolute worktree root, in either separator style.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory = $true)] [ValidateSet('epic', 'parallel', 'item')] [string] $Kind,
        [Parameter(Mandatory = $true)] [string] $WorktreeRoot
    )

    if ($Kind -eq 'item') { return (Get-WorktreeItemCheckpointPath -WorktreeRoot $WorktreeRoot) }
    $relative = if ($Kind -eq 'epic') { $script:EpicCheckpointRelativePath } else { $script:ParallelCheckpointRelativePath }
    return (Join-WorktreeResolutionPath -WorktreeRoot $WorktreeRoot -RepoRelativePath $relative)
}

# Private: parse checkpoint text into an object, or $null for blank, unparseable, or
# non-object text. -NoEnumerate keeps a one-element JSON array from reading as an object.
function ConvertFrom-WorktreeRunCheckpointText {
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [Parameter(Mandatory = $true)] [AllowNull()] [AllowEmptyString()] [string] $Text
    )

    if ([string]::IsNullOrWhiteSpace($Text)) { return $null }
    $parsed = $null
    try { $parsed = ConvertFrom-Json -InputObject $Text -NoEnumerate } catch { return $null }
    # The full type name is tested, because a wrapped array also satisfies [pscustomobject].
    if ($null -eq $parsed -or $parsed -isnot [System.Management.Automation.PSCustomObject]) { return $null }
    return $parsed
}

# Private: a named property of a parsed object, or $null when absent (strict mode would
# otherwise throw on a missing member).
function Get-WorktreeRunPropertyValue {
    [CmdletBinding()]
    [OutputType([object])]
    param(
        [Parameter(Mandatory = $true)] [AllowNull()] [object] $InputObject,
        [Parameter(Mandatory = $true)] [string] $Name
    )

    if ($null -eq $InputObject -or $InputObject -isnot [System.Management.Automation.PSCustomObject]) { return $null }
    if ($InputObject.PSObject.Properties.Name -notcontains $Name) { return $null }
    return $InputObject.$Name
}

# Private: read and parse a live root's run checkpoint of one kind, keeping it only when
# it records the kind's route_id.
function Get-WorktreeRunCheckpoint {
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [Parameter(Mandatory = $true)] [ValidateSet('epic', 'parallel')] [string] $Kind,
        [Parameter(Mandatory = $true)] [string] $WorktreeRoot
    )

    $text = Get-WorktreeRunCheckpointText -Path (Get-WorktreeRunCheckpointPath -Kind $Kind -WorktreeRoot $WorktreeRoot)
    $checkpoint = ConvertFrom-WorktreeRunCheckpointText -Text $text
    $routeId = if ($Kind -eq 'epic') { $script:EpicRouteId } else { $script:ParallelRouteId }
    if ([string] (Get-WorktreeRunPropertyValue -InputObject $checkpoint -Name 'route_id') -cne $routeId) { return $null }
    return $checkpoint
}

# Private: the live roots reachable from the session path, assigned before wrapping
# because the seam returns its array as one object.
function Get-WorktreeRunLiveRoot {
    [CmdletBinding()]
    [OutputType([string[]], [object[]])]
    param(
        [Parameter(Mandatory = $true)] [string] $SessionRoot,
        [string] $Branch
    )

    $liveRoots = if ([string]::IsNullOrWhiteSpace($Branch)) {
        Get-WorktreeItemLiveRoot -SessionRoot $SessionRoot
    }
    else {
        Get-WorktreeItemLiveRoot -SessionRoot $SessionRoot -Branch $Branch
    }
    return , [string[]] @($liveRoots | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
}

# Private: the zero/one/many decision shared by the parallel and record resolvers.
function Resolve-WorktreeRunMatch {
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [Parameter(Mandatory = $true)] [AllowEmptyCollection()] [string[]] $Matched,
        [Parameter(Mandatory = $true)] [string] $SessionRoot,
        [Parameter(Mandatory = $true)] [string] $Description
    )

    if ($Matched.Count -eq 0) {
        return (New-WorktreeResolutionTargetResult -Status 'NoTarget' -SessionRoot $SessionRoot -Detail (
                'no live worktree''s run checkpoint records {0}' -f $Description))
    }
    if ($Matched.Count -eq 1) {
        return (ConvertTo-WorktreeItemResolvedResult -WorktreeRoot $Matched[0] -SessionRoot $SessionRoot -Detail (
                '{0} resolves to the live worktree whose run checkpoint records it' -f $Description))
    }
    $detail = ('{0} is recorded by the run checkpoints of {1} live worktrees; move the stale copy to {2}' -f
        $Description, $Matched.Count, $script:HandoffRemedy)
    return (New-WorktreeResolutionTargetResult -Status 'Ambiguous' -SessionRoot $SessionRoot -Candidate $Matched -Detail $detail)
}

function Resolve-WorktreeEpicTarget {
    <#
    .SYNOPSIS
        Resolve the live worktree whose epic checkpoint governs an integration branch.
    .DESCRIPTION
        Keeps each live root whose epic checkpoint records route_id epic and the branch.
        Zero matches resolve NoTarget; one resolves; several are narrowed to the root that
        has the branch checked out, and anything other than one survivor is Ambiguous. A
        supplied slug that a match's epic_feature_folder disagrees with is Ambiguous.
    .PARAMETER IntegrationBranch
        The integration branch the call names.
    .PARAMETER EpicSlug
        Optional. The epic_feature_folder the call names, used as a cross-check only.
    .PARAMETER SessionRoot
        The calling process's path, used only to find the repository and label a result.
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [AllowNull()] [AllowEmptyString()] [string] $IntegrationBranch,
        [AllowNull()] [AllowEmptyString()] [string] $EpicSlug,
        [Parameter(Mandatory = $true)] [string] $SessionRoot
    )

    if ([string]::IsNullOrWhiteSpace($IntegrationBranch)) {
        return (New-WorktreeResolutionTargetResult -Status 'NoTarget' -SessionRoot $SessionRoot -Detail (
                'the call carries no integration_branch: value, so the epic run it belongs to cannot be identified'))
    }

    $kept = [System.Collections.Generic.List[string]]::new()
    $slugs = [System.Collections.Generic.List[string]]::new()
    foreach ($root in (Get-WorktreeRunLiveRoot -SessionRoot $SessionRoot)) {
        $checkpoint = Get-WorktreeRunCheckpoint -Kind 'epic' -WorktreeRoot $root
        if ($null -eq $checkpoint) { continue }
        if ([string] (Get-WorktreeRunPropertyValue -InputObject $checkpoint -Name 'integration_branch') -cne $IntegrationBranch) { continue }
        $kept.Add($root)
        $recordedSlug = Get-WorktreeRunPropertyValue -InputObject $checkpoint -Name 'epic_feature_folder'
        if ($null -ne $recordedSlug) { $slugs.Add([string] $recordedSlug) }
    }

    # A slug the call names that a matching checkpoint contradicts is not guessed past.
    if (-not [string]::IsNullOrWhiteSpace($EpicSlug)) {
        $disagreeing = @($slugs | Where-Object { $_ -cne $EpicSlug })
        if ($disagreeing.Count -gt 0) {
            return (New-WorktreeResolutionTargetResult -Status 'Ambiguous' -SessionRoot $SessionRoot `
                    -Signal 'Branch' -SignalValue $IntegrationBranch -Candidate $kept.ToArray() -Detail (
                    "the call names epic_feature_folder '{0}', but an epic checkpoint recording integration_branch '{1}' records epic_feature_folder '{2}'" -f
                    $EpicSlug, $IntegrationBranch, $disagreeing[0]))
        }
    }

    if ($kept.Count -eq 0) {
        return (New-WorktreeResolutionTargetResult -Status 'NoTarget' -SessionRoot $SessionRoot -Detail (
                "no live worktree's epic checkpoint records integration_branch '{0}'" -f $IntegrationBranch))
    }
    if ($kept.Count -eq 1) {
        return (ConvertTo-WorktreeItemResolvedResult -WorktreeRoot $kept[0] -SessionRoot $SessionRoot -Branch $IntegrationBranch -Detail (
                "integration_branch '{0}' resolves to the live worktree whose epic checkpoint records it" -f $IntegrationBranch))
    }

    # Several copies: the owning worktree is the one with the integration branch checked out.
    $branchRoots = Get-WorktreeRunLiveRoot -SessionRoot $SessionRoot -Branch $IntegrationBranch
    $checkedOut = @(foreach ($root in $branchRoots) { ConvertTo-WorktreeResolutionNormalizedPath -Path $root })
    $survivors = @($kept | Where-Object { $checkedOut -contains (ConvertTo-WorktreeResolutionNormalizedPath -Path $_) })
    if ($survivors.Count -eq 1) {
        return (ConvertTo-WorktreeItemResolvedResult -WorktreeRoot $survivors[0] -SessionRoot $SessionRoot -Branch $IntegrationBranch -Detail (
                "integration_branch '{0}' resolves to the live worktree that has it checked out" -f $IntegrationBranch))
    }
    $detail = ("the epic checkpoints of {0} live worktrees record integration_branch '{1}', and {2} of them have it checked out; move the stale copy to {3}" -f
        $kept.Count, $IntegrationBranch, $survivors.Count, $script:HandoffRemedy)
    return (New-WorktreeResolutionTargetResult -Status 'Ambiguous' -SessionRoot $SessionRoot `
            -Signal 'Branch' -SignalValue $IntegrationBranch -Candidate $kept.ToArray() -Detail $detail)
}

function Resolve-WorktreeParallelTarget {
    <#
    .SYNOPSIS
        Resolve the live worktree whose parallel checkpoint records a parallel slug.
    .DESCRIPTION
        Keeps each live root whose parallel checkpoint records route_id parallel and the
        slug. Zero matches resolve NoTarget, one resolves, and several are Ambiguous.
    .PARAMETER ParallelSlug
        The parallel_slug the call names.
    .PARAMETER SessionRoot
        The calling process's path, used only to find the repository and label a result.
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [AllowNull()] [AllowEmptyString()] [string] $ParallelSlug,
        [Parameter(Mandatory = $true)] [string] $SessionRoot
    )

    if ([string]::IsNullOrWhiteSpace($ParallelSlug)) {
        return (New-WorktreeResolutionTargetResult -Status 'NoTarget' -SessionRoot $SessionRoot -Detail (
                'the call carries no parallel_slug: value, so the parallel run it belongs to cannot be identified'))
    }
    $matched = [System.Collections.Generic.List[string]]::new()
    foreach ($root in (Get-WorktreeRunLiveRoot -SessionRoot $SessionRoot)) {
        $checkpoint = Get-WorktreeRunCheckpoint -Kind 'parallel' -WorktreeRoot $root
        if ($null -eq $checkpoint) { continue }
        if ([string] (Get-WorktreeRunPropertyValue -InputObject $checkpoint -Name 'parallel_slug') -ceq $ParallelSlug) { $matched.Add($root) }
    }
    return (Resolve-WorktreeRunMatch -Matched $matched.ToArray() -SessionRoot $SessionRoot -Description ("parallel_slug '{0}'" -f $ParallelSlug))
}

# Private: compare two worktree paths, insensitive to separator style and trailing
# slashes, and case-insensitive only for drive-letter paths.
function Test-WorktreeRunPathEqual {
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [Parameter(Mandatory = $true)] [AllowNull()] [AllowEmptyString()] [string] $Left,
        [Parameter(Mandatory = $true)] [AllowNull()] [AllowEmptyString()] [string] $Right
    )

    if ([string]::IsNullOrWhiteSpace($Left) -or [string]::IsNullOrWhiteSpace($Right)) { return $false }
    $a = ($Left.Trim() -replace '\\', '/').TrimEnd('/')
    $b = ($Right.Trim() -replace '\\', '/').TrimEnd('/')
    $isDrivePath = ($a -match '^[A-Za-z]:') -or ($b -match '^[A-Za-z]:')
    $comparison = if ($isDrivePath) { [System.StringComparison]::OrdinalIgnoreCase } else { [System.StringComparison]::Ordinal }
    return [string]::Equals($a, $b, $comparison)
}

# Private: compare two pull request numbers as integers.
function Test-WorktreeRunPrNumberEqual {
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [Parameter(Mandatory = $true)] [AllowNull()] [object] $Recorded,
        [Parameter(Mandatory = $true)] [long] $Expected
    )

    if ($null -eq $Recorded) { return $false }
    $number = [long] 0
    if (-not [long]::TryParse(([string] $Recorded).Trim(), [ref] $number)) { return $false }
    return ($number -eq $Expected)
}

# Private: whether a parsed run checkpoint records a pull request number or worktree path.
function Test-WorktreeRunCheckpointRecord {
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [Parameter(Mandatory = $true)] [pscustomobject] $Checkpoint,
        [Parameter(Mandatory = $true)] [ValidateSet('epic', 'parallel')] [string] $Kind,
        [Parameter(Mandatory = $true)] [ValidateSet('pr_number', 'worktree_path')] [string] $RecordField,
        [Parameter(Mandatory = $true)] [string] $Value
    )

    $values = [System.Collections.Generic.List[object]]::new()
    $arrayName = if ($Kind -eq 'epic') { 'features' } else { 'items' }
    foreach ($record in @(Get-WorktreeRunPropertyValue -InputObject $Checkpoint -Name $arrayName)) {
        $values.Add((Get-WorktreeRunPropertyValue -InputObject $record -Name $RecordField))
    }
    if ($Kind -eq 'epic' -and $RecordField -eq 'pr_number') {
        $mergePr = Get-WorktreeRunPropertyValue -InputObject $Checkpoint -Name 'epic_merge_pr'
        $values.Add((Get-WorktreeRunPropertyValue -InputObject $mergePr -Name 'pr_number'))
    }

    foreach ($recorded in $values) {
        if ($RecordField -eq 'pr_number') {
            if (Test-WorktreeRunPrNumberEqual -Recorded $recorded -Expected ([long] $Value)) { return $true }
        }
        elseif (Test-WorktreeRunPathEqual -Left ([string] $recorded) -Right $Value) { return $true }
    }
    return $false
}

function Resolve-WorktreeRunTargetByRecord {
    <#
    .SYNOPSIS
        Resolve the live worktree whose run checkpoint records a pull request number or worktree path.
    .DESCRIPTION
        Epic checkpoints are searched in epic_merge_pr.pr_number and features[].pr_number, or
        features[].worktree_path; parallel checkpoints in items[].pr_number or
        items[].worktree_path. Zero matches resolve NoTarget, one resolves, and several are
        Ambiguous. A blank value, or a pr_number that is not all digits or does not fit a 64-bit integer, is NoTarget.
    .PARAMETER Kind
        epic or parallel.
    .PARAMETER RecordField
        pr_number or worktree_path.
    .PARAMETER Value
        The pull request number or worktree path the command names.
    .PARAMETER SessionRoot
        The calling process's path, used only to find the repository and label a result.
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [Parameter(Mandatory = $true)] [ValidateSet('epic', 'parallel')] [string] $Kind,
        [Parameter(Mandatory = $true)] [ValidateSet('pr_number', 'worktree_path')] [string] $RecordField,
        [AllowNull()] [AllowEmptyString()] [string] $Value,
        [Parameter(Mandatory = $true)] [string] $SessionRoot
    )

    $description = "{0} '{1}' in the {2} checkpoint" -f $RecordField, $Value, $Kind
    $isBlank = [string]::IsNullOrWhiteSpace($Value)
    $parsedNumber = [long] 0
    $isNumber = -not $isBlank -and $Value.Trim() -match '^\d+$' -and [long]::TryParse($Value.Trim(), [ref] $parsedNumber)
    if ($isBlank -or ($RecordField -eq 'pr_number' -and -not $isNumber)) {
        return (New-WorktreeResolutionTargetResult -Status 'NoTarget' -SessionRoot $SessionRoot -Detail (
                'the command names no usable {0}, so the {1} run it belongs to cannot be identified' -f $RecordField, $Kind))
    }
    $needle = $Value.Trim()
    $matched = [System.Collections.Generic.List[string]]::new()
    foreach ($root in (Get-WorktreeRunLiveRoot -SessionRoot $SessionRoot)) {
        $checkpoint = Get-WorktreeRunCheckpoint -Kind $Kind -WorktreeRoot $root
        if ($null -eq $checkpoint) { continue }
        if (Test-WorktreeRunCheckpointRecord -Checkpoint $checkpoint -Kind $Kind -RecordField $RecordField -Value $needle) { $matched.Add($root) }
    }
    return (Resolve-WorktreeRunMatch -Matched $matched.ToArray() -SessionRoot $SessionRoot -Description $description)
}

function Resolve-WorktreeOperandTarget {
    <#
    .SYNOPSIS
        Place a file_path or git -C operand in the worktree that contains it.
    .DESCRIPTION
        Ascends from the operand to its worktree root. A blank operand, and an operand
        inside no worktree, resolve to the session worktree; a relative operand is joined
        to the session root first.
    .PARAMETER Path
        The operand path, absolute or relative, or blank.
    .PARAMETER SessionRoot
        The calling process's path.
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [AllowNull()] [AllowEmptyString()] [string] $Path,
        [Parameter(Mandatory = $true)] [string] $SessionRoot
    )

    $sessionWorktree = Find-WorktreeResolutionRoot -Path $SessionRoot
    if ($null -eq $sessionWorktree) { $sessionWorktree = ConvertTo-WorktreeResolutionNormalizedPath -Path $SessionRoot }
    if ([string]::IsNullOrWhiteSpace($Path)) {
        return (ConvertTo-WorktreeItemResolvedResult -WorktreeRoot $sessionWorktree -SessionRoot $SessionRoot -Detail (
                'the call names no operand, so it acts on the session worktree'))
    }

    $operand = ConvertTo-WorktreeResolutionNormalizedPath -Path $Path
    if ($operand -notmatch $script:AbsolutePathPattern) {
        $operand = Join-WorktreeResolutionPath -WorktreeRoot $SessionRoot -RepoRelativePath $operand
    }
    $found = Find-WorktreeResolutionRoot -Path $operand
    if ($null -eq $found) {
        return (ConvertTo-WorktreeItemResolvedResult -WorktreeRoot $sessionWorktree -SessionRoot $SessionRoot -Detail (
                "the operand '{0}' lies in no worktree, so the call is evaluated against the session worktree" -f $operand))
    }
    return (ConvertTo-WorktreeItemResolvedResult -WorktreeRoot $found -SessionRoot $SessionRoot -Detail (
            "the operand '{0}' lies in the worktree '{1}'" -f $operand, $found))
}

Export-ModuleMember -Function `
    Find-WorktreeRunIdentitySignal, `
    Get-WorktreeRunCheckpointText, `
    Get-WorktreeRunCheckpointPath, `
    Resolve-WorktreeEpicTarget, `
    Resolve-WorktreeParallelTarget, `
    Resolve-WorktreeRunTargetByRecord, `
    Resolve-WorktreeOperandTarget
