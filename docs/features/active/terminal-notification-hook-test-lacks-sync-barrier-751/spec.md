# terminal-notification-hook-test-lacks-sync-barrier (Spec)

- **Issue:** #751
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-09-03T12-05
- **Status:** Approved
- **Version:** 1.0

> Work mode is full-bug (issue.md, `- Work Mode:` marker). Per the acceptance-criteria-tracking skill,
> this spec is the **sole** authoritative acceptance-criteria source for issue #751; no user-story.md is
> produced. All findings below are drawn from the completed research record at
> docs/features/active/terminal-notification-hook-test-lacks-sync-barrier-751/research/2026-09-03T09-45-terminal-notification-hook-sync-barrier-research.md
> (cited as "research §n"); the race analysis was established mechanistically there and is not re-derived here.
> Paths written as bare prose in this document are deliberately not backticked: a downstream tool derives the
> change footprint from backticked paths, so only files the fix creates or modifies carry backticks.

## Context

- **Summary.** `TerminalNotificationHookFailure_DoesNotReplaceDispatchFault`
  (`TaskMaster.Test/AppGlobals/AppOlObjectsFolderTreeServiceTests.cs:102-120`) asserts
  `sut.InvokedTerminalHookCount.Should().Be(1)` at line 114 without any happens-before edge to the thread
  that performs the increment. The assertion can therefore observe the counter before the terminal
  notification hook has run, producing an intermittent failure of a required CI check. Research §2
  establishes the exact interleaving; research §9 summarises it.
- **Correction carried forward from research §0.** The test method lives in
  `TaskMaster.Test/AppGlobals/AppOlObjectsFolderTreeServiceTests.cs`, but the fixture it exercises
  (`ControlledAppOlObjects`, `ControlledUiDispatcher`, `ControlledDispatchOperation`, `Signal<T>`) lives in
  the sibling partial-class file `TaskMaster.Test/AppGlobals/AppOlObjectsFolderTreeServiceLifecycleTests.cs`.
  The issue text's single-file framing is incomplete; both files are named throughout this spec.
- **Observed environment(s).** GitHub Actions `windows-latest`, CI job `mstest-coverage / Run MSTest suite
  with coverage`, invoked as `vstest.console.exe TaskMaster.Test\bin\Debug\TaskMaster.Test.dll
  /EnableCodeCoverage /InIsolation`. Reproducible in principle on any Windows host running the MSTest
  suite; coverage instrumentation widens the race window without changing semantics (research §2.4).
- **Customer impact and severity.** Medium (issue.md, Impact/Severity). No end-user impact: the defect is
  confined to test code. A flaky required check blocks unrelated pull requests, forces CI re-runs, and
  violates the determinism requirement of the General Unit Test Policy (UT1) and the determinism
  infrastructure section of the general unit test rule file (.claude/rules/general-unit-test.md).
- **First observed / versions impacted.** Observed on the PR #746 CI run and recorded on 2026-09-03. The
  current file state was introduced by PR #746 (merge commit `a679cd08`); every commit at or after that
  merge carries the defect.

## Repro & Evidence

- **Steps to reproduce.**
  1. Check out `main` at or after merge commit `a679cd08` (PR #746).
  2. Run the `TaskMaster.Test` assembly repeatedly under vstest with `/EnableCodeCoverage /InIsolation`
     (loop locally, or re-run the CI `mstest-coverage` job).
  3. Observe `TerminalNotificationHookFailure_DoesNotReplaceDispatchFault` intermittently fail.
- **Expected vs actual.** Expected: the test observes the terminal-hook invocation before asserting on
  `InvokedTerminalHookCount`, and passes on every run. Actual: the assertion sometimes runs before the
  increment, and the test fails; an identical-code re-run then passes.
- **Error snippet.** `Expected sut.InvokedTerminalHookCount to be 1, but found 0.` (FluentAssertions
  output from the failed CI run on PR #746; recorded under `latent_defects_pending_promotion` in the
  parallel-orchestrator state artifact cited by issue.md).
- **Frequency / determinism.** Intermittent and scheduling-dependent, not data-dependent. Research §2.4
  identifies two interleavings: interleaving (b) runs the hook inline on the worker thread and passes
  unconditionally; interleaving (a) queues the continuation to `TaskScheduler.Default` and races the test
  thread. The race window between the worker unblocking and the counter increment is sub-microsecond, so a
  naturally red run is **not** reliably producible on demand. This directly shapes the fail-before evidence
  strategy (see Test Strategy).

## Scope & Non-Goals

- **In scope.**
  - `TaskMaster.Test/AppGlobals/AppOlObjectsFolderTreeServiceTests.cs` — add the synchronization barrier
    assertion; adopt the volatile read of the counter.
  - `TaskMaster.Test/AppGlobals/AppOlObjectsFolderTreeServiceLifecycleTests.cs` — make the counter
    increment atomic, matching the `_loadCount` precedent already present in the same fixture.
  - Regression evidence under
    `docs/features/active/terminal-notification-hook-test-lacks-sync-barrier-751/evidence/regression-testing/`
    and repeat-run stress evidence under
    `docs/features/active/terminal-notification-hook-test-lacks-sync-barrier-751/evidence/qa-gates/`.
- **Out of scope / non-goals.** *(Paths in this subsection are written as bare prose without backticks on
  purpose: a downstream tool derives the change footprint from backticked paths, so a backticked
  out-of-scope path would falsely widen the blast radius. Do not "fix" this formatting.)*
  - Production code: TaskMaster/AppGlobals/AppOlObjects.FolderTreeService.cs must not be modified. Research
    §3 establishes the defect is test-only: the virtual hook has no production override, AppOlObjects has no
    production subclass, and the notify-after-publish ordering is deliberate.
  - TaskMaster.Test/AppGlobals/AppOlObjectsFolderTreeServiceLifecycleTests.Coverage.cs — its own terminal
    hook override and probes already await the barrier and need no change.
  - UtilitiesCS/Threading/WpfUiDispatcher.cs and UtilitiesCS/Threading/IUiDispatcher.cs — the real
    dispatcher exhibits the same thread hand-off and no contract change is proposed (research §3.4-3.5).
  - Test-runner configuration: TaskMaster.runsettings and .github/workflows/_mstest-coverage.yml are not
    changed. Parallelism is not a contributing factor (research §7).
  - Issue #729 (test-determinism-and-hygiene-debt): thematically adjacent, but research §8 confirms no
    overlapping file, seam, or conflict. Its only role here is precedent for an evidence artifact shape.
- **Explicitly excluded systems, integrations, or datasets.** No production assembly, no external service,
  no data migration, no build or CI configuration.

## Root Cause Analysis

- **Confirmed root cause** (research §2.2, established by code trace rather than hypothesis). On the fault
  path exercised by this test, the worker task the test awaits is released by `TrySetException` at
  AppOlObjects.FolderTreeService.cs:261, inside the composition lock. The terminal notification is
  deliberately dispatched **after** that lock is released (AppOlObjects.FolderTreeService.cs:269-272), and
  it is that dispatch which reaches the fixture override and performs
  `InvokedTerminalHookCount++` at AppOlObjectsFolderTreeServiceLifecycleTests.cs:200. So
  `await GetExceptionAsync(run.Worker)` at test line 111 establishes **no** happens-before relationship with
  the increment; from line 261 onward the notifying thread and the test thread proceed concurrently.
- **`ReleaseAsync()` is not a barrier** (research §2.3). `await run.Operation.ReleaseAsync()` at test line
  112 is a complete no-op on this path: with no release backend it executes the captured composition
  closure, which returns immediately through the identity guard at AppOlObjects.FolderTreeService.cs:177-183
  because the initialization field was already nulled at line 262; the subsequent `TrySetResult` also fails
  because the completion was already faulted. The issue text's phrase "immediately after
  `await run.Operation.ReleaseAsync()`" describes position accurately but implies a synchronization the call
  does not provide.
- **Why it is intermittent, not always red** (research §2.4). If the fault lands before the worker
  evaluates the completion check at AppOlObjects.FolderTreeService.cs:159, the terminal observer runs inline
  on the worker thread and the counter is already 1 when `run.Worker` completes. If the fault lands after
  that check, only the registered continuation fires; because the fixture's completion source is created
  with `RunContinuationsAsynchronously`, the `ExecuteSynchronously` request is overridden and the
  continuation is queued to `TaskScheduler.Default`, i.e. a thread-pool thread distinct from both the test
  thread and the worker thread.
- **Second, independent latent cause** (research §2.5). `InvokedTerminalHookCount` is a plain non-volatile
  `int` field (`AppOlObjectsFolderTreeServiceLifecycleTests.cs:158-159`) written with a non-atomic `++` on
  one thread and read with a plain field read on another. The sibling counter in the same fixture uses
  `Interlocked.Increment` plus `Volatile.Read`. This is a visibility defect in addition to the ordering
  defect; ordering is the primary cause, and fixing it also closes the visibility gap, but the field is the
  only remaining unsynchronised cross-thread counter in the fixture.
- **Test-only determination** (research §3). Repo-wide grep over `*.cs` found the terminal hook overridden
  only in test code and no production subclass of `AppOlObjects` at all. The notification is dispatched from
  the production call sites enumerated in research §5, Claim N1, whose count is dual-derived there (textual
  multi-pattern search across the worktree, cross-checked by an exhaustive sequential read of the declaring
  file, with both member sets compared element-by-element and found equal). Under this test's trajectory
  exactly one of those sites fires, which research §5, Claim N2 derives twice independently (forward guard
  evaluation per site, cross-checked by a single-terminal-transition idempotence argument over the shared
  completion source, with equal member sets). The expected value `Be(1)` is therefore correct and must not
  be relaxed; only the ordering is defective.
- **Rejected alternative — reorder production** (research §3, counter-check). Invoking the hook before
  publishing the terminal result was considered and rejected: it inverts the meaning of "terminal", runs
  overridable user code under the composition lock, and creates a reentrancy path back into the service
  while the initialization is still live — the exact condition the reentry guard exists to reject.
- **Affected components/modules.** `TaskMaster.Test/AppGlobals/AppOlObjectsFolderTreeServiceTests.cs`
  (assertion site and `StartWorkerAsync`) and
  `TaskMaster.Test/AppGlobals/AppOlObjectsFolderTreeServiceLifecycleTests.cs` (fixture: counter field and
  hook override). Production behaviour under test is unchanged.

## Proposed Fix

### Design summary (what changes where):

Await the synchronization barrier the fixture already provides, instead of inventing one. `StartWorkerAsync`
(`TaskMaster.Test/AppGlobals/AppOlObjectsFolderTreeServiceTests.cs:249-262`) already captures the terminal
signal at line 257 — before the worker starts at line 258 — and returns it as the tuple's `Terminal` member.
Every other test in the class that observes terminal-hook side effects already awaits it; this test is the
only one that does not (research §1 enumerates the sibling call sites). The fix adds that await, in the same
shape the siblings use, and additionally hardens the counter itself.

### Boundaries and invariants to preserve:

- Production code is untouched; the notify-after-publish ordering and the containment `catch` in the
  production notifier remain exactly as they are.
- The test's existing assertions are preserved unchanged: `run.Worker` still faults with the
  object-identical `fault` instance (line 111), and `sut.LoadCount` is still 0 (line 113).
- The expected terminal-hook invocation count remains 1 (research §5, Claim N2).
- `CleanupAsync` is not modified and must still tolerate an already-terminal initialization.
- No wall-clock wait, `Thread.Sleep`, `Task.Delay`, polling loop, or new synchronization primitive is
  introduced.

### Dependencies or blocked work:

None. The fix depends only on members that already exist in the two in-scope files. Issue #729 shares the
determinism theme but supplies no seam and imposes no ordering (research §8).

### Implementation strategy (what changes, not sequencing):

#### Files/modules to change:

- `TaskMaster.Test/AppGlobals/AppOlObjectsFolderTreeServiceTests.cs` — required.
- `TaskMaster.Test/AppGlobals/AppOlObjectsFolderTreeServiceLifecycleTests.cs` — counter hardening
  (research §4.3), zero net lines.

#### Functions/classes/CLI commands impacted:

- `AppOlObjectsFolderTreeServiceLifecycleTests.TerminalNotificationHookFailure_DoesNotReplaceDispatchFault`
  — one inserted assertion between the existing lines 111 and 113:
  `(await GetExceptionAsync(await run.Terminal)).Should().BeSameAs(fault);`
  This is the shape already used by the sibling call sites. It is deterministic and cannot deadlock even
  with `throwFromTerminalHook: true`, because the fixture completes the signal (line 202) after the
  increment (line 200) and before it throws (line 204), with a full fence at line 201 between them.
- `ControlledAppOlObjects.OnFolderTreeServiceInitializationTerminal` — replace the non-atomic `++` with
  `Interlocked.Increment(ref InvokedTerminalHookCount);`.
- The assertion at line 114 becomes `Volatile.Read(ref sut.InvokedTerminalHookCount).Should().Be(1);`,
  matching the `Volatile.Read(ref ...)` assertion shape already used elsewhere in the fixture file.
  `System.Threading` is already imported in both files; no new `using` is needed.
- No CLI command and no production class is impacted.

#### Data flow and validation changes:

None. The added statement observes an existing completion signal; it changes no data flow in production or
in the fixture. Its assertion strengthens the test's stated claim: the test name promises that the hook's own
failure does not replace the dispatch fault, but today only the worker's exception is checked. Asserting that
the terminal task handed to the hook is faulted with the object-identical `fault` closes that gap.

#### Error handling and logging updates:

None. The production containment behaviour (the notifier swallowing the hook's exception) is the behaviour
under test and is unchanged. No logging or telemetry is added.

#### Rollback/feature-flag considerations (if applicable):

Not applicable. The change is confined to test code and is reverted by reverting the commit. No flag,
no staged rollout.

### Technical specifications (interfaces/contracts):

#### Inputs/outputs and formats:

No public or internal API signature changes. `InvokedTerminalHookCount` remains an `internal int` field on
the test-only fixture type, so `ref`-based access from the sibling test file in the same assembly remains
valid. The `StartWorkerAsync` tuple shape (`Worker`, `Operation`, `Terminal`) is unchanged.

#### Required configuration keys and defaults:

None. No configuration key, runsettings entry, or environment variable is added or read.

#### Backward-compatibility expectations:

No production surface changes, so there is no compatibility question for consumers. Within the test
assembly, the counter remains readable by any existing reader; the only behavioural difference is that
writes become atomic and the one cross-thread read becomes a volatile read.

#### Performance constraints (latency/throughput/memory):

The added await resolves against an already-created `TaskCompletionSource` and adds no measurable runtime
cost. `Interlocked.Increment` replaces `++` on a path executed a small number of times per test. No test
introduces a timed wait, so total suite duration is unaffected.

## Assumptions, Constraints, Dependencies

- **Assumptions.**
  - The fixture continues to complete the terminal signal after incrementing the counter and before
    throwing from the hook; this ordering is what makes the added await both a valid barrier and
    deadlock-free (research §4.1).
  - `run.Terminal` is the signal generation captured before the worker started, so it binds to the
    invocation this test provokes. Re-reading `sut.NextTerminal` at the assertion point would bind to the
    fresh signal swapped in at fixture line 201 and would hang; the captured value must be used.
  - The scenario fires the terminal hook exactly once (research §5, Claim N2), so there is no ambiguity
    about which generation the await observes.
- **Constraints.**
  - **500-line file cap** (.claude/rules/general-code-change.md, "File Size Limit"; CLAUDE.md §4). Research
    §4.4 measured, on 2026-09-03 in this worktree, 492 lines in
    `TaskMaster.Test/AppGlobals/AppOlObjectsFolderTreeServiceTests.cs` and 490 in
    `TaskMaster.Test/AppGlobals/AppOlObjectsFolderTreeServiceLifecycleTests.cs`. The selected fix costs one
    line in the first file and zero in the second. Any approach that adds a new gate field, a
    `TaskCompletionSource`, a helper method, or a fixture type would consume the remaining headroom and
    could force a file split; such approaches are excluded.
  - CSharpier may reflow the inserted statement across more than one physical line. The plan must budget for
    that against the cap and must accept formatter output over hand-formatting.
  - Framework constraints per CLAUDE.md CUT1/CUT2: MSTest, Moq, FluentAssertions only.
  - Determinism policy (.claude/rules/general-unit-test.md) bans `Thread.Sleep`, `Task.Delay`, and
    wall-clock waits in test code. The fix introduces none. No `TimeProvider`/`FakeTimeProvider` seam is
    required: the defect is an ordering omission, not a timing dependency, so there is nothing to advance.
- **External dependencies.** None. No package, service, or release is required.

## Data / API / Config Impact

- **User-facing or API changes:** none. The change is entirely within `TaskMaster.Test`; no production
  type, member, or signature is added, removed, or altered.
- **Data or migration considerations:** none. No persisted data, schema, or fixture data file is involved.
- **Logging/telemetry updates:** none.
- **Compatibility notes:** no CLI flag, config schema, runsettings entry, or version bump. CI workflow
  invocation is unchanged.

## Test Strategy

- **Regression tests to add or update.** No new test method. The existing test
  `TerminalNotificationHookFailure_DoesNotReplaceDispatchFault` in
  `TaskMaster.Test/AppGlobals/AppOlObjectsFolderTreeServiceTests.cs` is repaired in place by inserting the
  barrier assertion; the repaired test is itself the regression test. Adding a parallel test would duplicate
  the scenario and consume the file's remaining line headroom.
- **Unit tests (MSTest) for the fixed behavior and boundaries.** After the change the test must establish,
  in a single run and in both interleavings of research §2.4: the worker faults with the object-identical
  `fault`; the terminal task delivered to the hook is faulted with that same object-identical `fault` (i.e.
  the hook's own `InvalidOperationException` did not replace it); `LoadCount` is 0; and the terminal-hook
  count is read only after a happens-before edge from the increment.
- **Edge cases and negative scenarios.** Deadlock-freedom with `throwFromTerminalHook: true` is the critical
  negative case and is guaranteed by the fixture's increment-then-signal-then-throw ordering. The
  wrong-generation hazard (awaiting a freshly read `NextTerminal` instead of the captured `run.Terminal`)
  must be avoided; it would hang until the test timed out. `CleanupAsync` must still complete when the
  initialization is already terminal.
- **Error handling and logging verification.** The production notifier's exception containment is the
  behaviour under test; the added assertion verifies containment more directly than the current test does.
  No logging assertions are involved.
- **Coverage impact and targets.** No production line or member is added, so the coverage denominator is
  unchanged and no coverage exemption is involved. Changed-line coverage cannot regress: the changed lines
  are test code, executed by the test itself.
- **Fail-before evidence (decision required by the Bugfix Workflow in CLAUDE.md).** A naturally red run is
  not reliably producible: the window is sub-microsecond and interleaving (b) passes unconditionally
  (research §7). Two acceptable routes, in order of preference, to be operationalized by the atomic-planner
  as a Phase 2 task:
  1. **Preferred — forced red-before via temporary, reverted instrumentation.** Temporarily defer the
     fixture's increment/signal past the assertion point (for example, a scoped local gate awaited at the
     top of the hook override), capture the failing output as evidence under
     `docs/features/active/terminal-notification-hook-test-lacks-sync-barrier-751/evidence/regression-testing/`,
     then revert the instrumentation completely and land only the barrier fix and the counter hardening.
     This yields a genuine red-before artifact.
  2. **Fallback — documented rationale plus stress substitute.** If route 1 is judged to exceed the change
     budget or the remaining line headroom, author a `no-fail-before-rationale` dossier under
     `docs/features/active/terminal-notification-hook-test-lacks-sync-barrier-751/evidence/regression-testing/`
     following the artifact shape established for issue #729
     (docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/other/no-fail-before-rationale.2026-09-02T10-30.md),
     and substitute a repeat-run stress record under
     `docs/features/active/terminal-notification-hook-test-lacks-sync-barrier-751/evidence/qa-gates/`,
     following the precedent at
     docs/features/archive/2026-08-08-wpf-dispatcher-yield-test-order-dependent-508/evidence/qa-gates/repeat-run-comparison.2026-08-08T17-03.md.
     Exactly one of these two routes must be executed and its artifact committed.
- **Green-after validation.** Repeat runs of the `TaskMaster.Test` assembly under the CI-shaped invocation
  `vstest.console.exe <assemblies> /EnableCodeCoverage /InIsolation /Logger:trx
  /TestCaseFilter:"TestCategory!=LiveOutlook"`. Locally the invocation additionally needs the `\.claude\`
  worktree exclusion so sibling worktrees' assemblies are not collected. The repeat-run record is the
  green-after evidence and is also the substitute evidence under fallback route 2.
- **Toolchain commands to run (format → lint → type-check → test), per CLAUDE.md:**
  1. `dotnet tool run csharpier format .` (verify with `dotnet tool run csharpier check .`)
  2. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  3. `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`
  4. `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`

  Restart from step 1 if any step fails or rewrites a file.
- **Manual validation steps.** None beyond the repeat-run record. No Outlook host or live COM object is
  required by the affected test.

## Acceptance Criteria

- [ ] The barrier assertion `(await GetExceptionAsync(await run.Terminal)).Should().BeSameAs(fault);` is
      present in `TaskMaster.Test/AppGlobals/AppOlObjectsFolderTreeServiceTests.cs`, inside
      `TerminalNotificationHookFailure_DoesNotReplaceDispatchFault`, positioned after the existing
      `run.Worker` assertion and before the `LoadCount` assertion, and uses the captured `run.Terminal`
      rather than a freshly read `sut.NextTerminal`.
- [ ] The terminal-hook count assertion in that test still expects the value 1 (it is not relaxed, widened
      to a range, or deleted), and is reached only after the barrier assertion.
- [ ] Every read and write of `InvokedTerminalHookCount` across
      `TaskMaster.Test/AppGlobals/AppOlObjectsFolderTreeServiceTests.cs` and
      `TaskMaster.Test/AppGlobals/AppOlObjectsFolderTreeServiceLifecycleTests.cs` is synchronised:
      the increment uses `Interlocked.Increment` and the cross-thread read uses `Volatile.Read`, matching
      the `_loadCount` pattern already present in the fixture. No new `using` directive is required.
- [ ] TaskMaster/AppGlobals/AppOlObjects.FolderTreeService.cs is byte-identical to its state at branch point
      `f8414ee9`; the branch diff contains no production-assembly file.
- [ ] The repaired test passes on every run of a repeat-run series executed under the CI-shaped invocation
      with `/EnableCodeCoverage /InIsolation`, with no failure and no re-run required; the series output is
      committed as evidence under
      `docs/features/active/terminal-notification-hook-test-lacks-sync-barrier-751/evidence/qa-gates/`.
- [ ] Exactly one fail-before route from the Test Strategy is executed and its artifact is committed under
      `docs/features/active/terminal-notification-hook-test-lacks-sync-barrier-751/evidence/regression-testing/`:
      either a genuine red-before run captured with temporary instrumentation that is fully reverted before
      the final toolchain pass, or a `no-fail-before-rationale` dossier paired with the repeat-run stress
      record. If route 1 is taken, no instrumentation remains in the branch diff.
- [ ] After the change, neither `TaskMaster.Test/AppGlobals/AppOlObjectsFolderTreeServiceTests.cs` nor
      `TaskMaster.Test/AppGlobals/AppOlObjectsFolderTreeServiceLifecycleTests.cs` exceeds the 500-line cap
      defined in .claude/rules/general-code-change.md, and no new fixture type, gate field,
      `TaskCompletionSource`, or helper method was introduced.
- [ ] No banned determinism API (`Thread.Sleep`, `Task.Delay`, wall-clock wait, polling loop,
      `[DoNotParallelize]`) is added anywhere in the branch diff.
- [ ] A full toolchain pass completes cleanly in a single final pass, in order: `csharpier check` reports no
      unformatted file; the analyzer `msbuild` rebuild reports no error; the nullable `msbuild` rebuild
      reports no error; the MSTest run reports no failed test. The commands run and the clean-pass result
      are stated in the completion report.
- [ ] The issue.md checklist items under "Proposed Fix / Validation Ideas" are reconciled: the trace item
      and the production-reachability item are answered by the research record, and the barrier item is
      answered by the delivered change.

## Risks & Mitigations

- **Risk: an alternative "drain the pending dispatcher queue" fix is attempted.** Research §4.2 rejects
  this explicitly: `ControlledUiDispatcher` holds no queue, `Capture` creates a single operation published
  through a one-shot signal, `ReleaseAsync()` runs only that operation's own action — which is a no-op on
  this path — and the hook is not dispatched through the dispatcher at all. Any such plan would build a
  mechanism that does not exist. *Mitigation:* named here as a rejected alternative so the planner does not
  reintroduce it.
- **Risk: `[DoNotParallelize]` is added as a "determinism fix".** Rejected by research §7: `TaskMaster.Test`
  declares no assembly-level parallelization attribute, and the CI coverage step passes no settings file, so
  the class already runs sequentially in the failing job. The attribute would be a no-op that hides nothing.
  *Mitigation:* covered by an acceptance criterion above.
- **Risk: production reordering of the notification.** Rejected by research §3 (see Root Cause Analysis).
  *Mitigation:* the production file is out of scope and its immutability is an acceptance criterion.
- **Risk: awaiting the wrong signal generation causes a hang.** Reading `sut.NextTerminal` at the assertion
  point would bind to the fresh signal swapped in by the hook and never complete. *Mitigation:* the captured
  `run.Terminal` is required by an acceptance criterion; the fix uses the same shape as the existing sibling
  call sites.
- **Risk: the 500-line cap is breached by formatter reflow.** *Mitigation:* the selected fix costs one
  source line against measured headroom in both files, adds no new member, and the cap is verified as an
  acceptance criterion after formatting.
- **Risk: fail-before evidence cannot be produced naturally, weakening the regression story.**
  *Mitigation:* the Test Strategy defines two acceptable routes with committed artifacts and requires
  exactly one to be executed; if instrumentation is used it must be fully reverted before the final
  toolchain pass.
- **Risk: the counter hardening masks a future ordering regression by making a racy read appear stable.**
  The hardening fixes visibility, not ordering; ordering remains the barrier's responsibility.
  *Mitigation:* the barrier assertion is required independently, and the expected value stays at 1 so a
  missing invocation still fails the test.
- **Rollback.** Revert the single commit. No production behaviour, configuration, or data is affected.

## Rollout & Follow-up

- **Release/rollout steps.** Land as a normal pull request from
  `bug/terminal-notification-hook-test-lacks-sync-barrier-751` into `main` after a clean full-toolchain pass
  and the committed evidence artifacts. No deployment, no flag, no migration, no coordination with other
  teams. The required `mstest-coverage` check on the pull request is the gating signal.
- **Post-fix monitoring / clean-up.** Watch the `mstest-coverage` job on subsequent unrelated pull requests
  for any recurrence of the assertion failure; a recurrence would indicate a second unsynchronised
  observation not covered by this fix. If route 1 of the fail-before strategy was used, confirm the branch
  diff contains no residual instrumentation before merge. No follow-up issue is expected from this work;
  research §3 found no production-reachable variant of the race.
- **Links.**
  - Issue: https://github.com/drmoisan/TaskMaster/issues/751
  - Issue record: issue.md in this feature folder
  - Research: docs/features/active/terminal-notification-hook-test-lacks-sync-barrier-751/research/2026-09-03T09-45-terminal-notification-hook-sync-barrier-research.md
  - Origin of current file state: PR #746 (merge commit `a679cd08`)
  - Related programme (no overlap): issue #729, docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/
