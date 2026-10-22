# Fail-before — assembly-discovery mocked document (P2-T3)

Timestamp: 2026-09-14T18-31

Expected Result: RED

State of the tree when this observation was taken: immediately after P2-T2 wired the branch assertion at line 387 of `scripts/vscode/Invoke-MSTestWithCoverage.ps1`, and immediately before this task's edit to the mocked document literal.

Command: `pwsh -NoProfile -Command '<worktree prologue>; Import-Module Pester -MinimumVersion 5.0.0 -Force; $c = New-PesterConfiguration; $c.Run.Path = "tests/scripts/vscode/Invoke-MSTestWithCoverage.AssemblyDiscovery.Tests.ps1"; $c.Run.PassThru = $true; $r = Invoke-Pester -Configuration $c; "PESTER Passed=" + $r.PassedCount + " Failed=" + $r.FailedCount + " Total=" + $r.TotalCount'`
EXIT_CODE: 0

## Printed result line, verbatim

```
PESTER Passed=2 Failed=3 Total=5
```

Failed count: **3**. Passed count: **2**. Both match the values this task predicted before the run.

## The three failures

Each of the three pre-existing assembly-discovery cases, opening at lines 59, 74 and 90 of the test file, failed with the identical message:

```
FAILED: includes an assembly directly beneath a search root that is itself under a .claude worktree segment || Cobertura branch-rate is missing.
FAILED: excludes a nested sibling worktree beneath a non-dot-claude search root || Cobertura branch-rate is missing.
FAILED: retains the root-level assembly and excludes a further-nested worktree beneath a dot-claude search root || Cobertura branch-rate is missing.
```

`Cobertura branch-rate is missing.` is the first rejection message of the newly wired `Assert-CoberturaBranchCoverageThreshold`, which the three cases reach because they consume the single `Mock ConvertTo-KoverageCoberturaXml` statement in the `BeforeEach` block at line 55 and that mock's returned literal carries no `branch-rate` attribute. That single setup mock is the only source of the post-processed document for all three cases.

## The two passes

The two passing cases are the wiring cases added in P1-T2, `invokes the branch assertion once with the post-processed document` and `invokes the line assertion before the branch assertion`. They pass because they mock the branch assertion inside their own `It` bodies and therefore never reach the real one.

## Conclusion

This task's acceptance — a failed count of 0 and a passed count of exactly 5 — is proven to have been false immediately before the edit. The recorded observation is the fail-before half of the evidence; the pass-after half is the same command's result after the substitution, recorded in this task's check-off and re-confirmed by the whole-suite run in P2-T7.
