# P5-T5 — AC22 red-before control: the AC21 case failing against the pass-through tree

Timestamp: 2026-09-19T09-44

Command:

```
pwsh -NoProfile -Command 'Set-Location "<execution-worktree-root>"; Import-Module Pester -RequiredVersion 5.6.1; $c = New-PesterConfiguration; $c.Run.Path = @("tests/scripts/dependencies/ProjectConsistency.Tests.ps1"); $c.Filter.FullName = "*AC21-*"; $c.Run.PassThru = $true; $c.Output.Verbosity = "Detailed"; $c.CodeCoverage.Enabled = $true; $c.CodeCoverage.Path = @("scripts/dependencies","scripts/vscode"); $c.CodeCoverage.OutputFormat = "JaCoCo"; $c.CodeCoverage.OutputPath = "coverage/p5-t5-ac21-red-coverage.xml"; $r = Invoke-Pester -Configuration $c; "PESTER Passed=$($r.PassedCount) Failed=$($r.FailedCount) Skipped=$($r.SkippedCount) Total=$($r.TotalCount)"; if ($r.FailedCount -gt 0) { exit 1 } else { exit 0 }'
```

EXIT_CODE: 1

ExpectedExitCode: 1

## Output Summary

```
PESTER Passed=0 Failed=1 Skipped=0 Total=13
EXECUTED=1
NOTRUN=12
```

The failing run is the expected outcome of this task. The exit code was confirmed
separately by re-running the same configuration with `Output.Verbosity = "None"` and
reading the process exit status, which returned 1.

## Failing case and verbatim failure message

```
FAILED-NAME: Project consistency reconciliation and verification.Three-way divergence from pull request 908.AC21- reports separate guard and analyzer disagreements before repair and reconciles all three locations after
FAILED-MESSAGE: Expected the actual value to be greater than 0, because the Import and Error guards disagree with the manifest, but got 0.
```

## The red is behavioural, not structural

The failure is an assertion failure on the first assertion of the case: the pass-through
`Find-VersionDisagreement` returns an empty finding set, so the guard-disagreement count is
0 where the case requires it to be greater than 0. It is **not** a missing module and
**not** a missing command. Both conditions were positively excluded before the run:

- `Import-Module` of `ProjectConsistency.psm1` and `ConsistencyVerifier.psm1` succeeded at
  P5-T1 and P5-T2 with `-ErrorAction Stop`, and the suite's own `BeforeAll` imports both;
  a failed import would have produced a container-level error rather than one failed test.
- `Get-Command -Module ConsistencyVerifier` listed all nine cited functions at P5-T2 and
  `Get-Command -Module ProjectConsistency` listed both at P5-T1, so every command the case
  invokes resolves.

A failure message naming a missing module or a missing command would not have been an
acceptable red, and none was produced.

## Discrepancy: the emitted `Total` is the discovered population, not the filtered one

The task's acceptance states `Total` is exactly 1, as the non-vacuity guard against a
filter that matched no test and against a filter that over-matched. The `Total` that
CMD-PESTER-ALL emits is `$r.TotalCount`, and **Pester 5.6.1 counts filtered-out tests in
`TotalCount` as `NotRun`**: the run reports `Total=13`, which is the count of `It` blocks
discovered in the file, and is identical whatever the filter selects. Measured here:
`Total=13`, `NotRun=12`, `Passed + Failed + Skipped = 1`.

The consequence is that `TotalCount` cannot detect either condition the clause is written
to detect. It is invariant under the filter, so an over-matching filter leaves it at 13 and
a filter matching nothing also leaves it at 13. The quantity that does detect both is the
executed population, `Passed + Failed + Skipped`, which is 0 when the filter matches
nothing and greater than 1 when it over-matches.

This artifact therefore records both figures and evaluates the exact-1 clause against the
**executed population**, which is exactly 1. That is the reading the clause's own stated
rationale defines, and it is stricter than the emitted figure rather than weaker: `Total=13`
would satisfy no exact assertion at all and would have had to be waived.

The same reading applies at P5-T13, P5-T14, P5-T15, P5-T16, P5-T17, P5-T18, P5-T19 and
P5-T20, which are the remaining filtered runs in this phase. It did not surface earlier
because P3-T9's filtered run was over a file whose entire discovered population was the
filtered one, and P3-T10's clause was a lower bound that the discovered population also
satisfied.

Recorded as a plan discrepancy and reported to the coordinator; no plan text was edited.

## Standing-in statement is not required of this artifact

This artifact records no JaCoCo LINE figure. Gate rule 12's standing-in obligation falls on
the six tasks that record one — P0-T18, P1-T6, P2-T3, P4-T3, P6-T3 and P9-T3 — and this is
not one of them. The JaCoCo document the run produced is at
`coverage/p5-t5-ac21-red-coverage.xml`, which `.gitignore:144` covers; no collector document
is written under the evidence tree.
