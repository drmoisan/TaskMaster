# [P0-T2] Branch and base commit

Timestamp: 2026-09-07T06-38

Command: git rev-parse --abbrev-ref HEAD; git rev-parse HEAD; git status --porcelain --untracked-files=all

EXIT_CODE: 0

BASE-BRANCH: bug/breadcrumb-lineage-below-archive-root-799
BASE-SHA: 2085504e6daaa11b9ec0a8857e7777cf9b10143f

Output Summary: HEAD of the item worktree is the merge commit that brought origin/main into this branch
immediately before execution began, so every anchored diff later in this plan measures only this item's own
footprint. Porcelain status at the time of capture showed exactly two entries, both produced by [P0-T1] earlier in
this same phase: the modified plan file (its [P0-T1] checkbox) and the untracked
`<FEATURE>/evidence/baseline/phase0-instructions-read.md` artifact. No source file is modified. Recorded per R6;
every later `git diff` in this plan binds `$BaseSha` from this artifact's `BASE-SHA` line.
