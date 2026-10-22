# Issue #481 — Fail-before regression run against the unguarded unwire bodies

Timestamp: 2026-08-26T10-40
Task: [P5-T8] `[expect-fail]`

Decision D8 applies: this run is the evidence that the guards added by `[P5-T9]` are load-bearing. The
unwire bodies delivered by `[P5-T6]` and `[P5-T7]` are exact mirrors of the wiring methods and carry no
guard, so a teardown that runs on a partially-constructed controller now throws where it previously did
not.

## Step 1 — Build the test project (not a gate; decision D2)

Command (under `pwsh -NoProfile`, `[P0-T4]`-resolved MSBuild):

```
MSBuild.exe QuickFiler.Test\QuickFiler.Test.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU
```

EXIT_CODE: 0

## Step 2 — Run the two detachment tests plus the three teardown-robustness tests

Command (under `pwsh -NoProfile`, `[P0-T4]`-resolved `vstest.console.exe`):

```
vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~UnwireIntentEvents_DetachesAllSixteenIntentSubscriptions|FullyQualifiedName~UnwireControlTreeEvents_WithHeadlessItemViewer_DetachesKeyboardAndMouseHandlers|FullyQualifiedName~Cleanup_WithNullKeyboardHandlerAndNonItemViewerViewer_DoesNotThrow|FullyQualifiedName~Cleanup_NullsTrackedPrivateFields|FullyQualifiedName~Cleanup_ResetsInjectedHostForPooledViewerReuse" "/Logger:trx;LogFileName=481-unguarded-fail.trx" /ResultsDirectory:docs\features\active\qfc-item-controller-defects-484\evidence\regression-testing\481-unguarded-fail
```

EXIT_CODE: 1
ExpectedExitCode: 1

## Results (read from the TRX `UnitTestResult` elements)

| Test | Outcome | Reason |
|---|---|---|
| `UnwireIntentEvents_DetachesAllSixteenIntentSubscriptions` | **Passed** | The `[P5-T6]` body now detaches all sixteen subscriptions. |
| `UnwireControlTreeEvents_WithHeadlessItemViewer_DetachesKeyboardAndMouseHandlers` | **Passed** | The `[P5-T7]` body now detaches the keyboard and mouse handlers. |
| `Cleanup_WithNullKeyboardHandlerAndNonItemViewerViewer_DoesNotThrow` | **Failed** | `Did not expect any exception, but found System.InvalidCastException: Unable to cast object of type 'Castle.Proxies.IItemViewerProxy' to type 'QuickFiler.ItemViewer'` thrown from `QfcItemController.UnwireControlTreeEvents()`. |
| `Cleanup_NullsTrackedPrivateFields` | **Failed** | `System.InvalidCastException: Unable to cast object of type 'Castle.Proxies.IItemViewerProxy' to type 'QuickFiler.ItemViewer'` — the same unguarded concrete cast. |
| `Cleanup_ResetsInjectedHostForPooledViewerReuse` | **Failed** | `System.NullReferenceException` — the unguarded walk dereferences collaborators the pooled-reuse fixture leaves null. |

```
Total tests: 5
Test Run Failed.
 Total time: 2.1047 Seconds
```

TRX artifact: `docs/features/active/qfc-item-controller-defects-484/evidence/regression-testing/481-unguarded-fail/481-unguarded-fail.trx`.

Output Summary: EXIT_CODE 1 as expected. The two detachment tests are `Passed` and all three
teardown-robustness tests are `Failed`, exactly the distribution `[P5-T8]` specifies. This is the
non-vacuity evidence for the `[P5-T9]` guards: without them, teardown of a partially-constructed
controller throws `InvalidCastException` or `NullReferenceException`.
