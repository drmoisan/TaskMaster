# [P2-T8] Specification wildcard gate — the wildcard literal is gone from `spec.md`

Timestamp: 2026-09-06T01-45

Command:

```powershell
Select-String -SimpleMatch '*UiThread.Init()*' -Path 'docs\features\active\2026-09-05-pr-778-post-merge-review-residuals-782\spec.md'
Select-String -SimpleMatch 'WithMessage(UiThread.DispatcherNotInitializedMessage)' -Path 'docs\features\active\2026-09-05-pr-778-post-merge-review-residuals-782\spec.md'
```

Both searches were run from the worktree root against `spec.md` alone, with `-SimpleMatch` so the
asterisks and parentheses are matched as ordinary characters.

EXIT_CODE: 0

Output Summary: the negative search reports zero matching lines and the positive search reports four,
which is the count the acceptance requires.

| Search | Before ([P0-T2]) | After Phase 2 |
|---|---|---|
| `*UiThread.Init()*` | 3 | 0 |
| `WithMessage(UiThread.DispatcherNotInitializedMessage)` | 0 | 4 |

AFTER-WILDCARD-COUNT: 0
AFTER-CONSTANT-TOKEN-COUNT: 4

### Search 1 — `*UiThread.Init()*`

```text
(no matching lines)
```

### Search 2 — `WithMessage(UiThread.DispatcherNotInitializedMessage)`

Four matching lines, at `spec.md` lines 194, 652, 669, and 675 as the file now stands.

## The three sites the wildcard occupied, and which task rewrote each

The [P0-T2] inventory records that `*UiThread.Init()*` occurred exactly three times in `spec.md`
before Phase 2, at lines 193, 657, and 661 as the file then stood. All three are rewritten by
Phase 2:

- line 193 — the Write Set test-file table row for `UtilitiesCS.Test/Threading/UiThread_Tests.cs`,
  rewritten by this task;
- lines 657 and 661 — the two AC11 clauses, rewritten by [P2-T2].

## The four expected positive matches

- one written into AC10 by [P2-T1];
- two written into AC11 by [P2-T2];
- one written into the Write Set row by this task.

The line numbers moved during Phase 2 because [P2-T3] replaced a three-line bullet with a four-line
one, so the AC entries now begin one line later than the [P0-T2] inventory records. The counts, not
the line numbers, are what the gate decides on.

## What this task changed in the Write Set row

Only the clause ``assert `*UiThread.Init()*` `` was replaced, by
``assert the shared constant through `WithMessage(UiThread.DispatcherNotInitializedMessage)` ``. The
rest of the row is unchanged, including its measured line count, its other four clauses, and its
Findings cell `C06, C10, C11, C12, C13`.
