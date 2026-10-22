# Phase 0 — Coverage-Enabled Test Baseline

Timestamp: 2026-08-26T10-42
Task: [P0-T9]
Command: `pwsh -NoProfile -File "scripts\vscode\Invoke-MSTestWithCoverage.ps1" -Configuration Debug -CoverageOutput "coverage\coverage.cobertura.xml"`
EXIT_CODE: 0

RunDisposition: CLEAN

## Output Summary

### Test counts

```
Test Run Successful.
Total tests: 6503
     Passed: 6503
 Total time: 39.7457 Seconds
```

- Passed: **6503**
- Failed: **0**
- Skipped: **0** (none reported)

Neither of the runner's two run-related throws fired. `Invoke-MSTestWithCoverage.ps1:236` did
not fire because the dotnet-coverage/vstest process exited zero.
`Invoke-MSTestWithCoverage.ps1:341` did not fire because
`Assert-CoberturaLineCoverageThreshold` found the repository-wide line rate above the 80 percent
floor. The run therefore reached `ConvertTo-KoverageCoberturaXml` at `:340` and the `Set-Content`
at `:344`, and the log's closing lines confirm it:

```
Post-processing coverage XML for Koverage compatibility...
Done. Coverage artifact: <repo-root>/coverage/coverage.cobertura.xml
```

The Cobertura document on disk is therefore the **post-processed** form, with repository-relative
`filename` attributes using the `\` separator and one pre-merged `<class>` element per source
file. It is not the raw un-post-processed dotnet-coverage output, so the fallback reading
procedure the task describes for a `COVERAGE_FLOOR_TRIPPED` run was not needed.

Instrumentation was performed by `dotnet-coverage v18.5.2.0 [win-x64 - .NET 10.0.10]`.

### Repository-wide rates

Read from the attributes of the root `<coverage>` element:

| Metric | Raw attribute | Percentage |
| --- | --- | --- |
| `line-rate` | `0.848433` | **84.84%** |
| `branch-rate` | `0.788181` | **78.82%** |

Supporting counters: `lines-covered="53912"`, `lines-valid="63543"`,
`branches-covered="12737"`, `branches-valid="16160"`.

### Per-file line-rate, five owned production files

Each figure is computed by collecting every `<line>` element from every `<class>` element sharing
the same `filename` attribute, keying by line number so a line contributed by more than one class
is counted once, and dividing the number of line numbers with total hits greater than zero by the
number of distinct line numbers. This aggregation is what prevents compiler-generated async and
lambda classes from being counted as separate denominators. In this post-processed document the
Koverage step has already pre-merged to one `<class>` per file, so the class count is 1 for each
file below; the aggregation is applied regardless so that the identical method can be re-applied
to the post-change document in [P6-T5].

| Owned production file | Line-rate | Covered | Total | `<class>` elements merged |
| --- | --- | --- | --- | --- |
| `QuickFiler/Controllers/QfcHomeController.cs` | **68.40%** | 171 | 250 | 1 |
| `QuickFiler/Controllers/QfcHomeController.Metrics.cs` | **63.31%** | 88 | 139 | 1 |
| `QuickFiler/Controllers/EfcHomeController.cs` | **97.81%** | 223 | 228 | 1 |
| `QuickFiler/Controllers/EfcHomeController.Metrics.cs` | **97.73%** | 43 | 44 | 1 |
| `QuickFiler/Controllers/EfcHomeController.ExecuteMoves.cs` | **90.41%** | 66 | 73 | 1 |

### Baseline member-level readings (recorded for the [P6-T6] comparison)

| Member | Line-rate | Covered | Total |
| --- | --- | --- | --- |
| `BuildQuickFileMetricLines` | 100.00% | 16 | 16 |
| `SelectMoveMetricsItems` | 100.00% | 4 | 4 |
| `TryBeginExecuteMoves` | 100.00% | 7 | 7 |
| `ResetExecuteMovesState` | 100.00% | 3 | 3 |
| `QuickFileMetrics_WRITE` (all four overloads across both controllers) | 84.72% | 61 | 72 |
| `WriteMetricsAsync` | not addressable as a `<method>` element | — | — |

`WriteMetricsAsync` is an `async` method, so its body compiles into a compiler-generated state
machine and the Koverage per-file merge retains only the non-async `<method>` entries of the
enclosing class. Its lines are present in the merged file-level `<lines>` set but carry no
`<method>` wrapper. [P6-T6] therefore measures it by source line range within the merged
file-level line set, and applies the same range method on both sides of the comparison.
