---
name: powershell-batch-budget-is-tracked-and-carries-stale-paths
description: OBSOLETE as of 2026-09-08 — the tracked .default.json still carries foreign scratchpad paths, but the hook now worktree-scopes its state file AND drops out-of-root entries on load, so a fresh worktree is no longer pre-exhausted
metadata:
  type: project
---

**CORRECTION, verified 2026-09-08 on the `epic/review-residuals-2026-09-08-integration` base.**
The blocking behaviour described below has been fixed upstream. Two changes in
`.claude/hooks/enforce-powershell-batch-budget.ps1` remove it:

- `Get-PowerShellBatchBudgetSessionId` (around line 162-173) derives the state-file name from the
  worktree root when no `current-session-id` file exists, returning `worktree-<leaf>-<sha8>`. The
  hook therefore reads `.claude/state/powershell-batch-budget.worktree-<leaf>-<hash>.json`, not
  `.default.json`, so the tracked file is not consulted at all in an agent worktree.
- `ConvertTo-PowerShellBatchBudgetState` (around line 217-225) filters both `prodFiles` and
  `testFiles` through `Test-PowerShellBatchBudgetPathInRoot` on load, with the comment "Persisted
  entries that resolve outside this root belong to another worktree and are dropped, so a state
  file carried across worktrees cannot spend this worktree's budget."

`.claude/state/powershell-batch-budget.default.json` is still tracked and still contains the three
2026-08-23 scratchpad paths, so the file below still looks alarming. It is inert. Do NOT author a
plan task that deletes it or that resets the budget: the reset is unnecessary and it dirties a
push-down-owned tracked file for no gain.

What remains true: the caps are `prodCap: 3` / `testCap: 3` per batch, counted over distinct
in-root `.ps1`/`.psm1`/`.psd1` paths, a repeat write to an already-recorded path is free, an
out-of-root path (the scratchpad) spends no slot, and `tests/` or `.Tests.ps1` classifies a file as
test. So keep helper scripts in the scratchpad and keep a batch to at most three production files.

---

**Historical record (the pre-fix behaviour, retained for context):**

`.claude/state/powershell-batch-budget.default.json` caps how many `.ps1` files an agent may Write
per "batch" (`prodCap: 3`, `testCap: 3`). Two facts about it are not derivable from the error
message.

**1. It is git-TRACKED, not local scratch state.** `git ls-files .claude/state/` returns it. So the
committed blob carries whatever `prodFiles` array was current when someone last committed it, and
every fresh worktree inherits that array.

**2. It therefore arrives already full, holding absolute paths from a DIFFERENT session.** Observed
2026-08-29 in a brand-new agent worktree cut from `origin/main`: the very first `Write` of a
scratchpad `.ps1` was refused with

```
PowerShell per-batch budget exceeded: production file cap is 3 and is already full
```

naming three files under a `.../C--Users-<user>-repos-TaskMaster-wt-2026-08-23T22-51/...`
scratchpad — a session six days older that this worktree never had anything to do with. The cap was
exhausted before the run started.

**How to apply:**
- The remedy named in the error message is correct and safe: delete
  `.claude/state/powershell-batch-budget.default.json`. The hook recreates it.
- But because it is tracked, deleting it dirties the branch. `git restore` that one path before
  committing deliverables, or the reset rides along in your PR. Check `git status` for it
  specifically at commit time — it is easy to miss next to the feature folder.
- Prefer `pwsh -NoProfile -File <scratchpad.ps1>` over a `pwsh -Command` one-liner: a double-quoted
  `-Command` containing `$` is separately refused in an isolated worktree
  (see [[pwsh-double-quoted-command-refused-in-worktree]]). So you generally do need the one `.ps1`,
  and therefore do need the budget reset.
- Do NOT "fix" this file in this repository. `.claude/**` is push-down-owned from drm-copilot and is
  overwritten wholesale (see [[project_claude_files_are_pushdown_owned_fix_upstream]] in the user
  memory index). That a mutable per-run counter is committed at all is an upstream defect worth
  reporting, not patching here.

Related: [[bash-tool-rejects-complex-commands-in-isolated-worktree]],
[[agent-worktree-hooks-resolve-to-agent-cwd]].
