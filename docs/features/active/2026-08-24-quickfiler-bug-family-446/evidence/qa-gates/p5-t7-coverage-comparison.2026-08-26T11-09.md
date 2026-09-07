# [P5-T7] Coverage Comparison Against the Phase 0 Baseline

Timestamp: 2026-08-26T11-09

Task: [P5-T7]
Feature: docs/features/active/quickfiler-bug-family-446

## Command

The aggregation is byte-for-byte the command `[P0-T13]` used, retargeted only at the post-change
Cobertura artifact:

```
pwsh -NoProfile -Command '$x=[xml](Get-Content -Raw "docs/features/active/quickfiler-bug-family-446/evidence/qa-gates/coverage-final.cobertura.xml"); $all=$x.SelectNodes("//class"); foreach ($n in @("QfcStreamingDequeueConfidenceGate.cs","QfcFormController.Actions.cs","QfcHomeController.Iteration.cs","QfcStreamingDequeueConfidenceGate","QfcFormController","QfcHomeController")) { $s=@($all | Where-Object { $_.filename -like ("*" + $n + "*") }); $ln=@($s.lines.line); $h=@($ln | Where-Object { [int]$_.hits -gt 0 }); "{0} lines={1} covered={2} rate={3:N2}" -f $n,$ln.Count,$h.Count,(100*$h.Count/[math]::Max(1,$ln.Count)) }'
```

EXIT_CODE: 0

Execution note: as in `[P0-T13]`, the payload was executed through `pwsh -NoProfile` from a
scratchpad script file outside the repository rather than as a `-Command` string, because the
intermediate shell collapses doubled backslashes. The script body is byte-equivalent to the
payload and no helper script is retained anywhere under the `evidence/` tree.

Per D-Plan-7 the aggregation matches every `<class>` element whose `filename` contains the needle,
never a single named `<class>`, because an `async` method compiles into a nested state machine
that Cobertura emits as its own `<class>` element.

## Raw output

```
QfcStreamingDequeueConfidenceGate.cs lines=115 covered=112 rate=97.39
QfcFormController.Actions.cs lines=213 covered=102 rate=47.89
QfcHomeController.Iteration.cs lines=60 covered=60 rate=100.00
QfcStreamingDequeueConfidenceGate lines=115 covered=112 rate=97.39
QfcFormController lines=708 covered=392 rate=55.37
QfcHomeController lines=449 covered=319 rate=71.05
```

## Repository-wide line rate (recorded and reported; NOT a blocking threshold)

| scope | baseline | post-change |
| --- | --- | --- |
| repository-wide `line-rate` | `0.847782` (84.7782%) | `0.848402` (84.8402%) |

**Denominator statement.** Both figures are the **unfiltered repository-wide** rate read from the
Cobertura root `<coverage>` element, covering every instrumented package including vendored code.
They are not the filtered first-party denominator, and the two differ materially in this
repository. AC28 makes this figure explicitly record-and-report; no threshold is applied to it
here. No merge-base baseline exists for this feature folder other than the `[P0-T12]` run recorded
in Phase 0, which is what the baseline column cites.

## Changed-file scope (three rows) - carries the blocking 90.00 condition

**Denominator statement.** Each row denominator is the count of distinct `<line>` elements
aggregated across every `<class>` element whose `filename` attribute matches that single changed
file. This is the changed-file scope of D-Plan-7, not the repository-wide denominator above and
not the whole-type denominator below.

| changed file | baseline lines/covered/rate | post-change lines/covered/rate | at least 90.00? |
| --- | --- | --- | --- |
| `QuickFiler/Controllers/QfcStreamingDequeueConfidenceGate.cs` | 93 / 90 / `96.77` | 115 / 112 / `97.39` | yes |
| `QuickFiler/Controllers/QfcFormController.Actions.cs` | 204 / 73 / `35.78` | 213 / 102 / `47.89` | no - carve-out below |
| `QuickFiler/Controllers/QfcHomeController.Iteration.cs` | 56 / 45 / `80.36` | 60 / 60 / `100.00` | yes |

`QfcHomeController.Iteration.cs` moved from `80.36` to `100.00`. The eleven previously uncovered
occurrences were the two `catch` blocks at
`QuickFiler/Controllers/QfcHomeController.Iteration.cs:38-52`, which `[P1-T19]` was written to
drive; they are now covered.

### QfcFormController.Actions.cs carve-out

Its post-change rate `47.89` is below `90.00` but is greater than its `[P0-T13]` baseline value of
`35.78`, an increase of 12.11 percentage points, which is the alternative condition this task
permits. The required line follows verbatim.

REMEDIATION-REQUIRED: QfcFormController.Actions.cs changed-file coverage is bounded by the un-seamed MessageBox.Show calls in UndoDialog at :225, :238 and :248, which UT4 prohibits exercising and which no task in this plan seams

## Whole-type scope (three rows) - carries the no-regression condition

**Denominator statement.** Each row denominator is the count of distinct `<line>` elements
aggregated across every `<class>` element whose `filename` matches the type name, which for the
two partial types spans partial files owned by sibling epic children and not written by this
change set. This scope is held to no-regression only, per D-Plan-7.

| type | baseline lines/covered/rate | post-change lines/covered/rate | at or above baseline? | at least 90.00? |
| --- | --- | --- | --- | --- |
| `QfcStreamingDequeueConfidenceGate` | 93 / 90 / `96.77` | 115 / 112 / `97.39` | yes (+0.62) | yes |
| `QfcFormController` | 699 / 363 / `51.93` | 708 / 392 / `55.37` | yes (+3.44) | no |
| `QfcHomeController` | 445 / 304 / `68.31` | 449 / 319 / `71.05` | yes (+2.74) | no |

All three whole-type rates are at or above their recorded baselines, so the no-regression
condition holds. Two of the three are below `90.00`, which is the outcome `[P5-T17]` evaluates
against the literal whole-type wording of AC28.

## Acceptance conditions of this task

- `QfcStreamingDequeueConfidenceGate.cs` changed-file rate `97.39` is at least `90.00`. Satisfied.
- `QfcHomeController.Iteration.cs` changed-file rate `100.00` is at least `90.00`. Satisfied.
- `QfcFormController.Actions.cs` changed-file rate `47.89` is greater than or equal to its
  `[P0-T13]` baseline `35.78`, and this artifact carries the required line verbatim. Satisfied on
  the alternative branch.
- Each of the three whole-type line rates is greater than or equal to its recorded baseline value.
  Satisfied.
- All eight figures, being two repository-wide, three changed-file and three whole-type, are
  numeric. Satisfied.
- Every aggregation row `lines=` value is a positive integer (115, 213, 60, 115, 708, 449), so no
  needle matched zero `<class>` elements and no row is a silent measurement failure. Satisfied.

## Excluded types (recorded for completeness, per D-Plan-7)

Coverage credit cannot accrue to two types in the neighbourhood of this change set because they
carry `[ExcludeFromCodeCoverage]`:

- `QfcDatamodel`, excluded at `QuickFiler/Controllers/QfcDatamodel.cs:25`. The attribute is
  type-level on one partial declaration, so it also covers
  `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs`.
- `FolderScoringService`, excluded at `QuickFiler/Controllers/QfcHighConfidencePreFilter.cs:166`.

## Output Summary

Eight numeric coverage figures recorded. Changed-file scope `97.39` / `47.89` / `100.00`;
whole-type scope `97.39` / `55.37` / `71.05`; repository-wide `84.7782%` baseline to `84.8402%`
post-change. Both unconditional changed-file gates pass; `QfcFormController.Actions.cs` completes
on its documented carve-out branch, rising from `35.78` to `47.89`. All three whole-type rates are
at or above baseline, so there is no regression on any measured scope.
