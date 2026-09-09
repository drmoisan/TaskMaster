# P1-T2 — Fail-Before Evidence (expect-fail)

Timestamp: 2026-09-09T10-56
Task: [P1-T2] [expect-fail]
Command: `pwsh -NoProfile -Command '$r = Invoke-Pester -Path "tests/scripts/vscode/Invoke-MSTestWithCoverage.FirstParty.Tests.ps1" -PassThru; Write-Output ("PASSED=" + $r.PassedCount + " FAILED=" + $r.FailedCount)'`
EXIT_CODE: 0

No `ExpectedExitCode` field is written for this task, deliberately. `Invoke-Pester` returns exit
code 0 on a failing `It` block unless `Run.Exit` is enabled, so a non-zero expectation here would be
unsatisfiable. **The red result, not the exit code, is the fail-before evidence.**

## Result

```
Starting discovery in 1 files.
Discovery found 7 tests in 112ms.
Tests completed in 586ms
Tests Passed: 0, Failed: 7, Skipped: 0, Inconclusive: 0, NotRun: 0
PASSED=0 FAILED=7
```

## Failure text, per test

Six of the seven failures name `Get-CoberturaFirstPartyCoverageSummary` as an unresolved command;
the seventh names `Get-CoberturaFirstPartyCoverageReport`.

| Test | Failure |
| --- | --- |
| `counts a line repeated across constructor rows once, in both the line and the branch totals` | `CommandNotFoundException: The term 'Get-CoberturaFirstPartyCoverageSummary' is not recognized as a name of a cmdlet, function, script file, or executable program.` |
| `excludes a package outside the supplied first-party allowlist from both totals` | same `CommandNotFoundException` for `Get-CoberturaFirstPartyCoverageSummary` |
| `defaults the ProjectNames parameter to Get-KoverageProjectAllowlist` | same `CommandNotFoundException` for `Get-CoberturaFirstPartyCoverageSummary` |
| `throws when the document carries no packages node` | `Expected an exception with message like 'Cobertura XML does not contain a <packages> node.' to be thrown, but the message was 'The term 'Get-CoberturaFirstPartyCoverageSummary' is not recognized ...'` |
| `returns zero counts and a zero rate when a retained package carries no classes` | same `CommandNotFoundException` for `Get-CoberturaFirstPartyCoverageSummary` |
| `renders the four counts and both percentages on one line` | same `CommandNotFoundException` for `Get-CoberturaFirstPartyCoverageSummary` |
| `renders the report line from a Cobertura string without touching the filesystem` | `CommandNotFoundException: The term 'Get-CoberturaFirstPartyCoverageReport' is not recognized ...` |

Output Summary: `FAILED=7`, which satisfies the stated threshold of 6 or greater, and the recorded
failure text names `Get-CoberturaFirstPartyCoverageSummary` as an unresolved command in six of the
seven failures. Every failure is an `It`-level failure and none is a container-level error, which
confirms that the test file dot-sources `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1` alone
and never dot-sources the not-yet-created production file directly. This is the failing regression
test the `CLAUDE.md` Bugfix Workflow requires before the fix. P3-T1 records the pass-after result
over the identical file.
