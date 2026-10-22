# [P1-T6] — CR-1 Stale Assertion Because-Message Corrected (line 222)

Timestamp: 2026-08-29T23-23
Run performed: 2026-08-30T01-17
Task: [P1-T6]
File: `QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs`, line 222
EXIT_CODE: 0

## The edit

Line 222 is the `.BeEmpty(...)` because-message inside
`UnregisterNavigation_AfterRegisteringAtOneDigitAndGrowingToTen_RemovesTheOneDigitKeys` — the
same test method whose `<summary>` block `[P1-T1]` corrected. This is the identical defect in
the identical test, differing only in that it sits in a string literal rather than an XML
comment: the stale text named both mechanisms this cycle's fix deleted, the recorded width and
the grown loop bound, so leaving it would have closed this cycle with the documentation block
corrected while the assertion message three lines below still asserted the deleted mechanism as
the reason the assertion holds.

Replaced literal, 86 characters between the quotes:

```
"the recorded width 1 is replayed and the grown loop bound reaches every registered key"
```

Replacement literal, 87 characters between the quotes:

```
"the ledger replays each key verbatim, so every key is removed regardless of group count"
```

Both character counts were measured directly and match the figures the plan states. The
replacement is one character longer than the text it replaces, so CSharpier's line-breaking
decision for the enclosing `.BeEmpty( … )` call is unchanged and the surrounding lines do not
reflow. The plan quotes this replacement as a single-line inline-code literal rather than a
fenced block, so there is no nested block whose column could be misplaced.

Region on disk after the edit, lines 218-224:

```
            // Assert
            CollectionKeys(registry)
                .Should()
                .BeEmpty(
                    "the ledger replays each key verbatim, so every key is removed regardless of group count"
                );
        }
```

Only the string literal's contents changed, and only on line 222. The `.Should()` chain, the
`.BeEmpty(` call, its closing `);`, the test name, and every attribute are exactly as they
were.

## Acceptance clauses

All three clauses hold.

### Clause 1 — stale fragment removed

Command: `@(Select-String -Path QuickFiler.Test\Controllers\QfcCollectionControllerNavigationDigitsTests.cs -SimpleMatch -Pattern 'grown loop bound reaches').Count`
EXIT_CODE: 0
Before (`[P0-T2]`): `1`   After: `0`   Required: `0`   Result: PASS

### Clause 2 — corrected fragment present

Command: `@(Select-String -Path QuickFiler.Test\Controllers\QfcCollectionControllerNavigationDigitsTests.cs -SimpleMatch -Pattern 'regardless of group count').Count`
EXIT_CODE: 0
Before (`[P0-T2]`): `0`   After: `1`   Required: `1`   Result: PASS

### Clause 3 — file length invariant

Command: `(Get-Content QuickFiler.Test\Controllers\QfcCollectionControllerNavigationDigitsTests.cs).Count`
EXIT_CODE: 0
Before (`[P0-T2]` and `[P1-T1]` clause 3): `226`   After: `226`   Required: `226`   Result: PASS

This is a one-line-for-one-line replacement, so the 226-line invariant holds unaffected by this
task.

## Supplementary observations

- `[TestMethod]` count: `3`, unchanged.
- `[P1-T1]`'s pinned token `the nine recorded keys` still counts `1` after this task, confirming
  this edit did not disturb the block `[P1-T1]` corrected. This is why `[P1-T1]` pinned that
  token rather than `the ledger replays`, a phrase this task's replacement literal also
  introduces.
- Cumulative anchored diff for this file across `[P1-T1]` and `[P1-T6]`:
  `git diff a2c69aead286ad0ec6c7087f1bd8c46d39d0d472 -- QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs`
  shows eight changed lines, four removed and four added — three XML documentation comment lines
  from `[P1-T1]` and one string-literal line from this task. Filtering those eight lines for the
  tokens `Should()`, `BeEmpty(`, `[TestMethod]`, and `public void` returns `0` matches.

## Output Summary

The stale assertion because-message was corrected exactly as mandated. All three acceptance
clauses pass: the stale fragment count fell from `1` to `0`; the corrected fragment count rose
from `0` to `1`; the file held at `226` lines. Only the contents of one string literal changed.
The cumulative diff for this file is eight lines, all comment or string-literal text, with zero
executable tokens added or removed.
