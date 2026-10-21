# Regression testing — [expect-fail] red run against unmodified production code ([P1-T4])

- Issue: #644
- Task: `[P1-T4]` `[expect-fail]`
- Timestamp: 2026-08-29T08-15

**A failing test run is the expected and required outcome of this task.** No production file has
been edited at this point; Phase 2 has not started. This artifact is the **AC-1 fail-before
evidence**.

Command: `<resolved-vstest.console.exe> QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /Logger:trx /ResultsDirectory:coverage\trx\p1-t4 /TestCaseFilter:"FullyQualifiedName~QfcCollectionControllerNavigationLedgerTests"`
Runner: `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe`
Working directory: repository root (`<repo-root>`)
EXIT_CODE: 1
ExpectedExitCode: 1

TRX written to `coverage\trx\p1-t4\<account>_<HOST>_2026-08-29_13_49_54_net481.trx` (default
`vstest.console.exe` filename embeds the account and machine name, redacted here).

## TRX `Counters` element

```
COUNTERS total=6 passed=2 failed=4
```

- **total: 6** — matches the required `total="6"`
- **passed: 2** — matches the required `passed="2"`
- **failed: 4** — matches the required `failed="4"`

## Per-test outcomes

```
Passed :: RegisterAndUnregisterNavigation_RepeatedCycles_LeaveRegistryEmpty
Passed :: UnregisterNavigation_WithNoPriorRegistration_DoesNotThrowAndLeavesRegistryUnchanged
Failed :: UnregisterNavigation_AfterGroupRemovedThroughRemoveGroupByEntryIdSeam_RemovesEveryRegisteredKey
Failed :: UnregisterNavigation_AfterUnbracketedItemGroupsRemoval_ThenReRegister_DoesNotThrow
Failed :: UnregisterNavigation_AfterItemGroupsSetToNull_DoesNotThrow
Failed :: UnregisterNavigation_AfterTwoDigitRegistrationAndShrinkToNine_LeavesNoCollectionKeys
```

The four non-`Passed` tests are **exactly** the four the task names, and the two `Passed` tests
are **exactly** the two the task names. No substitution and no additional failure.

## Failure message of each of the four (host paths redacted)

### 1. `UnregisterNavigation_AfterGroupRemovedThroughRemoveGroupByEntryIdSeam_RemovesEveryRegisteredKey` (T1 — the AC-1 test)

```
Expected CollectionKeys(registry) to be empty because issue #644 requires unregistration to
replay the recorded registration set, so an unbracketed removal through the RemoveGroupByEntryId
seam cannot orphan the tail key, but found at least one item {"10"}.
```

The orphan is the tail key `"10"`, exactly as the root-cause analysis predicts: registration added
`"01"`..`"10"` at width 2; the seam removed one group; the count-bounded removal loop then ran
nine times and never visited the tenth key.

### 2. `UnregisterNavigation_AfterUnbracketedItemGroupsRemoval_ThenReRegister_DoesNotThrow` (T2)

```
Did not expect any exception because issue #644 requires the first unregistration to have been
total, so no orphaned key remains for the second registration to collide with, but found
System.ArgumentException: Cannot add key because it already exists. Key 5 SourceId Collection
Parameter name: instance
   at QuickFiler.Controllers.KbdActions`3.Add(UClass instance) in <repo-root>\QuickFiler\Controllers\KbdActions.cs:line 156
   at QuickFiler.Controllers.QfcCollectionController.RegisterNavigationAsyncAction(Int32 itemIndex, Int32 digits) in <repo-root>\QuickFiler\Controllers\QfcCollectionController.cs:line 1197
   at QuickFiler.Controllers.QfcCollectionController.RegisterNavigation() in <repo-root>\QuickFiler\Controllers\QfcCollectionController.cs:line 1180
```

This is the symptom the issue describes: the orphaned `"5"` collides with the re-registration and
surfaces as `ArgumentException` from a duplicate `Add`. It confirms the plan's correction to the
spec's non-acceptance "Pre-fix result" prediction — the extra Act step that restores the page to
five groups before the second `RegisterNavigation()` is what makes this throw reachable.

### 3. `UnregisterNavigation_AfterItemGroupsSetToNull_DoesNotThrow` (T5)

```
Did not expect any exception because issue #644 removes _itemGroups from the unregistration path
entirely, so a null field is no longer dereferenced, but found System.NullReferenceException:
Object reference not set to an instance of an object.
   at QuickFiler.Controllers.QfcCollectionController.UnregisterNavigation() in <repo-root>\QuickFiler\Controllers\QfcCollectionController.cs:line 1189
```

Line 1189 is `for (int i = 0; i < _itemGroups.Count; i++)` inside `UnregisterNavigation()` — the
loop bound that reads the live `_itemGroups`. This is the structural counterpart of the fix: after
`[P2-T3]`, `UnregisterNavigation` does not reference `_itemGroups` at all.

### 4. `UnregisterNavigation_AfterTwoDigitRegistrationAndShrinkToNine_LeavesNoCollectionKeys` (T6)

```
Expected CollectionKeys(registry) to be empty because the ledger replays all ten recorded keys,
so the width-crossing tail key '10' is no longer left behind, but found at least one item {"10"}.
```

The residual `{"10"}` is exactly the entry the #472 width-fidelity test in
`QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs` currently pins and
attributes by XML documentation comment to this follow-up issue. `[P3-T4]` flips that assertion
once the ledger closes the residual.

## Why the two green tests are green pre-fix

- `RegisterAndUnregisterNavigation_RepeatedCycles_LeaveRegistryEmpty` — no `_itemGroups` mutation
  occurs between register and unregister, so the count-bounded loop and the registration count
  coincide. It is state-transition coverage rather than a defect reproduction.
- `UnregisterNavigation_WithNoPriorRegistration_DoesNotThrowAndLeavesRegistryUnchanged` — the
  pre-fix loop issues `Remove("Collection", …)` calls that match nothing, because the seeded entry
  carries `SourceId` `"Other"` and `KbdActions.Remove` compares `SourceId` exactly. It is the
  empty-ledger negative case.

Output Summary: `[expect-fail]` satisfied. The TRX `Counters` element reports
**total="6", passed="2", failed="4"**, EXIT_CODE 1 against `ExpectedExitCode: 1`. The four failing
tests are exactly T1, T2, T5, and T6 as the task requires, and the two passing tests are exactly
T3 and T4. Each failure message is captured above and each matches the predicted pre-fix failure
mode: an orphaned tail key `"10"` for T1 and T6, an `ArgumentException` duplicate-`Add` collision
on key `"5"` for T2, and a `NullReferenceException` at the `_itemGroups.Count` loop bound
(`QfcCollectionController.cs` line 1189) for T5.
