# P1-T5 — Every remaining writer of UiThread._dispatcher moved out of the parallel bucket

Timestamp: 2026-09-03T08-34

Command:
```text
env -C <worktree-root> git grep -l -F '"_dispatcher"' -- UtilitiesCS.Test
env -C <worktree-root> git grep -c -F DoNotParallelize -- UtilitiesCS.Test/Threading/UiThread_Tests.cs UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs
```

EXIT_CODE: 0

The field carries a single integer, which is this gate's normalized outcome. It is not a
single process exit status: the gate ran several commands. Their individual exit codes are
listed below and are unchanged from the original record.

- command 1 — 0
- command 2 — 0

## Edits made (attribute-only, three files)

1. `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs` — `    [DoNotParallelize]` inserted
   immediately after the `[TestClass]` on line 28, giving the two-line form.
2. `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs` — `    [DoNotParallelize]` inserted
   immediately after the `[TestClass]` on line 13, same two-line form.
3. `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs` — line 14's `    [TestClass]` replaced with
   the single line `    [TestClass, DoNotParallelize]`. The combined attribute list is used for this
   one file only, because it is 514 lines at BASE and already exceeds the 500-line limit in
   `.claude/rules/general-code-change.md`; the combined form adds the attribute without adding a
   line, so this change does not deepen the pre-existing overage.

**These changes are attribute-only.** No `using`, no assertion, no test body, and no member was
added, removed, or reordered in any of the three files. `IdleAsyncQueue_Tests.cs` and
`ProgressTrackerAsync_Tests.cs` in particular have every assertion unaltered, which is what keeps
this task compatible with AC4's "all pass, unmodified assertions" wording. `DoNotParallelize`
resolves in all three files without a new `using`, because each already imports
`Microsoft.VisualStudio.TestTools.UnitTesting`.

Post-edit line counts, confirming the intended size effect:

```text
  348 UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs   (baseline 347, +1 line)
  206 UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs   (baseline 205, +1 line)
  514 UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs   (baseline 514, unchanged)
```

## Output Summary

Command 1 output, verbatim:

```text
UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs
UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs
UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs
UtilitiesCS.Test/Threading/UiThread_Tests.cs
```

Exactly the four expected paths and no other. The fourth is present because P1-T2 added the new
class's `DispatcherField()` helper to it.

Command 2 output, verbatim:

```text
UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs:1
UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs:1
UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs:1
UtilitiesCS.Test/Threading/UiThread_Tests.cs:1
```

Exactly four lines, one per path, each reporting a count of exactly `1`.

## Acceptance

Both clauses satisfied. This is the enforceable form of the isolation guarantee: every file in
`UtilitiesCS.Test` that names `UiThread._dispatcher` now carries the do-not-parallelize attribute, so
**zero writers of that field remain in the parallel bucket**. P0-T13 recorded the false-before state
for the identical second command (no output, exit 1), so the gate is demonstrably false before this
task and true after it.

Moving a class from the parallel bucket to the serial bucket can only reduce the concurrency those
tests experience, so it cannot introduce a race. Whether it exposes a latent ordering dependency is
not asserted here; it is verified empirically by P3-T3 and P4-T5.
