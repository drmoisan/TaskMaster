# [P0-T12] `.claude/` before state

Timestamp: 2026-09-06T01-35

Command:

```powershell
git status --porcelain --untracked-files=all -- .claude
git diff --name-only pre-782-base..HEAD -- .claude
```

EXIT_CODE: 0

Output Summary: both commands produced no output at all, so both line counts are zero. No `.claude/`
path is modified, staged, or untracked in this worktree, and no `.claude/` path differs between
`pre-782-base` and `HEAD`.

PORCELAIN_LINES=0
DIFF_LINES=0

Neither command printed a path, so there is nothing to enumerate here.

## Why both commands are required

They observe different things and each alone is wrong in one state. `git status --porcelain
--untracked-files=all` sees uncommitted and untracked paths but goes empty once a change is
committed. `git diff --name-only pre-782-base..HEAD` sees committed changes on this branch but cannot
see an uncommitted or untracked path. Together they cover both.

## Consumer

[P5-T1] re-runs exactly these two commands after Phase 4 and before any commit, and requires both to
report zero lines again. `evidence/qa-gates/p6-t3-dotclaude-untouched.md` of the parent delivery
certifies zero changed files under `.claude/`, and the feature review certified that PASS. This
remediation may not falsify that shipped audit result, so it writes nothing under `.claude/`,
including agent memory.
