---
name: tagcontroller-refactor-293
description: Issue #293 TagController testability research findings and epic #295 shared-pattern facts (ITagViewer, IForm gaps, seam design, orphan files)
metadata:
  type: project
---

Research completed 2026-07-09 for #293 (Tags project) under epic winforms-testability-refactor #295.

**Why:** wave-0 child; siblings #296/#297/#298 follow the same `IForm`-derived viewer-interface + COM/logic-split + seam pattern.

**How to apply:** when researching the sibling refactors, reuse these verified facts.

- `IForm` (`UtilitiesCS/Interfaces/IWinForm/IForm.cs`) provides `Close()`, `KeyPreview`, `ShowDialog()` but NOT `Text`, a `KeyDown` event, or `Controls` — a viewer interface must add those explicitly. A WinForms `Form`'s built-in members satisfy the `IForm` base implicitly (precedent: `QuickFiler/Interfaces/IQfcFormViewer.cs`, which uses a HYBRID of intent-named events/props + a few raw control abstractions + intent snapshot methods).
- Tags orphans NOT in `Tags.csproj` (dead, ignore/cleanup only): root `Tags/CheckBoxController.cs` (buggy `OK_Action`/NRE ctor) and `Tags/AutoAssignInterface.cs`. Compiled `IAutoAssign` is `UtilitiesCS/Interfaces/IToDo/IAutoAssign.cs` (namespace `Tags`, has `AutoFindAsync`); compiled CheckBoxController is `Helper Classes/CheckBoxController.cs` (already `[ExcludeFromCodeCoverage]`).
- `Tags/PrefixItem.cs` throws `NotImplementedException` on `PrefixType`/`OlUserFieldName` — tests need a complete `IPrefix` fake (existing `TestPrefix` in `TagControllerCoverageExpansionTests.cs`).
- Seam recommendation: one `IUserPrompt` interface covers `MessageBox.Show` + `InputBox.ShowDialog` (routing through it avoids constructing the live `InputBoxViewer` form, which the built-in `InputBox.DialogInvoker` seam does NOT avoid). Focus-rect draw (`Graphics.FromHwnd(cbx.Handle)` in `ControlPaint.DrawFocusRectangle`) is the only hard HWND dependency — seam as injectable `Action<CheckBox>`. `ControlPosition.CreateTemplate/Set` are host-neutral (no Handle). `.Focus()` is a safe no-op with no handle.
- Existing `Tags.Test` uses live `new TagViewer()` + `[STAThread]` + `Task.Delay(50)` (banned) — must migrate to mocked `ITagViewer`; extract `async Task ButtonAutoAssign_Action()` so tests await instead of delaying.
- Coverage: CLAUDE.md gate = 80% project / 90% new module (binding here); extract `LauncherAutoAssign` out of `[ExcludeFromCodeCoverage] TagLauncher.cs` to make it testable. Per `[[qfc-item-controller-227-r2-denial]]`, do NOT inherit blanket exemptions silently — per-member barrier analysis expected.

Artifact: `docs/features/active/2026-07-09-tagcontroller-testability-refactor-293/research/research-findings.2026-07-09T21-30-00Z.md`
