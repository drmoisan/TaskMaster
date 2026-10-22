# AC2 fail-before: three consecutive runs on the defect-preserving intermediate (P2-T9) [expect-fail]

Task: [P2-T9]
Timestamp: 2026-09-13T03-04
Command: `pwsh -Command 'for ($i = 1; $i -le 3; $i++) { & $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation "/Logger:trx;LogFileName=p2-t9-fail-before.trx" /ResultsDirectory:coverage\trx\p2-t9-$i "/TestCaseFilter:FullyQualifiedName~QfcItemController_SeamMarshallingTests"; Write-Output ("RUN " + $i + " EXIT " + $LASTEXITCODE) }'` Run from the item worktree root via Set-Location inside one pwsh invocation with the Command Reference tool resolution prepended; each run's console output redirected to the ignored path `coverage\p2-t9-run-<i>.log`. The whole loop ran while holding the shared machine build lock for item 743 (one acquisition, released after the loop returned). Outlook was closed.
EXIT_CODE: 1
ExpectedExitCode: 1
Output Summary: `RUN 1 EXIT 1` / `RUN 2 EXIT 1` / `RUN 3 EXIT 1`. Every run: total=5, passed=2, failed=3. Failing set in every run = tests 1, 2 and 5 of P2-T6; passing set = tests 3 and 4. Tests 1 and 2 fail with `InvalidCastException`; test 5 fails with `NullReferenceException`. The tree under test is the P2-T3 intermediate (widened signature, two `(ItemViewer)itemViewer` casts retained, the `_itemViewer.UiDispatcher.InvokeAsync` marshal not yet converted).

REGIME: SERIAL (no /Settings: argument).

## Run 1 (newest `.trx` under `coverage\trx\p2-t9-1`, sorted by LastWriteTime: `p2-t9-fail-before.trx`)

- total=5 executed=5 passed=2 failed=3 (TRX ResultSummary outcome=Failed)
- Failed `ResolveControlGroupsAsync_WithMockViewerAndSyncDispatcher_CompletesWithoutAConcreteViewer` — `System.InvalidCastException: Unable to cast object of type 'Castle.Proxies.IItemViewerProxy' to type 'QuickFiler.ItemViewer'.`
- Failed `ResolveControlGroupsAsync_WithMockViewer_PopulatesTipsAndControlGroups` — `System.InvalidCastException: Unable to cast object of type 'Castle.Proxies.IItemViewerProxy' to type 'QuickFiler.ItemViewer'.`
- Passed `ResolveControlGroupsAsync_FirstParameterType_IsTheViewerInterface`
- Passed `ResolveControlGroupsAsync_WithCancelledToken_ThrowsOperationCanceled`
- Failed `AssignControlsAsync_WithSyncDispatcherDouble_AssignsThroughTheInjectedSeam` — `System.NullReferenceException: Object reference not set to an instance of an object.`
- Console: `Total tests: 5` / `Passed: 2` / `Failed: 3` / `Total time: 2.0306 Seconds`

## Run 2 (newest `.trx` under `coverage\trx\p2-t9-2`: `p2-t9-fail-before.trx`)

- total=5 executed=5 passed=2 failed=3 (outcome=Failed)
- Failed `ResolveControlGroupsAsync_WithMockViewerAndSyncDispatcher_CompletesWithoutAConcreteViewer` — `System.InvalidCastException: Unable to cast object of type 'Castle.Proxies.IItemViewerProxy' to type 'QuickFiler.ItemViewer'.`
- Failed `ResolveControlGroupsAsync_WithMockViewer_PopulatesTipsAndControlGroups` — `System.InvalidCastException: Unable to cast object of type 'Castle.Proxies.IItemViewerProxy' to type 'QuickFiler.ItemViewer'.`
- Passed `ResolveControlGroupsAsync_FirstParameterType_IsTheViewerInterface`
- Passed `ResolveControlGroupsAsync_WithCancelledToken_ThrowsOperationCanceled`
- Failed `AssignControlsAsync_WithSyncDispatcherDouble_AssignsThroughTheInjectedSeam` — `System.NullReferenceException: Object reference not set to an instance of an object.`

## Run 3 (newest `.trx` under `coverage\trx\p2-t9-3`: `p2-t9-fail-before.trx`)

- total=5 executed=5 passed=2 failed=3 (outcome=Failed)
- Failed `ResolveControlGroupsAsync_WithMockViewerAndSyncDispatcher_CompletesWithoutAConcreteViewer` — `System.InvalidCastException: Unable to cast object of type 'Castle.Proxies.IItemViewerProxy' to type 'QuickFiler.ItemViewer'.`
- Failed `ResolveControlGroupsAsync_WithMockViewer_PopulatesTipsAndControlGroups` — `System.InvalidCastException: Unable to cast object of type 'Castle.Proxies.IItemViewerProxy' to type 'QuickFiler.ItemViewer'.`
- Passed `ResolveControlGroupsAsync_FirstParameterType_IsTheViewerInterface`
- Passed `ResolveControlGroupsAsync_WithCancelledToken_ThrowsOperationCanceled`
- Failed `AssignControlsAsync_WithSyncDispatcherDouble_AssignsThroughTheInjectedSeam` — `System.NullReferenceException: Object reference not set to an instance of an object.`

## Mechanism of each failure (D7)

- Tests 1 and 2: the intermediate reads `((ItemViewer)itemViewer).LblItemNumber` before any await; the Moq proxy for `IItemViewer` does not derive from the concrete viewer, so the cast throws deterministically with no timing dependency. This is the pre-change defect ("the member cannot be driven without a concrete viewer") made observable.
- Test 5: the not-yet-converted marshal reads `_itemViewer.UiDispatcher`, and Moq returns null for the sealed `System.Windows.Threading.Dispatcher`, so `.InvokeAsync` dereferences null.

## Results directories deleted

`coverage\trx\p2-t9-1`, `coverage\trx\p2-t9-2` and `coverage\trx\p2-t9-3` were deleted after transcription. `pwsh -Command 'Get-ChildItem coverage\trx -Filter "p2-t9-*" -Directory | Measure-Object | Select-Object -ExpandProperty Count'` printed `0`. No raw `.trx` was written outside the ignored `coverage` directory (D1).
