---
name: winforms-designer-partial-coverage
description: Reusable argument that a WinForms *.Designer.cs partial is `testable`, not exempt — type-level [ExcludeFromCodeCoverage] hides all partials (QfcFormViewer positive control), no per-file exemption is permitted, and branch is capped at 3/4 = 75% because `components` is never assigned
metadata:
  type: reference
---

Every `quickfiler-per-file-coverage` (#136) child owning a `*.Designer.cs` file needs the same four-part
argument. It does not need re-deriving per file.

1. **Type-level exclusion hides every partial, including the designer.**
   `ExcludeFromCodeCoverageAttribute` is `AllowMultiple = false, Inherited = false` and its documented
   behavior excludes *all members of the annotated class*. Repository positive control, executed-yet-absent:
   `QuickFiler/Viewers/QfcFormViewer.cs:17` is attributed, `QfcFormViewer.Designer.cs:3` is a bare
   unattributed partial, `QfcFormViewer.Designer.cs:42` provably executed (sole construction site of
   `ItemViewerExpanded`, whose designer shows `hits="1"`), yet neither file emits a `<class>` element.
   Negative controls (`ToolStripMenuItemCb`, `BayesianPerformanceViewer`) carry no attribute and both
   partials appear.

2. **No designer-only exemption mechanism is available AND permitted.** Attribute on the designer partial
   re-hides the whole type; on both partials it is CS0579. A `<Sources><Exclude>` entry is Blocking under
   `.claude/rules/general-unit-test.md` § Coverage Exclusion Policy and would require editing
   `coverage.config` and `TaskMaster.runsettings` (both `<ModulePaths>`-only, both F1-owned). A harness
   filename strip is report-level only. **Conclusion: classify `testable` and measure.**

3. **Line coverage of a designer is a pure function of "is the control constructed in any test."**
   `InitializeComponent()` has zero branches, so one construction covers ~99.9%. Corollary: attribute
   removal must land in the SAME COMMIT as an owned construction test, or the ledger sees a huge file at 0%.
   Sizing on `ItemViewer.Designer.cs` (~6,013 coverable lines) showed removing the attribute is **+0.57 pp**
   repo-wide and *exempting* the designer is **−0.16 pp** — the risk runs opposite to the usual assumption.

4. **Branch is capped at 3/4 = 75% (exactly the floor).** The only conditional is
   `if (disposing && (components != null))`; `components` is declared `= null` and never assigned (verify:
   exactly three occurrences in the file). Two trivial tests — `Dispose(false)` via a test-local derived
   probe, and `Dispose()` on a constructed instance — take it from 50% to a passing 75%. Keep both even if
   `Dispose(true)` already runs incidentally; the 50% today depends on an unpinned cross-test disposal.

Also: **quote no `<class>` `line-rate`** — issue #441 double-counts `<line>` nodes at
`scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1:121-122` (`.//lines/line` descendant axis). Recompute
from deduplicated `<line>` children; the per-file merge path at `:181,219` is already correct.

Related: [[interface-files-zero-coverage-denominator]], [[ac-gates-verify-satisfiability]].
