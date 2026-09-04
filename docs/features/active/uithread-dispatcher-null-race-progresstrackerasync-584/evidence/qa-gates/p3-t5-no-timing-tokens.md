# P3-T5 — AC5 verified across the whole change: no timing construct in any added line

Timestamp: 2026-09-03T08-37

Command:
```text
env -C <worktree-root> mkdir -p TestResults
env -C <worktree-root> git diff 87cb4df338322844abfa580abea14df77e738e5c -- UtilitiesCS UtilitiesCS.Test > TestResults/p3-t5-source.diff
env -C <worktree-root> grep -E '^\+' TestResults/p3-t5-source.diff | grep -E -i 'Thread\.Sleep|Task\.Delay|SpinWait|Retry|retries|Timeout\(|PushFrame'
```

EXIT_CODE:
- `mkdir -p TestResults` — 0
- `git diff ... > TestResults/p3-t5-source.diff` — 0
- the two-stage `grep` pipeline — 1 (the exit code of the second `grep`, which is what `grep`
  returns when it finds no match)

## Output Summary

`TestResults/p3-t5-source.diff` byte size: **5626 bytes**. The diff is non-empty, so the gate had
real content to inspect; an empty diff would have been BLOCKED rather than PASS.

The diff covers exactly this plan's five owned files:

```text
diff --git a/UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs b/UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs
diff --git a/UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs b/UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs
diff --git a/UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs b/UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs
diff --git a/UtilitiesCS.Test/Threading/UiThread_Tests.cs b/UtilitiesCS.Test/Threading/UiThread_Tests.cs
diff --git a/UtilitiesCS/Threading/UiThread.cs b/UtilitiesCS/Threading/UiThread.cs
```

It contains **94** lines beginning with `+` (including the five `+++` file headers), so the filter
was applied to a substantial added-line set rather than to nothing.

Third command output, verbatim:

```text
```

Nothing. The second `grep` found no match among the added lines and exited 1.

## Acceptance

- `TestResults/p3-t5-source.diff` is non-empty (5626 bytes) — satisfied.
- The third command printed nothing and its second `grep` exited 1 — satisfied.

None of the seven prohibited tokens — `Thread.Sleep`, `Task.Delay`, `SpinWait`, `Retry`, `retries`,
`Timeout(`, `PushFrame` — appears in any added line of this change, in any letter case. The search
was case-insensitive (`-i`) and did not distinguish code from comment, so the result covers the new
class's XML doc comment as well as its executable lines. The new regression test drives the accessor
contract directly through the private backing field, which is what makes it deterministic with no
timing construct at all.

The diff span is anchored to BASE rather than left bare, so it cannot degrade into a
worktree-versus-index comparison, and it uses the single-ref working-tree form rather than the
two-dot form; at this point in the plan no commit has been made, so a two-dot span would have
returned an empty diff and the gate would have passed vacuously.

`TestResults/` is gitignored by `.gitignore` line 39 (`[Tt]est[Rr]esult*/`) and `*.diff` files there
enter no porcelain, diff, or format gate in this plan.
