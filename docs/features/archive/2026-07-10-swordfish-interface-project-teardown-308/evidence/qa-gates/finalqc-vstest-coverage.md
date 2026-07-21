# Final QC — Test + Coverage (P5-T4)

- **Timestamp:** 2026-07-11T13-22
- **Command:** `dotnet-coverage collect -o final.cobertura.xml -f cobertura -- vstest.console.exe <8 bin/Debug *.Test.dll> /InIsolation /Logger:trx /TestCaseFilter:TestCategory!=LiveOutlook` (CI-faithful; identical method to the P0-T6 baseline)
- **EXIT_CODE:** 1 (22 pre-existing environmental failures, identical to baseline)
- **Output Summary:**
  - **Total tests:** 5346 — **Passed:** 5324 — **Failed:** 22 — Total time 43.5s.
  - Total dropped by 12 vs the baseline 5358 (the removed test methods: `ObservableDictionary_Tests`, `ConcurrentObservableCollectionSenderTests`, `ConcurrentObservableCollectionLockRecursionTests`).
  - **Repo-wide raw line coverage:** 69.44% (`line-rate="0.6943883313784265"`, lines-covered 127873 / lines-valid 184152). Up from the baseline 68.93% because the low-coverage (7.61%) vendored `Swordfish.NET.General` package left the denominator.
  - **The 22 failures are byte-identical to the baseline set** (verified by `diff` of sorted failing-test names — empty diff). All are Deedle / email-DataFrame tests failing with `Failed loading language 'eng'` (a local NLP language-model that does not load in this environment). None touch Swordfish or any F5 target. F5 introduces ZERO new test failures. These pass in CI where the language model is present.

## Post-change per-package line-rates vs baseline

| Package | baseline | final |
|---|---|---|
| Swordfish.NET.General | 0.0761 | REMOVED |
| UtilitiesCS | 0.88321 | 0.88336 (up) |
| UtilitiesCS.Test | 0.97629 | 0.97622 |
| QuickFiler | 0.72516 | 0.72516 |
| TaskMaster | 0.67432 | 0.67432 |
| ToDoModel | 0.54906 | 0.54906 |
| Tags | 0.92632 | 0.92632 |
| TaskTree | 0.95484 | 0.95484 |
| TaskVisualization | 0.89718 | 0.89718 |
| VBFunctions | 1.0000 | 1.0000 |

No surviving first-party PRODUCTION package regressed; `UtilitiesCS` (host of the clean
`ConcurrentObservableCollection`) rose slightly (removed dead `UpdateForMove` + dead interfaces).
