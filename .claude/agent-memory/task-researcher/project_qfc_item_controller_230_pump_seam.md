---
name: qfc-item-controller-230-pump-seam-blocks-exemption-removal
description: Issue #230 (open) is the single root cause of every remaining QfcItemController [ExcludeFromCodeCoverage] residual; 13 of the 19 attributes are removable today without it.
metadata:
  type: project
---

Open issue **#230** — "Build a WinForms message-pump test seam (`Application.Run()` background
thread) to unblock 9 `QfcItemController` orchestration members" — is the upstream enabler for the
`QfcItemController` exemption boundary that issues #227 and #453 (epic #136 child F10) both work on.

**Why:** as of 2026-08-07 the family carries 19 `[ExcludeFromCodeCoverage]` attributes, all
**member-level** (never on a partial type — that would be CS0579, and `epic.md`'s `[X]` file markers
are misleading). Classified against the irreducible-remainder standard: **12 removable-as-is,
3 removable-with-seam, 4 irreducible pending #230**. The four blocked sites
(`Initialization.cs:200,260,291` and `ViewerSetup.cs:253`) share one barrier — they await
`IItemViewer.UiSyncContext`, and WinForms control construction replaces the ambient
`SynchronizationContext` with a `WindowsFormsSynchronizationContext` that deadlocks on a pump-less
MSTest thread. #230 exists to fix exactly that.

**Always call-site-check an exempted member before planning tests for it.** Three of the 19 sites
sit on **dead** members with zero call sites solution-wide: `Initialize(9 params)` (private,
`Initialization.cs:139`), `CreateAsync` (`:404`) and `CreateSequentialAsync` (`:437`). The correct
disposition is deletion, not testing — deleting removes the exemption at zero coverage cost, since
an exempt member contributes no denominator lines to begin with. `Initialize(bool)`,
`InitializeAsync`, `InitializeGraphicsAsync` and `InitializeSequentialAsync` are all **live**
(callers in `QfcCollectionController.cs` and `QfcQueue.cs`).

**Stale justifications found in-file (the recurring pattern):** the comments at
`Navigation.cs:171-172` and `:189-190` still claim a `TlpCellSnapShot` barrier, but
`TlpCellSnapShotList.ApplyState` now takes `IContainerControlLocal` and `IItemViewer` derives from
it, so both `ToggleExpansion*(ToggleState)` overloads are testable with `Mock<IItemViewer>` today.
Likewise the five `async void` shells in `EventHandlers.cs` are exempt while their structurally
identical, non-exempt, already-tested siblings `BtnDelItem_Click` / `BtnFlagTask_Click` sit in the
same file.

**Removing an attribute lowers the reported percentage before new tests raise it.** Measured: all 19
removed with no new tests takes the family from 82.51% to 69.82% and pushes four currently-passing
files below the 80% gate. Budget tests against the post-removal denominator.

**How to apply:** any future audit of this boundary should (a) check whether #230 has landed before
accepting "irreducible", (b) re-read each residual's actual body rather than its justification
comment, and (c) grep for the `Application.Run()`/pump helper in
`QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs` — as of 2026-08-07 that file has
`StartRunningDispatcher()` (a real WPF `Dispatcher.Run()` on a background STA thread) but no
WinForms analogue.

Related: [[qfc-item-controller-227-r2-denial]], [[qfc227-headless-itemviewer-and-tlpcellsnapshot]],
[[feedback-exemption-audit-check-proven-techniques]], [[quickfiler-percoverage-epic-136]].
