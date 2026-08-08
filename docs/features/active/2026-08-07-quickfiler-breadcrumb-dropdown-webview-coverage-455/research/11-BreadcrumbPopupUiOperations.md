# F13 Per-File Research — `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs`

- Epic: #136 `quickfiler-per-file-coverage`, child F13, feature issue #455
- Production file: `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs` (494 lines)
- Research date: 2026-08-07
- Author: task-researcher
- Companion artifact: `research/00-cross-cutting-context.md` (shared F13 context; not repeated here)

## 0. Executive summary

| Question | Answer |
|---|---|
| Does the file pass the gates today? | **Yes.** 234/258 = **90.70% line**, 106/120 = **88.33% branch**. Both floors (80% line / 75% branch) cleared. |
| Is it type-level `[ExcludeFromCodeCoverage]`? | **No.** Seven **member-level** attributes at lines 105, 380, 383, 390, 394, 412, 457. Epic manifest `[X]` marker is wrong. |
| Are the seven exemptions defensible? | **Six of seven, yes — one is not.** `DisposeProductionSurface` (line 412) touches no SDK type at all and its body is already executed by existing tests. Exemption unjustified; remove it. |
| Where does the 9.3% line gap come from? | 23 of the 24 uncovered lines are **lambda bodies nested inside exempt members** (`[ExcludeFromCodeCoverage]` does not propagate to compiler-generated closures) plus one structurally unreachable `}`. **None is closeable by a test.** |
| Where does the 11.7% branch gap come from? | 14 uncovered condition-halves at exactly 14 source lines. **13 of 14 are closeable** with ~8 small deterministic tests. |
| 500-line action | Mandatory. File is at 494/500. Recommended split **reduces** it to ~417 lines and simultaneously fixes the lambda leak. |
| STA required? | **No.** Refuted with in-repo evidence. |
| Projected end state | **~99.6% line, ~99.2% branch**, exempt surface *smaller* than today. |

---

## 1. The seven exempted members — member-by-member verdict

Verified: `Grep ExcludeFromCodeCoverage QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs` returns
exactly seven hits, at lines **105, 380, 383, 390, 394, 412, 457**. There is no attribute on the type
declaration at line 29 (`internal sealed class BreadcrumbPopupUiOperations`); lines 25-28 are the doc
comment.

### 1.0 A precision problem with the exemption grounds themselves (DEVIATION)

`CLAUDE.md` §UT2 states three grounds: (a) VSTO add-in lifecycle classes, (b) WinForms form-derived
and Designer-generated code, (c) **Outlook Interop** event-handler classes depending directly on
`Outlook.Application`/`MailItem`/`Store`/`MAPIFolder` without an injectable seam.

**None of the seven members falls under any of the three grounds as literally written.** They touch
`Microsoft.Web.WebView2.*` and `System.Windows.Forms`, not Outlook Interop; none is form-derived.
The exemptions rest instead on:

- #400's ratified `scope_change`
  (`docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/evidence/qa-gates/coverage-accounting-scope-change.2026-07-21T18-01.md:25-30`), and
- the epic's general "irreducible remainder" standard (`epic.md:206-225`), which the epic itself
  frames as the operative test.

**F1's ledger must record these as an explicit ground extension** ("third-party WebView2 SDK adapter
with no seam beneath it, and WinForms popup presentation"), not silently file them under ground (c).
Otherwise a future auditor reading §UT2 literally will find seven unsupported attributes.

### 1.1 Verdict table

| # | Member / signature | Lines | What it does | Branches / state / guards | Unmockable surface | Verdict |
|---|---|---|---|---|---|---|
| E1 | `internal static void ShowOwnedPopup(ToolStripDropDown, Control, Point)` | 105-110 | `dropDown.Show(anchor, anchor.PointToClient(screenLocation))` | none / none / none | `ToolStripDropDown.Show` creates and displays a window; `Control.PointToClient` forces handle creation | **irreducible remainder** |
| E2 | `private static Control CreateProductionControl()` | 380-381 | `new WebView2 { Dock = DockStyle.Fill }` | none / none / none | `Microsoft.Web.WebView2.WinForms.WebView2` construction | **irreducible remainder (weak)** |
| E3 | `private static Task BeginProductionInitialization(IWebViewCoreInitializer, Control, CoreWebView2Environment)` | 383-388 | `initializer.EnsureCoreWebView2Async((WebView2)control, environment)` | none / none / none | the `(WebView2)` cast only — `initializer` is a **mockable interface** | **further decomposable (contingent)** |
| E4 | `private static CoreWebView2 ReadProductionCore(Control)` | 390-392 | `((WebView2)control).CoreWebView2` | none / none / none | `WebView2.CoreWebView2` property read | **irreducible remainder** |
| E5 | `private static Tuple<IWebViewMessenger,Task> BeginProductionNavigation(BreadcrumbUiDispatcher, CoreWebView2, Control, string)` | 394-410 | composes `CreateNavigationSurface(NavigateToDocument(...), () => new WebView2Messenger(core, dispatcher))` | none / none / none | lambda 406 `WebView2.NavigateToString`; lambda 409 `new WebView2Messenger(core, …)` whose ctor subscribes `core.WebMessageReceived` | **irreducible remainder** |
| E6 | `private static void DisposeProductionSurface(Control?, IWebViewMessenger?)` | 412-417 | `DisposeTwoResources(() => (messenger as IDisposable)?.Dispose(), () => control?.Dispose())` | 2 null/type-test branches / none / none | **NONE** — both parameters are already-abstracted types | **NOT JUSTIFIED — remove the exemption** |
| E7 | `private static BreadcrumbNavigationReadiness BindProductionNavigation(BreadcrumbUiDispatcher, CoreWebView2, Control, Action, string)` | 457-492 | supplies the `NavigationSubscriptionFactory` that subscribes 3 SDK events and returns a symmetric detach | none / none / none | `core.NavigationStarting/-Completed` add & remove accessors; `CoreWebView2Navigation*EventArgs` property getters | **irreducible remainder** |

### 1.2 Rationale per verdict

**E1 `ShowOwnedPopup` — irreducible remainder.** Ground: WinForms host-bound presentation. Epic
§Shared Design 2 states verbatim that unit tests "never show popups (a popup requiring human
interaction is a unit-test-policy violation)". The member contains zero decision logic — no guard, no
branch, no state — so nothing of value leaves the denominator. The only arguably-testable fragment,
`anchor.PointToClient(screenLocation)`, itself forces handle creation on a real `Control`.
Corroborating design evidence: the whole point of the parameter is that callers inject it — the
public host constructor at `BreadcrumbDropDownHost.cs:86` takes
`Action<ToolStripDropDown, Control, Point> showPopup`, and tests already substitute a fake there. The
seam is already in place; `ShowOwnedPopup` is only its production binding.

**E2 `CreateProductionControl` — irreducible remainder (weak).** `new WebView2 { Dock = Fill }` may
well be constructible in-memory, so this is not strictly unreachable. But a test could only assert
that a `WebView2` was constructed with `Dock == Fill` — a restatement of the object initializer with
no defect-detection value. `epic.md:521-522` prohibits "shape-assertion tests written purely to
manufacture coverage"; the same principle applies here even though the prohibition is written for the
`interface-only` bucket. **Keep exempt.** The real seam is `Func<Control> _createControl` (line 46),
which every test already substitutes.

**E3 `BeginProductionInitialization` — further decomposable (contingent).** This is the one exempt
forwarder whose collaborator is already an interface. `IWebViewCoreInitializer` is mockable with Moq;
`CoreWebView2Environment` is already obtained in-repo via `FormatterServices.GetUninitializedObject`
(`BreadcrumbPopupControlDispatchTests.cs:226-227`). The only host-bound fragment is the
`(WebView2)control` cast, and a `WebView2` reference can be produced by
`FormatterServices.GetUninitializedObject(typeof(WebView2))` — the identical technique already used
for `CoreWebView2` and `Control` at `BreadcrumbPopupUiOperationsDirectAdapterTests.cs:176, 197, 198`.
Reachability without any visibility change: `new BreadcrumbPopupUiOperations(dispatcher)
.BeginInitializationAsync(mockInitializer, uninitializedWebView2, environment)` routes through
`_beginInitialization`, which the production constructor binds to this member (line 56).

**Contingency and risk.** `System.ComponentModel.Component` has a finalizer that calls
`Dispose(false)`. An uninitialized `WebView2` reaching the finalizer thread could dereference null
SDK fields and, on .NET Framework, terminate the process. The in-repo precedent
(`GetUninitializedObject(typeof(Control))`) is green today, so the risk is empirically low for plain
`Control`, but it has **not** been demonstrated for `WebView2`, whose `Dispose(bool)` touches
`_coreWebView2Controller`. **Recommendation:** attempt the test; if it destabilizes the suite,
reclassify E3 as `irreducible remainder` and record the failed attempt as the evidence. Numerically
this is a zero-sum choice — the member body is excluded either way — so it must not be allowed to
block the plan.

**E4 `ReadProductionCore` — irreducible remainder.** A cast plus a property read; zero decision logic.
The behavior worth pinning — "a null core produces the diagnostic
`Popup CoreWebView2 initialization completed without a core instance.`" — lives in the **non-exempt**
`ReadCoreAsync(Func<WebCore>)` at lines 150-154, which is already covered (Cobertura lines
151-154 `hits="1"`). The exemption therefore removes no untested decision.

**E5 `BeginProductionNavigation` — irreducible remainder.** Every piece of decision logic it composes
is already elsewhere and already covered: `BreadcrumbPopupLifecycleOperations.CreateNavigationSurface`
(`BreadcrumbItemViewerLifecycleCoordinator.cs:357-378`, covered — see
`BreadcrumbPopupUiOperationsDirectAdapterTests.cs:65-79`) and `NavigateToDocument`
(lines 425-439, Cobertura `hits="1"` at 432-439). The exempt residue is two lambdas that call
`WebView2.NavigateToString` and construct `WebView2Messenger` over a live core. **However, those two
lambdas are the file's largest measurement defect — see §4.**

**E6 `DisposeProductionSurface` — NOT JUSTIFIED.** This is the strongest finding of the audit. The
signature is `(Control? control, Messenger? messenger)` — `Control` is the WinForms base type already
constructed in-memory throughout the existing suite, and `Messenger` is the alias for
`IWebViewMessenger`, an interface. **No WebView2 type appears anywhere in the member.** It forwards to
`BreadcrumbPopupLifecycleOperations.DisposeTwoResources`, which is itself non-exempt and directly
tested (`BreadcrumbPopupUiOperationsDirectAdapterTests.cs:115-135`). Decisive evidence that it is
already reachable: its two lambda bodies at source lines **415 and 416 report `hits="1"`** in the
Cobertura report — existing tests already execute this member end-to-end through the production
constructor. The attribute at line 412 is an `[ExcludeFromCodeCoverage]` on a testable seam, which
`epic.md:223` classifies as a **Blocking** finding. Remove it and close its two branch halves (§3,
cases T12-T13).

**E7 `BindProductionNavigation` — irreducible remainder.** `CoreWebView2` is `sealed` with no public
constructor; its `NavigationStarting`/`NavigationCompleted` add and remove accessors marshal into the
native COM interface, so an uninitialized instance throws inside the accessor. The event-args types
(`CoreWebView2NavigationStartingEventArgs`, `CoreWebView2NavigationCompletedEventArgs`) are likewise
sealed with internal constructors and native-backed property getters, so the two translation lambdas
at 472-473 and 474-479 cannot be driven. The *orchestration* it participates in is already fully
seamed and fully tested one level up: `BreadcrumbPopupLifecycleOperations.NavigateWithSubscription`
takes a `NavigationSubscriptionFactory` delegate
(`BreadcrumbItemViewerLifecycleCoordinator.cs:330-334, 434-479`) and
`BreadcrumbPopupUiOperations.NavigateToDocumentCore` (441-455) takes a `NavigationBinder`, both of
which existing tests substitute (`BreadcrumbPopupUiOperationsDirectAdapterTests.cs:82-112, 190-224`).

*Considered and rejected:* extracting an `INavigationEventSource` interface so the subscribe/detach
symmetry could be unit-tested. The interface would need to expose primitives (`ulong navigationId`,
`bool isSuccess`, `string status`) because the SDK arg types are unconstructible, which means the
translation lambdas simply move rather than disappear, and the interface would have exactly one
production implementation. That trades real complexity for ~15 lines of coverage that §4's
relocation removes from the denominator anyway. `CLAUDE.md` §1 "simplicity first" applies.

### 1.3 Net effect on the exempt surface

| | Today | After the recommended change |
|---|---|---|
| Exempted members | 7 | 6 (E6 removed) |
| Exempt lines actually leaving the denominator | 0 of 23 lambda lines (attribute does not propagate) | all 23 |
| Exempt lines wrongly counted as *uncovered production* | 23 | 0 |

The exempt surface shrinks by one member and becomes *honest* — currently the attribute claims to
exempt code that the report still counts.

---

## 2. Full structural map

`internal sealed class BreadcrumbPopupUiOperations` — line 29. Assembly `QuickFiler` grants
`InternalsVisibleTo("QuickFiler.Test")` (`QuickFiler/Properties/AssemblyInfo.cs:5`), so `internal`
visibility is not a testing blocker anywhere in this file.

### 2.1 File-scoped type aliases (lines 13-23) — read before interpreting any signature

| Alias | Actual type |
|---|---|
| `InstalledSurface` | `Tuple<ToolStripControlHost, Control, IWebViewMessenger>` |
| `LegacySurface` | `Tuple<Control, IWebViewMessenger>` |
| `Messenger` | `IWebViewMessenger` |
| `NavigationSurface` | `Tuple<IWebViewMessenger, Task>` |
| `PopupDropDown` | `ToolStripDropDown` |
| `PopupHost` | `ToolStripControlHost` |
| `Readiness` | `BreadcrumbNavigationReadiness` (declared in `BreadcrumbWebViewSurfaceFactory.cs:19`) |
| `ReadySurface` | `Tuple<Control, IWebViewMessenger, Task>` |
| `WebCore` | `CoreWebView2` |
| `WebEnvironment` | `CoreWebView2Environment` |
| `WebInitializer` | `IWebViewCoreInitializer` |

### 2.2 Nested delegate types

| Line | Declaration |
|---|---|
| 31-35 | `internal delegate Task BeginInitialization(IWebViewCoreInitializer, Control, CoreWebView2Environment)` |
| 37-43 | `internal delegate BreadcrumbNavigationReadiness NavigationBinder(BreadcrumbUiDispatcher, CoreWebView2, Control, Action, string)` |

### 2.3 Fields (all `private readonly`, lines 45-50)

| Line | Field | Type | Injected by |
|---|---|---|---|
| 45 | `_dispatcher` | `BreadcrumbUiDispatcher` | both constructors |
| 46 | `_createControl` | `Func<Control>` | 6-arg ctor; production binds `CreateProductionControl` |
| 47 | `_beginInitialization` | `BeginInitialization` | 6-arg ctor; production binds `BeginProductionInitialization` |
| 48 | `_readCore` | `Func<Control, WebCore>` | 6-arg ctor; production binds `ReadProductionCore` |
| 49 | `_beginNavigation` | `Func<WebCore, Control, string, NavigationSurface>` | 6-arg ctor; production binds a **lambda** (line 58) over `BeginProductionNavigation` |
| 50 | `_disposeSurface` | `Action<Control?, Messenger?>` | 6-arg ctor; production binds `DisposeProductionSurface` |

No mutable instance state. The type is effectively immutable after construction; all mutation lives
in method-local scope.

### 2.4 Constructors and injection mechanism

- **52-60 — production constructor** `internal BreadcrumbPopupUiOperations(BreadcrumbUiDispatcher)`.
  Delegates to the 6-arg constructor, binding four method groups and one lambda. The four method-group
  conversions generate Roslyn's cached-delegate pattern, which is why Cobertura reports **line 53 with
  4 jump conditions (8/8 covered)** — that is the `<>O.<n>__X ?? (…)` cache check for
  `CreateProductionControl`, `BeginProductionInitialization`, `ReadProductionCore` and
  `DisposeProductionSurface`, exercised on both cache-miss and cache-hit because tests construct the
  type more than once. **Line 58 is the navigation lambda body and is `hits="0"`.**
- **62-78 — test/injection constructor** taking all six dependencies with six `?? throw
  new ArgumentNullException` guards (71, 72, 73-74, 75, 76, 77). **All six throw-sides are uncovered.**

### 2.5 Members, in file order

| Lines | Visibility | Member | Notes |
|---|---|---|---|
| 80-81 | `internal static` | `CaptureCurrent()` | wraps `BreadcrumbUiDispatcher.CaptureCurrent()` |
| 83-84 | `internal static` | `CreateForCurrentThreadTests()` | wraps the owner-thread-only dispatcher |
| 86-89 | `internal static` | `CaptureCurrentOrTests()` | ternary on `SynchronizationContext.Current == null`; **used by production** — see §6.4 |
| 91-103 | `internal static` | `NormalizeFactory(Func<WebEnvironment, Task<LegacySurface>>)` | null guard (95); returns an **async lambda** (96-102) |
| 105-110 | `internal static` **[Exempt]** | `ShowOwnedPopup(PopupDropDown, Control, Point)` | E1 |
| 112-123 | `internal` | `RunAsync(Action, bool = true)` | null guard (114); adapts `Action` to `Func<bool>` |
| 125-126 | `internal` | `RunAsync<T>(Func<T>, bool = true)` | 1:1 to `DispatchValue` |
| 128 | `internal` | `PostAsync(Action)` | 1:1 to `Dispatch` |
| 130 | `internal` | `Report(Exception)` | 1:1 to `_dispatcher.Report` |
| 132 | `internal` | `CreateControlAsync()` | `RunAsync(_createControl)` |
| 134-139 | `internal` | `BeginInitializationAsync(WebInitializer, Control, WebEnvironment)` | closes over `_beginInitialization` |
| 141-145 | `internal` | `BeginInitializationAsync(Func<Task>)` | null-result guard (143) |
| 147-148 | `internal` | `ReadCoreAsync(Control)` | closes over `_readCore` |
| 150-154 | `internal` | `ReadCoreAsync(Func<WebCore>)` | supplies the missing-core message |
| 156-157 | `internal` | `ReadRequiredAsync<T>(Func<T>, string) where T : class` | generic null-result guard |
| 159-171 | `internal` | `BeginNavigationAsync(WebCore, Control, string)` | validates the returned tuple; disposes a partial result (169) |
| 173-174 | `internal` | `ObserveInitializationAsync(Task)` | `reportCancellation: true` |
| 176-177 | `internal` | `ObserveReadinessAsync(Task)` | `reportCancellation: false` |
| 179-186 | `internal` | `DisposeSurfaceAsync(Control?, Messenger?, bool = true)` | both-null short-circuit to `Task.CompletedTask` |
| 188-191 | `internal` | `DisposeSurfaceAfterFailureAsync(Control?, IWebViewMessenger?)` | swallows via `IgnoreFailureAsync` |
| 193-221 | `internal` | `PlaceSurfaceAsync(…7 args…)` | four `isCurrent()` re-checks (204, 211, 214, 217) |
| 223-237 | `internal` | `DisposeHostedSurfaceAsync(PopupDropDown, PopupHost?, Control?, Messenger?, bool = true)` | four cleanup lambdas via `RetryAsync(retry: false)` |
| 239-244 | `internal` | `DisposeHostedSurfaceAfterFailureAsync(…)` | swallows via `IgnoreFailureAsync` |
| 246-326 | `internal async` | `CreateAndInstallSurfaceAsync(…5 args…)` | the state machine — see §5.2 |
| 328-341 | `private async` | `ObserveExternalAsync(Task, bool)` | null guard (330); **exception filter** (336) |
| 343-350 | `private static async` | `IgnoreFailureAsync(Task)` | bare `catch { }` at 349 |
| 352-376 | `private async` | `RetryAsync(bool, bool, params Action[])` | two nested loops, per-cleanup completion tracking, first-failure retention |
| 378 | `private static` | `Invalid(string)` | `new InvalidOperationException(message)` |
| 380-381 | `private static` **[Exempt]** | `CreateProductionControl()` | E2 |
| 383-388 | `private static` **[Exempt]** | `BeginProductionInitialization(…)` | E3 |
| 390-392 | `private static` **[Exempt]** | `ReadProductionCore(Control)` | E4 |
| 394-410 | `private static` **[Exempt]** | `BeginProductionNavigation(…)` | E5 |
| 412-417 | `private static` **[Exempt]** | `DisposeProductionSurface(Control?, Messenger?)` | E6 |
| 419-423 | `internal static` | `CreateDispatchedReadiness(BreadcrumbUiDispatcher, string, Action)` | fire-and-forget `_ = dispatcher.Dispatch(detachHandlers)` |
| 425-439 | `internal static` | `NavigateToDocument(…5 args…)` | binds `BindProductionNavigation` as the default binder |
| 441-455 | `internal static` | `NavigateToDocumentCore(…6 args…)` | **the injection point**: takes a `NavigationBinder`; four null guards (450-453) |
| 457-492 | `private static` **[Exempt]** | `BindProductionNavigation(…)` | E7 |

---

## 3. Branch inventory

The Cobertura class element for this file is at XML lines **9383-10103** of
`docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml`;
the authoritative flat `<lines>` block is at XML **9648-10102**.

**Independently verified totals: 120 conditions, 106 covered, 14 uncovered halves at 14 distinct
source lines.** This reproduces the orchestrator's 88.33% exactly.

### 3.1 Every conditional in the file

| Source line | Construct | Conditions | Covered | Gap |
|---|---|---:|---:|---|
| 53 | 4× method-group delegate-cache `??` (compiler-generated) | 8 | 8 | — |
| 71 | `dispatcher ?? throw` | 2 | 1 | **throw side** |
| 72 | `create ?? throw` | 2 | 1 | **throw side** |
| 73-74 | `initialize ?? throw` | 2 | 1 | **throw side** |
| 75 | `readCore ?? throw` | 2 | 1 | **throw side** |
| 76 | `navigate ?? throw` | 2 | 1 | **throw side** |
| 77 | `dispose ?? throw` | 2 | 1 | **throw side** |
| 87 | `SynchronizationContext.Current == null ? … : …` | 2 | 2 | — |
| 95 | `factory ?? throw` | 2 | 2 | — |
| 114 | `action ?? throw` | 2 | 2 | — |
| 143 | `initialize() ?? throw` | 2 | 2 | — |
| 157 | `read() ?? throw` | 2 | 2 | — |
| 167 | `navigation?.Item1 != null && navigation.Item2 != null` | 6 | 6 | — |
| 169 | `(navigation?.Item1 as IDisposable)?.Dispose()` | 4 | 4 | — |
| 184 | `control == null && messenger == null` ternary | 4 | 4 | — |
| 204 | `if (!isCurrent())` #1 | 2 | 2 | — |
| 211 | `if (!isCurrent())` #2 | 2 | 2 | — |
| 214 | `if (!isCurrent())` #3 | 2 | 2 | — |
| 217 | `if (!isCurrent())` #4 | 2 | 2 | — |
| 234 | `host?.Dispose()` | 2 | 2 | — |
| 235 | `(host == null && control?.IsDisposed == false ? control : null)?.Dispose()` | 8 | 8 | — |
| 236 | `(messenger as IDisposable)?.Dispose()` | 2 | 2 | — |
| 259 | `created?.Item1 == null \|\| created.Item2 == null \|\| created.Item3 == null` | 8 | 7 | **`created == null` side of the `?.`** |
| 267 | `if (!ReferenceEquals(completed, created.Item3))` | 2 | 2 | — |
| 274 | `(surfaceToDispose.Item2 as IDisposable)?.Dispose()` | 2 | 1 | **non-`IDisposable` messenger side** |
| 283 | `if (!isCurrent())` (install #1) | 2 | 2 | — |
| 292 | `if (!isCurrent())` (install #2) | 2 | 2 | — |
| 298 | `if (!installed)` | 2 | 2 | — |
| 317 | `created?.Item1` / `created?.Item2` in the catch | 4 | 4 | — |
| 324 | `throw;` (async `catch` rewrite artifact) | 2 | 1 | **UNREACHABLE — see §3.3** |
| 330 | `operation ?? throw` | 2 | 1 | **throw side** |
| 336 | `when (reportCancellation \|\| !(exception is OperationCanceledException))` — exception filter | 2 | 2 | — |
| 356 | `attempt < (retry ? 2 : 1)` loop condition + ternary | 4 | 4 | — |
| 358 | `index < cleanups.Length` loop condition | 2 | 2 | — |
| 360 | `if (completed[index]) continue;` | 2 | 2 | — |
| 364 | `report && attempt == 0` (short-circuit on `report`) | 2 | 1 | **`report == false` side** |
| 370 | `failure ??= exception` | 2 | 2 | — |
| 374 | `if (failure != null)` | 2 | 2 | — |
| 415 | `(messenger as IDisposable)?.Dispose()` (E6 lambda) | 2 | 1 | **null / non-`IDisposable` side** |
| 416 | `control?.Dispose()` (E6 lambda) | 2 | 1 | **null-control side** |
| 432 | method-group cache for `BindProductionNavigation` | 2 | 2 | — |
| 450 | `dispatcher ?? throw` | 2 | 2 | — |
| 451 | `core ?? throw` | 2 | 2 | — |
| 452 | `owner ?? throw` | 2 | 2 | — |
| 453 | `bindNavigation ?? throw` | 2 | 1 | **throw side** |
| **Total** | | **120** | **106** | **14** |

### 3.2 The 14 uncovered halves, consolidated

| Source line | What is missing | Closeable? | Test case |
|---|---|:--:|---|
| 71 | 6-arg ctor with `dispatcher = null` | Yes | T1 |
| 72 | 6-arg ctor with `create = null` | Yes | T1 |
| 73 | 6-arg ctor with `initialize = null` | Yes | T1 |
| 75 | 6-arg ctor with `readCore = null` | Yes | T1 |
| 76 | 6-arg ctor with `navigate = null` | Yes | T1 |
| 77 | 6-arg ctor with `dispose = null` | Yes | T1 |
| 259 | factory resolves to a **null** `ReadySurface` | Yes | T3 |
| 274 | cancellation path with a messenger that is **not** `IDisposable` | Yes | T4 |
| 324 | async-`catch` rewrite artifact | **No** | — |
| 330 | `ObserveInitializationAsync(null)` / `ObserveReadinessAsync(null)` | Yes | T2 |
| 364 | `DisposeHostedSurfaceAsync(..., reportFailure: false)` | Yes | T5 |
| 415 | `DisposeSurfaceAsync` with a non-`IDisposable`/null messenger | Yes | T6/T7 |
| 416 | `DisposeSurfaceAsync` with `control == null` | Yes | T7 |
| 453 | `NavigateToDocumentCore(..., bindNavigation: null)` | Yes | T2 |

**13 of 14 closeable → projected 119/120 = 99.17% branch.**

### 3.3 Line 324 / line 325 are structurally unreachable (verified, not assumed)

`CreateAndInstallSurfaceAsync` is `async` and its `catch` block **contains an `await`** (line 317).
Roslyn cannot emit a real CLR catch handler around an await, so it rewrites the region: the exception
is captured to a local, the try is exited, the catch body runs outside the handler, and the rethrow
becomes `ExceptionDispatchInfo.Capture(ex).Throw()`. Everything after that call in the same block is
unreachable IL that still carries a sequence point.

Evidence in the report: the catch body executes (Cobertura lines 315-323 all `hits="1"`, and line 317
reports 4/4 conditions), line 324 `throw;` reports `hits="1"` with `condition-coverage="50% (1/2)"`,
and line 325 — the closing `}` of the catch — reports **`hits="0"`**. Because `throw;` always exits,
no input can make line 325 execute.

**Planning consequence: do not author an atomic task for line 324 or line 325.** They are a
permanent 1-line / 1-branch-half residue. Do not attempt to "fix" them by restructuring the catch —
that is a behavior-affecting change to the failure path, prohibited by the epic's no-behavior-change
NFR, for a 0.4% metric gain.

---

## 4. The uncovered line remainder — all 24 identified

24 of 258 instrumented lines report `hits="0"`. They are, exhaustively:

| Source line(s) | Count | Enclosing member | Nature | Reachable with existing seams? |
|---|---:|---|---|---|
| **58** | 1 | production ctor (52-60), **not exempt** | body of `(core, control, html) => BeginProductionNavigation(dispatcher, core, control, html)` | **No** — invoking it reaches `BindProductionNavigation` → `core.NavigationStarting +=` on a non-live `CoreWebView2` |
| **325** | 1 | `CreateAndInstallSurfaceAsync` | closing `}` after an unconditional `throw;` in an async catch | **No** — structurally unreachable (§3.3) |
| **406** | 1 | `BeginProductionNavigation` (E5, exempt) | `() => ((WebView2)control).NavigateToString(html)` | **No** — needs a live CoreWebView2 |
| **409** | 1 | `BeginProductionNavigation` (E5, exempt) | `() => new WebView2Messenger(core, dispatcher)` | **No** — ctor subscribes `core.WebMessageReceived` |
| **471-490** | 20 | `BindProductionNavigation` (E7, exempt) | the `NavigationSubscriptionFactory` closure and its four nested lambdas | **No** — subscribes three SDK events |

**Zero of the 24 is reachable by a policy-compliant test.** The line figure cannot be improved by
writing tests; it can only be improved by removing the leaked lambdas from the denominator.

### 4.1 Root cause — `[ExcludeFromCodeCoverage]` does not propagate to lambdas

22 of the 24 uncovered lines sit inside members that carry `[ExcludeFromCodeCoverage]`. The attribute
is applied to the *method*, but a lambda is lifted into a compiler-generated closure type whose
method does **not** inherit the attribute, so the collector still instruments it. Confirmed directly
in the report: the `<methods>` list for this class contains no entry for lines 58, 406, 409 or
471-490, yet the class-level `<lines>` block does. (This is `00-cross-cutting-context.md` item L4;
this artifact adds the two previously unidentified lines, **58** and **325**.)

### 4.2 The remedy is empirically proven in this very report

A **type-level** `[ExcludeFromCodeCoverage]` *does* suppress the nested closures. Proof from the same
Cobertura file: `QuickFiler/Viewers/WebView2Messenger.cs` carries a type-level attribute at line 20
and contains lambdas at lines 40-48 and 62-68 — and the file produces **no `filename=` entry at all**
in the report (`Grep filename="QuickFiler\Viewers\WebView2Messenger.cs"` → 0 matches). Same for
`WebView2BreadcrumbHost.cs:29` and `WebView2CoreInitializer.cs:15`.

Therefore: relocating the exempt production members into a **separate type carrying a type-level
attribute** removes their lambdas from the denominator. This is the basis of the §8 split.

### 4.3 Numeric verification of issue #441 (double-counting) on this exact file

The `<class>` attributes read `line-rate="0.929412"` and `branch-rate="0.86875"`, which do **not**
match the recomputed 90.70% / 88.33%. The discrepancy is fully explained:

- `0.929412 = 316/340`. Summing the per-method `<lines>` child counts gives exactly **82** lines, all
  covered. `258 + 82 = 340` and `234 + 82 = 316`. The writer counts method-level `<line>` nodes **in
  addition to** the class-level ones.
- `0.86875 = 139/160`. Per-method conditions sum to exactly **40**, of which **33** are covered.
  `120 + 40 = 160` and `106 + 33 = 139`.

This is an exact, arithmetic confirmation of issue #441 and of F1's decision to recompute from
deduplicated `<line>` nodes with `max(hits)`. **Any child citing a `<class> line-rate` attribute is
citing an inflated figure.** For this file the inflation is +2.24 points line, +1.46 points branch.

---

## 5. Concurrency, ordering, and time

### 5.1 Full inventory (file-wide)

| Construct | Present? | Locations |
|---|:--:|---|
| `lock` | **No** | — (all serialization is delegated to `BreadcrumbUiDispatcher`) |
| `Interlocked` / `Volatile` | **No** | — |
| `SemaphoreSlim` / `Monitor` / `Mutex` | **No** | — |
| `CancellationToken` / `CancellationTokenSource` | **No** | cancellation is modelled as a **`Task`** parameter (251) |
| `TaskCompletionSource` | **No** (in this file) | supplied by callers; `BreadcrumbUiDispatcher.cs:107, 190` owns them |
| `async void` | **No** | — |
| `async Task` methods | 4 + 1 lambda | 246 `CreateAndInstallSurfaceAsync`, 328 `ObserveExternalAsync`, 343 `IgnoreFailureAsync`, 352 `RetryAsync`, and the async lambda at 96-102 |
| `ConfigureAwait(false)` | 11 | 99, 258, 266, 277, 280, 297, 310, 323, 333, 347, 365 — **consistently applied; no omission found** |
| `Task.WhenAny` | 1 | 265 |
| Fire-and-forget (`_ = …`) | 1 | 423 `_ = dispatcher.Dispatch(detachHandlers)` inside `CreateDispatchedReadiness` |
| Timers / `Task.Delay` / `Thread.Sleep` / wall clock | **None** | — |
| Thread-affinity assumption | Implicit | every UI touch routes through `_dispatcher.DispatchValue`/`Dispatch`; `BreadcrumbUiDispatcher` proves the boundary via a `[ThreadStatic]` `_executingDispatcher` (`BreadcrumbUiDispatcher.cs:14-15, 166, 258`) |
| Captured mutable local across the dispatch boundary | 1 | `PopupHost? host` declared 255, assigned 291 inside the dispatched lambda, read 302/313/320 — serialized by the intervening `await` |

**Determinism consequence:** the file reads no clock and starts no timer, so **no `FakeTimeProvider`
or fake-timer facility is required**. Every asynchronous edge is driven by a caller-supplied `Task`
or `TaskCompletionSource` plus a manually pumped `SynchronizationContext`. Both fixtures already
exist in-repo: `SurfaceFactoryFixture.Drain(Task, int workLimit)`
(`BreadcrumbPopupControlDispatchTests.cs:249-279`) and
`QueuedCreatorThreadSynchronizationContext.DrainOnCreatorThread()`
(`BreadcrumbPopupUiOperationsDirectAdapterTests.cs:274-300`). Neither uses `Thread.Sleep`,
`Task.Delay`, or a wall-clock wait.

### 5.2 `CreateAndInstallSurfaceAsync` state machine (lines 246-326)

```
S0  enter
      |  await factory(environment)                     (258)
      +--> throws --------------------------------------> CATCH
S1  created
      |  validate Item1/Item2/Item3                      (259)
      +--> any null ------------------------------------> throw Invalid -> CATCH
      |  await Task.WhenAny(readiness, cancellation)     (265)
      +--> cancellation wins (267 false) --> RetryAsync(retry:true){dispose messenger(274), dispose control(275)} --> return null   [CANCELLED]
      |  await readiness                                 (280)
      +--> readiness faults ----------------------------> CATCH
S2  ready; dispatched install                             (281-296)
      +--> !isCurrent() at 283 -------------------------> installed=false
      |  new ToolStripControlHost(control)               (285-290)  <-- assigns `host`
      +--> !isCurrent() at 292 -------------------------> installed=false
      |  dropDown.Items.Add(host)                        (294)      --> installed=true
S3  installed==false (298) --> DisposeHostedSurfaceAsync(dropDown, host, control, messenger) --> return null   [SUPERSEDED]
S3' installed==true  --> return Tuple(host, control, messenger)                                                [INSTALLED]

CATCH (315-325): DisposeHostedSurfaceAfterFailureAsync(dropDown, host, created?.Item1, created?.Item2); rethrow.
```

Ownership-transfer invariant: on every non-success exit the method **nulls its local ownership
handles before disposing** (`created = null` at 270 and 301; `host = null` at 303) so the `catch`
block cannot double-dispose. This is real, load-bearing logic and it is currently covered.

### 5.3 Illegal / edge transitions and the deterministic mechanism each needs

| Transition | Currently covered? | Deterministic mechanism (no sleeps, no timers) |
|---|:--:|---|
| Factory throws | Yes | factory delegate throws synchronously |
| Factory returns **null** surface | **No** (line 259 gap) | `environment => Task.FromResult<ReadySurface>(null)` |
| Factory returns tuple with null `Item1`/`Item2`/`Item3` | Yes | `[DataRow]`-style tuple permutation, already present |
| **Cancellation wins** over readiness | Yes | pass `Task.CompletedTask` as `cancellation` and an incomplete `TaskCompletionSource.Task` as readiness |
| Cancellation cleanup **fails**, retry succeeds | Yes | `TrackingMessenger.FailOnlyFirstDispose = true` |
| Cancellation with a **non-`IDisposable`** messenger | **No** (line 274 gap) | `Mock<IWebViewMessenger>(MockBehavior.Strict).Object` — the interface does **not** extend `IDisposable` (`IWebViewMessenger.cs:13`) |
| Readiness faults (navigation failure) | Yes | `TaskCompletionSource.SetException` |
| Superseded during install — first `isCurrent()` false | Yes | counter-driven `Func<bool>` |
| Superseded during install — second `isCurrent()` false | Yes | counter-driven `Func<bool>` |
| `dropDown.Items.Add` throws → catch path | Yes | reached via line 317's 4/4 branch coverage |
| Double-open / open-during-close | N/A here | owned by `BreadcrumbDropDownOpenLifetime` / `BreadcrumbDropDownOpenCoordinator`, not this file |
| Dispose-during-initialization | Partially | modelled as the `cancellation` `Task` racing readiness at 265 |
| Initialization failure | Yes | `BreadcrumbPopupControlDispatchTests.cs:46-62` |
| Re-entrant dispatcher callback | Yes (in the dispatcher) | `BreadcrumbUiDispatcher._executingDispatcher` inline path |
| `RetryAsync` with `report: false` | **No** (line 364 gap) | `DisposeHostedSurfaceAsync(..., reportFailure: false)` with a throwing cleanup |
| Both cleanups fail on both attempts → first failure wins | Yes | covered at 370/374 |

---

## 6. UI-thread and live-control dependencies

### 6.1 Per-member requirement

| Member | Needs a real `Control`? | Needs a created **handle**? | Needs a `SynchronizationContext`? | Needs STA? |
|---|:--:|:--:|:--:|:--:|
| `ShowOwnedPopup` (105-110) | Yes | **Yes** (`Show`, `PointToClient`) | Yes | Effectively yes — **and it shows a popup, which is prohibited outright** |
| `PlaceSurfaceAsync` (193-221) | Yes (`ToolStripControlHost`, `Control`, `ToolStripDropDown`) | No — `Size` setters work handle-less | Yes (dispatcher) | **No** |
| `DisposeHostedSurfaceAsync` (223-237) | Yes | No | Yes | **No** |
| `CreateAndInstallSurfaceAsync` (246-326) | Yes — `new ToolStripControlHost(control)` at 285 | No | Yes | **No** |
| `DisposeProductionSurface` (412-417) | Optional (`Control?`) | No | No (called inside a dispatch) | **No** |
| Everything else | No | No | Yes (dispatcher only) | **No** |

### 6.2 STA is not required — refuted with in-repo evidence

`BreadcrumbPopupControlDispatchTests.cs` is a **plain `[TestClass]`** (line 19-20), not a
`*.StaTests.cs`, and it already constructs `TrackingControl : Panel` (line 224, 447) and
`new ToolStripDropDown()` (line 371) and adds a `ToolStripControlHost` to it. The suite is green.
`scripts/vscode/TaskMaster.cli.runsettings` sets `MSTest Parallelize Workers=0 Scope=ClassLevel` with
no apartment override.

**Do not budget any STA infrastructure for this file.** Epic §Shared Design 3's last-resort clause is
not engaged. (Note in passing: the existing precedent constructs in-memory controls *without* the
dedicated `*.StaTests.cs` file the clause prescribes — a pre-existing divergence between the epic
text and the repo's actual practice, worth recording in F1's ledger but not something F13 should
change.)

### 6.3 Minimal seam per host-bound member (interface > injectable delegate > adapter)

Every host-bound member in this file **already has an injectable-delegate seam**, which is the
strongest structural finding of this research:

| Host-bound operation | Existing seam | Seam consumer |
|---|---|---|
| Show the popup | `Action<ToolStripDropDown, Control, Point> showPopup` ctor parameter | `BreadcrumbDropDownHost.cs:86, 108, 128` |
| Create the WebView control | `Func<Control> _createControl` (field 46) | 6-arg ctor |
| Begin core initialization | `BeginInitialization _beginInitialization` (field 47) | 6-arg ctor |
| Read the core | `Func<Control, WebCore> _readCore` (field 48) | 6-arg ctor |
| Begin navigation | `Func<WebCore, Control, string, NavigationSurface> _beginNavigation` (field 49) | 6-arg ctor |
| Dispose the surface | `Action<Control?, Messenger?> _disposeSurface` (field 50) | 6-arg ctor |
| Bind SDK navigation events | `NavigationBinder` delegate (37-43) consumed by `NavigateToDocumentCore` (441-455) | tests inject a fake binder |
| Marshal to the UI boundary | `BreadcrumbUiDispatcher` (constructor-injected) | tests supply a fake `SynchronizationContext` |

**No new seam needs to be created for this file.** The seam architecture is complete. F13's work here
is exemption hygiene, branch closure, and the 500-line split — not seam extraction.

### 6.4 `CaptureCurrent()` / `CreateForCurrentThreadTests()` / `CaptureCurrentOrTests()` — assessment

These are lines 80-89. Assessment of whether test-only affordances in production code are a policy
concern:

- `CaptureCurrent()` (80-81) is genuine production code.
- `CreateForCurrentThreadTests()` (83-84) is **named** for tests but is **reachable from production**:
  `CaptureCurrentOrTests()` selects it whenever `SynchronizationContext.Current == null`, and
  `CaptureCurrentOrTests()` is called from four production sites —
  `BreadcrumbDropDownHost.cs:98` and `:118`, and `ItemViewer.Breadcrumb.cs:156` and `:192`.
- The behavioural difference is material. `BreadcrumbUiDispatcher.CaptureCurrent()` **throws**
  `InvalidOperationException("Breadcrumb UI components must be constructed on an owning UI
  synchronization context.")` (`BreadcrumbUiDispatcher.cs:46-50`). The test dispatcher instead
  returns an owner-thread-only instance that, for any cross-thread request, **reports and returns**
  rather than marshalling (`BreadcrumbUiDispatcher.cs:97-105, 180-188`).

**Verdict: this is a policy concern worth flagging, on fail-fast grounds rather than naming grounds.**
`CLAUDE.md` §3 and `.claude/rules/general-code-change.md` require failing fast and explicitly. The
`OrTests` fallback converts a loud, diagnosable construction-time failure into a silent runtime
degradation in which the breadcrumb popup never opens and only a log line is produced. Recorded as
latent defect **L9** in §11; **do not fix in F13** (behaviour change, and it would break existing
tests that rely on the fallback).

---

## 7. Relationship to `BreadcrumbUiDispatcher`

The two types split the marshalling problem cleanly, and the split is worth stating precisely because
it determines where new tests belong.

| Concern | Owner |
|---|---|
| *Whether* the caller is already on the owning UI boundary | `BreadcrumbUiDispatcher.IsCurrentBoundary()` (`:255-278`) |
| *How* work reaches that boundary (inline vs `SynchronizationContext.Post`) | `BreadcrumbUiDispatcher.Dispatch` / `DispatchValue` (`:71-235`) |
| Error-sink routing and single-report semantics | `BreadcrumbUiDispatcher.Report` (`:238-253`) + `ReportOnce` (`:112-118`, `:195-202`) |
| *What* popup/WebView2 work must run on that boundary, in what order, with what cleanup | `BreadcrumbPopupUiOperations` |
| Binding those operations to the real SDK | the seven exempt members |

`BreadcrumbPopupUiOperations` holds `BreadcrumbUiDispatcher` as a constructor-injected private
readonly field (line 45) and never reaches around it — every UI touch goes through `RunAsync`,
`PostAsync` or `Report`. Consequently: **a test that only needs marshalling semantics belongs in the
dispatcher's test files; a test of this file must supply a fake `SynchronizationContext` and a real
`BreadcrumbUiDispatcher`,** as the existing fixtures do.

**Confirmed: `BreadcrumbPopupUiOperations` is the intended injectable seam.** It is passed explicitly
as the third argument of `BreadcrumbWebViewSurfaceFactory.Create(IWebViewCoreInitializer, string,
BreadcrumbPopupUiOperations)` at `BreadcrumbWebViewSurfaceFactory.cs:173-186`, which null-guards it at
`:183-184` and closes over it in the returned factory at `:185`. The two-argument overload at `:164`
is the production convenience that supplies `CaptureCurrent()`. `BreadcrumbDropDownHost` takes the
same type as a constructor parameter (`BreadcrumbDropDownHost.cs:65, 129`). The seam is contractual
and consumed by two collaborators; **its public/internal signature must be treated as frozen.**

---

## 8. 500-line compliance and the recommended split

At **494 lines** the file has 6 lines of headroom, so it cannot absorb even a doc-comment addition.
A split is mandatory regardless of the coverage work.

### 8.1 Recommendation — extract the production SDK bindings into a new attributed type

Create **`QuickFiler/Viewers/BreadcrumbPopupProductionSurface.cs`** — an `internal static class`
carrying a **type-level** `[ExcludeFromCodeCoverage]`, holding exactly the members whose only content
is a third-party SDK or WinForms presentation call.

This single move accomplishes three things at once:

1. **500-line compliance** — removes ~77 lines from the primary file.
2. **Fixes the lambda leak (§4.1)** — a type-level attribute *does* suppress nested closures, proven
   at `WebView2Messenger.cs` (§4.2). Lines 58, 406, 409 and 471-490 leave the denominator.
3. **Makes the exemption boundary auditable** — one file, one attribute, one ledger row, instead of
   seven scattered member attributes that do not actually do what they claim.

It must be a **separate type, not a `partial` of `BreadcrumbPopupUiOperations`.** An attribute applied
to one partial declaration applies to the whole type, which would exempt the entire 234 covered lines
— a Blocking outcome under `epic.md:223`.

### 8.2 Member allocation

| Member | Destination | Rationale |
|---|---|---|
| `ShowOwnedPopup` (105-110) | **new file** | E1 irreducible |
| `CreateProductionControl` (380-381) | **new file** | E2 irreducible |
| `BeginProductionInitialization` (383-388) | **new file** (see §1.2 contingency) | E3 |
| `ReadProductionCore` (390-392) | **new file** | E4 irreducible |
| `BeginProductionNavigation` (394-410) | **new file** | E5 irreducible; carries lambdas 406/409 |
| `BindProductionNavigation` (457-492) | **new file** | E7 irreducible; carries lambdas 471-490 |
| *new* `NavigationBindingFor(BreadcrumbUiDispatcher)` | **new file** | returns the delegate currently written inline as the lambda at line 58, removing it from the primary file's denominator |
| **`DisposeProductionSurface` (412-417)** | **STAYS** in the primary file, **with its `[ExcludeFromCodeCoverage]` removed** | E6 is not justified (§1.2); its lambdas at 415/416 are already executed and their two remaining branch halves are closeable |

The production constructor (52-60) is rewritten to bind the four relocated method groups plus
`BreadcrumbPopupProductionSurface.NavigationBindingFor(dispatcher)` and the local
`DisposeProductionSurface` — **no lambda remains in the constructor**.

`NavigateToDocument` (425-439) rebinds its default binder to
`BreadcrumbPopupProductionSurface.BindNavigation`; `BreadcrumbDropDownHost.cs:74` rebinds
`BreadcrumbPopupUiOperations.ShowOwnedPopup` → `BreadcrumbPopupProductionSurface.ShowOwnedPopup`
(a one-line edit in an F13-owned file; grep confirms this is the **only** call site).

### 8.3 Projected line counts

| File | Lines | Headroom |
|---|---:|---:|
| `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs` (after) | **~417** | ~83 |
| `QuickFiler/Viewers/BreadcrumbPopupProductionSurface.cs` (new) | **~110-125** | ~375 |

Derivation of the reduction: `ShowOwnedPopup` block 105-111 (7) + production block 380-411 (32) +
the attribute line 412 (1) + `BindProductionNavigation` block 456-492 (37) = **77 lines removed**;
494 − 77 = 417, before the small additions for the rebound constructor and the removed
`Microsoft.Web.WebView2.WinForms` using directive.

*Contingency:* if a later change pushes the primary file back toward 500, the natural second cut is a
`BreadcrumbPopupUiOperations.Install.cs` partial holding `PlaceSurfaceAsync`,
`DisposeHostedSurfaceAsync`, `DisposeHostedSurfaceAfterFailureAsync`, `CreateAndInstallSurfaceAsync`
and `RetryAsync` (lines 193-376, ~184 lines). Not needed now; do not do it speculatively.

### 8.4 Build and ledger obligations

- Add exactly one entry to `QuickFiler/QuickFiler.csproj`, inside the existing F13 block at lines
  396-411, adjacent to line 397:
  `    <Compile Include="Viewers\BreadcrumbPopupProductionSurface.cs" />`
  The file is **CRLF-terminated on every line**. Use the `Edit` tool, or `perl -0777` with explicit
  `\r\n`. A git-bash `sed -i` strips CRLF and produces a whole-file diff that is guaranteed to
  conflict with F12 (whose entries at 393-395 and 400 are interleaved with F13's).
- Append two coverage-ledger rows in the same change:
  - `QuickFiler/Viewers/BreadcrumbPopupProductionSurface.cs` → **`ratified-exempt`**. Rationale:
    third-party WebView2 SDK forwarders and WinForms popup presentation with no seam beneath them;
    zero decision logic; every consumer already injects a delegate seam over it. This is not a *new*
    exemption — it is the relocation of six already-ratified member exemptions, minus one that is
    being withdrawn. Precedent in the same folder: `WebView2CoreInitializer.cs` (30 lines,
    type-level attribute, doc comment at `:12-14` naming the "exempt-forwarder pattern"), cited in
    turn by `WebView2Messenger.cs:14-18`.
  - `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs` → **`testable`**, target >= 80% line /
    >= 75% branch (existing file; the >= 90% new-file rule does not apply to it).
- Note the interaction flagged in `00-cross-cutting-context.md` §5.4: that artifact recommends
  *removing* the type-level exemptions from `WebView2Messenger.cs` and `WebView2BreadcrumbHost.cs`.
  There is no contradiction — those two types contain guards, disposal state and dispatcher routing
  (real decision logic), whereas `BreadcrumbPopupProductionSurface` would contain none. F1's ledger
  should state that distinction explicitly so the two decisions read as one consistent standard.

### 8.5 Projected coverage after the split plus §10's tests

| Metric | Today | After split (no new tests) | After split + T1-T7 |
|---|---:|---:|---:|
| Instrumented lines | 258 | 235 | 235 |
| Covered lines | 234 | 234 | 234 |
| **Line %** | **90.70%** | **99.57%** | **99.57%** |
| Conditions | 120 | 120 | 120 |
| Covered conditions | 106 | 106 | 119 |
| **Branch %** | **88.33%** | **88.33%** | **99.17%** |

(The split removes only `hits="0"` lines — 58, 406, 409, 471-490 — none of which carries a branch, so
the branch denominator is unchanged. Lines 415/416 and their four conditions remain, by design.)
Residual after all work: **line 325** (1 line) and **half of line 324** (1 condition), both
structurally unreachable per §3.3.

---

## 9. Existing tests

### 9.1 Files that target this type

| Test file | Lines | Headroom | What it asserts about `BreadcrumbPopupUiOperations` |
|---|---:|---:|---|
| `QuickFiler.Test/Viewers/BreadcrumbPopupUiOperationsDirectAdapterTests.cs` | 302 | **198** | `ReadRequiredAsync` present/absent; `BeginInitializationAsync(Func<Task>)` throw and null-task paths; `NavigateToDocument` null-guards for `dispatcher`/`core`/`owner`; `NavigateToDocumentCore` with an injected binder. Also the `PopupFixture` + `QueuedCreatorThreadSynchronizationContext` harness. |
| `QuickFiler.Test/Viewers/BreadcrumbPopupControlDispatchTests.cs` | 486 | 14 | Full surface-factory sequencing and off-boundary detection; initialization / navigation-action / readiness failures; `DisposeSurfaceAsync` messenger-failure ordering; `CreateAndInstallSurfaceAsync` cancellation-cleanup retry and stale-host cleanup; `CreateDispatchedReadiness` dispatch and post-failure; `NormalizeFactory(null)` guard; 3-row invalid-navigation-tuple matrix. Owns `SurfaceFactoryFixture`, `TrackingControl : Panel`, `TrackingMessenger`. |
| `QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.Part2.cs` | 480 | 20 | Injected-factory failure matrix (create/initialize/core/navigate/cleanup); `CaptureCurrentOrTests` null-vs-controlled context selection; `NormalizeFactory` success and null-result contract; `BreadcrumbNavigationReadiness` lifecycle. |
| `QuickFiler.Test/Viewers/BreadcrumbPopupBoundaryCoverageTests.cs` | 361 | **139** | Primary partial: shared harness helpers plus the remaining boundary cases. |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownReadinessTests.cs` | 498 | 2 | `DisposeSurfaceAsync(null, null)` short-circuit identity; `ObserveReadinessAsync` cancellation rethrow **without** reporting; `ObserveInitializationAsync` cancellation reports the identical exception once. |
| `QuickFiler.Test/Viewers/BreadcrumbSelectorOpenRetryTests.cs` | 461 | 39 | `PlaceSurfaceAsync` and `DisposeHostedSurfaceAsync` through the host retry path. |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownCoverageThresholdTests.cs` | 477 | 23 | `NormalizeFactory(legacyFactory)` in the host construction path. |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownLifecycleCoverageTests.cs` | 469 | 31 | Lifetime paths that route through `PlaceSurfaceAsync` / `DisposeHostedSurfaceAsync`. |
| `QuickFiler.Test/Viewers/BreadcrumbSelectorToggleUiBoundaryTests.cs` | 478 | 22 | UI-boundary toggle paths; exports `CapturingSynchronizationContext` reused by `…BoundaryCoverageTests.Part2.cs:13`. |
| `QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.cs` / `.Part2.cs` | 447 / 381 | 53 / 119 | Coordinator paths that consume the operations object. |

### 9.2 Headroom correction (DEVIATION from the delegation brief)

The brief instructs "assume every new test needs a NEW file". That is the right default, but two
existing F13-primary files have **material** headroom and the brief's framing understates it:
`BreadcrumbPopupUiOperationsDirectAdapterTests.cs` has **198 lines** free and
`BreadcrumbPopupBoundaryCoverageTests.cs` has **139**. The recommendation to use new files stands
anyway, for two reasons that are stronger than headroom: (i) F12 and F13 will fan in concurrently and
new files produce no textual conflict, and (ii) the projected test volume (~7 methods plus two
fixtures) exceeds what either file can absorb without breaching 500.

### 9.3 Reusable harness assets (do not re-implement)

| Asset | Location | Use |
|---|---|---|
| `SurfaceFactoryFixture : SynchronizationContext` with `Drain(Task, int workLimit)` | `BreadcrumbPopupControlDispatchTests.cs:209-419` | deterministic manual pump; also builds `Operations(initialization, readiness, navigation)` with all six delegates injected |
| `QueuedCreatorThreadSynchronizationContext` + `DrainOnCreatorThread()` | `BreadcrumbPopupUiOperationsDirectAdapterTests.cs:274-300` | simpler single-thread pump |
| `TrackingControl : Panel` (dispose counting, failure injection, `SuppressBaseDisposal`) | `BreadcrumbPopupControlDispatchTests.cs:447-463` | in-memory control |
| `TrackingMessenger : IWebViewMessenger, IDisposable` (`FailOnlyFirstDispose`) | `BreadcrumbPopupControlDispatchTests.cs:465-484` | disposable messenger |
| `FormatterServices.GetUninitializedObject(typeof(CoreWebView2))` / `typeof(Control))` / `typeof(CoreWebView2Environment))` | `BreadcrumbPopupUiOperationsDirectAdapterTests.cs:176, 197, 198`; `BreadcrumbPopupControlDispatchTests.cs:225-227` | unconstructible sealed SDK types |
| `RecordingNavigationBinding` | `BreadcrumbPopupUiOperationsDirectAdapterTests.cs:244-272` | fake `NavigationSubscriptionFactory` |

These fixtures are `private sealed` nested classes, so a new test file must declare its own. Keep
them minimal rather than lifting the 210-line `SurfaceFactoryFixture` wholesale.

---

## 10. Recommended test-case list

MSTest `[TestClass]`/`[TestMethod]`, Moq, FluentAssertions, Arrange–Act–Assert. Deterministic: no
`Thread.Sleep`, no `Task.Delay`, no wall-clock wait, no temp file, no live form, no popup, no STA.
Each row is sized to be one atomic plan task.

### New test files (each needs a `<Compile Include>` in `QuickFiler.Test/QuickFiler.Test.csproj`, breadcrumb block lines 58-89)

- **`QuickFiler.Test/Viewers/BreadcrumbPopupUiOperationsGuardTests.cs`** — T1, T2 (~170 lines)
- **`QuickFiler.Test/Viewers/BreadcrumbPopupInstallEdgeTests.cs`** — T3, T4 (~200 lines)
- **`QuickFiler.Test/Viewers/BreadcrumbPopupDisposalBoundaryTests.cs`** — T5, T6, T7 (~200 lines)

### Cases

| ID | Test method | File | Target | Sketch |
|---|---|---|---|---|
| **T1** | `Constructor_AnyNullDependency_ThrowsArgumentNullExceptionNamingIt` (`[DataTestMethod]`, 6 `[DataRow]`s indexed 0-5) | Guard | lines **71, 72, 73, 75, 76, 77** | Build a valid 6-tuple of dependencies; null exactly the indexed one; `Action ctor = () => new BreadcrumbPopupUiOperations(...)`; assert `Throw<ArgumentNullException>().WithParameterName(expected)`. Parameter names: `dispatcher`, `create`, `initialize`, `readCore`, `navigate`, `dispose`. |
| **T2** | `NullOperationAndNullBinder_FaultWithArgumentNullException` | Guard | lines **330, 453** | (a) `await operations.ObserveInitializationAsync(null)` must fault `ArgumentNullException("operation")` — it is `async`, so assert on the awaited task, not a synchronous throw; repeat for `ObserveReadinessAsync(null)` (same line 330). (b) `NavigateToDocumentCore(dispatcher, core, owner, () => {}, "Popup", null)` must throw `ArgumentNullException("bindNavigation")`; use `GetUninitializedObject` for `core`/`owner` exactly as `…DirectAdapterTests.cs:196-198` does. |
| **T3** | `CreateAndInstallSurfaceAsync_FactoryYieldsNullSurface_ThrowsDiagnosticAndCleansUp` | Install | line **259**, condition 0 | Factory `environment => Task.FromResult<Tuple<Control, IWebViewMessenger, Task>>(null)`. Assert `InvalidOperationException` with message `"Popup initialization did not provide a control, messenger, and readiness task."`, that `dropDown.Items` stays empty, and that the error sink recorded it exactly once. |
| **T4** | `CreateAndInstallSurfaceAsync_CancellationWithNonDisposableMessenger_DisposesControlOnly` | Install | line **274** | `cancellation = Task.CompletedTask`; readiness = an incomplete `TaskCompletionSource.Task`; messenger = `new Mock<IWebViewMessenger>(MockBehavior.Strict).Object` (the interface does **not** extend `IDisposable` — `IWebViewMessenger.cs:13`). Assert the method returns `null`, the control was disposed exactly once, and no error was reported. |
| **T5** | `DisposeHostedSurfaceAsync_ReportFailureFalse_SuppressesTheReportButStillThrows` | Disposal | line **364** | Host whose `Dispose()` throws. Call `DisposeHostedSurfaceAsync(dropDown, host, control, messenger, reportFailure: false)`. Assert the returned task faults with that exception **and** the dispatcher error sink is empty. Contrast with the default (`true`) in one extra assertion so the pair reads as a matrix. |
| **T6** | `DisposeSurfaceAsync_ControlOnly_DisposesControlAndSkipsMessenger` | Disposal | lines **415 (null half), 416 (non-null half)** | Production-constructed operations (`new BreadcrumbPopupUiOperations(dispatcher)`), so `_disposeSurface` binds `DisposeProductionSurface`. Call with `(control: trackingControl, messenger: null)`. Assert control disposed once. **Requires the E6 exemption to be removed first.** |
| **T7** | `DisposeSurfaceAsync_NonDisposableMessengerWithoutControl_CompletesWithoutDisposal` | Disposal | lines **415 (non-`IDisposable` half), 416 (null half)** | Production-constructed operations; call with `(control: null, messenger: mockMessenger)` where the mock implements only `IWebViewMessenger`. Assert the task completes, no exception, `MockBehavior.Strict` records no call. Note this exercises the non-short-circuit path of `DisposeSurfaceAsync` (line 184) because only one argument is null. |
| **T8** *(contingent, optional)* | `BeginInitializationAsync_ProductionBinding_ForwardsControlAndEnvironmentToInitializer` | Guard | E3 behaviour pin | Only if E3 is reclassified `testable` per §1.2. `Mock<IWebViewCoreInitializer>` + `(Control)FormatterServices.GetUninitializedObject(typeof(WebView2))` + `GetUninitializedObject(typeof(CoreWebView2Environment))`; assert the mock received the same instances and the returned task is the mock's. **Abandon and record the evidence if the uninitialized `WebView2` destabilizes the suite** (finalizer risk, §1.2). Contributes no denominator either way. |

### Explicitly NOT to be written

- Any test for line **324** or line **325** — structurally unreachable (§3.3).
- Any test for lines **58, 406, 409, 471-490** — unreachable without a live WebView2; the §8 split
  removes them from the denominator instead.
- A shape-assertion test for `CreateProductionControl` (asserting `Dock == Fill`) — prohibited in
  spirit by `epic.md:521-522` and of no defect-detection value.
- Any test that constructs a `Form`, calls `ToolStripDropDown.Show`, or requires STA.

---

## 11. Latent defects

Do **not** fix these in F13. Promote each through the MCP promotion lifecycle per `epic.md:538-546`.
`00-cross-cutting-context.md` §9 already lists L1-L8; the following are the findings specific to this
file. **L4 is restated here because this artifact adds two lines it did not identify.**

| ID | Location | Defect and impact | Confidence |
|---|---|---|---|
| **L4** (restated + extended) | `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs:394` and `:457` | `[ExcludeFromCodeCoverage]` on a **method** does not suppress instrumentation of lambdas lifted out of it. Source lines **406, 409, 471-490** remain instrumented and permanently uncovered. **This artifact adds line 58** — the same defect inside the *non-exempt* production constructor. 23 of the file's 24 uncovered lines are this one mechanism. Repo-wide measurement concern: any file using method-level exemption over lambda-bearing bodies silently misstates its exemption boundary. Proof of the remedy: type-level attributes *do* suppress closures (`WebView2Messenger.cs:20` with lambdas at `:40-48`, `:62-68`, absent from the report). | High (measured) |
| **L7** (restated) | `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs:349` | Bare `catch { }` in `IgnoreFailureAsync` with no rethrow and **no explanatory comment**. `.claude/rules/general-code-change.md` § Error Handling prohibits silent swallowing without immediate re-raise or added context. The swallow is arguably intended (it is the "after failure" cleanup path), which makes the missing rationale comment the actual defect. | High (textual) |
| **L9** *(new)* | `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs:86-89` (`CaptureCurrentOrTests`) | Silent degradation instead of fail-fast. When `SynchronizationContext.Current == null`, production callers (`BreadcrumbDropDownHost.cs:98, 118`; `ItemViewer.Breadcrumb.cs:156, 192`) receive an owner-thread-only dispatcher that **reports rather than marshals** every cross-thread UI request (`BreadcrumbUiDispatcher.cs:97-105, 180-188`). The popup then silently fails to open, with only a log entry — whereas `CaptureCurrent()` would have thrown a precise diagnostic at construction (`BreadcrumbUiDispatcher.cs:46-50`). Violates `CLAUDE.md` §3 / `.claude/rules/general-code-change.md` "fail fast and explicitly". | Medium-High |
| **L10** *(new)* | `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs:246-326` (`CreateAndInstallSurfaceAsync`) | The `await` inside the `catch` block (line 317) forces Roslyn's pending-exception rewrite, producing permanently unreachable IL at line 325 and an uncoverable branch half at line 324. This is a *measurement* defect rather than a behaviour defect — the failure path is correct — but it means **no file using `await` inside `catch` can reach 100%**. Worth an issue so the pattern is recognised repo-wide rather than re-diagnosed by each child. | High (measured) |
| **L11** *(new, minor)* | `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs:412` | `[ExcludeFromCodeCoverage]` applied to `DisposeProductionSurface`, whose signature is `(Control?, IWebViewMessenger?)` and which contains **no third-party SDK type**. Under `epic.md:223` an exemption on a testable seam is a Blocking finding. Its two lambda bodies already report `hits="1"`, proving reachability. **Unlike L4/L7/L9/L10 this one is in scope for F13's own execution** (it is an attribute removal on an F13-owned file with no behaviour change), and is a prerequisite for cases T6/T7. | High (measured) |

---

## 12. Deviations from the delegation brief, consolidated

| # | Brief statement | Finding |
|---|---|---|
| 1 | "SEVEN METHOD-LEVEL `[ExcludeFromCodeCoverage]` at lines 105, 380, 383, 390, 394, 412, 457" | **Confirmed exactly.** No type-level attribute at line 29. Epic manifest `[X]` marker at `epic.md:418` is wrong; carry the correction into `spec.md`. |
| 2 | 258 lines / 234 covered / 90.7%; 120 conditions / 106 covered / 88.3% | **Independently reproduced**, line by line, from XML 9648-10102. Additionally proved the `<class line-rate="0.929412">` / `branch-rate="0.86875"` attributes are inflated by exactly the per-method duplicate blocks (82 lines, 40 conditions) — a numeric confirmation of issue #441. |
| 3 | "I assess the seven exemptions as likely defensible" | **Refuted for one of seven.** E6 `DisposeProductionSurface` (line 412) touches no unmockable type and is already executed by existing tests. Six of seven hold. |
| 4 | "roughly 14 uncovered conditions; identify them specifically" | **14 exactly**, at lines 71, 72, 73, 75, 76, 77, 259, 274, 324, 330, 364, 415, 416, 453. **13 are closeable**; line 324 is not. |
| 5 | "24 of 258 instrumented lines are uncovered … state which are reachable" | **All 24 identified: 58, 325, 406, 409, 471-490. None is reachable.** `00-cross-cutting-context.md` §3.7 identified only 22 and did not name 58 or 325. |
| 6 | "`BreadcrumbPopupUiOperations` is a fully-realized injectable-delegate seam architecture" | **Confirmed and strengthened.** Every host-bound operation already has a seam (§6.3). **No new seam is required for this file** — F13's work here is exemption hygiene, branch closure and the split. |
| 7 | "`NormalizeFactory` (91-103) is decision logic in a non-exempt testable counterpart" | **Confirmed and already covered** (lines 94-103 all `hits="1"`, guard at 95 at 2/2), by `…BoundaryCoverageTests.Part2.cs:197-224` and `BreadcrumbPopupControlDispatchTests.cs:176-177`. |
| 8 | "Assume every new test needs a NEW file" | **Adopted, but the premise is partly inaccurate.** `BreadcrumbPopupUiOperationsDirectAdapterTests.cs` has 198 lines of headroom and `BreadcrumbPopupBoundaryCoverageTests.cs` has 139. New files are still recommended, for fan-in isolation rather than for headroom. |
| 9 | "Only where no seam is feasible, propose an STA test per epic §3" | **No STA test is needed anywhere in this file.** `BreadcrumbPopupControlDispatchTests.cs` is a plain `[TestClass]` that already constructs `Panel`, `ToolStripDropDown` and `ToolStripControlHost` in-memory and is green. |
| 10 | "Cross-child coupling risk: `BreadcrumbPopupLifecycleOperations` / `BreadcrumbNavigationSubscription` live in F12-owned `BreadcrumbItemViewerLifecycleCoordinator.cs`" | **Confirmed** (`:355`, `:337`; F12 file at 481 lines). Call sites in our file: 401, 414, 466. **The recommended design does not deepen the dependence** — it moves two of the three call sites (401, 466) into the new exempt file, where they cease to be measured at all, and leaves only 414 (`DisposeTwoResources`) in the measured primary file. If F12 splits its file for the 500-line rule, only namespace-level references matter, so a pure file move on F12's side is source-compatible with no edit on ours. |
| 11 | "`BreadcrumbNavigationReadiness` (the `Readiness` alias) lives in `BreadcrumbWebViewSurfaceFactory.cs:19` — our own file; one source file declaring two types matters for Cobertura per-file aggregation" | **Confirmed and materially important.** That file declares `BreadcrumbNavigationReadiness` (`:19`) *and* `BreadcrumbWebViewSurfaceFactory` (`:162`), and the report emits **one** `<class>` named `QuickFiler.Viewers.BreadcrumbNavigationReadiness` carrying both types' lines. Any harness keyed on the Cobertura **class name** rather than the `filename` attribute will lose `BreadcrumbWebViewSurfaceFactory` entirely. Key on `filename`. |
| 12 | "`ShowOwnedPopup` … showing a popup is itself a unit-test-policy violation" | **Confirmed** against `epic.md` Shared Design §2 verbatim. Also note the seam already exists: `BreadcrumbDropDownHost.cs:86` takes `Action<ToolStripDropDown, Control, Point> showPopup`, and `ShowOwnedPopup` is only its production binding (single call site, `BreadcrumbDropDownHost.cs:74`). |

---

## 13. Summary of recommended work for this file

1. **Remove** the `[ExcludeFromCodeCoverage]` at line **412** (E6) — Blocking finding under
   `epic.md:223`.
2. **Create** `QuickFiler/Viewers/BreadcrumbPopupProductionSurface.cs` with a type-level
   `[ExcludeFromCodeCoverage]`, and relocate E1, E2, E3, E4, E5, E7 plus the constructor's navigation
   lambda into it. Add the `<Compile Include>` (CRLF-preserving) and two ledger rows.
3. **Rebind** the four call sites: production constructor (52-60), `NavigateToDocument` (438), and
   `BreadcrumbDropDownHost.cs:74`.
4. **Add** test cases T1-T7 in three new test files, plus their `<Compile Include>` entries.
5. **Record** the unreachable residue (line 325, half of line 324) in the coverage evidence so the
   capstone does not re-investigate it.
6. **Promote** L9, L10, L11 as GitHub issues; L4 and L7 are already carried by
   `00-cross-cutting-context.md`.

Projected: **99.57% line, 99.17% branch**, with an exempt surface that is one member smaller and, for
the first time, actually excluded from measurement.
