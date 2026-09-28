# Fail-before — threshold call-site wiring (P1-T2)

Timestamp: 2026-09-14T18-27

Expected Result: RED

Command: `pwsh -NoProfile -Command '<worktree prologue>; Import-Module Pester -MinimumVersion 5.0.0 -Force; $c = New-PesterConfiguration; $c.Run.Path = "tests/scripts/vscode/Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1"; $c.Run.PassThru = $true; $c.Output.Verbosity = "Detailed"; $r = Invoke-Pester -Configuration $c; "PESTER Passed=" + $r.PassedCount + " Failed=" + $r.FailedCount + " Total=" + $r.TotalCount'`
EXIT_CODE: 0

The exit code is not the red signal; Pester does not exit non-zero on a failing test case. The red signal is the recorded failed count.

## Printed result line, verbatim

```
PESTER Passed=3 Failed=2 Total=5
```

Failed count: **2**, exactly as required.

## What changed in the test file

`tests/scripts/vscode/Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1` gained a new `Describe` block named `Invoke-MSTestWithCoverage threshold call-site wiring` holding exactly two test cases, named `invokes the branch assertion once with the post-processed document` and `invokes the line assertion before the branch assertion`.

Both drive `Invoke-MSTestWithCoverageMain` with a mock set mirroring the one this file already establishes for the discovery block: `Resolve-Path`, `Test-Path`, `Resolve-RunSettingsPath`, `Invoke-VsWhereExe`, `Get-Command`, `Get-ChildItem`, `Invoke-DotnetCoverageCollection`, `Get-Content`, `ConvertTo-KoverageCoberturaXml` and `Set-Content`. Both mock the two threshold functions so the invocation order and the received argument are captured into script-scope variables, and each case asserts that each threshold function is invoked exactly once. The first case additionally asserts the captured branch argument is the same post-processed string the mocked conversion returned; the second additionally asserts the recorded call order is `line` then `branch`, and that the captured line argument is that same string.

Both `Mock` statements for the threshold functions are placed inside the body of each `It`, and not in a `BeforeAll` or a `BeforeEach` block. This placement is load-bearing for this task's own acceptance: a mock of a command that does not yet exist, evaluated in a block-level setup, can make Pester report a single block-setup error instead of two discrete failing cases, and the failed count of exactly 2 that this task requires would then not be met even though the fail-before condition genuinely holds. The observed result confirms the placement worked: Pester reported two discrete failing cases rather than one block-setup error.

The new `Describe` is placed after the existing `Describe` block, at the end of the file, so that line 55, which P2-T3 cites as its substitution site in this file, is unchanged. That line still carries the `Mock ConvertTo-KoverageCoberturaXml` statement inside the `BeforeEach` of the `Describe` named `Invoke-MSTestWithCoverage assembly discovery`.

## Result breakdown

The three passing cases are the pre-existing assembly-discovery cases, which are unaffected by this addition.

Both failures name the unresolved command, verbatim:

```
CommandNotFoundException: Could not find Command Assert-CoberturaBranchCoverageThreshold
```

That message confirms the red is caused by the absent production function rather than by a malformed fixture or a wrong expectation. The absence is independently established by the P0-T15 artifact, which records a zero match count for that identifier across `scripts/vscode` and `tests/scripts/vscode`.

Output Summary: RED as expected. 5 tests discovered, 3 passed, 2 failed, 0 skipped. Both failures name the unresolved command `Assert-CoberturaBranchCoverageThreshold`. This is the fail-before evidence for the unwired call site; P2-T2 wires the call and P2-T3 records the pass-after count for this file.
