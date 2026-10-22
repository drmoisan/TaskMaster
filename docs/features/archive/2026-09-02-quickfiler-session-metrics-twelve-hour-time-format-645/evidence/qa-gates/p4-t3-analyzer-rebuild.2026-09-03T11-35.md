# P4-T3 — Analyzer Rebuild

Timestamp: 2026-09-03T11-35
Command: MSBuild.exe TaskMaster.sln -t:Rebuild -m -p:Configuration=Debug -p:Platform="Any CPU"
-p:EnableNETAnalyzers=true -p:EnforceCodeStyleInBuild=true
(dash-switch form; absolute paths for MSBuild.exe and the solution; see P0-T11 for the
git-bash slash-switch mangling rationale)
EXIT_CODE: 0
Output Summary: Build succeeded. 5 Warning(s) (same identical System.Reactive 7.0.0
packages.config PackagesConfigCheck notices as the baseline). 0 Error(s). Time Elapsed
00:00:16.29. 57 `CoreCompile:` entries confirmed in the log, proving this was a genuine
Rebuild-driven recompile, not an incremental no-op that skipped analyzer execution.

Pre-rebuild QuickFiler.Test.dll LastWriteTimeUtc: 2026-09-03 11:32:36 UTC
Post-rebuild QuickFiler.Test.dll LastWriteTimeUtc: 2026-09-03 11:35:19 UTC
AssemblyRebuilt: True
