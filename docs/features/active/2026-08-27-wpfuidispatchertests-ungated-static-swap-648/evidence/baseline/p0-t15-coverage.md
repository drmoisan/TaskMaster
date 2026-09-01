# P0-T15 — Baseline Coverage Figures

Timestamp: 2026-09-01T13-52

Command: `pwsh -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot .` (run from the
checkout root, allowed to run to completion)

EXIT_CODE: 0

BaselineLineRate: 0.853761
BaselineLinesCovered: 54966
BaselineLinesValid: 64381
BaselineLineCoveragePercent: 85.3761

Output Summary:

The run completed and printed:

```
Test Run Successful.
Total tests: 6925
     Passed: 6925
 Total time: 41.7253 Seconds
Code coverage results: <checkout-root>\coverage\coverage.cobertura.xml.
Post-processing coverage XML for Koverage compatibility...
Done. Coverage artifact: <checkout-root>\coverage\coverage.cobertura.xml
```

The checkout-root prefixes are elided; the remainder is verbatim. The script printed no coverage
percentage of its own, which matches the recorded fact that its only numeric coverage output is the
Cobertura document it writes.

Because the run reached the `Done. Coverage artifact:` line, the write at
`scripts/vscode/Invoke-MSTestWithCoverage.ps1:343` executed and the document on disk is the
post-processed one. A confirming observation: a fixed-string search for `QuickFiler.Test` in the
copied document returns 0 matches, so the test-assembly packages were stripped by the post-processing
allowlist as expected.

The document was copied to
`docs/features/active/2026-08-27-wpfuidispatchertests-ungated-static-swap-648/evidence/baseline/p0-t15-coverage.cobertura.xml`.

The four numeric fields above are read from the root `coverage` element of that copied document:

```
<coverage line-rate="0.853761" branch-rate="0.793918" complexity="25603" version="1.9"
          timestamp="1788283715" lines-covered="54966" lines-valid="64381"
          branches-covered="13106" branches-valid="16508">
```

`BaselineLineCoveragePercent:` is `line-rate` multiplied by 100.
