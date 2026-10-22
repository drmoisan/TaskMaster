# R2 Line 293 — `Repair-ProjectReferenceVersion` Early Return, Version Already Agrees

- Timestamp: 2026-09-20T08-41-40
- Task: [P1-T6]
- Finding: R2
- Command: CMD-PESTER-FILTERED, `<FILE>` = `tests/scripts/vscode/Sync-PackageReferences.Tests.ps1`,
  `<FILTER>` = `*R2- returns the project text unchanged when the Reference already*`
- EXIT_CODE: 0

## Test Added

`It 'R2- returns the project text unchanged when the Reference already names the resolved version'`

Calls `Repair-ProjectReferenceVersion` with `-AssemblyVersion '2.0.0.0'`, exactly equal to the
four-part version the text's `Include` already declares, and asserts the returned string equals
the input exactly.

## Counts Line, Verbatim

```
PESTER Passed=1 Failed=0 Skipped=0 Executed=1 Total=11 NotRun=10
```

| Measurement | Required | Measured | Result |
|---|---|---|---|
| `EXIT_CODE` | 0 | 0 | PASS |
| `Executed` = `Passed + Failed + Skipped` | 1 | **1** | PASS |
| `Passed` | 1 | **1** | PASS |
| `Total` (context) | — | 11 | — |
| `NotRun` (context) | — | 10 | — |

Pester reported `Filters selected 1 tests to run.`

## What This Exercises

**Line 293**, the `return $ProjectText` that fires when the matched version group already equals
the resolved version.

This is the **idempotence** pin. Applying the repair twice must be a no-op: the first application
writes the resolved version, and the second must reach line 293 and return the text untouched.

**How it fails.** If an already-correct version were rewritten, the returned string would differ
from the input and `Should -BeExactly` fails. The practical consequence of that defect is that
every second run of the repair pass produces a spurious change, which would break AC15's
formatting-stable-tree property and would make the workflow's push gate fire on a run that
repaired nothing.

Line 290 and line 293 are two distinct early returns and are asserted by two distinct tests: line
290 is reached when the regular expression does not match at all, line 293 when it matches and the
captured version already agrees. A single test could reach only one of them.

## Output Summary

One test executed, one passed, exit 0. Line 293 is now reached and idempotence is pinned.
