# Repository hygiene guard. Enumerates the tracked files through git ls-files and fails when any
# tracked file outside the governance directory is a raw test-platform or raw coverage-collector
# document (rule A), contains a line matching the generic user-profile path pattern (rule B), or
# is a backup file whose final extension is .bak (rule C).
# Finding lines carry the rule name, the path and a line number only, never the matched text, so
# the CI log cannot echo an identifier. The guard carries no exemption mechanism of any kind.

Set-StrictMode -Version Latest

. (Join-Path -Path $PSScriptRoot -ChildPath 'Test-RepositoryHygiene.Rules.ps1')
. (Join-Path -Path $PSScriptRoot -ChildPath 'Test-RepositoryHygiene.Git.ps1')

function Invoke-RepositoryHygieneMain {
    <#
    .SYNOPSIS
        Applies the hygiene rules to every tracked file and returns the findings and exit decision.
    .DESCRIPTION
        Drops records under the governance directory by a path-prefix test, classifies each
        remaining record with Get-RawEvidenceDocumentKind (content is passed only for the xml
        extension), and scans its decoded text with Find-UserProfilePathMatch. A record whose final
        extension is .bak yields "HYGIENE backup-file <path>" and is still content-scanned. Emits
        "HYGIENE raw-document <path>", "HYGIENE profile-path <path>:<line>" once per file with the
        first matching line, "HYGIENE unreadable <path>" when the content delegate throws, and a
        final "HYGIENE Findings=<n>" line. Writes nothing to any stream.
    .PARAMETER ReadContent
        Optional content delegate passed to Read-TrackedFileText.
    .OUTPUTS
        System.Management.Automation.PSCustomObject with Lines, FindingCount and ExitCode.
    #>
    [CmdletBinding()]
    [OutputType([pscustomobject])]
    param(
        [Parameter()]
        [scriptblock]$ReadContent = $null
    )

    $governancePrefix = '.claude/'
    $lines = [System.Collections.Generic.List[string]]::new()
    foreach ($record in @(Get-TrackedFileRecord)) {
        if ($record.Path.StartsWith($governancePrefix, [System.StringComparison]::Ordinal)) {
            continue
        }

        if (Test-BackupFilePath -RelativePath $record.Path) {
            $lines.Add('HYGIENE backup-file ' + $record.Path)
        }

        $text = $null
        $isUnreadable = $false
        try {
            $text = Read-TrackedFileText -Path $record.Path -ReadContent $ReadContent -IsBinaryInIndex:$record.IsBinaryInIndex
        }
        catch {
            $isUnreadable = $true
        }

        $isXml = [System.IO.Path]::GetExtension($record.Path) -ieq '.xml'
        $classifierContent = if ($isXml -and $null -ne $text) { $text } else { '' }
        $kind = Get-RawEvidenceDocumentKind -RelativePath $record.Path -Content $classifierContent
        if ($kind -ne 'none' -and $kind -ne 'jacoco-projection') {
            $lines.Add('HYGIENE raw-document ' + $record.Path)
        }

        if ($isUnreadable) {
            $lines.Add('HYGIENE unreadable ' + $record.Path)
            continue
        }

        if ([string]::IsNullOrEmpty($text)) {
            continue
        }

        $firstMatch = @(Find-UserProfilePathMatch -Text $text) | Select-Object -First 1
        if ($null -ne $firstMatch) {
            $lines.Add('HYGIENE profile-path ' + $record.Path + ':' + $firstMatch.LineNumber)
        }
    }

    $findingCount = $lines.Count
    $lines.Add('HYGIENE Findings=' + $findingCount)
    $exitCode = if ($findingCount -gt 0) { 1 } else { 0 }
    return [pscustomobject]@{
        Lines        = $lines.ToArray()
        FindingCount = $findingCount
        ExitCode     = $exitCode
    }
}

if ($MyInvocation.InvocationName -ne '.') {
    $result = Invoke-RepositoryHygieneMain
    $result.Lines
    exit $result.ExitCode
}
