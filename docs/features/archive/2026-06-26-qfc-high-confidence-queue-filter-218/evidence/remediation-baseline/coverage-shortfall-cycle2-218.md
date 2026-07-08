# Repository-Wide Coverage Baseline / Shortfall — Cycle 2 (Rebased Tree), Issue #218

Timestamp: 2026-06-28T17-31

Command: Parse root `line-rate`, `lines-covered`, `lines-valid` from `docs/features/active/2026-06-26-qfc-high-confidence-queue-filter-218/evidence/baseline/coverage-baseline-218.cobertura.xml`.

EXIT_CODE: 0

Baseline (merge-base 1b8536b6) repo-wide line coverage: line-rate `0.6202918410429243` = 62.02918410429243% (lines-covered 100491 / lines-valid 162006).

Numeric gap to raw 80% threshold: 80% - 62.02918410429243% = 17.97081589570757 percentage points.

Notes:
- This merge-base baseline Cobertura is the authoritative no-regression reference for the cycle-2 final comparison (P5-T6).
- Disposition of the repo-wide shortfall is an authority-scoped exception (Finding 3): per CLAUDE.md the 80% floor applies to the testable denominator after excluding VSTO add-in lifecycle, WinForms/Designer, and Outlook Interop event-handler classes; the raw 62% figure includes that exempt COM-bound code. Repo-wide uplift to the raw 80% figure is out of scope for issue #218 and requires maintainer ratification via `feature/csharp-coverage-uplift`. This baseline does NOT authorize raising repo-wide coverage with out-of-scope tests.

Output Summary: Baseline repo-wide line coverage is 62.02918410429243% (100491/162006), 17.97 points below the raw 80% threshold. Recorded as the no-regression reference; the shortfall is dispositioned as an authority-scoped exception in Phase 5 (P5-T8), not closed in this bug remediation.
