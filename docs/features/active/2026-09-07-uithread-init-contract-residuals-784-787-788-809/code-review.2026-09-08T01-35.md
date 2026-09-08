# Code Review — issue #809, `uithread-init-contract-residuals-784-787-788`

- Artifact timestamp: 2026-09-08T01-35
- Base `04a54e681bd21e841e124c016df30672ee701b75` to head `ef431e6a`; 12 files, +1134/-34
- Reviewer mode: read-only inspection of the head tree, the supplied patch, the 44 committed evidence
  artifacts, and both Cobertura documents. No file was modified.

**Blocking findings: 0.**

## 1. Overall assessment

The change is small, well-bounded, and matches the approved specification closely. The three defects
are fixed in the order and shape the spec prescribes, the two test seams are the minimum needed, and
the delivery does not widen scope: `ThreadSafeSingleShotGuard` is retained, no `InternalsVisibleTo`
grant is added, no `.runsettings` apartment setting is changed, and the two reflected field names
`_uiSyncContext` and `_dispatcher` survive intact for the four external reflection consumers.

The evidence discipline is above average. The executor discovered mid-execution that its own
apartment premise was wrong, recorded the falsifying measurement verbatim, re-authored the two
affected tests, and re-measured fail-before against a restored Phase 2 tree rather than leaving the
first pass standing. That is the correct handling of an invalidated premise.

## 2. Q1 — Is the new `IsCompleted` predicate safe?

**Verdict: the predicate honours the governing constraint. It does not evade it.**

The constraint, verified by the reviewer verbatim at `QuickFiler/Viewers/BreadcrumbUiDispatcher.cs:263-272`:

> Bare owner-thread identity must never substitute here: a continuation resumed after
> `ConfigureAwait(false)` can be scheduled onto a recycled thread-pool thread whose managed thread ID
> equals the captured owner thread ID, which would run UI work inline and complete the returned task
> without any post ever crossing the captured context.

The delivered predicate at `UtilitiesCS/Threading/UiThread.cs:155-190` has exactly three ways to
return `true`:

1. `ReferenceEquals(_context, ambient)` — an ambient reference match to the captured context. This is
   the strictest possible boundary and is identical to the pre-change behaviour.
2. `ReferenceEquals(_context, _uiSyncContext)`, reached only after the thread-id guard.
3. `_context is DispatcherSynchronizationContext && ReferenceEquals(Dispatcher.FromThread(Thread.CurrentThread), _dispatcher)`,
   reached only after the thread-id guard.

Thread identity is a **necessary but never sufficient** condition on branches 2 and 3. Each of those
branches additionally requires a reference match against an object captured at `Init()` time — the UI
synchronization context on branch 2, the UI dispatcher on branch 3. There is no path on which
`_uiThreadId == Thread.CurrentThread.ManagedThreadId` alone produces `true`. That is precisely what
the Breadcrumb rule forbids, and it is not what this predicate does.

Branch 3 is additionally self-securing. `Dispatcher.FromThread` returns the dispatcher bound to the
argument thread, and a `Dispatcher` is permanently affine to the thread that created it. A recycled
thread-pool thread cannot be handed the UI thread's dispatcher, so branch 3 stays false on any thread
other than the true owner regardless of what `_uiThreadId` holds. Branch 3 therefore does not depend
on the thread-id guard at all for correctness.

### The residual the caller identified

Branch 2 is the one branch whose only thread-locating evidence is `_uiThreadId`. The residual is real:
if the thread that ran `Init()` has exited and the runtime later reuses its managed thread id for a
different thread, then on that thread, with a non-null ambient context that is not `_context`, and
with `_context` being the still-referenced `_uiSyncContext` object, branch 2 returns `true` and the
continuation runs inline off the UI thread.

**Reachable in production: no.** Managed thread ids are unique among live threads; reuse requires the
original thread to have terminated. The only production caller of `Init()` is `TaskMaster/ThisAddIn.cs:35`
on the Outlook main STA thread, which lives for the process lifetime. While that thread is alive no
other thread can carry its id, so branch 2's guard is exact for the whole period in which the predicate
is ever evaluated. Reaching the residual in production would require the Outlook UI thread to die and
the add-in to keep running, at which point inline continuation ordering is not the failure that
matters.

**Reachable in tests: not as delivered, and not by accident.** Every test that writes `_uiThreadId`
does so inside `UiThreadStateScope`, which restores the prior value on disposal (`UiThreadStateScope.cs:320-335`),
and the four production statics are additionally reset at scope entry. Three further conditions would
have to coincide inside one scope: a dead host thread whose id was recycled to the evaluating thread,
`_uiSyncContext` installed and identical to the awaited context, and a third non-null ambient context.
No test in this delivery constructs that arrangement, and the coverage data confirms it: lines 177-178
have zero hits.

**Unreachable by the ConfigureAwait(false) mechanism specifically.** That mechanism resumes a
continuation on a pool thread. Pool threads normally carry a null ambient `SynchronizationContext`, in
which case the `ambient is null` early return at line 167 answers `false` before the thread-id guard is
ever evaluated. For the ambient to be non-null on such a thread, some other captured context must be
installed, in which case branch 1 would have already matched if that context were `_context`.

**Recommended hardening (not required for merge).** Adding the dispatcher-identity test to branch 2
would remove the residual entirely at no behavioural cost, because on the true UI thread
`Dispatcher.FromThread(Thread.CurrentThread)` already returns `_dispatcher`:

```csharp
if (ReferenceEquals(_context, _uiSyncContext)
    && ReferenceEquals(System.Windows.Threading.Dispatcher.FromThread(Thread.CurrentThread), _dispatcher))
{
    return true;
}
```

This is worth an issue, not a remediation cycle.

### `WinFormsPumpHostTests` confirmation

`QuickFiler.Test/TestSupport/WinFormsPumpHostTests.cs:181-204` (`AwaitingSyncContext_FromTheTestThread_ResumesOnThePumpThread`)
still passes, twice, recorded in `p4-t3-quickfiler-tests.md` with durations `00:00:00.0030778` and a
second pass after the Phase 4 rebuild. The reason it passes is structural rather than incidental: the
awaited context is the pump host's `WindowsFormsSynchronizationContext`, which is neither reference-equal
to `_uiSyncContext` nor a `DispatcherSynchronizationContext`, so branches 2 and 3 both fail and the
continuation posts to the pump thread as the assertion requires. The delivery additionally removed the
`UiThread.Init(false)` call from `QfcHomeControllerRunAsyncTests.cs:329` that made this failure mode
possible at all, so the test no longer depends on execution order within the assembly. A dedicated
regression guard, `IsCompleted_WithAForeignWindowsFormsContextWhileUiThreadIdMatches_ReturnsFalse`,
pins the reason in `UtilitiesCS.Test` where it is cheap to run.

## 3. Q2 — Is the `[P0-T15]` apartment inference sound?

**Verdict: the inference is unsound. The `MTA_` label is unsupported and most likely wrong. The AC2
design argument nevertheless holds. Grade the labelling risk Medium and the design risk nil.**

`p0-t15-mta-synccontextform-measurement.md:42` reasons: "The test is declared with a plain `[TestMethod]`
at `:325` on a class carrying no `[STATestClass]`, so the executing apartment is the MSTest default,
which research R4 established as MTA from three independent in-tree sources." That is an inference from
a premise, not a measurement. Nothing in the run read `Thread.CurrentThread.GetApartmentState()`.

The same delivery then falsified the premise. `p2-t10-fail-before.md` quotes the verbatim TRX message
`Expected Thread.CurrentThread.GetApartmentState() to be ApartmentState.MTA {value: 1}, but found
ApartmentState.STA {value: 0}.` from a plain `[TestMethod]` on a plain `[TestClass]`.

The caller's question is whether the `/Tests:` single-test selection makes the executor's stated
mechanism inapplicable. It does — and that is the point, because the reviewer's reading of the tree is
that the executor's stated mechanism is not the operative one:

- The `[P0-T15]` command passed **no `/Settings:` argument**. No runsettings applied.
- No `.runsettings` anywhere in this repository sets `ExecutionThreadApartmentState`; the reviewer
  grepped all five (`TaskMaster.runsettings`, `scripts/vscode/TaskMaster.cli.runsettings`,
  `UtilitiesCS.Test/test.runsettings`, `TaskTree.Test/coverage.tasktree.runsettings`,
  `TaskVisualization.Test/coverage.runsettings`). `UtilitiesCS.Test/test.runsettings` is an empty
  `<RunSettings />` carrying only a comment.
- `UtilitiesCS.Test/Properties/AssemblyInfo.cs:17-20` carries `[assembly: Parallelize(Workers = 0,
  Scope = ExecutionScope.ClassLevel)]`, so that assembly parallelizes even with no runsettings.
  `QuickFiler.Test` carries **no** assembly-level `Parallelize` attribute, so with no runsettings its
  tests do not parallelize at all.

That yields one explanation covering both observations without needing bucket-sharing: tests dispatched
to the MSTest parallel worker pool run on thread-pool threads and are MTA, while tests that run on the
main test-execution thread — the `[DoNotParallelize]` serial bucket, or every test when parallelization
is off — inherit that thread's apartment, and the vstest execution thread on .NET Framework is STA
unless `ExecutionThreadApartmentState` says otherwise. Under that explanation the `[P0-T15]` run, being
a single test in a non-parallelizing assembly with no runsettings, executed on the main thread and was
**STA**.

Consequences:

- The `MTA_INITIALIZE_OUTCOME: COMPLETED` token is mislabelled. What was measured is that
  `new SyncContextForm(); Show();` completes on the vstest main execution thread, whose apartment was
  not read.
- The refutation in `p6-t4-ac2-regression-reconciliation.md` — "the #782 mechanism narrative is refuted
  on this host" — does not follow. The #782 narrative requires a throw on an MTA thread; a successful
  run on an STA thread says nothing about it. The narrative's status reverts to UNKNOWN, which is where
  decision D5 found it.
- `p6-t13-closure-summary.md` section 6 propagates the narrower mechanism to future planners. Its
  operational advice ("a test that needs a caller of a known apartment must create a dedicated thread
  and set the apartment explicitly") is correct and is what the re-authored tests now do; only the
  stated cause is doubtful.

### Does the `[P6-T4]` "safe whichever value was measured" argument hold?

**Yes, and the reviewer verified it structurally rather than accepting it.** The argument is that the
AC1 precondition makes the expensive, potentially-throwing body of `Initialize()` unreachable from any
non-STA caller, so no retry storm can originate on a thread-pool thread whatever `SyncContextForm`
does there. Verified against the head tree:

- `Init()` reads `GetApartmentState()` and throws at `UiThread.cs:30-34`, before the four monitoring
  assignments and before `lock (InitLock)`.
- `Initialize()` is called from exactly one place, `UiThread.cs:57`, inside that lock.
- The only other entries are the two lazy getters, `UiSyncContext` at `UiThread.cs:207-210` and
  `AutoScaleFactor` at `UiThread.cs:281-284`, both of which call `Init()` and therefore hit the
  precondition first.
- `Dispatcher` at `UiThread.cs:251-269` does not call `Init()`, as its `<remarks>` states.

So a non-STA reader costs one `GetApartmentState()` and a throw. The argument is sound independently of
the measurement, exactly as decision D5 required. This is why F1 is an evidence defect rather than a
code defect, and why it is non-blocking.

Additionally, the AC2 anti-retry-storm test does not rest on the ambient apartment at all: it drives
its Act through `ApartmentThreadRunner.RunOnThread(ApartmentState.MTA, ...)`, which calls
`SetApartmentState` on a dedicated thread before `Start()`. Its apartment is set, not inherited.

## 4. Q3 — Are the two re-authored Phase 2 tests still genuine fail-before evidence?

**Verdict: yes. The re-measured fail-before is admissible, the failures are attributable to the
defects, and no test was weakened.**

Four checks were performed.

**The first pass is correctly disqualified for two rows, and only two.** The verbatim messages in
`p2-t10-fail-before.md` show `Init_OnMtaThread_ThrowsInvalidOperationExceptionNamingTheObservedApartmentState`
failing on its Arrange premise (`...but found ApartmentState.STA`) — it never reached the Act — and
`Init_OnMtaThread_CapturesNoGlobalStateAndLeavesMonitoringConfigurationUnchanged` reaching the Act on
an STA thread, where the AC1 precondition correctly does not throw, so it would have stayed red after
the fix. Both disqualifications are correct. The other four rows already drove their Act on dedicated
threads with explicitly set apartments and are unaffected, which the reviewer confirmed against the
delivered test source.

**The re-measurement is against the right tree.** `UiThread.cs` and `UiThreadStateScope.cs` were
restored to their committed Phase 2 state at `f7294d71`, the solution was rebuilt with `/t:Rebuild`, and
the identical `TestCaseFilter` was re-run. `REMEASURED_EXIT_CODE: 1` with `ExpectedExitCode: 1` under the
`[expect-fail]` convention in `.claude/skills/atomic-plan-contract/SKILL.md`. Counters are 22 total, 22
executed, 16 passed, 6 failed, derived skipped 0.

**The failures are defect failures, not harness or compilation failures.** A compilation failure would
have produced zero discovered tests, not 22 discovered with 16 green. The 16 green rows include the
five pre-existing awaiter tests and the four new tests that were expected to be green pre-fix, each with
a stated reason. The six red rows carry assertion messages, not infrastructure exceptions:
`Expected observed to be System.InvalidOperationException, but found <null>` on the three apartment
rows, `Expected working not to be <null>` on the retry row, `Expected InvalidOperationException.Message
... but found <null>` on the anti-storm row, and `Expected result to be True, but found False` on the
awaiter row. Each maps to the specific defect: no precondition existed, the latch was consumed before
`Initialize()`, and the predicate compared by reference. `found <null>` on the apartment rows is the
positive signature that `Init()` returned normally from a genuine MTA caller.

**No test was weakened.** The remedy moved the Act onto `ApartmentThreadRunner.RunOnThread(ApartmentState.MTA, ...)`.
Neither method was renamed, none was added or removed, the discovered total stayed at 22 across both
passes, and each method asserts the same contract — an `InvalidOperationException` whose message starts
with `NonStaInitMessagePrefix`, and the eight unchanged fields. This is a strengthening: the pre-change
form asserted the contract against a caller whose apartment was assumed, the post-change form asserts it
against one whose apartment is set. The pass-after run (`p3-t6-pass-after.md`) is 22/22 at exit 0 with
all six named rows green.

One process note: an earlier `p3t6.trx` pass-after attempt reported 20 passed and 2 failed. That is
recorded rather than discarded, and the two failures are the same defective premise. Retaining the
superseded pass in the artifact is the right call and made this review verifiable.

## 5. Q4 — Are the two residual coverage gaps acceptable?

**Verdict: not blocking. Both are acceptable, for different reasons.**

Reviewer-verified uncovered set at head, recomputed from `coverage/809-p5-final.cobertura.xml`:
`38, 39, 40, 177, 178`. File total 121/126 = 96.03%. Member total for `IsCompleted` 18/20 = 90.00%.

**`UiThread.cs:38-40`** — the body of `if (onLockupDetected is not null) { _onLockupDetected = onLockupDetected; }`.
The guard condition at line 37 is covered; only the assignment is unreached, because the single test
that supplies a callback supplies it precisely to prove that a rejected `Init()` performs no assignment.
This is a three-line, no-logic assignment on a path whose semantics are asserted from the negative side.
Acceptable.

**`UiThread.cs:177-178`** — the true arm of `ReferenceEquals(_context, _uiSyncContext)`. The caller is
right that this is the delivery's highest-risk branch and its least-covered one, and that combination
deserves an explicit answer rather than a percentage. It is acceptable for four reasons, taken
together rather than individually:

1. **The condition is exercised; only the arm is not.** Line 176 is covered with hits and evaluates
   false in `IsCompleted_OnOwningUiThreadWithADispatcherContextCapturedInsideAnInvoke_ReturnsTrue`,
   which falls through it to the dispatcher clause. The clause is not dead code that was never
   compiled into a reachable path — it is reached and evaluated, and the false outcome is asserted.
2. **The uncovered arm is `return true;`.** It contains no state mutation, no call, and no branch. The
   failure mode of an untested `return true` is a wrong answer, and the wrong answer it could give is
   exactly the F3 residual, which is unreachable in production for the reasons in Q1.
3. **The adjacent risk is covered from the other side.** The reason branch 2 exists — that a UI-owned
   context should not force a hop — is asserted through the dispatcher branch, and the reason it must
   not over-admit is asserted by `IsCompleted_WithAForeignWindowsFormsContextWhileUiThreadIdMatches_ReturnsFalse`
   and `IsCompleted_WhenTheDispatcherContextBelongsToADifferentThreadsDispatcher_ReturnsFalse`. The
   over-admission failure mode is therefore pinned even though this specific arm is not.
4. **It meets the governing floors.** The member sits at exactly 90.00%, at the `CLAUDE.md` new-code
   floor; changed-line coverage is 95.83%; the file is 96.03% against an 80% floor and a 76.83%
   baseline. There is no threshold under either the `CLAUDE.md` or the `.claude/rules` reading that
   this delivery fails on this file.

The gap is closable by one test that installs `_uiSyncContext`, installs `_uiThreadId` for a host
thread, sets a third context ambient, and awaits the installed instance from that thread. That is a
small addition and both `p6-t1` and `p6-t2` already name it. Recommended as a follow-up issue rather
than a remediation cycle, and it should be bundled with the F3 hardening, because hardening branch 2
changes what the new test must assert.

## 6. Coverage exclusion policy

**No first-party production path is excluded. PASS.**

- Repository-root `coverage.config` excludes seven third-party module path patterns: Deedle, FSharp,
  Castle.Core, FluentAssertions, Moq, Microsoft.Testing, MSTest. All are packages, none is a production
  source path.
- The derived `coverage/809-effective-coverage.config` is byte-identical to that file plus exactly one
  appended entry, `<ModulePath>.*\.Test\.dll$</ModulePath>`, verified by direct comparison. It matches
  test assemblies by output filename and cannot match a production assembly, since no first-party
  production assembly is named `*.Test.dll`.
- These are module-path exclusions in an instrumentation settings file, not `exclude` entries pointing
  at source paths, so the prohibited category in `.claude/rules/general-unit-test.md` does not arise.
- No `[ExcludeFromCodeCoverage]` attribute was added anywhere in the diff. The one such attribute
  mentioned in the closure summary, on `ThreadMonitor.PingAndAwaitDiagnosticWindow()`, is pre-existing
  and outside the Write Set.
- Cross-check on the effect of the `.Test.dll` exclusion: the first-party packages enumerated in the
  Cobertura document are the nine production assemblies, and no `*.Test` package appears, which is the
  intended outcome of `UT2`'s instruction to keep test files out of the metric.

## 7. Design and implementation notes

Strengths worth recording:

- **The precondition is placed where the spec argues it must be.** It is the first statement of
  `Init()`, ahead of the four monitoring assignments that previously executed on every call regardless
  of the latch. `Init_OnMtaThread_CapturesNoGlobalStateAndLeavesMonitoringConfigurationUnchanged`
  asserts all eight fields, so the placement is pinned rather than incidental.
- **The retry fix also closes a pre-existing race.** `lock (InitLock)` removes the #782 finding C04
  window in which a second caller could observe the `Interlocked.Exchange` latch consumed and read a
  half-populated static set. `Init_CalledConcurrentlyFromTwoStaThreads_InvokesTheFactoryExactlyOnce`
  covers it.
- **The interface is minimal and satisfied without change.** `IUiCaptureSource` declares nine members;
  `SyncContextForm` gains only the declaration. Implementing directly rather than adding an adapter
  type is the correct simplicity-first call and is stated as such.
- **The predicate reads private fields rather than the properties, deliberately.** Reading
  `UiSyncContext` would call `Init()`, which now throws off the UI thread, and reading `Dispatcher`
  throws when unset. A boolean query must be total and side-effect-free. The code comment at 180-183
  also records why the dispatcher type is fully qualified inside the nested struct.
- **The `QfcHomeControllerRunAsyncTests` reconciliation removes an order dependency rather than
  papering over it.** The dispatcher is created on the pump thread first, then resolved with
  `Dispatcher.FromThread`, with a comment explaining that resolving it earlier would return null and
  `Install(null)` would leave the static unset. The `finally` disposes the transaction and stops the
  host on every path.

Observations, none blocking, none requiring rework before merge:

- **O1 — `Initialize()` runs while `InitLock` is held**, and `Initialize()` constructs and `Show()`s a
  WinForms form and may start a `ThreadMonitor`. Holding a lock across UI work is a deadlock shape in
  general. It is safe here because the lock is private, is taken on exactly one path, runs once per
  process on the host STA thread during startup, and nothing inside `Initialize()` waits on another
  thread that could want the lock. Worth a comment rather than a change.
- **O2 — `ResetForTesting()` drops `_syncContextForm` without hiding or disposing it.** With a real
  `SyncContextForm` that would leak a live hidden window. It is test-only and every consumer installs a
  fake, so nothing leaks today; a caller who forgot to install a factory first would be the exposure.
- **O3 — `UiThreadStateScope.Resolve` asserts inside a static field initializer.** A field rename
  surfaces as `TypeInitializationException` wrapping the FluentAssertions message rather than as a
  clean failure. The intent — fail loudly rather than degrade to a silent no-op restore — is right and
  is documented at `UiThreadStateScope.cs:344-350`; only the diagnostic shape is imperfect.
- **O4 — the predicate reads three non-volatile statics without synchronization** while `Init()` writes
  them under a lock. A stale read yields `false`, which posts, which is the conservative and correct
  fallback, so this is safe by construction rather than by accident. The sibling `Dispatcher` property
  documents the same concern at `UiThread.cs:255-257`.
- **O5 — `FakeUiCaptureSource.ConstructionCount` is process-global mutable state.** Documented as such,
  reset by each consuming test, and every consuming class is `[DoNotParallelize]`. The invocation-count
  assertion it supports is the right choice over any duration-based assertion.
- **O6 — file size.** `FolderPredictorTests.cs` is 1067 lines at head against a 500-line limit that
  admits no exception for test code (Finding F4 in the policy audit). The delivery adds one attribute
  line to it. Non-blocking; recommend a follow-up issue to split the file.
- **O7 — the two `Assert.IsTrue` calls retained in the amended `QuickFiler.Test` method** are
  pre-existing and were correctly left alone; converting them would have widened the diff for no gain.

## 8. Recommendations

1. Correct `MTA_INITIALIZE_OUTCOME` in `p0-t15-mta-synccontextform-measurement.md` and the Disposition A
   conclusion in `p6-t4-ac2-regression-reconciliation.md` to state that the apartment was not measured,
   and revert the status of the #782 narrative to UNKNOWN. The AC2 design argument stands unchanged and
   should be retained verbatim.
2. Correct the mechanism sentence in `p6-t13-closure-summary.md` section 6, keeping the operational
   advice.
3. Open one issue covering the branch-2 hardening in `IsCompleted` and the test that closes lines
   177-178, since the hardening changes what that test asserts.
4. Open one issue for splitting `FolderPredictorTests.cs` under the 500-line limit.
5. Promote the remaining follow-up candidates in `p6-t13` section 5 into real issues before the feature
   folder is merged, so they survive.

None of the five is a precondition for merge.
