# [P4-T6] Final full-suite test run

Timestamp: 2026-08-27T19-53
Command:
```powershell
$assemblies = @(Get-ChildItem -Path . -Recurse -Filter *.Test.dll |
  Where-Object {
    $_.FullName -like '*\bin\Debug\*' -and
    $_.FullName -notlike '*\obj\*' -and
    $_.FullName -notlike '*\ref\*'
  } | ForEach-Object { Resolve-Path -Relative $_.FullName } |
  Where-Object { $_ -notlike '*\.claude\*' })
& $vstest @assemblies /Settings:scripts\vscode\TaskMaster.cli.runsettings /EnableCodeCoverage /InIsolation /Logger:"trx;LogFileName=p4-t6-final.trx" /ResultsDirectory:docs\features\active\quickfiler-keyboard-action-defects-444\evidence\qa-gates\p4-t6 /TestCaseFilter:"TestCategory!=LiveOutlook"
```
EXIT_CODE: 0
Output Summary: `Test Run Successful.` `Total tests: 6713`, `Passed: 6713`, failed 0, skipped 0.
The failed set is empty, so the `RECONCILED-AGAINST-BASELINE` branch is **not taken**.

`vstest.console.exe` resolved to
`<program-files>\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe`.
The run was launched from `WS` under `pwsh -NoProfile`, not through a POSIX shell, so no switch was
rewritten into a drive-style path.

## Discovered test assemblies (paths expressed relative to `WS`)

`DISCOVERED_COUNT=9`.

```
.\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll
.\SVGControl.Test\bin\Debug\SVGControl.Test.dll
.\Tags.Test\bin\Debug\Tags.Test.dll
.\TaskMaster.Test\bin\Debug\TaskMaster.Test.dll
.\TaskTree.Test\bin\Debug\TaskTree.Test.dll
.\TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll
.\ToDoModel.Test\bin\Debug\ToDoModel.Test.dll
.\UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll
.\VBFunctions.Test\bin\Debug\VBFunctions.Test.dll
```

The `.claude` exclusion is applied to the path **relative to `WS`**, not to the absolute `FullName`.
This execution worktree itself sits beneath a `.claude` segment, so every absolute `FullName`
contains `\.claude\` and an absolute-path exclusion would discard all nine assemblies and hand
`vstest` an empty input set. The `Resolve-Path -Relative` projection is applied first and the
exclusion evaluated against its output; none of the nine relative paths contains a `.claude`
segment. The set matches the nine assemblies discovered at `[P0-T20]` exactly.

## Result counts

| Measure | This run | `[P0-T20]` baseline |
| --- | --- | --- |
| Total | 6713 | 6686 |
| Passed | 6713 | 6686 |
| Failed | **0** | 0 |
| Skipped | 0 | 0 |
| Total test time | 42.52 s | 38.64 s |

```
FailedTestNames = none
BaselineFailureSet (from [P0-T20]) = none
```

The total rose by 27 against the Phase 0 baseline. That increase covers both this feature's new
regression tests and the tests brought in by the sibling feature merged into this branch's base
before this phase ran; either way it is an increase in the passing set with no failure.

## Reconciliation branch

**Not taken.** The failed set is empty, so `RECONCILED-AGAINST-BASELINE` is not recorded and no
test name needs reconciling against `BaselineFailureSet`. No failed name belongs to
`KbdActionsTests`, `KbdActionsRemainingBranchesTests`,
`QfcCollectionControllerNavigationDigitsTests`, `QfcItemController_NavigationTests`, or
`QfcCollectionControllerTests`, because there is no failed name at all.

## Acceptance

- The discovered assembly list is non-empty — met (9 assemblies).
- `.\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll` is a member of it — met; the
  `Resolve-Path -Relative` projection supplies the `.\` prefix, as recorded verbatim above.
- No discovered assembly path expressed relative to `WS` contains a `.claude` segment — met.
- The failed count is `0` — met, so the reconciliation branch is not required.

The raw TRX is at
`docs/features/active/quickfiler-keyboard-action-defects-444/evidence/qa-gates/p4-t6/p4-t6-final.trx`
and is normalized for host-identifying values by `[P4-T7]`. The binary `.coverage` attachments
written alongside it are matched by `.gitignore:140` (`*.coverage`) and are therefore not committed.
