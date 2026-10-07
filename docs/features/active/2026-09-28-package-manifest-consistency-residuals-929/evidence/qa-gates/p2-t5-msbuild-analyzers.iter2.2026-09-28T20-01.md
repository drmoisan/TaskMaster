# P2-T5 — C# QC step 2, analyzer rebuild (iteration 2)

Timestamp: 2026-09-30T11-01
Command: CMD-OUTLOOK (pwsh -NoProfile -Command '"OUTLOOK_COUNT=" + @(Get-Process outlook -ErrorAction SilentlyContinue).Count'); pwsh -NoProfile -Command 'Set-Location "<execution-worktree-root>"; msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true "/flp:LogFile=coverage\analyzers.msbuild.log;Verbosity=normal" | Out-Null; "MSBUILD_EXIT=$LASTEXITCODE"; ...OUT_LINES...; ...CS0006_LINES...; Get-Content -LiteralPath $l -Tail 6'
EXIT_CODE: 0
OUTLOOK-CLOSED: true
Output Summary:
- CMD-OUTLOOK printed OUTLOOK_COUNT=0 (nothing terminated).
- Console output was discarded with Out-Null; the msbuild arguments are exactly CMD-MSBUILD-ANALYZERS and the summary is read from the file log tail.
- File log tail: "Build succeeded." / "0 Warning(s)" / "0 Error(s)" / "Time Elapsed 00:00:25.28"
- MSBUILD_EXIT=0
- OUT_LINES=36 (at least 18; equal to the P0-T10 value 36)
- CS0006_LINES=0
- This is the post-change solution rebuild AC7 names.
