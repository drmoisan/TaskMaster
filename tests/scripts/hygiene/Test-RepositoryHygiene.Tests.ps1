Set-StrictMode -Version Latest

# Pester tests for the orchestration entry point of the repository hygiene guard.
# Invoke-GitExe is mocked (never the git executable) and returns an in-memory NUL-separated eol
# listing; file content is injected through the -ReadContent delegate keyed by path. Every
# violating fixture is assembled by concatenation at run time, so this tracked file never carries
# a contiguous drive-rooted profile path, and no test touches the file system.

Describe 'Invoke-RepositoryHygieneMain' {
    BeforeAll {
        . (Join-Path -Path $PSScriptRoot -ChildPath '../../../scripts/hygiene/Test-RepositoryHygiene.ps1')

        $script:Segment = 'fixtureuser'
        $script:Violation = 'C' + ':' + '\' + 'Users' + '\' + $script:Segment + '\repos\x'
        $script:Projection = @'
<report name="TaskMaster">
  <package name="QuickFiler">
    <counter type="LINE" missed="1" covered="9" />
    <counter type="BRANCH" missed="1" covered="3" />
  </package>
</report>
'@
        # A byte-array value is returned with the unary comma so it reaches the adapter whole.
        $script:Reader = {
            param([string]$FilePath)
            $value = $script:Content[$FilePath]
            if ($value -is [byte[]]) { , $value } else { $value }
        }

        function ConvertTo-EolListing {
            [CmdletBinding()]
            param([Parameter(Mandatory = $true)][string[]]$Path)

            $records = foreach ($item in $Path) { "i/lf    w/lf    attr/text=auto         `t" + $item + "`0" }
            -join $records
        }
    }

    BeforeEach {
        $script:Content = @{}
        $script:Listing = ''
        Mock Invoke-GitExe {
            param([string[]]$GitArgs)
            $null = $GitArgs
            $script:Listing
        }
    }

    It 'excludes governance-directory records and reports the remaining violation' {
        $governed = '.claude/agents/notes.md'
        $reported = 'docs/features/x/notes.md'
        $retained = 'docs/features/x/evidence/coverage.jacoco.xml'
        $script:Listing = ConvertTo-EolListing -Path @($governed, $reported, $retained)
        $script:Content[$governed] = "heading`n" + $script:Violation
        $script:Content[$reported] = "heading`n" + $script:Violation
        $script:Content[$retained] = $script:Projection

        $result = Invoke-RepositoryHygieneMain -ReadContent $script:Reader

        $findings = @($result.Lines | Where-Object { $_ -like 'HYGIENE profile-path *' })
        $findings.Count | Should -Be 1 -Because 'the governance-directory record is dropped before any rule runs'
        $findings[0] | Should -BeExactly ('HYGIENE profile-path ' + $reported + ':2')
        $result.FindingCount | Should -Be 1 -Because 'only the docs Markdown record is a violation'
    }

    It 'reports a raw document record as a finding' {
        $raw = 'docs/features/x/evidence/run.trx'
        $script:Listing = ConvertTo-EolListing -Path @($raw)
        $script:Content[$raw] = '<TestRun id="1" />'

        $result = Invoke-RepositoryHygieneMain -ReadContent $script:Reader

        @($result.Lines) | Should -Contain ('HYGIENE raw-document ' + $raw)
        $result.FindingCount | Should -Be 1 -Because 'a raw document produces exactly one finding'
        $result.ExitCode | Should -Be 1 -Because 'a raw document fails the guard'
    }

    It 'retains a package-level projection record' {
        $retained = 'docs/features/x/evidence/coverage.jacoco.xml'
        $script:Listing = ConvertTo-EolListing -Path @($retained)
        $script:Content[$retained] = $script:Projection

        $result = Invoke-RepositoryHygieneMain -ReadContent $script:Reader

        $result.FindingCount | Should -Be 0 -Because 'a package-level projection is a permitted evidence form'
        @($result.Lines) | Should -Be @('HYGIENE Findings=0')
    }

    It 'returns a non-zero exit decision when findings exist' {
        $leaking = 'docs/features/x/leak.md'
        $testRun = 'docs/features/x/evidence/run-results.xml'
        $openCover = 'docs/features/x/evidence/opencover.xml'
        $binaryCoverage = 'docs/features/x/evidence/run.coverage'
        $coverageXml = 'docs/features/x/evidence/run.coveragexml'
        $script:Listing = ConvertTo-EolListing -Path @($leaking, $testRun, $openCover, $binaryCoverage, $coverageXml)
        $script:Content[$leaking] = $script:Violation
        $script:Content[$testRun] = '<?xml version="1.0"?><TestRun id="1"></TestRun>'
        $script:Content[$openCover] = '<CoverageSession><Summary /></CoverageSession>'

        $result = Invoke-RepositoryHygieneMain -ReadContent $script:Reader

        $result.FindingCount | Should -Be 5 -Because 'one profile-path line and four raw documents are reported'
        @($result.Lines | Where-Object { $_ -like 'HYGIENE raw-document *' }).Count | Should -Be 4 -Because 'TestRun and CoverageSession roots and both collector extensions are raw documents'
        $result.ExitCode | Should -Be 1 -Because 'the exit decision is one when the findings total is non-zero'
        @($result.Lines)[-1] | Should -BeExactly 'HYGIENE Findings=5'
    }

    It 'returns a zero exit decision over clean content' {
        $clean = 'docs/features/x/clean.md'
        $empty = 'docs/features/x/empty.md'
        $absent = 'docs/features/x/absent.md'
        $markedUtf8 = 'docs/features/x/marked-utf8.md'
        $bigEndian = 'docs/features/x/big-endian.md'
        $emptyBytes = 'docs/features/x/empty-bytes.md'
        $emptyXml = 'docs/features/x/empty.xml'
        $projectXml = 'docs/features/x/project.xml'
        $proseXml = 'docs/features/x/prose.xml'
        $cleanText = 'Run <repo-root>\scripts\hygiene\Test-RepositoryHygiene.ps1 as <user> on <host>'
        $script:Listing = ConvertTo-EolListing -Path @($clean, $empty, $absent, $markedUtf8, $bigEndian, $emptyBytes, $emptyXml, $projectXml, $proseXml)
        $script:Content[$clean] = $cleanText
        $script:Content[$empty] = ''
        $script:Content[$absent] = $null
        $script:Content[$markedUtf8] = [byte[]]([System.Text.Encoding]::UTF8.GetPreamble() + [System.Text.Encoding]::UTF8.GetBytes($cleanText))
        $script:Content[$bigEndian] = [byte[]]([System.Text.Encoding]::BigEndianUnicode.GetPreamble() + [System.Text.Encoding]::BigEndianUnicode.GetBytes($cleanText))
        $script:Content[$emptyBytes] = [byte[]]@()
        $script:Content[$emptyXml] = ''
        $script:Content[$projectXml] = '<Project Sdk="Microsoft.NET.Sdk"><PropertyGroup /></Project>'
        $script:Content[$proseXml] = 'plain words without any element'

        $result = Invoke-RepositoryHygieneMain -ReadContent $script:Reader

        $result.FindingCount | Should -Be 0 -Because 'placeholders in any encoding, empty text, null text and non-raw XML carry no violation'
        $result.ExitCode | Should -Be 0 -Because 'the exit decision is zero when the findings total is zero'
        @($result.Lines) | Should -Be @('HYGIENE Findings=0')
    }

    It 'prints path and line only and never the matched text' {
        $leaking = 'docs/features/x/leak.md'
        $script:Listing = ConvertTo-EolListing -Path @($leaking)
        $script:Content[$leaking] = "one`ntwo`nsee " + $script:Violation + ' for details'

        $result = Invoke-RepositoryHygieneMain -ReadContent $script:Reader

        @($result.Lines) | Should -Be @(('HYGIENE profile-path ' + $leaking + ':3'), 'HYGIENE Findings=1')
        @($result.Lines | Where-Object { $_ -like ('*' + $script:Segment + '*') }).Count | Should -Be 0 -Because 'no output line may echo the matched text'
    }

    It 'reports a record whose content reader throws as unreadable' {
        $broken = 'docs/features/x/broken.md'
        $script:Listing = ConvertTo-EolListing -Path @($broken)
        $throwingReader = { param([string]$FilePath) throw ('cannot read ' + $FilePath) }

        $result = Invoke-RepositoryHygieneMain -ReadContent $throwingReader

        @($result.Lines | Where-Object { $_ -like 'HYGIENE unreadable *' }) | Should -Be @('HYGIENE unreadable ' + $broken)
        $result.FindingCount | Should -Be 1 -Because 'an unreadable record is a finding, never skipped silently'
        $result.ExitCode | Should -Be 1
    }

    It 'reports a tracked backup file as a finding and fails the guard' {
        $backup = 'TaskMaster.sln.bak'
        $script:Listing = ConvertTo-EolListing -Path @($backup)
        $script:Content[$backup] = 'plain text'

        $result = Invoke-RepositoryHygieneMain -ReadContent $script:Reader

        @($result.Lines) | Should -Be @('HYGIENE backup-file TaskMaster.sln.bak', 'HYGIENE Findings=1')
        $result.ExitCode | Should -Be 1 -Because 'a tracked backup file fails the guard'
    }

    It 'reports zero findings for backup-lookalike names' {
        $lookalikes = @('docs/bak/notes.md', 'notes.bakery', 'notes.bak.md', 'backup', 'docs/features/x/Makefile')
        $script:Listing = ConvertTo-EolListing -Path $lookalikes
        foreach ($item in $lookalikes) { $script:Content[$item] = 'plain text' }

        $result = Invoke-RepositoryHygieneMain -ReadContent $script:Reader

        @($result.Lines) | Should -Be @('HYGIENE Findings=0')
        $result.FindingCount | Should -Be 0 -Because 'none of the lookalike names has a final .bak extension'
        $result.ExitCode | Should -Be 0
    }

    It 'drops a governance-directory backup record before the backup rule runs' {
        $governed = '.claude/agent-memory/notes.bak'
        $script:Listing = ConvertTo-EolListing -Path @($governed)
        $script:Content[$governed] = 'plain text'

        $result = Invoke-RepositoryHygieneMain -ReadContent $script:Reader

        @($result.Lines) | Should -Be @('HYGIENE Findings=0')
        $result.ExitCode | Should -Be 0 -Because 'the governance skip runs ahead of the backup rule'
    }

    It 'still scans a backup file for a profile path' {
        $backup = 'docs/features/x/old.bak'
        $script:Listing = ConvertTo-EolListing -Path @($backup)
        $script:Content[$backup] = $script:Violation

        $result = Invoke-RepositoryHygieneMain -ReadContent $script:Reader

        @($result.Lines) | Should -Be @('HYGIENE backup-file docs/features/x/old.bak', 'HYGIENE profile-path docs/features/x/old.bak:1', 'HYGIENE Findings=2')
        $result.ExitCode | Should -Be 1 -Because 'both the backup-file rule and the profile-path rule fire'
    }
}
