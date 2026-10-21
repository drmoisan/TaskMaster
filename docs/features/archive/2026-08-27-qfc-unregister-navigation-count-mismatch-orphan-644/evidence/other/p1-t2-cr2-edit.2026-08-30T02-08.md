# [P1-T2] — CR-2 correction at line 145

- Timestamp: 2026-08-30T02-08
- Task: `[P1-T2]`
- Issue: #644
- Branch: `bug/qfc-unregister-navigation-count-mismatch-orphan-644`
- File: `QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs`, line 145
- Location: inside the `<summary>` block (lines 139-152) of
  `UnregisterNavigation_AfterRegisteringAtTwoDigitsAndShrinkingToNine_RemovesTheTwoDigitKeys`
- Command: the edit itself was applied with the file-edit tool, not a shell command.
  The verification commands are listed under each acceptance clause below.
- EXIT_CODE: 0

## The change

Before (57 characters of comment content following the `        /// ` prefix, line 69
columns wide):

```
        /// fix it replays the recorded width and removes "01".."09".
```

After (64 characters of comment content, line 76 columns wide):

```
        /// #472 fix, it replayed the recorded width and removed "01".."09".
```

Combined with the unedited line 144, which ends `... After the`, the sentence now
reads: "... After the #472 fix, it replayed the recorded width and removed
"01".."09"." — naming #472 and using the past tense throughout.

The two spaces of leading indentation before `///` and the `///` marker itself are
unchanged. The second paragraph of the block (lines 147-151) is not rewritten.

## Why the `recorded width` phrase is deliberately retained

`code-review.2026-08-30T01-46.md`'s CR-2 finding suggests a longer rewrite that also
restates the #644 mechanism a second time. That restatement is not used here because
the second paragraph (lines 147-151) already states it and the remediation inputs bar
rewriting that paragraph.

The phrase `recorded width` is retained on purpose. The class-scoped sweep pattern is
a proxy for the defect class "text naming the deleted mechanism as current behavior".
It already tolerates two hits — `QuickFiler/Controllers/QfcCollectionController.cs`
line 2372 and `QuickFiler.Test/Controllers/QfcCollectionControllerNavigationLedgerTests.cs`
line 305 — because both are past tense. Once line 145 is past tense and attributed to
#472 it belongs to that same tolerated category, so the correct outcome is to
enumerate it as a third tolerated hit rather than to reword the line out of the
pattern's match set. Rewording to escape the pattern would remove the line from every
future run of that detector without the defect being what changed. The post-edit sweep
recorded in `[P1-T3]` therefore returns `3`, not `2`.

The cycle input's exit condition names two surviving hits. That figure predates this
decision. The input file is a cycle-entry record and is not edited; the divergence
record lives in the plan's "Hard scope limits" section and is restated here.

## Acceptance clauses

| # | Command | Before | Required after | Measured after | EXIT_CODE | Result |
|---|---|---|---|---|---|---|
| 1 | `@(Select-String -Path QuickFiler.Test\Controllers\QfcCollectionControllerNavigationDigitsTests.cs -SimpleMatch -Pattern 'recorded width and removes').Count` | 1 | 0 | **0** | 0 | PASS |
| 2 | `@(Select-String -Path QuickFiler.Test\Controllers\QfcCollectionControllerNavigationDigitsTests.cs -SimpleMatch -Pattern '#472 fix, it replayed the recorded width').Count` | 0 | 1 | **1** | 0 | PASS |
| 3 | `(Get-Content QuickFiler.Test\Controllers\QfcCollectionControllerNavigationDigitsTests.cs).Count` | 226 | 226 | **226** | 0 | PASS |
| 4a | `@(Select-String -Path QuickFiler.Test\Controllers\QfcCollectionControllerNavigationDigitsTests.cs -SimpleMatch -Pattern 'The single residual').Count` (line 147, untouched second-paragraph opening) | 1 | 1 | **1** | 0 | PASS |
| 4b | `@(Select-String -Path QuickFiler.Test\Controllers\QfcCollectionControllerNavigationDigitsTests.cs -SimpleMatch -Pattern 'modelling the unbracketed').Count` (line 141, untouched first-paragraph sibling) | 1 | 1 | **1** | 0 | PASS |

Clause 1's token `recorded width and removes` does not survive the replacement, whose
text reads `removed`, not `removes`. Clause 2's token exists only in the replacement,
so both the before value of `0` and the after value of `1` are reachable and the
assertion is falsifiable.

## Line-width measurement

- Command: `Get-Content QuickFiler.Test\Controllers\QfcCollectionControllerNavigationDigitsTests.cs | Select-Object -Skip 143 -First 3` piped to a `.Length` read
- EXIT_CODE: 0
- Measured: line 144 is 101 columns (unchanged, pre-existing), line 145 is **76**
  columns after the edit (12-column `        /// ` prefix + 64 characters of content),
  and line 146 is 11 columns (unchanged). CSharpier does not reflow comment content,
  and `[P2-T1]`'s before/after SHA-256 check on this file confirms no rewrite occurs.

## Output Summary

All four acceptance clauses hold. The stale fragment `recorded width and removes`
count moved from `1` to `0`; the corrected fragment
`#472 fix, it replayed the recorded width` count moved from `0` to `1`; the file
remains `226` lines; and both bracketing sibling tokens at lines 141 and 147 still
count `1`, confirming neither region was disturbed. The retained `recorded width`
phrase keeps line 145 in the class-scoped sweep's match set as a third tolerated
past-tense hit by design.
