# Final PowerShell test step with coverage (P10-T4)

Timestamp: 2026-09-14T21-10

## MCP invocation

Tool: `mcp__drm-copilot__run_poshqc_test`
Workspace root: the item worktree root.

Returned payload:

```
ok: true
summary: Ran bundled PoshQC test against '<repo-root>'.
```

The payload carries no counts and no coverage figure, so this task's acceptance is judged on the paired direct run below.

## Paired direct run

Command: the directory-scoped Pester command with coverage as in P0-T8, namely `pwsh -NoProfile -Command '<worktree prologue>; Import-Module Pester -MinimumVersion 5.0.0 -Force; $c = New-PesterConfiguration; $c.Run.Path = "tests/scripts/vscode"; $c.Run.PassThru = $true; $c.CodeCoverage.Enabled = $true; $c.CodeCoverage.Path = "scripts/vscode"; $c.CodeCoverage.OutputFormat = "JaCoCo"; $c.CodeCoverage.OutputPath = "coverage/pester-coverage.xml"; $r = Invoke-Pester -Configuration $c; ...'`
EXIT_CODE: 0

The report-level LINE counter was read with the selector recorded in P0-T9, in the strict-safe XPath spelling adopted in P8-T3:

```
@($j.SelectNodes('/report/counter')) | Where-Object { $_.type -eq 'LINE' }
```

## Printed result lines, verbatim

```
PESTER Passed=174 Failed=0 Skipped=0 Total=174
PESTER COMMANDPERCENT=84.7195357833656
LINE covered=731 missed=140 total=871 pct=83.93
```

## Acceptance observations

- Failed count: **0**.
- Skipped count: **0**. The artifact records the skipped count without requiring it to be zero, because a pre-existing skipped case would be outside this delivery's scope. On this run there are none.
- Passed count: 174. Total: 174.

### LINE figure — the figure the gate asserts

- LINE covered: **731**
- LINE missed: **140**
- LINE total: **871**
- **LINE percentage: 83.93**

83.93 is at or above 80, so the floor is met. The margin is 34 covered lines above the ceiling of 0.80 times 871, which is 697.

### Command figure — informational only

- `PESTER COMMANDPERCENT` = **84.7195357833656**

Labelled informational. It carries no threshold, per settled decision D4 and the PowerShell rules file.

**No branch figure is recorded.** Pester measures no branch coverage and the delivery introduces no PowerShell branch assertion, threshold or reported figure.

## Stability against the P6-T4 measurement

Every figure is identical to the P6-T4 post-uplift measurement: 731 covered, 140 missed, 871 total, 83.93 percent, 174 passed, 0 failed, and the same command percentage to every printed digit. Together with the two consecutive runs recorded in P6-T6, this is a third consecutive run producing the same figures, which corroborates the determinism proof.

Output Summary: 174 tests, 0 failures, 0 skips. LINE coverage over `scripts/vscode` is 731 of 871, which is 83.93 percent, at or above the 80 floor. The informational command figure is 84.72 percent. No branch figure is recorded.
