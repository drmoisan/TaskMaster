Set-StrictMode -Version Latest

Describe 'Stop-RepoOwnedVSTestProcess' {
    # scripts/vscode/TestProcessCleanup.ps1 defines one function and one alias and has no
    # entry-point body, so dot-sourcing it is already a pure definition operation. Every external
    # dependency the function reaches is mocked here and every mock returns a plain object shape
    # rather than a live process object, so no case starts a process, sleeps, touches the network,
    # or uses a temporary file.

    BeforeAll {
        $script:cleanupRepoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..\..')).Path
        . (Join-Path $script:cleanupRepoRoot 'scripts\vscode\TestProcessCleanup.ps1')

        # Defined inside the setup block, never at file scope, because each It runs in a child
        # scope of the containing block.
        function ConvertTo-ProcessRecord {
            param(
                [string]$Name,
                [int]$ProcessId,
                [int]$ParentProcessId,
                [string]$CommandLine
            )

            [pscustomobject]@{
                Name            = $Name
                ProcessId       = $ProcessId
                ParentProcessId = $ParentProcessId
                CommandLine     = $CommandLine
            }
        }
    }

    BeforeEach {
        $script:capturedProcessIds = $null

        Mock Write-Verbose {}
        Mock Get-Process {
            param($Id)
            $script:capturedProcessIds = $Id
            [pscustomobject]@{ Id = 100 }
        }
        Mock Stop-Process {}
    }

    It 'returns without action when no matching process exists' {
        # Arrange: the CIM query yields nothing, so the early return at the record-count guard
        # is the only path taken.
        Mock Get-CimInstance { @() }

        # Act
        Stop-RepoOwnedVSTestProcess -RepoRoot 'C:\repo'

        # Assert
        Should -Invoke Get-Process -Times 0 -Exactly
        Should -Invoke Stop-Process -Times 0 -Exactly
    }

    It 'skips a vstest process whose command line does not name the repository root' {
        # Arrange: a vstest process owned by a different tree. The command-line filter must drop
        # it, leaving nothing to stop.
        Mock Get-CimInstance {
            @(ConvertTo-ProcessRecord -Name 'vstest.console.exe' -ProcessId 100 -ParentProcessId 1 -CommandLine 'vstest.console.exe C:\other-tree\A.Test.dll')
        }

        # Act
        Stop-RepoOwnedVSTestProcess -RepoRoot 'C:\repo'

        # Assert
        Should -Invoke Get-Process -Times 0 -Exactly
        Should -Invoke Stop-Process -Times 0 -Exactly
    }

    It 'walks the child process tree breadth first from a matching parent' {
        # Arrange: a matching vstest parent, its testhost child, and that child's own child. The
        # breadth-first walk must reach all three, and the identifiers are stopped in descending
        # order so a child is never orphaned before its parent.
        Mock Get-CimInstance {
            @(
                ConvertTo-ProcessRecord -Name 'vstest.console.exe' -ProcessId 100 -ParentProcessId 1 -CommandLine 'vstest.console.exe C:\repo\A.Test.dll'
                ConvertTo-ProcessRecord -Name 'testhost.exe' -ProcessId 200 -ParentProcessId 100 -CommandLine 'testhost.exe'
                ConvertTo-ProcessRecord -Name 'testhost.exe' -ProcessId 300 -ParentProcessId 200 -CommandLine 'testhost.exe'
            )
        }

        # Act
        Stop-RepoOwnedVSTestProcess -RepoRoot 'C:\repo'

        # Assert
        Should -Invoke Get-Process -Times 1 -Exactly
        $script:capturedProcessIds | Should -Be @(300, 200, 100)
        Should -Invoke Stop-Process -Times 1 -Exactly
    }

    It 'returns without action when the process lookup yields nothing' {
        # Arrange: the identifiers are collected, but every one of them has already exited by the
        # time the lookup runs, so the lookup yields nothing and the function returns before the
        # confirmation guard.
        Mock Get-CimInstance {
            @(ConvertTo-ProcessRecord -Name 'vstest.console.exe' -ProcessId 100 -ParentProcessId 1 -CommandLine 'vstest.console.exe C:\repo\A.Test.dll')
        }
        Mock Get-Process { $null }

        # Act
        Stop-RepoOwnedVSTestProcess -RepoRoot 'C:\repo'

        # Assert
        Should -Invoke Get-Process -Times 1 -Exactly
        Should -Invoke Stop-Process -Times 0 -Exactly
    }

    It 'honours the confirmation guard when the operation is not confirmed' {
        # Arrange: -WhatIf makes the ShouldProcess call return false, so control reaches the guard
        # and stops there. The confirm-suppressing switch is deliberately not used: it would make
        # the same call return true and let the stop run.
        Mock Get-CimInstance {
            @(ConvertTo-ProcessRecord -Name 'vstest.console.exe' -ProcessId 100 -ParentProcessId 1 -CommandLine 'vstest.console.exe C:\repo\A.Test.dll')
        }

        # Act
        Stop-RepoOwnedVSTestProcess -RepoRoot 'C:\repo' -WhatIf

        # Assert
        Should -Invoke Get-Process -Times 1 -Exactly
        Should -Invoke Stop-Process -Times 0 -Exactly
    }
}
