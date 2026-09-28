# Fail-before — runsettings mocked document literals (P2-T4)

Timestamp: 2026-09-14T18-33

Expected Result: RED

State of the tree when this observation was taken: immediately after P2-T2 wired the branch assertion at line 387 of `scripts/vscode/Invoke-MSTestWithCoverage.ps1`, and immediately before this task's edit to the three mocked document literals.

Command: `pwsh -NoProfile -Command '<worktree prologue>; Import-Module Pester -MinimumVersion 5.0.0 -Force; $c = New-PesterConfiguration; $c.Run.Path = "tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1"; $c.Run.PassThru = $true; $r = Invoke-Pester -Configuration $c; "PESTER Passed=" + $r.PassedCount + " Failed=" + $r.FailedCount + " Total=" + $r.TotalCount'`
EXIT_CODE: 0

## Printed result line, verbatim

```
PESTER Passed=25 Failed=3 Total=28
```

Failed count: **3**, exactly the value this task predicted.

## The three failures, matched to the cases the plan names

```
FAILED: collects and post-processes coverage on the fully mocked main happy path || Cobertura branch-rate is missing.
FAILED: passes the generated Cobertura result to the threshold evaluator before completing successfully || Cobertura branch-rate is missing.
FAILED: excludes assemblies discovered under a .claude worktree segment || Cobertura branch-rate is missing.
```

Those three names are the `It` blocks opening at lines 404, 413 and 436 of the test file respectively, which are exactly the three cases the plan identifies as reaching the assertion site. Each failed on the message `Cobertura branch-rate is missing.`, thrown by the newly wired `Assert-CoberturaBranchCoverageThreshold`.

## Reachability, confirmed by the observed result

The plan's reachability analysis is confirmed by which cases did **not** fail. The `Describe` named `Invoke-MSTestWithCoverageMain` opens at line 351 and holds exactly seven `It` blocks, opening at lines 386, 398, 404, 413, 421, 428 and 436.

- The cases at lines 386 and 398 pass `-NoExecute` and take the entry point's early return at its lines 363 to 365, well above the assertion site, so neither reaches it. Both passed.
- The case at line 421 mocks the path test to report absent and throws at the entry point's search-root check at its lines 308 to 310, so it does not reach the site. It passed.
- The case at line 428 mocks a line rate of 0.5 and therefore throws at the line assertion, one statement before the branch assertion, so its `Should -Throw` is already satisfied. It passed.
- Only the cases at lines 404, 413 and 436 reach the site, and exactly those three failed.

## Substitution sites, re-derived before the edit

A search for the current literal `coverage line-rate="0.8" lines-covered="4" lines-valid="5"` in this file returned exactly three matches, at lines 382, 416 and 418. Those are precisely the sites the plan names: line 382 is the `Describe`-level `BeforeEach` mock governing all seven cases, line 416 is a case-level `ConvertTo-KoverageCoberturaXml` override inside the `It` that opens at line 413, and line 418 is that same case's `Should -Be` expectation, which must change together with line 416 or the case breaks on a mismatch. No fourth occurrence exists, so no other line in the file is touched.

The literal `<coverage line-rate="0.5" />` at line 429 is deliberately not altered: that case's mocked line rate makes the existing line assertion throw first, so its `Should -Throw` expectation is already satisfied and adding a passing rate there would invert it. The observed pass of the case at line 428 confirms that.

## Conclusion

This task's acceptance — a failed count of 0 — is proven to have been false immediately before the edit.
