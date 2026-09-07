# Feature Audit — issue #796 (QuickFiler folder drop-down closes on open; row click does not select)

- Timestamp: 2026-09-07T17-05
- Issue: #796
- Work Mode: **full-bug** — marker read from `issue.md` line 12 (`- Work Mode: full-bug`) and corroborated by `spec.md` line 9
- Authoritative AC source: **`spec.md` only** (`## Acceptance Criteria`, lines 326-331). `user-story.md` does not exist for this feature and its absence is by design, recorded at `spec.md` line 11.
- Baseline: `a6b259160f9ac1fbe251708d897fd4721486259e`; head reported by the caller: `8e427fe1`
- `issue.md` also carries a mirrored copy of the six criteria (lines 79-84). It is not the AC source in full-bug mode; it is noted so the mirror is kept consistent.

## Method

Every criterion below was evaluated against three classes of evidence, in this order: the code as it
currently stands in the worktree (read directly), the executor's fail-before / pass-after regression
artifacts, and the manual observation and decision record. Where a criterion's evidence is weaker than
its wording, that is stated rather than smoothed over.

The Bash tool was not used, per the caller's binding constraint. Consequences are recorded in
`policy-audit.2026-09-07T17-05.md` § "Review Method and Its Limits".

## AC Evaluation

### AC1 — Opening the list by arrow click or by Down leaves it open until Escape, Left, a second arrow click, an item selection, or selection of a different QfcItem

**Verdict: PASS** (with a residual recorded below).

Evidence chain, each link checked:

1. The first cause of the flash was OBSERVED, not inferred, on both gestures AC1 names. Gesture A
   (arrow click) and Gesture B (Down key) both read `FIRST-CAUSE-GESTURE-*: CANDIDATE-1` in
   `evidence/other/close-ordering-decision.md`, derived from file-ordered excerpts in
   `evidence/other/2026-09-07T12-19-dropdown-close-ordering-observation.md`.
2. The observed close was not an independent framework close. Every close on every gesture reported
   `CloseReason=CloseCalled ProgrammaticClose=True`, i.e. it was the add-in's own close call arriving
   downstream of the deactivation cancel. Candidate 2 is REFUTED on all three gestures, twice over on
   Gesture C (`AutoClose=False` at the moment it fired).
3. The cancel that produced that close is exactly what the AC2 guard now suppresses
   (`QfcFormController.Deactivate.cs` line 118).
4. The guard fires at the right moment. I verified this rather than assuming it:
   `BreadcrumbDropDownOpenLifetime.ShowCurrentSurface` sets `_host.OpenState = true` (line 268) before
   calling `_host.ShowPopup(...)` (line 276), and `IsOpen => OpenState`
   (`BreadcrumbDropDownHost.cs` line 191), so `host.IsOpen` is already true at any deactivation the
   native show provokes.
5. Managed-seam assertion: `GestureOpen_ResolvesOpenAndLeavesHostOpenWithoutClose`
   (`BreadcrumbDropDownCloseOrderingTests.cs`) asserts the open task resolves true, the host reports
   open, the session reports the selector open, and `Close` is never invoked across the gesture.

Evidence paths: `evidence/other/close-ordering-decision.md`,
`evidence/other/2026-09-07T12-19-dropdown-close-ordering-observation.md`,
`evidence/regression-testing/p7-t3-ac1-ac5-guards.md`, `evidence/qa-gates/p9-t5-full-assembly-tests.md`.

Residual, recorded so PASS is not read as more than it is: no post-fix live observation exists. The
part of AC1 that is not automatable — that no FRAMEWORK close occurs, because no framework drop-down
is shown in a headless test — is stated in the test's own `<remarks>` (lines 359-367) rather than
asserted, and in the spec's automation-feasibility table. PASS is recorded because the causal chain
closes on measured evidence at every link: the only observed close on gestures A and B was
programmatic and downstream of the cancel, so removing the cancel removes the close. A post-fix
reproduction of gestures A and B remains advisable before the issue is closed, but Outlook is
deliberately closed and this review did not relaunch it.

### AC2 — A deactivation caused by the popup taking focus does not cancel the selector session; a deactivation caused by any other window still does (#677 preserved)

**Verdict: PASS.**

- Both branches are pinned by tests on the real seam:
  `FormDeactivated_SelfInflictedByOwnPopup_DoesNotCancelAnySelector` asserts `Times.Never()` on both
  injected controllers; `FormDeactivated_CancelsSelectorOnEveryItemController` retains `Times.Once()`
  on both and now states the genuine case explicitly in Arrange rather than relying on a mock default.
- Fail-before is documented at `evidence/regression-testing/p4-t5-ac2-fail-before.md`; pass-after at
  `evidence/regression-testing/p4-t9-ac2-pass-after.md`. The test appears in the expect-fail inventory
  and is recorded Passed in the final TRX.
- The #677 contract is preserved by polarity, not by convention: `false` means GENUINE, so a viewer
  that reports nothing keeps the pre-change behaviour. The polarity is documented as load-bearing at
  the interface declaration (`IQfcFormViewer.cs`).
- The guard is scoped to the cancel loop and not to focus parking, and that scoping is an explicitly
  recorded decision (`AC2-PARK-FOCUS-SUPPRESSED: NO`) derived from the measured `WebView2Focused`
  values, not a default.
- The discriminator originally proposed (`Form.ActiveForm == null`) was REFUTED by the observation on
  all four data points and is correctly not used, and correctly not inverted and re-used either.

Limitations recorded, neither of which defeats the criterion: the producer implementation
(`QfcFormViewer.IsDeactivationSelfInflictedByOwnPopup` and the `ItemViewer.Breadcrumb.cs` registration)
sits in coverage-exempt WinForms types with no automated test (CR-7); and the guard also applies to
the #791 Cancel teardown caller, where its predicate is not meaningful (CR-3). CR-3 concerns a
different caller and a different contract, so it does not make AC2 itself unmet.

Evidence paths: `evidence/regression-testing/p4-t5-ac2-fail-before.md`,
`evidence/regression-testing/p4-t9-ac2-pass-after.md`, `evidence/qa-gates/p4-t3-itemviewer-wiring.md`,
`evidence/qa-gates/p4-t7-park-focus-decision.md`, `evidence/qa-gates/p4-t8-guard-scope.md`,
`evidence/regression-testing/p4-t4-deactivate-suite-count.md`.

### AC3 — A mouse click on a row in the open list selects that row and closes the list; the selection is committed before any auto-close cancel runs

**Verdict: PARTIAL.**

The criterion has two clauses and they carry different evidential weight.

**Clause 2 — "the selection is committed before any auto-close cancel runs": PASS.** The pending-commit
latch is implemented at the site the evidence selected (`AC3-ENFORCEMENT-SITE: HOST`, derived from
`PendingClose=False` on all three gestures, which shows a coordinator-sited latch would have sat on a
path not taken). Both polarities are pinned — `NativeCloseWhileCommitPending_DoesNotCancelSelection`
and `NativeCloseWithNoCommitPending_StillCancelsSelection` — plus
`CloseWhilePendingOpenAndCommitPending_DoesNotCancelSelection` on the pending-open path, and the two
pre-existing `CancelCount.Should().Be(1)` assertions are retained unchanged as the scoping guard. I
also traced that `Close(ExplicitCommit)` is issued by the coordinator only after the session has
already committed, so the commit genuinely precedes the host-level cancel decision on that path
(detailed in `code-review.2026-09-07T17-05.md` § 1).

**Clause 1 — "a mouse click on a row selects that row": not established post-fix.** The delivered
change makes no alteration to the row-activation path. `QuickFiler/Resources/FolderBreadcrumb.html`
was deliberately not changed, on the recorded decision `AC3-HTML-POINTERDOWN: NOT REQUIRED`. That
decision rests on a positive account — the observed first cause of Gesture C was candidate 1, the
deactivation cancel running before the row's selection could commit, which AC2 now suppresses — and
that account is sound. But the decision record itself states the limit in terms this audit will not
soften:

> this decision states that the evidence does not support the page change, not that the page change
> has been shown unnecessary. If the AC2 seam lands and a row click still fails to select, the
> question is reopened.

and `spec.md` carries the matching assumption as explicitly UNSETTLED (line 174). No post-fix
observation of a row click exists, and no automated test can supply one: the activation path travels
through the bridge coordinator and router, neither of which is instrumented, and the transcript's
silence on activation "discriminates between them not at all".

So clause 1 currently rests on a causal argument, not an observation, and the feature's own record
flags it as reopenable. That is precisely the condition PARTIAL exists to express.

**Recommendation:** AC3 should be UNCHECKED in `spec.md` line 328 until one of the following is
recorded:

1. a single post-fix Gesture C observation (type, then click a row) confirming the row selects — one
   runbook step, at whatever time Outlook is next open; or
2. an explicit maintainer acceptance of clause 1 on the causal argument, transcribed into `spec.md`
   so the acceptance is visible in the merged history rather than living only in a review artifact.

This is a verification and documentation gap, not a code defect. **It is not blocking**, and no code
change is requested for it.

Evidence paths: `evidence/regression-testing/p5-t3-ac3-fail-before.md`,
`evidence/regression-testing/p5-t7-ac3-pass-after.md`, `evidence/regression-testing/p5-t6-scoping-guard.md`,
`evidence/qa-gates/p5-t4-enforcement-site.md`, `evidence/qa-gates/p5-t5-html-listener.md`,
`evidence/other/close-ordering-decision.md` § P3-T3.

### AC4 — The #680 leave-handoff latch covers the mouse open path as well as the Down-arrow path

**Verdict: PASS.**

- The mechanism is the `_searchOwnedDismissal` provenance latch, verified live in the current source:
  written at lines 183, 220, 234, 257 and 265 and read at line 263 in
  `if (!_searchOwnedDismissal) return;`, which is the guard the criterion asks for. Producers are the
  two search-driven open sites; the mouse path is covered by never setting the flag, so it needs no
  edit anywhere.
- RED-first is documented and is genuine: `evidence/regression-testing/p6-t4-ac4-fail-before.md`
  records EXIT_CODE 1 with `ExpectedExitCode: 1`, Total 2 / Passed 1 / Failed 1, the failing test named
  exactly, and the Moq message "Expected invocation on the mock should never have been performed, but
  was 1 times" — a failure for the intended reason. Total 2 rather than 0 also proves the new compile
  entry took effect.
- Both polarities are pinned: `SearchLeaveAfterMouseDrivenOpen_DoesNotCloseDropDown` (`Times.Never()`)
  and `SearchLeaveAfterSearchDrivenOpen_ClosesDropDown` (`Times.Once()`, driving the real open path).
- The #680 contract is preserved: the one-shot `_searchLeaveHandoffPending` latch is untouched and its
  read-and-clear semantics are intact at lines 247-251; the five sibling #680 tests in
  `SearchDismissalTests` are unchanged and pass.
- The dead accessor at line 209 does not affect this verdict: the field, not the property, is the
  mechanism, and the field is fully live. See `policy-audit.2026-09-07T17-05.md` § 8 F1.

Evidence paths: `evidence/qa-gates/p6-t1-ac4-seam.md`, `evidence/qa-gates/p6-t3-compile-entry.md`,
`evidence/regression-testing/p6-t4-ac4-fail-before.md`, `evidence/regression-testing/p6-t6-ac4-pass-after.md`,
`evidence/regression-testing/p8-t1-search-dismissal-repin.md`,
`evidence/regression-testing/p8-t2-search-dismissal-verification.md`.

### AC5 — Row-set refreshes while open continue not to close the list (#438 AC-3 regression guard)

**Verdict: PASS.**

- `RowSetRefreshWhileOpen_NeverClosesHost` performs two row-set replacements while the selector is
  open and asserts `Close` is never invoked on the mocked host and that
  `BreadcrumbCoordinator.IsSelectorOpen` is still true.
- The guard is meaningful rather than circular: the session-preserving replacement path in
  `UtilitiesCS` is deliberately outside this diff, so what the test observes is the untouched path.
  The exclusion is verified two ways — the scope-boundary gate records 0 paths under `UtilitiesCS/` or
  `UtilitiesCS.Test/`, and the caller-supplied full-code diff contains no such path.

Evidence paths: `evidence/regression-testing/p7-t3-ac1-ac5-guards.md`,
`evidence/qa-gates/p7-t4-ac5-exclusion.md`, `evidence/qa-gates/p9-t10-scope-boundary.md`.

### AC6 — The first implementation step instruments `ParkFocusAndCancelSelectors` and `OnDropDownClosed` with debug log lines so the runtime ordering is confirmed before the fix is chosen

**Verdict: PASS.**

Every clause of this criterion is separately checkable and each was checked:

- **Both named sites are instrumented.** `ParkFocusAndCancelSelectors` logs at entry
  (`QfcFormController.Deactivate.cs` lines 93-99) plus one line per item in the cancel loop (lines
  125-130). `OnDropDownClosed` logs at entry, ahead of the guard return
  (`BreadcrumbDropDownHost.Diagnostics.cs`), so a close the host suppresses is still visible.
- **The required fields are present.** Entry: `WebView2Focused`, `ActiveFormNull`, `Groups`; per item:
  `ItemNumber`, `SelectorWasOpen` (rendered `unavailable` rather than a fabricated boolean when
  unobserved). Close: `CloseReason` (from the event args, which the handler previously discarded),
  `ProgrammaticClose`, `OpenState`, `AutoClose`, `Disposed`, `PendingClose`. Two tests pin the field
  sets against the pure formatters, so the AC6 evidence is a deterministic managed-seam assertion
  rather than a source-text scan.
- **It was FIRST.** The instrumentation is commit `0dfcb402f4e3323c7f652b63701edd9bc5eb9fe0`
  (`evidence/qa-gates/p1-t15-instrumentation-commit.md`), and every behavioural change landed in later
  phases. The observation artifact records the build under test as `ec674e0c`, whose parent is
  `0dfcb402` and which "adds documentation and plan check-offs only and changes no compiled source",
  so the instrumentation is present in the observed build.
- **The ordering was read off the log and the fix was chosen from it.** The decision record derives
  five decisions from quoted, file-ordered excerpts, and follows the observation over the prediction
  in the two places they conflict. The `CloseReason` value — previously discarded — is what settled
  candidate 2.
- **Logging shape matches repository convention:** sentence prefix followed by `Key=Value` pairs,
  `logger` in the controller neighbourhood and `log` in the viewer neighbourhood, Debug level, no
  control-flow change.

The optional third site at the search-leave handler was not added; AC6 does not require it, and the
consequence — that candidate 3 is not directly observable — is recorded honestly in both the
observation and the decision record rather than being papered over with an argument from silence.

Judged, as directed, on the HI-796-1 artifact plus the automated instrumentation tests. Outlook was
not relaunched.

Evidence paths: `evidence/regression-testing/p1-t11-ac6-instrumentation-tests.md`,
`evidence/regression-testing/p1-t12-behaviour-neutrality.md`, `evidence/qa-gates/p1-t2-host-line-count.md`,
`evidence/qa-gates/p1-t15-instrumentation-commit.md`,
`evidence/other/2026-09-07T12-19-dropdown-close-ordering-observation.md`,
`evidence/other/p2-t2-manual-observation-conformance.md`, `evidence/other/close-ordering-decision.md`,
`runbooks/confirm-dropdown-close-ordering.runbook.md`.

## Verdict Table

| AC | Verdict | Checkbox action |
|---|---|---|
| AC1 | PASS | leave checked |
| AC2 | PASS | leave checked |
| AC3 | PARTIAL | **should be unchecked** pending clause 1 disposition |
| AC4 | PASS | leave checked |
| AC5 | PASS | leave checked |
| AC6 | PASS | leave checked |

Per the `acceptance-criteria-tracking` protocol, a reviewer checks off PASS items that are not already
checked and leaves PARTIAL items unchecked with the gap documented. All six are already marked `[x]`
in `spec.md`, so no item required a new check-off. AC3 is the one mark this review cannot support as
written; it is reported here rather than altered, because unchecking it is a decision about the
criterion's disposition that belongs with the orchestrator and the maintainer, and because AC3's gap
is a verification gap with two acceptable dispositions rather than a defect requiring code.

### Acceptance Criteria Status

- Source: `docs/features/active/2026-09-06-quickfiler-folder-dropdown-closes-on-open-and-click-does-not-select-796/spec.md`
- Total AC items: 6
- Checked off (delivered): 6
- Verified PASS by this review: 5
- Verified PARTIAL by this review: 1
- Remaining (unchecked): 0
- Items this review cannot support as checked: AC3 — "A mouse click on a row in the open list selects that row and closes the list; the selection is committed before any auto-close cancel runs." The commit-before-cancel clause is PASS; the row-selects clause has no post-fix evidence and is recorded as reopenable by the feature's own decision record.

## Baseline Comparison

| Dimension | Baseline | Head | Delta |
|---|---|---|---|
| `QuickFiler.Test` total tests | 1370 | 1380 | +10 |
| `QuickFiler.Test` failures | 0 (empty set) | 0 (empty set) | none |
| Analyzer warnings / errors | 0 / 0 | 0 / 0 | none |
| Nullable warnings / errors | 0 / 0 | 0 / 0 | none |
| CSharpier unformatted files | 0 | 0 | none |
| Repo-wide line coverage | 24.1387% | 24.1857% | +0.0470 pp |
| Repo-wide branch coverage | 22.9747% | 23.0082% | +0.0335 pp |
| Largest write-set file | 498 lines | 496 lines | −2 |
| Code paths changed vs base | — | 15 (all write-set members) | — |

## Residuals Recommended for Follow-up

Not blocking, and recorded here so they are not lost at merge:

1. **AC3 clause 1 disposition** — one post-fix Gesture C observation, or a transcribed maintainer
   acceptance in `spec.md`.
2. **CR-3** — the AC2 guard also gates the #791 Cancel teardown caller. Scope the guard to the
   deactivation caller and add a teardown regression test.
3. **CR-2** — `IsCommitPending` survives a popup lifetime when an open fails before `ShowPopup`.
4. **CR-1 / CR-4 / CR-6** — stale comment at `BreadcrumbDropDownHost.cs:450`; the dead
   `SearchOwnsDropDownDismissal` accessor; the reflection-by-field-name coupling in the re-pin. These
   three are best fixed together, since removing the reflection would give the accessor a reader or
   make its deletion obvious.
5. **`issue.md` AC mirror** — if AC3 is unchecked in `spec.md`, unchecked it in the `issue.md` mirror
   at line 81 too, so the two do not diverge.
