# Research: dispatcher pin call sites and fix design (issue #968)

- Issue: #968 `focus-and-theme-tests-leak-shared-dispatcher-setup` (full-bug)
- Branch: `bug/focus-and-theme-tests-leak-shared-dispatcher-setup-968`, base `origin/main` 94287369
- Date: 2026-10-02
- Method: every claim below was verified by reading the named file at the named lines in the item worktree or by the grep expressions quoted in section 2. `git log` was not available in this session (the Bash tool is disabled), so no history claim is made; where a comment in the code states history, it is quoted as a comment, not asserted as fact.
- Paths are repository-relative. `FEATURE` = `docs/features/active/2026-10-02-focus-and-theme-tests-leak-shared-dispatcher-setup-968`.

## 0. Findings that change the framing of the issue

1. **The fixture pin is not reference counted.** `UiThreadDispatcherFixture.EnsureDispatcher()` (`QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs:122-138`) seeds the parked dispatcher only when the static is `null` and returns a scope that remembers only whether *this call* installed it. A second concurrent caller receives a no-op scope. Whichever caller installed the value nulls it on `Dispose` (`:269-272`, `CompareExchange(_installed, null)`) regardless of how many other callers still hold a scope. This is the shared state the issue asks to "find and own".

2. **The two theme tests do not read the shared dispatcher at all.** `QfcItemController.SetThemeDark(bool)` / `SetThemeLight(bool)` (`QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs:274-286`, `:322-335`) call `Theme.SetQfcTheme(async: true)`, which goes through the theme's injected `_uiDispatcher` (`UtilitiesCS/HelperClasses/ThemeHelpers/Theme.cs:427-432`), and the test's themes are built by `QfcItemControllerTestSupport.BuildColorTheme`, which injects a `Mock<IUiDispatcher>` whose `InvokeAsync` returns `Task.CompletedTask` (`QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs:169-181`). No statement on that path dereferences `UtilitiesCS.UiThread.Dispatcher`. Consequently the issue's "a theme test can observe a null dispatcher" cannot be reproduced through the theme tests' own assertions; the `EnsureUiThreadDispatcher()` calls at `FocusAndThemeTests.cs:452` and `:468` are dead arrangement whose only effect is the gate-free write W1 that #950 identified as the concurrent writer that broke R4. The #950 code review (`.../950/code-review.2026-10-02T14-30.md:29`, O-1) assumed the theme tests "then read the process-wide dispatcher"; the code trace in section 3 shows they do not.

3. **Consequence for the issue's second validation idea** ("show the theme test fails without the fix"): it is not satisfiable as worded. The deterministic regression must live at the fixture level (section 5), where the missing reference counting is directly observable without concurrency.

4. **FocusAndThemeTests.cs is 497 content lines.** Any additive fix in that file (class-level `[ClassInitialize]`/`[ClassCleanup]` pin) breaches the 500-line limit and forces a file split. The recommended fix (section 4) removes lines from that file instead.

## 1. Current state

### 1.1 `UiThreadDispatcherFixture` (`QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs`, 342 content lines)

| Member | Lines | Behaviour verified |
|---|---|---|
| Class doc, lock design | 12-31 | `FieldLock` guards straight-line read-modify-write of `UiThread._dispatcher`; `TransactionGate` (`SemaphoreSlim(1,1)`) serialises install-to-restore transactions; ordering TransactionGate then FieldLock. Lines 26-30: `EnsureDispatcher` deliberately never takes `TransactionGate` because its callers carry no `[Timeout]`. |
| Statics | 34-46 | `FieldLock`, `TransactionGate`, `ParkedDispatcherLock`, cached `FieldInfo DispatcherField`, `_parkedDispatcher`, three monotonic counters (#743). **No pin counter exists.** |
| `Current` | 62-71 | Reads the private static `UiThread._dispatcher` under `FieldLock` via reflection. Does not call the throwing public getter. |
| `Exchange` | 77-85 | Atomic swap, returns previous. |
| `CompareExchange` | 92-104 | Writes `restoreTo` only if the field still holds `expected`; returns whether written. |
| `ReleaseTransactionGate` | 110-114 | Increments release counter, releases the semaphore. |
| `EnsureDispatcher` | 122-138 | Obtains the parked dispatcher outside `FieldLock` (:126); under `FieldLock`, if field is `null` writes parked and returns `new EnsureScope(parked)` (:130-134); otherwise returns `new EnsureScope(null)` (:137). Doc at 116-121 states: "Disposing the returned scope is optional: a discarded scope leaks exactly as the pre-fix helper did." |
| `TransactionGateAcquireTimeoutMs` | 146 | 120000 ms (#882). |
| `BeginTransactionAsync()` / `(TimeSpan)` | 155-190 | Bounded gate acquisition; throws `TimeoutException` with token `TRANSACTIONGATE_ACQUIRE_TIMEOUT`. |
| `ResolveDispatcherField` | 197-205 | Reflects `UiThread._dispatcher` (`NonPublic | Static`), asserts non-null. |
| `GetParkedDispatcher` | 213-241 | Lazily starts one STA background thread named `UiThreadDispatcherFixture.ParkedDispatcher` that captures `Dispatcher.CurrentDispatcher` and parks on a `ManualResetEventSlim` forever (never pumps). |
| `EnsureScope` (private sealed) | 249-274 | Fields `_installed` (the dispatcher this scope wrote, or `null`), `_disposed`. `Dispose` is idempotent; if `_installed != null` calls `CompareExchange(_installed, null)`. **Releasing the "last" pin is not a concept here: there is no count. The scope that installed nulls the field; any scope that installed nothing does nothing.** No dispatcher is shut down; the parked thread lives for the process. |
| `UiThreadDispatcherTransaction` | 284-341 | `Install` (one-shot, `Exchange`), `Dispose` (restore via `CompareExchange(_installedValue, _previous)` then release gate; idempotent). |

What "releasing the pin" does today, precisely: it writes `null` into `UiThread._dispatcher` if and only if (a) this scope was the one that installed the parked instance and (b) the field still holds that exact instance. It never nulls a transaction's value and never touches the parked thread.

### 1.2 `QfcItemControllerTestSupport.EnsureUiThreadDispatcher` (`QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs`, 440 content lines)

- `:238-239`: `internal static IDisposable EnsureUiThreadDispatcher() => UiThreadDispatcherFixture.EnsureDispatcher();` (pure forwarder).
- Doc `:216-237`: says the helper is "Needed for members that still delegate to a callee using the static `UiThread.Dispatcher` before the Phase 6 `IUiDispatcher` seam replaces it" and that "Discarding the scope is permitted and leaks exactly as the pre-issue-#493 `void` helper did, no more."
- `:161-168` (remarks on `BuildColorTheme`): "Cycle-3: the parameterless `Theme` constructor leaves `_uiDispatcher` null ... `SetThemeDark`/`SetThemeLight` route through `Theme.SetQfcTheme(async: true)`, which now reads `_uiDispatcher`, so a non-executing dispatcher mock ... is injected here". This comment is the in-code record that the theme path stopped reading the static.
- Other dispatcher-related helpers in the same file: `EnsureSynchronizationContext` (:90-96, installs a plain `SynchronizationContext` on the current thread when none exists, never restores), `StartRunningDispatcher` (:251-271, dedicated running STA dispatcher), `ShutdownDispatcher` (:277-280).

### 1.3 Every member in the solution that reads or writes the shared static `UiThread._dispatcher`

Production reader/writer (`UtilitiesCS/Threading/UiThread.cs`):
- `UiThread.Dispatcher` getter `:266-284` reads `_dispatcher` once and **throws `InvalidOperationException(DispatcherNotInitializedMessage)` when null** (:273-280); private setter `:283`.
- `Initialize()` `:82` writes it (W5 in #950's numbering); `ResetForTesting()` `:126` nulls it (W6; called only from `UtilitiesCS.Test`).

QuickFiler.Test readers/writers (all through the fixture; the fixture is `internal`, and a repo-wide grep for `UiThreadDispatcherFixture|UiThreadDispatcherTransaction` over `*.cs` hits only `QuickFiler.Test` files plus one doc-comment path mention in `UtilitiesCS.Test/TestHelpers/UiThreadDispatcherScope.cs:40`):
- `UiThreadDispatcherFixture.Current` reads: `EmailMoveMonitorTests.cs:52,61`; `UiThreadDispatcherFixtureTests.cs:55,61,63,115,120,122,170,225,241`.
- Writers: `EnsureDispatcher` (:132), `EnsureScope.Dispose` (:271), `Transaction.Install` (:316 via `Exchange`), `Transaction.Dispose` (:336 via `CompareExchange`).
- Transaction holders (gated, correct): `UiThreadDispatcherFixtureTests.cs:50,110,160,218,291,338,380,422`; `WpfUiDispatcherTests.cs:59`; `QfcHomeControllerRunAsyncTests.cs:354`; `QfcFormControllerUndoHandoffTests.cs:230,281,337`; `QfcItemController.InitializationTests.Part2.cs:53` (released by `PumpHarness.Restore` :317-330).

Other test assemblies own their own, different mechanisms over the same static: `UtilitiesCS.Test/TestHelpers/UiThreadDispatcherScope.cs` (:116-117 reflects `_dispatcher`) and `UiThreadStateScope.cs` (:34, :192). They never reference the QuickFiler fixture. #950's research (`.../950/research/2026-10-01T00-00-wall-clock-waits-research.md:129`, W7) records that each test assembly runs in its own AppDomain under the MSTest adapter on .NET Framework, so those statics are per-assembly; that closure was not re-verified here and is cited, not asserted.

## 2. Exhaustive call-site enumeration

### 2.1 Numeric Derivation Evidence: call sites of the pin helper

- **Complete Family:** every invocation expression of `QfcItemControllerTestSupport.EnsureUiThreadDispatcher()` and of `UiThreadDispatcherFixture.EnsureDispatcher()` in C# source anywhere in the repository (all projects, test and production).
- **Exhaustive Search Scope:** `<repo-root>/**/*.cs` (Grep tool, ripgrep, glob `*.cs`, no path restriction).
- **Inclusion Rules:** a line containing the identifier followed by `()` that is an executable statement or expression (not inside `///` or `//`).
- **Exclusion Rules:** method declarations, `<see cref>`/doc-comment mentions, test-method names that embed the identifier as a prefix.
- **Primary Search Strategy or Query Expression:** content grep `EnsureUiThreadDispatcher\(\)|EnsureDispatcher\(\)` over `*.cs`.
- **Primary Member Set (9 matching lines; 7 invocations after exclusions):**
  - Declarations excluded: `QfcItemController.UiThreadDispatcherFixture.cs:122`, `QfcItemController.TestSupport.cs:238`.
  - Invocations: `QfcItemController.TestSupport.cs:239` (forwarder calls `EnsureDispatcher()`); `QfcItemController.UiThreadDispatcherFixtureTests.cs:60`, `:119`, `:166`, `:222`; `QfcItemController.FocusAndThemeTests.cs:452`, `:468`.
- **Primary Count:** 7 invocations (6 of `EnsureUiThreadDispatcher()`, 1 of `EnsureDispatcher()`).
- **Cross-check Search Strategy or Query Expression:** count-mode grep of the bare identifiers `EnsureUiThreadDispatcher|EnsureDispatcher` over `*.cs` (catches calls written across lines or without parentheses), then classify each occurrence by reading the line.
- **Cross-check Member Set (20 occurrences in 5 files):**
  - `UiThreadDispatcherFixture.cs` 5: lines 26 (`<see cref>`), 27 (doc), 122 (declaration), 195 (doc), 244 (`<see cref>`) → 0 invocations.
  - `TestSupport.cs` 2: 238 (declaration), 239 (invocation) → 1.
  - `InitializationTests.Part2.cs` 1: line 124 (comment) → 0.
  - `UiThreadDispatcherFixtureTests.cs` 10: 44, 107, 157 (test names), 70, 128, 198 (doc/`because` text), 60, 119, 166, 222 (invocations) → 4.
  - `FocusAndThemeTests.cs` 2: 452, 468 (invocations) → 2.
- **Cross-check Count:** 7 invocations.
- **Member-set Comparison:** identical sets ({TestSupport:239, FixtureTests:60,119,166,222, FocusAndThemeTests:452,468}); counts agree (7 = 7). The assertion "six test-side call sites of `EnsureUiThreadDispatcher()` plus the single forwarder" is supported.
- **Non-vacuity control:** the same primary pattern applied to the fixture test file alone returns 4 lines; the pattern `EnsureUiThreadDispatcher\(\)\.Dispose\(\)` returns 0 (no site disposes inline), and the control pattern `BeginTransactionAsync\(` returns 15 lines across 6 files (section 1.3), showing the search tool and glob were live.

### 2.2 Classification of the six `EnsureUiThreadDispatcher()` test call sites

| # | Site | Test | Usage | Correct? |
|---|---|---|---|---|
| 1 | `FocusAndThemeTests.cs:452` | `QfcItemController_FocusAndThemeTests.SetThemeDark_FromNormal_SelectsDarkNormalTheme` | (a) return value discarded | **No.** Writes W1 (seeds parked into a null field) and never reverts; the test does not read the static (section 3), so the call is also unnecessary. |
| 2 | `FocusAndThemeTests.cs:468` | `...SetThemeLight_FromNormal_SelectsLightNormalTheme` | (a) discarded | **No.** Same as #1. |
| 3 | `UiThreadDispatcherFixtureTests.cs:60` | R1 `EnsureDispatcher_WhileATransactionHoldsALiveDispatcher_DoesNotReplaceIt` | (b) held in a local, explicitly disposed at :62; the acquire-observe-dispose triple is the act under test, no throwing statement between :60 and :62 | Correct for its purpose (observing the field immediately after disposal is the assertion; a `using` would not allow that). Runs inside a gated transaction with `try/finally`. |
| 4 | `UiThreadDispatcherFixtureTests.cs:119` | R2 `EnsureDispatcher_WhenTheFieldIsNull_InstallsAndRestoresOnDispose` | (b) local, disposed at :121 | Correct, same reasoning. Note R2 **nulls the field** on dispose when the baseline was null: this is a W2 write that today can race any other unpinned holder. Under reference counting it is only reachable when R2 is the sole holder. |
| 5 | `UiThreadDispatcherFixtureTests.cs:166` | R3 `EnsureDispatcher_ScopeDisposedTwice_IsIdempotent` | (b) local, disposed at :169 and :171 | Correct (idempotency is the subject). Same W2 note as R2. |
| 6 | `UiThreadDispatcherFixtureTests.cs:222` | R4 `Transaction_SecondCallerCannotInstallUntilTheFirstRestores` | `using` (:221-223), taken after the gate is acquired | Correct. Its disposal is the W2 write #950's plan named in the stop marker (`.../950/plan.2026-10-01T07-11.md:134`). The surrounding `transactionA` is **not** in a `try/finally` (CR-5 in `.../950/code-review.2026-10-02T14-30.md:24`), see section 6. |

### 2.3 Other touches of the shared dispatcher or related ambient state without a pin

| Pattern | Hits | Assessment |
|---|---|---|
| `UiThreadDispatcherFixture.Current` outside the fixture tests | `EmailMoveMonitorTests.cs:52,61` | Read-only snapshot and teardown equality assertion. The class is `[DoNotParallelize]` (:24), so MSTest runs it outside the parallel phase; a pin is not required. Correct. |
| `Dispatcher.CurrentDispatcher` in test projects (grep `Dispatcher\.CurrentDispatcher` over `*.Test/**/*.cs`) | QuickFiler.Test: `ItemViewerBreadcrumbThreadAffinityTests.cs:48,181`; `QfcHomeControllerRunAsyncTests.cs:340`; `TestSupport/WinFormsPumpHostTests.cs:226`; `QfcItemController.TestSupport.cs:258`; `QfcItemController.UiThreadDispatcherFixture.cs:225`. UtilitiesCS.Test / TaskMaster.Test: 15 further hits (listed in the grep output; e.g. `UiThread_Tests.cs:363,473`, `ProgressViewer_Tests.cs:246`, `AppOlObjectsFolderTreeServiceLifecycleTests.cs:375`). | These obtain the *current thread's* WPF dispatcher; none writes `UiThread._dispatcher` except via the two fixture mechanisms already enumerated. Not in scope. |
| `SynchronizationContext.SetSynchronizationContext(` in test projects | 200+ hits across QuickFiler.Test, UtilitiesCS.Test, TaskMaster.Test (persisted grep output) | Per-thread ambient context, not the shared static. Out of scope for this issue; one touched-file nit is recorded in section 6 (`TestSupport.EnsureSynchronizationContext` never restores). |
| Transactions without `using`/`try-finally` | `QfcHomeControllerRunAsyncTests.cs:353-355` with `finally { transaction?.Dispose(); }` at :388 (correct); `InitializationTests.Part2.cs:53-65` with catch-dispose-rethrow and `PumpHarness.Restore` (correct); `WpfUiDispatcherTests.cs:60-96` `try/finally` (correct); `QfcFormControllerUndoHandoffTests.cs:230,281,337` `using` (correct); fixture tests R1,R2,R3,R5(partly),R6,#743,#882 `try/finally` (correct); **R4 (:218-251) no `try/finally`** (defect, section 6). | |

## 3. How the production code under test consumes the dispatcher

Trace for `SetThemeDark_FromNormal_SelectsDarkNormalTheme` (`FocusAndThemeTests.cs:447-462`):

1. `new FocusController()` → `protected QfcItemController() { }` (`QuickFiler/Controllers/QfcItemController.Initialization.cs:27`): empty body, touches nothing.
2. `SetField(controller, "_themes", BuildAllThemes())`: `BuildAllThemes` (`FocusAndThemeTests.cs:41-55`) builds **one** `Theme` via `BuildColorTheme` and maps all four keys to it. `BuildColorTheme` (`TestSupport.cs:169-181`): `new Theme()` (`Theme.cs:141`, empty constructor; the only constructor that defaults `_uiDispatcher` to `new WpfUiDispatcher()` is the large overload at `Theme.cs:67`, not used here), three colours, then reflection-injects a `Mock<IUiDispatcher>` with `InvokeAsync(It.IsAny<Action>())` returning `Task.CompletedTask`.
3. `controller.SetThemeDark(async: true)` (`FocusAndTheme.cs:274-286`): `_activeTheme is null` → `_themes["DarkNormal"].SetQfcTheme(true)` then `_activeTheme = "DarkNormal"`.
4. `Theme.SetQfcTheme(bool async)` (`Theme.cs:427-445`): `if (async) { _uiDispatcher.InvokeAsync(() => SetQfcTheme()); }` → the mock returns a completed task and never runs the delegate. The commented-out line `:441` `//UiThread.Dispatcher.Invoke(() => SetQfcTheme());` is the former static read.
5. Assertion reads `_activeTheme` by reflection.

`UiThread.Dispatcher` (`UiThread.cs:266-284`) is never evaluated on this path, so a null or foreign value in `UiThread._dispatcher` cannot throw here. The same holds for `SetThemeLight`. Had the path still read the static, the failure mode would be `InvalidOperationException("The UI dispatcher has not been captured. Call UiThread.Init() ...")` from the getter (`UiThread.cs:240-241`, `:279`), not a `NullReferenceException`.

Members of `QfcItemController` that *do* reach the static exist (for example `QfcTipsDetails.ToggleAsync` via `UtilitiesCS/HelperClasses/ToolTips/QfcTipsDetails.cs:254,277`, which is why `InitializationTests.Part2.cs:121-132` installs the pump dispatcher under a transaction), but none is on the two theme tests' paths: `ToggleFocus*`/`ToggleTips*` tests in the same file go through `IItemViewer.Invoke` mocks and `EnableHandlelessThemeInvoke`, and `ToggleNavigationAsync` uses a `Mock<IQfcTipsDetails>` (:347-357).

## 4. Candidate approaches and recommendation

### Approach A (recommended): reference-count the fixture pin; delete the two dead theme-test calls

Description:
- `UiThreadDispatcherFixture` gains, under `FieldLock`, `private static int _pinCount` and `private static bool _fixtureInstalledParked`. `EnsureDispatcher`: increment `_pinCount`; if the field is `null`, write parked and set `_fixtureInstalledParked = true`; return a scope. `EnsureScope.Dispose` (idempotent via `_disposed`): under `FieldLock`, decrement `_pinCount`; when it reaches 0 and `_fixtureInstalledParked` is true and the field still references the parked instance, write `null` and clear the flag (do the field operations inline under the same lock rather than via the re-locking `CompareExchange`, so the decrement and the write are one straight-line critical section, consistent with the `FieldLock` contract at `:16-18`).
- Semantics preserved: a scope that found a transaction's value installs nothing and, at count zero, nulls nothing (R1 keeps passing). A single pin on a null baseline still nulls on dispose (R2, R3 keep passing). R4 is unchanged in shape; its baseline disposal nulls only if no other pin is live, which is the property the issue asks for.
- Residual: if a transaction installs a live dispatcher while pins are held and the last pin releases during the transaction, the parked value is restored by the transaction and remains with zero pins (the pre-#968 leak shape, benign: a non-null parked dispatcher). Keeping `_fixtureInstalledParked` true until the null write actually succeeds lets the next pin cycle clean it up. Record in the class doc.
- `FocusAndThemeTests.cs:452` and `:468` are deleted together with the `// Arrange — async:true queues the theme application on the dispatcher ...` comment at :450-451 rewritten to say the theme's injected `IUiDispatcher` mock absorbs the queued delegate. The file shrinks (497 → 495 before other in-scope nits).
- Comment drift fixed in the fixture (`:116-121`, `:243-248`), the wrapper (`TestSupport.cs:216-237`), and R4's doc (`FixtureTests.cs:196-208`, whose W2 invariant is superseded by counting).

Advantages: owns the shared state at its single mutation point (the fixture's own design goal, `:13-14`); protects every present and future pin holder, not only the theme tests; no new MSTest lifecycle attributes (none exist in `QuickFiler.Test` today: grep `\[ClassInitialize\]|\[ClassCleanup` returns 0); no file split; the regression test is single-threaded and deterministic (section 5); the theme tests stop depending on state they neither own nor need.

Limitations: changes a shared test fixture used by four classes; requires the fixture tests R1-R3 to be re-read for intent (they pass under the new semantics by the analysis above, to be confirmed by running them). A discarded scope now holds a pin forever (count never returns to zero), which keeps the parked dispatcher installed for the rest of the process; that is the same end state as today's discard, so no caller regresses, but the doc must say "a discarded scope pins for the process lifetime" instead of "leaks exactly as the pre-fix helper did".

Alignment: General Code Change Policy "Simplicity first" and "Reusability" (one fix at the shared seam instead of per-class ceremony); General Unit Test Policy Independence/Isolation (classes stop writing shared state they do not use).

### Approach B: class-level pin in `QfcItemController_FocusAndThemeTests` (`[ClassInitialize]` acquire, `[ClassCleanup]` release), fixture unchanged

Rejected as the primary fix because: (i) the theme tests do not read the static, so the pin protects nothing in that class and merely moves the W1 write to class start and the W2 write to class end, where it still races any other unpinned holder; (ii) without reference counting the class-end disposal nulls the field for every other class still relying on an unpinned ensure (exactly the exposure the issue describes, relocated); (iii) it requires a split of a 497-line file; (iv) `[ClassCleanup]` timing: `QuickFiler.Test` uses MSTest 4.4.1 (`QuickFiler.Test/packages.config:44-45`); the `ClassCleanupBehavior` default for that major version was not verified in this session and would have to be pinned explicitly (`ClassCleanupBehavior.EndOfClass`) to avoid holding the pin to end of assembly. If the orchestrator nevertheless requires the issue's literal proposal, it must be combined with Approach A's counting to be sound.

### Approach C: per-test `using (QfcItemControllerTestSupport.EnsureUiThreadDispatcher())` in the two theme tests, fixture unchanged

Rejected: same soundness gap as B (each test's disposal is a W2 write that can null the field under a concurrent unpinned holder), adds two lines to a 497-line file, and pins state the tests do not use.

### Rejected alternative from #950 (not re-litigated): making `EnsureDispatcher` take the transaction gate

Rejected by the fixture's design note (`:26-30`) and #950 research (`...research.md:152`): callers without `[Timeout]` would block without bound.

## 5. Deterministic regression test design (fails before, passes after)

Location: a **new** file `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherPinCountTests.cs` (the existing fixture test file is 470 content lines and the CR-5 `try/finally` fix adds to it; a ~60-line addition would breach 500). Requires a new `<Compile Include="Controllers\QfcItemController.UiThreadDispatcherPinCountTests.cs" />` next to `QuickFiler.Test.csproj:203`. MSTest + FluentAssertions; Moq is not needed for the fixture-level tests (state it in the class doc rather than adding an unused `using`). Same `[Timeout(60000)]` convention as the sibling file (`FixtureTests.cs:33,43`). No `Thread.Sleep`, `Task.Delay`, timers, or files; no concurrency is required because the defect is observable on one thread.

Test 1 (the regression; AC candidate): `EnsureDispatcher_TwoPinsHeld_ReleasingTheFirstKeepsTheDispatcherUntilTheLastRelease`
- Arrange: `using var transaction = await UiThreadDispatcherFixture.BeginTransactionAsync()`; `transaction.Install(null)` to force a known null baseline (the R2 pattern, `FixtureTests.cs:110-116`). `IDisposable pinA = QfcItemControllerTestSupport.EnsureUiThreadDispatcher(); IDisposable pinB = QfcItemControllerTestSupport.EnsureUiThreadDispatcher();` `Dispatcher afterBothPins = UiThreadDispatcherFixture.Current;`
- Act: `pinA.Dispose(); Dispatcher afterFirstRelease = UiThreadDispatcherFixture.Current; pinB.Dispose(); Dispatcher afterLastRelease = UiThreadDispatcherFixture.Current;`
- Assert: `afterBothPins.Should().NotBeNull()`; `afterFirstRelease.Should().BeSameAs(afterBothPins, because: "a holder that did not take the last pin must not lose the dispatcher")`; `afterLastRelease.Should().BeNull(because: "the last release reverts the fixture's own seeding")`.
- Fail-before (current code): `pinA` installed parked (`_installed = parked`), `pinB` is a no-op scope. `pinA.Dispose()` runs `CompareExchange(parked, null)` → field becomes null. The second assertion fails with FluentAssertions text of the form `Expected afterFirstRelease to refer to Dispatcher { ... Name = "UiThreadDispatcherFixture.ParkedDispatcher" } because a holder that did not take the last pin must not lose the dispatcher, but found <null>.` Deterministic: no scheduling is involved.
- Pass-after: count 2 → 1 on `pinA.Dispose()`, field untouched; count 1 → 0 on `pinB.Dispose()`, field nulled.

Test 2 (order independence of the count): same arrange, dispose `pinB` first then `pinA`; identical assertions. Before the fix `pinB.Dispose()` is a no-op (passes the middle assertion) but then `pinA.Dispose()` nulls (passes the last) — so this test passes before and after; it is a specification test, not the regression, and must be labelled as such in its doc comment.

Test 3 (foreign value protection, extends R1): under a transaction that installed a live dispatcher from `StartRunningDispatcher`, take two pins and release both; `Current` must still be the live dispatcher (count reaches 0 but `_fixtureInstalledParked` is false). Passes before and after; keeps the install-ownership rule honest.

Test 4 (counter hygiene, optional): after test 1's sequence, a fresh single pin on a null baseline still installs and its release still nulls (proves the flag was cleared). Passes before and after.

Negative control to record in evidence: run test 1 against the unmodified fixture by fully qualified name with `/Settings:scripts/vscode/TaskMaster.cli.runsettings` and capture the failure message above; then run after the fixture change and capture `Passed`.

On the issue's second idea ("show the theme test fails without the fix"): not achievable, section 3. The spec should state that the regression lives at the fixture level and that the theme tests are made independent of the static by removing the dead calls; the orchestrator should ratify that re-wording explicitly because it departs from the issue's proposed validation.

## 6. Related defects in the same files (in scope per the maintainer directive)

| # | File:lines | Defect | Proposed in-scope change |
|---|---|---|---|
| D1 | `UiThreadDispatcherFixture.cs:116-121`, `:243-248` | Comment drift once counting exists ("Disposing the returned scope is optional: a discarded scope leaks exactly as the pre-fix helper did"; "A scope that installed nothing carries null and is a no-op"). | Rewrite to describe pin counting, install ownership, and the discard consequence. Also extend the class doc (`:12-31`) with the counting rule under `FieldLock`. |
| D2 | `TestSupport.cs:216-237` | Same drift in the wrapper doc; also states the helper is "Needed for members that still delegate to a callee using the static" and "Becomes moot once the callee routes through the injectable dispatcher seam" while the theme callers it was written for no longer read the static (`:161-168`). | Rewrite the `<summary>`/`<para>` to describe the counted pin and name the remaining legitimate callers (the fixture tests). |
| D3 | `UiThreadDispatcherFixtureTests.cs:196-208` (R4 doc) | "Invariant for future editors: no other class may dispose an ensure scope holding the parked dispatcher (W2)" becomes false/obsolete under counting (another class's release can no longer null while R4 holds its pin). | Replace the W2 sentence with the counting guarantee; keep the W5 (`UiThread.Initialize`) residual. |
| D4 | `UiThreadDispatcherFixtureTests.cs:218-251` (R4) | `transactionA` is disposed only at :251 with no `try/finally`; a throw from `Install`, the pin, or `secondCallerStarted.Wait()` would hold the gate until the 120 s bound or the #743 counter test surfaces it (CR-5 of the #950 review; R1/R5/R6 use `try/finally`). Same file is touched by D3. | Wrap in `try/finally` with an idempotent re-dispose (R5 proves double dispose is safe). Adds ~6 lines (470 → ~476). Check that the already-filed #972 (950 review residuals) does not also carry CR-5, to avoid a duplicate fix; if it does, record that #968 delivers it and #972 should drop it. |
| D5 | `FocusAndThemeTests.cs:99-115` | Private `BuildExecutingViewer()` duplicates `QfcItemControllerTestSupport.BuildExecutingViewer()` (`TestSupport.cs:289-305`), whose own doc at `:282-288` says it "mirrors the private static ... which is not reachable from another test file". Copy-paste in a touched file at 497/500 lines. | Delete the private copy and call the shared helper (17 lines removed; also fix the shared helper's doc sentence). Pure test-quality change; no behaviour difference (both execute `Invoke`/`BeginInvoke` synchronously). |
| D6 | `FocusAndThemeTests.cs:450-451` | Arrange comment says "async:true queues the theme application on the dispatcher without executing it" without naming that the dispatcher is the theme's injected mock, which is why the shared static is irrelevant. | Reword when deleting :452/:468. |
| D7 | File sizes (content lines): `FocusAndThemeTests.cs` 497, `UiThreadDispatcherFixtureTests.cs` 470, `TestSupport.cs` 440, `UiThreadDispatcherFixture.cs` 342. | Two files are within 30 lines of the 500 limit; additive designs (B, C, or new tests in the fixture test file) breach it. | Approach A reduces `FocusAndThemeTests.cs`; new tests go to a new file (section 5); fixture growth (~25 lines) stays well under 500. Record post-change counts in evidence. |
| D8 (observed, recommend no change) | `TestSupport.cs:90-96` `EnsureSynchronizationContext` | Installs a plain `SynchronizationContext` on the MSTest worker thread and never restores it; documented as deliberate and relied on by handler tests. Not the dispatcher static and not this issue's root cause. | Leave; note in spec as observed and intentionally untouched (changing it would alter the premise of unrelated tests, #950 recorded the ambient context as an execution-time observation). |

## 7. Build and test facts the plan needs

- Test project: `QuickFiler.Test/QuickFiler.Test.csproj`, legacy (non-SDK) project, `<TargetFrameworkVersion>v4.8.1</TargetFrameworkVersion>` (:17), explicit `<Compile Include>` items. Relevant entries: `Controllers\QfcItemController.TestSupport.cs` (:200), `Controllers\QfcItemController.UiThreadDispatcherFixture.cs` (:201), `Controllers\QfcItemController.UiThreadDispatcherFixtureTests.cs` (:203), `Controllers\QfcItemController.FocusAndThemeTests.cs` (:212). **A new test file needs its own `<Compile Include>`**; `*.csproj` is excluded from CSharpier by `.csharpierignore`.
- Packages (`QuickFiler.Test/packages.config`): MSTest.TestAdapter/TestFramework 4.4.1 (:44-45), Moq 4.21.0 (:42), FluentAssertions 8.11.0 (:8). No `[ClassInitialize]`/`[ClassCleanup]` is used anywhere in `QuickFiler.Test` today.
- Namespace/visibility: fixture and support types are `internal` in `QuickFiler.Controllers.Tests`; tests in the same assembly reach them directly.
- Test-run route: `scripts/vscode/Invoke-MSTestWithCoverage.ps1`. Inner vstest arguments (`:82-94`): `/Settings:scripts/vscode/TaskMaster.cli.runsettings`, `/InIsolation`, `/TestCaseFilter:TestCategory!=LiveOutlook`, `/ResultsDirectory:coverage\test-results`, `/Logger:trx;LogFileName=mstest-coverage-run.trx` (fixed names `:297-298`, not overridable). Assemblies discovered as `*.Test.dll` under `bin\Debug` excluding `obj`, `ref`, and `.claude` segments (`:348-355`). Coverage settings from repo-root `coverage.config` with a derived test-assembly exclusion (`:97-134`). The run throws `MSTest with coverage failed with exit code N` on any test failure (`:262`) before post-processing.
- Runsettings: `scripts/vscode/TaskMaster.cli.runsettings` sets `<Workers>0</Workers>` and `<Scope>ClassLevel</Scope>` (:4-7) and no data collector. The repo-root `TaskMaster.runsettings` (Visual Studio auto-detect) carries the coverage exclusions; not used by the CLI route.
- Assembly init: `QuickFiler.Test/SetupAssemblyInitializer.cs:14-25` installs an assembly resolver and WinForms defaults; it does not write `UiThread._dispatcher`, so a class run alone starts with a null baseline (relevant to the fail-before run of section 5).
- Fully qualified names for targeted runs: `QuickFiler.Controllers.Tests.QfcItemController_FocusAndThemeTests.SetThemeDark_FromNormal_SelectsDarkNormalTheme`, `...SetThemeLight_FromNormal_SelectsLightNormalTheme`, `QuickFiler.Controllers.Tests.QfcItemController_UiThreadDispatcherFixtureTests.*` (R1-R6, #743, #882), and the new `QuickFiler.Controllers.Tests.QfcItemController_UiThreadDispatcherPinCountTests.*`.
- Concurrency check for the plan (mirrors #950 P4-T5): run the fixture test class, the new pin-count class, and `QfcItemController_FocusAndThemeTests` in one invocation under the CLI runsettings and require all `Passed`; this is a supporting observation, not the regression gate (MSTest cannot be made to interleave classes on demand).

## 8. Unrelated defects observed (for filing only; not in this item's scope)

None new. Two items already recorded elsewhere were re-observed and need no new issue: `QuickFiler/Controllers/QfcDatamodel.QueueProcessing.cs:17` comment drift (CR-2 of the #950 review, promoted with the 950 residuals to #972 per the branch's recent commit `89f0283c1`), and the `.claude/agent-memory/parallel-orchestrator/*` uncommitted modifications shown in the starting `git status` (session hygiene, not code).

## 9. Testing implications summary

- Regression gate: section 5 test 1 fails on the unmodified fixture with the quoted message and passes after reference counting; evidence under `FEATURE/evidence/regression-testing/`.
- Existing fixture tests R1-R6, #743 and #882 must still pass unchanged in behaviour (R1-R3 are the semantic guard for "installed nothing" and "single pin on null baseline").
- `EmailMoveMonitorTests` (`[DoNotParallelize]`, snapshot/equality on `Current`) is unaffected: counting never writes outside pin acquire/release.
- Theme tests pass with the calls removed (section 3 trace); run them alone and in the concurrent set.
- Coverage: all changes are in test assemblies, which the coverage route excludes from instrumentation; first-party line/branch figures should be unchanged within run-to-run noise. Record baseline and post-change figures per the fail-closed evidence rule.
- Prohibited constructs: no `[DoNotParallelize]`, `Workers=1`, retries, `Thread.Sleep`, `Task.Delay`, temporary files, or timeout changes are needed or proposed.
