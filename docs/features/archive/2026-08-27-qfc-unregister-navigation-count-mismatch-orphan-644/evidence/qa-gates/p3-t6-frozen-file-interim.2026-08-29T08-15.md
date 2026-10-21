# QA gate — Frozen-file constraints, interim measurement ([P3-T6])

- Issue: #644
- Task: `[P3-T6]`
- Timestamp: 2026-08-29T08-15
- File measured: `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs`

Measured **before** the final formatting pass. The authoritative post-format measurement for AC-10
is `[P4-T7]`.

## Commands and measured values

Command: `(Get-Content QuickFiler.Test\Controllers\QfcCollectionControllerTests.cs).Count`

```
lines=499
```

Command: `(Select-String -Path QuickFiler.Test\Controllers\QfcCollectionControllerTests.cs -Pattern '\[TestMethod\]').Count`

```
testmethods=13
```

EXIT_CODE: 0

## Evaluation against the `[P0-T7]` measured baseline

| Constraint | `[P0-T7]` baseline | Measured now | Verdict |
|---|---|---|---|
| Line count at or below the baseline | 500 | **499** | **PASS** (one below) |
| Line count no greater than 500 | ceiling 500 | **499** | **PASS** |
| `[TestMethod]` count equals the baseline | 13 | **13** | **PASS** |

Both constraints are hard. The file sits at the 500-line repository ceiling and may not grow by
even one line, and its `[TestMethod]` count is pinned at 13 by the issue #468 freeze. The Phase 3
edits moved the file **down** by one line and changed no `[TestMethod]`.

## Line-delta accounting across Phase 3

| Task | Change | Delta |
|---|---|---|
| `[P3-T1]` | one `SeedCollectionKey` line replaced by one `RegisterNavigation()` line | 0 |
| `[P3-T2]` | two `SeedCollectionKey` lines collapsed into one `RegisterNavigation()` line | -1 |
| `[P3-T3]` | one `SeedCollectionKey` line replaced by one `RegisterNavigation()` line | 0 |
| **Net** | | **-1** |

500 - 1 = 499, which is the measured value. The edits are arrangement-only; no `[TestMethod]` was
added or removed and no assertion was changed in any of the three amended tests.

Output Summary: Interim frozen-file gate **PASS**. `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs`
measures **499 lines** — at or below the `[P0-T7]` baseline of 500 and no greater than the 500-line
ceiling — and **13 `[TestMethod]` attributes**, equal to the baseline. The #468 freeze is
satisfied. `[P4-T7]` re-measures after `[P4-T1]`'s formatting pass and is the authoritative
measurement for AC-10.
