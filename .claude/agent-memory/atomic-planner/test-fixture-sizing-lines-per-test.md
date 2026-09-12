---
name: test-fixture-sizing-lines-per-test
description: Size new MSTest fixtures by measured lines-per-test, not by guess — 17 methods is the empirical sub-500 ceiling for mock-heavy QuickFiler.Test/Controllers files; split with .PartN.cs partials
metadata:
  type: feedback
---

When a plan creates new C# test files, project each file's line count as `test_count x measured_lines_per_test` and name every split file up front. Never assert "each new test file stays under 500 lines" without that arithmetic.

**Why:** #435 (F6) preflight returned `PREFLIGHT: REVISIONS REQUIRED` on exactly this. The plan asserted a sub-500 invariant for twelve new fixtures; at the repo's own measured ratios at least seven would have overflowed. Measured ratios in `QuickFiler.Test/Controllers/` (CSharpier-formatted, Moq): `QfcCollectionControllerTests.cs` 38.5, `QfcFormControllerSeamTests.cs` 31.5, `QfcItemController.NavigationTests.cs` 30.1, `QfcItemController.FolderHandlingTests.cs` 29.3, `QfcItemController.FocusAndThemeTests.cs` 29.2, `QfcItemController.EventHandlersTests.cs` 27.4, `QfcFormControllerTests.cs` 20.2 (leanest, and itself the deferred 827-line violation). Pure-logic fixtures run 15-18. **17 test methods is the empirical sub-500 ceiling for a mock-heavy fixture here.**

**How to apply:**
- Cap mock-heavy fixtures at 16 methods; cap at 12 when every test builds the full mock bundle *plus* a WinForms control graph or an Outlook COM chain.
- Pure-logic fixtures: still do the arithmetic. 26 cases x 18 = 468 plus skeleton leaves under 20 lines of headroom — split it.
- Split naming follows `QfcStreamingDequeueConfidenceGateTests.Part2.cs` / `.Part3.cs`.
- **Split shape is a partial, not a second test class.** `[TestClass]` is `AllowMultiple = false`, so repeating it in `.PartN.cs` is CS0579. Base file carries `[TestClass]` + `[TestInitialize]`/`[TestCleanup]` and is declared `partial`; each part declares `public partial class <Same>` with none of those three attributes. A delegation prompt asking for "the same `[TestClass]`/`[TestInitialize]`/`[TestCleanup]` skeleton as its Part 1" is asking for code that will not compile — follow the in-repo convention and say why in the plan.
- Bonus: partials keep one fully-qualified name, so existing `/TestCaseFilter:"FullyQualifiedName~<Class>"` tasks survive the split unchanged.
- Each new file costs two tasks (creation + `<Compile Include>` in `QuickFiler.Test/QuickFiler.Test.csproj`) — budget the renumbering. See [[plan-validator-task-id-sequential-constraint]].

Related: [[project_400_partial_class_headroom_placement]], [[per-phase-size-gates-need-scoped-csharpier]].
