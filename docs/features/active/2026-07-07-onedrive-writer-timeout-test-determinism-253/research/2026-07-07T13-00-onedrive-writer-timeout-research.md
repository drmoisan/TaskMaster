# Research: OneDrive Writer Timeout Test Determinism (Issue #253)

- Timestamp: 2026-07-07T13-00
- Scope: Independent verification of root cause for the intermittently failing test
  `OneDriveDownloader_Tests.TryGetFileStreamWriter_WhenWriterReturnsMemoryStream_ReturnsStream`
  and recommendation of the policy-compliant fix.
- Automation Feasibility: Not applicable. This is a code-only C# investigation with no
  third-party UI surface; no automation feasibility section is produced.

## 1. Current State Analysis

### 1.1 Production call chain

- `UtilitiesCS/OneDriveHelpers/OneDriveDownloader.cs:82-103` — `TryGetFileStreamWriter`
  wraps the synchronous `GetFileStreamWriter` factory (line 90-96):

  ```
  var stream = await GetFileStreamWriter.RunWithTimeout(destinationPath, cancel, timeoutMs, 3, false);
  ```

  wrapped in a `try { } catch (Exception) { return null; }` (lines 88-102). `GetFileStreamWriter`
  (lines 105-118) is a `virtual Func<string, Stream>` field defaulting to a real
  `new FileStream(...)` call; `TestableOneDriveDownloader.SetFileStreamWriter` (test file,
  lines 23-26) substitutes it in tests.

- `UtilitiesCS/Threading/TimeOutTask.cs:176-229` — the `Func<T1,TResult>` synchronous
  overload of `RunWithTimeout` used by the call above. It builds a real
  `CancellationTokenSource(milliseconds)` (line 188), links it to the caller's token
  (lines 189-192), and executes the delegate via `Task.Run(() => function(arg1), combinedToken.Token)`
  (line 197). The catch ladder is:
  - `catch (TimeoutException)` — line 199
  - `catch (System.Exception e)` — line 219, logs and (if `strict`) rethrows.

### 1.2 Exception-type mismatch confirmed against sibling overloads

Every other overload in `TimeOutTask.cs` that wraps a `Task.Run`/awaited task under a real
`CancellationTokenSource` catches `TaskCanceledException`, not `TimeoutException`:
- `RunWithTimeout<TResult>` (`Func<TResult>` overload), line 64: `catch (TaskCanceledException)`
- `RunWithTimeout<TResult>` (`Func<CancellationToken,Task<TResult>>` overload), line 129: `catch (TaskCanceledException)`
- `RunWithTimeout<T1,T2,TResult>` (`Func<T1,T2,TResult>` overload), line 350: `catch (TaskCanceledException)`
- `RunWithTimeout<T1,T2,T3,TResult>` (`Func<T1,T2,T3,TResult>` overload), line 580: `catch (TaskCanceledException)`

The `Func<T1,TResult>` overload at line 199 is the sole exception in the file: it catches
`TimeoutException` instead. This is the exact overload `OneDriveDownloader.TryGetFileStreamWriter`
uses.

`System.Threading.Tasks.TaskCanceledException` derives from `OperationCanceledException`,
which is unrelated to `System.TimeoutException` (both derive independently from
`SystemException`). A `CancellationTokenSource`-driven cancellation of `Task.Run(..., token)`
surfaces as `TaskCanceledException` on `await`, not `TimeoutException`. Therefore the `catch
(TimeoutException)` branch at line 199 does not — and cannot, under real timer-driven
cancellation — match the exception a real timeout produces in this overload. It only matches
a `TimeoutException` that the wrapped delegate throws directly.

This is independently confirmed by the overload's own existing coverage tests, which simulate
"timeout" by having the wrapped delegate throw `TimeoutException` explicitly rather than by
using a genuinely short `milliseconds` value and letting the real `CancellationTokenSource`
fire:
- `UtilitiesCS.Test/Threading/TimeOutTask_OverloadCoverageTests.cs:105-122`
  (`RunWithTimeout_FuncT1TResult_ShouldReturnDefault_WhenTimeoutOccursWithoutRetries`) —
  `Func<int, string> function = value => throw new TimeoutException("timeout");`
- `UtilitiesCS.Test/Threading/TimeOutTask_AdditionalTests.cs:147-173`
  (`RunWithTimeout_FuncT1TResult_ShouldRetryAfterTimeoutException`) — the delegate throws
  `TimeoutException` on the first invocation, and the test asserts the retry-and-succeed
  path (`attempts.Should().Be(2)`, `result.Should().Be("result-42")`). No existing test drives
  this overload's real `CancellationTokenSource` path with a short `milliseconds` value to
  prove genuine-timeout retry behavior — because the real cancellation manifests as
  `TaskCanceledException`, which this overload's retry branch cannot catch.

Consequence for `TryGetFileStreamWriter`: when a real timeout fires (via the linked
`CancellationTokenSource`), the resulting `TaskCanceledException` is *not* caught by line 199,
falls through to the generic `catch (System.Exception e)` at line 219, is logged, and — because
`OneDriveDownloader` calls with `strict=false` — is swallowed, returning `default(TResult)`
(`null`) **without any retry**. The `maxAttempts: 3` argument passed by
`TryGetFileStreamWriter`/`TryGetUrlStreamAsync` is therefore inert for genuine
`CancellationTokenSource` timeouts in this overload; it only has effect if the wrapped
delegate throws `TimeoutException` itself, which no real caller does.

### 1.3 Why the observed duration is ~18s against a 5000 ms argument

`CancellationTokenSource(milliseconds)` schedules its cancellation callback on a `Timer`,
which itself is serviced by the CLR thread pool, and `Task.Run(() => function(arg1), token)`
queues the delegate onto the same thread pool. Under the Visual Studio parallel test host
(`[assembly: Parallelize(Workers = 0, Scope = ExecutionScope.ClassLevel)]`,
`UtilitiesCS.Test/Properties/AssemblyInfo.cs:18-21`, `Workers = 0` meaning "use all available
processors"), many test classes run concurrently and can saturate the process-wide thread
pool. .NET's thread-pool growth algorithm adds new threads slowly once the pool is starved
(on the order of one thread per throttling interval), so both (a) the queued writer-factory
work item and (b) the `CancellationTokenSource`'s own timer callback can be delayed well
beyond the nominal 5000 ms wall-clock window. This is consistent with the observed ~18s
duration: the elapsed time is bounded by thread-pool scheduling latency for two competing
work items, not by the 5000 ms argument alone. VS Code's test host does not reproduce the
same contention profile, which is why the test passes there.

### 1.4 Confirmed asymmetry between the two sibling tests

- `TryGetFileStreamWriter_WhenWriterReturnsMemoryStream_ReturnsStream`
  (`OneDriveDownloader_Tests.cs:227-237`) sets `GetFileStreamWriter` to
  `_ => new MemoryStream()` and asserts the returned stream `Should().NotBeNull()`. This
  assertion is **only** true if the queued delegate actually executes before the (possibly
  delayed) cancellation fires.
- `TryGetFileStreamWriter_WhenWriterThrows_ReturnsNull` (`OneDriveDownloader_Tests.cs:250-258`)
  sets `GetFileStreamWriter` to `_ => throw new InvalidOperationException("boom")` and asserts
  `stream.Should().BeNull()`. This assertion holds under **both** outcomes of the race:
  - If the delegate runs before cancellation, it throws `InvalidOperationException`, which is
    caught by the generic `catch (System.Exception e)` (line 219) and swallowed
    (`strict=false`) → `null`.
  - If cancellation fires before the delegate is dispatched, `Task.Run` completes `Canceled`
    without ever invoking the delegate; the resulting `TaskCanceledException` is likewise
    swallowed by the same generic catch → `null`.

  Because both branches of the race converge on `null`, this sibling test cannot fail
  regardless of thread-pool pressure — it is not "passing under load" because the code is
  correct, but because its expected outcome is invariant to the very race that makes the
  other test flaky. This confirms the hypothesis's core claim precisely and rules out an
  alternative explanation (e.g., a logic defect specific to the memory-stream test case).

### Hypothesis verdict: CONFIRMED

The stated hypothesis is confirmed in full, with one refinement: the delayed cancellation
callback (not only delayed delegate dispatch) also contributes to the observed ~18s duration,
because the `CancellationTokenSource` timer callback itself is thread-pool-serviced and can be
delayed under the same starvation that delays the delegate. The `catch (TimeoutException)`
mismatch at `TimeOutTask.cs:199` is real and independently corroborated by the existing test
suite's own workaround pattern (simulating timeouts by throwing `TimeoutException` directly
rather than exercising the real timer).

## 2. Root Cause Classification

**Both** a production defect and a test-design policy violation are present; they are
independent defects that compound to produce the observed flakiness.

### 2.1 Production defect

`TimeOutTask.RunWithTimeout<T1,TResult>` (`TimeOutTask.cs:176-229`) catches `TimeoutException`
at line 199 instead of `TaskCanceledException`, unlike every sibling overload in the same file.
This means the overload's retry-on-timeout behavior is dead code for the only kind of timeout
its own internal `CancellationTokenSource` mechanism can actually produce. Any real caller of
this overload (currently only `OneDriveDownloader.TryGetFileStreamWriter`) gets zero retries on
a genuine timeout and instead fails on the first attempt via the generic exception handler.
This is a correctness/robustness gap independent of any test.

### 2.2 Test-design policy violation

`TryGetFileStreamWriter_WhenWriterReturnsMemoryStream_ReturnsStream` exercises a purely
synchronous, non-blocking, deterministic operation (`() => new MemoryStream()`) through a real
thread-pool dispatch (`Task.Run`) guarded by a real wall-clock `CancellationTokenSource(5000)`,
with no seam available to substitute a deterministic execution path. This violates:

- **CLAUDE.md — General Unit Test Policy, UT4 ("External Dependencies and Environment")**:
  "Environment Stability — Tests must not rely on mutable global state or external
  configuration that can change between runs." Thread-pool occupancy across concurrently
  running test classes is exactly this kind of externally mutable, run-to-run-varying state.
- **`.claude/rules/csharp.md`, "Deterministic Test Rules"**: "Unit tests must not depend on
  ... Use seam-based mocking for all external boundaries (processes, HTTP, filesystem,
  **clocks**). Tests must produce identical results in the IDE test runner and in CLI runs so
  local and CI behavior agree." The test's outcome differs between the VS Test Explorer and
  VS Code runners — the exact symptom this rule exists to prevent — because no clock/timer
  seam is exposed on the call path it exercises.
- **`.claude/rules/csharp.md`, "DI Seams"**: the repo's mandated seam preference order
  (interface > injectable delegate > adapter) has not been applied to the
  timeout-wrapping step of `TryGetFileStreamWriter`; the only seams present
  (`GetFileStreamWriter`, `ClientGetAsync`) stop short of the actual nondeterministic
  boundary (the `RunWithTimeout` invocation itself).
- **`.claude/rules/general-unit-test.md`, "Determinism Infrastructure"**: "Controllable clock
  — ... Do not read wall-clock time directly in production code under test" and the banned-API
  list for tests (real wall-clock waits are prohibited). The test indirectly depends on a real
  `CancellationTokenSource` timer with no injected/fake time source.

Note: `.claude/rules/general-unit-test.md` states repository-wide coverage floors of 85%
line / 75% branch under a T1–T4 tier model, while CLAUDE.md's embedded C# Unit Test Policy and
this task's own instructions state an 80% line floor with a 90% floor for new/changed code and
no tier model. Per this repository's Policy Compliance Order, CLAUDE.md is read first; prior
project memory (`.claude/agent-memory/task-researcher/project_claude_governance_sync_178.md`)
records that the 80/90 threshold was deliberately retained over the 85/75/tiers alternative
during a prior governance sync. Section 5 below uses the 80%/90% floor accordingly. This
inconsistency between the two rule files is noted for the orchestrator's awareness; it does not
affect the determinism findings above, which are cited from the "Deterministic Test Rules" and
"Determinism Infrastructure" sections that are consistent across both documents.

## 3. Candidate Fixes

### Option (a) — Injectable delegate seam on the writer-timeout step (RECOMMENDED)

Add the smallest DI seam directly around the nondeterministic boundary: the invocation of
`RunWithTimeout` inside `TryGetFileStreamWriter`. Per the repo's seam preference order
(interface > injectable delegate > adapter), and because this is a single call path (not a
multi-method external dependency warranting a full interface), an **injectable delegate**
is the appropriate seam — consistent with the class's existing style, which already uses
injectable `virtual` delegate properties (`ClientGetAsync`, `GetFileStreamWriter`) for its
other two external-ish boundaries.

Concrete design:
- Add `protected internal virtual Func<Func<string, Stream>, string, CancellationToken, int, Task<Stream>> WriterTimeoutRunner { get; protected set; }`
  to `OneDriveDownloader`, defaulting to
  `(factory, path, token, ms) => factory.RunWithTimeout(path, token, ms, 3, false)` — i.e.,
  today's exact production behavior, unchanged.
- Change `TryGetFileStreamWriter` (lines 88-97) to call
  `await WriterTimeoutRunner(GetFileStreamWriter, destinationPath, cancel, timeoutMs);`
  inside the existing `try`/`catch (Exception)` block. No other production behavior changes.
- Add `TestableOneDriveDownloader.SetWriterTimeoutRunner(Func<...> func)` (test-only helper,
  mirroring the existing `SetClientGetAsync`/`SetFileStreamWriter` pattern) so the failing
  test can inject a synchronous, non-timer, non-thread-pool runner, e.g.
  `(factory, path, token, ms) => Task.FromResult(factory(path))`.
- Rewrite `TryGetFileStreamWriter_WhenWriterReturnsMemoryStream_ReturnsStream` to inject this
  deterministic runner. The test still exercises the real `TryGetFileStreamWriter` method body
  (its try/catch wrapper contract) and the real `GetFileStreamWriter` factory substitution; it
  only removes the real timer/thread-pool dependency, which is not the concern under test.

This preserves production default behavior exactly (AC2), keeps the wrapper contract under
test (AC3 — writer-returns-stream still exercises the real method; writer-throws can keep
using the same deterministic runner or the existing exception-throwing writer factory, both of
which are now deterministic), and removes all real-timer/thread-pool dependency from the test
(AC1, AC4). It does not touch `TimeOutTask.cs` at all, so the exception-catching defect
identified in Section 2.1 is not conflated with this fix — consistent with the Bugfix
Workflow's "if you uncover deeper design problems, open a new issue instead of widening scope."

### Option (b) — Reframe/relocate the test to avoid the real timer without a production seam

`TryGetFileStreamWriter` is already `virtual`; `TestableOneDriveDownloaderFull` (test file,
lines 37-66) already demonstrates a pattern that overrides `TryGetFileStreamWriter` entirely,
bypassing `RunWithTimeout` altogether, for the `DownloadFileAsync_*` tests. The failing test
could analogously be rewritten against a subclass that overrides `TryGetFileStreamWriter` and
never exercises `RunWithTimeout`.

**Rejected**: this would eliminate the test's ability to verify the actual wrapping/timeout
behavior of `TryGetFileStreamWriter` (the very method the test method name claims to cover) —
it would instead just be testing that `GetFileStreamWriter("ignored")` returns a `MemoryStream`,
which duplicates coverage already present in
`GetFileStreamWriter_DefaultWriterWithNulPath_ThrowsNotSupportedException`-style tests and
narrows meaningful coverage of the wrapper contract without any offsetting benefit over
Option (a).

### Option (c) — Copilot's proposed production change: catch `TaskCanceledException` in the sync overload

Evaluated directly against the existing test suite. Two variants:

- **Replace** `catch (TimeoutException)` with `catch (TaskCanceledException)` at
  `TimeOutTask.cs:199`. This **breaks** two existing regression tests that rely on the current
  behavior:
  - `TimeOutTask_OverloadCoverageTests.cs:105-122`
    (`RunWithTimeout_FuncT1TResult_ShouldReturnDefault_WhenTimeoutOccursWithoutRetries`) — the
    delegate throws `TimeoutException` directly with `strict: true`; under the replacement,
    this exception would no longer be caught by the retry branch, would fall to the generic
    `catch (System.Exception e)`, and — because `strict: true` — would be **rethrown**,
    turning an expected `result.Should().BeNull()` into an unhandled exception.
  - `TimeOutTask_AdditionalTests.cs:147-173`
    (`RunWithTimeout_FuncT1TResult_ShouldRetryAfterTimeoutException`) — same mechanism; the
    expected retry-and-succeed path (`attempts.Should().Be(2)`) would instead throw on the
    first `TimeoutException`.
  This variant is confirmed to **regress existing, policy-compliant tests** and must not be
  implemented as a bare replacement.
- **Add** `catch (TaskCanceledException)` alongside the existing `catch (TimeoutException)`
  (does not break the two tests above, since `TimeoutException` is still caught by its own
  clause). This would restore retry-on-real-cancellation parity with the sibling overloads —
  a legitimate fix for the Section 2.1 production defect — but it does **not** by itself make
  `TryGetFileStreamWriter_WhenWriterReturnsMemoryStream_ReturnsStream` deterministic: it would
  extend the total time budget (retries), not remove the underlying race against thread-pool
  scheduling latency described in Section 1.3. Under sustained starvation the delegate could
  still fail to be dispatched within any of the (now more numerous) attempts, and the total
  wall-clock exposure of the test would grow (up to `maxAttempts + 1` timeout windows). Using
  a production retry/timing change as *the* mechanism to stabilize a unit test's outcome is
  precisely the pattern `.claude/rules/csharp.md`'s "Prohibited Behaviors" list forbids:
  "Adding sleeps, retries, or timing hacks to mask flaky behavior."

**Conclusion on Option (c)**: the underlying exception-type mismatch is a real, independently
worth-fixing production defect (Section 2.1), but it is not a substitute for a determinism fix
in the test, and the naive replacement form actively regresses existing coverage. If pursued,
it must be scoped as a separate change (additive `catch (TaskCanceledException)` clause) with
its own regression tests, decoupled from the flaky-test fix.

### Rejected alternatives (summary)

- Option (b): rejected — narrows meaningful test coverage of `TryGetFileStreamWriter`'s wrapper
  contract without a compensating benefit over Option (a).
- Option (c) as a bare production fix: rejected — the "replace" form breaks two existing
  `TimeOutTask` retry tests; the "additive" form does not resolve test determinism and risks
  violating the repo's prohibition on retry/timing hacks as a flakiness mask, if used for that
  purpose.

## 4. Recommendation: Test-Only vs. Production Touch

The recommended fix (Option a) requires a small, additive production change (one new
`virtual` delegate property, defaulting to current behavior, plus routing the existing call
through it) and a test-only change (a new setter on the test-only subclass and a rewrite of the
failing test). It is not purely test-only, because no existing seam reaches the specific
nondeterministic boundary (the `RunWithTimeout` call site) — per `.claude/rules/csharp.md`'s
DI Seams guidance, the correct response to a missing seam is to introduce the smallest one,
not to work around its absence from the test side alone (which is what Option (b) would do,
at the cost of coverage). The production change is additive-only: it does not alter behavior
for any existing caller because the default implementation of `WriterTimeoutRunner` is
byte-for-byte the current call (`GetFileStreamWriter.RunWithTimeout(destinationPath, cancel, timeoutMs, 3, false)`),
satisfying AC2 (production default behavior preserved).

The Section 2.1 production defect (`catch (TimeoutException)` mismatch in the sync `Func<T1,TResult>`
overload) should be filed as a separate follow-up issue rather than folded into this fix, per
the Bugfix Workflow's minimal-fix principle ("If you uncover deeper design problems, open a new
issue instead of widening scope"). Fixing it is not required to satisfy AC1–AC5 of issue #253,
since Option (a) removes the test's dependency on `RunWithTimeout`'s exception-handling behavior
entirely.

## 5. Files Changed (Recommended Fix)

- `UtilitiesCS/OneDriveHelpers/OneDriveDownloader.cs`
  - Add `WriterTimeoutRunner` virtual delegate property (backing field + getter/protected
    setter, mirroring the existing `GetFileStreamWriter`/`ClientGetAsync` pattern).
  - Change `TryGetFileStreamWriter` (lines 88-97) to invoke `WriterTimeoutRunner` instead of
    calling `GetFileStreamWriter.RunWithTimeout(...)` directly.
  - No change to `TimeOutTask.cs`.
- `UtilitiesCS.Test/OneDriveHelpers/OneDriveDownloader_Tests.cs`
  - Add `SetWriterTimeoutRunner` to `TestableOneDriveDownloader`.
  - Rewrite `TryGetFileStreamWriter_WhenWriterReturnsMemoryStream_ReturnsStream` to inject a
    deterministic, synchronous runner (no `Task.Run`, no `CancellationTokenSource`).
  - `TryGetFileStreamWriter_WhenWriterThrows_ReturnsNull` may be left unchanged (it already
    passes deterministically under both current code paths per Section 1.4), or optionally
    updated to also inject the deterministic runner for consistency and slightly stronger
    isolation from `TimeOutTask` behavior; either choice satisfies AC3.

Minimal production surface touched: one new `virtual` property and one call-site substitution
inside an existing `try`/`catch`, both in `OneDriveDownloader.cs`. `TimeOutTask.cs` is
untouched, so no existing `TimeOutTask_*` test is at risk.

## 6. Coverage Implications

- Per CLAUDE.md's C# Unit Test Policy (and this task's stated floor), repository-wide line
  coverage must remain `>= 80%`, and any new module/class/method must reach `>= 90%` coverage.
  `.claude/rules/general-unit-test.md` separately states an 85%/75% (line/branch) floor under a
  tier model; see the note in Section 2.2 on this discrepancy between the two rule documents.
- The new `WriterTimeoutRunner` property is a one-line default-factory expression plus a
  getter/setter, exercised on every existing call to `TryGetFileStreamWriter` (both the
  production default path via `OneDriveDownloader` and the injected path via
  `TestableOneDriveDownloader`); it does not introduce untested branches.
- The rewritten test continues to exercise the same production method (`TryGetFileStreamWriter`)
  and its existing `try`/`catch` — no lines in `OneDriveDownloader.cs` lose coverage; the fix
  changes *how* the call reaches `TryGetFileStreamWriter`'s internals (via the seam) rather than
  removing any assertion or code path.
- No new production file is introduced, so the 90% new-module floor is not separately
  implicated; the changed lines in `OneDriveDownloader.cs` should be fully covered by the
  existing test class (both the memory-stream success test and the writer-throws test already
  exercise the modified code path).

## 7. Testing Strategy Notes

- Keep the rewritten test in `UtilitiesCS.Test/OneDriveHelpers/OneDriveDownloader_Tests.cs`
  (existing location; no new test file needed).
- Use MSTest + FluentAssertions per repo convention (already in use in this file); no Moq
  needed since the seam is a plain delegate, consistent with the class's existing testable-
  subclass pattern rather than introducing a mocking framework for a single-call-path seam.
- Ensure the rewritten test asserts both the non-null stream and `CanWrite` (as today) so the
  wrapper contract (success path returns the factory's stream) remains verified end-to-end
  through `TryGetFileStreamWriter`.
- Confirm the full `OneDriveDownloader_Tests` class passes in both the Visual Studio Test
  Explorer (parallel) and VS Code runners with no multi-second duration, satisfying AC4.
- Run the full C# toolchain in order (CSharpier -> analyzers -> nullable/type-check -> MSTest)
  after the change, per CLAUDE.md's Policy Compliance Order and the C# Toolchain section,
  satisfying AC5.
