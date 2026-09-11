<#
.SYNOPSIS
    Keyed-union resolution of conflicted project-file text (issue #643).

.DESCRIPTION
    Splits a conflicted file into hunks, parses each side with the grammar
    module, and rebuilds the file as the keyed union of the two sides: ours
    entries in ours order, then theirs entries whose key ours does not carry.

    Nothing here is an XML operation. A line the merge keeps is the byte-for-byte
    line one side wrote, so indentation, attribute order, and line terminators
    survive; the one exception is the app.config oldVersion upper bound, which is
    rewritten in place so the retained redirect still covers the chosen version.

    The merge refuses far more often than it resolves. A side with any line
    outside the grammar, a version pair that does not rank, and a key carried at
    one version by both sides with different attributes all escalate, because a
    mechanical union of a conflict nobody understands is worse than a human one.
    CONVENTION: this module fails fast at module scope and imports its siblings with -ErrorAction Stop.
#>

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

Import-Module (Join-Path -Path $PSScriptRoot -ChildPath 'ProjectFileMergeGrammar.psm1') -Force -ErrorAction Stop

# Conflict markers. git writes seven characters; the base section appears only in
# the diff3 and zdiff3 styles, and the separator line carries nothing else.
$script:HunkStartPattern = '^<{7}'
$script:HunkBasePattern = '^\|{7}'
$script:HunkSplitPattern = '^={7}\s*$'
$script:HunkEndPattern = '^>{7}'

# Widest window Get-UnitKeySet offers the grammar when scanning a whole file. A
# paired item and a dependentAssembly block are both far shorter than this.
$script:MaximumUnitSpan = 16

# The app.config redirect range, whose upper bound follows the chosen newVersion.
$script:OldVersionRangePattern = '(oldVersion=")([^"-]*)(-)([^"]*)(")'

function Get-ConflictHunk {
    <#
    .SYNOPSIS
        Split conflicted text into its conflict hunks.
    .DESCRIPTION
        Accepts the merge style and the diff3/zdiff3 style, whose extra |||||||
        section carries the base text. Indices are zero-based positions in Line.
    .PARAMETER Line
        The whole conflicted file, each line retaining its own terminator.
    .OUTPUTS
        System.Object[] of hunks carrying Start, End, Ours, Theirs, and Base, or
        $null when a hunk opens and no closing marker follows it.
    #>
    [CmdletBinding()]
    [OutputType([System.Object[]])]
    param([Parameter(Mandatory = $true)][AllowEmptyCollection()][AllowEmptyString()][string[]] $Line)

    $hunk = [System.Collections.Generic.List[object]]::new()
    $index = 0
    while ($index -lt $Line.Count) {
        # Everything outside a hunk is context and is not collected here.
        if (-not [regex]::IsMatch($Line[$index], $script:HunkStartPattern)) { $index++; continue }
        $start = $index
        $index++
        $ours = [System.Collections.Generic.List[string]]::new()
        $base = [System.Collections.Generic.List[string]]::new()
        $theirs = [System.Collections.Generic.List[string]]::new()
        $section = 'ours'
        $end = -1
        while ($index -lt $Line.Count) {
            $text = $Line[$index]
            $index++
            # The two mid-hunk markers switch sections and carry no content.
            if ([regex]::IsMatch($text, $script:HunkBasePattern)) { $section = 'base'; continue }
            if ([regex]::IsMatch($text, $script:HunkSplitPattern)) { $section = 'theirs'; continue }
            if ([regex]::IsMatch($text, $script:HunkEndPattern)) { $end = $index - 1; break }
            if ($section -eq 'ours') { $ours.Add($text) } elseif ($section -eq 'base') { $base.Add($text) } else { $theirs.Add($text) }
        }
        # A hunk that never closes means the file is not a git conflict result.
        if ($end -lt 0) { return $null }
        $hunk.Add([pscustomobject]@{
                Start  = $start
                End    = $end
                Ours   = $ours.ToArray()
                Theirs = $theirs.ToArray()
                Base   = $base.ToArray()
            })
    }
    return , $hunk.ToArray()
}

function Get-UnitKeySet {
    <#
    .SYNOPSIS
        Collect the unit keys a complete file text carries.
    .DESCRIPTION
        Offers the grammar a widening window at each position and skips a line no
        window parses, so surrounding project structure contributes no key.
    .PARAMETER Line
        A complete file text, each line retaining its own terminator.
    .PARAMETER Kind
        'msbuild', 'packages', or 'appconfig'.
    .OUTPUTS
        System.Object[] of keys, deduplicated and ordinally sorted.
    #>
    [CmdletBinding()]
    [OutputType([System.Object[]])]
    param(
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][AllowEmptyString()][string[]] $Line,
        [Parameter(Mandatory = $true)][AllowEmptyString()][string] $Kind
    )

    $key = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
    $index = 0
    while ($index -lt $Line.Count) {
        $limit = [System.Math]::Min($Line.Count - $index, $script:MaximumUnitSpan)
        $span = 1
        $consumed = 0
        while ($span -le $limit) {
            $unit = Get-MergeableUnit -Line @($Line[$index..($index + $span - 1)]) -Kind $Kind
            # A parse of zero units is a run of blank lines, not a unit boundary.
            if ($null -ne $unit -and $unit.Count -ge 1) {
                foreach ($entry in $unit) { [void] $key.Add($entry.Key) }
                $consumed = $span
                break
            }
            $span++
        }
        # No window parsed here, so this line is structure and is stepped over.
        $index += [System.Math]::Max($consumed, 1)
    }

    $sorted = [System.Collections.Generic.List[string]]::new($key)
    $sorted.Sort([StringComparer]::Ordinal)
    return , $sorted.ToArray()
}

function ConvertTo-MergeOutcome {
    <#
    .SYNOPSIS
        Build the result record Merge-ConflictedText returns.
    .DESCRIPTION
        A non-null Reason is what makes the record an escalation, so the caller
        never has to keep the flag and the reason in step by hand.
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [AllowEmptyCollection()][AllowEmptyString()][string[]] $Line = @(),
        [AllowEmptyCollection()][AllowEmptyString()][string[]] $FromOurs = @(),
        [AllowEmptyCollection()][AllowEmptyString()][string[]] $FromTheirs = @(),
        [AllowEmptyCollection()][object[]] $Resolution = @(),
        [AllowNull()][AllowEmptyString()][string] $Reason = $null
    )

    return [pscustomobject]@{
        Lines                  = $Line
        EntriesAddedFromOurs   = $FromOurs
        EntriesAddedFromTheirs = $FromTheirs
        VersionResolutions     = $Resolution
        Escalate               = -not [string]::IsNullOrEmpty($Reason)
        EscalateReason         = if ([string]::IsNullOrEmpty($Reason)) { $null } else { $Reason }
    }
}

function Test-UnitEquivalent {
    <#
    .SYNOPSIS
        Report whether two units with one key are byte-equivalent.
    .DESCRIPTION
        Compares the keying element's attributes and then the unit's whole line
        run, so a differing metadata child is a difference like any other.
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [Parameter(Mandatory = $true)][object] $Left,
        [Parameter(Mandatory = $true)][object] $Right
    )

    if ($Left.Attributes.Count -ne $Right.Attributes.Count) { return $false }
    foreach ($name in $Left.Attributes.Keys) {
        # A missing or differing attribute makes the two sides genuinely opposed.
        if (-not $Right.Attributes.ContainsKey($name)) { return $false }
        if (-not [string]::Equals($Left.Attributes[$name], $Right.Attributes[$name], [System.StringComparison]::Ordinal)) { return $false }
    }
    return [string]::Equals(($Left.Lines -join ''), ($Right.Lines -join ''), [System.StringComparison]::Ordinal)
}

function ConvertTo-RetargetedRedirectLine {
    <#
    .SYNOPSIS
        Rewrite an app.config redirect's oldVersion upper bound.
    .DESCRIPTION
        The retained block was written for its own newVersion, so keeping the
        higher newVersion without widening the range would leave the redirect
        covering less than it claims.
    #>
    [CmdletBinding()]
    [OutputType([System.Object[]])]
    param(
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][AllowEmptyString()][string[]] $Line,
        [Parameter(Mandatory = $true)][AllowEmptyString()][string] $NewVersion
    )

    $rewritten = [System.Collections.Generic.List[string]]::new()
    foreach ($text in $Line) {
        # Only the range form carries an upper bound to move. The braced group
        # references keep the version digits from being read as a group number.
        $rewritten.Add([regex]::Replace($text, $script:OldVersionRangePattern, ('${1}${2}${3}' + $NewVersion + '${5}')))
    }
    return , $rewritten.ToArray()
}

function Merge-ConflictedText {
    <#
    .SYNOPSIS
        Resolve every conflict hunk of a project file as a keyed union.
    .DESCRIPTION
        Ours entries keep their order and lead; theirs entries whose key ours
        does not carry follow in theirs order. A key both sides carry at
        different versions keeps the ranked side and records the choice; the same
        key at one version with differing attributes or children escalates, as
        does any side outside the grammar.
    .PARAMETER Line
        The whole conflicted file, each line retaining its own terminator.
    .PARAMETER Kind
        'msbuild', 'packages', or 'appconfig'.
    .OUTPUTS
        A record carrying Lines, EntriesAddedFromOurs, EntriesAddedFromTheirs,
        VersionResolutions, Escalate, and EscalateReason.
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][AllowEmptyString()][string[]] $Line,
        [Parameter(Mandatory = $true)][AllowEmptyString()][string] $Kind
    )

    $hunk = Get-ConflictHunk -Line $Line
    if ($null -eq $hunk) { return (ConvertTo-MergeOutcome -Reason 'a conflict hunk is not terminated') }

    $merged = [System.Collections.Generic.List[string]]::new()
    $fromOurs = [System.Collections.Generic.List[string]]::new()
    $fromTheirs = [System.Collections.Generic.List[string]]::new()
    $resolution = [System.Collections.Generic.List[object]]::new()
    $cursor = 0
    foreach ($section in $hunk) {
        # Context ahead of the hunk is copied through untouched.
        while ($cursor -lt $section.Start) { $merged.Add($Line[$cursor]); $cursor++ }
        $oursUnit = Get-MergeableUnit -Line $section.Ours -Kind $Kind
        $theirsUnit = Get-MergeableUnit -Line $section.Theirs -Kind $Kind
        if ($null -eq $oursUnit -or $null -eq $theirsUnit) {
            return (ConvertTo-MergeOutcome -Reason ('the hunk opening at line {0} is outside the merge grammar' -f ($section.Start + 1)))
        }

        $theirsByKey = @{}
        foreach ($unit in $theirsUnit) { $theirsByKey[$unit.Key] = $unit }
        $oursKey = [System.Collections.Generic.HashSet[string]]::new([StringComparer]::Ordinal)
        foreach ($unit in $oursUnit) { [void] $oursKey.Add($unit.Key) }

        foreach ($unit in $oursUnit) {
            $kept = $unit
            if ($theirsByKey.ContainsKey($unit.Key)) {
                $other = $theirsByKey[$unit.Key]
                $oursVersion = [string] $unit.Version
                $theirsVersion = [string] $other.Version
                if (-not [string]::Equals($oursVersion, $theirsVersion, [System.StringComparison]::Ordinal)) {
                    $choice = Compare-UnitVersion -Ours $oursVersion -Theirs $theirsVersion
                    # An unrankable pair is exactly the case a human must settle.
                    if ($choice -eq 'escalate') {
                        return (ConvertTo-MergeOutcome -Reason ('{0} carries versions that do not rank: {1} and {2}' -f $unit.Key, $oursVersion, $theirsVersion))
                    }
                    if ($choice -eq 'theirs') { $kept = $other }
                    $chosen = if ($choice -eq 'theirs') { $theirsVersion } else { $oursVersion }
                    $resolution.Add([pscustomobject]@{ key = $unit.Key; ours = $oursVersion; theirs = $theirsVersion; chosen = $chosen })
                    # The retained redirect must still cover the version it pins.
                    if ($Kind -eq 'appconfig') {
                        $kept = [pscustomobject]@{
                            Key        = $kept.Key
                            Version    = $chosen
                            Lines      = (ConvertTo-RetargetedRedirectLine -Line $kept.Lines -NewVersion $chosen)
                            Attributes = $kept.Attributes
                        }
                    }
                } elseif (-not (Test-UnitEquivalent -Left $unit -Right $other)) {
                    return (ConvertTo-MergeOutcome -Reason ('{0} is carried at one version with differing content on each side' -f $unit.Key))
                }
            }
            foreach ($text in $kept.Lines) { $merged.Add($text) }
            $fromOurs.Add($unit.Key)
        }

        foreach ($unit in $theirsUnit) {
            # A key ours already placed was settled above and is not repeated.
            if ($oursKey.Contains($unit.Key)) { continue }
            foreach ($text in $unit.Lines) { $merged.Add($text) }
            $fromTheirs.Add($unit.Key)
        }
        $cursor = $section.End + 1
    }

    # Trailing context after the last hunk is copied through untouched.
    while ($cursor -lt $Line.Count) { $merged.Add($Line[$cursor]); $cursor++ }
    return (ConvertTo-MergeOutcome -Line $merged.ToArray() -FromOurs $fromOurs.ToArray() -FromTheirs $fromTheirs.ToArray() -Resolution $resolution.ToArray())
}

function Test-NeverDropPostCondition {
    <#
    .SYNOPSIS
        Report whether a merged text dropped nothing either side carried.
    .DESCRIPTION
        The merged key set must equal the union of the two stage key sets, and
        every key the base and both sides carried must survive. The second check
        is redundant with the first by construction and is asserted anyway,
        because it is the property the merge exists to guarantee.
    .PARAMETER MergedLine
        The merged file text.
    .PARAMETER OursLine
        The stage-2 text.
    .PARAMETER TheirsLine
        The stage-3 text.
    .PARAMETER BaseLine
        The stage-1 text.
    .PARAMETER Kind
        'msbuild', 'packages', or 'appconfig'.
    .OUTPUTS
        System.Boolean.
    #>
    [CmdletBinding()]
    [OutputType([bool])]
    param(
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][AllowEmptyString()][string[]] $MergedLine,
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][AllowEmptyString()][string[]] $OursLine,
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][AllowEmptyString()][string[]] $TheirsLine,
        [Parameter(Mandatory = $true)][AllowEmptyCollection()][AllowEmptyString()][string[]] $BaseLine,
        [Parameter(Mandatory = $true)][AllowEmptyString()][string] $Kind
    )

    $mergedKey = [System.Collections.Generic.HashSet[string]]::new([string[]] (Get-UnitKeySet -Line $MergedLine -Kind $Kind), [StringComparer]::Ordinal)
    $oursKey = [System.Collections.Generic.HashSet[string]]::new([string[]] (Get-UnitKeySet -Line $OursLine -Kind $Kind), [StringComparer]::Ordinal)
    $theirsKey = [System.Collections.Generic.HashSet[string]]::new([string[]] (Get-UnitKeySet -Line $TheirsLine -Kind $Kind), [StringComparer]::Ordinal)
    $union = [System.Collections.Generic.HashSet[string]]::new($oursKey, [StringComparer]::Ordinal)
    $union.UnionWith($theirsKey)
    # A merged set larger than the union means an invented key; smaller, a drop.
    if (-not $mergedKey.SetEquals($union)) { return $false }

    foreach ($key in (Get-UnitKeySet -Line $BaseLine -Kind $Kind)) {
        # Only a key both sides still carry is one the merge had to keep.
        if ($oursKey.Contains($key) -and $theirsKey.Contains($key) -and -not $mergedKey.Contains($key)) { return $false }
    }
    return $true
}

Export-ModuleMember -Function Get-ConflictHunk, Get-UnitKeySet, Merge-ConflictedText, Test-NeverDropPostCondition
