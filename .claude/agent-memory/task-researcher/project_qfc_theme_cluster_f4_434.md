---
name: qfc-theme-cluster-f4-434
description: "Issue #434 (epic #136 child F4) theme/layout cluster: all four files are tests-only (seams already exist); ThemeControlGroup exposes no colour getters; TlpCellStates has 38 sibling-owned refs"
metadata:
  type: project
---

Researched 2026-08-07 for issue #434 (`quickfiler-helper-classes-coverage`, child F4 of epic #136).
Four theme/layout files — `EfcThemeHelper.cs` (499), `QfcThemeHelper.cs` (375),
`QfcThemeControlSet.cs` (110), `TlpCellSnapShot.cs` (223) — all reach >= 80% with **tests only, no
production change and no new seam**.

Non-obvious findings that are expensive to re-derive:

- **`UtilitiesCS.ThemeControlGroup` exposes no public colour getters** (only `GroupName`). Any
  assertion about a theme's colours must go through `ControlGroups[key].ApplyTheme()` on real
  in-memory controls and then read `Control.BackColor`/`ForeColor`. Precedent:
  `QuickFiler.Test/Helper Classes/QfcThemeHelperTests.cs:167-194`.
- **Its constructors validate asymmetrically.** The `(controls, fore, back)` overload throws on null
  and on an empty list; the `TwoFieldAlt`, `TwoFieldAltHover`, object-setter, and WebView2 overloads
  validate nothing. That asymmetry is the source of most of the boundary/invalid-input test cases.
- **The seams already exist.** `QfcThemeControlSet` + `internal SetupThemes(QfcThemeControlSet)` came
  from the issue #236 "coverage seams" refactor; `TlpCellSnapShot.ApplyState(IContainerControlLocal)`
  came from a later de-exemption cycle (documented at `TlpCellSnapShotTests.cs:11-19`). Do not
  propose re-seaming either.
- **`TlpCellStates`/`TlpCellSnapShot` have ~38 sibling-owned production references** across F2, F6,
  F10, F11, F15 (`QfcQueue`, `IQfcQueue`, `IQfcQueue1`, `QfcFormController*`, `IQfcFormViewer`,
  `QfcItemController*`, `QfcCollectionController`, `QfcFormViewer` — twelve `new TlpCellSnapShot(tlp,
  control)` calls in `QfcFormViewer.cs:201-251` alone). Any signature change is a guaranteed
  multi-child merge conflict. Additive-only is a hard constraint.
- **`EfcThemeHelper.cs` at 499/500 is safe only because no production edit is needed.** Its eight
  `isAltHover` lambdas are separate compiled methods in the coverage denominator and need eight
  distinct `ApplyTheme()` calls; `nav` and `selectors` are dead parameters.
- **`QfcThemeHelperTests.cs` is 463/500** — the 500-line rule applies to test code, so new cases
  require extracting its test-support region (lines 226-461) into a `partial class` file first.
- **No STA anywhere in this cluster.** In-memory `Label`/`TableLayoutPanel`/`Button`/`MenuStrip`/
  `FastObjectListView` construction and `TableLayoutPanel.GetCellPosition`/`SetCellPosition` already
  run in plain `[TestClass]` files. `WebView2` is created via
  `FormatterServices.GetUninitializedObject`.

**Why:** epic #136 mandates one research artifact per production file and one atomic task per test
case; knowing up front that this cluster is tests-only collapses the plan to test authoring plus one
test-file split.

**How to apply:** when planning or reviewing F4, reject any proposal that edits these four
production files, and require colour assertions to route through `ApplyTheme()`. See also
[[qfc-item-controller-227-r2-denial]] and
[[feedback-exemption-audit-check-proven-techniques]].
