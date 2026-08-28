# Issue #481 — Fail-before regression run against the empty unwire bodies

Timestamp: 2026-08-26T10-31
Task: [P5-T5] `[expect-fail]`

Decision D7 applies: `UnwireEvents()`, `UnwireControlTreeEvents()`, and `UnwireIntentEvents()` had to
exist before the three regression tests could compile, so `[P5-T4]` introduced them in a
defect-preserving form — empty bodies that change no behaviour — and this run observes the two
detachment tests failing in that state.

## Step 1 — Build the test project (not a gate; decision D2)

Command (under `pwsh -NoProfile`, `[P0-T4]`-resolved MSBuild):

```
MSBuild.exe QuickFiler.Test\QuickFiler.Test.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU
```

EXIT_CODE: 0

## Step 2 — Run the three new #481 regression tests

Command (under `pwsh -NoProfile`, `[P0-T4]`-resolved `vstest.console.exe`):

```
vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~UnwireIntentEvents_DetachesAllSixteenIntentSubscriptions|FullyQualifiedName~UnwireControlTreeEvents_WithHeadlessItemViewer_DetachesKeyboardAndMouseHandlers|FullyQualifiedName~Cleanup_WithNullKeyboardHandlerAndNonItemViewerViewer_DoesNotThrow" "/Logger:trx;LogFileName=481-empty-bodies-fail.trx" /ResultsDirectory:docs\features\active\qfc-item-controller-defects-484\evidence\regression-testing\481-empty-bodies-fail
```

EXIT_CODE: 1
ExpectedExitCode: 1

## Results (read from the TRX `UnitTestResult` elements)

| Test | Outcome | Reason |
|---|---|---|
| `UnwireIntentEvents_DetachesAllSixteenIntentSubscriptions` | **Failed** | `Moq.MockException: Expected invocation on the mock once, but was 0 times: v => v.ConversationModeChanged -= It.IsAny<EventHandler>()` — the empty `UnwireIntentEvents()` body detaches nothing. |
| `UnwireControlTreeEvents_WithHeadlessItemViewer_DetachesKeyboardAndMouseHandlers` | **Failed** | `System.Reflection.TargetInvocationException ---> System.NullReferenceException` — the empty `UnwireControlTreeEvents()` body leaves the mouse-enter handler attached, so raising `OnMouseEnter` on the button still executes `Button_MouseEnter` on the torn-down controller. |
| `Cleanup_WithNullKeyboardHandlerAndNonItemViewerViewer_DoesNotThrow` | **Passed** | The empty bodies cannot throw, so teardown robustness holds trivially at this point. This test becomes load-bearing at `[P5-T8]`, once the bodies are implemented unguarded. |

```
Total tests: 3
Test Run Failed.
 Total time: 3.1266 Seconds
```

TRX artifact: `docs/features/active/qfc-item-controller-defects-484/evidence/regression-testing/481-empty-bodies-fail/481-empty-bodies-fail.trx`.

Output Summary: EXIT_CODE 1 as expected. The two detachment tests are `Failed` against the empty
bodies and `Cleanup_WithNullKeyboardHandlerAndNonItemViewerViewer_DoesNotThrow` is `Passed`, exactly the
distribution `[P5-T5]` specifies.
