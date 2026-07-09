---
name: winforms-testability-epic-298
description: Epic #295 WinForms testability refactor — cross-feature coordination constraints for TaskVisualization children #297/#298
metadata:
  type: project
---

Epic winforms-testability-refactor (#295) makes Tags/TaskTree/TaskVisualization UI projects unit-testable to >=80% line coverage via viewer interfaces deriving from `UtilitiesCS.Interfaces.IWinForm.IForm`. Children: #293 (Tags), #296 (TaskTree), #297 (TaskVisualization core: TaskController/TaskViewer->ITaskViewer), #298 (TaskVisualization secondary: EditFilterController/ManageFilters/Flag*/Auto* helpers). Only #298 depends_on #297 (shared csproj + test project); #293/#296/#297 are disjoint wave-0 parallel.

**Why:** reduce UI-layer regression escapes and enable autonomous agentic maintenance of the WinForms/Outlook-Interop layer.

**How to apply (for #298 research/planning):**
- Hard constraint: `FlagTasks(IApplicationGlobals, IList, bool, IntPtr, string)` ctor shape is pinned by a QuickFiler factory seam `Func<IApplicationGlobals, List<MailItem>, bool, IntPtr, FlagTasks>` (`QfcItemController.Initialization.cs:42,390` + QuickFiler.Test seam tests). Do not change the ctor or the `FlagTasks` type name — extract pure statics instead.
- `ManageFilters` three-call surface (`new ManageFilters(); LoadFilters(globals); Show()`) is consumed by `EfcFormController.cs:562`; preserve it.
- #298 inverts issue #197's class-level `[ExcludeFromCodeCoverage]` posture: #197 exempted EditFilterController/helpers wholesale; #298 introduces seams and narrows exemptions to irreducible UI/COM wiring. `FlagChangeGroup.TryEnqueue`, `FlagChangeTrainingQueue`, `FlagChangeItem` were already preserved measured by #197.
- Viewer interfaces must expose behavioral members (string props + `event EventHandler ...Click`), NOT raw Label/Button/TextBox, so Moq mocks need no live Control.
- As of 2026-07-09, sibling specs/plans (#293, #297) were still template stubs — #298 must record assumptions to verify at execution time against the post-#297 integration head (ITaskViewer shape, dialog seam, Interop adapter, MoqOlToDo reuse). Repo coverage policy is 80/90 with ratified COM/VSTO exemption (CLAUDE.md authoritative), not the 85/75 tier policy in `.claude/rules/`.
