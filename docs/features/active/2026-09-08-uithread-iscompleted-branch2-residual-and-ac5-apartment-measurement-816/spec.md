# 2026-09-08-uithread-iscompleted-branch2-residual-and-ac5-apartment-measurement (Spec)

- **Issue:** #816
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-09-12
- **Status:** Ready for planning
- **Version:** 1.0
- **Work Mode:** full-bug
- **Acceptance-criteria source:** this file only, per `acceptance-criteria-tracking`. The
  user-story document beside it is non-normative context and carries no acceptance criteria.
- **Research of record:**
  docs/features/active/2026-09-08-uithread-iscompleted-branch2-residual-and-ac5-apartment-measurement-816/research/2026-09-12T14-05-uithread-iscompleted-branch2-and-ac5-research.md

## Context

Issue #809 left two residuals in UtilitiesCS threading.

**Residual 1 — the unhardened `_uiSyncContext` exit of `SynchronizationContextAwaiter.IsCompleted`.**
The accessor occupies lines 155-190 of `UtilitiesCS/Threading/UiThread.cs` and has five exits. The
count of five is not restated here as a bare figure: it is derived twice, independently, in the
`## Numeric Derivation Evidence` section of the research of record, whose primary derivation reads
the accessor top to bottom and whose cross-check reconstructs the exit list from issue #809's
committed coverage and fail-before artifacts without rereading the accessor. In source order the
exits are the ambient-identity `true` at 160-163; the null-ambient `false` at 167-170; the
thread-id guard `false` at 171-174; the `_uiSyncContext` `true` at 176-179, which the issue text
calls "branch 2"; and the dispatcher expression at 184-188. Only the last of those carries a second,
independent proof of thread identity —
`ReferenceEquals(System.Windows.Threading.Dispatcher.FromThread(Thread.CurrentThread), _dispatcher)`.
The `_uiSyncContext` exit carries none, so for that one context it reduces to a bare
owning-thread-identity predicate, which is the predicate shape this work is forbidden to produce.

**Residual 2 — issue #809's AC5 merged unchecked.** Its apartment-state measurement was never
taken. The earlier probe inferred the apartment of its own thread from a research premise that the
same delivery falsified, so its conclusion was withdrawn and the status of the issue #782 latch
scenario on this host is recorded as unknown rather than as negative.

Environment:

- OS/version: Windows 11 Pro 10.0.26200, .NET Framework 4.8
- Test framework: MSTest, Moq, FluentAssertions per the repository instruction file CLAUDE.md
- Python version: not applicable. This is a C# change; no Python tooling participates.
- Data source or fixture: no live Outlook host is required at any point.

Impact / Severity:

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

Medium. Merged work carries one unmet acceptance criterion, and an asymmetric contract between two
exits of the same predicate is the shape that produced issues 784, 787 and 788.

## Repro & Evidence

1. Read lines 155-190 of `UtilitiesCS/Threading/UiThread.cs`. The exit at 176-179 returns `true`
   on a reference match against `_uiSyncContext` with no second proof of thread identity, while the
   exit at 184-188 requires both a type test and a live dispatcher-identity re-verification.
2. Read line 18 of UtilitiesCS.Test/Properties/AssemblyInfo.cs and confirm it carries the
   repository's only assembly-level `Parallelize` attribute. The research of record confirms this by
   a repository-wide search and qualifies it: it is the only such *attribute*, but
   TaskMaster.runsettings and scripts/vscode/TaskMaster.cli.runsettings each carry a
   runsettings-level parallelization directive that applies to every assembly in a run whenever a
   settings file is passed, so the attribute alone does not determine which thread a test lands on.
3. Search the nine .runsettings files in the tree for `ExecutionThreadApartmentState`. There are no
   hits in any of them. The claim in the issue document is confirmed.
4. Read issue #809's probe artifact and confirm it never calls
   `Thread.CurrentThread.GetApartmentState()`, and that a correction section withdrawing its
   conclusion was committed to it.
5. Read line 464 of
   `docs/features/active/2026-09-07-uithread-init-contract-residuals-784-787-788-809/spec.md` and
   confirm AC5 is unchecked.

Expected: every exit of `IsCompleted` that can return `true` after the thread-id guard carries a
second, independent proof that the caller stands on the captured UI thread; and issue #809's AC5
rests on a value read from the executing thread at runtime rather than on an inference.

Actual: the 176-179 exit carries no second proof, and no measurement exists.

Logs / screenshots: not applicable. The defect is established by reading the current tree and the
committed evidence artifacts of issue #809, both of which the research of record cites by line.

## Scope & Non-Goals

In scope:

- Hardening the 176-179 exit of `IsCompleted` so that the recycled-managed-thread-id leg fails
  closed.
- Two deterministic negative regression tests for that leg and one positive twin.
- Tightening the apartment-blind assertion at line 318 of
  `UtilitiesCS.Test/Threading/UiThreadInitContract_Tests.cs` and adding an explicit apartment
  assertion to the same test.
- A runtime apartment-state measurement that discharges clause (i) of issue #809's AC5.
- A repetition run that discharges clause (ii) of issue #809's AC5.
- Recording the two separate findings on issue #782.
- The full four-step C# toolchain with coverage.

Out of scope, stated in plain prose so that no excluded path is harvested as part of the change
footprint:

- **The WPF-dispatcher-operation leg is out of scope for this issue.** When a caller is genuinely
  on the captured UI thread inside a WPF dispatcher operation, WPF has installed a throwaway
  DispatcherSynchronizationContext as the ambient context. On that leg
  `Dispatcher.FromThread(Thread.CurrentThread)` is the captured dispatcher, so the conjunct this
  change adds evaluates true and the 176-179 exit still returns `true`, exactly as it does today.
  The hardening therefore closes the recycled-managed-thread-id leg and only that leg. It does not
  and must not change behaviour on the dispatcher-operation leg, because the 184-188 exit already
  returns `true` for a dispatcher context in that same state and the two exits would otherwise
  disagree in the opposite direction.
- Ordering assertions at the eleven production await sites issue #809 enumerated. No test asserts
  ordering at any of them today. That residual is restated here and carried forward unclosed; the
  hardening does not change behaviour on the leg those sites take today.
- The IsCurrentBoundary method of QuickFiler/Viewers/BreadcrumbUiDispatcher.cs. The research of
  record corrects the issue text on this point: that file is not a call site of the awaiter. It uses
  its own boundary predicate and posts directly, so no change to IsCompleted can affect it. It is
  cited only as the precedent that records the rule.
- The alternative hardening that hoists the dispatcher-identity check above both post-guard exits.
  It is structurally cleaner but changes the shape of a guard three existing tests were written
  against, with no additional behavioural gain. Recorded and rejected.
- Reverting UtilitiesCS/Threading/UiThread.cs to its pre-fix state in order to take the
  measurement. Issue #809's correction asserted this was required. It is not: SyncContextForm is a
  public Form-derived type compiled into the UtilitiesCS assembly, so the test assembly can
  construct it directly without touching UiThread at all.
- The UtilitiesCS production project file. This change adds no production source file, so no
  `<Compile Include>` item is needed there and the file is not touched.
- The injectable dispatcher seam conversion, which belongs to issue #584.
- Any change to UiThread.Init or its retry behaviour. The research of record establishes that the
  issue #782 retry behaviour is already present in production; see the Root Cause Analysis below.

Concurrency note, recorded for merge-risk awareness only: a sibling work item in the same run
concurrently edits both project files and the ProgressPackage and ProgressTrackerAsync sources. No
coordination with that sibling is to be attempted. The research of record assessed the merge risk as
low: this issue's only project-file edit is a single insertion in the UtilitiesCS.Test project file
sixteen lines away from the sibling's only deletion in the same item group, and this issue touches
the UtilitiesCS production project file not at all.

## Root Cause Analysis

### The asymmetry

The 184-188 exit requires two facts beyond the thread-id guard: that the captured context is a
DispatcherSynchronizationContext, and that a second, independent mechanism agrees the caller stands
on the captured UI thread. `Dispatcher.FromThread` is a per-thread runtime lookup that does not
consult `_uiThreadId`, so when the managed-thread-id comparison is a false positive that exit fails
closed.

The 176-179 exit has no second mechanism. Its only guards are the null-ambient check and the
managed-thread-id comparison. Once the captured context happens to be the persistent
`_uiSyncContext`, the exit is a bare owning-thread-identity predicate for that context.

### The invariant, in one sentence

An exit of `IsCompleted` may return `true` only when two independent mechanisms agree that the
caller stands on the captured UI thread; a managed-thread-id match alone is never sufficient,
because the CLR reuses managed thread ids after a thread dies.

### Trace of one accepted value through the boundary

Take a continuation resumed after `ConfigureAwait(false)` onto a thread-pool thread whose managed
id equals `_uiThreadId`, carrying some non-null ambient context, with the awaited context being the
persistent `_uiSyncContext`.

- Today: the ambient-identity exit is false; the null-ambient exit is false; the thread-id guard
  passes because the ids match; the 176-179 exit matches by reference and returns `true`. The
  compiler-generated await then calls `GetResult()` on the current stack and runs the continuation
  synchronously. `OnCompleted`, and therefore `_context.Post`, is never invoked, so UI work runs on
  a non-UI thread with no marshalling at all, and already-queued work runs after the inline
  continuation instead of before it.
- After the change: the same value reaches the 176-179 exit, the reference match still succeeds, and
  the added conjunct then asks whether this thread's WPF dispatcher is the captured UI dispatcher.
  A pool thread has no WPF dispatcher, so the lookup returns null, the conjunct is false, control
  falls through to the 184-188 exit, the type test fails for a WindowsFormsSynchronizationContext,
  and the accessor returns `false`. The await posts through `_context` as intended.

### A fail-open case the conjunct alone does not close

If the captured dispatcher field is null and the lookup on the current thread also returns null, a
bare reference comparison of the two matches null against null and the exit returns `true` again,
on the very thread shape the change exists to reject. The hardened exit must therefore fail closed
when no UI dispatcher was captured. This requirement is derived here and is not stated by the
research of record; it is recorded explicitly because without it the hardening is satisfiable
vacuously and the negative regression test could pass for the wrong reason. Production reaches the
null-dispatcher state only through the test reset path, since Initialize captures all four values
together or none, but the guard costs one token and removes the ambiguity.

### What must not be relaxed

The predicate constraint from issue #809 still applies. Replacing any reference comparison with an
owning-thread-identity test is prohibited. Two real consumers of this awaiter live in
QuickFiler.Test/TestSupport/WinFormsPumpHostTests.cs, at the await of the pump host's context
inside AwaitingSyncContext_FromTheTestThread_ResumesOnThePumpThread and inside
BothMarshalRoutes_WpfDispatcherAndSyncContext_ExecuteOnThePumpThread. Under a bare-id predicate
those tests would complete inline on the MSTest thread and their assertions would fail. The
in-repository guard for that shape is
`IsCompleted_WithAForeignWindowsFormsContextWhileUiThreadIdMatches_ReturnsFalse`, which must
continue to pass unchanged.

### Findings on issue #782

- **Finding 4A — production behaviour is present, not a residual.** Lines 47-59 of
  `UtilitiesCS/Threading/UiThread.cs` set the initialization flag after `Initialize()` returns,
  inside `lock (InitLock)`, so a failed first attempt leaves the flag false and a later call from an
  STA thread retries, and concurrent first attempts are serialized. The pre-fix shape consumed an
  `Interlocked.Exchange` latch before `Initialize()` ran. No production change to this code is in
  scope.
- **Finding 4B — the test coverage is apartment-dependent and was never measured.** The retry test
  is `Init_WhenFirstInitializeThrows_SecondInitWithWorkingFactorySucceedsAndPopulatesAllFourCaptureFields`
  in `UtilitiesCS.Test/Threading/UiThreadInitContract_Tests.cs`, whose class carries
  `[STATestClass]` and `[DoNotParallelize]`. The test calls `UiThread.Init()` directly on the test
  method's own thread. If that thread were MTA, the apartment precondition would throw before
  `Initialize()` ran and the retry assertion would fail, so the outcome depends on an apartment the
  test never measures. Its first assertion at line 318 is
  `failing.Should().Throw<InvalidOperationException>();` with no message constraint, and the
  non-STA rejection throws the same exception type as the capture failure the fake produces, so
  under MTA that assertion would pass for the wrong reason. Its sibling at lines 352-354 does
  constrain the message. Whether `[STATestClass]` forces STA for plain `[TestMethod]` members on the
  pinned MSTest version is not established from the tree, and the in-tree record is explicitly
  contradictory on the mechanism; the surviving operational rule is that a test needing a caller of
  a known apartment must create a dedicated thread and set the apartment explicitly.

### Why the hardening and its tests ship together

The hardening changes what a test of the 176-179 exit must assert, so splitting them produces an
intermediate state in which neither can be satisfied.

## Proposed Fix

### Design summary

Conjoin a live dispatcher-identity re-verification into the 176-179 exit of `IsCompleted` in
`UtilitiesCS/Threading/UiThread.cs`, so that the exit returns `true` only when the captured context
is the persistent UI context **and** a non-null captured UI dispatcher is reference-equal to the
dispatcher of the executing thread. Add three deterministic predicate tests, tighten one existing
assertion, add one runtime apartment measurement, and record the results as Markdown evidence.

### Boundaries and invariants to preserve

- The accessor keeps exactly five exits, in the same source order, and no exit other than 176-179
  changes its condition.
- Reference comparison remains the mechanism at every exit. No owning-thread-identity substitution.
- Behaviour on the dispatcher-operation leg is unchanged, as stated in Scope & Non-Goals.
- The four `UiThread` statics the tests install are process-global. Every test class that touches
  them carries `[DoNotParallelize]` and restores prior values through the existing install scope,
  which is documented as not internally synchronized.
- No test relies on the apartment of the ambient MSTest worker. Every new test that needs a known
  apartment creates a dedicated thread and sets the apartment explicitly.
- No temporary file is created anywhere, per CLAUDE.md.

### Implementation strategy

Files to change, and the role of each:

- `UtilitiesCS/Threading/UiThread.cs` — the 176-179 condition and its explanatory comment.
- `UtilitiesCS.Test/Threading/UiThread_Tests.cs` — one added test, the positive twin, placed inside
  `SynchronizationContextAwaiter_Tests` beside the other `IsCompleted` cases. That class already
  owns a private STA dispatcher host, so the twin needs no new fixture. The file is 458 lines, so
  the 500-line limit leaves room for one test of about thirty lines and no more.
- `UtilitiesCS.Test/Threading/UiThreadApartmentMeasurement_Tests.cs` — a new file. It hosts two
  `[TestClass]` types: the two negative predicate cases, and the apartment measurement. Both are
  `[DoNotParallelize]`. Two classes share one file because the write set permits exactly one new
  test file and neither existing file has the headroom: the awaiter test file has 42 lines to the
  limit and the init-contract test file has 40. Both negative cases and the measurement need
  more than that. The file may reuse, without any edit, two internal members already declared in
  namespace `UtilitiesCS.Test.Threading`: the shared STA dispatcher host, and the apartment thread
  runner described below.
- `UtilitiesCS.Test/UtilitiesCS.Test.csproj` — one `<Compile Include>` item for the new file. The
  project is not SDK-style, so a file without this item compiles into nothing and its tests
  silently do not exist. The Threading item block is not alphabetically ordered; the governing
  convention is topical adjacency, so the item goes immediately after the existing
  UiThreadInitContract_Tests item and before the WpfUiDispatcherTests item.
- `UtilitiesCS.Test/Threading/UiThreadInitContract_Tests.cs` — two in-file edits to the retry test:
  an apartment assertion as the first Arrange step, and a message constraint on the assertion at
  line 318.
- `docs/features/active/2026-09-07-uithread-init-contract-residuals-784-787-788-809/spec.md` —
  the AC5 checkbox at line 464, and only under the condition stated in AC11 below.
- The three requirement documents in this feature folder, as listed in the Write Set below.

### Test construction, stated precisely because determinism depends on it

The only helper in the repository that can host work on both an STA and an MTA thread is the
apartment thread runner declared at lines 85-104 of
`UtilitiesCS.Test/Threading/UiThreadInitContract_Tests.cs`. It starts a dedicated background
thread, sets the caller-supplied apartment, joins, and returns the thrown exception or null. It is
`internal` in the same namespace, so no new seam is required. Because it returns only an exception,
any measured value must be captured into a closure variable rather than returned.

The installer for the persistent UI context is the `SetUiSyncContext` member of the existing install
scope in UtilitiesCS.Test/TestHelpers/UiThreadStateScope.cs. It currently has zero callers anywhere
in the repository, which is why the 176-179 exit is uncovered today.

- **Negative case 1, the recycled-id leg.** Inside the apartment thread runner on a dedicated
  thread, install the captured dispatcher as a live STA host dispatcher owned by a different
  thread, install the captured thread id as the dedicated thread's own managed id, install the
  captured UI context as a context instance, set the ambient context to a different non-null
  instance, and assert the predicate is `false`. This is red on today's code, because the reference
  match alone returns `true`, and green after the change, because the dispatcher lookup on a
  dedicated thread returns null and cannot be reference-equal to the host dispatcher. It is
  constructed, not raced: no thread-id recycling is required, because installing the captured id
  reproduces the same false-positive state deterministically.
- **Negative case 2, the null-dispatcher fail-closed leg.** Same shape, with no captured dispatcher
  installed and a thread that has no dispatcher of its own, asserting `false`. This is the case
  that forbids a null-to-null reference match from satisfying the conjunct.
- **Positive twin.** Install the captured dispatcher as the STA host's dispatcher, evaluate the
  predicate inside an invoke on that host's own thread with the captured thread id installed as
  that thread's id, the captured UI context installed, and a different non-null ambient context set,
  and assert `true`. This must pass both before and after the change, and it is the case that closes
  the coverage gap on the 176-179 exit.
- **Apartment measurement.** Inside the apartment thread runner with MTA requested, read
  `Thread.CurrentThread.GetApartmentState()` on the executing thread and capture it as the guard
  value, then construct `SyncContextForm` and call `Show()`, disposing the form in a `finally`. The
  runner returns the thrown exception or null, which is the settling value. The measurement may not
  be inferred from a settings file, from the `Parallelize` attribute, or from documented
  `[STATestClass]` behaviour; only the value read on the executing thread counts. The structural
  guard `ExecutingAssembly_ContainsNoFormDerivedType` is not violated, because it reflects over the
  types compiled into the test assembly and `SyncContextForm` is declared in UtilitiesCS.

### Error handling and logging updates

None. No production logging or exception path changes. The added conjunct only narrows a predicate.

### Backward compatibility

The public signature of the awaiter is unchanged. The only observable difference is that the
176-179 exit now returns `false` on a thread that is not the dispatcher-owning thread, which is the
defect being fixed.

### Performance constraints

The added conjunct is one per-thread lookup evaluated only after the ambient, null and thread-id
checks have already passed. No throughput or latency target applies and none is asserted.

## Assumptions, Constraints, Dependencies

- Assumptions: the execution host has a WPF dispatcher available to a dedicated STA thread, which
  every existing `UiThread` test already relies on; no live Outlook host is required or used.
- Constraints: MSTest, Moq and FluentAssertions only; no temporary files; no test may depend on the
  ambient worker's apartment; no C# source file may exceed 500 lines.
- Coverage policy: CLAUDE.md governs, which is 80% repository-wide line coverage and 90% for
  newly added members. The 85% line and 75% branch figures and the T1-T4 tier system that appear in
  the rule files under the .claude directory are reference-repository leakage: the quality-tiers.yml
  file those rules name as their source of truth does not exist in this repository. CLAUDE.md's
  figures are the ones used throughout this specification, per the policy reading order.
- Evidence convention, effective now per the maintainer decision on issue #671 of 2026-09-11:
  commit projections only. Numeric coverage and test figures are transcribed into Markdown evidence
  artifacts and the raw tool output is written outside the repository working tree and discarded.
  No raw test-result or coverage document is committed or named in this specification. Evidence
  paths resolve under this feature folder's own evidence subtree, in the canonical
  evidence/<kind>/ layout defined by `evidence-and-timestamp-conventions`. Those paths are written
  in plain prose without backticks so that they are not harvested into the change footprint.
- Test-host hazard: the run must exclude the four shell-icon test classes the research of record
  enumerates, whose count of four is derived twice in that document's Claim N2 block. The
  exclusion is required not because the stall still reproduces, which it does not, but because two
  consecutive probe runs each produced one non-deterministic failure with an invalid-Win32-handle
  diagnostic. Any artifact that applies the exclusion must state that reason accurately. The
  positive and negative selectors may not be combined with an explicit test list, because the two
  vstest arguments are mutually exclusive.
- Dependency on issue #809: this delivery edits issue #809's specification file to settle its AC5,
  under the strict condition in AC11.

## Data / API / Config Impact

- User-facing or API changes: none.
- Data or migration considerations: none.
- Logging/telemetry updates: none.
- Compatibility notes: no configuration key, settings file, or command-line flag changes. No
  settings file is modified, and no apartment setting is introduced into one; the measurement is
  taken at runtime precisely so that no static configuration is relied upon.

## Test Strategy

- **Regression tests to add:** two negative predicate cases and one positive twin, as constructed
  above. The negative cases are the falsifiable pair for this change: they are red on the current
  predicate and green after it. The positive twin is the invariance control.
- **Fail-before and pass-after:** record both as Markdown projections under this feature folder's
  evidence/regression-testing directory, each naming the test, the recorded outcome, and the
  command and exit code. The apartment measurement has no fail-before and cannot have one, because
  it is a measurement obligation rather than a gate; record a fail-before exception dossier for it
  with an explicit statement of why a failing run is impossible.
- **Existing tests that must not regress:** every test method in `SynchronizationContextAwaiter_Tests`
  in `UtilitiesCS.Test/Threading/UiThread_Tests.cs`, the two pump-host consumers in
  QuickFiler.Test, and both retry-contract tests in
  `UtilitiesCS.Test/Threading/UiThreadInitContract_Tests.cs`.
- **Edge and negative scenarios:** null captured dispatcher; captured thread id unset; ambient
  context null; a foreign WindowsFormsSynchronizationContext while the thread id matches. The last
  three already have tests and must keep passing.
- **Determinism:** no wall-clock wait, no sleep, no retry loop, and no reliance on thread-id
  recycling actually occurring. Every thread the new tests use is created explicitly with an
  explicit apartment and joined before the test returns.
- **Coverage:** produce coverage with the repository's fourth toolchain command and transcribe the
  figures into a Markdown projection under this feature folder's evidence/qa-gates directory.
- **Toolchain:** the four commands in CLAUDE.md, in order, restarting from the first on any
  failure or auto-fix.
- **Manual validation:** none required. No live host scenario is part of any acceptance criterion.

## Acceptance Criteria

- [ ] **AC1 — The 176-179 exit is hardened and nothing else in the accessor changes.** After the
      change, `SynchronizationContextAwaiter.IsCompleted` in `UtilitiesCS/Threading/UiThread.cs`
      still has exactly five exits in the same source order — the count and its two independent
      derivations are in the `## Numeric Derivation Evidence` section of the research of record, and
      that section is cited rather than restated — and the only exit whose condition text differs
      from the pre-change source is the `_uiSyncContext` exit, which now also requires a non-null
      captured UI dispatcher that is reference-equal to the dispatcher of the executing thread.
      PASS requires a diff of that file confined to the `_uiSyncContext` condition and its comment.
      FAIL if the diff alters the ambient-identity exit, the null-ambient exit, the thread-id guard,
      the dispatcher exit, or `UiThread.Init`.
- [ ] **AC2 — The recycled-id negative regression test is red before and green after.** A test in
      `UtilitiesCS.Test/Threading/UiThreadApartmentMeasurement_Tests.cs` installs the captured UI
      context, installs a captured dispatcher owned by a different thread, installs the captured
      thread id as the executing dedicated thread's own managed id, sets a non-null ambient context
      distinct from the captured one, and asserts the predicate is false. A Markdown projection
      under this feature folder's evidence/regression-testing directory records that this named test
      reported Failed against the unmodified predicate and Passed against the modified one, with the
      command and exit code for each run. FAIL if either recorded outcome is absent, if the
      fail-before outcome is Passed, or if the test is skipped or not discovered.
- [ ] **AC3 — The null-dispatcher fail-closed negative test passes.** A second test in the same new
      file asserts the predicate is false when the captured UI context is installed, the captured
      thread id matches the executing thread, no captured UI dispatcher is installed, and the
      executing thread has no dispatcher of its own. It is recorded Passed in the pass-after
      projection. FAIL if the implementation satisfies the added conjunct by a null-to-null
      reference match, which this test detects.
- [ ] **AC4 — The positive twin passes both before and after.** A test named for the
      dispatcher-owning-caller case, added inside `SynchronizationContextAwaiter_Tests` in
      `UtilitiesCS.Test/Threading/UiThread_Tests.cs`, asserts the predicate is true when evaluated
      on the thread that owns the captured dispatcher with the captured UI context installed and a
      different non-null ambient context set. The same projection records it Passed against both the
      unmodified and the modified predicate. FAIL if either outcome is not Passed, because that
      would mean the change altered behaviour on the leg declared out of scope.
- [ ] **AC5 — The weak retry assertion is tightened and discriminates.** In
      `UtilitiesCS.Test/Threading/UiThreadInitContract_Tests.cs`, the assertion at line 318 of the
      pre-change file gains `.WithMessage(FakeUiCaptureSource.CaptureFailureMessage)`, and the same
      test gains `Thread.CurrentThread.GetApartmentState().Should().Be(ApartmentState.STA);` as its
      first Arrange step. Both
      `Init_WhenFirstInitializeThrows_SecondInitWithWorkingFactorySucceedsAndPopulatesAllFourCaptureFields`
      and `Init_WhenInitializeThrows_LeavesAllFourCaptureFieldsUnset` are recorded Passed in the
      pass-after projection. The projection also records that the non-STA rejection message is
      produced by the `NonStaInitMessage` helper and is not equal to the capture-failure message, so
      the constraint distinguishes the two `InvalidOperationException` sources. FAIL if the message
      constraint is absent, if the apartment assertion is absent, or if the projection does not
      record the message comparison.
- [ ] **AC6 — The runtime apartment measurement is taken and settles clause (i) of issue #809's
      AC5.** A test in `UtilitiesCS.Test/Threading/UiThreadApartmentMeasurement_Tests.cs` runs on a
      dedicated thread whose apartment was set to MTA explicitly, reads
      `Thread.CurrentThread.GetApartmentState()` on that thread inside the delegate, constructs
      `SyncContextForm`, calls `Show()`, and disposes the form in a `finally`. A Markdown artifact
      under this feature folder's evidence/other directory records a guard line whose value is the
      apartment read on the executing thread, and a settling line in the token form issue #809's
      plan already fixed, namely `MTA_INITIALIZE_OUTCOME: COMPLETED` or
      `MTA_INITIALIZE_OUTCOME: THREW` with the exception type and message on the `THREW` branch. The
      artifact also carries the timestamp, the command, and the exit code. PASS requires the guard
      value to be MTA and the settling line to be present. FAIL if the guard value is anything other
      than MTA, because the probe then measured nothing and the run is void; FAIL if the apartment
      is asserted from a settings file, from the `Parallelize` attribute, or from documented
      `[STATestClass]` behaviour instead of being read on the executing thread.
- [ ] **AC7 — The measurement leaves nothing behind and needs no host.** The measurement test
      disposes the form it constructs, creates no dispatcher and starts no message loop, joins the
      thread it creates before returning, and requires no live Outlook process. PASS requires the
      structural guard `ExecutingAssembly_ContainsNoFormDerivedType` to be recorded Passed in the
      same run, proving no Form-derived type was compiled into the test assembly, and the run to
      complete with no orphaned thread and no hang dump. FAIL if the run leaves a visible window, a
      live form, an un-shut dispatcher, or a non-background thread alive.
- [ ] **AC8 — Both issue #782 findings are recorded, and neither changes production.** A Markdown
      artifact under this feature folder's evidence/other directory records finding 4A, that the
      retry-after-failed-initialize behaviour is already present in production at lines 47-59 of
      `UtilitiesCS/Threading/UiThread.cs` where the flag is set after `Initialize()` returns inside
      `lock (InitLock)`, and finding 4B, that a retry test exists and passes but its apartment
      premise was unmeasured and its first assertion could not distinguish the two
      `InvalidOperationException` sources. The artifact states which of the two findings this
      delivery acts on, namely 4B only. PASS requires both findings to be named with their
      supporting line ranges and requires the AC1 diff to show `UiThread.Init` unchanged. FAIL if
      the artifact presents the issue #782 status as a single finding or asserts a production
      residual that the research of record contradicts.
- [ ] **AC9 — Clause (ii) of issue #809's AC5 is discharged or explicitly left outstanding.** A
      Markdown projection under this feature folder's evidence/regression-testing directory records
      a full-suite run over the explicitly named test assemblies, repeated at least three times,
      with the per-repetition outcome of
      `UtilitiesCS.Test.Extensions.DictionaryExtensions_Tests.TryAddValuesAsync_UpdatesExistingValue`
      listed for every repetition, together with the command, the exit code, whether a settings file
      was passed, and the shell-icon exclusion with its accurate stated reason. PASS requires at
      least three recorded repetitions and either every recorded outcome being Passed, or each
      recorded failure carrying a written attribution showing it is not caused by this delivery —
      for example that the failure reproduces on the unmodified base and that this delivery changes
      none of the code the failing test exercises. FAIL if fewer than three repetitions are
      recorded, if the per-repetition outcome of that test is omitted, or if a failure is recorded
      with no attribution.
- [ ] **AC10 — Coverage of the hardened exit and the repository floor.** A Markdown projection under
      this feature folder's evidence/qa-gates directory records line coverage for
      `UtilitiesCS/Threading/UiThread.cs` at or above the 80% floor in CLAUDE.md, records the
      executable lines of the hardened `_uiSyncContext` exit as covered — identified by their
      content rather than by the pre-change line numbers, which the added conjunct shifts — and
      records that no line changed by this delivery lost coverage. It also states that this delivery
      adds no new production member, so the 90% floor for newly added members has an empty
      denominator on the production side and the applicable production gate is the 80% line floor
      together with coverage of the hardened exit. FAIL if the projection omits the per-file figure,
      if the figure is below 80%, or if any executable line of the hardened exit is recorded
      uncovered.
- [ ] **AC11 — Issue #809's AC5 is checked off only on a complete discharge.** The checkbox at line
      464 of
      `docs/features/active/2026-09-07-uithread-init-contract-residuals-784-787-788-809/spec.md`
      changes from `- [ ]` to `- [x]` if and only if AC6 and AC9 are both PASS. Its criterion text
      is not modified. If either AC6 or AC9 is not PASS, the checkbox remains `- [ ]` and this
      delivery commits a note recording which clause of that criterion remains outstanding —
      clause (i), the measurement, or clause (ii), the three-repetition record — and why. A partial
      discharge never checks the box. That criterion names its own feature folder's evidence/other
      directory as the location of the measurement artifact, so the artifact is committed there as
      well as under this feature folder, at
      `docs/features/active/2026-09-07-uithread-init-contract-residuals-784-787-788-809/evidence/other/ac05-mta-initialize-measurement.md`.
      The two copies carry identical measured values. Writing into that folder does not breach the
      evidence-location invariant, which constrains the shape of an evidence path to a feature
      folder's own evidence subtree and does not restrict which feature folder is written to. FAIL
      if the box is checked while either criterion is short, if the criterion text is edited, if a
      partial discharge is committed without the outstanding-clause note, or if the artifact named
      by that criterion is absent from the folder the criterion names.
- [ ] **AC12 — The new file is registered and its tests actually ran.**
      `UtilitiesCS.Test/UtilitiesCS.Test.csproj` gains a `<Compile Include>` item for
      `UtilitiesCS.Test/Threading/UiThreadApartmentMeasurement_Tests.cs`, inserted between the
      existing UiThreadInitContract_Tests item and the WpfUiDispatcherTests item, both of which are
      named there by their project-relative form. PASS requires the pass-after projection to list the
      new tests by name among the executed tests, which is the only check that proves the file
      compiled into the assembly rather than silently producing nothing. FAIL if the tests are
      absent from the executed list, even if the item appears in the project file.
- [ ] **AC13 — File-size limit respected.** Each C# source file this delivery touches remains at or
      below the 500-line limit in CLAUDE.md. A projection records the post-change line count of
      `UtilitiesCS/Threading/UiThread.cs`, `UtilitiesCS.Test/Threading/UiThread_Tests.cs`,
      `UtilitiesCS.Test/Threading/UiThreadInitContract_Tests.cs` and
      `UtilitiesCS.Test/Threading/UiThreadApartmentMeasurement_Tests.cs`. The pre-change counts are
      293, 458 and 460 for the first three, so the headroom is 207, 42 and 40 lines respectively.
      FAIL if any recorded count exceeds 500.
- [ ] **AC14 — Full four-step toolchain passes in one final pass.** All four CLAUDE.md commands
      run in order in a single clean pass: `dotnet tool run csharpier format .` verified by
      `dotnet tool run csharpier check .`;
      `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`;
      `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`;
      and `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`. A projection under this
      feature folder's evidence/qa-gates directory records each command verbatim, its exit code, and
      for both msbuild commands a non-vacuity assertion that the log contains zero occurrences of
      the skipped-CoreCompile message. FAIL if any recorded exit code is non-zero, if the format
      check reports a diff, if either msbuild log shows a skipped compile, or if the last recorded
      formatting step modified a file without a subsequent restart from step one.

## Write Set

`UtilitiesCS/Threading/UiThread.cs`
`UtilitiesCS.Test/Threading/UiThread_Tests.cs`
`UtilitiesCS.Test/Threading/UiThreadInitContract_Tests.cs`
`UtilitiesCS.Test/Threading/UiThreadApartmentMeasurement_Tests.cs`
`UtilitiesCS.Test/UtilitiesCS.Test.csproj`
`docs/features/active/2026-09-08-uithread-iscompleted-branch2-residual-and-ac5-apartment-measurement-816/issue.md`
`docs/features/active/2026-09-08-uithread-iscompleted-branch2-residual-and-ac5-apartment-measurement-816/spec.md`
`docs/features/active/2026-09-08-uithread-iscompleted-branch2-residual-and-ac5-apartment-measurement-816/user-story.md`
`docs/features/active/2026-09-07-uithread-init-contract-residuals-784-787-788-809/spec.md`
`docs/features/active/2026-09-07-uithread-init-contract-residuals-784-787-788-809/evidence/other/ac05-mta-initialize-measurement.md`
`docs/features/active/2026-09-08-uithread-iscompleted-branch2-residual-and-ac5-apartment-measurement-816/plan.2026-09-12T13-23.md`

## Risks & Mitigations

- **The conjunct could be satisfied vacuously.** If the captured dispatcher is null and the lookup
  on the executing thread also returns null, a bare reference comparison matches null against null.
  Mitigation: the fail-closed requirement in the Root Cause Analysis and its dedicated test in AC3.
- **The measurement could be void without anyone noticing.** A probe that runs on an STA thread
  measures nothing about MTA behaviour. Mitigation: AC6 makes the guard value a PASS condition and
  requires the run to be declared void if it is not MTA. This is the precise failure of the earlier
  probe.
- **Clause (ii) of issue #809's AC5 depends on a test with known non-determinism.** Mitigation:
  AC9 permits a recorded failure only with a written attribution, and AC11 forbids checking the box
  on a partial discharge.
- **Process-global static state.** Every new test class installs `UiThread` statics that are shared
  by the whole assembly run. Mitigation: `[DoNotParallelize]` on every new class and the existing
  install scope's unconditional restore, including restoring a captured null.
- **Legacy project files silently drop files.** Mitigation: AC12 verifies registration by the
  executed-test list rather than by the presence of the project item.
- **Constructing a Form in a unit test.** Mitigation: the form is declared in the production
  assembly, is disposed in a `finally`, and AC7 requires the structural no-Form guard to pass in the
  same run. The alternative of a throwaway probe removed within the task was considered and
  rejected because it trades repeatability for hygiene.
- **File-size headroom is thin.** The awaiter test file has 42 lines to the limit. Mitigation: only
  the positive twin goes there; AC13 records the counts.
- **Concurrent sibling edit to the test project file.** Mitigation: none attempted by design. The
  merge risk is low, as recorded in Scope & Non-Goals.

## Rollout & Follow-up

- Release: no rollout step. The change is internal to a predicate and to test code, and no
  configuration or manifest changes.
- Restated residual, carried forward and not closed here: the eleven production await sites issue
  #809 enumerated still have no ordering assertions. This delivery does not change behaviour on the
  leg those sites take today, and the residual is restated in this issue's closure rather than
  silently inherited.
- Open question the tree cannot settle, recorded for a future issue rather than resolved here:
  whether the pinned MSTest version's `[STATestClass]` forces STA for plain `[TestMethod]` members,
  or whether previously observed STA came from the vstest main execution thread. The in-tree record
  is explicitly contradictory. The apartment assertion added by AC5 converts the assumption into a
  measured assertion for the one test that depended on it, which is the extent of what this issue
  claims.
- Open question, likewise not claimed here: whether a viewer's UI context and the captured
  `_uiSyncContext` are reference-equal on a live host. Code reading indicates yes; nothing in the
  repository asserts it, and no acceptance criterion above depends on it.
- Links: issue #816; issue #809 and its specification, edited per AC11; the research of record named
  in the header of this document.
