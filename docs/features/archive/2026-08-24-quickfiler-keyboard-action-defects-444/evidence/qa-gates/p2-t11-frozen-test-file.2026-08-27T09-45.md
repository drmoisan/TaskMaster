# [P2-T11] `QfcCollectionControllerTests.cs` is untouched

Timestamp: 2026-08-27T09-45
EXIT_CODE: 0

File: `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs`

## Metrics

| Measure | `[P0-T21]` baseline | Observed now | Equal |
| --- | --- | --- | --- |
| `(Get-Content …).Count` | 500 | 500 | yes |
| `[TestMethod]` occurrences | 13 | 13 | yes |

## Diff

Command:

```
git diff --stat $(git merge-base HEAD origin/epic/quickfiler-bug-family-integration) -- QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs
```

The merge base re-derives to `125c36b0669d9dd6095f156901bba138e2272f56`, identical to the value
`[P0-T6]` captured.

Output: **empty**. The working-tree form of the command is used deliberately, because this gate runs
before the phase's commit.

## Why this file is frozen

It sits at exactly the 500-line cap with zero spare, and upstream #468 decision `D12` / task `[P4-T5]`
pins its `[TestMethod]` count. Every test this feature would otherwise have added there went into
`QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs` instead.

## Acceptance evaluation

- Both counts equal the values recorded in `[P0-T21]`. PASS.
- The diff output is empty. PASS.

Output Summary: 500 lines and 13 `[TestMethod]` occurrences, both identical to the Phase 0 baseline;
empty diff against the re-derived merge base.
