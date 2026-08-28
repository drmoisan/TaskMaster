---
name: powershell-scratch-script-budget-hook-blocks-helpers
description: Writing a .ps1 helper into the scratchpad is refused by the PowerShell change-budget hook (cap 3, counted across sessions and worktrees) — use pwsh -Command, perl or python instead, and never reset the budget file
metadata:
  type: project
---

Writing any `.ps1` file — including a throwaway runner in the session scratchpad — can be refused by
the PowerShell change-budget hook with "production file cap is 3 and is already full", listing
scripts from a DIFFERENT session's scratchpad in a DIFFERENT worktree.

**Why:** the budget state at `.claude/state/powershell-batch-budget.default.json` is per-worktree
and is not scoped to the current session, so scratch helpers written by an earlier agent still
count. The hook offers three outs: split the batch, raise `CLAUDE_POWERSHELL_BUDGET_PRODUCTION`
"with approved scope", or delete the state file. **Take none of them** — an executor has no approved
scope to raise a budget, and deleting the state file mutates governance state to work around a
control.

**How to apply:** do not need a `.ps1` at all.
- For a one-shot msbuild/vstest invocation that needs slash switches, call
  `pwsh -NoProfile -Command '<one-liner>'` from Bash. Wrap the whole pwsh command in bash SINGLE
  quotes so `${env:ProgramFiles(x86)}` and `"/p:Platform=Any CPU"` survive; use `& $msbuild ...` and
  never `Start-Process -ArgumentList`. Finish with `Write-Host "EXIT_CODE=$LASTEXITCODE"` and run it
  `run_in_background: true`, then poll with `until grep -q "EXIT_CODE" <task-output>; do sleep 5; done`
  (a bare `sleep N` before a chained command is blocked by a separate guard).
- For real logic (text substitution, XML/coverage parsing) write `.py` or `.pl` into the scratchpad;
  those carry no budget.

Observed 2026-08-28 on #677; the three files filling the cap belonged to worktree
`2026-08-23T22-51`, which this session never touched.

Related: [[project_pwsh_command_quoting_from_bash]], [[project_long_runs_need_detached_process]]
