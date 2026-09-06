# Baseline — Diff Anchor (P0-T2)

Timestamp: 2026-09-05T19-19

Command:

```text
git tag -f pre-782-base HEAD
git rev-parse pre-782-base
git status --porcelain --untracked-files=all
```

EXIT_CODE: 0

Output Summary:

`git tag -f pre-782-base HEAD` exited 0. `git rev-parse pre-782-base` exited 0 and printed the
40-character SHA:

```text
b95a525282e1289a9c0616c2ae9c6ae5c0a28920
```

`git status --porcelain --untracked-files=all` exited 0 and printed exactly two lines, recorded
here verbatim:

```text
 M docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/plan.2026-09-05T15-47.md
?? docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/evidence/baseline/phase0-instructions-read.md
```

Both entries are expected and neither is a failure. The plan file is modified because P0-T1 was
checked off before this task ran, and `evidence/baseline/phase0-instructions-read.md` is untracked
because P0-T1 created it and Phase 0 has no commit task of its own; P1-T10 commits it. Both paths
are inside the subtraction set that P8-T20 applies when it compares its own porcelain output
against this record.

`spec.md` and `user-story.md` are absent from this porcelain output, so the worktree was otherwise
clean at `pre-782-base`.
