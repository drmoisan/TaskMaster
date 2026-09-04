# P3-T4 — AC3 re-verified against the staged tree: ProgressTrackerAsync.cs is unmodified

Timestamp: 2026-09-03T08-36

Command:
```text
env -C <worktree-root> git add -A -- UtilitiesCS UtilitiesCS.Test
env -C <worktree-root> git status --porcelain -- UtilitiesCS/Threading/ProgressTrackerAsync.cs
env -C <worktree-root> git diff --name-status --cached 87cb4df338322844abfa580abea14df77e738e5c -- UtilitiesCS/Threading/ProgressTrackerAsync.cs
env -C <worktree-root> git grep -n -F "UiDispatcher = UiThread.Dispatcher;" -- UtilitiesCS/Threading/ProgressTrackerAsync.cs
```

EXIT_CODE:
- `git add -A` — 0
- `git status --porcelain` — 0
- `git diff --name-status --cached` — 0
- `git grep -n -F` — 0

## Output Summary

Command 2 (`git status --porcelain` for that path) output, verbatim:

```text
```

Nothing. Neither the index nor the working tree carries any change to
`UtilitiesCS/Threading/ProgressTrackerAsync.cs`.

Command 3 (`git diff --name-status --cached` against BASE for that path) output, verbatim:

```text
```

Nothing. The staged state is identical to BASE `87cb4df338322844abfa580abea14df77e738e5c` for this
path. This span observes the index directly, so it reports a real staged change if one exists; the
porcelain span above is its complement, catching an unstaged working-tree edit that `--cached` would
be blind to. Both are empty.

Command 4 (`git grep`) output, verbatim:

```text
UtilitiesCS/Threading/ProgressTrackerAsync.cs:33:            UiDispatcher = UiThread.Dispatcher;
```

Exactly one line, at line **33**, the line number P0-T3 recorded.

For context, the complete staged footprint under the two source directories after the `git add -A`
is exactly this plan's five owned files and nothing else:

```text
M	UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs
M	UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs
M	UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs
M	UtilitiesCS.Test/Threading/UiThread_Tests.cs
M	UtilitiesCS/Threading/UiThread.cs
```

## Why the fix in UiThread.cs alone converts this consumer's failure mode

`ProgressTrackerAsync.InitializeAsync()` reads the process-global accessor at line 33
(`UiDispatcher = UiThread.Dispatcher;`) and does not touch the resulting instance field until line 35
(`await UiDispatcher.InvokeAsync(`). Before this change, a read taken before `UiThread.Initialize()`
had populated the backing field returned `null` silently at line 33, and the failure surfaced two
lines later as a bare `NullReferenceException` at the `InvokeAsync` dereference — an exception whose
message and stack named the dispatch site rather than the missing initialisation, which is what made
the defect hard to attribute. With the guard added to the `Dispatcher` getter in
`UtilitiesCS/Threading/UiThread.cs`, the property read on line 33 now throws an
`InvalidOperationException` whose message names `UiThread.Init()` and `UiThread.Initialize()`, and
control never reaches line 35. The consumer therefore receives a self-diagnosing exception at the
property-access line without any code change in this file, which is exactly what AC3 requires the
plan to record.

## Acceptance

All three clauses satisfied: the porcelain status command printed nothing, the `--cached` name-status
diff printed nothing, and the grep printed exactly one line whose line number is 33.
