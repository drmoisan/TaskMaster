# P0-T17 — Scope-Boundary Baseline

Timestamp: 2026-09-01T13-57

Command:
```
git status --porcelain -- QuickFiler.Test UtilitiesCS.Test UtilitiesCS
git diff --name-only issue-648-diff-anchor -- QuickFiler.Test UtilitiesCS.Test UtilitiesCS
```
(both run from the checkout root)

EXIT_CODE: 0

Output Summary:

`issue-648-diff-anchor` is the local tag P0-T3 created on this branch's merge base with
`origin/main`, at commit `c7b4f08f6d80296840f9a351042cb2113892e95f`. It is used as the diff operand
rather than the ref `origin/main`, which the fetch in P0-T3 advances to the current remote tip.

### `git status --porcelain -- QuickFiler.Test UtilitiesCS.Test UtilitiesCS`

Output, verbatim:

```
```

The command printed no lines. The output was **empty**. Exit code 0.

Note that `bin/` and `obj/` build output produced by P0-T11 through P0-T15 exists beneath
`QuickFiler.Test/` at the time of this measurement, but `.gitignore:26` and `.gitignore:27` ignore
`[Bb]in/` and `[Oo]bj/`, so porcelain status does not report it.

### `git diff --name-only issue-648-diff-anchor -- QuickFiler.Test UtilitiesCS.Test UtilitiesCS`

Output, verbatim:

```
```

The command printed no lines. The output was **empty**. Exit code 0.

### Interpretation

Both outputs are empty. Before any change is made, nothing beneath `QuickFiler.Test/`,
`UtilitiesCS.Test/`, or `UtilitiesCS/` is modified in the worktree and nothing beneath those three
paths differs from the merge base. This is the pre-change state that the AC-6 check in P2-T13 is
measured against.
