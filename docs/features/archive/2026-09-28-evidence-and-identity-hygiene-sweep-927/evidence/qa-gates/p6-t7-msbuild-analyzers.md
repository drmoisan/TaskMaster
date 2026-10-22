# P6-T7 C# analyzer pass (AC13 analyzer clause)

## iter1

Timestamp: 2026-09-29T22-18
Command: MSBUILD-ANALYZERS: pwsh -NoProfile -Command '$vw = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"; $mb = & $vw -latest -prerelease -products * -requires Microsoft.Component.MSBuild -find "MSBuild/**/Bin/MSBuild.exe" | Select-Object -First 1; "MSBUILD-LEAF=" + (Split-Path -Leaf $mb); & $mb TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true 2>&1 | Tee-Object -FilePath coverage/logs/927-analyzers.log; exit $LASTEXITCODE' (run from the item worktree root; the console echo was discarded with Out-Null after Tee-Object and the exit code printed as MSBUILD-EXIT, the log file content being unchanged); then MSBUILD-OBSERVE over coverage/logs/927-analyzers.log, with the warning-count and elapsed-time lines printed from the same log.
EXIT_CODE: 0
Output Summary:
- Outlook running-process count before the build: OUTLOOK-PROCESSES=0 (as in P0-T12)
- MSBUILD-LEAF=MSBuild.exe; MSBUILD-EXIT=0
- SUCCEEDED=1
- ZERO-ERRORS=1
- OUT-LINES=36 (at least 1: each project's csc command line was echoed, so CoreCompile ran)
- SKIP-CORECOMPILE=0
- Warning count line: "0 Warning(s)"; 0 is not greater than the P0-T12 BASELINE-WARNINGS: 0
- Time Elapsed 00:00:17.88
- The raw log stays under the ignored coverage/logs/ directory and is not committed.
