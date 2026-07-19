# Fail-Before Exception Dossier — Percentage-Obscuring Defect Runtime Reproduction (P1-T1)

Timestamp: 2026-07-18T08-52

WhyFailingRunImpossible: This execution environment is a non-interactive agent worktree with no
live Outlook host, no VSTO add-in process, and no interactive desktop session for screenshot
capture. The defect is host-bound and visual (owner-drawn `ComboBox` dropdown inside the Outlook
add-in); no unit-test process can host the dropped-down `CboFolders` list, drive theme toggles, or
capture display-scaled screenshots. A failing (defect-visible) runtime capture is therefore
structurally impossible here. This dossier branch is explicitly authorized by plan task P1-T1.

## Alternative Proof (code-inspection observation log)

All citations verified by direct inspection of the current branch
(`feature/quickfiler-breadcrumb-webview2-351`, base 8e242692) on 2026-07-18.

### Hypothesis 1 — theme color contrast (strongest; explicitly flagged in source)

- `UtilitiesCS/HelperClasses/ThemeHelpers/Theme.Rendering.cs:96-98`:
  ```csharp
  // TODO: Override the draw function because these colors do not work as expected
  _comboFolders.BackColor = CboFoldersBackColor;
  _comboFolders.ForeColor = CboFoldersForeColor;
  ```
  The production code carries an explicit maintainer admission that the `CboFolders` theme colors
  "do not work as expected". `CboFoldersBackColor`/`CboFoldersForeColor` are plain mutable theme
  properties (`Theme.cs:187-199`) assigned per theme family.
- The owner-draw paint (`QuickFiler/Viewers/ItemViewer.FolderSearch.cs:170-249`,
  `CboFolders_DrawItem`) draws the percentage with `e.ForeColor` after `e.DrawBackground()`
  (lines 172, 227-234). For a `DrawMode.OwnerDrawFixed` + `DropDownStyle.DropDownList` combo,
  `e.ForeColor`/`e.BackColor` vary with selection/highlight state independently of the theme
  assignment above; a theme state in which the effective fore color has low or no contrast against
  the drawn background renders the percentage text invisible while every rectangle is
  geometrically correct. This is the same text-present-but-unreadable failure mode as the
  documented issue #269 root cause (Light-theme fore/back swap).

### Hypothesis 2 — dropdown scrollbar overlay of the fixed 46 px column

- `ItemViewer.FolderSearch.cs:163`: `private const int FolderPercentColumnWidth = 46;`
- `ItemViewer.FolderSearch.cs:220-234`: the percentage rectangle is anchored flush at
  `e.Bounds.Right - FolderPercentColumnWidth` with width 46 and painted with
  `TextFormatFlags.Right`, i.e. the glyphs hug `e.Bounds.Right` exactly. The classic WinForms
  dropdown behavior of the vertical scrollbar painting over the rightmost pixels of item content
  would clip the percentage glyphs first, since they are the right-most painted content with zero
  right padding.

### Hypothesis 3 — DPI/font clipping of the fixed-width column

- `ItemViewer.FolderSearch.cs:161-163`: the 14 px indent step, 14 px glyph column, and 46 px
  percentage column are integer pixel constants, not DPI-scaled.
- The name rectangle is clamped to `e.Bounds.Right - 46 - nameLeft` (lines 204-210), so name text
  cannot geometrically overlap the percentage; however at >100 % display scaling the font (fixed
  10.875 pt per the Designer) scales while the 46 px column does not, so a wide rendered string
  ("100%") can exceed the fixed column and clip on its left edge, and the fixed item height can
  clip vertically.

### Geometric-overlap exclusion (why the defect is runtime-only)

The static column/rect math (lines 204-234) proves name text and percentage cannot overlap at
100 % scaling: the name rect is explicitly clamped to end where the percentage column begins.
This confirms the epic's statement that static review found no layout-level overlap, and is
precisely why the root cause is unconfirmed-by-design and only a runtime capture (or the Phase 4
by-construction elimination) can discriminate among the three hypotheses.

## Capture set equivalence statement

The plan's capture matrix (dark/light theme x scrollbar-forcing row count x 100 %/150 % scaling)
cannot be produced here; the code-fact table above maps each matrix axis to the code path that
makes the corresponding hypothesis plausible (theme axis -> Hypothesis 1 lines 96-98/227-234;
row-count axis -> Hypothesis 2 lines 163/220-234; scaling axis -> Hypothesis 3 lines 161-163).

MANUAL-VERIFICATION-REQUIRED: yes — a maintainer with a live Outlook host may still capture the
defect matrix against a pre-fix build if a confirmed single-hypothesis diagnosis is desired; the
Phase 4 CSS fix does not depend on which hypothesis is confirmed (see P1-T2 analysis).
