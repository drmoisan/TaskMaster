---
name: pwsh-command-quoting-in-plan-tasks
description: Any `pwsh -NoProfile -Command` string written into a plan task must use outer SINGLE quotes and inner double quotes, or the calling shell destroys it before pwsh runs
metadata:
  type: feedback
---

When a plan task embeds a `pwsh -NoProfile -Command "<script>"` invocation, write the outer
quoting as **single quotes** and every inner string literal as **double quotes**:

`pwsh -NoProfile -Command '$c = New-PesterConfiguration; $c.Run.Path = "tests/scripts/vscode"; ...; "PASSED=$($r.PassedCount) FAILED=$($r.FailedCount)"'`

Never the reverse (outer double, inner single), and never `\"$(...)\"` escapes.

**Why:** the executor runs plan commands through either the Bash tool or PowerShell. With outer
double quotes the CALLING shell consumes them and expands `$c`, `$r`, and `$(...)` before `pwsh`
ever sees the script. Observed on plan #432 iteration 3: `=: The term '=' is not recognized as a
name of a cmdlet`, `.Run.Path: The term '.Run.Path' is not recognized`, and (bash)
`line 4: .PassedCount: command not found`. Nothing ran, no output file was emitted, and the
task's own "if no output is emitted, halt" acceptance clause would have fired at Phase 0 for a
false reason — a shell-quoting defect masquerading as a missing-capability halt.

**How to apply:** applies to every `-Command`/`-c` one-liner in a plan task body, not just Pester.
Also state in the task that the command runs with the current directory at the worktree root when
its paths are repository-relative, and prefer a self-describing output token (`PASSED=.. FAILED=..`)
over a bare `a/b` string so the recorded evidence is unambiguous. For a multi-statement script
consider whether a durable `<FEATURE>/scripts/*.ps1` file is better than an inline one-liner
(see [[durable-script-copy-into-feature-folder]]).

Related: [[reference_poshqc_mcp_measurement_limits]] — the reason #432 needed a direct
`Invoke-Pester` run at all is that the PoshQC MCP coverage allow-list never instruments
`scripts/vscode/`.
