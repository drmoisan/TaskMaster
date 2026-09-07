# [P13-T6] Pass-after run for issue #474 defect 2 (move-readiness inspectability)

Timestamp: 2026-08-26T16-18

Command:

```
pwsh -NoProfile -File scripts/vscode/Invoke-VSBuild.ps1 -Target Build   # precondition, EXIT_CODE 0

$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
$vstest  = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll `
    /Settings:scripts\vscode\TaskMaster.cli.runsettings `
    /InIsolation `
    /Logger:"trx;LogFileName=p13-t6.trx" `
    /ResultsDirectory:docs\features\active\qfc-collection-controller-defects-468\evidence\regression-testing\p13-t6 `
    /TestCaseFilter:"FullyQualifiedName~TryGetMoveReadiness_"
```

EXIT_CODE: 0

ExpectedExitCode: 0

## Output Summary

`Test Run Successful. Total tests: 2  Passed: 2`. Total time 1.80 s, first attempt.

TRX `<Counters>`, verbatim from `evidence/regression-testing/p13-t6/p13-t6.trx`:

```
total="2" executed="2" passed="2" failed="0" error="0" timeout="0" aborted="0" inconclusive="0"
```

Passed count is exactly `2` and failed count is exactly `0`, as the task's acceptance requires.

Per-test outcome:

| Test | Outcome | Duration |
|---|---|---|
| `TryGetMoveReadiness_WithUnassignedDestination_ReturnsFalseAndProducesNotificationText` | Passed | 215 ms |
| `TryGetMoveReadiness_WithAllDestinationsAssigned_ReturnsTrueAndEmptyNotification` | Passed | 1 ms |

## Why there is no fail-before run for these two tests

These two tests could not have been written, let alone run red, before the P13-T1 seam. Before the
seam the only way to evaluate move readiness was to read the `ReadyForMove` property, and the false
path of that property called `MessageBox.Show` directly. A unit test that read the property in the
pre-seam code would have blocked on a modal dialog that no test host can dismiss; the run would have
hung rather than failed. A hung run is not a red state and produces no TRX.

The seam replaced the direct modal call with a private injectable delegate `_notifyNotReady`, whose
default is the unchanged modal call with the same message, caption, buttons, and icon. Both tests
substitute a recording delegate for that default, so no dialog is presented. This is recorded in the
fail-before exception dossier authored at P14-T1 as the `#474-2` entry.

## What the two tests assert

`TryGetMoveReadiness_WithUnassignedDestination_ReturnsFalseAndProducesNotificationText`
arranges four groups: one with a null `SelectedFolder`, and one carrying each of the three
list-header sentinel strings, so all four "not assigned" shapes are covered in a single arrangement.
It asserts:

1. `TryGetMoveReadiness` returns `false`;
2. the `ReadyForMove` property, read through the same seam, also returns `false`, which is the
   behaviour-preservation half of the assertion;
3. the `out string notifications` value opens with the fixed banner
   `Can't complete actions! Not all emails assigned to folder` and contains all four subjects, so
   each of the four unassigned shapes contributed a line;
4. the text captured by the injected recording delegate is byte-equal to the `out` value, which is
   what establishes that the getter hands the predicate's text to the notification path unchanged.

`TryGetMoveReadiness_WithAllDestinationsAssigned_ReturnsTrueAndEmptyNotification` arranges a single
group with a real destination folder and asserts the returned value is `true` and the notification
string is empty.

Neither test creates a temporary file, uses `Thread.Sleep`, `Task.Delay`, `UiThread.Init`, or
`ShowDialog`, and neither requires a live Outlook, a WinForms control, or an STA apartment. Both use
MSTest, Moq, and FluentAssertions as the C# Unit Test Policy requires.

## Provenance of the two tests

Both tests are present in `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.cs` at
lines 427 and 473 and were committed to this branch at `48c9ad8f`. P13-T4 and P13-T5 were verified
against the committed source rather than re-authored; the acceptance criteria of both tasks were
checked against the file as committed and hold in full.

## Host-identifier sanitisation

The TRX was sanitised case-insensitively before commit: 13 substitutions across the four token
classes recorded in `evidence/other/host-identifier-sanitisation.2026-08-26T10-11.md`
(workspace-root prefix 5, machine name 5, account name 3, user-profile prefix 0, 8.3 short-name form
0). A post-sanitisation residual scan over the same four patterns returned 0 hits.
