---
name: powershell-batch-budget-caps-plan-helper-scripts
description: A plan rule that mandates helper .ps1 files collides with the 3-file PowerShell batch-budget hook and with any exclusive porcelain gate — pin one gitignored fixed path
metadata:
  type: project
---

Before writing a plan rule that tells the executor to author snippets into `.ps1` files,
check two mechanisms that will otherwise deny or falsify the run.

**1. The batch-budget hook denies the fourth file.**
`.claude/settings.json:144` registers `.claude/hooks/enforce-powershell-batch-budget.ps1`
as a `Write|Edit` PreToolUse hook. Verified mechanics:

- `:316` default `ProdCap` is **3** distinct non-test `.ps1` paths.
- `:293` **denies** the write creating the fourth.
- `:289` allows a repeat write to an already-recorded path **unconditionally**.
- `:279-281` **discards** a path resolving outside the worktree root — no slot spent.
- `:284` classifies a file as *test* only via a `tests/` path segment or a `.Tests.ps1`
  suffix; everything else counts against the production cap.

The hook's own message offers three ways out — raise `CLAUDE_POWERSHELL_BUDGET_PROD`,
delete the state file, split the batch. All three are governance bypasses. Do not author a
plan that depends on taking one.

**2. An exclusive porcelain gate freezes its exclusion set too early.**
A gate asserting that `git status --porcelain` lists nothing outside a union computed in
Phase 0 cannot accommodate a helper created later: the union is frozen before the helper
exists, so the helper falls outside it and the gate becomes unsatisfiable in a state a
restart loop cannot clear, because the restart re-runs the same capture against the same
file.

**How to apply:** mandate **one** reusable helper at a single fixed **gitignored** path,
rewritten in place — for example under `coverage/`, which is gitignored at `.gitignore:144`.
That spends exactly one of three budget slots (repeat writes are free) and is invisible to
every porcelain and anchored-diff gate. Give a scratchpad fallback for the case where the
cap is already spent, since an out-of-root path is discarded rather than denied; never write
that absolute path into a committed artifact, because it contains a machine account name.

**Spell the path with forward slashes.** PowerShell accepts them, and a path with no
backslash cannot be corrupted by the doubled-backslash collapse that
[[coverage-seam-workaround-for-claude-worktrees]] and the Bash-tool notes describe. This
matters most when the rule mandating the helper is itself the anti-collapse rule.

Found on issue #731 round 8, where the defect was introduced by the anti-collapse delta the
orchestrator had authored one round earlier — the delta created a file-authoring obligation
that neither the plan's own gates nor the repository hook had been written to accommodate.
