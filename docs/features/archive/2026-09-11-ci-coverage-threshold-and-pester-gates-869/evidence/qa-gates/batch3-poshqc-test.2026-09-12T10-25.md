# Batch 3 — PowerShell test step (P5-T6)

Timestamp: 2026-09-14T19-44

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
PESTER Passed=173 Failed=0 Skipped=0 Total=173
```

Failed count: **0**.

## Total-count check

Total recorded in the P4-T5 artifact: **160**.
Total on this run: **173**.
Difference: **+13**, exactly the value required.

The thirteen cases are:

- five in the new `tests/scripts/vscode/TestProcessCleanup.Tests.ps1`, covering the no-matching-process early return, a non-matching command line, the breadth-first child walk, a null process lookup, and the confirmation guard;
- four added to `tests/scripts/vscode/Invoke-MSTestWithCoverage.Merge.Tests.ps1`, covering the synthesised-primary fallback, the created methods element, the condition-coverage behaviour on a winning line that carries none, and the replacement of condition elements from the winning line;
- four added to `tests/scripts/vscode/Install-RepoDotNetSdk.Tests.ps1`, covering the default install-directory derivation, the explicit install-directory normalisation, the already-installed early return, and the confirmation-guard early return.

160 + 5 + 4 + 4 = 173, which reconciles exactly. No case was removed.

## Purity observations from the run's own output

Two `What if:` lines appear in the console output, one from the SDK-install confirmation case and one from the process-cleanup confirmation case. Each is the evidence that its `ShouldProcess` call was reached and returned false, so neither case proceeded past its guard: no SDK archive was downloaded or extracted, and no process was stopped. No case started a process, slept, touched the network, or used a temporary file.

Output Summary: 173 tests, 0 failures, 0 skips. The total is exactly 13 greater than the 160 recorded in P4-T5, matching the thirteen cases this batch added.
