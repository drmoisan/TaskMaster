# [P7-T13] Full `QuickFiler.Test` suite after the issue #470 defect 2 fix

Timestamp: 2026-08-26T10-41

Command:

```
$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
$vstest  = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll `
    /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation `
    /Logger:"trx;LogFileName=p7-t13.trx" `
    /ResultsDirectory:docs\features\active\qfc-collection-controller-defects-468\evidence\qa-gates\p7-t13
```

EXIT_CODE: 0

## Output Summary

`Test Run Successful. Total tests: 955  Passed: 955`.

TRX `<Counters>`, verbatim from `evidence/qa-gates/p7-t13/p7-t13.trx`:

```
total="955" executed="955" passed="955" failed="0" error="0" timeout="0" aborted="0" inconclusive="0"
```

Failed count is exactly `0`, as the task's acceptance requires. The run completed on the first
attempt; no flaky retry was needed.

## Suite-size accounting

| Run | Total | Passed | Failed |
|---|---|---|---|
| P5-T6 (end of Phase 5) | 946 | 946 | 0 |
| P6-T6 (end of Phase 6) | 949 | 949 | 0 |
| P7-T13 (this run) | 955 | 955 | 0 |

The delta of `+6` is exactly the six tests in
`QuickFiler.Test/Controllers/QfcCollectionControllerDefects468ConversationTests.cs`, added by
P7-T1 and P7-T7 through P7-T11. No test was removed and no previously passing test regressed.

## Non-regression significance for this phase in particular

Phase 7 is the first phase in this plan that changes a **signature** on a public member
(`EnumerateConversationMembers`) and adds an early-return path to a member on the live VSTO event
path (`ToggleUnGroupConv`). Two existing tests drive `ToggleUnGroupConv` through a mocked parent —
`QfcItemController.MailActionsTests.cs:179` and `QfcItemController.SeamDispatcherTests.cs:177` —
and both still pass, because that member's signature is unchanged. `EnumerateConversationMembers`
has no caller outside `QfcCollectionController.cs`, verified by a repository-wide source search
before the retype.

## Toolchain state at this run

| Step | Command | Result |
|---|---|---|
| Format | `dotnet tool run csharpier check .` | `EXIT_CODE 0`, 1,524 files checked |
| Build | `pwsh -NoProfile -File scripts/vscode/Invoke-VSBuild.ps1 -Target Build` | `EXIT_CODE 0`, 0 errors |
| Test | this run | `EXIT_CODE 0`, 955 passed, 0 failed |

## Host-identifier sanitisation

The TRX was sanitised case-insensitively before commit: 2,872 substitutions. Post-sanitisation the
file contains zero occurrences of any of the four host-identifier patterns recorded in
`evidence/other/host-identifier-sanitisation.2026-08-26T10-11.md`. The empty
`Deploy_<user> <timestamp>_<pid>` scaffolding directory vstest creates in the results directory was
removed.
