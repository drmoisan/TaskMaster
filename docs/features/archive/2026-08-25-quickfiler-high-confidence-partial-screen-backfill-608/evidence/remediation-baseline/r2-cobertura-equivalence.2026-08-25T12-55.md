Timestamp: 2026-08-25T12-55
Command: Read and compare `evidence/baseline/csharp-coverage.2026-08-25T12-26.cobertura.xml` and `evidence/qa-gates/r1-csharp-coverage.2026-08-25T12-33.cobertura.xml`; normalize paths with `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` `ConvertTo-KoverageCoberturaXml`, merge classes by filename, and summarize with `Get-CoberturaCoverageSummary`.
EXIT_CODE: 0
Output Summary: The raw R1 report has five extra third-party packages and a different denominator. Equivalent repository-package post-processing gives 84.7876% R1 versus 84.7835% baseline; the raw 70.1716% value is not an Issue #608 code-regression classification.

Raw Root Totals:
- Baseline: `53,757 / 63,405` lines (84.7835%), 9 packages, 547 raw class entries.
- R1: `57,239 / 81,570` lines (70.1716%), 14 packages, 3,236 raw class entries.
- Raw delta: `+3,482 / +18,165` lines.

Package Sets:
- Baseline packages: `QuickFiler`, `SVGControl`, `Tags`, `TaskMaster`, `TaskTree`, `TaskVisualization`, `ToDoModel`, `UtilitiesCS`, `VBFunctions`.
- R1 additionally contains `log4net`, `Microsoft.IO.RecyclableMemoryStream`, `Mono.Reflection`, `System.Interactive`, and `System.Linq.Async`.
- Those five added packages contribute `2,719 / 17,014` raw covered/valid lines. They account for the material denominator drift.

Path and Class Normalization:
- Absolute R1 repository paths were reduced to repository-relative filenames before comparison; duplicate `<class>` entries were merged by normalized filename and per-line coverage was de-duplicated using the repository helper.
- Before repository-package filtering, normalized filename comparison found 529 common modules, 18 baseline-only modules, and 419 R1-only modules; the R1-only set includes the five third-party package families above.
- After the helper's repository allowlist and duplicate-class merge, both reports contain 9 packages and 547 merged classes. Baseline is `53,757 / 63,405` (84.7835%); R1 is `53,763 / 63,409` (84.7876%), a `+0.0041` percentage-point difference.
- The remaining normalized module-set difference is 529 common, 18 baseline-only, and 18 R1-only filenames. It is not attributable to the two-file Issue #608 diff without further proof.

Scoped Gate File:
- `QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs`: baseline `86 / 89` covered/valid lines; R1 `90 / 93` covered/valid lines. The new deadline-condition lines are `6 / 6` covered.

Conclusion:
`70.1716%` is not classified as an Issue #608 code regression because collection and post-processing scope are not equivalent: the R1 wrapper failure occurred before its normal post-processing path, leaving third-party package coverage in the raw file. No coverage-regression conclusion is made from the raw denominator.
