---
name: efc736-archiveroot-boundary-sink
description: "#736 research: finding 6's causal claim is false (#699 is authoritative); the keyboard path is silently logged not crashing; sink call sites are 6 not 4; a live test blocks a modal default sink"
metadata:
  type: project
---

Issue #736 consolidates six code-review findings on `QuickFiler/Controllers/EfcFormController.cs`
and `TaskMaster/AppGlobals/AppOlObjects.cs`. Four of its claims did not survive verification
against HEAD (2026-09-02). See
`docs/features/active/2026-09-02-efc-archiveroot-boundary-sink-defects-736/research/2026-09-02T13-15-efc-archiveroot-boundary-sink-defects-research.md`.

- **Finding 6's causal claim is false.** `MoveToFolderAsync_WhenArchiveRootResolves_StillReadsItOnce`
  is a SUCCESS-path test — the archive root resolves. Its `NullReferenceException` comes from
  `EmailFiler.SortAsync(IList<MailItemHelper>)` dereferencing a null `MailItemHelper.FolderInfo`,
  not from archive-root failure. Fixing findings 1/5 will NOT stop it. **Closed-as-NOT_PLANNED
  #699's body is the authoritative description of this defect**, not #736's text. #699 also pins a
  constraint: that test is the ONLY one reaching `EfcDataModel.cs:339`, so deleting it drops
  changed-line coverage below the 90% floor.
- **Finding 2's "can crash the EFC form" is wrong.** The only live `KeyDown` wiring for the EFC form
  is `KeyboardHandler_KeyDownAsync`, which HAS a `catch (System.Exception)` that logs. The sync
  `KeyboardHandler_KeyDown` is wired only from the QFC form viewers. The real defect is a silent
  swallow at the wrong boundary. A genuine unguarded async-void gap does exist nearby at
  `KeyboardHandler.ToggleKeyboardDialogAsync(object, KeyEventArgs)`, reached from
  `EfcViewer.ProcessCmdKey` on bare Alt — that one is not in #736.
- **`KeyboardHandler` is `[ExcludeFromCodeCoverage]`.** Any fix routed through it is unmeasurable;
  put keyboard-boundary fixes in `EfcFormController.KbdExecuteAsync`, which the existing headless
  `CreateMinimalController()` + `SetPrivateField` harness can drive.
- **A live test blocks the obvious finding-4 fix.**
  `EfcFormControllerTests.BoundaryErrorSink_DefaultDelegate_InvokesWithoutThrowing` invokes the
  DEFAULT sink directly. Giving the default a `MessageBox.Show` surface would hang the test run on a
  modal dialog.
- **Counts drifted:** sink call sites are 6 (not 4); `EfcFormController.cs` is 1216 lines (not the
  1189 a stale scope note claimed) and is NOT `partial`; `AppOlObjects.cs` is 494 lines (6 of
  headroom) and IS `partial` — the memory in [[store-runtime-reenable-263]] saying AppOlObjects.cs is
  already over 500 is stale.

**Why:** #736 was assembled by consolidating six separately-filed findings, and the consolidation
prose introduced causal claims none of the source issues made. #726 also landed in
`EfcFormController.cs` between the sweep and the fix, invalidating every cited line number.

**How to apply:** for any consolidated multi-finding issue in this repo, read the ORIGINAL
per-finding issue/potential-doc bodies under `docs/features/potential/promoted/` before trusting the
consolidated text, and re-derive every line citation. Related:
[[qfc-item-controller-defects-484]] (same failure mode: all five "Suspected Fix" sections were
wrong), [[issue-656-bypass-path-does-not-exist]], [[issue-469-already-fixed-residual-is-629]].
