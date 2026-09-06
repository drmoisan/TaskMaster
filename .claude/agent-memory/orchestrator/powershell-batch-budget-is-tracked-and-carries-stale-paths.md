---
name: powershell-batch-budget-is-tracked-and-carries-stale-paths
description: .claude/state/powershell-batch-budget.default.json is git-TRACKED and arrives pre-filled with another session's scratchpad paths, so a fresh agent worktree can be at its .ps1 cap before writing a single file
metadata:
  type: project
---

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
