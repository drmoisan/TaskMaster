# P0-T13 PowerShell budget baseline

Timestamp: 2026-10-02T03-08
Command: Read of `.claude/hooks/enforce-powershell-batch-budget.ps1` lines 1-60; Glob `.claude/state/powershell-batch-budget.*.json` and Glob `*` under `.claude/state` (path `<execution-worktree-root>`; read-only; no state file opened for writing)
EXIT_CODE: 0

Quoted regions of the hook (line numbers from the Read output):

Lines 10-11 (caps):

```text
      - 3 production PowerShell files per batch
      - 3 test PowerShell files per batch
```

Line 14 (state file location; the sentence begins on line 13 and the path is on line 14):

```text
    persisted under .claude/state/powershell-batch-budget.<session_id>.json. Only
```

Lines 42-45 (deny behaviour):

```text
    When the cap would be exceeded by a new file, the script emits a PreToolUse JSON
    response with hookSpecificOutput.permissionDecision = 'deny' and exits 0. The session
    must explicitly reset the counter by deleting the state file before starting a new
    batch. Files already counted are always allowed through.
```

State-file observation: Glob `.claude/state/powershell-batch-budget.*.json` returned no files in the execution worktree, and Glob `*` under `.claude/state` also returned no files. The Glob tool has been blind to gitignored paths at this worktree (see p0-t12 GLOB-BLIND), so the result is recorded as "no state file observed in the execution worktree" and not as proof of absence. The coordinator reported the session budget as production 1 of 3 and tests 2 of 3 used; this item needs 1 production slot (scripts/dependencies/BindingRedirectVerification.psm1) and 1 test slot (tests/scripts/dependencies/BindingRedirectVerification.Tests.ps1).

D9 stop rule restated: if any Write or Edit to a PowerShell path is denied, the executor records `BUDGET-DENIED:` with the hook message and stops; it does not delete or edit the state file under `.claude/state/`, does not set `CLAUDE_POWERSHELL_BUDGET_PROD` or `CLAUDE_POWERSHELL_BUDGET_TEST`, and does not split the batch on its own authority.

BUDGET-RULE: any denied PowerShell write stops the run; no state reset, no override variable

Output Summary: Hook caps are 3 production and 3 test files per session batch; deny is emitted on the write that would exceed a cap. No state file observed in the execution worktree. The D9 stop rule is restated.
