Timestamp: 2026-08-31T11-05
Command: pwsh -NoProfile -Command 'if (Get-Command msbuild -ErrorAction SilentlyContinue) { "ON_PATH" } else { "NOT_ON_PATH" }'
EXIT_CODE: 0

## Final-QC command audit

The final-QC commands were recorded in this order:

1. `dotnet tool run csharpier format .`
2. `pwsh -NoProfile -Command '$vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\\Installer\\vswhere.exe"; $msbuildExe = & $vswhere -latest -requires Microsoft.Component.MSBuild -find "MSBuild\\**\\Bin\\MSBuild.exe" | Select-Object -First 1; & $msbuildExe TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true; "EXIT_CODE=$LASTEXITCODE"'`
3. `pwsh -NoProfile -Command '$vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\\Installer\\vswhere.exe"; $msbuildExe = & $vswhere -latest -requires Microsoft.Component.MSBuild -find "MSBuild\\**\\Bin\\MSBuild.exe" | Select-Object -First 1; & $msbuildExe TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true; "EXIT_CODE=$LASTEXITCODE"'`
4. `pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput coverage\p7-t5-postchange.cobertura.xml`

The format step uses the manifest-pinned tool through `dotnet tool run`; no global CSharpier binary was used. Both MSBuild commands carry `/t:Rebuild`, not `/t:Build`; their source transcripts record `(Rebuild target(s)): observed.`

## Nullable opt-in token discipline

Command: `rg -n 'Nullable=enable' docs/features/active/2026-08-26-breadcrumb-selectrow-emits-rooted-path-leaving-d1-half-closed-637 --glob '**/evidence/**/*.md' --glob '!**/p7-t10-toolchain-audit.md'`

EXIT_CODE: 1

ExpectedExitCode: 1

Output Summary: The scan returned zero matches across the feature evidence subtree other than this artifact. All final-QC evidence records `NULLABLE_OPT_IN_PROPERTY: absent` instead of the solution-wide opt-in token.

## MSBuild resolution

The PATH probe returned `ON_PATH`.

Command: `pwsh -NoProfile -Command '(Get-Command msbuild).Source'`

EXIT_CODE: 0

Output: `C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe`

The vswhere resolution produced the same binary. The vswhere-resolved absolute path remains in use for determinism because the repository does not pin the PATH entry.

## Test-runner substitution

The test command is `scripts/vscode/Invoke-MSTestWithCoverage.ps1`. Its inner vstest invocation at `Invoke-MSTestWithCoverage.ps1:76` carries `/InIsolation` and `/TestCaseFilter:TestCategory!=LiveOutlook`; coverage is collected with `dotnet-coverage --output-format cobertura`, not `/EnableCodeCoverage`. This is the repository-standard runner and corresponds to `.github/workflows/_mstest-coverage.yml:83`.
