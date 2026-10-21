# Feature Audit — qfc-item-controller-defects (Issue #484)

- Reviewer: feature-review agent
- Timestamp: 2026-08-26T10-22
- Branch: `bug/qfc-item-controller-defects-484` @ `4f2b55f1` vs merge-base `61edc19b`
- Work mode: `full-bug` — `spec.md` is the sole AC source (50 checkbox criteria). `user-story.md` is
  intentionally absent per the spec and `issue.md`; its absence is not a defect.

## Method

Every criterion was evaluated against the delivered source (read directly), the branch diff, and the
committed evidence artifacts; figures quoted from evidence were spot-recomputed where possible
(line counts re-measured, coverage ratio re-derived from the recorded Cobertura attributes, new-test
result arithmetic re-counted from the test diff). Verdicts: PASS (criterion satisfied and evidence
verified), with any divergence between criterion prose and delivered state recorded explicitly.

## Evaluation

### Issue #480 — ToggleNavigation double toggle (5 criteria)

| # | Criterion (abbrev.) | Verdict | Evidence |
|---|---|---|---|
| 1 | Unconditional `Toggle(false)` dispatch deleted; one dispatch per branch | PASS | Source: `FocusAndTheme.cs:168-178` now has exactly one dispatch per branch; diff shows the single deletion. |
| 2 | `async: false` asserts `Times.Once()` | PASS | `FocusAndThemeTests.cs:323` tightened in place (only line changed in that file). |
| 3 | `async: true` asserts `Times.Once()` in a new test | PASS | `ToggleNavigation_Asynchronous_TogglesPositionTipsExactlyOnce` (MailActionsTests, C2-routed home), did not exist at base. |
| 4 | Tightened assertion failed against unfixed code | PASS | `evidence/regression-testing/480-sync-tightened-fail.md` records the pre-fix failure; `480-async-fail.md` covers the async test. |
| 5 | `ToggleNavigation(bool)` still declared; `IQfcItemController.cs` unmodified | PASS | Method and overload present in source; interface file absent from diff. |

### Issue #481 — event unwiring path (9 criteria)

| # | Criterion (abbrev.) | Verdict | Evidence |
|---|---|---|---|
| 6 | Three unwire methods exist; `UnwireEvents` order mirrors `WireEvents` + `DetachWebResourceRequestedHandler()` third | PASS | `EventWiring.cs:391-397`, verified in source. |
| 7 | All 16 intent subscriptions detached, `VerifyRemove` | PASS | Reviewer counted `WireIntentEvents` attachments (16) against `UnwireIntentEvents` detachments (16): exact set match, delegate-equal. Test carries 16 `VerifyRemove`/`Times.Once()` assertions. |
| 8 | All 6 control-tree subscriptions detached; wire-unwire-raise test with `Times.Never()` + unchanged `BackColor` | PASS | 6/6 statement match verified in source; `UnwireControlTreeEvents_WithHeadlessItemViewer_DetachesKeyboardAndMouseHandlers` delivers the mirrored fixture. |
| 9 | Same `ForAllControls` exclusion list | PASS | Both pass `L0vhBreadcrumb_WebView2` as the sole exclusion; verified in source. |
| 10 | `Cleanup()` calls `UnwireEvents()` before `_itemViewer = null` and `_kbdHandler = null` | PASS | `ViewerSetup.cs`: `UnwireEvents()` at :458, `_itemViewer = null` at :460, `_kbdHandler = null` at :472. |
| 11 | Null-tolerant guards in both unwire methods | PASS | All specified guards present (`_itemViewer` null early-return, `FolderKeyDown` kbd guard, concrete-`ItemViewer` early-return, kbd-walk skip, `Buttons`/`MenuItems` null guards). |
| 12 | `Cleanup()` does not throw with null kbd/Buttons and mock viewer | PASS | `Cleanup_WithNullKeyboardHandlerAndNonItemViewerViewer_DoesNotThrow`, in the 959/959 run. |
| 13 | Two pre-existing `Cleanup()` tests pass unchanged | PASS | Neither test file region was modified (diffs are append-only or absent); both in the 959/959 final run. |
| 14 | `WebResourceRequested` delegate + `CoreWebView2` captured; detached at teardown; inspection-only verification recorded with reason; no new `[ExcludeFromCodeCoverage]` | PASS | Fields at `ViewerSetup.cs:33-34`, capture at :85/:92, detach at :486-494 using the same delegate instance; inspection dossier `fail-before-exception.webresourcerequested-detach.md`; zero new exclusion attributes in diff. |

### Issue #483 — MoveMailAsync error handling and cancellation (7 criteria)

| # | Criterion (abbrev.) | Verdict | Evidence |
|---|---|---|---|
| 15 | Catch logs error, notifies, propagates | PASS | `MailActions.cs:140-155`: `logger.Error`, `NotifyMoveFailure`, `throw new InvalidOperationException(..., e)`. |
| 16 | Faulting factory test asserts `InnerException` is original fault | PASS | `MoveMailAsync_WhenFilerFactoryThrows_WrapsAndRethrowsWithInnerException` (plus the enqueue-fault variant). Fail-before: `483-fail.md` (6 targeted failures). |
| 17 | `internal Action<string> MoveFailureNotifier` with `MessageBox.Show` default; invoked exactly once on failure with no modal reached | PASS | `MailActions.cs:30-31`; test asserts `notifications == 1` with the seam replaced. |
| 18 | Marshalled through `_uiDispatcher` when non-null; direct when null | PASS | `NotifyMoveFailure` (`MailActions.cs:36-46`); dispatcher path verified by `MarshalsNotificationThroughDispatcher` (`Invoke` `Times.Once()`), null path by the factory-throw test. |
| 19 | `ThrowIfCancellationRequested()` first statement (outside `try`) of all three methods | PASS | Verified in source for `MoveMailAsync`, `FlagAsTaskAsync`, `EnumerateConversationAsync`. |
| 20 | Pre-cancelled-token test per method asserts OCE and no downstream collaborator call | PASS | Three tests present. For `MoveMailAsync` the factory-not-called assertion is explicit; for the other two, non-invocation is proven structurally — `_uiDispatcher` is null in the arrangement, so any downstream dispatch would have surfaced as `NullReferenceException` rather than the asserted `OperationCanceledException`. |
| 21 | `MoveMailAsync` return type unchanged; `QfcCollectionController.cs` unmodified | PASS | Signature `public async Task MoveMailAsync()` unchanged; file absent from diff. |

### Issue #484 — Cleanup timer disposal and stale `_mailActions` (6 criteria)

| # | Criterion (abbrev.) | Verdict | Evidence |
|---|---|---|---|
| 22 | Dispose before null (`_emailIsReadTimer?.Dispose(); _emailIsReadTimer = null;`) | PASS | `ViewerSetup.cs:478-479`, exact order verified in source. |
| 23 | T1: `Timeout.Infinite` timer, field null, `Change` throws `ObjectDisposedException`, no waits | PASS | `Cleanup_DisposesEmailIsReadTimerBeforeNullingIt`; no sleep/delay/wall-clock APIs (regex-verified). Fail-before: `484-fail.md`. |
| 24 | `ApplyReadEmailFormat` early-returns on four nulls; signature unchanged | PASS | `FocusAndTheme.cs:318-330`. Signature `public void ApplyReadEmailFormat(object state)` unchanged. (See code-review F1 for a residual TOCTOU observation beyond the criterion's scope.) |
| 25 | T2: post-`Cleanup` callback does not throw, `Save()` never called | PASS | `ApplyReadEmailFormat_AfterCleanup_IsInertAndDoesNotSave`. |
| 26 | `Cleanup()` nulls `_mailActions`; rebind test via `SaveParameters` | PASS | `ViewerSetup.cs:481`; `Cleanup_NullsMailActions_AndSaveParametersRebindsIt` asserts null-after-cleanup, adapter rebinding, and forwarding to the new `MailItem`. |
| 27 | `QfcItemController.Navigation.cs` not modified | PASS | Absent from diff. |

### Issue #485 — WebView2 handler unguarded inputs (6 criteria)

| # | Criterion (abbrev.) | Verdict | Evidence |
|---|---|---|---|
| 28 | `TryResolveCidResource` exists with the four guards | PASS | `ViewerSetup.cs:215-255`: URI, map, match, `AttachmentData` guards all present. |
| 29 | `Uri.TryCreate(..., UriKind.Absolute, ...)`; false + null outs for malformed/relative/empty-final-segment, each with its own test | PASS | Source verified; `[DataTestMethod]` with exactly three `[DataRow]` cases. Fail-before: `485-fail.md`. |
| 30 | False + null outs for map miss / null map / null `AttachmentData`, each with its own test | PASS | Three dedicated tests present. |
| 31 | True + exact payload reference + MIME type; octet-stream fallback; both tested | PASS | `BeSameAs(bytes)` asserts reference identity; both extension cases tested. |
| 32 | Lambda reduced to adapter; null-safe `ItemHelper?.AttachmentsInfo`; response only on true | PASS | `ViewerSetup.cs:92-106` verified; `BuildContentIdMap` handles a null argument by returning an empty map (verified in `CidImageResolver.cs:34-55`). |
| 33 | #485 tests construct no controller, `ItemViewer`, `MailItemHelper`, or `CoreWebView2*` type | PASS | The group calls the `internal static` member with plain values and `Mock<IAttachment>`; verified in the test diff. |

### Upstream contract and scope discipline (5 criteria)

| # | Criterion (abbrev.) | Verdict | Evidence |
|---|---|---|---|
| 34 | No public member added; no member removed | PASS | All nine added members are `internal`/`private`; nothing removed (the replaced anonymous lambda was not a member). Verified from the diff; corroborated by `evidence/qa-gates/public-surface.md`. |
| 35 | Both interface files byte-identical | PASS | Neither appears in `git diff --name-status` over the full branch. |
| 36 | Changed set is a subset of the nine owned files; named forbidden files unmodified | PASS | Exact match: 4 production + 5 test files, plus feature-folder docs. `Navigation.cs`, `ItemViewer*.cs`, `KbdActions.cs`, both `.csproj` files absent from diff. |
| 37 | All three `Cleanup()` statement-order constraints hold | PASS | Verified directly in source (unwire before viewer/kbd nulling; dispose before timer nulling; breadcrumb detach before `_breadcrumbViewer` nulling). |
| 38 | Post-`Cleanup()` lifecycle invariant with the single documented `WebResourceRequested` exception | PASS | 22 of 23 detachments regression-tested; the 23rd is delegate-identity-exact by construction (captured field) and documented in the inspection dossier; `evidence/qa-gates/lifecycle-invariant.md`. |

### File-size, toolchain, and coverage (12 criteria)

| # | Criterion (abbrev.) | Verdict | Evidence |
|---|---|---|---|
| 39 | Every touched file at most 500 lines; nine counts recorded | PASS with recorded divergence D-1 | Reviewer re-measured all nine: max 499. Divergence: the criterion's prose projects `ViewerSetupTests.cs` as an unchanged 474-line file; delivered is 498 (+24) because the #484 rebind test was relocated there under plan constraint C2 capacity rule 3, which this reviewer verified authorizes relocation verbatim ("the test group may be relocated to a different owned test file... with a header comment naming the issue number" — the header comment is present at `ViewerSetupTests.cs:474`). The plan labels the per-file table a "starting assignment, not a per-file mandate", and the criterion itself defers the distribution to "the plan's constraint C2 assignment". The binding requirement (cap + recorded counts) is fully met; the stale projection is documented in `evidence/other/ac-reconciliation.md` (D-1) and judged accurate by this reviewer. |
| 40 | New tests use MSTest/Moq/FluentAssertions; no sleeps/delays/waits/temp files | PASS | Verified by inspection and regex scan of the full test diff. |
| 41 | Exactly one new real `ItemViewer` test; no pump/`Show()`; context restored | PASS | Grep count = 1; try/finally context restoration verified. |
| 42 | `csharpier check` clean | PASS | Evidence EXIT 0 (1520/0); independently re-run by this reviewer on the head commit: EXIT 0. |
| 43 | msbuild analyzers rebuild, zero errors | PASS | `evidence/qa-gates/msbuild-analyzers.md` (EXIT 0; 5 pre-existing System.Reactive packages.config warnings, not errors). |
| 44 | msbuild TreatWarningsAsErrors rebuild, zero errors | PASS | `evidence/qa-gates/msbuild-nullable.md` (EXIT 0). |
| 45 | vstest zero failures; pass count >= baseline + added | PASS | 959/959 = 938 + 21; the 19-methods-to-21-results arithmetic re-verified by this reviewer against the test diff. |
| 46 | Single consecutive four-stage pass, no intervening modification | PASS | `toolchain-consecutive-pass.md`: ordered timestamps, SHA-256 identity across all nine files, zero restarts (one disclosed environment stall with no outcome and no file change before the successful test invocation). |
| 47 | Repo-wide line coverage >= 80%; changed-line coverage not reduced | PASS | 84.8323% (up from 84.775%); no baseline-covered line uncovered; per-file rates all up. The separate 85% floor in `.claude/rules/general-unit-test.md` is a pre-existing repository-wide conflict recorded in the policy audit; it is not part of this criterion. |
| 48 | New members >= 90% except the three named carve-outs | PASS with recorded divergence D-2 | All five named members at 100%; `DetachWebResourceRequestedHandler` at 62.5% (non-zero, uncovered lines exactly the guarded `-=` block) under the criterion's own carve-out. Divergence: the criterion predicts the default `MoveFailureNotifier` delegate measures zero; it measures 100% because the single-line property initializer registers a construction hit on the body's source line. The body is never invoked by any test (every failure-path test replaces the seam), exactly as the criterion's rationale describes, and the delivered state strictly exceeds the requirement — the member does not even need its carve-out. Relocating the previously-uncovered `MessageBox.Show` (baseline hits=0 on lines 119-121) reduces no changed line's coverage. Documented in `ac-reconciliation.md` (D-2); judged accurate. |
| 49 | No new `[ExcludeFromCodeCoverage]` anywhere | PASS | Zero occurrences in the branch diff. |
| 50 | Fail-before evidence per issue | PASS | Seven fail-before artifacts plus the documented inspection-only exception dossier; spot-checked `483-fail.md` (EXIT 1, 6 targeted failures with verbatim reasons) and `480-sync-tightened-fail.md`. |

## Recorded divergences (both judged accurate and non-blocking)

- **D-1** (criterion 39): `ViewerSetupTests.cs` delivered at 498 lines, not the projected unchanged
  474, under a verified plan-authorized relocation. Binding cap satisfied at 498 <= 500.
- **D-2** (criterion 48): default `MoveFailureNotifier` initializer measures 100%, not the predicted
  zero, from a construction hit on the single-line initializer; the delivered state dominates the
  requirement.

Both were transparently self-reported by the executor in `evidence/other/ac-reconciliation.md`
before this review; this reviewer independently confirmed each figure and each authorizing
provision. Neither divergence weakens a binding requirement, so both criteria remain checked.

## Check-off reconciliation

All 50 criteria evaluated PASS and all 50 were already checked in `spec.md`; no check-off changes
were required and none were made. No criterion was unchecked. Verified: the spec diff over the
branch consists of exactly 50 `- [ ]` to `- [x]` flips with zero text edits.

### Acceptance Criteria Status
- Source: `docs/features/active/qfc-item-controller-defects-484/spec.md`
- Total AC items: 50
- Checked off (delivered): 50
- Remaining (unchecked): 0
- Items remaining: none

## Verdict

All 50 acceptance criteria PASS against the delivered source and evidence. Zero blocking findings.
The feature is ready to merge to `epic/quickfiler-bug-family-integration`.
