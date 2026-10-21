# [P4-T9] Phase 4 commit — issue #469 defect 3

Timestamp: 2026-08-26T10-16

Command:

```
git add -- QuickFiler/Controllers/QfcCollectionController.cs \
           QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs \
           QuickFiler.Test/Controllers/QfcCollectionControllerDefects468MoveTests.cs \
           QuickFiler.Test/QuickFiler.Test.csproj \
           docs/features/active/qfc-collection-controller-defects-468/plan.2026-08-24T09-39.md \
           docs/features/active/qfc-collection-controller-defects-468/spec.md \
           docs/features/active/qfc-collection-controller-defects-468/evidence/
git commit -m "fix(469): replace the unordered move collection with an ordered snapshot"
git show --name-only HEAD
```

EXIT_CODE: 0

## Output Summary

Commit `d512fcfe19ef00625d06297f2ec88a6e5cc0d065`
`fix(469): replace the unordered move collection with an ordered snapshot`

### Acceptance verification — no path outside the owned file set

`git show --name-only HEAD | grep -E '\.(cs|csproj)$'` returns exactly four paths:

| Path | Owned because |
|---|---|
| `QuickFiler/Controllers/QfcCollectionController.cs` | `<CTRL>`, the feature's single production file |
| `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs` | D12 names it as receiving the `_itemGroupsToMove` injection change and no new test |
| `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468MoveTests.cs` | D12 test file 3 of 5, created by P4-T1 |
| `QuickFiler.Test/QuickFiler.Test.csproj` | D13 registration point for the new test file |

No path outside that set appears. `git status --porcelain`, filtered to paths outside `.claude/`,
is empty after the commit, so nothing owned by this feature was left behind.

Per D15 the commit also carries the plan checklist, `spec.md` (for the AC check-off), and this
phase's evidence artifacts including the P3-T7 commit artifact, which could only be written after
the Phase 3 commit existed.

`.claude/agent-memory/**` and `.claude/state/**` remain unstaged; every `git add` used an explicit
pathspec.

### Acceptance criteria checked off in this commit

**AC-6 (#469 defect 3)** — marked `[x]` in `spec.md`. Both halves are positively verified:

| Clause | Evidence |
|---|---|
| `_itemGroupsToMove`'s declared type is an ordered contract | now `IReadOnlyList<QfcItemGroup>`; asserted by `ItemGroupsToMoveFieldDeclaresAnOrderedContract`, red in P4-T3 and green in P4-T7 |
| `TryGetItemGroupByIndex` performs an explicit bounds check rather than catching `System.Exception` | the method body now contains **0** `catch` clauses and an explicit `is null \|\| index < 0 \|\| index >= Count` guard |
| (a) structural test asserting the field's `FieldType` is assignable to an ordered contract, failing before the fix | P4-T3 TRX, failed 1, message naming the concurrent dictionary type |
| (b) behavioural test asserting `[A, B, C]` with `B` removed and `D` added resolves to `A, C, D` | `TryGetItemGroupByIndexResolvesInsertionOrderAfterMutation`, passing in P4-T7; it also asserts index `-1` and index `== Count` each return `null` |

Result: PASS.
