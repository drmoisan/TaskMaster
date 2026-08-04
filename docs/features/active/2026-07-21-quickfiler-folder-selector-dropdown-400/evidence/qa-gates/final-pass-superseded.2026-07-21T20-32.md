# Final Pass Superseded

Timestamp: 2026-07-21T20-32Z
Command: Parse `coverage-final.2026-07-21T20-25.cobertura.xml` and inspect the `BreadcrumbDropDownHost.NormalizeFactory` method line entries before P5-T6 acceptance.
EXIT_CODE: 1
Output Summary: The otherwise successful `final-pass-2026-07-21T20-25Z` sequence is superseded because P5-T6 found `BreadcrumbDropDownHost.NormalizeFactory` at 4/5 executable lines (80.0000%), below AC-18's 90% measurable-member threshold. P5-T1 through P5-T5 were unchecked before any corrective edit. The original artifacts remain retained only as historical evidence and must not be cited as the final clean pass.

## Threshold diagnostic

- Cobertura method: `QuickFiler.Viewers.BreadcrumbDropDownHost.NormalizeFactory`
- Covered/valid: 4/5
- Line rate: 80.0000%
- Uncovered source line: `QuickFiler/Viewers/BreadcrumbDropDownHost.cs:457`
- Missing scenario: the legacy surface-factory constructor receives a null factory and must reject it with `ArgumentNullException`.
- Corrective action: add one deterministic constructor-guard regression outside the four protected assertion-inventory files, then restart the ordered Phase 5 sequence at P5-T1 with a fresh run identity.
