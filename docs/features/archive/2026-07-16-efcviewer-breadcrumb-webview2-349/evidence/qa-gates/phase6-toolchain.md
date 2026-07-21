# Phase 6 — Toolchain Gate (P6-T4)

Timestamp: 2026-07-18T11-20

Loop note: the first format pass reformatted the Phase 6 touched files, restarting the loop; the
pass below is the clean single pass (`csharpier check` EXIT 0 confirms idempotence).

## Step 1 — Format
Command: & "$env:USERPROFILE\.dotnet\tools\csharpier.exe" format . ; verification: csharpier check .
EXIT_CODE: 0 (format), 0 (check)
Output Summary: Repository format-clean (1384 files).

## Step 2 — Analyzers
Command: msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
EXIT_CODE: 0
Output Summary: Build succeeded; 0 errors, 0 warnings. Exempt wiring (WebView2BreadcrumbHost, Designer swap, EfcFormController rewire) compiles.

## Step 3 — Nullable / TreatWarningsAsErrors
Command: msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true
EXIT_CODE: 0
Output Summary: Build succeeded; 0 errors, 0 warnings.

## Step 4 — Tests with coverage
Command: vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /Settings:<Cobertura coverage runsettings (same as P0-T5)>
EXIT_CODE: 0
Output Summary: Total tests: 4913; Passed: 4913; Failed: 0 — no host-neutral test regressed. (EfcHomeControllerExecuteMovesTests helper was compile-fixed for the FolderListBox field retype: the removed `_selectedNode` injection is replaced by a router-backed selection over mocked seams; all 7 of its tests pass.)

All four steps green in a single pass after the format restart.
