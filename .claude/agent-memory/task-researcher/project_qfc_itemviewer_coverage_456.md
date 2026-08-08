---
name: qfc-itemviewer-coverage-456
description: "#456/epic #136 F14: committed Cobertura class-level line-rate is corrupt (issue #441) but branch-rate/conditions are sound; designer partials cannot be exempted independently and are branch-capped at 75%; STA attrs ship with MSTest 4.3.3"
metadata:
  type: project
---

Findings from F14 (`quickfiler-itemviewer-coverage`, issue #456) research on 2026-08-07 that
generalise beyond the three files researched.

**1. In committed Cobertura reports under `evidence/qa-gates/`, trust `branch-rate` and the
`<condition>` entries; do NOT trust the class-level `line-rate` attribute.**
**Why:** open issue **#441** ("Cobertura post-processing double-counts `<line>` nodes") states the
repo's `Invoke-MSTestWithCoverage.Helpers.ps1` post-processing corrupts class-level `line-rate`.
Reproduced twice on one file: `ItemViewerExpanded.cs` reports `line-rate="0.390244"` (=32/82) but its
own `<line>` children enumerate 106 lines with 40 covered (37.74%); its Designer partial reports
203/204 while enumerating three uncovered lines. In both cases `branch-rate` matched the
`<condition>` data exactly. Mixed numeric formatting in one file (six-decimal vs full-precision
line-rates) is the tell that the report is post-processed, not raw coverlet output.
**How to apply:** recompute line rate from distinct `<line>` children before quoting any per-file
baseline. This extends the epic's directive (which only covered the *denominator* decision) to the
rate itself. See [[quickfiler-percoverage-epic-136]].

**2. A WinForms `*.Designer.cs` partial can never be `ratified-exempt` on its own.**
**Why:** `[ExcludeFromCodeCoverage]` is type-level, so putting it on the designer partial exempts the
hand-written partial too; putting it on both is CS0579; and a `coverage.config` production-path
exclude is Blocking under `.claude/rules/general-unit-test.md`. All three routes are closed.
**How to apply:** classify designer partials `testable`, not exempt. Their line coverage is a pure
function of "is the control constructed in any test" (~98-100% if yes, ~0% if no), and their branch
coverage is governed solely by `Dispose(bool)`. In QuickFiler designers checked so far
(`ItemViewerExpanded.Designer.cs`, `ItemViewer.Designer.cs`) `components` is initialised to `null`
and never assigned, making `components.Dispose()` dead code and capping branch coverage at 3/4 =
**exactly 75%** — passing, but only after a test invokes `Dispose(false)` via a derived probe.
Annotate the structural caps in the ledger so the capstone does not read them as shortfalls.

**3. `[STATestClass]`/`[STATestMethod]` ship inside MSTest.TestFramework 4.3.3 itself** (namespace
`Microsoft.VisualStudio.TestTools.UnitTesting`), not a separate STAExtensions package — no package
exists in the repo. `QuickFiler.Test`, `Tags.Test`, and `UtilitiesCS.Test` all already pin 4.3.3, so
STA scoping needs no packages.config change anywhere.
**How to apply:** do not plan a package addition for STA work. Reuse the dedicated-file precedent in
`Tags.Test/*.StaTests.cs`.

**4. `QuickFiler` DOES grant `InternalsVisibleTo("QuickFiler.Test")`** (its own AssemblyInfo), and the
existing suite already calls `internal` production members directly (e.g. `QfcHomeController.InitAsync`).
So widening a `private` member to `internal` is an established, precedent-backed seam inside
QuickFiler. This is the opposite of the `UtilitiesCS` situation, which grants QuickFiler.Test nothing
— see [[qfc-keyboard-coverage-430]]. Do not conflate the two.

**5. `QuickFiler/Viewers/ToolStripMenuItemCb` shadows `Checked`/`CheckedChanged` with `new` and never
assigns `base.Checked` — but the consequence differs per viewer. CORRECTED 2026-08-07.**
- **`ItemViewerExpanded`**: the defect is live. Its Designer wires
  `CheckedChanged += MenuItem_CheckedChanged` on all four menu items
  (`ItemViewerExpanded.Designer.cs:171,180,189,198`) and that handler downcasts to the base
  `ToolStripMenuItem` (`ItemViewerExpanded.cs:169-176`), so it reads `false` and clears the image the
  setter just applied. A seam is mandatory there.
- **`ItemViewer`**: the defect is NOT live. `ItemViewer.Designer.cs` (6,224 lines) wires **zero**
  `CheckedChanged` and **zero** `.Click` handlers — its only event wiring is `_l0v2h2_WebView2.ParentChanged`
  at `:256`. `ItemViewer.cs:171-187 MenuItem_CheckedChanged` (both overloads) is therefore **dead code**.
  Because `ItemViewer.cs:404/409/414/419` type the wrappers as the derived `ToolStripMenuItemCb`, the
  `*Checked` properties in `ItemViewer.Commands.cs` bind to the shadowing members and round-trip cleanly
  with **no seam at all**. The residual `ItemViewer` defect is different: the Designer assigns a checked
  image at design time (`:6139`, `:6147`) while `_checked` defaults to `false`, and nothing normalises it.
**How to apply:** before asserting a `ToolStripMenuItemCb` consequence, check whether that viewer's
Designer actually wires `CheckedChanged`. `ToolStripMenuItemCb.cs` is F15-owned; do not edit it from F14.

**6. Open issue #457 `excludefromcodecoverage-does-not-suppress-nested-lambdas`.** A method-level
`[ExcludeFromCodeCoverage]` does not suppress lambdas the compiler hoists out of the method, so the
lambda bodies stay in the coverage denominator (cited ceiling: `BreadcrumbPopupUiOperations.cs` cannot
exceed ~91.5%). **How to apply:** treat attribute-based suppression as unreliable whenever a "thin exempt
forwarder" design is proposed, and cite #457 in support of filename/harness-level exclusion over
attribute-level exclusion. A lambda capturing only `this` is emitted as an instance method on the
containing type (not a `<>c` display class), so it stays attributed to its own file — #457's escape does
not apply to that shape.

**7. The three small `ItemViewer` partials (`.Commands.cs` 109L, `.DisplayState.cs` 81L,
`.FolderSearch.cs` 74L) need NO seam and NO STA.** All are pure forwarders onto Designer controls that
`ItemViewer.cs:207-430` already exposes as **public settable properties**, so the
`CreateUninitialized<ItemViewer>()` + assign-controls fixture (`QfcThemeHelperTests.cs:249-265,331-335`)
reaches every line. `.Commands.cs` and `.DisplayState.cs` contain **zero branch points**, making the 75%
branch gate vacuous — F1's harness must report a zero-`<condition>` class as N/A/100%, never 0%.
Only `.FolderSearch.cs` has branches (~10 `?.`/`&&`/`??` points), and its one irreducible line is
`FocusSearch` (`:72`, `Control.Invoke` needs a window handle), which is cheaper to leave uncovered
(90.5% line remains) than to seam or to STA.
