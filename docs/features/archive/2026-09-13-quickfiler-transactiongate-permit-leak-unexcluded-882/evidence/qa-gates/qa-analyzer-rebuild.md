# QA Analyzer Rebuild (P4-T3)

Timestamp: 2026-09-29T09-13
Command: pwsh -NoProfile -Command 'Set-Location "<repo-root>"; $m = & (Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio/Installer/vswhere.exe") -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1; $start = [DateTime]::UtcNow; & $m TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true 2>&1 | Tee-Object -FilePath coverage/logs/qa-analyzer-rebuild.log | Out-Null; "EXIT=$LASTEXITCODE"; Select-String -LiteralPath coverage/logs/qa-analyzer-rebuild.log -Pattern "Build succeeded|Build FAILED|Warning\(s\)$|Error\(s\)$" | ForEach-Object { $_.Line.Trim() }; "FIXTURE-WARNINGS=" + @(Select-String -LiteralPath coverage/logs/qa-analyzer-rebuild.log -Pattern "UiThreadDispatcherFixture.*warning|warning.*UiThreadDispatcherFixture").Count; "DLL-FRESH=" + ((Get-Item QuickFiler.Test/bin/Debug/QuickFiler.Test.dll).LastWriteTimeUtc -gt $start)'
EXIT_CODE: 0
ITERATION: 1
Output Summary:
- Build succeeded.
- 0 Warning(s)
- 0 Error(s)
- ANALYZER-WARNINGS: 0 (BASELINE-ANALYZER-WARNINGS: 0; not greater)
- ANALYZER-ERRORS: 0
- FIXTURE-WARNINGS=0
- DLL-FRESH=True
- Log retained at the gitignored path coverage/logs/qa-analyzer-rebuild.log.
