# P7-T6 — Coverage delta

Timestamp: 2026-09-08T10-17
Task: [P7-T6]
Command: pwsh -NoProfile -File coverage/plan811-helper.ps1 running PROC-COV over `coverage/p0-baseline.cobertura.xml` and `coverage/p7-final.cobertura.xml` and PROC-CHANGED (`git diff --unified=0 bb1c7d4b60f7b782227956f36859314d5c47bb03 -- <file>`)
EXIT_CODE: 0
Toolchain pass: 3

All eight production files are tracked at the base commit, so the anchored diff sees every change
to them and no staging companion is needed for this task.

## Clause 1 — per-file uncovered, final at most baseline

| Suffix | Baseline valid/covered/uncovered | Final valid/covered/uncovered | Verdict |
|---|---|---|---|
| `UtilitiesCS\Extensions\DictionaryExtensions.cs` | 108 / 101 / 7 | 106 / 99 / 7 | OK |
| `UtilitiesCS\OutlookObjects\Table\OlTableExtensions.Etl.cs` | 297 / 271 / 26 | 296 / 284 / 12 | OK |
| `UtilitiesCS\Extensions\DfDeedle.cs` | 159 / 157 / 2 | 163 / 163 / 0 | OK |
| `UtilitiesCS\Extensions\DfDeedle.FrameUtilities.cs` | 169 / 117 / 52 | 170 / 118 / 52 | OK |
| `UtilitiesCS\OutlookObjects\Filter DASL\DASLFilterParser.cs` | 79 / 76 / 3 | 79 / 76 / 3 | OK |
| `UtilitiesCS\HelperClasses\PrettyPrint.cs` | 440 / 375 / 65 | 440 / 375 / 65 | OK |
| `UtilitiesCS\OutlookObjects\Table\OlTableExtensions.TableAccess.cs` | 270 / 215 / 55 | 271 / 216 / 55 | OK |
| `UtilitiesCS\ReusableTypeClasses\Other\StackGeek.cs` | 93 / 93 / 0 | 96 / 96 / 0 | OK |

CLAUSE1_FAILURES: 0. No file regressed; two improved materially (`OlTableExtensions.Etl.cs` from
26 uncovered to 12, `DfDeedle.cs` from 2 to 0).

## Clause 2 — every added executable line has hits greater than 0

```
ADDED_TOTAL=76 EXECUTABLE_COVERED=38 NON_EXECUTABLE=38 UNCOVERED=0
```

76 post-image added lines across the eight files. 38 carry a line node in the final Cobertura
document and all 38 report `hits` greater than 0; the other 38 have no line node and are recorded
as `hits=non-executable` (signature continuation lines, `//` and `///` comment lines, braces and
`using` directives). CLAUSE2_FAILURES: 0.

This clause failed on the first evaluation of this task, with two uncovered added lines. Both were
closed by P7-T7 and this is the post-closure measurement; see `p7-t7-gap-closure.md`.

## Clause 3 — `StackGeek.Run` line rate

RUN_LINE_RATE: 1

`Get-MethodLineRate` for suffix `UtilitiesCS\ReusableTypeClasses\Other\StackGeek.cs`, method
`Run`, returns 1. `Run` is a new method extracted by P5-T4, so CLAUDE.md UT2's 90 percent floor
for new methods applies; 100 percent clears it. The rate is a number, not `ABSENT`, so the method
element was found and the measurement is not vacuous.

## Clause 4 — `EtlAsync` span rate

| | Span | Rate |
|---|---|---|
| Baseline (P0-T11) | 66-130 | 35/50 = 0.7000 |
| Final | 66-132 | 48/49 = 0.9796 |

The final span was read from the edited file by the rule the plan states: the line containing
`> EtlAsync(` through the first later line that is exactly eight spaces followed by `}`. The final
rate 0.9796 is at least the baseline 0.7000, so the clause passes with a wide margin.

The improvement is the point of the change: the `catch (TimeoutException)` block was entirely
uncovered at baseline (lines 117-123 of the pre-change file) and is now exercised by
`OlTableExtensionsEtlClockTests.EtlAsync_DeadlineExpires_ReturnsNullDataAndCancelsTokenSource`,
and the `GetArray` branch's `TimeoutAfter` is exercised by
`EtlAsync_NoBinaryOrObjectFields_UsesGetArrayBranchOnControlledClock`.

ETLASYNC_UNCOVERED_FINAL: 96 — a single remaining uncovered line in the span, down from fifteen.

## Clause 5 — repository-wide comparison

COMPARABILITY: A

| Observation | Value |
|---|---|
| Baseline `lines-valid` | 200385 |
| Final `lines-valid` | 200629 |
| Denominator delta | 0.1218 percent |
| Baseline `line-rate` | 0.860109289617486 |
| Final `line-rate` | 0.860423966624964 |
| Branch A gate: final at least baseline minus 0.005 | True |

The denominators differ by 0.1218 percent, which is within the 1 percent band, so branch A applies
and the rate comparison is gated. Final line-rate 0.86042 is above baseline 0.86011, so coverage
improved rather than regressed and the gate passes without needing the tolerance.

## Output Summary

All six clauses pass. Per-file uncovered counts held or improved on all eight production files;
all 38 added executable lines are covered; the new `Run` method is at line rate 1; the `EtlAsync`
span rose from 0.7000 to 0.9796; and repository-wide line coverage rose from 0.860109 to 0.860424
on a denominator that moved 0.12 percent (COMPARABILITY: A, gate satisfied).
