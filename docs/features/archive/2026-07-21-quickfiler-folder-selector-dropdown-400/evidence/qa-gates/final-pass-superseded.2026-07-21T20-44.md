# Final Pass Superseded

Timestamp: 2026-07-21T20-44Z
Command: Parse source sequence points in `coverage-final.2026-07-21T20-38.cobertura.xml` and map them to the complete source spans of changed/new `BreadcrumbDropDownHost` members before P5-T6 acceptance.
EXIT_CODE: 1
Output Summary: The successful `final-pass-2026-07-21T20-38Z` sequence is superseded because complete source-member accounting found four host members below AC-18's 90% threshold. P5-T1 through P5-T5 were unchecked before corrective edits. The 20:38 artifacts remain historical evidence and must not be cited as the final clean pass.

## Threshold diagnostics

| Source member | Covered/valid source sequence points | Coverage | Uncovered lines |
|---|---:|---:|---|
| `CompleteOpenAsync` | 20/29 | 68.9655% | 190-198 |
| `OpenCoreAsync` | 35/40 | 87.5000% | 220, 230, 246, 249, 256 |
| `WaitForReadinessAsync` | 6/7 | 85.7143% | 376 |
| `NormalizeFactory` including returned legacy-factory lambda | 14/17 | 82.3529% | 462-464 |

Cobertura's `<methods>` summary omits async members and attributes only the outer guard sequence points to `NormalizeFactory`; AC-18 requires complete source-member accounting, so those omissions cannot be treated as PASS.

Corrective action: add deterministic host-seam regressions for outer open-pipeline exception recovery, ready-event lifecycle invalidation, readiness cancellation, and a legacy factory returning no surface; then restart Phase 5 from P5-T1 with a fresh run identity.
