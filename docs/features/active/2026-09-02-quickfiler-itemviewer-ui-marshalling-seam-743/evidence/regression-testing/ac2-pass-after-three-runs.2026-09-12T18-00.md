# AC2 pass-after: three consecutive runs on the post-fix tree (P3-T6)

Task: [P3-T6]
Timestamp: 2026-09-13T03-28
Command: `pwsh -Command 'for ($i = 1; $i -le 3; $i++) { & $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation "/Logger:trx;LogFileName=p3-t6-pass-after.trx" /ResultsDirectory:coverage\trx\p3-t6-$i "/TestCaseFilter:FullyQualifiedName~QfcItemController_SeamMarshallingTests"; Write-Output ("RUN " + $i + " EXIT " + $LASTEXITCODE) }'` Run from the item worktree root via Set-Location inside one pwsh invocation with the Command Reference tool resolution prepended; each run's console output redirected to the ignored path `coverage\p3-t6-run-<i>.log`. The whole loop ran while holding the shared machine build lock for item 743 (one acquisition, released after the loop returned). Outlook was closed.
EXIT_CODE: 0
Output Summary: `RUN 1 EXIT 0` / `RUN 2 EXIT 0` / `RUN 3 EXIT 0`. Every run: total=5, passed=5, failed=0 (TRX ResultSummary outcome=Completed). The tree under test is the P3-T1/P3-T2/P3-T3 post-fix tree (additive interface members in use, injected-seam marshal with null tolerance), built by the P3-T5 analyzer Rebuild and one incremental `/t:Build` after the test-arrangement correction recorded below.

REGIME: SERIAL (no /Settings: argument).

## Run 1 (newest `.trx` under `coverage\trx\p3-t6-1`, sorted by LastWriteTime: `p3-t6-pass-after.trx`)

- total=5 executed=5 passed=5 failed=0 (outcome=Completed)
- Passed `ResolveControlGroupsAsync_WithMockViewerAndSyncDispatcher_CompletesWithoutAConcreteViewer` [00:00:00.3139675]
- Passed `ResolveControlGroupsAsync_WithMockViewer_PopulatesTipsAndControlGroups` [00:00:00.0102397]
- Passed `ResolveControlGroupsAsync_FirstParameterType_IsTheViewerInterface` [00:00:00.0014717]
- Passed `ResolveControlGroupsAsync_WithCancelledToken_ThrowsOperationCanceled` [00:00:00.0067090]
- Passed `AssignControlsAsync_WithSyncDispatcherDouble_AssignsThroughTheInjectedSeam` [00:00:00.0642799]
- Console: `Total tests: 5` / `Passed: 5` / `Total time: 1.5018 Seconds`

## Run 2 (newest `.trx` under `coverage\trx\p3-t6-2`: `p3-t6-pass-after.trx`)

- total=5 executed=5 passed=5 failed=0 (outcome=Completed)
- Passed `ResolveControlGroupsAsync_WithMockViewerAndSyncDispatcher_CompletesWithoutAConcreteViewer` [00:00:00.3124337]
- Passed `ResolveControlGroupsAsync_WithMockViewer_PopulatesTipsAndControlGroups` [00:00:00.0103034]
- Passed `ResolveControlGroupsAsync_FirstParameterType_IsTheViewerInterface` [00:00:00.0014899]
- Passed `ResolveControlGroupsAsync_WithCancelledToken_ThrowsOperationCanceled` [00:00:00.0067638]
- Passed `AssignControlsAsync_WithSyncDispatcherDouble_AssignsThroughTheInjectedSeam` [00:00:00.0654876]

## Run 3 (newest `.trx` under `coverage\trx\p3-t6-3`: `p3-t6-pass-after.trx`)

- total=5 executed=5 passed=5 failed=0 (outcome=Completed)
- Passed `ResolveControlGroupsAsync_WithMockViewerAndSyncDispatcher_CompletesWithoutAConcreteViewer` [00:00:00.3402185]
- Passed `ResolveControlGroupsAsync_WithMockViewer_PopulatesTipsAndControlGroups` [00:00:00.0114704]
- Passed `ResolveControlGroupsAsync_FirstParameterType_IsTheViewerInterface` [00:00:00.0018593]
- Passed `ResolveControlGroupsAsync_WithCancelledToken_ThrowsOperationCanceled` [00:00:00.0077894]
- Passed `AssignControlsAsync_WithSyncDispatcherDouble_AssignsThroughTheInjectedSeam` [00:00:00.0659865]

## Recorded deviation: a first attempt of this loop failed, and the test arrangement order was corrected

A first execution of this exact command at 2026-09-13T03-17, on the same post-fix production tree, returned `RUN 1 EXIT 1` / `RUN 2 EXIT 1` / `RUN 3 EXIT 1` with total=5, passed=3, failed=2 in every run: tests 1 and 2 (`ResolveControlGroupsAsync_WithMockViewerAndSyncDispatcher_CompletesWithoutAConcreteViewer`, `ResolveControlGroupsAsync_WithMockViewer_PopulatesTipsAndControlGroups`) each `timed out after 60000ms`; tests 3, 4 and 5 passed. Those three results directories were deleted before the re-run.

Cause, established against the tree: the seam test file as committed at the Phase 2 boundary installed the ambient `SynchronizationContext` BEFORE constructing the WinForms `Panel`, `Label`, `TableLayoutPanel` and `Button` instances. A `System.Windows.Forms.Control` constructor calls `WindowsFormsSynchronizationContext.InstallIfNeeded`, which replaces an ambient context whose exact type is `SynchronizationContext` with a `WindowsFormsSynchronizationContext`. The awaiter at UtilitiesCS/Threading/UiThread.cs lines 155-190 therefore never took its reference-equality inline branch; each continuation was posted to a WinForms context that is never pumped on the MSTest STA thread, which is the same mechanism the UtilitiesCS `QfcTipsDetails_Tests.CreateAsync_*_WithMatchingSyncContext_*` tests (lines 654-680, 696-720) avoid by constructing the controls first and installing the context afterwards.

Correction, confined to the Write Set file `QuickFiler.Test/Controllers/QfcItemController.SeamMarshallingTests.cs`: every control is now constructed before the ambient context is installed, and the class remarks record the ordering rule. This satisfies P2-T6 as written (the task fixes only "install it ... before the act"). The P2-T6 gate counts were re-verified after the rewrite: `[TestMethod]` = 5, `WinFormsPumpHost` = 0, `[Timeout(` = 5, banned constructs = 0; post-format line count 312 (at most 400). The P2-T9 fail-before evidence is unaffected in substance: its three failures were raised by the two `(ItemViewer)itemViewer` casts and the null `UiDispatcher` read, all of which execute before any `await` and are independent of the arrangement order. The fail-before ran against the earlier text of the test file, which is preserved in the Phase 2 commit (bce81049).

## Results directories deleted

`coverage\trx\p3-t6-1`, `coverage\trx\p3-t6-2` and `coverage\trx\p3-t6-3` were deleted after transcription; `Test-Path` printed `False` for each and the `p3-t6-*` directory count under `coverage\trx` is `0`. No raw `.trx` was written outside the ignored `coverage` directory (D1).

Together with the P2-T9 artifact this pair supplies the six outcomes AC2 component (i) requires, on the same machine in the same session.
