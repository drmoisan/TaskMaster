# Feature Audit — issue #810 (quickfiler-teardown-and-dropdown-residuals)

- **Timestamp:** 2026-09-08T20-15
- **Branch:** `bug/quickfiler-teardown-and-dropdown-residuals-810` @ `a9d11dd2048e98e4f66cee05dc6a0f30a20a4407`
- **Baseline:** `origin/main` @ `0e9c95a5dd45104d82f46fd801973a6bc068f25f`
- **Work mode:** `full-bug` — the authoritative acceptance-criteria source is `spec.md` only. `issue.md` mirrors the same eight criteria and was checked for consistency. `user-story.md` is correctly absent; its absence is not a finding.
- **Verdict:** PASS — 8 of 8 acceptance criteria satisfied. 0 blocking findings.

## 1. Method

Each criterion was evaluated against the source on disk first, and only then against the evidence artifact that claims it. Where a criterion is pinned by a test, the test was read to confirm it discriminates against the pre-fix behaviour rather than merely passing. Where a criterion is a preservation or deletion claim, it was checked by independent search over the head tree rather than by reading the diff alone.

## 2. Acceptance criteria evaluation

### AC1 — the self-inflicted-deactivation guard is scoped to the `Form.Deactivate` caller

**PASS.**

Code: `ParkFocusAndCancelSelectors(bool honourSelfInflictedGuard)` at `QfcFormController.Deactivate.cs:91`; the guard at `:122-128` gains `honourSelfInflictedGuard &&` as its first conjunct; `Deactivate.cs:27` passes `true`; `EventHandlers.cs:144-147` passes `false` through an explicit lambda while keeping the `"park-focus"` stage literal.

Independent check: a repository-wide identifier search returns exactly two invocations and no third caller anywhere in production or test code, so the parameter is supplied at every call site and the teardown path is the only one that suppresses the guard.

Test: `ActionCancelAsync_SelfInflictedByOwnPopup_StillCancelsEverySelector` sets `IsDeactivationSelfInflictedByOwnPopup` to `true` on the viewer mock and asserts `CancelBreadcrumbSelector()` `Times.Once` on both item controllers. Before the fix the guard returned ahead of the loop and the count was 0.

Evidence: `evidence/regression-testing/p1-t3-ac1-fail-before.md` (exit 1, declared 1, mechanism named), `evidence/regression-testing/p1-t8-ac1-pass-after.md` (exit 0).

### AC2 — the issue-677 keyboard-lock contract is preserved for a genuine deactivation

**PASS.**

This is a preservation criterion with two halves, and both hold.

The unmodified-file half: `QuickFiler.Test/Controllers/QfcFormControllerDeactivateTests.cs` does not appear in the branch diff's name-status listing, and `[P7-T10]` confirms it with both a scoped name-listing diff (0 lines) and a scoped porcelain status (0 lines) — the second span being necessary because a name-listing diff cannot report an untracked replacement.

The green half: all 9 cases pass at head, the same 9 that passed at the pre-change baseline, including the two load-bearing ones, `FormDeactivated_SelfInflictedByOwnPopup_DoesNotCancelAnySelector` and `FormDeactivated_CancelsSelectorOnEveryItemController`.

The substantive half, checked against the source rather than inferred from the two runs: every item on the plan's D16 prohibited-change list was searched for at head and none was made. The guard exists, its polarity is unchanged (an empty registry yields `false`, which is the GENUINE case and which Moq's default `bool` return also produces), its position remains below the focus-parking step at `:100-103`, `ParkFocusOffWebView2()` is still called on both paths, the `MayTakeFocus` / `FocusPending` / `FocusAnchorIfPermitted` machinery and the `ItemViewer.Breadcrumb.cs` wiring are untouched, and no `ActionCancelAsync` stage was removed or reordered. The per-item table is in `code-review.2026-09-08T20-15.md` section 2.1.

Evidence: `evidence/baseline/p0-t14-ac2-fence-baseline.md`, `evidence/regression-testing/p1-t9-ac2-fence-after-ac1.md`, `evidence/qa-gates/p7-t10-ac2-fence-final.md`.

### AC3 — no disposed shared `CancellationTokenSource` remains reachable

**PASS.**

Code: `QfcHomeController.cs:389-391` now reads `_tokenSource?.Dispose(); _tokenSource = null; _datamodel = null;`.

Reachability check performed against the callers rather than accepted from the criterion: the two paths the issue names are `ActionCancelAsync`'s `RunTeardownStage("cancel-token", () => _parent?.TokenSource?.Cancel())` at `EventHandlers.cs:133` and `_parent?.DataModel?.QuiesceLoaderAsync(...)` at `:155`. Both use null-conditional access, so once the backing fields are `null` each becomes a no-op instead of reaching a disposed source. The `TokenSource` and `DataModel` getters return the nulled fields directly, so no stale reference survives on that route.

Tests: `Cleanup_NullsTokenSourceSoLaterCancelCannotReachDisposedSource` asserts the property is null after cleanup (it was non-null before the fix); `Cleanup_DisposesTokenSourceAndDetachesWorkerCompleted` gains a second `Cleanup()` pass asserting `IQfcDatamodel.Cleanup()` ran `Times.Once` across both passes (it ran twice before the fix). Both discriminate.

Evidence: `evidence/regression-testing/p2-t3-ac3-fail-before.md` (2 failed), `evidence/regression-testing/p2-t6-ac3-pass-after.md` (3 of 3).

Note: the change adds one more unguarded post-cleanup read surface for `_datamodel`. Assessed as bounded and consistent with the five sibling fields already nulled beside it; see CR-5 in the code review. It does not qualify the PASS.

### AC4 — the ribbon-release callback runs under `finally`, exactly once, whichever stage threw

**PASS.**

Code: `QfcFormController.SetupDisposal.cs:213-273`. The body is wrapped in `try`; the `finally` reads the field into a local, clears the field, then invokes the local.

Exactly-once was verified by path analysis over all four cases (normal completion, body throws, callback throws, repeat call) rather than accepted from the artifact. The read-and-clear ordering is what closes the callback-throws case; a clear placed after the invoke would not. The full table is in `code-review.2026-09-08T20-15.md` section 2.2.

Test: `Cleanup_ViewerDisposeThrows_StillInvokesParentCleanupOnce` plants a throwing `Dispose()`, drives two passes, asserts both propagate the planted exception, and asserts the counter equals 1 across both. A callback that ran on both passes, or on neither, fails it.

Collateral invariant: `Cleanup_SourceContainsNoSynchronousWait` reads the whole file text and rejects `.Wait(`, `.Result`, `Thread.Sleep` and `Task.Delay`. The restructure introduces none and the test passes.

Evidence: `evidence/regression-testing/p3-t3-ac4-fail-before.md` (callback count 0, mechanism named), `evidence/regression-testing/p3-t6-ac4-pass-after.md` (8 of 8 in the class).

### AC5 — the commit-pending latch lives for exactly one popup lifetime on every path

**PASS, with a disclosed wording deviation.**

The criterion's text says the latch is "cleared on consumption in `BreadcrumbDropDownHost.RestoreAfterOpenFailure`". The delivered clear is in `FinishClose`, the single completion point through which `RestoreAfterOpenFailure` and both other close paths pass. The deviation is not silent: `spec.md`'s own "Production change that satisfies it" column for AC5 specifies the `FinishClose` placement and explains why clearing only in `RestoreAfterOpenFailure` would fix one path and restate the invariant as an obligation on two others. The delivered code matches the specified change, and it satisfies the criterion's operative clause ("on every path") more completely than its locative clause would.

Code: `BreadcrumbDropDownHost.Open.cs:160` places `() => IsCommitPending = false` as the fourth element of the `CompleteAll` operation list. Element rather than trailing statement is load-bearing: `CompleteAll` (`BreadcrumbDropDownHost.cs:432-451`) rethrows the first failing operation after running all of them, so a statement after the call would be skipped by an earlier throw while a list element would not.

Discrimination: `RestoreAfterOpenFailure_WithStaleCommitPending_StillCancelsAndClearsLatch` drives two closes — the first consumes the latch and asserts no cancel, the second (through `RestoreAfterOpenFailure`) asserts a cancel and a cleared latch. Without the fix the latch survives the first close and suppresses the second cancel, which is what the fail-before artifact records (`CancelCount` 0 against a required 1). Two closes are necessary, not incidental, because the read is an earlier operation of the same list than the clear.

Evidence: `evidence/regression-testing/p4-t3-ac5-fail-before.md` (2 of 2 failed, both mechanisms named), `evidence/regression-testing/p4-t12-ac5-pass-after.md` (48 of 48 across three breadcrumb host classes).

### AC6 — stale comment corrected, dead accessor removed

**PASS.**

Comment: `BreadcrumbDropDownHost.Open.cs:151-153` now reads that only the focus step is gated for issue #677, that the cancel step above is itself gated on the commit latch, and that the two gates are independent. The prior claim "the cancel step above always runs" is gone. The latch-lifetime XML doc at `:103-108` is likewise corrected to record the second clear site.

Deletion: `SearchOwnsDropDownDismissal` and its XML doc are gone from `QfcItemController.EventHandlers.cs`. A repository-wide search returns zero remaining references of any kind, in production or test code. The backing field survives with 5 writes (`:183, :214, :228, :251, :259`) and 1 read (`:257`, `if (!_searchOwnedDismissal)`), which matches the spec's stated counts exactly and is why no CS0649 or CS0169 can arise. The full-solution analyzer rebuild reports 0 warnings and 0 errors, which is the criterion's stated evidence.

Evidence: `evidence/qa-gates/p5-t2-ac6-analyzer-build.md`, `evidence/qa-gates/p7-t4-msbuild-nullable.md`, `evidence/regression-testing/p5-t3-search-dismissal-suite.md` (6 of 6, the reflection-based re-pin unaffected).

### AC7 — the AC2 producer has automated test coverage

**PASS for the derivation, with the wiring residual explicitly recorded.**

Code: `QuickFiler/Viewers/BreadcrumbPopupOwnerRegistry.cs` is new, carries `#nullable enable`, carries no `[ExcludeFromCodeCoverage]`, and holds both the store and the disjunction. `QfcFormViewer.cs:213-214, :227-228, :239` reduce to one field and two forwarding members. Both new files are registered in their legacy non-SDK projects (`QuickFiler.csproj:417`, `QuickFiler.Test.csproj:84`); the exact test-count reconciliation (7153 + 10 = 7163) independently proves both entered the compilation, which is the failure mode a missing `<Compile Include>` would have produced silently.

Tests: six cases as specified — empty registry, single closed, single open, two owners with one open, same-key replacement (with `Times.Never` on the superseded predicate, which is what distinguishes replacement from appending), and null arguments ignored.

Coverage: the new module reports 9 measurable lines, all 9 covered, against the 90 percent new-code bar. That is the criterion's substance: the derivation was previously inline on a class-level-exempt `Form` subclass that emits no Cobertura class element at all, so it could not be measured in either direction.

Residual: the registration hop at `ItemViewer.Breadcrumb.cs:216` (`FindForm() as QfcFormViewer`) and the two forwarding members on the exempt viewer remain unmeasurable. The criterion's own "Automatable end to end?" column declares this as "Seam only" before the work started, and `evidence/other/p6-t9-ac7-residual.md` records it with `RESIDUAL-IN-SCOPE: NO` and the reason (a seam over `FindForm()` would be a change to the wiring that D16 prohibits). The residual is stated rather than implied, which is what the criterion asked for.

Evidence: `evidence/regression-testing/p6-t3-ac7-compile-red.md` (the type does not exist, so the fail-before is a compile-red observation), `evidence/regression-testing/p6-t8-ac7-pass-after.md` (6 of 6), `evidence/qa-gates/p7-t9-changed-line-coverage.md`.

### AC8 — full C# toolchain pass in order, no regression

**PASS.**

Format, format-verify, analyzers, type-check and instrumented test ran in that order with every step meeting its declared expectation (`evidence/qa-gates/p7-t6-loop-closure.md`, `LOOP: CLEAN PASS`). The format step required two passes because the first rewrote the new test file; the loop correctly restarted rather than proceeding, and the read-only `csharpier check` afterwards independently confirms nothing changed between the final format pass and the gates below it.

Command compliance was checked individually: `/t:Rebuild` throughout, no `/t:Build`, no `/p:Nullable=enable`, CSharpier through `dotnet tool run`, and the solution-versus-project platform spelling asymmetry correctly preserved. Analyzers and nullable both report 0 warnings and 0 errors against a 0/0 baseline. The instrumented run executed 7163 tests with 0 failures and `NEWLY-FAILING: NONE`.

No regression: repository line coverage moved from 84.6332 to 84.6341 percent and branch coverage from 79.3903 to 79.3941 percent — both up. Changed-line coverage is 97.12 percent. The separate matter of the 85 percent absolute floor is recorded as FAIL in `policy-audit.2026-09-08T20-15.md` section 4, is pre-existing on this branch's own baseline, and does not bear on AC8's no-regression clause.

## 3. Scope conformance

- Every one of the 16 production and test paths is a declared Write Set member. The delivery's own scope check (`evidence/qa-gates/p7-t11-scope-boundary.md`) reports `OUT-OF-WRITE-SET: NONE` over an 85-path union; this review reconciles that against the 89-path branch diff — the 4-path difference is the four artifacts written at or after that check ran (`p7-t11` itself, `p7-t20`, `p7-t23`, and the issue-update mirror), which is arithmetic rather than a discrepancy.
- The three files owned by the concurrent run on issue #809 (`QfcHomeControllerRunAsyncTests.cs`, `TaskMaster/ThisAddIn.cs`, `UtilitiesCS/Threading/UiThread.cs`) are absent from the diff.
- The AC2 fence file is absent from the diff.
- No file under `CLAUDE.md`, `.claude/rules/`, `.github/` or `quality-tiers.yml` was touched.
- The nine report-only items named in `spec.md` were all left unfixed, as required, and are transcribed into `evidence/issue-updates/issue-810.2026-09-08T10-32.md` so they survive merge.

## 4. Residual work carried out of this delivery

Not blocking; ownership passes to the caller.

1. The live-Outlook runbook step for risk R2 (open QuickFiler, open a breadcrumb popup, click Cancel, confirm Outlook keyboard input works immediately). No agent can execute it, and `spec.md` assigns it to the maintainer.
2. Follow-up issues for the nine report-only items recorded in the issue-update mirror, plus CR-1 from this review (the `QfcHomeController` ribbon-release callback lacks the read-and-clear idiom AC4 applied one level down) as a tenth.
3. CR-2 through CR-4 from the code review, each a small local improvement with no behavioural consequence today.

## 5. Acceptance Criteria Status

### Acceptance Criteria Status
- Source: `docs/features/active/2026-09-07-quickfiler-teardown-and-dropdown-residuals-810/spec.md` (authoritative for `full-bug`); mirrored in `docs/features/active/2026-09-07-quickfiler-teardown-and-dropdown-residuals-810/issue.md`
- Total AC items: 8
- Checked off (delivered): 8
- Remaining (unchecked): 0
- Items remaining: none

All eight boxes were already `- [x]` in both files when this review began, checked off by plan tasks `[P7-T12]` through `[P7-T19]`. This review evaluated every one as PASS, so no box required a change and none was changed. No criterion was added, reworded or removed by this review.

## 6. Verdict

**PASS — 8 of 8 acceptance criteria satisfied, 0 blocking findings.**
