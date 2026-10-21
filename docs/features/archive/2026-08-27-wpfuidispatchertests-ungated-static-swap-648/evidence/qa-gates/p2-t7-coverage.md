# P2-T7 — Coverage-Enabled Suite (Post-Change)

Timestamp: 2026-09-01T14-39

Command: `pwsh -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot .` (run from the
checkout root, allowed to run to completion)

EXIT_CODE: 0

PostLineRate: 0.85373
PostLinesCovered: 54964
PostLinesValid: 64381
PostLineCoveragePercent: 85.373

Output Summary:

The run's exit code is 0. The integer recorded in
`docs/features/active/2026-08-27-wpfuidispatchertests-ungated-static-swap-648/evidence/baseline/p0-t15-coverage.md`
is also 0. The two are **equal**, so the first branch of this task applies and the run proceeded
without a retry. No `DiscardedFirstCommand:` or `DiscardedFirstExitCode:` field appears in this
artifact, because no run was discarded.

The run printed:

```
Test Run Successful.
Total tests: 6925
     Passed: 6925
 Total time: 29.4656 Seconds
Code coverage results: <checkout-root>\coverage\coverage.cobertura.xml.
Post-processing coverage XML for Koverage compatibility...
Done. Coverage artifact: <checkout-root>\coverage\coverage.cobertura.xml
```

The checkout-root prefixes are elided; the remainder is verbatim. The script printed no coverage
percentage of its own; its only numeric coverage output is the Cobertura document.

Because the run reached the `Done. Coverage artifact:` line, the write at
`scripts/vscode/Invoke-MSTestWithCoverage.ps1:343` executed and the document on disk is the
post-processed one. A confirming observation: a fixed-string search for `QuickFiler.Test` in the
copied document returns 0 matches, so the test-assembly packages were stripped by the post-processing
allowlist, matching the baseline document's shape.

The document was copied to
`docs/features/active/2026-08-27-wpfuidispatchertests-ungated-static-swap-648/evidence/qa-gates/p2-t7-coverage.cobertura.xml`.

The four numeric fields above are read from the root `coverage` element of that copied document:

```
<coverage line-rate="0.85373" branch-rate="0.793736" complexity="25603" version="1.9"
          timestamp="1788284730" lines-covered="54964" lines-valid="64381"
          branches-covered="13103" branches-valid="16508">
```

`PostLineCoveragePercent:` is `line-rate` multiplied by 100.
