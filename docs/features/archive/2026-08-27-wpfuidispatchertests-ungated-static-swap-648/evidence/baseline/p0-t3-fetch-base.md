# P0-T3 — Fetch Base Ref and Pin the Diff Anchor

Timestamp: 2026-09-01T13-20

Command:
```
git fetch origin main
git rev-parse origin/main
git merge-base origin/main HEAD
git tag issue-648-diff-anchor c7b4f08f6d80296840f9a351042cb2113892e95f
git rev-parse issue-648-diff-anchor
```
(all run from the checkout root)

EXIT_CODE: 0

OriginMainTip: c7b4f08f6d80296840f9a351042cb2113892e95f
DiffAnchor: c7b4f08f6d80296840f9a351042cb2113892e95f
DiffAnchorRef: issue-648-diff-anchor
DiffAnchorTagPreexisted: no

Output Summary:

- `git fetch origin main` exited 0 and reported `* branch main -> FETCH_HEAD`.
- `git rev-parse origin/main` printed the 40-character hash
  `c7b4f08f6d80296840f9a351042cb2113892e95f`.
- `git merge-base origin/main HEAD` printed the same 40-character hash. The two agree because
  `origin/main` was merged into this branch before execution began, which makes `origin/main` an
  ancestor of HEAD and therefore makes the merge base equal to the `origin/main` tip. That equality
  is the expected state for this execution, not a defect: it additionally means every two-dot diff
  against `issue-648-diff-anchor` equals the three-dot diff against `origin/main`.
- `git tag issue-648-diff-anchor c7b4f08f6d80296840f9a351042cb2113892e95f` exited 0. No tag of that
  name existed in this checkout beforehand, so this is the first execution and the creation branch
  applied. `DiffAnchorTagPreexisted:` is therefore `no`, and both the creation command and the
  verification command exited zero, so the two readings agree.
- `git rev-parse issue-648-diff-anchor` printed `c7b4f08f6d80296840f9a351042cb2113892e95f`, which is
  the same hash recorded in `DiffAnchor:`.

The tag is local and unpushed. It does not appear in `git status`, so it does not affect the
clean-worktree condition in P2-T18. Every diff in this plan uses `issue-648-diff-anchor` as its
operand rather than the moving ref `origin/main`.
