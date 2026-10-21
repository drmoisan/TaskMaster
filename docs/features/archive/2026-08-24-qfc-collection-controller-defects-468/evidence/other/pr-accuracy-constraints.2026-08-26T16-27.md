# PR-body accuracy constraints (AC-27 input)

Timestamp: 2026-08-26T16-27

Command: not applicable (this artifact is a written constraint record, not a command step)

EXIT_CODE: 0

ExpectedExitCode: 0

## Output Summary

Five constraints the PR body for this feature must satisfy. Two are prohibitions on repeating a
premise that is false on this base; two are statements the body must make; one governs how the
per-defect evidence is cited. They correspond one-for-one to the clauses of AC-27 in
`docs/features/active/qfc-collection-controller-defects-468/spec.md:1313-1319`.

AC-27 is checked off by the orchestrator, which authors the PR body — not by this executor. This
artifact is the input the orchestrator must satisfy.

---

## Constraint 1 — do not repeat the #468 coverage-denominator rationale

**Prohibited claim.** That removing the twelve dead members improves the coverage denominator, or
that the dead code was depressing the measured coverage of `QfcCollectionController`.

**Why it is invalid on this base.** The class carries `[ExcludeFromCodeCoverage]` at
`QuickFiler/Controllers/QfcCollectionController.cs:21`. Every line of the type is already outside both
the numerator and the denominator of the coverage metric, so removing lines from it cannot move any
coverage number in either direction. The rationale would have been sound for a measured type; it is
not sound for this one.

Removing the attribute is out of scope, per AC-25
(`docs/features/active/qfc-collection-controller-defects-468/spec.md:1304-1307`), and its continued
presence is audited by P14-T13.

**What the body may say instead.** That the removal deletes twelve unreachable members, that the
absence of any caller was verified by compilation plus a reflective-caller search over 398
build-input files and all 42 `GetMethod(` call sites
(`evidence/other/p1-t1-reflective-caller-search.2026-08-26T08-25.md`), and that the full suite stayed
green across the removal (`evidence/qa-gates/p1-t8-suite.2026-08-26T08-45.md`).

---

## Constraint 2 — do not repeat the #474 "unrelated sibling interfaces" premise

**Prohibited claim.** That `QuickFiler.Controllers.IQfcFormController` and
`QuickFiler.Interfaces.IFilerFormController` are unrelated siblings, or that "neither is a superset of
the other," or that the fix required consolidating two parallel interfaces.

**Why it is false on this base.** Verified in source:

```
QuickFiler/Controllers/IQfcFormController.cs:13
    public interface IQfcFormController : IFilerFormController
```

`IQfcFormController` **derives from** `IFilerFormController` and is a strict superset of it. There was
no consolidation to perform. The fix is a field and constructor-parameter retype from the base
interface to the derived one, which is why the diff for #474 defect 1 is as small as it is.

The false premise originates in the promoted research document
`2026-08-07-qfc-collection-controller-coupling-and-modal-getter.md:35-39` and is corrected in the
spec at `docs/features/active/qfc-collection-controller-defects-468/spec.md:274-284`.

**Related correction the body should also avoid.** The same source document (`:95-98`) states that
issue #454 introduces injectable delegate seams around both call sites. Issue #454 has **not** landed
on this base; those seams did not exist and this feature created its own.

---

## Constraint 3 — state that #473 defect 1 is latent under the current call graph

**Required statement.** The PR body must say plainly that #473 defect 1 (the background-task drain
window) is **latent** under the current call graph — a correctness hazard for a future caller, not an
observed failure.

**Supporting facts** (`spec.md:110-114`): both `Add` pairs occur in the same method body strictly
before their `WhenAll`; no other member adds to the bag; and each of the three production
construction sites — `QuickFiler/Controllers/QfcFormController.Actions.cs:49`, `:83`, and `:139` —
creates a fresh controller that is awaited. The fix closes the window; it does not repair an observed
production defect.

Wording must not overstate impact. "Fixes an intermittent hang" or similar would be unsupported.

---

## Constraint 4 — state that #474 is latent in the current single-implementation configuration

**Required statement.** The PR body must say plainly that #474 is **latent** in the current
single-implementation configuration.

**Supporting fact** (`spec.md:116-117`): `QfcFormController` is the only production implementation of
the parent role, so the runtime downcast at base `QuickFiler/Controllers/QfcCollectionController.cs:1232`
could not throw today. The value of the fix is that the cast is replaced by a compile-time binding to
`IQfcFormController.SkipGroupAsync()` (`QuickFiler/Controllers/IQfcFormController.cs:38`), so a second
implementation cannot reintroduce the hazard.

---

## Constraint 5 — cite specific test names per defect in place of a coverage delta

**Required form.** Per-defect evidence in the PR body is a named MSTest method, not a coverage
percentage. A coverage delta cannot serve as evidence here for the reason given in constraint 1: the
owned production file is excluded from measurement, so its delta is structurally zero and would be
misleading if quoted as a proxy for verification.

The plan states this directly at its `### Coverage scope note`: no acceptance condition in this plan
claims a coverage increase attributable to this feature.

**The map the body should use.**

| Defect | Test name(s) | Pass-after evidence |
|---|---|---|
| #286 | `RemoveSpecificControlGroupAsync_ThrowAtFirstStatement_RestoresReentrancyCounter`, `RemoveSpecificControlGroupAsync_ThrowLaterInBody_RestoresReentrancyCounter` | `p3-t5-pass-after.2026-08-26T09-53.md` |
| #468 | no test by construction (a removal) | `p1-t3-dead-identifier-sweep`, `p1-t4-live-member-nonregression`, `p1-t8-suite`, `other/p1-t1-reflective-caller-search` |
| #469 defect 1 | `GetMoveDiagnostics_WithNullItemController_ReturnsUnknownLineWithoutThrowing` | `p6-t5-pass-after.2026-08-26T10-22.md` |
| #469 defect 2 | `GetMoveDiagnostics_WithOneGroup_ReturnsExactlyOneLine`, `GetMoveDiagnostics_WithThreeGroups_ReturnsThreeLinesAndNoNulls` | `p6-t5-pass-after.2026-08-26T10-22.md` |
| #469 defect 3 | `ItemGroupsToMoveFieldDeclaresAnOrderedContract`, `TryGetItemGroupByIndexResolvesInsertionOrderAfterMutation` | `p4-t7-pass-after.2026-08-26T10-12.md` |
| #469 defect 4 | `MoveEmailsAsync_WithNullStack_BehavesIdenticallyToAnEmptyStack` | `p12-t3-pass-after.2026-08-26T11-37.md` |
| #470 defect 1 | `PromoteFirstChild_WithNoMatchingChild_ReturnsMinusOneWithoutSubscripting`, `ToggleGroupConv_WithNoMatchingOriginal_DoesNotSubscriptWithMinusOne` | `p8-t4-pass-after.2026-08-26T10-48.md` |
| #470 defect 2 | `ConversationReconciliationHelpersExist`, `ResolveConversationInsertions_ExcludesBaseEntryAndOrdersBySentOnDescending`, `ReconcileInsertionCount_AboveReservation_ReturnsInsertionsCountAndWarnsOnce`, `ReconcileInsertionCount_EqualToReservation_ReturnsInsertionsCountAndDoesNotWarn`, `ReconcileInsertionCount_BelowReservation_ReturnsInsertionsCountAndWarnsOnce`, `EnumerateConversationMembers_WithNoInsertions_DoesNotThrow` | `p7-t12-pass-after.2026-08-26T10-39.md` |
| #470 defect 3 | `SetVisualDigits_WithNullItemController_SkipsTheGroupWithoutThrowing` | `p9-t3-pass-after.2026-08-26T11-02.md` |
| #471 | `ShrinkByRows_WithPositiveRemovalCount_ReducesHeight`, `ShrinkByRows_WithNegativeRemovalCount_IncreasesHeight`, `EliminateSpaceForItems_ReducesMinimumHeightByTemplateHeightTimesRemovalCount`, `MakeSpaceThenEliminateSpace_IsMinimumHeightNeutral` | `p10-t9-pass-after.2026-08-26T11-19.md`, `p10-t10-neutrality.2026-08-26T11-21.md` |
| #473 defect 1 | `DrainBackgroundLoadingTasksAsync_AwaitsATaskAddedDuringTheDrainWindow` | `p11-t6-pass-after.2026-08-26T11-30.md` |
| #473 defect 2 | `MoveEmailsAsync_WhenMoveIsCancelled_PropagatesOperationCanceledException`, `MoveEmailsAsync_AfterFirstFailure_DoesNotReadSubjectASecondTime`, `MoveEmailsAsync_WithNullGroupFromIndexLookup_DoesNotThrow` | `p5-t5-pass-after.2026-08-26T10-33.md` |
| #474 defect 1 | `ParentFieldAndConstructorParameterAreTypedIQfcFormController` | `p2-t10-pass-after.2026-08-26T09-21.md` |
| #474 defect 2 | `TryGetMoveReadiness_WithUnassignedDestination_ReturnsFalseAndProducesNotificationText`, `TryGetMoveReadiness_WithAllDestinationsAssigned_ReturnsTrueAndEmptyNotification` | `p13-t6-pass-after.2026-08-26T16-18.md` |

All pass-after paths above are relative to
`docs/features/active/qfc-collection-controller-defects-468/evidence/regression-testing/`, except the
`p1-t*` and `p14-t*` entries, which are under `evidence/qa-gates/` and `evidence/other/` as named.

---

## Acceptance verification

Five constraints, enumerated: constraint 1 (do not repeat the #468 coverage-denominator rationale),
constraint 2 (do not repeat the #474 unrelated-sibling-interfaces premise), constraint 3 (state that
#473 defect 1 is latent under the current call graph), constraint 4 (state that #474 is latent in the
current single-implementation configuration), constraint 5 (cite specific test names per defect in
place of a coverage delta).
