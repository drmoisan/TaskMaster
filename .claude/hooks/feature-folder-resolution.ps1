<#
.SYNOPSIS
    Pure feature-folder target resolution shared by the Claude and Codex enforcement hooks
    (issue #565).

.DESCRIPTION
    One resolution rule, implemented once and parameterized per gate. A feature folder is
    identified structurally, as the single path segment immediately below
    docs/features/active/, and never by string length or prompt position.

      - Find-FeatureFolderCandidate extracts the distinct cited folder basenames (R1).
      - ConvertTo-FeatureFolderBasename normalizes a record value or token (R2).
      - Find-FeatureFolderRecord is the union-index record lookup (issue_num or folder).
      - Select-FeatureFolderTarget applies dependency pruning, the declared-issue-number
        tie-break, and the fallback issue number, and reports Resolved, NoTarget, or
        Ambiguous (R3-R6).
      - Resolve-FeatureFolderWorkMode parses the persisted work-mode marker of issue.md.
      - Get-FeatureFolderPlanPrerequisite maps a work mode to its plan prerequisites.

    The file is byte-identical in .claude/hooks/ and .codex/hooks/ and is dot-sourced by its
    callers, each of which guards the dot-source and fails closed when it cannot be loaded.

.NOTES
    Compatible with PowerShell 7+. PURITY: every function is pure string and object logic.
    The file opens no file, probes no path, reads no environment value, launches no process,
    issues no web request, and loads no module. It declares functions only.
#>

function Find-FeatureFolderCandidate {
    <#
    .SYNOPSIS
        Returns the distinct feature-folder basenames cited in a text, in first-occurrence
        order (R1).
    .DESCRIPTION
        Scans for docs/features/active/ path tokens with any prefix and either separator.
        Each token is normalized to forward slashes and split into segments; empty and '.'
        segments are dropped. A token with fewer than four segments yields no candidate.
        The fourth segment is the folder, with trailing '.', ',', ';', ':', ')', ']', '>', '*', and '_' trimmed.
        Duplicates are removed ordinally, keeping the first occurrence.
    .PARAMETER Text
        The prompt or other text to scan. Null or empty text yields no output.
    .OUTPUTS
        System.String[] (no output when no qualifying token is present)
    #>
    [CmdletBinding()]
    [OutputType([string[]])]
    param(
        [AllowNull()]
        [AllowEmptyString()]
        [string] $Text
    )

    if ([string]::IsNullOrEmpty($Text)) {
        return
    }

    $found = [System.Collections.Generic.List[string]]::new()
    foreach ($match in [regex]::Matches($Text, 'docs[\\/]+features[\\/]+active[\\/]+[^\s"''`]+')) {
        $segments = @(($match.Value -replace '\\', '/') -split '/' | Where-Object { $_ -and $_ -ne '.' })
        if ($segments.Count -lt 4) {
            continue
        }
        $folder = $segments[3].TrimEnd('.', ',', ';', ':', ')', ']', '>', '*', '_')
        if (-not $folder -or $found.Contains($folder)) {
            continue
        }
        $found.Add($folder)
    }
    return $found.ToArray()
}

function ConvertTo-FeatureFolderBasename {
    <#
    .SYNOPSIS
        Normalizes a feature-folder record value or token to its basename (R2).
    .DESCRIPTION
        Separators are normalized to forward slashes, surrounding whitespace and trailing
        slashes are trimmed, and one leading docs/features/<lifecycle>/, active/, or
        completed/ prefix is stripped. The last remaining segment is returned.
    .PARAMETER Value
        The record value or token to normalize.
    .OUTPUTS
        System.String, or $null when no folder segment remains.
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [AllowNull()]
        [AllowEmptyString()]
        [string] $Value
    )

    if ([string]::IsNullOrWhiteSpace($Value)) {
        return $null
    }
    $normalized = ($Value -replace '\\', '/').Trim().TrimEnd('/')
    $normalized = $normalized -replace '^(docs/features/[^/]+(/|$)|active/|completed/)', ''
    $segment = ($normalized -split '/')[-1]
    if (-not $segment -or $segment -eq '.') {
        return $null
    }
    return $segment
}

function Find-FeatureFolderRecord {
    <#
    .SYNOPSIS
        Finds the single record a reference identifies, through a union index of issue_num
        and normalized feature_folder.
    .DESCRIPTION
        A non-string reference, or a string of digits only, matches records whose issue_num
        string form equals it. Any other string matches records whose normalized
        feature_folder basename ordinally equals the reference's normalized basename.
    .PARAMETER Records
        The checkpoint records (features[] or items[]).
    .PARAMETER Reference
        An issue number, a folder basename, or a folder path.
    .OUTPUTS
        The matching record, or $null when zero or several records match.
    #>
    [CmdletBinding()]
    [OutputType([object])]
    param(
        [AllowNull()]
        [AllowEmptyCollection()]
        [object[]] $Records,

        [AllowNull()]
        [object] $Reference
    )

    if ($null -eq $Records -or $null -eq $Reference) {
        return $null
    }

    $byIssue = (-not ($Reference -is [string])) -or ($Reference -match '^\d+$')
    $key = if ($byIssue) { [string]$Reference } else { ConvertTo-FeatureFolderBasename -Value $Reference }
    if (-not $key) {
        return $null
    }
    $field = if ($byIssue) { 'issue_num' } else { 'feature_folder' }

    $matched = @(foreach ($record in $Records) {
            if ($null -eq $record) {
                continue
            }
            $property = $record.PSObject.Properties[$field]
            if ($null -eq $property -or $null -eq $property.Value) {
                continue
            }
            $value = if ($byIssue) { [string]$property.Value } else { ConvertTo-FeatureFolderBasename -Value ([string]$property.Value) }
            if ($value -ceq $key) {
                $record
            }
        })
    if ($matched.Count -ne 1) {
        return $null
    }
    return $matched[0]
}

function Select-FeatureFolderTarget {
    <#
    .SYNOPSIS
        Selects the target feature folder from the cited candidates and the run records
        (R2-R6).
    .DESCRIPTION
        Each candidate maps to at most one record. Unmatched candidates stay in the set.
        With -DependencyAware, a matched candidate is removed when its record lies in the
        transitive depends_on closure of another cited candidate's record and that record
        does not lie in its own closure, so a cycle removes neither. One remaining candidate
        is Resolved. Several remaining candidates are Resolved only when
        -DeclaredIssueNumber names exactly one remaining record. Zero candidates resolve
        through -FallbackIssueNumber when it names a record. Otherwise the result is
        NoTarget for zero and Ambiguous for several. Neither string length nor candidate
        position affects the status.
    .PARAMETER Records
        The checkpoint records (features[] or items[]).
    .PARAMETER Candidate
        The cited folder basenames, in first-occurrence order.
    .PARAMETER DeclaredIssueNumber
        Optional tie-break from the canonical issue-number line.
    .PARAMETER FallbackIssueNumber
        Optional issue number used only when no candidate is cited.
    .PARAMETER DependencyAware
        Removes cited candidates that are dependencies of another cited candidate.
    .OUTPUTS
        System.Management.Automation.PSCustomObject with Status, Record, Basename,
        Remaining, and Detail.
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [AllowNull()]
        [AllowEmptyCollection()]
        [object[]] $Records,

        [AllowNull()]
        [AllowEmptyCollection()]
        [string[]] $Candidate,

        [int] $DeclaredIssueNumber,

        [int] $FallbackIssueNumber,

        [switch] $DependencyAware
    )

    $recordList = @($Records | Where-Object { $null -ne $_ })
    $entries = @(foreach ($name in @($Candidate | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })) {
            [pscustomobject]@{ Basename = $name; Record = Find-FeatureFolderRecord -Records $recordList -Reference $name }
        })

    if ($DependencyAware -and $entries.Count -gt 1) {
        # Transitive depends_on closure of each matched candidate's record.
        $closures = @(foreach ($entry in $entries) {
                $closure = [System.Collections.Generic.List[object]]::new()
                $pending = [System.Collections.Generic.Stack[object]]::new()
                if ($null -ne $entry.Record) {
                    $pending.Push($entry.Record)
                }
                while ($pending.Count -gt 0) {
                    $dependsOn = $pending.Pop().PSObject.Properties['depends_on']
                    if ($null -eq $dependsOn) {
                        continue
                    }
                    foreach ($reference in @($dependsOn.Value)) {
                        $dependency = Find-FeatureFolderRecord -Records $recordList -Reference $reference
                        if ($null -ne $dependency -and -not $closure.Contains($dependency)) {
                            $closure.Add($dependency)
                            $pending.Push($dependency)
                        }
                    }
                }
                , $closure
            })

        $kept = for ($i = 0; $i -lt $entries.Count; $i++) {
            $pruned = $false
            for ($j = 0; $j -lt $entries.Count -and $null -ne $entries[$i].Record; $j++) {
                if ($i -eq $j -or $null -eq $entries[$j].Record) {
                    continue
                }
                if ($closures[$j].Contains($entries[$i].Record) -and -not $closures[$i].Contains($entries[$j].Record)) {
                    $pruned = $true
                    break
                }
            }
            if (-not $pruned) {
                $entries[$i]
            }
        }
        $entries = @($kept)
    }

    $remaining = [string[]]@($entries | ForEach-Object { $_.Basename })
    $selected = $null
    if ($entries.Count -eq 1) {
        $selected = $entries[0]
    }
    elseif ($entries.Count -gt 1 -and $PSBoundParameters.ContainsKey('DeclaredIssueNumber')) {
        $declared = [string]$DeclaredIssueNumber
        $byIssue = @($entries | Where-Object {
                $null -ne $_.Record -and $null -ne $_.Record.PSObject.Properties['issue_num'] -and
                ([string]$_.Record.issue_num) -ceq $declared
            })
        if ($byIssue.Count -eq 1) {
            $selected = $byIssue[0]
        }
    }
    elseif ($entries.Count -eq 0 -and $PSBoundParameters.ContainsKey('FallbackIssueNumber')) {
        $fallback = Find-FeatureFolderRecord -Records $recordList -Reference $FallbackIssueNumber
        if ($null -ne $fallback) {
            $folder = $fallback.PSObject.Properties['feature_folder']
            $basename = if ($null -ne $folder) { ConvertTo-FeatureFolderBasename -Value ([string]$folder.Value) } else { $null }
            $selected = [pscustomobject]@{ Basename = $basename; Record = $fallback }
        }
    }

    if ($null -ne $selected) {
        return [pscustomobject]@{
            Status    = 'Resolved'
            Record    = $selected.Record
            Basename  = $selected.Basename
            Remaining = $remaining
            Detail    = "resolved target feature folder '$($selected.Basename)'"
        }
    }
    if ($entries.Count -eq 0) {
        return [pscustomobject]@{
            Status    = 'NoTarget'
            Record    = $null
            Basename  = $null
            Remaining = $remaining
            Detail    = 'no target feature folder is cited'
        }
    }
    return [pscustomobject]@{
        Status    = 'Ambiguous'
        Record    = $null
        Basename  = $null
        Remaining = $remaining
        Detail    = 'ambiguous target feature folder; remaining candidates: ' + ($remaining -join ', ')
    }
}

function Resolve-FeatureFolderWorkMode {
    <#
    .SYNOPSIS
        Parses the persisted '- Work Mode:' marker out of issue.md content.
    .DESCRIPTION
        Returns minor-audit, full-bug, or full-feature. The legacy full marker normalizes to
        full-feature. Missing, empty, malformed, or unrecognized content returns
        -UnresolvedMode.
    .PARAMETER IssueContent
        The raw issue.md text, or $null when it could not be read.
    .PARAMETER UnresolvedMode
        The value returned when no recognized marker is present. Defaults to full-feature.
    .OUTPUTS
        System.String
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [AllowNull()]
        [AllowEmptyString()]
        [string] $IssueContent,

        [AllowNull()]
        [AllowEmptyString()]
        [string] $UnresolvedMode = 'full-feature'
    )

    if ([string]::IsNullOrWhiteSpace($IssueContent)) {
        return $UnresolvedMode
    }
    $match = [regex]::Match($IssueContent, '(?im)^-\s*Work Mode:\s*(minor-audit|full-feature|full-bug|full)\s*$')
    if (-not $match.Success) {
        return $UnresolvedMode
    }
    $rawMode = $match.Groups[1].Value
    if ($rawMode -eq 'full') {
        return 'full-feature'
    }
    return $rawMode
}

function Get-FeatureFolderPlanPrerequisite {
    <#
    .SYNOPSIS
        Maps a work mode to the files a feature folder must contain before a plan is written.
    .DESCRIPTION
        minor-audit requires issue.md; full-bug requires issue.md and spec.md; full-feature
        and every other value require issue.md, spec.md, and user-story.md.
    .PARAMETER WorkMode
        The resolved work mode.
    .OUTPUTS
        System.String[]
    #>
    [CmdletBinding()]
    [OutputType([string[]])]
    param(
        [AllowNull()]
        [AllowEmptyString()]
        [string] $WorkMode
    )

    switch ($WorkMode) {
        'minor-audit' { return [string[]]@('issue.md') }
        'full-bug' { return [string[]]@('issue.md', 'spec.md') }
        default { return [string[]]@('issue.md', 'spec.md', 'user-story.md') }
    }
}
