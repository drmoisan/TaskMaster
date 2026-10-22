# [P10-T9] Issue #471 STA regression test, green after the one-place sign correction

Timestamp: 2026-08-26T11-19

Command:

```
# Precondition
pwsh -NoProfile -File scripts/vscode/Invoke-VSBuild.ps1 -Target Build      # EXIT_CODE 0, 0 errors

$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
$vstest  = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll `
    /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation `
    /TestCaseFilter:"FullyQualifiedName~EliminateSpaceForItems_ReducesMinimumHeightByTemplateHeightTimesRemovalCount" `
    /Logger:"trx;LogFileName=p10-t9.trx" `
    /ResultsDirectory:docs\features\active\qfc-collection-controller-defects-468\evidence\regression-testing\p10-t9
```

EXIT_CODE: 0

ExpectedExitCode: 0

## Output Summary

`Test Run Successful. Total tests: 1  Passed: 1`. Duration 123 ms.

TRX `<Counters>`, verbatim from `evidence/regression-testing/p10-t9/p10-t9.trx`:

```
total="1" executed="1" passed="1" failed="0" error="0" timeout="0" aborted="0" inconclusive="0"
```

Passed count is exactly `1`, as the task's acceptance requires.

## Red-to-green transition

| Run | Tree state | Result |
|---|---|---|
| P10-T6 | `ShrinkByRows` seam, sign not yet corrected | failed 1 — minimum height 250 px instead of 150 px |
| P10-T9 (this run) | one-place sign correction applied | passed 1 — minimum height 150 px |

Nothing else changed between the two runs except the sign of the row count computed at the
`EliminateSpaceForItems` call site.

## The correction is confined to one place

`git diff` of the fix, complete:

```
@@ QuickFiler/Controllers/QfcCollectionController.cs
             TableLayoutHelper.RemoveSpecificRow(_itemTlp, removalInex, removalCount);
 
-            var rowsToShrinkBy = -removalCount;
+            var rowsToShrinkBy = removalCount;
             _itemTlp.MinimumSize = ShrinkByRows(
```

One line, one sign, one call site. D8 requires exactly this: the seam is sign-agnostic and both of
its uses are legitimate, so only the argument computed at the removal site is corrected. The
insertion site in `MakeSpaceForItems` still passes `-insertCount` and is deliberately untouched —
a negative row count grows, which is how that path expresses "make room for N rows", and the
in-code comment there says so. The subtraction inside `ShrinkByRows` is likewise unchanged.

The local `rowsToShrinkBy` exists so that the two `ShrinkByRows` calls at the removal site
(`MinimumSize` and `Size`) share a single computed row count. Without it the correction would
have had to be made twice, and "exactly one sign change" would not have been expressible.

## Host-identifier sanitisation

The TRX was sanitised case-insensitively in binary mode before commit: 10 substitutions. No
`Deploy_*` scaffolding directory was left behind. A post-sanitisation sweep returns zero hits for
every token class recorded in `evidence/other/host-identifier-sanitisation.2026-08-26T10-11.md`.
