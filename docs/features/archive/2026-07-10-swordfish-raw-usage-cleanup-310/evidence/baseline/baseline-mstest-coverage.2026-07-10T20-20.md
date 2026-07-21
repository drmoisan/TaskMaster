# Baseline — MSTest Run with Coverage

Timestamp: 2026-07-10T23-25
Command (as executed): `dotnet-coverage collect --output docs/features/active/2026-07-10-swordfish-raw-usage-cleanup-310/evidence/baseline/baseline-coverage-repository.xml --output-format cobertura --settings coverage.config -- "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe" QuickFiler.Test/bin/Debug/QuickFiler.Test.dll UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation /ResultsDirectory:docs/features/active/2026-07-10-swordfish-raw-usage-cleanup-310/evidence/baseline/coverage-results-baseline`
EXIT_CODE: 0
Output Summary:
- Total tests: 4758. Passed: 4758. Failed: 0. Total time: 46.40 seconds.
- Repo-wide (deduped Cobertura packages, all instrumented modules): line-rate 0.7713545978866421
  (77.14%), branch-rate 0.5261691301187303 (52.62%); lines-covered 109935 / lines-valid 142522.
- `QuickFiler` (production package): line-rate 0.7255711533168181 (72.56%), branch-rate
  0.6314285714285715 (63.14%).
- `UtilitiesCS` (production package): line-rate 0.8824581005586593 (88.25%), branch-rate
  0.8186980328533766 (81.87%).
- `QuickFiler.Controllers.KbdActions<TKey, UClass, VDelegate>` class: line-rate 0.9397590361445783
  (93.98%), branch-rate 1 (100%).
- `UtilitiesCS.TraceUtility` class: line-rate 0.900709219858156 (90.07%), branch-rate
  0.8076923076923077 (80.77%).
- `UtilitiesCS.FlagDetails` class: line-rate 1 (100%), branch-rate 0.9583333333333334 (95.83%).
- `UtilitiesCS.EmailIntelligence.FolderRemap.FolderRemapController` class: line-rate 0.875
  (87.5%), branch-rate 0.7380952380952381 (73.81%).
- `QuickFiler.Controllers.KeyboardHandler` class: not present as a distinct `<class>` entry in
  the Cobertura output (no measurable executable-line data was captured for this class by the
  instrumentation); its using-directive removal is verified by rebuild success, not by a
  coverage delta.

## Deviation from plan-literal command text

The plan task text specifies `vstest.console.exe <assemblies> /EnableCodeCoverage`. Two attempts
using that literal flag combination (`dotnet-coverage collect -- vstest.console.exe ...
/EnableCodeCoverage`, with and without `/InIsolation`) both produced
`No code coverage data available. Profiler was not initialized.` and an empty/near-empty
Cobertura output (see prior discarded attempts). The repository's own canonical coverage script
(`scripts/vscode/Invoke-MSTestWithCoverage.ps1`) uses a proven, different recipe: `dotnet-coverage
collect --settings coverage.config --output-format cobertura -- vstest.console.exe <assemblies>
/Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation` (no `/EnableCodeCoverage` flag;
instrumentation is driven by the outer `dotnet-coverage --settings coverage.config`, not vstest's
own Code Coverage data collector). This baseline uses that proven recipe, scoped to the two
plan-mandated assemblies (`QuickFiler.Test.dll`, `UtilitiesCS.Test.dll`). This is a mechanically
necessary toolchain-command correction to obtain the numeric coverage data required by the
Coverage Evidence Contract; it does not change scope, test content, or production code. The same
corrected recipe will be reused verbatim in Phase 6 (P6-T4) for methodology consistency.
