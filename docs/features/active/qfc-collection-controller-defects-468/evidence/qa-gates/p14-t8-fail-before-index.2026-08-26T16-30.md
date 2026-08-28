# [P14-T8] Fail-before / pass-after index (AC-18)

Timestamp: 2026-08-26T16-30

Command:

```
# per TRX under evidence/regression-testing/
grep -o 'total="[0-9]*" executed="[0-9]*" passed="[0-9]*" failed="[0-9]*"' <trx>
grep -o 'testName="[^"]*"' <trx>
```

EXIT_CODE: 0

ExpectedExitCode: 0

## Output Summary

Every defect in the issue #468 family that carries a Tier-1 or Tier-2 regression test is mapped below
to the path of its fail-before TRX and the path of its pass-after TRX. **Fifteen fail-before TRX files
and eleven pass-after TRX files** are indexed. Every fail-before entry records a failed count of at
least `1`; every pass-after entry records a failed count of exactly `0`.

Paths below are relative to
`docs/features/active/qfc-collection-controller-defects-468/evidence/regression-testing/`. Every path
was verified to exist on disk by the enumeration that produced this table.

## Index

| Defect | Test | Fail-before TRX | fail-before counters | Pass-after TRX | pass-after counters |
|---|---|---|---|---|---|
| #474 defect 1 | `ParentFieldAndConstructorParameterAreTypedIQfcFormController` | `p2-t6/p2-t6.trx` | `total="1" passed="0" failed="1"` | `p2-t10/p2-t10.trx` | `total="1" passed="1" failed="0"` |
| #286 | `RemoveSpecificControlGroupAsync_ThrowAtFirstStatement_RestoresReentrancyCounter` | `p3-t2/p3-t2.trx` | `total="1" passed="0" failed="1"` | `p3-t5/p3-t5.trx` | `total="2" passed="2" failed="0"` |
| #286 | `RemoveSpecificControlGroupAsync_ThrowLaterInBody_RestoresReentrancyCounter` | `p3-t3/p3-t3.trx` | `total="1" passed="0" failed="1"` | `p3-t5/p3-t5.trx` | `total="2" passed="2" failed="0"` |
| #469 defect 3 | `ItemGroupsToMoveFieldDeclaresAnOrderedContract` | `p4-t3/p4-t3.trx` | `total="1" passed="0" failed="1"` | `p4-t7/p4-t7.trx` | `total="2" passed="2" failed="0"` |
| #473 defect 2 | `MoveEmailsAsync_WhenMoveIsCancelled_PropagatesOperationCanceledException` | `p5-t1/p5-t1.trx` | `total="1" passed="0" failed="1"` | `p5-t5/p5-t5.trx` | `total="3" passed="3" failed="0"` |
| #473 defect 2 | `MoveEmailsAsync_AfterFirstFailure_DoesNotReadSubjectASecondTime` | `p5-t2/p5-t2.trx` | `total="1" passed="0" failed="1"` | `p5-t5/p5-t5.trx` | `total="3" passed="3" failed="0"` |
| #469 defect 2 | `GetMoveDiagnostics_WithOneGroup_ReturnsExactlyOneLine` | `p6-t1/p6-t1.trx` | `total="1" passed="0" failed="1"` | `p6-t5/p6-t5.trx` | `total="3" passed="3" failed="0"` |
| #469 defect 2 | `GetMoveDiagnostics_WithThreeGroups_ReturnsThreeLinesAndNoNulls` | `p6-t2/p6-t2.trx` | `total="1" passed="0" failed="1"` | `p6-t5/p6-t5.trx` | `total="3" passed="3" failed="0"` |
| #469 defect 1 | `GetMoveDiagnostics_WithNullItemController_ReturnsUnknownLineWithoutThrowing` | `p6-t3/p6-t3.trx` | `total="1" passed="0" failed="1"` | `p6-t5/p6-t5.trx` | `total="3" passed="3" failed="0"` |
| #470 defect 2 | `ConversationReconciliationHelpersExist` | `p7-t3/p7-t3.trx` | `total="1" passed="0" failed="1"` | `p7-t12/p7-t12.trx` | `total="6" passed="6" failed="0"` |
| #470 defect 1 | `PromoteFirstChild_WithNoMatchingChild_ReturnsMinusOneWithoutSubscripting` | `p8-t1/p8-t1.trx` | `total="1" passed="0" failed="1"` | `p8-t4/p8-t4.trx` | `total="2" passed="2" failed="0"` |
| #470 defect 1 | `ToggleGroupConv_WithNoMatchingOriginal_DoesNotSubscriptWithMinusOne` | `p8-t2/p8-t2.trx` | `total="1" passed="0" failed="1"` | `p8-t4/p8-t4.trx` | `total="2" passed="2" failed="0"` |
| #470 defect 3 | `SetVisualDigits_WithNullItemController_SkipsTheGroupWithoutThrowing` | `p9-t1/p9-t1.trx` | `total="1" passed="0" failed="1"` | `p9-t3/p9-t3.trx` | `total="1" passed="1" failed="0"` |
| #471 | `EliminateSpaceForItems_ReducesMinimumHeightByTemplateHeightTimesRemovalCount` | `p10-t6/p10-t6.trx` | `total="1" passed="0" failed="1"` | `p10-t9/p10-t9.trx` | `total="1" passed="1" failed="0"` |
| #473 defect 1 | `DrainBackgroundLoadingTasksAsync_AwaitsATaskAddedDuringTheDrainWindow` | `p11-t4/p11-t4.trx` | `total="1" passed="0" failed="1"` | `p11-t6/p11-t6.trx` | `total="1" passed="1" failed="0"` |

Fifteen rows, each a genuine red-to-green pair. In every fail-before TRX the failed count is `1`,
which satisfies "at least 1". In every pass-after TRX the failed count is `0`.

## Defects with a pass-after TRX but no fail-before TRX

Three entries have a permanent green test with no red counterpart. Each is covered by an item in the
fail-before exception dossier
`evidence/regression-testing/fail-before-exception.2026-08-26T16-24.md`.

| Defect | Test(s) | Pass-after TRX | counters | Dossier item |
|---|---|---|---|---|
| #469 defect 3 (behavioural half) | `TryGetItemGroupByIndexResolvesInsertionOrderAfterMutation` | `p4-t7/p4-t7.trx` | `total="2" passed="2" failed="0"` | item 1 |
| #469 defect 4 | `MoveEmailsAsync_WithNullStack_BehavesIdenticallyToAnEmptyStack` | `p12-t3/p12-t3.trx` | `total="1" passed="1" failed="0"` | item 4 |
| #474 defect 2 | `TryGetMoveReadiness_WithUnassignedDestination_ReturnsFalseAndProducesNotificationText`, `TryGetMoveReadiness_WithAllDestinationsAssigned_ReturnsTrueAndEmptyNotification` | `p13-t6/p13-t6.trx` | `total="2" passed="2" failed="0"` | item 7 |

## Additional green runs indexed for completeness

| Purpose | TRX | counters |
|---|---|---|
| #471 `MinimumSize` height-neutrality (D10) | `p10-t10/p10-t10.trx` | `total="2" passed="2" failed="0"` |
| #469 defects 1-2, superseded first attempt with a narrower assertion | `p6-t5/p6-t5-attempt1-narrower-assertion.trx` | `total="3" passed="3" failed="0"` |

The `p6-t5-attempt1-narrower-assertion.trx` file is retained deliberately: it records a run whose
assertions were subsequently widened, so the committed test is not the test that produced it. It is
**not** the pass-after of record for #469 defects 1 and 2; `p6-t5/p6-t5.trx` is.

## Defects with no regression test at all

| Defect | Reason | Alternative proof |
|---|---|---|
| #468 | a removal; there is no prior behaviour to assert | dossier item 2 — compilation, green suite, reflective-caller search |

## Acceptance verification

- The artifact exists at `evidence/qa-gates/p14-t8-fail-before-index.2026-08-26T16-30.md`.
- Every path listed above exists on disk; the enumeration that produced the counter values was a
  directory walk of `evidence/regression-testing/p*/` for `*.trx`, so a listed path that did not
  exist could not have produced a counter value.
- Every fail-before entry records `failed="1"`, which is at least `1`.
- Every pass-after entry records `failed="0"`, which is exactly `0`.
