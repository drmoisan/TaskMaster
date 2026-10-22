# [P1-T1] — CR-1 Stale `<summary>` Block Corrected

Timestamp: 2026-08-29T23-23
Run performed: 2026-08-30T01-17
Task: [P1-T1]
File: `QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs`, lines 189-196
EXIT_CODE: 0

## The edit

The eight-line `<summary>` block on
`UnregisterNavigation_AfterRegisteringAtOneDigitAndGrowingToTen_RemovesTheOneDigitKeys` was
replaced with the block the plan mandates verbatim. The plan's fenced replacement carries two
spaces of Markdown indentation; that indent was stripped before applying, leaving eight spaces
before each `///`, i.e. the replacement begins at column 9. The applied block matches the
surrounding file indentation exactly.

Block on disk after the edit, lines 189-196, verbatim:

```
        /// <summary>
        /// Issue #472, mirror direction. A nine-item page registers keys "1".."9" at width 1. A group
        /// is then added without an intervening unregister, so the live <c>Digits</c> getter now
        /// computes width 2. Before the fix <c>UnregisterNavigation</c> removed the never-registered
        /// "01".."10" and left all nine single-digit keys orphaned. After the fix the ledger replays
        /// the nine recorded keys "1".."9" verbatim, so the added tenth group is irrelevant to
        /// unregistration.
        /// </summary>
```

Only the fifth, sixth, and seventh lines of the block differ from the pre-edit text. The
`<summary>` and `</summary>` delimiters and the first four content lines are unchanged.

The sibling `Issue #472` summary at lines 139-152, which already names the #644 ledger
correctly and carries the CR-2 sentence (out of scope for this cycle), was not touched.

## Acceptance clauses

All four clauses hold. Baseline values are the measured values recorded in `[P0-T2]`, which
were identical to the plan's stated expected values.

### Clause 1

Command: `@(Select-String -Path QuickFiler.Test\Controllers\QfcCollectionControllerNavigationDigitsTests.cs -SimpleMatch -Pattern 'the loop bound has grown to ten').Count`
EXIT_CODE: 0
Before: `1`   After: `0`   Required: `0`   Result: PASS

### Clause 2

Command: `@(Select-String -Path QuickFiler.Test\Controllers\QfcCollectionControllerNavigationDigitsTests.cs -SimpleMatch -Pattern 'the nine recorded keys').Count`
EXIT_CODE: 0
Before: `0`   After: `1`   Required: `1`   Result: PASS

The phrase occurs exactly once, on the single source line reading
`/// the nine recorded keys "1".."9" verbatim, so the added tenth group is irrelevant to`,
inside the block this task replaced. `[P1-T6]` edits only line 222 and cannot affect this
count, which is why this token is pinned here in place of `the ledger replays` — a token that
`[P1-T6]`'s replacement literal also introduces and that therefore could not be pinned to `1`
without going stale later in Phase 1.

### Clause 3

Command: `(Get-Content QuickFiler.Test\Controllers\QfcCollectionControllerNavigationDigitsTests.cs).Count`
EXIT_CODE: 0
Before: `226`   After: `226`   Required: `226`   Result: PASS

Three lines replaced by three lines, so the file length invariant holds.

### Clause 4

Command: `@(Select-String -Path QuickFiler.Test\Controllers\QfcCollectionControllerNavigationDigitsTests.cs -SimpleMatch -Pattern '[TestMethod]').Count`
EXIT_CODE: 0
Before: `3`   After: `3`   Required: `3`   Result: PASS

Companion command: `git diff a2c69aead286ad0ec6c7087f1bd8c46d39d0d472 -- QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs`
EXIT_CODE: 0

The diff contains exactly six changed lines, three removed and three added. Filtering those
six lines for the tokens `Should()`, `[TestMethod]`, and `public void` returns `0` matches, so
no added or removed line contains any of them. Result: PASS

Removed lines:

```
-        /// "01".."10" and left all nine single-digit keys orphaned. After the fix it replays the
-        /// recorded width 1 and, because the loop bound has grown to ten, removes every registered
-        /// key.
```

Added lines:

```
+        /// "01".."10" and left all nine single-digit keys orphaned. After the fix the ledger replays
+        /// the nine recorded keys "1".."9" verbatim, so the added tenth group is irrelevant to
+        /// unregistration.
```

Every changed line is an XML documentation comment line. No assertion, no test name, no
attribute, and no executable line was altered.

## Output Summary

The stale `<summary>` block was corrected exactly as mandated. All four acceptance clauses
pass: stale token count fell from `1` to `0`; replacement token count rose from `0` to `1`;
file length held at `226`; `[TestMethod]` count held at `3` and the anchored diff shows six
changed lines, all XML documentation comments, none carrying `Should()`, `[TestMethod]`, or
`public void`. The change is documentation-comment text only.
