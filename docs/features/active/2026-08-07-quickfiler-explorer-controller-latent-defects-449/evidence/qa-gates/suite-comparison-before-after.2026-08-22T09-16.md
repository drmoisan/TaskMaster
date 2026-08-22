# AC-7 — Before/After Full-Suite Comparison (Issue #449, [P7-T8])

Timestamp: 2026-08-22T09-16
WORKTREE: `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a5600546d71e73061`

## Source artifacts compared

| Role | Artifact |
| --- | --- |
| **Before** | `<FEATURE>/evidence/baseline/step5-vstest-coverage.2026-08-22T09-16.md` |
| **After** | `<FEATURE>/evidence/qa-gates/step5-vstest-coverage.2026-08-22T09-16.md` |

Both runs used the identical command, the identical assembly-discovery logic (9 assemblies, filtered
on the WORKTREE-relative suffix with `\bin\Debug\` kept and `\obj\`, `\ref\`, `\.claude\` excluded),
the identical `/Settings:scripts\vscode\TaskMaster.cli.runsettings`, `/InIsolation`, and
`/TestCaseFilter:TestCategory!=LiveOutlook`. The only difference between them is the change under
review.

## Counts

| Metric | Before | After | Delta |
| --- | --- | --- | --- |
| **Executed** | **6437** | **6452** | **+15** |
| **Passed** | **6437** | **6452** | **+15** |
| Failed | 0 | 0 | 0 |
| Skipped | 0 | 0 | 0 |

## Set comparison — the added tests are the ONLY difference

Command:
```
grep -o '^  Passed [A-Za-z0-9_]*' p0t12-vstest.log      | sed 's/^  Passed //' | sort > baseline.txt
grep -o '^  Passed [A-Za-z0-9_]*' p7t6-vstest-rerun.log | sed 's/^  Passed //' | sort > run1.txt
comm -13 baseline.txt run1.txt   # present after, absent before  (ADDED)
comm -23 baseline.txt run1.txt   # present before, absent after  (REMOVED)
```
EXIT_CODE: 0

### ADDED — 15 entries, every one a test this plan created

```
CurrentConversationState_ReflectsCommandBarPressedState          (DataRow: True)
CurrentConversationState_ReflectsCommandBarPressedState          (DataRow: False)
ExplConvView_ReturnState_WhenFlagSet_TogglesOn
ExplConvView_ToggleOff_WhenConversationsNotGrouped_DoesNothing
ExplConvView_ToggleOff_WhenSiblingViewMissing_CopiesAndSavesTemporaryView
ExplConvView_ToggleOn_WhenFlagClear_DoesNothing
ExplConvView_ToggleOn_WhenFlagSet_AppliesRememberedView
GetSiblingView_WhenNamedViewAbsent_ReturnsNull
GetSiblingView_WhenNamedViewPresent_ReturnsIt
OpenQFItem_WhenActiveExplorerChangesAfterConstruction_UsesTheConstructorCapturedExplorer
OpenQFItem_WhenDialogSeamReturnsNo_DoesNotDisplayMailItem
OpenQFItem_WhenDialogSeamReturnsYes_DisplaysMailItem
OpenQFItem_WhenItemIsSelectableInView_ClearsAndAddsSelection
OpenQFItem_WhenItemNotSelectableInView_InvokesDialogSeamOnce
OpenQFItem_WhenMailIsAlreadyInTheCurrentFolder_DoesNotChangeCurrentFolder
```

That is 15 entries from 14 test METHODS: `CurrentConversationState_ReflectsCommandBarPressedState` is
a `[DataTestMethod]` with two `[DataRow]` cases, so it contributes two entries. The list matches, name
for name, the 14 test-method names the plan enumerates in its "Literals this plan creates" section.
The declined optional reflection test `Contract_ExplConvView_Cleanup_IsNotDeclaredOnTheInterface` is
correctly absent per [P6-T13].

### REMOVED — zero entries

```
(empty)
```

**No test that passed before this change fails or disappears after it.** No pre-existing test was
removed, renamed, disabled, weakened, or made to skip.

## Interpretation — AC-7 satisfied

The same set of passing tests appears in both runs, with the tests added by this plan as the **only**
additions and **no new failures**. Both runs report zero failed and zero skipped.

This comparison is also the alternative proof of no behaviour change for **defect 3**, whose
dead-region deletion admits no fail-before test (see
`../regression-testing/fail-before-exception.defect3.2026-08-22T09-16.md`). The 139 deleted lines were
unreachable from every compiled entry point, so if the deletion had changed any observable behaviour
it would have had to surface as a difference in this suite result. It did not: the 6,437 pre-existing
tests pass identically before and after.

It likewise corroborates **defect 1**: removing `ExplConvView_Cleanup` from the interface and its sole
implementation broke no caller, which the empty REMOVED set confirms behaviourally in addition to the
clean builds.

**The [P7-T8] stop condition did not trigger.** A new failure would have halted execution before the
acceptance-criteria check-offs. There is no new failure, so the check-offs [P7-T16] through [P7-T31]
proceeded. One unrelated `UtilitiesCS` Dispatcher STA test flaked on a first attempt at [P7-T6]; it
passed in isolation and in both subsequent consecutive clean runs, is not in the ADDED or REMOVED sets
above, and is fully disclosed in `step5-vstest-coverage.2026-08-22T09-16.md`.

## Output Summary

Comparing `../baseline/step5-vstest-coverage.2026-08-22T09-16.md` (before) against
`step5-vstest-coverage.2026-08-22T09-16.md` (after): executed **6437 -> 6452**, passed
**6437 -> 6452**, delta **+15**, failed 0 in both, skipped 0 in both. Set comparison shows **15 added**
entries — exactly the 14 test methods this plan created, with one contributing two `[DataRow]` cases —
and **zero removed**. The same set of tests passes in both runs with this plan's tests as the only
additions and no new failures, satisfying AC-7 and supplying the alternative no-behaviour-change proof
for defect 3.
