# Issue #608 R1 QA baseline provenance

Timestamp: 2026-08-25T12-50
Command: Read the four named baseline receipts and coverage report.
EXIT_CODE: 0
Output Summary: Baseline MSTest passed 6,474 tests at 84.7835% repository line coverage (53,757 of 63,405 lines). The earlier analyzer and global-nullable rebuilds were blocked by missing package assets; the global-nullable command is superseded by the local per-file type/nullable gate for R1.

Sources:

- `evidence/baseline/csharp-analyzers.2026-08-25T12-23.md`: exit 1; 37 errors and 4 warnings caused by missing restore assets, including Meziantou.Analyzer, NETStandard.Library, System.ValueTuple, Microsoft.Testing.Platform, ExCSS, and log4net.
- `evidence/baseline/csharp-nullable.2026-08-25T12-23.md`: exit 1; the same restore-asset block prevented global nullable analysis.
- `evidence/baseline/csharp-tests-coverage.2026-08-25T12-27.md`: exit 0; 6,474 tests passed; repository line coverage 84.7835% (53,757 / 63,405).
- `evidence/baseline/csharp-coverage.2026-08-25T12-26.cobertura.xml`: coverage root `line-rate="0.847835"`, `lines-covered="53757"`, `lines-valid="63405"`.

R1 comparison target: query `QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs` and `QfcStreamingDequeueConfidenceGate.DequeueAsync` in the post-change Cobertura report. The baseline has no Issue #608 changed-code lines, so all newly introduced or changed gate lines must meet the 90% coverage requirement.

Zero-regression thresholds:

- New analyzer findings: 0.
- New compiler or nullable diagnostics: 0.
- New failing tests: 0.
- Repository line coverage: at least 84.7835%.
- Changed-file coverage: not decreased; new or changed units: at least 90%.
