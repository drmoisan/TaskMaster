# Code Review — Issue #782 (pr-778-post-merge-review-residuals)

- **Date:** 2026-09-05
- **Reviewer:** feature-review agent
- **Base:** `main` -> `origin/main` @ `77c6d31404e2bc2291aec7eb9561e393c20cdcae`
- **Head:** `refactor/pr-778-post-merge-review-residuals-782` @ `4ed2f790e96d8c22abd36514db3848b71e073912`
- **Scope:** full branch diff — 87 files, 16 of them C# or csproj (+742 / -402)

## Executive Summary

This is a well-executed consolidation refactor. The production changes are small, behaviour-preserving
where they should be, and correct where they change behaviour. The test changes materially improve
the assembly's hygiene: six independently written reflection sites collapse to two, a leaked
never-shut dispatcher on a pooled worker is removed, a 514-line file is split without losing a single
fully-qualified test name, and three genuine regression tests are added with a fail-before record
that explicitly defends itself against being vacuous.

**Zero blocking findings.** Nine findings are recorded: two Should-fix and seven Nit or
informational. None prevents the pull request.

The single most consequential thing this review found is not a code defect but an **overstatement of
proof** that appears in two places. `spec.md` AC10 and the delivery's own code-review artifact both
state that the removal of the `WpfDispatcherYield` message tail is "pinned by the C20 `WithMessage`
assertion." It is not. `WithMessage("*UiThread.Init()*")` is a wildcard match; a future edit that
re-added the tail, or that rewrote the constant in any way that preserved the substring
`UiThread.Init()`, would leave every test green. No test in the repository asserts the constant's
value. That is finding **CR-1**.

The second Should-fix, **CR-2**, is an evidence-integrity defect: the re-recorded baseline coverage
figures cannot be reproduced from the baseline document the recording artifact itself names, and the
file's timestamps contradict the claimed measurement time. It changes no verdict, because every
candidate baseline value sits at or below the head value, but it means a reader following the
artifact's own instructions cannot arrive at its own numbers.

### Verified correct

Each of the following was checked against the tree rather than accepted from the delivery artifacts:

- **C02, the single-read getter.** `Dispatcher? captured = _dispatcher;` then test then return of the
  same local. The stated invariant — the getter never returns null and never observes a value other
  than the one it tested — holds.
- **C23, the captured-dispatcher lambdas.** This one needed checking, because the edit reads
  `UiDispatcher = UiDispatcher` and could have been a no-op. `ProgressTracker.cs:83-88` declares
  `internal Dispatcher UiDispatcher` over the private `_uiDispatcher` field, assigned from
  `UiThread.Dispatcher` at line 33. The lambda therefore now closes over the captured instance state
  and no longer re-reads the process-global static. The fix is real.
- **C01, the dead null comparisons.** Removing `dispatcher != null` from
  `RibbonViewer.EngineCommands.cs` is behaviour-preserving, because the accessor now throws rather
  than returning null, so the comparison could never be false where it was reached.
- **C12/C13, the reflection consolidation.** A search for the token `"_dispatcher"` across every
  `*.cs` in the repository returns exactly two hits, both intended and both guarding themselves with
  a static-initializer non-null assertion.
- **C16/C15, the split.** 24 test methods before, 25 after, zero lost, one added. Both parts declare
  the same `partial class`, so every fully-qualified name survives — which matters, because several
  are quoted verbatim inside committed `TestCaseFilter` expressions.
- **AC7 fail-before.** `evidence/regression-testing/p4-t7-fail-before.md` removes both guards, not
  one, and says why: "Removing only the `UiThread` throw leaves the sibling guard in
  `WpfDispatcherYield`, which throws the same exception type with the same shared constant, so the
  C21 test would still pass and the demonstration would be vacuous." All three tests fail with
  `NullReferenceException` and two carry production stack frames at the exact exposed lines. This is
  a genuine RED-first record.
- **The C03 withdrawal.** Honest, measured, bisected to a single line, mechanistically explained, and
  promoted as issue #788 rather than quietly dropped. The artifact explicitly declines to claim
  coverage for a branch that does not exist. This is the right way to withdraw a planned item.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Should-fix | `UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs`, `UtilitiesCS.Test/Threading/UiThread_Tests.cs` | `:136`, `:142` | The shared message constant's text is not pinned by any test. Both assertions are `WithMessage("*UiThread.Init()*")`, a wildcard that matches any message containing that substring. `spec.md` AC10 and `evidence/other/code-review.2026-09-05T23-00.md` entry (b) both claim the removed tail "before yielding folder tree work" is pinned by this assertion; re-adding the tail would keep the substring and every test would still pass. | Assert the constant directly: `.WithMessage(UiThread.DispatcherNotInitializedMessage)`. The constant is `internal` and `UtilitiesCS/Properties/AssemblyInfo.cs` grants `InternalsVisibleTo("UtilitiesCS.Test")`, so it is reachable; the literal contains no `*` or `?`, so it behaves as an exact match. Then correct the two "pinned by" claims. | An acceptance criterion and a delivery artifact both assert a protection the tree does not provide. A future edit that reverted SD5 would pass review on the strength of a claim that is not true. | A search for `DispatcherNotInitializedMessage` across all `*.cs` returns three hits, all in `UtilitiesCS` production code: the declaration at `UiThread.cs:135` and the two throw sites. Zero test references. |
| Should-fix | `docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/evidence/baseline/p0-t7-coverage.md` | Whole artifact | **EV-1.** The SD23 re-recorded baseline figures (112,355 lines covered; 26,500 branches covered) are not reproducible from `coverage/782-p0-baseline.cobertura.xml`, the output path the artifact's own command names. Re-aggregating that document with the artifact's own pinned all-descendant `.//line` selection yields **112,359 and 26,496** — exactly the values the artifact labels "superseded" and declares invalid as a baseline side. The file's `CreationTime` and `LastWriteTime` are both `2026-09-05 19:26:55`, whereas the artifact carries `Timestamp: 2026-09-05T21-59` and the re-anchor commit `11056a63` landed at `21:52:12`. A `dotnet-coverage collect` run at 21:59 writing to that path would have updated the mtime. | Either re-run the baseline collection so the document on disk matches the recorded figures, or amend the artifact to state that the re-measurement's output document was not retained and that the retained document yields 112,359 / 26,496. Remove the instruction that treats the only reproducible figures as invalid. | The artifact simultaneously asserts figures no reviewer can reproduce and forbids the only figures that are reproducible. A reader who follows its method against its named input is told their correct result is invalid. | Reviewer re-aggregation of both Cobertura documents; `Get-Item` on the baseline document; `git log --date=format:%H:%M:%S`. Timestamps: baseline document 19:26:55, re-anchored base commit `736c2cf2` 19:17:24, first production edit `351a242c` 20:37:55 — so the document is a legitimate measurement at the re-anchored base tree, taken before any edit, but it is the 19:26 run and not a 21:59 one. |
| Nit | `UtilitiesCS.Test/Threading/ProgressTracker_ReportAndViewerTests.cs`, `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs` | `:213-227`, `:139-153` | The two C26 tests assert only the exception type. `Throw<InvalidOperationException>()` and `ThrowAsync<InvalidOperationException>()` carry no message assertion, so either test would pass on an unrelated `InvalidOperationException` raised anywhere inside `Initialize()` or `InitializeAsync()`. | Add `.WithMessage(UiThread.DispatcherNotInitializedMessage)` to both, which also discharges part of CR-1. | The tests are meant to pin one specific guard. Without a message assertion they pin "something in this method throws `InvalidOperationException`", which is weaker than the finding they close. | Read of both test bodies. |
| Nit | `UtilitiesCS.Test/Threading/IdleActionQueue_Tests.cs` | `:38-57` | The new `[TestCleanup]` introduces a fresh reflection dependency on the private production method name `IdleActionQueue.OnApplicationIdle`, with no rename guard. If that method is renamed, `GetMethod` returns null and `Delegate.CreateDelegate(type, (MethodInfo)null)` throws `ArgumentNullException`, whose message does not name the cause. | Add `.Should().NotBeNull(because: "IdleActionQueue.OnApplicationIdle must exist")` on the `MethodInfo` before constructing the delegate, matching the `ResolveDispatcherField` idiom this delivery standardised. | The delivery's own theme is that a reflective lookup must fail loudly and informatively on a rename. The new site fails loudly but not informatively, so the idiom is applied inconsistently within the same change. | Read of the added cleanup. |
| Nit | `UtilitiesCS.Test/Threading/IdleActionQueue_Tests.cs` | `:55` | `ApplicationIdleTimer.Unsubscribe(handler)` runs unconditionally after every test, including tests that never subscribed. `Unsubscribe` calls `Stop()` when the invocation list empties, touching process-global `Application.Idle` state. | Consider making the unsubscribe conditional on the queue having subscribed, or document that an unsubscribe of an unregistered handler is a no-op in this implementation. | Low risk given the `[DoNotParallelize]` that the same edit correctly adds, but the cleanup asserts nothing about the state it is restoring. | Read of the added cleanup and the `spec.md` SD7 rationale. |
| Nit | `UtilitiesCS.Test/Threading/UiThread_Tests.cs`, `UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs` | `:186-206`, `:175-180` | `_ready.WaitOne()`, `_thread.Join()`, and the C21 `worker.Join()` all run without a timeout. A thread that failed to start or to complete would hang until the 5-minute `/Blame TestTimeout` fires rather than failing fast. | Optional: supply a bounded timeout and assert on it. | Consistent with the existing `StaDispatcherHost` precedent in the same assembly, so this is pre-existing style rather than a new hazard introduced here. Recorded for completeness. | Read of both host implementations. |
| Nit | `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs` | `:55-63` | The order-independence teardown `current.Should().BeSameAs(_capturedDispatcher)` still compares null to null when the process-global dispatcher is unset for the whole test class. | None required. | C18's stated condition — that the guard fails rather than passes if the fixture cannot resolve the field — **is** met, because `UiThreadDispatcherFixture.ResolveDispatcherField` asserts non-null inside a static initializer. The residual null-to-null case is the correct outcome for "nothing mutated", not a defect. Recorded so a future reader does not mistake it for an unfixed C18. | Read of `QfcItemController.UiThreadDispatcherFixture.cs:133-141`. |
| Nit | `docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/user-story.md` | AC-U2 | The criterion still names "the retry-after-failed-initialization behavior of `UiThread.Init()`" as one of two permitted production behaviour changes. C03 was withdrawn, so `Init()` is byte-identical to its `pre-782-base` form and that change does not exist. | Amend the AC-U2 text, or leave it and rely on the disclosure already present in the delivery's code-review artifact. | The delivery's own artifact argues AC-U2 "bounds the permitted production behaviour changes from above rather than requiring both of the two it names", which is a sound reading and the reason this is a Nit rather than an AC failure. The text is nonetheless stale and sends a future reader looking for a change that is not there. | `spec.md` Behavioral Contract `UiThread.Init()` subsection; `evidence/other/code-review.2026-09-05T23-00.md` entry (a). |
| Informational | `artifacts/pr_context.summary.txt` | `Changed files overview`, `Close candidates` | The summary reports `Core logic changes: 0 files` while the branch changes 16 C# files, and lists seven author-asserted auto-close issues (#394, #449, #476, #493, #508, #584, #778) that are prose scrapes from this delivery's own artifacts. | The PR author step must derive the changed-file set from git and must carry only **#782** as the closing issue. | Left uncorrected, the PR body would close six unrelated issues. #394 for example appears in `spec.md` Constraint 6 purely as a cited past defect. This is a recurring generator defect rather than a defect of this branch. | Reviewer simulation of the coverage hook's `Get-ChangedLanguageSet` against the summary returned an **empty** language set, confirming the overview lists only Markdown paths in the parsed format. |

## Design and Architecture Notes

**The shared constant is the right shape.** One `internal const string` adjacent to its primary
thrower, rather than a new holder type, is the simpler design and the spec records the rejected
alternative with a reason. `internal` is the correct accessibility: both consumers are in
`UtilitiesCS`, and `InternalsVisibleTo` makes it reachable from the test assembly, which is precisely
what CR-1's recommendation depends on.

**The deliberate non-lazy contract is now documented rather than implicit.** The `<remarks>` block
explains why `Dispatcher` does not self-heal when the sibling `UiSyncContext` and `AutoScaleFactor`
accessors do. That asymmetry was previously undiscoverable and is exactly the kind of thing that
causes a later contributor to "fix" it. The C03 withdrawal record then documents empirically what
happens when the related latch behaviour is changed — the two lazy accessors turn a re-armed latch
into repeated WinForms construction that starves the thread pool. Taken together these two additions
leave the type meaningfully safer to modify than they found it.

**`UiThreadDispatcherScope` is well designed for its constraints.** It documents that it is
deliberately not internally synchronized and that serialization is supplied by `[DoNotParallelize]`
on every installing class — and it states the obligation that imposes on a future caller. `Dispose`
restores a null prior unconditionally, which is the case a hand-rolled `finally` most often gets
wrong. Disposal is idempotent. The `Current` accessor exists specifically so a test can observe the
uninitialized state without tripping the guard, which is what makes the AC5 round-trip assertion
expressible.

**One residual asymmetry.** `QuickFiler.Test` cannot use the scope, because it is not named in the
`InternalsVisibleTo` grants on `UtilitiesCS`, so the repository ends with two reflection acquisitions
rather than one. The spec states this and the reason. Two guarded acquisitions is a large improvement
over six unguarded ones, and closing the gap would mean adding a grant to production `AssemblyInfo.cs`
for a test assembly — a worse trade. The current outcome is the right call.

## Policy Compliance Notes

- **Bugfix workflow.** Correctly scoped. This is a Refactor; the failing-test-first requirement was
  applied to the two latent defects (C10, C02) and discharged through a fail-before dossier where a
  deterministic in-suite failing test is structurally impossible, which is the route the evidence
  conventions prescribe.
- **500-line limit.** The only pre-existing violation is removed. Every touched file now measures
  under 400 lines.
- **Temporary files in tests.** None created. The C10 STA sentinel, the C21 fresh thread, and the C14
  cleanup all avoid the filesystem entirely.
- **Determinism.** Zero banned timing APIs introduced. The C21 test's use of a dedicated fresh thread
  is a determinism improvement, not a timing hack: it removes a dependency on which pooled worker the
  test lands on.
- **`.claude/` untouched.** Zero paths in the diff, and `evidence/qa-gates/p6-t3-dotclaude-untouched.md`
  records that residue written by earlier agents was cleared. The worktree is clean at HEAD.

## Recommendation

**Approve for pull request.** CR-1 and CR-2 are worth fixing but neither blocks: CR-1 is a test
strengthening plus two sentence corrections, and CR-2 is an evidence artifact amendment whose
resolution cannot change any verdict. Both are carried into
`remediation-inputs.2026-09-05T23-48.md` as recommended, non-blocking follow-ups.
