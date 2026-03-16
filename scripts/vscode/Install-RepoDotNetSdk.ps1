[CmdletBinding()]
param(
    [string]$Version = '8.0.205',

    [ValidateSet('x64', 'x86', 'arm64')]
    [string]$Architecture = 'x64',

    [string]$InstallDir,

    [switch]$Force
)

Set-StrictMode -Version Latest

function Get-RepoDotNetSdkDownloadUrl {
    [CmdletBinding()]
    param(
        [Parameter(Mandatory = $true)]
        [string]$Version,

        [Parameter(Mandatory = $true)]
        [ValidateSet('x64', 'x86', 'arm64')]
        [string]$Architecture
    )

    return "https://builds.dotnet.microsoft.com/dotnet/Sdk/$Version/dotnet-sdk-$Version-win-$Architecture.zip"
}

function Get-RepoDotNetSdkInstallDir {
    [CmdletBinding()]
    param(
        [string]$InstallDir
    )

    if ([string]::IsNullOrWhiteSpace($InstallDir)) {
        return [System.IO.Path]::GetFullPath((Join-Path $PSScriptRoot '..\..\.dotnet-sdk'))
    }

    return [System.IO.Path]::GetFullPath($InstallDir)
}

function Install-RepoDotNetSdk {
    [CmdletBinding(SupportsShouldProcess = $true)]
    param(
        [string]$Version = '8.0.205',

        [ValidateSet('x64', 'x86', 'arm64')]
        [string]$Architecture = 'x64',

        [string]$InstallDir,

        [switch]$Force
    )

    $resolvedInstallDir = Get-RepoDotNetSdkInstallDir -InstallDir $InstallDir
    $sdkMarkerPath = Join-Path $resolvedInstallDir (Join-Path 'sdk' $Version)

    if ((-not $Force) -and (Test-Path -LiteralPath $sdkMarkerPath)) {
        Write-Host "Repo-local .NET SDK $Version is already installed at $resolvedInstallDir."
        return
    }

    $downloadUrl = Get-RepoDotNetSdkDownloadUrl -Version $Version -Architecture $Architecture
    $zipPath = Join-Path ([System.IO.Path]::GetTempPath()) ("dotnet-sdk-$Version-win-$Architecture.zip")

    if (-not $PSCmdlet.ShouldProcess($resolvedInstallDir, "Install .NET SDK $Version from $downloadUrl")) {
        return
    }

    [System.IO.Directory]::CreateDirectory($resolvedInstallDir) | Out-Null

    if (Test-Path -LiteralPath $zipPath) {
        Remove-Item -LiteralPath $zipPath -Force
    }

    $client = [System.Net.Http.HttpClient]::new()

    try {
        Write-Host "Downloading .NET SDK $Version from $downloadUrl..."
        $response = $client.GetAsync($downloadUrl, [System.Net.Http.HttpCompletionOption]::ResponseHeadersRead).GetAwaiter().GetResult()
        [void]$response.EnsureSuccessStatusCode()

        $fileStream = [System.IO.File]::Create($zipPath)
        try {
            [void]$response.Content.CopyToAsync($fileStream).GetAwaiter().GetResult()
        }
        finally {
            $fileStream.Dispose()
            $response.Dispose()
        }

        Add-Type -AssemblyName System.IO.Compression.FileSystem
        [System.IO.Compression.ZipFile]::ExtractToDirectory($zipPath, $resolvedInstallDir, $true)
    }
    finally {
        $client.Dispose()
        if (Test-Path -LiteralPath $zipPath) {
            Remove-Item -LiteralPath $zipPath -Force
        }
    }

    if (-not (Test-Path -LiteralPath $sdkMarkerPath)) {
        throw "Expected SDK marker '$sdkMarkerPath' was not created."
    }

    Write-Host "Installed repo-local .NET SDK $Version to $resolvedInstallDir."
}

if ($MyInvocation.InvocationName -ne '.') {
    Install-RepoDotNetSdk @PSBoundParameters
}
