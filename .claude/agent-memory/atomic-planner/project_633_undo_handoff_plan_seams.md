---
name: project-633-undo-handoff-plan-seams
description: Issue #633 (FilerQueue drain barrier) planning seams — the orphan window has no deterministic fail-before; the barrier's real discriminator is an equal-priority dispatcher probe, NOT a null static and NOT IsCompleted; an empty Helpers list turns the preserved catch into a HANG; the recording metrics delegate must record synchronously
metadata:
  type: project
---

Planning seams derived while authoring and revising the atomic plan for issue #633
(`QuickFiler/Controllers/FilerQueue.cs` drain barrier + `QfcFormController.EventHandlers.cs`).

**Why:** each of these was asserted (by the spec, the research record, the delegating agent, or an
earlier planner pass) in a form that did not survive re-derivation against the tree.

**How to apply:** re-check these before reusing any #633-adjacent claim, and treat the coverage,
dispatcher, and `.gitignore` points as general.

## The orphaned-item window has NO deterministic fail-before

Closing the window requires a producer `Queue.Add` to land strictly between the worker's loop exit
(`FilerQueue.cs:48`) and the guard reinstall (`:63`). There is no seam, no await, and no observable
state change between those two statements, so no test can place a statement in that interval.
`Enqueue_AfterPreviousBatchDrained_ProcessesSecondBatch` additionally *names `WhenDrainedAsync()`*, so
it does not even compile pre-fix — an earlier pass called it "green before and after", which was false.
It belongs in the `fail-before-exception` dossier alongside the `WhenDrainedAsync_*` suite.

## The barrier fail-before discriminator: an equal-priority dispatcher probe

Two rejected discriminators, and the one that holds:

- **REJECTED — null `UiThread.Dispatcher`.** Not guaranteed and order-dependent.
  `QfcItemController.FocusAndThemeTests.cs:452` and `:468` call `EnsureUiThreadDispatcher()` and
  DISCARD the scope; `QfcItemController.TestSupport.cs:229-236` documents that discarding is permitted
  and leaks. Once either has run the static is non-null for the rest of the assembly run, which
  violates the order-independence rule in `.claude/rules/general-unit-test.md`.
- **REJECTED — `task.IsCompleted` on the returned `BackGroundMoveAsync` task.** BOTH dispatcher calls
  are awaited (`EventHandlers.cs:228` and `:233`), so under a pinned *running* dispatcher the pre-fix
  task is incomplete at the moment of return exactly as the post-fix task is.
- **HOLDS — the ContextIdle probe.** Mock `MoveEmailsAsync` to `Task.CompletedTask`, so the `await` at
  `:225` continues synchronously; pre-fix the metrics operation is therefore enqueued at
  `DispatcherPriority.ContextIdle` *before the method returns*. After the call, post your own probe to
  the same dispatcher at `ContextIdle` and await it: a WPF dispatcher runs equal-priority operations in
  enqueue order, so the probe cannot complete until the metrics operation has run. Assert the metrics
  recorder count is 0 with the gate closed. Pre-fix it is 1, post-fix 0 — no timing, no timeout.

Pin the dispatcher with `using (var transaction = await UiThreadDispatcherFixture.BeginTransactionAsync())`
plus `transaction.Install(QfcItemControllerTestSupport.StartRunningDispatcher())`. The `using` is
load-bearing, not stylistic: `BeginTransactionAsync` (`:122-126`) takes a `SemaphoreSlim(1,1)` released
only by `UiThreadDispatcherTransaction.Dispose` (`:261-276`), and no in-scope test file carries a
`[Timeout]`, so a permit leaked on an assertion-failure path hangs the run unboundedly.
See [[expect-fail-needs-a-synchronous-seam]] and [[dispatcher-repro-hang-trap]].

## An EMPTY `Helpers` list turns the preserved `catch` into a hang, not a failure

Found on round 2. Any test that drives the worker into the `catch` — the throwing-processor drain test
is the only one that does — must enqueue items whose `Helpers` list is non-empty. Use the existing
`OneHelper()` factory at `FilerQueueTests.cs:23`.

The chain: `FilerQueueItem`'s constructor (`FilerQueue.cs:70-78`) validates with
`helpers.Any(h => h is null)`, which is FALSE for an empty list, so an empty list is ACCEPTED. The
preserved catch at `:56` then calls `item.Helpers.First()`, which raises `InvalidOperationException`
INSIDE the catch — not caught by that same catch — so it escapes the `while (Queue.TryTake(...))` loop
at `:48`. The second item is never taken, its outstanding count is never decremented, and the awaited
drain never completes. `FilerQueueTests.cs` carries no `[Timeout]`, so the assembly run hangs rather
than failing. A default-constructed `MailItemHelper` is COM-free: `MailItemHelper()`
(`UtilitiesCS/OutlookObjects/MailItem/MailItemHelper.cs:80`) calls `InitializeSafeDefaults()` (`:167`),
which seeds `_sentOn`, `_subject`, `_senderName` (`:179-182`) with `string.Empty.ToLazy()` — exactly the
three members the `logger.Error` diagnostic reads. See [[dispatcher-repro-hang-trap]].

## The recording metrics delegate must record SYNCHRONOUSLY or the probe discriminator dies

`EventHandlers.cs:228` wraps `async () => await WriteMetrics(...)` (lambda on `:229`). A
`DispatcherOperation` over that lambda completes when the lambda RETURNS ITS TASK, at its first
suspension point — not when the Task completes. So a test recorder that awaits anything before
incrementing lets the ContextIdle probe complete with the count still 0, and the pre-fix run goes GREEN.
Mandate: record as the FIRST statement, then return an already-completed `Task`. The field type is
`private delegate Task WriteMetricsDelegate(string filename)` (`QfcFormController.cs:82`, field `:83`),
so the test must read the field's `FieldType` at run time and use `Delegate.CreateDelegate`.

## CSharpier splits `.ConfigureAwait(false)` off the acquisition line

A gate worded "every line containing `BeginTransactionAsync` also contains `using (`" is falsifiable by
the formatter, not by the executor. The repo's prevailing shape —
`QfcItemController.UiThreadDispatcherFixtureTests.cs:108-110` — is CSharpier's output for the chained
`await X.BeginTransactionAsync().ConfigureAwait(false)` form, and it puts `.BeginTransactionAsync()`
alone on line 109. Forbid the continuation and state the arithmetic: the single-line
`using (var transaction = await UiThreadDispatcherFixture.BeginTransactionAsync())` is 80 chars plus
indent, so 92-96 at 12-16 spaces, under CSharpier's default 100-column width (this repo has no
`.csharpierrc`). Also: `QfcItemController.SeamFactoryTests.cs` DOES carry `[Timeout(PumpTimeoutMs)]` at
304 and 375, so "no in-scope test file carries `[Timeout]`" is false — scope that claim to the fixture
file and the new test file, which are the only ones the permit-leak mechanism reaches.

## `InjectFilingCollaborators` hands out a REAL FilerQueue with the production default processor

`QfcItemController.TestSupport.cs:350` is `home.SetupGet(h => h.FilerQueue).Returns(new FilerQueue());`
and the helper assigns no processor. So "no unit test can execute the default `ItemProcessor` lambda,
every test assigns a fake" is FALSE, and a coverage-exemption clause that asserts it unconditionally is
a false justification. Make the quote-and-justify requirement conditional on the line actually appearing
in the uncovered list. See [[named-coverage-exception-verify-member-body]].

## A post-fix `Enqueue` removes the item before `ItemProcessor` runs

`QfcItemController.SeamFactoryTests.cs:234` asserts `filerQueue.Queue.Count.Should().Be(1)` and relies
on a pre-tripped `guard` (`:213-218`). After the handshake repair, `Enqueue` starts a worker whose
`TryTake` removes the item before the processor is invoked, so a gated processor parks with
`Queue.Count == 0`. Replace the count assertion with a `TaskCompletionSource<FilerQueueItem>` that the
processor completes: awaiting it is deterministic and asserts the identity of the filer the factory
produced.

## Invoke-MSTestWithCoverage leaves an UNFILTERED file on two paths, not one

Refines [[reference_invoke_mstest_with_coverage_script]] and
[[project_494_threshold_reconciliation_plan_seams]]. `Set-Content` at script line 343 is reached only
after **both** succeed: `Invoke-DotnetCoverageCollection` (throws at script 236 on any failing test) and
`Assert-CoberturaLineCoverageThreshold` (throws at helper 489 when the **filtered** rate is under 80).
So a sub-80 run also leaves the raw dotnet-coverage output. Gate this at **Phase 0**, not Phase 7: an
unfiltered baseline makes the final comparison and the coverage AC unreachable, and discovering that
after all implementation work is done costs the whole run.

The reliable discriminator is structural: post-processing injects a `sources` element (helper 430-439)
and rewrites the six summary attributes (helper 441-447); the raw file has no `sources` element.

## Environment facts verified in this worktree

- `vstest.console.exe` is **not on PATH**. Resolve it once in Phase 0 with
  `vswhere.exe -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe"` and
  have every scoped run task refer to that recorded path. `vswhere`, `msbuild`, `nuget`, and
  `dotnet-coverage` do resolve.
- `.gitignore:84` is `*.log`, so every `/flp:logfile=*.log` artifact is untracked and never committed.
  Name MSBuild file logs `*.msbuild.txt` instead; editing `.gitignore` is usually out of blast radius.
- MSBuild's file logger does not create intermediate directories: `/flp:logfile=` into a
  not-yet-existing `evidence/<kind>/` dies with MSB1029. Prepend `New-Item -ItemType Directory -Force`.
- The repo-sanctioned restore is `scripts/vscode/Invoke-Restore.ps1` (line 36:
  `msbuild /t:Restore /p:RestorePackagesConfig=true /m`), which covers packages.config AND
  PackageReference. A bare `nuget restore` covers only the former.

## Smaller verified facts

- `QuickFiler/Controllers/FilerQueue.cs` is **83** lines, not 84.
- `dotnet-tools.json` lives at the **repository root**. CSharpier 1.2.6, `isRoot` true.
- `QfcFormController.WriteMetrics` is a `private` field of a `private delegate` type
  (`QfcFormController.cs:82-83`). A test must read the field's `FieldType` at run time and call
  `Delegate.CreateDelegate(fieldType, target, methodInfo)`.
- `QfcItemController.SeamFactoryTests.cs` is 436 lines — only 64 of headroom.
- `EmailFiler.SortAsync(IList<MailItemHelper>)` returns `Task<bool>`, assignable to `Task`, so a
  `Func<FilerQueueItem, Task>` seam needs no generic parameter.
- `spec.md` check-off anchors `exposes`, `awaits`, and `is introduced` are NOT file-unique; they resolve
  only when the search is constrained to `^- \[[ x]\] ` lines between the `## Acceptance Criteria` and
  `## Risks & Mitigations` headings.
