# P9-T7 — C# QA step 4, MSTest with coverage (iteration 1)

Timestamp: 2026-09-20T09-44

Command: CMD-MSTEST-COVERAGE.

```
pwsh -NoProfile -Command 'Set-Location "C:\Users\DanMoisan\repos\TaskMaster-wt\dependabot-911"; & "C:\Users\DanMoisan\repos\TaskMaster-wt\dependabot-911\scripts\vscode\Invoke-MSTestWithCoverage.ps1" -SearchRoot .'
```

Absolute script path with `Set-Location`, per gate rule 16. `-SearchRoot .` is mandatory: the
script's single-search-root defect otherwise discovers assemblies from a sibling worktree. The script
always appends `/TestCaseFilter:TestCategory!=LiveOutlook`, so every figure below excludes that
category, and it enforces its own floors of 0.80 line and 0.75 branch.

EXIT_CODE: 0

## Test counts

| Count | Value |
|---|---|
| Total | 7343 |
| Passed | **7343** |
| Failed | **0** |
| Skipped | 0 |

The runner prints no `Failed:` or `Skipped:` line on a successful run, so those two figures are read
from the test-result summary the run produced, which states them explicitly:
`Total 7343, executed 7343, passed 7343, failed 0.` and
`Skipped 0, derived as total minus executed rather than reported by the test platform.`

Total run time 31.6302 seconds. `Test Run Successful.`

## Numeric coverage

One-line first-party coverage report, quoted verbatim as the runner printed it:

```
First-party coverage: lines 56476/65737 (85.91%), branches 13654/17052 (80.07%)
```

| Metric | Covered | Total | Percentage | Fractional | Runner floor |
|---|---|---|---|---|---|
| Line | 56476 | 65737 | **85.91%** | 0.8591 | 0.80 |
| Branch | 13654 | 17052 | **80.07%** | 0.8007 | 0.75 |

Both clear the runner's own floors, which is why the run exited 0: the script fails the run itself
when either floor is breached.

## Permitted evidence forms copied into the evidence tree

Gate rule 12 prohibits a raw collector document under the evidence tree and requires the permitted
forms to be copied instead. Both permitted forms were produced on this run and both were copied.

### Coverage projection — mandatory, produced

| | Path |
|---|---|
| Source, printed by the run as `Coverage projection:` | `coverage/coverage.cobertura.jacoco.xml` |
| Destination | `docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/p9-t7-coverage-projection.2026-09-19T09-44.jacoco.xml` |

38 lines. It carries no absolute host path: a search for the host account name returns 0 matches.
`.csharpierignore` line 4 excludes `**/evidence/**`, so the copy does not reach the formatter.

### Test-result summary — produced, therefore mandatory

The run printed `Test-result summary: <path>`, so the copy is mandatory and its absence from the
P9-T13 commit would be a failure.

| | Path |
|---|---|
| Source, printed by the run as `Test-result summary:` | `coverage/test-results/mstest-coverage-run.summary.txt` |
| Destination | `docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/evidence/qa-gates/p9-t7-test-results.2026-09-19T09-44.summary.txt` |

TEST-RESULT-SUMMARY: produced

Contents, verbatim:

```
Test run outcome: Completed
Total 7343, executed 7343, passed 7343, failed 0.
Skipped 0, derived as total minus executed rather than reported by the test platform.
Figures reported verbatim by the test platform: error 0, timeout 0, aborted 0, notExecuted 0, inconclusive 0.
Failed tests: none
```

It carries no absolute host path and no test-name payload that would need sanitising, because no test
failed.

### Raw collector document — left where it was written

`coverage/coverage.cobertura.xml` is the raw post-processed Cobertura document and is **not** copied
into the evidence tree. It stays under `coverage/`, which `.gitignore:144` covers.

## Acceptance

| Clause | Required | Measured | Result |
|---|---|---|---|
| `EXIT_CODE` | 0 | 0 | PASS |
| Numeric line-coverage percentage recorded | yes | 85.91% | PASS |
| Numeric branch-coverage percentage recorded | yes | 80.07% | PASS |
| Passed, failed and skipped counts recorded | yes | 7343, 0, 0 | PASS |
| Failed count is 0 | 0 | 0 | PASS |
| Passed count greater than zero | greater than 0 | 7343 | PASS |
| Coverage projection copied | mandatory | copied, 38 lines | PASS |
| Test-result summary copied when the line appears | mandatory when produced | produced and copied | PASS |
| One-line first-party coverage report quoted verbatim | yes | quoted above | PASS |

The projection is the delivered tree's committed coverage evidence and is what a reviewer checks the
P9-T9 figures against.

## Environment observation, recorded because it did not occur

The two preceding solution-wide `/m` rebuilds left 17 idle MSBuild node-reuse workers running when
this task began. Those workers are known to hold the solution file open and to fail a repo-wide
run's `FileInfoWrapper` `OpenRead` test. No such failure occurred here: all 7343 tests passed. The
workers were **not** terminated, because a node-reuse worker's command line does not name the
worktree that started it and killing by process name would reach a sibling session's workers.
