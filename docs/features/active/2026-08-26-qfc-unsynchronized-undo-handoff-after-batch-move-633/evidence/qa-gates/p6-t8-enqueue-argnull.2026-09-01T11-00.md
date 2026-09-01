# Enqueue ArgumentNullException behaviour, unmodified (P6-T8)

Timestamp: 2026-09-01T11-00
Task: [P6-T8]
Working directory: WORKTREE

## Command 1 — scoped test run

Command (leading executable substituted with the absolute path recorded by P0-T14):

```
vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:"FullyQualifiedName~MoveMailAsync_WhenEnqueueThrows_WrapsArgumentNullException" "/Logger:trx;LogFileName=p6-t8.trx" /ResultsDirectory:FEATURE\evidence\qa-gates\p6-t8
```

EXIT_CODE: 0

Count of `outcome="Passed"` occurrences in the produced TRX: **1**.
Count of `outcome="Failed"` occurrences: 0.

| Outcome | Test |
|---|---|
| Passed | `MoveMailAsync_WhenEnqueueThrows_WrapsArgumentNullException` |

## Command 2 — proof the test was not modified

Command:

```
git diff origin/main -- QuickFiler.Test/Controllers/QfcItemController.MailActionsTests.cs
```

EXIT_CODE: 0
Output: empty. The command produced no output at all.

The two-dot form is deliberate and matches P7-T10. It compares the **working tree** against the base, so
it observes the Phase 3 through Phase 6 changes even though they are still uncommitted at this point:
the last commit before Phase 7 is the one P2-T7 took, which covers Phases 1 and 2 only. A three-dot form
is commit-to-commit and would have reported nothing for work that is not yet committed, making the check
vacuous.

`git rev-parse origin/main` at this point returns `06b1e02e5d545b4dfae398cdbf9ae10a3f98ac72`, identical
to the value P0-T3 recorded, so `origin/main` did not advance during execution and no re-run against a
substituted merge-base SHA was required.

Output Summary: `Enqueue(EmailFiler, IList<MailItemHelper>)` still raises `ArgumentNullException`
synchronously in the caller's frame for a null or null-containing helper list, and the test that pins
that behaviour passes without any edit to its file.

This is the constraint that shaped the P3-T4 rewrite. The overload delegates to the item overload, but
it constructs the `FilerQueueItem` in its **own** frame first, so the constructor's `ThrowIfNull` and
any-null guard run before control leaves the caller's stack frame. Had the overload instead deferred
construction into the item overload or onto the worker, the exception would have surfaced later and
`QfcItemController.MoveMailAsync` would no longer have wrapped it into an `InvalidOperationException`.

This artifact supplies the evidence for the AC12 check-off in P8-T16.
