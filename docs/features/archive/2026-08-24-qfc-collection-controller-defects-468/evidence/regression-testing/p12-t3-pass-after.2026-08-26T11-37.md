# [P12-T3] Issue #469 defect 4 undo-stack contract test

Timestamp: 2026-08-26T11-37

Command:

```
# Precondition
pwsh -NoProfile -File scripts/vscode/Invoke-VSBuild.ps1 -Target Build      # EXIT_CODE 0, 0 errors

$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
$vstest  = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll `
    /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation `
    /TestCaseFilter:"FullyQualifiedName~MoveEmailsAsync_WithNullStack_BehavesIdenticallyToAnEmptyStack" `
    /Logger:"trx;LogFileName=p12-t3.trx" `
    /ResultsDirectory:docs\features\active\qfc-collection-controller-defects-468\evidence\regression-testing\p12-t3
```

EXIT_CODE: 0

ExpectedExitCode: 0

## Output Summary

`Test Run Successful. Total tests: 1  Passed: 1`. Duration 74 ms.

TRX `<Counters>`, verbatim from `evidence/regression-testing/p12-t3/p12-t3.trx`:

```
total="1" executed="1" passed="1" failed="0" error="0" timeout="0" aborted="0" inconclusive="0"
```

Passed count is exactly `1`, as the task's acceptance requires.

## This test has no pre-fix red state, and that is by design

Phase 12 is a **documentation** change plus a discard. It deliberately alters no behaviour, so
there is no tree at which this test could have failed. Recording it as a fail-before would require
either fabricating a red state or changing behaviour the plan forbids changing. The test's value is
as a standing guard: it pins the contract the new XML documentation states, so a later change that
starts making the argument matter — for example an argument-null throw, or pushing onto the supplied
instance — turns this test red.

`WhyFailingRunImpossible:` the change is a comment plus a discard assignment; no observable
behaviour differs between the pre- and post-change trees, so no assertion over observable behaviour
can distinguish them.

## What the test asserts

With `_itemGroupsToMove` set to an empty list:

| Call | Assertion |
|---|---|
| `MoveEmailsAsync(null)` | does not throw |
| `MoveEmailsAsync(stack)` with an in-memory `SloStack<IMovedMailInfo>` | does not throw |
| the supplied stack afterwards | `Count == 0` |

The two calls are observationally identical, which is exactly the documented contract: the
parameter is retained for source compatibility and does not carry the undo records.

## Why the contract reads the way it does

`QfcFormController` initialises `_movedItems` from `_globals.AF.MovedMails`
(`QuickFiler/Controllers/QfcFormController.cs:49`) and passes that field as the argument
(`QfcFormController.EventHandlers.cs:225`). The email filer's
`EmailFiler.PushToUndoStack` pushes onto `Globals.AF.MovedMails`
(`UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailFiler.cs:185-189`) — the same instance,
reached through the same globals object. The argument is therefore redundant rather than ignored:
the records do arrive on the caller's stack, just not by way of this parameter.

Per D11 the parameter is **not** removed and
`QuickFiler/Controllers/QfcFormController.EventHandlers.cs` is **not** edited. Full removal is
recorded as a follow-up candidate.

## Test-hygiene properties

The `SloStack<IMovedMailInfo>` is built with its parameterless constructor, held only in a local,
and never serialized, so the test touches no file. No COM object, no live Outlook, no WinForms
control, no STA apartment, no wall-clock wait, no mutable global state.

## Host-identifier sanitisation

The TRX was sanitised case-insensitively in binary mode before commit. Any `Deploy_*` scaffolding
directory was removed. A post-sanitisation sweep returns zero hits for every token class recorded in
`evidence/other/host-identifier-sanitisation.2026-08-26T10-11.md`.
