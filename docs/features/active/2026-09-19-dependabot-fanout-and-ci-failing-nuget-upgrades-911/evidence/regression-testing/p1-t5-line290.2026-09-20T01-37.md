# R2 Line 290 — `Repair-ProjectReferenceVersion` Early Return, No Matching `Include`

- Timestamp: 2026-09-20T08-41-05
- Task: [P1-T5]
- Finding: R2
- Command: CMD-PESTER-FILTERED, `<FILE>` = `tests/scripts/vscode/Sync-PackageReferences.Tests.ps1`,
  `<FILTER>` = `*R2- returns the project text unchanged when no Reference*`
- EXIT_CODE: 0

## Test Added

`It 'R2- returns the project text unchanged when no Reference names the assembly'`

Calls `Repair-ProjectReferenceVersion` with `-AssemblyName 'Fabrikam.Core'`, which appears in no
`Include` attribute of the supplied text, and asserts the returned string equals the input
**exactly**, by `Should -BeExactly`.

`Should -BeExactly` rather than `Should -Be` is deliberate: the comparison is case-sensitive and
ordinal, so a rewrite that differed only in the casing of the identifier would fail rather than
pass.

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

**Bookkeeping note.** This `It` and the one [P1-T6] records were added to the file in a single
edit, so at the time of this run the file already carried 11 `It` blocks rather than 10. `Total`
is context only and is not asserted, for the reason **gate rule 2** gives. The filter selected
exactly one test — Pester reported `Filters selected 1 tests to run.` — and `Executed=1` is the
figure this task asserts.

## What This Exercises

**Line 290**, the `return $ProjectText` that fires when the `Include="<name>, Version=<n.n.n.n>"`
regular expression does not match.

**How it fails.** If a non-matching assembly name mutated the text, `Should -BeExactly` fails. The
practical consequence of that defect would be a rewrite applied to whichever `Reference` the
regular expression happened to reach first, which is the class of defect R5 reports in the module
layer.

## Output Summary

One test executed, one passed, exit 0. Line 290 is now reached.
