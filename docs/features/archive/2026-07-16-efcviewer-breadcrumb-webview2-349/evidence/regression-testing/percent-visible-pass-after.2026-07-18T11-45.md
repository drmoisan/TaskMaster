# Percentage-Visibility Pass-After Verification (P8-T1)

Timestamp: 2026-07-18T11-45
Status: REMEDIATION-REQUIRED (manual verification outstanding) — this artifact does NOT record a pass.

Verification method (planned, outstanding): launch the EfcViewer against live Outlook, resize the
form to its minimum width, confirm via a JS-side rect check
(`document.querySelectorAll('.pct')` each `getBoundingClientRect().right <=` its row's
`getBoundingClientRect().right` and fully within the viewport) that every percent text node is
fully inside its row's client rect, and capture a sibling screenshot.

WhyOutstanding: a live Outlook runtime session is structurally unavailable to the executing agent
(repo policy prohibits agent runs from launching live Outlook, real forms, or a live WebView2), so
the runtime pass-after observation cannot be captured in this environment.

Structural evidence available now (NOT a substitute for the runtime pass):
- The CSS fix is implemented and unit-verified: `.pct { flex: 0 0 auto; margin-left: auto; white-space: nowrap; }`
  is a fixed non-shrinking flex item and only `.crumb` may truncate
  (`UtilitiesCS/OutlookObjects/Folder/BreadcrumbDocumentAssets.cs`); the renderer emits the percent
  as the trailing `.pct` item on EVERY row, asserted by
  `BreadcrumbHtmlRendererTests.RenderRowFragment_EveryRowKind_EmitsTrailingPctFlexItem` and
  `RenderRowFragment_CollapsedRow_StillEmitsTrailingPercent` (both green in the Phase 7 gate).
- The defect mechanism (fixed 3200/500 px unscaled ColumnHeader widths) is eliminated: the
  columns no longer exist after the P6-T2 Designer swap.
- Fail-before counterpart: `evidence/regression-testing/fail-before-exception.2026-07-18T09-00.md`.

Required remediation: perform the manual runtime check above during the next live Outlook session
and update this artifact (or add a sibling pass artifact with screenshot) before treating spec
AC-5 as fully verified.
