# [P10-T10] Issue #471 AC-11 minimum-height neutrality, both STA tests green

Timestamp: 2026-08-26T11-21

Command:

```
# Precondition
pwsh -NoProfile -File scripts/vscode/Invoke-VSBuild.ps1 -Target Build      # EXIT_CODE 0, 0 errors

$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
$vstest  = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll `
    /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation `
    /TestCaseFilter:"FullyQualifiedName~EliminateSpaceForItems_ReducesMinimumHeightByTemplateHeightTimesRemovalCount|FullyQualifiedName~MakeSpaceThenEliminateSpace_IsMinimumHeightNeutral" `
    /Logger:"trx;LogFileName=p10-t10.trx" `
    /ResultsDirectory:docs\features\active\qfc-collection-controller-defects-468\evidence\regression-testing\p10-t10
```

EXIT_CODE: 0

ExpectedExitCode: 0

## Output Summary

`Test Run Successful. Total tests: 2  Passed: 2`.

| Test | Result | Duration |
|---|---|---|
| `EliminateSpaceForItems_ReducesMinimumHeightByTemplateHeightTimesRemovalCount` | Passed | 104 ms |
| `MakeSpaceThenEliminateSpace_IsMinimumHeightNeutral` | Passed | 2 ms |

TRX `<Counters>`, verbatim from `evidence/regression-testing/p10-t10/p10-t10.trx`:

```
total="2" executed="2" passed="2" failed="0" error="0" timeout="0" aborted="0" inconclusive="0"
```

Passed count is exactly `2` and failed count exactly `0`, as the task's acceptance requires.

The two `/TestCaseFilter` clauses are joined with `|`. vstest 18.x rejects `OR` inside a filter
expression.

## The neutrality assertion, and why it is scoped to minimum height (D10)

The test records the panel's starting `MinimumSize.Height`, calls `MakeSpaceForItems(0, 2)`, then
calls `EliminateSpaceForItems(0, 2)` with the same index and the same count, and asserts the
minimum height is back at its recorded starting value. It passes.

**`Size.Height` is deliberately not asserted, and this is the required explicit record of that
asymmetry.** `MakeSpaceForItems` has exactly two statements in its body:

1. an assignment to `_itemTlp.MinimumSize`, and
2. a call to `TableLayoutHelper.InsertSpecificRow`.

Neither references `_itemTlp.Size`. `EliminateSpaceForItems`, by contrast, assigns **both**
`_itemTlp.MinimumSize` and `_itemTlp.Size`. Consequently:

- `Size.Height` is **reduced** by `EliminateSpaceForItems` and is **not restored** by
  `MakeSpaceForItems`, because no statement in `MakeSpaceForItems` can restore it.
- The pairing therefore cannot be size-height-neutral by construction. Asserting size-height
  neutrality would assert a property the production code has never had, and the assertion would be
  measuring the WinForms minimum-size clamp rather than this feature's arithmetic.

**This asymmetry is pre-existing and is NOT changed by this feature.** It predates the `ShrinkByRows`
seam and predates the sign correction; before the fix, `Size` was likewise assigned only on the
removal path. Issue #471 is about the *direction* of the removal-path adjustment, not about the
presence or absence of a size adjustment on the insertion path. Restoring symmetry between the two
methods would change the behaviour of the insertion path, which is out of scope here and is recorded
as a follow-up candidate rather than fixed.

One clarification for a future reader: in a panel whose `Size` is currently sitting at its
`MinimumSize`, raising `MinimumSize` causes `Control.SetBoundsCore` to lift `Size` with it. That
lift is a framework clamp, not a restoration performed by `MakeSpaceForItems`, and it is why a
naive size-height assertion could appear to pass on some fixtures and fail on others. Scoping the
assertion to `MinimumSize` removes that fixture dependence.

## STA hygiene of the added test

`Show()` and `ShowDialog()` are never called anywhere in
`QuickFiler.Test/Controllers/QfcCollectionControllerLayout.StaTests.cs`; the panel is never
parented, so no window handle is created; no message pump is required; the panel is disposed in
`[TestCleanup]`; no temporary file is created; no wall-clock wait is used. The file is 183 lines,
well under the 500-line cap, and declares exactly two tests — the one call-site sign test and this
neutrality test.

## Host-identifier sanitisation

The TRX was sanitised case-insensitively in binary mode before commit: 13 substitutions. No
`Deploy_*` scaffolding directory was left behind. A post-sanitisation sweep returns zero hits for
every token class recorded in `evidence/other/host-identifier-sanitisation.2026-08-26T10-11.md`.
