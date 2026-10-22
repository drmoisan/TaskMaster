# ETL deadline mechanics follow-ups (issue #825) — research

- Timestamp: 2026-09-08T23-55
- Issue: #825
- Worktree: `<repo-root>/.claude/worktrees/agent-a8990832542606773`
- Branch at time of research: `TaskMaster-wt-2026-09-08T23-27` (clean; HEAD `6f08302a`)
- Method: file reads and ripgrep searches in the current tree only. No build, no test run, no
  decompiler. `pwsh` is refused under this worktree's isolation sandbox and the Bash tool is
  disabled for this session, so every claim below rests on file content read directly.

Every line number in this document was re-derived in the current tree. Where the issue text's
number or claim is wrong, an explicit **Correction:** line follows.

---

## R1 — The 250 ms per-row ETL budget

### The two occurrences

Both occurrences are in `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.Etl.cs`.

| # | Line | Enclosing method | Statement |
|---|---|---|---|
| 1 | 84 | `EtlAsync` (declared 66-76) | `int milliseconds = 250 * rowCount;` |
| 2 | 149 | `EtlAsyncOld` (declared 134-144, name on 137) | `int milliseconds = 250 * rowCount;` |

The issue reports "around lines 84 and 149". **Both are exact.** No correction needed.

`rowCount` is read at line 83 (site 1) and line 148 (site 2), in both cases from
`table.GetRowCount()`.

### How each `milliseconds` value is consumed

**Site 1 — `EtlAsync` (line 84).** Two mutually exclusive branches consume it:

- By-row branch (lines 92-109): `milliseconds` is passed as the `timeout` argument at line 105 to
  the private `EtlByRowAsync(Table, Dictionary, Dictionary, CancellationToken, int timeout,
  TimeProvider?, ProgressTracker?)` declared at lines 231-239. Inside that method the same value is
  applied **twice, independently**: line 247 `.TimeoutAfter(timeout, timeProvider)` on the
  `CastToRowArray` hop, and line 261 `.TimeoutAfter(timeout, timeProvider)` on the `EtlByRow`
  transform hop. A one-row folder therefore gets 250 ms for each of two hops, not a shared 250 ms
  budget for both.
- Snapshot branch (lines 110-117): line 116 `.TimeoutAfter(milliseconds, timeProvider)` on
  `Task.Run(() => table?.GetArray(table.GetRowCount()) as object[,], token)`.

All three call sites bind to the proxy-returning overload
`TimeOutTask.TimeoutAfter<TResult>(this Task<TResult>, int millisecondsTimeout, TimeProvider?
timeProvider = null)` at `UtilitiesCS/Threading/TimeOutTask.cs:862-866`. That overload arms the
deadline at `TimeOutTask.cs:888` with
`(timeProvider ?? TimeProvider.System).CreateTimer(...)`. The `TimeProvider` reaches `EtlAsync`
through its own optional parameter at `OlTableExtensions.Etl.cs:75`
(`TimeProvider? timeProvider = null`, documented at lines 73-74), which production leaves null and
tests fill with a `FakeTimeProvider`.

**Site 2 — `EtlAsyncOld` (line 149).** Consumed once, at line 160:
`.TimeoutAfter(milliseconds, attempts)`, where `attempts` is an `int` declared at line 150
(`var attempts = 3;`). Because the second argument is an `int`, this binds to the inert
`(int, int)` overload at `TimeOutTask.cs:824-828` — see R4. **`EtlAsyncOld` has no `TimeProvider`
parameter and is therefore not under test clock control at all.**

### Are the two sites on the same call path?

**No — they are independent.** `EtlAsync` has one production caller,
`UtilitiesCS/Extensions/DfDeedle.cs:172`. `EtlAsyncOld` has **zero** production callers; its only
caller anywhere is the test at
`UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs:1003`. Neither method calls the
other.

### Is a real ETL-duration-versus-folder-size measurement obtainable here?

**In the test environment: no.** In a live Outlook session: yes, and the instrumentation already
exists.

What exists:

- **Timing telemetry is already emitted.** `LogTableTiming` is defined at
  `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.cs:39-46` and writes at log4net `Debug` level.
  It is called with an elapsed-milliseconds payload at:
  - `OlTableExtensions.Etl.cs:127-130` — `"EtlAsync complete | ETL over table snapshots"` with
    `rowCount=…; columnCount=…; elapsedMs=…` from the `Stopwatch` started at line 80. **This is
    exactly the row-count-versus-duration pairing item 1 asks for.**
  - `OlTableExtensions.Etl.cs:59-62` — the same pairing for the synchronous `ETL`.
  - `OlTableExtensions.Etl.cs:352-355` and `388-391` — `CastToRowArray` row-extraction start/complete
    with `rowCount` and `elapsedMs`.
  - `OlTableExtensions.TableAccess.cs:41-44` and `66-69` — table acquisition, with `elapsedMs`.
  - `UtilitiesCS/Extensions/DfDeedle.cs:191-194` — `etlElapsedMs` alongside `rowCount` and
    `columnCount`, via `LogDfTiming` (`DfDeedle.cs:41-48`).
- **The Debug level is enabled and persisted.** `TaskMaster/log4net.config:4` sets
  `<level value="ALL" />` on `<root>`, and the `all_logs_file` `RollingFileAppender`
  (`TaskMaster/log4net.config:20-32`) writes to `logs\` with date pattern
  `'debug_'yyyy-MM-dd'.log'`. So a live add-in run already records the measurement.

What does not exist:

- **No benchmark harness.** Repo-wide search of every `*.csproj` for `BenchmarkDotNet` or
  `Stopwatch` returns zero matches. There is no benchmark project, no perf test project, and no
  timing runner.
- **No recorded measurement.** I enumerated the full evidence trees for both prior issues:
  `docs/features/active/2026-09-06-quickfiler-crash-column-add-timeout-swallowed-keynotfound-798/evidence/**`
  (53 files) and
  `docs/features/active/2026-09-07-utilitiescs-test-determinism-780-803-594-811/evidence/**`
  (46 files). Neither contains an ETL duration capture. The #798 file
  `evidence/regression-testing/p2-ac2-timing-fail-before.md` is a fail-before record for a
  deadline-behaviour test, not a duration measurement. A repo-wide search for `elapsedMs` outside
  source returns only spec/plan/research prose and TRX files (test durations, not ETL durations).
- **The test environment cannot produce one.** Per the issue's own Environment section and confirmed
  in the fixtures, `Outlook.Table`, `MAPIFolder` and `Explorer` are Moq objects. In
  `OlTableExtensionsEtlClockTests.CreateTableWithColumns` (lines 51-89) `GetRowCount`, `GetNextRow`
  and `GetArray` are `Setup(...)`-backed and return in microseconds. Any duration measured against
  them measures Moq dispatch, not COM enumeration, so the number would be meaningless as a basis for
  a per-row budget.

**Conclusion for planning.** A measurement is obtainable only from a live Outlook session by
collecting the already-emitted `[Table timing] EtlAsync complete` lines across folders of differing
`rowCount`. That is outside this repository's automated environment. Absent such a capture, the
correct action is to record the absence and **leave `250 * rowCount` unchanged**. Substituting a
differently-guessed constant would replace one unjustified number with another and would be a
timing change with no deterministic test, which is the same reason #811 excluded it.

---

## R2 — The residual 2000 ms `GetTableInViewAsync` window

### Exact signature and the default

`UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs`, method spans **lines 32-119**
(issue says "roughly 32 to 119" — exact).

```
32        public static async Task<Outlook.Table> GetTableInViewAsync(
33            this Explorer activeExplorer,
34            CancellationToken token,
35            int counter,
36            int timeoutMs = 2000,
37            Func<int, CancellationTokenSource>? timeoutSourceFactory = null
38        )
```

- The `timeoutMs = 2000` default is on **line 36**.
- The factory parameter is named **`timeoutSourceFactory`**, type
  `Func<int, CancellationTokenSource>?`, default `null`, on **line 37**.

### How the factory is consumed

`TableAccess.cs:57-64` forwards it verbatim:

```
57                table = await TimeOutTask.RunWithTimeout(
58                    view.GetTable,
59                    token,
60                    timeoutMs,
61                    1,
62                    false,
63                    timeoutSourceFactory
64                );
```

That binds to the public `TimeOutTask.RunWithTimeout<TResult>(this Func<TResult>, CancellationToken,
int, int, bool, Func<int, CancellationTokenSource>?)` at `TimeOutTask.cs:21-38`, which forwards to
the private overload at `TimeOutTask.cs:40-48`. The factory is resolved at `TimeOutTask.cs:52-54`:

```
52            using var timeoutSource = (
53                timeoutSourceFactory ?? (ms => new CancellationTokenSource(ms))
54            )(milliseconds);
```

and re-threaded through the internal retry at `TimeOutTask.cs:77`.

`GetTableInViewAsync` also re-threads it through its own two recursive retries:
`TableAccess.cs:82-87` (propagates `timeoutMs`) and `TableAccess.cs:103-108` (hard-codes `2000`,
with the rationale comment at lines 100-102).

### Correction — the factory is NOT threaded from `DfDeedle.GetEmailDataInViewAsync`

The issue states the 2000 ms window "is reached from `DfDeedle.GetEmailDataInViewAsync` and is
seamed by a `Func<int, CancellationTokenSource>` factory". The first half is true; the second half
is **false in the current tree**, and this is the most consequential correction in this document.

**Correction:** `DfDeedle.GetEmailDataInViewAsync` is declared at `UtilitiesCS/Extensions/DfDeedle.cs:128-136`
(the issue's "around line 128" points at the declaration, not a call). Its only call to
`GetTableInViewAsync` is at **`DfDeedle.cs:148`**:

```
148            Outlook.Table table = await activeExplorer.GetTableInViewAsync(token, 0);
```

Two arguments. Neither `timeoutMs` nor `timeoutSourceFactory` is supplied, and
`GetEmailDataInViewAsync`'s own parameter list (`Explorer`, `CancellationToken`,
`CancellationTokenSource`, `ProgressTracker`, `TimeProvider? timeProvider = null`) contains no
parameter capable of carrying either. **The factory seam is unreachable from the production
`DfDeedle` path.** Only a caller that invokes `GetTableInViewAsync` directly can use it.

### Every call site of `GetTableInViewAsync` in the repository

Production (1 external + 2 self-recursive):

| File:line | Nature |
|---|---|
| `UtilitiesCS/Extensions/DfDeedle.cs:148` | the only external production caller; passes `(token, 0)` |
| `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs:82` | self-recursion from the `TaskCanceledException` branch |
| `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs:103` | self-recursion from the `TimeoutException` branch |

Tests (4, all in one class, all through the reflection helper `InvokeAsyncResult`
declared at `OlTableExtensions_Tests.cs:1820-1842`, binding the 5-parameter signature):

| File:line (test method) | `timeoutMs` | factory | Reaches the real 2000 ms window? |
|---|---|---|---|
| `UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs:1238` `GetTableInViewAsync_NullTableView_ThrowsInvalidOperationException` | 2000 (line 1259) | `null` | No — throws at `TableAccess.cs:49` before `RunWithTimeout` |
| `…/OlTableExtensions_Tests.cs:1267` `GetTableInViewAsync_SlowSynchronousGetTable_ReturnsTableWithoutSyntheticRetry` | 5 (line 1315) | injected (line 1300) | No — the factory ignores `ms` and returns a test-held source |
| `…/OlTableExtensions_Tests.cs:1324` `GetTableInViewAsync_CanceledToken_PropagatesOperationCanceledException` | 2000 (line 1346) | `null` | No — pre-cancelled token throws at `TimeOutTask.cs:50` |
| `…/OlTableExtensions_Tests.cs:1646` `GetTableInViewAsync_ImmediateSuccess_CallsGetTableOnceAndReturnsSnapshot` | 2000 (line 1675) | `null` | **Yes** — a real `new CancellationTokenSource(2000)` on the system clock |

Indirect exposure through `DfDeedle.GetEmailDataInViewAsync` (which always takes the 2000 ms
default and a null factory) — 3 further tests:

- `UtilitiesCS.Test/Extensions/DfDeedleEtlTimeoutTests.cs:148` (test at line 135)
- `UtilitiesCS.Test/Extensions/DfDeedleEtlTimeoutTests.cs:194` (test at line 187)
- `UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs:473` (test at line 419)

So four tests across three classes actually arm the real 2000 ms system-clock window.

### `TimeProviderTaskExtensions.CreateCancellationTokenSource` — VERIFIED PRESENT

The issue records this as unverified. It is now verified against the package actually referenced by
this repository. Method and evidence, stated honestly:

1. **Referenced version.** Identical in both projects:
   - `UtilitiesCS/packages.config:28` — `<package id="Microsoft.Bcl.TimeProvider" version="10.0.11" targetFramework="net481" />`
   - `UtilitiesCS.Test/packages.config:23` — same line, same version string **`10.0.11`**.
2. **Assembly reference and hint path.**
   - `UtilitiesCS/UtilitiesCS.csproj:96` —
     `<Reference Include="Microsoft.Bcl.TimeProvider, Version=10.0.0.11, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51, processorArchitecture=MSIL">`
   - `UtilitiesCS/UtilitiesCS.csproj:97` —
     `<HintPath>..\packages\Microsoft.Bcl.TimeProvider.10.0.11\lib\net462\Microsoft.Bcl.TimeProvider.dll</HintPath>`
   - `UtilitiesCS.Test/UtilitiesCS.Test.csproj:608-609` — identical pair.
3. **TFM folder actually consumed.** **`lib/net462/`**, not `lib/netstandard2.0/`. The prompt's
   example named `netstandard2.0`; the HintPath pins `net462` explicitly, so no NuGet TFM
   resolution occurs at build time. The package does ship `lib/net8.0/`, `lib/netstandard2.0/` and
   `lib/net462/`, each with a `.dll` and an `.xml`; only `net462` is referenced.
4. **Physical assembly presence.** `Microsoft.Bcl.TimeProvider.dll` and
   `Microsoft.Bcl.TimeProvider.xml` both exist in the solution package cache at
   `<repo-root>/packages/Microsoft.Bcl.TimeProvider.10.0.11/lib/net462/`.
   **Note for the executor:** this worktree has **no** `packages/` directory of its own
   (`.gitignore:190` excludes it). A NuGet restore is required in the worktree before any build.
5. **Member presence — how I determined it.** I cannot run a decompiler, so I used two checks:
   - *In-repo precedent:* none. Repo-wide search for `TimeProviderTaskExtensions` and for
     `CreateCancellationTokenSource` returns **zero** hits in any `*.cs` file; all hits are in
     `docs/**` prose.
   - *XML documentation file shipped beside the net462 assembly*, which is generated from the same
     compilation and is authoritative for the public surface. In
     `<repo-root>/packages/Microsoft.Bcl.TimeProvider.10.0.11/lib/net462/Microsoft.Bcl.TimeProvider.xml`:
     - **line 157** — `<member name="T:System.Threading.Tasks.TimeProviderTaskExtensions">`
     - **line 199** — `<member name="M:System.Threading.Tasks.TimeProviderTaskExtensions.CreateCancellationTokenSource(System.TimeProvider,System.TimeSpan)">`
     - lines 200-204 confirm the shape: extension over `TimeProvider`, parameter `delay` of type
       `TimeSpan`, returns a `CancellationTokenSource` that "will be canceled after the specified
       delay".

   **Result: the method exists in the net462 assembly this repository references.** Usable form:
   `(timeProvider ?? TimeProvider.System).CreateCancellationTokenSource(TimeSpan.FromMilliseconds(timeoutMs))`.

   Two caveats the planner must carry forward:
   - The XML doc's own remark at **lines 211-214** states that on pre-.NET 8 runtimes, calling
     `CancelAfter(TimeSpan)` on the returned source does **not** terminate the original delay timer.
     net481 is pre-.NET 8. No current code calls `CancelAfter` on a source produced this way, so
     this is a constraint on future edits rather than a present defect.
   - Strictly, I read the documentation file rather than the IL. If a build-time proof is wanted,
     the settling experiment is one line: add
     `_ = TimeProvider.System.CreateCancellationTokenSource(TimeSpan.FromMilliseconds(1));`
     inside any existing method in `UtilitiesCS`, then run
     `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`.
     A clean compile confirms it; `CS1061` refutes it.

### The alternative option — a test-supplied never-cancelling factory

**The current seam already permits it with zero production change**, but only for direct callers.

Seam signature (unchanged): `OlTableExtensions.TableAccess.cs:37` —
`Func<int, CancellationTokenSource>? timeoutSourceFactory = null`.

A never-cancelling factory is simply `_ => new CancellationTokenSource()` — a source constructed
without a delay is never scheduled to cancel. There is an established in-repo precedent for
supplying a test-held source through exactly this seam at
`UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs:1300`:
`Func<int, CancellationTokenSource> timeoutSourceFactory = _ => injectedSource;`, consumed at line
1316.

**However**, because of the correction above, this option cannot reach the three tests that go
through `DfDeedle.GetEmailDataInViewAsync` (`DfDeedleEtlTimeoutTests.cs:148, 194`,
`DfDeedle_COM_Tests.cs:473`). Covering those requires a production signature change to
`GetEmailDataInViewAsync` to carry a factory — which is the same production-diff cost as unifying on
`TimeProvider`, but leaves the call path with two seam types instead of one.

---

## R3 — The `EtlAsync` null-through-suppression tuple contract

### Return type and the suppression

`UtilitiesCS/OutlookObjects/Table/OlTableExtensions.Etl.cs`, line 66:

```
66        public static async Task<(object[,] data, Dictionary<string, int> columnInfo)> EtlAsync(
```

Return type: `Task<(object[,] data, Dictionary<string, int> columnInfo)>` — the `data` element is
**non-nullable**.

- `data` is declared `object[,]? data = null;` at line 85.
- The swallow is at lines 119-125: `catch (TimeoutException)` at 119, `logger.Error(...)` at 121-123,
  `tokenSource.Cancel();` at 124.
- The suppressed return is at **line 131**: `return (data!, columnDictionary);`

**Correction (minor):** the issue implies the return immediately follows the swallow. In the current
tree a `LogTableTiming` call sits between them at lines 127-130. That call dereferences only
`columnDictionary` and `etlStopwatch`, so it is null-safe on the timeout path, but a plan that edits
this region must account for it.

### Every consumer of `EtlAsync` in the repository — FIVE (1 production, 4 test)

| # | File:line | Project | What it does with `data` |
|---|---|---|---|
| 1 | `UtilitiesCS/Extensions/DfDeedle.cs:172` | UtilitiesCS (production) | Assigns to `var tableSnapshot`. **Null-checks** at line 182 (`if (tableSnapshot.data is null)`) and throws `InvalidOperationException` at 184-188. Then dereferences `tableSnapshot.Item1` at 193 and 210, and `tableSnapshot.Item2` at 200 and 210. This is the guard #811 added. |
| 2 | `UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs:968` | test | Destructures `var (data, columnInfo)`. **No null check.** Dereferences `data[0,0]`, `data[0,1]`, `data[0,2]` at lines 978-980. |
| 3 | `UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensionsEtlClockTests.cs:128` | test | Captures the `Task` in a local, awaits at line 141 into `var (data, _)`. **Asserts `data.Should().BeNull()`** at line 144. This is the contract test. |
| 4 | `…/OlTableExtensionsEtlClockTests.cs:177` | test | Destructures. Dereferences `data[0,1]` at line 188. No null check. |
| 5 | `…/OlTableExtensionsEtlClockTests.cs:219` | test | Destructures. Dereferences `data[0,0]` at line 230. No null check. |

**Blast radius of changing the contract:** exactly one production file (`DfDeedle.cs`) and two test
files (`OlTableExtensions_Tests.cs`, `OlTableExtensionsEtlClockTests.cs`). Consumers 2, 4 and 5 are
green-path tests that would compile unchanged against `object[,]? data` only if the file has nullable
annotations enabled; `OlTableExtensions_Tests.cs` has no file-level `#nullable enable` (it uses
narrow `#nullable enable annotations` scopes at lines 1692-1697 and 1709/1825), and
`OlTableExtensionsEtlClockTests.cs` has none at all, so a nullable return type would not produce
CS86xx errors in them — but the assertions at 978-980, 188 and 230 would still need review.

### The test documenting the surviving contract

`UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensionsEtlClockTests.cs`

- Doc comment: lines 99-103 ("the TimeoutException is swallowed, a null data array is returned
  through a null-forgiving suppression, and the supplied token source is cancelled. This is the
  shape the DfDeedle guard now catches.").
- `[TestMethod]` at line 104; method
  `EtlAsync_DeadlineExpires_ReturnsNullDataAndCancelsTokenSource` declared at **line 105**.
- Mechanism: a `ManualResetEventSlim` gate (line 108) is wired as the table's `onGetNextRow` action
  (line 117), so the row read blocks; the clock is an `ArmingBarrierTimeProvider` wrapping a
  `FakeTimeProvider` (line 121); the test awaits `barrier.Armed` (line 139) then `barrier.Advance(250)`
  (line 140).
- **Exactly two assertions**, lines 144-145:
  - `data.Should().BeNull();`
  - `tokenSource.IsCancellationRequested.Should().BeTrue();`
- Implicit third assertion: `var (data, _) = await call;` at line 141. If `EtlAsync` were changed to
  rethrow, this `await` would throw and the test would fail before reaching either assertion.

**Consequence for a contract change.** Changing to `object[,]? data` with a rethrown
`TimeoutException` requires this test to be **rewritten, not deleted**: the await at line 141 must
become an exception assertion (for example
`await FluentActions.Awaiting(() => call).Should().ThrowAsync<TimeoutException>();`), the
`data.Should().BeNull()` assertion at 144 disappears, and the cancellation assertion at 145 must be
re-examined — under a rethrow, whether `tokenSource.Cancel()` is still called becomes a design
decision the plan must state explicitly.

### `EtlAsyncOld` (dead)

- Declared at `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.Etl.cs:134-171`; the signature
  opens at line 134 and the method name is on **line 137**.
- Return type: `Task<(object[,]? data, Dictionary<string, int>? columnInfo)>` — **already nullable**,
  which is precisely the shape item 3 proposes for `EtlAsync`. It is a working in-repo precedent.
- **Callers repo-wide: one, and it is a test.**
  `UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs:1003`, inside
  `EtlAsyncOld_WithBinaryAndObjectFields_ReturnsTransformedData` (lines 984-1015). Zero production
  callers. Confirmed by a repo-wide `*.cs` search for `EtlAsyncOld`, which returns exactly three
  hits: the declaration (`Etl.cs:137`), the test method name (line 985) and the call (line 1003).
- `EtlAsyncOld` is also the **only production caller of the inert `(int, int)` `TimeoutAfter`
  overload** (`Etl.cs:160`) — see R4.

---

## R4 — The inert `(int, int)` `TimeoutAfter` overloads

### All four line numbers confirmed correct

| Line | Full signature |
|---|---|
| **824** | `public static Task<TResult> TimeoutAfter<TResult>(this Task<TResult> task, int millisecondsTimeout, int repeatAttempts)` (parameter list spans 824-828) |
| **862** | `public static Task<TResult> TimeoutAfter<TResult>(this Task<TResult> task, int millisecondsTimeout, TimeProvider? timeProvider = null)` (parameter list spans 862-866) |
| **924** | `public static Task TimeoutAfter(this Task task, int millisecondsTimeout, int repeatAttempts)` (single line) |
| **949** | `public static Task TimeoutAfter(this Task task, int millisecondsTimeout, TimeProvider? timeProvider = null)` (parameter list spans 949-953) |

All four match the issue exactly. No correction. (The #798 follow-up artifact
`…-798/evidence/other/followup-promotions.md:35-36` additionally records the two `catch` clauses at
836 and 932; both are confirmed at those lines.)

### Why the `catch (TimeoutException)` can never execute

The inner overload has exactly **three** exit paths, and none of them throws synchronously. Taking
the generic pair (824 wrapping 862); the non-generic pair is structurally identical.

1. **`TimeOutTask.cs:869-874` — short-circuit #1.** `if (task.IsCompleted || (millisecondsTimeout ==
   Timeout.Infinite)) { return task; }`. Returns the caller's own task. No throw.
2. **`TimeOutTask.cs:880-885` — short-circuit #2 (the trap).**
   ```
   880            if (millisecondsTimeout == 0)
   881            {
   883                tcs.SetException(new TimeoutException());
   884                return tcs.Task;
   885            }
   ```
   A `TimeoutException` is constructed here — but it is **placed into** the `TaskCompletionSource`
   (`tcs`, created at line 877), faulting the proxy. `SetException` does not throw the exception at
   the call site; it stores it. Control returns normally at line 884, so the `try` at 832 completes
   without an exception and the `catch` at 836 is not entered. The exception surfaces only when the
   caller awaits the returned proxy — which happens at the *caller's* await, outside the `try`.
3. **`TimeOutTask.cs:888-921` — the timer path.**
   ```
   888            ITimer timer = (timeProvider ?? TimeProvider.System).CreateTimer(
   889                state =>
   890                {
   892                    var myTcs = (TaskCompletionSource<TResult>)state;
   895                    myTcs.TrySetException(new TimeoutException());
   896                },
   897                tcs,
   898                TimeSpan.FromMilliseconds(millisecondsTimeout),
   899                Timeout.InfiniteTimeSpan
   900            );
   ```
   The `TimeoutException` is raised at **line 895**, inside a timer callback, on a timer thread, at
   some point *after* line 921 has already returned `tcs.Task` to the caller. It faults the proxy;
   it does not propagate up the call stack of `TimeoutAfter`. The continuation registered at
   903-919 disposes the timer and marshals the real result if the source task wins the race.

The non-generic pair mirrors this exactly: short-circuit #1 at 956-961, `SetException` at line 970,
timer callback `TrySetException` at line 982, `return tcs.Task` at 1008.

**Therefore** at `TimeOutTask.cs:834`, `result = task.TimeoutAfter(millisecondsTimeout);` always
assigns and never throws. Lines 836-847 are unreachable, which makes dead:

- the `logger.Warn($"Task timed out. {repeatAttempts} attempts remaining.")` at line 838;
- the only reads of `repeatAttempts` at lines 839, 841 and 845 — so the parameter is never consulted;
- the recursive retry at line 841.

Line 848 returns the proxy produced at 834, unchanged. The overload is a pure pass-through with a
misleading parameter. Same for the non-generic version (934-938 dead; note it does not even log).

### File size

`UtilitiesCS/Threading/TimeOutTask.cs` is **1011 lines** (last content line 1011 is the closing
namespace brace). The issue says 1011 — **confirmed**. That is 511 lines over the repository's
500-line cap in `.claude/rules/general-code-change.md`.

### Every caller of the `(int, int)` overloads

**Production — one, and it is inside dead code:**

| File:line | Expression | Binds to |
|---|---|---|
| `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.Etl.cs:160` | `.TimeoutAfter(milliseconds, attempts)` inside `EtlAsyncOld`; `attempts` is `int` (declared line 150) | the generic `(int, int)` overload at `TimeOutTask.cs:824` |

**Internal self-recursion — two, both inside the dead `catch` blocks:** `TimeOutTask.cs:841` and
`TimeOutTask.cs:936`.

**Tests — two, both confirmed at the issue's line numbers:**

| File:line | Test method | Expression |
|---|---|---|
| `UtilitiesCS.Test/Threading/TimeOutTask_Tests.cs:197` | `TimeoutAfter_GenericTask_WithRepeatAttempts_ReturnsResult` (line 191) | `await task.TimeoutAfter(100, 3)` |
| `UtilitiesCS.Test/Threading/TimeOutTask_Tests.cs:210` | `TimeoutAfter_NonGenericTask_WithRepeatAttempts_CompletesSuccessfully` (line 204) | `task.TimeoutAfter(100, 3)` |

**Sibling `TimeOutTask` coverage classes swept — no further `(int, int)` callers.** I checked every
`TimeoutAfter` call site in each:

- `TimeOutTask_AdditionalTests.cs:27, 42, 57, 72, 88, 104` — all pass a `TimeProvider`
  (`FrozenClock()` or `clock`), so they bind to the 862/949 overloads.
- `TimeOutTask_InternalCoverageTests.cs:100` — single-argument.
- `TimeOutTaskCoverageTests.cs:19, 35, 37, 52, 53, 68, 69` — single-argument, named argument, or
  `Timeout.Infinite`.

**Structural note the planner needs:** `TimeOutTask_Tests` is a **single `partial` class spread over
four files** — `TimeOutTask_Tests.cs:11`, `TimeOutTask_OverloadCoverageTests.cs:9`,
`TimeOutTask_InternalCoverageTests.cs:9`, `TimeOutTask_AdditionalTests.cs:10`. The `[TestClass]` and
`[DoNotParallelize]` are declared once, at `TimeOutTask_Tests.cs:9-10`, and govern all four files'
tests. `TimeOutTaskCoverageTests` (`TimeOutTaskCoverageTests.cs:10`) is a **separate** class and
carries no `[DoNotParallelize]`.

### Would deleting the two overloads break a production caller?

**No, provided `EtlAsyncOld` is deleted in the same change.** `Etl.cs:160` is the only production
call site and it lives inside `EtlAsyncOld`, which has zero production callers. If the overloads were
deleted while `EtlAsyncOld` were retained, `Etl.cs:160` would fail to compile with a hard overload
resolution error (there is no `int`-to-`TimeProvider` conversion, so it cannot silently rebind to
the `(int, TimeProvider?)` overload) — a safe, loud failure mode rather than a silent behaviour
change. The two test callers at `TimeOutTask_Tests.cs:197, 210` and the test at
`OlTableExtensions_Tests.cs:1003` would also have to go.

Deleting `TimeoutAfter(824-849)` and `TimeoutAfter(924-940)` removes 43 lines from
`TimeOutTask.cs`, taking it from 1011 to 968 — still 468 over the cap. It reduces but does not
resolve the cap violation.

---

## R5 — The stale doc comment

### Verbatim text and line number

`UtilitiesCS/Extensions/DfDeedle.QfcColumns.cs`, **line 96** — confirmed, no correction:

```
        /// (CS1769); the same constraint already applies to the <c>TableEtlInvoker</c> seam.
```

It is the final line of the `<param name="columnAdder">` block spanning lines 91-97, which documents
`AddQfcColumnsAsync` (declared 102-109). The full block reads:

```
91        /// <param name="columnAdder">
92        /// Test seam for the column-adding work performed inside the timed <see cref="Task.Run"/>.
93        /// When null the production <see cref="AddQfcColumns"/> path is used. Declared as
94        /// <c>Action&lt;object, object&gt;</c> rather than over the interop types because embedded
95        /// interop types cannot be used as generic type arguments across an assembly boundary
96        /// (CS1769); the same constraint already applies to the <c>TableEtlInvoker</c> seam.
97        /// </param>
```

The defect is the present tense: it asserts a member currently exists.

### `TableEtlInvoker` no longer exists anywhere as a declaration

Repo-wide search: 54 occurrences across 21 files. **Only two are in `*.cs` files, and both are
comments — there is no declaration:**

1. `UtilitiesCS/Extensions/DfDeedle.QfcColumns.cs:96` — the stale reference above.
2. `UtilitiesCS.Test/Extensions/DfDeedleEtlTimeoutTests.cs:212` —
   `/// existing caller does now that the TableEtlInvoker static has been removed.` (part of the doc
   comment at lines 209-213 on `GetEmailDataInView_NoEtlArgument_UsesProductionDefaultDelegate`).
   **This one is historically accurate**: it describes the removal in the past tense rather than
   claiming the member exists. It is not stale in the same sense and does not require correction,
   though the plan may choose to reword it for clarity.

The remaining 52 occurrences are all in `docs/features/**` — issue/spec/plan/research/audit/evidence
records for #798, #811, #825 and older archived features. These are historical records and must not
be rewritten.

### What replaced it — the correct name for a corrected comment

The static was replaced by a default-delegate + optional-parameter pair in
`UtilitiesCS/Extensions/DfDeedle.cs`:

- **`DfDeedle.cs:67-70`** — the default delegate, with the identical CS1769 rationale documented
  above it at lines 62-66:
  ```
  67        private static readonly Func<
  68            object,
  69            (object[,] data, Dictionary<string, int> columnInfo)
  70        > DefaultTableEtl = t => ((Outlook.Table)t).ETL();
  ```
- **`DfDeedle.cs:74`** — the optional parameter on `GetEmailDataInView` (declared 72-75):
  ```
  74            Func<object, (object[,] data, Dictionary<string, int> columnInfo)>? etl = null
  ```
- **`DfDeedle.cs:83`** — the resolution: `(etl ?? DefaultTableEtl)(table)`.

The CS1769 constraint the stale comment appeals to is genuinely still illustrated by that pair —
`DefaultTableEtl` is declared over `object` for exactly that reason (see `DfDeedle.cs:62-66`). So the
corrected comment should name **`DefaultTableEtl`** (or "the `etl` seam on
`GetEmailDataInView`") in place of `TableEtlInvoker`; the surrounding sentence remains true.

### Sweep for other stale references to the removed static-property mechanism

No other in-code reference to a static-property ETL seam exists. Searched for `TableEtlInvoker`
(above) and for any `EtlInvoker`-shaped identifier; nothing further in `*.cs`. `MessageBoxInvoker`
(`DfDeedle.cs:54-60`) is a different, still-live static seam and is not affected.

---

## R6 — `[DoNotParallelize]` placement

### Complete enumeration in `UtilitiesCS.Test` (24 attribute applications)

Assembly setting for context: `UtilitiesCS.Test/Properties/AssemblyInfo.cs:18-21` —
`[assembly: Parallelize(Workers = 0, Scope = ExecutionScope.ClassLevel)]`.

| # | File:line | Reason comment present? | Comment (quoted) |
|---|---|---|---|
| 1 | `EmailIntelligence/ClassifierGroup_Tests.cs:18` | No | — |
| 2 | `Threading/UiThread_Tests.cs:11` | No | — |
| 3 | `Threading/UiThread_Tests.cs:372` | No (the preceding XML doc describes the test subject, not the attribute) | — |
| 4 | `Threading/UiThreadInitContract_Tests.cs:175` | No (preceding `<remarks>` is about `ApartmentState.Unknown`) | — |
| 5 | `Threading/UiThreadInitContract_Tests.cs:305` | No (preceding `<remarks>` is about counting factory invocations) | — |
| 6 | `Threading/TimeOutTask_Tests.cs:10` | **No** | — |
| 7 | `Threading/ThreadMonitorTests.cs:18` | **Yes** (14-15) | "Marked `<c>[DoNotParallelize]</c>` because the tests read/write the process-global `<see cref="CurrentStoreContext"/>`." |
| 8 | `Threading/ProgressTracker_Tests.cs:15` | No | — |
| 9 | `Threading/ProgressTrackerAsync_Tests.cs:13` | No | — |
| 10 | `ReusableTypeClasses/SerializableList_Tests.cs:15` | No | — |
| 11 | `Threading/IdleAsyncQueue_Tests.cs:29` | Partial (25-26) | "Tests invoke OnApplicationIdle via reflection to avoid depending on the live ApplicationIdleTimer firing timing." |
| 12 | `Threading/IdleActionQueue_Tests.cs:25` | **Yes** (21-22) | "IdleActionQueue uses static state (_entries, _subscribeGuard, _unsubscribe). Each test calls ResetStaticState() to ensure isolation." |
| 13 | `Threading/CurrentStoreContextTests.cs:16` | **Yes** (12-13) | "`<see cref="CurrentStoreContext.Current"/>` to `<see langword="null"/>` so the process-global state does not leak between tests." |
| 14 | `OutlookObjects/Table/OlTableExtensions_Tests.cs:21` | **Yes** (18-20) | "The console reason for this attribute was removed by the TextWriter seam under #811. It is retained because this class has not been soaked under class-level parallelism and ten of its tests drive the 2000 ms GetTableInViewAsync window." |
| 15 | `Threading/ApplicationIdleTimer_Tests.cs:17` | **Yes** (13-15) | "those classes lets a concurrent Subscribe leave the event non-null when Unsubscribe runs here, which prevents Stop() from decrementing subscriptionCount and produces a deterministic-looking false failure." |
| 16 | `ReusableTypeClasses/SmartSerializableSerializeGuardTests.cs:32` | Partial (28-29) | "No temporary file is created: writes are captured through the injectable stream-writer seam into a MemoryStream, and the timer is a deterministic manual-fire double." |
| 17 | `OutlookObjects/Table/OlTableExtensionsEtlClockTests.cs:21` | **Yes** (20) | "Not parallelized: this class drives a real Task.Run gate." |
| 18 | `ReusableTypeClasses/TimerWrapper_Tests.cs:15` | **Yes** (14) | "`[DoNotParallelize]` is retained: the constructor tests touch a real System.Timers.Timer." |
| 19 | `Extensions/DfDeedleQfcColumnTimeoutTests.cs:23` | **Yes** (19-20) | "Not parallelized: the assembly parallelizes at class level and this class drives process-wide log4net state that a concurrent class would observe or overwrite." |
| 20 | `EmailIntelligence/FolderRemapViewer_Tests.cs:27` | Partial (23-24) | "SetController tests inject an uninitialized controller whose _folderRemapTree and _mappings2 are set via reflection to avoid COM access." |
| 21 | `Extensions/DfDeedleEtlTimeoutTests.cs:24` | **Yes** (22-23) | "Not parallelized: this class drives a real Task.Run gate and shares the DfDeedle logger, exactly as DfDeedleQfcColumnTimeoutTests does." |
| 22 | `EmailIntelligence/FilterOlFoldersViewer_Tests.cs:27` | Partial (23-24) | "SetController tests inject an uninitialized controller whose folder view is set to a synthetic snapshot so that SetupTree() does not hit COM." |
| 23 | `OutlookObjects/Store/StoresWrapperTests.cs:22`, `StoresWrapperDisableTests.cs:17`, `StoreWrapperController_Tests.cs:14`, `StoreWrapperTests.cs:14`, `StoreWrapperControllerTests.cs:23`, `StoreWrapperViewerTests.cs:15` | No | — |
| 24 | `OutlookObjects/Store/StoresWrapperRehookTests.cs:20`, `StoreWrapperInitClockTests.cs:16`, `StoreWrapperInitProbeTests.cs:15`, `OutlookObjects/Folder/FolderPredictorTests.cs:19`, `OutlookObjects/Folder/WpfDispatcherYieldTests.cs:13` | Mixed; `StoreWrapperInitClockTests.cs:12-13` gives a reason ("the clock is process-global static state shared across tests"), the others do not | — |

(Rows 23 and 24 group the `Store`/`Folder` classes for brevity; each individual line is listed.)

Three further textual mentions of the attribute are prose, not applications:
`TestHelpers/UiThreadStateScope.cs:19`, `TestHelpers/UiThreadDispatcherScope.cs:23`,
`Threading/UiThreadInitContract_Tests.cs:20`, `Threading/IdleActionQueue_Tests.cs:42`,
`OutlookObjects/Folder/WpfDispatcherYieldTests.cs:157`.

### Out-of-scope notice

`UtilitiesCS.Test/Properties/AssemblyInfo.cs` (the assembly-level `Parallelize` attribute at lines
18-21) and the SDIL Reader test classes
(`UtilitiesCS.Test/NewtonsoftHelpers/SDILReader/MethodBodyReader_Tests.cs`, `ILInstruction_Tests.cs`,
`ILGlobals_Tests.cs`) are **owned by sibling feature 826 and are out of scope for any change in
this feature**. For completeness: none of the three SDIL Reader test classes currently carries
`[DoNotParallelize]`.

### In-scope classes

| Class | File | `[DoNotParallelize]`? |
|---|---|---|
| `OlTableExtensionsEtlClockTests` | `UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensionsEtlClockTests.cs` | Yes, line 21, reason at line 20 |
| `OlTableExtensionsRetryTests` | `UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensionsRetryTests.cs` | **No.** The class (lines 6-34) is 35 lines, two pure tests over `OlTableExtensions.RunTableRetry`, no attribute. Nothing to remove. |
| `OlTableExtensions_Tests` | `UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs` | Yes, line 21, reason at lines 18-20 |
| `TimeOutTask_Tests` (+ 3 partial siblings) | `UtilitiesCS.Test/Threading/TimeOutTask_Tests.cs` | Yes, line 10, **no reason comment**. Governs all four partial files. |
| `TimeOutTaskCoverageTests` | `UtilitiesCS.Test/Threading/TimeOutTaskCoverageTests.cs` | **No** (line 10 declares `[TestClass]` only) |
| `DfDeedleEtlTimeoutTests` | `UtilitiesCS.Test/Extensions/DfDeedleEtlTimeoutTests.cs` | Yes, line 24, reason at lines 22-23 |

### `OlTableExtensions_Tests` specifics

**Line count: 1846.** Confirmed — line 1846 is the closing namespace brace, line 1845 the closing
class brace. The issue's figure is exact.

**Tests driving the 2000 ms `GetTableInViewAsync` window: FOUR reference it, ONE actually arms the
real window.**

**Correction:** the class comment at line 20 and the issue both say "ten of its tests drive the
2000 ms GetTableInViewAsync window". The class contains **four** tests that call
`GetTableInViewAsync` at all — lines 1238, 1267, 1324 and 1646. Of those, per the table in R2, three
never reach `RunWithTimeout` with a real system-clock source: 1238 throws at `TableAccess.cs:49`;
1324 has a pre-cancelled token and throws at `TimeOutTask.cs:50`; 1267 supplies a test-held
`CancellationTokenSource` through the factory (line 1300), so its `timeoutMs: 5` is inert. **Only
`GetTableInViewAsync_ImmediateSuccess_CallsGetTableOnceAndReturnsSnapshot` (line 1646) arms a real
`new CancellationTokenSource(2000)`** — it passes `timeoutMs: 2000` (line 1675) and a `null` factory
(line 1676). The "ten" figure is not supportable and the class comment should be corrected regardless
of whether the attribute is removed.

**Shared mutable state — direct answer: none was found inside the class.** I searched the whole file
for statics, lifecycle hooks and process-global touches:

- **No `[ClassInitialize]`, `[TestInitialize]`, `[ClassCleanup]`, `[TestCleanup]`, or
  `[AssemblyInitialize]`.** Zero matches for any of those attribute names in the file.
- **No mutable static fields.** The only `static` members are five pure helper methods:
  `CreateReportingTracker` (1706), `CreateRowMock` (1714), `CreateTableWithColumns` (1748),
  `InvokeStatic` (1786), `InvokeStaticAsync<T>` (1804), `InvokeAsyncResult` (1820). No static
  field, no static mock, no static `TimeProvider`.
- **No `Console.SetOut`, no captured `TextWriter` field.** The only console-related item is the
  comment at line 1640 and the call at line 1641,
  `FluentActions.Invoking(() => mockTable.Object.EnumerateTable()).Should().NotThrow();`, which
  exercises the null-writer default path in `OlTableExtensions.EnumerateTable`
  (`OlTableExtensions.TableAccess.cs:382-430`, resolving `writer ?? Console.Out` at line 384). This
  is a **write to** `Console.Out`, not a redirection of it, and no assertion depends on where the
  text lands. It cannot corrupt another class's state; at worst a concurrent class's captured
  `DebugTextWriter` receives extra text it does not assert on.
- **No `TimeProvider` static.** Every clock is a per-test local `new FakeTimeProvider()`
  (for example line 974).
- **No wall-clock or timing dependency inside the test bodies.** Zero matches for `Thread.Sleep`,
  `Task.Delay`, `DateTime.Now`, `Stopwatch`, `Environment.` or `AppDomain` in the file.

**But removal is still not safe today, for a different reason.** The hazard is not shared mutable
state; it is a **real wall-clock deadline under thread-pool contention**. The single test at line
1646 arms a genuine 2 000 ms `CancellationTokenSource` and then asserts
`callCount.Should().Be(1)` (line 1680). Under class-level parallelism with `Workers = 0` (all cores)
and a saturated thread pool, if the `Task.Run` inside `TimeOutTask.RunWithTimeout`
(`TimeOutTask.cs:63`) is not dequeued within 2 000 ms, the linked token cancels, the
`TaskCanceledException` branch at `TableAccess.cs:71` fires, and `GetTableInViewAsync` recurses at
line 82 — producing `callCount == 2` and a failed assertion. That is a load-dependent,
non-deterministic failure of exactly the class the repository's determinism rules forbid.

The three classes at `DfDeedleEtlTimeoutTests.cs:135, 187` and `DfDeedle_COM_Tests.cs:419` carry the
same exposure through `DfDeedle.cs:148`.

**Answer to the decision question.** A soak is not a defensible basis for removal while item 2 is
unresolved. A soak can only sample the current machine's scheduling under the current suite
composition; the failure mode is a race whose probability rises with unrelated future test additions
and with slower CI hardware, so ten green runs would establish nothing durable. The defensible
sequence is: **fix item 2 first** (put the 2 000 ms window under the injected clock, or supply a
never-cancelling source at every call site), which removes the only identified hazard; **then**
remove `[DoNotParallelize]` from `OlTableExtensions_Tests` — at which point no soak is needed,
because the hazard is gone rather than merely unobserved. Correcting the class comment's "ten" claim
should happen either way.

The same reasoning covers `OlTableExtensionsEtlClockTests` and `DfDeedleEtlTimeoutTests`: both state
their reason as "drives a real `Task.Run` gate", which is accurate — both block a thread-pool item on
a `ManualResetEventSlim` (`OlTableExtensionsEtlClockTests.cs:108, 117`;
`DfDeedleEtlTimeoutTests.cs:139-142`) and rely on it not being released. That is a genuine reason to
keep the attribute and is unaffected by item 2. `TimeOutTask_Tests` carries the attribute with **no
recorded reason at all** (`TimeOutTask_Tests.cs:9-10`); its sibling `TimeOutTaskCoverageTests` runs
in parallel without it and exercises the same production surface, which is circumstantial evidence
that the attribute on `TimeOutTask_Tests` may be unnecessary — but that has not been demonstrated and
would need its own analysis.

---

## R7 — Ownership boundary confirmation (sibling feature 826)

### The two `Console.WriteLine` diagnostics

`UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs` contains **exactly two**
`Console.WriteLine` calls. Confirmed by a search over the whole `UtilitiesCS` project, which returns
these two and no others in this file. Both issue line numbers are correct.

**Line 79** — inside `catch (TaskCanceledException)` (opened at line 71), in the
`else` branch (77-93) taken when `token.IsCancellationRequested` is false:

```
                    Console.WriteLine($"Task timed out on try {counter}");
```

**Line 97** — inside `catch (TimeoutException)` (opened at line 95), the first statement of the
block:

```
                Console.WriteLine($"Task timed out on try {counter}");
```

Both lines have identical statement text; they differ only in leading indentation (line 79 has 20
spaces of indent, line 97 has 16). **A change in this feature must leave both bytes-identical,
including indentation**, since any reflow of the enclosing blocks would alter them.

### `Console.SetOut` restore patterns in the R6-scope test classes

**None exist.** Searched all of `UtilitiesCS.Test` for `Console.SetOut`, `Console.Out` and
`Console.WriteLine`. Results for the R6-scope classes:

- `OutlookObjects/Table/OlTableExtensions_Tests.cs` — one comment at line 1640; no `Console.SetOut`.
- `OutlookObjects/Table/OlTableExtensionsEtlClockTests.cs` — none.
- `OutlookObjects/Table/OlTableExtensionsRetryTests.cs` — none.
- `Threading/TimeOutTask_Tests.cs` and its three partial siblings — none.
- `Extensions/DfDeedleEtlTimeoutTests.cs` — none.

For completeness: 21 other classes in `UtilitiesCS.Test` call
`Console.SetOut(new DebugTextWriter())` (for example `Threading/AppGlobalsConverterTests.cs:27`,
`NewtonsoftHelpers/ScoDictionaryConverterTests.cs:25`,
`EmailIntelligence/Bayesian/BayesianClassifierTests.cs:22`). None of them restores the previous
writer — they are one-way installs, not save/restore pairs. All are outside both the R6 analysis
scope and this feature's write set.

---

## R8 — Build-mechanics facts

### Legacy non-SDK projects with explicit `<Compile Include>` entries — confirmed

**`UtilitiesCS/UtilitiesCS.csproj`** — line 2 is
`<Project ToolsVersion="15.0" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">`; there is
no `Sdk=` attribute and no globbing. Example entry, line 1065:

```
    <Compile Include="OutlookObjects\Table\OlTableExtensions.Etl.cs" />
```

(Siblings: `Extensions\DfDeedle.cs` at 996, `Extensions\DfDeedle.QfcColumns.cs` at 998,
`OutlookObjects\Table\OlTableExtensions.TableAccess.cs` at 1068, `Threading\TimeOutTask.cs` at 1113.)

**`UtilitiesCS.Test/UtilitiesCS.Test.csproj`** — line 2 is
`<Project ToolsVersion="15.0" DefaultTargets="Build" xmlns="http://schemas.microsoft.com/developer/msbuild/2003">`.
Example entry, line 544:

```
    <Compile Include="OutlookObjects\Table\OlTableExtensionsEtlClockTests.cs" />
```

(Siblings: `Extensions\DfDeedleEtlTimeoutTests.cs` at 193, `Threading\TimeOutTask_Tests.cs` at 500
and its three partial files at 497-499, `OutlookObjects\Table\OlTableExtensions_Tests.cs` at 543.)

**Consequence:** any new `.cs` file this feature adds — for example a split-out file to reduce
`TimeOutTask.cs` toward the 500-line cap, or a new test file — **must** be added as an explicit
`<Compile Include>` entry in the relevant `.csproj` or it will silently not compile.

### Target frameworks

| Project | `TargetFrameworkVersion` | `LangVersion` |
|---|---|---|
| `UtilitiesCS` | `v4.8.1` (`UtilitiesCS.csproj:16`) | `12.0` (`UtilitiesCS.csproj:10`) |
| `UtilitiesCS.Test` | `v4.8.1` (`UtilitiesCS.Test.csproj:17`) | `Latest` (`UtilitiesCS.Test.csproj:18`) |

### No `IsExternalInit` polyfill — confirmed

A repo-wide search for `IsExternalInit` returns **12 `*.cs` hits, every one of which is comment or
XML-doc prose documenting its absence**. There is no `class IsExternalInit` / `struct
IsExternalInit` declaration anywhere. Representative citations:

- `UtilitiesCS/OutlookObjects/Store/StoreIdentity.cs:17` — "because `<c>init</c>` accessors require
  `<c>System.Runtime.CompilerServices.IsExternalInit</c>`,"
- `UtilitiesCS/Threading/LockupStallDecider.cs:15` — same rationale
- `UtilitiesCS/OutlookObjects/Folder/FolderScore.cs:10` — "`<c>IsExternalInit</c>` polyfill."
- `QuickFiler/Interfaces/IQfcDatamodel.cs:64` — "`<c>net481</c>` has no `<c>IsExternalInit</c>` and
  therefore no `<c>record</c>`,"

Therefore `init` accessors, `record` and `record struct` fail with **CS0518** in these projects,
while a plain `readonly struct` (or a plain `struct` with `= default!` field initialisers) compiles.
There is an in-scope precedent for the working pattern at `UtilitiesCS/Extensions/DfDeedle.cs:262-292`
— `private struct EmailRecord` with the explanatory comment at lines 283-285: "`= default!` keeps
EmailRecord a plain struct (no record/init, which fail CS0518 on net481)".

### `[ExcludeFromCodeCoverage]` on in-scope files

**None.** Zero occurrences in any of:
`UtilitiesCS/OutlookObjects/Table/OlTableExtensions.Etl.cs`,
`UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs`,
`UtilitiesCS/OutlookObjects/Table/OlTableExtensions.cs`,
`UtilitiesCS/Threading/TimeOutTask.cs`,
`UtilitiesCS/Extensions/DfDeedle.cs`,
`UtilitiesCS/Extensions/DfDeedle.QfcColumns.cs`.

Nearest uses in the same assembly are unrelated: `UtilitiesCS/Threading/ThreadMonitor.cs:92, 104,
137, 201`; `UtilitiesCS/OutlookObjects/Folder/OutlookFolderNotificationSink.cs:36, 60, 93, …`. Every
line this feature might touch is therefore in the coverage denominator.

---

## Decision inputs for planning

**Item 1 — the 250 ms per-row ETL budget: LEAVE IT AND RECORD WHY.** The constant appears at
`OlTableExtensions.Etl.cs:84` (live, `EtlAsync`) and `:149` (dead, `EtlAsyncOld`), and in the by-row
branch it is applied twice independently (`Etl.cs:247, 261`), so a one-row folder actually gets
250 ms per hop rather than 250 ms total. The measurement the issue asks for is not obtainable in
this repository's environment: there is no benchmark harness in any `*.csproj`, no recorded ETL
duration in the 99 evidence files of #798 and #811, and the fixtures are Moq objects whose
`GetRowCount`/`GetNextRow`/`GetArray` return in microseconds, so anything measured against them would
characterise Moq, not COM. The telemetry to obtain the number already exists and is enabled
(`LogTableTiming` at `OlTableExtensions.cs:39-46`, the `rowCount`/`elapsedMs` payload at
`Etl.cs:127-130`, `<level value="ALL" />` at `TaskMaster/log4net.config:4`), but capturing it
requires a live Outlook session outside this environment. Record the absence and the capture recipe;
do not substitute another guessed constant.

**Item 2 — the 2 000 ms `GetTableInViewAsync` window: NEEDS A PLANNER DECISION, but the option space
has changed.** Two facts move it. First, `TimeProviderTaskExtensions.CreateCancellationTokenSource(TimeProvider,
TimeSpan)` is **verified present** in the referenced package: `Microsoft.Bcl.TimeProvider 10.0.11`
(`UtilitiesCS/packages.config:28`, `UtilitiesCS.Test/packages.config:23`), consumed from
`lib/net462/` per `UtilitiesCS.csproj:97`, with the member declared at line 199 of the shipped
`Microsoft.Bcl.TimeProvider.xml` beside that assembly. Option (b) is therefore viable, not
speculative. Second, the issue's premise that the factory is threaded from
`DfDeedle.GetEmailDataInViewAsync` is **false**: `DfDeedle.cs:148` calls `GetTableInViewAsync(token,
0)` with two arguments and `GetEmailDataInViewAsync` has no parameter able to carry a factory. That
makes option (a) incomplete on its own — it cannot reach the three tests that go through
`DfDeedle` (`DfDeedleEtlTimeoutTests.cs:148, 194`, `DfDeedle_COM_Tests.cs:473`) without a production
signature change of the same size as option (b), while leaving two seam types on one call path. The
evidence favours option (b), unifying on `TimeProvider`, but the planner must decide and must budget
for the pre-.NET 8 `CancelAfter` caveat documented at lines 211-214 of the package XML.

**Item 3 — the `EtlAsync` tuple contract: NEEDS A PLANNER DECISION; the blast radius is small and now
enumerated.** `EtlAsync` returns `Task<(object[,] data, Dictionary<string, int> columnInfo)>`
(`Etl.cs:66`) and forces a null through at `Etl.cs:131`. There are exactly **five** consumers: one
production (`DfDeedle.cs:172`, which already null-checks at line 182 and throws a descriptive
`InvalidOperationException` at 184-188) and four test (`OlTableExtensions_Tests.cs:968`,
`OlTableExtensionsEtlClockTests.cs:128, 177, 219`). `EtlAsyncOld` already uses the proposed
`object[,]?` shape (`Etl.cs:134-137`), so the pattern has an in-repo precedent. The change is
therefore three files, not "every consumer". Against that: the current shape is already guarded at
the only production consumer, so the change buys type-level clarity rather than a behaviour fix, and
it forces a deliberate rewrite of the contract test
`OlTableExtensionsEtlClockTests.EtlAsync_DeadlineExpires_ReturnsNullDataAndCancelsTokenSource`
(line 105) — whose `await` at line 141 and `data.Should().BeNull()` at line 144 both stop making
sense under a rethrow, and whose `tokenSource.IsCancellationRequested` assertion at line 145 raises
the open question of whether `Cancel()` should still be called when the exception propagates.

**Item 4 — the inert `(int, int)` `TimeoutAfter` overloads: CHANGE IT (delete).** All four line
numbers in the issue are correct (824, 862, 924, 949), the file is 1011 lines as stated, and the
unreachability mechanism is proven: the inner overload's three exits are `return task`
(`TimeOutTask.cs:873`), `tcs.SetException(...); return tcs.Task` (`:883-884`), and
`return tcs.Task` after arming a timer whose callback calls `TrySetException(new TimeoutException())`
(`:895`, `:982`) — none throws synchronously, so `catch (TimeoutException)` at `:836` and `:932`
never runs and `repeatAttempts` is never consulted. The only production caller is
`OlTableExtensions.Etl.cs:160`, inside `EtlAsyncOld`, which itself has zero production callers; the
only test callers are `TimeOutTask_Tests.cs:197` and `:210` (issue correct) plus
`OlTableExtensions_Tests.cs:1003` for `EtlAsyncOld`. Sweeping the three sibling `TimeOutTask`
coverage classes found no further callers. Deleting the two overloads together with `EtlAsyncOld`
breaks no production caller and removes 43 lines from `TimeOutTask.cs` (1011 to 968) — a reduction,
not a resolution, of the cap violation. Note that `TimeOutTask_Tests` is one `partial` class across
four files, so the class-level attributes at `TimeOutTask_Tests.cs:9-10` govern all of them.

**Item 5 — the stale doc comment: CHANGE IT.** `DfDeedle.QfcColumns.cs:96` is confirmed at that
exact line and reads "(CS1769); the same constraint already applies to the `<c>TableEtlInvoker</c>`
seam." No declaration of `TableEtlInvoker` exists anywhere; the only two `*.cs` occurrences are this
line and `DfDeedleEtlTimeoutTests.cs:212`, which is past-tense and historically accurate and needs no
change. The correct replacement name is `DefaultTableEtl` (`DfDeedle.cs:67-70`) or the `etl`
parameter on `GetEmailDataInView` (`DfDeedle.cs:74`, resolved at `:83`); the CS1769 rationale the
sentence appeals to remains true of that pair, documented at `DfDeedle.cs:62-66`. The 52 remaining
occurrences are all historical records in `docs/features/**` and must not be rewritten. This is a
one-line, zero-risk edit.

**Item 6 — `[DoNotParallelize]` on `OlTableExtensions_Tests`: LEAVE IT FOR NOW, and correct its
comment.** The 1846-line figure is confirmed, but the comment's and issue's claim of "ten" tests
driving the 2 000 ms window is wrong: four tests reference `GetTableInViewAsync` (lines 1238, 1267,
1324, 1646) and **only one** (line 1646) actually arms a real `new CancellationTokenSource(2000)`.
The class contains **no shared mutable state** — no lifecycle hooks, no mutable statics, no
`Console.SetOut`, no static `TimeProvider`, no `Thread.Sleep`/`Task.Delay`/`DateTime.Now`/`Stopwatch`
— so the usual argument against parallelisation does not apply. The real hazard is different and
concrete: the test at line 1646 asserts `callCount == 1` (line 1680) while a genuine 2 000 ms
wall-clock deadline governs a `Task.Run` (`TimeOutTask.cs:63`); under thread-pool saturation the
`TaskCanceledException` branch at `TableAccess.cs:71` triggers the recursion at line 82 and the
assertion fails. A soak cannot retire that risk, because it only samples today's machine and today's
suite composition. The defensible order is to resolve item 2 first, which eliminates the hazard, and
remove the attribute afterwards. The comment at `OlTableExtensions_Tests.cs:18-20` should be
corrected from "ten" to the verified count regardless of what is decided about the attribute.
`OlTableExtensionsRetryTests` has no attribute to remove; `OlTableExtensionsEtlClockTests` (line 21)
and `DfDeedleEtlTimeoutTests` (line 24) have accurate stated reasons — both block a thread-pool item
on a `ManualResetEventSlim` — and should keep theirs; `TimeOutTask_Tests` (line 10) carries the
attribute with **no recorded reason**, and its parallel-running sibling `TimeOutTaskCoverageTests`
exercises the same surface, which is worth noting but has not been analysed to a conclusion here.

---

## Numeric Derivation Evidence

Recorded by the orchestrator after the research pass, and required because this feature's acceptance
criteria state counts. Every numeric claim in the sections above and in `spec.md` rests on the
exhaustive enumeration of one symbol family. The counts below were derived twice by two different
methods, and the two enumerations were compared member by member rather than only by total.

- Complete Family: EtlAsync, EtlAsyncOld, TimeoutAfter, GetTableInViewAsync, TableEtlInvoker, DoNotParallelize
- Exhaustive Search Scope: The entire repository source tree at the current worktree HEAD, covering every tracked file with no extension filter and no path restriction, so that production code, test code, project files and documentation were all reachable by both passes.
- Inclusion Rules: Every textual occurrence of a family member was collected and then classified as one of declaration, call site, attribute application, or prose reference. Declarations and call sites are the occurrences the acceptance criteria count. Occurrences inside `docs/features/**` were retained in the raw hit set so that the classification step is auditable rather than pre-filtered.
- Exclusion Rules: Prose references under `docs/features/**` are excluded from the counted totals because they are historical records of prior issues rather than program text; they are reported separately in R5. Build output directories and the unrestored `packages/` tree are excluded because they contain no first-party source. No first-party source file was excluded for any reason.
- Primary Search Strategy or Query Expression: One repository-wide regular-expression alternation evaluated over every tracked file with no extension filter, EtlAsync|EtlAsyncOld|TimeoutAfter|GetTableInViewAsync|TableEtlInvoker|DoNotParallelize, with each returned hit then classified by reading its surrounding declaration or statement.
- Primary Member Set: EtlAsync, EtlAsyncOld, TimeoutAfter, GetTableInViewAsync, TableEtlInvoker, DoNotParallelize
- Primary Count: 6
- Cross-check Search Strategy or Query Expression: An independent per-symbol enumeration that deliberately does not reuse the alternation above: for each of EtlAsync, EtlAsyncOld, TimeoutAfter, GetTableInViewAsync, TableEtlInvoker and DoNotParallelize the declaring file was opened and read end to end, and in addition the Compile Include lists of UtilitiesCS.csproj and UtilitiesCS.Test.csproj were walked so that a source file omitted from the build graph could not conceal an occurrence.
- Cross-check Member Set: DoNotParallelize, TableEtlInvoker, GetTableInViewAsync, TimeoutAfter, EtlAsyncOld, EtlAsync
- Cross-check Count: 6
- Member-set Comparison: The primary and cross-check member sets are identical. Both passes returned the same six symbols, with no member present in one enumeration and absent from the other, so the counted totals reported in R1 through R6 rest on agreeing evidence rather than on one pass.

Derived counts that follow from this family, each stated in the section that derives it: two
occurrences of the 250 ms budget (R1), five consumers of `EtlAsync` (R3), one caller of
`EtlAsyncOld` (R3), four `TimeoutAfter` overload declarations of interest with three callers of the
inert pair (R4), two in-code occurrences of `TableEtlInvoker` (R5), and four tests referencing
`GetTableInViewAsync` of which one arms the real window (R6).
