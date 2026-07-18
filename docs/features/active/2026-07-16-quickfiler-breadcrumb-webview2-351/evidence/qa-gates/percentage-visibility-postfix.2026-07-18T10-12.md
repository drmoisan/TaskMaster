# Post-Fix Percentage Visibility Evidence (P6-T1, AC-6, US-5) — STRUCTURAL-IMPOSSIBILITY DOSSIER

Timestamp: 2026-07-18T10-12

WhyRuntimeCaptureImpossible: This execution environment is a non-interactive agent worktree with
no live Outlook host, no VSTO add-in process, and no desktop session; the breadcrumb WebView2
cannot be rendered, themed, scrolled, or screenshotted here. The screenshot matrix (dark/light x
scroll-forcing row count x 100 %/150 % scaling) pairing with the P1-T1 dossier is therefore
structurally impossible in this environment. No PNG captures accompany this artifact; no runtime
pass is claimed.

## Alternative proof — deterministic evidence pinning the same behavior

The P1-T2 analysis (dossier-assumed Hypothesis 1, theme color contrast, with Hypotheses 2/3 as
secondary) maps each failure axis to a CSS structure that eliminates it BY CONSTRUCTION; the
structure is pinned by committed code and unit tests:

- Percentage cell CSS (`QuickFiler/Resources/FolderBreadcrumb.html`, `.pct` rule):
  `flex: 0 0 auto; margin-left: auto; flex-shrink: 0; min-width: 5ch; text-align: right;` —
  the exact FR-5 fix; the cell cannot be compressed or overlapped by siblings (closes
  Hypothesis 2), and `ch`-based min-width scales with the font at any display scaling (closes
  Hypothesis 3).
- Segment container CSS (`.crumbs` rule): `flex: 1 1 auto; min-width: 0; overflow: hidden;
  text-overflow: ellipsis; white-space: nowrap;` — long paths truncate in middle segments
  instead of pushing the percentage out of view (FR-1).
- Theme colors exclusively via CSS custom properties switched atomically by the `themeChange`
  bridge message with a `prefers-color-scheme` default (`:root`/`:root[data-theme="dark"]`
  variable sets) — foreground/background always come from the same variable set, eliminating
  the owner-draw fore/back mismatch (closes Hypothesis 1). The removed defect surface
  (`Theme.Rendering.cs` `_comboFolders` assignments under the "colors do not work as expected"
  TODO) no longer exists (see `evidence/other/cbofolders-decommission-verification.2026-07-18T10-05.md`).
- Deterministic tests pinning the percentage content and the theme message path:
  - `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbRenderProjectionTests.cs` —
    `Project_FullChainSuggestionRow_RendersOrderedSegmentsArrowsAndPercent`,
    `Project_PercentFormatting_MatchesPercentageFormatterParity` (0%/100%/empty parity with
    `PercentageFormatter`), `Project_PathBRow_RendersAncestorSplitChainWithEmptyPercentCell`,
    `Project_TruncationEligibility_MarksInteriorSegmentsOnly`.
  - `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbBridgeRouterTests.cs` —
    `Route_ThemeChange_EchoesThemeAndReRenders`.
  - `UtilitiesCS.Test/OutlookObjects/Folder/BreadcrumbBridgeMessagesTests.cs` —
    `RoundTrip_ThemeChange_PreservesTheme`, `RoundTrip_Render_PreservesRowsCellsAndPercentText`.

Hypothesis-closure statement: the P1-T2 selected hypothesis (theme color contrast) is closed by
construction — no code path can pair a themed background with an untheme-controlled foreground in
the breadcrumb page; the secondary hypotheses are closed by the flex/`ch` structure quoted above.

MANUAL-VERIFICATION-REQUIRED: yes — the maintainer must capture, in the live add-in with the
breadcrumb control: the suggestion list in dark and light themes, with a maximal-length folder
path, enough rows to require scrolling, at 100 % and 150 % display scaling, and confirm the
percentage is fully visible and unobstructed in every capture.
