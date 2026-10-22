# [P12-T4] Full `QuickFiler.Test` suite after the issue #469 defect 4 documentation change

Timestamp: 2026-08-26T11-37

Command:

```
$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
$vstest  = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll `
    /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation `
    /Logger:"trx;LogFileName=p12-t4.trx" `
    /ResultsDirectory:docs\features\active\qfc-collection-controller-defects-468\evidence\qa-gates\p12-t4
```

EXIT_CODE: 0

ExpectedExitCode: 0

## Output Summary

`Test Run Successful. Total tests: 964  Passed: 964`. Total time 11.21 s, first attempt.

TRX `<Counters>`, verbatim from `evidence/qa-gates/p12-t4/p12-t4.trx`:

```
total="964" executed="964" passed="964" failed="0" error="0" timeout="0" aborted="0" inconclusive="0"
```

Failed count is exactly `0`, as the task's acceptance requires.

## Suite-size accounting for Phase 12

| Run | Total | Passed | Failed | Delta |
|---|---|---|---|---|
| P11-T7 (end of Phase 11) | 963 | 963 | 0 | — |
| P12-T4 (this run) | 964 | 964 | 0 | +1 |

The one added test is `MoveEmailsAsync_WithNullStack_BehavesIdenticallyToAnEmptyStack` in
`QfcCollectionControllerDefects468MoveTests.cs`, named by P12-T3.

## Behaviour neutrality of Phase 12

Phase 12 changed one XML documentation block on the interface, one mirrored XML documentation block
on the implementation, and added a single discard statement `_ = stackMovedItems;` at the top of
`MoveEmailsAsync`. A discard of an already-evaluated parameter has no runtime effect. The three
pre-existing tests that call `MoveEmailsAsync(null)` continue to pass unchanged, which is the
evidence that no argument-null throw was introduced.

Per D11 the parameter was **not** removed, and
`QuickFiler/Controllers/QfcFormController.EventHandlers.cs` was **not** edited.

## File-size headroom

`QfcCollectionControllerDefects468MoveTests.cs` is 497 lines after this addition, three lines under
the hard 500-line cap. No further test is planned for that file; the two Phase 13 tests are assigned
by D12 to `QfcCollectionControllerDefects468Tests.cs`.

## Toolchain state at this run

| Step | Command | Result |
|---|---|---|
| Format | `dotnet tool run csharpier check .` | `EXIT_CODE 0`, 1,525 files checked, 0 needing formatting |
| Build | `pwsh -NoProfile -File scripts/vscode/Invoke-VSBuild.ps1 -Target Build` | `EXIT_CODE 0`, 0 errors, 5 pre-existing warnings |
| Test | this run | `EXIT_CODE 0`, 964 passed, 0 failed |

## Host-identifier sanitisation

The TRX was sanitised case-insensitively in binary mode before commit. Any `Deploy_*` scaffolding
directory was removed. A post-sanitisation sweep returns zero hits for every token class recorded in
`evidence/other/host-identifier-sanitisation.2026-08-26T10-11.md`.
