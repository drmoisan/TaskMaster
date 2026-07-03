---
name: qfc227-cycle4-toggle-focus-genuine-test-gotchas
description: Three compile/runtime blockers discovered making ToggleFocus tests genuinely execute (QuickFiler.Test), not anticipated by the remediation plan's Theme-NRE-only analysis
metadata:
  type: project
---

Cycle-4 remediation of issue #227 (`QfcItemController.FocusAndThemeTests.cs`, `ToggleFocus`/`ToggleFocus(Enums.ToggleState)`) replaced non-executing `Mock<IItemViewer>()` with `BuildExecutingViewer()` so the delegate body genuinely runs. The plan anticipated two `Theme`-internal NREs (handle-less `_lblItemNumber`/14 other fields) and gave a proven fix (reflection-inject doubles mirroring `Theme.DispatcherTests.cs:91-134`). Three *additional* blockers surfaced that the plan did not cover, all resolved as test-file-only micro-actions:

**Why:** Genuinely executing a previously-never-run delegate body exposes every dependency the delegate touches, not just the ones already analyzed.

**How to apply (if repeating this pattern elsewhere in QuickFiler/UtilitiesCS test suites):**
1. `QuickFiler.Test.csproj` has NO direct `<Reference>` to `ObjectListView.dll` (`BrightIdeasSoftware.*`) or `Microsoft.Web.WebView2.WinForms.dll` — only `QuickFiler.csproj`/`UtilitiesCS.csproj` do. Legacy non-SDK `ProjectReference`s do NOT flow transitive compile-time references to the referencing test project (though the DLLs DO load at runtime via the transitive project-reference output copy). A literal `new BrightIdeasSoftware.FastObjectListView()` / `new Microsoft.Web.WebView2.WinForms.WebView2()` in `QuickFiler.Test` source fails CS0246/CS0234. Fix without touching the .csproj: `Activator.CreateInstance(field.FieldType)` against the field's own runtime `Type` (via the same `GetField` reflection already used to set it) instead of a source-level `new` expression — produces the identical concrete instance.
2. `QfcItemController` has its OWN private `_tableLayoutPanels` field (`QfcItemController.cs:43`), distinct from `Theme`'s field of the same name. `ToggleTips` (called from `ToggleFocus`'s body) does `_tableLayoutPanels.ForEach(...)` unconditionally — throws `ArgumentNullException` (via `EnumerableEx.ForEach`) if `BuildFocusController()` never set it (it doesn't). The pre-existing sibling test `ToggleTips_Synchronous_DispatchesAndExecutesDelegate` already sets this directly in its own Arrange (`SetField(controller, "_tableLayoutPanels", new List<TableLayoutPanel>());`) rather than via the shared builder — reuse that same non-shared-builder pattern for any new test that reaches `ToggleTips` through a genuinely-executing path.
3. `ToggleFocus`'s outer `_itemViewer.Invoke(...)` wrapper calls `ToggleTips(async: false, ...)` inside its own delegate, which itself calls `_itemViewer.Invoke(...)` again via `InvokeBeginInvoke`'s synchronous branch — so a genuinely-executing viewer sees `Invoke` called **twice**, not once. Old marshaling-only tests never saw this because the non-executing mock never ran the outer delegate. When converting a "marshal-only" assertion (`Times.Once()`) to a genuine-execution assertion, recompute the actual call count from the real call graph rather than assuming the original count still holds.

See also [[project_qfc227_coverage_tooling]], [[project_theme_folderpredictor_seam_retrofit_gotchas]].
