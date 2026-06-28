# Repository-Wide Coverage Baseline / Shortfall (Cycle 2) — Issue #218

Timestamp: 2026-06-28T15-34

Command: parse root `line-rate`, `lines-covered`, `lines-valid` from `docs/features/active/2026-06-26-qfc-high-confidence-queue-filter-218/evidence/qa-gates/coverage-final-218.cobertura.xml` (cycle-entry post-change) and `docs/features/active/2026-06-26-qfc-high-confidence-queue-filter-218/evidence/baseline/coverage-baseline-218.cobertura.xml` (merge-base baseline).

EXIT_CODE: 0

Output Summary:
- Pre-cycle-2 repo-wide line coverage (cycle-entry post-change Cobertura): `line-rate="0.6204458810901509"` = **62.04458810901509%** (lines-covered 100578 / lines-valid 162106).
- Merge-base baseline repo-wide line coverage: `line-rate="0.6202918410429243"` = **62.02918410429243%** (lines-covered 100491 / lines-valid 162006).
- Raw policy threshold: 80%.
- Numeric gap to raw 80% threshold (from pre-cycle-2 figure): 80% - 62.04458810901509% = **17.95541189098491 percentage points**.
- Cycle-1 delta vs merge-base baseline: +0.0154040047226666 percentage points (positive, no regression).

Disposition (Finding 3):
- This shortfall is a pre-existing, repository-wide condition. It is dispositioned as an authority-scoped exception per `CLAUDE.md` (General Unit Test Policy / COM-VSTO coverage exemption): the 80% floor applies to the testable denominator after excluding VSTO add-in lifecycle classes, WinForms/Designer code, and Outlook Interop event-handler classes; the raw 62% figure includes that exempt COM-bound code in the denominator.
- Repo-wide uplift to the raw 80% figure is out of scope for issue #218 and requires maintainer ratification via `feature/csharp-coverage-uplift`. The formal exception evidence is produced in Phase 5 (P5-T8). No policy file is modified.
