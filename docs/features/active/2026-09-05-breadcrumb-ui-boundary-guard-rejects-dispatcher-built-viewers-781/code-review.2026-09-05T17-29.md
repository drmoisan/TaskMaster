# Code Review — breadcrumb-ui-boundary-guard-rejects-dispatcher-built-viewers (#781)

- Review timestamp: 2026-09-05T17-29
- Branch: `bug/breadcrumb-ui-boundary-guard-rejects-dispatcher-built-viewers-781` @ `4f74aa39799dca7233bcf286dac90e8691eabd99`
- Base: `main` @ `a007f72e394ee3038c6c52bfdf91f007df96fd6c`
- Scope: full branch diff, 44 paths (not a plan subset)

## Executive Summary

The change is small, correct, and well targeted. `ItemViewer.ThrowIfOffUiBoundary` stops proving UI
ownership by `SynchronizationContext` reference equality and starts proving it with
`Dispatcher.CheckAccess()` on the dispatcher captured in the constructor. That is the right
mechanism for the reported defect: every production `ItemViewer` is built inside a WPF dispatcher
operation, which installs a `DispatcherSynchronizationContext` for the duration of the callback, so
the captured context is never the UI thread's ambient context again and the old comparison rejected
every legitimate call.

The implementation is in fact stronger than its own stated rationale. `Dispatcher.CheckAccess()`
compares `Thread` object references, not managed thread ids, so it is immune to id recycling
entirely — a point the rewritten documentation argues around rather than states (CR-4).

Test quality is high. The new class reproduces the exact production construction shape, asserts
non-vacuity explicitly before acting, restores every ambient-context substitution in a `finally`,
and uses no sleeps, timers, or temporary files. The RED-first claim was independently reproduced by
this review: restoring the pre-fix production file yields 5 failures out of 7, matching the
executor's artifact test by test.

**Blocking findings: 0.** Seven findings are recorded, one Medium and the rest Low or
Informational. None of them justifies withholding the PR. Two are defects in the executor's
evidence prose rather than in shipped code (CR-1, CR-2), one is a residual promotion obligation
(CR-8).

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Medium | `docs/features/.../evidence/qa-gates/coverage-delta.2026-09-05T10-49.md` | "The two-line difference in `lines-covered`" paragraph | The artifact attributes the -2 `lines-covered` movement to the two deleted D4 tests in `QuickFiler.Test`. A class-by-class diff of the two Cobertura documents shows the `QuickFiler` package is unchanged (LINE missed=2376 covered=9960 in both); all three differing classes are in `UtilitiesCS` and none is touched by this branch. The explanation is also self-contradictory, since the same artifact establishes that the old throw path is outside the denominator and therefore cannot move a counter. | Replace the paragraph with the measured cause: run-to-run nondeterminism in `UtilitiesCS.HelperClasses.SegmentStopWatch` (1.0 -> 0.944954), `UtilitiesCS.SubjectMapSco` (0.969466 -> 0.938931), and `UtilitiesCS.OlTableExtensions` (0.885522 -> 0.912458, improved). | The artifact's conclusion (no regression attributable to this change) is correct and is strengthened by the corrected measurement, but a wrong causal claim in a coverage evidence artifact will mislead the next reviewer who inherits it, and it hides that the movement is in unrelated, possibly flaky code. | Reviewer diff of `coverage/baseline-781.cobertura.xml` against `artifacts/csharp/coverage.xml`: 564 classes each, exactly 3 differ, all `UtilitiesCS`, 0 `QuickFiler` classes differ. |
| Low | `docs/features/.../evidence/qa-gates/coverage-delta.2026-09-05T10-49.md` | same paragraph | States that `*.Test.dll` is "excluded from instrumentation by `coverage.config`". The committed `coverage.config` contains seven third-party `ModulePath` excludes and no test-assembly entry. | Cite `scripts/vscode/Invoke-MSTestWithCoverage.ps1` line 99, which injects `.*\.Test\.dll$` into the settings at run time, and `Invoke-MSTestWithCoverage.Helpers.ps1` lines 24-46, which omit `.Test`-suffixed assemblies from the allowlist. | The substantive effect is real and confirmed (no `.Test` package appears in either Cobertura document), but the citation names a file that does not implement the behaviour, so a future reader verifying the claim against `coverage.config` will conclude the evidence is wrong. | `cat coverage.config`; `grep -n 'testAssemblyPattern' scripts/vscode/Invoke-MSTestWithCoverage.ps1`. |
| Low | `QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs` | lines 379-391, `DrainableSynchronizationContext.Drain()` | `Drain()` is declared `internal` and is never called anywhere in the file. It is the only member of the type besides `Post`, and its body contains a thread-identity assertion that consequently never executes. Roughly 11 lines of dead test-support code. | Delete `Drain()`, `_callbacks` dequeue logic, and `_creatorThreadId`, keeping only the `Post` override that the inert-operations helper actually needs; or call it where a drain is intended. | The General Code Change Policy prefers the simplest design that works. A drainable queue that is never drained implies to a reader that posted work is executed at some point in these tests, which it is not; the queue only exists to swallow posts. | `grep -n "Drain()" QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs` returns only the declaration at line 381. |
| Low | `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` | lines 413-424, `<remarks>` on `ThrowIfOffUiBoundary` | The documentation justifies the new guard with "Managed thread ids are unique among live threads, so while the UI thread is alive an identity check cannot be satisfied by a recycled pool thread." `Dispatcher.CheckAccess()` does not compare ids; it compares `Thread.CurrentThread` against `Dispatcher.Thread` by object reference, so the guard is immune to id recycling unconditionally, not only while the owner thread is alive. | Restate as: ownership is proved by `Thread` object reference identity through `Dispatcher.CheckAccess()`; managed thread ids are not used, so id recycling is not a consideration. | The current wording understates the guard and, more importantly, invites a future refactor to an `Environment.CurrentManagedThreadId` comparison on the belief that the two are equivalent. They are not: an id comparison is weaker and reintroduces exactly the concern `BreadcrumbUiDispatcher.IsCurrentBoundary` documents (CR-7). | Diff hunk at `ItemViewer.Breadcrumb.cs:404-444`; WPF `Dispatcher.CheckAccess` semantics. |
| Low | `QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs` | lines 210-215, 251-256, 289-294 | The three cross-thread tests use `Task.Run(...).GetAwaiter().GetResult()` to obtain a non-owning thread. In .NET Framework, `Task.InternalWait` with an infinite timeout and no cancellation token attempts `WrappedTryRunInline()` first, and `ThreadPoolTaskScheduler` will pop and execute the task inline when the caller is itself a thread-pool thread. If MSTest ever executes a test body on a pool thread, the work would run on the owning thread, `CheckAccess()` would return true, and the two boundary-diagnostic tests would fail while `_NullOwningDispatcher_` would pass for the wrong reason. | Replace with an explicit `var t = new Thread(() => { try { act(); } catch (Exception e) { captured = e; } }); t.Start(); t.Join();` and rethrow `captured`. That removes the dependency on MSTest's internal thread strategy without adding a wait construct. | The General Unit Test Policy requires determinism against the environment, and the current construction is deterministic only as a consequence of an MSTest implementation detail the tests do not control or assert. | Observed correct in five independent runs (executor RED and GREEN; reviewer RED, GREEN, and the wider four-class run), consistent with MSTest executing test bodies on a dedicated non-pool thread. No failure was reproduced; the finding is a robustness objection, not an observed defect. |
| Low | `artifacts/orchestration/orchestrator-state.json` | deleted in commit `4f74aa39` | The fix commit also deletes a 386-line tracked file belonging to an unrelated feature (its `objective` names PR #704 CI format recovery and its `workspace_root` names a different worktree). The deletion is unmentioned in the commit subject and lies outside the AC7 write set. | Mention the deletion and its rationale in the PR body, or split it into its own commit. | Bundling an unrelated tracked-file removal into a targeted bugfix commit weakens `git log` traceability. The change is otherwise beneficial: the path is matched by `.gitignore` line 57 and the file contained an absolute host path including the OS account name, so removing it from tracking eliminates a host-path leak. The local working copy is intact on disk, so no orchestration state was lost. | `git log --name-status a007f72e..4f74aa39 -- artifacts/orchestration/orchestrator-state.json`; `git check-ignore -v` reports `.gitignore:57`. |
| Informational | `QuickFiler/Viewers/BreadcrumbUiDispatcher.cs` | lines 255-278, `IsCurrentBoundary` | Two boundary-proof strategies now coexist in the same feature area with rationales that read as opposed. `ItemViewer` documents owner-thread identity as correct and context reference equality as unsuitable; `BreadcrumbUiDispatcher` documents context reference equality as authoritative and states "Bare owner-thread identity must never substitute here". Both are individually defensible — `BreadcrumbUiDispatcher` accepts a currently executing dispatcher callback first and always posts through its captured context, and its fallback identity test uses a managed id rather than a `Thread` reference — but the pairing is a maintenance hazard. | Add one cross-reference sentence to each `<remarks>` explaining why the two guards legitimately differ (one proves "this is the thread that owns the control", the other proves "this callback is running on the captured posting context"). No behaviour change. | A reader who encounters both comments without a cross-reference is likely to conclude one of them is stale and change it. | Read of both members in this session; `issue.md` "Related observations" records the same divergence. |
| Informational | `UtilitiesCS/Threading/UiThread.cs` | line 100, `SynchronizationContextAwaiter.IsCompleted` | The awaiter's completion test is `_context == SynchronizationContext.Current`, a reference comparison. For a dispatcher-built `ItemViewer` the captured context is a `DispatcherSynchronizationContext`, so `await viewer.UiSyncContext` always posts rather than continuing inline. This is an extra dispatch hop, not a failure, and it is unchanged by this branch. | Promote to a GitHub issue through the promotion lifecycle before the feature folder is archived. Do not fix here; it is outside the AC7 write set. | The observation currently exists only as prose in `issue.md`, which does not survive merge. The repository's practice is that out-of-scope defects are promoted to real issues rather than left in feature-folder text. | Read of `UtilitiesCS/Threading/UiThread.cs:90-106`; the file is absent from the branch diff. |

## Detailed Notes

### The fix itself

The rewritten guard is four lines of logic:

- `Dispatcher owning = UiDispatcher;`
- `if (owning == null) return;`
- `if (!owning.CheckAccess()) throw new InvalidOperationException(...)`

This is the minimal correct change. Three properties are worth recording:

1. **The null-owner escape is preserved but re-keyed.** It previously guarded a null captured
   context; it now guards a null captured dispatcher. Because `Dispatcher.CurrentDispatcher` creates
   a dispatcher for the calling thread rather than returning null, a viewer that has actually run
   its constructor always has a non-null owner. The escape therefore only fires for a viewer built
   without running the constructor, which is exactly the `FormatterServices.GetUninitializedObject`
   test shape the documentation names, and the new
   `InitializeBreadcrumbPipeline_NullOwningDispatcher_DoesNotThrow` test covers it by nulling the
   field through reflection while asserting the field still exists.
2. **All four guard sites benefit.** The single private method is called from
   `InitializeBreadcrumbPipeline` (line 51), both `ConfigureBreadcrumbDropDown` overloads (lines 172
   and 229), and `EnsureBreadcrumbResourceOwnership` (line 389). AC2's enumeration is satisfied by
   construction rather than by four separate edits.
3. **The D5 statement-order comment was correctly updated.** The comment on
   `EnsureBreadcrumbResourceOwnership` previously reasoned about `UiSyncContext` being null; it now
   reasons about the owning dispatcher being null, so the FIRST STATEMENT / FIRST ACTION argument it
   documents remains accurate after the swap.

### Test design

The strongest element of the new class is the non-vacuity assertion in the flagship test:

```
ReferenceEquals(viewer.UiSyncContext, SynchronizationContext.Current)
    .Should()
    .BeFalse(
        "the dispatcher operation must have captured a context that is not the "
            + "thread's ambient context, or this test would pass vacuously"
    );
```

This makes the test self-policing: if a future runtime change made the dispatcher reuse the ambient
context, the test would fail loudly rather than silently stop discriminating. That is the pattern
this repository's review history repeatedly asks for and it was applied here without prompting.

The two tests that use a repeat-call-with-the-same-provider shape both carry a `<remarks>`
explaining that the shape is required, not stylistic: a first-time initialization reaches
`BreadcrumbUiDispatcher.CaptureCurrent()`, which throws under a null ambient context regardless of
the guard, so only a call that returns through the already-initialized early return can witness the
case under test. The remarks also state why the shape still discriminates. This is the correct level
of documentation for a non-obvious test construction.

### Deletion of the two D4 tests

Removing `InitializeBreadcrumbPipeline_AmbientContextNull_ThrowsBoundaryDiagnostic` and
`InitializeBreadcrumbPipeline_DifferentNonNullContext_ThrowsBoundaryDiagnostic` is correct rather
than a coverage retreat: both asserted the defective behaviour directly, so retaining them would
require them to fail. Their replacements invert the expectation for the same two ambient shapes
(`_OwningThreadNullAmbientContext_DoesNotThrow` and
`_OwningThreadDifferentPlainContext_DoesNotThrow`) and add the dispatcher-operation shape that
neither original covered. Net coverage of the guard's contract increased.

The reviewer re-ran the six retained lifecycle tests, which cover the issue #488 D3 (second-provider
fail-fast) and D5 (disposal) behaviours AC5 requires be left intact, and all six pass.

### Scope discipline

The C# write set is exactly the four paths AC7 permits. The reviewer confirmed independently with
`git diff --numstat` that zero paths under `QuickFiler/Controllers/` appear in the branch diff, so
`QfcCollectionController.LoadSecondaryAsync`, `QfcItemController.AssignFolderComboBox`, and
`QfcItemController.EnsureBreadcrumbPipeline` are untouched, as AC7 requires.

The nine `.claude/agent-memory/**` files were reviewed rather than excluded. They are well-formed
learning entries with correct frontmatter (`name`, `description`, `metadata.type`) and matching
one-line `MEMORY.md` index pointers, and their content is accurate against what this review observed
(notably the `[ExcludeFromCodeCoverage]`-on-partial-class entry and the Cobertura package-rollup
entry). No policy document under `.claude/rules/` was modified.
