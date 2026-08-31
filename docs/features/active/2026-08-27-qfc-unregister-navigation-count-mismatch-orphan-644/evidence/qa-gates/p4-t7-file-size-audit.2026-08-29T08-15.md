# QA gate — Frozen-file and file-size audit after the final formatting pass ([P4-T7])

- Issue: #644
- Task: `[P4-T7]`
- Timestamp: 2026-08-29T08-15
- Shell: PowerShell (`pwsh -NoProfile`), working directory repository root (`<repo-root>`)

This is the **authoritative** post-format measurement for AC-10 and for the `[TestMethod]` clause of
AC-11. It supersedes the interim measurement recorded by `[P3-T6]`, which was taken before the
`[P4-T1]` formatting pass. It was re-run in this pass, after the comment condensation applied to
`QuickFiler/Controllers/QfcCollectionController.cs` and after the `[P4-T1]`–`[P4-T5]` loop last
completed green, so it measures the final state of the tree rather than an earlier one.

Commands:

```
(Get-Content QuickFiler.Test\Controllers\QfcCollectionControllerTests.cs).Count
(Select-String -Path QuickFiler.Test\Controllers\QfcCollectionControllerTests.cs -Pattern '\[TestMethod\]').Count
(Get-Content QuickFiler.Test\Controllers\QfcCollectionControllerNavigationDigitsTests.cs).Count
(Select-String -Path QuickFiler.Test\Controllers\QfcCollectionControllerNavigationDigitsTests.cs -Pattern '\[TestMethod\]').Count
(Get-Content QuickFiler.Test\Controllers\QfcCollectionControllerNavigationLedgerTests.cs).Count
```

## Measured values against the acceptance bounds

| # | Measurement | `[P0-T7]` baseline | Bound the task states | Measured | Verdict |
|---|---|---|---|---|---|
| 1 | `QfcCollectionControllerTests.cs` line count | 500 | at or below baseline **and** no greater than 500 | **499** | **PASS** (1 below baseline) |
| 2 | `QfcCollectionControllerTests.cs` `[TestMethod]` count | 13 | equals baseline of 13 | **13** | **PASS** (equal) |
| 3 | `QfcCollectionControllerNavigationDigitsTests.cs` line count | 226 | at or below baseline of 226 **and** no greater than 500 | **226** | **PASS** (equal to baseline) |
| 4 | `QfcCollectionControllerNavigationDigitsTests.cs` `[TestMethod]` count | 3 | equals baseline of 3 | **3** | **PASS** (equal) |
| 5 | `QfcCollectionControllerNavigationLedgerTests.cs` line count | n/a (created by `[P1-T1]`) | no greater than 500 | **361** | **PASS** (139 under the ceiling) |

Raw command output:

```
M1=499 EXIT=True
M2=13 EXIT=True
M3=226 EXIT=True
M4=3 EXIT=True
M5=361 EXIT=True
```

`EXIT=True` is the PowerShell `$?` success indicator immediately after each expression; every one of
the five reported success, so every command's exit status is 0.

## Notes on the two frozen files

**`QfcCollectionControllerTests.cs` shrank by exactly one line, from 500 to 499.** That is the
expected consequence of `[P3-T2]`, which replaced the two arrangement lines
`SeedCollectionKey(kbd, "1");` and `SeedCollectionKey(kbd, "2");` with the single line
`controller.RegisterNavigation();`. `[P3-T1]` and `[P3-T3]` were one-for-one line replacements and
changed no count. The file sat exactly at the 500-line repository ceiling at baseline, so it could
not have grown by even one line; it did not grow, it shrank. Its `[TestMethod]` count is unchanged
at 13, confirming that Phase 3 added and removed no test in this file.

**`QfcCollectionControllerNavigationDigitsTests.cs` is unchanged in length at 226.** `[P3-T4]`
flipped one assertion and rewrote one XML-documentation paragraph without changing the total line
count. Its `[TestMethod]` count is unchanged at 3, confirming that `[P3-T4]` added and removed no
test, which is the `[TestMethod]` clause of AC-11.

**`QfcCollectionControllerNavigationLedgerTests.cs` is 361 lines**, comfortably inside the 500-line
ceiling in `.claude/rules/general-code-change.md`, and carries the six `[TestMethod]`s that
`[P1-T1]` specified.

EXIT_CODE: 0

Output Summary: **All five acceptance clauses PASS.** `QfcCollectionControllerTests.cs` measures
**499** lines (at or below the 500 baseline and at or below the 500 ceiling; it shrank by one line
via `[P3-T2]`'s two-lines-to-one replacement) with a `[TestMethod]` count of **13**, exactly equal
to the `[P0-T7]` baseline. `QfcCollectionControllerNavigationDigitsTests.cs` measures **226** lines
(equal to its 226 baseline, at or below 500) with a `[TestMethod]` count of **3**, exactly equal to
its baseline. The new `QfcCollectionControllerNavigationLedgerTests.cs` measures **361** lines, no
greater than 500. Neither frozen file grew and neither gained or lost a test method. This is the
authoritative post-format measurement for AC-10 and for the `[TestMethod]` clause of AC-11.
