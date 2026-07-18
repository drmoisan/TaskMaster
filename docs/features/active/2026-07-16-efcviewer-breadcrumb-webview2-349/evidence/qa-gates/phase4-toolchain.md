# Phase 4 — Toolchain Gate (P4-T4)

Timestamp: 2026-07-18T10-10

Loop note: the first format pass reformatted the new renderer file, restarting the loop; the pass
below is the clean single pass (`csharpier check` EXIT 0 confirms idempotence).

## Step 1 — Format
Command: & "$env:USERPROFILE\.dotnet\tools\csharpier.exe" format . ; verification: csharpier check .
EXIT_CODE: 0 (format), 0 (check)
Output Summary: Repository format-clean (1379 files).

## Step 2 — Analyzers
Command: msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true
EXIT_CODE: 0
Output Summary: Build succeeded; 0 errors, 0 warnings.

## Step 3 — Nullable / TreatWarningsAsErrors
Command: msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true
EXIT_CODE: 0
Output Summary: Build succeeded; 0 errors, 0 warnings.

## Step 4 — Tests with coverage
Command: vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /EnableCodeCoverage /Settings:<Cobertura coverage runsettings (same as P0-T5)>
EXIT_CODE: 0
Output Summary: Total tests: 4898; Passed: 4898; Failed: 0 (adds 12 renderer tests over Phase 3).

All four steps green in a single pass after the format restart.
