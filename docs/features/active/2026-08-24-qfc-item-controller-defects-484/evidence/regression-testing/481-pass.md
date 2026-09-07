# Issue #481 — Pass-after regression run (event unwiring path)

Timestamp: 2026-08-26T10-52
Task: [P5-T11]

## Step 1 — Build the test project (not a gate; decision D2)

Command (under `pwsh -NoProfile`, `[P0-T4]`-resolved MSBuild):

```
MSBuild.exe QuickFiler.Test\QuickFiler.Test.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU
```

EXIT_CODE: 0

## Step 2 — Run the same five tests as `[P5-T8]`, after the `[P5-T9]` guards and `[P5-T10]` detach

Command (under `pwsh -NoProfile`, `[P0-T4]`-resolved `vstest.console.exe`):

```
vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~UnwireIntentEvents_DetachesAllSixteenIntentSubscriptions|FullyQualifiedName~UnwireControlTreeEvents_WithHeadlessItemViewer_DetachesKeyboardAndMouseHandlers|FullyQualifiedName~Cleanup_WithNullKeyboardHandlerAndNonItemViewerViewer_DoesNotThrow|FullyQualifiedName~Cleanup_NullsTrackedPrivateFields|FullyQualifiedName~Cleanup_ResetsInjectedHostForPooledViewerReuse" "/Logger:trx;LogFileName=481-pass.trx" /ResultsDirectory:docs\features\active\qfc-item-controller-defects-484\evidence\regression-testing\481-pass
```

EXIT_CODE: 0

## Results

| Test | Outcome |
|---|---|
| `UnwireIntentEvents_DetachesAllSixteenIntentSubscriptions` | **Passed** |
| `UnwireControlTreeEvents_WithHeadlessItemViewer_DetachesKeyboardAndMouseHandlers` | **Passed** |
| `Cleanup_WithNullKeyboardHandlerAndNonItemViewerViewer_DoesNotThrow` | **Passed** |
| `Cleanup_NullsTrackedPrivateFields` | **Passed** |
| `Cleanup_ResetsInjectedHostForPooledViewerReuse` | **Passed** |

```
Test Run Successful.
Total tests: 5
 Total time: 2.2954 Seconds
```

Failed count: **0**. Skipped count: **0**.

## Assertion inventory carried by these results

- `UnwireIntentEvents_DetachesAllSixteenIntentSubscriptions` carries **sixteen** `VerifyRemove`
  assertions with `Times.Once()`, one per subscription made by `WireIntentEvents()`:
  `ConversationModeChanged`, `FlagTaskClicked`, `PopOutClicked`, `DeleteItemClicked`, `ReplyClicked`,
  `ReplyAllClicked`, `ForwardClicked`, `BodyDoubleClick`, `SearchTextChanged`, `FolderKeyDown`,
  `FolderSelectionChanged`, `WebViewInitializationCompleted`, `ConversationItemSelectionChanged`,
  `SearchKeyDown`, `EmailCopyChanged`, `AttachmentsChanged`.
- `UnwireControlTreeEvents_WithHeadlessItemViewer_DetachesKeyboardAndMouseHandlers` carries two
  `Times.Never()` assertions on the keyboard-handler mock (`KeyboardHandler_PreviewKeyDownAsync` and
  `KeyboardHandler_KeyDownAsync`) plus an unchanged-button-background-colour assertion, after raising
  `OnPreviewKeyDown`, `OnKeyDown`, and `OnMouseEnter` by reflection on a real headless `ItemViewer`.

TRX artifact: `docs/features/active/qfc-item-controller-defects-484/evidence/regression-testing/481-pass/481-pass.trx`.

Output Summary: EXIT_CODE 0, 5 of 5 Passed, 0 Failed. The two detachment tests that failed at
`[P5-T5]` and the three teardown-robustness tests that failed at `[P5-T8]` are all green.
