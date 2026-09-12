# Per-File Research: `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs`

- Epic: #136 QuickFiler Per-File 80% Coverage — child F10 (`quickfiler-item-controller-coverage`, issue #453)
- Branch: `feature/quickfiler-item-controller-coverage`
- Worktree: `C:\Users\DanMoisan\repos\TaskMaster\.claude\worktrees\agent-a359b62de7a79b16e`
- Production file: `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` (426 lines, verified)
- Research date: 2026-08-07

> Measurement basis, the union-vs-`line-rate` correction, the `QuickFiler.Test.csproj` no-globbing
> finding, and the cross-fixture constraint on `IQfcItemController` are established once in
> `file-QfcItemController.md` §0, §9.2 and §10. Read that artifact first.

---

## 0. Headline

| Metric | Value | Floor | Verdict |
| --- | --- | --- | --- |
| Line coverage (`line-rate` attribute) | 74.37% | >= 80% | **FAIL** |
| Line coverage (recomputed from `<line>` children) | 116/160 = **72.5%** | >= 80% | **FAIL** |
| Branch coverage (`branch-rate` attribute) | 56% | >= 75% | **FAIL** |
| Branch coverage (recomputed) | 30/54 = **55.6%** | >= 75% | **FAIL** |
| `[ExcludeFromCodeCoverage]` members | **3** (lines 38, 132, 253) | 0 preferred | FAIL — epic AC2 |
| File size | 426 / 500 | <= 500 | PASS, 74 lines headroom |

**Correction to the delegation brief.** The brief flags only the 74.4% line figure. **Branch coverage
is the worse failure at 56% against a 75% floor** — a 19-point shortfall versus the line gate's
5.6-point shortfall. Epic.md "Coverage-Target Reconciliation" makes these independent gates and
requires both to be reported. This is the only file in F10's assignment that fails both.

The brief's structural figures are confirmed: 426 lines, three method-level exemptions at exactly
lines 38, 132 and 253.

---

## 1. Member inventory

`internal partial class QfcItemController` (line 26). One field, thirteen methods, no properties, no
events, no nested types.

| Lines | Member | Accessibility | Exempt? | Callers |
| --- | --- | --- | --- | --- |
| 28 | `_breadcrumbViewer` (field, `ItemViewer`) | private | n/a | `EnsureBreadcrumbPipeline`, `Cleanup` |
| 39-125 | `Task InitializeWebViewAsync()` | `internal async` | **YES (38)** | `Initialization.cs:193, 255, 286, 321`; `EventHandlers` WebView path |
| 81-102 | *(anonymous)* `WebResourceRequested` handler lambda | compiler-generated, inside the above | **NO** — see §3.2 | WebView2 event |
| 133-158 | `void EnsureBreadcrumbPipeline()` | `internal` | **YES (132)** | `InitializeWebViewAsync:107` |
| 161-168 | `void ConfigureBreadcrumbDropDown(ItemViewer, CoreWebView2Environment)` | `internal` | No | `ConfigureAndAttachBreadcrumbAsync:179` |
| 171-181 | `Task<bool> ConfigureAndAttachBreadcrumbAsync(ItemViewer, CoreWebView2Environment, Func<Task<bool>>)` | `internal` (not `async`) | No | `InitializeWebViewAsync:113` |
| 183-189 | `void OnBreadcrumbUnhandledArrow(object, BreadcrumbArrowDirection)` | `private` | No | Event handler wired at 152, 155-156, 399 |
| 194-202 | `string ResolveImageMimeType(string)` | `private static`, expression-bodied `switch` | No | The lambda at line 95 only |
| 205-249 | `void ResolveControlGroups(ItemViewer)` | `internal` | No — de-exempted cycle-5 (see comment at 204) | `Initialization.cs:172, 264, 297` |
| 254-307 | `Task ResolveControlGroupsAsync(ItemViewer)` | `internal async` | **YES (253)** | `Initialization.cs:207` |
| 309-314 | `void PopulateControls(MailItem, int)` | `public` | No | `Initialization.cs:182` |
| 316-320 | `void PopulateControls(MailItemHelper, int)` | `public` (on `IQfcItemController:72`) | No | `QfcCollectionController` |
| 322-336 | `Task PopulateControlsAsync(MailItem, int, bool)` | `internal async` | No | `Initialization.cs:224, 314` |
| 338-352 | `Task AssignControlsAsync(MailItemHelper, int)` | `internal async` | No | `PopulateControlsAsync:335` |
| 354-390 | `void AssignControls(MailItemHelper, int)` | `internal` | No | `PopulateControls` x2, `AssignControlsAsync:350` |
| 392-421 | `void Cleanup()` | `public` (on `IQfcItemController:77`) | No | `QfcCollectionController` |
| 423-424 | `string GetItemSummary()` | `internal`, expression-bodied | No | **None. Dead code** — see §7.4 |

---

## 2. What is already covered

Two fixtures cover this file.

**`QuickFiler.Test/Controllers/QfcItemController.ViewerSetupTests.cs`** (407 lines,
`QfcItemController_ViewerSetupTests`, 9 tests) — uses `Mock<IItemViewer>`, `Mock<MailItem>`,
`Mock<IApplicationGlobals>`, a real running WPF dispatcher on a dedicated STA thread
(`StartRunningDispatcher()`), and one headless real `QuickFiler.ItemViewer`.

**`QuickFiler.Test/Controllers/QfcItemControllerBreadcrumbDropDownTests.cs`** (386 lines,
`QfcItemControllerBreadcrumbDropDownTests`, 6 tests) — introduces the reusable
`ViewerScope : IDisposable` (lines 365-383), which installs a `SynchronizationContext`, constructs a
headless `QuickFiler.ItemViewer`, and restores the context on dispose.

| Member | Status | Covering test | Measured |
| --- | --- | --- | --- |
| `ConfigureBreadcrumbDropDown` | COVERED | `ConfigureBreadcrumbDropDown_PassesExistingEnvironmentAndDarkThemeLazily` (:25), `_LightThemeUsesSameControllerSetupSeam` (:61), `_RepeatedSameEnvironmentReusesPopupHost` (:92) | 100% line / 100% branch |
| `ConfigureAndAttachBreadcrumbAsync` | **PARTIALLY COVERED** | `ConfigureAndAttachBreadcrumbAsync_CachesCurrentThemeAndCreatesOneCandidatePerSession` (:188) | 83.3% line / 50% branch — the `ArgumentNullException` arm is untaken |
| `OnBreadcrumbUnhandledArrow` | **PARTIALLY COVERED** | `OnBreadcrumbUnhandledArrow_ForViewer_RoutesOnceToKeyboardHandler` (:156) — covers both arms of the `sender is ItemViewer` test | 100% line / 75% branch — the null-`_kbdHandler` arm is untaken |
| `ResolveControlGroups` | COVERED | `ResolveControlGroups_WithHeadlessItemViewer_PopulatesConcreteControlCollections` (ViewerSetupTests:379) | 100% / 100% |
| `PopulateControls(MailItem,int)` | COVERED | `PopulateControls_WithMailItem_ConstructsHelperAndAssignsControls` (ViewerSetupTests:166) | 100% |
| `PopulateControls(MailItemHelper,int)` | COVERED | `PopulateControls_WithHelper_StoresHelperAndAssignsViewerFields` (ViewerSetupTests:136) | 100% |
| `PopulateControlsAsync` | COVERED | `PopulateControlsAsync_WithMailItem_LoadsHelperViaFromMailItemAsyncAndAssignsControls` (ViewerSetupTests:196) | lines 327, 330, 332, 335, 336 all hit |
| `AssignControlsAsync` | COVERED | `AssignControlsAsync_DispatchesAssignThroughViewerDispatcher` (ViewerSetupTests:300) | lines 339, 349-352 hit |
| `AssignControls` | COVERED | `AssignControls_WhenNotInvokeRequired_WritesAllIntentMembersFromSettings` (:229), `_WhenTaskFlagUnset_SetsCancelDialogResult` (:257), `_WhenInvokeRequired_MarshalsViaInvoke` (:278) | 100% line / 100% branch, both `InvokeRequired` arms and both `IsTaskFlagSet` arms |
| `Cleanup` | COVERED | `Cleanup_NullsTrackedPrivateFields` (ViewerSetupTests:338), `Cleanup_ResetsInjectedHostForPooledViewerReuse` (BreadcrumbDropDownTests:125) | 100% line / 100% branch |
| `ResolveImageMimeType` | **UNCOVERED** | none | 0% line / 0% branch (0 of 12 conditions) |
| `GetItemSummary` | **UNCOVERED** | none | 0% |
| `InitializeWebViewAsync` | UNCOVERED (exempt) | none | not instrumented; **its lambda is** |
| `EnsureBreadcrumbPipeline` | UNCOVERED (exempt) | none | not instrumented |
| `ResolveControlGroupsAsync` | UNCOVERED (exempt) | none | not instrumented; **its lambdas are** |

Verified by grepping the whole of `QuickFiler.Test/` for `ResolveImageMimeType`, `GetItemSummary`,
`EnsureBreadcrumbPipeline`, `ResolveControlGroupsAsync`, `InitializeWebViewAsync` — zero hits.

**Nine of the thirteen methods are already well covered. Do not duplicate any of them.** This file's
deficit is concentrated in four places, itemised next.

---

## 3. The gap list

### 3.1 The 44 uncovered lines

| Lines | Count | Construct | Enclosing member | Enclosing exempt? |
| --- | --- | --- | --- | --- |
| 82-86, 89-92, 95-102 | **17** | `WebResourceRequested` lambda body — URI parse, content-id map, MIME lookup, response construction | `InitializeWebViewAsync` | Yes (38) |
| 178 | 1 | `throw new ArgumentNullException(nameof(attachCollapsed));` | `ConfigureAndAttachBreadcrumbAsync` | No |
| 195-202 | **8** | `ResolveImageMimeType` switch expression, all arms | (own method) | No |
| 276, 281, 286-291, 294-299, 302-303, 306 | **17** | Lambda bodies: two `SelectAwait` projections, two `ForEach` predicates, two LINQ `Where`/`Select` projections | `ResolveControlGroupsAsync` | Yes (253) |
| 424 | 1 | `GetItemSummary` interpolated string | (own method) | No |
| **Total** | **44** | | | |

### 3.2 Structural finding: `[ExcludeFromCodeCoverage]` does not cover lambdas declared inside the exempt method

**34 of the 44 uncovered lines (77%) sit inside methods that carry the attribute.** The attribute
suppresses instrumentation of the declaring method's own body but not of the compiler-generated
closure methods it declares. Direct evidence in the report: the class carries an explicit
`<method ... name="&lt;InitializeWebViewAsync&gt;b__122_0" line-rate="0" branch-rate="0" complexity="6">`
entry with 17 lines at `hits="0"` (report lines 23720-23752), even though `InitializeWebViewAsync`
itself is absent from the report.

Consequence for planning: **the two exempt async members are already costing this file 34 uncovered
lines whether or not their attributes are removed.** The attribute is not buying what its author
assumed. Any strategy that leaves those lambdas in place cannot reach 80%.

### 3.3 The 24 uncovered branch conditions

| Line | Conditions | Covered | Construct |
| --- | --- | --- | --- |
| 83 | 2 | 0 | `Segments.LastOrDefault()?` null-conditional (lambda) |
| 84 | 2 | 0 | `string.IsNullOrEmpty(requestedId)` (lambda) |
| 90 | 2 | 0 | `!contentIdMap.TryGetValue(...)` (lambda) |
| 177 | 2 | 1 | `attachCollapsed == null` — only the non-null arm taken |
| 187 | 2 | 1 | `_kbdHandler?.` — only the non-null arm taken |
| **195** | **12** | **0** | `ResolveImageMimeType` switch with six arms — **the single largest branch cluster in the file** |
| 287 | 2 | 0 | `x.ColumnNumber == navColNum` (lambda) |
| 295 | 2 | 0 | `x.ColumnNumber == navColNum` (lambda) |
| **Totals** | **54 in file** | **30** | **55.6%** |

`ResolveImageMimeType` alone accounts for 12 of the 24 uncovered conditions. Covering that one
8-line pure static function moves branch coverage from 55.6% to **42/54 = 77.8%**, which **clears the
75% branch floor by itself**. It is by a wide margin the highest-value single item in F10.

### 3.4 What it takes to clear the line floor

Working from 116/160 = 72.5%, with no production change:

| Step | Lines gained | Running total | Line % |
| --- | --- | --- | --- |
| Cover `ResolveImageMimeType` | +8 | 124/160 | 77.5% |
| Cover `GetItemSummary` | +1 | 125/160 | 78.1% |
| Cover line 178 (null-argument throw) | +1 | 126/160 | 78.8% |

**Test-only work plateaus at 78.8% — below the 80% floor.** The remaining 34 uncovered lines are all
lambda bodies inside exempt async members, so **a production change is unavoidable for this file.**
That is the central planning conclusion, and it distinguishes ViewerSetup.cs from the other two F10
files analysed, neither of which needs a production edit.

---

## 4. Seam analysis

### 4.1 Barriers, verified against source

| Barrier | Evidence | Real? |
| --- | --- | --- |
| **A — concrete `(ItemViewer)` cast** (lines 66, 76, 109, 112, 135, and the `ItemViewer` parameters at 161, 171, 205, 254) | Headless `new QuickFiler.ItemViewer()` already runs in this very fixture pair — `ViewerSetupTests.cs:386` and the `ViewerScope` at `QfcItemControllerBreadcrumbDropDownTests.cs:365-383`, six tests deep | **DEFEATED** |
| **B — `await _itemViewer.UiSyncContext`** (lines 55, 265) with no WinForms message loop | `ItemViewer.cs:23-30`: `InitializeComponent()` runs at line 25 and `_context = SynchronizationContext.Current` is captured at line 26, i.e. *after* WinForms has installed a `WindowsFormsSynchronizationContext`. Its `Post` continuation requires a running loop | **REAL**, but defeasible test-side — see §4.3 |
| **C — WebView2 core initialization** | Line 76 dereferences `((ItemViewer)_itemViewer).L0v2h2_WebView2.CoreWebView2` **outside** the injected `IWebViewCoreInitializer` seam (`IWebViewCoreInitializer.cs:13-29` abstracts only `CreateEnvironmentAsync` and `EnsureCoreWebView2Async`). A non-null `CoreWebView2` requires the Edge WebView2 runtime to actually initialize — an external process dependency, prohibited by `.claude/rules/general-unit-test.md` § External Dependencies | **REAL and irreducible** |
| **D — `CoreWebView2WebResourceRequestedEventArgs`** (lambda parameter, line 81) | A sealed WebView2 SDK type with no interface; `e.Response` can only be assigned a `CoreWebView2WebResourceResponse` produced by `CoreWebView2Environment.CreateWebResourceResponse` | **REAL** for the lambda's two outer lines; **not real** for its logic — see §4.2 |
| **E — `OutlookFolderHierarchyProvider` construction** (lines 142-144) from `_globals.Ol.FolderTreeService` | The provider is a snapshot facade over `IOutlookFolderTreeService`; the existing tests already call `viewer.InitializeBreadcrumbPipeline(Mock<IFolderHierarchyProvider>)` headlessly (`QfcItemControllerBreadcrumbDropDownTests.cs:131, 193, 284`), proving the downstream path works with no live Outlook | **NOT A BARRIER**, subject to a plan-time check that `new OutlookFolderHierarchyProvider(mockService)` issues no COM call |
| **F — `QfcTipsDetails.CreateAsync(label, syncContext, token)`** (lines 258, 276, 281) | Takes a `SynchronizationContext` as a parameter — already a seam by injection | **NOT A BARRIER** once B is handled |

### 4.2 Recommended seam (the one production change): host-neutral extraction of the CID image resolver

**Problem.** 17 uncovered lines and 6 uncovered branch conditions sit in the `WebResourceRequested`
lambda (81-102), which is unreachable because it is registered inside a method whose barrier (C) is
irreducible. The lambda's *logic*, however, touches nothing host-bound: it parses a URI string,
builds a content-id map from `IAttachment[]`, and picks a MIME type. Only two of its lines actually
require WebView2 types.

**This is precisely the case `.claude/rules/general-unit-test.md` § Coverage Exclusion Policy
describes: "extract all logic into host-neutral, testable modules and leave only the thinnest
possible wiring in the host-bound entry point."** It is also the epic Non-Goals' stated preference:
*"Where a seam choice is open, prefer host-neutral extraction that a future WebView2/Office.js port
can reuse."* A `cid:` → `(bytes, mimeType)` resolver is exactly the kind of function an Office.js
port would reuse verbatim.

**Seam hierarchy check.** No interface seam is warranted (there is one implementation and no
polymorphism). No injectable delegate is warranted (there is no collaborator to substitute). No
adapter is warranted (nothing is being wrapped). The correct construct is a **pure static function
plus a small return DTO** — below the bottom of the hierarchy, because no seam is needed at all once
the logic stops living inside a host-bound closure. This is the minimum change.

**Proposed shape** — new file `QuickFiler/Controllers/QfcCidImageResolver.cs`:

```
internal static class QfcCidImageResolver
{
    internal static QfcCidImage Resolve(string requestUri, IAttachment[] attachments);  // null when unresolvable
    internal static string ResolveMimeType(string fileExtension);                       // moved from ViewerSetup:194-202
}

internal sealed class QfcCidImage   // or a plain readonly struct
{
    internal QfcCidImage(byte[] data, string mimeType) { ... }
    internal byte[] Data { get; }
    internal string MimeType { get; }
}
```

The lambda at 81-102 collapses to roughly:

```
coreWebView2.WebResourceRequested += (sender, e) =>
{
    var resolved = QfcCidImageResolver.Resolve(e.Request.Uri, ItemHelper.AttachmentsInfo);
    if (resolved is null) { return; }
    e.Response = _webViewEnvironment.CreateWebResourceResponse(
        new MemoryStream(resolved.Data), 200, "OK", $"Content-Type: {resolved.MimeType}");
};
```

Notes and constraints:

- `UtilitiesCS.CidImageResolver.BuildContentIdMap` (already unit-tested at
  `UtilitiesCS.Test/OutlookObjects/MailItem/CidImageResolverTests.cs:61-69`) stays where it is and is
  **called**, not moved. No `UtilitiesCS` edit — consistent with epic.md Cross-Child Constraints §2
  ("build a local seam in the child's own assignment").
- `QuickFiler.csproj` declares `<TargetFrameworkVersion>v4.8.1</TargetFrameworkVersion>` with
  `<LangVersion>preview</LangVersion>` (`QuickFiler/QuickFiler.csproj:13-14`). On .NET Framework,
  `record`, `record struct`, and `init`-only setters require an `IsExternalInit` shim. Shim
  references exist elsewhere in the solution (`UtilitiesCS/OutlookObjects/Folder/FolderScore.cs`,
  `UtilitiesCS/OutlookObjects/Store/StoreIdentity.cs`), but whether one is reachable from
  `QuickFiler` must be verified before use. **Safe default: a plain sealed class or `readonly struct`
  with an ordinary constructor. Do not use `record` or `init`.**
- The extraction is **behavior-preserving**: the same URI is parsed, the same map is built at request
  time (preserving the pooled-viewer comment at 71-75), the same MIME defaults apply. No observable
  QuickFiler behavior changes.

### 4.3 Second recommended change: de-exempt `EnsureBreadcrumbPipeline` (line 132)

Barriers present: A (defeated) and E (not a barrier). Nothing else. The method is 26 lines of pure
state management — a type test, a null check, a reference comparison, and an event-subscription swap.
Its exemption comment (lines 127-131) claims it is *"Skipped for mock viewers (unit tests drive the
coordinator directly through its own seams)"*, which describes the early-return branch rather than a
barrier.

**Minimum seam: none.** The existing `ViewerScope` plus `Mock<IOlObjects>.FolderTreeService` reaches
every line and every branch. Remove the attribute and cover it.

### 4.4 Third change, optional: de-exempt `ResolveControlGroupsAsync` (line 253)

Barriers: A (defeated), B (real), F (not a barrier). B is the only obstacle, and it is defeasible
**test-side** without any production change: after constructing the headless viewer, reflection-set
its private `_context` field (`ItemViewer.cs:59`) to a plain `new SynchronizationContext()`, whose
`Post` queues to the thread pool and resumes normally. This is the same established reflection
technique already used in this test project for `Theme._uiDispatcher`
(`QfcItemController.TestSupport.cs:174-176, 204-209`) and `UiThread._dispatcher` (`:240-249`).

Value: closes the remaining 17 lambda lines and 4 branch conditions. **Not required** to clear either
floor once §4.2 and §4.3 land (see §8 projection), so it is scheduled as optional.

### 4.5 The one exemption that should be retained

`InitializeWebViewAsync` (line 38). After §4.2, its residual body is: a cancellation guard, a path
join, an options construction, `await _itemViewer.UiSyncContext` (barrier B), two calls through the
injected `IWebViewCoreInitializer`, a **direct `.CoreWebView2` dereference at line 76 (barrier C)**,
a filter registration, the thin lambda, and the breadcrumb attach. Barrier C is irreducible: a
non-null `CoreWebView2` requires the Edge WebView2 runtime, an external process dependency the unit
test policy forbids.

Its comment (lines 30-37) must be rewritten, because the reason it currently gives — the concrete
`L0v2h2_WebView2` cast — is not the operative barrier. Accurate rationale:

> Residual: line 76 dereferences `.CoreWebView2` directly, outside the injected
> `IWebViewCoreInitializer` seam. A non-null `CoreWebView2` requires the Edge WebView2 runtime to
> initialize — an external process dependency prohibited by `.claude/rules/general-unit-test.md`
> § External Dependencies. The method additionally awaits `_itemViewer.UiSyncContext`, a
> `WindowsFormsSynchronizationContext` captured after `InitializeComponent()` (`ItemViewer.cs:25-26`)
> whose continuation requires a running WinForms message loop. All host-neutral logic previously in
> this method has been extracted to `QfcCidImageResolver`; what remains is WebView2 wiring only.

**No STA-constructed WinForms control is proposed anywhere in this file.** Both existing
headless-`ItemViewer` tests run in plain `[TestClass]`/`[TestMethod]` with no STA attribute, so the
epic's `*.StaTests.cs` last-resort clause is not engaged.

---

## 5. State-transition invariants

| # | Invariant | Source | Pinned today? | Pin with |
| --- | --- | --- | --- | --- |
| **VS-1** | `EnsureBreadcrumbPipeline` is **idempotent**: a second call with the same viewer must not rebuild the coordinator | Guard at 140 (`viewer.BreadcrumbCoordinator == null`) | No | **C3** |
| **VS-2** | **Subscription symmetry / no double-subscribe:** when the viewer changes, the *old* viewer is unsubscribed (152) before `_breadcrumbViewer` is reassigned (154), and the *new* viewer is defensively `-=` then `+=` (155-156) | 148-157 | No | **C3, C4** |
| **VS-3** | **Dispose ordering:** `ResetBreadcrumb()` (396) and the unsubscribe (399) must run **before** `_itemViewer` is nulled (403) | 396-403 | Yes | `Cleanup_ResetsInjectedHostForPooledViewerReuse` |
| **VS-4** | **Dispose-before-setup:** after `Cleanup()`, `_itemViewer` is null, so a subsequent `EnsureBreadcrumbPipeline()` must take the early return at 135-138 and not throw | 135-138 vs 403 | No | **C5** |
| **VS-5** | **Idempotent dispose:** `Cleanup()` called twice must not throw. Second pass: `(null as ItemViewer)?.ResetBreadcrumb()` is a no-op, `_breadcrumbViewer` is already null so 397-401 is skipped, and every remaining assignment is a null-to-null write | 392-421 | No | **A4** |
| **VS-6** | **Marshalling guard:** `AssignControls` must marshal through `Invoke` **and return** when `InvokeRequired` is true, never falling through to write twice | 357-361 | Yes | `AssignControls_WhenInvokeRequired_MarshalsViaInvoke` |
| **VS-7** | **Theme-before-attach ordering:** `ConfigureBreadcrumbDropDown` (179) must run **before** `attachCollapsed()` (180) so the active theme is cached before collapsed navigation can complete | 176-181 | Yes | `ConfigureAndAttachBreadcrumbAsync_CachesCurrentThemeAndCreatesOneCandidatePerSession` |
| **VS-8** | **Argument contract:** `ConfigureAndAttachBreadcrumbAsync` must reject a null `attachCollapsed` **before** performing any configuration side effect | 177-179 | No | **A1** |
| **VS-9** | **Request-time map rebuild:** the content-id map is built inside the handler (89), not at registration, so it always reflects the mail item currently loaded into the pooled viewer (documented at 71-75) | 89 | No | **B4** (via the extracted resolver taking attachments as a parameter, which structurally preserves this) |

### Ordering, re-entrancy, dispose-before-setup — explicit

- **Ordering:** VS-3 and VS-7 are pinned; VS-2 and VS-8 are not (C4, A1).
- **Re-entrancy:** VS-1, VS-2 and VS-5 are the re-entrancy surface. `EnsureBreadcrumbPipeline` is
  called from `InitializeWebViewAsync` (107), which can run more than once per pooled viewer, so its
  idempotence and subscription symmetry are load-bearing and currently unpinned. Covered by C3, C4
  and A4.
- **Dispose-before-setup:** VS-4 — calling `EnsureBreadcrumbPipeline()` after `Cleanup()` must be a
  safe no-op. Covered by C5. This is the file's dispose-before-setup case.

---

## 6. Determinism requirements

- **Wall-clock reads: none.** No `DateTime.Now`, `DateTime.UtcNow`, `DateTime.Today`, `Stopwatch`, or
  `Environment.TickCount` in this file (grep over `QuickFiler/Controllers/QfcItemController*.cs`).
  `GetItemSummary` (424) formats `ItemHelper.SentDate` — a value read from the helper, not the clock.
- **Randomness: none.**
- **`Thread.Sleep` / `Task.Delay` / real waits in production: none in this file.** The one family-wide
  hit is `QfcItemController.EventWiring.cs:135`, out of scope here.
- **Banned-API finding in scope: none.** No test proposed below needs `Thread.Sleep`, `Task.Delay`, a
  fake timer, or a `FakeTimeProvider`.
- **Timer:** `Cleanup()` writes `_emailIsReadTimer = null` at line 420. The timer is armed with a
  4000 ms due time at `QfcItemController.Navigation.cs:223-224`. **No test proposed here arms it**, so
  no wall-clock dependency enters. The defect is recorded in §7.1 but is not exercised.
- **Culture sensitivity — a real determinism risk for one proposed test.** Line 424 uses
  `ToString("MM/dd/yyyy")` and `ToString("HH:mm")` with **no** `CultureInfo` argument, so the
  rendered separators follow the ambient culture. A test asserting a literal string could pass on the
  developer machine and fail on a differently-configured runner. **Mitigation for A3: assert using
  the same culture-dependent format calls, or assert on stable substrings (subject, sender), rather
  than hard-coding `"01/15/2026"`.** Do **not** mutate `Thread.CurrentThread.CurrentCulture` — that is
  mutable global state and violates `.claude/rules/general-unit-test.md` § External Dependencies.
- **Thread pool:** `AssignControlsAsync` (349) and `PopulateControlsAsync` (335) dispatch through
  `_itemViewer.UiDispatcher`. The existing tests handle this deterministically with
  `StartRunningDispatcher()` / `ShutdownDispatcher()` (`TestSupport.cs:297-326`), awaiting the
  dispatched task rather than polling. New tests must reuse that helper, not invent a wait.

---

## 7. Latent defects for promotion

Report only; do not fix under this child.

### 7.1 `Cleanup()` nulls `_emailIsReadTimer` without disposing it — **Moderate**

`ViewerSetup.cs:420` — `_emailIsReadTimer = null;` with no `Dispose()`. The sibling path
`QfcItemController.Navigation.cs:211-214` disposes correctly before discarding, and `:223-224` arms
the timer for 4000 ms with callback `ApplyReadEmailFormat`. If `Cleanup()` runs inside that window —
which pooled-viewer recycling makes likely — the `System.Threading.Timer` stays rooted by its
callback and fires on a thread-pool thread against a controller whose `_itemViewer`, `_globals`,
`ItemHelper` and `_mailItem` were just nulled (lines 402-418), a probable `NullReferenceException` on
a background thread plus a per-item finalizer leak. Also recorded in `file-QfcItemController.md` §7.1
because the field is declared there.

### 7.2 Unguarded `new Uri(...)` inside a WebView2 event handler — **Low/Moderate**

`ViewerSetup.cs:83` — `new Uri(e.Request.Uri)` throws `UriFormatException` on a malformed URI. The
lambda is a `WebResourceRequested` handler, so the exception surfaces on the WebView2 callback with
no `try`/`catch` anywhere in the chain. The registered filter (77-80) makes a malformed URI unlikely
but not impossible. The §4.2 extraction makes this trivially guardable — but **guarding it would be a
behavior change**, so it must be characterised (test B7), not fixed, under this child.

### 7.3 Unguarded `new MemoryStream(match.AttachmentData)` — **Low**

`ViewerSetup.cs:97`. `IAttachment.AttachmentData` is nullable (`UtilitiesCS.Test` has an explicit
`AttachmentData_WhenSetToNull_ReturnsNullFromGetter` test), and `new MemoryStream(null)` throws
`ArgumentNullException`. Same handler, same unguarded path as 7.2.

### 7.4 `GetItemSummary()` is dead code and is culture-sensitive — **Low**

`ViewerSetup.cs:423-424`. A repo-wide grep for `GetItemSummary` returns only the definition — there
is no caller in any project. It additionally formats dates without `CultureInfo.InvariantCulture`
(a CA1305 candidate). Two dispositions: (a) delete it, freeing 2 lines and removing 1 uncovered line
from the denominator; or (b) retain and cover it with a real behavioural assertion (test A3). This
artifact recommends **(b)** because it is a plausible diagnostic helper and the cost is one small
test, but the plan owner should note that (a) is also defensible and that a coverage test for
provably dead code sits close to the "manufactured coverage" the epic prohibits.

### 7.5 `Cleanup()` contains duplicated assignments — **Informational**

`_itemViewer = null` appears at both line 403 and line 419; `_folderHandler = null` at both 408 and
411. Dead stores; no functional impact. They inflate the covered-line count slightly.

### 7.6 `ResolveControlGroups` reads from two different viewer references — **Informational**

`ViewerSetup.cs:205-249` takes an `ItemViewer itemViewer` parameter but reads
`itemViewer.GetAllChildren()` (213) and `itemViewer.LblItemNumber` (223) from the **parameter** while
reading `_itemViewer.TipsLabels` (215) and `_itemViewer.ExpandedTipsLabels` (219) from the **field**.
Every production call site passes `(ItemViewer)_itemViewer`, so they coincide today, and the existing
headless test passes the same object for both. If they ever diverge the resulting collections would
be inconsistent. The same split exists in `ResolveControlGroupsAsync` (254-307).

### 7.7 Two different marshalling mechanisms target the same control — **Informational**

`AssignControlsAsync` (349) marshals via the WPF `_itemViewer.UiDispatcher`, while the
`AssignControls` it invokes then tests the WinForms `_itemViewer.InvokeRequired` (357). Two
independent affinity mechanisms are used for one control. Not a defect today (the dispatcher is
created on the control's thread), but a latent hazard if either is ever repointed.

---

## 8. Proposed test case list

19 test cases across four groups, plus 4 non-test atomic tasks. Group A needs no production change.
Groups B and C are the two changes that clear the floors. Group D is optional.

### Group A — zero production change (4 tests)

| ID | Target | Scenario | Fixture | Closes |
| --- | --- | --- | --- | --- |
| **A1** | `ConfigureAndAttachBreadcrumbAsync` (177-178) | Negative / argument contract — invariant VS-8 | `HarnessController`; call with `attachCollapsed: null`; assert `ArgumentNullException` with `ParamName == "attachCollapsed"`, and that no configuration side effect occurred (a `Mock<IWebViewCoreInitializer>(MockBehavior.Strict).VerifyNoOtherCalls()`) | Line 178; branch 177 → 2/2 |
| **A2** | `OnBreadcrumbUnhandledArrow` (187) | Negative — null collaborator | `ViewerScope` viewer as `sender`; leave `_kbdHandler` null; invoke via `QfcItemControllerTestSupport.InvokeNonPublic`; assert no throw | Branch 187 → 2/2 |
| **A3** | `GetItemSummary` (424) | Positive | `HarnessController` with `ItemHelper = new MailItemHelper { Subject, SentDate, SenderName }`; assert the summary contains subject and sender and the date rendered by the **same** `ToString` calls (see §6 culture note) | Line 424 |
| **A4** | `Cleanup()` (392-421) | Re-entrancy / idempotent dispose — invariant VS-5 | Extend the arrangement of `Cleanup_NullsTrackedPrivateFields`; call `Cleanup()` **twice**; assert the second call does not throw and all tracked fields remain null | Pins VS-5 (no new lines) |

### Group B — CID resolver extraction (required; production change)

| ID | Kind | Description |
| --- | --- | --- |
| **B0** | Atomic task | Create `QuickFiler/Controllers/QfcCidImageResolver.cs` per §4.2; move `ResolveImageMimeType` out of `ViewerSetup.cs:191-202`; replace the lambda body at 81-102 with the 5-line adapter; add `<Compile Include="Controllers\QfcCidImageResolver.cs" />` to `QuickFiler/QuickFiler.csproj`; append the F1 ledger row (bucket `testable`, target >= 90% line). **No `record`/`init` — see §4.2.** |
| **B1** | Test | `ResolveMimeType` — `[DataTestMethod]` with rows `.jpg`→`image/jpeg`, `.jpeg`→`image/jpeg`, `.png`→`image/png`, `.gif`→`image/gif`, `.bmp`→`image/bmp`. Positive |
| **B2** | Test | `ResolveMimeType` — `[DataTestMethod]` rows `null`, `""`, `.pdf`, `.docx` → `application/octet-stream`. Negative / edge |
| **B3** | Test | `ResolveMimeType(".PNG")` → `image/png`. Edge — pins the `ToLowerInvariant` normalisation |
| **B4** | Test | `Resolve` with a URI whose last segment matches a content id → returns the matching attachment's bytes and the correct MIME. Positive. Also pins invariant VS-9 (the map is built from the supplied attachment array at call time) |
| **B5** | Test | `Resolve` with an unmatched content id → returns null. Negative |
| **B6** | Test | `Resolve` with a URI ending in `/` (empty last segment) → returns null. Edge |
| **B7** | Test | `Resolve` with a malformed URI → characterises the current `UriFormatException` (defect §7.2) without changing behavior. Error |
| **B8** | Test | `Resolve` with a null attachment array → characterises `CidImageResolver.BuildContentIdMap(null)`'s current behavior. Negative |

B1-B3 alone close all 12 conditions at old line 195 and clear the branch floor.

### Group C — de-exempt `EnsureBreadcrumbPipeline` (required; attribute removal only)

| ID | Kind | Description | Invariant |
| --- | --- | --- | --- |
| **C0** | Atomic task | Remove `[ExcludeFromCodeCoverage]` at `ViewerSetup.cs:132`; rewrite the comment at 127-131 | — |
| **C1** | Test | `_itemViewer` is a `Mock<IItemViewer>` (not an `ItemViewer`) → early return at 137; assert nothing is subscribed and `_breadcrumbViewer` stays null. Negative | — |
| **C2** | Test | `ViewerScope` viewer with no coordinator + `Mock<IOlObjects>.FolderTreeService` → `BreadcrumbCoordinator` becomes non-null and `_breadcrumbViewer` is set. Positive | — |
| **C3** | Test | Call twice with the **same** viewer → coordinator is the same instance and `BreadcrumbUnhandledArrow` fires the handler exactly once per raise. Re-entrancy | VS-1, VS-2 |
| **C4** | Test | Call with viewer A, then with viewer B → A is unsubscribed, B subscribed exactly once, `_breadcrumbViewer` is B. Ordering / state transition | VS-2 |
| **C5** | Test | Call `Cleanup()` first, then `EnsureBreadcrumbPipeline()` → early return, no throw. Dispose-before-setup | VS-4 |

### Group D — optional: de-exempt `ResolveControlGroupsAsync`

| ID | Kind | Description |
| --- | --- | --- |
| **D0** | Atomic task | Remove `[ExcludeFromCodeCoverage]` at `ViewerSetup.cs:253`; add a `QfcItemControllerTestSupport` helper that reflection-sets a headless `ItemViewer`'s private `_context` to a plain `SynchronizationContext` (§4.3), documented with the reason |
| **D1** | Test | Pre-cancelled `Token` → `OperationCanceledException` at line 256 before any viewer access. Reachable with a plain `Mock<IItemViewer>`; **cheapest item in Group D, schedule first**. Negative |
| **D2** | Test | Headless viewer with the plain sync context → assert `TableLayoutPanels`, `Buttons`, `ListTipsDetails`, `ListTipsExpanded` are populated and the nav-column flags match `ResolveControlGroups`'s result. Positive; closes the 17 lambda lines and 4 conditions |

### Scenario-completeness check

| Required scenario | Covered by |
| --- | --- |
| Positive, valid inputs | B1, B4, C2, D2 + 9 pre-existing tests |
| Negative / missing input | A1, A2, B2, B5, B8, C1, D1 |
| Edge / boundary | B3, B6 |
| Error handling | A1, B7 |
| Concurrency | Not applicable — no shared mutable state across threads; dispatcher use is awaited |
| Ordering | C4, VS-3/VS-7 already pinned |
| Re-entrancy | A4, C3 |
| Dispose-before-setup | C5 |

### Projected result

| Stage | Denominator | Covered | Line % | Branch % |
| --- | --- | --- | --- | --- |
| Today | 160 | 116 | 72.5% | 55.6% |
| + Group A | 160 | 118 | 73.8% | ~62.5% |
| + Group B (17 lambda lines and 8 mime lines leave the file; ~5 return as the thin adapter) | ~140 | 118 | **~84.3%** | **~86%** |
| + Group C (~11 lines, ~8 conditions, all covered) | ~151 | ~129 | **~85.4%** | **~91%** |
| + Group D (optional) | ~171 | ~149 | **~87%** | **~93%** |

Groups A + B + C clear both floors with roughly a 5-point line margin and a 16-point branch margin.
Group D is headroom, not a requirement.

New file `QfcCidImageResolver.cs` is projected at **100% line / 100% branch** from B1-B8, comfortably
above the >= 90% new-file target in epic.md "Coverage-Target Reconciliation".

Exemption count for this file: **3 → 1** (Groups B + C), or **3 → 1** with a smaller residual body if
Group D also lands. `InitializeWebViewAsync` (line 38) is the sole retained exemption, with the
rewritten rationale in §4.5.

---

## 9. File-size and creation impact

### 9.1 `QfcItemController.ViewerSetup.cs`

- Current **426 / 500**, headroom 74.
- Group B removes `ResolveImageMimeType` (lines 191-202, 12 physical lines) and shrinks the lambda
  (81-102, 22 lines) to roughly 7 → **net −27**.
- Group C removes one attribute line and rewrites a comment → net ≈ 0.
- Projected **~399 / 500**, headroom ~101. **No partial split is required.**

### 9.2 New production file — obligations (epic.md "Mid-Wave File Creation")

`QuickFiler/Controllers/QfcCidImageResolver.cs`, projected ~60-70 lines. Required in the **same
change** that creates it:

1. **`<Compile Include="Controllers\QfcCidImageResolver.cs" />` in `QuickFiler/QuickFiler.csproj`.**
   That project is legacy non-SDK with **no globbing** — the existing `QfcItemController*` entries are
   listed explicitly at `QuickFiler/QuickFiler.csproj:328-337`, and the code will not compile without
   the entry.
2. **CRLF preservation.** The csproj is CRLF-terminated. Use the **Edit tool** or `perl -0777` with
   explicit `\r\n`. **Never** a git-bash `sed -i`, which strips CRLF and produces a whole-file diff
   guaranteed to conflict at fan-in. Keep the edit to a single adjacent hunk next to line 337 so
   concurrent children collide on as few lines as possible; expect an additive conflict at fan-in and
   resolve by keeping both sides.
3. **An F1 ledger row**, appended in the same change, bucket `testable`, target **>= 90% line**
   (epic.md rule 4: files newly created by this epic take the `CLAUDE.md` §UT2 new-module target).
   Do **not** claim `ratified-exempt` — a pure static resolver meets none of the three exemption
   grounds.
4. **500-line limit** applies; ~70 lines is well inside it.

### 9.3 Test project

- Group A extends the existing `QfcItemController.ViewerSetupTests.cs` (currently 407 lines) — adding
  4 tests takes it near the **500-line limit**. Budget for it; if it breaches, split into
  `QfcItemController.ViewerSetupLifecycleTests.cs`.
- Group B needs a **new** fixture, e.g. `QuickFiler.Test/Controllers/QfcCidImageResolverTests.cs`,
  mirroring the production tree.
- Group C fits in `QfcItemControllerBreadcrumbDropDownTests.cs` (currently 386 lines) or a new
  `QfcItemController.BreadcrumbPipelineTests.cs`.
- **Every new test file needs its own `<Compile Include>` entry in
  `QuickFiler.Test/QuickFiler.Test.csproj`** — that project also has no globbing (existing entries at
  lines 90 and 132-147), under the same CRLF rule. This obligation is not stated in the delegation
  brief; see `file-QfcItemController.md` §9.2.
- Group D adds a helper to the shared `QfcItemController.TestSupport.cs` (currently 365 lines) — no
  csproj edit, but it is a file every F10 phase touches, so sequence it to avoid intra-child churn.

---

## 10. Sibling boundaries — do not edit

| Sibling asset | Owner | Dependency | Action |
| --- | --- | --- | --- |
| `ConversationResolver` | **F4 (#434)** | Not referenced in this file | None. |
| `IQfcKeyboardHandler` / `KeyboardHandler.cs` | **F3 (#430)** | `ViewerSetup.cs:187` — `_kbdHandler?.BreadcrumbArrowFallThrough(viewer, direction)` | Read-only use of an existing method. A2 asserts only the null-handler path. **No edit, no contract change needed.** |
| `IQfcDatamodel` | **F5** | Not referenced | None. |
| `ItemViewer` / `IItemViewer` — `L0v2h2_WebView2` (`ItemViewer.cs:309`), `L0vhBreadcrumb_WebView2`, `BreadcrumbCoordinator`, `InitializeBreadcrumbPipeline`, `AttachBreadcrumbWebViewAsync`, `SetBreadcrumbTheme`, `ResetBreadcrumb`, `BreadcrumbUnhandledArrow` (all `ItemViewer.Breadcrumb.cs:19-295`), `GetAllChildren` | **F14** | Consumed at 66, 76, 109, 112-116, 135-156, 166-167, 213, 266, 396 | **No edit.** Group D's reflection access to the private `_context` field (`ItemViewer.cs:59`) is **test-side only** and adds no production coupling. **Do not add members to `IItemViewer` or `IQfcItemController`** — see `file-QfcItemController.md` §10. |
| `BreadcrumbBridgeCoordinator`, `BreadcrumbPopupUiOperations`, `IBreadcrumbDropDownHost`, `IWebViewMessenger` | **F12 / F13** | Reached transitively through `ItemViewer.InitializeBreadcrumbPipeline`; already exercised headlessly by the existing tests | No edit. |
| `IWebViewCoreInitializer` / `WebView2CoreInitializer` | **F13** | `ViewerSetup.cs:61, 65, 108, 166` | Consumed as an injected interface. No edit. |
| `UtilitiesCS.CidImageResolver` (`BuildContentIdMap`, `DefaultVirtualHost`), `IAttachment` | UtilitiesCS — **outside every child's assignment** | `ViewerSetup.cs:78, 89` | **Called, never edited.** The §4.2 extraction deliberately keeps `BuildContentIdMap` in UtilitiesCS and builds the new resolver locally, per epic.md Cross-Child Constraints §2. |
| `UtilitiesCS.OutlookObjects.Folder.OutlookFolderHierarchyProvider`, `IOutlookFolderTreeService` | UtilitiesCS / F12 territory | `ViewerSetup.cs:142-144` | Constructed, never edited. C2 supplies a mocked `FolderTreeService` through `_globals.Ol`. |
| `MailItemHelper` (`AttachmentsInfo`, `Subject`, `SentDate`, `SenderName`, `Body`, `Triage`, ...) | UtilitiesCS | `ViewerSetup.cs:89, 311, 332, 363-368, 424` | No edit; already mockable/constructible as the existing tests show. |

### Cross-child contract notes (no edit requested, notification only)

1. **F14** must preserve `ItemViewer.BreadcrumbCoordinator`, `InitializeBreadcrumbPipeline(IFolderHierarchyProvider)`,
   `BreadcrumbUnhandledArrow`, and `ResetBreadcrumb` as currently shaped; C1-C5 and the existing six
   breadcrumb tests depend on them. It must also preserve the private field name `_context`
   (`ItemViewer.cs:59`) if Group D is scheduled.
2. **F13** must preserve `IWebViewCoreInitializer`'s two-method shape; the retained exemption
   rationale in §4.5 cites it by name.
3. **UtilitiesCS** `CidImageResolver.BuildContentIdMap(IAttachment[])` and `DefaultVirtualHost` become
   a compile-time dependency of a **new** QuickFiler file under Group B. They are already public and
   already tested; no change requested.

---

## 11. Summary

| Question | Answer |
| --- | --- |
| Current coverage reality | 72.5% line (attribute says 74.4%) and **55.6% branch** (attribute says 56%). **Fails both gates** — the only F10 file that does. The brief flagged only the line failure; the branch failure is larger. |
| Size of the gap | 44 uncovered lines and 24 uncovered branch conditions. **34 of the 44 lines (77%) are lambda bodies inside `[ExcludeFromCodeCoverage]` methods**, which the attribute does not suppress. |
| Seams required | One host-neutral extraction (`QfcCidImageResolver` — a pure static + DTO, no interface or delegate needed) plus one attribute removal (`EnsureBreadcrumbPipeline`, which needs **no** seam). Test-only work plateaus at 78.8%, so a production change is unavoidable here. |
| Proposed test cases | **19** (A1-A4, B1-B8, C1-C5, D1-D2) plus **4** non-test atomic tasks (B0, C0, D0, and the §4.5 comment rewrite). |
| File split needed | **No.** The extraction takes ViewerSetup.cs from 426 to ~399. One **new** production file is created (~70 lines), which requires a `QuickFiler.csproj` `<Compile Include>` entry with CRLF preserved and an F1 ledger row at bucket `testable`, >= 90%. The **test** project needs at least one new fixture file with its own `QuickFiler.Test.csproj` entry. |
| Exemption boundary | **3 → 1.** `EnsureBreadcrumbPipeline` (132) and, optionally, `ResolveControlGroupsAsync` (253) come off; `InitializeWebViewAsync` (38) is retained with a rewritten, member-specific rationale citing the irreducible `.CoreWebView2` dereference at line 76. |
| STA required | **No.** Headless `ItemViewer` construction already runs in plain `[TestClass]`/`[TestMethod]` in this fixture pair, so the epic's `*.StaTests.cs` last-resort clause is not engaged. |
| Latent defects found | 7 (§7): undisposed timer on `Cleanup` (Moderate), unguarded `new Uri` in a WebView2 handler (Low/Moderate), unguarded `new MemoryStream(null)` (Low), dead + culture-sensitive `GetItemSummary` (Low), duplicated `Cleanup` assignments, two-source viewer reads in `ResolveControlGroups`, and dual marshalling mechanisms (Informational). |
| Corrections to the brief | (a) Branch coverage is 56% against a 75% floor — a worse failure than the line gate and unmentioned in the brief; (b) `[ExcludeFromCodeCoverage]` does not cover lambdas inside the exempt method, which is where 77% of this file's gap lives; (c) test-only work cannot reach 80% here; (d) `QuickFiler.Test.csproj` also needs `<Compile Include>` entries. |
