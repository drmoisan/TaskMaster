# F13 Research — `QuickFiler/Viewers/WebView2Messenger.cs`

- Epic: #136 `quickfiler-per-file-coverage`, child F13, feature issue #455
- Production file: `QuickFiler/Viewers/WebView2Messenger.cs` (147 lines)
- Current state: class-level `[ExcludeFromCodeCoverage]` at **line 20**; entirely absent from the
  committed Cobertura instrumentation (unmeasured, not covered)
- Research date: 2026-08-07
- Companion artifacts: `00-cross-cutting-context.md` (shared evidence — not repeated here),
  `08-WebView2BreadcrumbHost.md` (SDK-type evidence in its §4 is shared and cited here),
  `10-WebView2CoreInitializer.md`

## 0. Tooling limitation (read first)

No `Bash`/shell tool was available. No `git`, `gh`, `msbuild`, `vstest` or `csharpier` was executed.
All findings derive from working-tree file content, committed Cobertura evidence, and the Microsoft
WebView2 .NET API reference for package version **1.0.4129.50** (`QuickFiler/packages.config:29`).
Items requiring a compile to confirm are labelled **UNVERIFIED — needs a Phase-0 spike**.

---

## 1. Headline verdict

**The file's own exemption justification is REFUTED, and this is the strongest refutation of the
three.** The doc comment at `:10-18` claims the type "forwards every `IWebViewMessenger` member 1:1
to the WebView2 SDK" and that "the body remains a forwarding shim over a third-party API (matching
the `WebView2CoreInitializer` exempt-forwarder pattern)".

Counting actual SDK statements in the body: there are **five** —
`_coreWebView.WebMessageReceived += OnWebMessageReceived` (`:46`),
`_coreWebView.PostWebMessageAsJson(json)` (`:66`),
`_coreWebView.WebMessageReceived -= OnWebMessageReceived` (`:86`),
`e.TryGetWebMessageAsString()` (`:114`), and `e.WebMessageAsJson` (`:119`, `:121`).
Everything else — roughly 85% of the coverable lines — is host-neutral concurrency and
lifecycle logic:

| Category | file:line | Detail |
|---|---|---|
| Disposal gate | `:75` | `Interlocked.Exchange(ref _disposeRequested, 1) != 0` -> early return (double-dispose protection) |
| Disposal read | `:127` | `Volatile.Read(ref _disposeRequested) != 0` |
| Disposal race guards | `:42`, `:64`, `:99`, `:106` | four separate `IsDisposalRequested()` early returns at distinct points in the lifecycle |
| Contract throw | `:130-136` | `ThrowIfDisposed` -> `ObjectDisposedException(nameof(WebView2Messenger))` |
| Null guards | `:38`, `:39`, `:57-60`, `:140-143` | four, with three distinct parameter names |
| Payload fallback | `:112-120` | `TryGetWebMessageAsString()` -> `catch (ArgumentException)` -> `WebMessageAsJson` |
| Null coalesce | `:121` | `payload ?? e.WebMessageAsJson` — a **second, independent** fallback |
| Subscription bookkeeping | `:47`, `:84`, `:91` | `_subscribed` set/read/cleared across three dispatched callbacks |
| Handler teardown | `:92` | `MessageReceived = null` inside a `finally` |
| Ambient-context capture | `:138-145` | `CaptureProductionDispatcher` — a 9-line static with its own null guard, invoking `BreadcrumbUiDispatcher.CaptureCurrent()` |

The orchestrator's preliminary finding #2 is **CONFIRMED in full**, with three additions the brief
did not list:

1. **`CaptureProductionDispatcher` (`:138-145`) is 100% host-neutral and 100% testable today.** It
   null-checks `coreWebView` and then calls `BreadcrumbUiDispatcher.CaptureCurrent()`, which is
   pure managed code (`BreadcrumbUiDispatcher.cs:44-56`) and throws `InvalidOperationException` when
   `SynchronizationContext.Current` is null. Both paths are reachable from a plain `[TestMethod]`.
2. **The public constructor `(CoreWebView2)` at `:33-34` is testable today**, for the same reason —
   MSTest runs with `SynchronizationContext.Current == null`, so the failure path is the default,
   and the success path is reached by `SynchronizationContext.SetSynchronizationContext(fake)` with
   a `finally` restore.
3. **There are two independent payload fallbacks, not one.** The brief named the
   `catch (ArgumentException)` fallback and the `?? e.WebMessageAsJson` coalesce separately, and
   they are separately reachable — three distinct outcomes, requiring three test cases.

**Disposition: remove the class-level exemption. Relocate the five SDK statements into one
class-level-exempt channel adapter. Target >= 95% line / >= 90% branch on this file.**

---

## 2. Member-by-member testability verdict

| # | Member | Lines | Branches / state / guards | Unmockable SDK type touched | Verdict |
|---|---|---|---|---|---|
| 1 | fields `_disposeRequested`, `_subscribed` | 25-26 | mutable state | none | n/a (state under test) |
| 2 | `public WebView2Messenger(CoreWebView2)` | 33-34 | delegates via `CaptureProductionDispatcher` | `CoreWebView2` as an opaque reference only — **no member is called on it here** | **testable today** — both the null path and the no-ambient-context path; the success path with a fake ambient context |
| 3 | `internal WebView2Messenger(CoreWebView2, BreadcrumbUiDispatcher)` | 36-49 | 2 null guards (`coreWebView`, `dispatcher`); fire-and-forget `Dispatch`; `IsDisposalRequested()` early return at `:42-45`; `_subscribed = true` at `:47` | **`:46` only** — `_coreWebView.WebMessageReceived +=` | **`:46` -> seam M1**; everything else **testable behind M1** |
| 4 | `event MessageReceived` (add/remove) | 52 | none | none | **testable today** |
| 5 | `PostJson(string)` | 55-69 | null guard (`:57-60`); `ThrowIfDisposed()` (`:61`); dispatched lambda with `!IsDisposalRequested()` branch (`:64`) | **`:66` only** — `PostWebMessageAsJson` | **testable behind M1**; the guard-ordering contract (null beats disposed) is directly assertable |
| 6 | `Dispose()` | 72-95 | `GC.SuppressFinalize`; `Interlocked.Exchange` gate (`:75`); `_subscribed` branch (`:84`); `try/finally` (`:82-94`) | **`:86` only** — `WebMessageReceived -=` | **testable behind M1** |
| 7 | `OnWebMessageReceived(object?, CoreWebView2WebMessageReceivedEventArgs)` | 97-123 | 2 `IsDisposalRequested()` early returns (`:99`, `:106`); `try`/`catch (ArgumentException)` (`:112-120`); `?? ` coalesce (`:121`); `?.Invoke` (`:121`) | **`:114`, `:119`, `:121`** — `CoreWebView2WebMessageReceivedEventArgs` has **no public constructor**, non-virtual members, and a documented `Finalize()` override indicating a native resource (see `08-WebView2BreadcrumbHost.md` §4) | split into `HandleInboundPayload(string)` (testable) + `ExtractPayload(Func<string>, Func<string>)` (pure, testable) + a 1-line SDK unwrap that moves into the exempt channel |
| 8 | `IsDisposalRequested()` | 125-128 | `Volatile.Read` | none | **testable today** |
| 9 | `ThrowIfDisposed()` | 130-136 | 1 branch, throws | none | **testable today** |
| 10 | `static CaptureProductionDispatcher(CoreWebView2)` | 138-145 | 1 null guard; `CaptureCurrent()` throw path | `CoreWebView2` as an opaque reference only | **testable today** |

### Irreducible remainder after refactor

Exactly five statements, each with zero branches and zero state, all relocated into
`CoreWebView2MessageChannel` (§3.2):

| Statement | Origin | Exemption ground |
|---|---|---|
| `core.WebMessageReceived += h` | `:46` | Third-party SDK adapter — event registration crosses into the browser process. Not covered by any literal CLAUDE.md §UT2 ground (see `08-WebView2BreadcrumbHost.md` §9). |
| `core.WebMessageReceived -= h` | `:86` | Same. |
| `core.PostWebMessageAsJson(json)` | `:66` | Same. |
| `e.TryGetWebMessageAsString()` | `:114` | Same; the args type is not constructible in a test. |
| `e.WebMessageAsJson` | `:119`/`:121` | Same. |

Note that the *decision* logic around the last two (the `ArgumentException` fallback and the null
coalesce) does **not** move into the adapter — it stays in `WebView2Messenger` as a pure static so
it remains measured. Moving it in would be exactly the "testable logic hiding behind an exemption"
failure the epic prohibits.

---

## 3. The exact seam design

Follows the `BreadcrumbPopupUiOperations.cs` template, promoted to the interface tier for the reason
in `08-WebView2BreadcrumbHost.md` §3.4 (class-level attributes suppress nested lambdas; method-level
attributes do not — measured, not assumed).

### 3.1 New interface — `QuickFiler/Viewers/IWebViewMessageChannel.cs` (~40 lines, `internal`)

```csharp
internal interface IWebViewMessageChannel
{
    void Subscribe(Action<string> onPayload);   // idempotent registration of one inbound sink
    void Unsubscribe();                         // detaches the registration made by Subscribe
    void PostJson(string json);                 // forwards one outbound JSON payload
}
```

Host-neutral by construction — no WebView2 type appears in the signature, so `WebView2Messenger`
needs no `Microsoft.Web.WebView2.Core` reference except in its two preserved public/internal
constructor signatures. Ledger bucket: **`interface-only / not-measured`** (no executable IL;
reported N/A, never 0%; **no** `[ExcludeFromCodeCoverage]`, per `epic.md:509-522`).

### 3.2 New production adapter — `QuickFiler/Viewers/CoreWebView2MessageChannel.cs` (~65 lines)

`internal sealed class CoreWebView2MessageChannel : IWebViewMessageChannel`, **class-level**
`[ExcludeFromCodeCoverage]`. Wraps one `CoreWebView2`, holds the bridging
`EventHandler<CoreWebView2WebMessageReceivedEventArgs>` field, and performs the SDK-arg unwrap by
calling back into the non-exempt pure helper:

```csharp
_bridge = (_, e) => onPayload(
    WebView2Messenger.ExtractPayload(e.TryGetWebMessageAsString, () => e.WebMessageAsJson));
```

The lambda lives inside a class-level-exempt type, so it is not instrumented (evidence: the four
dispatcher lambdas in the current `WebView2Messenger.cs` at `:40-48`, `:62-68`, `:80-94`, `:104-122`
are absent from the Cobertura report under the class-level attribute at `:20`). Ledger bucket:
**`ratified-exempt`**, argued per-statement in §2.

### 3.3 Modified `WebView2Messenger.cs` (147 -> ~165 lines)

Remove the class-level attribute at `:20`. Add one seam constructor and two testable members; keep
both existing constructors byte-compatible.

```csharp
public WebView2Messenger(CoreWebView2 coreWebView)                                   // unchanged signature
    : this(coreWebView, CaptureProductionDispatcher(coreWebView)) { }

internal WebView2Messenger(CoreWebView2 coreWebView, BreadcrumbUiDispatcher dispatcher)  // unchanged signature
    : this(dispatcher, CreateProductionChannel(coreWebView)) { }

internal WebView2Messenger(BreadcrumbUiDispatcher dispatcher, IWebViewMessageChannel channel)  // NEW seam ctor

internal static string ExtractPayload(Func<string> tryGetString, Func<string> readJson);       // pure, NOT exempt
internal void HandleInboundPayload(string payload);                                            // NOT exempt

[ExcludeFromCodeCoverage]
private static IWebViewMessageChannel CreateProductionChannel(CoreWebView2 coreWebView) =>
    new CoreWebView2MessageChannel(
        coreWebView ?? throw new ArgumentNullException(nameof(coreWebView)));
```

### 3.4 Two exception-fidelity traps the planner must not fall into

Both are behaviour-preserving requirements that a naive chaining refactor would silently break.

1. **Guard order in the internal constructor.** Today `:38` throws
   `ArgumentNullException("coreWebView")` **before** `:39` throws `ArgumentNullException("dispatcher")`.
   If the internal ctor chains as `: this(dispatcher, CreateProductionChannel(coreWebView))`, C#
   evaluates arguments left to right, so a call with **both** arguments null would throw
   `"dispatcher"` instead of `"coreWebView"`. **Order the chained arguments so `coreWebView` is
   evaluated first**, or place both guards in a single static factory. Add an explicit regression
   test (`X4` in §8).
2. **Parameter name preservation.** The `ArgumentNullException` parameter name must remain
   `"coreWebView"`, not `"core"` and not the adapter's own parameter name. Assert with
   FluentAssertions `.WithParameterName("coreWebView")`.

Both existing constructor signatures are called from production and must not change:
`QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs:409` and
`QuickFiler/Viewers/ItemViewer.Breadcrumb.cs:85` both use the internal 2-arg form. The
frozen-signature rule (`00-cross-cutting-context.md` §10) applies.

### 3.5 Seam-hierarchy compliance

Interface tier (`IWebViewMessageChannel`) for the SDK boundary; injectable delegates
(`Func<string>` pair) only inside the pure `ExtractPayload` helper; no adapter-tier-only seam. The
pre-existing partial seam the brief identified — the internal ctor taking a `BreadcrumbUiDispatcher`
at `:36` — is **retained and extended**, not replaced.

---

## 4. Which SDK types are actually unmockable

The full evidence table (declarations, constructor accessibility, virtuality, package version) is in
`08-WebView2BreadcrumbHost.md` §4 and is not duplicated. The two entries that decide this file:

- **`CoreWebView2`** — `public class`, no public constructor, no virtual members. Not mockable by
  Moq. Usable as an **opaque token** via
  `FormatterServices.GetUninitializedObject(typeof(CoreWebView2))`, proven in-repo at
  `BreadcrumbPopupUiOperationsDirectAdapterTests.cs:176,197` and
  `BreadcrumbPopupControlDispatchTests.cs:225`. This is sufficient for `WebView2Messenger`'s
  constructor null-guard and dispatcher-capture tests, which never dereference it.
  **Critically: no repository test has ever subscribed to a `CoreWebView2` event on such an
  instance.** The one production site that does (`BreadcrumbPopupUiOperations.BindProductionNavigation`,
  `:457`) is exempt, and its lambda bodies at `:471-490` are measured as permanently uncovered
  (`00-cross-cutting-context.md` §3.7). Treat `core.WebMessageReceived +=` as **not exercisable in a
  test** — which is precisely why `:46` and `:86` must go behind the channel seam rather than being
  reached with a `GetUninitializedObject` core.
- **`CoreWebView2WebMessageReceivedEventArgs`** — `public class`, **no documented constructor at
  all**, non-virtual members, and a documented `Finalize()` override implying a native resource.
  Not constructible, not mockable, and a `GetUninitializedObject` instance would fault on either
  member. This is the hard reason the payload unwrap cannot be tested in place and the
  `ExtractPayload(Func<string>, Func<string>)` shape is required.

---

## 5. Concurrency and ordering

Complete inventory. There is **no** `lock`, **no** `CancellationToken`, **no**
`TaskCompletionSource`, and **no** `async void` in this file.

| Construct | file:line | Notes |
|---|---|---|
| `Interlocked.Exchange(ref _disposeRequested, 1)` | `:75` | The single-entry gate for `Dispose`. Returns non-zero on the second call. |
| `Volatile.Read(ref _disposeRequested)` | `:127` | Every disposal check funnels through `IsDisposalRequested()`. |
| Fire-and-forget dispatch (`_ = _dispatcher.Dispatch(...)`) | `:40`, `:62`, `:80`, `:104` | Four. The returned `Task` is discarded in all four; failures are reported to the dispatcher's error sink (`BreadcrumbUiDispatcher.cs:86-89`), not propagated. |
| Disposal-race early returns | `:42`, `:64`, `:99`, `:106` | `:99` is checked on the raising thread **before** dispatch; `:106` again **inside** the dispatched callback. |
| Event subscribe / unsubscribe | `:46`, `:86` | Guarded by `_subscribed` at `:84`. |
| `_subscribed` bookkeeping | `:47`, `:84`, `:91` | Non-volatile. Safe today because every read and write happens inside a dispatcher callback on the same boundary. |
| Handler teardown | `:92` | `MessageReceived = null` inside a `finally`, dropping all subscribers. |
| `GC.SuppressFinalize(this)` | `:74` | Called **before** the double-dispose gate, so it runs on every `Dispose` call. Harmless. |
| Ambient-context capture | `:144` | `BreadcrumbUiDispatcher.CaptureCurrent()` throws if `SynchronizationContext.Current` is null (`BreadcrumbUiDispatcher.cs:46-50`). |

### Dispatcher inlining semantics — required for deterministic test design

`BreadcrumbUiDispatcher.Dispatch` runs the action **inline** when `IsCurrentBoundary()` is true
(`BreadcrumbUiDispatcher.cs:78-95`), and otherwise `Post`s (`:122-142`). `IsCurrentBoundary()`
(`:255-278`) is true when either the dispatcher is the currently-executing one, or
`ReferenceEquals(SynchronizationContext.Current, _context)`.

Consequences for tests:

- `new BreadcrumbUiDispatcher(fakeQueue, sink)` with `SynchronizationContext.Current == null`
  (the MSTest default) **posts**. Tests must call an explicit `Drain()`. This is the deterministic
  mechanism — no `Thread.Sleep`, no `Task.Delay`, no wall-clock wait.
- `BreadcrumbUiDispatcher.CreateForCurrentThreadTests()` (`:62-65`) runs **inline** on the creating
  thread. Use it for the simple assertions where no queue ordering is under test.

Both dispatcher factories are `internal` and reachable (`QuickFiler/Properties/AssemblyInfo.cs:5`).

### Legal and illegal transitions — each becomes one test case

| Transition | Legal? | Expected | Deterministic mechanism |
|---|---|---|---|
| Construct, then drain | legal | `channel.Subscribe` invoked exactly once; `_subscribed` true | queued context + `Drain()` |
| **Dispose before the constructor's subscribe callback runs** | legal | after `Drain()`: `Subscribe` **not** invoked (early return at `:42-45`) **and** `Unsubscribe` **not** invoked (`_subscribed` still false at `:84`) — no leak, no spurious detach | construct with the queued context (subscribe is queued), call `Dispose()` (its callback is queued second), then `Drain()` once |
| Double dispose | legal | exactly one `Unsubscribe`; `Interlocked` gate at `:75` short-circuits the second | two `Dispose()` calls + `Drain()` |
| `PostJson(null)` | illegal input | `ArgumentNullException("json")`, thrown **before** the disposed check | direct call |
| `PostJson(null)` **after** dispose | illegal input | still `ArgumentNullException`, **not** `ObjectDisposedException` — pins the guard order at `:57-61` | direct call |
| `PostJson` after dispose | illegal | `ObjectDisposedException("WebView2Messenger")` | direct call |
| Dispose between `ThrowIfDisposed` and the dispatched callback | legal race | callback no-ops at `:64`; `channel.PostJson` never invoked | `PostJson` (enqueues), `Dispose()`, then `Drain()` |
| Inbound payload after dispose (outer guard) | legal race | nothing dispatched at all; `MessageReceived` not raised | invoke the captured `Action<string>` after `Dispose()` |
| Inbound payload disposed between dispatch and drain (inner guard) | legal race | dispatched but no-ops at `:106` | invoke the sink, `Dispose()`, then `Drain()` |
| Re-entrant callback: a `MessageReceived` handler calls `PostJson` | legal | no deadlock (no lock held); the nested dispatch runs inline because `_executingDispatcher == this` (`BreadcrumbUiDispatcher.cs:166-178`) | subscribe a re-entrant handler; assert the post reached the channel |
| A `MessageReceived` handler throws | legal | the exception is caught by `Dispatch` (`:84-89`) and delivered to the error sink; it does not escape | error-sink recorder |
| `channel.Unsubscribe()` throws during `Dispose` | legal | `finally` still sets `_subscribed = false` and nulls `MessageReceived` (`:89-93`); the exception is reported by the dispatcher, not rethrown to the caller | throwing fake channel |
| Public ctor with no ambient `SynchronizationContext` | illegal environment | `InvalidOperationException` from `CaptureCurrent()` | MSTest default state |
| Public ctor with an ambient context | legal | constructs; the captured dispatcher posts to that context | `SetSynchronizationContext(fake)` inside `try` with a `finally` restore |

---

## 6. STA and live-control requirements

**None.** No WinForms control appears anywhere in this file or in any proposed test. The only types
constructed in tests are: a `SynchronizationContext` subclass, a fake `IWebViewMessageChannel`, and
a `FormatterServices.GetUninitializedObject(typeof(CoreWebView2))` token that is never dereferenced.
No `*.StaTests.cs` file is needed and the epic §3 last-resort clause is not invoked.

One environment caveat, not an STA one: tests that set an ambient `SynchronizationContext` must
restore it in a `finally`. `scripts/vscode/TaskMaster.cli.runsettings` sets MSTest
`Parallelize Workers=0 Scope=ClassLevel`, so classes run concurrently on different threads;
`SynchronizationContext.Current` is thread-local, so a leaked context would corrupt only the leaking
thread — still unacceptable under UT4's mutable-global-state rule. Restore it.

---

## 7. Existing tests

**`WebView2Messenger` has zero test references anywhere in `QuickFiler.Test/`.** A grep for the type
name across the repository returns only `QuickFiler/Viewers/WebView2Messenger.cs`,
`QuickFiler/Viewers/IWebViewMessenger.cs` (doc comment), `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs:409`,
`QuickFiler/Viewers/ItemViewer.Breadcrumb.cs:85`, and the two csproj files.

Tests that reference the **interface** `IWebViewMessenger` (and therefore constrain nothing about
this concrete type) include `BreadcrumbPopupUiOperationsDirectAdapterTests.cs`,
`BreadcrumbMessengerHubTests.cs`, `BreadcrumbCollapsedSurfaceReadinessTests.cs`, and
`QfcItemControllerBreadcrumbDropDownTests.cs` — all use fakes or `Mock<IWebViewMessenger>`.

---

## 8. Recommended test-case list

All files are **new**. Framework: MSTest, Moq (for `IWebViewMessageChannel` where a strict fake is
not clearer), FluentAssertions, Arrange-Act-Assert, deterministic, no temp files, no live forms, no
popups. Each row is sized to be one atomic plan task.

### `QuickFiler.Test/Viewers/WebView2MessengerConstructionTests.cs` (~200 lines)

| # | Test | Asserts |
|---|---|---|
| X1 | `PublicConstructor_NullCore_ThrowsArgumentNullExceptionNamedCoreWebView` | `.WithParameterName("coreWebView")` — pins `:140-143` |
| X2 | `PublicConstructor_WithoutAmbientSynchronizationContext_ThrowsInvalidOperationException` | message from `BreadcrumbUiDispatcher.cs:48-50`; pins `:144` |
| X3 | `PublicConstructor_WithAmbientSynchronizationContext_CapturesThatBoundary` | set/restore ambient context; after `Drain()` the fake context received the subscribe post |
| X4 | `InternalConstructor_BothArgumentsNull_ReportsCoreWebViewFirst` | **regression guard for §3.4 trap 1** — `.WithParameterName("coreWebView")` |
| X5 | `InternalConstructor_NullDispatcher_ThrowsArgumentNullExceptionNamedDispatcher` | `:39` |
| X6 | `SeamConstructor_NullChannel_ThrowsArgumentNullException` | new guard |
| X7 | `Construction_AfterDrain_SubscribesExactlyOnce` | `:40-48`; fake channel invocation count == 1 |
| X8 | `Construction_SubscribesWithASinkThatRoutesToMessageReceived` | the captured `Action<string>` drives `MessageReceived` end-to-end |
| X9 | `DisposeBeforeDrain_NeitherSubscribesNorUnsubscribes` | the `:42-45` / `:84` interaction; **the highest-value ordering test in this file** |
| X10 | `Type_IsNotExcludedFromCodeCoverage` | reflection; makes the ledger decision machine-checked (precedent: `ItemViewerBreadcrumbDropDownContractTests.cs:102-130`) |

### `QuickFiler.Test/Viewers/WebView2MessengerPostTests.cs` (~170 lines)

| # | Test | Asserts |
|---|---|---|
| P1 | `PostJson_Null_ThrowsArgumentNullExceptionNamedJson` | `:57-60` |
| P2 | `PostJson_NullAfterDispose_StillThrowsArgumentNullException` | guard order: null beats disposed (`:57-61`) |
| P3 | `PostJson_AfterDispose_ThrowsObjectDisposedException` | `.WithMessage("*WebView2Messenger*")` — `:130-136` |
| P4 | `PostJson_HappyPath_ForwardsExactJsonOnce` | `:66` via the channel |
| P5 | `PostJson_EmptyString_IsForwarded` | boundary: the guard is `== null`, not `IsNullOrEmpty` |
| P6 | `PostJson_DisposedBetweenDispatchAndDrain_DoesNotReachChannel` | `:64` disposal-race guard |
| P7 | `PostJson_ChannelThrows_ReportsToDispatcherSinkAndDoesNotEscape` | `BreadcrumbUiDispatcher.cs:86-89` |
| P8 | `PostJson_InlineDispatcher_ForwardsSynchronously` | uses `CreateForCurrentThreadTests()`; covers the inline branch at `BreadcrumbUiDispatcher.cs:78-95` |

### `QuickFiler.Test/Viewers/WebView2MessengerInboundTests.cs` (~200 lines)

| # | Test | Asserts |
|---|---|---|
| N1 | `ExtractPayload_StringAvailable_ReturnsIt` | `:114` path |
| N2 | `ExtractPayload_ArgumentException_FallsBackToJson` | `:116-120`; the documented "page posts JSON objects" case |
| N3 | `ExtractPayload_NullString_CoalescesToJson` | `:121` — the **second, independent** fallback |
| N4 | `ExtractPayload_NonArgumentException_Propagates` | negative: only `ArgumentException` is caught |
| N5 | `InboundPayload_RaisesMessageReceivedWithSenderIdentity` | `sender` is the messenger — `:121` |
| N6 | `InboundPayload_NoSubscriber_DoesNotThrow` | `?.Invoke` at `:121` |
| N7 | `InboundPayload_AfterDispose_IsNotDispatchedAtAll` | outer guard `:99-102` |
| N8 | `InboundPayload_DisposedBetweenDispatchAndDrain_IsNotRaised` | inner guard `:106-109` |
| N9 | `InboundPayload_HandlerThrows_ReportsToDispatcherSink` | `BreadcrumbUiDispatcher.cs:86-89` |
| N10 | `InboundPayload_ReentrantPostFromHandler_ReachesChannel` | inline nested dispatch, `BreadcrumbUiDispatcher.cs:166-178` |
| N11 | `InboundPayload_TwoSubscribers_BothInvokedInOrder` | multicast fan-out |

### `QuickFiler.Test/Viewers/WebView2MessengerDisposalTests.cs` (~170 lines)

| # | Test | Asserts |
|---|---|---|
| D1 | `Dispose_AfterSubscribe_UnsubscribesExactlyOnce` | `:84-87` |
| D2 | `Dispose_CalledTwice_UnsubscribesOnce` | `Interlocked` gate at `:75` |
| D3 | `Dispose_ClearsMessageReceivedSubscribers` | `:92`; a handler added before dispose is not invoked afterwards |
| D4 | `Dispose_WhenChannelUnsubscribeThrows_StillClearsStateAndDoesNotEscape` | `finally` at `:89-93` |
| D5 | `Dispose_WhenNeverSubscribed_DoesNotCallUnsubscribe` | `_subscribed` false branch at `:84` |
| D6 | `IsDisposalRequested_TransitionsFalseToTrue` | `:125-128` via `ThrowIfDisposed` observable behaviour |
| D7 | `Dispose_ThenPostJson_ThrowsObjectDisposedException` | end-to-end state-machine transition |

### Shared doubles

Extend `QuickFiler.Test/Viewers/WebViewTestDoubles.cs` (introduced in artifact 08 §10) with
`FakeWebViewMessageChannel` (records `Subscribe`/`Unsubscribe`/`PostJson`, exposes the captured
`Action<string>`, optionally throws on demand) and an error-sink recorder. Instance-based, no mutable
static state.

**Projected result:** ~36 test cases. After the refactor the file has roughly 70 coverable lines with
**zero** permanently-uncovered residue, so **>= 95% line and >= 90% branch is achievable**.
`WebView2Messenger.cs` is the single largest coverage gain available in this child.

---

## 9. 500-line and csproj impact

### Production

| File | Before | After | 500-line | Ledger bucket |
|---|---|---|---:|---|
| `QuickFiler/Viewers/WebView2Messenger.cs` | 147 | ~165 | OK (335 headroom) | `testable`, >= 80% (target >= 95%) |
| `QuickFiler/Viewers/IWebViewMessageChannel.cs` | — | ~40 (new) | OK | `interface-only / not-measured`, **no attribute** |
| `QuickFiler/Viewers/CoreWebView2MessageChannel.cs` | — | ~65 (new) | OK | `ratified-exempt`, class-level attribute, argued per-statement (§2) |

Two new `<Compile Include="Viewers\…" />` entries in the F13 block at
`QuickFiler/QuickFiler.csproj:396-411`. **Preserve CRLF** (all 593 lines are CRLF-terminated;
`epic.md:611-612`). Use the `Edit` tool or `perl -0777` with explicit `\r\n`. Additive conflict with
F12 expected at fan-in; resolution is keep-both. Each new file appends its own ledger row in the
same change (`epic.md:578-581`).

### Test

Four new `<Compile Include="Viewers\…" />` entries in `QuickFiler.Test/QuickFiler.Test.csproj`
(breadcrumb block at lines 60-89; also CRLF, also an explicit list):

```
Viewers\WebView2MessengerConstructionTests.cs
Viewers\WebView2MessengerPostTests.cs
Viewers\WebView2MessengerInboundTests.cs
Viewers\WebView2MessengerDisposalTests.cs
```

(`Viewers\WebViewTestDoubles.cs` is added once, by artifact 08's task set.)

---

## 10. Latent defects (report only — orchestrator promotes via the MCP lifecycle)

Defect L5 for this file (fire-and-forget subscription that fails silently) is already recorded in
`00-cross-cutting-context.md` §9 and is not restated. New findings:

| ID | Location | Impact | Confidence |
|---|---|---|---|
| **E1** | `WebView2Messenger.cs:74-78` | `GC.SuppressFinalize(this)` is called **before** the `Interlocked.Exchange` double-dispose gate, so it executes on every `Dispose` call rather than once. The type has no finalizer, so the runtime impact is nil, but it inverts the conventional dispose-pattern ordering and will be flagged by Roslynator/Sonar. Cosmetic. | High (textual) |
| **E2** | `WebView2Messenger.cs:118-121` | Two independent fallbacks stack: `catch (ArgumentException) { payload = e.WebMessageAsJson; }` then `payload ?? e.WebMessageAsJson`. If `TryGetWebMessageAsString()` returns null, `WebMessageAsJson` is read once; if it throws, `WebMessageAsJson` is read and then, if that itself returned null, read **again**. Each read is a COM call. Redundant, and the double-read is not obvious from the source. Low runtime cost, but it obscures the contract. | Medium |
| **E3** | `WebView2Messenger.cs:99-104` | `OnWebMessageReceived` is invoked by the SDK **already on the UI thread**, then re-dispatches through `_dispatcher` (`:104`). In production this executes inline (`BreadcrumbUiDispatcher.cs:78-95` — `IsCurrentBoundary()` is true), so behaviour is correct; but if the captured context ever differs from the raising context the inbound message becomes asynchronous relative to the SDK callback, silently reordering payloads relative to `PostJson`. Latent ordering hazard, not an active defect. | Low-Medium |
| **E4** | `WebView2Messenger.cs:92` | `Dispose` sets `MessageReceived = null`, silently detaching subscribers the messenger does not own. `BreadcrumbMessengerHub` (F12) attaches to this event; after disposal its handler is dropped with no notification. Contractually reasonable but undocumented on `IWebViewMessenger.MessageReceived` (`IWebViewMessenger.cs:15-19`). Documentation gap. | Medium |

---

## 11. Deviations from the delegation brief

| # | Brief claim | Finding |
|---|---|---|
| 1 | The "forwarding shim" claim at `:16-18` appears REFUTED by its own body | **Confirmed in full.** Only five of roughly 70 coverable lines are SDK statements. §1 enumerates ten categories of host-neutral logic. |
| 2 | Brief's list of host-neutral logic: `:75`, `:127`, `:130-136`, `:38-39`, `:57-60`, `:112-120`, `:121`, `:47/:84/:91` | **All confirmed at those exact lines.** Three additions the brief omitted: `CaptureProductionDispatcher` (`:138-145`, a 9-line testable static); the public ctor (`:33-34`, testable today via ambient-context manipulation); and that `:116-120` and `:121` are **two independent** fallbacks requiring separate cases. |
| 3 | "Its untestable part is the concrete `CoreWebView2` constructor parameter" | **Refined.** The *parameter* is not the problem — a `FormatterServices` token satisfies it and is never dereferenced by the constructor's own code. The untestable parts are the **five member calls** (`:46`, `:66`, `:86`, `:114`, `:119`/`:121`), plus the fact that `CoreWebView2WebMessageReceivedEventArgs` cannot be constructed at all. |
| 4 | "It ALREADY has an internal constructor taking a `BreadcrumbUiDispatcher` (`:36`) — a partial seam" | **Confirmed.** That seam is retained and extended, not replaced; both existing constructor signatures stay byte-compatible because `BreadcrumbPopupUiOperations.cs:409` and `ItemViewer.Breadcrumb.cs:85` call the 2-arg form. |
| 5 | Residual forwarders should follow the method-level `[ExcludeFromCodeCoverage]` precedent | **Recommend deviating** — class-level attribute on a dedicated adapter type. Rationale and measured evidence in `08-WebView2BreadcrumbHost.md` §3.4; this file supplies half that evidence (its own four lambdas are suppressed by the class-level attribute at `:20`). |
| 6 | Any seam must avoid deepening dependence on F12-owned code | **Satisfied.** `WebView2Messenger.cs` currently has **no** reference to `BreadcrumbPopupLifecycleOperations` (`BreadcrumbItemViewerLifecycleCoordinator.cs:355`) or `BreadcrumbNavigationSubscription` (`:337`), and the proposed design adds none. Its only cross-file dependency is `BreadcrumbUiDispatcher` (`QuickFiler/Viewers/BreadcrumbUiDispatcher.cs`), which is **F13-owned**. |
| 7 | Implicit premise that CLAUDE.md §UT2 supplies the exemption ground | **Refuted.** §UT2's three grounds (VSTO lifecycle; WinForms form-derived/Designer; Outlook Interop without a seam) do not cover a WebView2 messaging adapter. `WebView2Messenger` derives from nothing, is not a form, and imports no `Microsoft.Office.Interop.Outlook` type (`using` directives at `:2-5`). F1 must ratify a fourth ground or classify these adapters `testable`. See `08-WebView2BreadcrumbHost.md` §9. |
