# R2 Line 345 — The Examined-But-Unrepaired Project

- Timestamp: 2026-09-20T08-43-40
- Task: [P1-T9]
- Finding: R2
- Command: CMD-PESTER-FILTERED, `<FILE>` = `tests/scripts/vscode/Sync-PackageReferences.Tests.ps1`,
  `<FILTER>` = `*R2- returns an unskipped result*`
- EXIT_CODE: 0

## Test Added

`It 'R2- returns an unskipped result with no fix when no hint path needs repair'`

Calls `Invoke-ProjectReferenceSync` with a seam whose `TestPath` resolves every hint path, and
asserts `Skipped` is `$false`, `FixedCount` is 0, and the seam's `WriteText` delegate was **never
invoked**.

## Counts Line, Verbatim

```
PESTER Passed=1 Failed=0 Skipped=0 Executed=1 Total=14 NotRun=13
```

| Measurement | Required | Measured | Result |
|---|---|---|---|
| `EXIT_CODE` | 0 | 0 | PASS |
| `Executed` = `Passed + Failed + Skipped` | 1 | **1** | PASS |
| `Passed` | 1 | **1** | PASS |
| `Total` (context) | — | 14 | — |
| `NotRun` (context) | — | 13 | — |

## What This Exercises

**Line 345**, the `return $result` that fires when `$repairs.Count -eq 0` after `$result.Skipped`
has already been set to `$false` on line 343.

This test **distinguishes the two zero-fix outcomes**, which is the reason it exists alongside
[P1-T7]:

| Outcome | `Skipped` | `FixedCount` | Line reached |
|---|---|---|---|
| Directory holds no project file | `$true` | 0 | 330 |
| Project examined, nothing needed repairing | `$false` | 0 | 345 |

A test asserting only `FixedCount -eq 0` would pass in both states and would therefore prove
neither. The `Skipped` assertion is what separates them, and [P1-T7] asserts the opposite value of
the same property.

**How it fails.** The `WriteText` count is the second discriminating assertion. If a clean project
were written back, the count would be 1 and the test fails. That defect dirties the working tree
on every run, which would make the repair workflow's push gate fire on a run that repaired
nothing and would break AC15's formatting-stable-tree property.

## Output Summary

One test executed, one passed, exit 0. Line 345 is now reached, the two zero-fix outcomes are
distinguished, and the never-written property is asserted.
