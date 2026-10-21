# Research refresh — TransactionGate bounded acquisition (Issue #882)

- **Issue:** #882
- **Feature folder:** `docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/`
- **Branch:** `bug/quickfiler-transactiongate-permit-leak-unexcluded-882`, merged with `origin/main` (238 commits since the 2026-09-13 base `e6d86049e`)
- **Timestamp:** 2026-09-28T00-10
- **Supersedes for line references:** `research/2026-09-13T19-00-transactiongate-bounded-acquisition-research.md` (the "old research"). The old research's reasoning in its §1 (MSTest abandonment semantics), §2 (overload semantics, control-flow invariant) and §3a (determinism ruling) is unchanged by anything found here and is not restated; only its citations and its parallelism guidance are refreshed.
- **Scope:** research only. Read, Grep and Glob were the only tools used. No build, test or git command was run. No source file was modified.

> **Timestamp provenance.** No clock-reading tool was available; the timestamp is the one supplied in the delegation prompt.

---

## 0. Summary of material changes since 2026-09-13

1. **Issue #743 merged into this branch.** `QfcItemController.UiThreadDispatcherFixture.cs` grew from 278 to 304 lines. It now carries three `Interlocked` counters (`_transactionAcquisitions`, `_transactionReleases`, `_contendedAcquisitions`, lines 41–43, exposed at 46, 49, 54), a contended-acquisition pre-check inside `BeginTransactionAsync` (lines 144–147), an acquisitions increment after the wait (line 150), and a releases increment inside `ReleaseTransactionGate` (line 109). **The acquisition itself is still the parameterless, unbounded `TransactionGate.WaitAsync()` at line 149.** No bounded overload was delivered (§8).
2. **A seventh test exists in the fixture test file.** `TransactionGate_WhileThisTestHoldsATransaction_HasExactlyOneUnreleasedAcquisition` (lines 355–394) asserts `acquisitions - releases == 1` while holding the permit and writes a `GATECOUNTERS` line to `TestContext`. The file grew from 353 to 396 lines and now has a `TestContext` property (line 35).
3. **Acquisition inventory is now fifteen statements across five files** (was fourteen across five); the fifteenth is the new #743 test (§2, with derivation evidence).
4. **MSTest is 4.4.1, not 4.4.0** (`QuickFiler.Test/packages.config` lines 43–45; `QuickFiler.Test.csproj` lines 3, 393–398, 547–549, 566–567). This is a patch bump within the `mstest-net-4.4` documentation moniker the old research cited, so §1 of the old research is unaffected.
5. **`QuickFiler.Test.csproj` has 185 `<Compile Include=` items** (was 173); still no globbing and no `EnableDefaultCompileItems`. AC7's premise (a new file requires a project-file edit) still holds.
6. **The spec's `[DoNotParallelize]` guard must be withdrawn.** The operator now requires the parallel regime (`TaskMaster.runsettings` Workers 0 / Scope ClassLevel) to remain in force for the new test. §5 shows the C1 construction is parallel-safe without it, provided one rule is followed: zero-bound probes assert only failure, and only while this test holds the permit; success is asserted only through the production entry point.
7. **A correction to the old research §2.2 / spec AC5 reasoning about `SemaphoreFullException`.** A wrongly-released permit on the failure branch does not throw at the point of the wrong release while the probing test still holds the permit (count 0 → 1 succeeds silently); it throws later, at the legitimate holder's `Dispose`. The companion assertion in §5 is placed on that `Dispose` accordingly.

---

## 1. Citation re-verification

Every file, line and identifier citation in the old research and in `spec.md` was re-read against the current tree. `FX` = `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs`; `FT` = `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs`.

### 1.1 Fixture file (`FX`)

| Citation as written | Current location / value | Status |
|---|---|---|
| File is 278 lines, two types | 304 lines, two types | changed (+26 lines, #743 counters) |
| `internal static class UiThreadDispatcherFixture` `:29` | `:29` | unchanged |
| `UiThreadDispatcherTransaction : IDisposable` `:220` | `:246` | moved |
| Gate declaration `:32` `private static readonly SemaphoreSlim TransactionGate = new SemaphoreSlim(1, 1);` | `:32`, text identical | unchanged |
| `FieldLock` `:31` | `:31` | unchanged |
| Acquisition `:124` `await TransactionGate.WaitAsync().ConfigureAwait(false);` | `:149`, text identical | moved |
| `BeginTransactionAsync` `:122`–`:126`, "two-statement method" | `:142`–`:152`; now five statements (contended pre-check `:144`–`:147`, wait `:149`, acquisitions increment `:150`, construct `:151`) | changed |
| Transaction constructed in exactly one place `:125` | `:151`, still the only `new UiThreadDispatcherTransaction()` in the file | moved |
| `ReleaseTransactionGate` `:88`–`:91` | `:107`–`:111`; body now `Interlocked.Increment(ref _transactionReleases); TransactionGate.Release();` | changed |
| Release-helper doc `:85`–`:87` ("called only by Dispose") | `:103`–`:106` | moved |
| Sole caller of the release helper `:275` inside `Dispose()` `:261` | `:301` inside `Dispose()` `:287` | moved |
| Over-release doc `:258`–`:259` ("SemaphoreFullException") | `:284`–`:285`, text identical | moved |
| Lock-ordering doc `:18`–`:21` | `:17`–`:20` | moved |
| Single-owner doc `:11`–`:13` | `:12`–`:13` | moved |
| `EnsureDispatcher` never takes the gate, doc `:23`–`:27` | `:22`–`:27`, text identical | unchanged |
| `EnsureDispatcher` `:99` | `:119` | moved |
| `ResolveDispatcherField` `:133` | `:159` | moved |
| Gate never disposed anywhere in the file | still true (no `Dispose` on `TransactionGate`) | unchanged |
| No `WaitAsync` overload with `TimeSpan` or `CancellationToken` | still true; `TimeSpan` and `CancellationToken` do not appear in the file at all | unchanged |

### 1.2 Fixture test file (`FT`)

| Citation as written | Current location / value | Status |
|---|---|---|
| 353 lines, six tests | 396 lines, seven tests | changed |
| `[TestClass]` `:30`, class `:31`, `GateTimeoutMs = 60000` `:33` | `:30`, `:31`, `:33` | unchanged |
| Class doc `:10`–`:29`; "R1 primary / R4 probabilistic" `:14`–`:21`; "no sleep, no delay, no wall-clock wait" `:26`–`:28` | `:10`–`:29`; `:14`–`:21`; `:26`–`:27` | unchanged / unchanged / moved by one line |
| `[Timeout(GateTimeoutMs)]` at `:41, :104, :154, :203, :270, :317` (six) | `:43, :106, :156, :205, :272, :319, :364` (seven) | changed |
| R1 `EnsureDispatcher_WhileATransactionHoldsALiveDispatcher_DoesNotReplaceIt` `:40`–`:96` | `:42`–`:98` | moved |
| R2 `EnsureDispatcher_WhenTheFieldIsNull_InstallsAndRestoresOnDispose` `:103`–`:147` | `:105`–`:149` | moved |
| R3 `EnsureDispatcher_ScopeDisposedTwice_IsIdempotent` `:153`–`:188` | `:155`–`:190` | moved |
| R4 `Transaction_SecondCallerCannotInstallUntilTheFirstRestores` `:202`–`:262` | `:204`–`:264` | moved |
| R4 hold `:210`–`:214`; contended acquisition in `Task.Run` `:223`–`:225`; `secondCallerStarted.Wait()` `:237`; `transactionA.Dispose()` `:238` | `:212`–`:216`; `:225`–`:227`; `:239`; `:240` | moved |
| R4 issue #823 note `:194`–`:200` | `:196`–`:202` | moved |
| R5 `Transaction_DisposedTwice_DoesNotOverReleaseTheGate` `:269`–`:310` | `:271`–`:312` | moved |
| R5 `NotThrow` `:287`–`:292`; message "a second Dispose must not call Release again..." `:290`–`:291`; round trip `:294`–`:297` | `:289`–`:294`; `:292`–`:293`; `:296`–`:299` | moved |
| R6 `Install_CalledTwiceOnTheSameTransaction_ThrowsInvalidOperationException` `:316`–`:351` | `:318`–`:353` | moved |
| Acquisition statements `:49, :109, :159, :211, :224, :278, :295, :325` | `:51, :111, :161, :213, :226, :280, :297, :327`, plus new `:369` | moved / added |
| (not cited before) seventh test `TransactionGate_WhileThisTestHoldsATransaction_HasExactlyOneUnreleasedAcquisition` | `:355`–`:394` (doc `:355`–`:362`, attrs `:363`–`:364`, method `:365`–`:394`) | added |
| (not cited before) `public TestContext TestContext { get; set; }` | `:35` | added |

### 1.3 Other consumers and prior art

| Citation as written | Current location / value | Status |
|---|---|---|
| `WpfUiDispatcherTests.cs` class `:19`, `GateTimeoutMs` `:21`, `[Timeout]` `:49`, split statement `:58`–`:60`, CSharpier note `:56`–`:57` | all unchanged | unchanged |
| `QfcItemController.InitializationTests.Part2.cs` `BuildPumpHarnessAsync` `:46`, acquisition `:53`–`:55`, `catch { transaction.Dispose(); throw; }` `:61`–`:65`, `PumpHarness` `:285, :293, :300`, `Restore()` `:317`–`:330`, hold comment `:51`–`:52` | all unchanged | unchanged |
| `QfcItemController.InitializationTests.cs` `[TestClass]` `:29`, `PumpTimeoutMs = 60000` `:38` | `:29`, `:38` | unchanged |
| `QfcItemController.InitializationTests.Part3.cs` `[Timeout(PumpTimeoutMs)]` at `:39, :82, :130, :174, :244, :352, :400, :455` | identical eight lines | unchanged |
| `QfcHomeControllerRunAsyncTests.cs` class `:24`, `transaction = null` `:332`, bare `[TestMethod]` `:324`, acquisition `:353`–`:354` with no `ConfigureAwait`, test `:325` | all unchanged; the class is `partial` across four files (`QfcHomeControllerRunAsyncHighConfidenceTests.cs:16`, `...Part2.cs:25`, `...Part3.cs:23`) and a Grep for `Timeout|DoNotParallelize` over the glob `QfcHomeControllerRunAsync*.cs` returned no matches | unchanged (partial-class scope now verified) |
| `QfcFormControllerUndoHandoffTests.cs` class `:29`, `using (var transaction = await ...)` `:230, :281, :337`, bare `[TestMethod]` `:228, :279, :335` | all unchanged; no `Timeout` anywhere in the file | unchanged |
| `QfcItemController.TestSupport.cs` `EnsureUiThreadDispatcher` `:239`, doc `:234` | `:238`–`:239`, `:234` | unchanged |
| `Helper Classes/EmailMoveMonitorTests.cs` reads `Current` `:52, :61`, `[DoNotParallelize]` `:24` | unchanged | unchanged |
| `Helper Classes/ViewerQueueStaticWrapperTests.cs` `[DoNotParallelize]` `:11` | unchanged; these two remain the only `DoNotParallelize` usages in `QuickFiler.Test`, and there is still no `[assembly: Parallelize]` | unchanged |
| `UtilitiesCS.Test/TestHelpers/UiThreadDispatcherScope.cs` cross-ref `:40`; remarks `:19`–`:29`; reflection `:114` | `:40`; remarks span `:23`, `:27`; `GetField(` now `:116` | unchanged / moved |
| `TaskMaster.runsettings:4`–`:7` Workers 0, Scope ClassLevel; 30 lines | unchanged | unchanged |
| `.claude/rules/general-unit-test.md:98`–`:105` "Determinism Infrastructure" | heading `:98`, controllable clock `:102`, banned APIs `:104`, fake timers `:105` | unchanged |
| `QfcDatamodelLivenessTests.cs:48`–`:49` wording; `WaitForState` `:54`; `SpinUntil` `:56`; `.Task.Wait(TimeSpan.FromSeconds(5))` `:103`–`:105`, `:173`–`:175` | `:48`–`:49` text identical; `:54`; `:56`; the `Wait` calls are on `:103` and `:173` | unchanged |
| `QfcInitEmailQueueZeroBatchTests.cs:161`, `QfcDatamodelTeardownTests.cs:220` | unchanged | unchanged |
| `NonBlockingDelayTests.cs:29` "The outer MSTest `[Timeout]` is a deadlock bound, not a wait."; class doc `:16`–`:17`; `[Timeout(5000)]` `:32`; `FakeTimeProvider` `:42`, `Advance` `:51` | `:29` text identical; class-doc sentence spans `:15`–`:17`; `:32`; `:42`; `:51` | unchanged |
| Breadcrumb zero-probes `BreadcrumbCoordinatorLifecycleTests.cs:57`, `BreadcrumbUiThreadDispatchTests.cs:410`, `BreadcrumbSelectorToggleUiBoundaryTests.cs:419`, `BreadcrumbPopupBoundaryCoverageTests.cs:320` | all unchanged | unchanged |
| `BreadcrumbUiThreadDispatchTests.cs:366` `SemaphoreSlim(0)`, `:391` unbounded `_available.WaitAsync()` | unchanged; a further unbounded `_available.WaitAsync()` exists at `BreadcrumbPopupBoundaryCoverageTests.cs:305` inside `Task.WhenAny`, not previously listed | unchanged / addition |
| `QuickFiler.Test/packages.config:123`–`:124` MSTest 4.4.0; `:118` analyzers; `:86`–`:89` TimeProvider.Testing 10.10.0; 179 lines | `:44`–`:45` MSTest **4.4.1**; `:43` analyzers 4.4.1; `:31` TimeProvider.Testing 10.10.0; `:8` FluentAssertions 8.11.0; `:42` Moq 4.21.0; 74 lines | changed |
| `QuickFiler.Test.csproj:364`–`:365` TestFramework 4.4.0; `:4` and `:534` adapter imports; `Compile Include=` count 173; fixture `:194`, tests `:195`, UndoHandoff `:118`, WpfUiDispatcherTests `:211` | `:393`–`:394` 4.4.1; `:3` and `:567`; count **185**; fixture `:201`, tests `:203`, UndoHandoff `:120`, WpfUiDispatcherTests `:220`; no `EnableDefaultCompileItems` / `**` / `*.cs` match | changed |
| Flake-watch log `.../823/evidence/other/flake-watch-uithread-dispatcher-transaction.2026-09-09T00-15.md`: `OBSERVATIONS: 4` `:39`; "No sleep, retry attribute or timing tolerance" `:20`–`:21`; no-correlation `:86`–`:91`; append instructions `:93`–`:97`; `TestTimeout=4min` `:44, :57, :67, :79` | file exists, 97 lines, all citations at the same lines | unchanged |
| `CooperativeCancellation` absent tree-wide; no `testconfig.json` | still absent; still none | unchanged |
| `issue.md:28` wording ("finally ... no longer observed"); `:51`; `:53`; `:57` | unchanged (file 77 lines) | unchanged |

### 1.4 `spec.md` citations (beyond those already covered above)

| Spec statement | Current value | Status |
|---|---|---|
| Context: "`BeginTransactionAsync` (lines 122-126) acquires it at line 124"; "`ReleaseTransactionGate` (line 88)"; "sole caller ... `Dispose` (line 275)" | 142–152 / 149; 107; 301 | moved |
| Context: "The identical unbounded acquisition remains on the #743 branch at line 149" | #743 is merged; line 149 is now this branch's line | changed (now the same tree) |
| Environment: "MSTest 4.4.0 (`packages.config` lines 123-124)" | 4.4.1, lines 44–45 | changed |
| Repro fact 2: "Fourteen acquisition statements exist across five files" | fifteen across five (§2) | changed |
| Repro fact 3: `git grep -c Timeout` zero for the two classes | re-verified by Grep: still zero, including all four `QfcHomeControllerRunAsyncTests` partials | unchanged |
| Proposed Fix: "constructed in exactly one place (line 125)" | 151 | moved |
| The bound: `PumpTimeoutMs = 60000` at `InitializationTests.cs` line 38 | unchanged | unchanged |
| Files to change: "278 lines" / "353 lines" | 304 / 396 | changed |
| "173 occurrences" of `<Compile Include>` | 185 | changed |
| Determinism ruling: "the sixteen pre-existing `[Timeout(...)]` attributes in this same assembly" | the three gate-consuming classes now carry exactly sixteen (7 + 1 + 8); the assembly as a whole carries 50 across 10 files (`[Timeout(` Grep count) | wording now accurate for the consuming classes; assembly-wide figure is larger |
| Guards: "`[DoNotParallelize]` to the class holding the process-wide permit" | must be withdrawn per operator constraint (§5) | superseded |
| Existing tests to watch: R4 "(lines 202-262)"; R5 "(lines 269-310)" | 204–264; 271–312 | moved |
| AC4: `GateTimeoutMs` "declared at line 33" | unchanged | unchanged |
| AC6: "The six pre-existing tests" | seven | changed |
| AC6: "no test in any of the five consuming files ... required an edit" | five files still correct | unchanged |
| Risks row 3 (`[DoNotParallelize]` mitigation) | must be rewritten (§5) | superseded |
| Rollout: `BreadcrumbUiThreadDispatchTests.cs` second unbounded `WaitAsync()` | still present at `:391`; a third at `BreadcrumbPopupBoundaryCoverageTests.cs:305` | unchanged / addition |

---

## 2. Acquisition inventory (refreshed)

### Numeric Derivation Evidence — "fifteen acquisition statements across five files"

- **Complete Family:** every statement in `QuickFiler.Test` that invokes `UiThreadDispatcherFixture.BeginTransactionAsync()`, in any syntactic form (single-line, member chain split across lines, namespace-qualified, `using`-declaration operand, or unawaited `Task<>` assignment). Excludes the declaration (`FX:142`) and doc-comment mentions (`FX:242`).
- **Exhaustive Search Scope:** `**/*.cs` under the worktree (assembly-wide; other assemblies cannot reach the `internal` type, and the Grep confirmed no match outside `QuickFiler.Test`).
- **Inclusion Rules:** a call expression whose receiver is `UiThreadDispatcherFixture` (with or without the `QuickFiler.Controllers.Tests.` prefix) and whose member is `BeginTransactionAsync()`.
- **Exclusion Rules:** the method declaration; XML doc `<see cref>` text.
- **Primary Search Strategy:** single-line Grep, pattern `BeginTransactionAsync`, glob `**/*.cs`, content mode.
- **Primary Member Set (15):** `WpfUiDispatcherTests.cs:59`; `FT:51, :111, :161, :213, :226, :280, :297, :327, :369`; `QfcItemController.InitializationTests.Part2.cs:54`; `QfcHomeControllerRunAsyncTests.cs:354`; `QfcFormControllerUndoHandoffTests.cs:230, :281, :337`. (Two further hits, `FX:142` and `FX:242`, are the declaration and a doc comment and are excluded.)
- **Primary Count:** 15 statements, 5 files.
- **Cross-check Search Strategy:** multiline Grep, pattern `UiThreadDispatcherFixture\s*\.\s*BeginTransactionAsync\(\)`, glob `**/*.cs`, rooted at `QuickFiler.Test`. This matches the receiver-plus-member pair even when CSharpier has split them across lines, and by construction cannot match the declaration or the `cref` (which has no `()`).
- **Cross-check Member Set (15):** `WpfUiDispatcherTests.cs:59`; `QfcFormControllerUndoHandoffTests.cs:230, :281, :337`; `QfcHomeControllerRunAsyncTests.cs:354`; `QfcItemController.InitializationTests.Part2.cs:53–54`; `FT:50–51, :110–111, :160–161, :212–213, :225–226, :279–280, :296–297, :326–327, :368–369`.
- **Cross-check Count:** 15 statements, 5 files.
- **Member-set Comparison:** after normalising each multi-line match to the line carrying `.BeginTransactionAsync()`, the two sets are identical (15 = 15, same five files). The assertion "fifteen acquisition statements across five files" is therefore proposed.

### 2.1 Per-statement inventory

| # | File | Line | Enclosing method / helper | Release construct | Class `[Timeout]`? | Class `[DoNotParallelize]`? |
|---|---|---|---|---|---|---|
| 1 | `FT` | 51 | R1 `EnsureDispatcher_WhileATransactionHoldsALiveDispatcher_DoesNotReplaceIt` | `try`/`finally { transaction.Dispose(); }` (`:89`–`:92`) | Yes, `[Timeout(GateTimeoutMs)]` on every test | No |
| 2 | `FT` | 111 | R2 `EnsureDispatcher_WhenTheFieldIsNull_InstallsAndRestoresOnDispose` | `try`/`finally` (`:145`–`:148`) | Yes | No |
| 3 | `FT` | 161 | R3 `EnsureDispatcher_ScopeDisposedTwice_IsIdempotent` | `try`/`finally` (`:186`–`:189`) | Yes | No |
| 4 | `FT` | 213 | R4 `Transaction_SecondCallerCannotInstallUntilTheFirstRestores` (transaction A) | explicit `transactionA.Dispose()` at `:240`, **outside any `try`** | Yes | No |
| 5 | `FT` | 226 | R4, transaction B inside `Task.Run` | `try`/`finally { transactionB.Dispose(); }` (`:232`–`:235`) | Yes | No |
| 6 | `FT` | 280 | R5 `Transaction_DisposedTwice_DoesNotOverReleaseTheGate` | explicit `Dispose()` `:283`, second `Dispose()` `:286` (the test subject); no `finally` for this one | Yes | No |
| 7 | `FT` | 297 | R5 round trip | explicit `roundTrip.Dispose()` `:299`, no `finally` | Yes | No |
| 8 | `FT` | 327 | R6 `Install_CalledTwiceOnTheSameTransaction_ThrowsInvalidOperationException` | `try`/`finally` (`:344`–`:347`) | Yes | No |
| 9 | `FT` | 369 | #743 `TransactionGate_WhileThisTestHoldsATransaction_HasExactlyOneUnreleasedAcquisition` | `try`/`finally` (`:390`–`:393`) | Yes | No |
| 10 | `WpfUiDispatcherTests.cs` | 59 | `Invoke_InvokeAsync_BeginInvoke_ExecuteDelegateOnDispatcherThread` (unawaited `Task<>` at `:58`–`:59`, awaited `:60`) | `try`/`finally { transaction.Dispose(); }` (`:93`–`:96`) | Yes on this test (`:49`); the other test in the class (`:23`) has none but does not take the gate | No |
| 11 | `QfcItemController.InitializationTests.Part2.cs` | 54 | `internal static BuildPumpHarnessAsync` (`:46`) | `catch { transaction.Dispose(); throw; }` (`:61`–`:65`); ownership then transfers to `PumpHarness` (`:285`, `:300`) and is released by `Restore()` (`:317`–`:330`), called from each pump test's `finally` | Yes, `[Timeout(PumpTimeoutMs)]` on all eight Part3 tests | No |
| 12 | `QfcHomeControllerRunAsyncTests.cs` | 354 | `Worker_RunWorkerCompleted_HandlesCompletionCorrectly` (`:325`) | `transaction?.Dispose()` in `finally` (`:386`–`:390`); no `ConfigureAwait` | **No** (verified across all four partial files) | No |
| 13 | `QfcFormControllerUndoHandoffTests.cs` | 230 | `BackGroundMoveAsync_WithPendingQueueItem_DoesNotWriteMetricsBeforeDrain` | `using (var transaction = await ...)` | **No** | No |
| 14 | same | 281 | `BackGroundMoveAsync_WithPendingQueueItem_DoesNotDispatchCleanupBeforeDrain` | `using` | **No** | No |
| 15 | same | 337 | `BackGroundMoveAsync_AfterQueueDrains_WritesMetricsThenCleansUp` | `using` | **No** | No |

**Totals:** 15 acquisition statements, 5 files, 5 `[TestClass]` types (`QfcItemController_UiThreadDispatcherFixtureTests`, `WpfUiDispatcherTests`, `QfcItemController_InitializationTests`, `QfcHomeControllerRunAsyncTests`, `QfcFormControllerUndoHandoffTests`). None of the five classes carries `[DoNotParallelize]`. The four no-`[Timeout]` methods (rows 12–15) are exactly the population the old research §4.3 identified; unchanged.

`ReleaseTransactionGate` is still called from exactly one site (`FX:301`); the Grep for the identifier found only its declaration (`:107`), its call (`:301`), its doc (`:104`–`:105`) and the transaction-class doc.

---

## 3. Current content of `FT`

- **Total lines:** 396 (the file ends with a newline after line 396).
- **Class-level attributes:** `[TestClass]` (`:30`) only. No `[DoNotParallelize]`, no `[TestCategory]`.
- **Constant:** `private const int GateTimeoutMs = 60000;` (`:33`).
- **Property:** `public TestContext TestContext { get; set; }` (`:35`), consumed only by the #743 test's `TestContext.WriteLine` (`:386`–`:388`).
- **Usings (`:1`–`:6`):** `System`, `System.Threading`, `System.Threading.Tasks`, `System.Windows.Threading`, `FluentAssertions`, `Microsoft.VisualStudio.TestTools.UnitTesting`. `System` is already imported, so `TimeoutException`, `TimeSpan`, `Func<>` and `Action` need no new `using`.

| # | Test method | Attributes (lines) | Method lines |
|---|---|---|---|
| R1 | `EnsureDispatcher_WhileATransactionHoldsALiveDispatcher_DoesNotReplaceIt` | `[TestMethod]` 42, `[Timeout(GateTimeoutMs)]` 43 | 44–98 |
| R2 | `EnsureDispatcher_WhenTheFieldIsNull_InstallsAndRestoresOnDispose` | 105, 106 | 107–149 |
| R3 | `EnsureDispatcher_ScopeDisposedTwice_IsIdempotent` | 155, 156 | 157–190 |
| R4 | `Transaction_SecondCallerCannotInstallUntilTheFirstRestores` | 204, 205 | 206–264 |
| R5 | `Transaction_DisposedTwice_DoesNotOverReleaseTheGate` | 271, 272 | 273–312 |
| R6 | `Install_CalledTwiceOnTheSameTransaction_ThrowsInvalidOperationException` | 318, 319 | 320–353 |
| #743 | `TransactionGate_WhileThisTestHoldsATransaction_HasExactlyOneUnreleasedAcquisition` | 363, 364 | 365–394 |

**Headroom:** 500 − 396 = 104 lines. An addition of 60 lines lands at 456; 90 lines at 486. Both are under the ceiling, but the upper end leaves only 14 lines. The planner should budget the new test at no more than ~80 lines including its XML doc, or accept that a second future addition to this file will force a split. The fixture file (`FX`, 304 lines) has ample room for the overload, the constant and the message (estimated 25–35 lines including documentation).

---

## 4. Interaction of the #743 counters with a bounded acquisition

### 4.1 What the counters mean today (read from `FX:37`–`:54`, `:107`–`:111`, `:142`–`:152`)

- `_contendedAcquisitions` is incremented **before** the wait, when `TransactionGate.CurrentCount == 0` is observed (`:144`–`:147`). Its documented meaning (`:37`–`:40`) is "an acquisition that observed `CurrentCount == 0` immediately before waiting".
- `_transactionAcquisitions` is incremented **after** the wait returns (`:150`), i.e. only once the permit is held.
- `_transactionReleases` is incremented **before** `TransactionGate.Release()` (`:109`–`:110`), and `ReleaseTransactionGate` is reachable only from `Dispose` (`:301`), which is guarded by `_disposed` (`:289`–`:294`).

The invariant the #743 test relies on (`FT:379`–`:385`) is `TransactionAcquisitions − TransactionReleases == number of live (unreleased) transactions`, which equals 1 while the asserting test holds the sole permit. This holds because an acquisition is counted only after the permit is obtained and a release is counted exactly once per transaction before the permit is returned; no other party can increment acquisitions without first obtaining the permit the asserting test holds.

### 4.2 Required placement under a bounded wait

For the invariant to survive a failed bounded wait:

1. **Keep the contended pre-check where it is (before the wait).** A zero-bound probe issued while another transaction holds the permit genuinely "observed `CurrentCount == 0` immediately before waiting", so incrementing `_contendedAcquisitions` for it is consistent with the documented definition. Incrementing it is also harmless to every existing assertion (§4.3).
2. **Increment `_transactionAcquisitions` only on the `true` branch**, after the boolean result has been tested and before or after constructing the transaction (either order is equivalent; the constructor touches no counter). If the increment were moved before the wait, or executed unconditionally, a failed probe would raise acquisitions without a matching release and the #743 test's difference assertion would read 2 the next time it ran in the same process.
3. **The `false` branch touches neither `_transactionAcquisitions` nor `_transactionReleases` and constructs no transaction.** This is the same control-flow invariant the old research §2.3 stated; the counters add a second reason for it.

With that placement the arithmetic is: every `+1` to acquisitions corresponds to a permit actually held; every `+1` to releases corresponds to a permit actually returned; a failed bounded wait is `+0 / +0`. The difference therefore remains equal to the live-transaction count, and the #743 test at `FT:355`–`:394` keeps passing unchanged.

### 4.3 Does any existing test assert absolute counter values?

No. A Grep for `TransactionAcquisitions|TransactionReleases|ContendedAcquisitions|CurrentCount|GATECOUNTERS` over `**/*.cs` found consumers only in `FT:374`–`:376` (reads) and `FT:379`–`:388` (one assertion on the **difference** `acquisitions − releases`, and a `TestContext.WriteLine` of all three). `ContendedAcquisitions` is written to output and never asserted. `CurrentCount` is read only inside `FX:144`. R4 (`FT:204`–`:264`) produces a contended acquisition but asserts on dispatcher identity, not on any counter.

Consequently a new failed-acquisition test cannot perturb any existing assertion, whether it runs in the same class (sequential under `Scope ClassLevel`) or in another class (concurrent): the only counter assertion in the assembly is order-independent by construction, and the only counter a failed probe changes (`ContendedAcquisitions`) is never asserted.

---

## 5. Parallel-regime safety of the C1 construction without `[DoNotParallelize]`

**Operator constraint:** `TaskMaster.runsettings` (`Workers` 0, `Scope` ClassLevel) stays in force; the new test may not carry `[DoNotParallelize]`, retries or sleeps. The spec's "Guards on the new tests" bullet and its Risks row 3 must be rewritten accordingly.

### 5.1 (a) The probe deterministically observes the permit held by this test

`SemaphoreSlim(1, 1)` has exactly one permit. While this test holds it, `CurrentCount` is 0 and can be raised only by a `Release()`. The only `Release()` in the assembly is `FX:110`, reachable only through this test's own transaction's `Dispose` (single-caller property, `FX:301`). Concurrent acquirers from other classes are parked in the semaphore's wait queue and do not change `CurrentCount`. Therefore `WaitAsync(TimeSpan.Zero)` issued by this test, while it holds the permit, returns `false` immediately (documented: a zero timeout "doesn't block. It tests the state of the wait handle and returns immediately") irrespective of how many other classes are running or waiting. The observation is deterministic and independent of scheduling. It also contributes zero time to the run, satisfying clause (i) of the spec's determinism criterion.

### 5.2 (b) The test's own initial acquisition may wait on another class's hold

Yes, and this is bounded. The initial acquisition goes through the production entry point. Under the parallel regime it may queue behind any other class's transaction — the longest legitimate hold is a `PumpHarness` held for a whole pump-hosted test body, itself bounded by `[Timeout(PumpTimeoutMs)]` = 60000 ms. The new test carries `[Timeout(GateTimeoutMs)]` = 60000 ms (`FT:33`), so the wait is bounded by MSTest exactly as it is for the seven existing tests in the class, all of which already acquire through the same entry point under the same regime. The new test adds no exposure category that R1–R6 and the #743 test do not already carry. With the spec's 120000 ms gate bound, MSTest's 60000 ms reports first for this class, so the gate bound never changes this class's observable failure mode.

One consequence of the spec's bound should be recorded: if a test in a `[Timeout]`-carrying class is abandoned while parked on the gate and the 120000 ms bound then elapses in the abandoned continuation, the `TimeoutException` is thrown into a task nobody observes. A Grep for `UnobservedTaskException` over `**/*.Test/**/*.cs` found no subscriber, and the .NET Framework 4.5+ default does not fail the process on an unobserved task fault, so this changes nothing observable; it is noted so that a later reviewer does not mistake it for a new hazard.

### 5.3 The rule that makes it parallel-safe: zero-bound probes assert only failure

After this test releases its transaction, any other class may acquire the permit at once. A zero-bound probe expecting **success** after the release is therefore non-deterministic under `Workers 0 / ClassLevel` and must not be written. The success-path ("gate survives") assertion must instead go through the production entry point, which waits (bounded by `[Timeout]`) rather than fails when another class holds the permit. Stated as a rule for the planner and reviewer:

> A `TimeSpan.Zero` acquisition may be used only to assert **failure**, and only while the asserting test itself holds the permit. Success is asserted only through the production entry point.

### 5.4 Correction to the `SemaphoreFullException` reasoning

The old research §2.2 and spec AC5 describe a wrong-shape release on the failure branch as producing `SemaphoreFullException`. That is true only when the count is already 1. In the C1 construction the probing test holds the permit (count 0), so a wrong `Release()` on the `false` branch **succeeds silently** (count → 1) and mutual exclusion is broken from that instant. The exception surfaces later, at the legitimate holder's `Dispose` (count 1 → `Release()` → throw) — in a serial run, that is this test's own `Dispose`; under parallelism it may instead be another class's `Dispose`, because a parked acquirer would take the wrongly-released permit first. The companion assertion should therefore be placed on **this test's own `Dispose`** (`Action dispose = () => transaction.Dispose(); dispose.Should().NotThrow<SemaphoreFullException>()`), which is a deterministic detector in a serial run and a probabilistic one under parallelism. That is acceptable: the regression assertion is the `TimeoutException` on the probe, the control-flow invariant is enforced by review, and the counter-difference assertion (§5.5, item 4) is a second deterministic detector for the most likely wrong shape (an unconditional acquisitions increment).

### 5.5 Recommended design (no `[DoNotParallelize]`, no retries, no sleeps)

Place a single new test in the existing class `QfcItemController_UiThreadDispatcherFixtureTests`, with `[TestMethod]` and `[Timeout(GateTimeoutMs)]`, using only members already available in the file's `using` set. Shape, in order:

1. **Arrange — hold the permit.** `transaction = await UiThreadDispatcherFixture.BeginTransactionAsync().ConfigureAwait(false)` (production entry, production bound). Do **not** call `Install`: the test needs no dispatcher, no `StartRunningDispatcher`, and no write to `UiThread._dispatcher`, so the hold window is the probe plus assertions only, and `Dispose` takes the `!_hasInstalled` path (`FX:296`–`:299`) that skips `CompareExchange`. This also keeps the hold from perturbing `EmailMoveMonitorTests` (`[DoNotParallelize]`, reads `Current` at `:52`, `:61`).
2. **Act — probe with a zero bound while holding.** `Func<Task> probe = () => UiThreadDispatcherFixture.BeginTransactionAsync(TimeSpan.Zero);` then `await probe.Should().ThrowAsync<TimeoutException>().WithMessage("*<greppable token>*")`. FluentAssertions 8.11.0 (`packages.config:8`) supports `ThrowAsync`; in-assembly precedent at `BreadcrumbCoordinatorLifecycleTests.cs:240` and `WinFormsPumpHostTests.cs:380`–`:386`. This is AC4.
3. **Assert — no transaction was constructed on the failure path.** This is implied by the throw (the method has no other return), and is additionally evidenced by item 4.
4. **Assert — counters, still holding.** `(UiThreadDispatcherFixture.TransactionAcquisitions - UiThreadDispatcherFixture.TransactionReleases).Should().Be(1)`. Parallel-safe by the same argument as the #743 test (§4.1): only the holder can move either side of the difference. This proves the failed probe was not counted as an acquisition. Optionally, capture `ContendedAcquisitions` before the probe and assert `after >= before + 1` — monotonic, so other classes can only add to it, never subtract; a strict `== before + 1` would be non-deterministic under parallelism and must not be written.
5. **Assert — the failed probe released nothing.** Inside the `try`, `Action dispose = () => transaction.Dispose(); dispose.Should().NotThrow<SemaphoreFullException>(...)`. Keep an unconditional `transaction.Dispose()` in the `finally` as the safety net; `Dispose` is idempotent (`FX:289`–`:294`, proven by R5), so the double call is safe and cannot leak the process-wide permit even if an assertion fails.
6. **Assert — the gate is still usable (AC5 round trip).** After the `finally`, `roundTrip = await UiThreadDispatcherFixture.BeginTransactionAsync().ConfigureAwait(false); roundTrip.Dispose();` through the **production** entry, mirroring R5 `FT:296`–`:299`. Never `TimeSpan.Zero` here (§5.3).

Estimated size: 55–75 lines including XML doc, within the §3 headroom. No `Thread.Sleep`, no `Task.Delay`, no stopwatch, no elapsed-time assertion, no `[DoNotParallelize]`, no retry attribute.

**Why this needs no `[DoNotParallelize]`:** every assertion is either (i) made while this test holds the sole permit, where no other party can change the observed state, or (ii) made through the production entry point, which waits rather than fails under contention. No assertion depends on another class not running. The existing seven tests in the class already run under the same regime with the same exposure, and the R4 flake (§7) is a different mechanism (the second caller's scheduling) that this design does not use.

**Seam required in `FX`:** an `internal static Task<UiThreadDispatcherTransaction> BeginTransactionAsync(TimeSpan bound)` overload with the counter placement in §4.2 and the control-flow invariant of the old research §2.3, plus the parameterless overload delegating with the production default. The fixture's XML doc for `BeginTransactionAsync` (`FX:137`–`:141`) and the class doc (`FX:17`–`:19`, which currently states the gate "is held from transaction start until Dispose" without mentioning a bound) should be updated to state the bound and the failure type.

### 5.6 Fail-before framing (unchanged from spec AC9, restated for the planner)

The `TimeSpan` overload does not exist on the current tree, so the new test does not compile before the fix. Nothing merged since 2026-09-13 changes this. A compile-level `fail-before-exception` dossier under `evidence/regression-testing/` remains the correct record.

---

## 6. Determinism precedent citations (re-verified)

| Precedent | Current reading | Holds? |
|---|---|---|
| `QfcDatamodelLivenessTests.cs:48`–`:49` | `:48` "Bounded, event-driven wait for a state transition. This is not a fixed sleep: it returns" / `:49` "as soon as the condition holds, and fails the test with a clear message if it never does." | Yes, verbatim |
| `NonBlockingDelayTests.cs:29` | "The outer MSTest `[Timeout]` is a deadlock bound, not a wait." | Yes, verbatim |
| `NonBlockingDelayTests.cs` class doc "no elapsed-time measurement and no real wall-clock wait is used" | spans `:15`–`:17`; `[Timeout(5000)]` at `:32` | Yes (spec cites no line for this one) |
| `PumpTimeoutMs = 60000` at `QfcItemController.InitializationTests.cs:38` | `internal const int PumpTimeoutMs = 60000;` at `:38`; its doc `:33`–`:37` names `NonBlockingDelayTests.cs` as the `[Timeout]` precedent | Yes |
| MSTest version in `QuickFiler.Test/packages.config` | 4.4.1 (`:43`–`:45`), not 4.4.0 | Changed; patch bump, `mstest-net-4.4` documentation moniker still applies |
| `QuickFiler.Test.csproj` explicit `Compile Include` | 185 items; zero matches for `EnableDefaultCompileItems`, `Include="**`, `*.cs` | Yes; a new file still requires a project-file edit (AC7 premise intact) |

---

## 7. Issue #823 flake-watch reference and the two watched tests

- The append-only log at `docs/features/active/2026-09-08-quickfiler-teardown-review-residuals-823/evidence/other/flake-watch-uithread-dispatcher-transaction.2026-09-09T00-15.md` exists, is 97 lines, and still records `OBSERVATIONS: 4` (`:39`): one failure (Row 1, `:41`–`:52`, failure text not captured) and three passes, all under Workers 0 / ClassLevel with `/Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None`. Its prohibition on sleep/retry/tolerance is at `:20`–`:21`; append instructions at `:93`–`:97`. No row has been added since 2026-09-09.
- The in-code pointer to that log is at `FT:196`–`:202` (was `:194`–`:200`).
- `Transaction_SecondCallerCannotInstallUntilTheFirstRestores` (R4): attributes `FT:204`–`:205`, body `:206`–`:264`. Unchanged in content. Its contended acquisition (`:225`–`:227`) goes through the production entry point; with a 120000 ms bound and a hold window closed by `transactionA.Dispose()` at `:240` immediately after `secondCallerStarted.Wait()` at `:239`, it is not at risk from the bound. The recommended §5.5 design does not touch it and must not be presented as stabilising it.
- `Transaction_DisposedTwice_DoesNotOverReleaseTheGate` (R5): attributes `FT:271`–`:272`, body `:273`–`:312`. Unchanged in content; its round-trip acquisition at `:296`–`:299` remains the model for the §5.5 step 6 round trip.

---

## 8. Delivered work check — nothing in the 238 merged commits bounds this acquisition

- Grep for `WaitAsync\(|CancellationToken|TimeSpan` over `FX` returned exactly one line: `:149` `await TransactionGate.WaitAsync().ConfigureAwait(false);`. No `TimeSpan`, no `CancellationToken`, no bounded overload, no second acquisition path.
- Grep for `\.WaitAsync\(` over `**/*.Test/**/*.cs` found only: the subject (`FX:149`), two test-local unbounded `_available.WaitAsync()` calls (`BreadcrumbUiThreadDispatchTests.cs:391`, `BreadcrumbPopupBoundaryCoverageTests.cs:305`), and three `NonBlockingDelay.WaitAsync(...)` calls in `TaskMaster.Test` that are a different API. No bounded `SemaphoreSlim.WaitAsync` exists anywhere in the test assemblies; a bounded acquisition would still be new to them.
- Grep for `BeginTransactionAsync\(TimeSpan|WaitAsync\(TimeSpan|bounded acquisition|ContendedAcquisitions` over `docs/features` found the term only in this feature's own files, the #743 spec/research, and an agent-memory note; no other feature folder claims to deliver it.
- The #743 issue-update mirror (`.../743/evidence/issue-updates/issue-743.2026-09-13T18-10.md:22`) records the maintainer ratification's condition 4: "`TransactionGate` remains a `SemaphoreSlim(1,1)`, still awaited without timeout or cancellation token, still held from acquisition to disposal", and names #882 as the carrier. That statement is still true of the merged tree.

Conclusion: #743 changed the observability of the gate (counters) and the population of its consumers (one more test), not the acquisition's shape. There is no delivered work to avoid duplicating; the seam and the bound are still outstanding.

---

## 9. Rejected alternatives (brief)

- **`[DoNotParallelize]` on the new test's class** — rejected by the operator constraint, and shown unnecessary in §5.
- **A new test file** — would require a `QuickFiler.Test.csproj` edit (185 explicit `Compile Include` items, no globbing); the existing file has headroom (§3). Unchanged from the spec.
- **A `TimeSpan.Zero` success probe after release** — non-deterministic under Workers 0 / ClassLevel (§5.3). Must not be written.
- **Strict `ContendedAcquisitions == before + 1`** — non-deterministic under parallelism; use `>=` or omit (§5.5 item 4).
- **Moving the acquisitions increment before the wait** — breaks the #743 difference assertion on the first failed probe (§4.2).
- **Reflection onto `TransactionGate`, `CurrentCount`-only assertions, real MSTest abandonment, injectable gate** — rejected in the old research §3b and the spec; nothing merged changes those verdicts.

---

## 10. Testing implications (strategy only)

1. One new test in `FT` per §5.5; no `[DoNotParallelize]`; `[Timeout(GateTimeoutMs)]`.
2. AC6 must be re-worded to "the seven pre-existing tests" and the R4/R5 line ranges updated to `:204`–`:264` and `:271`–`:312`.
3. AC4/AC5 may additionally require the counter-difference assertion (§5.5 item 4) since it is the deterministic detector for an unconditional acquisitions increment; the spec's Risks table should replace the `[DoNotParallelize]` mitigation with the §5.3 rule.
4. The `SemaphoreFullException` companion assertion belongs on the holder's own `Dispose` (§5.4), not on the probe.
5. The seventh test (`#743`) must keep passing unmodified; its assertion depends only on the counter placement in §4.2.
6. R4 remains out of scope; append to the #823 log if a run produces a new observation.
7. Coverage: unchanged from the spec — test-assembly infrastructure only; no production-coverage movement, stated not measured.

---

## 11. Evidence index

Files read in full: `FX` (304 lines); `FT` (396 lines); `TaskMaster.runsettings` (30 lines); `QuickFiler.Test/packages.config` (74 lines); the #823 flake-watch log (97 lines); `.../743/evidence/issue-updates/issue-743.2026-09-13T18-10.md` (25 lines); the old research; `spec.md`; `issue.md`.

Files read in part: `WpfUiDispatcherTests.cs` (`:1`–`:100`); `QfcItemController.InitializationTests.Part2.cs` (`:20`–`:129`, `:275`–`:339`); `QfcItemController.InitializationTests.cs` (`:25`–`:44`); `QfcHomeControllerRunAsyncTests.cs` (`:1`–`:60`, `:315`–`:394`); `QfcFormControllerUndoHandoffTests.cs` (`:1`–`:60`, `:220`–`:349`); `QfcDatamodelLivenessTests.cs` (`:44`–`:59`); `TaskMaster.Test/AppGlobals/NonBlockingDelayTests.cs` (`:10`–`:39`).

Greps (all rooted at the worktree unless stated): `BeginTransactionAsync` (`**/*.cs`); multiline `UiThreadDispatcherFixture\s*\.\s*BeginTransactionAsync\(\)` (`QuickFiler.Test`); `UiThreadDispatcherFixture|UiThreadDispatcherTransaction|ReleaseTransactionGate|TransactionGate`; `TransactionAcquisitions|TransactionReleases|ContendedAcquisitions|CurrentCount|GATECOUNTERS`; `WaitAsync\(|CancellationToken|TimeSpan` (in `FX`); `\.WaitAsync\(` (`**/*.Test/**/*.cs`); `Timeout|DoNotParallelize|\[TestClass\]|class \w+Tests` over the five consuming files and the `QfcHomeControllerRunAsync*.cs` glob; `DoNotParallelize|assembly: Parallelize` (`QuickFiler.Test`); `\[Timeout\(` count (`QuickFiler.Test`); `Compile Include=` count and `EnableDefaultCompileItems|Include="\*\*|\*\.cs` (csproj); `MSTest`, `FluentAssertions|TimeProvider|Moq"` (packages.config); `ThrowAsync<` (`QuickFiler.Test`); `Wait\(TimeSpan\.FromSeconds\(5\)\)|SpinUntil\(` and `\.Wait\(0\)|WaitOne\(0\)` (`QuickFiler.Test`); `CooperativeCancellation` (tree-wide, no match); `UnobservedTaskException` (`**/*.Test/**/*.cs`, no match); `Determinism Infrastructure|Banned APIs in test code|...` (`.claude/rules/general-unit-test.md`). Globs: `**/testconfig.json` (none); the #823 evidence tree.

Commands NOT run: no `git`, `msbuild`, `vstest.console.exe`, `dotnet`, or `csharpier`. No file outside this research directory was created or modified.
