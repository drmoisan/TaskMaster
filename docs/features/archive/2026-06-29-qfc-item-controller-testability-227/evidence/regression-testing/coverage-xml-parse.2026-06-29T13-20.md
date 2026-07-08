# Phase 2 — Cobertura Numeric Parse (P2-T1)

Timestamp: 2026-06-29T13-20

Command: python parse_cov.py (xml.etree parse of artifacts/csharp/coverage.xml; read root and QuickFiler package line-rate/lines-covered/lines-valid; sum per-class line hits across the QfcItemController cluster filenames)

EXIT_CODE: 0

## Root (whole-process, single-assembly)

- `line-rate`: 0.13954594080589564
- `lines-covered`: 10566
- `lines-valid`: 75717
- Derived: 10566/75717 = 13.95%.

## Package-scope line-rate

- `QuickFiler`: line-rate 0.4134975897161221 (41.35%).
- `QuickFiler.Test`: line-rate 0.9313725490196079 (93.14%).

## QfcItemController cluster — raw Cobertura per-file (covered / total line elements)

| Cluster file | raw covered/valid | raw % |
|---|---|---|
| QfcItemController.cs | 124/130 | 95.38% |
| QfcItemController.Conversation.cs | 70/130 | 53.85% |
| QfcItemController.EventWiring.cs | 186/290 | 64.14% |
| QfcItemController.FolderHandling.cs | 52/107 | 48.60% |
| QfcItemController.MailActions.cs | 24/82 | 29.27% |
| QfcItemController.Navigation.cs | 28/114 | 24.56% |
| QfcItemController.ViewerSetup.cs | 0/76 | 0.00% |
| RAW AGGREGATE | 484/929 | 52.10% |

## Interpretation

- The **covered-line numerator** parsed from the produced XML is 484 across the cluster, and the
  per-cluster covered counts (124, 70, 186, 52, 24, 28, 0) reproduce the prior-cycle evidence
  numerators exactly.
- The raw Cobertura **denominator** (929) exceeds the gate-metric non-exempt denominator (585)
  because the VS `.coverage` collector does not honor `[ExcludeFromCodeCoverage]` on async state
  machines (documented in `p8-tests-coverage.2026-06-29T12-40.md`). The 82.74% gate metric is
  computed by excluding the brace-matched annotated exempt method source-line ranges from the same
  covered set: 484/585 = 82.74%.

## Output Summary

Parsed numeric figures from `artifacts/csharp/coverage.xml`: root 10566/75717 = 13.95%; cluster
covered numerator = 484 (matching prior evidence exactly per cluster). The raw cluster denominator
is 929 (no exempt adjustment); the non-exempt-adjusted gate metric is 484/585 = 82.74%. These figures
are reconciled against the existing evidence in P2-T2.
