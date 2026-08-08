# quickfiler-breadcrumb-dropdown-webview-coverage — Spec

- **Issue:** #455
- **Parent:** epic #136 `quickfiler-per-file-coverage`, child F13
- **Owner:** drmoisan
- **Last Updated:** 2026-08-07
- **Status:** Draft
- **Version:** 1.0
- **Work Mode:** full-feature (this file and `user-story.md` are the authoritative acceptance-criteria sources)
- **Upstream dependency:** F1 (#432) `quickfiler-coverage-denominator-and-exemption-ledger`
- **Integration branch:** `epic/quickfiler-per-file-coverage-integration`

---

## 1. Objective

Bring the 15 compiled files of the QuickFiler breadcrumb drop-down surface and WebView2 host
(`QuickFiler/Viewers/`, ~3,111 lines) into F1's per-file coverage ledger with an honest, auditable
exemption boundary, and raise measured coverage where measured coverage is genuinely absent.

The file set does not have the shape of a typical coverage child. Twelve per-file research
artifacts under `research/` recomputed every figure independently from the committed Cobertura in
`docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml`.
Three facts govern the whole design:

1. **Eight of the eleven production files already clear both gates** (>= 80% line, >= 75% branch).
   Their requirement is *retain-or-improve* plus a bounded set of named residual outcomes. They are
   not under-covered and no requirement in this spec may imply that they are.
2. **The genuine work is concentrated in three places:**
   (a) `WebView2BreadcrumbHost.cs` and `WebView2Messenger.cs`, whose class-level
   `[ExcludeFromCodeCoverage]` attributes remove them from instrumentation entirely and which
   therefore start at zero measured coverage;
   (b) the `BreadcrumbPopupUiOperations.cs` exemption restructure (seven method-level attributes,
   one of them unjustified, and a 494/500-line file that must be split regardless);
   (c) roughly a dozen named residual branch outcomes spread across the eight passing files.
3. **Several outcomes are provably unreachable.** Per-file ceilings below 100% are recorded in §5
   and are binding. No acceptance criterion, and no plan task, may target 100% on a file with a
   stated ceiling.

## 2. Non-Goals

- **No observable behavior change** to any QuickFiler flow. Every latent defect found during
  research (§11) is promoted as a GitHub issue and is explicitly not fixed on this branch.
- **No edits to F12-owned files:** `BreadcrumbBridgeRouter.cs`, `BreadcrumbBridgeCoordinator.cs`,
  `BreadcrumbCoordinatorUpgradeLifetime.cs`, `BreadcrumbItemViewerLifecycleCoordinator.cs`,
  `BreadcrumbMessengerHub.cs`.
- **No edits to F14-owned files:** `ItemViewer.Breadcrumb.cs`, `ItemViewer.cs`, and the
  `ItemViewer*.Designer.cs` family.
- **No public or internal signature changes** to any of the 15 files. Six sibling children (F2, F9,
  F10, F12, F14, and the capstone F16) compile against them.
- **No retyping of any Designer field or Designer-backed property** (§9.4).
- **No STA infrastructure and no `*.StaTests.cs` file** (§8.3).
- **No injected clock, `TimeProvider`, or fake-timer facility** (§8.1).
- **No repository-wide threshold changes**, no `coverage.config` assembly excludes, and no edit to
  `UtilitiesCS/Properties/AssemblyInfo.cs`.
- **No convergence of the two WebView2 hosting paths** (`IBreadcrumbWebHost` vs `IWebViewMessenger`).
  Recorded as a post-epic candidate.

## 3. Scope — 15 files

All paths are under `QuickFiler/Viewers/`.

**Production (11):** `BreadcrumbDropDownHost.cs`, `BreadcrumbDropDownOpenLifetime.cs`,
`BreadcrumbDropDownOpenCoordinator.cs`, `BreadcrumbCollapsedSurfaceController.cs`,
`BreadcrumbUiDispatcher.cs`, `BreadcrumbWebViewSurfaceFactory.cs`, `BreadcrumbPopupPlacement.cs`,
`BreadcrumbPopupUiOperations.cs`, `WebView2BreadcrumbHost.cs`, `WebView2Messenger.cs`,
`WebView2CoreInitializer.cs`.

**Interface-only (4):** `IBreadcrumbDropDownHost.cs`, `IBreadcrumbWebHost.cs`,
`IWebViewCoreInitializer.cs`, `IWebViewMessenger.cs`.

**Files this child creates (5):** `BreadcrumbPopupProductionSurface.cs`,
`IBreadcrumbControlSurface.cs`, `WebView2ControlSurface.cs`, `IWebViewMessageChannel.cs`,
`CoreWebView2MessageChannel.cs`.

## 4. Upstream Contract from F1 and the Phase 0 Halt Gate

### 4.1 Current state of the dependency

`docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md` **does not exist on this
branch**. `Glob docs/features/epics/quickfiler-per-file-coverage/*` returns exactly one file,
`epic.md`. No F1 feature folder exists under `docs/features/active/`, and no per-file coverage
harness script exists — `scripts/vscode/Invoke-MSTestWithCoverage.ps1` is the pre-existing
whole-repository collector, not a per-file reporter.

### 4.2 Phase 0 halt gate (mandatory, blocking)

Execution begins with a Phase 0 gate that tests, from repository root, for the existence of:

```
docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md
```

If the ledger is absent, execution **halts** and does not enter Phase 1. The gate result is recorded
under `<FEATURE>/evidence/qa-gates/`. F1's per-file harness is a **soft** dependency with a
documented fallback: if the ledger exists but no harness script is published, per-file line and
branch rates are derived directly from the Cobertura produced by
`scripts/vscode/Invoke-MSTestWithCoverage.ps1`, applying the reading rules in §4.4.

### 4.3 What F13 consumes from the ledger

Three buckets, per `epic.md:509-522`:

| Bucket | Meaning | Gate |
|---|---|---|
| `testable` | Production logic that must be covered | >= 80% line, >= 75% branch; newly created files >= 90% line |
| `ratified-exempt` | Irreducible remainder argued against a ratified exemption ground; carries `[ExcludeFromCodeCoverage]` | Not measured; rationale row required |
| `interface-only / not-measured` | Zero coverable lines, no executable IL | Reported **N/A**, never 0%, never a failure, **no** `[ExcludeFromCodeCoverage]`; shape-assertion tests written purely to manufacture coverage are prohibited |

F13 also consumes the ledger's **classification rules** (not just its rows), because this child
creates five production files after the ledger is authored (`epic.md:572-587`, "Mid-Wave File
Creation"). Each created file appends its own ledger row in the same change that adds its
`<Compile Include>` entry.

### 4.4 Harness reading rules F13 requires (stronger than epic Directive A/B)

- Key on the Cobertura `filename=` attribute, **never** on `<class name=>`. Three F13 files prove
  the necessity: `BreadcrumbPopupPlacement.cs` reports as `…BreadcrumbPopupPlacementResult`,
  `BreadcrumbWebViewSurfaceFactory.cs` reports as `…BreadcrumbNavigationReadiness`, and
  `BreadcrumbDropDownOpenLifetime.cs` reports as `…BreadcrumbDropDownOpenLease`.
- Sum **class-level** `<lines>` children only, deduplicated by line number with `max(hits)`. Never
  sum `<method>` blocks and never read the `<class>` `line-rate` / `branch-rate` attributes: they
  are inflated by open issue **#441**. On `BreadcrumbPopupUiOperations.cs` the inflation is
  exactly +2.24 points line and +1.46 points branch (`0.929412 = 316/340` where the true figure is
  `234/258 = 90.70%`).
- Epic Directive A (union multiple `<class>` elements sharing one `filename`) is a **no-op** for
  this report writer — there is exactly one `<class>` per `filename` in both the #424 and #400
  reports. Implementing it is harmless; F13 budgets no work for it and must not assume it exists.

### 4.5 The escalated decision — the fourth exemption ground

`CLAUDE.md` §UT2 enumerates exactly three exemption grounds: (a) VSTO add-in lifecycle classes;
(b) WinForms form-derived classes and Designer-generated code; (c) Outlook Interop event-handler
classes depending on `Application` / `MailItem` / `Store` / `MAPIFolder` **without an injectable
seam**.

**None of the three covers any WebView2 file in this scope.** None is a VSTO lifecycle class, none
is form-derived or Designer-generated, and none imports a `Microsoft.Office.Interop.Outlook` type.
All three current class-level attributes therefore rest on a ground that does not textually exist.

**F13 cannot ratify this itself.** F1 must either:

- **(i)** ratify a narrow fourth ground — recommended wording: *"third-party SDK adapter types in
  which every member is a single call into a vendor-supplied API requiring a live external runtime
  process, a created window handle, or filesystem side effects to execute, where an interface seam
  over that adapter already exists and is consumed by non-exempt callers; the adapter must contain
  zero branches and zero mutable state, and the presence of either disqualifies it"* — or
- **(ii)** classify the affected files `testable` and accept documented per-file sub-threshold
  figures.

**F13's recorded position, contingent on (i):** remove the exemptions from
`WebView2BreadcrumbHost.cs` and `WebView2Messenger.cs` (both carry branches and mutable state and so
fail ground (d) on their own terms), and retain the exemption on `WebView2CoreInitializer.cs`
(two expression-bodied members, zero branches, zero state, and executing either member is
*prohibited*, not merely difficult — §7.3).

**If F1 rules otherwise (option ii):** `WebView2CoreInitializer.cs` is reclassified `testable`, its
attribute is removed, and its measured figure (approximately 33% line — one of three coverable lines,
the implicit constructor, covered by the existing smoke test) is recorded as a documented per-file
exception citing §UT4's temporary-file prohibition and external-process prohibition. The disposition
of `WebView2BreadcrumbHost.cs` and `WebView2Messenger.cs` is unaffected by either ruling: both are
de-exempted in all cases. Ground (d) is self-policing in exactly this way — it grants an exemption
only to a file already reduced to a remainder.

## 5. Per-File Target Table

Baselines are the recomputed figures in `issue.md` D1 (deduplicated class-level `<line>` children,
keyed on `filename`). "Ceiling" is the maximum reachable value given the structurally unreachable
outcomes proved in the research artifacts; a blank ceiling means 100% is reachable.

### 5.1 Existing production files

| File | Current line | Current branch | Line ceiling | Branch ceiling | Target line | Target branch | Classification |
|---|---:|---:|---:|---:|---:|---:|---|
| `BreadcrumbDropDownHost.cs` | 99.42% | 91.49% | — | ~97.9% | 100% (>= 99.42%) | >= 91.49%, goal ~97.9% | `testable` |
| `BreadcrumbDropDownOpenLifetime.cs` | 99.13% | 91.86% | **99.13%** | ~97.7% | >= 99.13% | >= 91.86%, goal ~97.7% | `testable` |
| `BreadcrumbDropDownOpenCoordinator.cs` | 98.25% | 92.05% | — | ~98.9% | 100% (>= 98.25%) | >= 92.05%, goal ~98.9% | `testable` |
| `BreadcrumbCollapsedSurfaceController.cs` | 98.97% | 85.71% | — | **95.24%** | 100% (>= 98.97%) | >= 85.71%, goal 95.24% | `testable` |
| `BreadcrumbUiDispatcher.cs` | 100% | 97.22% | — | **97.22%** | 100% | 97.22% (retain) | `testable` |
| `BreadcrumbWebViewSurfaceFactory.cs` | 99.29% | 97.62% | **99.29%** | **97.62%** | 99.29% (retain) | 97.62% (retain) | `testable` |
| `BreadcrumbPopupPlacement.cs` | 100% | 100% | — | — | 100% (retain) | 100% (retain) | `testable` |
| `BreadcrumbPopupUiOperations.cs` | 90.70% | 88.33% | **99.57%** | **99.17%** | >= 99.0% | >= 97.5% | `testable` |
| `WebView2BreadcrumbHost.cs` | **N/A (exempt, unmeasured)** | N/A | — | — | >= 90% | >= 80% | `testable` after de-exemption |
| `WebView2Messenger.cs` | **N/A (exempt, unmeasured)** | N/A | — | — | >= 90% | >= 80% | `testable` after de-exemption |
| `WebView2CoreInitializer.cs` | N/A (exempt, unmeasured) | N/A | n/a | n/a | N/A | N/A | `ratified-exempt` (contingent, §4.5) |

Notes on the two de-exempted files: research projects >= 95% line / >= 90% branch for both after the
seam extraction in §6.2 and §6.3 (~60 and ~70 coverable lines respectively, with zero permanently
uncovered residue). The binding targets above are set at >= 90% / >= 80% to leave margin; the
projections are the expected outcome, not the gate.

### 5.2 Interface-only files (existing)

| File | Classification | Reporting |
|---|---|---|
| `IBreadcrumbDropDownHost.cs` | `interface-only / not-measured` | N/A. Ledger rationale must read "interface + enum declaration, no executable IL" — lines 9-16 declare `public enum BreadcrumbDropDownCloseReason`, which emits no IL and produces no `<class>` element. |
| `IBreadcrumbWebHost.cs` | `interface-only / not-measured` | N/A |
| `IWebViewCoreInitializer.cs` | `interface-only / not-measured` | N/A |
| `IWebViewMessenger.cs` | `interface-only / not-measured` | N/A |

None receives `[ExcludeFromCodeCoverage]`. None is reported as 0%. No shape-assertion test may be
written for any of them for the purpose of manufacturing coverage.

### 5.3 Files created by this child

| File | Projected lines | Classification | Target |
|---|---:|---|---|
| `BreadcrumbPopupProductionSurface.cs` | ~110-125 | `ratified-exempt` (class-level attribute) | N/A |
| `IBreadcrumbControlSurface.cs` | ~50 | `interface-only / not-measured` | N/A, no attribute |
| `WebView2ControlSurface.cs` | ~95 | `ratified-exempt` (class-level attribute) | N/A |
| `IWebViewMessageChannel.cs` | ~40 | `interface-only / not-measured` | N/A, no attribute |
| `CoreWebView2MessageChannel.cs` | ~65 | `ratified-exempt` (class-level attribute) | N/A |

Any created file classified `testable` takes the >= 90% line new-file target
(`CLAUDE.md` §UT2, `epic.md:583-585`). None of the five is currently expected to be `testable`;
each carries a ledger rationale row instead.

## 6. Design

### 6.1 `BreadcrumbPopupProductionSurface.cs` — the exemption restructure

Create `QuickFiler/Viewers/BreadcrumbPopupProductionSurface.cs` as an `internal static class`
carrying a single **type-level** `[ExcludeFromCodeCoverage]`, holding exactly the members whose only
content is a third-party SDK call or WinForms presentation:

| Relocated member | Origin | Reason |
|---|---|---|
| `ShowOwnedPopup` | `:105-110` | WinForms popup presentation; showing a popup is a unit-test-policy violation |
| `CreateProductionControl` | `:380-381` | `new WebView2 { Dock = Fill }` |
| `BeginProductionInitialization` | `:383-388` | the `(WebView2)` cast |
| `ReadProductionCore` | `:390-392` | `WebView2.CoreWebView2` property read |
| `BeginProductionNavigation` | `:394-410` | carries lambdas at 406 and 409 |
| `BindProductionNavigation` | `:457-492` | carries the closure at 471-490 |
| *new* `NavigationBindingFor(BreadcrumbUiDispatcher)` | replaces the inline lambda at `:58` | removes the last lambda from the primary file's denominator |

**Critical constraint: `BreadcrumbPopupProductionSurface` must be a separate type, NOT a `partial`
of `BreadcrumbPopupUiOperations`.** An `[ExcludeFromCodeCoverage]` applied to one partial declaration
applies to the whole type, which would exempt all 234 currently-covered lines of
`BreadcrumbPopupUiOperations.cs`. That outcome is Blocking under `epic.md:223`.

Why a type-level attribute rather than the existing method-level precedent: `[ExcludeFromCodeCoverage]`
**does not propagate to nested lambda bodies** (issue #457). Measured proof, both directions, from
the same Cobertura report:

- Method-level does *not* suppress: `BreadcrumbPopupUiOperations.cs:394` and `:457` carry method-level
  attributes, yet source lines 406, 409 and 471-490 remain instrumented and permanently uncovered —
  23 of that file's 24 uncovered lines (adding line 58, inside the non-exempt constructor).
- Class-level *does* suppress: `WebView2Messenger.cs` contains four dispatcher lambdas under its
  class-level attribute at `:20` and produces **no `filename=` entry at all** in the report.

Consequences of the split, all three achieved by one move: 500-line compliance
(494 → ~417 lines, 77 removed); the lambda leak is fixed (lines 58, 406, 409, 471-490 leave the
denominator, taking line coverage from 90.70% to 99.57% with no new test); and the exemption boundary
becomes one file, one attribute, one ledger row instead of seven scattered attributes that do not do
what they claim.

Call sites rebound by the move: the production constructor (`:52-60`), `NavigateToDocument` (`:438`),
and `BreadcrumbDropDownHost.cs:74` — the only call site of `ShowOwnedPopup`, an F13-owned file.

### 6.2 `WebView2BreadcrumbHost` seam — `IBreadcrumbControlSurface` + `WebView2ControlSurface`

New `internal interface IBreadcrumbControlSurface` (`QuickFiler/Viewers/IBreadcrumbControlSurface.cs`):

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

New `internal sealed class WebView2ControlSurface : IBreadcrumbControlSurface` with a **class-level**
`[ExcludeFromCodeCoverage]`, holding the `WebView2` control and the two bridge `EventHandler` fields
needed for idempotent unhook/hook. Every member is a single statement with zero branches and zero
state.

`WebView2BreadcrumbHost.cs` (143 → ~190 lines): remove the class-level attribute at `:29`; keep the
existing `public WebView2BreadcrumbHost(WebView2, IWebViewCoreInitializer)` signature as a
production-wiring constructor carrying a **method-level** `[ExcludeFromCodeCoverage]`; add a
non-exempt `internal` seam constructor taking `(IWebViewCoreInitializer, IBreadcrumbControlSurface,
Func<string> resolveCacheFolder)`; extract `HandleInitializationCompleted(bool, Exception?)` and
`RaiseMessageReceived(string)` as non-exempt internal members; keep
`ResolveProductionCacheFolder()` non-exempt (`Environment.GetFolderPath` + `Path.Combine` create no
file, so §UT4 is not engaged). `IsCoreInitialized`, `MessageReceived`, `CoreInitialized`,
`NavigateToString`, `PostMessageJson`, and `InitializeAsync` keep their exact existing signatures.

`InitializeAsync` is already fully testable today with no refactor, behind the `IWebViewCoreInitializer`
seam already injected into the constructor plus a fake `SynchronizationContext` — which is what makes
the current class-level attribute an exemption on a testable seam.

### 6.3 `WebView2Messenger` seam — `IWebViewMessageChannel` + `CoreWebView2MessageChannel`

New `internal interface IWebViewMessageChannel` (`QuickFiler/Viewers/IWebViewMessageChannel.cs`):

```csharp
internal interface IWebViewMessageChannel
{
    void Subscribe(Action<string> onPayload);   // idempotent registration of one inbound sink
    void Unsubscribe();                         // detaches the registration made by Subscribe
    void PostJson(string json);                 // forwards one outbound JSON payload
}
```

The interface is host-neutral by construction: no WebView2 type appears in any signature. New
`internal sealed class CoreWebView2MessageChannel : IWebViewMessageChannel` with a **class-level**
`[ExcludeFromCodeCoverage]`, wrapping one `CoreWebView2` and holding the bridging
`EventHandler<CoreWebView2WebMessageReceivedEventArgs>` field.

`WebView2Messenger.cs` (147 → ~165 lines): remove the class-level attribute at `:20`; keep both
existing constructor signatures byte-compatible (both are called from production at
`BreadcrumbPopupUiOperations.cs:409` and `ItemViewer.Breadcrumb.cs:85`); add a non-exempt
`internal WebView2Messenger(BreadcrumbUiDispatcher, IWebViewMessageChannel)` seam constructor, a pure
non-exempt `internal static string ExtractPayload(Func<string> tryGetString, Func<string> readJson)`,
and a non-exempt `internal void HandleInboundPayload(string)`.

Exactly five SDK statements move into the adapter: `WebMessageReceived +=` (`:46`),
`PostWebMessageAsJson` (`:66`), `WebMessageReceived -=` (`:86`), `TryGetWebMessageAsString()`
(`:114`), and `WebMessageAsJson` (`:119`/`:121`). The *decision* logic around the last two — the
`catch (ArgumentException)` fallback and the independent `?? e.WebMessageAsJson` coalesce — stays in
`WebView2Messenger` as a pure static so it remains measured. Moving it into the adapter would be
exactly the "testable logic hiding behind an exemption" failure the epic prohibits.

### 6.4 The exception-fidelity trap (requires an explicit regression test)

Today `WebView2Messenger.cs:38` throws `ArgumentNullException("coreWebView")` **before** `:39` throws
`ArgumentNullException("dispatcher")`. If the internal 2-arg constructor is chained naively as
`: this(dispatcher, CreateProductionChannel(coreWebView))`, C# evaluates constructor arguments left to
right, so a call with **both** arguments null would report `"dispatcher"` instead of `"coreWebView"` —
a silent behavior change in exception fidelity.

Required mitigation: order the chained arguments so `coreWebView` is evaluated first, or place both
guards in a single static factory. **An explicit regression test is required**
(`InternalConstructor_BothArgumentsNull_ReportsCoreWebViewFirst`, asserting
`.WithParameterName("coreWebView")`). Parameter names must remain `"coreWebView"` and `"dispatcher"` —
not `"core"`, and not the adapter's own parameter names.

A second guard-order contract in the same file must also be pinned: `PostJson(null)` after `Dispose()`
throws `ArgumentNullException`, **not** `ObjectDisposedException`, because the null guard at `:57-60`
precedes `ThrowIfDisposed()` at `:61`.

### 6.5 `WebView2CoreInitializer` — no new seam

The seam this file exists to serve already exists and is already correct: `IWebViewCoreInitializer`,
consumed by five callers across four epic children (F9, F10, F13, F14). Interposing a second seam
beneath it would relocate the same two SDK calls one layer deeper and leave an identically-untestable
innermost file — a strictly worse outcome. Folding it into either path's local adapter would change
contracts three sibling children compile against. **No new production file for this type; no
`QuickFiler.csproj` change for it.**

Its existing test file is relocated (§9.6) and strengthened into a seam-contract test asserting
construction, interface assignability, both seam signatures by reflection, adapter/seam member parity,
presence of the exemption attribute, and sealedness. No test may invoke `CreateEnvironmentAsync` or
`EnsureCoreWebView2Async`; a plan task proposing one is a policy violation and must be rejected.

## 7. Exemption Disposition Table

All ten `[ExcludeFromCodeCoverage]` attributes in scope: 3 class-level + 7 method-level.

| # | Location | Level | Disposition | Ground cited |
|---|---|---|---|---|
| 1 | `WebView2BreadcrumbHost.cs:29` | class | **REMOVE** | None of §UT2 (a)/(b)/(c) applies. The type has 2 constructor guards, a state transition, a failure branch, an idempotent hook pair, and a null-core drop branch. Its own doc comment's "1:1 SDK forwarding" claim is refuted in four places. `InitializeAsync` is testable today behind the already-injected `IWebViewCoreInitializer`. Exemption on a testable seam = Blocking (`epic.md:223`). |
| 2 | `WebView2Messenger.cs:20` | class | **REMOVE** | None of §UT2 applies. Only **five** of roughly 70 coverable lines are SDK statements; the rest is disposal gating (`Interlocked`/`Volatile`), four disposal-race guards, four null guards, two independent payload fallbacks, subscription bookkeeping, and a 9-line testable static (`CaptureProductionDispatcher`). |
| 3 | `WebView2CoreInitializer.cs:15` | class | **KEEP** (contingent on F1 ratifying ground (d), §4.5) | Two expression-bodied members, zero branches, zero state, three coverable lines. Executing either is *prohibited*, not merely hard: `CreateEnvironmentAsync` creates and populates a user-data folder on disk (§UT4 bans temp files, approved exceptions: none) and requires the Evergreen WebView2 Runtime (external process, §UT4 + §UT1 determinism); `EnsureCoreWebView2Async` additionally needs a created window handle and starts a browser process. The doc comment's "1:1 forwarding" rationale is false (it drops the SDK's `browserExecutableFolder` parameter) and must be restated (issue #477). |
| 4 | `BreadcrumbPopupUiOperations.cs:105` `ShowOwnedPopup` | method | **KEEP**, relocated to `BreadcrumbPopupProductionSurface` (class-level) | WinForms host-bound presentation. `ToolStripDropDown.Show` displays a window and `Control.PointToClient` forces handle creation; epic Shared Design §2 states unit tests "never show popups". Zero decision logic; the seam already exists as `Action<ToolStripDropDown, Control, Point> showPopup` at `BreadcrumbDropDownHost.cs:86`. |
| 5 | `BreadcrumbPopupUiOperations.cs:380` `CreateProductionControl` | method | **KEEP**, relocated | `new WebView2 { Dock = Fill }`. Any test could only restate the object initializer — a shape assertion with no defect-detection value (`epic.md:521-522` in spirit). The real seam is `Func<Control> _createControl`. |
| 6 | `BreadcrumbPopupUiOperations.cs:383` `BeginProductionInitialization` | method | **KEEP**, relocated (contingent) | The `(WebView2)control` cast only; `IWebViewCoreInitializer` is already mockable. May be reclassified `testable` if `FormatterServices.GetUninitializedObject(typeof(WebView2))` proves stable (the `Component` finalizer is an unproven risk on .NET Framework). **Numerically zero-sum** — the body is excluded either way — so this contingency must not block the plan. |
| 7 | `BreadcrumbPopupUiOperations.cs:390` `ReadProductionCore` | method | **KEEP**, relocated | Cast plus `WebView2.CoreWebView2` property read; zero decision logic. The behavior worth pinning (null-core diagnostic) lives in the non-exempt `ReadCoreAsync(Func<WebCore>)` at `:150-154` and is already covered. |
| 8 | `BreadcrumbPopupUiOperations.cs:394` `BeginProductionNavigation` | method | **KEEP**, relocated | Two lambdas calling `WebView2.NavigateToString` and constructing `WebView2Messenger` over a live core. Relocation removes its leaked lambda lines 406 and 409 from the denominator. |
| 9 | `BreadcrumbPopupUiOperations.cs:412` `DisposeProductionSurface` | method | **REMOVE** — stays in the primary file with no attribute | **No ground exists.** Signature is `(Control?, IWebViewMessenger?)`; no WebView2 type appears anywhere in the member; it forwards to the non-exempt, already-tested `DisposeTwoResources`. Decisive evidence of reachability: its two lambda bodies at source lines 415 and 416 already report `hits="1"` — existing tests execute it end to end. Blocking under `epic.md:223`. |
| 10 | `BreadcrumbPopupUiOperations.cs:457` `BindProductionNavigation` | method | **KEEP**, relocated | Subscribes three `CoreWebView2` navigation events and unwraps two SDK event-arg types that are sealed with no public constructor. Relocation removes leaked lambda lines 471-490. Extracting an `INavigationEventSource` was considered and rejected: the arg types are unconstructible, so the translation lambdas would merely move, and the interface would have exactly one implementation. |

**Net effect:** the exempt member count falls from 7 to 6 in `BreadcrumbPopupUiOperations`, the
class-level exemption count falls from 3 to 1 (plus 3 new class-level-exempt adapter types that
contain no decision logic), and — for the first time — every exempt line actually leaves the
denominator.

## 8. Determinism Requirements

### 8.1 No clock, no fake timers

There is **no** `DateTime`, `Stopwatch`, `Timer`, `Task.Delay`, `Thread.Sleep`, or `TimeProvider`
anywhere in the drop-down lifetime files, and none in `BreadcrumbPopupUiOperations.cs`. Determinism
here is **scheduler** control, not clock control. Any plan task that introduces an injected clock or a
fake-timer facility is out of scope and must be rejected — it would add a seam with no dependency to
control. This supersedes the "injected clock and fake timers" phrasing carried forward from the
potential entry.

### 8.2 The deterministic vehicle

A manually-pumped fake `SynchronizationContext` with an **explicit `Drain()`** call. The pattern is
already green in-repo at `BreadcrumbPopupUiOperationsDirectAdapterTests.cs:274-300`
(`QueuedCreatorThreadSynchronizationContext` + `DrainOnCreatorThread()`) and
`BreadcrumbPopupControlDispatchTests.cs:249-279` (`SurfaceFactoryFixture.Drain(Task, int workLimit)`).
Asynchronous edges elsewhere are driven by caller-supplied `Task` / `TaskCompletionSource` instances.

Dispatcher semantics that tests must account for: `BreadcrumbUiDispatcher.Dispatch` runs **inline**
when `IsCurrentBoundary()` is true and otherwise `Post`s. With MSTest's default
`SynchronizationContext.Current == null`, `new BreadcrumbUiDispatcher(fakeQueue, sink)` posts and the
test must `Drain()`; `CreateForCurrentThreadTests()` runs inline.

### 8.3 Prohibited in every F13 test

- `Thread.Sleep`, `Task.Delay`, any wall-clock wait, any real-time polling.
- Temporary files or any filesystem write (`CLAUDE.md` §UT4; approved exceptions: none).
- External services, external processes, network access, the Evergreen WebView2 Runtime.
- Live/shown forms and popups. `ToolStripDropDown.Show` must never be called.
- STA threads and `*.StaTests.cs` files. No proposed test constructs a WinForms control that needs
  one; `BreadcrumbPopupControlDispatchTests.cs` is a plain `[TestClass]` already constructing `Panel`,
  `ToolStripDropDown` and `ToolStripControlHost` in memory, and is green.
- Constructing `Microsoft.Web.WebView2.WinForms.WebView2` or
  `CoreWebView2WebMessageReceivedEventArgs` in any test. `CoreWebView2` and
  `CoreWebView2Environment` may be produced by `FormatterServices.GetUninitializedObject` and used
  **only as opaque tokens that are never dereferenced**.
  `CoreWebView2InitializationCompletedEventArgs` has a public `(Exception)` constructor and may be
  constructed directly.
- Mutable static state in test fixtures. `scripts/vscode/TaskMaster.cli.runsettings` sets MSTest
  `Parallelize Workers=0 Scope=ClassLevel`, so test classes run concurrently. Any test that sets an
  ambient `SynchronizationContext` must restore it in a `finally`.

### 8.4 Required test conventions

MSTest `[TestClass]` / `[TestMethod]`, Moq for mocks and stubs, FluentAssertions for assertions,
Arrange–Act–Assert, one clearly named scenario per test, tests under `QuickFiler.Test/Viewers/`
mirroring the production tree.

## 9. Constraints

### 9.1 500-line limit — production and test

No production file, test file, or reusable script may exceed 500 lines. The binding pressure is:

- `BreadcrumbPopupUiOperations.cs` at **494/500** (6 lines of headroom) — the split in §6.1 is
  mandatory regardless of the coverage work and reduces it to ~417.
- `BreadcrumbDropDownHost.cs` 480, `BreadcrumbDropDownOpenLifetime.cs` 477 — no production change is
  required for any recommended test case on either file, so **no partial split should be proposed**
  for them.
- **Thirteen F13-relevant test files sit within 25 lines of the limit**, including
  `BreadcrumbDropDownIntegrationTests.cs` at exactly **500** (zero headroom),
  `BreadcrumbDropDownHostTests.cs` at 499 and `BreadcrumbDropDownReadinessTests.cs` at 498.
  Essentially every new test case must go into a **new** test file. The repository already
  establishes the `.Part2.cs` convention for this.

### 9.2 Both `.csproj` files are non-SDK explicit-include with CRLF

`QuickFiler/QuickFiler.csproj` (121 `<Compile Include>` entries, 593 lines, **every line
CRLF-terminated**) and `QuickFiler.Test/QuickFiler.Test.csproj` (107 entries, also CRLF, also an
explicit list) use no globbing. Rules:

- One `<Compile Include="Viewers\<NewFile>.cs" />` per created production file, placed **inside the
  existing F13 block at lines 396-411**. No property changes, no reference changes, no reordering.
- One entry per created test file, inside the existing breadcrumb block at
  `QuickFiler.Test.csproj:58-91`.
- **Preserve CRLF.** Use the `Edit` tool, or `perl -0777` with explicit `\r\n`. A git-bash `sed -i`
  strips CRLF and produces a whole-file diff guaranteed to conflict with F12, whose entries at
  393-395 and 400 are interleaved with F13's. Fan-in conflicts on both files are expected and are
  resolved additively (keep both sides).

### 9.3 New production files require a ledger row

Each created production file appends its own coverage-ledger row **in the same change** that adds its
`<Compile Include>` entry. New files default to `testable` at >= 90% line; claiming
`ratified-exempt` or `interface-only / not-measured` requires a written rationale meeting a ratified
ground.

### 9.4 The Designer field must not be retyped

`ItemViewer.Designer.cs:6214` declares
`internal Microsoft.Web.WebView2.WinForms.WebView2 _l0vhBreadcrumb_WebView2;` (instantiated at `:46`,
named at `:206`). It is pinned by a **live, green** reflection test at
`QuickFiler.Test/Viewers/ItemViewerBreadcrumbDropDownContractTests.cs:18-29`
(`ExistingAnchor_RemainsTheDesignerWebViewClosedSurface`), which asserts that
`typeof(QuickFiler.ItemViewer).GetProperty("L0vhBreadcrumb_WebView2").PropertyType` is exactly
`Microsoft.Web.WebView2.WinForms.WebView2`.

**No task in this child may retype a Designer field or a Designer-backed property.** The working
pattern is to inject the host/router beside the control, never to change the control's declared type.
`ItemViewer.Designer.cs` must remain byte-identical.

### 9.5 Additional pinned contracts that must not be broken

Also in `ItemViewerBreadcrumbDropDownContractTests.cs`:

- `:31-49` — `ItemViewer.ConfigureBreadcrumbDropDown(CoreWebView2Environment, IWebViewCoreInitializer)`
  must exist.
- `:51-74` — `ItemViewer.ConfigureBreadcrumbDropDown(IBreadcrumbDropDownHost, Func<Rectangle>,
  Func<Rectangle>)` must exist; the injected-host seam is contractual.
- `:102-130` — `BreadcrumbDropDownOpenCoordinator` must remain `internal`, must **not** carry
  `[ExcludeFromCodeCoverage]`, and `ItemViewer` must not declare `OpenBreadcrumbDropDownAsync`. Any
  attempt to exempt the coordinator fails an existing test.

Treat every `public` / `internal` signature in the 15 files as **frozen**.

### 9.6 Sibling boundaries

- **F12 owns:** `BreadcrumbBridgeRouter.cs`, `BreadcrumbBridgeCoordinator.cs`,
  `BreadcrumbCoordinatorUpgradeLifetime.cs`, `BreadcrumbItemViewerLifecycleCoordinator.cs`,
  `BreadcrumbMessengerHub.cs`.
- **F14 owns:** `ItemViewer.Breadcrumb.cs`.
- `BreadcrumbPopupLifecycleOperations` and `BreadcrumbNavigationSubscription` are declared **inside**
  F12's `BreadcrumbItemViewerLifecycleCoordinator.cs`, at lines **355** and **337** respectively.
  F13 calls into them from `BreadcrumbPopupUiOperations.cs:401`, `:414`, `:466`. Tests exercising
  those members credit F12's file, not ours. The §6.1 split moves two of the three call sites (401,
  466) into the exempt file, leaving only `:414` measured — the dependence is reduced, not deepened.
  If F12 splits its file for the 500-line rule, a pure file move is source-compatible with no edit on
  F13's side.
- **In-scope structural corrections (D13):**
  - Relocate `QuickFiler.Test/Controllers/WebView2CoreInitializerTests.cs` to
    `QuickFiler.Test/Viewers/` to satisfy the mirror-layout rule in
    `.claude/rules/general-unit-test.md` § Test File Location. In `QuickFiler.Test.csproj`, remove
    the entry at line 150 and add `Viewers\WebView2CoreInitializerTests.cs` to the breadcrumb block.
  - `BreadcrumbPopupPlacementTests.cs:138-155` anchors reflection on
    `typeof(BreadcrumbBridgeCoordinator)` — an **F12-owned** type used only as an assembly handle.
    This is a cross-child compile coupling invisible to a file-set disjointness check. Re-anchor it on
    an F13-owned type.

### 9.7 Measurement environment

Repository-wide figures must be captured **before and after within the same session on the same
branch**, running the full `*.Test.dll` set (`-SearchRoot '.'`). Comparing against a number inherited
from another feature folder is unsound: #424's own evidence records the denominator growing 38.6%
(79,957 → 110,849 valid lines) between two full-suite runs. The epic's "70.19% merge-base" figure is
#424's merge base, not a current baseline, and must not be used as a target.

Run the harness from **this worktree root**. `Invoke-MSTestWithCoverage.ps1`'s discovery filter
excludes `\obj\` and `\ref\` but not `.claude\worktrees\`, so a run from the main repository root
picks up stale agent-worktree assemblies.

## 10. In-Scope Non-Coverage Items (D13)

1. Remove the unjustified `[ExcludeFromCodeCoverage]` on
   `BreadcrumbPopupUiOperations.DisposeProductionSurface` (`:412`) — a Blocking finding.
2. Relocate `QuickFiler.Test/Controllers/WebView2CoreInitializerTests.cs` to `Viewers/`.
3. Re-anchor `BreadcrumbPopupPlacementTests.cs:138-155` off the F12-owned
   `BreadcrumbBridgeCoordinator` and onto an F13-owned type.

## 11. Documented Deviations (D1-D13)

The epic manifest and the original delegation brief both contain claims that research disproved. The
following are authoritative over both where they conflict.

**D1 — The branch-coverage premise is refuted.** All eight instrumented files already clear both the
80% line floor and the 75% branch floor. Evidence: recomputed per-file figures in §5.1; the lowest
branch figure in scope is 85.71% (`BreadcrumbCollapsedSurfaceController.cs`), 10.7 points above the
floor. The acceptance bar for these eight is **retain-or-improve**, not gap closure.

**D2 — The exemption count is understated.** `BreadcrumbPopupUiOperations.cs` carries **seven**
method-level attributes (lines 105, 380, 383, 390, 394, 412, 457), not one file-level attribute. The
in-scope total is **3 class-level + 7 method-level = 10**, not four. The epic's `[X]` marker on that
file at `epic.md:418` is wrong — the file has no type-level attribute and is fully instrumented.

**D3 — `[ExcludeFromCodeCoverage]` does not propagate to nested lambdas (issue #457).** Method-level
attributes leak nested lambda bodies into the denominator as permanently uncovered lines; class-level
attributes do not. Evidence: 23 of `BreadcrumbPopupUiOperations.cs`'s 24 uncovered lines are this
defect (58, 406, 409, 471-490); by contrast `WebView2Messenger.cs`'s four lambdas are absent from the
report entirely under its class-level attribute at `:20`. The convention for this child is therefore
**class-level-exempt adapter types**, deviating from the method-level precedent in that same file.

**D4 — One exemption in scope is unjustified and is a Blocking finding.**
`BreadcrumbPopupUiOperations.DisposeProductionSurface` (`:412`) touches no WebView2 type — its
signature is `(Control?, IWebViewMessenger?)` — and its lambda bodies at 415-416 already report
`hits="1"`. Existing tests execute it. Per `epic.md:223` the attribute must be removed.

**D5 — `CLAUDE.md` §UT2's three exemption grounds do not cover any WebView2 file.** None is a VSTO
lifecycle class, none is form-derived or Designer-generated, and none imports an Outlook Interop type.
All three current class-level attributes rest on a ground that does not textually exist. **F1 (#432)
must ratify a narrow fourth ground or classify them testable with a documented exception. F13 cannot
ratify this itself.** See §4.5.

**D6 — Per-file exemption verdicts.** `WebView2Messenger.cs` — remove; only ~5 of ~70 coverable lines
are SDK statements. `WebView2BreadcrumbHost.cs` — remove; `InitializeAsync` is already testable today
behind the `IWebViewCoreInitializer` seam already injected into its constructor.
`WebView2CoreInitializer.cs` — retain; executing either member is *prohibited* (filesystem side
effects and an external runtime process), not merely hard. Its stated "1:1 forwarding" rationale is
false and must be restated (issue #477).

**D7 — No injected clock or fake timers are required.** There is no `DateTime`, `Stopwatch`, `Timer`,
`Task.Delay`, `Thread.Sleep`, or `TimeProvider` anywhere in the drop-down lifetime files. Determinism
here is scheduler control. The vehicle already exists and is green at
`BreadcrumbPopupUiOperationsDirectAdapterTests.cs:274-300`.

**D8 — No STA is required anywhere in this child.** `BreadcrumbPopupControlDispatchTests.cs` is a
plain `[TestClass]` already constructing `Panel`, `ToolStripDropDown`, and `ToolStripControlHost` in
memory. No proposed test constructs a WinForms control.

**D9 — Provably unreachable outcomes; the plan must not target 100%.**

| File | Outcome | Consequence |
|---|---|---|
| `BreadcrumbCollapsedSurfaceController.cs:245-246` | `IsCurrent`'s c2/c3 operands, unreachable because `InvalidateGeneration` is the sole atomic writer of both fields | branch ceiling **95.24%** |
| `BreadcrumbUiDispatcher.cs:276` | unreachable across all 24 construction sites | branch ceiling **97.22%** |
| `BreadcrumbWebViewSurfaceFactory.cs:221-222` | Roslyn `catch { await …; throw; }` rewrite artifact | ceilings **99.29% line / 97.62% branch** |
| `BreadcrumbDropDownOpenLifetime.cs:359` (and `:260` second `&&` operand) | leave-target of a catch that always rethrows; short-circuit precedence | line ceiling **99.13%** |
| `BreadcrumbPopupUiOperations.cs:325` and half of `:324` | `await` inside `catch` (issue-457 class) | ceilings **99.57% line / 99.17% branch** |
| `BreadcrumbDropDownHost.cs:420` (`_disposed == true`) | lease invalidated before the queued lambda can run | branch ceiling ~97.9% |
| `BreadcrumbDropDownOpenCoordinator.cs:241-242` | `CloseCore` released guard, unreachable through the public surface | branch ceiling ~98.9% |

No test may be authored for any row above. Each is recorded on the irreducible-outcome record instead.

**D10 — Conflict risk #400 is resolved.** #400 merged as PR #416 on 2026-08-04; commit `294132b4` is
an ancestor of HEAD. It authored all 15 F13 files, and its committed coverage report matches #424's
byte-for-byte per file across two distinct full-suite runs ten days apart. Open issue **#440**
(breadcrumb arrow-key navigation) is a live behavior bug in adjacent territory, in no active folder,
and is out of scope.

**D11 — New harness directive for F1 (stronger than epic Directive B).** Key on `filename=`, never
`<class name=>`, and sum **class-level** `<lines>` children only. Proven three times in this file set
(`BreadcrumbPopupPlacement.cs` → `…BreadcrumbPopupPlacementResult`, undercount 91.7% if keyed on the
`<methods>` block; `BreadcrumbWebViewSurfaceFactory.cs` → `…BreadcrumbNavigationReadiness`, omitting
the static factory type entirely; `BreadcrumbDropDownOpenLifetime.cs` → `…BreadcrumbDropDownOpenLease`).
Epic Directive A (union multiple `<class>` per filename) is a **no-op** for this writer.

**D12 — Latent defects promoted, not fixed here.** #457, #458, #462, #475, #476, #477. All are
behavior changes barred by the no-behavior-change non-functional requirement.

**D13 — In-scope items beyond coverage.** See §10.

**Additional corrections carried from the research artifacts:**

- The epic's "merge-base repository line rate of 70.19%" is #424's merge-base measurement, not a
  current baseline; the same document's post-change row reads 85.65% line / 79.00% branch and warns
  the two are not like-for-like (§9.7).
- Two existing F13-primary test files have material headroom
  (`BreadcrumbPopupUiOperationsDirectAdapterTests.cs` 198 lines,
  `BreadcrumbPopupBoundaryCoverageTests.cs` 139 lines), contradicting the blanket "every test file is
  full" premise. New files are still the recommendation, for fan-in isolation rather than headroom.
- The "two parallel WebView2 hosting paths" split is **EfcViewer form vs ItemViewer**, not "docked vs
  drop-down". `WebView2BreadcrumbHost` has exactly one construction site in the repository
  (`EfcFormController.cs:836`); `WebView2Messenger` serves both the ItemViewer collapsed surface and
  the drop-down popup.
- `WebView2BreadcrumbHost` and `WebView2Messenger` have **zero** test references anywhere in
  `QuickFiler.Test/`. `Controllers/WebView2CoreInitializerTests.cs` (25 lines) contributes zero
  coverage because its target type is exempt.

## 12. Acceptance Criteria

Each criterion is individually verifiable with numeric or textual evidence committed under
`docs/features/active/2026-08-07-quickfiler-breadcrumb-dropdown-webview-coverage-455/evidence/qa-gates/`.

- [ ] **AC-1 (Phase 0 halt gate).** Before any production or test edit, the existence of
      `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md` is tested from repository
      root and the result recorded as evidence. If the file is absent, execution halts at Phase 0 and
      no Phase 1 task runs.
- [ ] **AC-2 (fourth-ground ratification).** F1's ledger records an explicit exemption ground
      covering third-party SDK adapter types (proposed ground (d), §4.5), or an explicit ruling
      declining it. F13's evidence records which ruling applied and the resulting classification of
      `WebView2CoreInitializer.cs`. If ground (d) is declined, that file is reclassified `testable`,
      its attribute removed, and its measured line rate recorded with the §UT4 prohibition citations.
- [ ] **AC-3 (per-file measurement).** A per-file coverage report covering all 11 existing production
      files plus all 5 created files is committed, showing line and branch for each, computed by
      keying on Cobertura `filename=` and summing deduplicated class-level `<line>` children with
      `max(hits)`. No figure in the report is taken from a `<class>` `line-rate` or `branch-rate`
      attribute.
- [ ] **AC-4 (retain-or-improve for the eight already-passing files).** Measured line and branch for
      each of `BreadcrumbDropDownHost.cs` (99.42% / 91.49%), `BreadcrumbDropDownOpenLifetime.cs`
      (99.13% / 91.86%), `BreadcrumbDropDownOpenCoordinator.cs` (98.25% / 92.05%),
      `BreadcrumbCollapsedSurfaceController.cs` (98.97% / 85.71%), `BreadcrumbUiDispatcher.cs`
      (100% / 97.22%), `BreadcrumbWebViewSurfaceFactory.cs` (99.29% / 97.62%),
      `BreadcrumbPopupPlacement.cs` (100% / 100%), and `BreadcrumbPopupUiOperations.cs`
      (90.70% / 88.33%) is **greater than or equal to** the stated baseline. No file regresses on
      either metric.
- [ ] **AC-5 (`BreadcrumbPopupUiOperations.cs`).** After the §6.1 extraction and the residual-branch
      tests, the file measures **>= 99.0% line and >= 97.5% branch** (projected 99.57% / 99.17%;
      ceiling 99.57% / 99.17%). Its line count is <= 420.
- [ ] **AC-6 (`WebView2BreadcrumbHost.cs` de-exemption).** The class-level attribute at `:29` is
      removed, the file appears in the coverage report with a `filename=` entry, and it measures
      **>= 90% line and >= 80% branch**.
- [ ] **AC-7 (`WebView2Messenger.cs` de-exemption).** The class-level attribute at `:20` is removed,
      the file appears in the coverage report with a `filename=` entry, and it measures
      **>= 90% line and >= 80% branch**.
- [ ] **AC-8 (`DisposeProductionSurface` exemption removal, D4).** The `[ExcludeFromCodeCoverage]` at
      `BreadcrumbPopupUiOperations.cs:412` is removed and the member stays in the primary file. Both
      previously uncovered condition halves at source lines **415** and **416** are covered (4/4
      conditions across the two lines) by tests driven through the production constructor.
- [ ] **AC-9 (`BreadcrumbPopupProductionSurface.cs` extraction).** The new file exists as an
      `internal static class` with exactly one type-level `[ExcludeFromCodeCoverage]`. It is **not**
      declared `partial` and shares no type identity with `BreadcrumbPopupUiOperations`. A test
      asserts by reflection that `typeof(BreadcrumbPopupUiOperations)` carries **no**
      `ExcludeFromCodeCoverageAttribute`. Source lines 58, 406, 409 and 471-490 no longer appear in
      the primary file's Cobertura `<lines>` block.
- [ ] **AC-10 (WebView2 seams).** `IBreadcrumbControlSurface.cs` + `WebView2ControlSurface.cs` and
      `IWebViewMessageChannel.cs` + `CoreWebView2MessageChannel.cs` exist. Each adapter carries
      exactly one class-level `[ExcludeFromCodeCoverage]`; neither interface carries any coverage
      attribute. No WebView2 type appears in any `IWebViewMessageChannel` member signature.
- [ ] **AC-11 (interface-only classification).** `IBreadcrumbDropDownHost.cs`, `IBreadcrumbWebHost.cs`,
      `IWebViewCoreInitializer.cs`, `IWebViewMessenger.cs`, `IBreadcrumbControlSurface.cs`, and
      `IWebViewMessageChannel.cs` are classified `interface-only / not-measured`, carry no
      `[ExcludeFromCodeCoverage]`, and are reported **N/A** rather than 0%. No shape-assertion test is
      added for any of them for the purpose of manufacturing coverage. The
      `IBreadcrumbDropDownHost.cs` ledger rationale reads "interface + enum declaration, no executable
      IL".
- [ ] **AC-12 (exception fidelity, §6.4).** A regression test asserts that the internal two-argument
      `WebView2Messenger` constructor called with both arguments null throws `ArgumentNullException`
      with `ParamName == "coreWebView"`, and a second test asserts that `PostJson(null)` after
      `Dispose()` throws `ArgumentNullException` (parameter `"json"`), not `ObjectDisposedException`.
- [ ] **AC-13 (frozen signatures and pinned contracts).** No `public` or `internal` signature in the
      15 in-scope files changes. `ItemViewer.Designer.cs` is byte-identical to its pre-change state,
      `_l0vhBreadcrumb_WebView2` remains typed `Microsoft.Web.WebView2.WinForms.WebView2`, and all
      tests in `QuickFiler.Test/Viewers/ItemViewerBreadcrumbDropDownContractTests.cs` pass unmodified.
- [ ] **AC-14 (determinism).** No test added or modified by this child contains `Thread.Sleep`,
      `Task.Delay`, a wall-clock wait, a temporary file, an external service or process, a shown form,
      a popup, an STA attribute, an injected clock, or a `TimeProvider`. Every asynchronous edge is
      driven by a manually-pumped fake `SynchronizationContext` with an explicit `Drain()` or by a
      test-owned `TaskCompletionSource`. Any test setting an ambient `SynchronizationContext` restores
      it in a `finally`.
- [ ] **AC-15 (500-line limit).** Every production file and every test file created or modified by
      this child is <= 500 lines, verified by a committed line-count listing.
- [ ] **AC-16 (csproj mechanics).** Each created production file has exactly one
      `<Compile Include="Viewers\…" />` entry inside the F13 block of `QuickFiler/QuickFiler.csproj`
      (lines 396-411) and each created test file has one entry inside the breadcrumb block of
      `QuickFiler.Test/QuickFiler.Test.csproj` (lines 58-91). Both files remain **CRLF-terminated on
      every line** (verified and recorded), with no property change, no reference change, and no
      reordering of unrelated entries.
- [ ] **AC-17 (ledger rows for created files).** Each of the five created production files appends its
      own coverage-ledger row in the same change as its `<Compile Include>` entry, with its bucket and
      rationale. Any created file classified `testable` measures >= 90% line.
- [ ] **AC-18 (test relocation, D13).** `WebView2CoreInitializerTests.cs` lives at
      `QuickFiler.Test/Viewers/WebView2CoreInitializerTests.cs`; the `Controllers\…` entry at
      `QuickFiler.Test.csproj:150` is removed and a `Viewers\…` entry added; no test in the file
      invokes `CreateEnvironmentAsync` or `EnsureCoreWebView2Async`.
- [ ] **AC-19 (cross-child anchor removal, D13).** `BreadcrumbPopupPlacementTests.cs` no longer
      references `BreadcrumbBridgeCoordinator` or any other F12-owned type; its assembly-handle
      reflection is anchored on an F13-owned type.
- [ ] **AC-20 (scope containment).** `git diff --name-only` against the merge base lists no path
      outside: the 15 in-scope files, the 5 created production files, `QuickFiler/QuickFiler.csproj`,
      `QuickFiler.Test/QuickFiler.Test.csproj`, files under `QuickFiler.Test/Viewers/`, the epic
      coverage ledger, and this feature folder. No F12-owned or F14-owned file is modified.
- [ ] **AC-21 (no behavior change).** The complete pre-existing `QuickFiler.Test` suite passes with no
      assertion weakened, disabled, or deleted. The production diff consists only of the attribute
      removals in AC-6/AC-7/AC-8, the member relocations in §6.1-§6.3, and additive non-exempt seam
      members. No latent defect from §11/D12 is fixed on this branch.
- [ ] **AC-22 (full toolchain green).** In the final pass, in this order and with no step failing or
      modifying files: `csharpier .`; `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug
      /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`;
      `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable
      /p:TreatWarningsAsErrors=true`; `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`.
      The commands and their results are recorded as evidence.
- [ ] **AC-23 (repository-wide retain-or-improve).** Repository-wide line and branch coverage are
      measured **before and after in the same session on this branch**, over the full `*.Test.dll` set
      run from this worktree root, and the after-figures are >= the before-figures. No figure
      inherited from another feature folder is used as the baseline.
- [ ] **AC-24 (unreachable residue recorded; no 100% targets).** An irreducible-outcome record is
      committed naming every unreachable outcome in D9 with its proof, and no acceptance criterion or
      plan task targets 100% on any file carrying a stated ceiling.
- [ ] **AC-25 (latent defects deferred, not fixed).** The latent defects identified in research are
      tracked as GitHub issues (#457, #458, #462, #475, #476, #477 are already promoted) and the
      branch diff contains no change that alters the behavior any of them describes.

## 13. Definition of Done

- [ ] All 25 acceptance criteria in §12 are checked off with committed evidence.
- [ ] `user-story.md` acceptance criteria are checked off.
- [ ] Coverage evidence, toolchain evidence, and the irreducible-outcome record are committed under
      `<FEATURE>/evidence/qa-gates/`.
- [ ] The coverage ledger carries a row for every F13 file, existing and created.
- [ ] The working tree is clean and every audit-trail artifact is committed.
