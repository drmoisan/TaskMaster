# Batch 4 — PowerShell test step (P6-T9)

Timestamp: 2026-09-14T20-10

## MCP invocation

Tool: `mcp__drm-copilot__run_poshqc_test`
Workspace root: the item worktree root.

Returned payload:

```
ok: true
summary: Ran bundled PoshQC test against '<repo-root>'.
```

The payload carries no counts, so this task's acceptance is judged on the printed `PESTER Passed=` line from the paired direct run.

## Paired direct run

Command: `pwsh -NoProfile -Command '<worktree prologue>; Import-Module Pester -MinimumVersion 5.0.0 -Force; $c = New-PesterConfiguration; $c.Run.Path = "tests/scripts/vscode"; $c.Run.PassThru = $true; $c.Output.Verbosity = "Detailed"; $r = Invoke-Pester -Configuration $c; "PESTER Passed=" + $r.PassedCount + " Failed=" + $r.FailedCount + " Skipped=" + $r.SkippedCount + " Total=" + $r.TotalCount'`
EXIT_CODE: 0

Printed result line, verbatim:

```
PESTER Passed=174 Failed=0 Skipped=0 Total=174
```

Failed count: **0**.

## Total-count check

Total recorded in the P5-T6 artifact: **173**.
Total on this run: **174**.
Difference: **+1**, which satisfies the requirement that the total be at least 1 greater.

The single case is the one P6-T1 added, `resolves the vstest console path through the vswhere seam`, which drives `Get-VsTestConsolePath` against an in-process stand-in. The P6-T5 authorized branch did not fire and the P6-T7 contingency was not taken, so no further case was added in this phase and the difference is exactly 1.

## Purity observations

The two `What if:` lines in the console output are from the SDK-install and process-cleanup confirmation cases, each confirming its `ShouldProcess` call was reached and returned false. No case started a process, slept, touched the network, or used a temporary file.

Output Summary: 174 tests, 0 failures, 0 skips. The total is 1 greater than the 173 recorded in P5-T6, matching the single case this batch added.
