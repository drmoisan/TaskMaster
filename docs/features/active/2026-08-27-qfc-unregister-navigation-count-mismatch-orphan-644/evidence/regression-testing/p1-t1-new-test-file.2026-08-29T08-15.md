# Regression testing — New ledger test file created ([P1-T1])

- Issue: #644
- Task: `[P1-T1]`
- Timestamp: 2026-08-29T08-15
- File created: `QuickFiler.Test/Controllers/QfcCollectionControllerNavigationLedgerTests.cs`

MSTest `[TestClass]` in namespace `QuickFiler.Controllers.Tests`, using Moq for
`IQfcKeyboardHandler`, `Microsoft.Office.Interop.Outlook.MailItem`, and `IQfcItemController`, and
FluentAssertions for every assertion. No production file is edited in this phase.

## Testability posture

The private helpers are modelled on `CreateNavigationController` at
`QuickFiler.Test/Controllers/QfcCollectionControllerNavigationDigitsTests.cs` lines 104-126 and on
the `_removeGroupByEntryId` + `TopFolderScore` precedent in
`QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs`. The controller is allocated with
`FormatterServices.GetUninitializedObject`; a real parameterless
`KbdActions<string, KaStringAsync, Func<string, Task>>` is wired behind a Loose
`Mock<IQfcKeyboardHandler>.SetupGet(x => x.StringActionsAsync)`; and `_kbdHandler`, `_digits`,
`_itemGroups`, and where needed `_removeGroupByEntryId` are injected by reflection.

`_digits` is kept equal to the width each page needs, so `RegisterNavigation` never routes into
`SetVisualDigits`. No live Outlook process, no COM object, no WinForms handle, no STA apartment,
no temporary file, no wall-clock wait, and no mutable static state is used.

## The six `[TestMethod]`s

| # | Test | Arrangement | Expected pre-fix |
|---|---|---|---|
| T1 | `UnregisterNavigation_AfterGroupRemovedThroughRemoveGroupByEntryIdSeam_RemovesEveryRegisteredKey` | ten groups `entry-0`..`entry-9`, `_digits` 2, `TopFolderScore` 1000L except `entry-0` at 100L; injected `_removeGroupByEntryId`; register, `await RemoveBelowThresholdAsync(0.9)`, unregister; assert the `"Collection"` key set is empty | **red** — leaves `"10"` |
| T2 | `UnregisterNavigation_AfterUnbracketedItemGroupsRemoval_ThenReRegister_DoesNotThrow` | five groups, `_digits` 1; register, remove index 0 from the injected list, unregister, append one freshly built group so the page is five again, assert a second register does not throw and the key set is exactly `"1"`..`"5"` one entry per key | **red** — `ArgumentException` |
| T3 | `RegisterAndUnregisterNavigation_RepeatedCycles_LeaveRegistryEmpty` | three groups, `_digits` 1; register/unregister twice with no mutation between | green |
| T4 | `UnregisterNavigation_WithNoPriorRegistration_DoesNotThrowAndLeavesRegistryUnchanged` | three groups, `_digits` 1, registry pre-seeded with exactly one `SourceId` `"Other"` key `"1"` entry; unregister with no prior registration | green |
| T5 | `UnregisterNavigation_AfterItemGroupsSetToNull_DoesNotThrow` | five groups, `_digits` 1; register, set `_itemGroups` null by reflection, unregister | **red** — `NullReferenceException` |
| T6 | `UnregisterNavigation_AfterTwoDigitRegistrationAndShrinkToNine_LeavesNoCollectionKeys` | ten groups, `_digits` 2; register, remove index 0 from the injected list, unregister | **red** — leaves `"10"` |

T2 carries the one further Act step the plan's correction requires — restoring the page to five
groups before the second `RegisterNavigation()`. Without it a bare shrink-then-re-register adds
`"1"`..`"4"`, which do not collide with the orphaned `"5"`, so the documented `ArgumentException`
would not be reachable before the fix. The spec's Test Strategy "Pre-fix result" column is
descriptive text and not an acceptance criterion; AC-3 requires only that T2 pass after the fix.

## Acceptance verification

Command: `git status --porcelain -- QuickFiler.Test/Controllers/QfcCollectionControllerNavigationLedgerTests.cs`

```
?? QuickFiler.Test/Controllers/QfcCollectionControllerNavigationLedgerTests.cs
```

The new path is listed as **untracked**, as required.

Command: `(Select-String -Path QuickFiler.Test\Controllers\QfcCollectionControllerNavigationLedgerTests.cs -Pattern '\[TestMethod\]').Count`

```
testmethods=6
```

Command: `(Get-Content QuickFiler.Test\Controllers\QfcCollectionControllerNavigationLedgerTests.cs).Count`

```
lines=359
```

EXIT_CODE: 0

Output Summary: The new regression file exists and is untracked, carries exactly **6**
`[TestMethod]` attributes, and is **359 lines**, which is at or under the 500-line repository
ceiling. All three `[P1-T1]` acceptance clauses hold. The file is not yet registered in the
project; `[P1-T2]` adds the `Compile Include` item.
