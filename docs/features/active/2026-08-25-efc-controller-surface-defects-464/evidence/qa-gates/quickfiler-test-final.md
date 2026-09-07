# [P10-T6] Final `QuickFiler.Test` run

Timestamp: 2026-08-28T02-00
Task: [P10-T6]
Command: `& "C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe" "QuickFiler.Test\bin\Debug\QuickFiler.Test.dll" /EnableCodeCoverage /InIsolation /Settings:scripts\vscode\TaskMaster.cli.runsettings "/Logger:trx;LogFileName=final-quickfiler-test.trx" "/ResultsDirectory:docs\features\active\efc-controller-surface-defects-464\evidence\qa-gates\trx\p10-t6"` under `pwsh -NoProfile` from the worktree root
EXIT_CODE: 0

Run start (UTC): `2026-08-28T01-58-04`
Run end (UTC): `2026-08-28T01-58-25`

## Result

```
Test Run Successful.
Total tests: 1169
     Passed: 1169
 Total time: 12.6375 Seconds
```

TRX `<Counters>` element, verbatim:

```
total="1169" executed="1169" passed="1169" failed="0" error="0" timeout="0" aborted="0"
inconclusive="0" passedButRunAborted="0" notRunnable="0" notExecuted="0" disconnected="0"
warning="0" completed="0" inProgress="0" pending="0"
```

A direct count of `outcome="..."` attributes in the TRX gives **1169 `Passed`** and zero of any other
outcome value.

## Acceptance, condition by condition

| Condition | Required | Observed | Verdict |
|---|---|---|---|
| Total executed greater than zero | `> 0` | **1169** | PASS |
| Failed count | `0`, or a subset of `BASELINE_FAILED` | **0** | PASS — the first branch applies |
| Passed count exceeds `BASELINE_PASSED` by at least 44 | `>= 1099 + 44 = 1143` | **1169** | PASS, margin +26 |

The **first** failed-count branch applies: the failed count is 0, so the `BASELINE_FAILED` subset branch
is not exercised and no pre-existing failure has to be enumerated as out of scope. `BASELINE_FAILED` was
recorded as the empty set by `[P0-T12]`, so the subset branch would have permitted no failure in any
case.

### The passed-count arithmetic reconciles exactly

```
1099  BASELINE_PASSED ([P0-T12], isolated single-assembly run at BASELINE_SHA)
+ 26  test results brought in by the mandated integration merge 25924673 (merged siblings #476, #501),
      measured at the post-merge boundary in postmerge-quickfiler-test.md (1099 -> 1137 total, of which
      1111 predated the merge: +26)
+ 44  test results this feature adds
----
1169  observed
```

## The 44 new test results, enumerated

Counted by matching every test method declared in the four test files this feature writes against the
`testName` attributes in the TRX. Every one of the 44 is present exactly as expected and every one is
`Passed`.

### `QuickFiler.Test/Controllers/EfcFormControllerTests.cs` — 14 added methods, **18 results**

| Test method | Results |
|---|---|
| `FormDarkMode_OnAllFieldsNullController_ReturnsFalseAndDoesNotThrow` | 1 |
| `FormActiveTheme_OnAllFieldsNullController_ReturnsBackingFieldAndDoesNotThrow` | 1 |
| `FormLoadTheme_OnAllFieldsNullController_DoesNotThrow` | 1 |
| `FormCleanup_CalledTwice_DoesNotThrow` | 1 |
| `FormCleanup_InvokesParentCleanupExactlyOnce` | 1 |
| `AsyncVoidBoundary_WhenFaulted_LogsOnceAndDoesNotThrow` | **5** (`[DataTestMethod]`, five `[DataRow]`) |
| `BoundaryErrorSink_DefaultDelegate_InvokesWithoutThrowing` | 1 |
| `PopulateFolderCombobox_WhenDataModelFaults_LogsOnceAndDoesNotFault` | 1 |
| `MatchesForSearchText_WithRepresentativeInput_ReturnsExpectedMatches` | 1 |
| `WithTrashRow_AppliedTwice_YieldsExactlyOneTrashRow` | 1 |
| `ActionDeleteAsync_AwaitedTwice_LeavesExactlyOneTrashRowInFolderRows` | 1 |
| `IsBannerRow_ClassifiesByTheFourCharacterPrefix` | 1 |
| `IsBannerRow_NullOrShortRow_ReturnsFalseWithoutThrowing` | 1 |
| `IsSelectableFolder_AndIsBannerRow_ClassifyThreeAndFourEqualsRowsIdentically` | 1 |

### `QuickFiler.Test/Controllers/EfcItemControllerTests.cs` — 10 methods, **10 results**

`RegisterActions_IsAbsentFromEfcItemControllerMetadata`,
`ToggleExpansion_IsAbsentAtEveryArity`,
`InitializeWebView_IsAbsentFromEfcItemControllerMetadata`,
`SevenParameterConstructor_IsAbsentFromEfcItemControllerMetadata`,
`SelectorsCtrlsField_IsAbsentFromEfcItemControllerMetadata`,
`AsyncExpansionPath_OnOffOn_LeavesCharActionsKeysUnchanged`,
`ConversationResolverPropertyChanged_IsAbsentFromEfcItemControllerMetadata`,
`PopulateConversation_AssignsSetTopicThreadToConversationResolverUpdateUi`,
`IncognitoArgument_IsAsciiDoubleHyphenIncognitoWithTrailingSpace`,
`ThrowInitializationFailure_PreservesOriginalStackTrace` — 1 result each.

### `QuickFiler.Test/Controllers/EfcItemController.CleanupTests.cs` — 8 methods, **8 results**

`Cleanup_OnFiveArgumentConstructedController_DoesNotThrow`,
`Cleanup_CalledTwice_DoesNotThrow`,
`Cleanup_NullsButtonsField`,
`Cleanup_DisposesTimerBeforeNullingIt`,
`ApplyReadEmailFormat_AfterCleanup_DoesNotThrow`,
`SubjectSenderAndTo_ReadFromItemInfo_AndAreInertAfterCleanup`,
`ItemDarkMode_OnNullGlobalsController_ReturnsFalseAndDoesNotThrow`,
`ItemActiveThemeAndLoadTheme_OnNullThemesController_DoNotThrow` — 1 result each.

### `QuickFiler.Test/Controllers/EfcViewerTests.cs` — 8 methods, **8 results**

`SetControllerAndFormControllerField_AreAbsentFromEfcViewerMetadata`,
`EditFiltersMenuItemClick_IsAbsentFromEfcViewerMetadata`,
`FormEditFiltersMenuItemClick_IsStillDeclaredOnEfcFormController`,
`ClaimsAltChord_WithBareAltAndHandler_ReturnsTrue`,
`ClaimsAltChord_WithAltF_ReturnsFalse`,
`ClaimsAltChord_WithAltM_ReturnsFalse`,
`ClaimsAltChord_WithNonAltChord_ReturnsFalse`,
`ClaimsAltChord_WithNullHandler_ReturnsFalse` — 1 result each.

**Total: 18 + 10 + 8 + 8 = 44**, matching the plan's stated arithmetic
`6 + 3 + 2 + 1 + 13 + 8 + 6 + 5 = 44` and its independent reconciliation `40 − 1 + 5 = 44` over the 40
declared test methods.

## The two pre-existing `EfcFormControllerTests` methods

`PopulateFolderCombobox_WhenFormViewerIsNull_ReturnsWithoutTouchingDataModel` and
`Issue439BindBreadcrumbRowsAsync_SubmitsArchiveRootToRealRouter` each appear exactly once in the TRX
with outcome `Passed`, matching their `[P0-T13]` baseline outcomes.

## The 14 previously-observed aggregate failures

The Phase 8 boundary run of the same assembly reported 14 failures, all in three `QfcItemController.*`
test files outside this feature's owned set, each timing out at roughly 60 s under load. In **this** run
all of them pass, including `BuildPumpHarness_DoesNotCreateTheWebViewChildHandles` at 45 ms. This
reconfirms the classification recorded in `postmerge-quickfiler-test.md` and
`phase8-boundary-toolchain.md`: those failures are **load-driven WinFormsPumpHost / dispatcher-fixture
flakiness**, base-introduced by merged sibling work, not behavioural defects and not attributable to this
feature. They do not appear in this gate at all.

## `NoLiveFormInTestAssemblyTests` — required by `[P10-T10]`

`QuickFiler.Test/NoLiveFormInTestAssemblyTests.cs` declares one test,
`ExecutingAssembly_ContainsNoFormDerivedType`. It appears exactly once in this TRX and it **passed**,
so the assembly still contains no `Form`-derived type after this feature's four test files were added.

## Artifacts and hygiene

TRX retained at
`docs/features/active/efc-controller-surface-defects-464/evidence/qa-gates/trx/p10-t6/final-quickfiler-test.trx`.

Sanitisation performed in place before commit, case-insensitively:

| Substitution | Occurrences |
|---|---|
| absolute worktree path → `<repo-root>` | 2339 |
| account name → `<user>` | 4 |
| machine name → `<host>` | 1175 |

A case-insensitive search of the retained file for the account name or the machine name now returns
**0** matches. The `<Counters>` element is unaltered, and no test name, outcome or duration was changed.

Two directories written by the run were deleted rather than committed:

- the `/EnableCodeCoverage` attachment directory, which held a binary `.coverage` file whose filename
  embedded the account name, the machine name and the wall-clock time;
- the `/InIsolation` `Deploy_*`-equivalent scratch tree, likewise named after the account and machine.

Only the sanitised TRX remains.

## Loop position

Stage 4 (testing) of the first Phase 10 pass. No source file was written by the run, so no loop restart
is triggered.

Output Summary: PASS. vstest exits 0 with 1169 of 1169 tests passing and **zero** failures, so the
first acceptance branch applies and no `BASELINE_FAILED` subset argument is needed. The passed count
exceeds `BASELINE_PASSED` (1099) by 70, comfortably above the required 44; the arithmetic reconciles as
1099 + 26 merged-sibling results + 44 results this feature adds. All 44 new results are enumerated and
all pass. The 14 aggregate-run failures observed at the Phase 8 boundary all pass here, confirming they
are load-driven and base-introduced. TRX retained and sanitised; the binary coverage attachment and the
isolation scratch tree were deleted.
