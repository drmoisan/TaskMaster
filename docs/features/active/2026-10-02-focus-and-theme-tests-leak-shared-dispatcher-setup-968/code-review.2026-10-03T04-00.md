# Code Review: focus-and-theme-tests-leak-shared-dispatcher-setup (Issue #968, folding Issue #972)

- Timestamp: 2026-10-03T04-00
- Branch: bug/focus-and-theme-tests-leak-shared-dispatcher-setup-968, head 5570b337cfd11e57579228b36bc919f9673b6195
- Base: origin/main 993fdd01566dee82e5f37acb761a600feaaa1454 (ancestor of the head after the final merge)
- Companion artifacts: policy-audit.2026-10-03T04-00.md, feature-audit.2026-10-03T04-00.md, remediation-inputs.2026-10-03T04-00.md
- Method: no-Bash review (caller directive). All fourteen code paths were read in full from the item worktree; the ten pre-existing modified files were compared against the session checkout's unchanged copies (line counts 497, 342, 440, 470, 312, 244, 232, 371, 495 and 413 match the executor's BASE census, so those copies are the pre-change text); the executor's transcribed diffs and censuses were cross-read; Grep censuses were re-run with the Grep tool.

## Executive Summary

Code quality verdict: no blocking code-quality finding. 0 Blocking code-quality findings, 2 Non-blocking findings (CR-1, CR-2), 6 observations (O-1 to O-6). The single blocking item of this review, B-1 (AC22 pending this pull request's CI run, class awaiting_ci), is an evidence-source matter recorded in remediation-inputs.2026-10-03T04-00.md and the feature audit, not a code defect.

The fixture change is sound under every state the reviewer traced (fresh field, foreign transaction value, residual parked value with the ownership flag set, pins taken under a foreign value then restored to null); the regression test observes the defect on one thread; the R4 restructure keeps the test deterministic because the census proves every remaining pin nests inside a held transaction; the liveness rewrites replace scheduling-dependent steps with signals the production code already emits; the folded dead-code removal is supported by a two-strategy zero-caller proof and two clean rebuilds. Under the maintainer's related-defect directive every related finding was evaluated for in-item remediation; none of the items below is a defect that the ratified spec left open, so none is classified blocking autonomous.

## Scope

Fourteen code paths (eleven modified, three added) plus the feature folder and the two inherited promoted records:

- QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs (342 -> 375 lines)
- QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherPinCountTests.cs (new, 248)
- QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs (470 -> 472)
- QuickFiler.Test/Controllers/QfcItemController.FocusAndThemeTests.cs (497 -> 482)
- QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs (440 -> 442)
- QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs (312 -> 352)
- QuickFiler.Test/Controllers/QfcDatamodelTeardownTests.cs (244 -> 229)
- QuickFiler.Test/Controllers/QfcInitEmailQueueZeroBatchTests.cs (232 -> 226)
- QuickFiler.Test/Controllers/QfcDatamodelTests.cs (371 -> 394)
- QuickFiler.Test/TestSupport/SynchronousBackgroundWorker.cs (new, 27)
- QuickFiler.Test/TestSupport/ArmingFakeTimeProvider.cs (new, 49)
- QuickFiler.Test/QuickFiler.Test.csproj (+3 Compile items at lines 204, 230, 231)
- QuickFiler/Controllers/QfcDatamodel.cs (495 -> 367)
- QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs (413, comment-only)

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Non-blocking | QuickFiler/Controllers/QfcDatamodel.cs | lines 63, 70, 199, 268, 318, 350 | Pre-existing commented-out statements remain in the touched production file after the dead-member removal (two commented logger.Debug lines in LoadAsync, a commented argument extraction, a BUGFIX note, two commented item lookups). | Leave as delivered; the research (addendum F2) evaluated the remaining commented lines and recommended keeping the diff reviewable, and spec amendment 1.2 limits the production edit to the removals, the nameof retarget and the comment rewrites. Remove them in the next change that touches the file. | Not a defect the ratified scope left open: the scope statement for this file is explicit. The lines carry no stale reference to a removed member (Grep for Worker_RunWorkerCompleted, LoadRemainingEmailsToQueue word boundary and Linked List Locking over QuickFiler/: zero hits in QfcDatamodel.cs). | Read of QfcDatamodel.cs; research addendum section 6.1 F2; spec Scope and Non-Goals (production edits confined) |
| Non-blocking | QuickFiler.Test/Controllers/QfcDatamodelTests.cs | lines 125, 135, 148 | The rewritten sibling test registers the dequeue chain's awaits under whatever SynchronizationContext the MSTest worker thread carries at call time, unlike the Liveness rewrite, which registers them under a null context through SynchronizationContextScope. A non-pumping context left on the thread by an earlier test would stall `await pending` rather than fail it. | Accept as delivered. The spec (addendum section 6.2, ratified by amendment 1.2) states that no context handling is needed here because the flag is written directly by reflection and no loader continuation is involved; the only known context leaker in the assembly installs a plain SynchronizationContext whose Post reaches the thread pool, which does pump. If a future test leaves a non-pumping context behind, wrap the DequeueNextItemGroupAsync call in the same null-context scope. | The AC31 text (no Task.Yield, no loop, WhenAny over Armed, await the dequeue task) is met; the residual exposure is the same one every awaiting test in the assembly already has and is not introduced by this branch. The sensitivity check shows the test fails crisply on its re-arm assertion when the signal is dishonest. | QfcDatamodelTests.cs lines 103-152; QfcDatamodelLivenessTests.cs lines 123-134, 159-162, 189-199; evidence/regression-testing/liveness-sensitivity-check.md |

## Observations (not findings)

- O-1 Fixture soundness trace. EnsureDispatcher increments the count and seeds only on a null field, setting the ownership flag; EnsureScope.Dispose decrements and writes null only when count is zero, the flag is set and the field still references the parked instance, then clears the flag, all inside one lock (FieldLock) block (lines 149-157, 293-305). Traced states: (a) fresh null field, two pins, either release order: field kept until the last release, then null, flag cleared; (b) transaction holds a live dispatcher: pins install nothing, count reaches zero with the flag false, nothing written; (c) residual state (parked installed with zero pins and the flag set, reachable only if a transaction installs over a pinned parked value and restores it after the last release): the next single pin installs nothing, its release finds flag true and field parked and reverts, as the class doc states; (d) pins taken under a foreign value, transaction restores null while pins are live: count stays above zero, flag false, next pin seeds and sets the flag, last release reverts. No negative count is reachable because Dispose is idempotent per scope. The documented residual is unreached by any test (census: no Install between any pin's acquisition and release).
- O-2 R4 determinism after the pin removal. transactionA holds the gate from BeginTransactionAsync to its explicit Dispose; original is read and liveA installed under the gate; B blocks on the gate; A restores before releasing; B reads under the gate. Every remaining pin is acquired and released inside a held transaction (13 of 13 nested), so no gate-free writer exists in the assembly; the only residual writer is UiThread.Initialize (W5), which the doc names and which the former pin did not fence either. The try/finally re-dispose relies on the idempotency R5 proves.
- O-3 Liveness rewrite determinism. pending runs synchronously to the gate's first TimeProvider.Delay, so Armed is complete before the call returns (asserted); ReArm precedes the advance; WhenAny(Armed, pending) completes whether the gate's ConfigureAwait(false) continuation is inlined inside Advance or queued; loaderRelease is a plain TaskCompletionSource and the loader lambda's await and Worker_DoWork's await were registered under a null context, so SetResult runs the continuations inline and the finally clears the flag before ReadLivenessFlag; the second Advance fires the already-armed timer and the dequeue task is the completion signal. No step depends on pool scheduling.
- O-4 Canonical C# coverage artifact path (artifacts/csharp/coverage.xml) absent in the worktree; committed projections, summaries and the local raw Cobertura root were used under the standing ruling. Recurring across #948, #950 and #956; a repository convention, not a defect of this item.
- O-5 QuicFiler/Controllers/QfcDatamodel.QueueProcessing.cs line 52 comment ("the field is written on the worker thread") describes _remainingLoadTask, which Worker_DoWork writes on the thread that runs the handler: the BackgroundWorker thread in production, the test thread under the synchronous starter. The executor's D-20 decision to leave it unchanged is accepted; the comment's snapshot rationale (a cross-thread writer) holds in both environments and AC26 names only the _remainingLoadActive comment and the TryUnhookOrReplace range.
- O-6 QfcDatamodelLivenessTests test RemainingLoadActive_AcrossAsyncVoidFirstAwait_StaysTrueWhileLoaderProduces releases its loader with a RunContinuationsAsynchronously source and exits the using block, so the loader's continuation (and Worker_DoWork's bw.CancellationPending read) may run on a pool thread after the worker is disposed. BackgroundWorker.CancellationPending is a plain property read that Component.Dispose does not invalidate; this is the #950 design, documented in the research addendum (section 4.2), and no change is needed.

## Per-file notes

- UiThreadDispatcherFixture.cs: two new private statics with a comment stating the FieldLock invariant; the EnsureScope now carries the parked reference rather than an installed-or-null marker; CompareExchange is no longer used by the scope (SCOPE census CompareExchange 0). Docs for the class, EnsureDispatcher and EnsureScope describe counting, ownership, discard consequence and residual; the forbidden phrases are gone (Grep leaks exactly 0, installed nothing carries 0).
- UiThreadDispatcherPinCountTests.cs: four tests, every pin acquired and released inside a held transaction with the transaction disposed in finally; no assertion sits between a pin's acquisition and its release in tests 1 to 3, and in test 4 both assertion groups follow the releases, so an assertion failure cannot leak a pin. The class doc states why the regression lives at the fixture level and why Moq is not imported.
- UiThreadDispatcherFixtureTests.cs: only R4 changed (doc, pin removal, try/finally); the R1 to R3 and R5 to #882 bodies are character-identical to the pre-change copy on every line the reviewer compared (assertion and because text lines).
- FocusAndThemeTests.cs: seven call sites switched to the shared BuildExecutingViewer; the header comment names the switch; the two theme tests lose their ensure calls and gain accurate arrange comments naming the injected IUiDispatcher mock.
- TestSupport.cs: wrapper doc rewritten to the counted pin, naming the two fixture test classes and the dispose-inside-the-transaction rule; shared-helper doc updated; EnsureSynchronizationContext (lines 90-96) unchanged.
- QfcDatamodelLivenessTests.cs: shared worker, caller-owned workers through StartHeldOpenLoader(worker, ...), SynchronizationContextScope disposer with the no-await rule stated in its doc, test 1 rewritten; the header still documents the deliberate duplication of the two reflection helpers, which the spec keeps.
- QfcDatamodelTeardownTests.cs: nested helper removed; using directive added; otherwise unchanged.
- QfcInitEmailQueueZeroBatchTests.cs: nested helper removed; three using blocks; the "real BackgroundWorker" doc sentence corrected (F6).
- QfcDatamodelTests.cs: sibling test rewritten to the signal shape; two using blocks around BackgroundWorker instances.
- SynchronousBackgroundWorker.cs: internal sealed; RaiseDoWork and StartSynchronously; no Dispose(bool) override, with the reason in the doc.
- ArmingFakeTimeProvider.cs: CreateTimer override forwards to the base then completes the armed signal; signals created with RunContinuationsAsynchronously; the remarks state the consecutive-Advance prohibition that the tests obey.
- QfcDatamodel.cs: 128 lines removed (duplicate log field, Worker_RunWorkerCompleted, synchronous LoadRemainingEmailsToQueue, two-argument LoadRemainingEmailsToQueueAsync with its CS0618 pragma, the empty region, commented references); one line changed (nameof now names the live method). Both constructors still bind the one-argument loader; IQfcDatamodel members all remain.
- QfcDatamodel.QueueProcessing.cs: eight doc-comment lines changed; no statement touched (every changed line begins with ///).
- QuickFiler.Test.csproj: three Compile items in the positions the spec names (after the fixture-test item; after the DedicatedWorkerThread item).

## Unrelated defects (report for filing)

- None in the files examined. quality-tiers.yml is absent at the repository root (pre-existing, repository-wide, already promoted by the #956 review); no new issue is required from this item.

## Summary

No blocking code-quality finding. Two non-blocking findings, both evaluated against the ratified spec and accepted as delivered (CR-1 pre-existing commented-out statements the spec's confined production edit leaves in place; CR-2 the sibling liveness test's dependence on the ambient context being a pumping one, a pre-existing assembly-wide exposure that the spec explicitly accepted). Six observations record the soundness traces and standing conventions. The branch's single blocking item is B-1 (AC22 awaiting this pull request's CI run) and is recorded in remediation-inputs.2026-10-03T04-00.md.
