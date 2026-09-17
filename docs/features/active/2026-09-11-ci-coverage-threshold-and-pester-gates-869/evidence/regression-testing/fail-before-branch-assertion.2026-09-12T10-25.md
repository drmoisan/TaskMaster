# Fail-before — C# branch assertion (P1-T1)

Timestamp: 2026-09-14T18-22

Expected Result: RED

Command: `pwsh -NoProfile -Command '<worktree prologue>; Import-Module Pester -MinimumVersion 5.0.0 -Force; $c = New-PesterConfiguration; $c.Run.Path = "tests/scripts/vscode/Invoke-MSTestWithCoverage.Threshold.Tests.ps1"; $c.Run.PassThru = $true; $c.Output.Verbosity = "Detailed"; $r = Invoke-Pester -Configuration $c; "PESTER Passed=" + $r.PassedCount + " Failed=" + $r.FailedCount + " Total=" + $r.TotalCount'`
EXIT_CODE: 0

The exit code is **not** the red signal. Pester does not exit non-zero on a failing test case, so a zero exit code here carries no information about the result. The red signal is the recorded failed count below.

## Printed result line, verbatim

```
PESTER Passed=6 Failed=7 Total=13
```

Failed count: **7**, exactly as required.

## What changed in the test file

`tests/scripts/vscode/Invoke-MSTestWithCoverage.Threshold.Tests.ps1` gained:

- one further test case inside the existing `Describe 'Assert-CoberturaLineCoverageThreshold'` block, named `rejects a Cobertura line rate outside the closed unit interval`;
- a new `Describe 'Assert-CoberturaBranchCoverageThreshold'` block holding exactly seven test cases, named `accepts a branch rate at exactly the floor`, `accepts a branch rate above the floor`, `rejects a branch rate below the floor`, `rejects a missing branch rate`, `rejects a non-numeric branch rate`, `rejects a branch rate outside the closed unit interval`, and `rejects a projection with zero valid branches`.

Each case drives the function with a one-line document literal, matching the technique the existing cases in this file already use. The four non-threshold negative cases assert the exact messages `Cobertura branch-rate is missing.`, `Cobertura branch-rate must be numeric.`, `Cobertura branch-rate must be between 0 and 1.` and `Cobertura branch coverage has no valid branches.`; the below-floor case asserts a message matching the token `is below the required 75`.

The file moved from 15 lines to 26 lines and stays well under 500.

## Result breakdown

The six passing cases are the five pre-existing line cases plus the newly added out-of-range line case. The out-of-range line case passes immediately because the existing `Assert-CoberturaLineCoverageThreshold` already implements that rejection at lines 47 to 49 of `scripts/vscode/Invoke-MSTestWithCoverage.Threshold.ps1`; it closes a scenario gap in the test file rather than driving new production code, so it is correctly green on this run.

All seven failures are in the new `Describe` block. Each failure text names the unresolved command, verbatim:

```
The term 'Assert-CoberturaBranchCoverageThreshold' is not recognized as a name of a cmdlet, function, script file, or executable program.
```

That message is present in all seven failure records. It confirms the red is caused by the absent production function and not by a malformed fixture or a wrong expectation, which is exactly the fail-before condition this task records. The absence is independently established by the P0-T15 artifact, which records a zero match count for that identifier across `scripts/vscode` and `tests/scripts/vscode`.

Output Summary: RED as expected. 13 tests discovered, 6 passed, 7 failed, 0 skipped. All seven failures name the unresolved command `Assert-CoberturaBranchCoverageThreshold`. This is the fail-before evidence for the missing branch gate; P2-T1 adds the function and P2-T7 records the pass-after count.
