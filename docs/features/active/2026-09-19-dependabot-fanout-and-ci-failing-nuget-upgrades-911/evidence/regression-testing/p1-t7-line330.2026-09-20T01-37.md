# R2 Line 330 — `Invoke-ProjectReferenceSync` Skips a Directory With No Project File

- Timestamp: 2026-09-20T08-43-00
- Task: [P1-T7]
- Finding: R2
- Command: CMD-PESTER-FILTERED, `<FILE>` = `tests/scripts/vscode/Sync-PackageReferences.Tests.ps1`,
  `<FILTER>` = `*R2- skips the manifest directory*`
- EXIT_CODE: 0

## Test Added

`It 'R2- skips the manifest directory when no project file sits beside it'`

Calls `Invoke-ProjectReferenceSync` with a seam whose `ListProjectPath` returns an empty array,
and asserts three things: `Skipped` is `$true`, `FixedCount` is 0, and the seam's `ReadText`
delegate was **never invoked**. The invocation count is carried in a hashtable the delegate closes
over.

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

**Line 330**, the `return $result` that fires when `$projectPaths.Count -eq 0`. The result object
is constructed with `Skipped = $true` and `FixedCount = 0` before that point, so the early return
is what preserves both.

**How it fails.** The `ReadText` count is the discriminating assertion. If the function read a
project file it never found, the count would be 1 and the test fails. Against a real filesystem
that defect indexes an empty array and throws under `Set-StrictMode -Version Latest`, so the guard
is load-bearing rather than cosmetic. An assertion on `Skipped` alone would not distinguish the
two, because a function that threw would never return a result at all.

## Output Summary

One test executed, one passed, exit 0. Line 330 is now reached, and the never-read property is
asserted rather than inferred.
