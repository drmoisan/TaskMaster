---
Timestamp: 2026-08-07T20-50
Feature: quickfiler-qfc-home-controller-coverage (epic child F7, issue #433)
Epic: quickfiler-per-file-coverage (issue #136)
Target file: C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-afcf27830d48e5590\QuickFiler\Controllers\QfcHomeController.Iteration.cs
Line count: 86
Worktree: C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-afcf27830d48e5590
Coverage classification authority: docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md (delivered by child F1, wave 0; not present on disk at research time)
Coverage evidence mechanism: F1's per-file line-coverage harness derived from the Cobertura output of scripts/vscode/Invoke-MSTestWithCoverage.ps1
Research method: static per-member cross-reference. No msbuild, no vstest, no coverage run performed.
---

# QfcHomeController.Iteration.cs — Per-File Coverage Research

## 0. Upstream contract consumed

This artifact is written to consume, not to substitute for, child F1 (`quickfiler-coverage-ledger`, wave 0):

- **Classification authority.** Whether `QuickFiler/Controllers/QfcHomeController.Iteration.cs` is `testable` or `ratified-exempt` is decided by the F1 ledger at `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md`. This research assumes `testable` and records the supporting evidence: a search for `ExcludeFromCodeCoverage` across `QuickFiler/Controllers/` returns 14 files, **none of which is any `QfcHomeController` partial**; the type is not form-derived and not designer-generated; `coverage.config` excludes only third-party module paths (Deedle, FSharp, Castle.Core, FluentAssertions, Moq, Microsoft.Testing, MSTest) and does not touch QuickFiler; and every dependency this file reaches is already behind a Moq-able interface (`IQfcDatamodel`, `IQfcQueue`, `IQfcFormController`). Under the epic's ratified reading of `CLAUDE.md` § UT2 ("without an injectable seam" is a live obligation, not a standing permission), the COM/VSTO exemption does not apply.
- **Measurement authority.** Per-file line coverage for this file is produced by F1's harness and committed under `<FEATURE>/evidence/qa-gates/`. No substitute harness is proposed. Every "covered / not covered" claim below is a static cross-reference to be confirmed numerically by F1's harness at execution time. The one estimate offered (§ 3.4) is labelled as an estimate.

## 1. File purpose and responsibilities

`QfcHomeController.Iteration.cs` is the smallest of the three `QuickFiler.Controllers.QfcHomeController` partials (the others are `QfcHomeController.cs`, 487 lines, and `QfcHomeController.Metrics.cs`, 234 lines). It carries the **queue-refill and session-timing state machine**: the code that pulls the next group of mail items out of the data model, hands it to the UI queue, and rotates the two session stopwatches.

Four members are declared:

| Member | Lines | Shape | Role |
| --- | --- | --- | --- |
| `IterateQueueAsync()` | 11-53 | `public async Task` | Background refill. Dequeues the next group and either enqueues it into `QfcQueue` or closes the queue for adding. Sole error-handling surface in the file. |
| `Iterate()` | 55-68 | `public void` | Synchronous refill-and-load. Resets `_stopWatch`, dequeues synchronously (or sync-over-async in high-confidence mode), loads directly into the form controller. |
| `Iterate2()` | 70-77 | `public void` | Queue-backed refill-and-load. Resets `_stopWatch`, takes a pre-built group from `QfcQueue`, loads it, then starts an un-awaited background refill. |
| `SwapStopWatch()` | 79-84 | `public void` | Rotates `_stopWatch` into `_stopWatchMoved` and starts a fresh `_stopWatch`. |

All four are declared on `Controllers/IQfcHomeController.cs` (lines 13-16).

### 1.1 Production call graph (verified by repository-wide search)

| Member | Production callers | Status |
| --- | --- | --- |
| `IterateQueueAsync()` | `QfcHomeController.cs:323` (`await Task.Run(IterateQueueAsync)` at the end of `RunAsync`); `QfcFormController.EventHandlers.cs:162` (`MoveAndIterate`, awaited); `:199` (`UiThread.Dispatcher.InvokeAsync(_parent.IterateQueueAsync)` retry path); `:373` (`var iterate = _parent?.IterateQueueAsync();` — result assigned, never awaited); `Iterate2()` line 76 | **Live**, four production call sites |
| `SwapStopWatch()` | `QfcFormController.EventHandlers.cs:142` (`LoadUiFromQueue`), `:191` (`MoveAndIterate` end-of-database path), `:372` | **Live**, three production call sites |
| `Iterate()` | `QfcFormController.cs:48` binds `Iterate = parent.Iterate` into the private delegate field `IterateDelegate Iterate` (`QfcFormController.cs:85`). That field is **never invoked** anywhere — the only other reference is `Iterate = null` at `QfcFormController.SetupDisposal.cs:225`. `QuickFiler/Legacy/QuickFileController.cs:123,128,696` is a different type and is not compiled into the coverage denominator. | **Dead in production**; reachable only from tests (see § 9 LD1) |
| `Iterate2()` | **None.** Repository-wide search for `Iterate2` returns only the declaration (`Controllers/IQfcHomeController.cs:14`), the definition (this file, line 70), and `QfcHomeControllerIterationTests.cs:424`. | **Dead in production** (see § 9 LD1) |

Consequence for planning: 23 of the file's 86 lines (`Iterate` 55-68 and `Iterate2` 70-77, 27% of the file) exercise production code that no production path reaches. They remain in the coverage denominator and must still be covered by F7; deletion is *not* proposed (see § 7 and § 9 LD1).

### 1.2 Stopwatch state model

Two fields, both declared in the sibling partial (`QfcHomeController.cs:443-444`):

- `_stopWatch` — timing the *current* filing session. Exposed read-only as `public Stopwatch StopWatch` (`QfcHomeController.cs:445-448`). Reset and started by `Iterate()` (57-58), `Iterate2()` (72-73), `SwapStopWatch()` (82-83), and also by `Run()` (`QfcHomeController.cs:267-268`) and `RunAsync()` (`:315-316`).
- `_stopWatchMoved` — the stopwatch of the session that just completed. Written **only** by `SwapStopWatch()` (line 81). Read by `QfcHomeController.Metrics.cs:42,44`.

The invariant this file owns: **`SwapStopWatch` is the only transition that preserves the outgoing measurement; `Iterate`/`Iterate2` discard it.** Nothing currently pins that distinction (§ 4 group C).

## 2. Dependency and seam inventory

### 2.1 Injectable today — no new seam required for any member of this file

| Dependency | Declaration | Kind | Reached from this file at | Test-controllable how |
| --- | --- | --- | --- | --- |
| `_datamodel` / `DataModel` (`IQfcDatamodel`) | `QfcHomeController.cs:428-433` | private field + `public ... { internal set; }` property | 15, 21, 62-63, 66 | `_controller.DataModel = mock.Object` — **interface seam, already used** by `QfcHomeControllerIterationTests.cs:98,132,205,270,326,374,421` |
| `QfcQueue` (`IQfcQueue`) | `QfcHomeController.cs:156` | `internal IQfcQueue { get; set; }` | 28, 35, 74 | `_controller.QfcQueue = mock.Object` — **interface seam, already used** (`IT:99,146,220,281,412`) |
| `_formController` (`IQfcFormController`) | `QfcHomeController.cs:415` | private field; public property `FormController` is get-only | 22, 29, 63, 66, 67, 75 | Reflection field assignment — **established pattern** in every existing home-controller suite (`IT:153-160,227-234,287-294,330-337,379-386,413-420`; `RA:134-141`; `I218:122`) |
| `Globals` (`IApplicationGlobals`) | `QfcHomeController.cs:155` | `internal ... { get; set; }` | 60 | Direct assignment (`internal`, test-visible) or a `Mock<IApplicationGlobals>` |
| `Token` / `_token` (`CancellationToken`) | `QfcHomeController.cs:466-470`; writer `CreateCancellationToken()` at `:454-458` (`internal`) | public get-only property over a private field | 13, 35, 44 | `_controller.CreateCancellationToken(); _controller.TokenSource.Cancel();` — **internal method + public `TokenSource` getter, no reflection needed**. Reflection on `_token` is the alternative already used at `QfcHomeControllerPropertyTests.cs:291-298` |
| `_stopWatch` / `_stopWatchMoved` | `QfcHomeController.cs:443-444` | private fields; `_stopWatch` readable via `public StopWatch` | 57, 72, 81, 82 | Reflection seed (`IT:439-446`, `MT:332,367`); read back through the public `StopWatch` property |

`[assembly: InternalsVisibleTo("QuickFiler.Test")]` is declared at `QfcHomeController.cs:18`, so all `internal` members above are directly reachable from the test project.

**Conclusion: every member and every branch in this file is reachable through interfaces and members that already exist. No new production seam is required.** This is the single most important planning result in this artifact; § 6 records it formally.

### 2.2 Hard-coded values and whether they reach a wall clock

| Line | Literal | Consumer | Reaches a real wall-clock wait under mocks? |
| --- | --- | --- | --- |
| 23 | `2000` (ms empty-queue poll) | `IQfcDatamodel.DequeueNextItemGroupAsync(int, int)` | **No.** The parameter is consumed only inside `QfcDatamodel`/`QfcStreamingDequeueConfidenceGate`. With a `Mock<IQfcDatamodel>` the value is an inert argument. Confirmed empirically by #424's evidence: the exact-argument test `IterateQueueAsync_WhenDequeueReturnsFullQualifiedPage_EnqueuesAllItems` runs in 272 ms (`evidence/qa-gates/pinned-suites.2026-08-07T00-12.md`). |
| 35 | `10000` (ms `CompleteAdding` timeout) | `IQfcQueue.CompleteAddingAsync(CancellationToken, int)` | **No.** The real implementation (`QfcQueue.cs:46-71`) builds a `CancellationTokenSource(timeout)` and polls with `Task.Delay(100, …)`, but with a `Mock<IQfcQueue>` returning `Task.CompletedTask` none of that executes. |
| 63 | `2000` | same as line 23 | **No** (same reasoning) |

**No seam is needed for either magic number, and neither may be changed.** The `2000` at line 23 is byte-pinned: `QfcHomeControllerIterationTests.cs:268` asserts `DequeueNextItemGroupAsync(8, 2000)` exactly, and #424 AC 12 requires that file byte-unmodified (§ 7). Extracting the literals into named constants is therefore out of scope for F7 even though it would preserve behavior.

### 2.3 UI-thread and COM exposure

- No WinForms control is constructed by this file. `Iterate2` line 74 destructures `IQfcQueue.Dequeue()`, whose return type is `(TableLayoutPanel Tlp, List<QfcItemGroup> ItemGroups)` (`Controllers/IQfcQueue.cs:25`); a loose Moq mock returns `(null, null)`, so no `TableLayoutPanel` is instantiated. `IQfcFormController.LoadItems(TableLayoutPanel, List<QfcItemGroup>)` (`Controllers/IQfcFormController.cs:29`) is likewise mocked. Verified by the fact that `IT.Iterate2_ExecutesCorrectly:405` passes today with exactly this arrangement.
- `Microsoft.Office.Interop.Outlook.MailItem` appears only as the element type of `IList<MailItem>`. Existing tests use `new Mock<MailItem>().Object`; no live COM object is created.
- No `MessageBox`, no `Show()`, no `UiThread.Dispatcher` call in this file. The `UiThread.Dispatcher.InvokeAsync` wrapper that once guarded the enqueue is commented out at line 27.

## 3. Per-member / per-branch coverage cross-reference

Test-file shorthand: `IT` = `QuickFiler.Test\Controllers\QfcHomeControllerIterationTests.cs`, `HT` = `QfcHomeControllerTests.cs`, `MT` = `QfcHomeControllerMetricsTests.cs`, `PT` = `QfcHomeControllerPropertyTests.cs`, `RA` = `QfcHomeControllerRunAsyncTests.cs`, `RAHC` = `QfcHomeControllerRunAsyncHighConfidenceTests.cs`, `I218` = `QfcHomeControllerIssue218Tests.cs`, `ZB` = `QfcInitEmailQueueZeroBatchTests.cs`.

### 3.1 Verified negatives (read in full, confirmed not to reach this file)

- `HT` — exercises the constructor, `Init()` and `InitAsync()` only. Lines 166-176 and 254-285 are commented-out blocks.
- `PT` — property accessors only. `StopWatch_PropertyWorksCorrectly:232` seeds `_stopWatch` by reflection and reads the property back; it never calls `Iterate`, `Iterate2` or `SwapStopWatch`.
- `MT` — reads `_stopWatch`/`_stopWatchMoved` (`:133,142,226,332,367`) but only as *seeded* inputs to the metrics methods; no member of this file is invoked.
- `RA`, `RAHC`, `I218` — all reach `IterateQueueAsync` **indirectly** through `RunAsync`'s `await Task.Run(IterateQueueAsync)` (`QfcHomeController.cs:323`). Every one of them sets `mockDataModel.Setup(x => x.Complete).Returns(true)` (`RA:255`, `RAHC:57,147,341,440`, `I218:110,214`), so the incidental execution stops at line 17 and covers only lines 13, 15 and 17. This is why `RAHC.RunAsync_HighConfidenceEmptyBatch_StillLoadsItemsAndStartsIteration:466-470` asserts `m.Complete` was read `Times.AtLeastOnce` — that assertion is about *initiation*, not about the refill body.
- `ZB` — **targets `QfcDatamodel.InitEmailQueue`, not this file.** It never constructs a `QfcHomeController`. Its three tests (`:118,147,177`) pin the issue #244 zero-batch short-circuit inside the datamodel. It pins **nothing** about `IterateQueueAsync`'s `listObjects.Count == 0` branch. The zero-batch branch of *this* file is pinned by `IT.IterateQueueAsync_QueueEmpty:124` instead. Recording this explicitly because the two are easy to confuse and the mandate calls it out.

`IT` is therefore the sole suite that exercises the bodies of this file's members.

### 3.2 Cross-reference table

| # | Member / branch | Lines | Covered-by (file + method) | Residual gap | Reachable without new seam? |
| --- | --- | --- | --- | --- | --- |
| **M1** | **`IterateQueueAsync()`** | **11-53** | | | |
| M1.1 | entry `Token.ThrowIfCancellationRequested()` — **not** throwing | 13 | `IT.IterateQueueAsync_DataModelComplete:78`, `IT.IterateQueueAsync_QueueEmpty:124`, `IT.IterateQueueAsync_Queue2:185`, `IT.IterateQueueAsync_WhenDequeueReturnsFullQualifiedPage_EnqueuesAllItems:259`; incidentally `RA:300`, `RAHC:167,356,455`, `I218:155,242` | none | n/a |
| M1.2 | entry `ThrowIfCancellationRequested()` — **throwing** | 13 | **Nothing** | Whole path. Note the throw is *outside* the `try` (which opens at 19), so it escapes the method uncaught — a contract distinct from M1.7/M1.8 | **Yes** — `CreateCancellationToken()` + `TokenSource.Cancel()` (§ 2.1) |
| M1.3 | `_datamodel.Complete == true` → early `return` | 15-18 | `IT.IterateQueueAsync_DataModelComplete:78` (asserts all three collaborators `Times.Never`); incidentally every `RA`/`RAHC`/`I218` test | none | n/a |
| M1.4 | `_datamodel.Complete == false` → enter `try` | 15, 19 | `IT:124`, `IT:185`, `IT:259` | none | n/a |
| M1.5 | `await DequeueNextItemGroupAsync(ItemsPerIteration, 2000)` | 21-24 | `IT:124`, `IT:185`, `IT:259`. Exact-argument pin `(8, 2000)` at **`IT:268`** — **frozen by #424 AC 12** | none | n/a |
| M1.6 | `listObjects.Count > 0` → `await QfcQueue.EnqueueAsync(listObjects, _formController.Groups)` | 25, 28-30 | `IT.IterateQueueAsync_Queue2:185` (`Times.Once`), `IT:259` (asserts sequence equality of the items and identity of the `Groups` argument) | none | n/a |
| M1.7 | `listObjects.Count == 0` → `await QfcQueue.CompleteAddingAsync(Token, 10000)` | 32-36 | `IT.IterateQueueAsync_QueueEmpty:124` (`CompleteAddingAsync` `Times.Once`, `EnqueueAsync` `Times.Never`) | Argument values are matched with `It.IsAny<>` only; the `10000` and the identity of `Token` are unpinned. **Low value** — see § 4 rejected R3 | n/a |
| M1.8 | `catch (OperationCanceledException)` — swallow | 38-41 | **Nothing** | Whole handler | **Yes** — mock `DequeueNextItemGroupAsync` to throw `OperationCanceledException` |
| M1.9 | `catch (System.Exception)` with `Token.IsCancellationRequested == true` — swallow | 42-47 | **Nothing** | Whole branch | **Yes** — a Moq `Callback` cancels `TokenSource`, then `Throws` a non-OCE exception |
| M1.10 | `catch (System.Exception)` with `Token.IsCancellationRequested == false` — `throw;` | 42-44, 48-51 | **Nothing** | Whole branch, including the rethrow-preserves-type contract | **Yes** — mock throws a non-OCE exception, token never cancelled |
| **M2** | **`Iterate()`** | **55-68** | | | |
| M2.1 | `_stopWatch = new Stopwatch(); _stopWatch.Start();` | 57-58 | Executed by `IT.Iterate_ExecutesCorrectly:313` and `IT.Iterate_HighConfidenceEnabled_DoesNotLoadDirectSynchronousBatch:357` | **Executed but unasserted.** No test proves the field is replaced with a *new, running* instance | Yes |
| M2.2 | `Globals?.QfSettings?.HighConfidenceModeEnabled == true` — flag TRUE | 60 | `IT:357` (`SetupQfSettings(highConfidenceEnabled: true, …)`) | none | n/a |
| M2.3 | same — flag FALSE | 60 | `IT:313` (`SetupQfSettings(highConfidenceEnabled: false, …)`) | none | n/a |
| M2.4 | same — `Globals` **null** (first null-conditional short-circuit) | 60 | **Nothing** | Whole sub-branch | **Yes** — `_controller.Globals = null` (`internal` setter) |
| M2.5 | same — `Globals.QfSettings` **null** (second null-conditional short-circuit) | 60 | **Nothing** | Whole sub-branch | **Yes** — `SetupGet(x => x.QfSettings).Returns((IAppQuickFilerSettings)null)` |
| M2.6 | ternary TRUE arm: `DequeueNextItemGroupAsync(…, 2000).GetAwaiter().GetResult()` | 61-65 | `IT:357` (asserts the sync `DequeueNextItemGroup` was **not** used) | none | n/a |
| M2.7 | ternary FALSE arm: `_datamodel.DequeueNextItemGroup(ItemsPerIteration)` | 66 | `IT:313` (`Times.Once`) | none | n/a |
| M2.8 | `_formController.LoadItems(listObjects)` | 67 | `IT:313` (content assertion), `IT:357` (negative assertion) | none | n/a |
| **M3** | **`Iterate2()`** | **70-77** | | | |
| M3.1 | `_stopWatch = new Stopwatch(); _stopWatch.Start();` | 72-73 | Executed by `IT.Iterate2_ExecutesCorrectly:405` | **Executed but unasserted** (same gap as M2.1) | Yes |
| M3.2 | `(var tlp, var itemGroups) = QfcQueue.Dequeue();` | 74 | `IT:405` (`Times.Once`) | none | n/a |
| M3.3 | `_formController.LoadItems(tlp, itemGroups)` | 75 | `IT:405` (`Times.Once`, `It.IsAny<>` arguments) | none | n/a |
| M3.4 | `_ = IterateQueueAsync();` — **synchronously completing** case | 76 | `IT:405` arranges `Complete == true`, so the callee runs to line 17 without ever yielding and the discarded task is already completed when `Iterate2` returns | none for this case | n/a |
| M3.5 | `_ = IterateQueueAsync();` — **genuinely detached** case (`Complete == false`, refill continues after `Iterate2` returns) | 76 | **Nothing** | The whole fire-and-forget ordering invariant is unpinned: nothing proves `Iterate2` returns before the refill completes, and nothing proves the detached continuation runs to completion | **Yes** — `TaskCompletionSource` gating, § 5 |
| M3.6 | `_ = IterateQueueAsync();` — **faulting** detached task (callee reaches the M1.10 rethrow) | 76 | **Nothing** | The exception is discarded and never surfaces to the caller (latent defect LD2) | **Yes** — same gating |
| **M4** | **`SwapStopWatch()`** | **79-84** | | | |
| M4.1 | `_stopWatchMoved = _stopWatch;` | 81 | `IT.SwapStopWatch_ExecutesCorrectly:435` — asserts `_stopWatchMoved` is reference-equal to the seeded instance | none | n/a |
| M4.2 | `_stopWatch = new Stopwatch(); _stopWatch.Start();` | 82-83 | Executed by `IT:435` | **Executed but unasserted.** `IT:435` seeds a *never-started* stopwatch and asserts only the move; nothing pins that the outgoing stopwatch's measurement is preserved, that `StopWatch` afterwards is a *different* instance, or that it is running | Yes |

### 3.3 Duplication guard — assertions that already exist and must not be re-created

`IterateQueueAsync` early-return-on-`Complete` (`IT:78`), zero-batch routing to `CompleteAddingAsync` (`IT:124`), non-empty routing to `EnqueueAsync` (`IT:185`), exact-argument dequeue + item-identity enqueue (`IT:259`), `Iterate` synchronous-path load (`IT:313`), `Iterate` high-confidence bypass-avoidance (`IT:357`), `Iterate2` dequeue-and-load (`IT:405`), `SwapStopWatch` field move (`IT:435`). Every proposed test in § 4 was checked against this list; the two that touch the same members (C3 vs `IT:435`, D-group vs `IT:405`) assert strictly different post-conditions and are called out individually.

### 3.4 Estimated current line coverage (estimate only — F1's harness is authoritative)

The only lines with **no** covering test are the two catch handlers: line 38 (`catch (OperationCanceledException)`), line 44 (`if (this.Token.IsCancellationRequested)`) and line 50 (`throw;`), plus the associated compiler-emitted handler entry points. Against roughly 27-30 coverable sequence points in the file, that puts the current figure in the **mid-to-high 80s percent**, i.e. plausibly already at or above the epic's 80% per-file floor.

Planning consequence: **F7's value on this file is predominantly branch coverage, error-path coverage and behavior pinning, not headline line coverage.** The plan should not be written as if the file were starting from a low number, and the numeric baseline must be taken from F1's harness before the plan's acceptance criteria are fixed. If F1's harness reports the file already above 80%, the § 4 test set is still justified by `.claude/rules/general-unit-test.md` § Scenario Completeness (negative flows, error handling, concurrency, state transitions) and by the epic's own leading indicator on regression escapes — but the AC wording should reflect "raise branch coverage and close the error-path gaps" rather than "reach 80%".

## 4. Residual gap list and proposed individual test cases

Each row is one atomic plan task. All tests: MSTest `[TestClass]`/`[TestMethod]`, Moq, FluentAssertions, Arrange–Act–Assert, no `Thread.Sleep`, no `Task.Delay`, no wall-clock waits, no temporary files, no live COM, no live WinForms form, no popup, no UI-thread dependency.

**Home file: a new `QuickFiler.Test\Controllers\QfcHomeControllerIterationCoverageTests.cs`.** `QfcHomeControllerIterationTests.cs` (464 lines) is **frozen byte-unmodified** by #424 AC 12 (§ 7) and must not be touched, so a new file is mandatory rather than merely preferred. Projected size: a lean fixture on the `QfcHomeControllerIssue218Tests` model (that suite's `Setup` at `I218:26-40` omits the `SetUpMockIntelRes` boilerplate and works) at ≈ 60 lines, plus a shared `ArrangeIteration(...)` builder at ≈ 30 lines, plus 12 tests averaging ≈ 22 lines ≈ **355 lines**, leaving ≈ 145 lines under the 500-line ceiling. If the fixture grows, split along the groups below into `…IterationCancellationTests.cs` (A) and `…IterationOrderingTests.cs` (C + D).

### Group A — `IterateQueueAsync` error and cancellation paths (closes M1.2, M1.8, M1.9, M1.10)

| # | Gap | Proposed `[TestMethod]` | Arrange / Act / Assert sketch |
| --- | --- | --- | --- |
| A1 | M1.2 (line 13 throwing) | `IterateQueueAsync_WhenTokenAlreadyCancelled_ThrowsBeforeReadingTheDataModel` | **A:** `controller.CreateCancellationToken(); controller.TokenSource.Cancel();` assign a `Mock<IQfcDatamodel>` and a `Mock<IQfcQueue>` with no `Complete` setup. **Act:** `Func<Task> act = () => controller.IterateQueueAsync();` **Assert:** `await act.Should().ThrowAsync<OperationCanceledException>()` **and** `mockDataModel.VerifyGet(m => m.Complete, Times.Never)`. Pins that the entry guard sits *outside* the `try` and therefore propagates, unlike A2. |
| A2 | M1.8 (38-41) | `IterateQueueAsync_WhenDequeueThrowsOperationCanceled_CompletesWithoutFaulting` | **A:** `Complete == false`; `DequeueNextItemGroupAsync(It.IsAny<int>(), It.IsAny<int>())` `.Throws(new OperationCanceledException())`; token **not** cancelled; form-controller mock supplies `ItemsPerIteration` and `Groups`. **Act:** `await controller.IterateQueueAsync()`. **Assert:** `act.Should().NotThrowAsync()`; `EnqueueAsync` `Times.Never`; `CompleteAddingAsync` `Times.Never`. *(Alternative arrangement if `Throws` proves awkward on the async setup: `.Returns(Task.FromCanceled<IList<MailItem>>(alreadyCancelledToken))`, which surfaces `TaskCanceledException` — a subclass of `OperationCanceledException` — at the await and reaches the same handler.)* |
| A3 | M1.9 (42-47, swallow) | `IterateQueueAsync_WhenEnqueueFaultsAfterCancellationRequested_SwallowsTheException` | **A:** `controller.CreateCancellationToken()` (not yet cancelled); `Complete == false`; dequeue returns two mocked `MailItem`s; `EnqueueAsync` `.Callback(() => controller.TokenSource.Cancel()).Throws(new InvalidOperationException("com failure"))` — the callback flips `Token.IsCancellationRequested` to `true` **before** the non-OCE exception is raised inside the `try`. **Act/Assert:** `await act.Should().NotThrowAsync()`, and `EnqueueAsync` `Times.Once` to prove the handler was actually entered. |
| A4 | M1.10 (48-51, rethrow) | `IterateQueueAsync_WhenEnqueueFaultsWithoutCancellation_RethrowsTheOriginalException` | **A:** identical to A3 minus the cancel callback; token never cancelled. **Act/Assert:** `await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("com failure")`. Pins both the branch *and* the bare-`throw` type/message preservation. |

### Group B — `Iterate` null-guard sub-branches (closes M2.4, M2.5)

| # | Gap | Proposed `[TestMethod]` | Sketch |
| --- | --- | --- | --- |
| B1 | M2.4 (`Globals` null) | `Iterate_WhenGlobalsIsNull_UsesTheSynchronousDequeuePath` | **A:** `controller.Globals = null;` `DequeueNextItemGroup(It.IsAny<int>())` returns two mocked items; form-controller mock. **Act:** `controller.Iterate()`. **Assert:** `DequeueNextItemGroup` `Times.Once`; `DequeueNextItemGroupAsync(It.IsAny<int>(), It.IsAny<int>())` `Times.Never`; `LoadItems` called once with those two items. |
| B2 | M2.5 (`QfSettings` null) | `Iterate_WhenQuickFilerSettingsAreAbsent_UsesTheSynchronousDequeuePath` | **A:** globals mock with `SetupGet(x => x.QfSettings).Returns((IAppQuickFilerSettings)null)`. **Act/Assert:** as B1. Distinct arrange, distinct short-circuit point; both are required because the two `?.` operators are independent branch points in the Cobertura branch report. |

### Group C — stopwatch state transitions (closes M2.1, M3.1, M4.2)

| # | Gap | Proposed `[TestMethod]` | Sketch |
| --- | --- | --- | --- |
| C1 | M2.1 (57-58) | `Iterate_ReplacesTheSessionStopwatchWithAFreshRunningInstance` | **A:** seed `_stopWatch` by reflection with a *stopped* `Stopwatch`; high-confidence disabled; datamodel + form-controller mocks. **Act:** `controller.Iterate()`. **Assert:** `controller.StopWatch.Should().NotBeSameAs(seeded)` and `controller.StopWatch.IsRunning.Should().BeTrue()`. |
| C2 | M3.1 (72-73) | `Iterate2_ReplacesTheSessionStopwatchWithAFreshRunningInstance` | **A:** seed `_stopWatch` as in C1; `Complete == true` so the fire-and-forget completes synchronously; queue + form-controller mocks. **Act:** `controller.Iterate2()`. **Assert:** as C1. |
| C3 | M4.2 (82-83) | `SwapStopWatch_PreservesTheOutgoingElapsedTimeAndStartsAFreshInstance` | **A:** seed `_stopWatch` with a **started** `Stopwatch`. **Act:** `controller.SwapStopWatch()`. **Assert:** `_stopWatchMoved` (reflection) is reference-equal to the seeded instance; `controller.StopWatch` is `NotBeSameAs` the seeded instance **and** `IsRunning == true`. **Not a duplicate of `IT.SwapStopWatch_ExecutesCorrectly:435`**, which asserts only the first of these three post-conditions; the plan task text must say so explicitly so a reviewer does not read it as re-authoring a frozen test. |

### Group D — fire-and-forget re-entrancy in `Iterate2` (closes M3.5, M3.6)

Full design rationale in § 5. All three use a `TaskCompletionSource` as the dequeue gate; none uses a timer, a poll or a sleep.

| # | Gap | Proposed `[TestMethod]` | Sketch |
| --- | --- | --- | --- |
| D1 | M3.5 (non-blocking half) | `Iterate2_WhenBackgroundRefillIsPending_ReturnsWithoutWaitingForTheEnqueue` | **A:** `dequeueGate = new TaskCompletionSource<IList<MailItem>>()`; `Complete == false`; `DequeueNextItemGroupAsync(It.IsAny<int>(), It.IsAny<int>())` returns `dequeueGate.Task`; `QfcQueue.Dequeue()` returns default; form-controller mock supplies `ItemsPerIteration` and `Groups`; `EnqueueAsync` returns `Task.CompletedTask`. **Act:** `controller.Iterate2();` (returns synchronously). **Assert, taken while the gate is still open:** `LoadItems(It.IsAny<TableLayoutPanel>(), It.IsAny<List<QfcItemGroup>>())` `Times.Once` **and** `EnqueueAsync(...)` `Times.Never`. Race-free by construction: the continuation cannot have run because its antecedent is deliberately incomplete. |
| D2 | M3.5 (completion half) | `Iterate2_AfterTheBackgroundDequeueCompletes_TheRefillEnqueuesTheItems` | **A:** as D1, plus `enqueued = new TaskCompletionSource<bool>()` and `EnqueueAsync` `.Callback(() => enqueued.TrySetResult(true)).Returns(Task.CompletedTask)`. **Act:** `controller.Iterate2(); dequeueGate.SetResult(items); await enqueued.Task;` **Assert:** `EnqueueAsync(items, groups)` `Times.Once`. Decorate `[Timeout(5000)]` purely as a hang guard — on the passing path `SetResult` runs the continuation inline on the test thread and zero wall-clock time is consumed. Precedent for a bounded (non-sleep) completion wait: `ZB.InitEmailQueue_ZeroBatchSize_StillStartsBackgroundWorker:147-164`. |
| D3 | M3.6 (fault path) | `Iterate2_WhenTheBackgroundRefillFaults_DoesNotSurfaceTheFaultToTheCaller` | **A:** as D1 but `EnqueueAsync` `.Callback(() => faulted.TrySetResult(true)).Throws(new InvalidOperationException("refill failed"))`; token never cancelled, so `IterateQueueAsync` reaches the M1.10 rethrow inside the discarded task. **Act:** `System.Action act = () => { controller.Iterate2(); dequeueGate.SetResult(items); };` **Assert:** `act.Should().NotThrow()` and `faulted.Task.IsCompleted.Should().BeTrue()`. **Explicitly a characterization test** — it documents the current unobserved-exception behavior (§ 9 LD2) and must be labelled as such in the plan task so it is not read as endorsing it. Safety verified: no `<ThrowUnobservedTaskExceptions enabled="true"/>` exists anywhere in the repository (searched; zero matches), and `QuickFiler.Test\app.config` contains only binding redirects, so the finalizer-time unobserved exception is swallowed by the .NET Framework default and cannot destabilize sibling tests. |

**Total proposed: 12 test cases** (A ×4, B ×2, C ×3, D ×3). **Required new production seams: zero.**

### Rejected test candidates (recorded so the planner does not re-add them)

- **R1 — a second rethrow test via the zero-batch branch** (`CompleteAddingAsync` throws without cancellation). Same handler, same lines, same assertion as A4; differs only in which collaborator raises. Pure duplication.
- **R2 — `IterateQueueAsync` with a `null` dequeue result.** `DequeueNextItemGroupAsync` returning `null` makes line 25 throw `NullReferenceException`, which the general catch rethrows — i.e. it covers exactly the lines A4 already covers, while asserting a defect (§ 9 LD4) as if it were intended behavior. Record the defect; do not test it.
- **R3 — exact-argument assertions on `CompleteAddingAsync(Token, 10000)`.** Adds no line or branch coverage over `IT:124` and creates a second literal pin on a magic number that a future fix may legitimately change.
- **R4 — asserting the `2000` argument from `Iterate` (line 63).** `IT:268` already owns the exact-argument pin for the sibling call at line 23; a second one on a dead method is churn.

## 5. Deterministic-ordering strategy for the fire-and-forget `IterateQueueAsync` in `Iterate2`

### 5.1 The hazard

`Iterate2` line 76 is `_ = IterateQueueAsync();`. Three consequences:

1. **No completion handle.** The `Task` is discarded, so no caller — production or test — can await, observe or cancel the refill.
2. **Ordering is implicit.** The contract `Iterate2` intends is "load the already-dequeued UI group first, *then* refill in the background". Nothing enforces or records it.
3. **Faults are unobserved.** When `IterateQueueAsync` reaches its rethrow (line 50), the exception lands on a discarded `Task`. With the .NET Framework default (`ThrowUnobservedTaskExceptions` absent repository-wide — verified), it is swallowed at finalization: no log entry, no dialog, no crash. The refill silently stops.

Contrast with the sibling call site: `RunAsync` uses `await Task.Run(IterateQueueAsync)` (`QfcHomeController.cs:323`). That form **does** observe the fault and propagates it to `LaunchAsync`, whose only `catch` is for `OperationCanceledException` (`:74-84`) — so a non-OCE refill failure during startup escapes to the ribbon, while the same failure from `Iterate2` disappears. The two call sites have opposite, and equally unpinned, failure semantics. Line 322 (`//_ = IterateQueueAsync();`) shows `RunAsync` was deliberately migrated away from the discard form; `Iterate2` was not.

### 5.2 Chosen strategy — antecedent gating with a `TaskCompletionSource` (no production change)

The refill's first yielding point is `await _datamodel.DequeueNextItemGroupAsync(...)` at line 21. Everything before it — the entry guard, the `Complete` read, and the `try` entry — executes **synchronously on the caller's thread** inside `Iterate2`. That single fact makes the ordering observable without any timing primitive:

1. Arrange the mocked `DequeueNextItemGroupAsync` to return `tcs.Task` for a `TaskCompletionSource<IList<MailItem>>` the test owns and has **not** completed.
2. Call `Iterate2()`. It returns as soon as the refill suspends on that incomplete task.
3. Assert *synchronously*, before completing the TCS, that `LoadItems` ran and `EnqueueAsync` did not. This is race-free by construction — the continuation is provably not runnable while its antecedent is incomplete. **This is the ordering proof, and it costs zero wall-clock time.**
4. Complete the TCS with `SetResult`. Because the TCS is created without `TaskCreationOptions.RunContinuationsAsynchronously`, the continuation executes inline on the calling thread, so the refill has finished by the time `SetResult` returns.
5. Observe completion through a second `TaskCompletionSource<bool>` that the `EnqueueAsync` (or, for D3, the throwing) mock sets in a `Callback`, and `await` it. On the passing path it is already complete. `[Timeout(5000)]` on the method is a hang guard, not a wait.

This uses only existing interface seams (`IQfcDatamodel`, `IQfcQueue`, `IQfcFormController`), satisfies `.claude/rules/general-unit-test.md` § Determinism Infrastructure (no `Task.Delay`, no `Thread.Sleep`, no `Date.now`-equivalent, no real wall-clock wait), and requires **no production edit**.

### 5.3 Rejected ordering mechanisms

- **`internal Task LastIterationTask { get; private set; }` on `Iterate2`** (assign instead of discard, expose for awaiting). Would make ordering directly awaitable and would incidentally create the hook needed to fix LD2. Rejected for F7 because § 5.2 already achieves the coverage objective with zero production surface, and adding an observability field to a live type inside a coverage child is exactly the scope creep the epic's "minimum seam" rule targets. **Record it as the recommended remediation shape for the LD2 issue**, not as F7 work.
- **`internal Action<Func<Task>> FireAndForget { get; set; }` (injectable task-scheduling delegate).** Seam-hierarchy tier 2, and the natural place to add exception observation. Rejected for the same reason plus a larger production surface than the field.
- **`Task.Yield()` / `await Task.Yield()` in the test, or awaiting `Task.WhenAny` on a scheduler drain.** Rejected: relies on scheduler timing, is non-deterministic under load, and is the class of test the repository's determinism rule exists to prevent.
- **Polling `Mock.Invocations.Count` in a loop with a timeout.** Rejected: a wall-clock wait in disguise.

## 6. Required seams (minimum set, ranked against the existing seams)

**Minimum set: none.** All 12 proposed tests are reachable through seams that already exist. Ranked against `.claude/rules/csharp.md` § DI Seams (interface > injectable delegate > adapter), each dependency this file touches resolves at the *highest* tier:

| Dependency reached from this file | Tier available today | Verdict |
| --- | --- | --- |
| `_datamodel` (lines 15, 21, 62-63, 66) | **Tier 1 interface seam** — `IQfcDatamodel` (`Interfaces/IQfcDatamodel.cs`) already declares `Complete` (56), both `DequeueNextItemGroupAsync` overloads (26, 40-45) and `DequeueNextItemGroup` (46); injected through the `internal set` on `DataModel` | No seam needed |
| `QfcQueue` (lines 28, 35, 74) | **Tier 1 interface seam** — `IQfcQueue` (`Controllers/IQfcQueue.cs`) already declares `EnqueueAsync` (26), `CompleteAddingAsync` (24) and `Dequeue` (25); injected through the `internal` settable property | No seam needed |
| `_formController` (lines 22, 29, 63, 66, 67, 75) | **Tier 1 interface seam** — `IQfcFormController` (`Controllers/IQfcFormController.cs`) declares `ItemsPerIteration` (19), `Groups` (18) and both `LoadItems` overloads (28, 29). The *field* has no setter, so assignment is by reflection — the pattern already used in all six existing home-controller suites | No production change proposed. Widening to an `internal` setter is **not** recommended: it would add public-ish surface to satisfy a convention the suite has already settled, and it would put an F7 edit into `QfcHomeController.cs`, which has only 13 lines of headroom (§ 8) |
| `Globals` (line 60) | **Existing `internal` settable property** (`QfcHomeController.cs:155`) | No seam needed |
| `Token` (lines 13, 35, 44) | **Existing `internal CreateCancellationToken()`** (`:454-458`) plus the `public TokenSource` getter | No seam needed |
| `_stopWatch` / `_stopWatchMoved` (57, 72, 81, 82) | Private fields, seeded by reflection; read back through the `public StopWatch` property | No seam needed. Replacing `Stopwatch` with a `TimeProvider`-based elapsed source is **explicitly rejected** — it would break the live pin `RA.RunAsync_ExecutesCorrectly:303` (`_controller.StopWatch.IsRunning`) and `PT.StopWatch_PropertyWorksCorrectly:232` |

### Explicitly rejected production changes

- **Named constants for `2000` / `10000` (lines 23, 35, 63).** Behavior-preserving in principle, but `2000` at line 23 is byte-pinned by the frozen `IT:268` and the epic gives F7 no mandate to touch live literals. Reject.
- **A null guard on `listObjects` at line 25** (see LD4). It is a production behavior change, not a coverage enabler. Promote as an issue; do not implement in F7.
- **Deleting the dead `Iterate()` / `Iterate2()`** (see LD1). Would remove 23 lines from the denominator, but it is a breaking change to `IQfcHomeController` and requires an edit to `QfcFormController.cs:48,85` and `SetupDisposal.cs:225` — **F6-owned files**. Reject for F7; § 7 records the cross-child note.
- **`internal Task LastIterationTask` on `Iterate2`** (§ 5.3). Reject for F7; recommend as the LD2 remediation shape.

## 7. Cross-child contract notes

**No cross-child contract addition is required by this file.** Every member this file calls already exists on an interface a Moq mock can satisfy:

| Sibling-owned surface | Owning child | Use in this file | Contract change needed? |
| --- | --- | --- | --- |
| `IQfcDatamodel.Complete` / `.DequeueNextItemGroupAsync(int,int)` / `.DequeueNextItemGroup(int)` | **F5** | Lines 15, 21, 62-63, 66 | **No.** All three members are already on `Interfaces/IQfcDatamodel.cs` (lines 56, 26, 46) and are already mocked by `IT`. Advisory only: F5 must not remove or re-signature the two-argument `DequeueNextItemGroupAsync` overload — it is byte-pinned at `IT:268` under #424 AC 12. |
| `IQfcQueue.EnqueueAsync` / `.CompleteAddingAsync` / `.Dequeue` | **F2** | Lines 28, 35, 74 | **No.** All three are already on `Controllers/IQfcQueue.cs` (26, 24, 25). Advisory only: `Dequeue()`'s tuple return `(TableLayoutPanel, List<QfcItemGroup>)` is what lets `Iterate2` be tested without constructing a WinForms control (a loose mock yields `(null, null)`); F2 must not change it to a non-nullable or eagerly-constructed shape. |
| `IQfcFormController.ItemsPerIteration` / `.Groups` / `.LoadItems(IList<MailItem>)` / `.LoadItems(TableLayoutPanel, List<QfcItemGroup>)` | **F6** | Lines 22, 29, 63, 66, 67, 75 | **No.** All four are already on `Controllers/IQfcFormController.cs` (19, 18, 28, 29). |
| `QfcFormController.Iterate` delegate field (`QfcFormController.cs:48,85`; nulled at `SetupDisposal.cs:225`) | **F6** | Binds this file's dead `Iterate()` | **No change requested.** Recorded as a note: if F6 removes the never-invoked delegate field while covering `QfcFormController.cs`, `QfcHomeController.Iterate()` loses its last reference and its removal becomes a clean follow-up. F7 must not pre-empt that; see LD1. |
| `QfcCollectionController` / `IQfcCollectionController` | F11 | Only as the *type* of the `Groups` argument passed at line 29 | **No.** Never dereferenced by this file. |
| `KeyboardHandler` / `Kbd*` / `Ka*` | F3 | Not referenced | No |
| `coverage.config`, shared build property files | F1 | Not touched | No |

Files edited by the § 4 recommendations: **one new test file only** (`QuickFiler.Test\Controllers\QfcHomeControllerIterationCoverageTests.cs`). No production file is modified.

## 8. Issue #424 interaction findings (against the merged state)

Sources read: `docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/spec.md` (v1.2, AC list at lines 230-242), `evidence/qa-gates/pinned-suites.2026-08-07T00-12.md`, plus the merged production files.

### 8.1 Merge state confirmed

#424 has landed in this checkout. `QfcHomeController.cs:294-305` carries the `QfcScanProgressBandMapper` wiring, the 200 ms poll and the `QfcStreamingDequeueConfidenceGate.DefaultFirstBatchDeadline` argument, and `:323` is `await Task.Run(IterateQueueAsync)`. `QfcDatamodel.QueueProcessing.cs:21` carries the `_remainingLoadActive` liveness flag. Planning targets the merged shape.

### 8.2 Confirmed frozen-test-file list (#424 AC 12, `spec.md:241`)

Verified against the AC text and against `evidence/qa-gates/pinned-suites.2026-08-07T00-12.md`, which records byte-identity and a 64/64 pass:

| File | #424 obligation | F7 rule |
| --- | --- | --- |
| `QuickFiler.Test\Controllers\QfcHomeControllerIterationTests.cs` | **byte-unmodified**; the exact-argument pin `DequeueNextItemGroupAsync(8, 2000)` at line 268 must keep passing | **Do not edit.** This is F7's primary existing suite for the target file, which makes accidental edits the single largest process risk in this child. Add tests in a new file only. |
| `QuickFiler.Test\Controllers\QfcInitEmailQueueZeroBatchTests.cs` | byte-unmodified | Do not edit. (Also confirmed irrelevant to this file — § 3.1.) |
| `QuickFiler.Test\Controllers\QfcHighConfidencePreFilterTests.cs` | byte-unmodified | Do not edit. |
| `QuickFiler.Test\Controllers\QfcFormControllerTests.cs` | byte-unmodified | Do not edit (F6 territory in any case). |
| `QuickFiler.Test\Controllers\QfcHomeControllerIssue218Tests.cs` | diff constrained to the four already-applied overload-shape hunks (lines 101, 160, 192, 226) | Do not edit; do not re-shape its `Setup`/`Verify` matchers. |

Additional #424 pins that F7 must not contradict, even though the files are not on the byte-unmodified list:

- `RAHC.RunAsync_HighConfidenceEnabled_LoadsFirstPageFromStreamingDequeue:137-146,180-191` and `RAHC.RunAsync_HighConfidenceScanProgress_MapsReportsIntoTheZeroToThirtyBand:289-388` own the `RunAsync` → four-argument-dequeue contract (#424 AC 1, AC 6). None of the § 4 tests touches `RunAsync`.
- `RA.RunAsync_ExecutesCorrectly:303` asserts `_controller.StopWatch.IsRunning`. This is the direct reason § 6 rejects any seam that replaces the `Stopwatch` fields.

### 8.3 Members of the target file touched by #424: none — but one behavior changed underneath it

`QfcHomeController.Iteration.cs` does not appear in #424's files-to-change set, and `spec.md:86` states explicitly that "the post-UI iteration call site (`QfcHomeController.Iteration.cs:23`, pinned by the exact-argument test at `QfcHomeControllerIterationTests.cs:268`) is left unchanged." That is true of the *file*. It is **not** true of the *behavior*, and the planner must know why:

`IterateQueueAsync` line 21 calls the **two-argument** `DequeueNextItemGroupAsync`. Post-#424 that overload is no longer a distinct implementation — `QfcDatamodel.QueueProcessing.cs:66-76` now delegates to the four-argument overload with `QfcStreamingDequeueConfidenceGate.DefaultFirstBatchDeadline`, which is `TimeSpan.FromSeconds(12)` (`QfcStreamingDequeueConfidenceGate.cs:22`). **The post-UI background refill therefore inherited the 12-second first-batch deadline**, silently, as a consequence of the delegation. The gate's deadline exit at `QfcStreamingDequeueConfidenceGate.cs:110` returns the accepted-so-far set, so an empty return from line 21 no longer implies source exhaustion. See § 9 LD3 for the consequence.

This is a finding about #424's blast radius, not a criticism of its AC set — #424's ACs were satisfied as written. F7 should record it and promote it; F7 must **not** change the behavior.

### 8.4 Non-blocking observation

#424's coverage AC (`spec.md:242`) recorded a merge-base repository line rate of 70.19% and re-scoped the repo-wide 80% floor to the testable denominator. F7 must not treat that re-scoping as authority over its own per-file target: issue #136 measures per-file coverage, and the classification authority for this file is F1's ledger (§ 0).

## 9. Risks, latent defects, and open questions

### Latent production defects found during cross-reference (report only; do **not** fix inside a coverage child)

Per repository practice, each should be promoted through the issue lifecycle rather than left as prose in a feature folder.

- **LD1 — `Iterate()` and `Iterate2()` are dead production code.** `Iterate` is bound into `QfcFormController`'s private `IterateDelegate Iterate` field (`QfcFormController.cs:48`, declared `:85`) which is never invoked — its only other appearance is `Iterate = null` (`SetupDisposal.cs:225`). `Iterate2` has no reference anywhere outside its declaration (`Controllers/IQfcHomeController.cs:14`), its definition, and `IT:424`. Together they are 23 of the file's 86 lines and three of its four public members. Both are also on the public `IQfcHomeController` interface, so removal is a breaking API change touching an F6-owned file. **Severity: dead-code / interface-segregation.** Recommended sequencing: let F6 remove the never-invoked delegate field first, then remove `Iterate`, `Iterate2` and their interface declarations in a dedicated cleanup issue. Until then F7 covers them, because they are compiled production code in the denominator.
- **LD2 — the fire-and-forget refill discards its faults.** `_ = IterateQueueAsync();` (line 76) leaves the returned `Task` unobserved. When the refill reaches its rethrow (line 50) the exception is swallowed at finalization — no log, no dialog, no retry — and background refilling silently stops for the rest of the session. The sibling call site `QfcHomeController.cs:323` uses `await Task.Run(...)`, which observes the fault (and, via `LaunchAsync`'s OCE-only `catch` at `:74`, lets a non-OCE fault escape to the ribbon). `QfcFormController.EventHandlers.cs:373` (`var iterate = _parent?.IterateQueueAsync();`, assigned and never awaited) has the same defect as `Iterate2`. Recommended remediation shape: assign the task to an `internal Task` and attach a fault-logging continuation, or await it at the call sites that can. **Severity: silent failure of a background path.** Characterization test D3 documents current behavior only.
- **LD3 — post-#424, an empty refill batch no longer means "source exhausted", but line 32 still assumes it does.** `IterateQueueAsync` treats `listObjects.Count == 0` as end-of-source and calls `QfcQueue.CompleteAddingAsync(Token, 10000)`, which reaches `_queue.CompleteAdding()` (`QfcQueue.cs:59`) — an **irreversible** close of the UI queue. Since #424, the two-argument dequeue inherits a 12-second first-batch deadline (§ 8.3), so in high-confidence mode a slow scan that accepts nothing within 12 seconds returns empty **while unscanned items remain in the master queue**. The UI queue is then closed for the rest of the session and the user sees filing stop early. Before #424 the two-argument overload was unbounded, so an empty return could only mean exhaustion and the inference at line 32 was sound. **Severity: functional regression risk on the high-confidence path, introduced by #424 into a file #424 declared out of scope.** This is the highest-value finding in this artifact and should be promoted with priority. F7 must not fix it; the existing `IT:124` already characterizes the current routing, and no new test should assert that the routing is correct.
- **LD4 — no null guard on the dequeue result at line 25.** `IQfcDatamodel.DequeueNextItemGroupAsync` can return `null`: in normal mode `DequeueDirectAsync` (`QfcDatamodel.QueueProcessing.cs:101-108`) returns `UnhookDequeuedNodes(_masterQueue.TryTakeFirst(quantity)?.ToList())`, `UnhookDequeuedNodes` returns `null` for a `null` input (`:147-150`), and `LockingLinkedList.TryTakeFirst(int)` returns `null` when `n < 1` (`LockingLinkedList.cs:403-406`; same guard in `LockingObservableLinkedList.cs:309-312`). `listObjects.Count` at line 25 then throws `NullReferenceException`, which the general catch rethrows (token not cancelled) — escaping as an unobserved fault from `Iterate2` or to the ribbon from `RunAsync`. **Reachability is conditional on `_formController.ItemsPerIteration` being `<= 0`; verify at plan time against `QfcFormController.LoadItemsPerIteration()` and the spinner's minimum before assigning severity.** Recorded as a conditional finding, not an asserted defect.
- **LD5 — `Iterate()` blocks its calling thread on an async scan.** Line 61-65 uses `.GetAwaiter().GetResult()` over `DequeueNextItemGroupAsync`. Post-#424 that call is bounded by the 12-second default deadline rather than unbounded, so the exposure is materially smaller than the #424 defect — but a synchronous 12-second block on what would in production be the UI thread, with no progress reporting, is still the #424 defect class in miniature. Currently moot because `Iterate()` is dead (LD1). Record alongside LD1 rather than separately.
- **LD6 — swallowed cancellation is indistinguishable from success.** Both `catch` handlers (38-41, 42-47) have empty bodies whose only content is a commented-out `logger.Debug` (lines 40, 46). A cancelled or cancellation-adjacent refill returns a completed `Task` with no signal to the caller and no log line, so `MoveAndIterate` (`QfcFormController.EventHandlers.cs:162`) cannot distinguish "refilled" from "cancelled mid-refill". `.claude/rules/general-code-change.md` § Error Handling requires that errors not be silently ignored. **Severity: diagnosability.** The minimal fix is to restore the two `logger.Debug` lines; even that is a production change and belongs in an issue, not in F7.

### Risks to the plan

- **R1 — accidental edit of the frozen `QfcHomeControllerIterationTests.cs`.** F7 owns the file, the new tests live next to it, and the natural instinct when adding an iteration test is to append to the existing class. #424 AC 12 requires byte-identity. **Mitigation:** name the new file explicitly in every plan task, and add a Phase-0 baseline task that records the file's hash so the final QA gate can prove byte-identity, mirroring `evidence/qa-gates/pinned-suites.2026-08-07T00-12.md`.
- **R2 — the file may already exceed the 80% per-file floor (§ 3.4).** If F1's harness confirms this, an AC worded as "reach 80%" would be trivially satisfied and the § 4 work would look optional. **Mitigation:** take F1's numeric baseline **before** fixing the AC wording, and word the AC around the specific uncovered branches (M1.2, M1.8, M1.9, M1.10, M2.4, M2.5) plus the branch-coverage figure.
- **R3 — Moq `.Throws` on an `async Task`-returning setup.** A3/A4/D3 rely on the mock raising synchronously at the call site (inside the `try`), not on returning a faulted `Task`. Both reach the same handlers here, but the *timing* differs and D1's synchronous assertion window depends on the exact shape. **Mitigation:** the planner should fix the arrangement shape per test and note the fallback (`Task.FromException<T>` / `Task.FromCanceled<T>`) in the task text.
- **R4 — `CancellationTokenSource.Cancel()` inside a Moq `Callback` (A3).** Moq executes `Callback` before `Throws`, so the token flips before the exception is raised. This is documented Moq behavior but should be confirmed against the pinned Moq version at implementation time; the fallback is to cancel the source *before* the act and arrange the dequeue (rather than the enqueue) to throw the non-OCE exception.
- **R5 — `[Timeout]` in D2/D3.** Used strictly as a hang guard. If a reviewer reads any bounded wait as a policy violation, the alternative is the `ZB:160-163` pattern (`Task.Wait(TimeSpan)` with an explicit "not a fixed sleep" comment), which is already ratified in this test project. Prefer `await` + `[Timeout]`; keep the justification in the test's doc comment.
- **R6 — merge exposure against the epic integration branch.** #424 recently rewrote `QfcHomeController.cs:274-324` and `QfcDatamodel.QueueProcessing.cs`. Because § 4 proposes **no production edit at all** for this file, F7's diff for `QfcHomeController.Iteration.cs` is empty and its merge exposure is limited to one new test file. Preserve that property.
- **R7 — behavior-preservation discipline.** LD2, LD3, LD4 and LD6 are all defects an implementer may be tempted to fix while writing D3 or A4. The epic NFR is explicit: "No behavior change to end-user QuickFiler flows." D3 is deliberately a characterization test and the plan task must say so.

### Open questions for the planner / F1

1. **Ledger classification.** Confirm F1 classifies `QfcHomeController.Iteration.cs` as `testable` (§ 0 evidence supports it).
2. **Baseline before AC.** What does F1's harness report as this file's current per-file line and branch rate? § 3.4's estimate must be replaced with the measured figure before the plan's acceptance criteria are written.
3. **Dead-code disposition (LD1).** Does the epic want `Iterate()`/`Iterate2()` covered (23 lines of test effort on unreachable production code) or removed in a coordinated F6/F7 cleanup? Removal would shrink the denominator and is arguably the "refactor first" answer the epic ratified — but it is a breaking interface change touching an F6-owned file and is outside a coverage child's mandate as currently scoped. **This is the single decision most likely to change the shape of the plan.**
4. **LD3 promotion priority.** LD3 is a functional regression risk on a user-visible path introduced by a change that merged yesterday. Should it be promoted as its own bug feature ahead of, or independent of, this coverage child?
5. **Defect promotion set.** Which of LD1-LD6 the orchestrator wants promoted to GitHub issues.
