# Baseline — Per-File Coverage for the Five Edited Production Files

Timestamp: 2026-09-09T16-40

Source: evidence/baseline/coverage-baseline.cobertura.xml

## Derivation

The counters below are derived, not read. A Cobertura class element carries line-rate, branch-rate,
complexity, name and filename only, and no line counters. Each figure was produced with
Get-CoberturaClassLineSummary, declared at scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1
line 158, whose TotalLines and CoveredLines are deduplicated by line number. A direct count of the
line descendants of a class element double-counts, because each class carries both a
methods/method/lines tree and a class-level lines rollup repeating the same line numbers.

Aggregation is by filename over every class element whose filename attribute ends in that file's
name, because an async state machine can split a single source file across several class elements
and reading one class element would understate the file. The per-class line maps are merged by line
number, a repeated line number resolved by taking the maximum hits value. In this baseline each of
the five filenames resolved to exactly one class element, so the merge was a no-op; the merge is
retained because it is the derivation P8-T7 and P8-T9 also use and the post-change file may split
differently.

## Files

File: UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs
ClassElements: 1
LinesCovered: 216
LinesValid: 271

File: UtilitiesCS/OutlookObjects/Table/OlTableExtensions.Etl.cs
ClassElements: 1
LinesCovered: 284
LinesValid: 296

File: UtilitiesCS/Threading/TimeOutTask.cs
ClassElements: 1
LinesCovered: 576
LinesValid: 610

File: UtilitiesCS/Extensions/DfDeedle.cs
ClassElements: 1
LinesCovered: 163
LinesValid: 163

File: UtilitiesCS/Extensions/DfDeedle.QfcColumns.cs
ClassElements: 1
LinesCovered: 163
LinesValid: 167
