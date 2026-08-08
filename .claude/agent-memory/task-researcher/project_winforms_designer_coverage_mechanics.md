---
name: winforms-designer-coverage-mechanics
description: Coverage mechanics for WinForms partial classes — class-level [ExcludeFromCodeCoverage] hides the Designer file too, constructing a form auto-covers ~99% of its Designer, and Forms ARE constructed in this repo's tests despite the "no Forms" rule.
metadata:
  type: project
---

Established 2026-08-07 during F9 (`EfcViewer.cs`) research for epic #136. Four mechanics that any
child touching a `*.Designer.cs`-backed type must know:

1. **`[ExcludeFromCodeCoverage]` on one partial hides the WHOLE type**, including the
   `*.Designer.cs` half. C# merges attributes across partial declarations, so you cannot attribute
   only the designer part. Removing the attribute from `Foo.cs` therefore exposes `Foo.Designer.cs`
   too — for `EfcViewer` that is ~1,500-2,500 newly-measured lines that land at 0% if nothing
   constructs the form. This is a hidden repository-wide coverage regression bundled into every
   "remove the attribute" acceptance criterion.

2. **Constructing the form/control once auto-covers ~99% of its Designer file.** Verified in the
   committed Cobertura (`.../424/evidence/qa-gates/coverage-final.cobertura.xml`):
   `BayesianPerformanceViewer.Designer.cs` 99.14%, `ConfigViewer.Designer.cs` 99.60%,
   `FolderRemapViewer.Designer.cs` 100%, `ItemViewerExpanded.Designer.cs` 99.51%. Conversely the
   designers of attribute-suppressed types (`EfcViewer`, `ItemViewer`, `QfcFormViewer`) are absent
   entirely.

3. **Designer branch-rate is structurally ~0.50** and cannot be improved: generated `Dispose(bool)`
   tests `disposing && (components != null)` but `components` is initialized to `null` and never
   reassigned. So a `*.Designer.cs` classified `testable` can never pass a 75% branch gate. It needs
   a ledger bucket meaning "measured and counted repo-wide, but not gated on the per-file floors" —
   distinct from "carries `[ExcludeFromCodeCoverage]`".

4. **`Form`-derived types ARE constructed in this repo's unit tests**, contradicting the
   `winforms-testability-refactor` epic's condition (d) ("Form-derived types remain prohibited in
   tests even when unshown") that #136 inherited. The in-`QuickFiler.Test` precedent is
   `QuickFiler.Test/Controllers/BayesianPerformanceController.TestSupport.cs:16-53`: dedicated
   `Thread` + `SetApartmentState(STA)` + `SynchronizationContext.SetSynchronizationContext(new ...)`
   install/restore + `new BayesianPerformanceViewer(controller)` + `Dispose()` in `finally` +
   `ExceptionDispatchInfo` marshalling. `UtilitiesCS.Test` does the same under `[STATestClass]` for
   `ProgressViewer`, `ConfigViewer`, `FolderSelector`, `FolderRemapViewer`, `FilterOlFoldersViewer`,
   `FolderInfoViewer`, `InputBoxViewer`. The rule the repo actually enforces is **shown vs unshown**
   (no `Show()`/`ShowDialog()`/pump/popup), not `Form` vs `Control`.

Also load-bearing and reusable: `FormatterServices.GetUninitializedObject(typeof(TForm))` allocates a
form/control with no constructor run, on any thread, no STA needed — 25+ existing call sites,
including on `Form` types (`ProgressViewer_Tests.cs:34`, `ConfigViewer_Tests.cs:28`) and controls
(`QfcThemeHelperTests.cs:334`). It reaches every member that only reads or writes fields. What it
does NOT reach: the constructor, and anything calling `base.` into `Control`/`Form` (e.g.
`base.ProcessCmdKey` dereferences the `PropertyStore` that only `Control`'s ctor allocates).

**How to apply:** before planning any "remove `[ExcludeFromCodeCoverage]` from a WinForms partial"
task, (a) compute the Designer file's exposed denominator, (b) decide whether one STA construction is
in scope — it is usually the cheapest way to satisfy both the per-file gate and the repo-wide
retain-or-improve gate, and (c) raise the Form-vs-Control constraint conflict with the maintainer
rather than silently choosing. See [[qfc227-headless-itemviewer-and-tlpcellsnapshot]] and
[[feedback-exemption-audit-check-proven-techniques]].
