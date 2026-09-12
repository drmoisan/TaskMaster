---
Timestamp: 2026-08-07T20-50
Feature: quickfiler-qfc-home-controller-coverage (epic child F7, issue #433)
Epic: quickfiler-per-file-coverage (issue #136)
Target file: C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-afcf27830d48e5590\QuickFiler\Controllers\QfcHomeController.Metrics.cs
Line count: 234
Worktree: C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-afcf27830d48e5590
Coverage classification authority: docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md (delivered by child F1, wave 0; not present on disk at research time)
Coverage evidence mechanism: F1's per-file line-coverage harness derived from the Cobertura output of scripts/vscode/Invoke-MSTestWithCoverage.ps1
Research method: static per-member cross-reference. No msbuild, no vstest, no coverage run performed.
---

# QfcHomeController.Metrics.cs — Per-File Coverage Research

## 0. Upstream contract consumed

This artifact is written to consume, not to substitute for, child F1 (`quickfiler-coverage-ledger`, wave 0):

- **Classification authority.** Whether `QuickFiler/Controllers/QfcHomeController.Metrics.cs` is `testable` or `ratified-exempt` is decided by the F1 ledger at
  `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md`. This research assumes `testable` and records the supporting evidence for that classification: the file carries **no** `[ExcludeFromCodeCoverage]` attribute on any `QfcHomeController` partial (verified by search across `QuickFiler/Controllers/QfcHomeController*.cs` — zero matches), it is not form-derived, it is not designer-generated, and every Outlook Interop touch in it is already reachable through Moq-able interop interfaces or through an existing/proposed injectable seam. Under the epic's ratified reading of `CLAUDE.md` § UT2 ("without an injectable seam" is a live obligation, not a standing permission), the COM/VSTO exemption does not apply to this file.
- **Measurement authority.** Per-file line coverage for this file is produced by F1's harness and committed as evidence under `<FEATURE>/evidence/qa-gates/`. No substitute harness is proposed here, and no numeric coverage figure is asserted in this artifact — every "covered / not covered" statement below is a static cross-reference claim about which existing test reaches which line, to be confirmed numerically by F1's harness at execution time.

## 1. File purpose and responsibilities

`QfcHomeController.Metrics.cs` is one of three partials of `QuickFiler.Controllers.QfcHomeController` (the others are `QfcHomeController.cs`, 487 lines, and `QfcHomeController.Iteration.cs`, 86 lines). It carries the **session-metrics accumulation and write paths** for the QuickFiler filing session:

1. **Time seam ownership.** The partial declares the type's `TimeProvider` property (line 17), which is the single injectable clock for the whole `QfcHomeController` type. `QfcHomeController.LaunchAsync` assigns it (`QfcHomeController.cs:54`) and `QfcHomeController.Metrics.cs`, `QfcHomeController.cs:77` consume it.
2. **Synchronous metrics write** — `QuickFileMetrics_WRITE(string)` (19-88): builds a CSV data-line prefix from the clock, computes a per-email duration from `_stopWatchMoved`, optionally creates an Outlook "Email Time" calendar appointment, asks the collection controller for diagnostic lines, and writes them to `MyDocuments` through the static `FileIO2.WriteTextFile`.
3. **Asynchronous metrics write** — `WriteMetricsAsync(string)` (90-155): the same shape, but it delegates the appointment creation to `WriteMoveToCalendar` and, instead of writing to disk directly, pushes each diagnostic line into a `BlockingCollection<string>` producer queue.
4. **Calendar side-effect helper** — `WriteMoveToCalendar` (157-188).
5. **Non-blocking producer pair** — `NonBlockingProducer(string[], CancellationToken)` (190-199) and `NonBlockingProducer(string, CancellationToken)` (201-232): bounded-retry enqueue into `_metrics` with a `TimeProvider`-gated 20 ms back-off, plus a consumer-scheduling branch.

### Production call graph (verified)

| Entry point | Reached from | Status |
| --- | --- | --- |
| `WriteMetricsAsync(string)` | `QfcFormController.cs:47` binds `WriteMetrics = parent.WriteMetricsAsync`; invoked at `QfcFormController.EventHandlers.cs:229` inside `BackGroundMoveAsync`, marshalled through `UiThread.Dispatcher.InvokeAsync`, with `_globals.FS.Filenames.EmailSession` as the filename | **Live** |
| `QuickFileMetrics_WRITE(string)` | Declared by `QuickFiler/Interfaces/IFilerHomeController.cs:41`. A repository-wide search for `QuickFileMetrics_WRITE` finds **no production caller** of the `QfcHomeController` implementation (the only non-test callers are `QuickFiler/Legacy/QuickFileController.cs:694` — a legacy file not compiled into the coverage denominator — and `EfcHomeController.ExecuteMoves.cs:141`, which calls the EFC three-argument overload) | **Interface-obligation only; dead in production** |
| `WriteMoveToCalendar` | `WriteMetricsAsync` only (line 136) | Live via (1) |
| `NonBlockingProducer` (both) | `WriteMetricsAsync:154` → array overload → single-line overload | Live via (1) |

### Metrics state-transition context (`SwapStopWatch`)

`SwapStopWatch()` lives in the sibling partial `QfcHomeController.Iteration.cs:79-84`, is exposed on `IQfcHomeController` (`QfcHomeController.cs`-adjacent `Controllers/IQfcHomeController.cs:16`), and performs `_stopWatchMoved = _stopWatch; _stopWatch = new Stopwatch(); _stopWatch.Start();`. It is invoked from `QfcFormController.EventHandlers.cs:142` (`LoadUiFromQueue`), `:191` (`MoveAndIterate` end-of-database path) and `:372`. The metrics accumulators consumed by this file are therefore:

- `_stopWatchMoved` — the stopwatch covering the *filing session that just completed*. Read by `QuickFileMetrics_WRITE` at line 42/44.
- `_stopWatch` — the freshly restarted stopwatch for the *next* session. Read by `WriteMetricsAsync` through the public `StopWatch` property at line 121.
- `_metrics` (`BlockingCollection<string>`, declared `QfcHomeController.cs:353`), `_metricsConsumers` (`:356`), `_fileName` (`:358`).

The `_stopWatch` vs `_stopWatchMoved` asymmetry is a defect, recorded in § 9.

## 2. Dependency and seam inventory

### 2.1 Injectable today

| Seam | Declaration | Kind | Default | Consumed at |
| --- | --- | --- | --- | --- |
| `TimeProvider` | this file, line 17 (`internal TimeProvider TimeProvider { get; set; }`) | property (BCL abstract class) | `TimeProvider.System` | 27, 107, 222; also `QfcHomeController.cs:54,77` |
| `_formController` (`IQfcFormController`) | `QfcHomeController.cs:415` | private field, assigned by loader delegate or by test reflection | production loader at `QfcHomeController.cs:199-229` | 46, 75, 125, 144 |
| `Globals` (`IApplicationGlobals`) | `QfcHomeController.cs:155` | internal property | constructor argument | 33, 60, 84, 114, 169 |
| `Token` (`CancellationToken`) | `QfcHomeController.cs:467` | public property over `_token` | `default` | 154 |

`InternalsVisibleTo("QuickFiler.Test")` is already declared at `QfcHomeController.cs:18`, so `internal` members of this partial are directly reachable from the test project without reflection.

### 2.2 Clock and wall-clock inventory (determinism hazards)

Every time read in the file, with line number:

| Line | Expression | Seamed? | Determinism status |
| --- | --- | --- | --- |
| 27 | `TimeProvider.GetLocalNow().LocalDateTime` | Yes — `TimeProvider` | Deterministic under `FakeTimeProvider`. Pinned by `QuickFileMetrics_WRITE_UsesInjectedClock_ForDataLine`. |
| 42 | `_stopWatchMoved.Elapsed.Seconds` | **No** | `System.Diagnostics.Stopwatch.Elapsed` is a non-virtual property on a non-abstract class; Moq cannot control it. Tests today are deterministic only because they inject a never-started `Stopwatch` (`Elapsed == TimeSpan.Zero`). Any assertion about a *non-zero* duration is unreachable without the § 5 S3 extraction. |
| 44 | `endTime.Subtract(_stopWatchMoved.Elapsed)` | **No** | Same instance, second read. Note it uses the full `Elapsed`, whereas line 42 uses only `.Seconds` — the two derived values are mutually inconsistent (§ 9 D2). |
| 107 | `TimeProvider.GetLocalNow().LocalDateTime` | Yes | Deterministic. Pinned by `WriteMetricsAsync_UsesInjectedClock_ForDateAndTimeStamps`. |
| 121 | `StopWatch.Elapsed.Seconds` (public property → `_stopWatch`) | **No** | Same limitation as line 42. Reads the *wrong* stopwatch (§ 9 D1). |
| 123 | `OlEndTime.Subtract(new TimeSpan(0,0,0,(int)Duration))` | n/a | Derived from line 121; no independent clock read. |
| 222 | `await TimeProvider.Delay(TimeSpan.FromMilliseconds(20))` | Yes — `TimeProvider` (extension `TimeProviderTaskExtensions.Delay`) | Deterministic under `FakeTimeProvider.Advance`. Currently pinned only as an isolated expression, not through the production method (§ 3, row 6). |
| 229 | `new System.Timers.Timer(2000)` | **No** | Wall-clock timer construction. The instance is a local that is never started and never disposed, so it produces no observable behavior (§ 9 D4). No test may rely on it firing. |

There are **no** `DateTime.Now` / `DateTime.UtcNow` / `DateTimeOffset.Now` call sites in this file — issue #222 already migrated sites 4-8 to the `TimeProvider` seam. `BannedSymbols.txt` (per `.claude/rules/csharp.md`) bans `DateTime.Now`, `DateTime.UtcNow`, `Thread.Sleep`, `Task.Delay`; this file violates none of them.

### 2.3 File-write inventory

| Line | Expression | Touches disk? | Seamed? |
| --- | --- | --- | --- |
| 40 | `Path.Combine(folderRoot, filename)` → local `filepath` | No | n/a. `filepath` is **assigned and never used** (§ 9 D6). |
| 86 | `FileIO2.WriteTextFile(filename, strOutput, myDocuments)` | **Yes** — `UtilitiesCS/To Depricate/FileIO2.cs:36-48` opens a `StreamWriter` in append mode, once per element of `strOutput` | **No.** Static method; not mockable. |
| 118 | `Path.Combine(myDocuments, filename)` → local `LOC_TXT_FILE` | No | n/a. Assigned and never used (§ 9 D6). |
| 154 | `await NonBlockingProducer(strOutput, Token)` | No — enqueues into an in-memory `BlockingCollection<string>` | n/a |

The disk-write at line 86 is the single hard constraint on this file's test design. It is currently *survivable* only because every existing test mocks `IQfcCollectionController.GetMoveDiagnostics` to return `Array.Empty<string>()`, and `FileIO2.WriteTextFile` iterates zero elements, so no file handle is opened. **The moment any test returns a non-empty diagnostics array, `QuickFileMetrics_WRITE` writes a real file to the mocked `MyDocuments` path**, violating `.claude/rules/general-unit-test.md` ("Creation and use of temporary files in tests is strictly prohibited"). A writer seam is therefore mandatory before covering line 86 with meaningful output (§ 5 S2).

The asynchronous drain path (`TimedConsumerAsync`, `QfcHomeController.cs:362-386`, which calls `FileIO2.WriteTextFileAsync`) lives in the sibling partial and is **out of this file's scope**, but this file's line 226-231 is the only code that would ever schedule it — and it never does (§ 9 D3).

### 2.4 Hard-coded / non-injectable dependencies

| Line | Dependency | Mockable as-is? |
| --- | --- | --- |
| 33, 84, 114 | `Globals.FS.SpecialFolders` (`ConcurrentDictionary<string,string>`) | Yes — existing tests build a real `ConcurrentDictionary` behind `Mock<IFileSystemFolderPaths>` |
| 35 | `logger.Debug(...)` (static log4net field, `QfcHomeController.cs:24`) | Not mockable; harmless (no assertion needed, no I/O in test config) |
| 46, 75, 125, 144 | `_formController.Groups` → `IQfcCollectionController` (**F11-owned**) | Yes — `Mock<IQfcCollectionController>`; `GetMoveDiagnostics` has a `ref AppointmentItem` parameter requiring `It.Ref<AppointmentItem>.IsAny` matchers |
| 58-61, 167-170 | `UtilitiesCS.Calendar.GetCalendar(string, NameSpace)` — static, but purely a wrapper over `Session.GetDefaultFolder(olFolderCalendar).Folders` enumeration (`UtilitiesCS/OutlookObjects/Calendar/Calendar.cs:8-23`) | **Yes, without a seam.** `NameSpace`, `Folders`, `Folder`, `Items`, `AppointmentItem` are all interop *interfaces* and are already Moq-ed elsewhere in the repo (`UtilitiesCS.Test/OutlookObjects/Calendar/CalendarTests.cs:58-79` for the folder-enumeration graph; `TaskMaster.Test/AppGlobals/AppEventsTests.Helpers.cs:74` and `UtilitiesCS.Test/OutlookObjects/Table/OlToDoTable_Tests.cs:36` for `Mock<Items>`). |
| 65, 177 | `olEmailCalendar.Items.Add()` returning `object`, cast to `AppointmentItem` | Yes via `Mock<Items>` + `Mock<AppointmentItem>`. Implementation note for the planner: `Items.Add` carries an optional `object Type` parameter in the interop, so the Moq setup must match `It.IsAny<object>()`. |
| 86 | `FileIO2.WriteTextFile` (static) | **No** — needs S2 |
| 211 | `_metrics.TryAdd(line, 20, ct)` on a concrete `BlockingCollection<string>` (`TryAdd` is non-virtual) | Partially — the success and cancelled paths are reachable; the uncancelled-OCE retry path is not (§ 4 G8, § 5 S1) |
| 226-231 | `Interlocked.CompareExchange(ref _metricsConsumers, 0, 2)`, `new System.Timers.Timer(2000)`, `timer.Elapsed += TimedConsumerAsync` | Branch is reachable only by pre-setting `_metricsConsumers` (field on `QfcHomeController.cs:356`) via reflection; the timer produces no observable effect |

## 3. Per-member coverage cross-reference

All members declared in the target file. There are **no local functions and no lambdas** in this file; the two `Func<>`-shaped items are property declarations, not lambdas.

Test-file shorthand:
`MT` = `QuickFiler.Test\Controllers\QfcHomeControllerMetricsTests.cs`,
`HT` = `QfcHomeControllerTests.cs`, `IT` = `QfcHomeControllerIterationTests.cs`, `PT` = `QfcHomeControllerPropertyTests.cs`, `RA` = `QfcHomeControllerRunAsyncTests.cs`, `RAHC` = `QfcHomeControllerRunAsyncHighConfidenceTests.cs`, `I218` = `QfcHomeControllerIssue218Tests.cs`, `ZB` = `QfcInitEmailQueueZeroBatchTests.cs`.

Verified negative: `HT`, `PT`, `IT`, `RA`, `RAHC`, `I218` contain **no** invocation of any member of this file (`HT:255-285` is a fully commented-out block). `ZB` targets `QfcDatamodel` exclusively and never constructs a `QfcHomeController`. `MT` is therefore the sole existing suite reaching this file.

| # | Member | Lines | Covered-by (test file + method) | Residual gap (uncovered branches / paths) | Reachable without new seam? |
| --- | --- | --- | --- | --- | --- |
| M1 | `TimeProvider` property (get/set + `= TimeProvider.System` initializer) | 12-17 | Initializer: every `MT` test (construction). Setter: `MT.WriteMetricsAsync_UsesInjectedClock_ForDateAndTimeStamps:334`, `MT.QuickFileMetrics_WRITE_UsesInjectedClock_ForDataLine:369`, `MT.NonBlockingProducer_DelaySeam_HonorsInjectedTwentyMillisecondDelay:405`. Getter: `MT:408` and production lines 27/107/222 | No line-level gap. No test asserts the **production default** is `TimeProvider.System` (the property is the type-wide clock contract; nothing pins it) | Yes |
| M2 | `QuickFileMetrics_WRITE(string)` — prologue + data-line | 19-31 | `MT.QuickFileMetrics_WRITE_WhenGetCalendarReturnsNull_DoesNotThrow`, `MT.GetMoveDiagnostics_NullAppointment_DoesNotThrow`, `MT.QuickFileMetrics_WRITE_UsesInjectedClock_ForDataLine` | none | n/a |
| M2a | └ `MyDocuments` absent guard | 33-39 | `MT.GetMoveDiagnostics_NullAppointment_DoesNotThrow` (its `specialFolders` at :170-173 is **empty**, so the method returns at line 38 — this is incidental coverage; the test's name and doc comment describe behavior it never reaches, see § 9 D12) | Branch is executed but **unasserted**. No test proves `GetMoveDiagnostics` is *not* called on this path | Yes |
| M2b | └ `Path.Combine` → unused `filepath` | 40 | The two tests that seed `MyDocuments` | none (dead local) | n/a |
| M2c | └ duration from `_stopWatchMoved` | 42-44 | `MT.QuickFileMetrics_WRITE_WhenGetCalendarReturnsNull_DoesNotThrow` (injects `new Stopwatch()`), `MT.QuickFileMetrics_WRITE_UsesInjectedClock_ForDataLine` (injects `new Stopwatch()`) | Only the `Elapsed == 0` case. Non-zero elapsed, the `.Seconds`-vs-`.TotalSeconds` truncation, and the line-42/line-44 inconsistency are all unexercised | **Needs seam** — `Stopwatch.Elapsed` is not mockable. Minimal answer is **not** a seam but a pure-function extraction (S3) |
| M2d | └ `emailsLoaded > 0` division | 46-51 | TRUE branch only (all `MT` tests set `EmailsToMove == 1`) | FALSE branch (`EmailsToMove == 0`, and the negative case) never executed | Branch is reachable (mock returns 0), but the *assertion* is only meaningful once elapsed is controllable → S3 |
| M2e | └ numeric formatting | 53-56 | Executed at `duration == 0` only | Rounding of `"##0"`, two-decimal `"##0.00"`, and the culture sensitivity of both are unexercised | Needs S3 for meaningful input |
| M2f | └ calendar lookup, `is not null` **false** branch | 58-63 | All three `MT` `QuickFileMetrics_WRITE` tests (mocked `Folders` yields an empty `ArrayList`) | none | n/a |
| M2g | └ calendar-found appointment creation | 65-73 | **Nothing** | Entire 9-line block: `Items.Add()`, `Subject`, `Start`, `End`, `Categories`, `ReminderSet`, `Sensitivity`, `Save()` | **Yes** — Moq graph only (`Mock<Folder>` named `"Email Time"` in the `Folders` enumerator, `Mock<Items>`, `Mock<AppointmentItem>`), per the ratified `CalendarTests.cs:58-79` pattern |
| M2h | └ `GetMoveDiagnostics` call | 75-82 | `MT.QuickFileMetrics_WRITE_WhenGetCalendarReturnsNull_DoesNotThrow`, `MT.QuickFileMetrics_WRITE_UsesInjectedClock_ForDataLine` | none | n/a |
| M2i | └ second `MyDocuments` lookup + disk write | 84-87 | Executed with `strOutput.Length == 0`, so `FileIO2.WriteTextFile` opens no handle | The write is never exercised with real content; the `false` branch of line 84 is **unreachable** (line 33 already returned on that condition — a permanently-partial branch) | **Needs seam S2** — covering line 86 with content would create a real file |
| M3 | `WriteMetricsAsync(string)` — prologue + data-line | 90-112 | `MT.WriteMetricsAsync_UsesInjectedClock_ForDateAndTimeStamps` | none | n/a |
| M3a | └ `MyDocuments` absent early return | 114-117 | **Nothing** (the single `WriteMetricsAsync` test seeds `MyDocuments`) | Whole guard | Yes |
| M3b | └ `Path.Combine` → unused `LOC_TXT_FILE` | 118 | `MT.WriteMetricsAsync_UsesInjectedClock_ForDateAndTimeStamps` | none (dead local) | n/a |
| M3c | └ duration from `StopWatch` | 121-123 | `MT.WriteMetricsAsync_UsesInjectedClock_ForDateAndTimeStamps` (injects `new Stopwatch()` at :332) | `Elapsed == 0` only; same limitation as M2c | Needs S3 |
| M3d | └ `emailsLoaded > 0` division | 125-130 | TRUE branch only | FALSE branch | Needs S3 for a meaningful assertion |
| M3e | └ formatting + `WriteMoveToCalendar` call + `GetMoveDiagnostics` | 132-151 | `MT.WriteMetricsAsync_UsesInjectedClock_ForDateAndTimeStamps` | Formatting only at 0 | Needs S3 for values |
| M3f | └ `_fileName = filename` + producer hand-off | 153-154 | `MT.WriteMetricsAsync_UsesInjectedClock_ForDateAndTimeStamps` — but `strOutput` is empty, so the array overload iterates zero times | The producer is never driven with real content from this call site | Yes |
| M4 | `WriteMoveToCalendar(...)` — lookup + `null` branch | 157-174 | `MT.WriteMetricsAsync_UsesInjectedClock_ForDateAndTimeStamps` | none | n/a |
| M4a | └ `else` appointment-creation branch | 175-187 | **Nothing** | Entire 13-line block, including the `out AppointmentItem` handed to `GetMoveDiagnostics` as a non-null `ref` | **Yes** — same Moq graph as M2g |
| M5 | `NonBlockingProducer(string[], CancellationToken)` | 190-199 | `MT.WriteMetricsAsync_UsesInjectedClock_ForDateAndTimeStamps` reaches the method with a **zero-length** array → only the method entry and the loop-exit are executed | Loop body: `ct.ThrowIfCancellationRequested()` (196) and the recursive `await` (197) | **Yes** (see S5 — prefer widening visibility to `internal` over reflection-invoke) |
| M6 | `NonBlockingProducer(string, CancellationToken)` | 201-232 | **Nothing invokes this method.** `MT.NonBlockingProducer_DelaySeam_HonorsInjectedTwentyMillisecondDelay` evaluates the *expression* `_controller.TimeProvider.Delay(TimeSpan.FromMilliseconds(20))` in the test body; it never enters the production method (the test's own doc comment at :397-398 concedes the catch branch is "not deterministically reachable") | Every line 203-231: the `TryAdd` success path, the `do/while` retry on `false`, the `catch (OperationCanceledException)` cancelled `break`, the uncancelled `else` + 20 ms `TimeProvider.Delay`, and the `_metricsConsumers == 2` consumer-scheduling branch | Mixed: success / cancelled-break / consumer-branch are **yes**; the uncancelled-OCE retry is **needs seam S1** |

### Duplication guard

The following assertions already exist and **must not be re-created**:
`QuickFileMetrics_WRITE` null-calendar no-throw (`MT:76`), `WriteMetricsAsync` clock-sourced `dataLineBeg`/`endTime` (`MT:328`), `QuickFileMetrics_WRITE` clock-sourced `dataLineBeg`/`endTime` (`MT:363`), the isolated 20 ms `TimeProvider.Delay` gating (`MT:401`), and `SwapStopWatch` field-swap (`IT.SwapStopWatch_ExecutesCorrectly:435`).

## 4. Residual gap list and proposed individual test cases

Each row is one atomic plan task. All tests: MSTest `[TestClass]`/`[TestMethod]`, Moq, FluentAssertions, Arrange–Act–Assert, `FakeTimeProvider` for all time, no temp files, no live COM, no `Thread.Sleep`/`Task.Delay`/wall-clock waits, no live WinForms form, no popup, no UI-thread dependency. Proposed home file: a **new** `QuickFiler.Test\Controllers\QfcHomeControllerMetricsCoverageTests.cs` (keeps `QfcHomeControllerMetricsTests.cs` — 421 lines — under the 500-line ceiling; that file has only ~79 lines of headroom).

### Group A — reachable with no new seam

| # | Gap (lines) | Proposed `[TestMethod]` | Arrange / Act / Assert sketch |
| --- | --- | --- | --- |
| A1 | M3a (114-117) | `WriteMetricsAsync_WhenMyDocumentsIsAbsent_ReturnsWithoutBuildingDiagnostics` | **A:** loose-mock controller with an **empty** `SpecialFolders`; `Mock<IQfcCollectionController>` groups. **Act:** `await controller.WriteMetricsAsync("metrics.csv")`. **Assert:** `groups.Verify(GetMoveDiagnostics(...), Times.Never)` and `_metrics` (reflection) is empty. |
| A2 | M2a (33-39) assertion gap | `QuickFileMetrics_WRITE_WhenMyDocumentsIsAbsent_AbortsBeforeDiagnostics` | **A:** loose-mock controller with an empty `SpecialFolders`; `_stopWatchMoved = new Stopwatch()`. **Act:** `controller.QuickFileMetrics_WRITE("metrics.csv")`. **Assert:** `groups.Verify(GetMoveDiagnostics(...), Times.Never)`. *(Not a duplicate of `MT.GetMoveDiagnostics_NullAppointment_DoesNotThrow`, which asserts only `NotThrow`; see § 9 D12 for the recommended disposition of that test.)* |
| A3 | M2g (63-73) | `QuickFileMetrics_WRITE_WhenEmailTimeCalendarExists_CreatesPrivateAppointmentForFiledEmails` | **A:** `Folders` enumerator yields a `Mock<Folder>` whose `Name == "Email Time"`; that folder's `Items.Add(It.IsAny<object>())` returns a `Mock<AppointmentItem>`; `EmailsToMove = 3`; `_stopWatchMoved = new Stopwatch()`; `FakeTimeProvider` fixed. **Act:** `QuickFileMetrics_WRITE("metrics.csv")`. **Assert:** appointment `VerifySet` `Subject == "Quick Filed 3 emails"`, `End == fake local now`, `Categories == "@ Email"`, `ReminderSet == false`, `Sensitivity == OlSensitivity.olPrivate`, and `Verify(x => x.Save(), Times.Once)`. |
| A4 | M4a (175-187) | `WriteMetricsAsync_WhenEmailTimeCalendarExists_PassesCreatedAppointmentToDiagnostics` | **A:** same calendar graph; `_stopWatch = new Stopwatch()`; `GetMoveDiagnostics` returns `Array.Empty<string>()`. **Act:** `await WriteMetricsAsync("metrics.csv")`. **Assert:** appointment `Save()` once **and** the captured `ref AppointmentItem` handed to `GetMoveDiagnostics` is the mocked appointment (capture via a `Callback` on the `ref`-matcher setup). Distinct member and distinct assertion from A3. |
| A5 | M5 (194-197) + M6 happy path (203-212, 225-226 false) | `WriteMetricsAsync_WithDiagnosticLines_EnqueuesEveryLineInOrder` | **A:** loose controller with `MyDocuments` seeded; `GetMoveDiagnostics` returns `["l1","l2","l3"]`; `_stopWatch = new Stopwatch()`. **Act:** `await WriteMetricsAsync("metrics.csv")`. **Assert:** `_metrics` (read by reflection) `.Should().Equal("l1","l2","l3")`. Disk-safe: `WriteMetricsAsync` never calls `FileIO2`. |
| A6 | M6 cancelled-break (213-218) | `NonBlockingProducer_WhenTokenAlreadyCancelled_BreaksWithoutEnqueuingLine` | **A:** controller; pre-cancelled `CancellationTokenSource`. **Act:** `await controller.NonBlockingProducer("line", cts.Token)` (requires S5). **Assert:** `_metrics` empty and no exception escapes. *Implementation note:* .NET Framework `BlockingCollection<T>.TryAdd(T,int,CancellationToken)` throws `OperationCanceledException` for an already-cancelled token before any wait; **confirm this at plan time** — if it does not hold, route this case through the S1 adder seam instead. |
| A7 | M5 cancellation (196) | `NonBlockingProducerBatch_WhenTokenCancelled_ThrowsBeforeEnqueuingAnyLine` | **A:** controller; pre-cancelled token; array `["a","b"]`. **Act/Assert:** `await act.Should().ThrowAsync<OperationCanceledException>()` and `_metrics` empty. |
| A8 | M6 consumer branch (226-231) | `NonBlockingProducer_WhenTwoConsumersPending_ResetsCounterAndDoesNotThrow` | **A:** set `_metricsConsumers = 2` by reflection. **Act:** `await NonBlockingProducer("line", CancellationToken.None)`. **Assert:** `_metricsConsumers` (reflection) `== -1`, documenting the current 2 → 0 → −1 transition, and the line is present in `_metrics`. Characterization only — see § 9 D3/D4; do **not** assert the timer fires. |
| A9 | M1 default | `TimeProvider_WhenNotInjected_DefaultsToSystemProvider` | **A/Act:** construct `QfcHomeController` with loose-mocked globals. **Assert:** `controller.TimeProvider.Should().BeSameAs(TimeProvider.System)`. Pins the production-parity contract of the type-wide clock seam. |

### Group B — requires S1 (`MetricsAdder` delegate)

| # | Gap (lines) | Proposed `[TestMethod]` | Sketch |
| --- | --- | --- | --- |
| B1 | M6 retry-on-`false` (205-212, 225 loop-back) | `NonBlockingProducer_WhenAddReportsTimeout_RetriesUntilItSucceeds` | **A:** `controller.MetricsAdder` returns `false` on call 1 and `true` on call 2, recording each invocation. **Act:** `await NonBlockingProducer("line", CancellationToken.None)`. **Assert:** exactly 2 invocations, each with `timeoutMs == 20`, and the task completes without awaiting any delay. |
| B2 | M6 uncancelled-OCE back-off (219-223) | `NonBlockingProducer_WhenAddThrowsWithoutCancellation_AwaitsInjectedTwentyMillisecondDelay` | **A:** `FakeTimeProvider`; `MetricsAdder` throws `OperationCanceledException` on call 1 (token **not** cancelled), returns `true` on call 2. **Act:** start the task without awaiting. **Assert:** `task.IsCompleted == false` before `fake.Advance(20 ms)` (the `TimeProvider.Delay` timer registers synchronously before the method yields), then `await task` completes and the adder was invoked twice. No wall-clock wait. |

### Group C — requires S2 (`MetricsLineWriter` delegate)

| # | Gap (lines) | Proposed `[TestMethod]` | Sketch |
| --- | --- | --- | --- |
| C1 | M2i (84-87) | `QuickFileMetrics_WRITE_WithDiagnosticLines_InvokesInjectedWriterWithMyDocumentsRoot` | **A:** recording `MetricsLineWriter`; `SpecialFolders["MyDocuments"] = @"C:\FakeDocs"`; `GetMoveDiagnostics` returns `["l1","l2"]`. **Act:** `QuickFileMetrics_WRITE("metrics.csv")`. **Assert:** exactly one write with `filename == "metrics.csv"`, `lines == ["l1","l2"]`, `folderRoot == @"C:\FakeDocs"`. **No file is created** — this is the whole reason S2 exists. |

### Group D — requires S3 (pure `BuildDurationTexts` extraction)

| # | Gap | Proposed `[TestMethod]` | Sketch |
| --- | --- | --- | --- |
| D1 | M2d/M3d FALSE branch | `BuildDurationTexts_WithZeroEmailsLoaded_DoesNotDivideElapsedSeconds` | `BuildDurationTexts(TimeSpan.FromSeconds(30), 0, CultureInfo.InvariantCulture)` → `duration == 30`, `durationText == "30"`. |
| D2 | M2d/M3d TRUE branch with real values | `BuildDurationTexts_WithMultipleEmailsLoaded_DividesElapsedSecondsPerEmail` | `(TimeSpan.FromSeconds(30), 4)` → `duration == 7.5`, `durationText == "8"` (pins the `"##0"` rounding). |
| D3 | § 9 D2 truncation | `BuildDurationTexts_WithElapsedOverOneMinute_UsesSecondsComponentNotTotalSeconds` | `(TimeSpan.FromSeconds(90), 1)` → `duration == 30`. **Characterization test**: documents current behavior; must not be "fixed" inside a coverage child. |
| D4 | M2e minutes formatting | `BuildDurationTexts_FormatsMinutesWithTwoDecimalPlaces` | `(TimeSpan.FromSeconds(45), 1)` → `durationMinutesText == "0.75"`. |
| D5 | negative-count boundary | `BuildDurationTexts_WithNegativeEmailsLoaded_DoesNotDivide` | `(TimeSpan.FromSeconds(30), -2)` → `duration == 30` (guard is `> 0`). |

**Total proposed: 17 test cases** (A ×9, B ×2, C ×1, D ×5).

## 5. Required seams (minimum set, ranked)

Ranked per `.claude/rules/csharp.md` § DI Seams (interface > injectable delegate > adapter), with the epic's addition that a **pure-function extraction** that removes the need for any seam outranks all three.

### S3 — Pure-function extraction (rank 0: no seam at all). **Required.**

Extract the duration arithmetic and formatting shared by lines 42/48-56 and 121/127-135 into a pure static:

```
internal static (double duration, string durationText, string durationMinutesText)
    BuildDurationTexts(TimeSpan elapsed, int emailsLoaded, IFormatProvider formatProvider = null)
```

The body must reproduce today's semantics **exactly**, including `elapsed.Seconds` (not `TotalSeconds`), the `emailsLoaded > 0` guard, `"##0"` and `"##0.00"`, and a default `formatProvider` of `CultureInfo.CurrentCulture` so no behavior changes.

- **Why a lower-cost option does not suffice.** There is no lower-cost option: `System.Diagnostics.Stopwatch` is a concrete class whose `Elapsed` property is non-virtual, so neither an interface seam, a delegate seam, nor an adapter can control it without *replacing* the stopwatch field. Replacing `_stopWatch` is prohibited by a live pin: `QfcHomeControllerRunAsyncTests.RunAsync_ExecutesCorrectly:303` asserts `_controller.StopWatch.IsRunning`, and `QfcHomeControllerPropertyTests.StopWatch_PropertyWorksCorrectly:232` asserts the property returns the assigned `_stopWatch` instance. Extraction leaves the stopwatch read as a single unavoidable wiring line and moves 100% of the decision logic into a testable pure function — exactly the `.claude/rules/general-unit-test.md` "refactor first, exempt only the irreducible remainder" posture the epic ratified, and exactly the shape already ratified for the sibling controller (`EfcHomeController.BuildQuickFileMetricLines`, `EfcHomeController.Metrics.cs:55-85`, covered by `EfcHomeControllerMetricsTests.cs:20-61`).

### S2 — Injectable delegate `MetricsLineWriter` (rank 2). **Required.**

```
internal Action<string, string[], string> MetricsLineWriter { get; set; } = FileIO2.WriteTextFile;
```
Consumed at line 86 in place of the direct static call.

- **Why not an interface seam (rank 1).** An `IMetricsFileWriter` would exist to carry a single three-argument method with one production implementation and no expected second implementation, contradicting `.claude/rules/general-code-change.md` ("use interfaces when multiple implementations are likely") and `.claude/rules/csharp.md` ("keep interfaces minimal"). The delegate shape `Action<string,string[],string>` is **already ratified in this exact assembly** as `EfcHomeControllerDependencies.MetricsLineWriter` (`EfcHomeController.Metrics.cs:51`), so the delegate is also the consistency-preserving choice.
- **Why the seam is unavoidable.** `FileIO2.WriteTextFile` is a `static` method that opens a `StreamWriter` per output line (`FileIO2.cs:36-48`). Statics are not mockable, and `.claude/rules/general-unit-test.md` prohibits temporary files outright. Without S2, gap C1 cannot be covered at all — the alternative would be to leave line 86 permanently uncovered or to write real files, both of which are Blocking under the epic's policy reconciliation.
- **Scope discipline.** Declare the property on **this file** (`QfcHomeController.Metrics.cs`). Do **not** reuse or extend `EfcHomeControllerDependencies` — that type belongs to sibling child F8.

### S1 — Injectable delegate `MetricsAdder` (rank 2). **Required.**

```
internal Func<string, int, CancellationToken, bool> MetricsAdder { get; set; }
```
Defaulted in the field initializer / constructor to `(line, timeoutMs, ct) => _metrics.TryAdd(line, timeoutMs, ct)` and consumed at line 211.

- **Why the seam is unavoidable.** The `else` branch at 219-223 is **unreachable through the real `BlockingCollection<T>`**. `BlockingCollection<T>.TryAdd(T,int,CancellationToken)` only lets an `OperationCanceledException` escape when the caller's own token is cancelled (an internal cancellation caused by `CompleteAdding()` is converted to `InvalidOperationException` before it leaves the method). Therefore `catch (OperationCanceledException)` with `ct.IsCancellationRequested == false` cannot be produced by any arrangement of the concrete type — the existing `MT` test at lines 392-399 documents exactly this and settles for testing the delay expression in isolation. `TryAdd` is also non-virtual, so subclassing cannot intercept it.
- **Why not an interface seam.** Same reasoning as S2: one call site, one three-argument operation, no expected second implementation. The delegate also matches the file's own established convention — `QfcHomeController.cs:159-244` already exposes six `Func<>`-typed `internal` loader seams (`QfcDataModelLoader`, `QfcQueueLoader`, `HighConfidencePreFilterLoader`, …).
- **Why not an adapter (rank 3).** An adapter class around `BlockingCollection<string>` would add a type and a file for one method and would not be cheaper than the delegate.

### S5 — Visibility widening on `NonBlockingProducer` (not a seam; lowest cost). **Recommended.**

Change both `NonBlockingProducer` overloads (190, 201) from `private` to `internal`. `InternalsVisibleTo("QuickFiler.Test")` is already declared (`QfcHomeController.cs:18`).

- **Why.** Gaps A6, A7, A8, B1 and B2 all target these two methods. Without S5 they must be invoked by `MethodInfo.Invoke`, which for `async Task` methods requires unwrapping the returned `Task` from `object` and converts every argument-shape mistake into a run-time `TargetInvocationException` instead of a compile error. The repository does use reflection-invoke where unavoidable (`QfcHomeControllerRunAsyncTests.cs:346-353` for `Worker_RunWorkerCompleted`), but where an `internal` widening is available it is strictly lower-cost and keeps the public surface unchanged (`.claude/rules/csharp.md`: "Prefer `internal` for non-public APIs").

### Rejected seams (recorded, not planned)

- **Timer-factory seam for lines 229-230.** Rejected: the branch is dead in production (§ 9 D3) and the timer it builds has no observable effect (§ 9 D4). Adding a seam to make dead code observable is gold-plating; characterization test A8 plus a promoted defect issue is the proportionate response.
- **Replacing `Stopwatch` with `TimeProvider.GetTimestamp()`/`GetElapsedTime`.** Rejected: it is a behavior change to a live path and it breaks two existing pins (`RunAsync_ExecutesCorrectly:303`, `StopWatch_PropertyWorksCorrectly:232`). S3 achieves the coverage objective with zero behavior change.
- **`ICalendarService` interface over `UtilitiesCS.Calendar.GetCalendar`.** Rejected: unnecessary. The full calendar graph is already Moq-able through interop interfaces (§ 2.4), proven by `UtilitiesCS.Test/OutlookObjects/Calendar/CalendarTests.cs`. Introducing a seam where none is needed would also touch `UtilitiesCS`, outside F7's file set.

## 6. Cross-child contract notes

**No cross-child contract addition is required by this file.** Verified against each sibling-owned surface this file touches:

| Sibling-owned type | Owning child | Use in this file | Contract change needed? |
| --- | --- | --- | --- |
| `IQfcCollectionController.GetMoveDiagnostics(string,string,double,string,DateTime,ref AppointmentItem)` (`QuickFiler/Interfaces/IQfcCollectionController.cs:109-116`) | **F11** | Read-only call at 75-82 and 144-151 | **No.** The member already exists on the interface and is already Moq-ed by `MT`. Constraint to record for the planner, not a request: because of the `ref AppointmentItem` parameter, Moq requires `ref It.Ref<AppointmentItem>.IsAny` in both `Setup` and `Verify`, and a specific-instance `ref` argument cannot be matched — assert the passed appointment by capturing it in a `Callback` (test A4) rather than by argument matcher. |
| `IQfcFormController.Groups` (F6's `Interfaces/IQfcFormController.cs`) | **F6** | Read-only property access at 46, 75, 125, 144 | **No.** |
| `IQfcDatamodel` / `QfcDatamodel*` | F5 | Not referenced by this file | No |
| `IQfcQueue` / `QfcQueue*` / `FilerQueue` | F2 | Not referenced by this file | No |
| `KeyboardHandler` / `Kbd*` / `Ka*` | F3 | Not referenced by this file | No |
| `QfcFormController*` / `QfcExplorerController` | F6 | Only as the *caller* of `WriteMetricsAsync` (`QfcFormController.cs:47`, `EventHandlers.cs:229`). This file's public signature is unchanged by every recommendation above, so F6 needs no edit. | No |
| `coverage.config` / shared build property files | F1 | Not touched | No |
| `EfcHomeControllerDependencies.MetricsLineWriter` | F8 | Cited as **precedent only**; S2 declares a QfcHomeController-local property and does not reference the EFC type | No |

Files edited by the recommendations above are all inside F7's own set: `QuickFiler/Controllers/QfcHomeController.Metrics.cs` (S1, S2, S3, S5), and optionally `QuickFiler/Controllers/QfcHomeController.cs` (§ 8 cohesion move). `QuickFiler/Interfaces/IFilerHomeController.cs` is also F7-owned but needs **no** change.

## 7. Issue #424 interaction findings

Sources read: `docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/issue.md`, `spec.md` (v1.2), and the folder's evidence set.

**Merge state.** #424 has already landed in this checkout. `QfcHomeController.cs:294-305` contains the `QfcScanProgressBandMapper` wiring, the 200 ms poll (O1), and the `QfcStreamingDequeueConfidenceGate.DefaultFirstBatchDeadline` argument; `QfcHomeControllerRunAsyncHighConfidenceTests.cs:137-146` pins that exact four-argument call. Planning should assume the post-#424 shape, not the pre-#424 shape.

**Members of the target file touched by #424: none.**
`QfcHomeController.Metrics.cs` does not appear in the #424 spec's files-to-change table (`spec.md` lines 106-113: `QfcStreamingDequeueConfidenceGate.cs`, `QfcDatamodel.QueueProcessing.cs`, `QfcDatamodel.cs`, `IQfcDatamodel.cs`, `QfcHomeController.cs`). The file contains no #424 comment markers, and none of `TimeProvider`, `QuickFileMetrics_WRITE`, `WriteMetricsAsync`, `WriteMoveToCalendar`, or either `NonBlockingProducer` overload is named anywhere in the #424 spec or issue. The overlap the epic flagged under "Known Conflict Risks" is real for the **type** (`QfcHomeController`) but lands entirely in the sibling partial `QfcHomeController.cs` (`RunAsync`).

**Tests that must not be written or altered because they would contradict #424's regression suite:**

1. Do **not** add or modify any test asserting the arguments of `IQfcDatamodel.DequeueNextItemGroupAsync` from `RunAsync`. #424 AC 1/AC 6 are pinned by `RunAsync_HighConfidenceEnabled_LoadsFirstPageFromStreamingDequeue` (`RAHC:137-146,180-191`) and `RunAsync_HighConfidenceScanProgress_MapsReportsIntoTheZeroToThirtyBand` (`RAHC:289-388`). None of the § 4 tests goes near this path.
2. Do **not** change `QfcHomeControllerIterationTests.cs:268` (`DequeueNextItemGroupAsync(8, 2000)`) — #424 AC 12 requires this file byte-unmodified. F7 owns the file, which makes accidental edits possible; treat it as frozen.
3. Do **not** re-shape the `Setup`/`Verify` matchers in `QfcHomeControllerIssue218Tests.cs` — #424 AC 12 constrains that file's diff to the four already-applied overload-shape hunks.
4. Do **not** introduce any seam that replaces or wraps `_stopWatch`. `QfcHomeControllerRunAsyncTests.RunAsync_ExecutesCorrectly:303` asserts `_controller.StopWatch.IsRunning` immediately after `RunAsync`, which is post-#424 behavior on the live startup path. This is the direct reason S3 is a pure extraction rather than a clock seam.
5. Do **not** re-test the `TimeProvider` delay seam in isolation. `MT.NonBlockingProducer_DelaySeam_HonorsInjectedTwentyMillisecondDelay:401` already pins it; tests B1/B2 drive the *production method* instead, which is additive rather than duplicative.

**Non-blocking observation.** #424's coverage AC (spec AC 13) recorded a merge-base repository line rate of 70.19% and re-scoped the repo-wide 80% floor to the *testable denominator*. F7 must not treat that re-scoping as authority over its own per-file target: issue #136 measures per-file coverage, and the authority for this file's classification is F1's ledger.

## 8. File-size analysis

| File | Current | Ceiling | Headroom |
| --- | --- | --- | --- |
| `QuickFiler/Controllers/QfcHomeController.Metrics.cs` (target) | **234** | 500 | **266** |
| `QuickFiler/Controllers/QfcHomeController.cs` | 487 | 500 | **13** |
| `QuickFiler/Controllers/QfcHomeController.Iteration.cs` | 86 | 500 | 414 |
| `QuickFiler/Controllers/IQfcHomeController.cs` | 20 | 500 | 480 |
| `QuickFiler/Interfaces/IFilerHomeController.cs` | 45 | 500 | 455 |
| `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs` | 421 | 500 | 79 |

**Projected growth of the target file from § 5.** S1 property with XML doc ≈ +8; S2 property with XML doc ≈ +8; S3 pure method with XML doc ≈ +22, less ≈ −8 removed from the two call sites; S5 ≈ ±0. Net ≈ **+30 → ≈ 264 lines**, leaving ≈ 236 lines of headroom. No file-size risk.

**Capacity implication for `QfcHomeController.cs` (487 / 500, 13 lines of headroom).** F7 owns both files, and `QfcHomeController.cs` is the constrained one. `QfcHomeController.cs:353-386` (34 lines) is a block of **metrics-only** members mis-located in the main partial: `_metrics`, `_metricsConsumers`, `_lockObject`, `_fileName`, and `TimedConsumerAsync`. They are consumed exclusively by this file (lines 211, 226, 228, 153, 230) plus their own bodies. Moving that block into `QfcHomeController.Metrics.cs` would put `QfcHomeController.cs` at ≈ **453** (47 lines of headroom) and this file at ≈ **298** (202 of headroom), with no behavior change and no signature change.

Recommendation: treat the move as **conditional**. Execute it only if F7's own work needs to add lines to `QfcHomeController.cs`; otherwise defer it, because it is a cross-partial code move on a file #424 recently edited, and an unnecessary move raises merge cost against the epic integration branch for no coverage benefit. If executed, it does not conflict textually with #424's `RunAsync` hunks (lines 274-324).

**Test-file sizing.** Adding 17 test cases to the 421-line `QfcHomeControllerMetricsTests.cs` would breach 500. Put them in a new `QuickFiler.Test\Controllers\QfcHomeControllerMetricsCoverageTests.cs`. If that file also approaches the ceiling, split along the § 4 groups (`...MetricsCalendarTests.cs` for A3/A4, `...MetricsProducerTests.cs` for A5-A8/B1/B2, `...MetricsDurationTests.cs` for D1-D5).

## 9. Risks, latent defects, and open questions

### Latent production defects found during cross-reference (report only; do **not** fix inside a coverage child)

Per the repository's standing practice, each of these should be promoted through the issue lifecycle rather than left as prose in a feature folder.

- **D1 — `WriteMetricsAsync` reads the wrong stopwatch.** Line 121 reads `StopWatch.Elapsed` (`_stopWatch`) while the commented-out line 120 shows it previously read `_stopWatchMoved`. Production calls `SwapStopWatch()` *before* the metrics write on the end-of-database path (`QfcFormController.EventHandlers.cs:191-192` → `BackGroundMoveAsync` → `WriteMetrics`), so `_stopWatch` at that moment is the **freshly restarted** stopwatch and the recorded duration is ≈ 0 s; the true session duration sits unread in `_stopWatchMoved`. `QuickFileMetrics_WRITE` (line 42) reads `_stopWatchMoved` — the two sibling methods disagree. On the `MoveAndIterate` first path (`EventHandlers.cs:157-161`), the swap in `LoadUiFromQueue` races `BackGroundMoveAsync`, so the recorded value is also non-deterministic. **Severity: metrics data correctness.**
- **D2 — `.Elapsed.Seconds` truncation.** Lines 42 and 121 use `TimeSpan.Seconds` (the 0-59 seconds *component*) rather than `TotalSeconds`. A 90-second session records 30. Line 44 compounds it by computing `startTime` from the *full* `Elapsed` while `duration` uses the truncated value, so the appointment span and the CSV duration disagree.
- **D3 — the metrics consumer is never scheduled.** `_metricsConsumers` (`QfcHomeController.cs:356`) is initialized to 0 and is only ever **decremented** (`Metrics.cs:228`, `QfcHomeController.cs:366`) — there is no increment anywhere in the repository. `Interlocked.CompareExchange(ref _metricsConsumers, 0, 2) == 2` at line 226 can therefore never be true in production, so `TimedConsumerAsync` is never subscribed and the `_metrics` `BlockingCollection` accumulates lines that are **never written to disk**. The entire `WriteMetricsAsync` output path is effectively a no-op after the enqueue.
- **D4 — the consumer timer is inert even if D3 were fixed.** Lines 229-230 construct `new System.Timers.Timer(2000)` into a local, subscribe `TimedConsumerAsync` to `Elapsed`, and never call `Start()`/set `Enabled`. The local is immediately eligible for collection and is never disposed.
- **D5 — `_fileName` is write-only.** Assigned at line 153; `TimedConsumerAsync` (`QfcHomeController.cs:372-377`) uses `Globals.FS.Filenames.EmailSession` instead. It is also `static` on an instance-scoped concern.
- **D6 — dead locals and dead `out` parameter.** `filepath` (line 40) and `LOC_TXT_FILE` (line 118) are assigned and never read; `WriteMoveToCalendar`'s `out Folder OlEmailCalendar` (line 162) is received at line 141 and never used.
- **D7 — unreachable branch from a duplicated lookup.** `Globals.FS.SpecialFolders.TryGetValue("MyDocuments", ...)` is evaluated twice in `QuickFileMetrics_WRITE` (lines 33 and 84). The `false` branch of line 84 cannot be taken because line 33 already returned on that condition, so a coverage report will show a permanently-partial branch. Behavior-preserving simplification: reuse `folderRoot` from line 33. (Worth doing inside F7 since it removes an uncoverable branch from F7's own file — flag it to the planner as a judgment call.)
- **D8 — culture-sensitive CSV output.** `now.ToString("MM/dd/yyyy")`, `"hh:mm"`, `"##0"` and `"##0.00"` (lines 31, 53, 56, 108, 110, 132, 135) all use `CultureInfo.CurrentCulture`. On a non-invariant culture the CSV gains a comma decimal separator (`"2,00"`), corrupting the comma-delimited file. Existing tests mask this by deriving expectations from the same expression.
- **D9 — 12-hour timestamp without a designator.** `"hh:mm"` renders 14:30 as `02:30`, making the recorded time ambiguous.
- **D10 — `QuickFileMetrics_WRITE(string)` has no production caller.** It exists solely to satisfy `IFilerHomeController.QuickFileMetrics_WRITE` (`IFilerHomeController.cs:41`), whose EFC implementation *throws* `NotImplementedException` (`EfcHomeController.Metrics.cs:26-29`). This is an interface-segregation smell on an F7-owned interface. F7 must still cover the method (it is compiled production code in the denominator), but the finding is worth recording.
- **D11 — near-duplicate logic.** `QuickFileMetrics_WRITE` (58-73) inlines the appointment creation that `WriteMoveToCalendar` (157-188) already encapsulates, and both methods independently build `dataLineBeg`, the duration texts, and the diagnostics call. S3 removes part of this duplication; the appointment duplication could be removed by having `QuickFileMetrics_WRITE` call `WriteMoveToCalendar`, but that is a behavior-adjacent refactor (the two blocks are not byte-identical: line 67 passes `startTime` derived from the full `Elapsed`, line 180 passes `OlStartTime` derived from the truncated seconds) — **do not merge them inside a coverage child**.
- **D12 — an existing test is vacuous.** `MT.GetMoveDiagnostics_NullAppointment_DoesNotThrow` (`MT:161-241`) arranges an **empty** `SpecialFolders` (`:170-173`) and then calls `QuickFileMetrics_WRITE` (`:239`), so execution returns at line 38 and never reaches `GetCalendar`, `GetMoveDiagnostics`, or any appointment. Its name, its `<summary>`, and its inline comments all describe behavior it does not exercise. Recommended disposition: **retarget** the test (seed `MyDocuments`, assert `GetMoveDiagnostics` was called once with a null `ref` appointment) rather than delete it or duplicate it — test A2 covers the abort path with a real assertion, so leaving both as-is would be the duplication the epic prohibits. Flag this to the planner as a deliberate modification of an existing test, with a note in the plan.

### Risks to the plan

- **R1 — `BlockingCollection.TryAdd` cancellation semantics (affects A6).** Test A6 relies on `TryAdd(item, 20, ct)` throwing `OperationCanceledException` for an already-cancelled token on an *unbounded* collection. This matches the .NET Framework reference implementation, but it is a framework-behavior assumption, not a repository-verified fact. **Mitigation:** if A6 fails, route it through the S1 `MetricsAdder` seam (the seam is being added anyway for B2), which makes the cancelled-break path deterministic regardless of framework behavior.
- **R2 — `Items.Add()` optional-parameter matching in Moq (affects A3, A4).** The interop `Items.Add` signature carries an optional `object Type`; the exact `It.IsAny<object>()` vs no-arg setup shape must be confirmed against the interop assembly at implementation time. Existing `Mock<Items>` usages in `TaskMaster.Test` and `UtilitiesCS.Test` are the reference.
- **R3 — `ref AppointmentItem` verification (affects A4).** Moq cannot match a specific instance for a `ref` parameter. A4 must capture through `Callback` rather than an argument matcher; if that proves impractical for `ref` parameters in this Moq version, fall back to asserting `Save()` was called once and that the returned diagnostics reached the producer.
- **R4 — behavior-preservation discipline.** D2, D8 and D9 are defects that a well-intentioned implementer may "fix" while writing D1-D5. The epic NFR is explicit: "No behavior change to end-user QuickFiler flows." Tests D3 is deliberately a **characterization** test; the plan must say so in the task text so a reviewer does not read it as endorsing the truncation.
- **R5 — `QfcHomeController.cs` headroom (13 lines).** Any F7 task that adds lines to that file risks breaching 500. Route new metrics members into this file (which has 266 lines of headroom) and only then consider the § 8 cohesion move.
- **R6 — merge exposure against the epic integration branch.** #424 recently edited `QfcHomeController.cs`. Every S1/S2/S3/S5 change lands in `QfcHomeController.Metrics.cs` instead, which #424 did not touch — this is deliberate and should be preserved.

### Open questions for the planner / F1

1. **Ledger classification.** Confirm F1 classifies `QfcHomeController.Metrics.cs` as `testable` (the evidence in § 0 supports it). If F1 instead marks any part of it exempt, § 4 shrinks accordingly.
2. **Per-file target.** Issue #136 states 80% per file; `.claude/rules/csharp.md` requires ≥ 90% for new/changed modules and methods. S1/S2/S3 introduce *new* members in an *existing* file — confirm with F1 whether the 90% rule attaches to the new members (assumed yes) while the file target remains 80%.
3. **D7 disposition.** Is removing the uncoverable duplicate `TryGetValue` branch (lines 84-87 reusing `folderRoot`) in scope for F7 as a coverage-enabling simplification, or must it be promoted as a separate issue? It is behavior-identical and removes a permanently-partial branch from F7's own coverage number.
4. **D12 disposition.** Confirm that retargeting the existing vacuous test `GetMoveDiagnostics_NullAppointment_DoesNotThrow` is acceptable, versus leaving it untouched and accepting a partially-redundant assertion surface.
5. **Defect promotion.** Which of D1-D11 the orchestrator wants promoted to GitHub issues, and whether D1 (wrong stopwatch) and D3 (metrics never flushed) warrant their own bug features ahead of, or independent of, this coverage child.
