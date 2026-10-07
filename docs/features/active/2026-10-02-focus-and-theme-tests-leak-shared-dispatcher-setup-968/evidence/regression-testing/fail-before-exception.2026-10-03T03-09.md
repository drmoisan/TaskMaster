# Fail-before exception dossier: the two dequeue-liveness tests (issue #968, task P5-T1)

Timestamp: 2026-10-03T03-09
Command: pwsh -NoProfile -Command '<CMD-SPAN-TOKEN-COUNT payload>' with FILE `QuickFiler.Test\Controllers\QfcDatamodelLivenessTests.cs`, START `public async Task DequeueNextItemGroupAsync_WhileLoaderStillProducing_KeepsPollingAfterWorkerIdle()`, END `/// <summary>Reads the issue #424 producer-liveness flag by reflection.</summary>` and the P0-T13 T1-LIVE token list; then pwsh -NoProfile -Command '<CMD-SPAN-TOKEN-COUNT payload>' with FILE `QuickFiler.Test\Controllers\QfcDatamodelTests.cs`, START `public async Task DequeueNextItemGroupAsync_HighConfidenceMode_WaitsWhileSourceWorkerActive()`, END `public async Task TryQueueRemainingMailItemAsync_HighConfidenceEnabled_AddsBelowThresholdCandidate()` and the P0-T13 T-SIB token list (both the Command Reference macro executed verbatim with PREFIX expanded and WORKTREE substituted, before any Phase 5 edit)
Canonical command: CMD-SPAN-TOKEN-COUNT on T1-LIVE and on T-SIB
EXIT_CODE: 0
Output Summary:
- WORKTREE-LEAF: agent-a291a7fbabf9d0229 (both payloads)
- T1-LIVE: SPAN: 96-151; `await` 5, `using (NoSynchronizationContext())` 0, `Task.Yield` 3, `fake.Advance` 3, `for (int i` 1, `clock.ReArm();` 0, `(await pending)` 1
- T-SIB: SPAN: 97-134; `await Task.Yield();` 1, `clock.ReArm();` 0, `await Task.WhenAny(clock.Armed, pending)` 0, `using (var worker = new BackgroundWorker())` 0, `IList<MailItem> result = await pending;` 1
- The old shapes are still on disk: T1-LIVE `Task.Yield` 3, `fake.Advance` 3, `for (int i` 1, and T-SIB `await Task.Yield();` 1 (the constructs whose removal AC31 requires).

WhyFailingRunImpossible: The old tests fail only when the thread pool delays a queued continuation past the bounded retry loop or past the second clock advance, which no test input can force, and the production behaviour under test is correct both before and after the change (addendum section 5.5). A deterministic failing run of the old shape therefore cannot be produced on demand.

## Alternative proof

(i) Mechanism reading. Addendum section 5.3, read with the web-verified timer facts of section 5.2: `FakeTimeProvider.Advance` invokes due timer callbacks synchronously, `TimeProvider.Delay` reaches the overridable `CreateTimer`, and the gate's `ConfigureAwait(false)` continuation runs inline on the advancing thread only when that thread carries no derived synchronization context. On the queued path the second `Advance` can run before the gate re-arms its timer, so the advance is lost; `await Task.Yield()` gives no ordering guarantee relative to a queued pool work item; and the `for (int i = 0; i < 20 && !pending.IsCompleted; i++)` loop is a bounded retry whose success depends on pool scheduling. The sibling test in `QfcDatamodelTests` has the same first two steps without the retry loop, so on the queued path its `await pending` waits indefinitely rather than failing.

(ii) Labelled sensitivity check. Task P5-T8 temporarily edits the `sourceActive` lambda in `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs` to `() => false,`, builds, runs both rewritten tests by fully qualified name and requires each to fail on its re-arm assertion, then reverts the edit and proves the revert. That check is evidence of the new tests' sensitivity to a dishonest liveness signal, not a fail-before of the old tests, and is recorded separately in FEATURE/evidence/regression-testing/liveness-sensitivity-check.md.

## Negative evidence (search for a failing-run artifact of these two tests)

- SearchScope: docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968/evidence/regression-testing/ (the feature is not versioned, so the feature-root folder is the only scope)
- SearchPatterns: liveness-*.md, fail-before-*.md
- SearchResult: liveness-*.md: none. fail-before-*.md: fail-before-build.md and fail-before-pin-count.md, both of which belong to the pin-count regression test (tasks P1-T4 and P1-T5) and record no run of `DequeueNextItemGroupAsync_WhileLoaderStillProducing_KeepsPollingAfterWorkerIdle` or `DequeueNextItemGroupAsync_HighConfidenceMode_WaitsWhileSourceWorkerActive`. No failing-run artifact exists for these two tests.

After this task exactly one file matching `fail-before-exception.*.md` exists in FEATURE/evidence/regression-testing/ (this file).
