---
name: never-triage-children-by-commit-count
description: Commit count is blind to uncommitted work and to plan progress; read each worktree's actual branch, git status, and on-disk plan/AC counts before resuming or restarting any child
metadata:
  type: feedback
---

Before resuming or launching ANY child, derive its state from the worktree itself: the **actual checked-out
branch** (`git -C <wt> rev-parse --abbrev-ref HEAD`), `git status --porcelain` **in that worktree**, and plan
task / acceptance-criteria counts **from the files on disk**. Never triage by commit count, and never match
branches with a number glob.

**Why:** On the quickfiler-bug-family epic, four children were killed mid-run by an API spend limit. I assessed
them by branch name and `rev-list --count` and got two badly wrong:
- **501** showed "0 commits ahead" — implying nothing done. It actually had **16 uncommitted entries**
  including two new source files, plan at **73/115**, and an evidence tree with red/green regression pairs
  through P5 and *final* csharpier/analyzer/nullable/coverage records. Roughly two-thirds delivered, one
  `git checkout` from oblivion.
- **476** showed "0 ahead" on `bug/...-476`. The real work was on **`bug/...-476-exec`** in a different
  worktree: 4 commits, 7 dirty files, plan **44/88**. I had matched the branch name I *assigned*, not the one
  that exists. `-exec` variants exist in this repo precisely because a locked worktree forced a rename, so a
  `*NNN` glob silently selects the wrong branch.

Acting on that assessment would have restarted two largely-complete features.

**How to apply:** When a killed child's worktree is dirty, **commit the work yourself before launching
anything** — it is one checkout from loss. Disclose authorship in the message ("committed by the parent, not
the feature's own child") and state plainly that it has had no review, no toolchain pass and no AC
verification, so the resuming child knows to validate rather than trust it. Never `git checkout --` or
`reset --hard` a dirty child worktree to get a clean base. Related:
[[double-delegation-idleness-test]], [[preserve-halted-child-worktree]], [[region-ownership-is-a-prefix-claim]].
