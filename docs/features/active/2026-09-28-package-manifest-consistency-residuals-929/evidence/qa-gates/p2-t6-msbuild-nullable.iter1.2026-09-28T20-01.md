# P2-T6 — C# QC step 3, nullable rebuild (iteration 1)

Timestamp: 2026-09-30T10-50
Command: CMD-OUTLOOK (pwsh -NoProfile -Command '"OUTLOOK_COUNT=" + @(Get-Process outlook -ErrorAction SilentlyContinue).Count'); pwsh -NoProfile -Command 'Set-Location "<execution-worktree-root>"; msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true "/flp:LogFile=coverage\nullable.msbuild.log;Verbosity=normal" | Out-Null; "MSBUILD_EXIT=$LASTEXITCODE"; ...OUT_LINES...; Get-Content -LiteralPath $l -Tail 6'
EXIT_CODE: 0
OUTLOOK-CLOSED: true
Output Summary:
- CMD-OUTLOOK printed OUTLOOK_COUNT=0 (nothing terminated).
- Console output was discarded with Out-Null; the msbuild arguments are exactly CMD-MSBUILD-NULLABLE and the summary is read from the file log tail.
- File log tail: "Build succeeded." / "0 Warning(s)" / "0 Error(s)" / "Time Elapsed 00:00:21.14"
- MSBUILD_EXIT=0
- OUT_LINES=36 (at least 18; equal to the P0-T11 value 36)
