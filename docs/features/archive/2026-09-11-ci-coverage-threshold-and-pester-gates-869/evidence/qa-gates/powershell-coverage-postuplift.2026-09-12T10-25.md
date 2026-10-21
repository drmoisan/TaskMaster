# Post-uplift PowerShell coverage (P6-T4)

Timestamp: 2026-09-14T19-56

Command: the same directory-scoped Pester command as P0-T8, namely `pwsh -NoProfile -Command '<worktree prologue>; Import-Module Pester -MinimumVersion 5.0.0 -Force; $c = New-PesterConfiguration; $c.Run.Path = "tests/scripts/vscode"; $c.Run.PassThru = $true; $c.Output.Verbosity = "Detailed"; $c.CodeCoverage.Enabled = $true; $c.CodeCoverage.Path = "scripts/vscode"; $c.CodeCoverage.OutputFormat = "JaCoCo"; $c.CodeCoverage.OutputPath = "coverage/pester-coverage.xml"; $r = Invoke-Pester -Configuration $c; ...'`
EXIT_CODE: 0

The report-level LINE counter was then read from `coverage/pester-coverage.xml` with the selector recorded in the P0-T9 artifact, unchanged:

```
@($j.report.counter) | Where-Object { $_.type -eq "LINE" }
```

## Printed result lines, verbatim

```
PESTER Passed=174 Failed=0 Skipped=0 Total=174
PESTER COMMANDPERCENT=84.7195357833656
LINE covered=731 missed=140
```

## The four test counts

- Passed: 174
- Failed: 0
- Skipped: 0
- Total: 174

The total of 174 is one greater than the 173 recorded in the P5-T6 artifact, which is the single case P6-T1 added.

## LINE figure — the figure the gate asserts

- LINE covered: **731**
- LINE missed: **140**
- LINE total: **871**
- **LINE percentage: 83.93** (to two decimal places)

This is the LINE figure. It is the figure the Pester gate asserts and the figure the #563 decision's PowerShell floor of 80 is about.

## Command figure — informational only

- `PESTER COMMANDPERCENT` = **84.7195357833656**

This is Pester's command (instruction) figure. It carries no threshold and is recorded for information only, per settled decision D4 of the specification and the PowerShell rules file.

## Delta against the baseline and against the required uplift

| Quantity | Baseline (P0-T9 / P0-T10) | Post-uplift (this run) | Change |
| --- | --- | --- | --- |
| LINE covered | 662 | 731 | **+69** |
| LINE missed | 177 | 140 | −37 |
| LINE total | 839 | 871 | +32 |
| LINE percentage | 78.90 | 83.93 | +5.03 points |

Required delta N from the P0-T10 artifact: **10**.
Measured delta in covered lines: **69**.

69 is greater than or equal to 10, so the required uplift is met with a wide margin. The total denominator grew by 32 because this delivery added production code — the branch assertion, the two extracted main functions and the five wrapper seams — and that new code enters both the numerator and the denominator; the measured covered count grew by more than the denominator did, which is why the percentage rose.

Independent check against the floor on the current denominator: the ceiling of 0.80 times 871 is **697**, and the measured covered count is **731**, so the suite is **34 covered lines above the floor**. That margin is the tolerance figure the P8-T3 negative-path proof records.

No branch figure is recorded for PowerShell. Pester measures no branch coverage, and settled decision D4 forbids introducing a PowerShell branch assertion, threshold or reported figure.

Output Summary: post-uplift LINE coverage over `scripts/vscode` is 731 covered of 871, which is 83.93 percent, above the 80 floor by 34 covered lines. The suite reports 174 passed, 0 failed, 0 skipped. The informational command figure is 84.72 percent. The covered count rose by 69 against a required delta of 10.
