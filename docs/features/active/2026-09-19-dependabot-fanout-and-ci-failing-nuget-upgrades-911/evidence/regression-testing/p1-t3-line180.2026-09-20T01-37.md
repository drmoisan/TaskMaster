# R2 Line 180 — `Resolve-PackageAssetFolder` Returns Empty for an Absent Library Directory

- Timestamp: 2026-09-20T08-38-55
- Task: [P1-T3]
- Finding: R2
- Command: CMD-PESTER-FILTERED, `<FILE>` = `tests/scripts/vscode/Sync-PackageReferences.Tests.ps1`,
  `<FILTER>` = `*R2- returns no asset folder*`
- EXIT_CODE: 0

## Test Added

`It 'R2- returns no asset folder when the library directory is absent'`

Calls `Resolve-PackageAssetFolder` with a seam whose `TestPath` delegate returns `$false` for every
path, and asserts both that the result is empty and that the seam's `ListAssetFolder` delegate was
**never invoked**. The invocation count is carried in a hashtable the delegate closes over, so the
second assertion reads a real observation rather than a mock expectation.

## Counts Line, Verbatim

```
PESTER Passed=1 Failed=0 Skipped=0 Executed=1 Total=8 NotRun=7
```

| Measurement | Required | Measured | Result |
|---|---|---|---|
| `EXIT_CODE` | 0 | 0 | PASS |
| `Executed` = `Passed + Failed + Skipped` | 1 | **1** | PASS |
| `Passed` | 1 | **1** | PASS |
| `Total` (context) | — | 8 | — |
| `NotRun` (context) | — | 7 | — |

`Total` rose from 7 to 8 because the file now carries one more `It`; it is invariant under the
filter and is not asserted. Pester reported `Filters selected 1 tests to run.`

## What This Exercises

**Line 180**, the `return ''` guard in `Resolve-PackageAssetFolder` that fires when
`& $Seam.TestPath $LibraryDirectory` is false.

**How it fails.** The `ListAssetFolder` count is the discriminating assertion. If the function
enumerated a directory it had not confirmed exists, the count would be 1 and the test would fail
even though the returned value might still be empty. A result-only assertion would pass in that
state, because an enumerator over a non-existent directory returns nothing anyway. Against the real
filesystem the same defect throws rather than returning empty, so the guard is load-bearing.

## Output Summary

One test added, one executed, one passed, exit 0. Line 180 is now reached, and the
never-enumerated property is asserted rather than inferred.
