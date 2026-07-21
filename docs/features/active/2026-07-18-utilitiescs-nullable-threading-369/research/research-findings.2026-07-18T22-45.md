# Research: utilitiescs-nullable-threading (Issue #369) — Wave-0

- Date: 2026-07-18T22-45
- Feature: `docs/features/active/2026-07-18-utilitiescs-nullable-threading-369/`
- Epic: `utilitiescs-nullable-remediation` (Wave 0, complexity C3 — floored by `concurrency_or_ordering`)
- Scope: per-file `#nullable enable` remediation of `UtilitiesCS/Threading/` (25 hand-written `.cs`, 4 WinForms Designer, 4 `.resx`)
- Method: static reading only (no build permitted in this environment; consistent with the sibling Extensions research). All null-risk assessments are static hypotheses to be confirmed by the pragma-only build during execution.

---

## 0. Confirmed environment facts (evidence)

- `UtilitiesCS/UtilitiesCS.csproj`: `<TargetFrameworkVersion>v4.8.1</TargetFrameworkVersion>` (net481), `<LangVersion>12.0</LangVersion>`, non-SDK `packages.config` VSTO/.NET-Framework project. **No `<Nullable>` element anywhere** (confirmed by reading the property groups). AC requires it stays absent.
- No directory-scoped `.editorconfig` overrides nullable; enforcement is per-file `#nullable enable` pragma only.
- **No file in `UtilitiesCS/Threading/` currently carries a `#nullable` pragma** (grep returned zero matches). Every remediated file is a fresh opt-in.
- The sibling child `utilitiescs-nullable-extensions` research established the working template; its language-version conclusions apply identically here (see §3).

### 0.1 net481 BCL is nullable-OBLIVIOUS — this materially shapes the debt

.NET Framework 4.8.1 reference assemblies do **not** ship nullable metadata. Consequently BCL/framework surfaces (`System.*`, `System.Windows.Threading.Dispatcher` in WindowsBase, `System.Threading.Tasks.Task`, `SynchronizationContext.Current`, `string.IsNullOrWhiteSpace`, `MethodBase.GetCurrentMethod().DeclaringType`, `Microsoft.Office.Interop.Outlook`) are **oblivious**: assigning their return to a non-null field does not warn, and dereferencing their return does not warn. Some NuGet packages ARE annotated and DO flow warnings (`Microsoft.Bcl.TimeProvider`, `System.Text.Json`, `Microsoft.Extensions.*`, `System.Memory`) — but the Threading module leans almost entirely on the oblivious framework surface plus `Microsoft.Bcl.TimeProvider` (used cleanly by `ThreadMonitor`).

The practical result: the real Threading debt is dominated by **compiler-internal diagnostics that do not depend on BCL annotations**:

- **CS8618** — non-nullable field / auto-property / event uninitialized after construction (the largest category here).
- **CS8625** — `null` literal assigned to a non-nullable reference (`= null` default parameters, `instance = null`, `ITimerWrapper timer = null`, `StackTrace stackTrace = null`).
- **CS8603 / CS8600** — `default(TResult)` returns on unconstrained generics, and `x as T` cast results.
- **CS8602 (self-induced)** — appears only AFTER a field is annotated `T?`; resolved with justified `!` (behavior-preserving) rather than new guards.

BCL-return dereferences that a naive reading might flag (e.g. `SynchronizationContext.Current` assigned to a field, `GetCurrentMethod().DeclaringType` passed to `GetLogger`, `Dispatcher.FromThread(...)` dereferenced) **likely do NOT warn** because the source is oblivious. The executor must confirm the exact diagnostic set with the pragma build; the per-file notes below flag both the near-certain (CS8618/CS8625) and the oblivious-dependent (CS860x) cases.

---

## 1. Per-file CS86xx risk survey (25 hand-written files)

Legend: **Clean** = expected zero CS86xx (pragma for cluster consistency only); **Trivial** = 1–2 mechanical edits; **Substantive** = real annotation work; **Contract** = annotations become cross-module downstream contracts (see §2).

### Interface-only files

| File | Lines | Class | Notes |
|---|---:|---|---|
| `IProgressViewer.cs` | 22 | Clean / **Contract** | Interface declaration; no bodies/fields → emits no CS86xx. Adding the pragma *declares* `Bar`/`JobName`/`ButtonCancel`/`UiDispatcher` as non-null and `SetCancellationTokenSource(CancellationTokenSource)` non-null. The concrete `ProgressViewer` does NOT implement it (the `IProgressViewer` region is commented out), so there is no implementer obligation today, but the interface is a public contract — opt in deliberately. |
| `IUiDispatcher.cs` | 36 | Clean / **Contract** | Interface with `Action`/`Func<TResult>`/`CancellationToken` members; no CS86xx. Pragma declares all delegate params non-null. Implemented by `WpfUiDispatcher` (same batch). Consumed by 50+ files repo-wide (§2). |

Interfaces with no executable bodies emit no CS86xx; per General-Unit-Test policy they are legitimately 0% executable coverage. The pragma still matters because it fixes the nullability of the declared contract.

### Commented-out / no-code files

| File | Lines | Class | Notes |
|---|---:|---|---|
| `TaskPriority.cs` | 35 | Clean | Entire body is commented out. No code → no CS86xx. Pragma optional (cluster consistency only). |
| `AsyncIdleQueue1.cs` | 138 | Clean | Entire file commented out (dead reference copy). No code. Pragma optional. |

### Value-type / lock-primitive files (concurrency primitives — annotation is a near-no-op)

| File | Lines | Class | Notes |
|---|---:|---|---|
| `ThreadSafeSingleShotGuard.cs` | 28 | Clean | Only `int` fields + `Interlocked.Exchange`. No reference types → no CS86xx. **Single-shot guard**: annotation cannot touch the `Interlocked` semantics. Widely consumed (§2) but has **no nullable surface** → zero contract risk. |
| `ThreadSafeFunctions.cs` | 203 | Clean / Trivial | `this ref double`/`ref int` extension math with `Func<double,double>` (non-null) params and `Interlocked.CompareExchange`. All value types + non-null delegates. Expected zero/near-zero CS86xx. `using Microsoft.Office.Interop.Outlook;`/`Deedle.Internal` are unused for null-flow. Consumed by `BayesianClassifierShared` but no nullable surface. |
| `LockupStallDecider.cs` | 83 | **Substantive / Contract** | Two types. `LockupStallDecider` (double/bool only) = clean. **`LockupAttribution` struct** carries `StoreIdentity` which is documented as null "when no per-store context was active"; ctor param `string storeIdentity` → `string?`, property `string StoreIdentity` → `string?`. This is the highest-value concurrency annotation (see §3). Struct fields do not trigger CS8618 (implicit default ctor), so the only edits are the two `?` on the identity param/property. |

### Ambient-context file (concurrency, volatile)

| File | Lines | Class | Notes |
|---|---:|---|---|
| `CurrentStoreContext.cs` | 98 | **Substantive / Contract** | `private static volatile string _current;` is a non-null field that is *designed* to hold null ("no context") → **CS8618** → must become `volatile string?`. `Current => _current` return → `string?`. `Normalize(string)` returns null → `string?` param and return. `Begin(string storeIdentity)` param (null/whitespace/`<unavailable>` normalize to null) → `string?`. `Scope._previous` `readonly string` → `string?`. Every annotation matches the *documented* contract; **no behavior change, no touch to `volatile`/ordering.** `Current` feeds `ThreadMonitor.EvaluatePoll` and `LockupAttribution` (both same cluster) and is read cross-module by `StoresWrapper`/`AppOlObjects`. |

### Idle-scheduling files (concurrency scheduling)

| File | Lines | Class | Notes |
|---|---:|---|---|
| `IdleActionQueue.cs` | 94 | Substantive | `private static ConcurrentQueue<Action> _entries;` lazily `??=`-initialized → **CS8618** → `ConcurrentQueue<Action>?`. `Entries` property returns non-null after init. `TryDequeue(out Action action)` → use `out Action? action` or `out var` (framework marks it `MaybeNullWhen(false)` only if annotated; net481 oblivious → either compiles). Logger idiom `GetCurrentMethod().DeclaringType` is oblivious (likely no warning). `_subscribeGuard` reset touches no null. **Idle scheduling / single-shot subscribe guard must not change** — the annotation is field-nullability only. |
| `IdleAsyncQueue.cs` | 97 | Trivial | `Entries` is `{ get; } = new()` (initialized) → no CS8618. `TryDequeue(out (bool, Func<Task>) entry)` is a value tuple → no null. Logger idiom oblivious. Expected near-clean; verify. |
| `ApplicationIdleTimer.cs` | 481 | **Substantive** + **500-line watch** | `private static ApplicationIdleTimer instance = null;` → **CS8625**; singleton is set in the static ctor → annotate `= null!` (behavior-preserving) rather than `?` (would force `instance!.` at every static property). `_timer` set inside `StartTimer()` (called from ctor); Roslyn does not track field-init through called methods → **CS8618** → `= null!`. `syncContext` from `SynchronizationContext.Current` (oblivious) but is genuinely nullable and already null-checked (`if (syncContext != null)`) → `SynchronizationContext?`. `public static event ApplicationIdleEventHandler ApplicationIdle;` is checked `== null`/`is null` → genuinely nullable → `event ApplicationIdleEventHandler? ApplicationIdle`. `FindTriggeringEventHandler` returns null → `Delegate?`; locals `idleEventInfo`/`eventField`/`eventFieldValue` follow the reflection returns (oblivious source, but the method returns null so its own return must be `Delegate?`). See §5 for the line-count flag. |

### WinForms hand-written form partials (Designer left oblivious — see §4)

| File | Lines | Class | Notes |
|---|---:|---|---|
| `ProgressMultiStepViewer.cs` | 20 | Clean | Only a ctor calling `InitializeComponent()`. No own fields dereferenced → expected zero CS86xx. Designer partial stays oblivious. |
| `ProgressPane.cs` | 57 | **Substantive / Contract** | Namespace `UtilitiesCS.EmailIntelligence.TaskPane` (in `Threading/` folder). Own fields: `_dispatcher` (uninitialized → CS8618 → `Dispatcher?`), `_tokenSource` (uninitialized → CS8618 → `CancellationTokenSource?`), `_context`/`_uiScheduler` (assigned in ctor from oblivious BCL → likely no warning but set only in the parameterless-context path). `UiDispatcher`/`UiSyncContext`/`UiScheduler` public getters feed `ProgressTrackerPane`. `CancelButton_Click` calls `_tokenSource.Cancel()`; once `_tokenSource` is `?`, use `_tokenSource!.Cancel()` (invariant: button enabled only after `SetCancellationTokenSource`) — preserves the current NRE-if-null behavior. Designer fields (`ButtonCancel`) untouched. |
| `ProgressViewer.cs` | 84 | **Substantive / Contract** | Namespace `UtilitiesCS`. Own fields: `_dispatcher` (CS8618 → `Dispatcher?`), `_cancelSource` (CS8618 → `CancellationTokenSource?`; public `CancelSource` get/set consumed by `ProgressTracker.Initialize`), `_context`/`_uiScheduler`/`_uiThreadNumber(int)`. `CancelButton_Click` → `_cancelSource!.Cancel()`. Designer fields (`Bar`, `JobName`, `ButtonCancel`) untouched. |
| `SyncContextForm.cs` | 47 | **Substantive / Contract** | Namespace `QuickFiler.Viewers`. Auto-props `UiSyncContext` (SynchronizationContext) and `UiDispatcher` (Dispatcher) are non-null but never set in the ctor (set later in `CaptureUiVariables()`) → **CS8618**. Options: annotate `SynchronizationContext?`/`Dispatcher?` or `{ get; private set; } = null!`. `FormAutoScaleFactor`(SizeF)/`UiThreadId`(int) are value types. Consumed by `UiThread.Initialize` which reads these into its own public `UiSyncContext`/`Dispatcher`. |

### Progress trackers (medium/high cross-module contract)

| File | Lines | Class | Notes |
|---|---:|---|---|
| `ProgressPackage.cs` | 149 | **Substantive / Contract** | All optional reference params default `= null`: `CancellationTokenSource cancelSource = null`, `ProgressTracker progressTracker = null`, `ProgressTrackerPane`, `SegmentStopWatch stopWatch = null`, `Screen screen = null` → **CS8625** → each param `T? … = null` (default stays null; behavior identical). Fields `_progressTracker`/`_progressTrackerPane` — only one is set per `InitializeAsync` overload, the other stays null → **CS8618** → `ProgressTracker?`/`ProgressTrackerPane?`. Public props + tuple return shapes become nullable. `SpawnChild` already uses `?.`. High consumer count (QuickFiler/TaskMaster/EmailIntelligence). |
| `ProgressTracker.cs` | 264 | **Substantive / Contract** | `_jobName`(string, uninitialized → `string?`), `_cancelSource`/`_screen`/`_uiDispatcher`/`_progressViewer` set only in some ctors/`Initialize` → CS8618 → `?` (or `= null!` where an init-order invariant holds). `new StackFrame(1,false).GetMethod().Name` → `GetMethod()` return is oblivious on net481 (likely no warning; if annotated, `!`). `_pvIsDisposed` single-shot guard and `_parent` (struct) untouched. `ParentProgress<T>` struct: non-null ref field `_progress` does NOT trigger CS8618 (implicit default struct ctor) — leave as-is. `Report`/`ReportAsync` close-on-100 logic unchanged. |
| `ProgressTrackerAsync.cs` | 108 | Substantive | Mirrors `ProgressTracker`: `_cancelSource` (ctor-set), `_screen`(`Screen?`), `_progressViewer` (set in `InitializeAsync` → `?` or `= null!`), `_jobName`(`string?`), `_uiDispatcher`(`Dispatcher?`), int fields. Lower external contract (consumed mainly by its tests). |
| `ProgressTrackerPane.cs` | 169 | **Substantive / Contract** | `_progressViewer` is assigned inside a `UiThread.Dispatcher.Invoke(...)` lambda in the ctor → Roslyn cannot prove ctor-init → **CS8618**; `SafeAction` already null-checks `_progressViewer == null || .IsDisposed` → the field is genuinely nullable → `ProgressPane?`. `_jobName`(`string?`). `new StackTrace().GetMyTraceString()` / `new StackFrame(1,false).GetMethod().Name` oblivious. Consumed cross-module via `IAppAutoFileObjects.ProgressTracker` (see §2). |

### Watchdog / dispatch core (CRITICAL concurrency)

| File | Lines | Class | Notes |
|---|---:|---|---|
| `WpfUiDispatcher.cs` | 39 | Clean/Trivial | `IUiDispatcher` implementation; five one-line forwards to `UiThread.Dispatcher.Invoke(...)`. `UiThread.Dispatcher` and `DispatcherOperation` are oblivious framework surface → expected zero CS86xx. Same batch as `IUiDispatcher`. |
| `UiThread.cs` | 162 | **Substantive / Contract** | Static hub. `_onLockupDetected` (`Action<LockupAttribution>`, only conditionally set → CS8618 → `Action<LockupAttribution>?`), `_monitorTimeProvider` (`TimeProvider?`), `_syncContextForm`(`SyncContextForm?`), `_threadMonitor`(`ThreadMonitor?`), `_uiSyncContext`(set in `Init` → `SynchronizationContext?` or `= null!`), `_dispatcher`(`Dispatcher?` or `= null!`). Public `UiSyncContext`/`Dispatcher`/`UiThreadId`/`AutoScaleFactor` are the widest contract in the module (50+ consumers, §2). `_autoScaleFactor` is already `System.Drawing.SizeF?`. The `SynchronizationContextAwaiter` struct already null-guards its ctor (`if (context is null) throw`). No change to lock/ordering/`Post`. |
| `ThreadMonitor.cs` | 240 | **Substantive / Contract / CRITICAL** | `thread` ctor param is documented "may be null on the attribution seam path (tests)" → `Thread?` (and field `Thread?`). `_onLockupDetected` from nullable param → `Action<LockupAttribution>?`. `_pollTimer`(ITimer, set in `Run` → `ITimer?`; already used as `_pollTimer?.Change`). Ctor param defaults `TimeProvider timeProvider = null`/`Action<…> onLockupDetected = null` → CS8625 → `?`. `PingAndAwaitDiagnosticWindow`: `Dispatcher.FromThread(thread)` returns oblivious `Dispatcher`; the `if (dispatcher is null){ Send(x => dispatcher = …) }` reassignment cannot be proven by flow analysis → `dispatcher!.InvokeAsync(...)` (behavior-preserving). `GetStackTrace` sets `stackTrace = null` and local `StackTrace stackTrace = null` → **CS8625** → `StackTrace?` return + local. `EvaluatePoll` (the covered seam) constructs `new LockupAttribution(elapsed, CurrentStoreContext.Current)` — `Current` is now `string?` matching the `string?` ctor param → clean. `Run`/`Tick`/`Ping`/`GetStackTrace` are `[ExcludeFromCodeCoverage]` but STILL must compile clean under the pragma (the attribute does not exempt CS86xx). **No change to the polling loop, timer re-arm ordering, or `Thread.Suspend/Resume` diagnostic path.** |
| `StoreLockupResponder.cs` | 158 | **Substantive / Contract / CRITICAL (null-branch hazard)** | Ctor already null-guards (`?? throw new ArgumentNullException`). Params `StoreLockupNotifier notify = null`/`Action<string> logSink = null` → CS8625 → `?`. `OnLockupDetected(LockupAttribution attribution)`: `var displayName = attribution.StoreIdentity;` is now `string?`. The three guards (`IsNullOrWhiteSpace` no-context; unresolved-sentinel; `<Stores-enumeration>` phase — issue #292; already-disabled) are the documented null-store-model protection (issue #260/#264). **On net481, `string.IsNullOrWhiteSpace` is NOT annotated `[NotNullWhen(false)]`, so it does not refine `displayName` to non-null.** `StoreIdentity.Resolve(displayName)` takes an oblivious `string` (its file is in the wave-1 Store cluster, not opted in) → passing `string?` to an oblivious param does **not** warn, so no `!` is even required there. **The critical rule: annotate around the guards; do NOT add, reorder, or alter any null-branch.** If any residual CS8604 appears on a same-cluster non-null consumer, resolve it with `displayName!` at the guaranteed-non-null call site (guarded above), never with a new runtime guard. |

### Timeout helpers (highest contract + 500-line breach)

| File | Lines | Class | Notes |
|---|---:|---|---|
| `AsyncMultiTasker.cs` | 465 | **Substantive / Contract** + **500-line watch** | Four `AsyncMultiTaskChunker` overloads. `Func<TimeSpan, ITimerWrapper> timerFactory = null` → `?` then `??=`. In the second overload `ITimerWrapper timer = null;` (line 182) is assigned inside `await Task.Run(() => { timer = … })`; the `catch`/`finally` then call `timer.StopTimer()`/`timer.Dispose()` → **CS8602** (timer `ITimerWrapper?`). Use `timer!.StopTimer()` (preserves the current behavior, which would already NRE if `timerFactory` threw before assignment) — do **not** switch to `timer?.` (that changes behavior). **Flag this as a null-flow decision to confirm.** `((IItemInfo)x).Sw.Durations` where `x` is unconstrained `TOut` → possible CS8602 → `!` or pattern guard. `progress`/`sw`/`GetReportMessage` params non-null. Line count 465 + one pragma line = 466; annotations are in-place → stays < 500 (see §5). CONCURRENCY: `Task.Run` fan-out and `Task.WhenAll` ordering unchanged. |
| `TimeOutTask.cs` | 975 | **Substantive / Contract / HIGHEST** + **500-line breach (FLAG, do not fix)** | ~15 `RunWithTimeout`/`TimeoutAfter` overloads. `Func<int, CancellationTokenSource> timeoutSourceFactory = null` → `?`. Pervasive `TResult result = default;` on unconstrained `TResult` returned from `Task<TResult>` → **CS8603** when `TResult` is a reference type. **Recommendation: keep the public return type `Task<TResult>` stable and use `result = default!` / `return result!`** (the runtime already returns `default(TResult)` on non-strict timeout today; callers already handle it). Do **not** widen to `Task<TResult?>` — that is a downstream contract change across the many consumers in §2 and must be raised with the maintainer, not done silently. `MarshalTaskResults`: `castedSource = source as Task<TResult>` → CS8600 → `Task<TResult>?` local (already null-checked in the ternary); `proxy.TrySetException(source.Exception)` — `Task.Exception` is oblivious on net481 (likely no warning). Timer-callback `state` casts (`(TaskCompletionSource<TResult>)state`) where `state` is `object?` → CS8600/CS8605 → cast is safe (state is the captured tcs) → `!`. **This file cannot be brought under 500 lines by annotation; per spec, FLAG the pre-existing breach and do NOT refactor.** |

---

## 2. Cross-module contract files (annotations become downstream contracts)

Grep across the repo confirms these Threading public members are consumed **outside** `Threading/`. Annotate these deliberately and last within their batches; group for consistent review.

- **`UiThread` / `IUiDispatcher` / `WpfUiDispatcher`** — HIGHEST fan-out. ~50 non-test consumers across `TaskMaster` (`ThisAddIn`, `ApplicationGlobals`, `AppOlObjects`, `AppAutoFileObjects`), `QuickFiler` (12+ controllers/helpers: `QfcItemController`, `QfcHomeController`, `QfcFormController`, `QfcQueue`, `KeyboardHandler`, `Efc*`), and `UtilitiesCS` (`FolderPredictor`, `Theme`/`ThemeControlGroup`, `SegmentStopWatch`, `FolderRemapViewer`, `FilterOlFoldersViewer`). The public `UiThread.UiSyncContext`/`Dispatcher` and the `IUiDispatcher` member signatures are load-bearing contracts. Test seams: `UiThread_Tests.cs`, `QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs`, `Theme.DispatcherTests.cs`.
- **`ProgressTracker` / `ProgressTrackerPane` / `ProgressPackage` / `IProgressViewer` / `ProgressViewer`** — HIGH. Consumed by `QuickFiler` (`QfcHomeController`, `QfcFormController.Actions`, `QfcDatamodel`, `BayesianPerformanceController`, `IFilerHomeController`, `IQfcFormController`), `TaskMaster.AppAutoFileObjects`, `UtilitiesCS` (`OlTableExtensions.Etl`, `FolderTree`, `EmailDataMiner`, classifier groups, Bayesian performance), and the interface **`IAppAutoFileObjects.ProgressTracker` returns `ProgressTrackerPane`** (a formal cross-module contract). The `IProgress<(int Value, string JobName)>` tuple contract is shared by `ProgressTracker`/`ProgressTrackerPane`.
- **`LockupAttribution` (StoreIdentity → `string?`)** — HIGH concurrency contract. Consumed by `StoresWrapper`, `StoreWrapper`, `StoreLockupAttribution` (formatter), `TaskMaster` (`ThisAddIn`, `AppOlObjects`) and tests (`StoreLockupAttributionTests`, `AppOlObjectsAttributionContextTests`, `StoresWrapperEnumerationScopeTests`). `StoreLockupAttribution.FormatLine(string identity, …)` already renders null/empty as `<null>` — its `identity` param is a candidate for `string?` but lives in the wave-1 Store cluster, so leave it oblivious for now; the `string?` on `LockupAttribution.StoreIdentity` flowing into it does not warn.
- **`StoreLockupResponder` / `StoreLockupNotifier`** — consumed by `TaskMaster` (`AppOlObjects`, `ThisAddIn`) and `MyBoxModeless`.
- **`TimeOutTask` (`RunWithTimeout`/`TimeoutAfter`)** — HIGH. Consumed by `UtilitiesCS` (`OlTableExtensions[.TableAccess/.Etl]`, `ConversationHelper[.Formatting]`, `OneDriveDownloader`, `StreamExtensions`, `DfDeedle`) and `QuickFiler` (`QfcItemControllerTests`). Return-type stability (§1, keep `Task<TResult>` + `!`) protects all of these.
- **`AsyncMultiTasker.AsyncMultiTaskChunker`** — consumed by `EmailIntelligence` (`EmailDataMiner.Transform`/`.FolderExtraction`, `OlFolderClassifierGroup`, `CategoryClassifierGroup`, `ActionableClassifierGroup`).
- **`ThreadSafeFunctions`** — consumed by `BayesianClassifierShared` (value-type surface only → no contract risk).
- **`ThreadSafeSingleShotGuard`** — widely consumed (`ReusableTypeClasses/TimedActions`, `SmartSerializable*`, `MailItemHelper`, `IOutlookReadinessGate`, `Flags`, `SpamBayes`, `Corpus*`, `TaskMaster`, `QuickFiler`) but exposes **no nullable surface** → opting it in imposes no contract.

Consumers not opted in for this child remain nullable-oblivious, so they do not warn when they consume a newly-annotated Threading member. This is what makes the child independently mergeable, but it also means an *incorrect* annotation would silently propagate a wrong null assumption to those consumers only when they are later opted in — hence the deliberate/last treatment above.

---

## 3. Concurrency-semantics preservation (per-file safe-vs-behavior-risk)

The module-wide rule: annotate types and add `?`/`!`; **never** add, remove, reorder, or alter a runtime null-branch, lock, `Interlocked`, `volatile`, timer arm/re-arm, single-shot guard, or `Dispatcher.Post`/`Send` call.

- **`ThreadSafeSingleShotGuard`, `ThreadSafeFunctions`** — `Interlocked`-based; annotation is a no-op (no reference types). Fully safe.
- **`CurrentStoreContext`** — `volatile string _current` → `volatile string?`. Annotation-only; the `volatile` keyword and the single-writer/single-reader discipline are untouched. Matches the documented "null = no context" contract exactly. **Safe.**
- **`LockupStallDecider` / `LockupAttribution`** — the `StoreIdentity` → `string?` chain is the correct, behavior-neutral annotation (identity is genuinely null when no per-store scope is open). `IsStallConfirmed` `>=` boundary and the decider are value-typed. **Safe.**
- **`IdleActionQueue` / `IdleAsyncQueue`** — field-nullability (`_entries?`) and `TryDequeue(out …)` only. The idle subscribe/unsubscribe single-shot guard reset and the `Application.Idle` scheduling are untouched. **Safe.**
- **`ApplicationIdleTimer`** — `event …? ApplicationIdle`, `instance = null!`, `_timer = null!`, `syncContext?`. The `Heartbeat`/`ComputeCPUUsage`/`OnApplicationIdle` timing math and the `Interlocked` subscription counting are untouched. **Safe** (watch line count — §5).
- **`UiThread`** — field nullability + `= null!`; the `SynchronizationContextAwaiter.Post` marshaling, the `_loaded` single-shot init guard, and the `ThreadMonitor` wiring order are untouched. **Safe.**
- **`ThreadMonitor`** — `Thread?`, `ITimer?`, `dispatcher!` in the `[ExcludeFromCodeCoverage]` ping path, `StackTrace?` in `GetStackTrace`. The one-shot timer re-arm (`_pollTimer?.Change` in `finally`), the `_lockupReported` once-per-episode latch, and the obsolete `Thread.Suspend/Resume` diagnostic path are untouched. **Safe.**
- **`StoreLockupResponder`** — **the one file where a null-branch is load-bearing** (issue #260/#292 null-store-model hazard). All edits are *around* the four guards: `string?` on the identity flowing in, `?` on the two optional ctor params. Because net481's `IsNullOrWhiteSpace` is un-annotated, prefer `displayName!` at any residual guaranteed-non-null call site over a new guard. **No edit may change the order or content of the no-context / unresolved-sentinel / `<Stores-enumeration>` / already-disabled branches.** Safe *if* the rule is honored; flag any diagnostic that seems to require touching a branch.
- **`AsyncMultiTasker`** — the only genuine null-flow *decision*: the second overload's `timer` local starts null and is assigned inside `await Task.Run(...)`; `catch`/`finally` dereference it. Use `timer!` (preserve current NRE-if-unassigned behavior), not `timer?.` (would swallow). **Flag for confirmation.** `Task.Run` fan-out / `Task.WhenAll` ordering untouched.

**Nothing in the module requires a behavior change to reach zero CS86xx.** Every case is expressible as `?`, `= null!`, or justified `!`. The single item to escalate is the `AsyncMultiTasker` `timer!` vs `timer?.` choice; the recommendation (`timer!`) preserves behavior.

---

## 4. WinForms Designer / `.resx` handling

- **Default confirmed: leave `*.Designer.cs` non-opted-in (oblivious) and do NOT hand-edit; leave the 4 `.resx` untouched.** `#nullable enable` is **lexical / file-scoped**, so a Designer partial that carries no pragma stays oblivious even though it is the same class as an opted-in hand-written partial. Fields declared in the oblivious Designer partial (e.g. `Bar`, `JobName`, `ButtonCancel`) are treated as **oblivious** when referenced from the opted-in partial → they emit **no** CS86xx and therefore need **no** `?`. This is why the hand-written partials do not need to annotate Designer-declared fields.
- The 4 forms with Designer+resx: `ProgressMultiStepViewer`, `ProgressPane`, `ProgressViewer`, `SyncContextForm`. Of these, `ProgressMultiStepViewer.cs` is expected clean; `ProgressPane.cs`, `ProgressViewer.cs`, and `SyncContextForm.cs` need `?`/`= null!` only on **their own** hand-declared fields/auto-props (`_dispatcher`, `_tokenSource`, `_cancelSource`, `_context`, `UiSyncContext`, `UiDispatcher`) — never on Designer-declared controls.
- Do not add `#nullable enable` to any `*.Designer.cs`; doing so would opt generated code into analysis and risk a churny CS8618 wave on auto-declared controls with no behavior benefit.

---

## 5. Pre-existing 500-line breach and near-limit files

- **`TimeOutTask.cs` (975 lines) — PRE-EXISTING breach of the repo 500-line limit. FLAG, do NOT fix.** Annotation-only work cannot reduce it below 500 (that requires splitting the ~15 overloads into multiple files, which is a refactor and out of scope). Adding a `#nullable enable` pragma makes it 976. Record the breach as pre-existing and defer any split to a separate issue. (This mirrors the Extensions research handling of `ArrayExtensions.cs` at 544.)
- **`ApplicationIdleTimer.cs` (481) and `AsyncMultiTasker.cs` (465)** — both under 500 today. Adding one `#nullable enable` line yields 482 / 466. Because the remediation prefers **in-place** `?`/`!`/`= null!` annotations over new guard blocks (§3), neither is expected to cross 500. **Risk is low but real (≈18 lines of headroom on `ApplicationIdleTimer`).** Instruction: keep annotations in-place; if csharpier reflow + annotations push either file to 501+, treat it as a pragma-induced NEW breach and **flag to the maintainer** rather than splitting the file (splitting is a refactor, out of scope for this child). Do not add multi-line guard blocks to these two files.

---

## 6. Recommended batch sequence (8 batches, low-risk → high-contract)

Each batch opts in its files, runs the pragma-only toolchain loop, and reaches zero CS86xx before the next batch. Ordering: foundational/no-op first, concurrency-core middle, highest cross-module contract + `TimeOutTask` last.

- **Batch 1 — no-op / confirm-clean (5):** `TaskPriority.cs`, `AsyncIdleQueue1.cs`, `ThreadSafeSingleShotGuard.cs`, `ThreadSafeFunctions.cs`, `ProgressMultiStepViewer.cs`. Establishes the pragma + csharpier + pragma-build + vstest loop at near-zero risk (commented-out, value-type, and empty-partial files).
- **Batch 2 — interfaces + dispatcher adapter (3, Contract):** `IUiDispatcher.cs`, `WpfUiDispatcher.cs`, `IProgressViewer.cs`. Contract *declarations*; small; group for consistent review because §2's largest fan-out flows through `IUiDispatcher`.
- **Batch 3 — ambient/value concurrency types (2, Contract):** `CurrentStoreContext.cs`, `LockupStallDecider.cs` (incl. `LockupAttribution`). The `string?` identity chain that Batches 7 consume — annotate first so the watchdog batch consumes the settled contract.
- **Batch 4 — idle scheduling + idle timer (3):** `IdleActionQueue.cs`, `IdleAsyncQueue.cs`, `ApplicationIdleTimer.cs`. Cohesive `Application.Idle` scheduling cluster. Watch `ApplicationIdleTimer` line count (§5).
- **Batch 5 — WinForms hand-partials (4):** `ProgressPane.cs`, `ProgressViewer.cs`, `SyncContextForm.cs`, (and `ProgressMultiStepViewer.cs` if not already done in Batch 1). Own-field nullability only; Designer/resx left oblivious (§4).
- **Batch 6 — progress trackers (4, Contract):** `ProgressPackage.cs`, `ProgressTracker.cs`, `ProgressTrackerAsync.cs`, `ProgressTrackerPane.cs`. High cross-module contract (QuickFiler/TaskMaster/`IAppAutoFileObjects`); careful, consistent review of the `IProgress<(int,string)>` tuple and nullable optional params.
- **Batch 7 — dispatch + watchdog core (3, CRITICAL):** `UiThread.cs`, `ThreadMonitor.cs`, `StoreLockupResponder.cs`. Depends on Batch 3 (`LockupAttribution`/`CurrentStoreContext`). Highest concurrency scrutiny; enforce the "annotate around guards, never alter a null-branch" rule (§3); the store-lockup null-store-model hazard lives here.
- **Batch 8 — high-contract parallel + timeout (2, LAST):** `AsyncMultiTasker.cs`, `TimeOutTask.cs`. Highest consumer count; the `timer!` decision (§3) and the `TimeOutTask` 500-line flag (§5) plus return-type-stability decision (§1) are resolved here under focused review.

Rationale for ordering: Batches 1–2 de-risk the loop; Batch 3 settles the concurrency contract the watchdog consumes; Batches 4–6 handle scheduling/UI/progress with growing contract weight; Batches 7–8 carry the CRITICAL concurrency semantics and the widest consumer surface, reviewed last when the pattern is fully established.

---

## 7. Existing test surface (evidence: `UtilitiesCS.Test/Threading/`)

There is a dedicated `UtilitiesCS.Test/Threading/` directory that mirrors the production types. Located test files:

| Threading type | Dedicated test file(s) |
|---|---|
| `ApplicationIdleTimer` | `ApplicationIdleTimer_Tests.cs` |
| `AsyncMultiTasker` | `AsyncMultiTasker_Tests.cs` |
| `CurrentStoreContext` | `CurrentStoreContextTests.cs` |
| `IdleActionQueue` | `IdleActionQueue_Tests.cs` |
| `IdleAsyncQueue` | `IdleAsyncQueue_Tests.cs` |
| `LockupStallDecider` / `LockupAttribution` | `LockupStallDeciderTests.cs` |
| `ProgressPackage` | `ProgressPackage_Tests.cs` |
| `ProgressPane` | `ProgressPane_Tests.cs` |
| `ProgressTracker` | `ProgressTracker_Tests.cs` |
| `ProgressTrackerAsync` | `ProgressTrackerAsync_Tests.cs` |
| `ProgressTrackerPane` | `ProgressTrackerPane_Tests.cs` |
| `ProgressViewer` | `ProgressViewer_Tests.cs` |
| `StoreLockupResponder` | `StoreLockupResponderTests.cs` |
| `TaskPriority` | `TaskPriority_Tests.cs` |
| `ThreadMonitor` | `ThreadMonitorTests.cs` |
| `ThreadSafeFunctions` | `ThreadSafeFunctions_Tests.cs` |
| `ThreadSafeSingleShotGuard` | `ThreadSafeSingleShotGuard_Tests.cs` |
| `TimeOutTask` | `TimeOutTask_Tests.cs`, `TimeOutTaskCoverageTests.cs`, `TimeOutTask_AdditionalTests.cs`, `TimeOutTask_InternalCoverageTests.cs`, `TimeOutTask_OverloadCoverageTests.cs` |
| `UiThread` | `UiThread_Tests.cs` |

Cross-module tests that exercise Threading contracts: `QuickFiler.Test/Controllers/WpfUiDispatcherTests.cs`, `QuickFiler.Test/Controllers/QfcItemControllerTests.cs` (TimeOutTask), `TaskMaster.Test/OutlookObjects/Store/StoresWrapperEnumerationScopeTests.cs` and `TaskMaster.Test/AppGlobals/AppOlObjectsAttributionContextTests.cs` (LockupAttribution/CurrentStoreContext), `UtilitiesCS.Test/OutlookObjects/Store/StoreLockupAttributionTests.cs`.

Test files with no direct Threading target (present in the folder but out of scope): `AppGlobalsConverterTests.cs`, `AppGlobalsConverterTests_Unfinished.cs`.

**Implication for AC "no coverage regression on changed lines":** nearly every Threading type has dedicated coverage, including the deterministic seams designed for it (`LockupStallDecider`, `ThreadMonitor.EvaluatePoll`, `CurrentStoreContext`, `WpfUiDispatcher` on a real STA thread). Because the edits are annotation-only, executable-line counts are materially unchanged; the only coverage risk is if a new runtime guard is added to satisfy flow analysis. **Prefer `?` / `= null!` / justified `!` over new `if (x is null) …` guards** so no new uncovered executable line is introduced. Require the full `UtilitiesCS.Test` suite green with `/EnableCodeCoverage` after each batch, and confirm behavior-identical results for the concurrency seams.

---

## 8. Toolchain / per-file verification (deviation confirmed)

1. **Format:** `dotnet tool run csharpier .` (or `csharpier .`) before each build; a pragma line + `?`/`!` edits reflow.
2. **Analyzer/codestyle:** `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`.
3. **Nullable verification — PRAGMA-ONLY GATE (the documented per-child deviation):**
   `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true`.
   **Do NOT pass `/p:Nullable=enable`.** Under `TreatWarningsAsErrors`, any CS86xx in a pragma-enabled file becomes an error while non-opted files stay silent. Passing the global flag would surface the whole epic's ~2131 diagnostics across ~234 files as false failures. This is a deliberate, documented deviation from the stock `.claude/rules/csharp.md` step-3 command and **must not be resolved by editing `.claude/rules/*`** (policy prohibits it); the rules-vs-convention conflict is FLAGGED for the maintainer and owned by the Wave-2 capstone (issue-level, epic §"Rules-vs-convention conflict").
4. **Tests:** `vstest.console.exe <UtilitiesCS.Test assembly> /EnableCodeCoverage` (add QuickFiler/TaskMaster test assemblies when a batch touches a contract they consume — §2). MSTest + Moq + FluentAssertions for any test additions.
5. Restart the loop from step 1 whenever any step changes files or fails.

---

## 9. Automation Feasibility

The entire task is automatable end-to-end with no human-interaction requirement. It is a local C# annotation task with no third-party or interactive-UI dependency:

- **Format** (`csharpier`), **analyze/type-check** (`msbuild /t:Rebuild /p:TreatWarningsAsErrors=true`, pragma-only), and **test** (`vstest.console.exe /EnableCodeCoverage`) all run unattended in the existing toolchain.
- The WinForms forms are remediated by editing only their hand-written partials; no form designer, no live Outlook/WPF host, and no manual UI step is needed (the Designer/resx files are left oblivious — §4). The dispatcher/watchdog tests already run headless on dedicated STA threads (`WpfUiDispatcherTests`, `ThreadMonitor.EvaluatePoll`).
- The only judgment items — the `AsyncMultiTasker` `timer!` vs `timer?.` choice (§3) and the `TimeOutTask` return-type-stability decision (§1) — have documented behavior-preserving defaults (`timer!`, keep `Task<TResult>` + `!`), so they do not require a human gate; they are recorded here for reviewer confirmation, not for interactive resolution.
- Two items are FLAG-only (no code action): the pre-existing `TimeOutTask.cs` 500-line breach and the rules-vs-convention nullable-gate conflict. Neither blocks automated execution.

No `human_interaction` block is required for this child.

---

## 10. Rejected alternatives (brief)

- **Project-level `<Nullable>enable`** — rejected by the maintainer-mandated architecture and epic non-goals; it would make no child independently mergeable and would surface the full ~2131-diagnostic debt at once.
- **`System.Diagnostics.CodeAnalysis` post-condition attributes** (`[NotNullWhen]`, `[MaybeNullWhen]`, `[NotNullIfNotNull]`, …) — rejected: not available/polyfilled on net481 (confirmed by the sibling Extensions research) and unnecessary (zero CS86xx is reachable with plain `?`, `= null!`, and justified `!`). Adding a polyfill would be new production surface (scope creep). This matters specifically for `StoreLockupResponder` (net481's `IsNullOrWhiteSpace` cannot be relied on to refine null-state → use `!` at the guarded call site, not an attribute).
- **Widening `TimeOutTask` returns to `Task<TResult?>`** — rejected as a silent downstream contract change across many consumers (§2). Keep `Task<TResult>` + `!`; escalate any genuine desire to widen to the maintainer.
- **Splitting `TimeOutTask.cs` / `ApplicationIdleTimer.cs` to satisfy the 500-line limit** — rejected: a refactor, out of scope for an annotation-only child. Flag instead.
- **One large batch, or 25 single-file batches** — rejected: the first is unreviewable for a C3 concurrency contract change; the second is excessive churn. Cohesive risk-ordered batches (§6) balance reviewability against the contract-propagation ordering constraint (Batch 3 before Batch 7).
