# [P6-T5] Pass-after run for issue #469 defects 1 and 2

Timestamp: 2026-08-26T10-22

Command:

```
pwsh -NoProfile -File scripts/vscode/Invoke-VSBuild.ps1 -Target Build   # precondition, EXIT_CODE 0

$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
$vstest  = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll `
    /Settings:scripts\vscode\TaskMaster.cli.runsettings `
    /InIsolation `
    /TestCaseFilter:"FullyQualifiedName~GetMoveDiagnostics_WithOneGroup_ReturnsExactlyOneLine|FullyQualifiedName~GetMoveDiagnostics_WithThreeGroups_ReturnsThreeLinesAndNoNulls|FullyQualifiedName~GetMoveDiagnostics_WithNullItemController_ReturnsUnknownLineWithoutThrowing" `
    /Logger:"trx;LogFileName=p6-t5.trx" `
    /ResultsDirectory:docs\features\active\qfc-collection-controller-defects-468\evidence\regression-testing\p6-t5
```

The three clauses are joined with `|`. vstest 18.x rejects the `OR` keyword inside
`/TestCaseFilter`.

EXIT_CODE: 0

## Output Summary

Build precondition: `EXIT_CODE 0`, 0 errors, 5 warnings (all pre-existing
`System.Reactive.PackagesConfigCheck` `packages.config` notices).

Test run: `Test Run Successful. Total tests: 3  Passed: 3`.

TRX `<Counters>`, verbatim from `evidence/regression-testing/p6-t5/p6-t5.trx`:

```
total="3" executed="3" passed="3" failed="0" error="0" timeout="0" aborted="0" inconclusive="0"
```

Passed count is exactly `3` and failed count is exactly `0`, as the task's acceptance requires.

## Red-to-green mapping

| Test | Fail-before artifact | Pre-fix observation | Post-fix |
|---|---|---|---|
| `GetMoveDiagnostics_WithOneGroup_ReturnsExactlyOneLine` | `p6-t1-fail-before.2026-08-26T10-17.md` | length 2, expected 1 | passed |
| `GetMoveDiagnostics_WithThreeGroups_ReturnsThreeLinesAndNoNulls` | `p6-t2-fail-before.2026-08-26T10-17.md` | length 4, expected 3, surplus element null | passed |
| `GetMoveDiagnostics_WithNullItemController_ReturnsUnknownLineWithoutThrowing` | `p6-t3-fail-before.2026-08-26T10-17.md` | `NullReferenceException` at `QfcCollectionController.cs:2097` | passed |

Every one of the three tests has a recorded, non-fabricated red state produced before the P6-T4 fix
was written.

## What P6-T4 changed

`QuickFiler/Controllers/QfcCollectionController.cs`, `GetMoveDiagnostics`, one edit pass:

1. The allocation lost its `+ 1`: `new string[_itemGroupsToMove.Count]`. The loop bound was already
   `Count`, so the array and the loop now agree and no element is left unassigned.
2. The item-controller null test moved to the top of the loop body as an early-out that writes the
   `Unknown` diagnostics line and `continue`s. It now dominates both former dereferences
   (`qf.ItemHelper` and the `xComma(qf.ItemHelper.Subject)` interpolation).
3. The trailing `if (qf is not null) / else` collapsed to its non-null arm. After the early-out `qf`
   is provably non-null there, so retaining the test would have reintroduced an unreachable branch
   — the same defect in a different position.

The `Unknown` line keeps the pre-existing column count of the data line by emitting an empty
subject column, so a downstream consumer parsing the metrics file by column index is unaffected.

## Two TRX files in this run directory

`p6-t5-attempt1-narrower-assertion.trx` is the first pass-after run, taken before the string
assertion in `GetMoveDiagnostics_WithNullItemController_ReturnsUnknownLineWithoutThrowing` was
widened from `Folder Unknown` to the full `To Unknown,Sender Unknown,Email,Folder Unknown` literal
that AC-4 in `spec.md` names. It also reports 3 passed / 0 failed.

`p6-t5.trx` is the authoritative run: it was taken after the widening, on the same tree that P6-T7
commits, and after a clean `csharpier check` (`EXIT_CODE 0`) and a clean build. Both files are
retained so the amendment is visible in the audit trail rather than overwritten. The disclosure is
also recorded in `p6-t3-fail-before.2026-08-26T10-17.md`.

## Host-identifier sanitisation

The authoritative TRX was sanitised case-insensitively before commit: 16 substitutions.
Post-sanitisation the file contains zero occurrences of any of the four host-identifier patterns
recorded in `evidence/other/host-identifier-sanitisation.2026-08-26T10-11.md`.
