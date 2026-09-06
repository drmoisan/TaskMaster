# Research — PR #778 post-merge review residuals (Issue #782)

- Timestamp: 2026-09-05T16-10
- Issue: #782
- Feature folder: `docs/features/active/2026-09-05-pr-778-post-merge-review-residuals-782/`
- Research root (worktree): `C:\Users\DanMoisan\repos\TaskMaster-wt\2026-09-05T10-47`
- Method: Read / Grep / Glob only. No shell, no `git`, no build, no test execution.
  Every claim below is anchored to a file path and a current line number or a quoted identifier.
  Claims that could not be established by static reading are labelled UNVERIFIED with a reason.

---

## 0. Executive summary

- Every one of the five line counts asserted in `issue.md` § Constraints & Risks is **exact**.
- The six `UiThread._dispatcher` reflection sites asserted in `issue.md` are **exact** (four in
  `UtilitiesCS.Test`, two in `QuickFiler.Test`).
- The `git ls-tree` evidence count of 38 asserted for S3-3 is **corroborated** by an independent
  Glob enumeration: 38 files under the #584 `evidence/` tree.
- Five premises are **refuted or materially narrowed**; all are listed under
  § Discrepancies with the requirements source. The two that most affect the plan are:
  1. `ProgressTrackerAsync.InitializeAsync` is an `async` method, so its `InvalidOperationException`
     surfaces **inside the returned task**, not synchronously. `issue.md` § Test Conditions asserts
     the opposite.
  2. S3-5's "three named evidence files" is under-inclusive: **15** of the 37 #584 evidence files
     that carry an `EXIT_CODE:` line deviate from the schema's `EXIT_CODE: <int>`.
- One structural fact changes the toolchain plan: `artifacts/csharp/coverage.xml` must be **JaCoCo**,
  but the repository's coverage pipeline emits **Cobertura**, and no committed converter exists.

---

## A. Production-code facts

### A1. `UtilitiesCS/Threading/UiThread.cs`

**Line count: 172** (Grep line-count over the file). Matches the `issue.md` assertion exactly.

Structure relevant to the findings:

| Element | Location | Current state |
|---|---|---|
| `Init(...)` | `UiThread.cs:19-40` | `public static void Init(bool monitorUiThread = false, Action<LockupAttribution>? onLockupDetected = null, TimeProvider? timeProvider = null, int lockupAttributionThresholdMs = 5000)` |
| Single-shot latch | `UiThread.cs:36` | `if (_loaded.CheckAndSetFirstCall) { Initialize(); }` |
| Latch field | `UiThread.cs:46` | `private static ThreadSafeSingleShotGuard _loaded = new ThreadSafeSingleShotGuard();` — **not** `readonly`, so reassignment is legal |
| `Initialize()` | `UiThread.cs:48-79` | `private static void Initialize()`; shows a hidden `SyncContextForm`, calls `CaptureUiVariables()`, assigns `Dispatcher` at `:61` |
| `Dispatcher` property | `UiThread.cs:135-148` | see verbatim below |
| Backing field | `UiThread.cs:149` | `private static Dispatcher? _dispatcher;` — private static, **not** `volatile`, nullable-annotated, no initializer |
| XML docs | — | **Zero.** Grep for `///` in `UiThread.cs` returns 0 matches across the whole file |

`Dispatcher` getter, verbatim (`UiThread.cs:135-148`):

```csharp
public static Dispatcher Dispatcher
{
    get
    {
        if (_dispatcher is null)
        {
            throw new InvalidOperationException(
                "The UI dispatcher has not been captured. Call UiThread.Init() so that UiThread.Initialize() runs before reading UiThread.Dispatcher."
            );
        }
        return _dispatcher;
    }
    private set => _dispatcher = value;
}
```

Exact current message string (`UiThread.cs:142`), the only one in this file:

```
The UI dispatcher has not been captured. Call UiThread.Init() so that UiThread.Initialize() runs before reading UiThread.Dispatcher.
```

`ThreadSafeSingleShotGuard` (`UtilitiesCS/Threading/ThreadSafeSingleShotGuard.cs:17-28`) exposes
exactly one member, `public bool CheckAndSetFirstCall` (`:24-27`), implemented as
`Interlocked.Exchange(ref _state, CALLED) == NOTCALLED`. **There is no reset method.**

#### Finding-by-finding verdicts

| ID | Verdict | Anchor and note |
|---|---|---|
| **C02** | **CONFIRMED** | `UiThread.cs:139` and `:145` are two separate reads of the non-volatile static `_dispatcher`. The fix `Dispatcher? dispatcher = _dispatcher; if (dispatcher is null) throw ...; return dispatcher;` matches the in-repo precedent at `UtilitiesCS/OutlookObjects/Folder/OutlookFolderTreeService.cs:336` (`var dispatcher = _dispatcher;`) and `QuickFiler/Viewers/WebView2BreadcrumbHost.cs:159` (`BreadcrumbUiDispatcher? dispatcher = _dispatcher;`) — both verified present. |
| **C03** | **CONFIRMED** | `UiThread.cs:36` sets the latch *before* `Initialize()` at `:38`, with no `try`/`catch` and no reset. Because `_loaded` at `:46` is a mutable static, the minimal retry-enabling shape is a `catch { _loaded = new ThreadSafeSingleShotGuard(); throw; }` around `Initialize()`. That re-arm idiom already exists twice in-repo: `UtilitiesCS/Threading/IdleActionQueue.cs:65` and `UtilitiesCS/Threading/ApplicationIdleTimer.cs:454`. Moving `CheckAndSetFirstCall` to *after* `Initialize()` is the wrong remedy — it would let two concurrent callers both run `Initialize()`. |
| **C05** | **CONFIRMED** | `UiThread.cs:117-120` (`UiSyncContext` calls `Init()` when null) and `:160-163` (`AutoScaleFactor` calls `Init()` when null) both self-heal; `Dispatcher` at `:139` throws. No comment explains the asymmetry. The justification is verifiable: `Initialize()` at `:51-54` constructs and `Show()`s a WinForms `SyncContextForm`. |
| **C06** | **CONFIRMED** | The message at `:142` names the private `Initialize()`. The sibling message at `WpfDispatcherYield.cs:65` names only the public `Init()`. The single test assertion on this text is `UiThread_Tests.cs:152` (see § B15). |
| **C08** | **CONFIRMED** | Zero `///` comments anywhere in `UiThread.cs`. |
| **C09 (message part)** | **CONFIRMED** | Neither `Init()` (`:19-40`) nor `Initialize()` (`:48-79`) inspects apartment state. `QuickFiler/Viewers/SyncContextForm.cs:34-40` `CaptureUiVariables()` reads `SynchronizationContext.Current`, `AutoScaleFactor`, `Dispatcher.CurrentDispatcher` and `Thread.CurrentThread.ManagedThreadId` on whatever thread calls it. A worker-thread `Init()` therefore succeeds silently and installs a non-pumping dispatcher. |
| **C11** | **CONFIRMED** | See § B9. |

### A2. `UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs`

**Line count: 77.** Matches the `issue.md` assertion exactly.

**There is exactly ONE `throw` site in this file**, not two (see § Discrepancies, D-1). It is
`WpfDispatcherYield.cs:62-67`:

```csharp
if (dispatcher is null)
{
    throw new InvalidOperationException(
        "The UI dispatcher has not been captured. Call UiThread.Init() before yielding folder tree work."
    );
}
```

C20's "both throws" refers to this one plus `UiThread.cs:141-143`; the two live in different files
but in the **same assembly** (`UtilitiesCS`), which is what makes a single shared constant viable
(see § A5).

The comment C20 calls false, verbatim, `WpfDispatcherYield.cs:53-59`:

```csharp
// Prefer the dispatcher already affinitized to this thread so a traversal that the
// service marshalled onto a captured dispatcher keeps yielding through that same
// dispatcher. Only a worker thread with no dispatcher of its own falls back to the
// process-global UI dispatcher, which is the case Dispatcher.Yield() could not serve.
// UiThread.Dispatcher is set-once state populated by UiThread.Init() and is null
// outside a live host, so that null state is surfaced as InvalidOperationException to
// preserve the strict contract callers relied on.
```

The false clause is at **lines 57-59**: `UiThread.Dispatcher` no longer "is null outside a live
host" — the production fallback provider at `:45-46`
(`_fallbackDispatcherProvider = fallbackDispatcherProvider ?? (() => UtilitiesCS.UiThread.Dispatcher);`)
throws directly, so the local `dispatcher is null` guard at `:62` is unreachable on the production
path and covers only injected providers typed `Func<Dispatcher?>` (`:14-15`).

### A3. `TaskMaster/Ribbon/RibbonViewer.EngineCommands.cs` — C01

Two null-comparison sites, both provably dead now:

| Line | Text | Why dead |
|---|---|---|
| `:71-72` | `var dispatcher = UiThread.Dispatcher;` then `if (dispatcher != null && !dispatcher.CheckAccess())` | `UiThread.Dispatcher` either returns non-null or throws (`UiThread.cs:139-145`); the local can never be null at `:72` |
| `:114-115` | `var dispatcher = UiThread.Dispatcher;` then `if (dispatcher != null && !dispatcher.CheckAccess())` | same |

Two supporting facts for the planner:

- The file carries **no `#nullable enable`** directive (verified: `RibbonViewer.EngineCommands.cs:1-16`
  begins with `using System.Threading.Tasks;`). The always-true comparison therefore raises no
  nullable-flow diagnostic today, and removing it introduces none.
- The whole type is coverage-exempt: `TaskMaster/Ribbon/RibbonViewer.cs:31-33` declares
  `[System.Runtime.InteropServices.ComVisible(true)]`, `[ExcludeFromCodeCoverage]`,
  `public partial class RibbonViewer : Office.IRibbonExtensibility`. The C01 cleanup therefore has
  **zero** coverage impact in either direction.
- Three `UiThread.Dispatcher` mentions in this file are XML-doc prose (`:54`, `:93`) and must not be
  edited by the C01 cleanup.

### A4. `ProgressTracker.cs` / `ProgressTrackerAsync.cs` — C23 and C26

**C23 — exactly two lambda re-read sites**, both confirmed:

| File | Captured local | Lambda re-read | Marshalling call |
|---|---|---|---|
| `UtilitiesCS/Threading/ProgressTrackerAsync.cs` | `:33` `UiDispatcher = UiThread.Dispatcher;` | `:39` `UiDispatcher = UiThread.Dispatcher,` (inside the `ProgressViewer` initializer) | `:35` `await UiDispatcher.InvokeAsync(() => {` |
| `UtilitiesCS/Threading/ProgressTracker.cs` | `:33` `UiDispatcher = UiThread.Dispatcher;` | `:39` `UiDispatcher = UiThread.Dispatcher,` | `:35` `UiDispatcher.Invoke(() => {` (synchronous `Invoke`, not `InvokeAsync`) |

`UiDispatcher` is `internal Dispatcher UiDispatcher` at `ProgressTrackerAsync.cs:87` and
`ProgressTracker.cs:83`, reachable from `UtilitiesCS.Test` through the assembly's
`InternalsVisibleTo` grant (§ A6). One further `InvokeAsync` exists at `ProgressTracker.cs:203`
(`await _progressViewer.UiDispatcher!.InvokeAsync(...)`) but it reads the **viewer's** dispatcher,
not the `UiThread` static, and is out of C23's scope.

**C26 — `InitializeAsync` throw timing.** `ProgressTrackerAsync.cs:31` declares
`public async Task<ProgressTrackerAsync> InitializeAsync()`. The guarded read is at `:33`, the first
statement of the body. Because the method is `async`, the C# compiler captures any exception thrown
before the first suspension point into the returned `Task` rather than propagating it out of the
call. **The `InvalidOperationException` therefore surfaces on `await`, not at the call site.**

Consequence for the test:

```csharp
Func<Task> act = () => tracker.InitializeAsync();
await act.Should().ThrowAsync<InvalidOperationException>();   // correct
// tracker.Invoking(t => t.InitializeAsync()).Should().Throw<...>()  // would FAIL: no synchronous throw
```

By contrast `ProgressTracker.Initialize()` (`ProgressTracker.cs:31`, `public virtual ProgressTracker
Initialize()`) is **not** async and **does** throw synchronously from `:33`. If the plan wants a
synchronous-throw assertion it must target `ProgressTracker.Initialize()`, not
`ProgressTrackerAsync.InitializeAsync()`. See § Discrepancies D-2.

### A5. The shared "not initialized" message constant — recommendation

**No such constant exists today.** The two messages are independent string literals at
`UiThread.cs:142` and `WpfDispatcherYield.cs:65`.

**Recommendation:** introduce one constant on `UiThread`:

- **File:** `UtilitiesCS/Threading/UiThread.cs`
- **Member:** `internal const string DispatcherNotInitializedMessage = "...";`
- **Accessibility:** `internal` — CLAUDE.md C#5.2 ("Prefer `internal` for non-public APIs") and
  C#6.2. `internal` is sufficient for every consumer.
- **`const`, not `static readonly`** — the value is a compile-time literal with no initialization
  order concern, and `const` permits use in attribute arguments and `switch` patterns if the tests
  ever need it. `static readonly` would buy nothing here.
- **Placement:** inside `UiThread`, adjacent to the `Dispatcher` property region
  (`UiThread.cs:135-149`), so the constant and its thrower are read together.

**Reference reachability, verified:**

- `WpfDispatcherYield.cs` lives at `UtilitiesCS/OutlookObjects/Folder/`, i.e. **inside the
  `UtilitiesCS` project** (`TaskMaster.sln:14` maps `UtilitiesCS` to `UtilitiesCS\UtilitiesCS.csproj`).
  Same assembly, so `internal` is directly visible. It already references the type by full name at
  `WpfDispatcherYield.cs:46` (`UtilitiesCS.UiThread.Dispatcher`).
- `UtilitiesCS.Test` can read it through `InternalsVisibleTo` (§ A6), so a test may assert against
  the constant rather than a hard-coded literal.
- `QuickFiler.Test` **cannot** — `UtilitiesCS/Properties/AssemblyInfo.cs` grants IVT only to
  `DynamicProxyGenAssembly2` (`:18`), `UtilitiesCS.Test` (`:19`) and `ToDoModel.Test` (`:20`). No
  QuickFiler.Test assertion on this text exists today (§ B15), so this is not a blocker.

**Alternative considered and rejected:** a new `UtilitiesCS/Threading/UiThreadMessages.cs` holder
class. Rejected because it adds a file and a csproj `<Compile Include>` entry for one string, and
`UiThread.cs` at 172 lines has ample headroom under the 500-line limit even after the C08 XML docs
and the C05 comment are added.

Two message texts must be reconciled by the constant. C06 shortens the `UiThread` text to name only
`Init()`; C09 appends the UI/STA thread requirement; C20 routes the `WpfDispatcherYield` throw
through the same constant. The `WpfDispatcherYield` message today carries a domain-specific tail
("before yielding folder tree work") that a single shared constant necessarily drops. That loss is
intentional per C20 ("production always emits UiThread's message"), but the planner should state it
explicitly in an acceptance criterion so a reviewer does not read it as a regression.

### A6. `InternalsVisibleTo("UtilitiesCS.Test")` — C12/C13 enabler

**CONFIRMED:** `UtilitiesCS/Properties/AssemblyInfo.cs:19`:

```csharp
[assembly: InternalsVisibleTo("UtilitiesCS.Test")]
```

Two duplicate grants of the same name also exist elsewhere in the assembly —
`UtilitiesCS/HelperClasses/Tokenizer.cs:11` and
`UtilitiesCS/OutlookObjects/Item/OlItemSummary.cs:10`. They compile today (the attribute is
`AllowMultiple`), and this delivery should not disturb them.

Adjacent grants in the same file: `DynamicProxyGenAssembly2` (`:18`), `ToDoModel.Test` (`:20`).
There is **no** grant to `QuickFiler.Test` from `UtilitiesCS`.

---

## B. Test-code facts

### B7. Every reflection site on `UiThread._dispatcher` — SIX, confirmed

Enumerated by two independent queries (see § Numeric Derivation Evidence, claim 3).

| # | Assembly | File | `GetField(` line | `"_dispatcher"` line | Missing-field handling |
|---|---|---|---|---|---|
| 1 | UtilitiesCS.Test | `UtilitiesCS.Test/Threading/UiThread_Tests.cs` | `:127` | `:128` | none in helper; caller asserts `field.Should().NotBeNull()` at `:138` in test 1 only, `:164` unguarded in test 2 |
| 2 | UtilitiesCS.Test | `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs` | `:421` | `:422` | `!` null-forgiving at `:424` |
| 3 | UtilitiesCS.Test | `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs` | `:138` | `:139` | `dispatcherField.Should().NotBeNull();` at `:142` |
| 4 | UtilitiesCS.Test | `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs` | `:144` | `:145` | none; consumed by `ForceDispatcherNull` (`:165-171`) / `RestoreDispatcher` (`:184-187`) |
| 5 | QuickFiler.Test | `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs` | `:40` | `:41` | `?.` null-conditional at `:55` and `:64` — the C18 defect |
| 6 | QuickFiler.Test | `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs` | `:135` | `:136` | `field.Should().NotBeNull(because: "UiThread._dispatcher backing field must exist");` at `:139` |

**The `issue.md` count of six, four of them in UtilitiesCS.Test, is exactly correct.** The review
body's line numbers (`:125`, `:421`, `:138`, `:144`, `:40`, `:135`) name the `GetField(` call lines
and also match.

Two adjacent facts the planner needs:

- There is **one further reflection site on a different `UiThread` static**:
  `UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs:469-472` reads
  `typeof(UiThread)` then `GetField("_uiSyncContext", NonPublic|Static)`. It is **out of scope** for
  C12/C13 (different field) but is the reason `policy-audit.2026-09-04T04-05.md:348-351` claims the
  parallel-bucket isolation "holds partly by coincidence".
- The `using static UtilitiesCS.UiThread` route and the reflective `GetProperty("Dispatcher")` route
  each have **zero hits** repo-wide (Grep for `using static .*UiThread|GetProperty\(\s*"Dispatcher"|nameof\(UiThread`
  across `**/*.cs` returns "No matches found"). The PR body's census claim holds at HEAD.

**Recommended C12/C13 landing site:** a new file
`UtilitiesCS.Test/TestHelpers/UiThreadDispatcherScope.cs`, registered in the csproj alongside the
two existing entries at `UtilitiesCS.Test/UtilitiesCS.Test.csproj:74-75`
(`TestHelpers\ManualFireInnerTimer.cs`, `TestHelpers\ManualFireTimerWrapper.cs`). Rationale: it keeps
the reflection out of production code (CLAUDE.md C#5.2), mirrors the already-reviewed
`QuickFiler.Test` fixture, and the `issue.md` C12/C13 wording explicitly permits it
("on `UiThread` (or under `UtilitiesCS.Test/TestHelpers/`)"). The `internal`-seam alternative on
`UiThread` itself is viable through the IVT grant but adds a test-only member to a production type.

### B8. `UiThreadDispatcherFixture` and the C18/C25 targets

**Fixture** — `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs`,
namespace `QuickFiler.Controllers.Tests` (`:9`), `internal static class UiThreadDispatcherFixture`
(`:29`).

Public surface:

| Member | Line | Signature |
|---|---|---|
| `Current` | `:40-49` | `internal static Dispatcher Current { get { lock (FieldLock) { return (Dispatcher)DispatcherField.GetValue(null); } } }` |
| `Exchange` | `:55-63` | `internal static Dispatcher Exchange(Dispatcher replacement)` |
| `CompareExchange` | `:70-82` | `internal static bool CompareExchange(Dispatcher expected, Dispatcher restoreTo)` |
| `ReleaseTransactionGate` | `:88-91` | `internal static void ReleaseTransactionGate()` |
| `EnsureDispatcher` | `:99-115` | `internal static IDisposable EnsureDispatcher()` |
| `BeginTransactionAsync` | `:122-126` | `internal static async Task<UiThreadDispatcherTransaction> BeginTransactionAsync()` |

**`Current` is exactly what C18 needs.** It is `internal` in the same assembly as
`EmailMoveMonitorTests`, so no new grant is required. Two mechanical notes:

- `EmailMoveMonitorTests.cs` is in namespace `QuickFiler.Helper_Classes.Tests` (`:13`), so the
  migration needs `using QuickFiler.Controllers.Tests;` or a qualified reference.
- `Current` returns `System.Windows.Threading.Dispatcher`, not `object`. The snapshot field
  `private object _capturedDispatcher;` (`:38`) can either be retyped to `Dispatcher` (WindowsBase is
  already referenced — `QuickFiler.Test/QuickFiler.Test.csproj:460` `<Reference Include="WindowsBase" />`)
  or left as `object`. Retyping is the cleaner outcome once the C25 comments are deleted.
- The rename-safety property C18 wants comes from the fixture's `ResolveDispatcherField()`
  (`:133-141`), whose `field.Should().NotBeNull(because: ...)` at `:139` runs inside a **static field
  initializer** (`:34`). A renamed field therefore raises `TypeInitializationException` and **fails**
  the tests, instead of the current silent `null == null` pass.

**C18/C25 edit targets in `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs`** (320 lines):

| Item | Lines | Verbatim |
|---|---|---|
| `FieldInfo` declaration | `:39-43` | `private static readonly System.Reflection.FieldInfo DispatcherField = typeof(UiThread).GetField("_dispatcher", System.Reflection.BindingFlags.NonPublic \| System.Reflection.BindingFlags.Static);` |
| `?.` read 1 (Setup) | `:55` | `_capturedDispatcher = DispatcherField?.GetValue(null);` |
| `?.` read 2 (Cleanup) | `:64` | `object current = DispatcherField?.GetValue(null);` |
| WindowsBase fragment 1 | `:29` | `// (avoiding a compile-time WindowsBase dependency on System.Windows.Threading.Dispatcher)` |
| WindowsBase fragment 2 | `:53` | `// Snapshot the static UiThread.Dispatcher (reflectively, to avoid WindowsBase) so` |

The class is `[TestClass]` `[DoNotParallelize]` at `:21-22`; the snapshot field is
`private object _capturedDispatcher;` at `:38`; the assertion is
`current.Should().BeSameAs(_capturedDispatcher);` at `:65`. The accurate paragraph the PR appended
sits at `:33-37` and must be retained.

### B9. `UtilitiesCS.Test/Threading/UiThread_Tests.cs` — C10, C11, C06

File is **179 lines** and holds two classes:
`SynchronizationContextAwaiter_Tests` (`:9-104`, `[TestClass]` only) and
`UiThread_Dispatcher_Tests` (`:121-178`, `[TestClass]` at `:121` and `[DoNotParallelize]` at `:122`
— already two separate attributes).

**C10 — the pooled-MTA sentinel**, verbatim (`UiThread_Tests.cs:160-177`):

```csharp
[TestMethod]
public void Dispatcher_WhenBackingFieldIsPopulated_ReturnsThatSameInstance()
{
    // Arrange
    var field = DispatcherField();
    var prior = field.GetValue(null);
    var expected = System.Windows.Threading.Dispatcher.CurrentDispatcher;
    field.SetValue(null, expected);
    try
    {
        // Act / Assert
        UiThread.Dispatcher.Should().BeSameAs(expected);
    }
    finally
    {
        field.SetValue(null, prior);
    }
}
```

`Dispatcher.CurrentDispatcher` is called at **`:166`** inside a plain `[TestMethod]`, i.e. on a
pooled MTA MSTest worker, and is never shut down. `UtilitiesCS.Test/test.runsettings:2-6` documents
the opt-in model verbatim: "Global STA execution is intentionally disabled. Tests that require an STA
apartment must opt in with MSTest's STATestMethod or STATestClass attributes...".

Two in-repo STA-host patterns are available for the fix, both verified:

- `UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs:172-199` — `StaDispatcherHost`,
  a `private sealed class ... : IDisposable` that starts a background STA thread, captures
  `Dispatcher.CurrentDispatcher`, runs `Dispatcher.Run()`, and in `Dispose()` calls
  `Dispatcher.BeginInvokeShutdown(DispatcherPriority.Send); _thread.Join(); _ready.Dispose();`.
  This is the closest match to what C10 asks for.
- `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs:132-199` — an inline
  `new Thread(...)`/`SetApartmentState(ApartmentState.STA)`/`Start()`/`Join()` with exception capture
  into `Exception threadException` and a trailing `threadException.Should().BeNull(...)`.

**C11** — `DispatcherField()` at `:125-131` returns the raw `FieldInfo` with no guard. Test 1
(`:133-158`) asserts `field.Should().NotBeNull();` at `:138`; test 2 (`:160-177`) calls
`field.GetValue(null)` at `:165` unguarded. Test 1 uses a block-bodied lambda at `:144-147`:

```csharp
Action act = () =>
{
    _ = UiThread.Dispatcher;
};
```

**Every assertion on the exception message text in this file:** exactly one, at `:150-152`:

```csharp
act.Should()
    .Throw<InvalidOperationException>()
    .WithMessage("*UiThread.Initialize()*");
```

The method name itself also encodes the old contract:
`Dispatcher_WhenBackingFieldIsNull_ThrowsInvalidOperationExceptionNamingInitialize` (`:134`). If
C06 renames the target from `Initialize()` to `Init()`, the method name becomes misleading; the
plan should decide explicitly whether to rename it (renaming changes the fully-qualified test name
recorded in the #584 evidence artifacts).

### B10. `IdleAsyncQueue_Tests.cs` (C19) and `IdleActionQueue_Tests.cs` (C14)

**C19 — three passages in `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs` (348 lines):**

| Item | Lines | Verbatim |
|---|---|---|
| P27-T2 docstring | `:236-240` | `///     One entry is added with useUiThread=true. UiThread.Dispatcher is null` / `///     in the test environment (no WinForms/WPF message loop). When InvokeAsync` / `///     is called on a null Dispatcher, the NullReferenceException is caught by` / `///     the internal try/catch in OnApplicationIdle, which is the expected` / `///     production fault-isolation behaviour.` |
| Act comment | `:266-267` | `// Act: InvokeOnIdle triggers the Dispatcher-routing branch; null Dispatcher` / `// causes NullReferenceException that is caught internally.` |
| `NotThrow` reason | `:272` | `"exceptions after the await in the Dispatcher path are caught by the internal try/catch"` |

The correct mechanism, verified in production source: `UtilitiesCS/Threading/IdleAsyncQueue.cs:72`
reads `UiThread.Dispatcher` inside the `try` opened at `:68` and **before** the first `await`
completes, so the getter throws `InvalidOperationException` synchronously; it is swallowed by
`catch (Exception ex)` at `:83`. The entry is dequeued at `:65`, before the `try`, which is why the
`Count == 0` assertion at `:276-278` still holds.

A fourth passage in the same file also describes the pre-#778 world and is arguably in C19's spirit:
`:155-160` ("If any earlier test in this assembly triggers UiThread.Initialize(), Dispatcher becomes
non-null..."). It is not factually wrong, so leaving it is defensible; flag it in the plan as a
decision rather than an omission.

**C14 — `UtilitiesCS.Test/Threading/IdleActionQueue_Tests.cs` (241 lines).**

- Class declaration: `[TestClass]` at `:24`, `public class IdleActionQueue_Tests` at `:25`.
  **No `[DoNotParallelize]`.**
- **There is no `[TestCleanup]` and no `[TestInitialize]`.** Confirmed by Grep over
  `UtilitiesCS.Test/Threading/` — only `AppGlobalsConverterTests*.cs` and
  `ApplicationIdleTimer_Tests.cs` declare those attributes in that folder.
- Existing private helper `ResetStaticState()` at `:39-69` is called at the **start** of each of the
  three tests (`:132`, `:163`, `:207`) but never after.

State a new `[TestCleanup]` must drain, with exact members (production source
`UtilitiesCS/Threading/IdleActionQueue.cs`):

| Member | Production declaration | Cleanup action |
|---|---|---|
| `_entries` | `:45` `private static ConcurrentQueue<Action>? _entries;` | set to `null` (lazily recreated by the `Entries` getter at `:46-53`) or drain |
| `_subscribeGuard` | `:55` `private static ThreadSafeSingleShotGuard _subscribeGuard = new ThreadSafeSingleShotGuard();` | replace with a fresh guard |
| `_unsubscribe` | `:57-67` `private static TimedBatchAction _unsubscribe = new(TimeSpan.FromSeconds(3), () => {...});` | `CancelAction()`, then null `TimedBatchAction._timer` |
| **heartbeat subscription** | `:37` `ApplicationIdleTimer.Subscribe(OnApplicationIdle);` inside `AddEntry`, handler `:69` `private static async void OnApplicationIdle(ApplicationIdleTimer.ApplicationIdleEventArgs e)` | `ApplicationIdleTimer.Unsubscribe(handler)` where the handler is rebuilt via `Delegate.CreateDelegate(typeof(ApplicationIdleTimer.ApplicationIdleEventHandler), typeof(IdleActionQueue).GetMethod("OnApplicationIdle", NonPublic\|Static))` |

`ApplicationIdleTimer.Subscribe` / `Unsubscribe` are `public static` at
`UtilitiesCS/Threading/ApplicationIdleTimer.cs:465-478`; the delegate type is
`public delegate void ApplicationIdleEventHandler(ApplicationIdleEventArgs e)` at `:83`.

**Risk the planner must weigh:** `Unsubscribe` at `:471-478` calls `Stop()` when the invocation list
empties, and `Stop()` (`:451-455`) calls `instance.StopTimer()` — which touches
`System.Windows.Forms.Application.Idle` and resets `ApplicationIdleTimer.Guard`. That is
process-global state shared with `IdleAsyncQueue_Tests` and `ApplicationIdleTimer_Tests`.
`ApplicationIdleTimer_Tests` already defends itself: it is `[TestClass]` + `[DoNotParallelize]`
(`:16-17`) with a `TestInitialize`/`TestCleanup` pair (`:20-30`) both calling `ResetSingletonState()`
(`:32-42`, which itself calls `ApplicationIdleTimer.Stop()`), and its file header comment
(`:10-15`) explains that the static event backing field is shared with `IdleAsyncQueue` and
`IdleActionQueue`. **Recommendation:** if C14's cleanup unsubscribes, add `[DoNotParallelize]` to
`IdleActionQueue_Tests` in the same edit, matching the precedent.

### B11. `ProgressTracker_Tests.cs` — C16 split and C15

**Exact current line count: 514.** Matches `issue.md`. (The count is unchanged from the #584
post-format record at `evidence/qa-gates/p4-t1-format.md:82`.)

**C15 target:** `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs:14`
`    [TestClass, DoNotParallelize]` — the only comma-combined form among the Threading test classes.
Every sibling uses two lines (`IdleAsyncQueue_Tests.cs:28-29`, `ProgressTrackerAsync_Tests.cs:13-14`,
`UiThread_Tests.cs:121-122`, `ApplicationIdleTimer_Tests.cs:16-17`, `TimeOutTask_Tests.cs:9-10`).

**Shared state that both halves need — exactly one member:**

- `private sealed class CapturingProgressTracker : ProgressTracker` at `:81-95` (15 lines), used by
  **every** test method in the file.
- **No** `[TestInitialize]`, **no** `[TestCleanup]`, **no** instance or static fields.

**Recommended split shape: `partial class`, not a base class and not two classes.**
Repo precedent is direct and current: `TimeOutTask_Tests` is split across four files —
`TimeOutTask_Tests.cs:9-11` carries `[TestClass]` / `[DoNotParallelize]` / `public partial class
TimeOutTask_Tests`, while `TimeOutTask_AdditionalTests.cs:10`,
`TimeOutTask_InternalCoverageTests.cs:9` and `TimeOutTask_OverloadCoverageTests.cs:9` each declare
`public partial class TimeOutTask_Tests` with **no attributes**. Applying `[TestClass]` to two parts
of the same partial class is a compile error (`AllowMultiple = false`), so the attributes must stay
on one part only. `partial` also preserves every fully-qualified test name, which two separate
classes would not.

**Concrete split:**

*File A — `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs` (kept name), ~271 lines.*
Retains current lines 1-268 plus the two closing braces, with `:14` expanded to two attribute lines
and the class declared `public partial class ProgressTracker_Tests`. Holds 17 tests:

1. `Increment_ShouldUpdateProgressAndForwardScaledValueAndJobName` (`:17-31`)
2. `Report_ShouldClampValuesAboveOneHundred` (`:33-47`)
3. `Report_ShouldThrowForNegativeValues` (`:49-61`)
4. `SpawnChild_ShouldUseRemainingAllocationFromCurrentProgress` (`:63-79`)
   — plus `CapturingProgressTracker` (`:81-95`)
5. `Increment_ShouldAccumulateProgressValues` (`:99-110`)
6. `Increment_ShouldClampAt100` (`:112-122`)
7. `Report_WithTupleOverload_ShouldSetValueAndJobName` (`:124-134`)
8. `Report_DoubleOverload_ShouldThrowForNegative` (`:136-145`)
9. `Report_DoubleOverload_ShouldClampAbove100` (`:147-156`)
10. `SpawnChild_WithAllocation_ShouldCreateChildWithSpecifiedAllocation` (`:158-168`)
11. `SpawnChild_WithDoubleAllocation_ShouldRoundAndCreateChild` (`:170-180`)
12. `Report_WithDoubleAndJobName_ShouldClampAt100` (`:182-191`)
13. `Report_WithDoubleAndJobName_ShouldThrowForNegative` (`:193-202`)
14. `Constructor_WithParent_ShouldInheritJobName` (`:204-211`)
15. `Report_WithJobName_RootReportsToStubPane` (`:217-230`)
16. `SpawnChild_FromProgressedParent_MapsChildProgressIntoParentRange` (`:232-249`)
17. `Report_At100Percent_SetsProgressToMaxAndForwardsToParent` (`:251-266`)

*File B — new, `UtilitiesCS.Test/Threading/ProgressTracker_ReportAndViewerTests.cs`, ~260 lines.*
Takes the whole `#region P74 — ProgressTracker core Report/child/root-close behaviour` block,
current lines 270-512. Holds 7 tests:

1. `Report_WithValueAndJobName_UpdatesProgressAndForwardsMessage` (`:290-304`)
2. `Report_ViaChild_ShiftsParentProgressByAllocatedRange` (`:325-344`)
3. `Report_At100Percent_WhenRootTracker_ClosesProgressViewer` — `[STATestMethod]` (`:366-409`)
4. `Initialize_WithCurrentDispatcherAndScreen_InitializesViewerAndUpdatesUi` — `[STATestMethod]`,
   holds the `_dispatcher` reflection site (`:411-453`)
5. `ReportAsync_WithNegativeValue_ThrowsArgumentOutOfRangeException` (`:455-464`)
6. `ReportAsync_WithValueOver100_ClampsTo100` (`:466-476`)
7. `ReportAsync_At100Percent_WhenRootTracker_ClosesProgressViewer` — `[STATestMethod]` (`:478-510`)

**Estimation method (stated so the planner can reproduce it):** exact arithmetic over the current
file. File A = 268 retained source lines + 2 closing braces + 1 line from expanding the combined
attribute = **271**. File B = 243 moved source lines (270-512 inclusive) + a 15-line preamble
(10 `using` directives copied from `:1-10`, one blank line, `namespace UtilitiesCS.Test`, `{`,
`    public partial class ProgressTracker_Tests`, `    {`) + 2 closing braces = **260**. Both are
well under 500 with >200 lines of headroom, so a CSharpier re-wrap cannot push either over.

Notes for sequencing:

- File B needs `using System.Reflection;`, `using System.Windows.Forms;` (for `Screen`,
  `FormStartPosition`) and `using System.Windows.Threading;` (for `Dispatcher`). `ProgressViewer` is
  in namespace `UtilitiesCS` (`UtilitiesCS/Threading/ProgressViewer.cs:14`), which resolves from
  `namespace UtilitiesCS.Test` by enclosing-namespace lookup; the existing `using UtilitiesCS;` at
  `:9` covers it either way.
- `[STATestMethod]` has **no definition in this repository** (Grep for `STATestMethodAttribute`
  returns no matches). It ships with `MSTest.TestAdapter`/`MSTest.TestFramework`, pinned at
  `4.4.0` in `UtilitiesCS.Test/packages.config:146`, and resolves from the existing
  `Microsoft.VisualStudio.TestTools.UnitTesting` using. No new using is required.
- If C12/C13 lands **first**, `ProgressTracker_Tests.cs` shrinks by roughly six lines (the
  `dispatcherField` block at `:421-426`, `:432`, `:450`) to ~508 — still over 500, so the split is
  required regardless of ordering.

### B12. `UtilitiesCS.Test/UtilitiesCS.Test.csproj` — where to register the new files

The `Threading\` block of the `<Compile>` `ItemGroup`, lines **473-498**, verbatim:

```xml
    <Compile Include="ThemeHelpers\SystemThemeDetectorTests.cs" />
    <Compile Include="Threading\AppGlobalsConverterTests.cs" />
    <Compile Include="Threading\AppGlobalsConverterTests_Unfinished.cs" />
    <Compile Include="Threading\ProgressPackage_Tests.cs" />
    <Compile Include="Threading\ProgressTracker_Tests.cs" />
    <Compile Include="Threading\ProgressTrackerPane_Tests.cs" />
    <Compile Include="Threading\ProgressTrackerAsync_Tests.cs" />
    <Compile Include="Threading\TaskPriority_Tests.cs" />
    ...
    <Compile Include="Threading\UiThread_Tests.cs" />
```

(`ProgressTracker_Tests.cs` is at **:477**; `IdleActionQueue_Tests.cs` at **:489**;
`IdleAsyncQueue_Tests.cs` at **:490**; `UiThread_Tests.cs` at **:494**.)

Conventions observed:

- Four-space indent, single self-closing `<Compile Include="..." />` element per line.
- Windows backslash separators, path relative to the project directory.
- Ordering is **grouped by folder but not alphabetical within the group** (`ProgressTrackerPane_Tests`
  precedes `ProgressTrackerAsync_Tests`; `ApplicationIdleTimer_Tests` at `:488` follows the
  `TimeOutTask*` entries). New entries are appended adjacent to their sibling, not sorted.
- The `TestHelpers\` entries sit at **:74-75**, far from the `Threading\` block, so a new
  `TestHelpers\UiThreadDispatcherScope.cs` entry belongs there.
- **Duplicate `<Compile Include>` entries are a known past defect in this project**
  (`docs/features/archive/2026-08-10-utilitiescs-test-cs2002-duplicate-compile-entry-394/`), so the
  plan should assert exactly one entry per new file.

### B13. `QfcItemController.InitializationTests.Part2.cs` — S2-1

**Line count: 393.** Matches `issue.md`.

The Arrange comment, verbatim, `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs:121-127`:

```csharp
// QfcTipsDetails.ToggleAsync marshals through the process-wide static
// UtilitiesCS.UiThread.Dispatcher. In production that is the live UI thread's
// dispatcher; in this assembly it is either unset or the deliberately parked instance
// from QfcItemControllerTestSupport.EnsureUiThreadDispatcher, neither of which can
// complete an InvokeAsync. Point it at the pump thread's dispatcher (serviced by the
// WinForms loop, proven by WinFormsPumpHostTests.BothMarshalRoutes_*) for the duration
// of the test, and restore the previous value in PumpHarness.Restore so no state leaks.
```

The false clause is `neither of which can complete an InvokeAsync` (`:124-125`). Post-#778 the
*unset* case does not reach `InvokeAsync` at all — the getter throws `InvalidOperationException`
first. The *parked* case is still accurately described (a real dispatcher that never pumps). The
correction must preserve that distinction rather than replacing the whole sentence.

Two further `UiThread.Dispatcher` mentions in this file are unaffected: `:52` and `:308`.

### B14. `WpfDispatcherYieldTests` — C21

- File: `UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs`, 201 lines.
- Declaration: `[TestClass]` at `:12`, `public sealed class WpfDispatcherYieldTests` at `:13`.
  **It is NOT `[DoNotParallelize]`.**
- The class is `sealed`, so a new test must either go inside it or into a separate class.
- **No existing test in this class nulls or restores the `UiThread` static.** All four tests
  (`:15-50`, `:52-82`, `:84-115`, `:117-142`) construct `new WpfDispatcherYield(threadProvider.Provide,
  fallbackProvider.Provide)` through the `internal` two-provider constructor
  (`WpfDispatcherYield.cs:37-47`) and never touch `UiThread`. `YieldAsync_WithoutDispatcher_RemainsStrict`
  (`:117-142`) asserts `ThrowAsync<InvalidOperationException>()` at `:131-134` **without** a
  `WithMessage`, which is the assertion C20 asks to strengthen.
- Reusable helpers already in the file: `CountingDispatcherProvider` (`:148-165`) and
  `StaDispatcherHost` (`:172-199`).

**Design note for the new C21 test.** It must reach the production fallback provider, i.e. construct
`new WpfDispatcherYield()` (the parameterless ctor at `:21-22`) on a thread whose
`Dispatcher.FromThread(Thread.CurrentThread)` is null, with `UiThread._dispatcher` nulled. On a
pooled MSTest worker, `Dispatcher.FromThread` returns non-null if any earlier test on that same
thread ever called `Dispatcher.CurrentDispatcher` — which is exactly the C10 hazard. The test must
therefore run its Act on a **dedicated fresh thread that never touches `CurrentDispatcher`**, and
join it, to be deterministic. `[DoNotParallelize]` alone does not remove that coupling.

### B15. Grep discipline for the message change (C06/C09)

**`UiThread.Initialize()` — 5 occurrences repo-wide, all `*.cs`:**

| File:line | Kind | Breaks on a message change? |
|---|---|---|
| `UtilitiesCS/Threading/UiThread.cs:142` | the message literal itself | it *is* the change |
| `UtilitiesCS.Test/Threading/UiThread_Tests.cs:152` | `.WithMessage("*UiThread.Initialize()*")` | **YES — the only breaking assertion** |
| `UtilitiesCS.Test/Threading/UiThread_Tests.cs:113` | XML doc prose | no (stale, optional edit) |
| `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs:156` | XML doc prose | no |
| `UtilitiesCS.Test/OutlookObjects/Folder/WpfDispatcherYieldTests.cs:122` | `// Arrange` comment | no |

**`UiThread.Init` — 7 occurrences repo-wide:**

| File:line | Kind |
|---|---|
| `TaskMaster/ThisAddIn.cs:35` | live production call, `UiThread.Init(monitorUiThread: true, onLockupDetected: ..., timeProvider: TimeProvider.System)` (`:35-40`) |
| `QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncTests.cs:329` | live test call, `UiThread.Init(false);` |
| `QuickFiler.Test/Controllers/QfcHomeControllerTests.cs:170` | commented out: `//    UiThread.Init(false);` |
| `UtilitiesCS/Threading/UiThread.cs:142` | inside the message literal |
| `UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs:57` | comment prose |
| `UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs:65` | inside the message literal |
| `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468Tests.cs:106` | XML doc prose |

**Conclusion:** the C06/C09 message change breaks exactly **one** assertion,
`UiThread_Tests.cs:152`. There is no assertion on the `WpfDispatcherYield` message text anywhere;
C20's proposed `WithMessage("*UiThread.Init()*")` at `WpfDispatcherYieldTests.cs:131-134` would be
the first, and it must be authored **after** the shared constant is in place so the wildcard matches.

### B16. Line counts asserted in `issue.md` § Constraints & Risks

| File | `issue.md` asserts | Measured | Verdict |
|---|---|---|---|
| `UtilitiesCS/Threading/UiThread.cs` | 172 | **172** | exact |
| `UtilitiesCS/OutlookObjects/Folder/WpfDispatcherYield.cs` | 77 | **77** | exact |
| `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs` | 320 | **320** | exact |
| `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs` | 393 | **393** | exact |
| `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs` | 514 | **514** | exact |

Measurement method: `rg --count '.*'` semantics via the Grep tool (counts every line, including
blank lines; a trailing newline adds no phantom line). No discrepancies.

Additional counts the plan will touch: `UiThread_Tests.cs` **179**, `IdleAsyncQueue_Tests.cs`
**348**, `ProgressTrackerAsync_Tests.cs` **206**, `IdleActionQueue_Tests.cs` **241**,
`WpfDispatcherYieldTests.cs` **201**, `QfcItemController.UiThreadDispatcherFixture.cs` **278**.

---

## C. Documentation and evidence facts (#584 feature folder)

**Confirmed path** (Glob):
`docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/`

Contents: 7 top-level documents (`issue.md`, `spec.md`, `plan.2026-09-02T09-02.md`,
`code-review.2026-09-04T04-05.md`, `feature-audit.2026-09-04T04-05.md`,
`policy-audit.2026-09-04T04-05.md`, `research/defect-scoping.2026-09-02T09-02.md`) plus **38** files
under `evidence/`.

### S3-1 — ordering prose contradicted by the recorded timestamps

| Artifact | Line | Verbatim text to soften | Contradicting timestamp |
|---|---|---|---|
| `evidence/regression-testing/p1-t4-expect-fail.md` | `:48` | "P1-T3 recorded a clean `0 Error(s)` build immediately before this run" | this file `Timestamp: 2026-09-03T08-31` (`:3`); `p1-t3-build-before-fix.md:3` is `2026-09-03T08-33` — two minutes **later** |
| `evidence/qa-gates/p3-t1-analyzer-build.md` | `:30-31` | "This is the first build that compiles P1-T5's three attribute-only edits together with P2-T1's production fix" | this file `Timestamp: 2026-09-03T08-38` (`:3`); `p3-t2-regression-green.md:3` is `2026-09-03T08-34` and `p3-t3-at-risk-tests.md:34` records a TRX mtime of `2026-09-03 08:35:42.461615800 -0400` — both **earlier** |
| `feature-audit.2026-09-04T04-05.md` | `:37-39` | "The build immediately preceding it (`p1-t3-build-before-fix.md`) was clean, so this is an assertion-level RED, not a compile error" | same as row 1 |
| `policy-audit.2026-09-04T04-05.md` | `:115` | "...against a tree that `p1-t3-build-before-fix.md` had just built with `0 Error(s)`" | same as row 1 |

**Recommended replacement wording** (same claim, no ordering assertion):
"P1-T3 recorded a clean `0 Error(s)` build of the same tree state, so this is an assertion-level RED
rather than a compile failure. The two artifacts' recorded `Timestamp:` values do not establish
their relative execution order, and the RED does not depend on it: the sibling positive test passed
in the same run."

Supporting fact for the artifact note: `.claude/skills/evidence-and-timestamp-conventions/SKILL.md:109`
specifies only `Timestamp: <ISO-8601>` and **defines no semantics** for which instant it denotes.
The skill lives under `.claude/`, which is push-down-owned; the definition request is correctly
out of scope per `issue.md:112-114`.

### S3-2 — the formatter-command misstatement

**What was actually run** — `evidence/qa-gates/p4-t1-format.md:8`, verbatim:

```
env -C <worktree-root> dotnet tool run csharpier format UtilitiesCS/Threading/UiThread.cs UtilitiesCS.Test/Threading/UiThread_Tests.cs UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs "QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs"
```

Six explicit paths. The artifact itself is explicit about the scoping at `:12-13` and `:73`
(`RESTORED_UNOWNED_FORMAT_DRIFT: NOT APPLICABLE (formatter write scope restricted to the six owned paths)`).

**What the audits claim:**

| Artifact | Line | Verbatim cell |
|---|---|---|
| `policy-audit.2026-09-04T04-05.md` | `:229` | `\| Format (apply) \| \`dotnet tool run csharpier format .\` \| exit 0, \`Formatted 6 files\` \| \`p4-t1-format.md\` \|` |
| `feature-audit.2026-09-04T04-05.md` | `:149` | `\| 1. Format \| \`dotnet tool run csharpier format .\` \| exit 0, \`Formatted 6 files\`, identical before/after unscoped porcelain \| \`p4-t1-format.md\` \|` |
| `policy-audit.2026-09-04T04-05.md` | `:421` | Appendix B "Toolchain Commands Reference": `1. dotnet tool run csharpier format .` |

**Row 3.1** is `policy-audit.2026-09-04T04-05.md:123`, verdict `PASS`, evidence cell
"`p4-t1-format.md` `EXIT_CODE: 0`, `Formatted 6 files`, byte-identical before/after unscoped
porcelain. `p4-t2-format-check.md` `EXIT_CODE: 0`, `Checked 1576 files`, empty reported set, run
over `.` (full repo, CI parity)." The cell does not itself misquote the command; what it omits is
the deviation from the CLAUDE.md approved-command list.

**Section 8** begins at `policy-audit.2026-09-04T04-05.md:244` (`## 8. Gaps and Exceptions`), first
entry `### B1` at `:246`. A new gap entry belongs there, citing the plan's P4-T1 rationale
(lines 1068-1084 of `plan.2026-09-02T09-02.md` per the review body — UNVERIFIED, not re-read here)
and the `p4-t2` whole-tree `check .` mitigation as substantively equivalent.

Recommended corrections: change the two command cells to the scoped six-path form; leave Appendix B
line 421 as the *reference* command but label it "CLAUDE.md reference commands, not a transcript of
what ran"; append row 3.1 with "deviation from the approved `format .` invocation disclosed in
section 8"; add the section 8 entry.

### S3-3 — the "34 evidence artifacts" claim

**Location:** `policy-audit.2026-09-04T04-05.md:68`, verbatim:
"All 34 evidence artifacts for this feature are under the canonical".

**My independent Glob count: 38.** Method and full member set in
§ Numeric Derivation Evidence, claim 2. The count agrees with the `git ls-tree` figure of 38
asserted in `issue.md:84` and in the review body. It also reconciles with the PR body's "45
documentation and evidence files under the feature folder" (38 evidence + 7 top-level documents).

**Recommended replacement:** "All 38 evidence artifacts for this feature are under the canonical".

### S3-4 — filename / `Timestamp:` mismatch

- **Filename:** `evidence/issue-updates/issue-584.2026-09-02T09-02.md` — the timestamp segment is
  `2026-09-02T09-02`, which is the plan's timestamp (`plan.2026-09-02T09-02.md`).
- **In-file value:** line `:3` reads `Timestamp: 2026-09-03T22-24`.
- The file also records `PostedAs: comment` (`:5`) and a live comment URL (`:7`), so it is a
  genuine posted mirror, not a draft.
- The skill's naming rule is `<FEATURE>/evidence/issue-updates/issue-<N>.<timestamp>.md`
  (`.claude/skills/evidence-and-timestamp-conventions/SKILL.md:165`) and does not say which instant
  `<timestamp>` denotes, which is why the mismatch was possible.

**Recommended in-place note**, inserted immediately after line `:3`, renaming nothing and altering
no existing value:

```markdown
> Naming note (added 2026-09-05, issue #782): this file's name carries the plan's timestamp
> (`2026-09-02T09-02`), while its `Timestamp:` field records when the comment was posted
> (`2026-09-03T22-24`). The file is committed evidence and is deliberately neither renamed nor
> re-stamped. A future update to issue #584 must use its own posting timestamp in the filename
> (`issue-584.<posting-timestamp>.md`) so the two artifacts sort correctly and cannot collide.
```

### S3-5 — `EXIT_CODE:` normalization

The schema requires `EXIT_CODE: <int>`
(`.claude/skills/evidence-and-timestamp-conventions/SKILL.md:111`).

**The three files S3-5 names, with their current values:**

| File | Line | Current value |
|---|---|---|
| `evidence/baseline/p0-t6-mcp-probe.md` | `:12` | `EXIT_CODE: non-zero (tool invocation error; no exit code is returned by the MCP transport)` |
| `evidence/qa-gates/p1-t5-donotparallelize.md` | `:11-13` | `EXIT_CODE:` then `- command 1 — 0` / `- command 2 — 0` |
| `evidence/qa-gates/p3-t5-no-timing-tokens.md` | `:12-16` | `EXIT_CODE:` then three bullets, the third being `- the two-stage \`grep\` pipeline — 1 (the exit code of the second \`grep\`, which is what \`grep\` returns when it finds no match)` |

**But 15 files deviate, not 3.** Full enumeration in § Numeric Derivation Evidence, claim 4. This
is a scope discrepancy the planner must resolve before writing AC3 (see § Discrepancies D-3).

A design note the planner needs for `p3-t5`: the artifact's real exit code is `1` and that is the
*expected* outcome for a no-match grep gate. The skill provides the exact mechanism for this at
`SKILL.md:113-124`: write `EXIT_CODE: 1` plus `ExpectedExitCode: 1`, which the collector normalizes
to `pass`. That is the correct normalization for `p3-t5` rather than inventing a `0`. For
`p0-t6-mcp-probe.md`, no process ran at all; the honest normalization is a single integer plus a
prose line below it recording that the MCP transport returned no exit code.

### S3-6 — `spec.md` Status and the three disagreeing file lists

| Item | Location | Current text |
|---|---|---|
| Status | `spec.md:7-11` | `- **Status:** Draft (amended in plan revision round 15: write set and AC4 extended to a sixth file; amended in plan revision round 16: AC5 returned to unchecked pending the sixth file's token-filter artifact; amended in plan revision round 17 ...)` |
| List 1 — "In scope" | `spec.md:62-69` | **three** files: `UtilitiesCS/Threading/UiThread.cs`; "A new deterministic regression test in `UtilitiesCS.Test/Threading/UiThread_Tests.cs`"; `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs` |
| List 2 — "Files/modules to change" | `spec.md:160-163` | **two** files: `UtilitiesCS/Threading/UiThread.cs`; `UtilitiesCS.Test/Threading/UiThread_Tests.cs (new regression test class)` |
| List 3 — Write Set | `spec.md:86-95` | **six** files: `UiThread.cs`, `UiThread_Tests.cs`, `IdleAsyncQueue_Tests.cs`, `ProgressTrackerAsync_Tests.cs`, `ProgressTracker_Tests.cs`, `EmailMoveMonitorTests.cs` |

The Write Set (six) is the authoritative list — it matches `evidence/qa-gates/p4-t1-format.md:78-83`,
which records post-format line counts for exactly those six paths.

**Recommended:** set Status to `Merged (PR #778, merge commit 1c3b210c, 2026-09-05)`, keeping the
amendment history as a following sentence; then make lists 1 and 2 point at the Write Set
(list 1 gains the three `[DoNotParallelize]` attribute-only files; list 2 is replaced by a
cross-reference to the Write Set rather than a third independent enumeration).

*Note:* the review body records "all seven ACs checked" for `spec.md`. I did not re-read the AC block
line by line — **UNVERIFIED** here, since the remedy (a Status change) does not depend on it.

### S3-7 — call-site counts in `spec.md`

| Location | Current text |
|---|---|
| `spec.md:50-52` | "the first dereference downstream (`ProgressTrackerAsync.InitializeAsync()`, or any of **~40 other call sites** across `UtilitiesCS`, `QuickFiler`, and `TaskMaster` that read `UiThread.Dispatcher` without a guard)" |
| `spec.md:73-76` | "The injectable-seam conversion replacing **~62 remaining direct reads** of `UiThread.Dispatcher` across **~29 production files**" |
| `spec.md:171-172` | "instead of relying on each of the **~40 call sites** (or none) to guard independently" |

**Verified figure: 49 live reads across 25 production files.** Derivation, member sets, and the
independent cross-check are in § Numeric Derivation Evidence, claim 1. The complete textual family
is 64 occurrences across 30 production files; 15 of those are comments, XML docs, commented-out code,
or the exception-message literal.

**Recommended replacement figure for all three sites:** "49 live reads across 25 production files
(verified 2026-09-05 at issue #782; 64 textual occurrences across 30 files, of which 15 are
comments, XML documentation, commented-out code, or the exception message literal)."

### S3-8 — tonality spans

`.claude/rules/tonality.md` bans hyperbole ("Claims that something is perfect, flawless, amazing...")
and requires evidence-first, measured wording. Six spans:

| Artifact | Line | Current span | Proposed neutral replacement |
|---|---|---|---|
| `feature-audit.2026-09-04T04-05.md` | `:117` | "The amendment note on AC4 (round 15) is **honest and correct**:" | "The amendment note on AC4 (round 15) is accurate:" |
| `feature-audit.2026-09-04T04-05.md` | `:119` | "returning the criterion to unchecked until the pass-after evidence existed **was the right call** — the alternative would have left..." | "returning the criterion to unchecked until the pass-after evidence existed keeps the criterion binding; the alternative would have left..." |
| `code-review.2026-09-04T04-05.md` | `:22` | "Two aspects of the execution are worth naming specifically because they are **stronger than typical**:" | "Two aspects of the execution are recorded here because they bear on the verdict:" |
| `code-review.2026-09-04T04-05.md` | `:191` | "**Exemplary** at `EmailMoveMonitorTests.cs:33-37`: the comment records the causal chain..." | "Satisfied at `EmailMoveMonitorTests.cs:33-37`: the comment records the causal chain..." |
| `policy-audit.2026-09-04T04-05.md` | `:111` | "...(`PropertyInfo.GetValue` would surface the guard as `TargetInvocationException` from setup/teardown). **This is a model instance of the rule.**" | "...from setup/teardown). The comment states the reason rather than restating the code, which is what the rule requires." |
| `evidence/qa-gates/p2-t3-file-size.md` | `:42` | "so the post-change count is unchanged at 514, **comfortably inside** the baseline-plus-one tolerance." | "so the post-change count is unchanged at 514, which equals the baseline and is therefore within the baseline-plus-one tolerance." |

A seventh candidate the plan may wish to include, same file and same category:
`policy-audit.2026-09-04T04-05.md:115` "This is a **provable assertion-level RED-first**, not a
compile-red" — "provable" is an evaluative intensifier over an already-evidenced claim. Flag as
optional.

### S3-9 — was the ProgressTrackerAsync_Tests synchronization follow-up promoted?

**NO. It was not promoted.** Evidence:

1. Grep for `ProgressTrackerAsync` across `docs/features/potential/` (including
   `potential/promoted/`) returns exactly **two** files:
   - `docs/features/potential/promoted/2026-09-05-pr-778-post-merge-review-residuals.md` — this
     delivery's own entry (the C26 mention).
   - `docs/features/potential/promoted/2026-08-27-wpfuidispatchertests-ungated-static-swap.md`
     (issue #648), whose **Out of scope** section at `:40-43` states verbatim:

     > "Out of scope: the cross-assembly mutators in `UtilitiesCS.Test` (`ProgressTracker_Tests.cs`,
     > `ProgressTrackerAsync_Tests.cs`, `IdleAsyncQueue_Tests.cs`) mutate the same process-wide static
     > and are **not** covered here. No test-side lock inside `QuickFiler.Test` can reach them. They
     > are accepted residual risk R-2 of #493 and overlap #584."

2. No active feature folder covers it: `docs/features/active/2026-08-27-wpfuidispatchertests-ungated-static-swap-648/`
   is #648's folder and inherits that out-of-scope boundary;
   `docs/features/active/quickfiler-test-uithread-dispatcher-493/` is #493's, whose R-2 is the same
   deferral.
3. The recommendations that asked for it are still open at
   `code-review.2026-09-04T04-05.md:85` ("**Recommendation:** promote item 1 to a GitHub issue before
   merge.") and `policy-audit.2026-09-04T04-05.md:323-330` (finding F5).

**UNVERIFIED:** whether a GitHub issue exists that was never mirrored into a potential entry. No
network access to the private repository from this session; the negative claim above is scoped to
the repository tree.

**Correction the artifacts should record.** `issue.md:91-92` says the follow-up "is satisfied by C26
in this delivery". That is **not accurate**: F5 asks for *synchronization around the reflective
mutation*, which is satisfied by **C12/C13** (the single shared install scope that all four
UtilitiesCS.Test sites migrate to), not by C26 (which adds a new null-dispatcher test). The artifacts
should say C12/C13 satisfies it, and may note C26 as adjacent coverage. See § Discrepancies D-4.

---

## D. The C09 behavioral follow-up (AC8)

### D18.1 Recommendation

| Field | Recommendation | Reasoning |
|---|---|---|
| Promotion type | **`bug`** | The defect is a missing precondition check on an existing contract that silently installs a non-pumping dispatcher into set-once process-global state. `SyncContextForm.CaptureUiVariables` (`QuickFiler/Viewers/SyncContextForm.cs:34-40`) captures `Dispatcher.CurrentDispatcher` on whatever thread calls it, with no validation anywhere in the chain. That is a correctness gap in existing behavior, not a new capability, so the bug-report template (which forces Steps to Reproduce / Expected / Actual / Impact) fits and the repo's Bugfix Workflow — failing regression test first — applies. |
| Work mode | **`full-bug`** | Consistent with the `bug` type and with the sibling entry `2026-08-27-wpfuidispatchertests-ungated-static-swap.md`, which is the same shape (test-isolation/threading precondition on the same static) and used the bug-report template. |
| Short name | **`uithread-init-accepts-non-sta-callers`** | kebab-case, names the defect not the remedy, and does not collide with `uithread-dispatcher-null-race-progresstrackerasync-584` or `wpfuidispatchertests-ungated-static-swap`. |

### D18.2 Blast radius — every `UiThread.Init` call site

**Direct calls (3 textual, 2 live):**

| # | Site | Verbatim | Apartment |
|---|---|---|---|
| 1 | `TaskMaster/ThisAddIn.cs:35-40` | `UiThread.Init(monitorUiThread: true, onLockupDetected: attribution => GetStoreLockupResponder()?.OnLockupDetected(attribution), timeProvider: TimeProvider.System);` | **STA.** Called from `ThisAddIn_Startup` (`:21`), the VSTO add-in startup callback, which Outlook raises on the host STA thread. Unaffected by an STA check. |
| 2 | `QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncTests.cs:329` | `UiThread.Init(false);` | **MTA.** The enclosing test `Worker_RunWorkerCompleted_HandlesCompletionCorrectly` (`:326`) is a plain `[TestMethod]`, and the class is `[TestClass]` only (`:23-24`) with no `[STATestClass]`. `UtilitiesCS.Test/test.runsettings:2-5` documents the repository-wide opt-in model, and `QuickFiler.Test` has no runsettings of its own forcing STA. **This call would throw under an STA check.** |
| 3 | `QuickFiler.Test/Controllers/QfcHomeControllerTests.cs:170` | `//    UiThread.Init(false);` | commented out — no effect |

**Indirect calls — this is the part the review's one-line summary understates.** Two public
accessors on `UiThread` call `Init()` lazily whenever their own backing field is null:

- `UiThread.cs:117-120` — `UiSyncContext` getter: `if (_uiSyncContext is null) { Init(); }`
- `UiThread.cs:160-163` — `AutoScaleFactor` getter: `if (_autoScaleFactor is null) { Init(); }`

Every reader of those two properties is therefore a latent `Init()` call site on whatever thread it
runs. Complete production enumeration (from the exhaustive `UiThread.<member>` census in
§ Numeric Derivation Evidence, claim 1):

| Member | Production readers | Apartment |
|---|---|---|
| `UiSyncContext` | `UtilitiesCS/Threading/ThreadMonitor.cs:143`; `UtilitiesCS/OutlookObjects/Folder/FolderPredictor.cs:178`; `TaskMaster/AppGlobals/AppOlObjects.cs:367` | ThreadMonitor's is a watchdog thread — **not necessarily STA**. The other two run post-startup on the STA in production. |
| `AutoScaleFactor` | `TaskMaster/ThisAddIn.cs:114`; `UtilitiesCS/EmailIntelligence/OlFolderTools/FolderRemap/FolderRemapViewer.cs:40`; `UtilitiesCS/EmailIntelligence/OlFolderTools/FilterOlFolders/FilterOlFoldersViewer.cs:79` | WinForms paint/layout paths — STA in production |
| `UiThreadId` (not lazy, no `Init()`) | `UtilitiesCS/HelperClasses/SegmentStopWatch.cs:24`; `TaskMaster/AppGlobals/AppOlObjects.cs:364` | n/a |

Test-side readers of the lazy properties: `policy-audit.2026-09-04T04-05.md:343-345` records that
**no** file in `UtilitiesCS.Test` reads `UiThread.Init(`, `UiThread.UiSyncContext`, or
`UiThread.AutoScaleFactor` directly, and that `FolderPredictorTests` sets `_uiSyncContext`
reflectively at `FolderPredictorTests.cs:479` before the call so the lazy branch is never taken. I
re-derived the reflective write independently: `FolderPredictorTests.cs:469-472` does
`typeof(UiThread)` then `GetField("_uiSyncContext", NonPublic|Static)`. That claim holds at HEAD.

**Verdict on "bug fix vs breaking behavior change":** in **production** the change is a pure bug fix
— every production `Init()` entry point (direct or lazy) is on the STA during and after
`ThisAddIn_Startup`, with `ThreadMonitor.cs:143` the one path worth re-checking during
implementation. In **test code** it is a breaking change to exactly one call site,
`QfcHomeControllerRunAsyncTests.cs:329`, which must be moved to an STA thread (or the test converted
to `[STATestMethod]`) as part of the same change. That single, named, bounded breakage is why this
belongs in its own entry rather than inside #782.

### D18.3 Draft body for the promoted potential entry

```markdown
## Summary

`UtilitiesCS.UiThread.Init()` accepts a call from any thread. It performs no apartment-state check,
and neither does the `Initialize()` it guards. A worker-thread call therefore succeeds silently and
installs that worker's non-pumping `Dispatcher`, `SynchronizationContext`, and managed thread id
into set-once process-global state, after which every consumer of `UiThread.Dispatcher`,
`UiThread.UiSyncContext`, `UiThread.AutoScaleFactor`, and `UiThread.UiThreadId` marshals onto a
thread that never runs a message loop.

Raised as the behavioral half of finding C09 in the three-phase post-merge review of PR #778
(issue #584). The message-text half of C09 is delivered in issue #782; this entry is the behavior
change that #782 explicitly placed out of scope.

## Problem

- `UtilitiesCS/Threading/UiThread.cs:19-40` — `Init(...)` validates none of its callers' context.
  Its only gate is the single-shot latch at `:36`, `if (_loaded.CheckAndSetFirstCall)`.
- `UtilitiesCS/Threading/UiThread.cs:48-79` — `Initialize()` constructs and `Show()`s a WinForms
  `SyncContextForm` and then calls `CaptureUiVariables()`. No apartment check.
- `QuickFiler/Viewers/SyncContextForm.cs:34-40` — `CaptureUiVariables()` reads
  `SynchronizationContext.Current`, `this.AutoScaleFactor`, `Dispatcher.CurrentDispatcher`, and
  `Thread.CurrentThread.ManagedThreadId` from the calling thread unconditionally.
- Because the latch at `UiThread.cs:36` is single-shot, the **first** caller wins permanently. A
  worker-thread `Init()` that happens to run first poisons the globals for the process lifetime, and
  the exception message added by #782 ("Call UiThread.Init()...") offers no remedy, because `Init()`
  has already run.
- The hazard is presently reachable only from tests: `QuickFiler.Test/Controllers/
  QfcHomeControllerRunAsyncTests.cs:329` calls `UiThread.Init(false)` from a plain `[TestMethod]` on
  an MTA pooled worker. In production, `TaskMaster/ThisAddIn.cs:35-40` is the only direct caller and
  runs on the Outlook STA.
- Two additional latent entry points exist: the `UiSyncContext` getter (`UiThread.cs:117-120`) and
  the `AutoScaleFactor` getter (`UiThread.cs:160-163`) both call `Init()` when their backing field is
  null, so any reader of either property on a non-STA thread is an implicit `Init()` caller.

## Proposed Behavior

- `UiThread.Init(...)` throws `InvalidOperationException` when
  `Thread.CurrentThread.GetApartmentState() != ApartmentState.STA`, with a message naming the
  requirement and the caller's observed apartment state.
- The check runs **before** the single-shot latch at `UiThread.cs:36` is consumed, so a rejected call
  does not burn the one-shot and a subsequent correct call still initializes. This composes with the
  C03 change delivered in #782 (re-arm the latch when `Initialize()` throws).
- The two lazy accessors keep their current self-healing behavior on the STA and surface the same
  named exception off it, instead of silently capturing a worker thread's context.
- `QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncTests.Worker_RunWorkerCompleted_HandlesCompletionCorrectly`
  is migrated to an STA context (`[STATestMethod]`, or a dedicated STA thread with `Join()`), which is
  the only in-repo caller the change breaks.

## Acceptance Criteria

- [ ] AC1: `UiThread.Init()` called from an MTA thread throws `InvalidOperationException` whose
      message names the STA requirement and the observed apartment state. Covered by a deterministic
      test that runs the Act on a dedicated MTA thread and joins it.
- [ ] AC2: `UiThread.Init()` called from an STA thread behaves exactly as before. Covered by a test
      that asserts the single-shot latch, the captured dispatcher, and the captured
      `UiThreadId` are unchanged.
- [ ] AC3: A rejected non-STA call does not consume the single-shot latch: a subsequent STA call in
      the same process still runs `Initialize()`.
- [ ] AC4: `QfcHomeControllerRunAsyncTests.Worker_RunWorkerCompleted_HandlesCompletionCorrectly`
      passes on an STA context, and a repository-wide grep confirms no remaining `UiThread.Init`
      call site executes off the STA.
- [ ] AC5: The `UiSyncContext` and `AutoScaleFactor` lazy-`Init()` branches are covered for both the
      STA (self-heals) and non-STA (throws) cases.
- [ ] AC6: The full C# toolchain (csharpier, analyzers, nullable, vstest with coverage) passes and
      changed-line coverage does not decrease.
```

---

## E. Toolchain and coverage facts

### E19. Test assemblies in the solution

Nine test projects (`TaskMaster.sln`), all `<TargetFrameworkVersion>v4.8.1` and all
`Debug|AnyCPU` → `<OutputPath>bin\Debug\</OutputPath>`:

| Project (sln line) | `AssemblyName` (csproj line) | `OutputPath` (csproj line) | Built assembly path |
|---|---|---|---|
| `ToDoModel.Test` (`:10`) | `ToDoModel.Test` (`:16`) | `bin\Debug\` (`:35`) | `ToDoModel.Test\bin\Debug\ToDoModel.Test.dll` |
| `UtilitiesCS.Test` (`:16`) | `UtilitiesCS.Test` (`:16`) | `bin\Debug\` (`:51`) | `UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll` |
| `QuickFiler.Test` (`:25`) | `QuickFiler.Test` (`:17`) | `bin\Debug\` (`:36`) | `QuickFiler.Test\bin\Debug\QuickFiler.Test.dll` |
| `TaskVisualization.Test` (`:27`) | `TaskVisualization.Test` (`:16`) | `bin\Debug\` (`:35`) | `TaskVisualization.Test\bin\Debug\TaskVisualization.Test.dll` |
| `Tags.Test` (`:33`) | `Tags.Test` (`:16`) | `bin\Debug\` (`:34`) | `Tags.Test\bin\Debug\Tags.Test.dll` |
| `TaskTree.Test` (`:38`) | `TaskTree.Test` (`:16`) | `bin\Debug\` (`:34`) | `TaskTree.Test\bin\Debug\TaskTree.Test.dll` |
| `SVGControl.Test` (`:42`) | `SVGControl.Test` (`:15`) | `bin\Debug\` (`:33`) | `SVGControl.Test\bin\Debug\SVGControl.Test.dll` |
| `VBFunctions.Test` (`:46`) | `VBFunctions.Test` (`:16`) | `bin\Debug\` (`:35`) | `VBFunctions.Test\bin\Debug\VBFunctions.Test.dll` |
| `TaskMaster.Test` (`:48`) | `TaskMaster.Test` (`:16`) | `bin\Debug\` (`:35`) | `TaskMaster.Test\bin\Debug\TaskMaster.Test.dll` |

`UtilitiesCS.Test`, `TaskVisualization.Test`, `SVGControl.Test`, `ToDoModel.Test` and
`QuickFiler.Test` also declare `bin\x86\Debug\` / `bin\x86\Release\` outputs for the `x86` platform;
the repository toolchain uses `"/p:Platform=Any CPU"` throughout, so only the `bin\Debug\` paths are
in play.

Ten production projects: `Tags`, `ToDoModel`, `TaskVisualization`, `UtilitiesCS`, `QuickFiler`,
`TaskTree`, `TaskMaster`, `SVGControl`, `VBFunctions` (nine, plus the `Solution Items` folder at
`:18` which is not a project).

The #584 delivery ran only `UtilitiesCS.Test.dll` and `QuickFiler.Test.dll` locally (finding S4-2,
`evidence/qa-gates/p4-t5-utilitiescs-tests.md` and `p4-t6-quickfiler-tests.md`). This delivery
touches `UtilitiesCS`, `TaskMaster/Ribbon`, `UtilitiesCS.Test`, and `QuickFiler.Test`, so at minimum
`UtilitiesCS.Test.dll`, `QuickFiler.Test.dll`, and `TaskMaster.Test.dll` should be run; naming all
nine avoids the S4-2 finding recurring.

### E20. Shell-icon test classes that stall locally

Four classes. `SHGetFileInfo` appears only in
`UtilitiesCS/HelperClasses/FileSystem/ShellUtilitiesStatic.cs` and
`UtilitiesCS/HelperClasses/FileSystem/ShellUtilities.cs`; the affected test classes are:

| # | Fully-qualified class name | Declaration |
|---|---|---|
| 1 | `UtilitiesCS.Test.HelperClasses.ShellUtilities_Tests` | `UtilitiesCS.Test/HelperClasses/ShellUtilities_Tests.cs:7` (namespace), `:10` (class) |
| 2 | `UtilitiesCS.Test.HelperClasses.ShellUtilitiesStatic_Tests` | `UtilitiesCS.Test/HelperClasses/ShellUtilitiesStatic_Tests.cs:7`, `:10` |
| 3 | `UtilitiesCS.Test.HelperClasses.SysImageListHelperTests` | `UtilitiesCS.Test/HelperClasses/SysImageListHelperTests.cs:9`, `:12` (`[TestClass]` at `:11`) |
| 4 | `UtilitiesCS.Test.EmailIntelligence.OSBrowser_Tests` | `UtilitiesCS.Test/EmailIntelligence/OSBrowser_Tests.cs:27` (`[STATestClass]` at `:26`) |

A fifth file, `UtilitiesCS.Test/HelperClasses/ShellUtilitiesTests.cs`, declares no live class — its
`class ShellUtilitiesTests` is commented out at `:16` — so it contributes no tests.

`/TestCaseFilter` expression (must be combined with the pipeline's existing `TestCategory!=LiveOutlook`):

```
/TestCaseFilter:"TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests"
```

Note `!~HelperClasses.ShellUtilities_Tests` also matches nothing else because the
`ShellUtilitiesStatic_Tests` name does not contain the `ShellUtilities_Tests` substring; the two
clauses are independent.

Environmental, not a regression: the same four stall against a build of `main` on this workstation.
Rely on CI for those classes. Also expect
`UtilitiesCS.Test...DictionaryExtensions_Tests.TryAddValuesAsync_UpdatesExistingValue` to fail
sporadically under high-worker coverage runs (tracked as issue #780).

### E21. The coverage pipeline, and how to get `artifacts/csharp/coverage.xml`

**Ready-to-run invocation:**

```powershell
pwsh -NoProfile -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -Configuration Debug
```

Behavior, read from `scripts/vscode/Invoke-MSTestWithCoverage.ps1`:

| Step | Line(s) | Detail |
|---|---|---|
| Runsettings resolution | `:33`, `:278` | `scripts/vscode/TaskMaster.cli.runsettings` (MSTest parallelization only: `<Workers>0</Workers>`, `<Scope>ClassLevel</Scope>`; **no** coverage data collector) |
| vstest discovery | `:284-290` | via `vswhere -latest -products * -find Common7\IDE\Extensions\TestPlatform\vstest.console.exe` |
| Assembly discovery | `:296-303` | every `*.Test.dll` under `bin\<Configuration>\`, excluding `\obj\`, `\ref\`, and any path matching `(^\|\\)\.claude\\` |
| Coverage settings | `:321` | repo-root `coverage.config`, cloned in memory and augmented with a `.*\.Test\.dll$` module exclusion (`:99-113`), written to a derived `*.effective-coverage.config` beside the output and deleted in `finally` (`:198-242`) |
| Command shape | `:70-77` | `dotnet-coverage collect --output <out> --output-format cobertura --settings <derived> -- <vstest> <assemblies…> /Settings:<cli.runsettings> /InIsolation /TestCaseFilter:TestCategory!=LiveOutlook` |
| **Output path** | `:9`, `:309` | default `coverage\coverage.cobertura.xml`, resolved against the repo root; override with `-CoverageOutput` |
| **Output format** | `:73` | **Cobertura**, then post-processed |
| Post-processing | `:339-342` | `ConvertTo-KoverageCoberturaXml` — rewrites absolute paths to workspace-relative, injects `<sources><source>.</source></sources>`, drops third-party `<package>` elements, and **recomputes the root totals** (`Invoke-MSTestWithCoverage.Helpers.ps1:454-459` sets `line-rate`, `branch-rate`, `lines-covered`, `lines-valid`, `branches-covered`, `branches-valid`) |
| Threshold gate | `:344` | `Assert-CoberturaLineCoverageThreshold` (`Invoke-MSTestWithCoverage.Threshold.ps1:3-56`) — **throws when the post-processed root `line-rate` is below 80%** |

**Two operational facts the plan must encode:**

1. **The artifact is written before the threshold assertion.** `Set-Content` at `:342` precedes
   `Assert-CoberturaLineCoverageThreshold` at `:344`. If the repo-wide figure is below 80%, the
   script throws but `coverage\coverage.cobertura.xml` **already exists and is complete**. The #584
   PR body records a raw repository line rate of `0.7073604`, so a throw at this step is the expected
   outcome, not a failure of the delivery. The plan should record the exit as an expected non-zero
   with `ExpectedExitCode:` rather than treating it as a red gate.
2. **The script's `/TestCaseFilter` is hard-coded** at `:76` and cannot be extended by a parameter.
   To apply the § E20 shell-icon exclusion, the plan must invoke `dotnet-coverage collect` directly
   in the shape `policy-audit.2026-09-04T04-05.md:427-432` records for #584:

   ```
   dotnet-coverage collect --output coverage/<name>.cobertura.xml --output-format cobertura \
     --settings coverage.config -- vstest.console.exe <assembly.dll> \
     /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation /Logger:trx \
     /ResultsDirectory:TestResults/<name> /TestCaseFilter:"<extended filter>"
   ```

   Doing so loses the automatic `.*\.Test\.dll$` module exclusion the script injects at `:99-113`,
   so `coverage.config` must be supplemented or the test packages stripped in post-processing. State
   this trade-off explicitly in the plan.

**Getting to `artifacts/csharp/coverage.xml` — a format conversion is mandatory.**

`.claude/hooks/validate-feature-review-coverage.ps1` reads that path with
`Get-JacocoRepoCoverage` / `Get-JacocoBranchCoverage` (`:216`, `:221-234`, `:186-206`), which do
`$doc.SelectNodes('//counter[@type="LINE"]')` and `'//counter[@type="BRANCH"]'` and return `$null`
when no `<counter>` element is found. **A Cobertura document placed at that path yields zero
counters and is treated as absent.** The file must be **JaCoCo**.

Prior art: `docs/features/archive/2026-08-08-wpf-dispatcher-yield-test-order-dependent-508/evidence/qa-gates/coverage-artifact-substitution.2026-08-08T17-30.md:72-84`
records the same conclusion verbatim ("That hook parses JaCoCo `<counter>` elements and cannot read
Cobertura, which is why the format conversion is required rather than optional") and states that
`artifacts/` is gitignored so the file is local-only and regenerated, not committed.

**There is no committed Cobertura→JaCoCo converter.** Grep for `jacoco` (case-insensitive) across
`scripts/` returns no files; the #508 run used a scratchpad script
(`<scratchpad>/Convert-CoberturaToJacoco.ps1`). The plan must therefore include a throwaway
conversion step. Two supporting facts make that acceptable:

- `.gitignore:57` is `artifacts/` — the output is never committed.
- `.claude/rules/general-code-change.md` exempts "temporary throwaway scripts created and deleted
  within an agent session" from the 500-line file limit; such a script is not a repository asset.

The conversion must aggregate per-package `<class>`/`<line>` `hits` into JaCoCo
`<counter type="LINE" missed covered>` and per-line `condition-coverage` into
`<counter type="BRANCH" missed covered>`, as #508 did losslessly (its derived counts reproduced the
Cobertura root `lines-covered`/`lines-valid` exactly).

**Plain (non-coverage) runs:** `scripts/vscode/Invoke-MSTest.ps1` builds
`<assemblies> /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation
/TestCaseFilter:TestCategory!=LiveOutlook` (`:54`) and throws on a non-zero exit (`:195-197`).

### E22. `.claude\` worktree exclusion

| Location | Excludes `.claude`? | Detail |
|---|---|---|
| `scripts/vscode/Invoke-MSTestWithCoverage.ps1:301` | **YES** | `([System.IO.Path]::GetRelativePath($resolvedSearchRoot, $_.FullName)) -notmatch '(^\|\\)\.claude\\'` |
| `scripts/vscode/Invoke-MSTest.ps1:120-127` | **NO** | `Get-MSTestAssemblyPathList` filters only `\\bin\\$Configuration\\`, `\\obj\\`, `\\ref\\`. **The `.claude` clause is absent.** |
| `TaskMaster.runsettings` | n/a | carries `<Parallelize>` and the `DataCollectionRunSettings` module excludes only (`:2-30`); no assembly-discovery filter exists in a runsettings file |
| `scripts/vscode/TaskMaster.cli.runsettings` | n/a | `<Parallelize>` only (`:2-9`) |
| `coverage.config` | n/a | seven third-party `<ModulePath>` excludes (`:14-20`); no path filter |

**Current state of this worktree:** Glob for `.claude/**/*.Test.dll` returns **no files**, so no
stale worktree assembly is discoverable here today and no exclusion is strictly required for this
delivery.

**What the plan must add, if anything:** nothing to the coverage path — the guard is already there.
If the plan invokes `Invoke-MSTest.ps1` (the non-coverage path) or a bare
`vstest.console.exe <glob>`, it must supply explicit assembly paths (as § E19 lists) rather than
relying on discovery, because that script has no `.claude` guard. Adding the guard to
`Invoke-MSTest.ps1` would be a PowerShell production change outside this delivery's write set; if
the planner wants it, promote it as a separate entry rather than folding it into #782.

One further defect visible in the same file and worth a separate promotion, not a #782 edit:
`Get-VsTestArgumentList`'s docstring at `:42-44` says the `/Settings:` argument points at
"the repo-root `TaskMaster.runsettings`", while the code at `:29`/`:167` resolves
`scripts/vscode/TaskMaster.cli.runsettings`. The docstring names the wrong file.

---

## Automation Feasibility

**No step of this delivery requires a human to interact with a third-party user interface.**

Reasoning, item by item:

| Work item | Interface required | Automatable? |
|---|---|---|
| All production and test source edits (C01–C26, S2-1) | local filesystem | Yes — Read/Edit/Write |
| `UtilitiesCS.Test.csproj` registration of the split file and the new TestHelpers file | local filesystem | Yes |
| Documentation and evidence edits in the #584 folder (S3-1…S3-9) | local filesystem | Yes |
| Format gate | `dotnet tool run csharpier format .` / `check .` | Yes — CLI, exit-code observable |
| Analyzer gate | `msbuild TaskMaster.sln /t:Rebuild ...` | Yes — CLI |
| Nullable gate | `msbuild TaskMaster.sln /t:Rebuild ... /p:TreatWarningsAsErrors=true` | Yes — CLI |
| Test + coverage gate | `dotnet-coverage collect -- vstest.console.exe ...` (or `scripts/vscode/Invoke-MSTestWithCoverage.ps1`) | Yes — CLI. Tool discovery is automated through `vswhere` at `Invoke-MSTestWithCoverage.ps1:279-290`. |
| Cobertura → JaCoCo conversion for `artifacts/csharp/coverage.xml` | throwaway PowerShell script | Yes |
| AC8 promotion of the C09 follow-up | GitHub issue creation | Yes — the repository's promotion lifecycle plus `gh` CLI. `evidence/issue-updates/issue-584.2026-09-02T09-02.md:9` records "`gh` was available and authenticated" during the #584 delivery, and `:7` carries the resulting comment URL, so the CLI path is proven in this environment. No browser interaction. |
| AC8 recording of the S4-1 upstream follow-up for drm-copilot | a note in this repository's artifacts, per `issue.md:112-114` | Yes — text only; the upstream fix itself is explicitly out of scope here |
| PR authoring | `pr-author` skill + `gh pr create --body-file` | Yes — CLI |

Two contingencies that are still not third-party-UI interactions:

- **`gh` unavailable or unauthenticated at run time.** The documented fallback is a
  `POSTING BLOCKED` evidence artifact
  (`.claude/skills/evidence-and-timestamp-conventions/SKILL.md:173`), which is a file write. It
  degrades the evidence, not the automation model.
- **The four shell-icon classes stalling locally (§ E20).** Mitigated by a `/TestCaseFilter`
  argument. The underlying cause is a machine-level stuck shell icon handler; restarting Explorer
  would be a user-visible system change and is deliberately not part of the plan.

Explicitly excluded from the write set and therefore raising no automation question:
`.claude/**` (push-down-owned from drm-copilot, `issue.md:163`), which covers both the S4-1 agent-memory
notes and the `evidence-and-timestamp-conventions` skill.

---

## Numeric Derivation Evidence

### Claim 1 — `UiThread.Dispatcher` production call sites: **49 live reads across 25 production files** (S3-7)

- **Complete Family.** Every syntactic route by which first-party **production** (non-`*.Test`) C#
  code can reach the value of the static property `UtilitiesCS.UiThread.Dispatcher`. The family has
  four members: (a) the qualified expression `UiThread.Dispatcher`; (b) the fully-qualified
  expression `UtilitiesCS.UiThread.Dispatcher`; (c) an unqualified `Dispatcher` reached through
  `using static UtilitiesCS.UiThread;`; (d) a reflective property read
  `typeof(UiThread).GetProperty("Dispatcher")`. Routes (a) and (b) are both matched by the pattern
  `\bUiThread\.` because (b) contains (a) as a suffix.
- **Exhaustive Search Scope.** All `*.cs` files in the nine production projects declared in
  `TaskMaster.sln` (`Tags`, `ToDoModel`, `TaskVisualization`, `UtilitiesCS`, `QuickFiler`,
  `TaskTree`, `TaskMaster`, `SVGControl`, `VBFunctions`). Routes (c) and (d) were additionally
  searched repository-wide with no project restriction, so a hit in any assembly would have surfaced.
- **Inclusion Rules.** A member is one textual occurrence of the property access in executable
  source: the expression appears outside `//`, `///`, and `/* */` contexts and outside a string
  literal, and is not a commented-out statement.
- **Exclusion Rules.** Excluded: (i) all `*.Test` projects; (ii) `//` and `///` comment prose;
  (iii) commented-out code (a `//`-prefixed statement); (iv) the exception-message string literal at
  `UtilitiesCS/Threading/UiThread.cs:142`; (v) the private setter write at `UiThread.cs:61`
  (`Dispatcher = _syncContextForm.UiDispatcher;`), which is a write, not a read, and does not match
  the `UiThread.` qualifier.
- **Primary Search Strategy or Query Expression.** Grep, regex `UiThread\.Dispatcher`, glob
  `**/*.cs`, repository-wide, `head_limit: 0`, output mode `content` with line numbers; results then
  partitioned by project and each line classified live / non-live by reading its text.
- **Primary Member Set.**
  *UtilitiesCS (14):* `Threading/IdleActionQueue.cs:78`; `Threading/WpfUiDispatcher.cs:25`;
  `Threading/ProgressTrackerPane.cs:13`, `:16`; `Threading/ProgressTrackerAsync.cs:33`, `:39`;
  `Threading/ProgressTracker.cs:33`, `:39`; `Threading/IdleAsyncQueue.cs:72`;
  `HelperClasses/ToolTips/QfcTipsDetails.cs:254`, `:277`;
  `HelperClasses/ThemeHelpers/ThemeControlGroup.cs:218`, `:222`;
  `OutlookObjects/Folder/WpfDispatcherYield.cs:46`.
  *QuickFiler (30):* `Helper Classes/ItemViewerQueue.cs:21`, `:27`, `:88`, `:90`;
  `Helper Classes/EmailMoveMonitor.cs:44`; `Helper Classes/EfcViewerQueue.cs:20`, `:67`;
  `Helper Classes/ConversationResolver.Loading.cs:150`, `:320`;
  `Controllers/QfcQueue.cs:476`, `:484`, `:492`; `Controllers/QfcHomeController.cs:360`;
  `Controllers/QfcFormController.EventHandlers.cs:197`, `:237`, `:242`;
  `Controllers/QfcFormController.Actions.cs:255`;
  `Controllers/QfcCollectionController.cs:951`, `:982`, `:1210`, `:1220`, `:1238`, `:1256`, `:1333`;
  `Controllers/KeyboardHandler.cs:362`, `:370`, `:401`;
  `Controllers/EfcItemController.cs:998`, `:1007`; `Controllers/EfcHomeController.cs:297`.
  *TaskMaster (5):* `ThisAddIn.cs:227`; `Ribbon/RibbonViewer.EngineCommands.cs:71`, `:114`;
  `AppGlobals/AppOlObjects.FolderTreeService.cs:344`; `AppGlobals/ApplicationGlobals.cs:293`.
- **Primary Count.** 14 + 30 + 5 = **49** live reads, in **25** distinct files
  (9 UtilitiesCS + 12 QuickFiler + 4 TaskMaster).
- **Cross-check Search Strategy or Query Expression.** A different and strictly wider query over the
  same family: Grep, regex `\bUiThread\.[A-Za-z_]+`, `-o` (match-only) with line numbers, glob
  `**/{TaskMaster,UtilitiesCS,QuickFiler,ToDoModel,Tags,TaskVisualization,TaskTree,SVGControl}/**/*.cs`,
  `head_limit: 0`. This enumerates **every** member access on the `UiThread` type — `Init`,
  `Initialize`, `Dispatcher`, `UiSyncContext`, `AutoScaleFactor`, `UiThreadId` — so no `.Dispatcher`
  occurrence can escape it regardless of formatting, and the non-`Dispatcher` members are visible for
  subtraction. Routes (c) and (d) were covered by a third, separate query: regex
  `using static .*UiThread|GetProperty\(\s*"Dispatcher"|nameof\(UiThread` over `**/*.cs`
  repository-wide.
- **Cross-check Member Set.** The exhaustive member census returned **123 occurrences of
  `UiThread.<member>` across 54 files** repository-wide, and for production projects the
  `Dispatcher` member accounts for **64** textual occurrences across **30** files:
  `ThisAddIn.cs` 190, 227 (2); `ItemViewerQueue.cs` 21, 27, 88, 90 (4);
  `RibbonViewer.EngineCommands.cs` 54, 71, 93, 114 (4); `EmailMoveMonitor.cs` 38, 44 (2);
  `EfcViewerQueue.cs` 20, 67 (2); `ConversationResolver.Loading.cs` 150, 320 (2);
  `EngineToggleStateCoordinator.cs` 42 (1); `WpfUiDispatcher.cs` 11, 25 (2); `UiThread.cs` 142 (1);
  `QfcTipsDetails.cs` 254, 277 (2); `ProgressTrackerPane.cs` 13, 16 (2);
  `ProgressTrackerAsync.cs` 33, 39 (2); `ProgressTracker.cs` 33, 39 (2);
  `ThemeControlGroup.cs` 218, 222 (2); `Theme.cs` 441 (1); `IUiDispatcher.cs` 11 (1);
  `AppOlObjects.FolderTreeService.cs` 344 (1); `IdleAsyncQueue.cs` 72 (1); `IdleActionQueue.cs` 78 (1);
  `QfcQueue.cs` 476, 484, 492, 502 (4); `ApplicationGlobals.cs` 159, 271, 293 (3);
  `QfcHomeController.Iteration.cs` 31 (1); `QfcHomeController.cs` 360 (1);
  `QfcFormController.EventHandlers.cs` 197, 237, 242 (3); `QfcFormController.Actions.cs` 255 (1);
  `QfcCollectionController.cs` 933, 951, 982, 1210, 1220, 1238, 1256, 1333 (8);
  `KeyboardHandler.cs` 362, 370, 401 (3); `EfcItemController.cs` 998, 1007 (2);
  `EfcHomeController.cs` 297 (1); `WpfDispatcherYield.cs` 46, 57 (2).
  Non-`Dispatcher` members observed and subtracted, confirming the family split is complete:
  `UiThread.Init` at `ThisAddIn.cs:35`, `UiThread.cs:142`, `WpfDispatcherYield.cs:57`, `:65`;
  `UiThread.Initialize` at `UiThread.cs:142`, `SyncContextForm.cs:26`;
  `UiThread.AutoScaleFactor` at `ThisAddIn.cs:114`, `FolderRemapViewer.cs:40`,
  `FilterOlFoldersViewer.cs:79`; `UiThread.UiSyncContext` at `ThreadMonitor.cs:143`,
  `AppOlObjects.cs:367`, `FolderPredictor.cs:178`; `UiThread.UiThreadId` at `AppOlObjects.cs:364`,
  `SegmentStopWatch.cs:24`; plus one incidental `UiThread.cs` filename match inside an XML doc at
  `RibbonViewer.EngineCommands.cs:54`. Routes (c) and (d): **zero hits** ("No matches found").
- **Cross-check Count.** 64 textual occurrences minus 15 excluded — `ThisAddIn.cs:190` (comment),
  `RibbonViewer.EngineCommands.cs:54` and `:93` (XML doc), `EmailMoveMonitor.cs:38` (XML doc),
  `EngineToggleStateCoordinator.cs:42` (XML doc), `WpfUiDispatcher.cs:11` (XML doc),
  `UiThread.cs:142` (message literal), `Theme.cs:441` (commented-out), `IUiDispatcher.cs:11`
  (XML doc), `QfcQueue.cs:502` (commented-out), `ApplicationGlobals.cs:159` and `:271` (comments),
  `QfcHomeController.Iteration.cs:31` (commented-out), `QfcCollectionController.cs:933`
  (commented-out), `WpfDispatcherYield.cs:57` (comment) — = **49**. Distinct files: 30 minus the 5
  whose only occurrences are excluded (`EngineToggleStateCoordinator.cs`, `UiThread.cs`, `Theme.cs`,
  `IUiDispatcher.cs`, `QfcHomeController.Iteration.cs`) = **25**.
- **Member-set Comparison.** Normalized to `<project-relative path>:<line>`, the primary live set and
  the cross-check live set are **identical**: both contain the same 49 elements and the same 25
  distinct files, with no element present in one and absent from the other. The two counts agree at
  49 reads / 25 files. `VBFunctions` was not in the cross-check glob, but the unrestricted 54-file
  repository census returned no `VBFunctions` file, so the omission removes no member.
- **Assertion.** `spec.md` should read **49 live reads across 25 production files**.

### Claim 2 — #584 evidence artifacts: **38** (S3-3)

- **Complete Family.** Every file stored under
  `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/`, at any
  depth, in any of the canonical `<kind>` sub-paths defined by
  `.claude/skills/evidence-and-timestamp-conventions/SKILL.md:14-20`.
- **Exhaustive Search Scope.** The whole `evidence/` subtree, unrestricted by extension or `<kind>`,
  so a non-`.md` artifact or an unexpected sub-folder would have appeared.
- **Inclusion Rules.** Any file (not directory) whose path begins with that `evidence/` prefix.
- **Exclusion Rules.** The seven top-level feature documents outside `evidence/` (`issue.md`,
  `spec.md`, `plan.2026-09-02T09-02.md`, `code-review.…md`, `feature-audit.…md`,
  `policy-audit.…md`, `research/defect-scoping.…md`).
- **Primary Search Strategy or Query Expression.** Glob,
  pattern `docs/features/active/uithread-dispatcher-null-race-progresstrackerasync-584/evidence/**/*`
  — a path-shaped enumeration that does not read file contents.
- **Primary Member Set.** `baseline/` (14): `p0-t10-utilitiescs-tests-coverage.md`,
  `p0-t11-quickfiler-tests.md`, `p0-t12-threshold-reconciliation.md`,
  `p0-t13-parallel-bucket-census.md`, `p0-t14-reflective-dispatcher-census.md`,
  `p0-t2-uithread-rederivation.md`, `p0-t3-progresstrackerasync-rederivation.md`,
  `p0-t4-test-rederivation.md`, `p0-t5-toolchain-resolution.md`, `p0-t6-mcp-probe.md`,
  `p0-t7-csharpier-check.md`, `p0-t8-analyzer-build.md`, `p0-t9-nullable-build.md`,
  `phase0-instructions-read.md`. `issue-updates/` (1): `issue-584.2026-09-02T09-02.md`.
  `other/` (3): `p3-t4-progresstrackerasync-unmodified.md`, `p5-t10-footprint.md`,
  `p5-t12-ac-status-summary.md`. `qa-gates/` (14): `p1-t5-donotparallelize.md`,
  `p2-t2-nullforgiving-removed.md`, `p2-t3-file-size.md`,
  `p2-t4-emailmovemonitor-reflection-target.md`, `p3-t1-analyzer-build.md`,
  `p3-t5-no-timing-tokens.md`, `p4-t1-format.md`, `p4-t2-format-check.md`, `p4-t3-analyzer-build.md`,
  `p4-t4-nullable-build.md`, `p4-t5-utilitiescs-tests.md`, `p4-t6-quickfiler-tests.md`,
  `p4-t7-coverage-delta.md`, `p4-t8-loop-closure.md`. `regression-testing/` (6):
  `p1-t3-build-before-fix.md`, `p1-t4-expect-fail.md`, `p3-t2-regression-green.md`,
  `p3-t3-at-risk-tests.md`, `p3-t6-quickfiler-wpfuidispatcher.md`, `p4-t6-first-pass-failure.md`.
- **Primary Count.** 14 + 1 + 3 + 14 + 6 = **38**.
- **Cross-check Search Strategy or Query Expression.** A content-based query over the same scope
  rather than a path-shaped one: Grep, regex `^EXIT_CODE:`, path
  `…/uithread-dispatcher-null-race-progresstrackerasync-584/evidence`, output mode `content` with
  line numbers. Because the evidence schema (`SKILL.md:106-111`) requires an `EXIT_CODE:` field in
  every machine-checkable artifact, this reaches every artifact independently of its filename.
- **Cross-check Member Set.** The query returned exactly **37** distinct files, each with one
  `^EXIT_CODE:` line: the 38 members above **minus** `issue-updates/issue-584.2026-09-02T09-02.md`.
  That file is an issue-update mirror, for which `SKILL.md:167-173` prescribes `Timestamp:`,
  `PostedAs:`, and the comment URL, and does **not** require `EXIT_CODE:` — verified by direct read
  of `:1-14`, which shows `Timestamp:` at `:3`, `PostedAs: comment` at `:5`, and the comment URL at
  `:7`, with no `EXIT_CODE:` line.
- **Cross-check Count.** 37 + 1 schema-exempt mirror = **38**.
- **Member-set Comparison.** The normalized cross-check set is a proper subset of the primary set
  whose single missing element is fully accounted for by a named schema exemption. Adding it back
  makes the two sets identical at 38 elements. Both counts therefore agree at **38**, which also
  matches the `git ls-tree` figure asserted in `issue.md:84`.
- **Assertion.** `policy-audit.2026-09-04T04-05.md:68` should read "All **38** evidence artifacts".

### Claim 3 — reflection sites on `UiThread._dispatcher`: **6 total, 4 in UtilitiesCS.Test**

- **Complete Family.** Every site in any test project that obtains a `FieldInfo` for the private
  static field `UtilitiesCS.UiThread._dispatcher`, by any means: a string literal argument to
  `GetField`, a `nameof`, a cached constant, or an indirection through a helper.
- **Exhaustive Search Scope.** All `*.cs` files in the repository, unrestricted by project.
- **Inclusion Rules.** A member is one `GetField`/`FieldInfo` acquisition whose target is
  `UiThread._dispatcher`, in live (non-commented) source.
- **Exclusion Rules.** Unrelated `_dispatcher` identifiers — instance fields and locals in
  `QuickFiler/Viewers/*`, `UtilitiesCS/Threading/StoreLockupResponder.cs`, `ProgressViewer.cs`,
  `ProgressPane.cs`, `OutlookFolderTreeService.cs`, and the several test doubles in
  `UtilitiesCS.Test/OutlookObjects/Folder/*` and `UtilitiesCS.Test/EmailIntelligence/*` — plus the
  declaration itself at `UtilitiesCS/Threading/UiThread.cs:149` and its two in-getter uses at `:139`
  and `:145`, and all documentation prose mentioning the field name.
- **Primary Search Strategy or Query Expression.** Grep, regex `_dispatcher`, glob `**/*.cs`,
  repository-wide, `head_limit: 0`, output mode `content` — a deliberately over-broad identifier
  search (≈120 hits) followed by manual classification of every hit.
- **Primary Member Set.** `UtilitiesCS.Test/Threading/UiThread_Tests.cs:128`;
  `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs:422`;
  `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs:139`;
  `UtilitiesCS.Test/Threading/IdleAsyncQueue_Tests.cs:145`;
  `QuickFiler.Test/Helper Classes/EmailMoveMonitorTests.cs:41`;
  `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs:136`.
  (These are the `"_dispatcher"` argument lines; the enclosing `GetField(` calls are one line
  earlier in each case.)
- **Primary Count.** **6** sites; the UtilitiesCS.Test subset is **4**.
- **Cross-check Search Strategy or Query Expression.** A structurally different query anchored on the
  *type* rather than the field name: Grep, regex `typeof\(\s*(UtilitiesCS\.)?UiThread\s*\)`, glob
  `**/*.cs`, repository-wide, with 3 lines of trailing context so the member name reached from each
  `typeof` is visible. This catches any reflective access to the type even if the field name were
  supplied by `nameof`, a constant, or a variable, and it independently surfaces reflection on *other*
  `UiThread` statics for subtraction.
- **Cross-check Member Set.** Seven `typeof(UiThread)` sites: `UiThread_Tests.cs:127` →
  `"_dispatcher"`; `ProgressTracker_Tests.cs:421` → `"_dispatcher"`;
  `ProgressTrackerAsync_Tests.cs:138` → `"_dispatcher"`; `IdleAsyncQueue_Tests.cs:144` →
  `"_dispatcher"`; `EmailMoveMonitorTests.cs:40` → `"_dispatcher"`;
  `QfcItemController.UiThreadDispatcherFixture.cs:135` → `"_dispatcher"`; and
  `UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorTests.cs:469-472` →
  **`"_uiSyncContext"`**, which is outside the family and is excluded.
- **Cross-check Count.** 7 `typeof(UiThread)` sites minus 1 non-`_dispatcher` target = **6**;
  UtilitiesCS.Test subset **4**.
- **Member-set Comparison.** Normalized to `<file>:<GetField-call line>`, the two member sets are
  identical: `UiThread_Tests.cs:127`, `ProgressTracker_Tests.cs:421`,
  `ProgressTrackerAsync_Tests.cs:138`, `IdleAsyncQueue_Tests.cs:144`, `EmailMoveMonitorTests.cs:40`,
  `QfcItemController.UiThreadDispatcherFixture.cs:135`. No element appears in one set only. Both
  counts agree at 6 total / 4 in UtilitiesCS.Test, matching `issue.md:36-37` exactly.
- **Assertion.** The `issue.md` figure ("Six independent reflection sites … four … in
  UtilitiesCS.Test") is correct and needs no correction.

### Claim 4 — #584 evidence files whose `EXIT_CODE:` is not a single integer: **15** (S3-5)

- **Complete Family.** Every `EXIT_CODE:` field in the 38-file #584 evidence tree whose value
  deviates from the schema's `EXIT_CODE: <int>`
  (`.claude/skills/evidence-and-timestamp-conventions/SKILL.md:111`).
- **Exhaustive Search Scope.** The whole `evidence/` subtree of the #584 folder.
- **Inclusion Rules.** A member is a file whose `EXIT_CODE:` line is (i) empty, with the value
  carried in a following bullet list, or (ii) an integer followed by parenthetical prose, or
  (iii) a non-numeric token.
- **Exclusion Rules.** Files whose line is exactly `EXIT_CODE: <int>` and nothing else; and
  `issue-updates/issue-584.2026-09-02T09-02.md`, which carries no `EXIT_CODE:` field and is exempt
  from that schema field (`SKILL.md:167-173`).
- **Primary Search Strategy or Query Expression.** Grep, regex `^EXIT_CODE:`, output mode `content`
  with line numbers, over the `evidence` path — returns the full line text for every artifact, from
  which the value shape is read directly.
- **Primary Member Set.**
  *Empty value with a following bullet list (11):* `qa-gates/p4-t6-quickfiler-tests.md:16`;
  `qa-gates/p2-t2-nullforgiving-removed.md:11`;
  `qa-gates/p2-t4-emailmovemonitor-reflection-target.md:18`;
  `qa-gates/p1-t5-donotparallelize.md:11`; `qa-gates/p4-t1-format.md:15`;
  `qa-gates/p3-t5-no-timing-tokens.md:12`; `other/p3-t4-progresstrackerasync-unmodified.md:13`;
  `other/p5-t10-footprint.md:11`; `baseline/p0-t13-parallel-bucket-census.md:13`;
  `baseline/p0-t14-reflective-dispatcher-census.md:12`; `baseline/p0-t5-toolchain-resolution.md:30`.
  *Integer plus parenthetical, or non-numeric (4):* `baseline/p0-t2-uithread-rederivation.md:11`
  (`EXIT_CODE: 0 (both commands)`); `baseline/p0-t3-progresstrackerasync-rederivation.md:12`
  (`EXIT_CODE: 0 (all three commands)`); `baseline/p0-t4-test-rederivation.md:13`
  (`EXIT_CODE: 0 (all four commands)`); `baseline/p0-t6-mcp-probe.md:12`
  (`EXIT_CODE: non-zero (tool invocation error; no exit code is returned by the MCP transport)`).
- **Primary Count.** 11 + 4 = **15**.
- **Cross-check Search Strategy or Query Expression.** A complementary query that enumerates the
  *conforming* members instead of the deviating ones, using a different regex anchored on the value
  shape rather than the field name: from the same `^EXIT_CODE:` result set, the members matching the
  strict form `^EXIT_CODE: -?[0-9]+$` were separated by reading each returned line, and the total
  population was independently fixed at 37 by Claim 2's cross-check. Conforming − total is then
  computed as the complement.
- **Cross-check Member Set (conforming, 22).** `regression-testing/p4-t6-first-pass-failure.md:13`
  (`1`); `regression-testing/p3-t6-quickfiler-wpfuidispatcher.md:10` (`0`);
  `regression-testing/p3-t3-at-risk-tests.md:10` (`0`);
  `regression-testing/p3-t2-regression-green.md:10` (`0`);
  `regression-testing/p1-t4-expect-fail.md:10` (`1`);
  `regression-testing/p1-t3-build-before-fix.md:10` (`0`); `qa-gates/p4-t8-loop-closure.md:11` (`0`);
  `qa-gates/p4-t7-coverage-delta.md:14` (`0`); `qa-gates/p4-t5-utilitiescs-tests.md:13` (`0`);
  `qa-gates/p4-t4-nullable-build.md:10` (`0`); `qa-gates/p4-t3-analyzer-build.md:10` (`0`);
  `qa-gates/p4-t2-format-check.md:10` (`0`); `qa-gates/p2-t3-file-size.md:13` (`0`);
  `qa-gates/p3-t1-analyzer-build.md:10` (`0`);
  `baseline/p0-t10-utilitiescs-tests-coverage.md:10` (`0`);
  `baseline/p0-t11-quickfiler-tests.md:10` (`0`);
  `baseline/p0-t12-threshold-reconciliation.md:11` (`0`); `other/p5-t12-ac-status-summary.md:10` (`0`);
  `baseline/p0-t7-csharpier-check.md:10` (`0`); `baseline/p0-t9-nullable-build.md:10` (`0`);
  `baseline/phase0-instructions-read.md:9` (`0`); `baseline/p0-t8-analyzer-build.md:10` (`0`).
- **Cross-check Count.** 37 files carrying an `EXIT_CODE:` field − 22 conforming = **15** deviating.
- **Member-set Comparison.** The union of the primary deviating set (15) and the cross-check
  conforming set (22) is exactly the 37-member population established independently in Claim 2, and
  their intersection is empty. Every file appears in exactly one of the two sets. The counts agree at
  **15**.
- **Assertion.** S3-5's remediation, as scoped, corrects 3 of the 15 deviations. The plan must either
  widen the scope to all 15 or record explicitly that the remaining 12 are knowingly left. See
  § Discrepancies D-3.

---

## Discrepancies with the requirements source

### D-1. `WpfDispatcherYield.cs` has ONE throw site, not two

- **Delegation prompt asserts:** "both throw sites verbatim with their message strings" in
  `WpfDispatcherYield.cs`.
- **Measured:** exactly one `throw` in that file, `WpfDispatcherYield.cs:64-66`. Verified by full
  read of all 77 lines.
- **Reconciliation:** C20's "route both throws through one shared message constant"
  (`pr-778-review-source.md:134`, `issue.md:52`) means `UiThread.cs:141-143` **and**
  `WpfDispatcherYield.cs:64-66` — two files, one throw each, same assembly. Neither figure is wrong;
  the delegation prompt's phrasing localizes both to one file. Recorded here so a plan task does not
  go looking for a second `throw` that does not exist.

### D-2. `InitializeAsync` does not throw synchronously

- **`issue.md:172` asserts:** "`ProgressTrackerAsync_Tests`: `InitializeAsync` with null dispatcher
  throws synchronously."
- **Measured:** `ProgressTrackerAsync.cs:31` is `public async Task<ProgressTrackerAsync>
  InitializeAsync()`. C# `async` methods capture all body exceptions into the returned `Task`; the
  guarded read at `:33` therefore faults the task rather than throwing at the call site.
- **Impact:** a C26 test written as a synchronous `Should().Throw<InvalidOperationException>()` would
  **fail**. It must be `await act.Should().ThrowAsync<InvalidOperationException>()` with
  `Func<Task> act = () => tracker.InitializeAsync();`.
- **What the review body actually said** (`pr-778-review-source.md:157`, C26): only that no test
  drives `InitializeAsync()` or `ProgressTracker.Initialize()` with a null dispatcher — it makes no
  synchrony claim. The synchrony claim was introduced in `issue.md`'s Test Conditions.
- **Note:** `ProgressTracker.Initialize()` (`ProgressTracker.cs:31`, non-async) **does** throw
  synchronously. If the plan wants a synchronous assertion it should add a second test there; that
  would also close C26's second named gap (`ProgressTracker.Initialize()`), which the current C26
  wording covers only in prose.

### D-3. S3-5 names three files; fifteen deviate

- **`issue.md:87` asserts:** "normalize `EXIT_CODE:` to a single integer in the **three** named
  evidence files."
- **Measured: 15** of the 37 #584 evidence files carrying an `EXIT_CODE:` field deviate from
  `EXIT_CODE: <int>`. Full member set and dual derivation in § Numeric Derivation Evidence, claim 4.
  The three named files are a subset. The twelve unnamed ones include `p4-t1-format.md:15`, which
  S3-2 already touches for an unrelated reason.
- **Decision the planner must take:** either (a) widen AC3's S3-5 clause to all 15 files, or (b) keep
  the three named files and record in the delivery's code-review artifact that twelve further
  deviations are knowingly left, with the list. Silently doing three and declaring S3-5 resolved
  would make AC3 misleading.

### D-4. The S3-9 follow-up is satisfied by C12/C13, not C26

- **`issue.md:91-92` asserts:** the ProgressTrackerAsync_Tests synchronization follow-up, "if not
  [promoted], is satisfied by C26 in this delivery".
- **Measured:** the follow-up as written in `policy-audit.2026-09-04T04-05.md:325-330` (finding F5)
  and `spec.md:77-80` asks for "**adding synchronization** around
  `UtilitiesCS.Test/Threading/ProgressTrackerAsync_Tests.cs`'s existing reflection-based,
  unsynchronized mutation of the shared static `UiThread._dispatcher`". C26 adds a *new test*
  (`InitializeAsync_WhenDispatcherNotCaptured_...`) and changes no existing mutation. The item that
  actually discharges F5 is **C12/C13** — the single shared install scope that all four
  UtilitiesCS.Test reflection sites, including `ProgressTrackerAsync_Tests.cs:138-141`, migrate to.
- **Impact:** if the artifacts record "satisfied by C26", a future reader auditing F5 will look at
  the wrong deliverable. The S3-9 artifact note should cite C12/C13 as the discharging item and may
  cite C26 as adjacent coverage.

### D-5. The verified S3-7 file count is 25, not 26

- **The PR review body (`pr-778-review-source.md:169`) states:** "a grep at the research base yields
  about **49** live reads in **26** production files."
- **Measured: 49 live reads in 25 production files**, by two independent enumerations that agree
  element-for-element (§ Numeric Derivation Evidence, claim 1).
- The read count agrees exactly. The file count differs by one. The most likely source of the extra
  file is one of the five whose only `UiThread.Dispatcher` occurrence is non-live
  (`EngineToggleStateCoordinator.cs`, `UiThread.cs`, `Theme.cs`, `IUiDispatcher.cs`,
  `QfcHomeController.Iteration.cs`), but that is inference, not evidence — **UNVERIFIED**, because
  the review's own member set is not recorded in the PR body.
- **Recommendation:** `spec.md` should carry **25**, with the derivation cited, and the delivery's
  review artifact should note the 25-vs-26 divergence rather than silently adopting either figure.

### D-6. `IdleActionQueue_Tests` is not `[DoNotParallelize]`; C14's cleanup touches shared globals

- **`issue.md:69-70` asserts:** "C14: add a `TestCleanup` to `IdleActionQueue_Tests` that drains
  entries and unsubscribes the heartbeat." No parallelization change is mentioned.
- **Measured:** the class carries `[TestClass]` only (`IdleActionQueue_Tests.cs:24-25`), and
  `ApplicationIdleTimer.Unsubscribe` (`ApplicationIdleTimer.cs:471-478`) calls `Stop()` →
  `instance.StopTimer()` (`:451-455`, `:159-182`) when the invocation list empties, mutating
  process-global `Application.Idle` and `ApplicationIdleTimer.Guard` state shared with
  `IdleAsyncQueue_Tests` and `ApplicationIdleTimer_Tests`.
- **Impact:** adding an unsubscribing cleanup to a parallel-bucket class can produce exactly the
  class of cross-class interference the file header of `ApplicationIdleTimer_Tests.cs:10-15`
  documents. The plan should pair C14 with `[DoNotParallelize]` on `IdleActionQueue_Tests`, or
  restrict the cleanup to draining `_entries` / resetting `_subscribeGuard` / cancelling
  `_unsubscribe` without unsubscribing the handler.

### D-7. `artifacts/csharp/coverage.xml` format

- **The delegation prompt asks:** "The delivery must produce `artifacts/csharp/coverage.xml` before
  feature review; state exactly how to get there, including any conversion step."
- **Measured:** the pipeline produces **Cobertura**; the consuming hook parses **JaCoCo** only
  (`.claude/hooks/validate-feature-review-coverage.ps1:216`, `:221-234`, `:186-206`). No committed
  converter exists (Grep for `jacoco` under `scripts/`: no files). Prior art for the manual
  conversion is
  `docs/features/archive/2026-08-08-wpf-dispatcher-yield-test-order-dependent-508/evidence/qa-gates/coverage-artifact-substitution.2026-08-08T17-30.md:72-84`.
- This is not a contradiction of the requirements source — the requirement is silent on format — but
  it is a step the plan must contain explicitly or the feature-review gate will read the artifact as
  absent.

---

## Open questions for the planner

1. **S3-5 scope (D-3).** Normalize all 15 deviating `EXIT_CODE:` fields, or the three named ones plus
   a recorded exception? This changes AC3's wording and the size of the documentation task.
2. **C06 test-method rename.** `UiThread_Tests.cs:134`
   (`Dispatcher_WhenBackingFieldIsNull_ThrowsInvalidOperationExceptionNamingInitialize`) encodes the
   old message target in its name. Renaming keeps the name honest but changes the fully-qualified
   test id recorded in the #584 evidence artifacts
   (`evidence/regression-testing/p1-t4-expect-fail.md:7` names it verbatim in a `/TestCaseFilter`).
   Rename, or leave and note?
3. **The `WpfDispatcherYield` domain tail.** Routing both throws through one constant necessarily
   drops "before yielding folder tree work" from the production message. Confirm that loss is
   intended (C20's text implies yes) and state it in an acceptance criterion.
4. **C12/C13 vs C16 ordering.** Both touch `ProgressTracker_Tests.cs`. Migrating the reflection first
   shrinks the file to ~508 lines (still over 500, so the split remains mandatory); splitting first
   means the migration then edits the new file. Either order is workable; pick one so the plan's
   task-level line-count assertions are stable.
5. **C14 parallelization (D-6).** Pair the new `TestCleanup` with `[DoNotParallelize]` on
   `IdleActionQueue_Tests`, or narrow the cleanup so it does not unsubscribe?
6. **`spec.md` AC block for S3-6.** The review body states all seven #584 ACs are checked while
   Status remains "Draft". I did not re-read the AC checkboxes line by line (UNVERIFIED), since the
   remedy is a Status change either way. If the plan wants to assert the AC state in an artifact, it
   must read `spec.md`'s AC block first.
7. **`plan.2026-09-02T09-02.md` line references.** S3-2's recommended section 8 entry cites the
   plan's P4-T1 rationale at lines 1068-1084, and C16 cites a "baseline + 1" clause at line 941.
   Both are taken from the review body and were **not** re-verified in this research (the plan file
   was not read). Confirm both line numbers before quoting them in an audit amendment.
