# [P7-T12] Pass-after run for issue #470 defect 2

Timestamp: 2026-08-26T10-39

Command:

```
dotnet tool run csharpier check .                                       # EXIT_CODE 0, 1,524 files
pwsh -NoProfile -File scripts/vscode/Invoke-VSBuild.ps1 -Target Build   # precondition, EXIT_CODE 0

$vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
$vstest  = & $vswhere -latest -products * -find 'Common7\IDE\Extensions\TestPlatform\vstest.console.exe' | Select-Object -First 1
& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll `
    /Settings:scripts\vscode\TaskMaster.cli.runsettings `
    /InIsolation `
    /TestCaseFilter:"FullyQualifiedName~ConversationReconciliationHelpersExist|FullyQualifiedName~ResolveConversationInsertions_ExcludesBaseEntryAndOrdersBySentOnDescending|FullyQualifiedName~ReconcileInsertionCount_AboveReservation_ReturnsInsertionsCountAndWarnsOnce|FullyQualifiedName~ReconcileInsertionCount_EqualToReservation_ReturnsInsertionsCountAndDoesNotWarn|FullyQualifiedName~ReconcileInsertionCount_BelowReservation_ReturnsInsertionsCountAndWarnsOnce|FullyQualifiedName~EnumerateConversationMembers_WithNoInsertions_DoesNotThrow" `
    /Logger:"trx;LogFileName=p7-t12.trx" `
    /ResultsDirectory:docs\features\active\qfc-collection-controller-defects-468\evidence\regression-testing\p7-t12
```

Six clauses joined with `|`; vstest 18.x rejects `OR`.

EXIT_CODE: 0

## Output Summary

`Test Run Successful. Total tests: 6  Passed: 6`.

TRX `<Counters>`, verbatim from `evidence/regression-testing/p7-t12/p7-t12.trx`:

```
total="6" executed="6" passed="6" failed="0" error="0" timeout="0" aborted="0" inconclusive="0"
```

Passed count is exactly `6` and failed count is exactly `0`, as the task's acceptance requires.

## The six tests

| Test | Task | What it fixes in place |
|---|---|---|
| `ConversationReconciliationHelpersExist` | P7-T1 | both helpers exist and are static |
| `ResolveConversationInsertions_ExcludesBaseEntryAndOrdersBySentOnDescending` | P7-T7 | resolution excludes the base entry and orders newest first |
| `ReconcileInsertionCount_AboveReservation_ReturnsInsertionsCountAndWarnsOnce` | P7-T8 | surplus members: resolved count wins, one warning |
| `ReconcileInsertionCount_EqualToReservation_ReturnsInsertionsCountAndDoesNotWarn` | P7-T9 | agreement: resolved count returned, no warning |
| `ReconcileInsertionCount_BelowReservation_ReturnsInsertionsCountAndWarnsOnce` | P7-T10 | shortfall: resolved count wins, one warning |
| `EnumerateConversationMembers_WithNoInsertions_DoesNotThrow` | P7-T11 | the enumerator consumes the supplied list and issues no resolver query |

`ConversationReconciliationHelpersExist` is the one with a recorded pre-fix red state:
`p7-t3-fail-before.2026-08-26T10-33.md`, `ExpectedExitCode: 1`, failed count 1.

## What P7-T4, P7-T5 and P7-T6 changed

`QuickFiler/Controllers/QfcCollectionController.cs`:

1. **`ResolveConversationInsertions(ConversationResolver, string)`** — the member-resolution
   expression lifted out of `EnumerateConversationMembers` unchanged: filter out the base entry,
   order by `SentOn` descending, materialise. `internal static`, reads no field.
2. **`ReconcileInsertionCount(string, int, int, int, int, int, System.Action<string>)`** — returns
   `insertionsCount` unconditionally and invokes `warn` once, and only on the branch
   `insertionsCount != conversationCount - 1`. Contains no `throw`, per D5.
3. **`ToggleUnGroupConv`** — now returns early (restoring navigation and layout state) when
   `baseEmailIndex == -1`; resolves the member list exactly once, before `MakeSpaceForItems`;
   derives `insertCount` from `ReconcileInsertionCount`; and passes the resolved list into
   `EnumerateConversationMembers`. The loop is **not** clamped.
4. **`EnumerateConversationMembers`** — lost its dead `conversationCount` parameter, gained
   `IReadOnlyList<MailItem> insertions`, and no longer queries the resolver.

Verified in source: `ResolveConversationInsertions` is called at `:1635` and `MakeSpaceForItems` at
`:1648`, so the single resolution provably precedes the reservation.

`EnumerateConversationMembers` is not declared on `IQfcCollectionController`, so its signature
change breaks no interface. The only external caller of `ToggleUnGroupConv`,
`QuickFiler/Controllers/QfcItemController.MailActions.cs:41`, is unchanged: that member's signature
is untouched.

### One parameter deliberately retained

`EnumerateConversationMembers` keeps its `entryID` parameter although the body no longer reads it,
because member filtering moved into `ResolveConversationInsertions`. D6 scopes this signature change
to replacing `conversationCount` with `insertions`; removing a second parameter is a separate change
and is not taken here. The parameter's XML documentation states this plainly rather than implying a
use that does not exist.

## Pre-fix behavioural red states with no permanent post-fix counterpart (D7)

Two pre-fix behaviours cannot be captured as a failing test run, and per D7 they are recorded here
so P14-T1's fail-before exception dossier can carry them:

### 1. Above-reservation `ArgumentOutOfRangeException` at the `ToggleUnGroupConv` level

`WhyFailingRunImpossible:` `ToggleUnGroupConv` cannot be driven COM-free. Its first two statements
are `SafeSetTlpLayout(false)` and `UnregisterNavigation()`, and `MakeSpaceForItems` reaches
`TableLayoutHelper.InsertSpecificRow` on the WinForms `_itemTlp`. Reaching the loop that raised the
exception requires a realised table layout panel and live `QfcItemController` instances, which the
repository's test policy prohibits constructing.

Alternative proof: the mechanism is fully determined by the pre-fix source. `ToggleUnGroupConv`
reserved `conversationCount - 1` rows through `MakeSpaceForItems` and `InsertItemGroups`, while
`EnumerateConversationMembers` looped `Enumerable.Range(0, insertions.Count)` over an
independently resolved list and indexed `_itemGroups[i + insertionIndex]`. When
`insertions.Count > conversationCount - 1` the index exceeds the rows that were inserted. The
permanent assertion is `ReconcileInsertionCount_AboveReservation_ReturnsInsertionsCountAndWarnsOnce`,
which asserts the count contract that prevents the excess from arising.

### 2. The `baseEmailIndex == -1` guard

`WhyFailingRunImpossible:` same COM boundary. Additionally, reaching the pre-fix subscript
`_itemGroups[insertionIndex - 1]` inside `EnumerateConversationMembers` requires at least one loop
iteration, and each iteration calls `InitializeGroup`, which constructs a real `QfcItemController`
around a WinForms item viewer.

Alternative proof: `_itemGroups.FindIndex(...)` returns `-1` when the base email is no longer in the
collection; `insertionIndex` was then `0`, `MakeSpaceForItems(0, insertCount)` reserved rows at the
head of the collection, and the first loop iteration evaluated `_itemGroups[-1]`. The fix returns
before any of that, restoring the navigation registration and the layout state the method had
already turned off. The guard is visible in the diff at the top of `ToggleUnGroupConv`, and the
early-out restores exactly the two pieces of state the method mutates before the guard.

Both items are listed in P14-T1's required set, which this artifact supplies the material for.

## Host-identifier sanitisation

The TRX was sanitised case-insensitively before commit: 25 substitutions. Post-sanitisation the
file contains zero occurrences of any of the four host-identifier patterns recorded in
`evidence/other/host-identifier-sanitisation.2026-08-26T10-11.md`.
