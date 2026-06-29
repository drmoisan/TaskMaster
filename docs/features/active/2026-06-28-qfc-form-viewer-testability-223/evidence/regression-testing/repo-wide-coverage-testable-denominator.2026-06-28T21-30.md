# P2-T2 — Repo-Wide Testable-Denominator Figure (Remediation Cycle 1, Issue #223)

Timestamp: 2026-06-28T21-50

## Exemption-boundary confirmation (collector honors [ExcludeFromCodeCoverage])
Probed the instrumented class set (446 classes total) for the documented COM/VSTO/WinForms exempt types:
- QfcFormViewer — ABSENT (exemption honored)
- QfcFormViewerDark — ABSENT (exemption honored)
- QfcFormViewerExpanded — ABSENT (exemption honored)
- Designer-generated classes — ABSENT (exemption honored)

The `[ExcludeFromCodeCoverage]`-marked Form-derived/Designer/COM-host-bound classes are absent from the instrumented denominator: the collector honors the attribute, so the instrumented denominator already represents the testable denominator after attribute-based exemptions. coverage.config additionally excludes third-party/F# assemblies from instrumentation. No threshold or exemption was weakened (G2 honored); no exemption was widened to inflate the figure.

## Testable-denominator figure
- Authoritative (#197 per-`<line>`, vendored included): 39585 / 53969 = 73.35%
- Cobertura root aggregate: 71654 / 96685 = 74.11%
- For transparency, vendored-excluded per-`<line>`: 38607 / 50745 = 76.08%

All three measurement conventions yield a figure below the 80% floor.

Output Summary:
The repo-wide first-party testable-denominator coverage figure is 73.35% (authoritative #197 method) / 74.11% (Cobertura root). The documented `[ExcludeFromCodeCoverage]` exemption boundary was applied as-written (Form-derived/Designer/COM-host-bound classes absent from instrumentation) and was NOT weakened. The figure is below 80% regardless of measurement convention.
