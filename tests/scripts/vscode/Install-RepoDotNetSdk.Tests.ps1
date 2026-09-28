Set-StrictMode -Version Latest

BeforeAll {
    $repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..\..')).Path
    $scriptPath = Join-Path $repoRoot 'scripts\vscode\Install-RepoDotNetSdk.ps1'
    . $scriptPath
}

Describe 'Get-RepoDotNetSdkDownloadUrl' {
    It 'returns the deterministic .NET 8 SDK archive URL used by the repo-local formatter workaround' {
        $url = Get-RepoDotNetSdkDownloadUrl -Version '8.0.205' -Architecture 'x64'

        $url | Should -Be 'https://builds.dotnet.microsoft.com/dotnet/Sdk/8.0.205/dotnet-sdk-8.0.205-win-x64.zip'
    }
}

Describe 'Get-RepoDotNetSdkInstallDir' {
    It 'resolves the default repo-local install directory from the script directory' {
        # No install directory is supplied, so the helper derives one from the directory of the
        # production script it is defined in. This is pure path arithmetic: nothing is read from
        # or written to disk.
        $resolved = Get-RepoDotNetSdkInstallDir

        $resolved | Should -Be ([System.IO.Path]::GetFullPath((Join-Path $repoRoot '.dotnet-sdk')))
    }

    It 'resolves an explicitly supplied install directory to a full path' {
        # An explicitly supplied directory is normalised rather than returned verbatim. The input
        # carries a redundant current-directory segment so a pass-through implementation would be
        # distinguishable from a normalising one. The path is never created or touched.
        $resolved = Get-RepoDotNetSdkInstallDir -InstallDir 'C:\alpha\.\beta'

        $resolved | Should -Be 'C:\alpha\beta'
    }
}

Describe 'Install-RepoDotNetSdk early exits' {
    # Neither case reaches the download or the archive-extraction region. Both are pinned by the
    # invocation count of the path-test cmdlet: the marker check is the first call, and the second
    # call sits below the confirmation guard, so a count of exactly one proves control returned
    # before that region.

    BeforeEach {
        Mock Write-Host {}
        Mock Get-RepoDotNetSdkDownloadUrl { 'https://example.invalid/sdk.zip' }
    }

    It 'returns early when the SDK marker already exists and force is not supplied' {
        # Arrange: the marker is present and -Force is absent, so the already-installed branch is
        # the only path taken.
        Mock Test-Path { $true }

        # Act
        Install-RepoDotNetSdk -Version '8.0.205'

        # Assert
        Should -Invoke Write-Host -Times 1 -Exactly
        Should -Invoke Test-Path -Times 1 -Exactly
        Should -Invoke Get-RepoDotNetSdkDownloadUrl -Times 0 -Exactly
    }

    It 'returns early at the confirmation guard when the operation is not confirmed' {
        # Arrange: the marker is absent, so control passes the already-installed branch and
        # reaches the confirmation guard. The what-if switch is what makes the ShouldProcess call
        # return false. The confirm-suppressing switch is deliberately not used here: it makes the
        # same call return true and lets control reach the download and archive-extraction region.
        Mock Test-Path { $false }

        # Act
        Install-RepoDotNetSdk -Version '8.0.205' -WhatIf

        # Assert
        Should -Invoke Get-RepoDotNetSdkDownloadUrl -Times 1 -Exactly
        Should -Invoke Test-Path -Times 1 -Exactly
        Should -Invoke Write-Host -Times 0 -Exactly
    }
}

Describe 'global.json SDK selection' {
    It 'pins the repository to the repo-local .NET 8 SDK path so dotnet format avoids the broken 10.0.200 host SDK' {
        $globalJsonPath = Join-Path $repoRoot 'global.json'
        $globalJson = Get-Content -LiteralPath $globalJsonPath -Raw | ConvertFrom-Json

        $globalJson.sdk.version | Should -Be '8.0.205'
        $globalJson.sdk.rollForward | Should -Be 'latestFeature'
        $globalJson.sdk.allowPrerelease | Should -BeFalse
        $globalJson.sdk.paths | Should -Contain '.dotnet-sdk'
        $globalJson.sdk.paths | Should -Contain '$host$'
    }
}
