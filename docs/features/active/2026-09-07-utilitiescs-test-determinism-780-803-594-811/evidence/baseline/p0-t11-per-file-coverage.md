# P0-T11 — Baseline per-file coverage for the eight production files

Timestamp: 2026-09-08T09-26
Task: [P0-T11]
Command: pwsh -NoProfile -File coverage/plan811-helper.ps1 running PROC-COV (`Get-FileLineMap`, `Get-FileSummary`, `Get-MethodLineRate`) over `coverage/p0-baseline.cobertura.xml`
EXIT_CODE: 0

PROC-COV merges the `./lines/line` and `./methods/method/lines/line` node axes by line number,
keeping the maximum `hits`, which is the repository helper's own dedup rule. Suffixes are written
with the single backslashes the collector emits.

## Per-file summaries

| Suffix | valid | covered | uncovered |
|---|---|---|---|
| `UtilitiesCS\Extensions\DictionaryExtensions.cs` | 108 | 101 | 7 |
| `UtilitiesCS\OutlookObjects\Table\OlTableExtensions.Etl.cs` | 297 | 271 | 26 |
| `UtilitiesCS\Extensions\DfDeedle.cs` | 159 | 157 | 2 |
| `UtilitiesCS\Extensions\DfDeedle.FrameUtilities.cs` | 169 | 117 | 52 |
| `UtilitiesCS\OutlookObjects\Filter DASL\DASLFilterParser.cs` | 79 | 76 | 3 |
| `UtilitiesCS\HelperClasses\PrettyPrint.cs` | 440 | 375 | 65 |
| `UtilitiesCS\OutlookObjects\Table\OlTableExtensions.TableAccess.cs` | 270 | 215 | 55 |
| `UtilitiesCS\ReusableTypeClasses\Other\StackGeek.cs` | 93 | 93 | 0 |

Every one of the eight matched at least one `class` element, so `valid` is greater than 0 in all
eight cases and no suffix silently measured nothing.

## Baseline uncovered ceilings for P7-T6

These are the figures P7-T6 clause 1 compares against; final `uncovered` must be at most the value
below for each file.

```
BASELINE_UNCOVERED: UtilitiesCS\Extensions\DictionaryExtensions.cs=7
BASELINE_UNCOVERED: UtilitiesCS\OutlookObjects\Table\OlTableExtensions.Etl.cs=26
BASELINE_UNCOVERED: UtilitiesCS\Extensions\DfDeedle.cs=2
BASELINE_UNCOVERED: UtilitiesCS\Extensions\DfDeedle.FrameUtilities.cs=52
BASELINE_UNCOVERED: UtilitiesCS\OutlookObjects\Filter DASL\DASLFilterParser.cs=3
BASELINE_UNCOVERED: UtilitiesCS\HelperClasses\PrettyPrint.cs=65
BASELINE_UNCOVERED: UtilitiesCS\OutlookObjects\Table\OlTableExtensions.TableAccess.cs=55
BASELINE_UNCOVERED: UtilitiesCS\ReusableTypeClasses\Other\StackGeek.cs=0
```

## `StackGeek.Main` method line rate

MAIN_LINE_RATE: 1

`Get-MethodLineRate` for suffix `UtilitiesCS\ReusableTypeClasses\Other\StackGeek.cs`, method
`Main`, returned the number `1`, not `ABSENT`. `Main` is fully covered at baseline, which is the
comparison point for the new `Run(TextWriter)` extraction that P5-T4 creates and P7-T6 clause 3
holds to a line rate of at least 0.90.

## `EtlAsync` span

`EtlAsync` is `async` and emits no `<method>` element in this pipeline's Cobertura output, so the
span rule applies instead: the declaration line through the closing brace.

```
ETLASYNC_SPAN: 66-130
ETLASYNC_SPAN_RATE: 35/50
```

50 of the 65 lines in the span carry a line node; 35 of those are covered, giving a span rate of
0.700. The 15 uncovered lines in the span are:

```
94, 109, 110, 111, 112, 113, 114, 115, 117, 118, 119, 120, 121, 122, 123
```

Lines 117 through 123 are the `catch (TimeoutException)` block — the deadline-expiry path that is
the mechanism of the AC2 defect and is entirely unexercised at baseline. Line 114 is the
`.TimeoutAfter(milliseconds, attempts)` call on the `GetArray` branch. P7-T6 clause 4 requires the
final span rate to be at least this 35/50, and the new
`OlTableExtensionsEtlClockTests.EtlAsync_DeadlineExpires_ReturnsNullDataAndCancelsTokenSource`
test is what covers the `catch` block for the first time.

## Acceptance evaluation

- All eight suffixes matched at least one `class` element with `valid` greater than 0. PASS
- `Get-MethodLineRate` for `Main` returned `1`, a number rather than `ABSENT`. PASS
- The `EtlAsync` span `valid` is 50, greater than 0. PASS
- `BASELINE_UNCOVERED: <suffix>=<n>` is recorded for each of the eight. PASS

## Output Summary

Eight production files measured against the P0-T10 baseline Cobertura document. Uncovered line
counts range from 0 (`StackGeek.cs`) to 65 (`PrettyPrint.cs`). `StackGeek.Main` is at line rate 1.
The `EtlAsync` span 66-130 is at 35 of 50 covered lines, with the entire `catch (TimeoutException)`
block (117-123) uncovered at baseline.
