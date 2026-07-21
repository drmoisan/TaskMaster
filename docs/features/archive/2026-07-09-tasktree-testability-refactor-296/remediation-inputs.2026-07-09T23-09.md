# Remediation Inputs — tasktree-testability-refactor (#296)

- Timestamp: 2026-07-09T23-09
- Branch: `feature/tasktree-testability-refactor-296` @ `b320336a`
- Source artifacts: `policy-audit.2026-07-09T23-09.md`, `code-review.2026-07-09T23-09.md`, `feature-audit.2026-07-09T23-09.md`
- Overall verdict: NOT READY TO MERGE — 3 Blocking findings. The child→integration PR runs zero required CI checks, so this local audit is the gate.

## Blocking Findings (must remediate with seams, not attributes)

### B1 — E4 `TaskTreeController.ActivateOlItem(dynamic item)`
- File/location: `TaskTree/TaskTreeController.cs` L64-80.
- Violated rule: general-unit-test.md Coverage Exclusion Policy ("the correct response to untestable lines is to refactor it"; testable seams are never exempt) + CLAUDE.md UT2 COM exemption qualifier "without an injectable seam" (a seam exists here).
- Why it is a testable seam, not irreducible COM: the Outlook `Explorer` is obtained via `_globals.Ol.App.ActiveExplorer()`, where `_globals` is the already-mockable `IApplicationGlobals`. The only obstacle is the `dynamic item` parameter, which forces the whole call to late-bind and throws `RuntimeBinderException` against a Moq proxy. spec.md L351 planned to cover this by mocking `Explorer.IsItemSelectableInView`.
- Required seam-based fix: replace `dynamic item` with `object item` and dispatch `Display()` by explicit type (`if (item is Outlook.MailItem m) m.Display(); else if (item is Outlook.TaskItem t) t.Display();`) — the caller's covered `IsValidType` gate already guarantees Mail/Task only; OR introduce a narrow `IExplorerItemActivator.Activate(object)` interface seam whose single host-bound impl is the minimal exempt wrapper. Then add tests covering selectable→Clear/AddToSelection and not-selectable→Display against a mocked `Explorer`, and remove the `[ExcludeFromCodeCoverage]`.

### B2 — E5 `TaskTreeController.ActivateOlItemAsync(dynamic item)`
- File/location: `TaskTree/TaskTreeController.cs` L84-104.
- Violated rule: same as B1.
- Why testable: identical seam analysis; `await Task.Run(...)` does not affect mockability of the Explorer.
- Required seam-based fix: same as B1 (typed dispatch or `IExplorerItemActivator`), plus a deterministic awaited test of both branches.

### B3 — E6 `TaskTreeController.HandleModelDropped`
- File/location: `TaskTree/TaskTreeController.MoveLogic.cs` L77-139.
- Violated rule: general-unit-test.md Coverage Exclusion Policy (exempting testable dispatch logic).
- Why partly testable: only the terminal `e.RefreshObjects()` and the adapter construction from live `e.ListView`/`e.SourceListView` are irreducible. The `switch (e.DropTargetLocation)` routing to `MoveObjectsToSibling/Roots/Children` (with offsets 0/1) is pure enum dispatch over the mockable `ITreeVisual` seam. spec.md L340 planned to cover it with `ITreeVisual` mocks; `ModelDropEventArgs` is already test-constructible via the `DropArgs` reflection helper.
- Required seam-based fix: extract the switch into a covered host-neutral method, e.g. `RouteDrop(DropTargetLocation location, ITreeVisual target, ITreeVisual source, TreeNode<ToDoItem> targetModel, IList sources)`, and keep `[ExcludeFromCodeCoverage]` only on the residual thin wrapper (adapter build + `e.RefreshObjects()` + `_viewer.SetModelFilter`/`SortTree`, though the latter two are themselves mockable). Add tests verifying each `DropTargetLocation` routes to the correct `MoveObjects*` call with the correct offset. This mirrors the E3/`ResolveRowStyle` pattern the executor already applied correctly.

## Consequential (resolved by the fixes above)

### B4 — Uncovered non-exempt caller branches
- File/location: `TaskTree/TaskTreeController.cs` `TreeLvActivateItem` L165-180 and `TreeLvActivateItemAsync` L182-197.
- The valid-type branch (`ActivateOlItem(objItem)` / `await ActivateOlItemAsync(objItem)`) is currently uncovered because the callee throws against a mock. Fixing B1/B2 makes these branches verifiable (Moq `Verify` on the activator seam). Add positive-path tests.

## Secondary (non-blocking) findings

- S1 — Canonical Cobertura artifact `artifacts/csharp/coverage.xml` is not committed to the branch; the 94.04% figure is not independently recomputable in-worktree. Commit the artifact under the canonical evidence path, or record the numbers in a committed evidence file with a checksum.
- S2 — Branch coverage (general-unit-test.md >= 75%) is not reported anywhere. Record it; re-measure after B1-B4 remediation, since removing the E4/E5/E6 exclusions changes the denominator.

## Exit Condition

Remediation complete when: E4/E5/E6 `[ExcludeFromCodeCoverage]` attributes are removed from the testable seams (replaced by extracted covered methods and/or narrow interface seams with minimal exempt wrappers), the corresponding branches and the B4 caller branches are covered by MSTest tests, the full C# toolchain re-runs green in a single pass, and re-measured TaskTree.dll line coverage remains >= 80% (new files >= 90%) and branch coverage >= 75% WITHOUT relying on the removed exclusions. Re-review before merge to integration.
