# Human-readable guidance for the repository hygiene guard. The finding lines stay terse and
# machine-stable; these pure functions turn them into an explanation per finding, a remediation
# block per rule, and optional GitHub Actions error annotations. Like the finding lines, the
# guidance carries only rule names, paths and line numbers, never the matched text.

function Get-HygieneRuleGuidance {
    <#
    .SYNOPSIS
        Returns the explanation and remediation text for one hygiene rule.
    .DESCRIPTION
        Maps a rule name (profile-path, raw-document, backup-file, unreadable) to a record with a
        Title, a one-sentence Problem and a list of Remediation sentences. An unknown rule name
        throws, so a new rule cannot ship without guidance.
    .PARAMETER Rule
        The rule name as it appears in a finding line.
    .OUTPUTS
        System.Management.Automation.PSCustomObject
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [Parameter(Mandatory = $true)]
        [string]$Rule
    )

    switch -CaseSensitive ($Rule) {
        'profile-path' {
            return [pscustomobject]@{
                Title       = 'Absolute user-profile path committed'
                Problem     = 'The line contains an absolute Windows user-profile path (a drive letter followed by \Users\<name>\...). It identifies one person and one machine and does not resolve on any other checkout or on the CI runner. The matched text is deliberately not printed so the CI log does not repeat it; open the file at the reported line to see it.'
                Remediation = @(
                    'Replace the path with a repository-relative path, an environment variable or MSBuild property (for example %USERPROFILE% or $(MSBuildProjectDirectory)), or a placeholder such as <user>.'
                    'In a .csproj, Visual Studio writes machine-specific values such as the ClickOnce <PublishUrl> into the project file. Keep the committed value relative (for example publish\) and put the personal value in the untracked <Project>.csproj.user file, which MSBuild imports after the project file.'
                    'Run ./scripts/hygiene/Test-RepositoryHygiene.ps1 locally to confirm the finding is gone before pushing.'
                )
            }
        }
        'raw-document' {
            return [pscustomobject]@{
                Title       = 'Raw test or coverage document committed'
                Problem     = 'The file is a raw test-platform document (trx) or a raw coverage-collector document (Cobertura, OpenCover, dotnet-coverage, or a JaCoCo report with class, method or line detail). These carry absolute host paths and machine identifiers and are too large to review, so CLAUDE.md prohibits committing them in any form.'
                Remediation = @(
                    'Remove it from the index with git rm --cached <path> and keep it out of future commits.'
                    'Commit a package-level JaCoCo projection, the one-line coverage summary, or a test-result summary derived from the trx document instead.'
                )
            }
        }
        'backup-file' {
            return [pscustomobject]@{
                Title       = 'Backup file committed'
                Problem     = 'The tracked file has a final .bak extension. Backup copies duplicate source, drift from the real file, and are ignored by .gitignore, so a tracked one was added by force or before the ignore rule existed.'
                Remediation = @('Remove it from the index with git rm --cached <path>; keep the content in the real file or in git history instead.')
            }
        }
        'unreadable' {
            return [pscustomobject]@{
                Title       = 'Tracked file could not be read'
                Problem     = 'The guard could not read the tracked file, so it could not prove the file is clean. An unreadable file is a finding rather than a silent pass.'
                Remediation = @('Check that the file exists in the checkout, is not a broken symbolic link or an unfetched LFS pointer, and is readable by the runner.')
            }
        }
    }

    throw ('No guidance is defined for hygiene rule ' + $Rule + '.')
}

function ConvertTo-HygieneFinding {
    <#
    .SYNOPSIS
        Parses one finding line into a rule, path and optional line number.
    .DESCRIPTION
        Accepts "HYGIENE <rule> <path>" and, for profile-path, "HYGIENE <rule> <path>:<line>".
        Returns null for the "HYGIENE Findings=<n>" total and for any line that is not a finding.
    .PARAMETER Line
        One line produced by Invoke-RepositoryHygieneMain.
    .OUTPUTS
        System.Management.Automation.PSCustomObject
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string]$Line
    )

    $match = [regex]::Match($Line, '\AHYGIENE (profile-path|raw-document|backup-file|unreadable) (.+?)(?::(\d+))?\z')
    if (-not $match.Success) {
        return $null
    }

    $lineNumber = if ($match.Groups[3].Success) { [int]$match.Groups[3].Value } else { 0 }
    return [pscustomobject]@{
        Rule       = $match.Groups[1].Value
        Path       = $match.Groups[2].Value
        LineNumber = $lineNumber
    }
}

function ConvertTo-GitHubAnnotationValue {
    <#
    .SYNOPSIS
        Escapes text for a GitHub Actions workflow command.
    .DESCRIPTION
        Percent, carriage return and line feed are always escaped; colon and comma are escaped as
        well when the value is a property (file, line, title) rather than the message.
    .PARAMETER Value
        The text to escape.
    .PARAMETER IsProperty
        Set when the value is a workflow-command property.
    .OUTPUTS
        System.String
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter(Mandatory = $true)]
        [AllowEmptyString()]
        [string]$Value,

        [Parameter()]
        [switch]$IsProperty
    )

    $escaped = $Value.Replace('%', '%25').Replace("`r", '%0D').Replace("`n", '%0A')
    if ($IsProperty) {
        $escaped = $escaped.Replace(':', '%3A').Replace(',', '%2C')
    }

    return $escaped
}

function Get-HygieneFindingGuidance {
    <#
    .SYNOPSIS
        Builds the human-readable diagnostic block for a set of finding lines.
    .DESCRIPTION
        Returns no lines when the input holds no findings. Otherwise returns a header, one
        explanation line per finding naming its file, line and rule title, and one remediation
        section per distinct rule in first-seen order. With -EmitAnnotations, each finding also
        yields a GitHub Actions "::error file=...,line=...,title=...::" annotation so the finding is
        shown against the file in the pull-request diff.
    .PARAMETER Lines
        The Lines produced by Invoke-RepositoryHygieneMain.
    .PARAMETER EmitAnnotations
        Set when running under GitHub Actions.
    .OUTPUTS
        System.String
    #>
    [CmdletBinding()]
    [OutputType([string])]
    param(
        [Parameter()]
        [AllowEmptyCollection()]
        [string[]]$Lines = @(),

        [Parameter()]
        [switch]$EmitAnnotations
    )

    $findings = @(foreach ($line in $Lines) { ConvertTo-HygieneFinding -Line $line } )
    $findings = @($findings | Where-Object { $null -ne $_ })
    if ($findings.Count -eq 0) {
        return
    }

    $output = [System.Collections.Generic.List[string]]::new()
    $rules = [System.Collections.Generic.List[string]]::new()
    $output.Add('')
    $output.Add('Repository hygiene guard failed with ' + $findings.Count + ' finding(s):')
    foreach ($finding in $findings) {
        $guidance = Get-HygieneRuleGuidance -Rule $finding.Rule
        $location = if ($finding.LineNumber -gt 0) { $finding.Path + ' line ' + $finding.LineNumber } else { $finding.Path }
        $output.Add('  - ' + $location + ': ' + $guidance.Title + ' [' + $finding.Rule + ']')
        if (-not $rules.Contains($finding.Rule)) {
            $rules.Add($finding.Rule)
        }

        if ($EmitAnnotations) {
            $properties = 'file=' + (ConvertTo-GitHubAnnotationValue -Value $finding.Path -IsProperty)
            if ($finding.LineNumber -gt 0) {
                $properties += ',line=' + $finding.LineNumber
            }

            $properties += ',title=' + (ConvertTo-GitHubAnnotationValue -Value ('Hygiene: ' + $guidance.Title) -IsProperty)
            $message = $guidance.Problem + ' ' + ($guidance.Remediation -join ' ')
            $output.Add('::error ' + $properties + '::' + (ConvertTo-GitHubAnnotationValue -Value $message))
        }
    }

    foreach ($rule in $rules) {
        $guidance = Get-HygieneRuleGuidance -Rule $rule
        $output.Add('')
        $output.Add('[' + $rule + '] ' + $guidance.Title)
        $output.Add('  Why it fails: ' + $guidance.Problem)
        foreach ($step in $guidance.Remediation) {
            $output.Add('  How to fix: ' + $step)
        }
    }

    return $output.ToArray()
}
