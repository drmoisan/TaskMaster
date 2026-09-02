# Research — Issue #729 "Bug: test-determinism-and-hygiene-debt"

Timestamp: 2026-09-02T09-30
Branch: `bug/test-determinism-and-hygiene-debt-729` (cut from `origin/main`; working tree clean at session start)
Governing rules read: `.claude/rules/general-unit-test.md` (§ Determinism Infrastructure), `.claude/rules/quality-tiers.md`, `CLAUDE.md`

## Executive Summary

Four findings were re-verified against the current tree. **Three of the four delegation premises were materially wrong** and are corrected below with citations:

| Finding | Delegation premise | Verified state |
|---|---|---|
| 1 | Add an optional `TimeProvider?` parameter to `NonBlockingDelay.WaitAsync` | **Would not compile.** `WaitAsync` is consumed as a *method group* at `StoreRehookCoordinator.cs:102`; C# forbids method-group conversion that omits optional parameters. An explicit overload pair is required. |
| 2 | `Form1`/`Form2` are compiled into `UtilitiesCS.Test` | **False.** `ResourceTests.cs`, `Form1/2/3.cs`, their Designers and `.resx` are **not in `UtilitiesCS.Test.csproj`** — orphan files. The real live violation is **`SVGControl.Test`**, which *does* compile `Form1.cs`/`Form2.cs`. |
| 3 | Two `[TestClass]`es conflict with each other | **False.** `UtilitiesCS.Test/OutlookObjects/DASLFilterParser_Tests.cs` is also **not in the csproj**. Only `DASLFilterParserTests.cs` is compiled. The real hazard is process-global `Console.Out` mutation against ~30 sibling classes. |
| 4 | Assess test-only fixability | **Confirmed non-fixable test-side.** Conclusion (b). Scope out; promote as a follow-up issue. |

---

## 0. Toolchain / Tier Context

- `quality-tiers.yml` **does not exist at the repository root**. Verified by `Glob quality-tiers.*` → only `.claude/rules/quality-tiers.md` matched. The tier-classification gate described in `.claude/rules/quality-tiers.md` therefore has no data file in this repository, and no tier can be cited for `UtilitiesCS`, `TaskMaster`, or `QuickFiler`. Per that rule the uniform gates (format 100%, 0 lint, 0 type errors, line >= 85%, branch >= 75%) apply regardless of tier, so no tier-specific obligation attaches to this work either way.
- CI test invocation (`.github/workflows/_mstest-coverage.yml:83`):
  `& $vstestPath $testAssemblies /EnableCodeCoverage /InIsolation /Logger:trx /TestCaseFilter:"TestCategory!=LiveOutlook"`
  **No `/Settings:` argument.** Consequences used throughout this document:
  - `UtilitiesCS.Test` parallelism comes from `UtilitiesCS.Test/Properties/AssemblyInfo.cs:18-21` (`[assembly: Parallelize(Workers = 0, Scope = ExecutionScope.ClassLevel)]`) and **is live in CI**.
  - `QuickFiler.Test/Properties/AssemblyInfo.cs` has **no** `Parallelize` attribute (verified, file read in full), so `QuickFiler.Test` runs **sequentially** in CI. `TaskMaster.runsettings` is not passed by CI.
- `BannedSymbols.txt` (repo root, 7 entries) bans `DateTime.Now`, `DateTime.UtcNow`, `Random.Shared`, `Thread.Sleep(int|TimeSpan)`, `Task.Delay(int|TimeSpan)`. `System.Diagnostics.Stopwatch` is **not** banned by the analyzer, so Finding 1 is a rule violation (`.claude/rules/general-unit-test.md:104` "real wall-clock waits") rather than an analyzer failure.
- **No `.csproj` in the solution sets `DocumentationFile` or `GenerateDocumentationFile`** (verified repo-wide; only `*.csproj.bak` files carry it). Therefore `CS0419` (ambiguous cref) cannot be emitted, and adding an overload to `WaitAsync` will not break the existing parameterless `<see cref="NonBlockingDelay.WaitAsync"/>` references.
- The `packages/` directory **does not exist in this worktree** (`Glob packages/*/` → no files). `nuget restore` (or a restore-on-build) is a prerequisite for any csproj/packages.config edit in Finding 1.

---

## 1. Finding 1 — `NonBlockingDelayTests` wall-clock wait

### 1.1 Current state (verified)

`TaskMaster.Test/AppGlobals/NonBlockingDelayTests.cs`:
- L38-39: `var interval = TimeSpan.FromMilliseconds(30); var stopwatch = Stopwatch.StartNew();`
- L42-44: `var waitTask = NonBlockingDelay.WaitAsync(interval); await waitTask; stopwatch.Stop();`
- L53-58: `stopwatch.Elapsed.Should().BeGreaterThanOrEqualTo(interval, ...)`

`TaskMaster/AppGlobals/NonBlockingDelay.cs`:
- L31: `internal static class NonBlockingDelay`
- L42: `public static Task WaitAsync(TimeSpan delay)`
- L55-64: `timer = new Timer(callback, null, delay, Timeout.InfiniteTimeSpan)` — raw `System.Threading.Timer`, no `TimeProvider`.
- L52-54 carry a narrow `#nullable enable annotations` / `#nullable restore annotations` pair around `Timer? timer = null`. Any edit must preserve this pragma pair or the file re-emits `CS8632` under the `TreatWarningsAsErrors` gate.

`TaskMaster.Test` visibility: `TaskMaster/Properties/AssemblyInfo.cs:38` and `TaskMaster/ThisAddIn.cs:14` both declare `[assembly: InternalsVisibleTo("TaskMaster.Test")]`, so the `internal static class` is already reachable.

### 1.2 Blocking constraint — method-group conversion (the delegation's proposed shape does not compile)

`TaskMaster/AppGlobals/StoreRehookCoordinator.cs`:
- L55: `private readonly Func<TimeSpan, Task> _delay;`
- L83: `Func<TimeSpan, Task>? delay = null` (constructor parameter)
- **L102: `_delay = delay ?? NonBlockingDelay.WaitAsync;`** — a method-group conversion to `Func<TimeSpan, Task>`.

The C# standard (dotnet/csharpstandard, `standard/conversions.md`, "Method group conversions") states that candidate methods "are ignored if they are applicable only in their expanded form, **or if one or more of their optional parameters do not have a corresponding parameter in `D`**". Adding `TimeProvider? timeProvider = null` as an optional trailing parameter to the single `WaitAsync` method therefore removes the only candidate and produces **CS0123** at `StoreRehookCoordinator.cs:102`.

**Recommended shape — two explicit overloads, no optional parameter:**

```
public static Task WaitAsync(TimeSpan delay)                       // 1-arg: delegates with TimeProvider.System
public static Task WaitAsync(TimeSpan delay, TimeProvider timeProvider)  // 2-arg: seam used by the test
```

Overload resolution for the `Func<TimeSpan, Task>` conversion selects the 1-arg overload unambiguously (the 2-arg overload has the wrong parameter count and is not compatible with `D`). `NonBlockingDelay` stays `internal static`; no class-shape change is needed.

The 2-arg body replaces `new Timer(...)` with `timeProvider.CreateTimer(callback, null, delay, Timeout.InfiniteTimeSpan)` returning `ITimer`. **In-repo compile proof of this exact API on net481**: `UtilitiesCS/Threading/ThreadMonitor.cs:43` (`private ITimer? _pollTimer;`) and L96-101 (`_pollTimer = _timeProvider.CreateTimer(_ => Tick(), null, TimeSpan.FromMilliseconds(pollingFrequency), Timeout.InfiniteTimeSpan);`). `ITimer` is `IDisposable`, so the existing `timer?.Dispose()` in the callback is unchanged.

### 1.3 Precedent pattern (verified)

`UtilitiesCS/Threading/ThreadMonitor.cs:64-82` — optional `TimeProvider? timeProvider = null` constructor parameter, `_timeProvider = timeProvider ?? TimeProvider.System;` (L78). `UtilitiesCS.Test/Threading/ThreadMonitorTests.cs:4` imports `Microsoft.Extensions.Time.Testing`; L44, L58, L63 use `new FakeTimeProvider()` and `fake.Advance(TimeSpan.FromMilliseconds(...))`.

Note the constructor-injection form is safe there only because a *constructor* is never converted to a delegate. That is why the same optional-parameter shape cannot be reused for `WaitAsync`.

### 1.4 `FakeTimeProvider` timer semantics (verified against upstream source)

From `dotnet/extensions` `FakeTimeProvider.cs`:
- `CreateTimer(callback, state, dueTime, period)` constructs the fake timer and calls `Change(dueTime, period)`; it does **not** invoke the callback at creation.
- `Advance(TimeSpan delta)` guards `delta.Ticks >= 0` (so `TimeSpan.Zero` is legal), advances `_now`, then calls `WakeWaiters()`, which fires every waiter whose wake-up time is at or before `_now`.

Test consequence: a zero-due-time timer fires on the **next** `Advance`/`SetUtcNow`, not at creation. The rewritten tests must therefore be ordered *start task → advance → await*:

- `WaitAsync_WithNoDispatcher_CompletesAfterInterval`: start the task, assert it is **not** completed, `fake.Advance(interval)`, then `await`. This is a strictly stronger assertion than the current `Stopwatch` check (it proves the task cannot complete early) and removes the `Stopwatch` entirely.
- `WaitAsync_ZeroDelay_CompletesWithoutPump`: start the task, `fake.Advance(TimeSpan.Zero)`, then `await`. If the comparison in `WakeWaiters` should prove to be strict rather than inclusive, `fake.Advance(TimeSpan.FromTicks(1))` is the equivalent fallback; this is the single point in the plan that must be confirmed by an actual test run.

`TaskCreationOptions.RunContinuationsAsynchronously` (NonBlockingDelay.cs:44-46) means the `await` continuation is scheduled off the advancing thread, so no re-entrancy deadlock occurs when `Advance` runs the callback inline.

The existing `[Timeout(5000)]` attributes stay: after the fix they are a harness deadlock bound, not a wait.

### 1.5 Exact project-file edits required for `TaskMaster.Test`

Verified absent today: `TaskMaster.Test/packages.config` contains **neither** `Microsoft.Bcl.TimeProvider` **nor** `Microsoft.Extensions.TimeProvider.Testing` (full file read, 169 lines); `TaskMaster.Test/TaskMaster.Test.csproj` has no `Reference` for either.

Package dependency check: `Microsoft.Extensions.TimeProvider.Testing` 10.9.0 declares exactly one net462 dependency — `Microsoft.Bcl.TimeProvider >= 8.0.1` (nuget.org). The repo pin of 10.0.11 satisfies it. Two package entries only.

`TaskMaster.Test/packages.config` — insert (CSharpier's packages.config formatting: single-line when short, wrapped when long; mirror `UtilitiesCS.Test/packages.config:23` and `:90-94`):

- After line 17 (`Microsoft.Bcl.AsyncInterfaces`), before line 18 (`Microsoft.CodeAnalysis.BannedApiAnalyzers`):
  `  <package id="Microsoft.Bcl.TimeProvider" version="10.0.11" targetFramework="net481" />`
- After line 82 (`Microsoft.Extensions.Primitives`), before line 83 (`Microsoft.Identity.Client`):
  ```
    <package
      id="Microsoft.Extensions.TimeProvider.Testing"
      version="10.9.0"
      targetFramework="net481"
    />
  ```

`TaskMaster.Test/TaskMaster.Test.csproj` — insert (verbatim mirror of `UtilitiesCS.Test.csproj:591-592` and `:643-644`):

- After line 73 (`</Reference>` closing `Microsoft.Bcl.AsyncInterfaces`), before line 74 (`<Reference Include="Microsoft.Build" />`):
  ```
      <Reference Include="Microsoft.Bcl.TimeProvider, Version=10.0.0.11, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51, processorArchitecture=MSIL">
        <HintPath>..\packages\Microsoft.Bcl.TimeProvider.10.0.11\lib\net462\Microsoft.Bcl.TimeProvider.dll</HintPath>
      </Reference>
  ```
- After line 121 (`</Reference>` closing `Microsoft.Extensions.Primitives`), before line 122 (`Microsoft.Identity.Client`):
  ```
      <Reference Include="Microsoft.Extensions.TimeProvider.Testing, Version=10.9.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35, processorArchitecture=MSIL">
        <HintPath>..\packages\Microsoft.Extensions.TimeProvider.Testing.10.9.0\lib\net462\Microsoft.Extensions.TimeProvider.Testing.dll</HintPath>
      </Reference>
  ```

**No `app.config` edit is required.** `TaskMaster.Test/app.config:265-271` already carries the `Microsoft.Bcl.TimeProvider` binding redirect (`oldVersion="0.0.0.0-10.0.0.11" newVersion="10.0.0.11"`). No project in the repo declares a redirect for `Microsoft.Extensions.TimeProvider.Testing`, so none is needed.

`.csharpierignore` keeps `*.csproj` out of the format check, but **`packages.config` is formatted by CSharpier 1.2.6** (per `CLAUDE.md` § C#1.1). Run `dotnet tool run csharpier format .` after editing it.

### 1.6 Rejected alternatives

- Optional trailing `TimeProvider?` parameter — rejected, breaks `StoreRehookCoordinator.cs:102` (CS0123), §1.2.
- Convert `NonBlockingDelay` to an instance class with an injected provider — rejected: forces edits to two production call sites plus the delegate default, for no test benefit over the overload pair.
- A settable `internal static TimeProvider` on `NonBlockingDelay` — rejected: mutable process-global state, prohibited by `.claude/rules/general-unit-test.md` § External Dependencies, and would reintroduce a cross-class ordering hazard identical to Finding 3.

---

## 2. Finding 2 — live `Form` types in a unit-test assembly

### 2.1 The stated site is orphan source, not compiled code

`UtilitiesCS.Test/ResourceTests.cs` exists on disk with `[Ignore]`d `TestMethod1`/`TestMethod2`/`TestMethod5` constructing `Form1`/`Form2` and calling `ShowDialog()` (L20-21, L28-29, L111-112). **However:**

- `UtilitiesCS.Test.csproj` uses an explicit `<Compile Include=.../>` list; there is **no wildcard include** (`Grep Include="\*|Include=".*\*` → no matches) and no SDK-style implicit globbing (legacy project, `Import Microsoft.CSharp.targets` at line 936).
- `Grep ResourceTests\.cs|Form1\.cs|Form2\.cs|Form3\.cs|Form[123]\.resx` across **all** `*.csproj` returns matches **only in `SVGControl.Test/SVGControl.Test.csproj`**. `UtilitiesCS.Test.csproj` contains none of them.
- Of the 21 `.cs` files at the `UtilitiesCS.Test/` root, only `TestAssemblyInitializer.cs` appears in the csproj (line 76). `SerializableListTest.cs` and `DeedleTests.cs` are compiled from their `HelperClasses\`/`Extensions\` copies, confirming a historical file move that left the root copies stranded.

**Conclusion:** `ResourceTests`, `Form1`, `Form2`, `Form3` are not in the `UtilitiesCS.Test` assembly. There is no live `Form` in `UtilitiesCS.Test` today, and no `ShowDialog()` can execute from it. A ported `NoLiveFormInTestAssemblyTests` guard would be **green from birth** in `UtilitiesCS.Test` — it is a regression-prevention guard, **not** a fail-before regression test. The plan must state this explicitly rather than claim a red-before run.

There is also a third form, **`Form3`**, not mentioned in the delegation: `UtilitiesCS.Test/Form3.cs:13` (`public partial class Form3 : Form`), plus `Form3.Designer.cs` and `Form3.resx`. Any cleanup task that omits it leaves the hygiene debt half-removed.

### 2.2 The real live violation is `SVGControl.Test`

`SVGControl.Test/SVGControl.Test.csproj:54-66` compiles `Form1.cs` (`<SubType>Form</SubType>`), `Form1.Designer.cs`, `Form2.cs`, `Form2.Designer.cs`; L86-91 embed `Form1.resx`/`Form2.resx`. `SVGControl.Test` is in the solution (`TaskMaster.sln:42`) and its assembly is picked up by CI's `Get-ChildItem -Filter '*.Test.dll'` sweep (`_mstest-coverage.yml:70`).

`SVGControl.Test/Form1.cs:13` and `Form2.cs:13` both declare `: Form`. **No test in `SVGControl.Test` references `Form1` or `Form2`** (`Grep Form1|Form2|ShowDialog` over `SVGControl.Test/*.cs` matches only the four form/designer files themselves). They are unreferenced compiled dead weight.

Because they are compiled and unreferenced, porting the guard to `SVGControl.Test` is a **genuine red-before / green-after regression test**: the guard fails today (two `Form`-derived types), and passes once the four `<Compile>` entries and two `<EmbeddedResource>` entries are removed and the six files deleted.

### 2.3 Porting the guard

`QuickFiler.Test/NoLiveFormInTestAssemblyTests.cs` is portable verbatim apart from the namespace. Required usings (already in the source): `System`, `System.Linq`, `System.Reflection`, `FluentAssertions`, `Microsoft.VisualStudio.TestTools.UnitTesting`. It references `System.Windows.Forms.Form` by fully-qualified name (L20), so no `using System.Windows.Forms;` is needed. The `GetLoadableTypes` `ReflectionTypeLoadException` fallback (L42-52) should be carried over unchanged — `UtilitiesCS.Test` has a very large reference surface and is the assembly most likely to need it.

For `UtilitiesCS.Test`: namespace becomes `UtilitiesCS.Test`; add a `<Compile Include=...>` entry. `UtilitiesCS.Test` already references `FluentAssertions` and MSTest.
For `SVGControl.Test`: confirm `FluentAssertions` and MSTest references exist in `SVGControl.Test.csproj` before porting (its reference list is much smaller); if `FluentAssertions` is absent, either add the package or use MSTest `Assert` (permitted by `CLAUDE.md` § CUT2 when FluentAssertions is not practical).

### 2.4 Recommended disposition

1. Delete the 10 orphan files in `UtilitiesCS.Test/`: `ResourceTests.cs`, `Form1.cs`, `Form1.Designer.cs`, `Form1.resx`, `Form2.cs`, `Form2.Designer.cs`, `Form2.resx`, `Form3.cs`, `Form3.Designer.cs`, `Form3.resx`. No csproj edit is needed (none are referenced). Deleting the whole set is preferable to gutting `[Ignore]`d method bodies, because gutting leaves the `Form` sources on disk one csproj line away from re-entering the assembly.
2. Add the ported guard to `UtilitiesCS.Test`, documented as green-from-birth regression prevention.
3. **Decide scope for `SVGControl.Test`.** It is the only live violation and is where the fail-before evidence exists. It is not named in issue #729. Recommendation: include it, because excluding it means #729 ships a guard in the assembly that was already clean while leaving the dirty assembly untouched. If the orchestrator prefers strict issue fidelity, promote it as a follow-up issue and say so in the spec.

---

## 3. Finding 3 — parallel-execution hazard in the DASL parser tests

### 3.1 Corrected premise

`UtilitiesCS.Test/OutlookObjects/DASLFilterParser_Tests.cs` and `UtilitiesCS.Test/OutlookObjects/Filter DASL/DASLFilterParserTests.cs` are byte-identical apart from namespace and class name (both 118 lines; same six test methods, same bodies). **Only the latter is compiled** — `UtilitiesCS.Test.csproj:270` includes `OutlookObjects\Filter DASL\DASLFilterParserTests.cs`, and `Grep DASLFilterParser_Tests` against the csproj returns no match.

Therefore there is **no class-versus-class conflict between these two files**, because one of them is not in the assembly. The delegation's framing of the hazard is wrong.

### 3.2 The actual shared resource

`DASLFilterParserTests.PrintTree_WritesIndentedTreeToConsole` (L94-116) mutates **process-global `System.Console.Out`**:

```
L100  using var writer = new StringWriter();
L101  var originalOut = Console.Out;
L102  Console.SetOut(writer);
L107      parser.PrintTree(tree, 0);
L111      Console.SetOut(originalOut);
L115  writer.ToString().Should().Contain("AND").And.Contain("  A").And.Contain("  B");
```

The redirect is unavoidable at the test level because the production method writes directly to the console: `UtilitiesCS/OutlookObjects/Filter DASL/DASLFilterParser.cs:97-104`, `public void PrintTree(TreeNode<string> node, int level) { Console.WriteLine(...); ... }` — there is no `TextWriter` parameter to inject.

Under `[assembly: Parallelize(Workers = 0, Scope = ExecutionScope.ClassLevel)]` (`UtilitiesCS.Test/Properties/AssemblyInfo.cs:18-21`, live in CI per §0), this class runs concurrently with roughly thirty sibling classes that call `Console.SetOut(...)` in `[TestInitialize]` (e.g. `Threading/AppGlobalsConverterTests.cs:27`, `NewtonsoftHelpers/*.cs`, `EmailIntelligence/Bayesian/*.cs`). Two concrete failure modes:

1. **Swallowed output.** A sibling's `Console.SetOut(new DebugTextWriter())` executes between L102 and L107, so `PrintTree`'s output lands in the sibling's writer, `writer.ToString()` is empty, and the assertion at L115 fails.
2. **Stale-writer leak.** If a sibling captures `Console.Out` while this test's `StringWriter` is installed, its own restore reinstalls that `StringWriter` *after* the `using` at L100 has disposed it. Every later `Console.Write` in the process then throws `ObjectDisposedException`, turning one interleaving into a cascade of unrelated failures.

### 3.3 The repo already has the remedy and its precedent

Two `UtilitiesCS.Test` classes already carry `[DoNotParallelize]` with a comment naming this exact hazard:

- `UtilitiesCS.Test/HelperClasses/PrettyPrint_Tests.cs:14-20` — "captures and restores `Console.Out`, which is process-wide state. Under the class-level parallel scope ... a sibling test class's `Console.SetOut` overrides this class's redirect mid-test".
- `UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensions_Tests.cs:17-21` — "redirects `Console.Out`, which is process-wide state. Under class-level parallel execution another test class can replace the writer mid-test and make the captured output empty."

`[DoNotParallelize]` is in `Microsoft.VisualStudio.TestTools.UnitTesting`, already imported by both target files (`DASLFilterParserTests.cs:4`). MSTest partitions the assembly into a parallel set and a non-parallel set and runs the non-parallel set serially, so marking a class removes it from every concurrent window.

### 3.4 A third unprotected class exists

`UtilitiesCS.Test/ReusableTypeClasses/StackGeek_Tests.cs` is compiled (`UtilitiesCS.Test.csproj:431`), declares `[TestClass]` at L9 with **no** `[DoNotParallelize]`, and `Main_RunsSampleScenarioWithoutThrowing` (L139-161) performs the identical capture-assert-restore on `Console.Out` (L144, L146, L155, asserting at L159-160). It is exposed to exactly the same two failure modes.

`UtilitiesCS.Test/HelperClasses/NLogTraceWriter_Test.cs` (csproj:73) is a related but distinct case: it captures `originalOut` in `[TestInitialize]` (L22-23) and restores it in `[TestCleanup]` (L56), but installs a `DebugTextWriter` and asserts through Moq rather than on captured text. It cannot fail from a swallowed redirect, but it *is* a stale-writer-leak source (failure mode 2). It is listed here for completeness; marking it is optional and not recommended as part of this fix, since it would serialize a class with no failing mode of its own.

### 3.5 Recommended disposition

- Add `[DoNotParallelize]` plus a one-line hazard comment (matching the `PrettyPrint_Tests` / `OlTableExtensions_Tests` wording) to **`DASLFilterParserTests`** and **`StackGeek_Tests`**.
- Delete the orphan duplicate `UtilitiesCS.Test/OutlookObjects/DASLFilterParser_Tests.cs`. Leaving it invites a future contributor to add it to the csproj, at which point two identically-named test methods in two classes both mutate `Console.Out`.
- Do **not** attempt to seam `DASLFilterParser.PrintTree` to accept a `TextWriter`. That is a `UtilitiesCS` production API change with no defect behind it, outside the bugfix minimal-change rule.

Fail-before evidence: this hazard is a race, so a deterministic red run is not producible. The plan should record a fail-before exception dossier (`evidence/regression-testing/fail-before-exception.<timestamp>.md`) with `WhyFailingRunImpossible` stating that the failure requires a specific interleaving of `Console.SetOut` across two threads, and use the two in-repo precedent comments as the alternative proof that the hazard is real and previously observed.

---

## 4. Finding 4 — `PumpTimeoutMs = 60000` load sensitivity

### 4.1 What the constant actually is

`PumpTimeoutMs` is used in **exactly 19 places, all of them the argument of an MSTest `[Timeout(...)]` attribute**, plus 4 declarations. It is **never** used as a wait duration, a poll interval, or a `WaitOne`/`Wait` argument. Verified by `Grep PumpTimeoutMs` over `QuickFiler.Test` (full result set, 23 lines: 4 declarations + 19 `[Timeout(PumpTimeoutMs)]` usages).

Every wait inside these tests is on a `TaskCompletionSource`-backed `Task` completed by the pump — see `WinFormsPumpHost.CreateCompletion<TResult>()` (`QuickFiler.Test/TestSupport/WinFormsPumpHost.cs:364-365`) and the `Post`/`RunAsync` members. There is no `Thread.Sleep`, no `Task.Delay`, no `Stopwatch`, and no polling loop. **The test logic is already deterministic in the sense the determinism rule requires** (`.claude/rules/general-unit-test.md:104` bans `Thread.Sleep`, `Task.Delay`, real wall-clock waits, `Date.now()` — none is present).

The failure mode is narrower than "non-deterministic tests": under CPU contention the *real elapsed cost of the work under test* can exceed the 60 s harness bound and MSTest aborts an otherwise-correct test.

### 4.2 Why the pump cannot be faked from the test side

`WinFormsPumpHost` is test-owned (`QuickFiler.Test/TestSupport/WinFormsPumpHost.cs`, compiled at `QuickFiler.Test.csproj:202`), so replacing it is *permissible* under the file-ownership constraint. It is nevertheless not *sufficient*, for four independently verified reasons:

1. **The production code reads the context off the control, not from an injected seam.** `QuickFiler/Viewers/ItemViewer.cs:23-29`:
   ```
   public ItemViewer() { InitializeComponent(); _context = SynchronizationContext.Current; _uiDispatcher = Dispatcher.CurrentDispatcher; InitControlGroups(); }
   ```
   with `UiSyncContext => _context` (L59-62), exposed read-only through `IItemViewer.UiSyncContext` (`QuickFiler/Viewers/IItemViewer.cs:37`). The members under test await it directly — `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:64` (`await _itemViewer.UiSyncContext;`), L320, L331, L336. To substitute a synchronous fake context the *viewer* must be constructed on a thread where that fake is `SynchronizationContext.Current`, which means the ItemViewer must be constructed off the pump. That is a production-behaviour dependency, not a test-harness one.

2. **The fixture's cost is the real WinForms control tree, not the pump.** `QuickFiler.Test/Controllers/QfcItemController.InitializationTests.Part2.cs:74` constructs a real `QuickFiler.ItemViewer` on the pump, and L84 forces `viewer.Handle`. The in-file comment at L77-83 records the measured fact (2026-08-22) that "**both WebView2 children — and therefore the parent ItemViewer — are already handle-created when construction returns**". Constructing two WebView2 controls plus their WinForms handles is the dominant cost, and it is unaffected by any change to the pump implementation. `QuickFiler.Test/Viewers/WebView2BreadcrumbHostTests.cs:38`, `:88` construct `new WebView2()` for the same reason. A fake `SynchronizationContext` cannot service WinForms handle creation or `Control.BeginInvoke` marshalling; only a real Win32 message loop can.

3. **`[DoNotParallelize]` would be a no-op.** `QuickFiler.Test/Properties/AssemblyInfo.cs` declares no `Parallelize` attribute and CI passes no `/Settings:` (§0), so `QuickFiler.Test` already runs serially. The contention comes from the runner host and from `/EnableCodeCoverage` instrumentation, neither of which an in-repo attribute can influence. (Two classes already carry `[DoNotParallelize]` — `Helper Classes/ViewerQueueStaticWrapperTests.cs:11`, `Helper Classes/EmailMoveMonitorTests.cs:22` — for static-state reasons, not timing.)

4. **Removing `[Timeout]` trades a bounded failure for an unbounded hang.** The attribute's documented purpose is stated in-file at `QfcItemController.InitializationTests.cs:33-37`: it "only converts a genuine deadlock in production code into a test failure instead of a CI hang." Deleting it would let a real deadlock consume the workflow's 30-minute budget instead.

### 4.3 Definitive recommendation — conclusion (b)

**No test-only change removes the load sensitivity.** The only durable fix is a `QuickFiler/` production seam: give the members under test an injectable UI-marshalling abstraction (an `IUiDispatcher`/`SynchronizationContext` parameter or settable seam on `QfcItemController`, and an interface over the `WebView2` control accepted by `WebView2BreadcrumbHost`'s constructor) so a synchronous fake can replace the message loop entirely. `QuickFiler/` production sources are owned by other parallel work items in this run.

**Action:** scope Finding 4 **out** of issue #729's spec and promote it as its own follow-up issue via the potential-to-issue lifecycle. The follow-up should carry the four points above verbatim as its evidence, and should note that issue #711 was already closed as "superseded by #729", so closing #729 without either fixing or re-promoting this finding would silently drop it a second time.

**Permitted but non-remedial hygiene, if the orchestrator wants any test-side change here:** the constant is declared four times — `QfcItemController.InitializationTests.cs:38` (`internal const`, consumed by `.Part3.cs`), `QfcItemController.SeamFactoryTests.cs:327` (`private const`), `QfcItemController.ViewerSetupTests.cs:34` (`private const`), `WebView2BreadcrumbHostTests.cs:25` (`private const`). Hoisting one `internal const int PumpTimeoutMs` into `QuickFiler.Test/TestSupport/` and referencing it from all four gives a single source of truth. This is duplication hygiene only; it does not address the finding and must not be presented as doing so.

---

## 5. Numeric Derivation Evidence

### N1 — Production call sites of `NonBlockingDelay.WaitAsync`

- **Complete Family:** every reference to any member of `TaskMaster.NonBlockingDelay` from non-test C# source in the repository, in all forms (direct invocation, method-group conversion, XML-doc `cref`).
- **Exhaustive Search Scope:** all `*.cs` files in the repository working tree, excluding `docs/` prose; both the invocation form `WaitAsync(` and the bare method-group form `WaitAsync` without parentheses must be covered, since a bare-identifier form is the one that constrains the signature change.
- **Inclusion Rules:** references in production (non-`*.Test`) projects that bind to the `WaitAsync` member.
- **Exclusion Rules:** the declaration itself (`NonBlockingDelay.cs:31`, `:42`); XML-doc `cref` text (no binding constraint on the parameter list once `DocumentationFile` is unset, §0); references inside `*.Test` projects; Markdown in `docs/`.
- **Primary Search Strategy:** `Grep pattern="NonBlockingDelay" glob="*.cs"` — deliberately matches the *type* name, not the member name, so both invocation and method-group forms and any `using`-aliased form are captured.
- **Primary Member Set:**
  1. `TaskMaster/AppGlobals/StoreRehookCoordinator.cs:102` — `_delay = delay ?? NonBlockingDelay.WaitAsync;` (method group → `Func<TimeSpan, Task>`)
  2. `TaskMaster/AppGlobals/AppEvents.cs:456` — `await NonBlockingDelay.WaitAsync(TimeSpan.FromMilliseconds(100));` (invocation)
  Non-members observed and excluded: `NonBlockingDelay.cs:31`/`:42` (declaration), `StoreRehookCoordinator.cs:72` and `AppEvents.cs:452` (comment/cref), `TaskMaster.Test/AppGlobals/NonBlockingDelayTests.cs:11,24,42,71` (test project), 4 comment-only hits in `QuickFiler.Test`.
- **Primary Count:** 2
- **Cross-check Search Strategy:** independent strategy — enumerate the *consumers by type shape* rather than by identifier: `Grep pattern="Func<TimeSpan,\s*Task>"` over `TaskMaster/` to find every delegate field/parameter that a `WaitAsync` method group could bind to, plus reading `StoreRehookCoordinator.cs:55-103` and `AppEvents.cs:440-460` in full to confirm each binding site. This finds delegate-typed consumers that an identifier grep would miss if the method were reached through an alias or a local delegate variable.
- **Cross-check Member Set:** `StoreRehookCoordinator.cs:55` (`private readonly Func<TimeSpan, Task> _delay;`) bound at `:102`; `StoreRehookCoordinator.cs:83` (`Func<TimeSpan, Task>? delay = null`) is the injected override, not a `WaitAsync` binding. The `AppEvents.cs:456` site is a direct `await` of the invocation form, confirmed by reading L440-460. Set = { `StoreRehookCoordinator.cs:102`, `AppEvents.cs:456` }.
- **Cross-check Count:** 2
- **Member-set Comparison:** normalized primary set `{StoreRehookCoordinator.cs:102, AppEvents.cs:456}` is identical to the normalized cross-check set. **Agreement.** Assertion admitted: *`NonBlockingDelay.WaitAsync` has exactly 2 production call sites, exactly 1 of which (`StoreRehookCoordinator.cs:102`) is a method-group conversion and therefore forbids an added optional parameter.*

### N2 — `System.Windows.Forms.Form`-derived types in `UtilitiesCS.Test`

- **Complete Family:** every type compiled into the `UtilitiesCS.Test` assembly that is assignable to `System.Windows.Forms.Form`, including types deriving indirectly via a `Form`-derived base declared in `UtilitiesCS` or any other referenced assembly.
- **Exhaustive Search Scope:** all class declarations under `UtilitiesCS.Test/`, cross-referenced against `UtilitiesCS.Test.csproj`'s `<Compile Include>` list; the base-type candidate list must include every `Form`-derived type in `UtilitiesCS`, not just the literal token `Form`.
- **Inclusion Rules:** a type is a member only if (a) its declaration derives directly or transitively from `System.Windows.Forms.Form`, **and** (b) its source file appears in `UtilitiesCS.Test.csproj`.
- **Exclusion Rules:** `UserControl`-derived types (not `Form`-derived); types in the referenced `UtilitiesCS` production assembly (the guard scopes to `Assembly.GetExecutingAssembly()`); files present on disk but absent from the csproj.
- **Primary Search Strategy:** two-stage. Stage 1 enumerates every `Form`-derived type in `UtilitiesCS` (`Grep "class\s+\w+\s*:\s*[^\n{;=]*\b(Form|MyBox|MyBoxBase)\b"` over `UtilitiesCS/`) to build the transitive base-type candidate list (20 types: `MyBoxViewer`, `SubjectMapMetrics`, `InputBoxViewer`, `FolderNotFoundViewer`, `DgvForm`, `DelegateButtonTemplate`, `DisabledStoresViewer`, `SyncContextForm`, `ProgressViewer`, `StoreWrapperViewer`, `FolderSelector`, `FolderRemapViewer`, `ProgressMultiStepViewer`, `OSFolder`, `FilterOlFoldersViewer`, `OSBrowser`, `FolderInfoViewer`, `MetricChartViewer`, `ConfusionViewer`, `ConfigViewer`). Stage 2 greps `UtilitiesCS.Test/` for `class \w+ : <any of Form | those 20 names>`.
- **Primary Member Set (declarations found):** `UtilitiesCS.Test/Form1.cs:13`, `UtilitiesCS.Test/Form2.cs:13`, `UtilitiesCS.Test/Form3.cs:13`. **Compiled subset after applying inclusion rule (b):** empty — none of `Form1.cs`, `Form2.cs`, `Form3.cs` appears in `UtilitiesCS.Test.csproj` (`Grep Form1\.cs|Form2\.cs|Form3\.cs` over all `*.csproj` matches only `SVGControl.Test.csproj`).
- **Primary Count:** 3 declared on disk; **0 compiled**.
- **Cross-check Search Strategy:** independent structural strategy — every WinForms designer-backed type must have a `.Designer.cs` partial and call `InitializeComponent()`. `Glob "UtilitiesCS.Test/**/*.Designer.cs"` enumerates designer partials, and `Grep "InitializeComponent"` over `UtilitiesCS.Test/` enumerates every type that initializes a designer surface. This catches a `Form` subclass that a base-name grep would miss (e.g. one declared across a partial where the base clause lives in the Designer file).
- **Cross-check Member Set:** `Glob` → `Form1.Designer.cs`, `Form2.Designer.cs`, `Form3.Designer.cs`, `Properties/Resources.Designer.cs` (the last is a `ResXFileCodeGenerator` output, not a control). `Grep InitializeComponent` → 9 files: `Form1.cs`, `Form1.Designer.cs`, `Form2.cs`, `Form2.Designer.cs`, `Form3.cs`, `Form3.Designer.cs`, plus `Threading/ProgressViewer_Tests.cs`, `ReusableTypeClasses/ConfigController_Tests.cs`, `OutlookObjects/Store/StoreWrapperController_Tests.ButtonAndPopulate.cs` — the last three *invoke* `InitializeComponent` on production viewers via reflection/instantiation and declare no `Form` subclass of their own. Designer-backed type set = { `Form1`, `Form2`, `Form3` }. `Properties/Resources.Designer.cs` is the only one of the four designer partials that is in the csproj (line 403), and it is not a control.
- **Cross-check Count:** 3 declared on disk; **0 compiled**.
- **Member-set Comparison:** normalized primary set `{Form1, Form2, Form3}` equals the normalized cross-check set `{Form1, Form2, Form3}`; both strategies independently return an empty compiled subset. **Agreement.** Assertion admitted: *`UtilitiesCS.Test` declares exactly 3 `Form`-derived types on disk (`Form1`, `Form2`, `Form3`) and compiles exactly 0 of them; the ported structural guard is therefore green from birth in that assembly.*

### N3 — `Form`-derived types compiled into `SVGControl.Test`

- **Complete Family:** every type compiled into the `SVGControl.Test` assembly assignable to `System.Windows.Forms.Form`.
- **Exhaustive Search Scope:** all `<Compile Include>` entries in `SVGControl.Test/SVGControl.Test.csproj` (the project has a single, short, fully-enumerated compile list, L54-77 — 8 entries), cross-referenced with the class declarations in each named file.
- **Inclusion Rules / Exclusion Rules:** as N2.
- **Primary Search Strategy:** read `SVGControl.Test/SVGControl.Test.csproj` L54-96 in full and classify each of the 8 `<Compile>` entries.
- **Primary Member Set:** `Form1.cs` (`<SubType>Form</SubType>`, L55-57) → `Form1`; `Form2.cs` (`<SubType>Form</SubType>`, L61-63) → `Form2`. The other six entries are `Form1.Designer.cs`, `Form2.Designer.cs` (partials of the same two types), `GetRelativePath_Test.cs`, `RelativePathCoverageTests.cs`, `SvgAssemblyProbeDirectoryTests.cs`, `SvgRendererNullToleranceTests.cs`, `SvgRendererParseContractTests.cs`, `Properties/AssemblyInfo.cs`, `Resources.Designer.cs` — none `Form`-derived.
- **Primary Count:** 2
- **Cross-check Search Strategy:** independent source-declaration strategy — `Grep "Form1|Form2|ShowDialog"` over `SVGControl.Test/*.cs`, which enumerates declarations and every usage site regardless of csproj membership, plus the `<EmbeddedResource>` list (L86-91) which must contain one `.resx` per designer-backed form.
- **Cross-check Member Set:** declarations `SVGControl.Test/Form1.cs:13` (`public partial class Form1 : Form`) and `SVGControl.Test/Form2.cs:13` (`public partial class Form2 : Form`); `EmbeddedResource` entries `Form1.resx` (L86-88) and `Form2.resx` (L89-91). **Zero usage sites** — no test file constructs or calls `ShowDialog` on either.
- **Cross-check Count:** 2
- **Member-set Comparison:** normalized primary set `{Form1, Form2}` equals normalized cross-check set `{Form1, Form2}`. **Agreement.** Assertion admitted: *`SVGControl.Test` compiles exactly 2 `Form`-derived types, both unreferenced by any test; a ported structural guard fails red there today and passes green after their 4 `<Compile>` and 2 `<EmbeddedResource>` entries are removed.*

### N4 — `UtilitiesCS.Test` compiled classes that capture and restore `Console.Out` and assert on the captured text

- **Complete Family:** every `[TestClass]` compiled into `UtilitiesCS.Test` that (a) reads `Console.Out` into a local/field, (b) installs a `StringWriter`/`TextWriter` via `Console.SetOut`, (c) restores the captured writer, and (d) asserts on the captured text. This is the exact set exposed to both the swallowed-output and stale-writer-leak failure modes of §3.2.
- **Exhaustive Search Scope:** all `*.cs` in the repository for the three `Console` members (`Console.SetOut`, `Console.SetError`, `Console.Out`) — a search restricted to `Console.SetOut` alone would miss a class that reads `Console.Out` without setting it — then narrowed to `UtilitiesCS.Test` files present in `UtilitiesCS.Test.csproj`, then narrowed by the assert-on-captured-text criterion.
- **Inclusion Rules:** all four of (a)–(d) hold, and the file is in the csproj.
- **Exclusion Rules:** classes that install a `DebugTextWriter` in `[TestInitialize]` without asserting on captured text (failure mode 2 only — listed separately); orphan files not in the csproj; production files; commented-out calls.
- **Primary Search Strategy:** `Grep pattern="Console\.SetOut|Console\.SetError|Console\.Out" glob="*.cs"` repo-wide (60 hits), then filter to `UtilitiesCS.Test`, then check csproj membership and read each candidate's assertion.
- **Primary Member Set:**
  1. `HelperClasses/PrettyPrint_Tests.cs` (csproj:230) — L194/196/218; **already `[DoNotParallelize]` at L19**
  2. `OutlookObjects/Table/OlTableExtensions_Tests.cs` (csproj:527) — L1636/1640/1645; **already `[DoNotParallelize]` at L20**
  3. `ReusableTypeClasses/StackGeek_Tests.cs` (csproj:431) — L144/146/155, asserts L159-160; **no attribute** (L9 `[TestClass]`)
  4. `OutlookObjects/Filter DASL/DASLFilterParserTests.cs` (csproj:270) — L101/102/111, asserts L115; **no attribute** (L8 `[TestClass]`)
  Excluded by rule: `HelperClasses/NLogTraceWriter_Test.cs` (csproj:73) — satisfies (a)(b)(c) at L22/23/56 but not (d); ~26 classes with `[TestInitialize] Console.SetOut(new DebugTextWriter())` and no capture; `OutlookObjects/DASLFilterParser_Tests.cs` (orphan, not in csproj); `BayesianClassifierTests_UnfinishedStubs.cs:31` (commented out).
- **Primary Count:** 4 members; 2 already protected; **2 unprotected**.
- **Cross-check Search Strategy:** independent attribute-first strategy — `Grep "\[DoNotParallelize\]"` over `UtilitiesCS.Test` to enumerate the already-protected set, then invert: for each of the four `Console.Out`-capturing files identified structurally by the presence of the triple `var originalOut = Console.Out` / `Console.SetOut(writer)` / `Console.SetOut(originalOut)`, check membership in the protected set. This derives the unprotected count from the attribute census rather than from the `Console` census.
- **Cross-check Member Set:** `[DoNotParallelize]` in `UtilitiesCS.Test` appears on `PrettyPrint_Tests` (L19), `OlTableExtensions_Tests` (L20), and `Threading/ThreadMonitorTests.cs` (L18, marked for `CurrentStoreContext` global state, not `Console`). Files containing the capture/restore triple: `PrettyPrint_Tests.cs`, `OlTableExtensions_Tests.cs`, `StackGeek_Tests.cs`, `DASLFilterParserTests.cs`, `NLogTraceWriter_Test.cs` (field-based variant), `DASLFilterParser_Tests.cs` (orphan). Intersecting: protected ∩ capturing = { `PrettyPrint_Tests`, `OlTableExtensions_Tests` }; capturing ∧ asserting ∧ compiled ∧ unprotected = { `StackGeek_Tests`, `DASLFilterParserTests` }.
- **Cross-check Count:** 4 members; 2 protected; **2 unprotected**.
- **Member-set Comparison:** normalized primary member set `{PrettyPrint_Tests, OlTableExtensions_Tests, StackGeek_Tests, DASLFilterParserTests}` equals the normalized cross-check set, and both strategies return the same unprotected subset `{StackGeek_Tests, DASLFilterParserTests}`. **Agreement.** Assertion admitted: *exactly 4 compiled `UtilitiesCS.Test` classes capture, restore and assert on `Console.Out`; exactly 2 of them already carry `[DoNotParallelize]`, and exactly 2 (`StackGeek_Tests`, `DASLFilterParserTests`) do not.*

### N5 — `PumpTimeoutMs` declarations and usages in `QuickFiler.Test`

- **Complete Family:** every declaration of, and every reference to, an identifier named `PumpTimeoutMs` within `QuickFiler.Test`, together with the syntactic position of each reference.
- **Exhaustive Search Scope:** the whole `QuickFiler.Test` tree; the classification must distinguish attribute-argument positions from expression positions, because the entire disposition of Finding 4 turns on whether any reference is a *wait duration*.
- **Inclusion Rules:** any lexical occurrence of `PumpTimeoutMs`.
- **Exclusion Rules:** none (the family is deliberately total).
- **Primary Search Strategy:** `Grep pattern="PumpTimeoutMs" path="QuickFiler.Test"` — total identifier census, 23 lines.
- **Primary Member Set:** Declarations (4): `Viewers/WebView2BreadcrumbHostTests.cs:25` (`private const`), `Controllers/QfcItemController.InitializationTests.cs:38` (`internal const`), `Controllers/QfcItemController.SeamFactoryTests.cs:327` (`private const`), `Controllers/QfcItemController.ViewerSetupTests.cs:34` (`private const`). Usages (19), all of the literal form `[Timeout(PumpTimeoutMs)]`: `WebView2BreadcrumbHostTests.cs` L32, 82, 135, 181, 226, 257, 302, 348 (8); `QfcItemController.InitializationTests.Part3.cs` L39, 82, 130, 174, 244, 352, 400, 455 (8); `QfcItemController.SeamFactoryTests.cs` L338, 409 (2); `QfcItemController.ViewerSetupTests.cs` L425 (1).
- **Primary Count:** 4 declarations, 19 usages, **0 usages in a wait/expression position**.
- **Cross-check Search Strategy:** independent wait-API census rather than identifier census — `Grep pattern="Timeout\(|WaitAsync|Task\.Run|TaskCompletionSource|\.Wait\(|WaitOne"` over `QuickFiler.Test`, enumerating every blocking/waiting construct in the assembly and checking whether any of them takes a timeout argument at all. If `PumpTimeoutMs` were used as a wait duration it would have to appear as an argument to one of these.
- **Cross-check Member Set:** the `[Timeout(...)]` occurrences recovered by this census are the same 19 attribute sites plus `WinFormsPumpHostTests.cs` L31/58/87/114/152/182/217/269/301/333/366/394/415 which use that file's own `TimeoutMs = 30000` (a separate constant, `WinFormsPumpHostTests.cs:24`). No `.Wait(`, `WaitOne`, `WaitAsync` or `Task.Run` call anywhere in `QuickFiler.Test` takes `PumpTimeoutMs` or any other timeout argument; the waiting constructs found are `TaskCompletionSource`-backed (`WinFormsPumpHost.cs:364-365` and the Breadcrumb test gates) and un-timed. `ManualResetEventSlim` appears once, at `WinFormsPumpHost.cs:29/60` (`_ready.Wait()` with no timeout).
- **Cross-check Count:** 19 `PumpTimeoutMs` attribute sites; **0 wait-position usages**.
- **Member-set Comparison:** the normalized 19-element usage set from the identifier census is identical to the `[Timeout]` subset recovered by the wait-API census, and both strategies independently return an empty wait-position set. **Agreement.** Assertion admitted: *`PumpTimeoutMs` is declared 4 times and used 19 times, exclusively as an MSTest `[Timeout]` harness bound and never as an in-test wait; the pump-hosted tests contain no wall-clock wait to seam out, which is why no test-only change can remove their load sensitivity.*

---

## 6. Behaviour Semantics

| # | Behaviour | Success | Failure | Ordering / edge cases |
|---|---|---|---|---|
| 1 | `WaitAsync(delay, fake)` completes only after virtual time reaches `delay` | Task is `RanToCompletion` after `Advance(delay)` | Task completes before `Advance`, or never completes after it | Start-then-advance ordering is mandatory (§1.4). `TimeSpan.Zero` fires on the first `Advance`, not at creation. `[Timeout(5000)]` remains a deadlock bound. |
| 1b | Existing production callers keep compiling | `msbuild /t:Rebuild` clean at `StoreRehookCoordinator.cs:102` and `AppEvents.cs:456` | CS0123 at `:102` if an optional parameter is used instead of an overload | The 1-arg overload must remain the unique 1-parameter candidate. |
| 2 | No `Form`-derived type is compiled into a unit-test assembly | Guard's `formDerivedTypeNames` is empty | Any `Form` subclass appears | `SVGControl.Test` red-before → green-after; `UtilitiesCS.Test` green-from-birth. `ReflectionTypeLoadException` must degrade to the loadable subset, not fail the guard. |
| 3 | A `Console.Out`-capturing class never overlaps another `Console.SetOut` | Both marked classes run in MSTest's serial partition | Class remains in the parallel partition | Race, so no deterministic red run; requires a fail-before exception dossier. Marking is idempotent and order-independent. |
| 4 | (Out of scope) Pump-hosted tests complete under contention | — | — | Requires a `QuickFiler/` production seam; promoted as a follow-up issue. |

---

## 7. Requirements Mapping — proposed file changes

**In scope (recommended):**

| File | Change |
|---|---|
| `TaskMaster/AppGlobals/NonBlockingDelay.cs` | Split `WaitAsync` into a 1-arg overload delegating to a new 2-arg `WaitAsync(TimeSpan, TimeProvider)`; replace `new Timer(...)` with `timeProvider.CreateTimer(...)` returning `ITimer`; preserve the L52-54 `#nullable ... annotations` pragma pair |
| `TaskMaster.Test/AppGlobals/NonBlockingDelayTests.cs` | Remove `Stopwatch` and the `System.Diagnostics` using; inject `FakeTimeProvider`; add the not-completed-before-advance assertion |
| `TaskMaster.Test/TaskMaster.Test.csproj` | 2 `Reference` blocks (§1.5) |
| `TaskMaster.Test/packages.config` | 2 `package` entries (§1.5) |
| `UtilitiesCS.Test/` | Delete 10 orphan files (§2.4 item 1) |
| `UtilitiesCS.Test/NoLiveFormInTestAssemblyTests.cs` (new) + csproj `<Compile>` entry | Ported guard, namespace `UtilitiesCS.Test` |
| `UtilitiesCS.Test/OutlookObjects/Filter DASL/DASLFilterParserTests.cs` | Add `[DoNotParallelize]` + hazard comment |
| `UtilitiesCS.Test/ReusableTypeClasses/StackGeek_Tests.cs` | Add `[DoNotParallelize]` + hazard comment |
| `UtilitiesCS.Test/OutlookObjects/DASLFilterParser_Tests.cs` | Delete (orphan duplicate) |

**Scope decision required from the orchestrator:** whether `SVGControl.Test` (delete `Form1.cs`, `Form1.Designer.cs`, `Form1.resx`, `Form2.cs`, `Form2.Designer.cs`, `Form2.resx`; remove 4 `<Compile>` + 2 `<EmbeddedResource>` entries; add the ported guard) is included in #729 or promoted separately. It is the only site with fail-before evidence for Finding 2.

**Out of scope:** all of Finding 4; every file under `QuickFiler/`.

**Coverage impact:** `NonBlockingDelay.cs` is production code whose covered lines change. The 2-arg overload is exercised by the tests; the 1-arg overload is exercised transitively by `StoreRehookCoordinator`/`AppEvents` tests. Add a direct test of the 1-arg overload if `StoreRehookCoordinator`'s tests inject a stub delay (they do — `delay` is an injectable constructor parameter at `StoreRehookCoordinator.cs:83`), otherwise the 1-arg body is uncovered and coverage on the changed lines regresses.

---

## 8. Testing Implications (strategy only, no test code)

- **Finding 1:** rewrite the two existing methods in place; do not add a third. Keep `[Timeout(5000)]`. The determinism gain is asserted by proving non-completion before `Advance`. Add one test exercising the 1-arg overload so both changed production paths are covered.
- **Finding 2:** one guard class per test assembly, metadata-only reflection, no instantiation. Record the green-from-birth status for `UtilitiesCS.Test` in the plan so no reviewer expects a red run there.
- **Finding 3:** attribute-only change; no new test. Record a fail-before exception dossier at `docs/features/active/2026-09-02-test-determinism-and-hygiene-debt-729/evidence/regression-testing/fail-before-exception.<timestamp>.md` with `WhyFailingRunImpossible` and the two in-repo precedent comments as alternative proof.
- **Finding 4:** no test change.
- **Toolchain:** `nuget restore` first (the `packages/` folder is absent from this worktree), then the four-step loop from `CLAUDE.md` § CUT3, restarting at step 1 after the `packages.config` edit because CSharpier reformats that file.
- **Local test run:** per the repo's known local-run constraint, exclude `\.claude\` worktree copies from the assembly list and pass `/InIsolation` to match CI.
