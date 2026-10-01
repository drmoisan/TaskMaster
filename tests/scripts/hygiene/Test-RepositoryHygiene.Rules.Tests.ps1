Set-StrictMode -Version Latest

# Pester tests for the pure rule functions of the repository hygiene guard.
# Every violating fixture is assembled by concatenation at run time (drive letter, colon,
# separator, profile parent and segment as separate literals) so that this tracked file never
# contains a contiguous drive-rooted profile path. XML fixtures are here-strings; the guard's
# extension gate keeps them out of scope for a .ps1 file. No test touches the file system.

Describe 'Find-UserProfilePathMatch' {
    BeforeAll {
        . (Join-Path -Path $PSScriptRoot -ChildPath '../../../scripts/hygiene/Test-RepositoryHygiene.ps1')
        $script:Drive = 'C' + ':'
        $script:Parent = 'Users'
        $script:Segment = 'fixtureuser'
    }

    It 'matches a backslash-separated profile path assembled at run time' {
        $text = 'Output written to ' + $script:Drive + '\' + $script:Parent + '\' + $script:Segment + '\repos\x'

        $result = @(Find-UserProfilePathMatch -Text $text)

        $result.Count | Should -Be 1 -Because 'a backslash-separated profile path is a violation'
    }

    It 'matches a forward-slash-separated profile path' {
        $text = 'cd ' + $script:Drive + '/' + $script:Parent + '/' + $script:Segment + '/repos'

        $result = @(Find-UserProfilePathMatch -Text $text)

        $result.Count | Should -Be 1 -Because 'forward slashes are accepted separators'
    }

    It 'matches a doubled-backslash profile path' {
        $text = '{"root": "' + $script:Drive + '\\' + $script:Parent + '\\' + $script:Segment + '\\repos"}'

        $result = @(Find-UserProfilePathMatch -Text $text)

        $result.Count | Should -Be 1 -Because 'a JSON-escaped separator run is still a separator run'
    }

    It 'matches a lower-case drive letter and profile parent' {
        $text = 'c' + ':' + '\' + 'users' + '\' + $script:Segment + '\AppData'

        $result = @(Find-UserProfilePathMatch -Text $text)

        $result.Count | Should -Be 1 -Because 'the pattern is case-insensitive'
    }

    It 'matches an upper-case profile parent' {
        $text = $script:Drive + '\' + 'USERS' + '\' + $script:Segment + '\Documents'

        $result = @(Find-UserProfilePathMatch -Text $text)

        $result.Count | Should -Be 1 -Because 'the profile parent is matched in any case'
    }

    It 'matches an eight-dot-three user segment' {
        $shortSegment = 'FIXTUR' + '~' + '1'
        $text = $script:Drive + '\' + $script:Parent + '\' + $shortSegment + '\AppData\Local'

        $result = @(Find-UserProfilePathMatch -Text $text)

        $result.Count | Should -Be 1 -Because 'an eight-dot-three segment begins with a letter'
    }

    It 'does not match a user-profile placeholder path' {
        $text = '<user-profile>\repos\x and ' + $script:Drive + '\' + $script:Parent + '\' + '<user>' + '\repos'

        $result = @(Find-UserProfilePathMatch -Text $text)

        $result.Count | Should -Be 0 -Because 'a placeholder segment begins with a less-than sign'
    }

    It 'does not match a repo-root placeholder path' {
        $text = 'Run <repo-root>\scripts\hygiene\Test-RepositoryHygiene.ps1 from <repo-root>'

        $result = @(Find-UserProfilePathMatch -Text $text)

        $result.Count | Should -Be 0 -Because 'the repo-root placeholder carries no drive or profile parent'
    }

    It 'does not match a bare drive root' {
        $text = 'Drive roots ' + $script:Drive + '\' + ' and ' + $script:Drive + '/' + ' only'

        $result = @(Find-UserProfilePathMatch -Text $text)

        $result.Count | Should -Be 0 -Because 'a drive root without the profile parent is not a profile path'
    }

    It 'does not match a fixtures root' {
        $text = $script:Drive + '\' + 'Fixtures' + '\' + 'testuser' + '\OneDrive - Contoso'

        $result = @(Find-UserProfilePathMatch -Text $text)

        $result.Count | Should -Be 0 -Because 'the fixtures root replaces the profile parent'
    }

    It 'reports the line number and not the matched text' {
        $violation = $script:Drive + '\' + $script:Parent + '\' + $script:Segment + '\repos'
        $text = "first line`r`nsecond line " + $violation + "`r`nthird line"

        $result = @(Find-UserProfilePathMatch -Text $text)

        $result.Count | Should -Be 1 -Because 'exactly one line carries the violation'
        $result[0].LineNumber | Should -Be 2 -Because 'the violation is on the second line'
        @($result[0].PSObject.Properties.Name) | Should -Be @('LineNumber') -Because 'the record carries the line number only'
    }
}

Describe 'Get-RawEvidenceDocumentKind' {
    BeforeAll {
        . (Join-Path -Path $PSScriptRoot -ChildPath '../../../scripts/hygiene/Test-RepositoryHygiene.ps1')
    }

    It 'classifies a trx extension as trx' {
        $kind = Get-RawEvidenceDocumentKind -RelativePath 'docs/features/x/evidence/run.trx' -Content ''

        $kind | Should -Be 'trx' -Because 'the extension gate runs before the empty-content return'
    }

    It 'classifies a coverage root on its own line as cobertura' {
        $content = @'
<?xml version="1.0" encoding="utf-8"?>
<coverage
  line-rate="0.5" branch-rate="0.5" version="1.9">
  <packages />
</coverage>
'@

        $kind = Get-RawEvidenceDocumentKind -RelativePath 'docs/features/x/evidence/coverage.xml' -Content $content

        $kind | Should -Be 'cobertura' -Because 'a root name terminated by a line break is still the coverage root'
    }

    It 'classifies a results root as dotnet-coverage' {
        $content = @'
<?xml version="1.0" encoding="utf-8"?>
<results>
  <modules />
</results>
'@

        $kind = Get-RawEvidenceDocumentKind -RelativePath 'docs/features/x/evidence/native.xml' -Content $content

        $kind | Should -Be 'dotnet-coverage' -Because 'a results root is the native collector document'
    }

    It 'classifies a report root with class elements as jacoco-raw' {
        $content = @'
<?xml version="1.0" encoding="UTF-8" standalone="yes"?>
<report name="Pester">
  <package name="scripts/x">
    <class name="scripts/x/Tool" sourcefilename="Tool.ps1">
      <method name="Get-Thing" desc="()" line="3" />
    </class>
    <sourcefile name="Tool.ps1">
      <line nr="3" mi="0" ci="1" mb="0" cb="0" />
    </sourcefile>
  </package>
</report>
'@

        $kind = Get-RawEvidenceDocumentKind -RelativePath 'docs/features/x/evidence/pester.jacoco.xml' -Content $content

        $kind | Should -Be 'jacoco-raw' -Because 'class, method, sourcefile and line elements mark a raw document'
    }

    It 'classifies a package-only report root as jacoco-projection' {
        $content = @'
<report name="TaskMaster">
  <package name="QuickFiler">
    <counter type="LINE" missed="1" covered="9" />
    <counter type="BRANCH" missed="1" covered="3" />
  </package>
</report>
'@

        $kind = Get-RawEvidenceDocumentKind -RelativePath 'docs/features/x/evidence/coverage.jacoco.xml' -Content $content

        $kind | Should -Be 'jacoco-projection' -Because 'package and counter children only form a permitted projection'
    }

    It 'classifies a report root behind a DOCTYPE' {
        $content = @'
<?xml version="1.0" encoding="UTF-8" standalone="no"?>
<!-- generated by the coverage writer -->
<!DOCTYPE report PUBLIC "-//JACOCO//DTD Report 1.1//EN" "report.dtd">
<report name="Pester">
  <sessioninfo id="x" start="0" dump="0" />
  <package name="scripts/x">
    <class name="scripts/x/Tool" sourcefilename="Tool.ps1" />
  </package>
</report>
'@

        $kind = Get-RawEvidenceDocumentKind -RelativePath 'docs/features/x/evidence/pester-coverage.xml' -Content $content

        $kind | Should -Be 'jacoco-raw' -Because 'the declaration, comment and DOCTYPE are skipped before the root is read'
    }

    It 'classifies a byte-order-mark prefixed document' {
        $content = [string][char]0xFEFF + '<?xml version="1.0"?><coverage line-rate="1"></coverage>'

        $kind = Get-RawEvidenceDocumentKind -RelativePath 'docs/features/x/evidence/bom.xml' -Content $content

        $kind | Should -Be 'cobertura' -Because 'a leading byte-order mark is stripped before the root is read'
    }

    It 'classifies a ps1 file containing a TestRun element as none' {
        $content = @'
$fixture = @"
<TestRun id="1" name="run">
  <ResultSummary outcome="Completed" />
</TestRun>
"@
'@

        $kind = Get-RawEvidenceDocumentKind -RelativePath 'tests/scripts/x/Fixture.Tests.ps1' -Content $content

        $kind | Should -Be 'none' -Because 'the extension gate keeps non-XML files out of rule A'
    }
}
