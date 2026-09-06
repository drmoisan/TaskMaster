# [P2-T4] Specification claim gate — the false pinning phrase is gone and the true one is present

Timestamp: 2026-09-06T01-43

Command:

```powershell
Select-String -SimpleMatch 'is pinned by' -Path 'docs\features\active\2026-09-05-pr-778-post-merge-review-residuals-782\spec.md'
Select-String -SimpleMatch 'WithMessage(UiThread.DispatcherNotInitializedMessage)' -Path 'docs\features\active\2026-09-05-pr-778-post-merge-review-residuals-782\spec.md'
```

Both searches were run from the worktree root against `spec.md` alone, with `-SimpleMatch`.

EXIT_CODE: 0

Output Summary: the negative search reports zero matching lines and the positive search reports three,
which is above the two the acceptance requires.

| Search | Before ([P0-T2]) | After [P2-T1] through [P2-T3] |
|---|---|---|
| `is pinned by` | 2 | 0 |
| `WithMessage(UiThread.DispatcherNotInitializedMessage)` | 0 | 3 |

AFTER-IS-PINNED-BY-COUNT: 0
AFTER-CONSTANT-TOKEN-COUNT: 3

### Search 1 — `is pinned by`

```text
(no matching lines)
```

### Search 2 — `WithMessage(UiThread.DispatcherNotInitializedMessage)`

Three matching lines: one inside AC10, written by [P2-T1]; and two inside AC11, written by [P2-T2].
[P2-T8] later adds a fourth in the Write Set test-file table row and asserts the higher count.

## Why both counts are recorded

A zero-hit search alone can pass vacuously. The phrase `is pinned by` would also return zero if it
had merely re-wrapped across a line boundary, if the file had been renamed, or if the path had been
mistyped, and none of those is the intended edit. The positive count cannot be satisfied without the
intended edit, because the [P0-T2] inventory records that
`WithMessage(UiThread.DispatcherNotInitializedMessage)` occurred zero times in `spec.md` before this
phase. The two counts together decide the gate.

## The two sites the phrase occupied, and the one deliberately retained

The [P0-T2] inventory records that `is pinned by` occurred exactly twice in `spec.md` before this
phase, at lines 167 and 649 as they then stood:

- line 167 — the Behavioral Contract `WpfDispatcherYield` bullet, rewritten by [P2-T3];
- line 649 — the AC10 pinning clause, rewritten by [P2-T1].

Both were sites of the R3 claim, and both are rewritten, so the count reaching zero is a consequence
of the intended edits rather than of anything else.

The SD5 scope-decision row's `pinned by AC10` wording is a different token, is not matched by this
search, and is deliberately retained. AC10 now states a property the C20 assertion actually has, so
that row is true as it stands.
