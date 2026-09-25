# P7-T7 — AC17: the repair workflow exists and is statically valid

Timestamp: 2026-09-20T01-56

Commands:

```
pwsh -NoProfile -Command 'Set-Location "<execution-worktree-root>"; & "<execution-worktree-root>\scripts\dev-tools\run-actionlint.ps1"'
pwsh -NoProfile -Command 'Set-Location "<execution-worktree-root>"; Get-ChildItem .github/workflows -Filter *.yml | Measure-Object | Select-Object -ExpandProperty Count'
pwsh -NoProfile -Command 'Import-Module Pester -RequiredVersion 5.6.1; $c = New-PesterConfiguration; $c.Run.Path = @("tests/scripts/dependencies/DependabotConfig.Tests.ps1"); $c.Filter.FullName = "*AC17-*"; $c.Run.PassThru = $true; $c.CodeCoverage.Enabled = $true; $c.CodeCoverage.Path = @("scripts/dependencies","scripts/vscode"); $c.CodeCoverage.OutputFormat = "JaCoCo"; $c.CodeCoverage.OutputPath = "coverage/p7-t7-ac17-coverage.xml"; $r = Invoke-Pester -Configuration $c; "PESTER Passed=$($r.PassedCount) Failed=$($r.FailedCount) Skipped=$($r.SkippedCount) Total=$($r.TotalCount)"; if ($r.FailedCount -gt 0) { exit 1 } else { exit 0 }'
```

EXIT_CODE: 0

## Output Summary

```
ACTIONLINT-EXIT: 0
ACTIONLINT-STDOUT-LINES: 0
ACTIONLINT-STDOUT-BEGIN
ACTIONLINT-STDOUT-END
WORKFLOW-YML-COUNT: 9
PESTER Passed=3 Failed=0 Skipped=0 Total=10 NotRun=7
PESTER_EXIT=0
```

## Acceptance conditions

| Condition | Observed | Result |
|---|---|---|
| `run-actionlint` exit code | 0 | PASS |
| actionlint stdout, verbatim | empty, 0 lines, reproduced between the BEGIN and END markers above | PASS |
| Independent enumeration of `.github/workflows/*.yml` | 9 | PASS |
| That count is one greater than the 8 P0-T21 recorded | 8 + 1 = 9 | PASS |
| Pester exit code | 0 | PASS |
| `Failed` | 0 | PASS |
| `Total` (executed population), at least 2 | 3 | PASS |

**The 9 is an independent filesystem enumeration, not actionlint output.** A clean actionlint run
prints nothing at all — no file count and no summary line, as the empty capture above shows — so no
count of any kind can be read from it, and a non-vacuity observation has to come from somewhere
else. The `Get-ChildItem .github/workflows -Filter *.yml | Measure-Object` enumeration is that
somewhere else. It reads 9 because P7-T6 added `dependabot-repair.yml` to the 8 files P0-T21
counted.

**`Total` is the executed population**, `Passed + Failed + Skipped` = 3 + 0 + 0 = 3, per the
`CMD-PESTER-ALL` retarget governing any task whose command sets `$c.Filter.FullName`. Recorded as
context: `TotalCount` is **10** and `NotRunCount` is **7**, the seven being the AC1 and AC4 cases in
the same file that the filter did not select.

## The three AC17 cases

```
[+] AC17- restricts its work to head branches under the Dependabot branch prefix
[+] AC17- does not use the base-context variant of the pull-request trigger
[+] AC17- declares the write permissions the repair and the disclosure need
```

The branch restriction is asserted as a **positive match on a named expression**:
`startsWith(github.event.workflow_run.head_branch, 'dependabot/')`. The absence assertion is paired
with a positive one in the same case — the count of `workflow_run:` declarations must be greater
than zero — so a file that declared no trigger at all could not satisfy it. The file-readable clause
does the same work for the restriction case.

Coverage was written to `coverage/p7-t7-ac17-coverage.xml`, which `.gitignore:144` covers. No
collector document is written under the evidence tree.

This task checks off **AC17** in
`docs/features/active/2026-09-19-dependabot-fanout-and-ci-failing-nuget-upgrades-911/spec.md`.
