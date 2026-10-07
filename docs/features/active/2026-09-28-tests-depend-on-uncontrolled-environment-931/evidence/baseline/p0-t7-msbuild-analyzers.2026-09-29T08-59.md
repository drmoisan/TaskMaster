# P0-T7 Baseline Analyzer Rebuild

Timestamp: 2026-09-29T08-59
Command: msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true (resolved through vswhere, plus /nodeReuse:false; CMD-REBUILD with TASKID p0-t7; file log coverage\logs\p0-t7.msbuild.log; console output discarded, the file log is the observed source)
EXIT_CODE: 0

Output Summary:
- ANALYZE-BASELINE-EXIT: 0
- SKIP_CORECOMPILE_LINES: 0
- QF_TEST_CSC_OUT_LINES: 2
- UCS_TEST_CSC_OUT_LINES: 2
- ZERO_ERRORS_LINES: 1
- WARNINGS: 0 (ANALYZE-BASELINE-WARNINGS: 0; the first `N Warning(s)` match in the log, as CMD-REBUILD defines it)
- ERRORS: 0
- WRITESET_DIAGNOSTIC_LINES: 0

Environment provisioning before the recorded run (git-ignored packages directory only; no tracked file changed):
- First attempt of the same command (log preserved as coverage\logs\p0-t7.attempt1.msbuild.log) exited 1 with 4 errors, all `CSC : error CS0006: Metadata file ... could not be found` for `..\packages\Meziantou.Analyzer.3.0.235\analyzers\dotnet\roslyn5.0\cs\Meziantou.Analyzer.dll` (VBFunctions, UtilitiesCS) and `..\packages\MSTest.Analyzers.4.4.0\analyzers\dotnet\cs\MSTest.Analyzers.dll` / `MSTest.Analyzers.CodeFixes.dll` (SVGControl.Test).
- Cause: pre-existing repository skew. The first-party csproj `<Analyzer Include>` entries name Meziantou.Analyzer 3.0.235 and MSTest.Analyzers 4.4.0, while every packages.config and the csproj restore-check Import name 3.0.290 and 4.4.1, so a fresh worktree restore (P0-T5) installs only the newer folders. No Write Set file and no file this item edits is involved.
- Remedy: `nuget.exe install Meziantou.Analyzer -Version 3.0.235 -OutputDirectory packages -NonInteractive` and `nuget.exe install MSTest.Analyzers -Version 4.4.0 -OutputDirectory packages -NonInteractive` (both exit 0, served from the local NuGet cache). `git status --porcelain -- packages "*.csproj" "*/packages.config"` was empty afterwards.
- The recorded run above is the second invocation, after provisioning.

Acceptance: EXIT_CODE 0; SKIP_CORECOMPILE_LINES 0; both test-project compiler echoes at least 1; ERRORS 0; WRITESET_DIAGNOSTIC_LINES 0. All five hold.
