# [P6-T1] [expect-fail] `GetMoveDiagnostics_WithOneGroup_ReturnsExactlyOneLine`

Timestamp: 2026-08-26T10-17

Issue #469 defect 1 (diagnostics array length off by one).

Command:

```
pwsh -NoProfile -File scripts/vscode/Invoke-VSBuild.ps1 -Target Build   # precondition, EXIT_CODE 0

$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
$vstest  = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll `
    /Settings:scripts\vscode\TaskMaster.cli.runsettings `
    /InIsolation `
    /TestCaseFilter:"FullyQualifiedName~GetMoveDiagnostics_WithOneGroup_ReturnsExactlyOneLine" `
    /Logger:"trx;LogFileName=p6-t1.trx" `
    /ResultsDirectory:docs\features\active\qfc-collection-controller-defects-468\evidence\regression-testing\p6-t1
```

EXIT_CODE: 1

ExpectedExitCode: 1

## Output Summary

Build precondition: `EXIT_CODE 0`, 0 errors, 5 warnings (all pre-existing
`System.Reactive.PackagesConfigCheck` `packages.config` notices).

Test run: `Test Run Failed. Total tests: 1  Failed: 1`, 2.4038 seconds.

TRX `<Counters>`, verbatim from
`evidence/regression-testing/p6-t1/p6-t1.trx`:

```
total="1" executed="1" passed="0" failed="1" error="0" timeout="0" aborted="0" inconclusive="0"
```

Failed count is exactly `1`, as the task's acceptance requires.

## Acceptance: the observed length is one greater than the expected length

The recorded failure message is:

```
Expected lines to contain 1 item(s) because issue #469 defect 1 requires one diagnostics line per
cached move group; a length greater than the group count is the surplus unassigned element produced
by the off-by-one allocation, but found 2:
{"01/01/2026,12:00, Subject 0,QuickFiled,5,0.08,Recipient 0,Sender 0,Email,Inbox,01/01/2026,00:00",
<null>}
```

Expected `1`, observed `2`. The second element is `<null>` — it is the element the allocation
reserves and the loop never assigns, which is the defect in its most direct form. The consumer at
`QuickFiler/Controllers/QfcHomeController.Metrics.cs:75` and `:144` writes every element of this
array to the metrics output, so the surplus element becomes a blank diagnostics row attributed to a
message that does not exist.

## Pre-fix source state

`QuickFiler/Controllers/QfcCollectionController.cs`, inside `GetMoveDiagnostics`:

```
string[] strOutput = new string[_itemGroupsToMove.Count + 1];
var loopTo = _itemGroupsToMove.Count;
for (k = 0; k < loopTo; k++)
```

The allocation is `Count + 1` while the loop bound is `Count`, so index `Count` is allocated and
never written.

## Host-identifier sanitisation

The TRX was sanitised **case-insensitively** before commit: 11 substitutions, covering the
workspace-root prefix, the user-profile path, the machine name, and the account name. A
case-sensitive substitution is not sufficient, because vstest writes the `storage=` attribute of
every `<UnitTest>` element in all-lower-case; see
`evidence/other/host-identifier-sanitisation.2026-08-26T10-11.md`. Post-sanitisation the file
contains zero occurrences of any of the four host-identifier patterns.
