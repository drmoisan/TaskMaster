# [P1-T1] — CR-6 correction at line 179

- Timestamp: 2026-08-30T02-08
- Task: `[P1-T1]`
- Issue: #644
- Branch: `bug/qfc-unregister-navigation-count-mismatch-orphan-644`
- File: `QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs`, line 179
- Test: `UnregisterNavigation_AfterRegisteringAtTwoDigitsAndShrinkingToNine_RemovesTheTwoDigitKeys`
- Command: the edit itself was applied with the file-edit tool, not a shell command.
  The verification commands are listed under each acceptance clause below.
- EXIT_CODE: 0

## The change

Before (72 characters between the quotes, line 94 columns wide):

```
                    "the recorded registration width is replayed, so the '0'-prefixed keys go"
```

After (78 characters between the quotes, line exactly 100 columns wide):

```
                    "the ledger replays each recorded key verbatim, so no '0'-prefixed key survives"
```

The replacement literal is the text `code-review.2026-08-30T01-46.md`'s CR-6 finding
suggests verbatim, copied without paraphrase.

Rationale: "the recorded registration width" names `_registeredDigits`, which this
branch's fix deleted. The message stated, as the reason the assertion holds, a
mechanism that no longer exists.

## Pre-edit collision check

`each recorded key verbatim` measured `0` occurrences in the file at `[P0-T2]`, so it
collides with no other pinned token in this plan. It does not appear in the `[P1-T2]`
line-145 replacement. Line 222, corrected in cycle 1, reads
`"the ledger replays each key verbatim, so every key is removed regardless of group count"`
— the three-word phrase `each key verbatim`, not the four-word phrase
`each recorded key verbatim`, so the two do not overlap as tokens.

## Acceptance clauses

| # | Command | Before | Required after | Measured after | EXIT_CODE | Result |
|---|---|---|---|---|---|---|
| 1 | `@(Select-String -Path QuickFiler.Test\Controllers\QfcCollectionControllerNavigationDigitsTests.cs -SimpleMatch -Pattern 'registration width is replayed').Count` | 1 | 0 | **0** | 0 | PASS |
| 2 | `@(Select-String -Path QuickFiler.Test\Controllers\QfcCollectionControllerNavigationDigitsTests.cs -SimpleMatch -Pattern 'each recorded key verbatim').Count` | 0 | 1 | **1** | 0 | PASS |
| 3 | `(Get-Content QuickFiler.Test\Controllers\QfcCollectionControllerNavigationDigitsTests.cs).Count` | 226 | 226 | **226** | 0 | PASS |
| 4a | `@(Select-String -Path QuickFiler.Test\Controllers\QfcCollectionControllerNavigationDigitsTests.cs -SimpleMatch -Pattern '[TestMethod]').Count` | 3 | 3 | **3** | 0 | PASS |

### Clause 4b — no executable line changed

- Command: `git diff 85a1939f92f64ebada4e71d19cc095dc2e8e8a26 -- QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs`
- EXIT_CODE: 0
- The diff contains exactly one removed line and one added line, both the string
  literal shown above. Neither the added nor the removed line contains the token
  `Should()`, `[TestMethod]`, or `public void`. The `.Should()` occurrences visible in
  the hunk are unchanged context lines, which begin with a space rather than `+` or `-`.

Measured diff hunk:

```
@@ -176,7 +176,7 @@ namespace QuickFiler.Controllers.Tests
                 .Where(k => k.StartsWith("0", StringComparison.Ordinal))
                 .Should()
                 .BeEmpty(
-                    "the recorded registration width is replayed, so the '0'-prefixed keys go"
+                    "the ledger replays each recorded key verbatim, so no '0'-prefixed key survives"
                 );
             remaining
                 .Should()
```

### Clause 5 — the seven sibling tokens are unchanged

Each command has the shape
`@(Select-String -Path QuickFiler.Test\Controllers\QfcCollectionControllerNavigationDigitsTests.cs -SimpleMatch -Pattern '<token>').Count`.
All returned `EXIT_CODE: 0`.

| Token | Before | Required after | Measured after | Result |
|---|---|---|---|---|
| `modelling the unbracketed` | 1 | 1 | **1** | PASS |
| `The single residual` | 1 | 1 | **1** | PASS |
| `the tenth key was never visited whatever the digit width` | 1 | 1 | **1** | PASS |
| `StartsWith("0", StringComparison.Ordinal)` | 1 | 1 | **1** | PASS |
| `issue #644 replaced the count-bounded removal loop with a ledger that replays` | 1 | 1 | **1** | PASS |
| `the added tenth group is irrelevant to` | 1 | 1 | **1** | PASS |
| `regardless of group count` | 1 | 1 | **1** | PASS |

## Line-width measurement

- Command: `Get-Content QuickFiler.Test\Controllers\QfcCollectionControllerNavigationDigitsTests.cs | Select-Object -Skip 178 -First 1` piped to a `.Length` read
- EXIT_CODE: 0
- Measured: **100** columns = 20 columns of indentation + 2 quote characters + 78
  characters of literal content.

This equals CSharpier's default print width of 100, which is inside the limit rather
than over it. Line 184 of the same already-formatted file is likewise exactly 100
columns and passes the repository-wide `csharpier check .`. `[P2-T1]` verifies
independently, by a before/after SHA-256 of this file across a `csharpier format .`
run, that the formatter performs no further rewrite.

## Output Summary

All five acceptance clauses hold. The stale fragment `registration width is replayed`
count moved from `1` to `0`; the corrected fragment `each recorded key verbatim` count
moved from `0` to `1`; the file remains `226` lines with `3` `[TestMethod]` attributes;
the anchored diff shows a single one-line string-literal replacement with no added or
removed line carrying `Should()`, `[TestMethod]` or `public void`; and all seven
sibling-region tokens still count `1`.
