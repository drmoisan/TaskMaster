# P0-T10 — Analyzer rebuild baseline (CMD-MSBUILD-ANALYZERS)

Timestamp: 2026-09-30T09-21
Command: pwsh -NoProfile -Command 'Set-Location "<execution-worktree-root>"; msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true "/flp:LogFile=coverage\analyzers.msbuild.log;Verbosity=normal"; "MSBUILD_EXIT=$LASTEXITCODE"; $l = "coverage\analyzers.msbuild.log"; "OUT_LINES=" + @(Select-String -LiteralPath $l -SimpleMatch -Pattern ("/out:obj" + [char]92 + "Debug" + [char]92)).Count; "CS0006_LINES=" + @(Select-String -LiteralPath $l -SimpleMatch -Pattern "CS0006").Count'
EXIT_CODE: 0
OUTLOOK-CLOSED: true
Output Summary:
- CMD-OUTLOOK: Get-Process outlook count printed 0 (Outlook not running; nothing terminated).
- msbuild summary: "Build succeeded." / "0 Warning(s)" / "0 Error(s)" / "Time Elapsed 00:00:16.19"
- MSBUILD_EXIT=0
- OUT_LINES=36 (at least 18)
- CS0006_LINES=0
- The file log stays under the ignored coverage directory and is not committed.
