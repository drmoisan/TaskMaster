# P2-T2 — Null-forgiving suppression removed, nullable field declaration present

Timestamp: 2026-09-03T08-36

Command:
```text
env -C <worktree-root> git grep -c -F "null!" -- UtilitiesCS/Threading/UiThread.cs
env -C <worktree-root> git grep -n -F "private static Dispatcher? _dispatcher;" -- UtilitiesCS/Threading/UiThread.cs
```

EXIT_CODE:
- command 1 — 1 (`git grep` exits 1 on zero matches)
- command 2 — 0

## Output Summary

Command 1 output, verbatim:

```text
```

No matching line. **Zero `null!` matches in `UtilitiesCS/Threading/UiThread.cs`.**

Command 2 output, verbatim:

```text
UtilitiesCS/Threading/UiThread.cs:149:        private static Dispatcher? _dispatcher;
```

Exactly one line, whose path is `UtilitiesCS/Threading/UiThread.cs`.

## Acceptance

Both clauses satisfied. Both commands are scoped by pathspec to this one file, so neither is affected
by `null!` occurrences elsewhere in the repository (three remain in
`UtilitiesCS/Threading/ProgressTrackerAsync.cs`, which is deliberately outside this plan's write set
and untouched).

The gate is false-before and true-after: P0-T2 recorded the pre-change state as exactly one `null!`
match, on line 140, in the backing-field declaration
`private static Dispatcher _dispatcher = null!; // set in Initialize() before any access`. That
declaration is now `private static Dispatcher? _dispatcher;` with both the null-forgiving initialiser
and its trailing comment removed, and the comment is no longer needed because the accessor now
enforces the invariant it used to assert in prose.
