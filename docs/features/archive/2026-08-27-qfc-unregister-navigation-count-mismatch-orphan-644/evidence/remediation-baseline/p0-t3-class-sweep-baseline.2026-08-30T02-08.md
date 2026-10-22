# [P0-T3] — Pre-edit class-scoped sweep baseline

- Timestamp: 2026-08-30T02-08
- Task: `[P0-T3]`
- Issue: #644
- Branch: `bug/qfc-unregister-navigation-count-mismatch-orphan-644`
- Head at cycle entry: `85a1939f92f64ebada4e71d19cc095dc2e8e8a26`
- Shell: PowerShell 7.6.5, run from the repository root of the branch worktree.

## Scope statement

The sweep is repository-wide with no path exclusion of any kind. This plan file is
categorically outside the file set the sweep can match, because `-Include *.cs`
restricts the enumeration to C# source files and the plan is a `.md` file. No manual
exclusion is applied and none is needed.

## Command 1 — sweep count

- Command: `@(Get-ChildItem -Recurse -File -Include *.cs -Path . | Select-String -Pattern 'recorded (registration )?width|loop bound').Count`
- EXIT_CODE: 0
- Expected: `4`
- Measured: **`4`**

## Command 2 — sweep listing with file and line number

- Command: `Get-ChildItem -Recurse -File -Include *.cs -Path . | Select-String -Pattern 'recorded (registration )?width|loop bound' | Select-Object -ExpandProperty Path`, paired with the line numbers
- EXIT_CODE: 0
- Measured listing, repository-relative:

```
QuickFiler\Controllers\QfcCollectionController.cs:2372
QuickFiler.Test\Controllers\QfcCollectionControllerNavigationDigitsTests.cs:145
QuickFiler.Test\Controllers\QfcCollectionControllerNavigationDigitsTests.cs:179
QuickFiler.Test\Controllers\QfcCollectionControllerNavigationLedgerTests.cs:305
```

## Classification of the four hits

| File | Line | Matched text | Classification |
|---|---|---|---|
| `QuickFiler.Test\Controllers\QfcCollectionControllerNavigationLedgerTests.cs` | 305 | `/// has been set to null: before the ledger the loop bound dereferenced that null field and` | Legitimate, past tense. Not a defect and not touched by this cycle. |
| `QuickFiler.Test\Controllers\QfcCollectionControllerNavigationDigitsTests.cs` | 145 | `/// fix it replays the recorded width and removes "01".."09".` | This cycle's item 2, CR-2. Corrected in place to a past-tense form attributed to #472, and thereafter tolerated as a legitimate past-tense hit. It remains in the match set by design. |
| `QuickFiler.Test\Controllers\QfcCollectionControllerNavigationDigitsTests.cs` | 179 | `"the recorded registration width is replayed, so the '0'-prefixed keys go"` | This cycle's item 1, CR-6. To be removed from the match set. |
| `QuickFiler\Controllers\QfcCollectionController.cs` | 2372 | `// was allocated as Count + 1 while the loop bound stayed Count, so the trailing` | Legitimate, past tense. Production file, not touched by this cycle. |

The measured listing matches the four hits the plan names, in the same files and at
the same line numbers, and matches the entry baseline of four that the cycle input
states.

## Output Summary

Both commands returned `EXIT_CODE: 0`. The sweep count is `4`, equal to the expected
value. The four hits are exactly the ones enumerated above: two legitimate past-tense
hits (`QfcCollectionControllerNavigationLedgerTests.cs` line 305 and
`QfcCollectionController.cs` line 2372), line 145 which is corrected in place and
thereafter tolerated as a third past-tense hit, and line 179 which is removed from the
match set. The post-edit expectation recorded in `[P1-T3]` is therefore `3`.
