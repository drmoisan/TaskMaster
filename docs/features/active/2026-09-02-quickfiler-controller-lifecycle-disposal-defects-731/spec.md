# quickfiler-controller-lifecycle-disposal-defects (Spec)

- **Issue:** #731
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-09-02
- **Status:** Draft
- **Version:** 1.0
- **Work Mode:** full-bug (spec.md is the sole acceptance-criteria source; no user story applies to a static-analysis defect consolidation)

## Context

Issue #731 consolidates five static-analysis findings that cluster on QuickFiler's collection-controller, queue, and form-controller lifecycle/disposal surface. They were consolidated into one issue because they share overlapping files and a fix for one lands in the same region as another. All five were independently re-verified against origin/main during consolidation, again by the orchestrator, and a third time by the task-researcher whose fix-design artifact is the authoritative design source for this spec (research/2026-09-02T13-10-controller-lifecycle-disposal-fix-design-research.md in this feature folder).

Environment:

- OS/version: Windows 11 Pro (repo default)
- Language/runtime: C# on .NET Framework 4.8.1, WinForms VSTO add-in. There is no Python toolchain in this repository.
- Test stack: MSTest with Moq and FluentAssertions.
- Data source or fixture: n/a — findings are from static code review.

Impact / Severity:

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

Medium: none of the five is confirmed live under current production call patterns, but finding 2 (uncancelled undo-consumer task) and finding 4 (unsynchronized reentrancy read) are real correctness gaps one caller change away from being live, and finding 1 is an architectural inconsistency that should be closed with a documented rationale before it masks a real bug.

## Repro & Evidence

Steps to reproduce: not applicable in the usual sense. Each finding below is a static code-review finding with its own reachability note.

Expected behavior: stated inline per finding.

Actual behavior:

**1. Three independent IEmailMoveMonitor instances.** QfcCollectionController (line 83), QfcDatamodel (line 103), and QfcQueue (line 40) each declare `private IEmailMoveMonitor _moveMonitor = new EmailMoveMonitor();` independently. The original report framed this as a defect on the theory that move-hook state could diverge between the three collaborating types. (Source: #620.)

**2. Cleanup() does not stop the undo consumer task.** In QfcFormController.SetupDisposal (lines 210-230), `Cleanup()` disposes `_undoQueue` and invokes `_parentCleanup`, but never touches `_undoConsumerTask`. The identifier `_undoConsumerTask` does not appear anywhere in that file. The consumer loop reads `_undoQueue.IsCompleted` and calls `_undoQueue.TryTake` on every iteration; both throw `ObjectDisposedException` against a disposed collection, and the resulting task fault is unobserved (the loop's `finally` nulls `_undoConsumerTask` and nothing awaits it; this repository sets no `ThrowUnobservedTaskExceptions` and has no `TaskScheduler.UnobservedTaskException` backstop). (Source: #621.)

**3. Dead constructor parameter in QfcRemainingQueueAdmission.** The constructor null-checks its `scoreLoader` parameter but never stores or invokes it. The task-researcher additionally verified that the constructor's first parameter, `globals`, is equally dead: it appears exactly once, at its own declaration, and is neither guarded, stored, nor used. (Source: #622.)

**4. Unsynchronized reentrancy-counter guard read.** In QfcCollectionController, `removespecificcontrolgroupcounter` is a plain `private static int` (line 909). Writes go through `Interlocked.Increment` (line 913) and `Interlocked.Decrement` (line 1008), but the guard at line 991 is a bare unsynchronized read. The guarded body is a `logger.Error` diagnostic, so a stale observation degrades a diagnostic rather than corrupting state. (Source: #634.)

**5. QfcFormController.SetupDisposal coverage debt.** Tracked separately as issue #683. No active feature folder for #683 exists; the only in-repo record is the promoted potential document dated 2026-08-28, which carries the last measured whole-file line-coverage baseline. Finding 2's fix and its regression tests land inside `Cleanup()`, which is part of the currently-uncovered surface, so the figure should be re-measured here rather than in an independent pass. (Source: #683.)

Logs / screenshots: none. Each finding is cited by file and line above and in the research artifact.

## Scope & Non-Goals

In scope:

- Findings 1-4 as designed in the Proposed Fix section below, plus the finding-5 evidence-only obligation.
- Removal of the `globals` constructor parameter in QfcRemainingQueueAdmission alongside `scoreLoader`. **This is an explicit in-scope decision beyond the literal text of issue #731**, recorded here so it is auditable: `globals` is the identical defect class (an unused, never-stored constructor parameter that misrepresents the type's contract) in the same 48-line file and the same constructor already being edited, so removing it in the same edit costs one production line and a handful of test lines, whereas leaving it behind would reproduce the exact API-dishonesty defect this issue is closing.
- Correcting the stale class comment at the head of EmailMoveMonitor. The comment states the class is malfunctioning and temporarily disabled; the class is fully live with nine production hook/unhook call sites, and the comment is the likely origin of the "state can diverge" framing in #620. Comment-only edit, no behavior change.

Out of scope / non-goals:

- Sharing a single IEmailMoveMonitor instance across the three owners. See the Proposed Fix section for the design rationale: the issue's first-listed option is rejected on evidence, and its second-listed option is adopted.
- Any redesign of EmailMoveMonitor into a multi-action, per-owner-scoped registry.
- Making the reentrancy counter instance-scoped rather than process-wide static. This is a real design smell but changing it is a behavior change to a diagnostic whose original intent is arguably process-wide, and it would break the existing static-state reset fixture. Record it as a follow-up potential entry; do not change it here.
- Reaching any coverage percentage target on QfcFormController.SetupDisposal. That remains the scope of issue #683.
- Wiring `scoreLoader` into real use. Admission-time threshold scoring was deliberately moved to dequeue-time enforcement by issue #233; re-introducing it here would reverse a ratified design.

Explicitly excluded files, integrations, and datasets:

- Do not touch the QuickFiler Controllers files QfcHomeController.Metrics.cs or EfcHomeController.Metrics.cs. A sibling parallel work item owns those two files.
- Do not split the QuickFiler Controllers file QfcCollectionController.cs. It carries pre-existing debt well over this repository's 500-line ceiling; splitting it is a separate work item. The only changes to it in this fix are one statement (finding 4) plus one comment line (finding 1), and it must not grow materially.
- No live Outlook COM, no shown WinForms `Form`, no temporary files, and no wall-clock waits in any new test.

## Root Cause Analysis

- **Finding 1** is not a defect in the shared sense the original report assumed. The three instances are load-bearing: `EmailMoveMonitor.BeforeItemMove` resolves its target with `_hookedItems.FirstOrDefault(x => x.Mail.EntryID == mail.EntryID)` and invokes exactly one action per mail, and `UnhookAll` clears the whole list for the instance it is called on. The same MailItem is hooked by more than one owner, and each owner registers a different action (`_masterQueue.Remove`, `RemovedItemMonitor`, and `QfcQueue.RemoveItem`). The root cause of the report is a stale, misleading class comment plus an absent rationale, not divergent state.
- **Finding 2** is a missing stop signal. The consumer loop already reads a stop condition (`_undoQueue.IsCompleted`), but `Cleanup()` never sets it and disposes the collection underneath the running loop instead. `BlockingCollection<T>.Dispose` is documented as not thread-safe and leaves the instance unusable; `TryTake` declares `ObjectDisposedException`.
- **Finding 3** is a vestige of the pre-#233 design in which threshold scoring happened at admission. When enforcement moved to dequeue time, the parameter was left in the signature.
- **Finding 4** is an incomplete application of the `Interlocked` writes / `Volatile.Read` guard idiom already established elsewhere in this assembly.
- **Finding 5** is accumulated test debt on a file that finding 2's fix will touch.

## Proposed Fix

### Design summary (what changes where)

| Finding | Design | Change size |
|---|---|---|
| 1 | Do **not** share. Adopt the issue's second option: document why per-owner instances are required, and pin the topology with a structural regression test. Correct the stale class comment on EmailMoveMonitor. | Three comment lines in production, one comment correction, one new test file |
| 2 | `CompleteAdding()` first, then defer `Dispose()` onto a continuation of the consumer task that observes the antecedent's fault. Never block the UI thread. | Roughly 15 lines in QfcFormController.SetupDisposal, one new test file |
| 3 | Remove the `scoreLoader` parameter and its guard; also remove the equally dead `globals` parameter. Update the sole production caller and the sole test factory. Replace the #233 intent pin rather than deleting it. | Net line reduction in production and test |
| 4 | Replace the bare read with `Volatile.Read`. Do **not** mark the field `volatile`. | One statement |
| 5 | No separate work item. Re-measure and record as evidence; leave the residual gap to #683. | Evidence only |

### Finding 1 — document the three-owner topology, do not share

The issue's first-listed option (share one instance via constructor injection or a shared owner) is **rejected on evidence**. Collapsing the three monitors into one would silently drop two of every three per-mail move actions: today each owner's monitor adds its own `folder.BeforeItemMove` subscription, so a move raises three handlers and all three registered actions run; with one shared monitor there is one subscription and one `FirstOrDefault`, so exactly one action runs and the other registrations are orphaned. Additionally, `UnhookAll` on the collection controller's page-teardown path would unhook every item the datamodel still has queued, and the per-folder unsubscribe predicate would fire at a different point than today. Making sharing safe would require redesigning a COM-bound helper with a live `BeforeItemMove` subscription into a multi-action, per-owner-scoped registry — the opposite of the smallest deterministic change the repository's bugfix workflow requires, and it would need a live Outlook folder to validate. A secondary obstacle also exists: `IEmailMoveMonitor` and `EmailMoveMonitor` are `internal` while all three owner types and their interfaces are `public`, so adding the parameter to any of those constructors is CS0051.

Adopted design:

1. Add one explanatory comment line above each of the three field initializers (QfcCollectionController line 83, QfcDatamodel line 103, QfcQueue line 40) stating that the instance is deliberately per-owner because `EmailMoveMonitor.BeforeItemMove` dispatches at most one action per MailItem via `FirstOrDefault` (EmailMoveMonitor lines 206-222) and `UnhookAll` is instance-scoped (lines 185-200), so a shared instance would drop the other owners' move actions and would let one owner's teardown unhook another owner's items. Cite issue #731 finding 1 and issue #620.
2. Add one new structural regression test file that pins the topology so a future consolidation cannot silently collapse it. It asserts, by source inspection, that each of the three owner files contains exactly one `new EmailMoveMonitor()` field initializer, and that no type declares more than one `IEmailMoveMonitor`-typed field. Follow the existing `ReadControllerSource` / `ResolveRepositoryPath` source-inspection precedent in the QuickFiler.Test controllers folder.
3. Correct the stale class comment at the head of EmailMoveMonitor.

Constraint carried forward: every existing test that injects a monitor does so by reflection on the private field name `_moveMonitor`. **Do not rename that field.**

### Finding 2 — signal, then defer disposal

Replace the bare `_undoQueue?.Dispose();` in `Cleanup()` with a signal-then-deferred-dispose sequence:

1. Capture `_undoQueue` and `_undoConsumerTask` into locals.
2. Call `CompleteAdding()` on the queue, guarded narrowly for the already-disposed case. This is the stop signal the consumer loop already reads: once the queue drains, `IsCompleted` becomes `true` and the loop exits normally through its existing `finally`. No new cancellation token and no change to the consumer loop are required. `CompleteAdding` must run before `Dispose`, because it declares `ObjectDisposedException`.
3. If the captured consumer task is `null`, dispose the queue immediately — no consumer can be mid-`TryTake`.
4. Otherwise dispose the queue from a continuation on the consumer task scheduled on `TaskScheduler.Default`, and read the antecedent's `Exception` inside that continuation so the fault is observed and routed to the existing logger. This matches the fault-boundary shape already ratified for issues #670 and #726 in this assembly.
5. Do **not** null `_undoQueue`: the consumer dereferences the field on every iteration, and `UndoDialog` is already inert post-cleanup because its existing guard trips once `Cleanup()` has nulled `_globals` and `_movedItems`.

A synchronous wait is **prohibited**. `Cleanup()` has exactly one production caller — the unqualified `Cleanup();` inside `ActionCancelAsync` in QfcFormController.EventHandlers (line 93), immediately after awaiting the form viewer's UI synchronization context, so it runs on the UI thread. If `Cleanup()` blocked on `_undoConsumerTask.Wait(...)` while the consumer was suspended at the dispatcher hop inside `ProcessUndoItemAsync`, the UI thread would block waiting for a continuation only the UI thread can run. That is a hard deadlock, it violates the standing STA-pumping directive, and it would put a wall-clock wait into a path the tests must drive.

Residual risk accepted: if the per-item processor hangs, the consumer never drains and the queue is never disposed. That is strictly better than today's dispose-under-an-active-consumer behavior and does not block the UI thread.

New tests go in a new file, QfcFormControllerCleanupTests in the QuickFiler.Test controllers folder, because QfcFormControllerSeamTests is at the repository's 500-line ceiling and cannot grow. Coverage of at least these paths, all achievable with the existing inline `UndoConsumerStarter` seam, the `FakeTimeProvider` clock seam, and the `UndoItemProcessor` seam (no live COM, no dispatcher, no wall clock):

1. Consumer running: after `Cleanup()` and a fake-clock advance, the consumer task reaches `RanToCompletion` rather than `Faulted`. This fails before the fix with `ObjectDisposedException`.
2. Adding was completed before disposal, observable through the fault-free termination in (1).
3. Consumer task `null` (the never-opened-undo-dialog path, which is the common case): `Cleanup()` still disposes the queue and does not throw.
4. Consumer parked: `Cleanup()` returns without blocking, guarding against the rejected synchronous-wait design regressing in later.

### Finding 3 — remove both dead constructor parameters

1. In QfcRemainingQueueAdmission, delete the `scoreLoader` parameter and its null guard, and delete the `globals` parameter (see the in-scope decision recorded under Scope & Non-Goals). Remove a `using` directive only if the compiler or analyzers show it unused after the edit; `System.Threading` and `System.Threading.Tasks` remain required by `TryQueueAsync`.
2. In QfcDatamodel, update the sole production construction site (lines 353-359) to stop passing the removed arguments. The `async (m, t) => (await ScoreRemainingQueueMailItemAsync(m, t)).Score` lambda disappears entirely. `ScoreRemainingQueueMailItemAsync` itself is independently used and independently tested, so it is not orphaned.
3. In QfcDatamodelTests, update the single private construction factory and its call sites.
4. **Do not simply delete** the existing test `TryQueueRemainingMailItemAsync_HighConfidenceEnabled_IgnoresThresholdAtAdmission`. It loses its mechanism when the parameter goes, but it is the only pin on intentional issue #233 design (threshold scoring belongs to dequeue-time enforcement, not admission). Replace it with a structural test asserting that the type's single constructor declares no scoring-delegate parameter and the type declares no scoring-delegate field, carrying the same rationale verbatim in its FluentAssertions `because:` message. A structural-pin precedent exists in the QuickFiler.Test viewers folder.

The type is `internal sealed`, so this is not a public API break.

### Finding 4 — read the counter through Volatile.Read

Change the guard at QfcCollectionController line 991 to `if (Volatile.Read(ref removespecificcontrolgroupcounter) > 1)`. Leave the declaration and both `Interlocked` writes untouched. No new `using` is needed; `System.Threading` is already imported.

Do **not** mark the field `volatile`. Passing a `volatile` field by `ref` to `Interlocked.Increment` and `Interlocked.Decrement` produces CS0420 at both call sites, and this repository's type-check gate runs msbuild with `/p:TreatWarningsAsErrors=true` with no `NoWarn` or `WarningsNotAsErrors` element in either affected project, so `volatile` would convert two clean lines into two build errors. The issue's first-listed suggestion is therefore not viable as written.

`Volatile.Read` is preferred over `Interlocked.CompareExchange(ref …, 0, 0)` because both are correct but the former is a pure acquire load with no write traffic, reads as a read at the call site, and is the established idiom in this same assembly (an `int` field with `Interlocked` writes and a `Volatile.Read` guard already exists in the QuickFiler viewers layer, with further precedent in UtilitiesCS).

A memory-visibility fix cannot be proven by a deterministic unit test, and a thread-racing test would violate the repository's determinism rules. Add a structural proxy instead, in QfcCollectionControllerDefects468Tests, which already owns this counter's test surface (the reflective field-name constant and the test initialize/cleanup resets). The test asserts by source inspection that the sole read of the counter goes through `Volatile.Read`, and its `<remarks>` must carry an explicit disclaimer that the assertion is a structural proxy for the memory-ordering fix and is not a proof that the race is eliminated, following the existing precedent in the QuickFiler.Test viewers folder. The existing issue-#286 reentrancy-restoration tests in that same file must continue to pass unchanged — a visibility fix changes no single-threaded observable behavior.

### Finding 5 — evidence only

No separate work item. Finding 2's regression tests execute `Cleanup()` on several distinct paths where today the only test that calls it is assertion-free, so the file's coverage will move as a side effect. The repository's mandatory post-change toolchain already runs the vstest console with code coverage enabled, so the new figure is produced as a by-product of a gate that must run anyway; recording it costs one evidence file, not a work item. The residual uncovered surface is WinForms and form-viewer-bound and unrelated to this issue's four code findings; closing it is #683's stated scope.

### Boundaries and invariants to preserve

- The private field name `_moveMonitor` on all three owner types (reflection target for many existing tests).
- The public constructor shape of QfcCollectionController (an existing test asserts it has a single public constructor and pins a parameter position).
- The consumer loop in QfcFormController.Actions: unchanged. The fix supplies the stop signal the loop already reads.
- The issue-#233 invariant that threshold enforcement happens at dequeue time, not at admission.
- The issue-#286 invariant that the reentrancy counter is restored on every throw path.
- No synchronous block on the UI thread anywhere in the teardown path.

### Dependencies or blocked work

None. No active feature folder exists for issue #683, so there is no in-flight work to collide with. The two excluded metrics files named under Scope & Non-Goals belong to a sibling parallel item.

### Error handling and logging updates

The deferred-dispose continuation must read the antecedent task's `Exception` and route it to the existing logger, converting today's silently dropped unobserved fault into a logged one.

### Rollback considerations

No feature flag. Every change is a small, independently revertable edit; findings 1, 3, and 4 are behavior-preserving or behavior-narrowing, and finding 2's revert is a one-line restoration.

## Write Set

`QuickFiler/Controllers/QfcCollectionController.cs`
`QuickFiler/Controllers/QfcDatamodel.cs`
`QuickFiler/Controllers/QfcQueue.cs`
`QuickFiler/Controllers/QfcFormController.SetupDisposal.cs`
`QuickFiler/Controllers/QfcRemainingQueueAdmission.cs`
`QuickFiler/Helper Classes/EmailMoveMonitor.cs` — contains a space
`QuickFiler.Test/Controllers/QfcMoveMonitorTopologyTests.cs`
`QuickFiler.Test/Controllers/QfcFormControllerCleanupTests.cs`
`QuickFiler.Test/Controllers/QfcDatamodelTests.cs`
`QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.cs`
`QuickFiler.Test/QuickFiler.Test.csproj`

The project file is part of the change footprint because QuickFiler.Test is a legacy non-SDK project that requires an explicit `<Compile Include>` entry for every source file; the two new test files will not compile into the assembly without one.

## Assumptions, Constraints, Dependencies

- Assumptions: the working tree matches origin/main for all five cited regions, as independently re-verified three times during consolidation, orchestration, and research.
- Constraints: 500-line file ceiling for new and modified files (QfcCollectionController is pre-existing debt above it and must not grow materially; QfcFormControllerSeamTests is at the ceiling and must not grow at all); MSTest plus Moq plus FluentAssertions only; no temporary files; no wall-clock waits; no live COM in tests; `TreatWarningsAsErrors=true` on the type-check gate.
- External dependencies: none. No new NuGet package, no new project reference.

## Data / API / Config Impact

- User-facing or API changes: none. QfcRemainingQueueAdmission is `internal sealed`, so its constructor-signature change is not a public API break, and its only two call sites are updated in the same change.
- Data or migration considerations: none.
- Logging/telemetry updates: the previously unobserved undo-consumer fault becomes a logged error.
- Compatibility notes: no CLI flags, config schemas, or versioned contracts are affected.

## Test Strategy

- **Finding 1:** one new source-inspection test file pinning the three-owner topology. No behavioral test is possible or appropriate; the assertion is structural by design.
- **Finding 2:** one new behavioral test file driving `Cleanup()` through the four paths listed in the Proposed Fix section, using the inline consumer-starter seam, a `FakeTimeProvider`, and an inert item processor. Test 1 is the failing-first regression test required by the repository's bugfix workflow.
- **Finding 3:** update the single construction factory and its call sites in QfcDatamodelTests; replace the #233 pin with an equivalent structural pin carrying the same rationale.
- **Finding 4:** one structural proxy test with an explicit not-a-proof disclaimer, added to the file that already owns the counter's test surface. The existing issue-#286 tests must pass unchanged.
- **Finding 5:** no new tests; re-measure coverage from the mandatory gate run.
- Edge and negative scenarios covered: consumer running, consumer absent, consumer parked, repeated `Cleanup()` invocation, and `CompleteAdding` against an already-disposed queue.
- Coverage: changed lines must not lose coverage. New test files target the standard new-code threshold.
- Toolchain commands, run in this exact order and restarted from step 1 on any failure or auto-fix:
  1. `dotnet tool run csharpier format .` (verify with `dotnet tool run csharpier check .`)
  2. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
  4. The vstest console runner over the built QuickFiler test assemblies with code coverage enabled, exactly as specified in the C# Toolchain section of CLAUDE.md.
- Manual validation: none required. No change alters a code path that requires a live Outlook session to observe.

## Acceptance Criteria

- [ ] Each of the three per-owner IEmailMoveMonitor field initializers (in QfcCollectionController, QfcDatamodel, and QfcQueue) carries an explanatory comment stating that per-owner instances are deliberate, citing the at-most-one-action-per-mail dispatch in `BeforeItemMove` and the instance-scoped `UnhookAll`, and referencing issue #731 finding 1 and issue #620.
- [ ] A new structural regression test pins the three-owner topology: it asserts that each of those three owner files contains exactly one `new EmailMoveMonitor()` field initializer and that no type declares more than one IEmailMoveMonitor-typed field. The test fails if a fourth owner is added or if the three are collapsed into one.
- [ ] The private field name `_moveMonitor` is unchanged on all three owner types, and every existing test that injects a monitor by reflection on that name still passes.
- [ ] The stale class comment on EmailMoveMonitor no longer describes the class as malfunctioning or disabled.
- [ ] `Cleanup()` calls `CompleteAdding()` on the undo queue before any disposal, and disposes the queue only after the consumer task has completed, via a continuation that reads and logs the antecedent's fault.
- [ ] No synchronous wait on the consumer task exists anywhere in the teardown path: no `Task.Wait`, no `.Result`, no `Thread.Sleep`, and no wall-clock timeout is introduced on the UI-thread-reachable `Cleanup()` path.
- [ ] A new test file, QfcFormControllerCleanupTests, contains a regression test that fails before the fix with `ObjectDisposedException` and passes after it, showing the consumer task reaches `RanToCompletion` rather than `Faulted`, plus tests for the null-consumer path, the parked-consumer non-blocking path, and completion-before-disposal.
- [ ] QfcFormControllerSeamTests is unmodified and its line count is unchanged.
- [ ] The QfcRemainingQueueAdmission constructor no longer declares the `scoreLoader` parameter, its null guard, or the `globals` parameter, and neither is stored in a field.
- [ ] The sole production construction site of QfcRemainingQueueAdmission, in QfcDatamodel, is updated to the new signature and the admission-time scoring lambda is removed; the test construction factory in QfcDatamodelTests is updated to match. The solution compiles with no remaining reference to the removed parameters.
- [ ] The issue-#233 intent is still pinned: the replaced admission-scoring test asserts structurally that the constructor declares no scoring-delegate parameter and the type declares no scoring-delegate field, carrying the original "threshold scoring belongs to dequeue-time enforcement" rationale in its assertion message. The original test is replaced, not deleted outright.
- [ ] The sole read of `removespecificcontrolgroupcounter` in QfcCollectionController goes through `Volatile.Read`, and the field is not marked `volatile`. Both `Interlocked` write sites and the field declaration are unchanged.
- [ ] A structural proxy test in QfcCollectionControllerDefects468Tests asserts that the counter's sole read goes through `Volatile.Read`, and its documentation carries an explicit statement that the assertion is a structural proxy for the memory-ordering fix and is not a proof that the race is eliminated.
- [ ] The existing issue-#286 reentrancy-counter restoration tests pass unchanged.
- [ ] Whole-file line coverage for QfcFormController.SetupDisposal is re-measured in this issue's final QA run, recorded as an evidence artifact under this feature folder's evidence/coverage directory, and compared in writing against the baseline figure recorded in the promoted potential document for issue #683 (dated 2026-08-28). Any residual gap is explicitly assigned to #683. Reaching any specific coverage percentage on that file is **not** a criterion of this issue.
- [ ] Both new test files are registered with `<Compile Include>` entries in the QuickFiler.Test project file and are confirmed present in the built test assembly by appearing in the test run.
- [ ] The full toolchain passes in a single uninterrupted pass in the documented order: csharpier format check clean, analyzer build clean, `TreatWarningsAsErrors=true` build clean, and the test run green with coverage enabled.
- [ ] No regression: the pre-existing QuickFiler test suite passes with no newly failing or newly skipped tests, and no public API surface changes.
- [ ] Neither of the two excluded metrics files named under Scope & Non-Goals is modified, and QfcCollectionController is not split; the diff to that file is limited to one statement and one comment line.

## Risks & Mitigations

- **Risk:** the deferred-dispose continuation never runs because the per-item processor hangs, leaving the queue undisposed. **Mitigation:** accepted and documented. It is strictly better than disposing under an active consumer, and it does not block the UI thread. The fault-observing continuation ensures a hang is at least not compounded by a silent exception.
- **Risk:** a future consolidation pass re-attempts the shared-monitor refactor. **Mitigation:** the finding-1 comments plus the topology pin test make the intent explicit and machine-enforced.
- **Risk:** removing the `globals` parameter renders strict mocks or factory parameters in the test file inert, producing analyzer noise. **Mitigation:** remove the now-inert arrangement in the same edit and rely on the analyzer gate to confirm no unused `using` or unused local remains.
- **Risk:** the structural source-inspection tests break on unrelated reformatting by csharpier. **Mitigation:** assert on normalized, whitespace-tolerant patterns, following the existing source-inspection precedents rather than exact-string matching.
- **Risk:** scope creep into QfcCollectionController's size debt or into #683's coverage work. **Mitigation:** both are explicit non-goals with named owners.

## Rollout & Follow-up

- Rollout: single pull request against main. No staged rollout, no feature flag, no configuration change.
- Post-fix follow-ups to record as potential entries:
  - Make the reentrancy counter instance-scoped rather than process-wide static, including a replacement for the static-state reset fixture.
  - Split QfcCollectionController to bring it under the 500-line ceiling.
- Links: issue #731 (https://github.com/drmoisan/TaskMaster/issues/731), consolidating #620, #621, #622, #634, and #683. Fix design source: research/2026-09-02T13-10-controller-lifecycle-disposal-fix-design-research.md in this feature folder. Numeric acceptance criteria in this spec (three monitor owners; the sole counter read; the sole production construction site of the admission type) are backed by the dual-derivation Numeric Derivation Evidence section N1 through N3 of that artifact.
