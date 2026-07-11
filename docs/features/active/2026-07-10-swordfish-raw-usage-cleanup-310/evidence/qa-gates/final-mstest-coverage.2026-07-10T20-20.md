# Phase 6 — Final MSTest Run with Coverage

Timestamp: 2026-07-10T23-37
Command (identical assembly set and recipe to P0-T5): `dotnet-coverage collect --output docs/features/active/2026-07-10-swordfish-raw-usage-cleanup-310/evidence/qa-gates/final-coverage-repository.xml --output-format cobertura --settings coverage.config -- "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe" QuickFiler.Test/bin/Debug/QuickFiler.Test.dll UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation /ResultsDirectory:docs/features/active/2026-07-10-swordfish-raw-usage-cleanup-310/evidence/qa-gates/coverage-results-final`
EXIT_CODE: 0
Output Summary:
- Total tests: 4758. Passed: 4758. Failed: 0. Total time: 46.44 seconds. Both `QuickFiler.Test.dll`
  and `UtilitiesCS.Test.dll` were freshly rebuilt against the changed production DLLs during the
  Phase 1-3 `msbuild` passes (confirmed via build-log project-graph entries), so this run
  exercises the post-change code.
- Repo-wide: line-rate 0.771175975301712 (77.12%), branch-rate 0.526047976738551 (52.60%);
  lines-covered 109908 / lines-valid 142520.
- `QuickFiler` (production package): line-rate 0.7254335260115607 (72.54%), branch-rate
  0.6314285714285715 (63.14%).
- `UtilitiesCS` (production package): line-rate 0.882551585429444 (88.26%), branch-rate
  0.818900831474346 (81.89%).
- `QuickFiler.Controllers.KbdActions<TKey, UClass, VDelegate>` class: line-rate
  0.9397590361445783 (93.98%), branch-rate 1 (100%) — identical to baseline.
- `UtilitiesCS.TraceUtility` class: line-rate 0.9 (90.00%), branch-rate 0.8076923076923077
  (80.77%).
- `UtilitiesCS.FlagDetails` class: line-rate 1 (100%), branch-rate 0.9583333333333334 (95.83%) —
  identical to baseline.
- `UtilitiesCS.EmailIntelligence.FolderRemap.FolderRemapController` class: line-rate 0.875
  (87.5%), branch-rate 0.7380952380952381 (73.81%) — identical to baseline.
- `QuickFiler.Controllers.KeyboardHandler` class: still not present as a distinct `<class>` entry
  in the Cobertura output, consistent with the baseline.

Same recipe deviation noted in `baseline-mstest-coverage.2026-07-10T20-20.md` applies here for
methodology consistency (reused verbatim).
