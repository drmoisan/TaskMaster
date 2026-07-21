Timestamp: 2026-07-21T15-27Z

Command: `$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'; if (-not (Test-Path -LiteralPath $vswhere)) { throw 'vswhere.exe not found.' }; $vstest = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1; if (-not $vstest -or -not (Test-Path -LiteralPath $vstest)) { throw 'vstest.console.exe not found via vswhere.' }; Get-Command csharpier, msbuild, dotnet-coverage | Select-Object Name, Source, Version; Get-Item -LiteralPath $vstest | Select-Object FullName, VersionInfo`

EXIT_CODE: 0

| Tool | Absolute path | Version |
|---|---|---|
| CSharpier | `C:\Users\DanMoisan\.dotnet\tools\csharpier.exe` | 1.3.0.0 |
| MSBuild | `C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe` | 18.8.2.0 |
| dotnet-coverage | `C:\Users\DanMoisan\.dotnet\tools\dotnet-coverage.exe` | 18.5.2.0 |
| VSTest | `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe` | 18.800.26.27701 |

ResolvedVSTestPath: C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe

Output Summary: CSharpier, MSBuild, dotnet-coverage, and vswhere-resolved VSTest are installed. Every direct test command will resolve and invoke the recorded absolute VSTest path in the same PowerShell process.
