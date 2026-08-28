# [P7-T3] [expect-fail] `ConversationReconciliationHelpersExist`

Timestamp: 2026-08-26T10-33

Issue #470 defect 2 (conversation insertion count derived from two disagreeing sources).

Command:

```
dotnet tool run csharpier check .                                       # EXIT_CODE 0, 1,524 files
pwsh -NoProfile -File scripts/vscode/Invoke-VSBuild.ps1 -Target Build   # precondition, EXIT_CODE 0

$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
$vstest  = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll `
    /Settings:scripts\vscode\TaskMaster.cli.runsettings `
    /InIsolation `
    /TestCaseFilter:"FullyQualifiedName~ConversationReconciliationHelpersExist" `
    /Logger:"trx;LogFileName=p7-t3.trx" `
    /ResultsDirectory:docs\features\active\qfc-collection-controller-defects-468\evidence\regression-testing\p7-t3
```

EXIT_CODE: 1

ExpectedExitCode: 1

## Output Summary

Build precondition: `EXIT_CODE 0`, 0 errors. The new test file compiles because it looks both
members up by name through reflection; it does not name them in source, so their absence is a
runtime failure rather than a compile error. That is what makes a clean, attributable red state
possible here.

Test run: `Test Run Failed. Total tests: 1  Failed: 1`.

TRX `<Counters>`, verbatim from `evidence/regression-testing/p7-t3/p7-t3.trx`:

```
total="1" executed="1" passed="0" failed="1" error="0" timeout="0" aborted="0" inconclusive="0"
```

Failed count is exactly `1`, as the task's acceptance requires.

## Recorded failure

```
Expected resolve not to be <null> because issue #470 defect 2 extracts the conversation
member-resolution expression into a pure static helper so it can be resolved once, before
MakeSpaceForItems, instead of being re-resolved inside the loop.
```

The first assertion fails, so the run stops before reaching the `ReconcileInsertionCount` lookup.
Both members are absent at this commit; the recorded message names the first.

## Pre-fix source state

In `QuickFiler/Controllers/QfcCollectionController.cs`:

- `ToggleUnGroupConv` computes `int insertCount = conversationCount - 1;` from the caller-supplied
  `conversationCount` and passes that reservation to `MakeSpaceForItems` and `InsertItemGroups`.
- `EnumerateConversationMembers` independently re-resolves the member list with
  `resolver.ConversationItems.SameFolder.Where(...).OrderByDescending(...).ToList()` and then loops
  over `insertions.Count`.

Nothing reconciles the two. When `insertions.Count` exceeds the reservation the loop indexes past
the rows that were inserted; when it is smaller, rows are left empty. Neither condition is
detected, logged, or reported. There is no static helper of either name to look up, which is
exactly what this test records.

## Files added by P7-T1 and P7-T2

- `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468ConversationTests.cs`, 77 lines,
  one `[TestMethod]`. Well under the 500-line repository cap, with room for the five further tests
  P7-T7 through P7-T11 add.
- One `<Compile Include>` line in `QuickFiler.Test/QuickFiler.Test.csproj`, placed immediately
  after the `QfcCollectionControllerDefects468MoveTests.cs` entry and immediately before the
  `QfcDatamodelTests.cs` entry, per D13. `git diff --stat` on the csproj reports
  `1 file changed, 1 insertion(+)`: no other line of the project file changed.

## Host-identifier sanitisation

The TRX was sanitised case-insensitively before commit: 11 substitutions. Post-sanitisation the
file contains zero occurrences of any of the four host-identifier patterns recorded in
`evidence/other/host-identifier-sanitisation.2026-08-26T10-11.md`.
