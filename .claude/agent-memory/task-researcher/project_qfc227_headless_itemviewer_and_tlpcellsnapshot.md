---
name: qfc227-headless-itemviewer-and-tlpcellsnapshot
description: Issue #227 cycle-3 re-audit (2026-07-02) — headless ItemViewer construction verdict and TlpCellSnapShot retrofit scope, revised target 24 -> 19.
metadata:
  type: project
---

On 2026-07-02, deep research (`artifacts/research/2026-07-02T16-15-qfc-item-controller-headless-itemviewer-research.md`)
resolved two open questions on the `QfcItemController` 24-member exemption boundary
(`docs/features/active/2026-06-29-qfc-item-controller-testability-227/evidence/other/exemption-boundary.2026-07-02T15-05.md`):

1. **Headless `new ItemViewer()` construction is CONFIRMED SAFE**, proven by an exact structural
   precedent already in the repo: `UtilitiesCS/Threading/ProgressPane.cs`'s constructor has the
   identical `InitializeComponent(); _context = SynchronizationContext.Current; _uiScheduler =
   TaskScheduler.FromCurrentSynchronizationContext();` shape, and
   `UtilitiesCS.Test/Threading/ProgressPane_Tests.cs` already constructs it directly inside a
   `SynchronizationContext.SetSynchronizationContext(new SynchronizationContext())` try/finally wrapper
   (a passing test). This resolves `ResolveControlGroups(ItemViewer)` and `WireControlTreeEvents()` (2 of
   the 12-member concrete-control-tree bucket) as testable with **zero new production seam** — only a
   test-side pairing of the existing `EnsureSynchronizationContext()` helper
   (`QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs`) with real `ItemViewer` construction.
   The other 10 of that 12-bucket remain blocked: async orchestration methods (`InitializeAsync` et al.)
   `await itemViewer.UiSyncContext`, and WinForms `Control` construction silently replaces the ambient
   `SynchronizationContext` with a `WindowsFormsSynchronizationContext` — awaiting that on a thread-pool
   MSTest async thread with no message pump can deadlock (documented precedent:
   `UtilitiesCS.Test/Extensions/AsyncSerialization_Tests.cs` deliberately avoids constructing `ProgressPane`
   in an async test for exactly this reason). No WinForms-`Application.Run()`-pump analogue of the
   proven WPF `Dispatcher.Run()` background-thread technique exists yet in this repo — building one is a
   separate, larger follow-up, not part of this reduction.
2. **`TlpCellSnapShot.ApplyState(Control)` -> `ApplyState(IContainerControlLocal)` retrofit is small
   (4 production files + 1 test file)**, comparable to the already-completed `FolderPredictor`/`Theme`
   retrofits. `IContainerControlLocal` (`UtilitiesCS/Interfaces/IWinForm/IContainerControl.cs`) already
   exists with the exact name the prior follow-up recommendation used, but **zero classes currently
   implement it** — `IItemViewer` extends `IUserControl` which extends the *framework*
   `System.Windows.Forms.IContainerControl` (unrelated, only `ActiveControl`), not the custom one. The
   retrofit needs: retype both `ApplyState` overloads; add `IContainerControlLocal` to `IItemViewer`'s and
   `ItemViewer`'s base-interface lists; two explicit-interface-implementation forwarders in `ItemViewer.cs`
   for `CurrentAutoScaleDimensions`/`PerformAutoScale()` (these two are `protected`, not `public`, on
   `System.Windows.Forms.ContainerControl` — the only non-mechanical part); drop the `(ItemViewer)` cast in
   `ToggleExpansionOff`/`ToggleExpansionOn`. Once done, this seam is testable with a **bare `Control` +
   `Mock<IItemViewer>`** (no real `ItemViewer`/Designer tree needed) — independent of finding #1.

**Revised defensible target if both are pursued: 24 -> 19** (2 + 2 + `WireEvents` as a free 1-member
follow-on once `WireControlTreeEvents` is de-exempted).

**Why this matters for future re-audits**: this is the second consecutive cycle (see
[[qfc-item-controller-227-r2-denial]] and [[feedback-exemption-audit-check-proven-techniques]]) where a
delivered residual boundary turned out to be reducible by cross-checking each residual's stated barrier
against an already-proven technique elsewhere in the SAME repo (here: `ProgressPane`/`ProgressViewer`
tests proving headless WinForms-control construction with the `TaskScheduler.FromCurrentSynchronizationContext()`
precondition, and the pre-existing but unused `IContainerControlLocal` interface). Do not accept "requires
a live control tree" at face value without first grepping for `ProgressPane_Tests`/`ProgressViewer_Tests`-style
precedent and confirming which named "already exists but unused" interfaces (like `IContainerControlLocal`)
are cited in prior follow-up recommendations but never actually wired in.

**How to apply**: if a future cycle re-audits this boundary again, check whether this reduction (24 -> 19)
was actually executed (re-verify via `grep -rn "ExcludeFromCodeCoverage" QuickFiler/Controllers/QfcItemController*.cs`)
before assuming the number is still 24. If the WinForms-message-pump test-infrastructure gap for
`UiSyncContext`-awaiting async paths has since been built (check `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs`
for an `Application.Run()`/pump helper), re-evaluate whether the remaining 9-10 orchestration-method bucket
can shrink further.
