# [P0-T13] Baseline Coverage Scope Extraction

Timestamp: 2026-08-26T09-00

Task: [P0-T13]
Feature: docs/features/active/quickfiler-bug-family-446

## Command

```
pwsh -NoProfile -Command '$x=[xml](Get-Content -Raw "docs/features/active/quickfiler-bug-family-446/evidence/baseline/coverage-baseline.cobertura.xml"); $all=$x.SelectNodes("//class"); foreach ($n in @("QfcStreamingDequeueConfidenceGate.cs","QfcFormController.Actions.cs","QfcHomeController.Iteration.cs","QfcStreamingDequeueConfidenceGate","QfcFormController","QfcHomeController")) { $s=@($all | Where-Object { $_.filename -like ("*" + $n + "*") }); $ln=@($s.lines.line); $h=@($ln | Where-Object { [int]$_.hits -gt 0 }); "{0} lines={1} covered={2} rate={3:N2}" -f $n,$ln.Count,$h.Count,(100*$h.Count/[math]::Max(1,$ln.Count)) }'
```

Execution note: the payload above is the plan's aggregation command verbatim. It was executed
through `pwsh -NoProfile` from a scratchpad script file rather than as a `-Command` string,
because the intermediate shell collapses the doubled backslashes the payload's regular
expressions rely on. The script body is byte-equivalent to the payload; no scratchpad script is
retained under the `evidence/` tree.

EXIT_CODE: 0

## Six Recorded Rows

### Changed-file scope (blocking `>= 90.00` condition of AC28 per D-Plan-7)

| Needle | lines | covered | rate |
| --- | --- | --- | --- |
| `QfcStreamingDequeueConfidenceGate.cs` | 93 | 90 | 96.77 |
| `QfcFormController.Actions.cs` | 204 | 73 | 35.78 |
| `QfcHomeController.Iteration.cs` | 56 | 45 | 80.36 |

### Whole-type scope (no-regression condition)

| Needle | lines | covered | rate |
| --- | --- | --- | --- |
| `QfcStreamingDequeueConfidenceGate` | 93 | 90 | 96.77 |
| `QfcFormController` | 699 | 363 | 51.93 |
| `QfcHomeController` | 445 | 304 | 68.31 |

Every row's `lines=` value is a positive integer, so no needle matched zero `<class>` elements
and no row is a measurement failure.

## Raw Output

```
QfcStreamingDequeueConfidenceGate.cs lines=93 covered=90 rate=96.77
QfcFormController.Actions.cs lines=204 covered=73 rate=35.78
QfcHomeController.Iteration.cs lines=56 covered=45 rate=80.36
QfcStreamingDequeueConfidenceGate lines=93 covered=90 rate=96.77
QfcFormController lines=699 covered=363 rate=51.93
QfcHomeController lines=445 covered=304 rate=68.31
```

## Observations Recorded for Later Phases

1. **Divergence from the figures quoted in D-Plan-7.** D-Plan-7 quotes
   `QfcStreamingDequeueConfidenceGate.cs` at `lines=136 covered=132 rate=97.06` and
   `QfcHomeController.Iteration.cs` at `lines=80 covered=69 rate=86.25`, measured before this plan
   was cleared against "the most recent QuickFiler Cobertura artifact on `main`". The baseline
   measured here is taken from this worktree's own run against the epic integration tree
   (`61edc19b`) with the repository's `coverage.config` instrumentation excludes, so the
   denominators differ. Per the plan, every later `<mb>` coverage comparison reads the value
   recorded by **this** task, not a value pinned in the plan document.
2. **`QfcHomeController.Iteration.cs` uncovered set.** 56 measured lines, 45 covered, so 11 are
   uncovered — exactly the eleven occurrences D-Plan-7 attributes to the two `catch` blocks at
   `QuickFiler/Controllers/QfcHomeController.Iteration.cs:38-52`. `[P1-T19]` drives those
   branches and is what makes the `[P5-T7]` `>= 90.00` gate on this file reachable.
3. **`QfcFormController.Actions.cs` at 35.78.** This is the file the `[P5-T7]` carve-out covers:
   its changed-file gate is satisfied either by reaching `>= 90.00` or by being at or above this
   `35.78` baseline with the verbatim `REMEDIATION-REQUIRED` line recorded.
4. **AC28 whole-type reading.** Two of the three whole-type rows (`QfcFormController` 51.93 and
   `QfcHomeController` 68.31) are below `90.00` at baseline. Per D-Plan-7 and `[P5-T17]`, the
   AC28 checkbox is checked only when every whole-type figure reaches `90.00`; otherwise the box
   stays unchecked and the AC28/AC18 conflict is recorded rather than silently resolved.

## Output Summary

Six coverage rows extracted from the Phase 0 Cobertura baseline, each with a positive integer
`lines=`, an integer `covered=` and a two-decimal `rate=`. Changed-file scope: 96.77 / 35.78 /
80.36. Whole-type scope: 96.77 / 51.93 / 68.31.
