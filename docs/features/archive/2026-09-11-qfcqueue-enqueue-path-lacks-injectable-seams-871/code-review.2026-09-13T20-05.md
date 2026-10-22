# Code Review — Issue #871, QfcQueue enqueue-path injectable seams

- Date: 2026-09-13
- Reviewer: feature-review agent
- Diff anchor: `8213826f695439e86e3ed34faa575de493a11ec7`; head `8277b0c4c`; PR base `main`
- Scope: the nine code and project paths of the branch diff, reviewed by direct file inspection in the item worktree

## Executive Summary

This is a careful, well-argued change. The seam design is minimal and each seam retains its previous
construction expression as its production default, which is what keeps the no-behaviour-change claim
credible rather than merely asserted. The split that the 500-line ceiling forced was executed as a
relocation, not a rewrite: 299 of the 498 added lines match a removed line character for character
after whitespace stripping, and every one of the 24 genuinely-new executable lines is a seam
declaration, a seam accessor, a substituted call site or a one-line forward. Public surface is
unchanged; all six seams are `internal`, and `MoveMonitor` must be, because `IEmailMoveMonitor` is
itself internal.

**No finding in this review is Blocking.** One Medium finding concerns a documentation criterion rather
than code. The remaining findings are Low or Informational and concern the discriminating power and
failure mode of individual tests, not their correctness.

I specifically looked for the failure modes the caller asked about and record the outcomes here.

**Are the seams tautological?** Mostly no. The suite consistently asserts that a value supplied to a
seam flows *through production code* to an observable outcome, which is the property that survives a
production regression. `EnqueueAsync_WithSubstitutedTemplate_FlowsThatPanelToTheDequeuedEntry` proves
the factory's return value reaches the queued tuple unmodified; `AddAsync_WithSubstitutedViewerSeams_...`
leaves `ItemGroupFactory` at its production default so the real `AddAsync` body runs and the seams only
stand in at its edges; `EnqueueAsync_WithDefaultItemGroupFactory_UsesEachDispatcherShapeOnce` counts
real traversals of all three marshalling shapes. The one exception is finding CR-2: two of the nine
argument assertions in the controller pass-through test compare a harness-supplied null against null and
would survive a swap of those two argument positions.

**Do the seams widen the public surface?** No. All six are `internal`, the new interface is `internal`,
and the new adapter is `internal sealed`. Sixteen public and protected member declarations existed at
the anchor and all sixteen are present unchanged.

**Was behaviour preserved by the split?** Yes, on the evidence available. The strongest single item is
that the enqueue part's two catch blocks, its `logger.Error` call and message, and its `finally` block
compare byte-identical to the anchor after a two-line offset, and that the `CS0618` pragma pair appears
in no hunk of that file's diff. The two recorded byte-level deviations — a CSharpier re-wrap of one
statement and the loss of a UTF-8 byte-order mark — are both formatter output under the command CLAUDE.md
mandates, and CLAUDE.md states the formatter wins when a diff disagrees with it.

**Are the tests deterministic and isolated?** Yes. A direct pattern search over both new test files for
`Thread.Sleep`, `Task.Delay`, `DateTime.Now`, `DateTime.UtcNow`, `.Result`, `.Wait()`, `File.*`,
`Directory.*`, `Process.Start` and `HttpClient` returned no matches. The substituted dispatcher runs
every callback inline. The `[TestInitialize]` that clears the synchronization context is a legitimate
determinism fix and is examined in detail below.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Medium | `docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/spec.md` | AC22, spec line 714; Write Set, spec lines 519-537 | AC22's first clause states "The final diff contains no change to any file outside the Write Set", but the branch diff carries three tracked files under `.claude/agent-memory/orchestrator/` that the Write Set does not list. The plan admitted them through a pre-declared scope-lock clause, which cannot amend an acceptance criterion. | Amend the spec: either add `.claude/agent-memory/` to the Write Set or qualify AC22's first clause to exclude agent-memory records. Do not change code. | An acceptance criterion whose literal text is contradicted by the artifact it gates is not earned, and a plan clause is not an amendment vehicle for a spec criterion. | `evidence/qa-gates/p6-t6-scope-lock.2026-09-12T10-25.md` lines 28-30 list the three paths and adjudicate them under clause M; `spec.md` Write Set contains no agent-memory entry. |
| Low | `QuickFiler.Test/Controllers/QfcQueueEnqueueTests.cs` | lines 414-423, `EnqueueAsync_WithSubstitutedTemplate_FlowsThatPanelToTheDequeuedEntry` | The test calls `queue.Dequeue()` without first asserting the queue is non-empty. `Dequeue` calls `BlockingCollection.Take()`, which blocks indefinitely on an empty, non-completed collection. `EnqueueAsync` swallows both `OperationCanceledException` and `System.Exception`, so a regression that made the enqueue fail silently would leave the queue empty and this test would hang the whole assembly rather than fail. | Assert `queue.Count.Should().Be(1)` before dequeuing, as the sibling test at line 159 already does, or add `[Timeout]` to the method. | The test exists to catch a regression in the value flow; a regression that turns a catchable failure into an indefinite hang converts a diagnosable red into an unattributable stall. | `QfcQueue.cs:95` `_queue.Take()`; `QfcQueue.Enqueue.cs:116-125` the two swallowing catch clauses; contrast with `QfcQueueEnqueueTests.cs:159` which asserts the count first. |
| Low | `QuickFiler.Test/Controllers/QfcQueueEnqueueTests.cs` | lines 317-335, `EnqueueAsync_WithOneItem_PassesEveryControllerArgumentThrough` | Two of the nine argument assertions have no discriminating power. `call.HomeController.Should().BeNull()` and `call.Viewer.Should().BeNull()` both compare null against null: the home controller is a literal null the harness passes to the constructor, and `ItemViewer` is never set on the `QfcItemGroup` the substituted `RecordItemGroup` returns. A regression that swapped those two argument positions would pass this test. | Have `RecordItemGroup` set `grp.ItemViewer = _stubViewer` before returning, then assert `call.Viewer.Should().BeSameAs(_stubViewer)`. The home-controller slot cannot be strengthened without a constructible home controller and may be left as is with a comment. | AC16 claims "every argument captured and asserted"; the capture is complete but two of the nine assertions cannot fail under an argument-order defect, which is the defect class a nine-argument pass-through test exists to catch. | `QfcQueueEnqueueTests.Harness.cs:161` returns `new QfcItemGroup(mailItem)` with no viewer; `QfcQueueEnqueueTests.Harness.cs:102` passes `(QfcHomeController)null`. The viewer flow is covered elsewhere by `AddAsync_WithSubstitutedViewerSeams_...` at line 389. |
| Low | `QuickFiler.Test/Controllers/QfcQueueEnqueueTests.Harness.cs` | lines 81-93, `Initialize` and `Cleanup` | `[TestInitialize]` sets the thread's `SynchronizationContext` to null and `[TestCleanup]` does not restore it, so the class leaves the shared MSTest worker thread with a null ambient context. | Capture the previous context in `Initialize` and restore it in `Cleanup`. | The determinism fix itself is correct and necessary, but mutating ambient thread state without restoring it is the shape the general unit-test policy warns about under "tests must not rely on mutable global state". The practical risk is low because this class installs the context it clears, but the asymmetry is avoidable. | `QfcQueueEnqueueTests.Harness.cs:84` sets the context; `Cleanup` at lines 87-93 disposes three objects and does not touch the context. |
| Low | `QuickFiler.Test/Controllers/QfcQueueEnqueueTests.cs` | lines 136-145, `EnqueueAsync_WithEmptyItemList_ThrowsArgumentException` | `ThrowAsync<ArgumentException>` also accepts `ArgumentNullException`, which derives from it. The test would pass if the empty-list guard were changed to throw `ArgumentNullException`. | Use `ThrowExactlyAsync<ArgumentException>()` so the two guards are pinned to distinct exception types. | AC10 asks for `ArgumentNullException` for null and `ArgumentException` for empty; only the first of those two is currently pinned exactly. | `QfcQueue.Enqueue.cs:79-86` declares the two distinct guards; `QfcQueueEnqueueTests.cs:144` asserts the assignable form. |
| Low | `docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/spec.md` | AC19, spec line 685 | The pre-change base-part baseline is recorded as 0.503205 where the measurement is 0.496795. The two sum to exactly 1.000000, so the spec figure is the complement of the measured line rate, that is the miss rate, not a figure from a different run. | Correct the spec literal to 0.496795 and note the provenance, or record the complement relationship explicitly. | The plan's own P6-T1 artifact identified the divergence and defensively tested against both comparands, which is the right handling, but it attributed it to "a measurement of a different run or a different tracked-file state". The exact complementarity makes that explanation implausible and points at a transcription of the wrong column. | `evidence/qa-gates/p6-t1-coverage-file-rates.2026-09-12T10-25.md` lines 30-31 and 100-111; 155/312 = 0.496795 and 1 - 0.496795 = 0.503205. |
| Low | `docs/features/active/2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871/spec.md` | AC18, spec line 676 | AC18 cites the obsolete-API pragma pair at "lines 173 and 196". The other AC line citations in this document are anchor-relative, and on that convention the pair sits at 173 and 198; in the current tree it sits at 171 and 196. The citation mixes an anchor line number with a post-change one. | State the convention once at the head of the acceptance-criteria section and make all citations anchor-relative. | A reader checking AC18 against the current tree finds one of the two numbers right and the other off by two, which costs time and casts doubt on the other citations, all of which are in fact correct as anchor-relative references. | `QfcQueue.Enqueue.cs:171` and `:196` in the current tree; `evidence/qa-gates/p6-t5-diff-review.2026-09-12T10-25.md` lines 151-153 record the current pair. |
| Informational | `QuickFiler/Controllers/QfcQueue.UiIdle.cs` | lines 36-45, XML documentation of seam S2 | The documentation states that the default adapter "is built on first read, which no headless test triggers". Two headless tests do trigger the first read: the seam-contract test at `QfcQueueEnqueueTests.cs:43` and the construction test at line 104, which asserts the default is a `UiThreadIdleDispatcher`. | Reword to "which construction does not trigger". | The substantive claim — that constructing a queue performs no read of the process-wide dispatcher — is correct and is what AC7 gates. Only the incidental clause about tests is wrong; building the adapter reads nothing. | `QfcQueue.UiIdle.cs:49` builds the adapter but does not touch `UiThread.Dispatcher`; the three bodies at lines 77-106 do. |
| Informational | `QuickFiler.Test/Controllers/QfcQueueEnqueueTests.Harness.cs` | lines 47-48 | The viewer stand-in is produced by `FormatterServices.GetUninitializedObject(typeof(ItemViewer))`, which constructs a `Control`-derived object without running any constructor. | No change required now. If the row-placer seam is ever retyped over `Control` or an interface, replace the stand-in with a mock. | The technique is sound here and is the only way to obtain a distinguishable `ItemViewer` reference headlessly, because seam S4's signature requires the concrete type for the `Parent`, `Dock` and `AutoSizeMode` assignments in `AddViewerToTlp`. The object is only ever stored and compared by reference, never dereferenced, and the `Component` finalizer path is null-safe for an uninitialized instance. | `QfcQueue.Tlp.cs:153-164` requires the concrete type; `QfcQueueEnqueueTests.Harness.cs:198-207` only stores and returns the reference. |
| Informational | `QuickFiler/Controllers/QfcQueue.Tlp.cs` | line 64 | The S3 default is a field initializer holding a delegate to a static method on `ItemViewerQueue`, a type whose own static state includes `_core = CreateProductionCore()` and four scheduler properties, two of which read `UiThread.Dispatcher` when invoked. I checked whether eagerly creating the delegate could pull the type's initialization forward relative to the pre-change behaviour. | No change required. | `ItemViewerQueue` declares no static constructor, so it is `beforefieldinit`, and creating a delegate to one of its static methods does not access a static field and does not force initialization. Even if it did, the schedulers read the dispatcher only when invoked. The headless construction test, which constructs a queue and asserts no throw while `UiThread.Dispatcher` would throw, is direct evidence that no dispatcher read occurs. | `QuickFiler/Helper Classes/ItemViewerQueue.cs:11-29`; `UtilitiesCS/Threading/UiThread.cs:251-266` throws until `Init` runs; `QfcQueueEnqueueTests.cs:97-105`. |
| Informational | `QuickFiler.Test/Controllers/QfcQueueEnqueueTests.cs` | whole file | The file measures 425 physical lines against the 500-line ceiling, the tightest of the seven files in the Write Set at 85 percent of the limit. | Add further cases to the harness part or a third part rather than to this file. | The suite is already split across two parts of one partial class for exactly this reason, and the next contributor should be told which part to grow. | `evidence/qa-gates/p5-t6-line-counts-final.2026-09-12T10-25.md`. |

## Design Notes

**Seam initialization styles are justified, not inconsistent.** Three seams use a lazy `??=` getter
(`UiIdleDispatcher`, `ViewerRowPlacer`, `ItemGroupFactory`) and three initialize at the declaration
(`MoveMonitor` through its retained field, `ItemViewerFactory`, `BackgroundTlpFactory`). The split is
forced: `ViewerRowPlacer` and `ItemGroupFactory` default to instance methods, which a field initializer
cannot reference, and `UiIdleDispatcher` is lazy specifically so that constructing a queue performs no
read of the process-wide dispatcher. The two eager ones default to a static method group and to a
closure over no instance state. Each XML comment states its own reason, which is the right level of
documentation for a decision a later reader would otherwise "normalize".

**The explicit type argument on both generic forwards is load-bearing.** `UiIdleCallAsync<T>(Func<T>)`
and `UiIdleAsyncCallAsync<T>(Func<Task<T>>)` both forward to `InvokeIdleAsync<T>(...)` with `T` written
out. An inferred call from the second forward would be ambiguous, because a `Func<Task<X>>` argument is
applicable to `InvokeIdleAsync<T>(Func<T>)` with `T = Task<X>` and to `InvokeIdleAsync<T>(Func<Task<T>>)`
with `T = X`. Writing the type argument removes the second candidate in each case. A later contributor
"simplifying" these two lines would reintroduce CS0121.

**The S4/S5 pair is the right call and AC17 is the reason.** A single coarse seam at `ItemGroupFactory`
would have made `LoadControllersViewersAsync` coverable while leaving the whole of `AddAsync`
permanently unreached — relocating the untestable region instead of closing it. Keeping S3 and S4 as
separate seams, and requiring one test to exercise the production `AddAsync` with `ItemGroupFactory`
left at its default, is what turns the seam work into an actual coverage gain. The measurement confirms
it: `AddAsync`'s body lines report hits while `AddViewerToTlp`'s body, the genuinely untestable part,
does not.

**The retained commented-out `ContextIdle` line is correctly left alone.** `QfcQueue.UiIdle.cs:105`
carries a commented-out alternative implementation that travelled with the relocated body. Deleting it
would break the verbatim-move property AC18 gates, and it carries no coverage line element, so it
appears in neither the numerator nor the denominator. There are exactly four `ContextIdle` occurrences
in that file: three executable priority arguments at lines 81, 89 and 102 and this comment at 105. I
verified the count directly.

**No coverage exclusion was introduced.** No `[ExcludeFromCodeCoverage]` attribute and no assembly-level
or file-level exclusion appears anywhere in the change. Under the Coverage Exclusion Policy in
`.claude/rules/general-unit-test.md`, an exclusion matching a production source path would have been a
Blocking finding. Every residual region stays in the denominator, including the one genuinely-new
uncovered line, which is why the new-code rate reads 95.83 percent rather than 100 percent. Reporting
that honestly instead of excluding the line is the right choice.

**The out-of-scope counter leak is genuinely undisturbed.** I verified both halves of AC21
independently of the evidence artifacts. The increment sits at `QfcQueue.Enqueue.cs:94`, the `try` at
101, the `finally` at 126 and the decrement at 128, so the leak window between 94 and 101 is intact. On
the second half, the only exception any test injects is `_itemGroupFailure`, consumed solely by
`RecordItemGroup`, which the loader calls from inside the `try`; no test makes `BackgroundTlpFactory` or
the move-monitor hook throw. The harness documents this constraint in the comment on
`AssertLoaderFailureIsContainedAsync`. No test therefore codifies the leak as correct behaviour.

## Verdict

**PASS with non-blocking findings.** Nothing found in this review blocks merge. The Medium finding is a
documentation amendment to `spec.md`; the five Low findings are test-strengthening and documentation
corrections that can be taken as follow-ups or folded into a subsequent touch of these files.
