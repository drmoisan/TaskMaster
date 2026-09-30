# P0-T11 — Nullable rebuild baseline (CMD-MSBUILD-NULLABLE)

Timestamp: 2026-09-30T09-22
Command: pwsh -NoProfile -Command 'Set-Location "<execution-worktree-root>"; msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true "/flp:LogFile=coverage\nullable.msbuild.log;Verbosity=normal" | Out-Null; "MSBUILD_EXIT=$LASTEXITCODE"; $l = "coverage\nullable.msbuild.log"; "OUT_LINES=" + @(Select-String -LiteralPath $l -SimpleMatch -Pattern ("/out:obj" + [char]92 + "Debug" + [char]92)).Count; Get-Content -LiteralPath $l -Tail 8'
EXIT_CODE: 0
OUTLOOK-CLOSED: true
Output Summary:
- CMD-OUTLOOK: @(Get-Process outlook -ErrorAction SilentlyContinue).Count printed 0 (nothing terminated).
- Console output was discarded with Out-Null to keep the capture readable; the msbuild arguments are exactly CMD-MSBUILD-NULLABLE and the summary is read from the file log tail.
- File log tail: "Build succeeded." / "0 Warning(s)" / "0 Error(s)" / "Time Elapsed 00:00:14.18"
- MSBUILD_EXIT=0
- OUT_LINES=36 (at least 18)
- No Nullable property was added and the Rebuild target was used (CLAUDE.md C#1.3).
