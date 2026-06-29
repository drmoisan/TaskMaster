# Acceptance Criteria Traceability (Issue #223)

Timestamp: 2026-06-28T20-52

| AC | Requirement | Satisfying tasks | Evidence | Status |
|---|---|---|---|---|
| AC1 | `IsAltKeyCommand` exists, pure non-Form unit, called by all three viewers; Dark/Expanded `[ExcludeFromCodeCoverage]` | P2-T1, P2-T3, P2-T4, P2-T5, P2-T6 | evidence/qa-gates/p2-*; QfcFormKeyHandler.cs; QfcFormViewer/Dark/Expanded ProcessCmdKey | PASS |
| AC2 | Intent events/state props replace 4 Buttons + NumericUpDown; no raw clickable control on interface | P3-T1, P3-T2, P3-T5, P3-T6, P3-T10, P3-T11, P3-T12 | evidence/qa-gates/p3-*; IQfcFormViewer.cs (23 members) | PASS |
| AC3 | `SwapItemTableLayout` added; `L1v0L2L3v_TableLayout` get-only; `ActivateQueuedTlp` swaps via new method | P3-T1, P3-T3, P3-T9 | evidence/qa-gates/p3-*; QfcCollectionController.ActivateQueuedTlp | PASS |
| AC4 | `CaptureTlpCellStates`/`GetKeyEventExclusionControls`/`ItemViewerTemplateMargin` added; templates removed; consumers updated | P3-T1, P3-T4, P3-T7, P3-T8 | evidence/qa-gates/p3-*; QfcFormController.SetupDisposal.cs | PASS |
| AC5 | New MSTest coverage (routing, skip flow, CaptureItemSettings populated/null/early-return); new code >= 90%; no changed-line regression; repo-wide >= 80% | P2-T6, P2-T11, P3-T11, P3-T12, P3-T13, P4-T4, P4-T5 | evidence/qa-gates/p2-tests-coverage; evidence/regression-testing/coverage-delta | PASS (KeyHandler 100%; QfcFormController +12.62pp; repo-wide first-party not reduced) |
| AC6 | No modified production file > 500 lines; new `QfcFormControllerSeamTests.cs` < 500; `QfcCollectionController.cs` net-negative debt and `QfcFormControllerTests.cs` net-neutral test-cap dispositions recorded | P0-T6, P1-T5, P3-T13, P3-T15, P4-T6 | evidence/baseline/baseline-file-sizes; evidence/qa-gates/p1-file-sizes; evidence/qa-gates/p3-file-sizes | PASS |
| AC7 | Full C# toolchain passes in order with no regressions | P1-T6..T9, P2-T8..T11, P3-T16..T19, P4-T1..T4 | evidence/qa-gates/final-* | PASS |

## AC6 File-Size Dispositions

(a) `QuickFiler/Controllers/QfcCollectionController.cs` — pre-existing production 500-line-cap violation.
- P0-T6 baseline: 2299 lines. P3-T15 post: 2296 lines (net -3 from the Seam C `ActivateQueuedTlp` rewrite).
- Disposition: receives ONLY a net-negative edit this cycle; NOT split (it is `[ExcludeFromCodeCoverage]`; splitting it would be a broad out-of-scope refactor). Post-edit count <= baseline. Recorded as pre-existing-debt disposition.

(b) `QuickFiler.Test/Controllers/QfcFormControllerTests.cs` — pre-existing test-code 500-line-cap violation.
- P0-T6 baseline: 823 lines. P3-T15 post: 821 lines (net-neutral; in-place Seam B migration added no new [TestMethod] cases, slightly reduced).
- Disposition: held net-neutral (count not increased versus the 823 baseline). All 11 new seam tests routed to the new `QfcFormControllerSeamTests.cs` (326 lines, < 500). Recorded as pre-existing test-cap disposition.

## Summary
All seven acceptance criteria (AC1–AC7) are mapped to at least one completed task and one evidence artifact, and are PASS. Both AC6 disposition statements are present with their P0-T6 and P3-T15 line-count evidence.
