# Fail-Before Exception Dossier — Percentage-Obscuring Defect (P1-T2, [expect-fail])

Timestamp: 2026-07-18T09-00
Task: [P1-T2] runtime reproduction of the EfcViewer percentage-obscuring defect (#349)

WhyFailingRunImpossible: A live Outlook runtime session is structurally unavailable to the executing agent — repo policy prohibits unit tests and agent runs from launching live Outlook, real WinForms forms, or a live WebView2, and the EfcViewer can only be driven end-to-end from a live Outlook explorer session on the user's display. The runtime screenshot + Form.Shown diagnostic capture therefore cannot be produced in this environment. This dossier records the plan-authorized alternative geometry proof instead.

Capture method (alternative proof): static Designer-geometry analysis per research §D.2 (candidate cause 1 — unscaled ColumnHeader widths under WinForms font autoscaling), cross-checked against the committed Designer source. The P1-T1 temporary log4net instrumentation (`// TEMP repro instrumentation (#349) — removed in P8-T3` in `QuickFiler/Viewers/EfcViewer.cs`, OnShown override) remains in place so a manual runtime capture can still be taken during Phase 8 manual verification if a live session becomes available.

## Alternative geometry proof (Designer widths vs expected runtime client width)

Observed committed values in `QuickFiler/Viewers/EfcViewer.Designer.cs` (verified 2026-07-18):

| Item | Value | Source line |
|---|---|---|
| `olvColumnFolder.Width` | 3200 px (fixed) | line 915 |
| `olvColumnPercent.Width` | 500 px (fixed, right-aligned) | line 921 |
| `FolderListBox` design size | (3728, 1) | line 905 |
| Form `AutoScaleDimensions` | (12F, 25F), `AutoScaleMode.Font` | lines ~4250-4251 |
| Form design `ClientSize` | (3844, 1065) | line ~4252 |

Reasoning chain:
1. Design-time math shows no overlap: 3200 + 500 = 3700 <= 3728 (the control's design width), matching the epic's "static column/rect math shows no overlap" observation (research §D.1).
2. The form is authored at a high-DPI design scale (`AutoScaleDimensions (12F, 25F)` ~ 250-300% of a standard 96-DPI font scale). WinForms `AutoScaleMode.Font` rescales `Control` bounds at runtime but does NOT rescale `ColumnHeader.Width` values (ColumnHeaders are not `Control`s). The two fixed pixel widths (3200 / 500) therefore survive unscaled at runtime.
3. At runtime, `EfcFormController.CaptureConfigureItemViewer` additionally sizes the form to 75% of the explorer screen. On an ordinary display (e.g., 1920- or 2560-px-wide screens) the rescaled `FolderListBox.ClientSize.Width` lands far below 3700 px — on the order of 1000-1800 px.
4. Expected runtime condition (research §D.2/§D.3): `olvColumnFolder.Width (3200) > FolderListBox.ClientSize.Width`, so the right-aligned `%` column begins beyond the right edge of the viewport and is reachable only by horizontal scroll — i.e., the percent is always obscured pre-fix.

This documents the pre-fix defect state BEFORE any fix is applied (no rendering/layout change has been made at this point in the plan; only the temporary diagnostic was added by P1-T1).

## Cross-references

- Research: `research/2026-07-16T22-30-efcviewer-breadcrumb-webview2-research.md` §D.1, §D.2 (candidate 1), §D.3.
- Instrumentation: `QuickFiler/Viewers/EfcViewer.cs` OnShown override (P1-T1), logging `FolderListBox.ClientSize.Width`, `olvColumnFolder.Width`, `olvColumnPercent.Width`, `CurrentAutoScaleDimensions`, `DeviceDpi`.
- Pass-after counterpart: P8-T1 (`evidence/regression-testing/percent-visible-pass-after.<timestamp>.md`) — will record remediation-required if the live session remains unavailable.
