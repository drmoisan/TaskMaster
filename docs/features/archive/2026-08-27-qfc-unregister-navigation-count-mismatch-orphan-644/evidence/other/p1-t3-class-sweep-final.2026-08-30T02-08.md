# [P1-T3] — Closing class-scoped sweep

- Timestamp: 2026-08-30T02-08
- Task: `[P1-T3]`
- Issue: #644
- Branch: `bug/qfc-unregister-navigation-count-mismatch-orphan-644`
- Shell: PowerShell 7.6.5, run from the repository root of the branch worktree.

## Scope statement

The sweep is repository-wide with `-Include *.cs` and no path exclusion of any kind.
This plan file is categorically outside the `*.cs` file set by construction, not by
exclusion — the same reasoning recorded in `[P0-T3]`.

## Command 1 — post-edit sweep count

- Command: `@(Get-ChildItem -Recurse -File -Include *.cs -Path . | Select-String -Pattern 'recorded (registration )?width|loop bound').Count`
- EXIT_CODE: 0
- Before (`[P0-T3]`): `4`
- Required after: `3`
- Measured after: **`3`**

## Command 2 — post-edit sweep listing

- Command: `Get-ChildItem -Recurse -File -Include *.cs -Path . | Select-String -Pattern 'recorded (registration )?width|loop bound' | Select-Object -ExpandProperty Path`, paired with the line numbers
- EXIT_CODE: 0
- Measured listing, repository-relative:

```
QuickFiler\Controllers\QfcCollectionController.cs:2372
QuickFiler.Test\Controllers\QfcCollectionControllerNavigationDigitsTests.cs:145
QuickFiler.Test\Controllers\QfcCollectionControllerNavigationLedgerTests.cs:305
```

The three survivors are exactly the three named in the plan. Line 179 of the digits
file is absent from the listing, confirming the CR-6 correction removed it from the
match set.

## The three surviving hits, enumerated

| File | Line | Matched text | Status |
|---|---|---|---|
| `QuickFiler.Test\Controllers\QfcCollectionControllerNavigationLedgerTests.cs` | 305 | `/// has been set to null: before the ledger the loop bound dereferenced that null field and` | Tolerated. Past tense, legitimate history. Untouched by this cycle. |
| `QuickFiler\Controllers\QfcCollectionController.cs` | 2372 | `// was allocated as Count + 1 while the loop bound stayed Count, so the trailing` | Tolerated. Past tense, legitimate history. Production file, untouched by this cycle. |
| `QuickFiler.Test\Controllers\QfcCollectionControllerNavigationDigitsTests.cs` | 145 | `/// #472 fix, it replayed the recorded width and removed "01".."09".` | Tolerated. Corrected in place by `[P1-T2]` to a past-tense form attributed to #472; retained in the match set by design. |

## Command 3 — the surviving digits-file hit is the corrected form

- Command: `@(Select-String -Path QuickFiler.Test\Controllers\QfcCollectionControllerNavigationDigitsTests.cs -SimpleMatch -Pattern '#472 fix, it replayed the recorded width').Count`
- EXIT_CODE: 0
- Required: `1`
- Measured: **`1`**

This confirms the one surviving hit in that file is the past-tense attributed form and
not the original text.

## Falsifiability

The gate is falsifiable across the correction: immediately before `[P1-T1]` and
`[P1-T2]` ran, the count was `4` as recorded in `[P0-T3]`, and it reads `3` only once
both edits are in place. Had `[P1-T1]` not run, line 179 would still match and the
count would be `4`.

## Divergence from the cycle input's stated exit figure

The cycle input's exit condition names two surviving hits. That figure assumed line
145 would be reworded out of the pattern's match set. The plan declines that rewording
as gate evasion and corrects line 145 in place to a past-tense attributed form that
remains in the match set as a third legitimate hit. The correct post-edit figure is
therefore three. Three is the required outcome, not a failure. The input file is a
cycle-entry record and is not edited; the plan's "Hard scope limits" section carries
the divergence record.

## Output Summary

All three commands returned `EXIT_CODE: 0`. The class-scoped sweep count moved from
`4` to `3`. The three survivors are exactly
`QfcCollectionControllerNavigationLedgerTests.cs` line 305,
`QfcCollectionController.cs` line 2372, and
`QfcCollectionControllerNavigationDigitsTests.cs` line 145; line 179 of the digits file
is absent. The surviving digits-file hit is confirmed to be the corrected past-tense
attributed form.
