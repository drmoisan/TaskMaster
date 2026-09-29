Set-StrictMode -Version Latest

# Pester tests for the git seam of the repository hygiene guard. Invoke-GitExe is mocked (never
# the git executable) and file content is supplied as in-memory byte arrays, so no test invokes
# git or touches the file system.

Describe 'Get-TrackedFileRecord' {
    BeforeAll {
        . (Join-Path -Path $PSScriptRoot -ChildPath '../../../scripts/hygiene/Test-RepositoryHygiene.ps1')
    }

    It 'parses a NUL-separated eol listing into path records' {
        Mock Invoke-GitExe {
            param([string[]]$GitArgs)
            $null = $GitArgs
            "i/lf    w/lf    attr/text=auto         `tdocs/a.md`0i/crlf  w/crlf  attr/text=auto         `tscripts/b c.ps1`0i/lf    w/lf    attr/                  `tREADME.md`0"
        }

        $records = @(Get-TrackedFileRecord)

        $records.Count | Should -Be 3 -Because 'the listing carries three NUL-terminated records'
        $records[0].Path | Should -Be 'docs/a.md'
        $records[1].Path | Should -Be 'scripts/b c.ps1' -Because 'a path containing a space is kept whole'
        $records[2].Path | Should -Be 'README.md'
        @($records | Where-Object { $_.IsBinaryInIndex }).Count | Should -Be 0 -Because 'no record is binary in the index'
        Should -Invoke Invoke-GitExe -Times 1 -Exactly -ParameterFilter { ($GitArgs -join ' ') -eq 'ls-files --eol -z' }

        # A record without the tab that separates the attributes from the path fails fast.
        Mock Invoke-GitExe {
            param([string[]]$GitArgs)
            $null = $GitArgs
            "i/lf    w/lf    attr/text=auto         docs/no-separator.md`0"
        }
        { Get-TrackedFileRecord } | Should -Throw -ExpectedMessage '*Malformed git ls-files --eol record*'
    }

    It 'flags an index-binary record' {
        Mock Invoke-GitExe {
            param([string[]]$GitArgs)
            $null = $GitArgs
            "i/-text w/-text attr/                  `tassets/logo.png`0i/lf    w/lf    attr/text=auto         `tdocs/a.md`0"
        }
        $reader = { param([string]$FilePath) $null = $FilePath; , [byte[]](0x89, 0x50, 0x4E, 0x47) }

        $records = @(Get-TrackedFileRecord)
        $text = Read-TrackedFileText -Path 'assets/logo.png' -ReadContent $reader -IsBinaryInIndex

        $records[0].IsBinaryInIndex | Should -BeTrue -Because 'the index attribute segment reads i/-text'
        $records[1].IsBinaryInIndex | Should -BeFalse -Because 'a text record is not binary in the index'
        $text | Should -BeNullOrEmpty -Because 'an index-binary record without a UTF-16 mark is not scanned'
    }
}

Describe 'Read-TrackedFileText' {
    BeforeAll {
        . (Join-Path -Path $PSScriptRoot -ChildPath '../../../scripts/hygiene/Test-RepositoryHygiene.ps1')
    }

    It 'decodes UTF-16 little-endian bytes by byte-order mark' {
        $encoding = [System.Text.Encoding]::Unicode
        $bytes = [byte[]]($encoding.GetPreamble() + $encoding.GetBytes('line one'))
        $reader = { param([string]$FilePath) $null = $FilePath; , $bytes }.GetNewClosure()

        $text = Read-TrackedFileText -Path 'docs/utf16.txt' -ReadContent $reader -IsBinaryInIndex

        $text | Should -BeExactly 'line one' -Because 'a UTF-16 file is decoded by its mark even when it is binary in the index'
    }

    It 'decodes UTF-8 bytes without a byte-order mark' {
        Mock Get-Content { [System.Text.Encoding]::UTF8.GetBytes('plain text') }

        $text = Read-TrackedFileText -Path 'docs/plain.md'

        $text | Should -BeExactly 'plain text' -Because 'the default reader returns bytes that are decoded as UTF-8'
        Should -Invoke Get-Content -Times 1 -Exactly
    }
}

Describe 'Assert-GitExitCode' {
    BeforeAll {
        . (Join-Path -Path $PSScriptRoot -ChildPath '../../../scripts/hygiene/Test-RepositoryHygiene.ps1')
    }

    It 'throws when the git wrapper reports a non-zero exit' {
        $gitArgs = @('ls-files', '--eol', '-z')

        { Assert-GitExitCode -ExitCode 128 -GitArgs $gitArgs } | Should -Throw -ExpectedMessage '*ls-files --eol -z*'
        { Assert-GitExitCode -ExitCode 0 -GitArgs $gitArgs } | Should -Not -Throw
    }
}
