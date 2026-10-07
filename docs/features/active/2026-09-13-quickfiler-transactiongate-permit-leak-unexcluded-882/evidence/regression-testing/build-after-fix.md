# Build After Fix (P3-T2)

Timestamp: 2026-09-29T09-08
Command: pwsh -NoProfile -Command 'Set-Location "<repo-root>"; $m = & (Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio/Installer/vswhere.exe") -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1; $start = [DateTime]::UtcNow; & $m QuickFiler.Test\QuickFiler.Test.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU 2>&1 | Tee-Object -FilePath coverage/logs/build-after-fix.log | Out-Null; "EXIT=$LASTEXITCODE"; Select-String -LiteralPath coverage/logs/build-after-fix.log -Pattern "Build succeeded|Build FAILED|Warning\(s\)$|Error\(s\)$" | ForEach-Object { $_.Line.Trim() }; "FIXTURE-WARNINGS=" + @(Select-String -LiteralPath coverage/logs/build-after-fix.log -Pattern "UiThreadDispatcherFixture.*warning|warning.*UiThreadDispatcherFixture").Count; "DLL-FRESH=" + ((Get-Item QuickFiler.Test/bin/Debug/QuickFiler.Test.dll).LastWriteTimeUtc -gt $start)'
EXIT_CODE: 0
Output Summary:
- Build succeeded.
- 0 Warning(s)
- 0 Error(s)
- FIXTURE-WARNINGS=0 (no warning names either Write Set file)
- DLL-FRESH=True
- Log retained at the gitignored path coverage/logs/build-after-fix.log.
