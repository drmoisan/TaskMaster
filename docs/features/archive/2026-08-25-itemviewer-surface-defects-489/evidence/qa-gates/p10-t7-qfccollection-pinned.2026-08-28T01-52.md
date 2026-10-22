# P10-T7 — `QfcCollectionControllerTests.cs` is untouched, unchanged in size, and unchanged in test count

Timestamp: 2026-08-28T01-52
Command: git diff --name-only cecd78130a489fcfdc2ddac7970f344256f4a75a -- QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs ; (Get-Content -LiteralPath QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs).Count ; ((Select-String -LiteralPath QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs -Pattern "\[TestMethod\]" -AllMatches).Matches).Count
EXIT_CODE: 0

`BASELINE_SHA` is `cecd78130a489fcfdc2ddac7970f344256f4a75a`.

## Result

```
LineCount=500
TestMethodCount=13
DiffLines=0
```

| Condition | Required | Observed | Verdict |
|---|---|---|---|
| Path absent from the P10-T2 diff | absent | 0 diff lines; the path is not among the 25 in the P10-T2 list | Met |
| `(Get-Content -LiteralPath …).Count` equals the P0-T15 baseline | **500** | **500** | Met |
| `[TestMethod]` occurrence count | **13** | **13** | Met |

The P0-T15 baseline artifact
`FEATURE/evidence/baseline/phase0-file-line-counts.2026-08-27T23-31.md:47` records
`QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs = 500`, and its summary at `:87` states
the file is "still pinned by 468". The current reading matches that baseline exactly.

## Line-counting method

The count is taken with `(Get-Content -LiteralPath <path>).Count` as the plan's § Execution
conventions require. `Measure-Object -Line` is deliberately not used: the two disagree on files
without a trailing newline, and the pin is an exact-equality assertion where that disagreement would
matter.

## Why the file is pinned

`QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs` is pinned by sibling **468**
(444 spec line 868) and sits at exactly the 500-line repository ceiling, leaving zero spare capacity.
`FEATURE/spec.md` § Sibling-collision resolution records it under "Test files that are off-limits or
capacity-constrained" with the disposition **"Receives no test and no edit."** This feature added no
test to it and made no edit to it.

Output Summary: `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs` is **untouched**. It is
absent from the P10-T2 scope-lock diff (zero diff lines), measures **500** lines — exactly its P0-T15
baseline and exactly the repository ceiling — and carries **13** `[TestMethod]` occurrences, the
required count. The file is pinned by sibling 468 and received no test and no edit from this feature.
