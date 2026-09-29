# Baseline Analyzer Rebuild (P0-T12)

Timestamp: 2026-09-29T09-01
Command: pwsh -NoProfile -Command 'Set-Location "<repo-root>"; $m = & (Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio/Installer/vswhere.exe") -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1; $start = [DateTime]::UtcNow; & $m TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true 2>&1 | Tee-Object -FilePath coverage/logs/baseline-analyzer-rebuild.log | Out-Null; "EXIT=$LASTEXITCODE"; Select-String -LiteralPath coverage/logs/baseline-analyzer-rebuild.log -Pattern "Build succeeded|Build FAILED|Warning\(s\)$|Error\(s\)$" | ForEach-Object { $_.Line.Trim() }; "DLL-FRESH=" + ((Get-Item QuickFiler.Test/bin/Debug/QuickFiler.Test.dll).LastWriteTimeUtc -gt $start)'
EXIT_CODE: 0
Output Summary:
- Build succeeded.
- BASELINE-ANALYZER-WARNINGS: 0
- BASELINE-ANALYZER-ERRORS: 0
- DLL-FRESH=True
- Log retained at the gitignored path coverage/logs/baseline-analyzer-rebuild.log.
