# [P5-T5] Final QC loop, step 4 — the full nine-assembly suite with coverage

Timestamp: 2026-09-08T02-50

Command:

```
dotnet-coverage collect --output coverage\809-p5-final.cobertura.xml --output-format cobertura --settings coverage\809-effective-coverage.config -- $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll SVGControl.Test\bin\Debug\SVGControl.Test.dll Tags.Test\bin\Debug\Tags.Test.dll TaskMaster.Test\bin\Debug\TaskMaster.Test.dll TaskTree.Test\bin\Debug\TaskTree.Test.dll TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll ToDoModel.Test\bin\Debug\ToDoModel.Test.dll UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll VBFunctions.Test\bin\Debug\VBFunctions.Test.dll '/Settings:scripts\vscode\TaskMaster.cli.runsettings' '/InIsolation' '/Logger:trx;LogFileName=p5t5.trx' '/ResultsDirectory:TestResults\809-p5t5' '/Blame:CollectHangDump;TestTimeout=5min;HangDumpType=None' '/TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests'
```

The derived settings file `coverage\809-effective-coverage.config` was rebuilt exactly as [P0-T13] builds it: repository-root `coverage.config` loaded, one `<ModulePath>.*\.Test\.dll$</ModulePath>` appended to `/Configuration/CodeCoverage/ModulePaths/Exclude`, saved under `coverage\`.

EXIT_CODE: 0

## Output Summary

```
Test Run Successful.
Total tests: 7137
     Passed: 7137
```

TRX selected: `p5t5.trx`, `LastWriteTimeUtc` `2026-09-08T05:05:32.6088790Z`.

TRX `ResultSummary/Counters`: `total` 7137, `executed` 7137, `passed` 7137, `failed` 0.

The `failed` attribute is 0.

SKIPPED_DERIVED: 0

## Discovered-total comparison

The baseline value is located in `docs/features/active/2026-09-07-uithread-init-contract-residuals-784-787-788-809/evidence/baseline/p0-t12-vstest.md` by its `BASELINE_TOTAL_TESTS:` token.

| Quantity | Value |
|---|---|
| `BASELINE_TOTAL_TESTS:` recorded by [P0-T12] | 7120 |
| Test methods this delivery adds | 17 |
| Expected discovered total | 7137 |
| Observed discovered total | 7137 |
| Difference | 0 |

The seventeen are four in `UiThreadInitApartmentContract_Tests` from [P2-T2], six in `UiThreadInitRetryContract_Tests` being four from [P2-T3] plus one from [P2-T4] plus one from [P2-T5], and seven in `SynchronizationContextAwaiter_Tests` from [P2-T8]. This delivery removes no test, and [P4-T1] changed the signature of one existing method without renaming it, so the total is baseline plus seventeen exactly.

## First-party coverage, pinned counting method

The selection is the all-descendant `.//line` selection over each first-party `<package>`, and only that one; the two narrower selections `classes/class/lines/line` and `classes/class/methods/method/lines/line` are rejected by name and were not substituted. A `<line>` counts as covered when `hits` is greater than zero. Branch figures are summed from the `(numerator/denominator)` pair inside each `condition-coverage` attribute over the same line set. The allowlist is the nine production assembly names.

FINAL_FIRSTPARTY_LINES_COVERED: 113481
FINAL_FIRSTPARTY_LINES_VALID: 134111
FINAL_FIRSTPARTY_BRANCHES_COVERED: 26920
FINAL_FIRSTPARTY_BRANCHES_VALID: 33912
FINAL_FIRSTPARTY_LINE_PCT: 84.62
FINAL_FIRSTPARTY_BRANCH_PCT: 79.38

Per-package breakdown, same selection:

| Package | Lines covered | Lines valid | Line % | Branches covered | Branches valid | Branch % |
|---|---|---|---|---|---|---|
| QuickFiler | 20545 | 25572 | 80.34 | 4890 | 6330 | 77.25 |
| UtilitiesCS | 79132 | 89046 | 88.87 | 18694 | 22446 | 83.28 |
| TaskVisualization | 2899 | 3230 | 89.75 | 666 | 800 | 83.25 |
| SVGControl | 1757 | 3712 | 47.33 | 600 | 1276 | 47.02 |
| ToDoModel | 2193 | 3819 | 57.42 | 496 | 1016 | 48.82 |
| Tags | 1428 | 1540 | 92.73 | 348 | 380 | 91.58 |
| TaskMaster | 4927 | 6564 | 75.06 | 1038 | 1460 | 71.10 |
| TaskTree | 592 | 620 | 95.48 | 188 | 204 | 92.16 |
| VBFunctions | 8 | 8 | 100.00 | 0 | 0 | 0.00 |

These are locally-filtered nine-assembly figures, not CI figures.

## The AC5-named test, recorded individually by name

| Fully-qualified test | Outcome | Duration |
|---|---|---|
| `UtilitiesCS.Test.Extensions.DictionaryExtensions_Tests.TryAddValuesAsync_UpdatesExistingValue` | Passed | 00:00:18.3738732 |

The AC5 clause naming that test names the full-suite run as the place it is recorded, and an aggregate pass count does not carry the name, so the row is reproduced here. Its duration of 18.4 seconds is consistent with the documented issue #780 wall-clock behaviour of this test; it passed. The three dedicated repetitions are [P5-T6] and are separate from this row.

## Collector substitution

`/EnableCodeCoverage` was not passed. `scripts/vscode/TaskMaster.cli.runsettings` carries no data collector, and `scripts/vscode/Invoke-MSTestWithCoverage.ps1:19-26` records that the omission is deliberate because the outer `dotnet-coverage` instrumentation and the built-in Code Coverage collector conflict. Coverage was collected by `dotnet-coverage collect ... -- vstest.console.exe ...` instead. [P6-T10] records this as `AC6-COLLECTOR-SUBSTITUTION`.

TOOLCHAIN_LOOP_PASS: 1
