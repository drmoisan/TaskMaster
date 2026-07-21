# C# Test + Coverage Baseline (Issue #283)

Timestamp: 2026-07-08T17-56
Command: `dotnet-coverage collect --output <scratch>.cobertura.xml --output-format cobertura --settings coverage.config -- <vstest.console.exe> TaskMaster.Test/bin/Debug/TaskMaster.Test.dll /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:TestCategory!=LiveOutlook`
EXIT_CODE: 0

Output Summary:
- Test result: Total tests: 223, Passed: 223, Failed: 0. The developer-only `[TestCategory("LiveOutlook")]` harness is excluded via `/TestCaseFilter:TestCategory!=LiveOutlook` per the environment mandate (it constructs a live Outlook and is not runnable here).
- Scope: coverage measured for the `TaskMaster.Test.dll` assembly run against the whole instrumented production set (coverage.config instrumentation excludes vendored assemblies). Root Cobertura headline: `line-rate=0.16755` = 16.75% overall (lines-covered 11638 / lines-valid 69461); branch-rate 11.80%. This is a single-test-assembly figure (TaskMaster.Test only), not a full-suite repo number; the same scope + filter is reused at final QC (P2-T4) so the baseline-vs-final delta is apples-to-apples.
- Seam file `LiveOutlookHarnessRunner.cs` does not exist at baseline (created in P1-T1); its baseline coverage is N/A. New-code coverage target (>= 90%) is verified at final QC.
- Runsettings `TaskMaster.cli.runsettings` (MSTest parallelization) applied; run completed without the full-suite parallelism timeout.
