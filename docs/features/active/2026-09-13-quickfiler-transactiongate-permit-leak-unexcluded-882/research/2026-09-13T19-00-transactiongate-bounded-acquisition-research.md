# Research — TransactionGate permit leak / bounded acquisition (Issue #882)

- **Issue:** #882
- **Feature folder:** `docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/`
- **Worktree:** `bugs-2026-09-11-item-882`, branch `bug/quickfiler-transactiongate-permit-leak-unexcluded-882`, base `origin/main` `e6d86049e`
- **Timestamp:** 2026-09-13T19-00
- **Scope:** research only. No build, test, msbuild, vstest or dotnet command was run. No source file was modified.

> **Timestamp provenance.** The Bash tool is disabled in this session and no other clock-reading
> tool was available, so the wall-clock minute in the filename could not be read from the machine.
> The date component `2026-09-13` is the session date supplied in the task context and matches the
> feature folder's own `Last Updated: 2026-09-13T18-24`. The time component is an approximation
> placed after that recorded folder timestamp. It is recorded here as approximate rather than
> presented as a measured reading.

---

## 0. Current state of the subject

All line references below were read in this worktree.

`QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs` (278 lines) contains
two types in one file:

- `internal static class UiThreadDispatcherFixture` (`:29`)
- `internal sealed class UiThreadDispatcherTransaction : IDisposable` (`:220`)

The gate and its two endpoints:

| Element | Path : line | Text read |
|---|---|---|
| Gate declaration | `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs:32` | `private static readonly SemaphoreSlim TransactionGate = new SemaphoreSlim(1, 1);` |
| Acquisition | same file `:124` | `await TransactionGate.WaitAsync().ConfigureAwait(false);` |
| Release helper | same file `:88`–`:91` | `internal static void ReleaseTransactionGate()` -> `TransactionGate.Release();` |
| Sole caller of the release helper | same file `:275` | inside `UiThreadDispatcherTransaction.Dispose()` (`:261`) |
| Over-release hazard, already documented in-file | same file `:258`–`:259` | "Idempotent: a second call neither re-writes the static nor releases the gate again, because a second release on a `SemaphoreSlim(1, 1)` throws `SemaphoreFullException`." |

`BeginTransactionAsync` (`:122`–`:126`) is a two-statement method: acquire, then
`return new UiThreadDispatcherTransaction();`. The transaction object is the only thing in the
assembly that can reach `ReleaseTransactionGate`, and it is constructed unconditionally after the
acquisition completes. That coupling is what makes the fix shape in §2.3 available.

A second, separate lock (`FieldLock`, `:31`) guards the read-modify-write of the static and is not
in scope. The file's own class doc (`:18`–`:21`) records the lock ordering `TransactionGate` then
`FieldLock` and states no cycle exists.

`EnsureDispatcher` (`:99`) deliberately does **not** take `TransactionGate`; the file's doc at
`:23`–`:27` states the reason: its callers "live in test files that carry no `[Timeout]`, so making
them wait on a gate another test class holds for a whole test body would convert a bounded failure
elsewhere into an unbounded hang there." That reasoning is directly relevant to §4 below, because
two consumer classes of `BeginTransactionAsync` do in fact carry no `[Timeout]`.

---

## 1. MSTest abandonment semantics

### 1.1 The version this repository actually references

`QuickFiler.Test` uses `packages.config`, not `PackageReference`.

- `QuickFiler.Test/packages.config:123` — `<package id="MSTest.TestAdapter" version="4.4.0" targetFramework="net481" />`
- `QuickFiler.Test/packages.config:124` — `<package id="MSTest.TestFramework" version="4.4.0" targetFramework="net481" />`
- `QuickFiler.Test/packages.config:118` — `MSTest.Analyzers` 4.4.0 (developmentDependency)

Corroborated by the project file:

- `QuickFiler.Test/QuickFiler.Test.csproj:364`–`:365` — `Reference Include="MSTest.TestFramework, Version=4.4.0.0, ..."` with `HintPath` `..\packages\MSTest.TestFramework.4.4.0\lib\net462\MSTest.TestFramework.dll`
- `QuickFiler.Test/QuickFiler.Test.csproj:4` and `:534` — adapter props/targets imported from `..\packages\MSTest.TestAdapter.4.4.0\build\net462\`

So the framework in force is **MSTest 4.4.0 on `net481`, resolved against the `net462` lib folder**.

**Negative claim, with scope.** The `packages/` directory is not restored in this worktree: a Glob
for `packages/MSTest.TestFramework.4.4.0/**/*.xml` and a Glob for `packages/MSTest*/**`, both rooted
at the worktree, returned no files. I therefore could not read the shipped MSTest XML documentation
or assembly locally, and the behavioural evidence below is external.

### 1.2 What the runner does on expiry — the authoritative statement

Microsoft Learn, `TimeoutAttribute` class reference, page `defaultMoniker: mstest-net-4.4` and whose
"Package" list explicitly includes **`MSTest.TestFramework v4.4.0`** (so the page's default moniker
is the version this repository references). The `CooperativeCancellation` property remarks read, in
full:

> "Gets or sets a value indicating whether the test method should be cooperatively canceled on
> timeout. When set to `true`, the cancellation token is canceled on timeout, and the method
> completion is awaited. The test method and all the code it calls, must be designed in a way that
> it observes the cancellation and cancels cooperatively. If the test method does not complete, the
> timeout does not force it to complete. **When set to `false`, the cancellation token is canceled on
> timeout, timeout result is reported and the method task will continue running on background. This
> may lead to conflicts in file access on test cleanup, unobserved exceptions, and memory leaks.**"

(URL: `https://learn.microsoft.com/en-us/dotnet/api/microsoft.visualstudio.testtools.unittesting.timeoutattribute`)

The default is `false`. Microsoft Learn "Configure MSTest", `testconfig.json` `timeout` settings
table, states for `useCooperativeCancellation`:

> Default `false`. "When set to `true`, in case of timeout, MSTest will only trigger cancellation of
> the `CancellationToken` but will not stop observing the method. This behavior is more performant
> but relies on the user to correctly flow the token through all paths."

(URL: `https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-mstest-configure`)

**This repository is on the default.** A Grep for `CooperativeCancellation` (case-insensitive) across
the whole worktree returned **no files**. A Glob for `**/testconfig.json` across the whole worktree
returned **no files**. `TaskMaster.runsettings` (read in full, 30 lines) contains only
`MSTest/Parallelize` (`Workers` 0, `Scope` ClassLevel) and a `CodeCoverage` module-exclude list; it
sets no timeout key and no cancellation key. The six `[Timeout(GateTimeoutMs)]` attributes in the
subject's test file therefore run in **non-cooperative** mode.

### 1.3 Corroborating implementation detail, and its limits

The `microsoft/testfx` `main`-branch file
`src/Adapter/MSTestAdapter.PlatformServices/Services/ThreadOperations.cs` implements the
non-cooperative path two ways:

- `ExecuteWithThreadPool`: `var executionTask = Task.Run(action, cancellationToken); return executionTask.Wait(timeout, cancellationToken);` — on expiry `Wait` returns `false` and the method returns without observing the task; the task keeps running.
- `ExecuteWithCustomThread`: `executionThread.Join(timeout)` — on expiry the thread is neither aborted nor interrupted; it keeps running as a background thread.

**Limit on this evidence.** I fetched this from the repository's `main` branch, not from a tag pinned
to 4.4.0: attempts to fetch `src/Adapter/MSTest.TestAdapter/Execution/TestMethodInfo.cs` at both
`main` and at commit `79f26ff23fac8769c84783fe2fa0cfc20134bcf8` (the commit the Learn page cites for
the 4.4 `TimeoutAttribute.cs` source) both returned HTTP 404, so the file layout has changed and I
could not read the version-pinned async-specific path. Treat §1.3 as corroboration of §1.2, not as
independent version-pinned evidence. The §1.2 quote is version-scoped and is the load-bearing one.

### 1.4 Consequence for H-LEAK — and a correction to the issue's wording

`issue.md:28` states: "MSTest abandons a timed-out `async` test rather than unwinding it, so the
`finally` that would release the permit is no longer observed."

The documented behaviour is **narrower than that**. The task is not aborted and is not torn down: it
"will continue running on background". Its `finally` blocks and `using` disposals therefore **do
still run**, whenever the background task eventually reaches them. Stack unwinding is not skipped;
it is merely no longer synchronised with the runner's notion of when the test ended.

That splits H-LEAK into two distinct hypotheses, only one of which the documented semantics support
directly:

- **H-LEAK-strong — the permit is never released.** Requires the abandoned background task to never
  reach `UiThreadDispatcherTransaction.Dispose`. The documented semantics do not by themselves
  produce this. It needs an additional condition, e.g. the abandoned body being blocked forever on
  something else. Note the bootstrap problem: "blocked forever on `TransactionGate`" cannot be that
  additional condition, because it presupposes a prior permanent loss. **Not established.**
- **H-LEAK-weak — the permit is released late, after the owning test has already been reported as
  timed out.** This follows directly from the quoted behaviour. Between expiry and the background
  task's eventual `Dispose`, the permit is held by a test the runner considers finished. Any later
  test that calls `BeginTransactionAsync` in that window waits on it, with **no bound at all** as the
  code stands (`:124`). **Directly supported by the documented semantics.**

A third mechanism is worth recording because it is specific to this file and is *not* conditional on
any timeout at all:

- **H-TOKEN-BLIND.** MSTest cancels the `CancellationToken` on expiry in *both* modes ("the
  cancellation token is canceled on timeout" appears in both halves of the quote). The acquisition at
  `:124` uses the parameterless `WaitAsync()` overload, which takes no token and returns a
  non-generic `Task`. It is therefore structurally incapable of observing that cancellation. A test
  abandoned while parked on this acquisition will, when the permit finally arrives, construct a fresh
  `UiThreadDispatcherTransaction` and hand it to a continuation whose test has already been reported.
  This is the mechanism by which the current code converts MSTest's bounded failure into an unbounded
  one, and it is true independently of whether H-LEAK-strong is ever demonstrated.

**Bottom line for the planner:** H-LEAK-strong remains unproven and the documented runner semantics
do not establish it. H-LEAK-weak and H-TOKEN-BLIND are both established from version-scoped
documentation without needing any run. The bounded-acquisition deliverable that `issue.md:51` names
as primary is justified by H-LEAK-weak and H-TOKEN-BLIND alone, so it does not depend on H-LEAK-strong
reproducing. Any spec wording that repeats `issue.md:28`'s "the `finally` ... is no longer observed"
should be corrected to the narrower, documented claim.

---

## 2. Bounded acquisition options and exact semantics

### 2.1 Overload inventory for `net481`

Target framework is `net481` (`QuickFiler.Test/packages.config`, every `targetFramework="net481"`
attribute). Microsoft Learn's `SemaphoreSlim.WaitAsync` page lists six overloads and its moniker
range for every one of them includes `netframework-4.8` and `netframework-4.8.1`. All six are
therefore available.

| Overload | Return | Result when acquisition does not succeed | Permit consumed on failure? |
|---|---|---|---|
| `WaitAsync()` | `Task` | cannot fail to acquire; waits indefinitely | n/a |
| `WaitAsync(int millisecondsTimeout)` | `Task<bool>` | task completes with `false` | No — documented as "otherwise with a result of `false`", i.e. did not enter |
| `WaitAsync(TimeSpan timeout)` | `Task<bool>` | task completes with `false` | No — same documented wording |
| `WaitAsync(CancellationToken)` | `Task` | task faults with `OperationCanceledException` | No — did not enter |
| `WaitAsync(int, CancellationToken)` | `Task<bool>` | `false` on timeout; `OperationCanceledException` on cancellation | No |
| `WaitAsync(TimeSpan, CancellationToken)` | `Task<bool>` | `false` on timeout; `OperationCanceledException` on cancellation | No |

Documented returns quote, identical across the four `Task<bool>` overloads:

> "A task that will complete with a result of `true` if the current thread successfully entered the
> `SemaphoreSlim`, otherwise with a result of `false`."

Documented remarks, relevant to the deterministic construction in §3b:

> "If the timeout is set to zero milliseconds, the method doesn't block. It tests the state of the
> wait handle and returns immediately."

Additional documented exceptions on the timeout overloads: `ObjectDisposedException` if the semaphore
was disposed; `ArgumentOutOfRangeException` for a negative timeout other than -1 or a timeout greater
than `Int32.MaxValue`. `TransactionGate` is `static readonly` and is never disposed anywhere in the
file, so `ObjectDisposedException` is not reachable here.

**Negative claim, with scope.** I searched the `WaitAsync` and `Release` reference pages for any
statement that a *failed* acquisition (timeout or cancellation) can consume a permit and found none;
the documented contract is exclusively "entered / did not enter". I did **not** find, and therefore
do not assert, any .NET Framework 4.8-specific guarantee about a release racing a cancellation. This
is one reason to prefer the `TimeSpan` overload over the `CancellationToken` overload (§2.4): a
`false` return is a plain value, not an exception racing a state transition.

### 2.2 Release semantics — why the pairing is the whole problem

`SemaphoreSlim.Release()` reference page, `netframework-4.8` in range:

- Returns `Int32` — the previous count.
- Throws `SemaphoreFullException` when "The `SemaphoreSlim` has already reached its maximum size."
- Throws `ObjectDisposedException` if disposed.

On a `SemaphoreSlim(1, 1)` whose count is already 1, a `Release()` throws. The file already records
this at `:258`–`:259`, and `QfcItemController.UiThreadDispatcherFixtureTests.cs:269`–`:292` (test
`Transaction_DisposedTwice_DoesNotOverReleaseTheGate`) is an existing regression test for exactly
that. Its assertion message at `:290`–`:291` reads "a second Dispose must not call Release again,
which would throw SemaphoreFullException on a SemaphoreSlim(1, 1)".

A `SemaphoreFullException` here is worse than a lost permit: it permanently raises the available
count above the maximum invariant and destroys mutual exclusion for every subsequent test in the
process, converting a hang into silent cross-test interference. Any bounded-acquisition change must
be shown not to create a second `Release` path.

### 2.3 The control-flow shape that keeps release paired with acquisition

The invariant to state (and the one a reviewer should check against) is:

> **The object that owns the release must be constructed only on the branch where the acquisition
> returned `true`. Never construct it first and then decide.**

The current code already satisfies the "only one releaser" half by construction:
`UiThreadDispatcherTransaction` is the sole type that calls `ReleaseTransactionGate`
(`QfcItemController.UiThreadDispatcherFixture.cs:275`, and `:85`–`:87` documents that as the sole
caller), and it is constructed in exactly one place (`:125`). So making the acquisition bounded is a
two-line change inside `BeginTransactionAsync` that does not disturb the release side at all,
provided the `false` branch exits **before** `new UiThreadDispatcherTransaction()`.

Shape (described, not prescribed — sequencing is the planner's job):

- acquire with a bounded overload into a `bool`;
- on `false`, leave the method by throwing, *before* any transaction object exists;
- on `true`, fall through to the existing `return new UiThreadDispatcherTransaction();`.

Because no object capable of calling `Release` has been created on the `false` path, there is no
release to omit, no `finally` to get wrong, and no way for a caller to dispose something it never
received. The `using`/`try-finally` blocks at all existing call sites (§4) stay correct unchanged,
because a throw from `BeginTransactionAsync` happens before their `using` scope or their assignment
is entered.

Shapes that are **wrong** and should be named as such so review can reject them:

- `try { acquired = await Wait(...); } finally { Release(); }` — releases on the failure path and
  raises count above maximum.
- constructing the transaction first and disposing it when the acquisition failed — same defect,
  routed through `Dispose`.
- returning `null` on failure — every call site immediately dereferences the result
  (`transaction.Install(...)`, `using (var transaction = ...)`), so `null` converts a diagnosable
  timeout into a `NullReferenceException` at a site far from the cause. Three call sites use
  `using (var transaction = await ...)` (`QfcFormControllerUndoHandoffTests.cs:230`, `:281`, `:337`),
  where a `null` would additionally be a silent no-op disposal rather than an error.

### 2.4 Option comparison

**Option A — `WaitAsync(TimeSpan)` with a fixed fixture-owned bound, throw `TimeoutException` on
`false`.**
Advantages: no new parameter on `BeginTransactionAsync`, so none of the eight call sites in §4
change; no `CancellationToken` needs to be threaded through two static helper factories that have no
`TestContext`; the `false` result is a plain value with no exception race; it converts an unbounded
hang into a named, diagnosable failure that names the gate. Limitations: the bound is a real
wall-clock upper limit (see §3a); it does not make the acquisition cancellable, so an abandoned test
still occupies the permit until its bound elapses; picking the number is a judgement call.

**Option B — `WaitAsync(CancellationToken)` flowing MSTest's `TestContext.CancellationTokenSource.Token`.**
Advantages: directly addresses H-TOKEN-BLIND; MSTest cancels that token on timeout even in
non-cooperative mode, so an abandoned acquisition would fail fast rather than complete into a dead
continuation. Limitations, all verified against this tree: `BeginTransactionAsync` is called from two
*static* helpers that have no `TestContext` instance —
`QfcItemController.InitializationTests.Part2.cs:53` inside `internal static async Task<PumpHarness>
BuildPumpHarnessAsync` (`:46`), and the three call sites in `QfcFormControllerUndoHandoffTests.cs`
are instance test methods but the class has no `TestContext` property visible in the file; adding a
token parameter changes the signature at all eight call sites, widening the blast radius from one
file to five; and two consumer classes carry no `[Timeout]` at all (§4), so their token is never
cancelled and they would gain nothing. It also does not bound the wait for those two classes.

**Option C — both: `WaitAsync(TimeSpan, CancellationToken)`.** Strictly more capable, strictly more
blast radius. Inherits Option B's signature churn.

**Assessment.** Option A is the one that matches `issue.md:51`'s stated primary deliverable ("The
bounded-acquisition half of the disjunction is deliverable and testable regardless of whether H-LEAK
reproduces") at the smallest blast radius, and it is the only one of the three that fixes the
no-`[Timeout]` consumers. Option B addresses a mechanism Option A does not, and is a reasonable
follow-up issue rather than part of this change. This is an assessment for the planner to accept or
reject, not a plan.

---

## 3. Determinism constraint

### 3a. Is a bounded `WaitAsync(TimeSpan)` in fixture infrastructure a banned construct?

**The rule text that decides it**, read at
`.claude/rules/general-unit-test.md:98`–`:105`, section heading "Determinism Infrastructure":

> "All test code must be deterministic. The following infrastructure requirements apply uniformly:
> - **Controllable clock** — use a `Clock` interface (TypeScript) or `TimeProvider` (.NET) injected into code under test. Do not read wall-clock time directly in production code under test.
> - **Seeded RNG** — ...
> - **Banned APIs in test code** — `setTimeout`, `Thread.Sleep`, `Task.Delay`, real wall-clock waits, and `Date.now()` outside the clock interface are prohibited in tests.
> - **Virtual scheduler / fake timers / `FakeTimeProvider`** — async tests must use the framework's fake-timer facility (`jest.useFakeTimers()` for Jest, `FakeTimeProvider` for .NET) to advance simulated time deterministically."

**Reading A — it is banned.** The bullet at `:104` says "real wall-clock waits ... are prohibited in
tests", with no carve-out for infrastructure versus test body. `QfcItemController.UiThreadDispatcherFixture.cs`
is compiled into `QuickFiler.Test` (`QuickFiler.Test.csproj:194`), so it is test code by any
plain reading of "in tests". A `WaitAsync(TimeSpan.FromSeconds(n))` on a contended gate does elapse
real wall-clock time before returning `false`. On this reading the change substitutes one prohibited
construct for an unbounded one and the correct remedy is a non-temporal one — a `TimeProvider`-backed
gate, or `[DoNotParallelize]`, or removing the gate.

**Reading B — the rule does not reach it.** Three textual arguments. First, the section is titled
"Determinism Infrastructure" and every other bullet is about making the *outcome* of a test a
function of simulated rather than real time: inject a `TimeProvider`, seed the RNG, use fake timers
"to advance simulated time deterministically" (`:105`). The banned list pairs `setTimeout`,
`Thread.Sleep` and `Task.Delay` — three constructs whose entire purpose is to *consume* time so that
something else can happen — with `Date.now()`, which *reads* time. A bounded acquisition does
neither: on the success path it returns the instant the permit is available, exactly as the
unbounded form does, so its contribution to a passing run's duration is zero and it cannot be the
mechanism by which an expected state is reached. Second, `:102` scopes the clock requirement to
"production code under test", which this fixture is not. Third, and decisively, the same words would
condemn MSTest's own `[Timeout]`: `[Timeout(60000)]` is a real wall-clock bound authored in test
code, and it appears six times in the subject's own test file at
`QfcItemController.UiThreadDispatcherFixtureTests.cs:41`, `:104`, `:154`, `:203`, `:270` and `:317`,
plus at `WpfUiDispatcherTests.cs:49` and at eight sites in
`QfcItemController.InitializationTests.Part3.cs` (`:39`, `:82`, `:130`, `:174`, `:244`, `:352`,
`:400`, `:455`). Reading A makes all of those pre-existing violations.

**Conclusion — Reading B, with a stated criterion.** The repository has already settled this reading
in code, in this exact idiom, and has written down why:

- `QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs:48`–`:49` (doc for `WaitForState` at `:54`): "Bounded, event-driven wait for a state transition. **This is not a fixed sleep: it returns as soon as the condition holds, and fails the test with a clear message if it never does.**"
- `TaskMaster.Test/AppGlobals/NonBlockingDelayTests.cs:29`: "**The outer MSTest `[Timeout]` is a deadlock bound, not a wait.**"
- The same file's class doc at `:16`–`:17`: "no elapsed-time measurement and no real wall-clock wait is used" — written about a test that nonetheless carries `[Timeout(5000)]` at `:32`. The repository's own authors therefore do not count a failure bound as a "real wall-clock wait".

The criterion those two comments jointly establish, and the one the spec should record verbatim so
review does not relitigate it:

> A real-time bound is permitted in test code when (i) it returns immediately once the awaited
> condition holds, so it contributes nothing to the duration of a passing run, and (ii) it is a
> failure bound whose expiry is reported as a failure, never the mechanism by which the expected
> state is reached. A construct that consumes time in order to let something else happen is banned
> regardless of where it is written.

A `WaitAsync(TimeSpan)` in `BeginTransactionAsync` satisfies both clauses. Note the corollary that
constrains §3b: a *test* that observes `false` must not get there by letting the bound elapse, because
that would breach clause (i).

`issue.md:53` already asserts this conclusion ("A bounded `WaitAsync(TimeSpan)` on a synchronization
primitive owned by a fixture is infrastructure timeout policy, not a test-body sleep") and directs
that it be recorded in `spec.md`. The above supplies the rule text and the in-tree precedent that
support it. **`spec.md` is currently an unfilled template** — all of its Acceptance Criteria at
`:83`–`:90` are the generic bug-template placeholders and none of the §2/§3 content is recorded
there yet.

### 3b. Deterministic constructions for an abandoned-transaction state

Enumerated candidates, with a determinism verdict for each. The reachability facts matter:
`TransactionGate` is `private static readonly` inside an `internal static` class
(`QfcItemController.UiThreadDispatcherFixture.cs:29`, `:32`), so a test in the same assembly can
reach it only by reflection or through `BeginTransactionAsync`.

**C1 — Hold a real transaction and probe with a zero-length bound. DETERMINISTIC.**
Acquire a transaction via `BeginTransactionAsync`, deliberately do not dispose it yet, then perform a
bounded acquisition with `TimeSpan.Zero`. Per the documented remark quoted in §2.1, a zero timeout
"doesn't block. It tests the state of the wait handle and returns immediately", so the probe returns
`false` with zero elapsed time and zero scheduling dependence. Dispose the held transaction in a
`finally`, then probe again to observe `true`. This exercises the real `false` branch of the real
acquisition and satisfies both clauses of the §3a criterion. It requires a seam: the fixture needs an
acquisition entry point that accepts the timeout (with the public `BeginTransactionAsync()`
delegating to it with the production default), because a test cannot otherwise ask for
`TimeSpan.Zero`. Risks to record: (i) the gate is process-wide and shared with every other class in
`QuickFiler.Test`, and `TaskMaster.runsettings:4`–`:7` sets `Workers 0, Scope ClassLevel`, so other
classes run concurrently — the hold window must stay short, and `[DoNotParallelize]` is the
established local mitigation for exactly this (used at `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs:24`
and `QuickFiler.Test/Helper Classes/ViewerQueueStaticWrapperTests.cs:11`); (ii) the release must be in
a `finally` so a failing assertion cannot itself leak the permit and poison the rest of the run.

**C2 — Reflect onto the private `TransactionGate` field and `Wait()` it directly. DETERMINISTIC but
higher risk.** The file already establishes a reflection idiom (`ResolveDispatcherField` at `:133`,
and `UtilitiesCS.Test/TestHelpers/UiThreadDispatcherScope.cs:114` mirrors it), so the technique has
local precedent. But that precedent reflects across an assembly boundary onto a field the test cannot
otherwise reach; here the same effect is reachable without reflection via C1. C2 also couples a test
to a private field name in its own assembly, and a failure between the raw `Wait()` and the raw
`Release()` corrupts the gate for the whole process with no `Dispose` to recover it. Deterministic,
but strictly worse than C1.

**C3 — Observe `SemaphoreSlim.CurrentCount`. DETERMINISTIC, but does not exercise the fix.**
Assert the count is 0 while a transaction is held and 1 after disposal. Zero wall-clock involvement.
It proves the gate's state but never drives the `false` branch of the bounded acquisition, so it
cannot be the regression test for this change. Useful only as a supporting assertion, and it needs
the same kind of seam or reflection to reach the private field.

**C4 — Reproduce genuine MSTest abandonment. NOT DETERMINISTIC. Reject.**
Author a test with a very small `[Timeout]` that parks on the gate, so the runner abandons it, then
assert a later test's behaviour. This depends on cross-test ordering (`TaskMaster.runsettings` sets
`Scope ClassLevel`, and MSTest gives no ordering guarantee across classes), on a deliberately failing
test being present in the suite, and per §1.2 the abandoned task keeps running and will release the
permit at an unpredictable later moment. It also breaches §3a clause (ii): the expected state is
reached *by* elapsed time. It is the exact shape `issue.md:57` warns against depending on.

**C5 — Inject the `SemaphoreSlim` instance as a seam. Deterministic, but conflicts with the design.**
Making `TransactionGate` settable would defeat the file's stated "single owner" property
(`:11`–`:13`) and create a new way for one test to substitute a gate the rest of the assembly is not
using. Not recommended.

**Answer to the question as asked:** yes. C1 produces an abandoned-transaction-equivalent state —
a permit held by a holder that will not release during the probe — deterministically and with no
wall-clock wait, and lets a bounded acquisition be observed returning `false`. C2 and C3 are also
deterministic; C4 is not.

---

## 4. Blast radius of changing the acquisition

### 4.1 Every call site of `BeginTransactionAsync`

Search scope: Grep for `BeginTransactionAsync|UiThreadDispatcherFixture|UiThreadDispatcherTransaction`
over `**/*.cs` rooted at the worktree. Eight call sites in five files, plus the declaration.

| # | Repository-relative path | Line(s) | Shape |
|---|---|---|---|
| — | `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs` | `122`–`126` | the declaration itself |
| 1 | `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs` | `48`–`50` | `try` / `finally { transaction.Dispose(); }` |
| 2 | same | `108`–`110` | assign, then `try` / `finally` |
| 3 | same | `158`–`160` | assign, then `try` / `finally` |
| 4 | same | `210`–`212` | assigned outside any `try`; also `223`–`225`, a **second, concurrent** acquisition inside a `Task.Run` |
| 5 | same | `277`–`279`, and `294`–`296` | a second round-trip acquisition in the same test |
| 6 | same | `324`–`326` | `try` / `finally` |
| 7 | `QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs` | `58`–`60` | split across two statements (CSharpier note at `:56`–`:57`), then `try` / `finally` |
| 8 | `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs` | `53`–`55` | inside `internal static async Task<PumpHarness> BuildPumpHarnessAsync` (`:46`); `catch { transaction.Dispose(); throw; }` at `:61`–`:65`; ownership then transfers to `PumpHarness` (`:285`, `:293`, `:300`) and is released by `PumpHarness.Restore()` at `:317`–`:330` |
| 9 | `QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncTests.cs` | `353`–`354` | `transaction` declared `null` at `:332` inside a `try`; **no `ConfigureAwait`** |
| 10 | `QuickFiler.Test/Controllers/QfcFormControllerUndoHandoffTests.cs` | `230`, `281`, `337` | `using (var transaction = await UiThreadDispatcherFixture.BeginTransactionAsync())` |

(Rows 4, 5 and 10 each contain more than one acquisition; the eight-file-site count above collapses
them per statement. The complete statement-level list is: `FixtureTests` `:49`, `:109`, `:159`,
`:211`, `:224`, `:278`, `:295`, `:325`; `WpfUiDispatcherTests` `:59`; `InitializationTests.Part2`
`:54`; `QfcHomeControllerRunAsyncTests` `:354`; `QfcFormControllerUndoHandoffTests` `:230`, `:281`,
`:337` — fourteen acquisition statements across five files.)

Every one of these sites already routes the release through `UiThreadDispatcherTransaction.Dispose`.
None of them calls `ReleaseTransactionGate` directly; a Grep for `ReleaseTransactionGate` found it
only at its declaration (`:88`) and its single call (`:275`), plus two doc-comment mentions (`:85`,
`:257` region).

### 4.2 Consumers that use the fixture without the gate

- `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs:239` — calls `UiThreadDispatcherFixture.EnsureDispatcher()`; documented at `:234`. Does not touch the gate.
- `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs:52` and `:61` — read `UiThreadDispatcherFixture.Current` only. That class carries `[DoNotParallelize]` at `:24`.
- `UtilitiesCS.Test/TestHelpers/UiThreadDispatcherScope.cs:40` — a doc-comment cross-reference only. It is a *different* type in a *different* assembly with no semaphore at all; its remarks at `:19`–`:29` record that it is deliberately unsynchronised and relies on `[DoNotParallelize]` instead, and that `QuickFiler.Test` "uses its own fixture accessor rather than this type". It is **not** in the blast radius.

### 4.3 `[Timeout]` status of the consuming classes — the material finding

| Class | File | `[Timeout]` on its tests? |
|---|---|---|
| `QfcItemController_UiThreadDispatcherFixtureTests` (`:31`) | `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs` | Yes — `[Timeout(GateTimeoutMs)]`, `GateTimeoutMs = 60000` at `:33`, applied at `:41`, `:104`, `:154`, `:203`, `:270`, `:317` |
| `WpfUiDispatcherTests` (`:19`) | `QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs` | Yes — `[Timeout(GateTimeoutMs)]` at `:49` |
| `QfcItemController_InitializationTests` (`[TestClass]` at `QfcItemController.InitializationTests.cs:29`, partials in Part2/Part3) | three files | Yes — `[Timeout(PumpTimeoutMs)]`, `PumpTimeoutMs = 60000` at `QfcItemController.InitializationTests.cs:38`, applied eight times in Part3 |
| `QfcHomeControllerRunAsyncTests` (`:24`) | `QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncTests.cs` | **No.** A Grep for `Timeout` over the whole file returned **no matches**. The gate-acquiring test at `:325` carries a bare `[TestMethod]` at `:324` |
| `QfcFormControllerUndoHandoffTests` (`:29`) | `QuickFiler.Test/Controllers/QfcFormControllerUndoHandoffTests.cs` | **No.** A Grep for `Timeout` over the whole file returned **no matches**. All three gate-acquiring tests (`:228`, `:279`, `:335`) carry a bare `[TestMethod]` |

This is the sharpest argument in the tree for the change. The fixture's own doc at `:23`–`:27`
justifies keeping `EnsureDispatcher` off the gate precisely because its callers "carry no
`[Timeout]`" and making them wait "would convert a bounded failure elsewhere into an unbounded hang
there". Two classes that *do* take the gate carry no `[Timeout]` either. For those four test methods,
a lost or late-released permit today has **no bound of any kind** — not MSTest's, not the gate's —
and the run hangs until the outer runner-level guard (`/Blame:...;TestTimeout=4min` in the recorded
commands, see the flake-watch log below) kills it. A bounded acquisition inside `BeginTransactionAsync`
is the only mechanism that bounds those four.

### 4.4 Does `QuickFiler.Test.csproj` need modification for a new test file?

**Yes.** The project uses explicit compile items exclusively: a Grep for `Compile Include=` counts
**173** occurrences in `QuickFiler.Test/QuickFiler.Test.csproj`, and a Grep for
`\*\*\\\*\.cs|Include="\*\*|EnableDefaultCompileItems` over the same file returned **no matches**.
There is no globbing and no SDK-style default include. Relevant existing entries:

- `QuickFiler.Test/QuickFiler.Test.csproj:194` — `<Compile Include="Controllers\QfcItemController.UiThreadDispatcherFixture.cs" />`
- `:195` — `<Compile Include="Controllers\QfcItemController.UiThreadDispatcherFixtureTests.cs" />`
- `:118` — `<Compile Include="Controllers\QfcFormControllerUndoHandoffTests.cs" />`
- `:211` — `<Compile Include="Controllers\WpfUiDispatcherTests.cs" />`

So `QuickFiler.Test.csproj` is a shared file in the blast radius for any plan that adds a new test
file. Adding assertions to the existing `QfcItemController.UiThreadDispatcherFixtureTests.cs`
(currently 353 lines, comfortably under the 500-line ceiling in
`.claude/rules/general-code-change.md`) avoids touching the project file entirely. The subject file
itself is 278 lines, so the fix has ample room without a split.

---

## 5. Existing coverage in `QfcItemController.UiThreadDispatcherFixtureTests.cs`

Six tests, all `[TestClass] public class QfcItemController_UiThreadDispatcherFixtureTests` (`:30`–`:31`),
all `[Timeout(GateTimeoutMs)]` with `GateTimeoutMs = 60000` (`:33`). The class doc at `:10`–`:29`
frames them as issue #493 regression tests and states at `:26`–`:28` that "there is no sleep, no
delay, no wall-clock wait, and no temporary file".

| Test | Lines | What it asserts | Affected by a bounded acquisition? |
|---|---|---|---|
| `EnsureDispatcher_WhileATransactionHoldsALiveDispatcher_DoesNotReplaceIt` (R1) | `40`–`96` | With a live dispatcher installed by a transaction, `EnsureUiThreadDispatcher()` installs nothing and its scope's disposal is a no-op; then transaction disposal restores the captured original | No behavioural change. Acquires once, uncontended, so a bounded acquire returns `true` immediately |
| `EnsureDispatcher_WhenTheFieldIsNull_InstallsAndRestoresOnDispose` (R2) | `103`–`147` | With the field forced null, ensure seeds the parked dispatcher and its scope reverts to null | No |
| `EnsureDispatcher_ScopeDisposedTwice_IsIdempotent` (R3) | `153`–`188` | A second `Dispose` on the ensure scope neither throws nor rewrites the field | No |
| `Transaction_SecondCallerCannotInstallUntilTheFirstRestores` (R4) | `202`–`262` | A second caller acquiring from a `Task.Run` observes the pre-install value, never the first transaction's installed value — i.e. restore strictly precedes release | **Yes — the one to watch.** This is the only test with a genuinely contended acquisition (`:223`–`:225`) while transaction A still holds the permit (`:210`–`:214`). Under a bounded acquisition the waiter would fail with a timeout instead of blocking if the release were ever delayed past the bound. The synchronisation is `secondCallerStarted.Wait()` at `:237` followed by `transactionA.Dispose()` at `:238`, so the hold window is short and a bound of seconds is not at risk; but any bound shorter than the scheduling latency of `Task.Run` would convert this test into a flake. The class doc at `:14`–`:21` already records that this test is probabilistic by construction, and `:194`–`:200` records it as an intermittent failure tracked by issue #823 with an append-only log |
| `Transaction_DisposedTwice_DoesNotOverReleaseTheGate` (R5) | `269`–`310` | A second `Dispose` does not call `Release` again (which would throw `SemaphoreFullException`), and a subsequent round-trip acquisition (`:294`–`:297`) still succeeds, proving the gate is intact | **Yes — the guard that must keep passing.** Its round-trip acquisition at `:294`–`:296` is precisely the assertion that would catch a `false`-path that wrongly released. It is the existing protection against the §2.3 wrong shapes and must not be weakened |
| `Install_CalledTwiceOnTheSameTransaction_ThrowsInvalidOperationException` (R6) | `316`–`351` | A second `Install` throws `InvalidOperationException` | No |

Independent corroboration of R4's intermittency, read in this worktree:
`docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence/other/flake-watch-uithread-dispatcher-transaction.2026-09-09T00-15.md`
— an append-only log with `OBSERVATIONS: 4` (`:39`): one failure (Row 1, `:41`–`:52`, failure text not
captured) and three passes, all under `Workers 0, Scope ClassLevel`, all with
`/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None`. Its `:86`–`:91` explicitly declines to
assert any correlation. The log's `:20`–`:21` forbids introducing "no sleep, retry attribute or
timing tolerance ... to stabilise the test"; a bounded acquisition is not a stabiliser for R4 and
should not be presented as one.

---

## 6. Prior art for bounded / timeout-bounded synchronization in the test assemblies

**Search scope and patterns.** Grep over the worktree with glob `**/*Test*/**/*.cs` for the regex
`WaitAsync\([^)]+\)|\.Wait\([^)]+\)|WaitOne\([^)]+\)` (match-only mode); a separate Grep over
`**/*.Test/**/*.cs` for `WaitAsync\(|\.Wait\(Time|WaitOne\(|SemaphoreSlim|TimeoutAfter|CancellationTokenSource\(`;
a Grep for `DoNotParallelize|assembly: Parallelize` over `QuickFiler.Test`.

**Prior art exists.** The established local style is *a bounded real-time wait whose boolean result is
asserted with FluentAssertions and a stated reason*:

| Path : line | Shape |
|---|---|
| `QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs:56` | `SpinWait.SpinUntil(condition, TimeSpan.FromSeconds(5)).Should().BeTrue(because);` inside `private static void WaitForState(Func<bool> condition, string because)` (`:54`), documented at `:48`–`:52` |
| `QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs:103`–`:105` | `loaderEntered.Task.Wait(TimeSpan.FromSeconds(5)).Should().BeTrue("the started worker must reach the injected RemainingEmailLoader");` |
| `QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs:173`–`:175` | same shape, reason "the started worker must reach the injected loader" |
| `QuickFiler.Test/Controllers/QfcInitEmailQueueZeroBatchTests.cs:161` | `.Wait(TimeSpan.FromSeconds(5))` in the same asserted-boolean shape |
| `QuickFiler.Test/Controllers/QfcDatamodelTeardownTests.cs:220` | `.Wait(TimeSpan.FromSeconds(5))` in the same shape |

Non-blocking zero-timeout probes also have precedent, which is directly relevant to construction C1
in §3b:

| Path : line | Shape |
|---|---|
| `QuickFiler.Test/Viewers/BreadcrumbCoordinatorLifecycleTests.cs:57` | `Action observeCanceledSource = () => staleToken.WaitHandle.WaitOne(0);` — a zero-timeout state probe |
| `QuickFiler.Test/Viewers/BreadcrumbUiThreadDispatchTests.cs:410`, `BreadcrumbSelectorToggleUiBoundaryTests.cs:419`, `BreadcrumbPopupBoundaryCoverageTests.cs:320` | `.Wait(0)` — zero-timeout probes |

`SemaphoreSlim` in a test assembly, and the closest existing analogue of the subject's own usage:

| Path : line | Shape |
|---|---|
| `QuickFiler.Test/Viewers/BreadcrumbUiThreadDispatchTests.cs:366` | `private readonly SemaphoreSlim _available = new SemaphoreSlim(0);` |
| same file `:391` | `Task available = _available.WaitAsync();` — an **unbounded** acquisition, i.e. the same shape as the subject. Worth flagging to the planner as a possible sibling, but it is a per-instance test-local semaphore, not a process-wide static, so its blast radius on abandonment is confined to one test. Out of scope for #882 |

Virtual-time prior art, for completeness (this is the repository's answer to *delay*, not to
*mutual exclusion*): `TaskMaster.Test/AppGlobals/NonBlockingDelayTests.cs` uses
`Microsoft.Extensions.Time.Testing.FakeTimeProvider` (`:5`, `:42`) with `Advance` (`:51`) and asserts
non-completion before the advance (`:46`–`:50`). `Microsoft.Extensions.TimeProvider.Testing` 10.10.0
is referenced by `QuickFiler.Test/packages.config:86`–`:89`, so `FakeTimeProvider` is available in
this assembly. It is not applicable to a `SemaphoreSlim` acquisition, because `SemaphoreSlim` has no
`TimeProvider` overload on `net481` (§2.1 lists all six `WaitAsync` overloads; none accepts one).

**Negative claim, with scope.** Within the search scope and patterns above I found **no** existing
bounded (`TimeSpan` or `int`) or `CancellationToken`-observing `SemaphoreSlim.WaitAsync` anywhere in
any test assembly in this tree. The only two `SemaphoreSlim.WaitAsync` call sites found are the
subject at `QfcItemController.UiThreadDispatcherFixture.cs:124` and
`BreadcrumbUiThreadDispatchTests.cs:391`, and both use the unbounded parameterless overload.
A bounded `SemaphoreSlim` acquisition would therefore be **new to the test assemblies**, but it is a
direct application of an already-established local idiom (bounded wait, boolean result asserted or
branched on, failure reported with a stated reason) rather than a new pattern.

`[DoNotParallelize]` prior art in `QuickFiler.Test`: exactly two classes carry it —
`QuickFiler.Test/Helper Classes/ViewerQueueStaticWrapperTests.cs:11` and
`QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs:24`. A Grep for `assembly: Parallelize` over
`QuickFiler.Test` returned no matches, so the assembly inherits
`TaskMaster.runsettings:4`–`:7` (`Workers 0`, `Scope ClassLevel`).

---

## 7. Testing implications (strategy only; no test code)

Consistent with `.claude/rules/general-unit-test.md` and the C# Unit Test Policy (MSTest, Moq,
FluentAssertions):

1. **The regression assertion should be the `false` branch of the bounded acquisition, observed by
   construction C1** — a held permit plus a zero-length probe — not by letting a bound elapse. That
   keeps clause (i) of the §3a criterion intact and makes the test contribute zero time to a passing
   run.
2. **A companion assertion must prove the gate survives a failed acquisition**: after a probe returns
   `false`, releasing the held transaction and acquiring again must succeed. This is the assertion
   that would catch every one of the §2.3 wrong shapes, and it mirrors the round-trip already used by
   R5 at `QfcItemController.UiThreadDispatcherFixtureTests.cs:294`–`:304`.
3. **A negative assertion that no `SemaphoreFullException` is reachable on the failure path** —
   FluentAssertions `.Should().NotThrow<SemaphoreFullException>()` in the shape R5 already uses at
   `:287`–`:292`.
4. **Fail-before / pass-after framing.** The `false` branch does not exist on the current tree, so a
   test written against it will not compile before the fix rather than fail. The planner should
   decide how to satisfy the repository's fail-before evidence requirement for that — one option is a
   test that asserts the *current* unbounded acquisition's observable signature (a non-generic `Task`
   return) and is replaced, another is to accept a compile-level fail-before with that fact recorded
   explicitly. This is a real constraint and should not be discovered at execution time.
5. **Determinism guards.** Any new test should carry `[Timeout(...)]` matching the local 60000 ms
   convention (`:33`), and the planner should consider `[DoNotParallelize]` on the class that holds
   the process-wide permit, following the local precedent at `EmailMoveMonitorTests.cs:24`.
6. **Do not touch R4.** `flake-watch-uithread-dispatcher-transaction.2026-09-09T00-15.md:20`–`:21`
   forbids stabilising it with a sleep, retry or tolerance, and this change is not a fix for it.
   If the run produces a new observation for R4, append a row per that file's `:93`–`:97` instructions.
7. **Coverage.** The change is confined to test-assembly infrastructure. `QuickFiler.Test` is a test
   project and is excluded from the coverage denominator, so no production-coverage movement is
   expected. This should be stated in the spec rather than measured.

---

## 8. Open questions the planner must resolve

1. **The bound value.** Nothing in the tree fixes it. The two candidate anchors are the local
   `[Timeout]` convention (60000 ms, `:33` and `QfcItemController.InitializationTests.cs:38`) and the
   recorded runner-level hang guard (`TestTimeout=4min` in the flake-watch commands at
   `flake-watch-...md:44`, `:57`, `:67`, `:79`). A gate bound must be strictly below the MSTest
   `[Timeout]` for the bound to be the thing that reports, and must exceed the longest legitimate
   hold — which is `PumpHarness`'s, held for a whole pump-hosted test body
   (`QfcItemController.InitializationTests.Part2.cs:51`–`:52`, `:317`–`:330`). Those two constraints
   are in tension and the planner must reconcile them explicitly. Note that the two no-`[Timeout]`
   classes (§4.3) have no upper anchor at all.
2. **Failure type.** `TimeoutException` versus a fixture-specific exception. The message should name
   the gate, name the abandonment hypothesis, and be greppable, since the whole point is that the
   cause is not local to the failing test.
3. **Whether to also add the `CancellationToken` overload (Option B).** Recommended as a separate
   follow-up issue rather than part of this change, for the blast-radius reasons in §2.4.
4. **`spec.md` is an unfilled template.** Every section from `:10` to `:99` is placeholder text and
   all eight Acceptance Criteria at `:83`–`:90` are the generic bug-template defaults. Per
   `issue.md:11`, `full-bug` resolves acceptance criteria from `spec.md` only, so `spec.md` must be
   authored before any AC-bearing plan can be written. `issue.md:53` specifically requires the
   §3a infrastructure-versus-test-body distinction to be recorded there.
5. **The H-LEAK-strong / H-LEAK-weak split (§1.4).** `issue.md:28` overstates the runner behaviour.
   The spec should carry the corrected, documented statement so that a later reviewer does not reject
   the change for resting on a claim that MSTest's own documentation contradicts.

---

## 9. Evidence index

Files read in full or in part in this worktree:

- `docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/issue.md` (77 lines, full)
- `docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/spec.md` (100 lines, full — unfilled template)
- `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs` (278 lines, full)
- `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs` (353 lines, full)
- `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs` (`:20`–`:129`, `:275`–`:334`)
- `QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncTests.cs` (`:315`–`:384`)
- `QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs` (`:30`–`:89`)
- `QuickFiler.Test/Controllers/QfcDatamodelLivenessTests.cs` (`:46`–`:134`, `:168`–`:220`)
- `TaskMaster.Test/AppGlobals/NonBlockingDelayTests.cs` (129 lines, full)
- `UtilitiesCS.Test/TestHelpers/UiThreadDispatcherScope.cs` (126 lines, full)
- `QuickFiler.Test/packages.config` (179 lines, full)
- `TaskMaster.runsettings` (30 lines, full)
- `.claude/rules/general-unit-test.md` (`:95`–`:105`)
- `docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence/other/flake-watch-uithread-dispatcher-transaction.2026-09-09T00-15.md` (97 lines, full)

External sources:

- Microsoft Learn, `TimeoutAttribute` class (`defaultMoniker: mstest-net-4.4`; package list includes `MSTest.TestFramework v4.4.0`) — `CooperativeCancellation` remarks. Load-bearing for §1.
- Microsoft Learn, "Configure MSTest" — `testconfig.json` `timeout` table, `useCooperativeCancellation` default `false`.
- Microsoft Learn, `SemaphoreSlim.WaitAsync` — six overloads, all in moniker range `netframework-4.8`.
- Microsoft Learn, `SemaphoreSlim.Release` — `SemaphoreFullException` condition, in moniker range `netframework-4.8`.
- `microsoft/testfx` `main`, `src/Adapter/MSTestAdapter.PlatformServices/Services/ThreadOperations.cs` — corroboration only, not version-pinned (see the limit recorded in §1.3).

Commands NOT run, per the assignment: no `msbuild`, no `vstest.console.exe`, no `dotnet`, no
`csharpier`. No file outside this research directory was created or modified. Nothing was committed.
