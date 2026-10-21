# [P0-T2] — CR-1 Pre-Edit Baseline

Timestamp: 2026-08-29T23-23
Run performed: 2026-08-30T01-17
Task: [P0-T2]
Working directory: repository root of the worktree
EXIT_CODE: 0 (all eight commands; the PowerShell session returned exit 0)

## Command 1 — verbatim `<summary>` block, lines 189-196

Command: `Get-Content QuickFiler.Test\Controllers\QfcCollectionControllerNavigationDigitsTests.cs | Select-Object -Skip 188 -First 8`
EXIT_CODE: 0

Eight returned lines, verbatim:

```
        /// <summary>
        /// Issue #472, mirror direction. A nine-item page registers keys "1".."9" at width 1. A group
        /// is then added without an intervening unregister, so the live <c>Digits</c> getter now
        /// computes width 2. Before the fix <c>UnregisterNavigation</c> removed the never-registered
        /// "01".."10" and left all nine single-digit keys orphaned. After the fix it replays the
        /// recorded width 1 and, because the loop bound has grown to ten, removes every registered
        /// key.
        /// </summary>
```

This is byte-identical to the block `[P1-T1]` quotes as its replacement target (after stripping the
two-space Markdown indent from the plan's fence, which leaves eight spaces before each `///`).

## Command 2 — stale sentence token

Command: `@(Select-String -Path QuickFiler.Test\Controllers\QfcCollectionControllerNavigationDigitsTests.cs -SimpleMatch -Pattern 'the loop bound has grown to ten').Count`
EXIT_CODE: 0
Measured: `1`   Expected: `1`   Match: yes

## Command 3 — corrected phrase token (not yet present)

Command: `@(Select-String -Path QuickFiler.Test\Controllers\QfcCollectionControllerNavigationDigitsTests.cs -SimpleMatch -Pattern 'the nine recorded keys').Count`
EXIT_CODE: 0
Measured: `0`   Expected: `0`   Match: yes

## Command 4 — file line count

Command: `(Get-Content QuickFiler.Test\Controllers\QfcCollectionControllerNavigationDigitsTests.cs).Count`
EXIT_CODE: 0
Measured: `226`   Expected: `226`   Match: yes

## Command 5 — `[TestMethod]` count

Command: `@(Select-String -Path QuickFiler.Test\Controllers\QfcCollectionControllerNavigationDigitsTests.cs -SimpleMatch -Pattern '[TestMethod]').Count`
EXIT_CODE: 0
Measured: `3`   Expected: `3`   Match: yes

## Command 6 — verbatim line 222

Command: `Get-Content QuickFiler.Test\Controllers\QfcCollectionControllerNavigationDigitsTests.cs | Select-Object -Skip 221 -First 1`
EXIT_CODE: 0

Single returned line, verbatim:

```
                    "the recorded width 1 is replayed and the grown loop bound reaches every registered key"
```

Measured character count between the quotes: `86`, matching the figure the plan states.

## Command 7 — stale fragment at line 222

Command: `@(Select-String -Path QuickFiler.Test\Controllers\QfcCollectionControllerNavigationDigitsTests.cs -SimpleMatch -Pattern 'grown loop bound reaches').Count`
EXIT_CODE: 0
Measured: `1`   Expected: `1`   Match: yes

## Command 8 — corrected fragment (not yet present)

Command: `@(Select-String -Path QuickFiler.Test\Controllers\QfcCollectionControllerNavigationDigitsTests.cs -SimpleMatch -Pattern 'regardless of group count').Count`
EXIT_CODE: 0
Measured: `0`   Expected: `0`   Match: yes

## Discrepancies

None. All eight measured values equal the values the plan states as expected. `[P1-T1]` and
`[P1-T6]` are therefore evaluated against these measured values, which are identical to the
expected values.

## Output Summary

Pre-edit CR-1 baseline captured. Stale `<summary>` token `the loop bound has grown to ten` = 1;
replacement token `the nine recorded keys` = 0; file length 226 lines; `[TestMethod]` = 3; stale
line-222 fragment `grown loop bound reaches` = 1; replacement fragment `regardless of group count`
= 0. Both stale regions confirmed present and both replacement tokens confirmed absent, so both
Phase 1 text gates are falsifiable across the correction. Line-222 literal measured at 86
characters between the quotes; the mandated replacement measures 87, one character longer, as the
plan states.
