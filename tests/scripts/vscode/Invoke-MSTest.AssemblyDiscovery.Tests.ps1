Set-StrictMode -Version Latest

BeforeAll {
    $script:repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..\..')).Path
    $script:mstestScript = Join-Path $script:repoRoot 'scripts\vscode\Invoke-MSTest.ps1'

    # Import the existing non-coverage script's definitions. Its top-level wiring is guarded
    # by an InvocationName check, so dot-sourcing imports definitions without running the body.
    . $script:mstestScript
}

Describe 'Get-MSTestAssemblyPathList' {
    # Issue #733 finding 7: the discovery pipeline in Invoke-MSTest.ps1 was a bare, un-wrapped
    # Get-ChildItem | Where-Object | Select-Object assignment, so a zero-match run yielded $null
    # and a one-match run yielded a bare string. Under Set-StrictMode -Version Latest every
    # downstream array member access on those shapes is unsafe. These three cases pin the
    # array-safe contract at the zero, one, and many boundaries.

    It 'returns an empty array when discovery matches nothing' {
        Mock Get-ChildItem { @() }

        { $script:discovered = Get-MSTestAssemblyPathList -SearchRoot 'C:\repo' -Configuration 'Debug' } |
            Should -Not -Throw

        @($script:discovered).Count | Should -Be 0
    }

    It 'returns a single-element array when discovery matches exactly one assembly' {
        Mock Get-ChildItem {
            @([pscustomobject]@{ FullName = 'C:\repo\A.Test\bin\Debug\A.Test.dll' })
        }

        { $script:discovered = Get-MSTestAssemblyPathList -SearchRoot 'C:\repo' -Configuration 'Debug' } |
            Should -Not -Throw

        @($script:discovered).Count | Should -Be 1
    }

    It 'returns every match when discovery matches multiple assemblies' {
        Mock Get-ChildItem {
            @(
                [pscustomobject]@{ FullName = 'C:\repo\A.Test\bin\Debug\A.Test.dll' },
                [pscustomobject]@{ FullName = 'C:\repo\B.Test\bin\Debug\B.Test.dll' },
                [pscustomobject]@{ FullName = 'C:\repo\C.Test\bin\Debug\C.Test.dll' }
            )
        }

        $discovered = Get-MSTestAssemblyPathList -SearchRoot 'C:\repo' -Configuration 'Debug'

        @($discovered).Count | Should -Be 3
    }

    # The three cases above wrap the returned value with @(...) at the assertion site, which
    # restores array shape locally and therefore cannot observe whether the function itself
    # preserved it. The two cases below read the returned value's own shape directly, with no
    # re-wrapping, so they fail if the unary comma in Get-MSTestAssemblyPathList's return is
    # removed and PowerShell's return-value enumeration unwraps the array again.

    It 'returns a value that is itself an array when discovery matches exactly one assembly' {
        Mock Get-ChildItem {
            @([pscustomobject]@{ FullName = 'C:\repo\A.Test\bin\Debug\A.Test.dll' })
        }

        $result = Get-MSTestAssemblyPathList -SearchRoot 'C:\repo' -Configuration 'Debug'

        ($result -is [array]) | Should -BeTrue -Because 'the single-match return must not unwrap to a bare string'
        $result.Count | Should -Be 1
        $result[0] | Should -Be 'C:\repo\A.Test\bin\Debug\A.Test.dll'
    }

    It 'returns a value that is itself an array when discovery matches nothing' {
        Mock Get-ChildItem { @() }

        $result = Get-MSTestAssemblyPathList -SearchRoot 'C:\repo' -Configuration 'Debug'

        ($result -is [array]) | Should -BeTrue -Because 'the zero-match return must not unwrap to $null'
        $result.Count | Should -Be 0
    }
}
