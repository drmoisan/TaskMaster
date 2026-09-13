# P3-T3 — Coverage Entry-Point Wiring

Timestamp: 2026-09-13T06-03
Task: [P3-T3]

`Invoke-MSTestWithCoverageMain` in `scripts/vscode/Invoke-MSTestWithCoverage.ps1` gained a
results-directory parameter defaulting to the single-quoted literal `coverage\test-results` and a
log-file-name parameter defaulting to the single-quoted literal `mstest-coverage-run.trx`, both
resolved against the repository root; it dot-sources the summary part file explicitly; it forwards
both to the collection function; and it performs the projection, the reconciliation, the summary and
the conditional discard in the mandated order.

Command: pwsh -NoProfile -Command '<parse the entry point file, locate the Invoke-MSTestWithCoverageMain definition, print the source offset of every call to each wired command, then print every dot-source invocation inside that function>'
EXIT_CODE: 0

```
OFFSET=14826 CMD=Assert-CoberturaLineCoverageThreshold
OFFSET=15459 CMD=ConvertTo-JacocoPackageProjection
OFFSET=14727 CMD=Set-Content
OFFSET=15532 CMD=Set-Content
OFFSET=16725 CMD=Set-Content
OFFSET=15608 CMD=Assert-JacocoProjectionReconciliation
OFFSET=16295 CMD=Get-TrxRunSummary
OFFSET=16794 CMD=Format-TrxRunSummary
OFFSET=16946 CMD=Test-RawCoverageDocumentRetained
OFFSET=17094 CMD=Remove-Item
---DOTSOURCE---
DOTSOURCE=. (Join-Path $ScriptRoot 'Invoke-MSTestWithCoverage.Helpers.ps1')
DOTSOURCE=. (Join-Path $ScriptRoot 'Invoke-MSTest.TrxSummary.ps1')
```

## Ordering, read from the offsets above

| Step | Command | Offset |
|---|---|---|
| Post-processed Cobertura write | `Set-Content` | 14727 |
| Threshold assertion | `Assert-CoberturaLineCoverageThreshold` | 14826 |
| Projection build | `ConvertTo-JacocoPackageProjection` | 15459 |
| Projection write | `Set-Content` | 15532 |
| Reconciliation assertion | `Assert-JacocoProjectionReconciliation` | 15608 |
| Test-result read and parse | `Get-TrxRunSummary` | 16295 |
| Summary render and write | `Format-TrxRunSummary`, `Set-Content` | 16794, 16725 |
| Retention predicate | `Test-RawCoverageDocumentRetained` | 16946 |
| Conditional discard | `Remove-Item` | 17094 |

The offsets are strictly increasing across the threshold assertion, the projection write, the
reconciliation assertion, the summary write and the discard, which is the order the task mandates.

## Acceptance mapping

- The entry point dot-sources `scripts/vscode/Invoke-MSTest.TrxSummary.ps1`. That dot-source is
  present alongside the pre-existing helpers dot-source, and is mandatory rather than incidental: the
  helpers file's own chain reaches four part files, none of which is the summary part file, so
  without it `Get-TrxRunSummary` would be unresolvable here and every summary attempt would take the
  non-fatal warning branch.
- The projection writer is invoked with the post-processed content and not with the raw collector
  string. The document argument is the variable assigned from `ConvertTo-KoverageCoberturaXml`, which
  the abstract-syntax-tree test P3-T8 adds asserts directly; see the P3-T8 artifact.
- The reconciliation assertion is invoked after the projection write: offset 15608 follows 15532.
- The discard is invoked only after all three of the threshold assertion, the projection write and
  the reconciliation assertion: offset 17094 follows 14826, 15532 and 15608. It is additionally
  guarded by the retention predicate and by the run-summary gate.
- Any failure to obtain a run summary is reported as a non-fatal warning and suppresses both the
  summary write and the discard. The summary write, the predicate and the discard all sit inside the
  single `if ($runSummary)` block, so a caught read or parse failure leaves all three unreached. The
  three entry-point calls in
  `tests/scripts/vscode/Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1` exercise that branch
  and each emitted the warning `Test-result summary was not written: Test-result XML has no
  <ResultSummary> node.` while still passing; see the P3-T5 artifact.
- Verified by the tests P3-T7 and P3-T8 add; see those artifacts.

## Output Summary

EXIT_CODE: 0. The entry point declares both new parameters with the mandated single-quoted defaults,
dot-sources the summary part file explicitly, and the source offsets of its wired calls are strictly
increasing in the mandated order: threshold 14826, projection write 15532, reconciliation 15608,
summary write 16725, discard 17094.
