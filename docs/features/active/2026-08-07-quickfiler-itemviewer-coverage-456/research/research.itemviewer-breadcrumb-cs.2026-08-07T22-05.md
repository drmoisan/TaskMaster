# Research — `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs`

- Feature: F14 `quickfiler-itemviewer-coverage` (issue #456), child of epic #136 `quickfiler-per-file-coverage`
- Timestamp: 2026-08-07T22-05
- Worktree: `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a5e4b635834feedd7`
- Target file: `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` (298 code lines; 299 physical incl. trailing newline)
- Compile entry: `QuickFiler/QuickFiler.csproj:423-426` (`<DependentUpon>ItemViewer.cs</DependentUpon>`, `<SubType>UserControl</SubType>`)

Every claim below is marked **[V]** verified by direct file read / artifact inspection, or **[I]** inferred
from verified facts. No claim rests on assumption alone.

---

## 0. Premises supplied by the orchestrator — confirmations and disproofs

| # | Supplied premise | Verdict | Evidence |
|---|---|---|---|
| P1 | `ItemViewer` is `public partial class ItemViewer : UserControl, IItemViewer, IContainerControlLocal` | **CONFIRMED [V]** | `QuickFiler/Viewers/ItemViewer.cs:21` |
| P2 | Only `ItemViewer.cs:20` carries a real `[ExcludeFromCodeCoverage]`; this file carries none | **CONFIRMED [V]** | `ItemViewer.cs:20`; full read of `ItemViewer.Breadcrumb.cs` shows zero occurrences of the attribute (the two method-level exclusions that once existed here were removed — see §8, issue #400 P9-T12) |
| P3 | No `ItemViewer.*` partial appears in the committed Cobertura report | **CONFIRMED [V]** | `docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml` — grep for `filename="[^"]*ItemViewer[^"]*"` returns only `Helper Classes\ItemViewerQueue.cs`, `Viewers\ItemViewerExpanded.Designer.cs`, `Viewers\ItemViewerExpanded.cs`. **No `Viewers\ItemViewer*.cs` entry of any partial.** Sibling breadcrumb files in the same folder ARE present (`Viewers\BreadcrumbItemViewerLifecycleCoordinator.cs` at line 7850, `Viewers\BreadcrumbUiDispatcher.cs` at 8874, etc.), which is a positive control proving the folder was instrumented. |
| P4 | "Assume 0% measured coverage and plan from zero." | **HALF DISPROVED [V]** | *Measured* coverage is 0/absent — correct. *Executed* coverage is **substantial**: at least 8 existing test files construct a live headless `ItemViewer` and drive this file's members. See §5.1. Planning "from zero" would produce a large volume of duplicate test cases. The correct plan is **remove the type attribute first, measure, then close the residual gap**. |
| P5 | `QuickFiler.Test` targets net481 with MSTest 4.3.3 supplying `[STATestClass]`/`[STATestMethod]`; no new package needed | **CONFIRMED [V]** | `QuickFiler.Test/QuickFiler.Test.csproj:18` (`<TargetFrameworkVersion>v4.8.1</TargetFrameworkVersion>`); `QuickFiler.Test/packages.config:113-119` (`MSTest.Analyzers`, `MSTest.TestAdapter`, `MSTest.TestFramework` all `4.3.3`) |
| P6 | Because net481, `TimeProvider`/`FakeTimeProvider` is **NOT** available; do not recommend `TimeProvider` | **DISPROVED [V]** | `QuickFiler.Test/packages.config:18` `Microsoft.Bcl.TimeProvider 10.0.10`; `:85-88` `Microsoft.Extensions.TimeProvider.Testing 10.8.0`. Assembly references wired at `QuickFiler.Test/QuickFiler.Test.csproj:205-206` and `:255-256`. Production side wired at `QuickFiler/QuickFiler.csproj:68-69`. `FakeTimeProvider` is in **active use** in this very test project — `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs:318-319`, `QuickFiler.Test/Controllers/QfcDatamodelTests.cs:106,254,288`, `QuickFiler.Test/Controllers/QfcStreamingDequeueConfidenceGateTests.Part2.cs:20`. `QfcHomeControllerMetricsTests.cs:316` states in-code: *"Moq cannot mock the non-virtual GetLocalNow(); FakeTimeProvider is the prescribed seam."* **`TimeProvider`/`FakeTimeProvider` IS the repo-standard clock seam and IS available.** See §4. |
| P7 | Issue #441 (double-counted `<line>` nodes) | **CONFIRMED [V]** | `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1:121-122` uses `$pkg.SelectNodes('.//class')` then `$cls.SelectNodes('.//lines/line')` — the descendant axis picks up both `<class><lines><line>` and `<class><methods><method><lines><line>`, double-counting every line into `$totalLines` (`:123`) and hence into `LinesValid`/`LineRate` (`:137-143`). Note the *per-file merge* path at `:181,219` correctly uses the child axes `./class[@filename]` and `./lines/line`, so the defect is confined to the summary function. |
| P8 | `UtilitiesCS` grants no `InternalsVisibleTo` to `QuickFiler.Test` | **NOT RE-VERIFIED HERE** — this file requires no `UtilitiesCS` internal. Its only `UtilitiesCS` dependencies are the **public** `IFolderHierarchyProvider` and the **public** `BreadcrumbArrowDirection` enum (`UtilitiesCS/OutlookObjects/Folder/BreadcrumbBridgeMessages.cs:11`). No local seam is required on that account. |
| P9 | Bash/`gh` available for open-issue search | **DISPROVED** — the Bash tool is disabled for this session (`Error: No such tool available: Bash`). Open-issue analysis in §9 is therefore based on repo artifacts plus the facts the orchestrator supplied, and is explicitly labelled where it could not be verified against GitHub. |

---

## 1. Current state

### 1.1 What the file is

A partial of `ItemViewer` (a `UserControl`) that owns **the WinForms-facing wrappers for the breadcrumb
selector**. It holds two fields (`_breadcrumbLifecycleCoordinator`, `_breadcrumbResourceOwner`,
`:15-16`), exposes a settable property over the Designer WebView2 field (`:19-23`), and forwards
16 members to `BreadcrumbItemViewerLifecycleCoordinator` (F12-owned).

The file's XML summary (`:12`) is accurate: *"Owns the WinForms wrappers for the breadcrumb selector
lifecycle coordinator."* All host-neutral state machinery already lives in the F12/F13 siblings; this
file is the thin ItemViewer-side adapter that issue #400's P9-T12 remediation deliberately left behind.

### 1.2 Provenance — why the file has the shape it has

This file was **already the subject of a targeted de-exemption remediation**, which is directly relevant
to how F14 should approach it. [V] `docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/remediation-plan.2026-07-21T21-37.md:622` (task P9-T7) records:

> `ItemViewer`'s type exclusion is inherited from `QuickFiler/Viewers/ItemViewer.cs`, while
> `ItemViewer.Breadcrumb.cs` grew 141 to 399 lines and adds method exclusions
> `AttachBreadcrumbWebViewAsync` 71-73 and `CreateCollapsedBreadcrumbCandidate` 84-116. … The mandatory
> correction decision is: **retain the ItemViewer type exclusion only for its wider legacy UI ownership;
> remove the two branch-added ItemViewer method exclusions; extract every breadcrumb host-neutral
> selector/configuration/lifecycle branch from that excluded type and the host-neutral body of
> `NavigateToDocument` into a new unexcluded coordinator**…

And `:626` (P9-T12) authorised exactly `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs`
as that new unexcluded coordinator, "at most 500 physical lines and without `[ExcludeFromCodeCoverage]`".

**Consequence for F14 [I]:** the host-neutral extraction this file needs has already been done once,
under maintainer-visible review. The residue in `ItemViewer.Breadcrumb.cs` is the deliberately-retained
thin wrapper layer. F14 should **not** re-extract; it should remove the *type-level* attribute at
`ItemViewer.cs:20` (the only thing still hiding this file) and close the measured gap.

`:725` further records that the second `InitializeBreadcrumbPipeline` overload taking a
`BreadcrumbPopupUiOperations` (this file, `:40-43`) was added under P9-T28 as a *"narrow internal
testability overload … retain the one-parameter production wrapper and all runtime defaults"*. That is
the sanctioned precedent for the seam style F14 should extend.

### 1.3 The exemption coupling — an F14-internal ordering constraint

`[ExcludeFromCodeCoverage]` at `ItemViewer.cs:20` is applied to the **type**, and a partial type has one
identity. Removing it un-hides **all six** `ItemViewer` partials simultaneously
(`ItemViewer.cs`, `.DisplayState.cs`, `.Commands.cs`, `.Breadcrumb.cs`, `.FolderSearch.cs`,
`.WebViewThread.cs`) plus `ItemViewer.Designer.cs` (6,224 lines). [V] all six + Designer are listed at
`QuickFiler/QuickFiler.csproj:412-435` as partials of the same type.

**All seven files are F14-owned** (epic.md `### F14`), so this creates no cross-child conflict — but it
does create a hard **intra-child ordering dependency**: *no* per-file number for `ItemViewer.Breadcrumb.cs`
can be produced until the `ItemViewer.cs:20` attribute is removed, and the moment it is removed
`ItemViewer.Designer.cs` (6,224 lines of generated code) enters the denominator at near-0%. The plan
must therefore, in one atomic change:

1. remove `[ExcludeFromCodeCoverage]` from `ItemViewer.cs:20`, **and**
2. add `[ExcludeFromCodeCoverage]` to the Designer partial `ItemViewer.Designer.cs` — which is
   legitimate under the epic's ratified ground (b) "WinForms Designer-generated code" (epic.md § Shared
   Design 1) and is already an epic-declared exempt-candidate (`epic.md:431`).

Adding a *file-level* attribute to a partial declaration is not possible in C#; the attribute must go on
the `partial class ItemViewer` declaration inside `ItemViewer.Designer.cs:5`. **Applying it there
re-applies it to the whole type and re-hides everything.** [V] this is a real constraint, not a
hypothetical: it is exactly why the type attribute currently hides this file.

**Recommended resolution [I], to be recorded in `spec.md` as an F14 design decision:** do **not**
attempt per-partial exemption. Remove the type attribute entirely and accept `ItemViewer.Designer.cs`
into the denominator as an `interface-only`-adjacent ledger row of kind `ratified-exempt` handled by
**`coverage.config` / harness-level filename exclusion**, not by attribute. Two supporting facts:
- The epic already anticipates ledger rows for Designer files as "exempt-candidate" (`epic.md:431`,
  `:389`, `:440-443`), i.e. resolved *"by ledger classification rather than by refactor"* (`epic.md:473`).
- F1's harness keys on `filename` (`Invoke-MSTestWithCoverage.Helpers.ps1:181` `./class[@filename]`), so a
  filename-based exemption is mechanically available without touching any attribute.

**This is the single highest-risk decision in F14 and must be settled in `spec.md` before planning.**

---

## 2. Q1 — The thread boundary, mapped

### 2.1 The marshalling mechanism

There is **no** `Control.Invoke`/`BeginInvoke`, no `Task.Run`, no dedicated STA thread, and no custom
dispatcher **inside this file**. The entire boundary is delegated to one type:

`BreadcrumbUiDispatcher` (`QuickFiler/Viewers/BreadcrumbUiDispatcher.cs`, F13-owned), which wraps a
captured `SynchronizationContext` and posts with `SynchronizationContext.Post` (`:122`, `:206`).

Two capture entry points are reachable from this file:

| Entry point | Line in this file | Behaviour |
|---|---|---|
| `BreadcrumbPopupUiOperations.CaptureCurrent()` | `:38`, `:83` | `BreadcrumbUiDispatcher.CaptureCurrent()` (`BreadcrumbUiDispatcher.cs:44-56`) — **throws `InvalidOperationException`** if `SynchronizationContext.Current` is null (`:46-50`). Also records `Environment.CurrentManagedThreadId` as `_ownerThreadId`. |
| `BreadcrumbPopupUiOperations.CaptureCurrentOrTests()` | `:156`, `:192` | `BreadcrumbPopupUiOperations.cs:86-89` — when `SynchronizationContext.Current == null`, returns `CreateForCurrentThreadTests()`, an **owner-thread-only dispatcher with `_context == null`** (`BreadcrumbUiDispatcher.cs:62-65`). |

`BreadcrumbUiDispatcher.Dispatch` (`:71-151`) decides inline-vs-post through `IsCurrentBoundary()`
(`:255-278`):

1. If a dispatcher callback of *this same dispatcher instance* is currently executing on this thread
   (`[ThreadStatic] _executingDispatcher`, `:14-15`, `:258-260`) → **run inline**.
2. Else, if a context was captured, boundary proof is **reference equality of `SynchronizationContext.Current`
   with the captured context** (`:271`). The in-code comment at `:263-268` documents *why* bare thread
   identity is refused: *"a continuation resumed after `ConfigureAwait(false)` can be scheduled onto a
   recycled thread-pool thread whose managed thread ID equals the captured owner thread ID, which would
   run UI work inline and complete the returned task without any post ever crossing the captured context."*
3. Else, for the test-only dispatcher (`_context == null`), thread identity is the only available proof
   (`:276-277`).
4. Off-boundary with `_context == null` → the work is **not executed at all**; an
   `InvalidOperationException` is fed to the error sink and `Task.CompletedTask` is returned (`:97-105`).

### 2.2 Per-member thread map

Legend: **UI** = must run on the ItemViewer's creating thread; **Any** = thread-agnostic;
**Marshalled** = crosses the boundary via `BreadcrumbUiDispatcher`.

| Member | Lines | Thread | Boundary mechanism |
|---|---|---|---|
| `L0vhBreadcrumb_WebView2` get/set | 19-23 | **Any** (field access only; the *returned control* is UI-affine) | none |
| `BreadcrumbCoordinator` get/private set | 25 | Any | none |
| `BreadcrumbDropDownHost` get | 26-27 | Any | none (reads `_openCoordinator?.Host`) |
| `BreadcrumbOpenTask` get | 29-30 | Any | none |
| `BreadcrumbUnhandledArrow` event | 32 | Any (raised on whatever thread the coordinator raises on) | none |
| `InitializeBreadcrumbPipeline(provider)` | 37-38 | **UI (hard)** | `CaptureCurrent()` throws off-context |
| `InitializeBreadcrumbPipeline(provider, ops)` | 40-60 | UI by convention | `BreadcrumbUiDispatcher.CaptureCurrent()` at `:56` — **also throws off-context** |
| `AttachBreadcrumbWebViewAsync()` | 62-63 | **UI (hard)** | delegates to `CreateCollapsedBreadcrumbCandidate` |
| `AttachBreadcrumbWebViewAsync(factory)` | 65-75 | Any (factory decides) | none directly |
| `CreateCollapsedBreadcrumbCandidate()` | 77-98 | **UI (hard)** | reads `_l0vhBreadcrumb_WebView2.CoreWebView2` (`:82`) and calls `CaptureCurrent()` (`:83`) |
| `AttachBreadcrumbMessengerWhenReadyAsync` | 100-124 | Any | none |
| `AttachBreadcrumbMessenger` | 126-140 | Any | none |
| `ConfigureBreadcrumbDropDown(env, initializer)` | 142-177 | **UI (hard)** | `new BreadcrumbDropDownHost(_l0vhBreadcrumb_WebView2, …)` (`:159-160`); lambdas at `:172-174` (`RectangleToScreen`) and `:175` (`Screen.FromControl`) are UI-affine **when later invoked** |
| `ConfigureBreadcrumbDropDown(host, anchor, working)` | 179-195 | Any at call; **Marshalled** downstream | `lifecycle.ConfigureHost` → `_operations.PostAsync` (`BreadcrumbItemViewerLifecycleCoordinator.cs:120`) |
| `SetBreadcrumbTheme` | 197-198 | Any | none (synchronous fan-out in coordinator `:155-160`) |
| `FocusBreadcrumb` | 200-209 | Any at call; **Marshalled** | `lifecycle.Focus(FocusBreadcrumbCore)` → `_operations.PostAsync` (`coordinator:167`) |
| `FocusBreadcrumbCore` | 211-221 | **UI (hard)** | `_l0vhBreadcrumb_WebView2.Focus()` (`:219`) |
| `SetBreadcrumbDropDownState` | 223-235 | Any at call; **Marshalled** when it routes to `Focus` | `coordinator:176-189` |
| `ResetBreadcrumb` | 237 | Any | none |
| `OnBreadcrumbSelectionChanged` | 239-240 | **inherits caller's thread** | none |
| `OnBreadcrumbFolderArrowKeyDown` | 242-248 | **inherits caller's thread** | none |
| `OnBreadcrumbUnhandledArrow` | 250-251 | **inherits caller's thread** | none |
| `EnsureBreadcrumbLifecycle` | 253-277 | Any | none |
| `EnsureBreadcrumbResourceOwnership` | 279-289 | Any (touches `components`, a Designer field) | none |
| `DisposeBreadcrumbResources` | 291-296 | **UI in production** (runs from `Control.Dispose`) | none |

### 2.3 Ordering invariants that must hold

| # | Invariant | Enforced at | Failure mode if broken |
|---|---|---|---|
| O1 | `InitializeBreadcrumbPipeline` is idempotent — a second call with a different provider is a **silent no-op** | `:45-48` (`if (BreadcrumbCoordinator != null) return;`) | Second provider is silently discarded. See LD-3. |
| O2 | The lifecycle coordinator must exist before any messenger attach | `:113-118`, `:132-137` throw `InvalidOperationException` | — |
| O3 | `ConfigureBreadcrumbDropDown(env, init)` is idempotent **only** while the environment reference is unchanged | `:147-153` (`is BreadcrumbDropDownHost existing && ReferenceEquals(existing.Environment, environment)`) | A *different* environment silently constructs a second `BreadcrumbDropDownHost` and leaks the first — see LD-1 |
| O4 | `ConfigureHost` work is generation-guarded against a concurrent `Reset`/`Dispose` | `BreadcrumbItemViewerLifecycleCoordinator.cs:119-125` (`int generation = _generation;` captured *before* the post, checked inside) | Stale host installed after reset |
| O5 | `DisposeBreadcrumbResources` must null both `_breadcrumbLifecycleCoordinator` and `BreadcrumbCoordinator` so a pooled viewer re-initialises | `:291-296` | Pooled reuse re-attaches a disposed coordinator |
| O6 | `_breadcrumbResourceOwner` must be registered in `components` so `Control.Dispose` reaches it | `:286-288`; consumed by `ItemViewer.Designer.cs:16-23` (`if (disposing && (components != null)) components.Dispose();`) | Coordinator/hub/messengers leak on viewer disposal |
| O7 | `FocusBreadcrumbCore` must not touch a disposed control | `:213-217` (`!IsDisposed && … && !IsDisposed`) | `ObjectDisposedException` on the UI thread |

### 2.4 Race / lost-update / out-of-order surface

| ID | Location | Nature | Severity |
|---|---|---|---|
| R1 | `:159-168` — the `host` local is captured by the `() => host.ControlHost?.Control.Focus()` closure at `:164` **before** `host` is assigned at `:159`. The `BreadcrumbDropDownHost host = null;` at `:158` exists solely to make this self-reference compile. | Deliberate self-referential closure. Safe **only** because the closure is never invoked during construction. [V] `BreadcrumbDropDownHost.cs:37-160` — no constructor overload invokes the `returnFocus` delegate. | Low, but fragile: an F13 change that invokes the delegate from the constructor would NRE. Worth an F13 cross-child note (§6, X-3). |
| R2 | `:45-48` vs `:59` — `BreadcrumbCoordinator` is read then written **without synchronisation**. Two threads calling `InitializeBreadcrumbPipeline` concurrently both observe null and both construct a coordinator; one is silently lost, along with its hub subscription. | Lost update | Real but low likelihood — production calls it only from `QfcItemController.EnsureBreadcrumbPipeline` (`QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:133-146`) on the UI thread. Documented as LD-4. |
| R3 | `:147-153` vs `:155-176` — same read-then-write shape on the drop-down host. | Lost update + resource leak | LD-1. |
| R4 | `:281-288` — `_breadcrumbResourceOwner` read-then-write, plus `components ??= new Container()` at `:286` racing with `ItemViewer.Designer.cs:18` reading `components` during `Dispose`. | Torn dispose: a `BreadcrumbResourceOwner` added to a `Container` after `Dispose(true)` has already read `components` never runs its cleanup. | Low likelihood; documented as LD-5. |
| R5 | `:29-30` `BreadcrumbOpenTask` reads `_breadcrumbLifecycleCoordinator` then `.CurrentOpenTask` non-atomically; a concurrent `DisposeBreadcrumbResources` (`:294`) nulls the field between the two dereferences of the null-conditional — **not** actually possible, because `?.` evaluates the receiver once into a temporary. **[V] no defect here.** | none | n/a |
| R6 | Out-of-order delivery: `ConfigureHost` posts asynchronously (`coordinator:120`) while `SetTheme` (`:197-198` → `coordinator:155-160`) runs **synchronously**. A `SetBreadcrumbTheme` issued immediately after `ConfigureBreadcrumbDropDown` can therefore reach `DropDownHost?.SetTheme` while `DropDownHost` is still null. | Lost theme on the popup surface | **Mitigated in production**: `QfcItemController.ViewerSetup.cs:166-167` calls `ConfigureBreadcrumbDropDown` then `SetBreadcrumbTheme` on the UI thread, where `Dispatch` runs the post inline (`BreadcrumbUiDispatcher.cs:78-95`), so ordering holds. **Off the UI thread it does not.** Documented as LD-2. |

---

## 3. Q2 — Testability seams

### 3.1 Member classification

| Member | Lines | Class | Notes |
|---|---|---|---|
| `L0vhBreadcrumb_WebView2` get/set | 19-23 | thin wiring | pure field passthrough |
| `BreadcrumbCoordinator` | 25 | thin wiring | |
| `BreadcrumbDropDownHost` | 26-27 | thin wiring | |
| `BreadcrumbOpenTask` | 29-30 | pure/host-neutral | `??` fallback is real branch logic |
| `BreadcrumbUnhandledArrow` | 32 | thin wiring | |
| `InitializeBreadcrumbPipeline(provider)` | 37-38 | **COM/WebView-bound** | `CaptureCurrent()` throws without an ambient context |
| `InitializeBreadcrumbPipeline(provider, ops)` | 40-60 | **pure/host-neutral** | the existing #400 P9-T28 seam; already injectable |
| `AttachBreadcrumbWebViewAsync()` | 62-63 | **COM/WebView-bound** | |
| `AttachBreadcrumbWebViewAsync(factory)` | 65-75 | **pure/host-neutral** | existing seam |
| `CreateCollapsedBreadcrumbCandidate` | 77-98 | **COM/WebView-bound** | reads `.CoreWebView2` on a real control |
| `AttachBreadcrumbMessengerWhenReadyAsync` | 100-124 | pure/host-neutral | |
| `AttachBreadcrumbMessenger` | 126-140 | pure/host-neutral | |
| `ConfigureBreadcrumbDropDown(env, init)` | 142-177 | **COM/WebView-bound** | constructs the concrete `BreadcrumbDropDownHost` over the Designer WebView2 |
| `ConfigureBreadcrumbDropDown(host, anchor, work)` | 179-195 | **pure/host-neutral** | existing seam |
| `SetBreadcrumbTheme` | 197-198 | thin wiring | |
| `FocusBreadcrumb` | 200-209 | pure/host-neutral | branch on coordinator null |
| `FocusBreadcrumbCore` | 211-221 | **COM/WebView-bound** (but benign) | `Control.Focus()` on an unshown control returns `false` without throwing |
| `SetBreadcrumbDropDownState` | 223-235 | pure/host-neutral | |
| `ResetBreadcrumb` | 237 | thin wiring | |
| `OnBreadcrumbSelectionChanged` | 239-240 | pure/host-neutral | |
| `OnBreadcrumbFolderArrowKeyDown` | 242-248 | **pure/host-neutral** | contains a real ternary branch (`:246`) |
| `OnBreadcrumbUnhandledArrow` | 250-251 | pure/host-neutral | |
| `EnsureBreadcrumbLifecycle` | 253-277 | pure/host-neutral | |
| `EnsureBreadcrumbResourceOwnership` | 279-289 | thin wiring | touches Designer `components` |
| `DisposeBreadcrumbResources` | 291-296 | pure/host-neutral | |

**Counts: 12 pure/host-neutral, 8 thin wiring, 5 COM/WebView-bound.**

### 3.2 The router-injection prior art (the orchestrator's Q2 pointer)

I could not locate a document containing the literal sentence *"a retyped Designer field breaks
reflection-injected tests"*. What I did find is the **mechanical situation that sentence describes**, in
three mutually reinforcing pieces of verified evidence:

**(a) A live test injects a synthetic `WebView2` through the `L0vhBreadcrumb_WebView2` property.** [V]
`QuickFiler.Test/Helper Classes/QfcThemeHelperTests.cs:247-265`:

```csharp
private static ItemViewer CreateItemViewer()
{
    var viewer = CreateUninitialized<ItemViewer>();
    viewer.LblItemNumber = new Label();
    …
    viewer.L0vhBreadcrumb_WebView2 = CreateUninitialized<WebView2>();   // :256
    viewer.TopicThread = new FastObjectListView();
    viewer.L0v2h2_WebView2 = CreateUninitialized<WebView2>();           // :258
    SetPrivateField(viewer, "_menuItems", …);                          // :259-263
```

`CreateUninitialized<T>` bypasses the constructor, and `SetPrivateField` (`:287-294`) uses
`GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)` + `SetValue`. **The property setter at
`ItemViewer.Breadcrumb.cs:22` is what makes this injection possible.** Changing the property's declared
type to an interface would break the assignment at `QfcThemeHelperTests.cs:256` at compile time.

**(b) A contract test pins the property's exact concrete type.** [V]
`QuickFiler.Test/Viewers/ItemViewerBreadcrumbDropDownContractTests.cs:19-29`:

```csharp
PropertyInfo property = typeof(QuickFiler.ItemViewer).GetProperty("L0vhBreadcrumb_WebView2");
property.Should().NotBeNull();
property.PropertyType.Should().Be(typeof(Microsoft.Web.WebView2.WinForms.WebView2));
```

Named in-file as *"Failure-first ItemViewer surface and compatibility contracts for issue #400"* (`:14`).
**Any retyping of `L0vhBreadcrumb_WebView2` is a red test by construction.**

**(c) The working approach is to inject the coordinator/host — not the field.** [V] the entire
`QuickFiler.Test/Viewers/BreadcrumbDropDownIntegrationTests.cs` harness
(`ItemViewerDropDownHarness`, `:328-473`) constructs a real `ItemViewer` and then injects a
`Mock<IBreadcrumbDropDownHost>` through the **overload seam** at `ItemViewer.Breadcrumb.cs:179-195`:

```csharp
MethodInfo method = typeof(QuickFiler.ItemViewer).GetMethod(
    "ConfigureBreadcrumbDropDown", BindingFlags.Instance | BindingFlags.NonPublic, null,
    new[] { typeof(IBreadcrumbDropDownHost), typeof(Func<Rectangle>), typeof(Func<Rectangle>) }, null);
method.Should().NotBeNull("issue #400 requires an injectable ItemViewer popup integration seam");  // :400
method.Invoke(Viewer, new object[] { Host.Object, new Func<Rectangle>(() => AnchorScreenBounds),
                                     new Func<Rectangle>(() => WorkingArea) });                    // :401-409
```

The same file injects the **routing surface** rather than the control at `:340` (`InitializeBreadcrumbPipeline(provider.Object)`),
`:414-421` (`AttachBreadcrumbMessenger`), `:450-455` (`SetBreadcrumbTheme`), and reaches the router
through `Viewer.BreadcrumbCoordinator.CancelSelector()` (`:363`, `:437`).

**Design rule for F14, derived from (a)+(b)+(c) [I]:**

> **Never retype, rename, or remove `L0vhBreadcrumb_WebView2` or the Designer field it wraps.** Add every
> new seam as a **sibling overload that accepts the collaborator** (host / operations / messenger /
> candidate factory / geometry delegate), exactly as `:40-43`, `:65-67`, and `:179-183` already do.
> Keep the zero-/one-argument production wrapper unchanged so no call site moves.

### 3.3 Recommended seam set

Applying the epic's hierarchy (interface seam > injectable delegate > adapter) to the five
COM/WebView-bound members. **Three of the five already have a seam**; only two need new work.

| Member | Existing seam? | Recommendation |
|---|---|---|
| `InitializeBreadcrumbPipeline(provider)` `:37-38` | **Yes** — the two-arg overload `:40-43` | No change. Test the one-arg wrapper under an ambient `SynchronizationContext` (see §5.2 pattern). |
| `AttachBreadcrumbWebViewAsync()` `:62-63` | **Yes** — the factory overload `:65-67` | No change. |
| `ConfigureBreadcrumbDropDown(env, init)` `:142-177` | **Partly** — the injected overload `:179-183` covers the tail | **S-1 (new, injectable delegate):** extract the host construction at `:158-168` into a private `Func<CoreWebView2Environment, IWebViewCoreInitializer, IBreadcrumbDropDownHost>`-shaped factory field defaulting to the current `new BreadcrumbDropDownHost(...)`, plus an `internal` overload accepting that factory and the two geometry `Func<Rectangle>`s. This makes `:147-153` (the environment-identity idempotence branch) and the `:169-176` wiring reachable **without a real `CoreWebView2Environment`**. |
| `CreateCollapsedBreadcrumbCandidate` `:77-98` | **No** | **S-2 (new, injectable delegate):** the only irreducible line is `:82` (`_l0vhBreadcrumb_WebView2.CoreWebView2`). Extract it behind a `private Func<CoreWebView2> _readBreadcrumbCore = () => _l0vhBreadcrumb_WebView2.CoreWebView2;` and add an `internal` overload `CreateCollapsedBreadcrumbCandidate(Func<CoreWebView2> readCore, BreadcrumbUiDispatcher dispatcher, Action navigate)`. `:84-97` then becomes reachable with a `FormatterServices.GetUninitializedObject(typeof(CoreWebView2))` stand-in — the exact technique already used for `CoreWebView2Environment` at `QuickFiler.Test/Controllers/QfcItemControllerBreadcrumbDropDownTests.cs:30-31`. |
| `FocusBreadcrumbCore` `:211-221` | n/a | **No seam needed.** `Control.Focus()` on a never-shown control with no handle returns `false` without side effects. Testable directly on a headless `ItemViewer` with a `CreateUninitialized<WebView2>()` assigned via the property setter. |

Both S-1 and S-2 are **injectable delegates**, tier 2 of the hierarchy. An interface seam (tier 1) was
considered and rejected: the collaborators already have interfaces (`IBreadcrumbDropDownHost`,
`IWebViewMessenger`, `IWebViewCoreInitializer`); the thing that is not injectable is the *construction*
of the concrete host and the *read* of a sealed SDK property, neither of which an interface can abstract
without inventing a wrapper type that would live in F13's assignment.

### 3.4 Rejected alternatives (brief)

- **Retype `L0vhBreadcrumb_WebView2` to an `IBreadcrumbWebHost`.** Rejected: breaks
  `QfcThemeHelperTests.cs:256` and fails the pinned contract test at
  `ItemViewerBreadcrumbDropDownContractTests.cs:28`; also breaks the production call site
  `QfcItemController.ViewerSetup.cs:109` which passes the concrete control to
  `_webViewInitializer.EnsureCoreWebView2Async`.
- **Extract the whole file into a new host-neutral coordinator class.** Rejected: already done once under
  issue #400 P9-T12 (`BreadcrumbItemViewerLifecycleCoordinator.cs`); a second extraction would duplicate
  F12's assignment and breach the epic's disjoint-file-set rule.
- **STA-thread tests with a real WebView2 core.** Rejected: `CoreWebView2` requires a real browser process
  — an external process, banned by `.claude/rules/general-unit-test.md` § External Dependencies.

---

## 4. Q3 — Determinism

### (a) Does this file read wall-clock time or use timers?

**No. [V]** Full read of all 298 lines: zero occurrences of `DateTime`, `DateTimeOffset`, `Stopwatch`,
`Timer`, `Thread.Sleep`, `Task.Delay`, `CancellationTokenSource(TimeSpan)`, or any `using System.Diagnostics`.
The `using` set is `System`, `System.ComponentModel`, `System.Drawing`, `System.Threading.Tasks`,
`System.Windows.Forms`, `Microsoft.Web.WebView2.Core`, `QuickFiler.Viewers`, `UtilitiesCS.OutlookObjects.Folder`
(`:1-8`).

Nor do its immediate collaborators: `BreadcrumbUiDispatcher.cs` and
`BreadcrumbItemViewerLifecycleCoordinator.cs` contain no clock or timer reads (verified by full read).

### (b) What clock abstraction does the repo provide?

**`System.TimeProvider`, polyfilled for net481 by `Microsoft.Bcl.TimeProvider`, with
`Microsoft.Extensions.TimeProvider.Testing.FakeTimeProvider` as the test double.** [V]

- Production reference: `QuickFiler/QuickFiler.csproj:68-69` →
  `..\packages\Microsoft.Bcl.TimeProvider.10.0.10\lib\net462\Microsoft.Bcl.TimeProvider.dll`
- Test references: `QuickFiler.Test/QuickFiler.Test.csproj:205-206` (`Microsoft.Bcl.TimeProvider`) and
  `:255-256` (`Microsoft.Extensions.TimeProvider.Testing 10.8.0`)
- Package declarations: `QuickFiler.Test/packages.config:18`, `:84-88`
- QuickFiler production files already consuming `TimeProvider`: `Helper Classes/EmailMoveMonitor.cs`,
  `Controllers/QfcStreamingDequeueConfidenceGate.cs`, `Controllers/QfcHomeController.cs`,
  `Controllers/QfcHomeController.Metrics.cs`, `Controllers/QfcDatamodel.cs`,
  `Controllers/QfcDatamodel.QueueProcessing.cs`, `Controllers/QfcDatamodel.FrameBuilding.cs`
- QuickFiler.Test files already consuming `FakeTimeProvider`: `Controllers/QfcDatamodelTests.cs:106,254,288`,
  `Controllers/QfcDatamodelLivenessTests.cs:84`, `Controllers/QfcHomeControllerMetricsTests.cs:318-319,404`,
  `Controllers/QfcStreamingDequeueConfidenceGateTests*.cs`
- In-code statement of the repo rule, `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs:316`:
  *"Moq cannot mock the non-virtual GetLocalNow(); FakeTimeProvider is the prescribed seam."*

There is **no** `IClock`, `ISystemClock`, or `ITimerService` seam in QuickFiler; `Func<DateTime>` appears
nowhere in the breadcrumb tree.

`.claude/rules/general-unit-test.md` § Determinism Infrastructure names *"`FakeTimeProvider` for .NET"* as
the required facility — so the repo is already aligned with policy on net481, contrary to premise P6.

### (c) Concrete deterministic-time recommendation for these tests

**No clock seam is needed for this file.** Determinism here is a *scheduling* problem, not a *time*
problem, and the repo already provides two proven mechanisms:

1. **Inline dispatch via ambient-context reference identity (preferred, already in use).** Set a plain
   `SynchronizationContext` before constructing the viewer:

   ```csharp
   _previous = SynchronizationContext.Current;
   SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
   Viewer = new QuickFiler.ItemViewer();
   ```
   [V] `QuickFiler.Test/Viewers/BreadcrumbDropDownIntegrationTests.cs:336-338`, and identically at
   `QuickFiler.Test/Controllers/QfcItemControllerBreadcrumbDropDownTests.cs:369-374`,
   `Viewers/BreadcrumbCoordinatorLifecycleTests.cs:477`, `Viewers/BreadcrumbPendingOpenCloseTests.cs:363`,
   `Viewers/BreadcrumbCollapsedSurfaceReadinessTests.cs:413`, `Viewers/BreadcrumbSubfolderActivationTests.cs:305`,
   `Viewers/BreadcrumbSelectorOpenRetryTests.cs:255`.

   Because `BreadcrumbUiDispatcher.IsCurrentBoundary()` (`:271`) compares
   `ReferenceEquals(SynchronizationContext.Current, _context)`, every `Dispatch`/`PostAsync` issued from
   the test method body on the same thread runs **synchronously inline** (`:78-95`). No pump, no timer,
   no wait. [I from V code]

2. **A drainable test context for the genuinely-asynchronous cases.** [V] the repo already has one —
   `QuickFiler.Test/Viewers/BreadcrumbSelectorOpenRetryTests.cs:38,41,61,69` uses
   `harness.Context.DrainUntil(harness.Viewer.BreadcrumbOpenTask)`. Reuse that type rather than adding a
   wait.

3. **Off-boundary paths:** use `BreadcrumbPopupUiOperations.CreateForCurrentThreadTests()`
   (`BreadcrumbPopupUiOperations.cs:83-84`) via the `:40-43` overload when the test needs the
   "cannot marshal cross-thread UI work" branch (`BreadcrumbUiDispatcher.cs:97-105`).

**Banned-API compliance:** the plan must contain zero `Thread.Sleep`, `Task.Delay`, `DateTime.Now`,
`SpinWait`, or `Task.Wait(timeout)`. Note the one pre-existing exception to watch:
`QuickFiler.Test/Viewers/BreadcrumbSelectorOpenRetryTests.cs:37` uses
`Task.Run(() => firstOpen.SetException(failure)).GetAwaiter().GetResult()` — an unbounded synchronous
join, not a timed wait. It is acceptable, but F14 should not copy it; prefer completing the TCS on the
test thread.

---

## 5. Baseline reality: what is already exercised

### 5.1 Existing tests that drive this file

[V] Eight test files construct a live headless `ItemViewer` in a **plain `[TestClass]`** (no
`[STATestClass]` anywhere in `QuickFiler.Test` — grep for `STATestClass` across the repo returns
`Tags.Test`, `TaskVisualization.Test`, and docs only):

| Test file | Construction site | Members of this file it drives |
|---|---|---|
| `Controllers/QfcItemControllerBreadcrumbDropDownTests.cs` | `:373` | `:37-60`, `:65-75`, `:142-177`, `:179-195`, `:26-27`, `:237` |
| `Viewers/BreadcrumbDropDownIntegrationTests.cs` | `:338` | `:37-60`, `:126-140`, `:179-195`, `:197-198`, `:25`, `:291-296` |
| `Viewers/BreadcrumbCoordinatorLifecycleTests.cs` | `:477` | `:126-140`, `:237` |
| `Viewers/BreadcrumbCollapsedSurfaceReadinessTests.cs` | `:413` | `:100-124` (`:438`), `:237` |
| `Viewers/BreadcrumbSelectorOpenRetryTests.cs` | `:255` | `:29-30`, `:126-140` |
| `Viewers/BreadcrumbSubfolderActivationTests.cs` | `:305` | `:126-140` |
| `Viewers/BreadcrumbPendingOpenCloseTests.cs` | `:363` | drop-down state paths |
| `Controllers/QfcItemController.EventWiringTests.cs` / `.ViewerSetupTests.cs` | `:236`, `:327`, `:386` | `:32`, `:37-60` |

**This is the single most important planning input.** [I] The measured line rate of this file, once the
attribute is removed, will very plausibly be in the 50-75% range rather than 0%. **The plan's Phase 1
must be "remove attribute → run F1 harness → record actual per-file line and branch rate", and every
subsequent test task must be justified against that measured gap, not against an assumed zero.**

### 5.2 Sanctioned headless-`ItemViewer` construction — and its one hazard

[V] Two distinct working techniques exist, and they are not interchangeable:

- **Full construction** — `new QuickFiler.ItemViewer()` inside a `SynchronizationContext` scope. Runs
  `InitializeComponent()`, so the whole Designer control tree exists (including a real
  `Microsoft.Web.WebView2.WinForms.WebView2` at `ItemViewer.Designer.cs:46`, which is constructed but
  never initialised — `CoreWebView2` stays null). Required whenever the test needs the real
  `_l0vhBreadcrumb_WebView2` or `components`.
- **Uninitialised construction** — `CreateUninitialized<ItemViewer>()` + property assignment
  (`QfcThemeHelperTests.cs:249-264`). No `InitializeComponent`, no `_context`, no `components`. Cheaper,
  but `EnsureBreadcrumbResourceOwnership` (`:279-289`) will then create its own `Container` at `:286`
  which **`Control.Dispose` will never dispose**, because the uninitialised object's base `Control`
  state is not valid for disposal. Use only for members that do not touch `components`.

**Hazard [V, from agent memory corroborated by code]:** constructing a WinForms `Control` installs a
`WindowsFormsSynchronizationContext` as `SynchronizationContext.Current`. `ItemViewer`'s constructor
(`ItemViewer.cs:23-30`) then captures whatever is current at `:26` and calls
`TaskScheduler.FromCurrentSynchronizationContext()` at `:27`, which throws if none is present — hence the
mandatory pre-set at the harness sites. Awaiting a continuation that posts back to a
`WindowsFormsSynchronizationContext` on a pumpless MSTest thread **can deadlock**. The existing
`async Task` test at `QfcItemControllerBreadcrumbDropDownTests.cs:188-262` avoids this only because its
readiness completes synchronously (`CompletedReadiness`, `:264-273`). **Every new `async` test task in
F14's plan must complete its readiness/TCS synchronously on the test thread.**

---

## 6. Q4 — Sibling boundaries

### 6.1 Dependency inventory

| Symbol used | Declared in | Owning child | Public surface sufficient? |
|---|---|---|---|
| `BreadcrumbItemViewerLifecycleCoordinator` (`:15,50,253-277`) | `Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs:13` | **F12** | **YES** — all 12 members used (`SetBridgeCoordinator`, `AttachCollapsedAsync`, `AttachCollapsedWithReadinessAsync`, `AttachCollapsedMessenger`, `ConfigureHost`, `SetTheme`, `Focus`, `SetDroppedDown`, `Reset`, `Dispose`, `DropDownHost`, `CurrentOpenTask`, `Operations`, `Hub`) are already `internal` and reachable |
| `BreadcrumbBridgeCoordinator` (`:25,53,59`) | `Viewers/BreadcrumbBridgeCoordinator.cs` | **F12** | **YES** — only the constructor `(hub, provider, dispatcher)` and `CancelSelector()` are used |
| `BreadcrumbMessengerHub` (`:263`) | `Viewers/BreadcrumbMessengerHub.cs:?` | **F12** | **YES** — parameterless ctor only |
| `BreadcrumbCollapsedAttachment` (`:264`) | `Viewers/BreadcrumbMessengerHub.cs:277` | **F12** (same file) | **YES** — ctor only |
| `BreadcrumbResourceOwner` (`:16,287`) | `Viewers/BreadcrumbMessengerHub.cs:436` | **F12** (same file) | **YES** — `BreadcrumbResourceOwner(Action dispose)` ctor only |
| `BreadcrumbPopupLifecycleOperations.CreateCollapsedCandidate` (`:84`) | `Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs:380-409` | **F12** | **YES** — `internal static`, two `Func<>` parameters |
| `BreadcrumbPopupUiOperations` + `.CaptureCurrent()` / `.CaptureCurrentOrTests()` / `.NavigateToDocument()` (`:38,42,84-97,156,192`) | `Viewers/BreadcrumbPopupUiOperations.cs:29,80,86,425` | **F13** | **YES** — all `internal static`; `CreateForCurrentThreadTests()` (`:83`) is the test seam and is already public-to-assembly |
| `BreadcrumbUiDispatcher` + `.CaptureCurrent()` (`:56,83`) | `Viewers/BreadcrumbUiDispatcher.cs:12,44` | **F13** | **YES** |
| `BreadcrumbCollapsedSurfaceController` (`:266`) | `Viewers/BreadcrumbCollapsedSurfaceController.cs` | **F13** | **YES** — parameterless ctor only |
| `BreadcrumbDropDownHost` (`:148,158-168`) | `Viewers/BreadcrumbDropDownHost.cs:22` | **F13** | **YES** for the type-pattern test at `:148`; the 8-arg ctor at `:159-168` is `public` (`BreadcrumbDropDownHost.cs:37` or `:79`) |
| `IBreadcrumbDropDownHost` (`:26,180`) | `Viewers/IBreadcrumbDropDownHost.cs:19` (`public interface`) | **F13** | **YES** |
| `IWebViewMessenger` (`:66,78,101,126`) | `Viewers/IWebViewMessenger.cs` | **F13** | **YES** |
| `IWebViewCoreInitializer` (`:144`) | `Viewers/IWebViewCoreInitializer.cs` | **F13** | **YES** |
| `WebView2Messenger` (`:85`) | `Viewers/WebView2Messenger.cs` | **F13** | **YES** — ctor `(CoreWebView2, BreadcrumbUiDispatcher)` |
| `BreadcrumbNavigationReadiness` (`:66,79,102`) | `Viewers/BreadcrumbWebViewSurfaceFactory.cs:19` | **F13** | **YES** — `internal sealed`, and tests already construct it directly (`QfcItemControllerBreadcrumbDropDownTests.cs:266`) |
| `IFolderHierarchyProvider` (`:41`) | `UtilitiesCS/OutlookObjects/Folder/` | **outside the epic** | YES (public) |
| `BreadcrumbArrowDirection` (`:32,242,250`) | `UtilitiesCS/OutlookObjects/Folder/BreadcrumbBridgeMessages.cs:11` (`public enum`) | **outside the epic** | YES |
| `QfcItemController` call sites | `Controllers/QfcItemController.ViewerSetup.cs:107-179`, `:396` | **F10** | YES — no change needed to F10 |
| `components` (`:286,288`) | `Viewers/ItemViewer.Designer.cs:10` | **F14 (own)** | n/a |
| `IsDisposed` (`:214`) | `System.Windows.Forms.Control` | framework | n/a |

### 6.2 Cross-child notes

**Conclusion: F14 requires ZERO signature changes in any F10/F12/F13 file.** Every collaborator this file
touches is already reachable at its current visibility, and every one of the existing eight harnesses
proves it empirically. Three advisory notes for the orchestrator to carry into `spec.md` anyway:

- **X-1 (advisory, F13 → F14 — freeze request).** `BreadcrumbPopupUiOperations.CaptureCurrentOrTests()`
  (`BreadcrumbPopupUiOperations.cs:86-89`) and `BreadcrumbUiDispatcher.CreateForCurrentThreadTests()`
  (`BreadcrumbUiDispatcher.cs:62-65`) are **F14's only mechanism for driving `:155-157` and `:191-193`
  without an ambient UI context**. F13 must not remove, rename, or change the semantics of either. If
  F13's own coverage work "cleans up" these test-only factories, F14's plan breaks. **Record as a frozen
  contract.**
- **X-2 (advisory, F12 → F14 — freeze request).** `BreadcrumbItemViewerLifecycleCoordinator`'s
  constructor signature `(hub, collapsedAttachment, operations, Action, Action<BreadcrumbArrowDirection>,
  Action<BreadcrumbArrowDirection>)` (`:29-36`) is consumed verbatim at `ItemViewer.Breadcrumb.cs:268-275`.
  Any reordering or type change by F12 is a compile break in F14's file. **Record as a frozen contract.**
- **X-3 (defect note, F13-owned, no signature change requested).** The self-referential closure at
  `ItemViewer.Breadcrumb.cs:158-164` is safe **only** because no `BreadcrumbDropDownHost` constructor
  overload invokes its `returnFocus` delegate. F13 should be told not to introduce constructor-time
  invocation of that delegate. This is a note, not a change request.

**No edit is recommended to any file listed in the orchestrator's F13/F12/F10 prohibition lists.**

---

## 7. Q5 — Test plan sketch

### 7.1 Gate arithmetic

Independent gates per epic.md § Coverage-Target Reconciliation: **>= 80% line AND >= 75% branch** per
production file (the `EfcHomeController.Timing.cs` cautionary example at 100% line / 66.67% branch is
cited at `epic.md:500-502`). New files created by F14, if any: **>= 90% line**.

`ItemViewer.Breadcrumb.cs` has a branch-dense shape — roughly 26 decision points across 298 lines
(guards at `:45`, `:69`, `:106`, `:109`, `:113`, `:128`, `:132`, `:147-149` (compound `is` + `&&`),
`:185`, `:189`, `:190`, `:202`, `:213-217` (three-term `&&`), `:225`, `:227`, `:257`, `:281`; null-conditionals
at `:27`, `:30`, `:198`, `:237`, `:240`, `:243`, `:251`, `:293`; `??` at `:30`, `??=` at `:286`; ternary at `:246`).
**The branch gate, not the line gate, is the binding constraint.** Several cases below exist purely to
close branch pairs.

### 7.2 Sequencing (mandatory)

- **T0 — Remove `[ExcludeFromCodeCoverage]` from `ItemViewer.cs:20`** and settle the
  `ItemViewer.Designer.cs` disposition per §1.3. **Not a test case; a prerequisite.**
- **T0b — Run the F1 harness and record the actual per-file line and branch rate.** Every case below is
  **conditional on the measured gap**. Do not author a case whose lines T0b shows already covered.
  Cite issue #441 whenever quoting a `<class>` `line-rate` attribute; prefer harness-recomputed figures.

### 7.3 Case inventory

All cases: MSTest `[TestClass]`/`[TestMethod]`, Moq, FluentAssertions, AAA, no temp files, no external
services, no live Form, no popup, no `Thread.Sleep`/`Task.Delay`/wall-clock wait.
**Proposed home: `QuickFiler.Test/Viewers/ItemViewerBreadcrumbWiringTests.cs`** (new;
needs `<Compile Include="Viewers\ItemViewerBreadcrumbWiringTests.cs" />` in
`QuickFiler.Test/QuickFiler.Test.csproj`, CRLF preserved, adjacent to `:80`). Split into `.Part2.cs`
if it approaches 500 lines.

Fixture `V` = full `new QuickFiler.ItemViewer()` inside a `SynchronizationContext` scope, disposed in
`finally` (pattern of `BreadcrumbDropDownIntegrationTests.cs:328-473`).
Fixture `U` = `CreateUninitialized<ItemViewer>()` + property assignment (pattern of
`QfcThemeHelperTests.cs:247-265`).

| # | Test name | Production lines / branches covered | Fixture | Seam used | Mocks |
|---|---|---|---|---|---|
| C1 | `L0vhBreadcrumb_WebView2_RoundTripsTheDesignerField` | `:21`, `:22` | U | property setter | none (`CreateUninitialized<WebView2>()`) |
| C2 | `BreadcrumbDropDownHost_BeforeInitialize_IsNull` | `:26-27` **null branch** | U | — | none |
| C3 | `BreadcrumbOpenTask_BeforeInitialize_ReturnsCompletedFalse` | `:29-30` **`??` right branch** | U | — | none |
| C4 | `BreadcrumbOpenTask_AfterInitialize_ReturnsCoordinatorTask` | `:29-30` **`??` left branch** | V | `:40-43` | `Mock<IFolderHierarchyProvider>` |
| C5 | `InitializeBreadcrumbPipeline_WithInjectedOperations_CreatesCoordinatorAndBridge` | `:40-44`, `:50-60` | V | `:40-43` + `CreateForCurrentThreadTests()` | `Mock<IFolderHierarchyProvider>(Strict)` |
| C6 | `InitializeBreadcrumbPipeline_SecondCall_IsNoOpAndKeepsFirstCoordinator` | `:45-48` **true branch** | V | `:40-43` | two distinct `Mock<IFolderHierarchyProvider>` |
| C7 | `InitializeBreadcrumbPipeline_SingleArgOverload_CapturesAmbientContext` | `:37-38` | V | ambient `SynchronizationContext` | `Mock<IFolderHierarchyProvider>` |
| C8 | `AttachBreadcrumbWebViewAsync_BeforeInitialize_ReturnsFalseWithoutInvokingFactory` | `:69-72` **true branch** | U | `:65-67` | factory delegate counting invocations |
| C9 | `AttachBreadcrumbWebViewAsync_AfterInitialize_DelegatesToCollapsedAttachment` | `:69`, `:74` | V | `:65-67` | fake `IWebViewMessenger` + synchronous `BreadcrumbNavigationReadiness` |
| C10 | `CreateCollapsedBreadcrumbCandidate_WithInjectedCoreReader_BuildsMessengerAndReadiness` | `:82-97` | V | **new seam S-2** | `FormatterServices.GetUninitializedObject(typeof(CoreWebView2))` |
| C11 | `AttachBreadcrumbMessengerWhenReadyAsync_NullMessenger_Throws` | `:106-108` | U | — | none |
| C12 | `AttachBreadcrumbMessengerWhenReadyAsync_NullReadiness_Throws` | `:109-112` | U | — | fake `IWebViewMessenger` |
| C13 | `AttachBreadcrumbMessengerWhenReadyAsync_BeforeInitialize_ThrowsInvalidOperation` | `:113-118` **true branch** | U | — | fake messenger + readiness |
| C14 | `AttachBreadcrumbMessengerWhenReadyAsync_AfterInitialize_DelegatesToCoordinator` | `:113` false, `:120-123` | V | `:40-43` | fake messenger + synchronous readiness |
| C15 | `AttachBreadcrumbMessenger_NullMessenger_Throws` | `:128-131` | U | — | none |
| C16 | `AttachBreadcrumbMessenger_BeforeInitialize_ThrowsInvalidOperation` | `:132-137` **true branch** | U | — | fake messenger |
| C17 | `AttachBreadcrumbMessenger_AfterInitialize_AttachesToHub` | `:132` false, `:139` | V | `:40-43` | `TrackingMessenger` (existing pattern, `BreadcrumbDropDownIntegrationTests.cs:475+`) |
| C18 | `ConfigureBreadcrumbDropDown_SameEnvironmentTwice_ReusesHost` | `:147-153` **true branch** | V | **new seam S-1** | `Mock<IWebViewCoreInitializer>(Strict)`, uninitialised `CoreWebView2Environment` |
| C19 | `ConfigureBreadcrumbDropDown_DifferentEnvironment_ConstructsNewHost` | `:147-149` **false branch**, `:155-176` | V | **new seam S-1** | two uninitialised environments |
| C20 | `ConfigureBreadcrumbDropDown_ReturnFocusDelegate_FocusesControlHost` | closure body `:164` | V | S-1 + `Mock<IBreadcrumbDropDownHost>` | Moq |
| C21 | `ConfigureBreadcrumbDropDown_CancelDelegate_CancelsSelector` | closure body `:166` | V | S-1 | `Mock<IFolderHierarchyProvider>` |
| C22 | `ConfigureBreadcrumbDropDown_GeometryDelegates_ProjectAnchorAndWorkingArea` | closure bodies `:172-174`, `:175` | **V, STA — see §7.4** | S-1 | none |
| C23 | `ConfigureBreadcrumbDropDown_InjectedOverload_NullHost_Throws` | `:185-188` | U | `:179-183` | none |
| C24 | `ConfigureBreadcrumbDropDown_InjectedOverload_NullAnchorBounds_Throws` | `:189` **`??` throw branch** | U | `:179-183` | `Mock<IBreadcrumbDropDownHost>` |
| C25 | `ConfigureBreadcrumbDropDown_InjectedOverload_NullWorkingArea_Throws` | `:190` **`??` throw branch** | U | `:179-183` | `Mock<IBreadcrumbDropDownHost>` |
| C26 | `ConfigureBreadcrumbDropDown_InjectedOverload_ConfiguresLifecycleHost` | `:191-194` | V | `:179-183` | `Mock<IBreadcrumbDropDownHost>` |
| C27 | `SetBreadcrumbTheme_BeforeInitialize_IsNoOp` | `:197-198` **null branch** | U | — | none |
| C28 | `SetBreadcrumbTheme_AfterInitialize_ForwardsToCoordinatorAndHost` | `:197-198` **non-null branch** | V | `:179-183` | `Mock<IBreadcrumbDropDownHost>` verifying `SetTheme` |
| C29 | `FocusBreadcrumb_BeforeInitialize_CallsCoreDirectly` | `:202-205` **true branch** | V | — | none |
| C30 | `FocusBreadcrumb_AfterInitialize_RoutesThroughCoordinator` | `:202` false, `:208` | V | `:40-43` | none |
| C31 | `FocusBreadcrumbCore_WhenViewerDisposed_DoesNotTouchControl` | `:213-214` **short-circuit branch** | V (disposed) | — | none |
| C32 | `FocusBreadcrumbCore_WhenWebViewNull_DoesNotThrow` | `:215` **null branch** | U (`L0vhBreadcrumb_WebView2 = null`) | property setter | none |
| C33 | `FocusBreadcrumbCore_WhenWebViewDisposed_DoesNotThrow` | `:216` **true branch** | U + disposed `WebView2` | property setter | none |
| C34 | `FocusBreadcrumbCore_WhenLive_FocusesWebView` | `:213-219` **all-false path** | V | — | none |
| C35 | `SetBreadcrumbDropDownState_BeforeInitializeAndDroppedDown_FocusesBreadcrumb` | `:225`, `:227-229` **both true** | V | — | none |
| C36 | `SetBreadcrumbDropDownState_BeforeInitializeAndClosed_IsNoOp` | `:227` **false branch**, `:231` | U | — | none |
| C37 | `SetBreadcrumbDropDownState_AfterInitialize_ForwardsToCoordinator` | `:225` false, `:234` | V | `:179-183` | `Mock<IBreadcrumbDropDownHost>` |
| C38 | `ResetBreadcrumb_BeforeInitialize_IsNoOp` | `:237` **null branch** | U | — | none |
| C39 | `ResetBreadcrumb_AfterInitialize_ResetsCoordinator` | `:237` **non-null branch** | V | `:40-43` | none |
| C40 | `OnBreadcrumbSelectionChanged_RaisesFolderSelectionChanged` | `:239-240` **both branches** (no subscriber / one subscriber) | V | coordinator selection callback | `Mock<IFolderHierarchyProvider>` |
| C41 | `OnBreadcrumbFolderArrowKeyDown_Right_RaisesKeysRight` | `:242-248` **ternary true** | V | coordinator arrow callback | none |
| C42 | `OnBreadcrumbFolderArrowKeyDown_Left_RaisesKeysLeft` | `:246` **ternary false** | V | coordinator arrow callback | none |
| C43 | `OnBreadcrumbFolderArrowKeyDown_NoSubscriber_DoesNotThrow` | `:243` **null-conditional branch** | V | — | none |
| C44 | `OnBreadcrumbUnhandledArrow_RaisesEventToSubscriber` | `:250-251` **both branches** | V | `BreadcrumbUnhandledArrow` | none |
| C45 | `EnsureBreadcrumbLifecycle_SecondCall_ReturnsSameCoordinator` | `:257-260` **true branch** | V | `:40-43` twice via distinct entry points (`:50` then `:155`) | none |
| C46 | `EnsureBreadcrumbResourceOwnership_SecondCall_DoesNotAddSecondComponent` | `:281-284` **true branch** | V | — | none |
| C47 | `EnsureBreadcrumbResourceOwnership_WhenComponentsNull_CreatesContainer` | `:286` **`??=` null branch** | U + `SetPrivateField(viewer,"components",null)` | reflection helper | none |
| C48 | `DisposeBreadcrumbResources_OnViewerDispose_ClearsCoordinatorAndBridge` | `:291-296` | V | `Viewer.Dispose()` | none |
| C49 | `DisposeBreadcrumbResources_BeforeInitialize_IsNoOp` | `:293` **null branch** | V (dispose without initialise) | — | none |

**49 cases.** Per issue #136 each becomes one atomic task. Expect T0b to eliminate a meaningful fraction —
C5, C9, C14, C17, C18, C19, C26, C28, C37, C39, C40, C48 are all plausibly already covered by the eight
existing harnesses. **The planner must prune against measured data, not against this list.**

### 7.4 STA determination

**Exactly one case requires STA: C22.**

`ConfigureBreadcrumbDropDown`'s geometry closures at `:172-174` and `:175` call
`Control.RectangleToScreen(...)` and `Screen.FromControl(control)`. Both require a real
`System.Windows.Forms.Control` instance; `Screen.FromControl` in particular reads `control.Handle`
indirectly through `MonitorFromWindow`. No seam can cover the *bodies* of these two closures, because a
seam would replace them — which is precisely what seam S-1 does for every *other* line of the method.
Covering the closure bodies therefore requires a real control.

Justification for the STA last-resort clause (epic.md § Shared Design 3):
- (a) A seam was tried first and adopted for everything else in the method (S-1 covers `:147-176` minus
  the two closure bodies). The residual two closures are the irreducible remainder.
- (b) The test lives in a dedicated `QuickFiler.Test/Viewers/ItemViewerBreadcrumbGeometry.StaTests.cs`
  with `[STATestClass]`/`[STATestMethod]`, following `Tags.Test/CheckBoxControllerWiring.StaTests.cs`
  (`:20-21`) and `TaskVisualization.Test/TaskControllerAccelerator.StaTests.cs`.
- The control is a never-shown in-memory `Microsoft.Web.WebView2.WinForms.WebView2` (a `Control`, not a
  `Form`); no `Show()`, no message pump, no timer; disposed in `finally`.
- **This would be the first `*.StaTests.cs` file in `QuickFiler.Test`** — verified: grep for
  `STATestClass` across the repo returns hits only in `Tags.Test`, `TaskVisualization.Test`, docs, and
  agent memory. No new package is needed (MSTest 4.3.3, packages.config `:113-119`).

**Alternative worth pricing before committing to STA [I]:** if T0b shows the file already clears 80/75
without C22, drop C22 and leave the two closure bodies uncovered. Two uncovered lines out of ~298 is
0.7% — almost certainly affordable. **Recommend deferring the STA decision to after T0b.**

---

## 8. Q6 — 500-line rule

- Current: **298 code lines** (299 physical). Limit 500. Headroom **202 lines**.
- Projected additions: S-1 (host factory field + one `internal` overload + parameter forwarding) ≈ **+22
  lines after CSharpier**; S-2 (core-reader field + one `internal` overload) ≈ **+16 lines**. XML doc
  comments on the two new `internal` members ≈ **+8 lines**.
- **Projected post-refactor: ~344 lines. No split required.**

For reference, this file was previously at **399 lines** before the issue #400 P9-T12 extraction
(`remediation-plan.2026-07-21T21-37.md:622`), so 344 is well inside its own historical envelope.

**If a split later becomes necessary** (e.g. if T0b forces more seams than projected), the natural
cleavage is by lifecycle stage, and both halves stay coherent:
- `ItemViewer.Breadcrumb.cs` — property, coordinator/host accessors, initialise, attach, reset, dispose
  (`:15-140`, `:237`, `:253-296`) ≈ 190 lines.
- `ItemViewer.Breadcrumb.DropDown.cs` (new) — drop-down configuration, theme, focus, dropped-down state,
  and the three `On*` raisers (`:142-251`) ≈ 150 lines.

A new file requires a `<Compile Include="Viewers\ItemViewer.Breadcrumb.DropDown.cs">` entry with
`<DependentUpon>ItemViewer.cs</DependentUpon>` adjacent to `QuickFiler/QuickFiler.csproj:423-426`
(CRLF preserved, minimal hunk), a ledger row at **>= 90% line** per epic.md § Mid-Wave File Creation
rule 4, and — critically — it inherits the same type-level exemption question as §1.3.

---

## 9. Q9 — Open-issue bearing

**Constraint: `gh` could not be run (Bash tool disabled this session).** The following is what the
repository itself evidences; items marked *(unverified)* rest on the orchestrator's supplied
description only.

| Issue | Bearing on this file |
|---|---|
| **#441** — coverage harness double-counts `<line>` nodes | **Direct and load-bearing. [V]** `scripts/vscode/Invoke-MSTestWithCoverage.Helpers.ps1:121-122`. Every headline `line-rate`/`lines-valid` this harness emits for QuickFiler is inflated. F14's acceptance evidence must come from F1's recomputed per-file figures (deduplicated `<line>` nodes, child axis), and any `<class>`-attribute figure quoted anywhere in F14's artifacts must carry an explicit "#441 — unreliable" annotation. Note the per-file merge path at `:181,219` is already correct, so F1 has a working precedent inside the same script. |
| **#400** — `quickfiler-folder-selector-dropdown-400` (active) | **Highest conflict risk for this file. [V]** Its feature folder is live at `docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/` and its remediation plan explicitly authorises edits to **`QuickFiler/Viewers/ItemViewer.Breadcrumb.cs`** (`remediation-plan.2026-07-21T21-37.md:626` P9-T12, `:725` P9-T28). It also owns the eight test harnesses this file depends on. **F14 must read the current merged state of #400 before planning and must not assume the file shape in this artifact survives a #400 merge.** epic.md `:638` already flags #400 as overlapping F13; **this artifact extends that flag to F14**, which the epic does not currently record. |
| **#230** — WinForms message-pump test seam *(unverified)* | If a pump seam lands, the §5.2 deadlock hazard and the C22 STA determination both change. **F14 should check #230's state at plan time**; if a pump helper exists in `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs`, prefer it over STA. |
| **#440** — breadcrumb left/right arrow parent-child navigation *(unverified)* | Touches `OnBreadcrumbFolderArrowKeyDown` (`:242-248`) and `OnBreadcrumbUnhandledArrow` (`:250-251`) semantics — the exact ternary at `:246` that C41/C42 pin. If #440 changes the Right→`Keys.Right` / Left→`Keys.Left` mapping, C41/C42 become wrong. **Cases C41, C42, C44 should be written to assert the mapping as currently implemented and cite #440 in a comment so a future change is a visible red test, not a silent semantic drift.** |
| **#439** — efcviewer missing lineage and segment navigation *(unverified)* | EfcViewer surface; **no dependency on this file** (`EfcViewer.cs` is F9-assigned and has no breadcrumb pipeline). No bearing. |
| **#426** — emailmovemonitor hook retention | No bearing on this file. |

---

## 10. Latent defect promotion candidates

Each is a distinct promotion candidate. **All are out of scope to fix under the epic's
no-behavior-change NFR** and must be promoted through the MCP promotion lifecycle per epic.md
§ Latent Defect Promotion, not left as prose.

### LD-1 — `ConfigureBreadcrumbDropDown` leaks the previous `BreadcrumbDropDownHost` when the WebView2 environment changes

`ItemViewer.Breadcrumb.cs:147-176`. The idempotence guard at `:147-153` returns early only when the
existing host is a concrete `BreadcrumbDropDownHost` **and** `ReferenceEquals(existing.Environment,
environment)`. When the environment reference differs, control falls through to `:158-168` and
constructs a **second** `BreadcrumbDropDownHost` over the same `_l0vhBreadcrumb_WebView2`. The first host
is never disposed by this file: `lifecycle.ConfigureHost` (`BreadcrumbItemViewerLifecycleCoordinator.cs:127-142`)
calls `ReleaseHostCore()`, which unsubscribes `PopupMessengerReady` and calls `coordinator.Release()`
(`:300-303`) — it does **not** call `IBreadcrumbDropDownHost.Dispose()`. `IBreadcrumbDropDownHost` is
`IDisposable` (`Viewers/IBreadcrumbDropDownHost.cs:19`), and the harness at
`BreadcrumbDropDownIntegrationTests.cs:308` asserts `host.Dispose()` is called exactly once **on viewer
disposal** — proving disposal is viewer-lifetime-scoped, not host-replacement-scoped. Every environment
change therefore leaks one WebView2-backed popup host for the lifetime of the viewer. Guaranteed to be
reachable in production: `QfcItemController.ViewerSetup.cs:166` passes `_webViewEnvironment`, which is
re-created per controller initialisation while `ItemViewer` instances are pooled and reused
(`ViewerSetup.cs:396` calls `ResetBreadcrumb()` on reuse, which does not reset the host identity).

### LD-2 — `SetBreadcrumbTheme` can be lost when issued off the UI thread immediately after `ConfigureBreadcrumbDropDown`

`ItemViewer.Breadcrumb.cs:197-198` forwards synchronously to
`BreadcrumbItemViewerLifecycleCoordinator.SetTheme` (`:155-160`), which reads
`DropDownHost` → `_openCoordinator?.Host`. But `_openCoordinator` is assigned **inside an asynchronous
post** (`ConfigureHost`, `:120-152`). On the UI thread the post runs inline
(`BreadcrumbUiDispatcher.cs:78-95`), so ordering holds and production is currently safe
(`QfcItemController.ViewerSetup.cs:166-167`). Off the UI thread — or after any `ConfigureAwait(false)`
resumption, which `BreadcrumbUiDispatcher.cs:263-268` explicitly documents as a real scenario — the post
is genuinely deferred, `DropDownHost` is still null when `SetTheme` reads it, and the popup surface keeps
the previous theme with no error surfaced. This is the same class of defect as the dark-mode stale-label
family (issues #254/#269).

### LD-3 — `InitializeBreadcrumbPipeline` silently discards a second, different `IFolderHierarchyProvider`

`ItemViewer.Breadcrumb.cs:45-48`. The guard returns without comparing providers, so a caller supplying a
different hierarchy provider to an already-initialised viewer gets no error and no effect. Pooled viewer
reuse (`QfcItemController.ViewerSetup.cs:140-146`) reaches this path: `EnsureBreadcrumbPipeline` guards on
`viewer.BreadcrumbCoordinator == null`, so a viewer reused across two controllers with different
`_globals.Ol.FolderTreeService` instances keeps the first controller's provider. Fail-fast (throw when the
provider differs) or explicit re-initialisation would both be defensible; today it is a silent stale-data
path. Contrast `BreadcrumbItemViewerLifecycleCoordinator.SetBridgeCoordinator` (`:66-69`), which does
compare by reference before short-circuiting — the coordinator is stricter than its own wrapper.

### LD-4 — `BreadcrumbCoordinator` initialisation is a non-atomic read-then-write

`ItemViewer.Breadcrumb.cs:45` reads and `:59` writes with no synchronisation and no memory barrier. Two
threads entering `InitializeBreadcrumbPipeline` concurrently both construct a
`BreadcrumbItemViewerLifecycleCoordinator` and a `BreadcrumbBridgeCoordinator`; one pair is silently
discarded **without being disposed**, leaking its `BreadcrumbMessengerHub` and its bridge subscriptions
(`BreadcrumbItemViewerLifecycleCoordinator.cs:73-76`). The same shape recurs at `:147/:159` (host) and
`:281/:287` (resource owner). Production currently calls only from the UI thread, so the window is not
known to be hit — but nothing in the type declares or enforces UI-thread affinity, and
`AttachBreadcrumbWebViewAsync` is `async`-facing, which invites off-thread callers.

### LD-5 — `EnsureBreadcrumbResourceOwnership` can create a `Container` that `Dispose` never disposes

`ItemViewer.Breadcrumb.cs:286-288` executes `components ??= new Container();` then
`components.Add(_breadcrumbResourceOwner)`. `ItemViewer.Designer.cs:16-23` disposes `components` only if
it is non-null **at the moment `Dispose(bool)` runs**. If breadcrumb configuration first occurs after
disposal has begun — reachable via the deferred `ConfigureHost` post
(`BreadcrumbItemViewerLifecycleCoordinator.cs:120`) racing `Control.Dispose` — the newly created
`Container` and the `BreadcrumbResourceOwner` inside it are never disposed, so
`DisposeBreadcrumbResources` (`:291-296`) never runs and the hub/messengers leak. The generation guard at
`coordinator:122-125` protects the coordinator's own state but not this file's container creation.

### LD-6 — Three `internal` members of this file have no production caller

`AttachBreadcrumbMessengerWhenReadyAsync` (`:100-124`), `AttachBreadcrumbMessenger` (`:126-140`), and
`BreadcrumbOpenTask` (`:29-30`) are invoked **only from tests**. [V] repo-wide grep for each identifier
returns exactly: the declaration in `ItemViewer.Breadcrumb.cs`, and call sites in
`QuickFiler.Test/Viewers/BreadcrumbCollapsedSurfaceReadinessTests.cs:438`,
`Viewers/BreadcrumbSubfolderActivationTests.cs:340`, `Viewers/BreadcrumbSelectorOpenRetryTests.cs:38,41,61,69,265`,
`Viewers/BreadcrumbCoordinatorLifecycleTests.cs:123`, and `Viewers/BreadcrumbDropDownIntegrationTests.cs:415-421`.
No `QuickFiler/**` production file references them. This is ~40 lines of production surface maintained
solely for tests. It is **not** a bug and F14 must not delete it (deleting would break seven existing
tests), but it is a design-debt item worth an issue: either promote these to the production attach path
(the `AttachCollapsedMessenger` route is arguably what `CreateCollapsedBreadcrumbCandidate` should use)
or mark them explicitly as test seams. **Flagging it also warns the planner that covering these three
members will show up as "already covered" in T0b** — they are not gap.

---

## 11. Summary of decisions the planner must make

1. **Settle the `ItemViewer.Designer.cs` exemption mechanism (§1.3) in `spec.md` before Phase 1.** This is
   the gating decision for all six ItemViewer partials.
2. **Measure before authoring (T0b).** The 49-case inventory in §7.3 is an upper bound, not a work order.
3. **Add exactly two seams (S-1, S-2), both injectable delegates, both as sibling overloads.** No retyping
   of `L0vhBreadcrumb_WebView2`.
4. **Defer the STA decision (C22) until after T0b.** If the file clears 80/75 without it, do not create
   the first `*.StaTests.cs` in `QuickFiler.Test`.
5. **Use `TimeProvider`/`FakeTimeProvider` if any clock need emerges** — it is available and is the repo
   standard. It is not needed for this file.
6. **Promote LD-1 … LD-6 as GitHub issues** before F14 completes.
