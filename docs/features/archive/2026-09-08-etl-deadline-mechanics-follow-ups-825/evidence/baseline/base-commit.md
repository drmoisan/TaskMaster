# Baseline — Pre-Change Commit Anchor (D3)

Timestamp: 2026-09-09T16-33

Command: git rev-parse HEAD; git rev-parse --abbrev-ref HEAD; git status --porcelain --untracked-files=all -- . ":(exclude).claude"

EXIT_CODE: 0

BaseCommit: 96fd3dd86cff542226d192158f1c2f63d5eee926
Branch: bug/etl-deadline-mechanics-follow-ups-825-exec
WorktreeStatusLines: 2

Output Summary: The worktree HEAD is 96fd3dd86cff542226d192158f1c2f63d5eee926 on branch
bug/etl-deadline-mechanics-follow-ups-825-exec. The porcelain status, taken with the D4 exclusion
pathspec, reports two lines: a modification to this feature's plan file, which carries the P0-T1
check-off written moments earlier, and the untracked P0-T1 evidence artifact. No source file is
modified at this point. This sha is the D3 anchor for every diff in this plan; origin/main is
deliberately not used, because this feature branches from the epic integration branch whose own
manifest commits under docs/features/epics/ would otherwise enter every diff.
