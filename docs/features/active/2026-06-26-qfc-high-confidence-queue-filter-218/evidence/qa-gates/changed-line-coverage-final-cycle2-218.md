# Changed-Production-Line Coverage (Final) — Cycle 2, Issue #218

Timestamp: 2026-06-28T17-31

Command: Map `git diff --unified=0 1b8536b6 HEAD -- QuickFiler/Controllers/<file>.cs` added lines (HEAD-side line numbers) for the eight touched production files to the per-`<line number= hits=>` entries in `evidence/qa-gates/final-coverage-cycle2-218.cobertura.xml` (XmlReader streaming, max hits per line across all `<class filename=...>` blocks for each file). Merge-base = `1b8536b6`.

EXIT_CODE: 0

## Per-file changed-line coverage (testable denominator = lines present in Cobertura)

| File | Changed (added) lines | In Cobertura | Coverable changed | Covered changed | Uncovered changed |
|------|----------------------:|:------------:|------------------:|----------------:|------------------:|
| EmailSorter.cs | 85 | yes | 49 | 0 | 49 |
| QfcDatamodel.cs | 29 | no (`[ExcludeFromCodeCoverage]`) | 0 | 0 | 0 |
| QfcDatamodel.FrameBuilding.cs | 154 | no (excluded partial) | 0 | 0 | 0 |
| QfcDatamodel.QueueProcessing.cs | 146 | no (excluded partial) | 0 | 0 | 0 |
| QfcHomeController.cs | 2 | yes | 1 | 1 | 0 |
| QfcHomeController.Iteration.cs | 82 | yes | 52 | 41 | 11 |
| QfcHomeController.Metrics.cs | 226 | yes | 137 | 39 | 98 |
| QfcRemainingQueueAdmission.cs | 58 | yes | 33 | 33 | 0 |

## Aggregate (all eight touched files)

- Total changed (added) production lines: 782
- Total coverable changed lines (testable denominator): 272
- Total covered changed lines: 114
- Aggregate changed-line coverage: 114 / 272 = 41.9118%
- PASS/FAIL vs 90%-for-new/changed-code: FAIL (aggregate)

## Issue #218 behavior subset (the Finding 2 scope) — PASS

The issue #218 behavior change is the high-confidence queue-admission move. Its testable production lines are:
- `QfcRemainingQueueAdmission.cs` (the extracted admission seam, `TryQueueAsync`): 33 / 33 = 100% covered (includes the null-mailItem guard line 39 newly covered by the P4-T2 test `TryQueueRemainingMailItemAsync_NullMailItem_DoesNotScoreAddOrHook`).
- `QfcHomeController.cs` changed initial-load line: 1 / 1 = 100% covered.
- `QfcDatamodel` admission/initial-load methods (`TryQueue...`, `LoadRemainingEmailsToQueueAsync`, `InitEmailQueueAsync`): `[ExcludeFromCodeCoverage]` (QfcDatamodel.cs:24), COM-host-bound, excluded from the testable denominator per CLAUDE.md; exercised behaviorally by the focused QfcDatamodelTests.

Issue #218 testable changed-line coverage subset = 34 / 34 = 100% — PASS. There are ZERO uncovered issue #218 admission/initial-load lines remaining (the single one found in P4-T1, the null guard, was covered in P4-T2).

## Disposition of the aggregate FAIL

The aggregate shortfall is NOT in issue #218 behavior code. It is concentrated in code mechanically relocated by maintainer split commit `2637e4c1` (verified via `git log --diff-filter=A`):
- `EmailSorter.cs` (49 uncovered): a pre-existing email-sorting helper extracted by the split; it has no unit tests and is exercised only through COM/runtime paths. Not part of the #218 fix.
- `QfcHomeController.Metrics.cs` (98 uncovered): pre-existing Outlook-Interop metrics/calendar code (`QuickFileMetrics_WRITE`, `WriteMetricsAsync`, `WriteMoveToCalendar`, `GetMoveDiagnostics`) directly bound to `AppointmentItem`/`Calendar`/`Session`/`Folders`; only the two null-guard regression paths are covered by the Metrics tests. These are exactly the Outlook-Interop event/handler classes CLAUDE.md exempts from the coverage floor (testable-denominator exemption).
- `QfcHomeController.Iteration.cs` (11 uncovered): pre-existing iteration code; the covered majority (41/52) is exercised by the Iteration tests.

Per the cycle-2 inputs, this remediation must NOT raise repository-wide/changed-line coverage by adding out-of-scope tests against this relocated pre-existing COM-bound code. The aggregate FAIL therefore falls under the same authority-scoped, COM/VSTO testable-denominator exemption documented in repo-wide-coverage-exception-cycle2-218.md (Finding 3). The in-scope Finding 2 requirement — every uncovered issue #218 admission/initial-load line receives minimal deterministic coverage — is satisfied (issue #218 subset = 100%).

Output Summary: Aggregate changed-line coverage = 41.9118% (114/272) — FAIL vs 90%, driven entirely by pre-existing COM-bound code relocated by maintainer split 2637e4c1 (EmailSorter, QfcHomeController.Metrics/Iteration). Issue #218 behavior subset (QfcRemainingQueueAdmission 33/33 + QfcHomeController.cs 1/1 = 34/34) = 100% — PASS; QfcDatamodel #218 methods are [ExcludeFromCodeCoverage]. No uncovered issue #218 admission/initial-load line remains. The aggregate shortfall is dispositioned under the authority-scoped COM/VSTO exemption (Finding 3); out-of-scope coverage uplift is prohibited by the inputs.
