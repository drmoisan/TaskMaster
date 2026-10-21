# Issue #810 Update Mirror

Timestamp: 2026-09-08T10-32
PostedAs: unknown

This agent does not post to GitHub and did not run `gh`. The exact text intended for issue #810 is recorded below so it survives merge and can be posted by the caller. No GitHub URL is available and none is fabricated.

REPORT-ONLY-ITEMS: 9

---

## Exact text intended for issue #810

All eight acceptance criteria are delivered. The full C# toolchain passed in order on the final loop with no regression.

### Acceptance criteria outcomes

- AC1 — DELIVERED. The self-inflicted-deactivation guard in `ParkFocusAndCancelSelectors` is now scoped to its caller through a required `bool honourSelfInflictedGuard` parameter. `FormViewer_Deactivated` passes true, so the issue-677 contract is unchanged for a genuine `Form.Deactivate`; the Cancel teardown's `park-focus` stage passes false through an explicit lambda, so it cancels every open selector. The parameter is required rather than defaulted because C# method-group conversion does not apply optional-argument defaults and a defaulted parameter would break the teardown call site with CS0123. Fail-before: the new regression test failed with `CancelBreadcrumbSelector` invoked zero times. Pass-after: it passes.
- AC2 — DELIVERED. `QfcFormControllerDeactivateTests.cs` is byte-unmodified relative to the base commit and all 9 of its cases pass, both at baseline and at the end of the change set. The two load-bearing cases, `FormDeactivated_SelfInflictedByOwnPopup_DoesNotCancelAnySelector` and `FormDeactivated_CancelsSelectorOnEveryItemController`, both pass.
- AC3 — DELIVERED. `QfcHomeController.Cleanup()` now nulls `_tokenSource` immediately after disposing it, and nulls `_datamodel` alongside the five sibling nullings. A disposed source is no longer reachable through the `TokenSource` getter, and a repeat cleanup pass cannot clean the datamodel twice. Fail-before: 2 failed. Pass-after: 3 of 3 pass.
- AC4 — DELIVERED. `QfcFormController.Cleanup()` now wraps its body in a `try` and invokes the ribbon-release callback from a `finally` that reads the field into a local, clears the field, then invokes the local. Reading and clearing before invoking is what makes exactly-once unconditional: a `finally` that invoked before clearing would close the earlier-throw hole but not the callback-throws hole. Fail-before: the callback ran zero times when the viewer's `Dispose()` threw. Pass-after: it runs exactly once across two passes, and all 8 cases in the class pass including the no-synchronous-wait source guard.
- AC5 — DELIVERED. `FinishClose` now clears `IsCommitPending` as a fourth operation of its `CompleteAll` list. It is an element of the list rather than a statement after the call because `CompleteAll` rethrows the first failing operation, which would skip a trailing statement. The clear does not make the cancel run on the call that read a stale latch — the read is an earlier operation of the same list — it ensures the latch cannot survive the close that consumed it, so the next close, including the one `RestoreAfterOpenFailure` performs, sees a cleared latch and cancels. The regression test therefore drives two closes. Fail-before: 2 of 2 failed. Pass-after: 48 of 48 pass across the three breadcrumb host test classes.
- AC6 — DELIVERED. The stale `FinishClose` comment claiming the cancel step always runs is replaced with a two-clause correction recording that both steps are gated and the gates are independent. The latch-lifetime XML doc claiming nothing but `ShowPopup` clears the latch is corrected. The dead `SearchOwnsDropDownDismissal` accessor is removed; its backing field and all six of its live sites remain, so no CS0649 or CS0169 arises, and the full-solution analyzer rebuild is clean.
- AC7 — DELIVERED. The popup-owner store and the `AnyOpen` derivation are extracted from `QfcFormViewer` into a new `BreadcrumbPopupOwnerRegistry`, with six MSTest cases. The derivation was previously inline on a form-derived class exempt from coverage measurement, so it emitted no Cobertura class element and could not be measured; changed-line coverage of the new module is 100 percent against a 90 percent bar. The polarity is preserved: a form with no registered popup reports false, which is the genuine case.
- AC8 — DELIVERED. CSharpier format, CSharpier check, msbuild analyzers, msbuild nullable and instrumented vstest ran in that order and every step met its declared expectation. The analyzer and nullable builds each report 0 warnings and 0 errors against a baseline of 0 and 0. The nine-assembly instrumented run executed 7163 tests with zero failures, up from a 7153-test baseline by exactly the ten tests added here.

### Coverage

Repository line coverage moved from 84.6332 to 84.6341 percent and branch coverage from 79.3903 to 79.3941 percent, so `NO-REGRESSION` passes and neither rate fell. Changed-line coverage over the measurable subset is 97.12 percent.

The 85 percent line floor stated in `.claude/rules/general-unit-test.md` and `.claude/rules/quality-tiers.md` is NOT MET at 84.63 percent. That floor was already unmet on the baseline tree captured before any source edit in this work, so it is a pre-existing repository condition rather than a regression introduced here, and it is not in scope for this issue. The 80 percent floor in CLAUDE.md UT2 is MET and the 75 percent branch floor is MET.

### Out of scope, report-only — nine items requiring follow-up issues

Each of the following was identified during this work and is deliberately not fixed on this branch. Prose in a feature folder does not survive merge, so each needs a real issue.

1. `UtilitiesCS/Threading/ProgressViewer.cs:75` — un-null-guarded `Cancel()` on the shared source. `_cancelSource!.Cancel()` runs from the progress dialog's Cancel button on the same `CancellationTokenSource` created at `QfcHomeController.cs:54`. It is the only `Cancel()` on that instance that is neither null-guarded nor ordered by teardown, and it is a fourth sharer the #791 review did not enumerate. The AC3 nulling does not affect it, because it holds its own captured reference assigned at construction. The file was not changed on this branch.
2. Token-source ownership redesign. Replacing the shared `CancellationTokenSource` handoff with `CancellationToken` parameters across `QfcDatamodel`, `QfcFormController`, `QfcCollectionController`, `QfcItemController` and `ProgressTracker` is the structural fix for the whole family. Multi-file API change.
3. `QfcDatamodel` coverage exclusion. The class-level coverage exemption at `QfcDatamodel.cs:25` removes `QuiesceLoaderAsync`, `TryQueueRemainingMailItemAsync` and the whole queue-processing partial from the coverage denominator. The #791 review already recommended extracting the host-neutral logic. The conflict between the Coverage Exclusion Policy in `.claude/rules/general-unit-test.md` and the ratified exemption in CLAUDE.md UT2 pre-exists this branch.
4. CR-6 — reflection by private field name in the #796 AC4 re-pin. `QfcItemController.SearchDismissalTests.cs:85` sets `_searchOwnedDismissal` by string literal. Driving the real `TextBoxSearch_TextChanged` path instead would remove the coupling. Not required by AC6, which asked only for removal of the accessor.
5. CR-5 — popup-owner entries are never removed. Growth is bounded by the item-viewer pool and a stale entry cannot wrongly report true, because a disposed host leaves `OpenState` false. The new `BreadcrumbPopupOwnerRegistry` is the natural home for an eventual `Unregister`; none was added speculatively here.
6. `ItemViewer.Breadcrumb.cs:216` registration hop remains untested. `FindForm() as QfcFormViewer` needs a real form hierarchy. This is the AC7 residual and is recorded as such.
7. #791 N12 — misleading unconditional log lines. `QfcFormController.EventHandlers.cs:171` and `QfcHomeController.cs:404` both log that the ribbon release callback was invoked unconditionally, including when it did not run. AC4 makes the first true more often but not always, because a `RunTeardownStage` catch can still swallow a throw.
8. The reverse-ordering limit of the commit-pending latch. A native uncommitted-reason close arriving before the commit's `Close(ExplicitCommit)` would still cancel. AC5 does not change that and must not be read as doing so.
9. `QuickFiler.Test/QuickFiler.Test.csproj` exceeds 500 lines, at 533. Pre-existing. The ceiling rule enumerates production code, test code and reusable script files, and an MSBuild project file is none of these. AC7's registration adds one `<Compile Include>` line. Recorded so it is not read as a new violation.

---

## Posting status

POSTING NOT PERFORMED BY THIS AGENT. The delegation for this work reserves `gh` usage, pull-request creation and pull-request editing to the caller, so no post was attempted and `PostedAs` is recorded as `unknown` rather than as `body` or `comment`. Because `PostedAs` is not `body`, no mirror into the local `issue.md` body is required by the evidence conventions; `issue.md` was nonetheless updated by [P7-T12] through [P7-T19], which checked off its eight acceptance-criteria lines.
