# P0-T13 — Parallel-bucket census and baseline file sizes

Timestamp: 2026-09-03T08-30

Command:
```text
env -C <worktree-root> git grep -n -F '"_dispatcher"' -- UtilitiesCS.Test
env -C <worktree-root> git grep -c -F DoNotParallelize -- UtilitiesCS.Test/Threading/UiThread_Tests.cs UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs
env -C <worktree-root> git grep -n -F "[TestClass" -- UtilitiesCS.Test/Threading/UiThread_Tests.cs UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs
env -C <worktree-root> wc -l UtilitiesCS/Threading/UiThread.cs UtilitiesCS.Test/Threading/UiThread_Tests.cs UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs
```

EXIT_CODE: 0

The field carries a single integer, which is this gate's normalized outcome. It is not a
single process exit status: the gate ran several commands. Their individual exit codes are
listed below and are unchanged from the original record.

- command 1 — 0
- command 2 — 1 (`git grep` exits 1 on zero matches)
- command 3 — 0
- command 4 — 0

## Output Summary

### BASELINE_DISPATCHER_WRITERS:

```text
UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs:144:                "_dispatcher",
UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs:138:                    "_dispatcher",
UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs:422:                "_dispatcher",
```

Exactly three lines, in exactly the three files and at exactly the three line numbers the plan
states.

### BASELINE_DONOTPARALLELIZE_COUNTS:

```text
```

The command printed nothing and exited 1. None of the four files carries the `DoNotParallelize`
token at BASE. This is the false-before half of P1-T5's gate.

### BASELINE_TESTCLASS_LINES:

```text
UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs:28:    [TestClass]
UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs:13:    [TestClass]
UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs:14:    [TestClass]
UtilitiesCS.Test/Threading/UiThread_Tests.cs:8:    [TestClass]
```

Exactly four lines, at exactly the four line numbers the plan states. These are P1-T5's edit sites
(lines 28, 13, and 14) and P1-T2's existing-class attribute (line 8).

### BASELINE_LINE_COUNTS:

```text
  163 UtilitiesCS/Threading/UiThread.cs
  104 UtilitiesCS.Test/Threading/UiThread_Tests.cs
  347 UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs
  205 UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs
  514 UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs
 1333 total
```

The five per-file rows are the recorded baseline counts referred to by P2-T3 and P4-T8. The counting
idiom is `wc -l`, the physical newline count, and it is the idiom used identically in P0-T13, P2-T3,
and P4-T8. The trailing `total` row is expected and is ignored.

PRE-EXISTING FILE-SIZE OVERAGE: UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs 514

That overage exists at BASE `87cb4df338322844abfa580abea14df77e738e5c` — the file is 514 lines
before this plan touches it, already above the 500-line limit in
`.claude/rules/general-code-change.md` — and is not introduced by this change. P1-T5 adds the
attribute to that one file by extending its existing attribute list on line 14 rather than by adding
a line, so the change does not deepen the overage.

## Acceptance

All four stated values match the tree exactly. No BLOCKED condition applies.
