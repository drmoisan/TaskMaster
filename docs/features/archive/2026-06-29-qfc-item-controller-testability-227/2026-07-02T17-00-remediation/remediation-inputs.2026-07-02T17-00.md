# Remediation Inputs — Cycle 5 (Issue #227)

**Generated:** 2026-07-02T17-00 (orchestrator, cycle entry)
**Feature Folder:** `docs/features/active/2026-06-29-qfc-item-controller-testability-227/`
**Base Branch:** `main` (`4611fd60b7d1a782a8024f54cbfd4d28f6d4c264`)
**Head:** `TaskMaster-wt-2026-06-29-09-38` (`808ea8f1` — cycle-4 delivery, committed)
**Trigger:** Maintainer directly questioned whether the ratified-pending 24-member exemption
boundary was genuinely untestable. Research
(`artifacts/research/2026-07-02T16-15-qfc-item-controller-headless-itemviewer-research.md`) found a
confirmed, no-open-risk path to reduce 5 more members. Maintainer approved cycle 5 on that basis.

## Cycle scope

### R1 — Headless `ItemViewer` construction (2 members, no new production seam)

- **Finding:** `ItemViewer`'s constructor (`QuickFiler/Viewers/ItemViewer.cs:22-29`) has no external
  dependency beyond requiring a non-null ambient `SynchronizationContext` on the calling thread (for
  `TaskScheduler.FromCurrentSynchronizationContext()`) — a precondition already proven safe in this
  exact repo for the structurally identical `ProgressPane`/`ProgressViewer` constructors, both
  already tested by constructing the real object directly after installing
  `new SynchronizationContext()` (`UtilitiesCS.Test/Threading/ProgressPane_Tests.cs:56-83`).
- **Remediation:** Remove `[ExcludeFromCodeCoverage]` from `ResolveControlGroups(ItemViewer)`
  (`QfcItemController.ViewerSetup.cs:79-123`) and `WireControlTreeEvents()`
  (`QfcItemController.EventWiring.cs:43-71`). Add tests that construct a real, headless
  `new ItemViewer()` (after installing a synchronization context, mirroring
  `QfcItemController.TestSupport.cs:87-93`'s existing `EnsureSynchronizationContext()` helper — this
  pairing with actual `ItemViewer` construction is the only new test-file work needed) and exercise
  both methods end-to-end against it. No production code change is needed for these two members
  themselves.
- **Acceptance:** both members carry no exemption; each is covered by >= 1 passing test constructing
  a real headless `ItemViewer`; no mocking of the control tree itself is required or used.

### R2 — `TlpCellSnapShot` / `IContainerControlLocal` retrofit (2 members, small mechanical seam)

- **Finding:** `TlpCellSnapShot.ApplyState(Control root)` and `TlpCellSnapShotList.ApplyState(Control
  root)` (`QuickFiler/Helper Classes/TlpCellSnapShot.cs:61-64, 181-210`) use only `root.Controls.Find(...)`.
  `IContainerControlLocal` (`UtilitiesCS/Interfaces/IWinForm/IContainerControl.cs:7`) already exists
  in this repo and exposes exactly the needed `Controls` member, but has zero implementers today.
- **Remediation:**
  1. Retype both `ApplyState` overloads in `TlpCellSnapShot.cs` from `Control` to
     `IContainerControlLocal`.
  2. Add `IContainerControlLocal` to `IItemViewer`'s base-interface list (`IItemViewer.cs`), so
     `Mock<IItemViewer>` automatically satisfies it via Moq's interface-proxy generation.
  3. Add `IContainerControlLocal` to `ItemViewer`'s class declaration (`ItemViewer.cs`), with two
     one-line explicit-interface-implementation forwarders for the two `protected`
     `ContainerControl` members `CurrentAutoScaleDimensions`/`PerformAutoScale()` that are not
     already public.
  4. In `QfcItemController.Navigation.cs`'s `ToggleExpansionOff`/`ToggleExpansionOn`, drop the
     `(ItemViewer)` concrete cast (`_tlpStates["Compressed"].ApplyState((ItemViewer)_itemViewer)` →
     `_tlpStates["Compressed"].ApplyState(_itemViewer)`) and remove both
     `[ExcludeFromCodeCoverage]` attributes.
  5. Add tests exercising `ApplyState`'s real `Find`/style-copy/`Enabled`/`Visible` logic using a
     bare `Control` hosting named children and a `Mock<IItemViewer>` whose `Controls` is set up to
     return that host's `ControlCollection` — no real `ItemViewer` or Designer control tree needed;
     this seam is independent of R1.
- **Acceptance:** both `ToggleExpansionOff`/`ToggleExpansionOn` carry no exemption; each is covered
  by >= 1 passing test; the two new forwarder members on `ItemViewer` are trivial one-line delegations
  to the existing protected base members (no behavior change).

### R3 — `WireEvents` free follow-on (1 member)

- **Finding:** `WireEvents()` (`QfcItemController.EventWiring.cs:33-37`) is a 2-line pass-through that
  calls `WireControlTreeEvents()` (now testable per R1) plus the already-non-exempt
  `WireIntentEvents()`.
- **Remediation:** Remove `[ExcludeFromCodeCoverage]` from `WireEvents()` and add a test verifying it
  calls through to both sub-methods against a real headless `ItemViewer` (reuse the R1 test fixture).
- **Acceptance:** `WireEvents()` carries no exemption; covered by >= 1 passing test.

### Explicitly NOT in scope

- The remaining 19 residuals after this cycle (10 orchestration/`Create*Async`/`InitializeWebViewAsync`
  members genuinely blocked by an unbuilt WinForms message-pump test seam for `UiSyncContext`-awaiting
  async paths, or by the `IItemViewer` narrowing invariant for `InitializeWebViewAsync`'s raw WebView2
  accessor; 3 deliberate virtual test seams; 6 `async void` shells; 1 genuine `WebView2CoreInitializer`
  external-runtime dependency) are NOT touched this cycle. Building a WinForms `Application.Run()`-on-
  background-thread pump analogous to the existing WPF `Dispatcher.Run()` pump is a materially larger,
  distinct piece of test infrastructure — tracked as a separate follow-up issue, not part of this cycle
  or this remediation loop.
- Leaf-control interfaces / `IList<IButton>` retyping (Option B) remains declined.

## Constraints

- Legacy non-SDK VSTO/.NET Framework project: explicit `<Compile Include>` wiring (no glob) for any
  new test file.
- Behavior preservation: the `IContainerControlLocal` retrofit must not change `ApplyState`'s runtime
  behavior — it only widens the accepted parameter type. The headless `ItemViewer` tests must not
  leak a live synchronization context across tests (install/restore pattern, mirroring
  `ProgressPane_Tests.cs`).
- 500-line-per-file cap on all touched/new files.
- Toolchain order per CLAUDE.md: csharpier → analyzers → nullable/TWAE → vstest with coverage.
- Determinism: no test may depend on a live message pump completing; the R1 tests target only the
  synchronous `ResolveControlGroups`/`WireControlTreeEvents`/`WireEvents` methods, not the async
  orchestration path (which remains out of scope per the "Explicitly NOT in scope" section).

## Exit condition for cycle 5

`blocking_count == 0` across the re-audit (`code-review`, `feature-audit`, `policy-audit`), which
requires: the residual exemption count is reduced from 24 to 19 (the 5 members above de-exempted and
genuinely covered — tests must exercise real behavior, not just construction, per the cycle-4
precedent), no changed-line regression, all files <= 500 lines, toolchain green, and the reduced
19-member boundary is individually justified and documented for ratification alongside the prior 24.
