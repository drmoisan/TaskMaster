---
Timestamp: 2026-08-07T20-50
Epic: quickfiler-per-file-coverage (parent issue #136)
Child: F7 quickfiler-qfc-home-controller-coverage (issue #433)
Target file: QuickFiler/Controllers/QfcHomeController.cs
Target file (absolute): C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-afcf27830d48e5590\QuickFiler\Controllers\QfcHomeController.cs
Line count: 487 (500-line hard limit; 13 lines of headroom)
Worktree: C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-afcf27830d48e5590
Base commit: 74be1964
Upstream contract: F1 quickfiler-coverage-ledger (wave 0) — not yet on disk
Toolchain executed: none (msbuild/vstest deliberately not run; measurement deferred to F1's harness)
---

# Research — `QuickFiler/Controllers/QfcHomeController.cs` (F7, issue #433)

## Upstream contract consumed (F1, wave 0)

This artifact is written to **consume** F1's contract, not to substitute for it.

1. **Classification authority.** Whether `QuickFiler/Controllers/QfcHomeController.cs` is `testable`
   or `ratified-exempt` is decided by the ratified exemption ledger at
   `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md`. That file does not exist at
   the time of this research. **This artifact assumes the classification will be `testable`** — the
   file carries no `[ExcludeFromCodeCoverage]` attribute today (verified: no such attribute anywhere
   in `QfcHomeController.cs`), it is already ~71% covered by an existing eight-file test suite, and
   under the epic's Shared Design §1 reconciliation ("refactor first, exempt only the irreducible
   remainder") a file with seven working injectable seams cannot qualify as irreducible. If F1's
   ledger classifies it otherwise, every recommendation below is void and this artifact must be
   re-run.
2. **Measurement authority.** F1's per-file line-coverage harness, derived from the Cobertura output
   of `Invoke-MSTestWithCoverage.ps1`, is the only accepted evidence mechanism for the per-file
   figure. No substitute harness is proposed here.
3. **Indicative prior measurement (not evidence).** The figures used for gap sizing in §3 and §8 are
   read from an *existing committed Cobertura artifact produced by a different feature*:
   `docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml`,
   line 21643. It reports `line-rate="0.713924" branch-rate="0.51" complexity="65"` for
   `QuickFiler.Controllers.QfcHomeController` / `QuickFiler\Controllers\QfcHomeController.cs`. That
   artifact was generated against the same file content that is on disk now (the #424 changes are
   present at `QfcHomeController.cs:292-305`), so its per-line hit map is a sound *planning input*.
   It is **not** F1 harness output and must not be cited as acceptance evidence.

Sibling F7 files not covered by this artifact (one research artifact per production file per issue
#136): `QfcHomeController.Metrics.cs` (indicative 65.09% line / 62.5% branch, same Cobertura line
22314), `QfcHomeController.Iteration.cs` (86.25% / 66.67%, line 22612), `IQfcHomeController.cs`
(interface-only), `Interfaces/IFilerHomeController.cs` (interface-only).

---

## 1. File purpose and responsibilities

`QfcHomeController` is the top-level lifecycle coordinator for a QuickFiler session. This file is the
primary partial of a three-file partial family (`QfcHomeController.cs`,
`QfcHomeController.Metrics.cs`, `QfcHomeController.Iteration.cs`), all declaring
`public partial class QfcHomeController : IQfcHomeController` in namespace `QuickFiler.Controllers`.

Responsibilities carried by **this** partial:

| Responsibility | Lines | Notes |
| --- | --- | --- |
| Assembly attribute | 18 | `[assembly: InternalsVisibleTo("QuickFiler.Test")]` — the whole child's test access depends on this line staying in a compiled file. |
| Static logger | 24-26 | log4net, initialized in `.cctor`. |
| Construction | 30-36 | Private parameterless ctor (used only by `LaunchAsync`) + public `(IApplicationGlobals, Action)` ctor. |
| Host-bound entry point | 38-87 | `static LaunchAsync` — installs a `WindowsFormsSynchronizationContext`, builds a `CancellationTokenSource`, builds and `Initialize()`s a real `ProgressTracker` (which constructs and `Show()`s a `ProgressViewer` form), then orchestrates `InitAsync` → `Loaded = true` → `RunAsync`, catching `OperationCanceledException`. |
| Component wiring | 89-153 | `Init()` (synchronous) and `InitAsync(...)` (async, background data-model load) construct/assign `_datamodel`, `_explorerController`, `_formViewer`, `_keyboardHandler`, `QfcQueue`, `_formController`, `_uiSyncContext`, `_uiScheduler`. |
| Seam surface | 155-245 | Seven injectable `Func<>` loader properties plus `Globals` / `QfcQueue` / `ParentCleanup`. |
| Session start | 248-324 | `Run()` (legacy synchronous path) and `RunAsync(ProgressTracker)` (the ribbon path). Both branch on `Globals?.QfSettings?.HighConfidenceModeEnabled`. |
| Background-worker completion | 326-351 | `Worker_RunWorkerCompleted` — enables two viewer controls through `UiThread.Dispatcher.Invoke`, sets `WorkerComplete`, or shows a `MessageBox` on error. |
| Metrics buffer + timer consumer | 353-386 | `BlockingCollection<string> _metrics`, `_metricsConsumers`, `_lockObject`, `_fileName`, and `async void TimedConsumerAsync(object, ElapsedEventArgs)` which drains the collection and writes a session file via `FileIO2.WriteTextFileAsync`. |
| Teardown | 388-397 | `Cleanup()` — nulls every component reference and invokes `ParentCleanup`. |
| State surface | 399-485 | `Loaded`, `ExplorerController`, `FormController`, `KeyboardHandler`, `DataModel`, `FilerQueue`, `UiScheduler`, `StopWatch`, `CreateCancellationToken()`, `TokenSource`, `Token`, `WorkerComplete`, `UiSyncContext`. |

**Separation-of-concerns assessment.** The file mixes four concerns: host bootstrap (`LaunchAsync`),
component composition (`Init`/`InitAsync` + seams), session orchestration (`Run`/`RunAsync`/
`Worker_RunWorkerCompleted`), and a metrics-buffer/file-writer sub-system (353-386). The metrics
sub-system is cohesively tied to `QfcHomeController.Metrics.cs` (which owns
`NonBlockingProducer`, the only writer into `_metrics`) and is arguably misfiled here.

---

## 2. Dependency and seam inventory

### 2.1 Already injectable (no work required)

| Seam | Lines | Kind | Default | Test precedent |
| --- | --- | --- | --- | --- |
| `QfcDataModelLoader` | 159-163 | delegate | `new QfcDatamodel(globals, cancel)` | `QfcHomeControllerTests.Init_InitializesCorrectly` |
| `QfcAsyncDataModelLoader` | 165-173 | delegate | `QfcDatamodel.LoadAsync(...)` | `QfcHomeControllerTests.InitAsync_InitializesCorrectly` |
| `QfcExplorerControllerLoader` | 175-182 | delegate | `new QfcExplorerController(...)` | both `Init*` tests |
| `QfcKeyboardHandlerLoader` | 184-189 | delegate | `new KeyboardHandler(...)` | both `Init*` tests |
| `QfcQueueLoader` | 191-197 | delegate | `new QfcQueue(...)` | both `Init*` tests |
| `QfcFormControllerLoader` | 199-229 | delegate | `new QfcFormController(...).Init()` | both `Init*` tests |
| `HighConfidencePreFilterLoader` | 236-244 | delegate | `QfcHighConfidencePreFilter.FilterAsync` | `QfcHomeControllerRunAsyncHighConfidenceTests.HighConfidencePreFilterLoader_CanBeOverridden_ForTesting` |
| `TimeProvider` | `QfcHomeController.Metrics.cs:17` | property | `TimeProvider.System` | `QfcHomeControllerMetricsTests` (`FakeTimeProvider`) |
| `DataModel` (internal setter) | 429-433 | property | — | used directly by most suites |
| `QfcQueue` (internal setter) | 156 | property | — | `QfcHomeControllerIterationTests` |
| Private-field injection by reflection | — | test-side | — | `_formController`, `_formViewer`, `_stopWatch`, `_stopWatchMoved`, `_token`, `_tokenSource`, `_uiScheduler`, `_uiSyncContext`, `_workerComplete`, `_datamodel` are all assigned by reflection in the existing suites. |

`[assembly: InternalsVisibleTo("QuickFiler.Test")]` (line 18) makes every `internal` member directly
reachable. `QfcHighConfidencePreFilter.cs:11` additionally declares
`[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]`, so Moq can proxy internal QuickFiler
types if needed.

### 2.2 Hard-coded (not injectable today)

| Hard-coded dependency | Line(s) | Why it blocks a test |
| --- | --- | --- |
| `new WindowsFormsSynchronizationContext()` | 47-50 | Mutates thread-static `SynchronizationContext.Current`; host bootstrap. |
| `new ProgressTracker(tokenSource).Initialize()` | 59 | `ProgressTracker.Initialize()` (`UtilitiesCS/Threading/ProgressTracker.cs:31-58`) reads the static `UiThread.Dispatcher`, constructs a real `ProgressViewer`, and calls `ShowProgressViewer(_progressViewer)` → **shows a form**. Directly prohibited in unit tests. |
| `new QfcHomeController()` inside a `static` method | 53 | The instance is created *inside* `LaunchAsync`, so a test cannot pre-assign any of the seven loader seams on it. This is the single structural reason `LaunchAsync` is 0% covered. |
| `new QfcFormViewer()` | 93, 133 | A `Form`-derived WinForms type. Constructed (never shown) by the *existing* `Init_InitializesCorrectly` / `InitAsync_InitializesCorrectly` tests — verified by hits on lines 93 and 133 in the indicative Cobertura. This is a pre-existing live-form construction in the test suite. |
| `TaskScheduler.FromCurrentSynchronizationContext()` | 136 | Throws `InvalidOperationException` when `SynchronizationContext.Current` is null. It currently succeeds only as a **side effect** of line 133: constructing a WinForms `Form` auto-installs a `WindowsFormsSynchronizationContext` on the calling thread. Any seam that removes the live-form construction at line 133 breaks line 136 unless a scheduler seam is added at the same time. This coupling is load-bearing. |
| `MessageBox.Show(msg)` | 338 | A modal popup requiring human interaction — an outright unit-test-policy violation. This is why the whole `e.Error != null` branch (335-339) is 0%. |
| `UiThread.Dispatcher.Invoke(...)` | 343 | Static UI dispatcher. Currently reached only because `QfcHomeControllerRunAsyncTests.Worker_RunWorkerCompleted_HandlesCompletionCorrectly` calls `UiThread.Init(false)` (line 329 of that test) — mutable global state, a UT4 concern. A `IUiDispatcher` interface seam already exists at `UtilitiesCS/Threading/IUiDispatcher.cs` with a production implementation `WpfUiDispatcher`. |
| `FileIO2.WriteTextFileAsync(...)` | 372-377 | Static filesystem write. Real file I/O and temporary files are both prohibited. |
| `_metrics.GetConsumingEnumerable().ToArray()` | 367 | `BlockingCollection.GetConsumingEnumerable()` blocks until `CompleteAdding()` is called. `CompleteAdding()` is **never** called on `_metrics` anywhere in the repository. See §9 R3. |
| `new Stopwatch()` / `.Start()` | 267-268, 315-316 | Real wall clock, but only ever asserted through `IsRunning`, so it is not a determinism problem today. |

---

## 3. Per-member coverage cross-reference

Line ranges are physical source ranges in the current file. "Covered-by" lists only tests that
actually execute the member. "Indicative" hit data is from the #424 Cobertura artifact cited above.

| # | Member | Lines | Covered-by (file :: test method) | Residual gap | Reachable without new seam? |
| --- | --- | --- | --- | --- | --- |
| 1 | `logger` static field init | 24-26 | every suite (via `.cctor`) | none | n/a — covered |
| 2 | `QfcHomeController()` private ctor | 30 | **none** | line 30 uncovered; also leaves the 0%-half of the lambda-cache conditions on lines 163, 172, 181, 189, 197, 210, 243 (7 conditions, the main driver of the 51% branch rate) | **yes** — `Activator.CreateInstance(..., BindingFlags.NonPublic \| BindingFlags.Instance, ...)` |
| 3 | `QfcHomeController(IApplicationGlobals, Action)` | 32-36 | `QfcHomeControllerTests::Constructor_InitializesCorrectly`; `[TestInitialize] Setup` of all 7 controller suites | none (100% line, 100% branch) | n/a — covered |
| 4 | `static LaunchAsync(IApplicationGlobals, Action, TimeProvider)` | 38-87 | **none** (a test existed and is commented out at `QfcHomeControllerTests.cs:166-176`) | **34 uncovered lines**: 43, 47-50, 53-54, 57-59, 62-70, 72-81, 83-84, 86-87. Uncovered paths: sync-context install (both branches of `is null` at 47), controller creation, `timeProvider ?? TimeProvider.System` (54), progress-tracker construction, the `try` happy path, the `OperationCanceledException` catch with `progress.Report(100)` and `controller = null`. | **no** — needs seam S4 (extract host-neutral core) plus S5a/S5b for the `InitAsync` it calls |
| 5 | `Init()` | 89-109 | `QfcHomeControllerTests::Init_InitializesCorrectly` | none on lines (100%); constructs a live `QfcFormViewer` at line 93 (policy debt, not a coverage gap) | n/a — covered |
| 6 | `InitAsync(...)` | 111-153 | `QfcHomeControllerTests::InitAsync_InitializesCorrectly` | none on lines (118-153 all hit); constructs a live `QfcFormViewer` at 133 and depends on its side effect for line 136 (policy debt) | n/a — covered |
| 7 | `Globals` / `QfcQueue` / `ParentCleanup` auto-props | 155-157 | `Constructor_InitializesCorrectly`; `IterateQueueAsync_*` (for `QfcQueue`) | none | n/a — covered |
| 8 | `QfcDataModelLoader` decl + default lambda | 159-163 | both `Init*` tests (declaration + cache branch) | none at line granularity (initializer and lambda body share line 163) | n/a |
| 9 | `QfcAsyncDataModelLoader` decl | 165-172 | `InitAsync_InitializesCorrectly` | none | n/a |
| 10 | ↳ its **default lambda body** (`QfcDatamodel.LoadAsync`) | 173 | **none** | 1 uncovered line | **no** — `QfcDatamodel.LoadAsync` is live-COM; `QfcDatamodel` is sibling **F5**-owned |
| 11 | `QfcExplorerControllerLoader` decl | 175-181 | both `Init*` tests | none | n/a |
| 12 | ↳ its **default lambda body** (`new QfcExplorerController`) | 182 | **none** | 1 uncovered line | **yes** — the ctor (`QfcExplorerController.cs:27-37`) needs only `globals.Ol.App.ActiveExplorer()`, which every existing `[TestInitialize]` already mocks |
| 13 | `QfcKeyboardHandlerLoader` decl + default lambda | 184-189 | both `Init*` tests (single-line initializer) | none at line granularity | n/a |
| 14 | `QfcQueueLoader` decl + default lambda | 191-197 | both `Init*` tests (single-line initializer) | none at line granularity | n/a |
| 15 | `QfcFormControllerLoader` decl | 199-219 | both `Init*` tests | none | n/a |
| 16 | ↳ its **default lambda body** (`new QfcFormController(...).Init()`) | 220-229 | **none** | **10 uncovered lines** | **probably yes, medium confidence** — `QfcFormController`'s ctor is mock-tolerant and all four `Init()` steps early-return against a loose `IQfcFormViewer` (`QfcFormController.SetupDisposal.cs:24-30, 50-57, 77-80, 151-154`). Fragile because it depends on sibling **F6** internals. See §4 TC18. |
| 17 | `HighConfidencePreFilterLoader` decl | 231-243 | `RunAsync*HighConfidenceTests::HighConfidencePreFilterLoader_CanBeOverridden_ForTesting` (which only exercises an *override*, never the default) | none on the declaration | n/a |
| 18 | ↳ its **default lambda body** (`QfcHighConfidencePreFilter.FilterAsync`) | 244 | **none** | 1 uncovered line | **yes** — `FilterAsync` has an explicit empty-input fast path at `QfcHighConfidencePreFilter.cs:57-60` that returns without touching COM |
| 19 | `Run()` | 248-272 | `QfcHomeControllerRunAsyncTests::Run_ExecutesCorrectly`; `::Run_HighConfidenceEnabled_DoesNotLoadUnfilteredInitialBatch` | **line coverage 100%; branch 75%.** Line 250 `Globals?.QfSettings?.HighConfidenceModeEnabled == true` is at 50%/50%: neither `Globals == null` nor `QfSettings == null` is exercised | **yes** — pure null arrangement |
| 20 | `RunAsync(ProgressTracker)` | 274-324 | `RunAsync_ExecutesCorrectly`; `RunAsync_HighConfidenceEnabled_LoadsFirstPageFromStreamingDequeue`; `RunAsync_HighConfidenceDisabled_DoesNotPreFilterUsesPlainOverload`; `RunAsync_HighConfidenceDisabled_UsesPlainOverloadOnly`; `RunAsync_HighConfidenceScanProgress_MapsReportsIntoTheZeroToThirtyBand`; `RunAsync_HighConfidenceEmptyBatch_StillLoadsItemsAndStartsIteration`; `QfcHomeControllerIssue218Tests::RunAsync_HighConfidenceEnabled_DoesNotPreFilterInitialGuiBatch`; `::RunAsync_HighConfidence_LoadsInitialBatchWithoutPreFilter` | **line coverage 100%; branch gaps only.** Line 279 at 50%/50% (null `Globals`, null `QfSettings` unexercised). Line 312 `progress?.Report(100)` at 50% — the null arm is **unreachable dead code**: line 277 already dereferences `progress` unconditionally | partially — null-`Globals`/null-`QfSettings` **yes**; line 312's null arm **no (dead)** |
| 21 | `Worker_RunWorkerCompleted(object, RunWorkerCompletedEventArgs)` | 326-351 | `QfcHomeControllerRunAsyncTests::Worker_RunWorkerCompleted_HandlesCompletionCorrectly` (success path only) | **6 uncovered lines**: 329, 333 (`e.Cancelled == true` arm) and 335, 337, 338, 339 (`e.Error != null` arm). Branches at 328 and 334 both 50% | cancelled arm **yes**; error arm **no** — line 338 is `MessageBox.Show` (seam S1) |
| 22 | ↳ display-enable lambda `<Worker_RunWorkerCompleted>b__48_0` | 344-347 | same test (via `UiThread.Init(false)`) | none, but the coverage depends on mutating the process-global `UiThread` | n/a — covered (with a UT4 caveat) |
| 23 | `_metrics`, `_metricsConsumers` field initializers | 353-356 | via ctor | none | n/a — covered |
| 24 | `_lockObject` static field initializer | 357 | via `.cctor` | none. **`_lockObject` (357) and `_fileName` (358) are never read in this file**; `_fileName` is written at `QfcHomeController.Metrics.cs:153` and never read anywhere | n/a |
| 25 | `TimedConsumerAsync(object, ElapsedEventArgs)` | 362-386 | **none** | **22 uncovered lines**: 363, 365-384, 386. Largest single testable block in the file. Also unreachable in production (see §9 R3) | **no** — needs seam S2 (file-writer delegate); test must also `CompleteAdding()` `_metrics` by reflection |
| 26 | `Cleanup()` | 388-397 | `QfcHomeControllerPropertyTests::Cleanup_ExecutesCorrectly` | none (100%) | n/a — covered |
| 27 | `_loaded` field + `Loaded` get/set | 399-404 | `QfcHomeControllerPropertyTests::Loaded_PropertyWorksCorrectly` | none | n/a — covered |
| 28 | `_explorerController` + `ExplorerController` get/set | 408-413 | `::ExplorerController_PropertyWorksCorrectly` | none | n/a — covered |
| 29 | `_formController` + `FormController` get | 415-419 | `::FormController_PropertyWorksCorrectly` | none | n/a — covered |
| 30 | `_keyboardHandler` + `KeyboardHandler` get/set | 421-426 | `::KeyboardHandler_PropertyWorksCorrectly` | none | n/a — covered |
| 31 | `_datamodel` + `DataModel` get/set | 428-433 | `::DataModel_PropertyWorksCorrectly` | none | n/a — covered |
| 32 | `FilerQueue` get + initializer | 435 | `::FilerQueue_PropertyWorksCorrectly` | none | n/a — covered |
| 33 | `_uiScheduler` + `UiScheduler` get | 437-441 | `::UiScheduler_PropertyWorksCorrectly` | none | n/a — covered |
| 34 | `_stopWatchMoved`, `_stopWatch`, `StopWatch` get | 443-448 | `::StopWatch_PropertyWorksCorrectly`; `QfcHomeControllerIterationTests::SwapStopWatch_ExecutesCorrectly` | none | n/a — covered |
| 35 | `_formViewer` field | 450 | reflection-injected by 4 suites | none | n/a |
| 36 | `CreateCancellationToken()` | 454-458 | **none** | **4 uncovered lines**: 455-458. `internal`, zero dependencies, no callers anywhere in the repository | **yes** — trivially |
| 37 | `_tokenSource` + `TokenSource` get | 460-464 | `::TokenSource_PropertyWorksCorrectly` | none | n/a — covered |
| 38 | `_token` + `Token` get | 466-470 | `::Token_PropertyWorksCorrectly` | none | n/a — covered |
| 39 | `_workerComplete` + `WorkerComplete` get/private set | 472-477 | `::WorkerComplete_PropertyWorksCorrectly`; set exercised by `Worker_RunWorkerCompleted_HandlesCompletionCorrectly` | none | n/a — covered |
| 40 | `_uiSyncContext` + `UiSyncContext` get | 479-483 | `::UiSyncContext_PropertyWorksCorrectly` | none | n/a — covered |

### 3.1 Aggregate gap

Uncovered lines, grouped (79 total against ≈276 coverable lines; 197 covered → 71.39%):

| Block | Lines | Count | Seam required |
| --- | --- | --- | --- |
| `LaunchAsync` | 43, 47-50, 53-54, 57-59, 62-70, 72-81, 83-84, 86-87 | 34 | S4 + S5a + S5b |
| `TimedConsumerAsync` | 363, 365-384, 386 | 22 | S2 |
| `QfcFormControllerLoader` default body | 220-229 | 10 | none (F6-coupled) |
| `Worker_RunWorkerCompleted` error arm | 335, 337-339 | 4 | S1 |
| `CreateCancellationToken` | 455-458 | 4 | none |
| `Worker_RunWorkerCompleted` cancelled arm | 329, 333 | 2 | none |
| `QfcAsyncDataModelLoader` default body | 173 | 1 | none feasible (F5-owned) |
| `QfcExplorerControllerLoader` default body | 182 | 1 | none |
| `HighConfidencePreFilterLoader` default body | 244 | 1 | none |
| Private parameterless ctor | 30 | 1 | none |

**Branch-rate drivers (51%).** Seven lambda-cache conditions sit at 0% purely because the private
parameterless ctor is never invoked (163, 172, 181, 189, 197, 210, 243). Covering member #2 lifts all
seven at once. The remaining branch gaps are the four null-guard halves at 250 (×2) and 279 (×2), the
two arms at 328 and 334, and the unreachable null arm at 312.

---

## 4. Residual gaps → proposed individual test cases

One row per test case; each becomes its own atomic-plan task. All are MSTest + Moq +
FluentAssertions, AAA, no live form, no popup, no UI thread, no `Thread.Sleep`/`Task.Delay`, no temp
files. **Every name below was checked against all eight existing suites; none duplicates an existing
`[TestMethod]`.**

Proposed new test files (mirroring the production tree under `QuickFiler.Test/Controllers/`):
`QfcHomeControllerLifecycleTests.cs` (TC1-TC3, TC16-TC18),
`QfcHomeControllerWorkerCompletionTests.cs` (TC4-TC6),
`QfcHomeControllerMetricsConsumerTests.cs` (TC7-TC11),
`QfcHomeControllerModeGuardTests.cs` (TC12-TC15),
`QfcHomeControllerLaunchTests.cs` (TC19-TC23, only if S4 is adopted).

### Tier A — required, no production change

| TC | `[TestMethod]` name | Arrange / Act / Assert sketch | Lines gained | Branch gained |
| --- | --- | --- | --- | --- |
| TC1 | `PrivateParameterlessConstructor_LeavesEveryLoaderSeamAtItsNonNullDefault` | **A:** nothing. **Act:** `Activator.CreateInstance(typeof(QfcHomeController), BindingFlags.Instance \| BindingFlags.NonPublic, null, null, null)`. **Assert:** instance is `QfcHomeController`; each of the seven loader properties `.Should().NotBeNull()`; `Loaded.Should().BeFalse()`; `FilerQueue.Should().NotBeNull()`. | 30 (1) | 7 conditions on 163/172/181/189/197/210/243 |
| TC2 | `CreateCancellationToken_AssignsFreshSourceAndItsMatchingToken` | **A:** controller from public ctor. **Act:** `_controller.CreateCancellationToken()`. **Assert:** `TokenSource.Should().NotBeNull()`; `Token.Should().Be(TokenSource.Token)`; `Token.IsCancellationRequested.Should().BeFalse()`. | 455-458 (4) | — |
| TC3 | `CreateCancellationToken_CalledTwice_ReplacesSourceWithoutCancellingThePrevious` | **A:** call once, capture `first = TokenSource` and `firstToken = Token`. **Act:** call again. **Assert:** `TokenSource.Should().NotBeSameAs(first)`; `first.IsCancellationRequested.Should().BeFalse()`; `Token.Should().NotBe(firstToken)`. *(state-transition scenario per UT2; no additional line gain — keep only if the plan wants scenario completeness)* | 0 | — |
| TC4 | `WorkerRunWorkerCompleted_WhenCancelled_DoesNotEnableViewerControlsAndLeavesWorkerCompleteFalse` | **A:** loose `Mock<IQfcFormViewer>` with `ItemsPerLoadEnabled`/`SkipButtonEnabled` set up as properties defaulting `false`; reflection-inject `_formViewer`. **Do not** call `UiThread.Init`. **Act:** reflection-invoke `Worker_RunWorkerCompleted(null, new RunWorkerCompletedEventArgs(null, null, cancelled: true))`. **Assert:** both viewer flags `.Should().BeFalse()`; `_controller.WorkerComplete.Should().BeFalse()`. | 329, 333 (2) | 328 → 100% |
| TC12 | `Run_WhenGlobalsIsNull_UsesTheFullInitializationBatchAndSkipsStreamingDequeue` | **A:** `Mock<IQfcDatamodel>` returning an empty list from `InitEmailQueue`; `Mock<IQfcFormController>` with `ItemsPerIteration = 5`; `Mock<IQfcFormViewer>`; set `Globals = null` (internal setter). **Act:** `Run()`. **Assert:** `InitEmailQueue(5, It.IsAny<BackgroundWorker>())` `Times.Once`; `DequeueNextItemGroupAsync` never invoked. | 0 | 250 cond 0 |
| TC13 | `Run_WhenQfSettingsIsNull_UsesTheFullInitializationBatchAndSkipsStreamingDequeue` | As TC12 but `Globals` mocked with `QfSettings` returning `null`. | 0 | 250 cond 1 |
| TC14 | `RunAsync_WhenGlobalsIsNull_UsesTheFullInitializationBatchAndSkipsStreamingDequeue` | **A:** mocked `ProgressTracker` (existing `SetupMockProgressTracker` pattern); data model returning empty from `InitEmailQueueAsync` and `Complete == true`; form controller `ItemsPerIteration = 5`; `Globals = null`. **Act:** `await RunAsync(progress)`. **Assert:** `InitEmailQueueAsync(5, ...)` `Times.Once`; no 4-arg `DequeueNextItemGroupAsync`. | 0 | 279 cond 0 |
| TC15 | `RunAsync_WhenQfSettingsIsNull_UsesTheFullInitializationBatchAndSkipsStreamingDequeue` | As TC14 but `QfSettings` returns `null`. | 0 | 279 cond 1 |
| TC16 | `QfcExplorerControllerLoaderDefault_ConstructsAnExplorerControllerFromGlobals` | **A:** the fixture's existing globals mock (`Ol.App.ActiveExplorer()` already stubbed). **Act:** `_controller.QfcExplorerControllerLoader(QfEnums.InitTypeEnum.Sort, _mockApplicationGlobals.Object, _controller)`. **Assert:** result `.Should().NotBeNull()` and `.Should().BeAssignableTo<IQfcExplorerController>()`. | 182 (1) | — |
| TC17 | `HighConfidencePreFilterLoaderDefault_WithEmptyBatch_ReturnsEmptyWithoutScoring` | **A:** none beyond the fixture. **Act:** `await _controller.HighConfidencePreFilterLoader(new List<MailItem>(), globals, 0.90, CancellationToken.None)`. **Assert:** result `.Should().BeEmpty()`. *(Distinct from the existing `HighConfidencePreFilterLoader_CanBeOverridden_ForTesting`, which never touches the default.)* | 244 (1) | — |
| TC18 | `QfcFormControllerLoaderDefault_ConstructsAndInitializesAgainstAMockViewer` | **A:** loose `Mock<IApplicationGlobals>` (so `AF.MaximizeQuickFileWindow` and `AF.MovedMails` are settable/null-returning); loose `Mock<IQfcFormViewer>` returning `null` for `L1v0L2L3v_TableLayout`, `Panels`, `Buttons`, `Controls`; loose `Mock<IQfcQueue>`; a controller whose `KeyboardHandler` is left `null`. **Act:** invoke the default `QfcFormControllerLoader` delegate. **Assert:** result `.Should().NotBeNull()`; no exception. **Risk:** depends on four early-return guards inside sibling F6's `QfcFormController.SetupDisposal.cs`; treat as *buffer*, and if F6 changes those guards mid-wave, drop this task rather than editing F6. | 220-229 (10) | — |

**Tier A subtotal: +19 lines** (or +29 with TC18) → 216/276 = **78.3%** (226/276 = **81.9%** with TC18).
Tier A alone does not reliably clear 80%. Tier B is required.

### Tier B — required, each unlocked by exactly one seam

| TC | `[TestMethod]` name | Seam | Arrange / Act / Assert sketch | Lines gained |
| --- | --- | --- | --- | --- |
| TC5 | `WorkerRunWorkerCompleted_WhenErrorPresent_ReportsTheFormattedErrorThroughTheNotificationSeam` | **S1** | **A:** `string captured = null; _controller.ShowUserMessage = m => captured = m;` plus a loose viewer mock. **Act:** reflection-invoke the handler with `new RunWorkerCompletedEventArgs(null, new InvalidOperationException("boom"), false)`. **Assert:** `captured.Should().Be("An error occurred: boom")`; viewer flags still `false`; `WorkerComplete.Should().BeFalse()`. | 335, 337-339 (4) |
| TC6 | `ShowUserMessageSeam_DefaultsToANonNullNotifier` | **S1** | **A:** fresh controller. **Assert:** `_controller.ShowUserMessage.Should().NotBeNull()`. Guards against a null-seam regression without ever invoking the `MessageBox` default. | 0 |
| TC7 | `TimedConsumerAsync_WhenMetricsPendingAndMyDocumentsResolves_WritesTheSessionFileThroughTheSeam` | **S2** | **A:** loose globals with `FS.SpecialFolders["MyDocuments"] = @"C:\Fake"` and `FS.Filenames.EmailSession = "session.csv"`; capture the seam's four arguments; reflection-get `_metrics`, `Add("line-1")`, `Add("line-2")`, `CompleteAdding()`. **Act:** reflection-invoke `TimedConsumerAsync(null, null)`. **Assert:** seam invoked once with filename `"session.csv"`, payload `["line-1","line-2"]`, folder `@"C:\Fake"`. **No real file is written.** | 363, 365-380, 386 (18) |
| TC8 | `TimedConsumerAsync_WhenNoMetricsArePending_DoesNotInvokeTheWriterSeam` | **S2** | **A:** as TC7 but `CompleteAdding()` with nothing added. **Act:** invoke. **Assert:** seam never invoked. | 0 (368 false arm) |
| TC9 | `TimedConsumerAsync_WhenMyDocumentsIsNotResolvable_DoesNotInvokeTheWriterSeam` | **S2** | **A:** as TC7 but an empty `SpecialFolders` dictionary. **Act:** invoke. **Assert:** seam never invoked. | 0 (370 false arm) |
| TC10 | `TimedConsumerAsync_WhenTheWriterSeamThrows_PropagatesTheException` | **S2** | **A:** as TC7 but the seam returns a faulted task / throws synchronously. **Act:** reflection-invoke. **Assert:** a `TargetInvocationException` whose `InnerException` is the seeded exception (fail-fast per §3 of the general policy). Deterministic because the seam completes synchronously — no true async suspension occurs, so the `async void` never escapes to a captured context. | 381-384 (4) |
| TC11 | `TimedConsumerAsync_DecrementsTheMetricsConsumerCount` | **S2** | **A:** as TC7, reflection-read `_metricsConsumers` before. **Act:** invoke. **Assert:** value decreased by exactly 1. *(Asserts the `Interlocked` side effect on line 366; no extra line gain — optional.)* | 0 |

**Tier A + Tier B: +19 (+10 with TC18) + 26 = 45 (55) lines** → 242/276 = **87.7%**
(252/276 = **91.3%** with TC18). Both clear 80% with margin.

### Tier C — optional stretch, closes `LaunchAsync`

Only required if F1's ledger **declines** to record lines 38-87 as irreducible host wiring. Requires
seams S4, S5a, S5b.

| TC | `[TestMethod]` name | Sketch | Lines gained |
| --- | --- | --- | --- |
| TC19 | `LaunchCoreAsync_HappyPath_InitializesSetsLoadedAndRunsInOrder` | Controller with all seven loaders replaced by mocks + `QfcFormViewerLoader` returning a mock viewer + `UiSchedulerLoader` returning `TaskScheduler.Default`; mocked `ProgressTracker`. Assert ordered `InitAsync` → `Loaded == true` → `RunAsync`, and the returned instance is the same controller. | ~14 |
| TC20 | `LaunchCoreAsync_WhenInitializationIsCancelled_ReportsOneHundredAndReturnsNull` | `QfcAsyncDataModelLoader` throws `OperationCanceledException`. Assert `progress.Report(100)` once and the result is `null`. | ~6 |
| TC21 | `LaunchCoreAsync_WhenRunIsCancelled_ReportsOneHundredAndReturnsNull` | Data model's `InitEmailQueueAsync` throws `OperationCanceledException`. Same assertions; distinguishes the two cancellation entry points. | ~2 |
| TC22 | `LaunchCoreAsync_SpawnsAnEightySixPercentInitializationChildAndADefaultRunChild` | Mocked tracker: assert `SpawnChild(86)` once and `SpawnChild()` once — pins the progress allocation contract. | 0 |
| TC23 | `LaunchAsync_WhenNoSynchronizationContextIsInstalled_InstallsAWindowsFormsContext` | *Not recommended.* Mutates thread-static state and still reaches `ProgressTracker.Initialize()`. Record as the residual irreducible remainder (≈8 lines) instead. | 0 |

**Tier C leaves ≈8 uncovered lines** in the thin `LaunchAsync` wrapper (sync-context install,
`new ProgressTracker(...).Initialize()`, the two `new` statements) — the honest irreducible remainder
under the epic's Shared Design §1 standard.

### Explicitly NOT proposed

- Any test asserting the two-argument `DequeueNextItemGroupAsync(itemsPerIteration, 1000)` at the
  pre-UI call site — retired by #424 (see §7).
- Any test re-asserting the 0→30 progress band, the empty-first-batch path, or the streaming-dequeue
  first page — all already pinned by #424's suites (see §7).
- Any test of `progress?.Report(100)`'s null arm (line 312) — provably dead.
- Any test that constructs or shows `QfcFormViewer`, `ProgressViewer`, or a `MessageBox`.
- Any `[STATestMethod]` — the epic's STA last-resort clause (Shared Design §3) is **not** needed for
  this file; every remaining gap is reachable by seam or reflection.

---

## 5. Required seams (minimum set, ranked by the hierarchy)

Ranking follows `.claude/rules/csharp.md:49-53` — interface seam > injectable delegate > adapter.

### S1 — user-notification seam for `Worker_RunWorkerCompleted` (line 338) — **required**

- **Preferred form under the hierarchy:** a narrow interface, e.g. `IQfcUserNotification { void Show(string message); }`.
- **Recommended form:** an injectable delegate.
  ```
  internal Action<string> ShowUserMessage { get; set; } = msg => MessageBox.Show(msg);
  ```
- **Why the interface is not used here:** the rule itself carves this out — "use a narrow
  `Func<>`/`Action<>` delegate for a single call path when a full interface is excessive"
  (`csharp.md:52`). This is exactly one call path with one `string` parameter and no expectation of
  multiple implementations. The file already exposes seven `Func<>` loader seams, so the delegate is
  the locally consistent choice. `Tags/IUserPrompt.cs` exists but **QuickFiler does not reference the
  Tags project** (verified against `QuickFiler/QuickFiler.csproj:464-479` — only SVGControl,
  TaskVisualization, ToDoModel, UtilitiesCS), so there is no interface to reuse; introducing one would
  add a new compiled file to the epic denominator for a single void method.
- **Why a lower-cost option does not suffice:** none exists. `MessageBox.Show` is static, modal, and
  blocks on human interaction; without a seam the 4-line branch is permanently untestable.
- **Formatting note:** if the initializer fits on one physical line (it does at CSharpier's default
  width), the default lambda body shares that line and is therefore counted as covered when any ctor
  runs. Do not *rely* on that; if CSharpier wraps it, one additional uncovered line results.

### S2 — metrics file-writer seam for `TimedConsumerAsync` (lines 372-377) — **required**

- **Recommended form:** injectable delegate matching `FileIO2.WriteTextFileAsync`'s signature.
  ```
  internal Func<string, string[], string, CancellationToken, Task> MetricsFileWriter { get; set; } =
      FileIO2.WriteTextFileAsync;
  ```
  (`UtilitiesCS/To Depricate/FileIO2.cs:50-55` — `(string filename, string[] strOutput, string folderpath, CancellationToken token)`.)
- **Why not an interface:** an `IFileWriter` interface would be the hierarchy-preferred form, but
  `FileIO2` is a static utility in `UtilitiesCS` and is **not** F7-owned; introducing an interface
  there would widen the change into another project. The delegate keeps the change inside
  `QfcHomeController` and uses a method group as its default, so there is no default lambda body to
  leave uncovered.
- **Ratified in-repo precedent (strongest argument for this exact shape):**
  `QuickFiler/Controllers/EfcHomeControllerDependencies.cs:78` already does
  `MetricsLineWriter = metricsLineWriter ?? FileIO2.WriteTextFile;` with an
  `Action<string, string[], string>` shape, and it is test-pinned at
  `QuickFiler.Test/Controllers/EfcHomeControllerDependenciesTests.cs:33` (default non-null) and
  `:114-116` (injected delegate receives filename/payload/root). S2 is the async-overload twin of a
  seam this repository has already accepted and shipped. Note that `EfcHomeControllerDependencies.cs`
  is **sibling F8-owned** — declare the Qfc seam locally in `QfcHomeController.cs`; do not reuse or
  extend the Efc dependencies type.
- **Why a lower-cost option does not suffice:** the alternative is writing a real file, which the
  general unit-test policy prohibits outright ("Creation and use of temporary files in tests is
  strictly prohibited"). Redirecting to a memory stream is not possible through `FileIO2`'s API.
- **Blocked-on note:** the same tests must call `CompleteAdding()` on the reflection-obtained
  `_metrics` field, otherwise `GetConsumingEnumerable().ToArray()` (line 367) blocks forever. That is
  a property of the production code, not of the seam — see §9 R3.

### S3 — `IUiDispatcher` for line 343 — **recommended, not required**

- `UtilitiesCS/Threading/IUiDispatcher.cs` already exists with a production implementation
  `WpfUiDispatcher` that forwards 1:1 to `UiThread.Dispatcher`. Injecting it
  (`internal IUiDispatcher UiDispatcher { get; set; } = new WpfUiDispatcher();`) is a genuine
  **interface seam** — the top of the hierarchy — and it removes the existing test's dependency on the
  process-global `UiThread.Init(false)` (`QfcHomeControllerRunAsyncTests.cs:329`), a UT4
  mutable-global-state concern.
- **Coverage gain: zero** (lines 343-347 are already hit). It is a policy-quality improvement only,
  and it requires editing the existing `Worker_RunWorkerCompleted_HandlesCompletionCorrectly` test.
  Recommend adopting it **only if** the line budget after the partial split is comfortable.

### S4 — `LaunchCoreAsync` extraction — **required only for Tier C**

Move the host-neutral orchestration out of the static host-bound entry point, per
`.claude/rules/general-unit-test.md`'s Coverage Exclusion Policy ("extract all logic into
host-neutral, testable modules and leave only the thinnest possible wiring in the host-bound entry
point"):

```
public static async Task<QfcHomeController> LaunchAsync(
    IApplicationGlobals appGlobals, System.Action parentCleanup, TimeProvider timeProvider = null)
{
    if (SynchronizationContext.Current is null)
        SynchronizationContext.SetSynchronizationContext(new WindowsFormsSynchronizationContext());
    var controller = new QfcHomeController();
    controller.TimeProvider = timeProvider ?? TimeProvider.System;
    var tokenSource = new CancellationTokenSource();
    var progress = new ProgressTracker(tokenSource).Initialize();
    return await controller.LaunchCoreAsync(appGlobals, parentCleanup, tokenSource, progress);
}

internal async Task<QfcHomeController> LaunchCoreAsync(
    IApplicationGlobals appGlobals, System.Action parentCleanup,
    CancellationTokenSource tokenSource, ProgressTracker progress) { /* try/catch body, lines 61-86 */ }
```

- Behaviour-preserving: the original assigns `controller = null` in the catch and returns it; the
  extraction returns `null` from the catch and `this` otherwise — identical observable result.
- **Why no lower-cost seam suffices:** the controller is constructed *inside* the static method, so
  no instance seam can be pre-assigned by a test, and `ProgressTracker.Initialize()` shows a form.
  A static factory-delegate field would work but introduces process-global mutable state shared
  across tests — worse than the extraction on both the isolation and the simplicity axes.

### S5a / S5b — form-viewer and UI-scheduler seams — **required only for Tier C, and only as a pair**

```
internal Func<IQfcFormViewer> QfcFormViewerLoader { get; set; } = () => new QfcFormViewer();
internal Func<TaskScheduler> UiSchedulerLoader { get; set; } =
    () => TaskScheduler.FromCurrentSynchronizationContext();
```

- **They must be adopted together.** Line 136 (`TaskScheduler.FromCurrentSynchronizationContext()`)
  succeeds today only because line 133 constructs a WinForms `Form`, which auto-installs a
  `WindowsFormsSynchronizationContext` on the calling thread. Injecting a mock viewer without also
  seaming line 136 turns `InitAsync_InitializesCorrectly` from green to `InvalidOperationException`.
- **Side benefit:** adopting S5a/S5b lets `Init_InitializesCorrectly` and
  `InitAsync_InitializesCorrectly` be updated to stop constructing a live WinForms form — resolving a
  pre-existing violation of the epic's Shared Design §2 that this file's test suite carries today.
  Note that this is an **update** to two existing tests, not a duplicate.
- The alternative — setting `SynchronizationContext.SetSynchronizationContext(...)` in test Arrange —
  is rejected: it mutates thread-static state that MSTest reuses across tests in the same thread,
  violating the independence/isolation principles.

---

## 6. Cross-child contract notes

No edit to any sibling-owned file is proposed. The following are recorded as contract notes only.

| # | Type / file | Owning child | Note |
| --- | --- | --- | --- |
| CC-1 | `QfcDatamodel` / `QfcDatamodel.LoadAsync` (`QuickFiler/Controllers/QfcDatamodel.cs`) | **F5** | Line 173 (the `QfcAsyncDataModelLoader` default body) can only be covered if `QfcDatamodel.LoadAsync(IApplicationGlobals, CancellationToken, CancellationTokenSource, ProgressTracker)` becomes reachable without live Outlook COM. **No addition requested** — F7 accepts this 1 line as a residual and does not depend on F5. If F5 introduces such a seam independently, F7 can add a one-line test later. |
| CC-2 | `QfcFormController` + `QfcFormController.SetupDisposal.cs` | **F6** | TC18 covers lines 220-229 by invoking the default `QfcFormControllerLoader`, which relies on four existing null-guard early returns in F6's code (`SetupDisposal.cs:24-30`, `50-57`, `77-80`, `151-154`). **No addition requested**; F7 requests only that these guards not be removed. If F6 removes or narrows them mid-wave, F7 drops TC18 (costing 10 lines of buffer, not the 80% target). |
| CC-3 | `QfcExplorerController` (`QuickFiler/Controllers/QfcExplorerController.cs`) | **F6** | TC16 constructs it through the default loader. Its ctor (`:27-37`) reads only `_globals.Ol.App.ActiveExplorer()`. **No addition requested.** F6 owns the disposition of its `[ExcludeFromCodeCoverage]` attribute (line 20); F7 takes no position. |
| CC-4 | `IQfcQueue` / `QfcQueue` | **F2** | Only consumed through the already-injectable `QfcQueueLoader` and the `QfcQueue` internal setter. **No addition requested.** |
| CC-5 | `KeyboardHandler` | **F3** | Only consumed through `QfcKeyboardHandlerLoader`. **No addition requested.** |
| CC-6 | `IQfcCollectionController` | **F11** | Reached only indirectly via `_formController.Groups` in `Iteration.cs` / `Metrics.cs`, not from this file. **No addition requested.** |
| CC-7 | `QfcFormViewer` (`QuickFiler/Viewers/QfcFormViewer.cs`) | **F15** | S5a adds `QfcFormViewerLoader` **inside `QfcHomeController.cs`**; `QfcFormViewer.cs` is not edited. Its `[ExcludeFromCodeCoverage]` disposition remains F15's. |
| CC-8 | `coverage.config` | shared | **Not edited.** Verified it excludes only third-party module paths (Deedle, FSharp, Castle.Core, FluentAssertions, Moq, MSTest, Microsoft.Testing) — QuickFiler is instrumented, so no change is needed for this child. |
| CC-9 | `QuickFiler/QuickFiler.csproj` | shared-ish | **Not on the sibling-owned list**, but any new partial file requires a `<Compile Include="Controllers\QfcHomeController.*.cs" />` entry near lines 325-327. F9, F11 and F13 are also expected to add `<Compile>` entries during wave 1, so this ItemGroup is a **merge-conflict hotspot**. Mitigation: insert new entries alphabetically adjacent to the three existing `QfcHomeController*` lines so the conflict region is minimal and mechanically resolvable. |

---

## 7. Issue #424 interaction findings

**Status: #424 has landed.** Its feature folder is still under `docs/features/active/`, but its
spec records 13/13 acceptance criteria delivered
(`.../2026-08-06-quickfiler-high-confidence-queue-init-stall-424/spec.md:263-273`) and the
post-change code is present in the target file at base commit `74be1964`
(`QfcHomeController.cs:292-305` uses the four-argument dequeue overload,
`QfcScanProgressBandMapper`, the 200 ms poll, and `QfcStreamingDequeueConfidenceGate.DefaultFirstBatchDeadline`).
There is therefore **no live edit conflict** — only a set of pins F7 must respect.

### (a) Members of `QfcHomeController.cs` that #424 touched

Exactly one: **`RunAsync(ProgressTracker)`**, lines 292-305 — per the spec's own change table
(`spec.md:112`, "Wire deadline + progress mapping into RunAsync (0→30 band); (O1) poll argument at
line 294"). Concretely:

- lines 294-297: the explanatory comment block;
- line 298: `var scanProgress = new QfcScanProgressBandMapper(progress.Report);`
- lines 299-304: `DequeueNextItemGroupAsync(itemsPerIteration, 200, QfcStreamingDequeueConfidenceGate.DefaultFirstBatchDeadline, scanProgress.Report)` — replacing the previous `(itemsPerIteration, 1000)` two-argument call.

Nothing else in the file was modified. `Run()` (248-272) was **deliberately** left on the legacy
two-argument overload (`spec.md:60`, legacy synchronous paths listed as out of scope), and
`Iteration.cs:21-24` was left at `(ItemsPerIteration, 2000)`.

### (b) #424 regression tests that already pin behaviour F7 would otherwise test

| Test | File | Pins |
| --- | --- | --- |
| `RunAsync_HighConfidenceEnabled_LoadsFirstPageFromStreamingDequeue` | `QfcHomeControllerRunAsyncHighConfidenceTests.cs:112-210` | Exact argument tuple `(itemsPerIteration, 200, DefaultFirstBatchDeadline, non-null sink)`; `InitEmailQueueAsync(0, ...)` exactly once; the streamed batch reaches `LoadItemsAsync`; the unfiltered batch does not. |
| `RunAsync_HighConfidenceScanProgress_MapsReportsIntoTheZeroToThirtyBand` | same file, 289-388 | Every report between `"Initializing Email Queue"` (value 0) and `"Initializing Qfc Items"` (value 30) lies in [0, 30], is monotonically non-decreasing, and is labelled `"Scanning for high-confidence items…"`. |
| `RunAsync_HighConfidenceEmptyBatch_StillLoadsItemsAndStartsIteration` | same file, 396-471 | An empty deadline result still reaches `LoadItemsAsync`, and `IterateQueueAsync` is still initiated (`DataModel.Complete` read at least once). |
| `RunAsync_HighConfidenceDisabled_DoesNotPreFilterUsesPlainOverload` / `..._UsesPlainOverloadOnly` | same file, 217-280 | Disabled mode never invokes the pre-filter and uses only the `IList<MailItem>` overload. |
| `RunAsync_HighConfidenceEnabled_DoesNotPreFilterInitialGuiBatch` / `RunAsync_HighConfidence_LoadsInitialBatchWithoutPreFilter` | `QfcHomeControllerIssue218Tests.cs:137-259` | Same intent at the four-argument overload shape (updated by #424's "overload shape only" correction, `spec.md:185`). |
| `IterateQueueAsync_WhenDequeueReturnsFullQualifiedPage_EnqueuesAllItems` | `QfcHomeControllerIterationTests.cs:259-310` | Exact-argument pin `DequeueNextItemGroupAsync(8, 2000)` at the **post-UI** call site — `spec.md:183` marks this file "Unchanged". |
| `InitEmailQueue_ZeroBatchSize_*` | `QfcInitEmailQueueZeroBatchTests.cs` | Issue #244 zero-batch behaviour in `QfcDatamodel` (F5 territory), reached only indirectly. `spec.md:184` marks it "Unchanged". |

Net effect on F7: **`RunAsync`'s line coverage is already 100%.** F7 has nothing to add there except
the two null-guard **branch** cases (TC14, TC15), which no #424 test touches.

### (c) Tests F7 must NOT write

1. **Do not** assert the two-argument `DequeueNextItemGroupAsync(itemsPerIteration, 1000)` at the
   pre-UI site in `RunAsync`. It was deliberately retired; such a test contradicts
   `RunAsync_HighConfidenceEnabled_LoadsFirstPageFromStreamingDequeue`.
2. **Do not** "harmonize" the poll interval — do not assert `200` at the post-UI site
   (`Iteration.cs:21-24`) or `2000` at the pre-UI site. #424 AC 12 requires
   `QfcHomeControllerIterationTests.cs`'s `(8, 2000)` pin to remain byte-unmodified and passing.
3. **Do not** change or duplicate the 0→30 band assertions. Any new progress test would either
   duplicate `RunAsync_HighConfidenceScanProgress_MapsReportsIntoTheZeroToThirtyBand` (a defect per
   the delegation brief) or contradict it.
4. **Do not** assert that `Run()` (the legacy synchronous path, line 261) uses the deadline/progress
   overload. #424 explicitly left `Run()`/`Iterate()` out of scope (`spec.md:60`); asserting
   otherwise would force a production change F7 has no mandate for.
5. **Do not** assert `HighConfidencePreFilterLoader` is invoked from `RunAsync`. Issue #218 (pinned in
   `QfcHomeControllerIssue218Tests.cs`) established that dequeue-time admission owns high-confidence
   filtering and the pre-filter must **not** run in `RunAsync`. TC17 tests the *delegate in isolation*,
   never through `RunAsync` — that distinction is essential.
6. **Do not** modify `QfcInitEmailQueueZeroBatchTests.cs`, `QfcHighConfidencePreFilterTests.cs`, or
   `QfcFormControllerTests.cs` — all three are byte-unmodified pins under #424 AC 12.

---

## 8. File-size / partial-split analysis and line budget

### 8.1 Current state

`QfcHomeController.cs` = **487 physical lines** against the 500-line hard limit
(`.claude/rules/general-code-change.md`, "File Size Limit"). **Headroom: 13 lines.**

### 8.2 Estimated production growth

| Change | Est. physical lines |
| --- | --- |
| S1 `ShowUserMessage` delegate + XML doc + `MessageBox` call-site edit | +6 |
| S2 `MetricsFileWriter` delegate + XML doc + call-site edit (372-377 → one call) | +9 net |
| S3 `IUiDispatcher` property + doc + call-site edit (optional) | +6 |
| S4 `LaunchCoreAsync` extraction (Tier C only) | +15 net |
| S5a `QfcFormViewerLoader` + doc (Tier C only) | +7 |
| S5b `UiSchedulerLoader` + doc (Tier C only) | +7 |

- **Tier A + Tier B only (S1 + S2): +15 → 502 lines. The limit is breached. A partial split is
  MANDATORY even for the minimum recommendation.**
- Tier A + B + C (S1, S2, S4, S5a, S5b): +44 → 531 lines.
- All seams including S3: +50 → 537 lines.

### 8.3 Recommended split

**Split 1 (mandatory) — `QuickFiler/Controllers/QfcHomeController.Properties.cs`**

Move the entire `#region Public Properties` block, source lines **406-485** (80 physical lines): the
backing fields and accessors for `ExplorerController`, `FormController`, `KeyboardHandler`,
`DataModel`, `FilerQueue`, `UiScheduler`, `StopWatch`, `_formViewer`, `CreateCancellationToken()`,
`TokenSource`, `Token`, `WorkerComplete`, `UiSyncContext`.

- **Why this block:** it is a pure mechanical move of a single `#region` with no cross-references
  outside the type; the existing reflection-based tests resolve fields via `_controller.GetType()`,
  which is unaffected by which partial file declares them.
- **Coverage arithmetic for the new file:** 22 coverable lines, of which 18 are covered today; TC2
  covers the remaining 4 (`CreateCancellationToken`) → **100%**. It clears 80% on its own.
- **Coverage arithmetic for the main file after the move:** denominator ≈276 − 22 = **254**;
  covered 197 − 18 = 179 (70.5% before new tests). After Tier A+B (excluding the 4
  `CreateCancellationToken` lines that moved out): 179 + 41 = **220/254 = 86.6%**; with TC18,
  230/254 = **90.6%**. Clears 80% with margin.
- **Line budget:** main file 487 − 80 = 407, + 15 (S1, S2) = **422 / 500 → 78 lines of headroom**.
  New file ≈95 physical lines.
- Keep `[assembly: InternalsVisibleTo("QuickFiler.Test")]` (line 18) in `QfcHomeController.cs`.

**Split 2 (only if Tier C is adopted) — `QuickFiler/Controllers/QfcHomeController.Lifecycle.cs`**

Move source lines **38-153** (`LaunchAsync`, `Init`, `InitAsync` — 116 physical lines) plus the new
`LaunchCoreAsync` and the S5a/S5b seams.

- Main file: 487 − 80 − 116 = 291, + 6 (S1) + 9 (S2) = **306 / 500**.
- Lifecycle file: ≈116 + 15 (S4) + 14 (S5a/S5b) + file scaffolding ≈ **155 / 500**.
- Lifecycle-file coverage after Tier C: `Init` and `InitAsync` are already 100%; `LaunchCoreAsync`
  reaches ~90%; the `LaunchAsync` wrapper leaves ≈8 uncovered → roughly **60 of 68 coverable lines,
  ≈88%**. Clears 80%.
- Main-file coverage after both splits: denominator ≈254 − 68 = 186, covered ≈179 (all remaining
  gaps having been closed by Tier A+B except the F5/F6 lambda bodies) → ≈**94%**.

**Alternative to Split 1 — relocate the metrics block into the existing Metrics partial.** Lines
**353-386** (the `_metrics` / `_metricsConsumers` / `_lockObject` / `_fileName` fields plus
`TimedConsumerAsync`, 34 physical lines) are metrics-only members mis-located in the main partial;
their only writer is `NonBlockingProducer` in `QfcHomeController.Metrics.cs`. Moving them there is
more cohesive than Split 1 and frees 34 lines instead of 80.

- **Budget:** main file 487 − 34 = 453, + 15 (S1, S2) = **468 / 500 → 32 lines of headroom.** Tighter
  than Split 1 (78 lines) and it does not accommodate Tier C.
- **Coverage side effect (must be checked):** this moves 22 *uncovered* lines into
  `QfcHomeController.Metrics.cs`, which is itself only at an indicative 65.09%. It is safe **only if**
  TC7-TC10 land in the same change, in which case the block arrives ~91% covered and lifts the Metrics
  partial rather than depressing it. Sequencing matters: a plan that relocates first and tests later
  produces a transient sub-80% Metrics file.
- **Coordination:** `QfcHomeController.Metrics.cs` is a *sibling file inside the same child F7*, so
  this is not a cross-child conflict — but it does couple this file's plan to the Metrics file's plan.
  See the companion F7 research artifact for `QfcHomeController.Metrics.cs` (234 lines, ~266 lines of
  headroom) before choosing.
- **Recommendation:** prefer **Split 1** if Tier C is in play or the ledger is undecided (78 lines of
  headroom absorbs S4/S5a/S5b later without a second split); prefer the relocation if the plan is
  firmly capped at Tier A+B and the two F7 plans are sequenced together.

**Not recommended:** a `QfcHomeController.Seams.cs` holding lines 159-245. Measured against the
indicative hit map, that block is 16 covered / 13 uncovered = **55%** — the new file would fail the
epic's own 80% bar on creation. Splitting must not be chosen on cohesion grounds alone; the per-file
coverage of *both* halves must be checked first.

### 8.4 Budget summary

| Scenario | Main file lines | Main file est. coverage | Second file | Third file |
| --- | --- | --- | --- | --- |
| No split, Tier A+B | **502 — VIOLATION** | 87.7% | — | — |
| Split 1, Tier A+B **(recommended minimum)** | 422 | ≈86.6% (90.6% w/ TC18) | Properties ≈95 lines, 100% | — |
| Split 1+2, Tier A+B+C | 306 | ≈94% | Properties ≈95 lines, 100% | Lifecycle ≈155 lines, ≈88% |

---

## 9. Risks and open questions

### Risks

- **R1 — F1 has not landed.** The `testable` classification, the exemption disposition for
  `LaunchAsync`'s residual ≈8 lines, and the harness that produces acceptance evidence are all
  upstream. If F1 classifies this file `ratified-exempt`, this artifact is void. If F1's conventions
  mandate interface-first with no delegate carve-out, S1 and S2 must be re-formed as interfaces
  (adding two files to the denominator). **Gate the plan's Phase 0 on reading the ledger.**
- **R2 — the 500-line limit is breached by the minimum recommendation.** Split 1 is not optional. Any
  plan that adds S1 and S2 without it produces a 502-line file and a Blocking finding.
- **R3 — latent production defect: the metrics pipeline is inert.** `TimedConsumerAsync` is
  unreachable in production, on two independent grounds:
  1. `_metricsConsumers` is initialized to `0` (line 356) and is only ever *decremented* (line 366 and
     `Metrics.cs:228`). The guard `Interlocked.CompareExchange(ref _metricsConsumers, 0, 2) == 2`
     (`Metrics.cs:226`) can therefore never be true, so the timer at `Metrics.cs:229-230` is never
     created.
  2. Even if it were, `Metrics.cs:229-230` constructs a `System.Timers.Timer(2000)` and subscribes
     `TimedConsumerAsync` but **never calls `Start()`**, and the local goes out of scope immediately.

  Consequence: `WriteMetricsAsync` (`Metrics.cs:90-155`) pushes every diagnostic line into `_metrics`
  via `NonBlockingProducer` and nothing ever drains it — the async metrics file is never written, and
  `_fileName` (line 358) is assigned at `Metrics.cs:153` and never read. Additionally,
  `_metrics.GetConsumingEnumerable().ToArray()` (line 367) would block indefinitely if the handler
  *were* invoked, because `CompleteAdding()` is never called on `_metrics`.
  **Recommended disposition:** do **not** fix in F7 (a coverage child is not the vehicle for a
  behaviour change). Promote to its own GitHub issue via the MCP promotion lifecycle and cover the
  method as dead-but-testable code per TC7-TC10. Note that TC7-TC10 will document, in test comments,
  that the method is currently unreachable in production.
- **R4 — the existing test suite constructs a live WinForms form.** `Init_InitializesCorrectly` and
  `InitAsync_InitializesCorrectly` both execute `new QfcFormViewer()` (lines 93 and 133). This is a
  pre-existing violation of the epic's Shared Design §2. S5a/S5b fix it, but only under Tier C. If
  the plan stops at Tier A+B, F7 should record this as a known, unresolved policy debt rather than
  silently leaving it. Feature-review may flag it either way.
- **R5 — `QuickFiler.csproj` `<Compile>` merge conflict.** F9, F11 and F13 are all expected to add
  partial files during wave 1. Adding one or two entries adjacent to lines 325-327 keeps the conflict
  region small but does not eliminate it.
- **R6 — TC18 depends on sibling F6 internals.** It is buffer, not baseline. The 80% target must hold
  without it (it does: 86.6% with Split 1).
- **R7 — reflection-heavy tests.** Six of the proposed cases invoke private members or read private
  fields by reflection. This matches the file's established suite convention (all eight existing
  suites do it), but it couples the tests to member names. Any rename during a future refactor breaks
  them silently at run time, not compile time. Accepted as consistent with existing practice.
- **R8 — the indicative coverage figures are one revision old in spirit.** They were produced on the
  #424 branch, not by F1's harness, and the arithmetic in §3.1 and §8.3 derives the denominator
  (≈276) from the reported `line-rate` and a hand-count of 79 uncovered lines; it may be off by one.
  All percentages in this artifact are planning estimates. F1's harness output is authoritative.

### Open questions for the planner / F1

1. Does F1's ledger classify `QfcHomeController.cs` as `testable`? (Assumed yes.)
2. Does F1's ledger accept the residual `LaunchAsync` wrapper (≈8 lines after S4, or the full 34 lines
   without S4) as irreducible host wiring, or does it require the Tier C extraction? This single
   answer determines whether the child is a ~2-day Tier A+B change or a ~4-day Tier A+B+C change.
3. Does F1's shared seam convention permit the `csharp.md:52` delegate carve-out for S1 and S2, or
   does it mandate interface seams uniformly across the epic?
4. Should R3 (the inert metrics pipeline) be promoted as its own issue before or after F7 executes?
   Covering dead code to satisfy a coverage target is defensible only if the defect is on record.
5. Is the S3 `IUiDispatcher` adoption in scope for F7, given it yields zero coverage gain but removes
   a `UiThread.Init(false)` global-state dependency from an existing test?

### Recommended scope for the atomic plan

**Adopt Split 1 + Tier A (TC1, TC2, TC4, TC12-TC17) + Tier B (TC5-TC10) + seams S1 and S2.**
That is 15 required test tasks, 2 seam tasks, 1 split task, and 1 csproj task — projecting
**≈86.6% line coverage** on `QfcHomeController.cs` and **100%** on the new
`QfcHomeController.Properties.cs`, with the file at 422/500 lines. Treat TC3, TC11, TC18 as buffer and
Tier C (S4/S5a/S5b, TC19-TC22) as a conditional phase gated on open question 2.
