# Baseline First-Party Coverage Summary

Timestamp: 2026-06-13T12-00

Source: artifacts/csharp/coverage-firstparty.cobertura.xml (post-#189, post-#193 pipeline), copied verbatim to
docs/features/active/2026-06-13-com-vsto-coverage-exemption-197/evidence/baseline/coverage-firstparty.baseline.cobertura.xml

## Authoritative production-only deduped baseline (roadmap §0.2)
- lines-covered: 38,767
- lines-valid: 65,768
- line rate: 58.95%

## Per-assembly baseline line-rate attributes (read directly from the `<package>` elements)

| Assembly | line-rate | Plan-expected |
|---|---|---|
| QuickFiler | 0.2519870 | 25.15% |
| Tags | 0.3139764 | 31.15% (plan)/31.40% (memo Appendix) |
| TaskMaster | 0.2577566 | 25.68% |
| TaskVisualization | 0.0036859 | 0.37% |
| ToDoModel | 0.1082444 | 10.43% |
| UtilitiesCS | 0.8738085 | (not annotated; reference) |
| VBFunctions | 1.0000000 | (not annotated; reference) |
| Swordfish.NET.General | 0.4652548 | (vendored; out of scope) |
| SVGControl | 0.1627907 | (vendored; out of scope) |

The five exemption-target assemblies' rates match the plan/spec baseline figures. `.Test` packages
are present in the XML but stripped from the denominator by the Koverage allowlist (Issue #193).
