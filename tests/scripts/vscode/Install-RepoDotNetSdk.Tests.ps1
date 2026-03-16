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
