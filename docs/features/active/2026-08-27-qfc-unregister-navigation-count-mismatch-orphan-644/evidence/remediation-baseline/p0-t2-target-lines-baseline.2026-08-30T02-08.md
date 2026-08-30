# [P0-T2] — Pre-edit baseline of both target lines and every sibling region

- Timestamp: 2026-08-30T02-08
- Task: `[P0-T2]`
- Issue: #644
- Branch: `bug/qfc-unregister-navigation-count-mismatch-orphan-644`
- Head at cycle entry: `85a1939f92f64ebada4e71d19cc095dc2e8e8a26`
- Target file: `QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs`
- Shell: PowerShell 7.6.5, run from the repository root of the branch worktree.

All fifteen commands are recorded below with their `EXIT_CODE:` and measured value.
Every measured value equals the value the plan states as expected, so no discrepancy
is recorded and `[P1-T1]`/`[P1-T2]` acceptance is evaluated against the plan's stated
before-values unchanged.

## 1. Verbatim capture of line 145

- Command: `Get-Content QuickFiler.Test\Controllers\QfcCollectionControllerNavigationDigitsTests.cs | Select-Object -Skip 144 -First 1`
- EXIT_CODE: 0
- Returned line, verbatim, delimited by square brackets that are not part of the line:

```
[        /// fix it replays the recorded width and removes "01".."09".]
```

- Measured length: 69 columns. The `        /// ` prefix is 12 columns, leaving 57
  characters of comment content through the terminal period. This equals the plan's
  stated 57 characters.

## 2. Verbatim capture of line 179

- Command: `Get-Content QuickFiler.Test\Controllers\QfcCollectionControllerNavigationDigitsTests.cs | Select-Object -Skip 178 -First 1`
- EXIT_CODE: 0
- Returned line, verbatim, delimited by square brackets that are not part of the line:

```
[                    "the recorded registration width is replayed, so the '0'-prefixed keys go"]
```

- Measured length: 94 columns = 20 columns of indentation + 2 quote characters + 72
  characters of literal content. This equals the plan's stated 72 characters between
  the quotes.

## 3. Target-fragment counts (four commands)

| # | Command | Expected | Measured | EXIT_CODE |
|---|---|---|---|---|
| 3 | `@(Select-String -Path QuickFiler.Test\Controllers\QfcCollectionControllerNavigationDigitsTests.cs -SimpleMatch -Pattern 'recorded width and removes').Count` | 1 | **1** | 0 |
| 4 | `@(Select-String -Path QuickFiler.Test\Controllers\QfcCollectionControllerNavigationDigitsTests.cs -SimpleMatch -Pattern '#472 fix, it replayed the recorded width').Count` | 0 | **0** | 0 |
| 5 | `@(Select-String -Path QuickFiler.Test\Controllers\QfcCollectionControllerNavigationDigitsTests.cs -SimpleMatch -Pattern 'registration width is replayed').Count` | 1 | **1** | 0 |
| 6 | `@(Select-String -Path QuickFiler.Test\Controllers\QfcCollectionControllerNavigationDigitsTests.cs -SimpleMatch -Pattern 'each recorded key verbatim').Count` | 0 | **0** | 0 |

Commands 3 and 5 are the stale fragments this cycle removes from the match set (line
145's `recorded width and removes` and line 179's `registration width is replayed`).
Commands 4 and 6 are the corrected fragments, which do not exist yet, so both the
before value of `0` and the after value of `1` are reachable and each assertion is
falsifiable.

## 4. File shape (two commands)

| # | Command | Expected | Measured | EXIT_CODE |
|---|---|---|---|---|
| 7 | `(Get-Content QuickFiler.Test\Controllers\QfcCollectionControllerNavigationDigitsTests.cs).Count` | 226 | **226** | 0 |
| 8 | `@(Select-String -Path QuickFiler.Test\Controllers\QfcCollectionControllerNavigationDigitsTests.cs -SimpleMatch -Pattern '[TestMethod]').Count` | 3 | **3** | 0 |

## 5. Sibling-region tokens (seven commands)

Each command has the shape
`@(Select-String -Path QuickFiler.Test\Controllers\QfcCollectionControllerNavigationDigitsTests.cs -SimpleMatch -Pattern '<token>').Count`
with the token given below. Each is expected `1` and each is re-measured unchanged in
`[P3-T1]` clause 6. Line numbers were captured alongside the counts and confirm the
plan's re-derived positions.

| # | Token | Expected | Measured | Line | EXIT_CODE |
|---|---|---|---|---|---|
| 9 | `modelling the unbracketed` | 1 | **1** | 141 | 0 |
| 10 | `The single residual` | 1 | **1** | 147 | 0 |
| 11 | `the tenth key was never visited whatever the digit width` | 1 | **1** | 149 | 0 |
| 12 | `StartsWith("0", StringComparison.Ordinal)` | 1 | **1** | 176 | 0 |
| 13 | `issue #644 replaced the count-bounded removal loop with a ledger that replays` | 1 | **1** | 184 | 0 |
| 14 | `the added tenth group is irrelevant to` | 1 | **1** | 194 | 0 |
| 15 | `regardless of group count` | 1 | **1** | 222 | 0 |

Tokens 9 and 10 bracket the line-145 edit (line 141 is the untouched first-paragraph
sibling; line 147 opens the untouched second paragraph). Token 12 is the untouched
`.Where` clause immediately above the line-179 edit. Tokens 13, 14 and 15 are the
already-correct regions the plan bars this cycle from disturbing.

## Output Summary

All fifteen commands returned `EXIT_CODE: 0`. Every measured value equals the expected
value stated in the plan: the two verbatim line captures match at 57 and 72 characters
of content respectively; the two stale fragments each count `1`; the two corrected
fragments each count `0`; the file is `226` lines with `3` `[TestMethod]` attributes;
and all seven sibling-region tokens count `1`, at lines 141, 147, 149, 176, 184, 194
and 222. No discrepancy recorded.
