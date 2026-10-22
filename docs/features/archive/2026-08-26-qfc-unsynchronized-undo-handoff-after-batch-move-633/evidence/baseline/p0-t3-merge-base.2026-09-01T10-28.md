# Merge-base capture (P0-T3)

Timestamp: 2026-09-01T10-28
Task: [P0-T3]
Working directory: WORKTREE
Branch: `bug/qfc-unsynchronized-undo-handoff-after-batch-move-633`

## Command 1

Command: `git fetch origin main`
EXIT_CODE: 0
Output:

```
From https://github.com/drmoisan/TaskMaster
 * branch              main       -> FETCH_HEAD
```

## Command 2

Command: `git merge-base origin/main HEAD`
EXIT_CODE: 0
Output:

```
06b1e02e5d545b4dfae398cdbf9ae10a3f98ac72
```

## Command 3

Command: `git rev-parse origin/main`
EXIT_CODE: 0
Output:

```
06b1e02e5d545b4dfae398cdbf9ae10a3f98ac72
```

## Command 4 (recorded for the diff-anchoring tasks)

Command: `git rev-parse HEAD`
EXIT_CODE: 0
Output:

```
649e22930e4b0ce7d24ddcccb35442ce77d831d4
```

Output Summary: The merge-base SHA is `06b1e02e5d545b4dfae398cdbf9ae10a3f98ac72`, a 40-character
hexadecimal commit id, and `git merge-base` exited 0. The merge-base is byte-identical to
`git rev-parse origin/main`, because `origin/main` at 06b1e02e was merged into this branch before
execution began and the merge was clean. That equality is the fact P6-T8 and P7-T10 depend on: their
`git diff origin/main -- <paths>` two-dot form therefore compares the working tree against the true
merge base, and no substitution of a recorded merge-base SHA is required unless `origin/main` advances
during execution. HEAD at the start of execution is
`649e22930e4b0ce7d24ddcccb35442ce77d831d4`.
