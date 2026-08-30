# Baseline — AC-0 counting figures re-derived with executed commands ([P0-T7])

- Issue: #644
- Task: `[P0-T7]`
- Timestamp: 2026-08-29T08-15
- Shell: PowerShell (`pwsh -NoProfile`), working directory repository root (`<repo-root>`)

AC-0's sixth figure, the repository coverage figure, is captured separately by `[P0-T12]`.

The spec records a baseline caveat: the research session had no shell, so all five figures below
were derived by reading files rather than by running counting commands. This task re-derives them
with executed commands, as AC-0 and the plan require, before any of them is used as a gate
baseline.

## Measured values against expected values

| # | Command | Expected (spec) | Measured | EXIT_CODE | Discrepancy |
|---|---|---|---|---|---|
| 1 | `(Get-Content QuickFiler.Test\Controllers\QfcCollectionControllerTests.cs).Count` | 500 | **500** | 0 | none |
| 2 | `(Select-String -Path QuickFiler.Test\Controllers\QfcCollectionControllerTests.cs -Pattern '\[TestMethod\]').Count` | 13 | **13** | 0 | none |
| 3 | `(Get-Content QuickFiler.Test\Controllers\QfcCollectionControllerNavigationDigitsTests.cs).Count` | 226 | **226** | 0 | none |
| 4 | `(Select-String -Path QuickFiler.Test\Controllers\QfcCollectionControllerNavigationDigitsTests.cs -Pattern '\[TestMethod\]').Count` | 3 | **3** | 0 | none |
| 5 | `(Get-Content QuickFiler\Controllers\QfcCollectionController.cs).Count` | 2437 | **2437** | 0 | none |

Raw command output:

```
C1=500 EXIT=True
C2=13 EXIT=True
C3=226 EXIT=True
C4=3 EXIT=True
C5=2437 EXIT=True
```

`EXIT=True` is the PowerShell `$?` success indicator immediately after each expression; every one
of the five reported success, so every command's exit status is 0.

## Baseline values consumed by later gates

`[P3-T6]` and `[P4-T7]` are evaluated against these **measured** values:

- `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs` — line-count baseline **500**,
  `[TestMethod]` baseline **13**. The file is exactly at the 500-line repository ceiling, so it
  may not grow by even one line.
- `QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs` — line-count
  baseline **226**, `[TestMethod]` baseline **3**.
- `QuickFiler/Controllers/QfcCollectionController.cs` — line-count baseline **2437**. This is a
  known pre-existing 500-line-ceiling violation, recorded and deliberately not fixed by this plan.

EXIT_CODE: 0

Output Summary: All five counting figures were re-derived with executed PowerShell commands and
every one matches the spec's expected value exactly — 500 / 13 / 226 / 3 / 2437. **Zero
discrepancies.** The measured baseline and the expected baseline coincide, so `[P3-T6]` and
`[P4-T7]` are evaluated against 500, 13, 226, and 3 without adjustment.
