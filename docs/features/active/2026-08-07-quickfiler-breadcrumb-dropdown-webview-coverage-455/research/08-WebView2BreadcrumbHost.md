# F13 Research — `QuickFiler/Viewers/WebView2BreadcrumbHost.cs`

- Epic: #136 `quickfiler-per-file-coverage`, child F13, feature issue #455
- Production file: `QuickFiler/Viewers/WebView2BreadcrumbHost.cs` (143 lines)
- Current state: class-level `[ExcludeFromCodeCoverage]` at **line 29**; entirely absent from the
  committed Cobertura instrumentation (unmeasured, not covered)
- Research date: 2026-08-07
- Companion artifacts: `00-cross-cutting-context.md` (shared evidence — not repeated here),
  `09-WebView2Messenger.md`, `10-WebView2CoreInitializer.md`

## 0. Tooling limitation (read first)

No `Bash`/shell tool was available in this session. Only `Read`, `Grep`, `Glob`, `Write`, `Edit`,
`WebFetch`. Therefore **no `git`, no `gh`, no `msbuild`, no `vstest`, no `csharpier` was executed**.
Every finding below is derived from working-tree file content, from committed Cobertura evidence, or
from the official Microsoft WebView2 .NET API reference for the exact package version referenced by
this project. Items that could not be verified without compiling are labelled
**UNVERIFIED — needs a Phase-0 spike**.

---

## 1. Headline verdict

**The file's own exemption justification is REFUTED.** The doc comment at lines 14 and 23-27 claims
"1:1 SDK-forwarding adapter" and "every member forwards 1:1 to the WebView2 SDK". The body
contradicts this in four places:

| Claim location | Contradicting code | What it actually is |
|---|---|---|
| `:14`, `:23-24` "every member forwards 1:1" | `:72-84` `PostMessageJson` | A null-core guard with an **error-log-and-drop early return**. Payload is silently discarded before initialization. That is a behavioural contract, not a forward. |
| same | `:115-136` `OnCoreInitializationCompleted` | A failure branch (`:120-127`), an idempotent unhook/hook pair (`:131-132`), an `IsCoreInitialized` **state transition** (`:134`), and a conditional event raise (`:135`). |
| same | `:43-51` constructor | Two null guards (`:45`, `:46`) and idempotent event hookup (`:49-50`). |
| same | `:92-113` `InitializeAsync` | A null guard, a cache-folder computation, an options construction, a **documented ordering invariant** (UI-context hop must precede environment creation, `:105-106`), and two sequenced awaits. |

The orchestrator's preliminary finding #3 is **CONFIRMED** and extended: `InitializeAsync` is not
merely "not a forward", it is **already fully testable today** behind the pre-existing
`IWebViewCoreInitializer` seam plus a fake `SynchronizationContext`, with **no refactor at all**.
That alone makes the class-level `[ExcludeFromCodeCoverage]` at `:29` a Blocking finding under epic
Shared Design §1 (`epic.md:218-225`): "`[ExcludeFromCodeCoverage]` on a *testable* seam is a
Blocking finding."

**Disposition: remove the class-level exemption. Move the residual SDK surface into one
class-level-exempt adapter type. Target >= 90% line / >= 80% branch on this file.**

---

## 2. Member-by-member testability verdict

Coverable-line and branch counts are estimates from source reading; the Cobertura numbers do not
exist because the file is unmeasured.

| # | Member | Lines | Branches / state / guards | Unmockable SDK type touched | Verdict |
|---|---|---|---|---|---|
| 1 | `log` static field initializer | 32-34 | none | none | **testable today** (executes on first type use) |
| 2 | ctor `(WebView2, IWebViewCoreInitializer)` | 43-51 | 2 null guards (2 branches); idempotent `-=`/`+=` pair | `WebView2` (field type, event add/remove) | **testable behind seam S1** — split into an exempt production ctor + a non-exempt seam ctor |
| 3 | `IsCoreInitialized { get; private set; }` | 54 | none | none | **testable today** |
| 4 | `event MessageReceived` (add/remove) | 57 | none | none | **testable today** (field-like event) |
| 5 | `event CoreInitialized` (add/remove) | 63 | none | none | **testable today** |
| 6 | `NavigateToString(string)` | 66-69 | none | `WebView2.NavigateToString` | **testable behind seam S2** (`IBreadcrumbControlSurface.NavigateToString`) |
| 7 | `PostMessageJson(string)` | 72-84 | **1 branch** (`core == null`), log + early return, else forward | `WebView2.CoreWebView2` (read) and `CoreWebView2.PostWebMessageAsJson` | **testable behind seam S3** — both branches become directly assertable |
| 8 | `InitializeAsync(SynchronizationContext)` | 92-113 | 1 null guard; `Path.Combine`; `new CoreWebView2EnvironmentOptions()`; `await uiSyncContext` context hop; 2 sequenced awaits | `CoreWebView2EnvironmentOptions` ctor (constructible — see §4); `_control` passed to the already-mocked `IWebViewCoreInitializer` | **testable today** (needs only `_control` to become optional; see S4) |
| 9 | `OnCoreInitializationCompleted(object?, CoreWebView2InitializationCompletedEventArgs)` | 115-136 | `!e.IsSuccess` branch; `?.` on `InitializationException`; unhook/hook pair; state transition; `?.Invoke` | `_control.CoreWebView2` (`:129`), `core.WebMessageReceived` (`:131-132`). **The args type itself is constructible** (§4) | **failure branch testable today**; success branch **testable behind seam S5** (`IBreadcrumbControlSurface.BindMessageHandler`) |
| 10 | `OnWebMessageReceived(object?, CoreWebView2WebMessageReceivedEventArgs)` | 138-141 | `?.Invoke` null-conditional | `CoreWebView2WebMessageReceivedEventArgs.WebMessageAsJson` — **no public constructor, non-virtual members, finalizer-bearing** (§4) | **irreducible remainder as written**; split into a testable `RaiseMessageReceived(string)` plus a 1-line bridge that lives inside the exempt adapter |

**Irreducible remainder after the proposed refactor** — exactly these operations, all relocated into
one class-level-exempt adapter type:

| Operation | Exemption ground cited |
|---|---|
| `control.CoreWebView2` property read | Third-party SDK adapter: the property is non-virtual on a `Control`-derived type whose value is produced only by a live browser process. Not covered by any of the three literal CLAUDE.md §UT2 grounds — see §9. |
| `core.PostWebMessageAsJson(json)` | Same. Single statement, no branch. |
| `control.NavigateToString(html)` | Same. Single statement, no branch. |
| `control.CoreWebView2InitializationCompleted -=` / `+=` | Same. Event registration on a live control. |
| `core.WebMessageReceived -=` / `+=` | Same. Event registration crosses into the browser process. |
| `e.IsSuccess` / `e.InitializationException` / `e.WebMessageAsJson` unwrap | Same. Non-virtual members on RCW-backed event-arg types. |
| `initializer.EnsureCoreWebView2Async(control, env)` call site (the `control` argument) | Same. The call itself is behind the existing `IWebViewCoreInitializer` mock; only the concrete `WebView2` argument is host-bound. |

Every one of those is a single statement with **zero branches and zero state**. That is the
irreducible-remainder test the epic asks for, met per operation rather than per file.

---

## 3. The exact seam design

Follows the `BreadcrumbPopupUiOperations.cs` template (production ctor + seam ctor + exempt
production forwarders) but promotes the forwarders from *method-level-exempt statics* to a
**single class-level-exempt adapter type**, for the measured reason in §3.4.

### 3.1 New interface — `QuickFiler/Viewers/IBreadcrumbControlSurface.cs` (~50 lines, `internal`)

```csharp
internal interface IBreadcrumbControlSurface
{
    CoreWebView2? ReadCore();
    void PostJson(CoreWebView2 core, string json);
    void NavigateToString(string html);
    void BindInitializationHandler(Action<bool, Exception?> onCompleted);   // idempotent
    void BindMessageHandler(Action<string> onPayload);                      // idempotent
    Task EnsureCoreAsync(IWebViewCoreInitializer initializer, CoreWebView2Environment environment);
}
```

`internal` matches the established style for every other F13 seam (`BreadcrumbUiDispatcher`,
`BreadcrumbPopupUiOperations`, `BreadcrumbDropDownOpenCoordinator` are all `internal`), and
`QuickFiler/Properties/AssemblyInfo.cs:5` grants `InternalsVisibleTo("QuickFiler.Test")`.
Ledger bucket: **`interface-only / not-measured`** (no executable IL; reported N/A, never 0%; no
`[ExcludeFromCodeCoverage]` attribute per `epic.md:509-522`).

### 3.2 New production adapter — `QuickFiler/Viewers/WebView2ControlSurface.cs` (~95 lines)

`internal sealed class WebView2ControlSurface : IBreadcrumbControlSurface`, carrying a **class-level**
`[ExcludeFromCodeCoverage]`. Holds the `WebView2` control and the two bridge `EventHandler` fields
needed for idempotent unhook/hook. Every member is one statement. Ledger bucket:
**`ratified-exempt`**, argued per-operation in the §2 residual table.

### 3.3 Modified `WebView2BreadcrumbHost.cs` (143 -> ~190 lines)

Remove the class-level attribute at `:29`. Add:

```csharp
[ExcludeFromCodeCoverage]                                   // production wiring only
public WebView2BreadcrumbHost(WebView2 control, IWebViewCoreInitializer initializer)
    : this(initializer,
           new WebView2ControlSurface(control ?? throw new ArgumentNullException(nameof(control))),
           ResolveProductionCacheFolder) { }

internal WebView2BreadcrumbHost(                            // seam ctor — NOT exempt
    IWebViewCoreInitializer initializer,
    IBreadcrumbControlSurface surface,
    Func<string> resolveCacheFolder)

internal static string ResolveProductionCacheFolder();      // NOT exempt — see below
internal void HandleInitializationCompleted(bool isSuccess, Exception? initializationException);
internal void RaiseMessageReceived(string payload);
```

- `ResolveProductionCacheFolder()` stays **non-exempt**. `Environment.GetFolderPath(LocalApplicationData)`
  and `Path.Combine` execute correctly in a test process and **create no file**, so UT4's temporary-file
  prohibition is not engaged. Assert the returned path ends in `\WindowsFormsWebView2` and is rooted.
- `HandleInitializationCompleted` replaces the body of `OnCoreInitializationCompleted`; the
  SDK-arg unwrap becomes a one-line bridge inside `WebView2ControlSurface.BindInitializationHandler`.
- `RaiseMessageReceived` replaces the body of `OnWebMessageReceived`; the unwrap moves into
  `WebView2ControlSurface.BindMessageHandler`.
- `IsCoreInitialized`, `MessageReceived`, `CoreInitialized`, `NavigateToString`, `PostMessageJson`,
  `InitializeAsync` keep their exact existing public signatures. **No public signature changes** —
  required by the frozen-signature rule (`00-cross-cutting-context.md` §10: six sibling children
  compile against these files).

### 3.4 Residual forwarders: class-level attribute, NOT method-level — with measured evidence

The brief asks whether each residual forwarder keeps a method-level `[ExcludeFromCodeCoverage]` per
the `BreadcrumbPopupUiOperations` precedent. **Recommendation: no — use a class-level attribute on a
dedicated adapter type instead.** Reason, established by comparing two measured facts already in the
repository:

- **Method-level does not suppress nested lambdas.** `BreadcrumbPopupUiOperations.cs:394` and `:457`
  carry method-level attributes, yet source lines 406, 409 and 471-490 are instrumented and
  permanently uncovered — 22 of that file's 24 uncovered lines (`00-cross-cutting-context.md` §3.7).
- **Class-level does suppress nested lambdas.** `WebView2Messenger.cs` contains four dispatcher
  lambdas (`:40-48`, `:62-68`, `:80-94`, `:104-122`) and is absent from the Cobertura report in its
  entirety, lambdas included, under its class-level attribute at `:20`.

A production forwarder that must capture the control is naturally written as a lambda. Under a
method-level attribute those lambda bodies stay in the denominator forever. Under a class-level
attribute on a dedicated adapter type they do not. This asymmetry is the deciding factor and should
be recorded in F1's ledger as a general rule for the epic.

### 3.5 Seam-hierarchy compliance

`.claude/rules/csharp.md` / epic §2: **interface > injectable delegate > adapter**. This design takes
the top tier (`IBreadcrumbControlSurface`) for the control surface and the top tier again
(`IWebViewCoreInitializer`, already present) for initialization, with a single injectable delegate
(`Func<string> resolveCacheFolder`) for the one pure computation. No new adapter-tier-only seam.

### 3.6 Reuse of existing seams — confirmed

`IBreadcrumbWebHost` (`QuickFiler/Viewers/IBreadcrumbWebHost.cs`) and `IWebViewCoreInitializer`
(`QuickFiler/Viewers/IWebViewCoreInitializer.cs`) are both reused unchanged. `IBreadcrumbWebHost`
must not be extended: `BreadcrumbBridgeRouter.cs:42` and `BreadcrumbOutboundQueue.cs:23` (both
F12/F2-owned) take it as a constructor parameter and mock it. Note that `CoreInitialized` (`:63`) and
`InitializeAsync` (`:92`) are **not** on `IBreadcrumbWebHost` — they are concrete-only members
consumed directly by `EfcFormController.cs:850` and `:862`.

---

## 4. Which SDK types are actually unmockable — evidence

Package under test: `Microsoft.Web.WebView2` **1.0.4129.50**, `targetFramework="net481"`
(`QuickFiler/packages.config:29`). All facts below are from the Microsoft Learn API reference whose
`defaultMoniker` is `webview2-dotnet-1.0.4129.50` — the exact version.

| Type | Declaration | Constructors | Mockable by Moq? | Consequence for this file |
|---|---|---|---|---|
| `CoreWebView2` | `public class` (not sealed) | none public — produced only by the SDK | **No.** No public ctor, so Castle cannot call a base ctor; all members non-virtual. | Obtain instances with `FormatterServices.GetUninitializedObject(typeof(CoreWebView2))` — proven in-repo at `BreadcrumbPopupUiOperationsDirectAdapterTests.cs:176,197` and `BreadcrumbPopupControlDispatchTests.cs:225`. Use it **only as an opaque token** passed through the seam; never call a member on it. |
| `CoreWebView2Environment` | `public class` | none public (`CreateAsync` factory) | **No.** | Same treatment; already used as an opaque token in seven existing test files (e.g. `BreadcrumbDropDownHostTests.cs:306`). |
| `CoreWebView2EnvironmentOptions` | `public class` (not sealed), inherits `Object` | Four documented overloads, all parameterised: `(String,String,String)`, `(String,String,String,Boolean)`, `(String,String,String,Boolean,List<…>)`, `(String,String,String,Boolean,List<…>,ReleaseChannels,ChannelSearchKind)`. `WebView2BreadcrumbHost.cs:103` compiles `new CoreWebView2EnvironmentOptions()` today, so the 3-arg overload carries defaults. | Not needed — it is directly constructible. | `InitializeAsync` can run end-to-end in a test process. **UNVERIFIED**: that the parameterless form does not touch the loader at runtime. The documented remarks describe it as a plain options bag. Confirm with a one-test Phase-0 spike. |
| `CoreWebView2InitializationCompletedEventArgs` | `public class : EventArgs` (not sealed) | **`CoreWebView2InitializationCompletedEventArgs(Exception)` is public.** | Not needed — directly constructible. | **Significant.** The failure branch at `:120-127` and the success path at `:129-135` can both be driven with a real args instance: `new CoreWebView2InitializationCompletedEventArgs(new InvalidOperationException("boom"))` and `new CoreWebView2InitializationCompletedEventArgs(null)`. **UNVERIFIED**: that `IsSuccess` is computed as `InitializationException == null`. Confirm in the same spike; if it is not, the `HandleInitializationCompleted(bool, Exception?)` seam in §3.3 makes the question moot, which is why the seam is recommended regardless. |
| `CoreWebView2WebMessageReceivedEventArgs` | `public class` (not sealed) | **none documented** — no public constructor | **No.** Non-virtual members; the reference lists a `Finalize()` override, which indicates a native resource, so a `GetUninitializedObject` instance would fault on `WebMessageAsJson` / `TryGetWebMessageAsString`. | Confirms the payload-unwrap must move behind the seam. Never construct this type in a test. |
| `Microsoft.Web.WebView2.WinForms.WebView2` | `public class : Control` | public parameterless ctor exists (used at `BreadcrumbPopupUiOperations.cs:381`) | Effectively **no** — members non-virtual; a Moq proxy would run the real WinForms `Control` constructor chain. | **UNVERIFIED** whether `new WebView2()` is safe in a headless test process. Do not find out — the design never constructs one. No test in the repository constructs `WebView2` (grep for `new WebView2(` returns only the production site at `BreadcrumbPopupUiOperations.cs:381`), and that site is exempt precisely for this reason. |

**Net conclusion:** none of the five WebView2 types is mockable with Moq. Two (`CoreWebView2`,
`CoreWebView2Environment`) are usable as opaque tokens via `FormatterServices`. One
(`CoreWebView2InitializationCompletedEventArgs`) is directly constructible. Two
(`CoreWebView2WebMessageReceivedEventArgs`, the WinForms `WebView2`) must never appear in a test.
That distribution is exactly what the §3 seam is shaped around.

---

## 5. The retyped-Designer-field gotcha — bears directly on this file

Full evidence is in `00-cross-cutting-context.md` §5; summarised here because
`WebView2BreadcrumbHost` takes a Designer-owned control.

- **Exact field:** `_l0vhBreadcrumb_WebView2`, declared
  `internal Microsoft.Web.WebView2.WinForms.WebView2 _l0vhBreadcrumb_WebView2;` at
  `QuickFiler/Viewers/ItemViewer.Designer.cs:6214`; instantiated at `:46`; named at `:206`.
- **Affected test:** `QuickFiler.Test/Viewers/ItemViewerBreadcrumbDropDownContractTests.cs:18-29`,
  `ExistingAnchor_RemainsTheDesignerWebViewClosedSurface`, asserts by reflection that
  `typeof(QuickFiler.ItemViewer).GetProperty("L0vhBreadcrumb_WebView2").PropertyType` is exactly
  `Microsoft.Web.WebView2.WinForms.WebView2`. **Retyping the field or its property to
  `IBreadcrumbWebHost` or any interface fails this live, green test.**
- **Working pattern (evidenced):** inject the host/router, do not retype the control —
  `ItemViewerBreadcrumbDropDownContractTests.cs:51-74`
  (`ConfigureBreadcrumbDropDown(IBreadcrumbDropDownHost, Func<Rectangle>, Func<Rectangle>)`).
- **Applicability here:** the control `WebView2BreadcrumbHost` receives is
  `_formViewer.BreadcrumbWebView` on the **EfcViewer** form (`EfcFormController.cs:837`), not
  `ItemViewer`'s field. Nevertheless the rule is the same and the §3 design honours it: the
  production constructor keeps its exact `(WebView2, IWebViewCoreInitializer)` signature, and the
  seam is introduced *beside* it, never by changing the control's declared type anywhere.

**LOUD WARNING for the planner: no task in this child may retype a Designer field or a
Designer-backed property. That approach is known-broken and pinned by a passing test.**

---

## 6. Concurrency and ordering

Complete inventory for this file. There is **no** `lock`, **no** `Interlocked`, **no** `Volatile`,
**no** `CancellationToken`, **no** `TaskCompletionSource`, and **no** `async void` in
`WebView2BreadcrumbHost.cs`.

| Construct | file:line | Notes |
|---|---|---|
| `async Task` with a context hop | `:92`, `:106` | `await uiSyncContext` resolves to the extension `SynchronizationContextAwaiter GetAwaiter(this SynchronizationContext)` at `UtilitiesCS/Threading/UiThread.cs:108`. `IsCompleted` is `_context == SynchronizationContext.Current` (`UiThread.cs:100`); otherwise `OnCompleted` calls `_context.Post` (`UiThread.cs:102-103`). A fake `SynchronizationContext` therefore drives it deterministically. |
| Event subscribe/unsubscribe (initialization) | `:49-50` | Unhook-then-hook. **Not idempotent across instances** — delegate equality is instance-based (defect L1, `00-cross-cutting-context.md` §9). |
| Event subscribe/unsubscribe (messages) | `:131-132` | Same pattern, same limitation. |
| State transition | `:134` | `IsCoreInitialized = false -> true`. Never returns to false. |
| Conditional event raise | `:135` | `CoreInitialized?.Invoke(this, EventArgs.Empty)`. |
| Fire-and-forget caller | `EfcFormController.cs:853` | `_ = InitializeBreadcrumbHostAsync();` with a `try/catch` boundary at `:860-867`. Not this file's code but it is the only production entry into `InitializeAsync`. |

### Legal and illegal transitions — each becomes one test case

| Transition | Legal? | Expected | Deterministic mechanism |
|---|---|---|---|
| Initialization failure (`IsSuccess == false`) | legal | `IsCoreInitialized` stays `false`; `CoreInitialized` NOT raised; message handler NOT bound; failure logged | Direct call to `HandleInitializationCompleted(false, ex)`; fake surface records zero `BindMessageHandler` calls |
| Initialization failure with `InitializationException == null` | legal | no `NullReferenceException` (`?.Message` at `:123`) | `HandleInitializationCompleted(false, null)` |
| Initialization success | legal | binds message handler exactly once; `IsCoreInitialized == true`; `CoreInitialized` raised once with `sender` == the host | `HandleInitializationCompleted(true, null)` |
| Pooled-viewer re-initialization (success twice) | legal | `BindMessageHandler` invoked twice (idempotency is the surface's contract); `CoreInitialized` raised twice; `IsCoreInitialized` remains `true` | two calls; fake surface counts |
| Post before initialization | legal | payload dropped, error logged, `PostJson` never reaches the surface | fake surface `ReadCore()` returns `null` |
| Post after initialization | legal | exact JSON string forwarded once | `ReadCore()` returns a `GetUninitializedObject` token |
| Dispose-during-initialization | **not representable** | the type is not `IDisposable` and holds no cancellation | Record as defect D2/L1; do not write a test that asserts a behaviour the type does not have |
| Double-subscribe across two host instances over one control | illegal-in-effect | duplicate fan-out (defect L1) | Out of scope (no-behaviour-change NFR); promote as an issue |
| Re-entrant callback (`CoreInitialized` handler calls back into `PostMessageJson`) | legal | no deadlock (no lock held); post succeeds because `IsCoreInitialized` was set at `:134` **before** the raise at `:135` | subscribe a handler that re-enters; assert ordering |
| `InitializeAsync` invoked twice | legal today, but see D1 | two environments created | assert with a `Mock<IWebViewCoreInitializer>` invocation count; documents current behaviour |

**No `Thread.Sleep`, `Task.Delay`, or wall-clock wait is required by any of the above.** The two
mechanisms are (a) direct invocation of the newly-internal `HandleInitializationCompleted` /
`RaiseMessageReceived`, and (b) a manually-pumped fake `SynchronizationContext` with an explicit
`Drain()` call — the pattern already proven at
`BreadcrumbPopupUiOperationsDirectAdapterTests.cs:274-300`.

---

## 7. STA and live-control requirements

**None of the proposed tests requires the STA last-resort clause (epic §3).** Justification:

- The design never constructs a WinForms control. `IBreadcrumbControlSurface` is faked with a plain
  class; `CoreWebView2` appears only as a `FormatterServices` token that is never dereferenced in
  test code.
- `SynchronizationContext` subclassing requires no apartment state.
- Existing precedent: `BreadcrumbDropDownHostTests.cs:24-59` constructs `ToolStripDropDown` and
  `ToolStripControlHost` in a plain `[TestClass]` with no STA attribute, and the suite is green.
- Consequently **no `*.StaTests.cs` file should be created for this child**, and no per-test STA
  justification is needed.

---

## 8. Existing tests

**`WebView2BreadcrumbHost` has zero test references anywhere in `QuickFiler.Test/`.** A grep for the
type name across the repository returns only production files plus `QuickFiler/QuickFiler.csproj`.
It is not referenced by `BreadcrumbBridgeRouterTests.cs` or `BreadcrumbBridgeRouterQueueTests.cs`,
which mock `IBreadcrumbWebHost` instead — exactly the arrangement the interface's doc comment
(`IBreadcrumbWebHost.cs:6-10`) describes.

Constraining tests that this child must not contradict (all in
`QuickFiler.Test/Viewers/ItemViewerBreadcrumbDropDownContractTests.cs`): `:18-29`
(Designer field type), `:31-49` and `:51-74` (`ConfigureBreadcrumbDropDown` overloads), `:102-130`
(`BreadcrumbDropDownOpenCoordinator` must remain `internal` and must **not** carry
`[ExcludeFromCodeCoverage]`).

---

## 9. Exemption-ground analysis — a gap in CLAUDE.md §UT2 that F1 must close

`CLAUDE.md` §UT2 enumerates exactly three grounds:

(a) VSTO add-in lifecycle classes; (b) WinForms **form-derived** classes and Designer-generated code;
(c) Outlook Interop event-handler classes depending on `Application` / `MailItem` / `Store` /
`MAPIFolder` **without an injectable seam**.

`WebView2BreadcrumbHost` matches **none** of them:

- (a) it is not a VSTO lifecycle class;
- (b) it is `sealed class WebView2BreadcrumbHost : IBreadcrumbWebHost` — it derives from nothing, is
  not a form, and is not Designer-generated;
- (c) it touches no `Microsoft.Office.Interop.Outlook` type; `using` directives at `:2-9` are
  `System*`, `Microsoft.Web.WebView2.*`, `UtilitiesCS`.

**Therefore the current class-level attribute at `:29` has no basis in the literal §UT2 text at
all**, quite apart from the "without an injectable seam" qualifier — and one of the seams it would
need (`IWebViewCoreInitializer`) already exists and is already injected into this very constructor.

The proposed `WebView2ControlSurface` adapter has the same problem: it is not covered by any of the
three literal grounds either. **F1's ledger must therefore either (i) ratify a fourth ground —
"third-party SDK adapter type in which every member is a single call requiring a live external
runtime process" — or (ii) classify such adapters `testable` and accept a documented sub-threshold
figure.** This artifact recommends (i), because it is the only reading consistent with epic §1's
"exempt only the irreducible remainder", and because ground (b)'s existing treatment of
Designer-generated code is the same species of argument.

Recording this explicitly is the point: three F13 files currently rest on an exemption ground that
does not textually exist. Do not let the plan assume §UT2 covers them.

---

## 10. Recommended test-case list

All files are **new** (`00-cross-cutting-context.md` §2.3: thirteen F13-relevant test files sit
within 25 lines of the 500-line ceiling). Framework: MSTest `[TestClass]`/`[TestMethod]`, Moq for
`IWebViewCoreInitializer`, FluentAssertions, Arrange-Act-Assert, no temp files, no live forms, no
popups. Each row is sized to be one atomic plan task.

### `QuickFiler.Test/Viewers/WebView2BreadcrumbHostContractTests.cs` (~150 lines)

| # | Test | Asserts |
|---|---|---|
| C1 | `Constructor_NullControl_ThrowsArgumentNullException` | `.WithParameterName("control")` — pins the guard at `:45` |
| C2 | `Constructor_NullInitializer_ThrowsArgumentNullException` | `.WithParameterName("initializer")` — `:46` |
| C3 | `SeamConstructor_NullInitializer_ThrowsArgumentNullException` | new seam ctor guard |
| C4 | `SeamConstructor_NullSurface_ThrowsArgumentNullException` | new seam ctor guard |
| C5 | `SeamConstructor_NullCacheFolderResolver_ThrowsArgumentNullException` | new seam ctor guard |
| C6 | `Construction_BindsInitializationHandlerExactlyOnce` | fake surface records one `BindInitializationHandler` |
| C7 | `NewInstance_ReportsCoreNotInitialized` | `IsCoreInitialized == false` |
| C8 | `Type_IsNotExcludedFromCodeCoverage` | reflection: `typeof(WebView2BreadcrumbHost)` carries no `ExcludeFromCodeCoverageAttribute`. Mirrors the existing precedent at `ItemViewerBreadcrumbDropDownContractTests.cs:102-130`; makes the ledger decision machine-checked |
| C9 | `ProductionCacheFolder_IsRootedAndEndsWithWindowsFormsWebView2` | `ResolveProductionCacheFolder()`; creates no file |

### `QuickFiler.Test/Viewers/WebView2BreadcrumbHostInitializationTests.cs` (~190 lines)

| # | Test | Asserts |
|---|---|---|
| I1 | `InitializeAsync_NullContext_ThrowsArgumentNullException` | `.WithParameterName("uiSyncContext")` — `:94-97` |
| I2 | `InitializeAsync_MarshalsToUiContextBeforeCreatingEnvironment` | fake context records a `Post` **before** the first `CreateEnvironmentAsync` invocation — pins the documented invariant at `:105-106` |
| I3 | `InitializeAsync_PassesResolvedCacheFolderAndNonNullOptions` | `Mock<IWebViewCoreInitializer>` verifies `CreateEnvironmentAsync(expectedFolder, It.IsNotNull<CoreWebView2EnvironmentOptions>())` |
| I4 | `InitializeAsync_PassesCreatedEnvironmentToEnsureCore` | environment token identity flows `:108` -> `:112` |
| I5 | `InitializeAsync_EnvironmentCreationFaults_PropagatesAndSkipsEnsureCore` | faulted task; `EnsureCoreAsync` never invoked |
| I6 | `InitializeAsync_EnsureCoreFaults_Propagates` | faulted task surfaces to the caller |
| I7 | `InitializeAsync_InvokedTwice_CreatesTwoEnvironments` | documents current re-initialization behaviour (defect D1) |

### `QuickFiler.Test/Viewers/WebView2BreadcrumbHostMessagingTests.cs` (~170 lines)

| # | Test | Asserts |
|---|---|---|
| M1 | `PostMessageJson_BeforeCoreInitialized_DropsPayload` | `ReadCore()` returns null; surface `PostJson` never invoked; no throw — pins `:74-81` |
| M2 | `PostMessageJson_AfterCoreInitialized_ForwardsExactJson` | exact string, exactly once — `:83` |
| M3 | `PostMessageJson_EmptyString_IsForwardedNotDropped` | boundary: the guard is on the core, not the payload |
| M4 | `NavigateToString_ForwardsExactHtml` | `:66-69` |
| M5 | `RaiseMessageReceived_WithSubscriber_RaisesWithExactPayloadAndSenderIdentity` | `sender` is the host — `:140` |
| M6 | `RaiseMessageReceived_WithNoSubscriber_DoesNotThrow` | null-conditional at `:140` |
| M7 | `RaiseMessageReceived_WithTwoSubscribers_InvokesBoth` | multicast fan-out |

### `QuickFiler.Test/Viewers/WebView2BreadcrumbHostLifecycleTests.cs` (~180 lines)

| # | Test | Asserts |
|---|---|---|
| L1 | `InitializationCompleted_Failure_DoesNotTransitionOrRaise` | `IsCoreInitialized` false; `CoreInitialized` not raised; `BindMessageHandler` not called — `:120-127` |
| L2 | `InitializationCompleted_FailureWithNullException_DoesNotThrow` | `?.Message` at `:123` |
| L3 | `InitializationCompleted_Success_BindsMessageHandlerOnce` | `:131-132` |
| L4 | `InitializationCompleted_Success_SetsIsCoreInitialized` | `:134` |
| L5 | `InitializationCompleted_Success_RaisesCoreInitializedOnceWithHostAsSender` | `:135` |
| L6 | `InitializationCompleted_SuccessTwice_RebindsAndRaisesTwice` | pooled-viewer re-initialization |
| L7 | `InitializationCompleted_SuccessThenFailure_LeavesIsCoreInitializedTrue` | the flag never reverts |
| L8 | `CoreInitializedHandler_PostingReentrantly_ObservesInitializedState` | ordering of `:134` before `:135` |
| L9 | `InboundMessageBoundHandler_DeliversPayloadThroughMessageReceived` | end-to-end through the fake surface's captured `Action<string>` |

### `QuickFiler.Test/Viewers/WebViewTestDoubles.cs` (~120 lines, shared, no `[TestClass]`)

`FakeBreadcrumbControlSurface` (records invocations, exposes the captured `Action<bool,Exception?>`
and `Action<string>`), and a `QueuedSynchronizationContext` with an explicit `Drain()`. Modelled on
`BreadcrumbPopupUiOperationsDirectAdapterTests.cs:274-300`. **Must be instance-based with no mutable
static state** — `scripts/vscode/TaskMaster.cli.runsettings` sets MSTest
`Parallelize Workers=0 Scope=ClassLevel`, so class-level parallelism is active
(`00-cross-cutting-context.md` §8.1). Shared with the `WebView2Messenger` tests (artifact 09).

**Projected result:** ~32 test cases. After the refactor the file has roughly 60 coverable lines with
zero permanently-uncovered residue (all SDK statements relocated to the exempt adapter type), so
**>= 95% line and >= 90% branch is achievable**, comfortably clearing the 80%/75% gates and the
>= 90% new-file bar for the seam ctor.

---

## 11. 500-line and csproj impact

### Production

| File | Before | After | 500-line | Ledger bucket |
|---|---|---|---:|---|
| `QuickFiler/Viewers/WebView2BreadcrumbHost.cs` | 143 | ~190 | OK (310 headroom) | `testable`, >= 80% (the modified file is not "newly created") |
| `QuickFiler/Viewers/IBreadcrumbControlSurface.cs` | — | ~50 (new) | OK | `interface-only / not-measured`, **no attribute** |
| `QuickFiler/Viewers/WebView2ControlSurface.cs` | — | ~95 (new) | OK | `ratified-exempt`, class-level attribute, argued per-operation (§2) |

Two new `<Compile Include="Viewers\…" />` entries inside the existing F13 block at
`QuickFiler/QuickFiler.csproj:396-411`. **Preserve CRLF** — the file is CRLF-terminated on all 593
lines; a git-bash `sed -i` strips it and guarantees a whole-file conflict (`epic.md:611-612`). Use
the `Edit` tool or `perl -0777` with explicit `\r\n`. Expect an additive textual conflict with F12
at fan-in (its entries at 393-395 and 400 are interleaved with F13's); resolution is keep-both.

Each new production file appends its own ledger row in the same change (`epic.md:578-581`).

### Test

Five new `<Compile Include="Viewers\…" />` entries in `QuickFiler.Test/QuickFiler.Test.csproj`,
appended to the existing breadcrumb block at lines 60-89 (also CRLF, also explicit-list):

```
Viewers\WebView2BreadcrumbHostContractTests.cs
Viewers\WebView2BreadcrumbHostInitializationTests.cs
Viewers\WebView2BreadcrumbHostMessagingTests.cs
Viewers\WebView2BreadcrumbHostLifecycleTests.cs
Viewers\WebViewTestDoubles.cs
```

---

## 12. Latent defects (report only — orchestrator promotes via the MCP lifecycle)

New findings from this file. Defects L1 and L6 for this file are already recorded in
`00-cross-cutting-context.md` §9 and are not restated.

| ID | Location | Impact | Confidence |
|---|---|---|---|
| **D1** | `WebView2BreadcrumbHost.cs:92-113` | `InitializeAsync` has no idempotence guard. Each invocation calls `CreateEnvironmentAsync` again (`:108`) against the same `%LocalAppData%\WindowsFormsWebView2` cache folder. The doc comment at `:89` claims "Safe to re-run for pooled viewers"; the code creates a fresh environment every time. `EfcViewerQueue` pooling therefore accumulates environments. | High (textual) |
| **D2** | `WebView2BreadcrumbHost.cs:72-84` | **Cross-thread SDK access.** `PostMessageJson` reads `_control.CoreWebView2` and calls `PostWebMessageAsJson` **on the caller's thread with no UI-thread marshalling**, whereas the parallel `WebView2Messenger` routes every SDK call through `BreadcrumbUiDispatcher` (`WebView2Messenger.cs:62-68`). `PostMessageJson` is reached from `BreadcrumbBridgeRouter` / `BreadcrumbOutboundQueue`, which are not thread-affine. A non-STA caller would touch the WebView2 RCW off its apartment. This is a genuine asymmetry between the two hosting paths, not a style difference. | Medium-High |
| **D3** | `WebView2BreadcrumbHost.cs:54` and `:134` | `IsCoreInitialized` is a plain auto-property written on the UI thread at `:134` and read by `BreadcrumbOutboundQueue` (`QuickFiler/Controllers/BreadcrumbOutboundQueue.cs:17,23`) with no memory barrier. A reader on another thread may observe a stale `false` and keep queueing indefinitely, or a stale `true` and post to a null core. Related to D2. | Medium |
| **D4** | `WebView2BreadcrumbHost.cs:29` (and `WebView2Messenger.cs:20`, `WebView2CoreInitializer.cs:15`) | The class-level exemptions cite grounds that do not exist in `CLAUDE.md` §UT2 (see §9). This is a governance defect in the exemption ledger, not a runtime defect, and is in-scope for F1 to ratify rather than to promote as a bug. | High (textual) |

---

## 13. Deviations from the delegation brief

| # | Brief claim | Finding |
|---|---|---|
| 1 | `WebView2BreadcrumbHost`'s "1:1 SDK-forwarding adapter" claim appears REFUTED | **Confirmed and strengthened.** Four contradictions listed in §1. Additionally `InitializeAsync` (`:92-113`) is **already testable today with no refactor whatsoever**, behind the `IWebViewCoreInitializer` seam that is already injected into this constructor — which the brief did not claim. |
| 2 | "Its untestable part is the concrete `Microsoft.Web.WebView2.WinForms.WebView2` control parameter" | **Partially refuted.** The control is one of three untestable surfaces; the others are `CoreWebView2WebMessageReceivedEventArgs` (no public ctor, finalizer-bearing) and the `CoreWebView2` member calls. Conversely `CoreWebView2InitializationCompletedEventArgs` **has a public `(Exception)` constructor** and is *not* untestable — a fact that materially enlarges what is reachable. |
| 3 | Residual forwarders should follow the `BreadcrumbPopupUiOperations` method-level `[ExcludeFromCodeCoverage]` precedent | **Recommend deviating.** Measured evidence (§3.4) shows a method-level attribute does **not** suppress nested lambdas while a class-level attribute **does**. Use a class-level-exempt adapter type instead. This is a general improvement F1 should adopt epic-wide. |
| 4 | Two parallel WebView2 hosting paths exist | **Confirmed with a correction to the framing** — see artifact 09 §12 and below. |
| 5 | Any seam must avoid deepening dependence on F12-owned code | **Satisfied.** The §3 design touches only `IWebViewCoreInitializer`, `IBreadcrumbWebHost` and new F13-owned types. It references neither `BreadcrumbPopupLifecycleOperations` (declared at `BreadcrumbItemViewerLifecycleCoordinator.cs:355`) nor `BreadcrumbNavigationSubscription` (`:337`), both F12-owned inside a 481-line file likely to be split. `WebView2BreadcrumbHost.cs` has **no** existing reference to either. |
| 6 | CLAUDE.md §UT2 supplies the exemption ground | **Refuted.** §UT2's three grounds do not textually cover any WebView2 adapter. See §9. |

### On finding #4 — the two paths, mapped

| Path | Entry point | Types used | Surface |
|---|---|---|---|
| **A — EfcViewer docked breadcrumb (#349)** | `QuickFiler/Controllers/EfcFormController.cs:834-854` `ConfigureBreadcrumbControl()` | `new WebView2BreadcrumbHost(_formViewer.BreadcrumbWebView, new WebView2CoreInitializer())` -> `BreadcrumbBridgeRouter` + `BreadcrumbOutboundQueue` | The EfcViewer form's own `BreadcrumbWebView` control |
| **B — ItemViewer collapsed surface + drop-down popup (#351/#400)** | `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs:82-97` and `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs:394-410` | `new WebView2Messenger(core, dispatcher)` + `BreadcrumbPopupUiOperations.NavigateToDocument` -> `BreadcrumbMessengerHub` / `BreadcrumbBridgeCoordinator` | `_l0vhBreadcrumb_WebView2` (collapsed) and a popup-hosted `WebView2` (drop-down) |

**Correction to the brief's framing:** the split is not "docked vs drop-down". It is
**EfcViewer form vs ItemViewer**. Path B covers *both* the ItemViewer collapsed surface and the
drop-down popup; `WebView2Messenger` serves both. `WebView2BreadcrumbHost` has exactly **one**
construction site in the entire repository (`EfcFormController.cs:836`).

`WebView2CoreInitializer` is the one type shared across both paths (`EfcFormController.cs:838` and
`QfcItemController.Initialization.cs:381`).

**Should they converge? No — not in this child.** The two seams have genuinely different contracts:
`IBreadcrumbWebHost` owns navigation, initialization state and posting; `IWebViewMessenger` owns
post/receive only. Converging them would change public signatures that six sibling children compile
against, breaching the frozen-signature rule and the epic's no-behaviour-change NFR. Record
convergence as a post-epic candidate; keep them separate here.
