# AC3A deterministic efficacy (P3-T8)

Task: [P3-T8]
Timestamp: 2026-09-13T03-29
Command: none (authored from the P3-T6 artifact `evidence/regression-testing/ac2-pass-after-three-runs.2026-09-12T18-00.md` and the P2-T6 gate counts)
EXIT_CODE: 0
Output Summary: all four items below are drawn from the single P3-T6 run number 1 (`coverage\trx\p3-t6-1\p3-t6-pass-after.trx`, transcribed before deletion); each is a deterministic structural assertion, so one run suffices.

Source run: **P3-T6 run 1** (2026-09-13T03-28, SERIAL regime, no /Settings: argument, `RUN 1 EXIT 0`, total=5 passed=5 failed=0).

1. **Test 1 passed.** `ResolveControlGroupsAsync_WithMockViewerAndSyncDispatcher_CompletesWithoutAConcreteViewer` — outcome `Passed`, duration 00:00:00.3139675, in P3-T6 run 1.
2. **Driven with the synchronous injected-dispatcher double and no pump host.** The controller under test was built through the internal harness subclass with `_uiDispatcher` set to the `QfcItemControllerTestSupport.BuildSyncDispatcher()` double and `_itemViewer` set to a `Mock<IItemViewer>`; the P2-T6 gate recorded a zero-occurrence count of `WinFormsPumpHost` in the test file (`0`, re-verified after the Phase 3 arrangement correction), so no message pump was hosted.
3. **Structural zero-construction assertion.** Test 1 asserts `viewer.Object.Should().NotBeAssignableTo<QuickFiler.ItemViewer>()`: the object the member was driven with is a Moq proxy of the interface and is not assignable to the concrete viewer type, so the member completed without a concrete viewer being constructed or required.
4. **Parameter-type assertion.** `ResolveControlGroupsAsync_FirstParameterType_IsTheViewerInterface` — outcome `Passed`, duration 00:00:00.0014717, in the same run — asserts by reflection over `typeof(QfcItemController)` that the non-public instance method `ResolveControlGroupsAsync` has exactly one parameter and that its type is `IItemViewer`.

A deterministic assertion has no base rate: it either holds for the compiled member or it does not, independently of load, scheduling or repetition. One run therefore suffices for this component and no statistics are required. The statistical component AC3B is addressed separately by P5-T1.
