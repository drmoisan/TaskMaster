# Phase 1 — Canonical Artifact Verification (P1-T5)

Timestamp: 2026-06-29T13-20

Command: python -c "import xml.etree.ElementTree as ET; parse artifacts/csharp/coverage.xml; read root line-rate/lines-covered/lines-valid; enumerate packages and QuickFiler classes"

EXIT_CODE: 0

## Parse result

- File exists: `artifacts/csharp/coverage.xml` (13,261,135 bytes).
- WELL_FORMED: YES — parses as valid XML; root element `<coverage>` (Cobertura schema).
- Root `line-rate`: `0.13954594080589564` (readable).
- Root `lines-covered`: `10566`.
- Root `lines-valid`: `75717`.
- Root `branch-rate`: `1`.
- PACKAGE_COUNT: 9.
- QuickFiler packages present: `QuickFiler`, `QuickFiler.Test`.
- QfcItemController class entries: 86 (production partials + compiler-generated async state
  machines across the cluster).

## Output Summary

The canonical artifact `artifacts/csharp/coverage.xml` exists, is well-formed Cobertura XML, and
exposes a readable root `line-rate` of 0.13955 (10566/75717 = 13.95%). The QuickFiler package and
the QfcItemController classes under test are present. This resolves the R1 artifact-existence
sub-claim. The whole-process root line-rate (13.95%) matches the documented single-assembly
post-change figure 10566/75717 = 13.95% in `coverage-delta.2026-06-29T12-50.md`. Numeric coverage
consistency against the affected testable non-exempt denominator is evaluated in Phase 2.
