# [P6-T2] [expect-fail] `GetMoveDiagnostics_WithThreeGroups_ReturnsThreeLinesAndNoNulls`

Timestamp: 2026-08-26T10-17

Issue #469 defect 1 (diagnostics array length off by one), multi-group case.

Command:

```
pwsh -NoProfile -File scripts/vscode/Invoke-VSBuild.ps1 -Target Build   # precondition, EXIT_CODE 0

$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
$vstest  = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll `
    /Settings:scripts\vscode\TaskMaster.cli.runsettings `
    /InIsolation `
    /TestCaseFilter:"FullyQualifiedName~GetMoveDiagnostics_WithThreeGroups_ReturnsThreeLinesAndNoNulls" `
    /Logger:"trx;LogFileName=p6-t2.trx" `
    /ResultsDirectory:docs\features\active\qfc-collection-controller-defects-468\evidence\regression-testing\p6-t2
```

EXIT_CODE: 1

ExpectedExitCode: 1

## Output Summary

Build precondition: `EXIT_CODE 0`, 0 errors, 5 warnings (all pre-existing
`System.Reactive.PackagesConfigCheck` `packages.config` notices).

Test run: `Test Run Failed. Total tests: 1  Failed: 1`.

TRX `<Counters>`, verbatim from `evidence/regression-testing/p6-t2/p6-t2.trx`:

```
total="1" executed="1" passed="0" failed="1" error="0" timeout="0" aborted="0" inconclusive="0"
```

Failed count is exactly `1`, as the task's acceptance requires.

## Recorded failure

```
Expected lines to contain 3 item(s) because issue #469 defect 1 requires exactly one diagnostics
line per cached move group, and three groups were cached, but found 4:
{"01/01/2026,12:00, Subject 0,QuickFiled,5,0.08,Recipient 0,Sender 0,Email,Inbox,01/01/2026,00:00",
 "01/01/2026,12:00, Subject 1,QuickFiled,5,0.08,Recipient 1,Sender 1,Email,Inbox,01/01/2026,00:00",
 "01/01/2026,12:00, Subject 2,QuickFiled,5,0.08,Recipient 2,Sender 2,Email,Inbox,01/01/2026,00:00",
 <null>}
```

Three groups produce four elements, and the surplus fourth element is `<null>`. The length
assertion fails first, so the `NotContainNulls` assertion is not reached in the pre-fix run; the
`<null>` visible in the length message is nevertheless the same surplus element that assertion
targets after the fix.

This is the same defect as P6-T1 at a different group count, which establishes that the surplus is
a constant `+ 1` on the allocation rather than an artefact of the single-group arrangement.

## Host-identifier sanitisation

The TRX was sanitised case-insensitively before commit: 11 substitutions. Post-sanitisation the
file contains zero occurrences of any of the four host-identifier patterns recorded in
`evidence/other/host-identifier-sanitisation.2026-08-26T10-11.md`.
