# F13 Cross-Cutting Context — quickfiler-breadcrumb-dropdown-webview-coverage (#455)

- Epic: #136 `quickfiler-per-file-coverage`, child F13
- Branch: `feature/quickfiler-breadcrumb-dropdown-webview-coverage` (base `origin/epic/quickfiler-per-file-coverage-integration`)
- Research date: 2026-08-07
- Author: task-researcher

## 0. Tooling limitation affecting this artifact (read first)

**No `Bash` / shell tool was available in this research session.** Only `Read`, `Grep`, `Glob`,
`Write`, `Edit`, and `WebFetch` were exposed. Therefore:

- **No `git` command was executed.** No `git log --oneline -- <paths>`, no
  `git log --all --grep=400`, no `git diff`. Every merge/divergence conclusion below is derived
  from **working-tree file state**, not from commit history, and is labelled as such.
- **No `gh` command was executed.** `gh issue list --state open --limit 200` could not be run.
  Section 4 is therefore a **filesystem-derived proxy** for the open-issue scan and is explicitly
  incomplete. The orchestrator must re-run the `gh` scan before planning closes.

Command/EXIT_CODE fields are supplied only where a command was actually run; none was. Where the
brief asked for command output, the equivalent filesystem evidence and its exact path/line are
given instead.

---

## 1. In-flight conflict #400 — `2026-07-21-quickfiler-folder-selector-dropdown-400`

### (a) Has it merged into the epic integration base?

**Yes — with high confidence, on file-state evidence.** Four independent indicators:

1. The feature folder itself is present on this branch at
   `docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/` (756 files).
2. **All 15 F13 production/interface files exist in the working tree** and are the artifacts #400
   created. `QuickFiler/Viewers/BreadcrumbDropDownHost.cs`,
   `BreadcrumbDropDownOpenLifetime.cs`, `BreadcrumbDropDownOpenCoordinator.cs`,
   `BreadcrumbPopupUiOperations.cs`, `BreadcrumbCollapsedSurfaceController.cs`,
   `BreadcrumbWebViewSurfaceFactory.cs`, `BreadcrumbPopupPlacement.cs`,
   `IBreadcrumbDropDownHost.cs`, `IWebViewMessenger.cs` are all present and all have
   `<Compile Include>` entries at `QuickFiler/QuickFiler.csproj:396-411`.
3. Measured line counts on this branch match the epic manifest exactly (see §2.3), and the epic
   states its line counts are "as of `origin/main` at 74be1964".
4. The **final** #400 remediation (per `feature-audit.2026-08-04T15-50.md:57-58`, "Final changes
   are two PowerShell paths only") is present: `tests/scripts/vscode/Invoke-MSTest.RunSettings.Tests.ps1`
   exists on this branch.

`feature-audit.2026-08-04T15-50.md:62-69` records **PASS on all 19 acceptance criteria**, head
`62c4eb1c2b99ae6e9fa7742a31d283ec4a8d7151`, merge base `050f7cd5`. #400 is functionally complete;
the folder simply has not been moved to `completed/`.

**Residual uncertainty:** without `git log` I cannot rule out a small post-`62c4eb1c` commit on
`bug/quickfiler-folder-selector-dropdown-400` that has not merged. The orchestrator should confirm
with `git log --oneline origin/main..origin/bug/quickfiler-folder-selector-dropdown-400`.

### (b) Which of OUR 15 files does #400 touch?

**All 15.** #400 authored the entire F13 surface. The `WebView2*` trio predates #400 (introduced by
issue #349/#351 — see the doc comments at `WebView2BreadcrumbHost.cs:14-28` and
`WebView2CoreInitializer.cs:8-14`), but #400 touched `WebView2Messenger.cs` (it is constructed at
`BreadcrumbPopupUiOperations.cs:409` from the #400 popup path and carries a `BreadcrumbUiDispatcher`
ctor overload at `WebView2Messenger.cs:36` that only #400 needed).

Practical consequence: **there is no pre-#400 F13 baseline.** Every line in this child's scope is
#400-era code with #400-era tests.

### (c) Regression tests our new tests must not contradict

`evidence/qa-gates/coverage-accounting-scope-change.2026-07-21T18-01.md:74-103` enumerates 29
named MSTest cases that #400 committed as the automated substitute for direct numeric coverage of
the excluded UI adapters. The load-bearing ones for F13 are:

| Test | Location | What it pins |
|---|---|---|
| `ExistingAnchor_RemainsTheDesignerWebViewClosedSurface` | `QuickFiler.Test/Viewers/ItemViewerBreadcrumbDropDownContractTests.cs:19-29` | `ItemViewer.L0vhBreadcrumb_WebView2` **must stay typed `Microsoft.Web.WebView2.WinForms.WebView2`** |
| `ProductionConfiguration_AcceptsExistingEnvironmentAndInitializer` | same file `:31-49` | `ItemViewer.ConfigureBreadcrumbDropDown(CoreWebView2Environment, IWebViewCoreInitializer)` must exist |
| `InjectedConfiguration_AcceptsHostAndScreenGeometryProviders` | same file `:51-74` | `ItemViewer.ConfigureBreadcrumbDropDown(IBreadcrumbDropDownHost, Func<Rectangle>, Func<Rectangle>)` must exist — **the injected-host seam is contractual** |
| `HostNeutralPopupOpenOrchestration_IsOwnedByInstrumentedCoordinator` | same file `:102-130` | `BreadcrumbDropDownOpenCoordinator` must be `internal`, must **NOT** carry `[ExcludeFromCodeCoverage]`, and `ItemViewer` must **NOT** declare `OpenBreadcrumbDropDownAsync` |
| `Constructor_OwnsAutoClosingToolStripDropDownWithoutGlobalTopmostForm` | `QuickFiler.Test/Viewers/BreadcrumbDropDownHostTests.cs:21-37` | `BreadcrumbDropDownHost` must own an `AutoClose=true` `ToolStripDropDown` and must hold **no `Form`-typed field** |
| `ExplicitCommitAndUncommittedClose_HaveDistinctCallbacks` | same file `:64-84` | Uncommitted close invokes `_cancelSelection`; explicit commit does not |

The fourth row is the strongest constraint: **any attempt by F13 to exempt
`BreadcrumbDropDownOpenCoordinator` fails an existing test.** Conversely it is a green light — that
test was written specifically to keep the coordinator measurable.

### (d) Version divergence between #400's committed evidence and our branch?

**No divergence detected, and I can prove it numerically.** #400's newest committed Cobertura
(`evidence/qa-gates/coverage-nonnumeric-adapter-member-coverage-relative-output.2026-07-27T10-55.cobertura.xml`)
and #424's newest (`.../424/evidence/qa-gates/coverage-final.cobertura.xml`, 2026-08-06) report
**byte-identical `line-rate` and `branch-rate` at identical XML offsets** for all eight instrumented
F13 files (compare §3 table; both files place `BreadcrumbUiDispatcher` at XML line 8874,
`BreadcrumbPopupUiOperations` at 9383, `BreadcrumbDropDownHost` at 12304, etc.).

The two reports are genuinely distinct runs — their `<coverage>` headers differ:

- #400 2026-07-27: `line-rate="0.845568" branch-rate="0.771915" timestamp="1785150493" lines-valid="109252"`
- #424 2026-08-06: `line-rate="0.856453" branch-rate="0.790039" timestamp="1786072633" lines-valid="110849"`

Identical per-file figures across two distinct full-suite runs 10 days apart is strong evidence that
**neither the F13 production files nor their tests changed between 2026-07-27 and 2026-08-06**, and
that the figures are reproducible rather than run-dependent.

---

## 2. Existing test inventory

### 2.1 Test file → production file map

`QuickFiler.Test/Viewers/` holds 31 `.cs` files; `QuickFiler.Test/Controllers/` holds two more that
touch F13. Classification (F13 = ours, F12 = sibling-owned, F14 = ItemViewer):

| Test file | Lines | Primary target(s) | Owner |
|---|---:|---|---|
| `Viewers/BreadcrumbDropDownHostTests.cs` | 499 | `BreadcrumbDropDownHost` | **F13** |
| `Viewers/BreadcrumbDropDownIntegrationTests.cs` | **500** | `BreadcrumbDropDownHost` + coordinator | **F13** |
| `Viewers/BreadcrumbDropDownReadinessTests.cs` | 498 | `BreadcrumbDropDownHost`, `BreadcrumbWebViewSurfaceFactory`, `BreadcrumbPopupUiOperations` | **F13** |
| `Viewers/BreadcrumbDropDownCoverageThresholdTests.cs` | 477 | `BreadcrumbDropDownHost`, `BreadcrumbPopupUiOperations` | **F13** |
| `Viewers/BreadcrumbDropDownLifecycleTests.cs` | 277 | `BreadcrumbDropDownHost` | **F13** |
| `Viewers/BreadcrumbDropDownLifecycleCoverageTests.cs` | 469 | `BreadcrumbDropDownOpenLifetime`, host, `BreadcrumbPopupUiOperations` | **F13** |
| `Viewers/BreadcrumbDropDownLifecycleConcurrencyTests.cs` | 406 | `BreadcrumbDropDownHost` | **F13** |
| `Viewers/BreadcrumbDropDownOpenCoordinatorTests.cs` | 447 | `BreadcrumbDropDownOpenCoordinator` | **F13** |
| `Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part2.cs` | 381 | `BreadcrumbDropDownOpenCoordinator`, `BreadcrumbDropDownOpenLifetime`, `BreadcrumbPopupUiOperations` | **F13** |
| `Viewers/BreadcrumbPopupBoundaryCoverageTests.cs` | 361 | `BreadcrumbPopupUiOperations`, `BreadcrumbUiDispatcher`, `BreadcrumbWebViewSurfaceFactory` | **F13** |
| `Viewers/BreadcrumbPopupBoundaryCoverageTests.Part2.cs` | 480 | `BreadcrumbDropDownOpenLifetime`, `BreadcrumbPopupUiOperations`, `BreadcrumbWebViewSurfaceFactory` | **F13** |
| `Viewers/BreadcrumbPopupControlDispatchTests.cs` | 486 | `BreadcrumbPopupUiOperations`, `BreadcrumbUiDispatcher` | **F13** |
| `Viewers/BreadcrumbPopupUiOperationsDirectAdapterTests.cs` | 302 | `BreadcrumbPopupUiOperations`, `BreadcrumbWebViewSurfaceFactory` | **F13** |
| `Viewers/BreadcrumbPopupPlacementTests.cs` | 169 | `BreadcrumbPopupPlacement` | **F13** |
| `Viewers/BreadcrumbUiThreadDispatchTests.cs` | 480 | `BreadcrumbUiDispatcher` (9 refs) | **F13** |
| `Viewers/BreadcrumbCollapsedSurfaceReadinessTests.cs` | 487 | `BreadcrumbCollapsedSurfaceController`, `BreadcrumbWebViewSurfaceFactory` | **F13** |
| `Viewers/BreadcrumbPendingOpenCloseTests.cs` | 380 | `BreadcrumbDropDownHost` | **F13** |
| `Viewers/BreadcrumbSelectorOpenRetryTests.cs` | 461 | host, `BreadcrumbUiDispatcher`, `BreadcrumbPopupUiOperations`, `BreadcrumbPopupPlacement` | **F13** |
| `Viewers/BreadcrumbSelectorToggleUiBoundaryTests.cs` | 478 | host, `BreadcrumbPopupUiOperations`, `BreadcrumbUiDispatcher` | **F13** |
| `Viewers/BreadcrumbSubfolderActivationTests.cs` | 480 | `BreadcrumbDropDownHost` (+ F12 hub) | mixed |
| `Viewers/BreadcrumbSelectorCoordinatorTests.cs` | 434 | mostly F12 coordinator; 1 `BreadcrumbUiDispatcher` ref | F12 |
| `Viewers/BreadcrumbMessengerHubTests.cs` | 414 | `BreadcrumbMessengerHub` (F12) + `BreadcrumbCollapsedSurfaceController` | F12 |
| `Viewers/BreadcrumbMessengerHubCoverageTests.cs` | 478 | `BreadcrumbMessengerHub` (F12) + collapsed controller / surface factory | F12 |
| `Viewers/BreadcrumbBridgeCoordinatorTests.cs` | 488 | `BreadcrumbBridgeCoordinator` | F12 |
| `Viewers/BreadcrumbBridgeCoordinatorProbabilityTests.cs` | 168 | `BreadcrumbBridgeCoordinator` | F12 |
| `Viewers/BreadcrumbCoordinatorLifecycleTests.cs` | 489 | F12 lifecycle coordinator | F12 |
| `Viewers/BreadcrumbCoordinatorUpgradeLifetimeTests.cs` | 122 | `BreadcrumbCoordinatorUpgradeLifetime` | F12 |
| `Viewers/BreadcrumbItemViewerLifecycleCoordinatorTests.cs` | 327 | F12 lifecycle coordinator (+ our host/factory/dispatcher) | F12 |
| `Viewers/BreadcrumbDuplicateIdentityIntegrationTests.cs` | 218 | F12 bridge/hub | F12 |
| `Viewers/FolderBreadcrumbAssetContractTests.cs` | 405 | `QuickFiler/Resources/FolderBreadcrumb.html` asset | F12/asset |
| `Viewers/ItemViewerBreadcrumbDropDownContractTests.cs` | 132 | `ItemViewer` reflection contracts + our host/coordinator | **F14 boundary, F13-constraining** |
| `Controllers/QfcItemControllerBreadcrumbDropDownTests.cs` | 385 | `QfcItemController` wiring; 4 host refs, 5 `IWebViewMessenger` refs | F10 boundary |
| `Controllers/WebView2CoreInitializerTests.cs` | 25 | `WebView2CoreInitializer` construction smoke test only | **F13** |

**19 of 31 `Viewers/` test files are F13-primary; 11 are F12-primary; 1 is asset-contract.**

### 2.2 Production files with NO dedicated test file

Five of the eleven production files have **no `<TypeName>Tests.cs`** of their own, only incidental
coverage from neighbouring fixtures:

| Production file | Dedicated test file? | Notes |
|---|---|---|
| `BreadcrumbDropDownOpenLifetime.cs` | **No** | Covered only from `BreadcrumbDropDownLifecycleCoverageTests.cs`, `BreadcrumbPopupBoundaryCoverageTests.Part2.cs` (4 refs total across 2 files) |
| `BreadcrumbCollapsedSurfaceController.cs` | **No** | Covered from `BreadcrumbCollapsedSurfaceReadinessTests.cs` (2 refs) and F12 hub tests |
| `BreadcrumbWebViewSurfaceFactory.cs` | **No** | Reached via `BreadcrumbNavigationReadiness` in 9 files |
| `WebView2BreadcrumbHost.cs` | **No** | **Zero test references anywhere in `QuickFiler.Test/`** |
| `WebView2Messenger.cs` | **No** | **Zero test references anywhere in `QuickFiler.Test/`** |
| `WebView2CoreInitializer.cs` | Nominal only | `Controllers/WebView2CoreInitializerTests.cs` (25 lines) asserts construction and interface assignability; contributes **zero** coverage because the type is excluded |

`WebView2BreadcrumbHost` and `WebView2Messenger` are genuinely untested (grep across
`QuickFiler.Test/` returns matches only in `QuickFiler.Test.csproj` and
`Controllers/WebView2CoreInitializerTests.cs`).

### 2.3 500-line headroom — **this is the binding structural constraint**

Production files (measured on this branch; matches the epic manifest exactly):

| File | Lines | Headroom |
|---|---:|---:|
| `BreadcrumbPopupUiOperations.cs` | 494 | 6 |
| `BreadcrumbDropDownHost.cs` | 480 | 20 |
| `BreadcrumbDropDownOpenLifetime.cs` | 477 | 23 |
| `BreadcrumbDropDownOpenCoordinator.cs` | 309 | 191 |
| `BreadcrumbCollapsedSurfaceController.cs` | 308 | 192 |
| `BreadcrumbUiDispatcher.cs` | 285 | 215 |
| `BreadcrumbWebViewSurfaceFactory.cs` | 225 | 275 |
| `WebView2Messenger.cs` | 147 | 353 |
| `WebView2BreadcrumbHost.cs` | 143 | 357 |
| `BreadcrumbPopupPlacement.cs` | 87 | 413 |
| `WebView2CoreInitializer.cs` | 30 | 470 |

Test files — **thirteen F13-relevant test files sit within 25 lines of the limit**:

`BreadcrumbDropDownIntegrationTests.cs` **500 (zero headroom)**, `BreadcrumbDropDownHostTests.cs` 499,
`BreadcrumbDropDownReadinessTests.cs` 498, `BreadcrumbCoordinatorLifecycleTests.cs` 489,
`BreadcrumbBridgeCoordinatorTests.cs` 488, `BreadcrumbCollapsedSurfaceReadinessTests.cs` 487,
`BreadcrumbPopupControlDispatchTests.cs` 486, `BreadcrumbUiThreadDispatchTests.cs` 480,
`BreadcrumbSubfolderActivationTests.cs` 480, `BreadcrumbPopupBoundaryCoverageTests.Part2.cs` 480,
`BreadcrumbMessengerHubCoverageTests.cs` 478, `BreadcrumbSelectorToggleUiBoundaryTests.cs` 478,
`BreadcrumbDropDownCoverageThresholdTests.cs` 477.

**Design consequence:** essentially every new test case F13 writes must go into a **new** test file.
The repo already establishes the `.Part2.cs` convention for this
(`BreadcrumbDropDownOpenCoordinatorTests.Part2.cs`, `BreadcrumbPopupBoundaryCoverageTests.Part2.cs`).
Each new test file needs its own `<Compile Include>` in `QuickFiler.Test/QuickFiler.Test.csproj`
(existing breadcrumb entries at lines 58-91, 150).

---

## 3. Measured coverage evidence

### 3.1 Committed Cobertura reports that instrument these files

| Report | Date | Repo line-rate | Repo branch-rate |
|---|---|---:|---:|
| `docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml` | 2026-08-06 | 0.856453 | 0.790039 |
| `docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/evidence/qa-gates/coverage-nonnumeric-adapter-member-coverage-relative-output.2026-07-27T10-55.cobertura.xml` | 2026-07-27 | 0.845568 | 0.771915 |
| 25 further `*.cobertura.xml` under #400's `evidence/qa-gates/` and `evidence/baseline/` | 2026-07-21 … 2026-07-27 | various | various |

The #424 report is the most recent. Its `QuickFiler` package node reports
`line-rate="0.8001906577693041" branch-rate="0.7371154614462645"` (XML line 7).

### 3.2 Per-file line AND branch rates (most recent report, #424 2026-08-06)

| # | Production file | Cobertura `<class>` name | Line-rate | Branch-rate | 80% line | 75% branch |
|---|---|---|---:|---:|:--:|:--:|
| 1 | `Viewers/BreadcrumbUiDispatcher.cs` | `BreadcrumbUiDispatcher` | **100.0000%** | **96.9697%** | PASS | PASS |
| 2 | `Viewers/BreadcrumbPopupPlacement.cs` | `BreadcrumbPopupPlacementResult` | **100.0000%** | **100.0000%** | PASS | PASS |
| 3 | `Viewers/BreadcrumbWebViewSurfaceFactory.cs` | `BreadcrumbNavigationReadiness` | **99.5763%** | **98.6111%** | PASS | PASS |
| 4 | `Viewers/BreadcrumbCollapsedSurfaceController.cs` | `BreadcrumbCollapsedSurfaceController` | **99.4302%** | **85.8974%** | PASS | PASS |
| 5 | `Viewers/BreadcrumbDropDownHost.cs` | `BreadcrumbDropDownHost` | **99.4220%** | **91.4894%** | PASS | PASS |
| 6 | `Viewers/BreadcrumbDropDownOpenLifetime.cs` | `BreadcrumbDropDownOpenLease` | **99.1254%** | **91.8605%** | PASS | PASS |
| 7 | `Viewers/BreadcrumbDropDownOpenCoordinator.cs` | `BreadcrumbDropDownOpenCoordinator` | **98.2544%** | **92.0455%** | PASS | PASS |
| 8 | `Viewers/BreadcrumbPopupUiOperations.cs` | `BreadcrumbPopupUiOperations` | **92.9412%** | **86.8750%** | PASS | PASS |
| 9 | `Viewers/WebView2BreadcrumbHost.cs` | — **ABSENT** | N/A (unmeasured) | N/A | — | — |
| 10 | `Viewers/WebView2Messenger.cs` | — **ABSENT** | N/A (unmeasured) | N/A | — | — |
| 11 | `Viewers/WebView2CoreInitializer.cs` | — **ABSENT** | N/A (unmeasured) | N/A | — | — |
| I1 | `Viewers/IBreadcrumbDropDownHost.cs` | — ABSENT | N/A | N/A | interface-only | |
| I2 | `Viewers/IBreadcrumbWebHost.cs` | — ABSENT | N/A | N/A | interface-only | |
| I3 | `Viewers/IWebViewCoreInitializer.cs` | — ABSENT | N/A | N/A | interface-only | |
| I4 | `Viewers/IWebViewMessenger.cs` | — ABSENT | N/A | N/A | interface-only | |

**Headline: all eight instrumented F13 production files already clear BOTH gates** (>= 80% line and
>= 75% branch). The lowest branch figure in scope is 85.90%
(`BreadcrumbCollapsedSurfaceController.cs`) — 10.9 points above the floor.

### 3.3 Confirmation of the brief's WebView2-absence claim — **CONFIRMED**

`Grep` for `filename="[^"]*(WebView2BreadcrumbHost|WebView2Messenger|WebView2CoreInitializer|IBreadcrumbDropDownHost|IBreadcrumbWebHost|IWebViewCoreInitializer|IWebViewMessenger)` against the
#424 report returns **0 occurrences**. The 36 textual hits on those names in the report are
parameter/return types inside `<method signature="...">` attributes, not `filename=` values.

Root cause is verified in source — three type-level attributes:

- `QuickFiler/Viewers/WebView2BreadcrumbHost.cs:29` — `[ExcludeFromCodeCoverage]`
- `QuickFiler/Viewers/WebView2Messenger.cs:20` — `[ExcludeFromCodeCoverage]`
- `QuickFiler/Viewers/WebView2CoreInitializer.cs:15` — `[ExcludeFromCodeCoverage]`

### 3.4 DEVIATION — the epic's `[X]` marker on `BreadcrumbPopupUiOperations.cs` is misleading

`epic.md:418` marks `Viewers/BreadcrumbPopupUiOperations.cs (494) [X]`. **The file carries no
type-level `[ExcludeFromCodeCoverage]`.** It is fully instrumented (92.94% line / 86.88% branch).
What it carries is **seven member-level attributes**:

| Line | Member | Nature |
|---:|---|---|
| 105 | `ShowOwnedPopup(ToolStripDropDown, Control, Point)` | direct `ToolStripDropDown.Show` |
| 380 | `CreateProductionControl()` | `new WebView2 { Dock = Fill }` |
| 383 | `BeginProductionInitialization(...)` | `initializer.EnsureCoreWebView2Async(...)` |
| 390 | `ReadProductionCore(Control)` | `((WebView2)control).CoreWebView2` |
| 394 | `BeginProductionNavigation(...)` | composes navigation + `new WebView2Messenger` |
| 412 | `DisposeProductionSurface(...)` | disposes control/messenger |
| 457 | `BindProductionNavigation(...)` | subscribes CoreWebView2 SDK events |

Provenance: `#400 evidence/qa-gates/coverage-accounting-scope-change.2026-07-21T18-01.md:25-30`
records these as a ratified `scope_change` (originally attached to
`BreadcrumbDropDownHost.CreateProductionSurfaceAsync` / `ShowOwnedPopup`, later relocated into
`BreadcrumbPopupUiOperations`), explicitly stating "`BreadcrumbDropDownHost` has no class-level
exclusion".

**These seven members are the correct target of F13's exemption audit** — not the file as a whole.

### 3.5 DEVIATION — harness directive A is a no-op for this report writer

`epic.md:530-533` (Directive A) asserts "One source file can produce multiple Cobertura `<class>`
elements sharing a single `filename` — a type plus its compiler-generated `<>c` closure class. The
harness must union them, taking max hits per line."

**In the #424 and #400 reports this is not the case: there is exactly one `<class>` element per
`filename`.** Evidence:

- `QuickFiler\Controllers\QfcHomeController.cs` — grep count = **1**.
- `QuickFiler\Viewers\BreadcrumbPopupPlacement.cs` produces one `<class>` named
  `QuickFiler.Viewers.BreadcrumbPopupPlacementResult` whose aggregate `<lines>` block (XML lines
  12229-12302) contains **both** types' lines — source lines 11-14 (the `readonly struct`) *and*
  35-85 (the static `BreadcrumbPopupPlacement` class). A grep for
  `name="QuickFiler.Viewers.BreadcrumbPopupPlacement"` returns **no matches**.
- `BreadcrumbPopupUiOperations.cs` lambda bodies at source lines 471-490 appear inside the **same**
  `<class>` element as their enclosing type, not in a separate `<>c` element.
- Partial classes emit one element per **file**: `QuickFiler.Controllers.QfcHomeController` appears
  twice with different `filename=` values (`QfcHomeController.cs` and `QfcHomeController.Metrics.cs`).

This writer is `dotnet-coverage --output-format cobertura` post-processed by
`scripts/vscode/Invoke-MSTestWithCoverage.ps1`. Implementing the union is harmless (idempotent) and
should still be done for safety, but **F13 must not assume the union step exists** and must not
budget work for it. Directive B (decide the denominator on `<line>` child count, never on
`line-rate`) is **confirmed necessary and correct** — all seven zero-denominator F13 files are
simply absent from the report, so a `line-rate`-keyed reader would either crash or emit a false 0%.

### 3.6 DEVIATION — the epic's 70.19% repository baseline needs restating

`epic.md:480-481` cites "a merge-base repository line rate of 70.19%". That figure is #424's
**merge-base** measurement, recorded at
`#424 evidence/qa-gates/coverage-delta.2026-08-07T00-48.md:58`. The same document's **post-change**
row reads **85.65% line / 79.00% branch**, and lines 65-67 explicitly warn that the two are *not
like-for-like*: the denominator grew from 79,957 to 110,849 valid lines (+38.6%) because
`dotnet-coverage` instruments a varying set of loaded assemblies between full-suite runs.

**Implication for F13:** the repository-wide "retain or improve" criterion must be evaluated by
measuring **before and after within the same session on the same branch**. Comparing against any
number inherited from another feature folder is unsound.

### 3.7 Where `BreadcrumbPopupUiOperations.cs` actually loses its 24 lines

`line-rate=0.929412` = 316/340 lines. 22 of the 24 uncovered lines are identified exactly:

- source lines **406** and **409** — the two lambdas inside `BeginProductionNavigation`
  (`() => ((WebView2)control).NavigateToString(html)` and `() => new WebView2Messenger(core, dispatcher)`)
- source lines **471-490** — the closure body inside `BindProductionNavigation`

Both enclosing methods carry `[ExcludeFromCodeCoverage]` (at `:394` and `:457`). **The attribute does
not propagate to compiler-generated lambda bodies**, so the lambdas remain instrumented and
uncovered. This is a measurement defect, not dead code — see §9 item L4.

Partially covered branches (`condition-coverage="50%"`) in the same file: source lines 364, 415,
416, 453.

---

## 4. Open GitHub issues by keyword — **INCOMPLETE, `gh` unavailable**

`gh issue list --state open --limit 200` could **not** be executed (no shell tool). The following is
a filesystem proxy over `docs/features/potential/`, `docs/features/potential/promoted/`,
`docs/features/active/`, and `docs/features/archive/`.

### 4.1 Active feature folders touching our 15 files

| Folder | Issue | Status vs F13 |
|---|---|---|
| `docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400/` | #400 | **Complete (19/19 AC PASS), merged, not yet moved to `completed/`.** Authored all 15 F13 files. |
| `docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/` | #424 | Does not touch F13 files; is only the source of the most recent coverage report. |
| `docs/features/active/2026-08-07-quickfiler-breadcrumb-dropdown-webview-coverage-455/` | #455 | This child. |

### 4.2 Promoted-but-not-active candidates matching the keyword set

Under `docs/features/potential/promoted/`, keyword-matching entries (all breadcrumb/WebView2/folder
related) are already **archived-complete** features, not open work:

- `2026-07-16-quickfiler-breadcrumb-webview2.md` → archive `2026-07-16-quickfiler-breadcrumb-webview2-351`
- `2026-07-16-efcviewer-breadcrumb-webview2.md` → archive `...-349`
- `2026-07-16-folder-hierarchy-live-provider.md` → archive `...-350`
- `2026-07-15-quickfiler-folder-tree-percentage.md`, `2026-07-15-folder-probability-plumbing.md` → archived
- `2026-07-09-winforms-testability-refactor.md` → archived epic

Un-promoted candidates in `docs/features/potential/` (still open, none touching F13):
`2026-07-07-ci-nullable-check-skipped-vendored-projects.md`,
`2026-08-04-invoke-mstest-scalar-count-strictmode.md`,
`2026-08-04-stale-fizzler-and-unsafe-binding-redirects.md`,
`2026-08-05-svgcontrol-coverage-uplift.md`.

**No promoted-but-not-active issue touching the F13 file set was found on disk.** The `#426`-class
blind spot the epic warns about (`epic.md:642-653`) therefore appears absent for F13 — **but this
conclusion is only as good as the filesystem proxy.** The orchestrator MUST run:

```
gh issue list --state open --limit 200 --json number,title,labels
```

and grep for `breadcrumb|dropdown|drop-down|WebView2|WebView|popup|folder selector|coverage` before
freezing the plan.

---

## 5. Prior art: the retyped-Designer-field gotcha

### 5.1 The exact field

`QuickFiler/Viewers/ItemViewer.Designer.cs`:

- `:46` — `this._l0vhBreadcrumb_WebView2 = new Microsoft.Web.WebView2.WinForms.WebView2();`
- `:206` — `this._l0vhBreadcrumb_WebView2.Name = "L0vhBreadcrumb_WebView2";`
- `:6214` — `internal Microsoft.Web.WebView2.WinForms.WebView2 _l0vhBreadcrumb_WebView2;`

Public surface: `ItemViewer` exposes property `L0vhBreadcrumb_WebView2` (asserted by reflection in
the test below; declared in `ItemViewer.Breadcrumb.cs` / the Designer partial).

### 5.2 The test that pins it

`QuickFiler.Test/Viewers/ItemViewerBreadcrumbDropDownContractTests.cs:18-29`:

```csharp
[TestMethod]
public void ExistingAnchor_RemainsTheDesignerWebViewClosedSurface()
{
    PropertyInfo property = typeof(QuickFiler.ItemViewer).GetProperty("L0vhBreadcrumb_WebView2");
    property.Should().NotBeNull();
    property.PropertyType.Should().Be(typeof(Microsoft.Web.WebView2.WinForms.WebView2));
}
```

**Retyping the Designer-backed member to `IBreadcrumbWebHost` (or any interface) fails this test.**
This is the "retyped Designer field breaks reflection-injected tests" finding, and it is a *live,
committed, passing* test on our branch — not historical prose. It is one of the 29 tests #400
registered as its non-numeric-adapter evidence
(`coverage-accounting-scope-change.2026-07-21T18-01.md:95`).

Two further hard constraints in the same file:

- `:102-130` `HostNeutralPopupOpenOrchestration_IsOwnedByInstrumentedCoordinator` asserts
  `BreadcrumbDropDownOpenCoordinator` is `internal` **and carries no `ExcludeFromCodeCoverageAttribute`**,
  and that `ItemViewer` declares no `OpenBreadcrumbDropDownAsync`.
- `#400 remediation-plan.2026-07-21T21-37.md:58` and `spec.md:255` (AC-17) forbid adding
  hand-written runtime behavior to `ItemViewer.Designer.cs`. `evidence/qa-gates/batch5-format-size.2026-07-21T16-50.md:37-42`
  proves the file was kept byte-identical (sha256 `0AB37A8F…D356A5F`).

### 5.3 The working pattern — inject the host/router, do not retype the control

The evidenced approach, in the same test file:

- `:31-49` `ConfigureBreadcrumbDropDown(CoreWebView2Environment, IWebViewCoreInitializer)` — the
  production overload; the *initializer* is the seam.
- `:51-74` `ConfigureBreadcrumbDropDown(IBreadcrumbDropDownHost, Func<Rectangle>, Func<Rectangle>)`
  — the injected overload; the *host* and the *screen-geometry providers* are the seams.

For `WebView2BreadcrumbHost` specifically, the seam already exists and is already used by
production: `IBreadcrumbWebHost` (`QuickFiler/Viewers/IBreadcrumbWebHost.cs`), consumed by
`QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` (F12) via `Mock<IBreadcrumbWebHost>`. Its doc
comment (`IBreadcrumbWebHost.cs:6-10`) states the design intent verbatim: "Implemented by the
coverage-exempt `WebView2BreadcrumbHost` adapter and mocked in router tests, so the non-exempt bridge
router never touches WebView2 types directly."

**Therefore F13 must NOT retype `_l0vhBreadcrumb_WebView2`.** The correct move is to cover
`WebView2BreadcrumbHost` *as an adapter over a supplied `WebView2` control*, using the techniques in
§5.4, and to leave the Designer field alone.

### 5.4 Proven in-repo techniques that make the WebView2 trio reachable

Discovered in `QuickFiler.Test/Viewers/BreadcrumbPopupUiOperationsDirectAdapterTests.cs`
(302 lines; already green; already in the csproj at line 65):

| Technique | Location | Why it matters to F13 |
|---|---|---|
| `FormatterServices.GetUninitializedObject(typeof(CoreWebView2))` | `:176`, `:197` | Produces a real `CoreWebView2` instance with **no browser process**. This is the key that makes `WebView2Messenger`'s internal ctor `(CoreWebView2, BreadcrumbUiDispatcher)` at `WebView2Messenger.cs:36` constructible in a unit test. |
| `FormatterServices.GetUninitializedObject(typeof(Control))` | `:198` | Same trick for WinForms controls. |
| `QueuedCreatorThreadSynchronizationContext` + `DrainOnCreatorThread()` | `:274-300` | Deterministic fake `SynchronizationContext` with manual pumping — **no `Thread.Sleep`, no `Task.Delay`, no real UI thread**. Satisfies the determinism rules in `.claude/rules/general-unit-test.md`. |
| `new BreadcrumbUiDispatcher(queue, _ => {})` | `:231` | The dispatcher's `internal` 2-arg ctor (`BreadcrumbUiDispatcher.cs:25`) is directly constructible with a fake context and an error sink. |
| `BreadcrumbPopupUiOperations.CreateForCurrentThreadTests()` / `CaptureCurrentOrTests()` | `BreadcrumbPopupUiOperations.cs:83-89` | Existing owner-thread-only test dispatcher factories. |
| Plain `[TestClass]` construction of `ToolStripDropDown` / `ToolStripControlHost` | `BreadcrumbDropDownHostTests.cs:24-59` | **No STA thread is required** for these WinForms types in this suite. The STA last-resort clause is likely unnecessary for F13. |

Also relevant: `WebView2BreadcrumbHost.InitializeAsync` (`:92-113`) does `await uiSyncContext`,
which resolves to the public extension `SynchronizationContextAwaiter GetAwaiter(this SynchronizationContext)`
at `UtilitiesCS/Threading/UiThread.cs:108`. A fake `SynchronizationContext` therefore drives it, and
everything downstream (`CreateEnvironmentAsync`, `EnsureCoreWebView2Async`) is behind the injected
`IWebViewCoreInitializer` and mockable with Moq.

**Assessment of the three exemptions:**

| File | Recommended disposition | Rationale |
|---|---|---|
| `WebView2Messenger.cs` (147) | **Remove the exemption; cover it.** | The internal ctor takes an injectable `BreadcrumbUiDispatcher`; `CoreWebView2` is obtainable via `GetUninitializedObject`; `PostJson`/`Dispose`/`OnWebMessageReceived` all route through the dispatcher, which catches and reports. Only the raw `PostWebMessageAsJson` / event-add IL is host-bound, and it is one line each. |
| `WebView2BreadcrumbHost.cs` (143) | **Remove the exemption; cover most of it.** | `InitializeAsync` is 100% behind the `IWebViewCoreInitializer` mock + a fake `SynchronizationContext`. `PostMessageJson`'s null-core branch (`:74-81`) is directly testable. Ctor null-guards are testable. The residual host-bound surface is `NavigateToString` (`:68`) and the success half of `OnCoreInitializationCompleted` (`:129-135`), which needs a live `CoreWebView2` on the control. Consider extracting that residual into a 3-line adapter and covering the rest. |
| `WebView2CoreInitializer.cs` (30) | **Retain the exemption; argue the irreducible remainder.** | Two expression-bodied members, each a single call into `CoreWebView2Environment.CreateAsync` / `control.EnsureCoreWebView2Async`. Executing either starts a browser process. This is the canonical irreducible remainder and is already the precedent the other two cite. |

---

## 6. F1 upstream contract

### 6.1 What exists today on this branch

`Glob docs/features/epics/quickfiler-per-file-coverage/*` returns exactly one file: `epic.md`.

- **`docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md` DOES NOT EXIST.**
- No F1 feature folder exists under `docs/features/active/` (F1 is manifest entry `issue_num: 1001`,
  `feature_folder: quickfiler-coverage-denominator-and-exemption-ledger` — `epic.md:22-24`; the
  brief's name `quickfiler-coverage-ledger` does not match the manifest).
- No per-file coverage harness script exists. `Glob **/Invoke-MSTestWithCoverage.ps1` returns only
  `scripts/vscode/Invoke-MSTestWithCoverage.ps1`, which is the pre-existing whole-repo collector,
  not a per-file reporter.

### 6.2 Exact Phase 0 halt-gate path

The plan's Phase 0 must test, from repo root:

```
docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md
```

This is the only F1 deliverable the epic names with a concrete path (`epic.md:262-264` requires
children to commit per-file results under `<FEATURE>/evidence/qa-gates/`; `epic.md:509-536`
describes the ledger's buckets and harness rules but assigns the harness no path).

**Recommendation:** gate on the ledger path alone, and treat the harness as a soft dependency with a
documented fallback — derive per-file line/branch rate directly from the Cobertura produced by
`scripts/vscode/Invoke-MSTestWithCoverage.ps1`, applying Directive B (denominator = `<line>` child
count, never `line-rate`) and Directive A (union same-`filename` `<class>` elements, max hits per
line) even though §3.5 shows Directive A is currently a no-op.

### 6.3 The ledger's three buckets

Per `epic.md:509-522`:

1. **`testable`** — must reach >= 80% line (issue #136 AC1) and >= 75% branch
   (`.claude/rules/general-unit-test.md`). Newly created files take >= 90% line
   (`CLAUDE.md` §UT2).
2. **`ratified-exempt`** — untestable production logic argued against the three exemption grounds
   in `epic.md:206-225` (VSTO lifecycle; WinForms form-derived/Designer; Outlook-interop event
   handlers **without an injectable seam**). Carries `[ExcludeFromCodeCoverage]`.
3. **`interface-only / not-measured`** — zero coverable lines. **Reported N/A, never 0%, never a
   failure, and receives NO `[ExcludeFromCodeCoverage]`.** Shape-assertion tests written purely to
   manufacture coverage for such files are **prohibited** (`epic.md:521-522`).

### 6.4 F13's proposed bucket assignment (for F1 to ratify)

| Bucket | Files |
|---|---|
| `testable` | `BreadcrumbDropDownHost.cs`, `BreadcrumbDropDownOpenLifetime.cs`, `BreadcrumbDropDownOpenCoordinator.cs`, `BreadcrumbCollapsedSurfaceController.cs`, `BreadcrumbUiDispatcher.cs`, `BreadcrumbWebViewSurfaceFactory.cs`, `BreadcrumbPopupPlacement.cs`, `BreadcrumbPopupUiOperations.cs`, **plus** `WebView2BreadcrumbHost.cs` and `WebView2Messenger.cs` after exemption removal (§5.4) |
| `ratified-exempt` | `WebView2CoreInitializer.cs`; the seven member-level exemptions in `BreadcrumbPopupUiOperations.cs` (§3.4), each individually argued |
| `interface-only / not-measured` | `IBreadcrumbDropDownHost.cs`, `IBreadcrumbWebHost.cs`, `IWebViewCoreInitializer.cs`, `IWebViewMessenger.cs` |

**Precision note for F1:** `IBreadcrumbDropDownHost.cs` is not a pure interface file. Lines 9-16
declare `public enum BreadcrumbDropDownCloseReason`. An enum emits no executable IL and produces no
Cobertura `<class>` element, so the `interface-only / not-measured` classification still holds — but
the ledger rationale should say "interface + enum declaration, no executable IL", not "interface
only", so a future reader does not think the file was misclassified.

---

## 7. `QuickFiler.csproj` mechanics and `InternalsVisibleTo`

### 7.1 Non-SDK, explicit `<Compile Include>`, no globbing — CONFIRMED

`QuickFiler/QuickFiler.csproj` lists every source file individually. The F13 block is contiguous at
lines **396-411**:

```
396  <Compile Include="Viewers\BreadcrumbUiDispatcher.cs" />
397  <Compile Include="Viewers\BreadcrumbPopupUiOperations.cs" />
398  <Compile Include="Viewers\BreadcrumbDropDownOpenLifetime.cs" />
399  <Compile Include="Viewers\BreadcrumbDropDownOpenCoordinator.cs" />
401  <Compile Include="Viewers\BreadcrumbPopupPlacement.cs" />
402  <Compile Include="Viewers\IBreadcrumbDropDownHost.cs" />
403  <Compile Include="Viewers\BreadcrumbDropDownHost.cs" />
404  <Compile Include="Viewers\BreadcrumbCollapsedSurfaceController.cs" />
405  <Compile Include="Viewers\BreadcrumbWebViewSurfaceFactory.cs" />
406  <Compile Include="Viewers\IWebViewCoreInitializer.cs" />
407  <Compile Include="Viewers\IBreadcrumbWebHost.cs" />
408  <Compile Include="Viewers\WebView2BreadcrumbHost.cs" />
409  <Compile Include="Viewers\IWebViewMessenger.cs" />
410  <Compile Include="Viewers\WebView2CoreInitializer.cs" />
411  <Compile Include="Viewers\WebView2Messenger.cs" />
```

F12-owned entries sit at 393-395 and 400 (`BreadcrumbBridgeCoordinator.cs`,
`BreadcrumbCoordinatorUpgradeLifetime.cs`, `BreadcrumbItemViewerLifecycleCoordinator.cs`,
`BreadcrumbMessengerHub.cs`) — **interleaved with ours**. Expect a textual conflict with F12 at
fan-in; resolution is additive (keep both sides), per `epic.md:594-617`.

### 7.2 CRLF — CONFIRMED

`Grep -c '\r$'` on `QuickFiler/QuickFiler.csproj` returns **593** matches in a 593-line file: every
line is CRLF-terminated. Per `epic.md:611-612`, a git-bash `sed -i` would strip CRLF and produce a
whole-file diff guaranteed to conflict. **Use the `Edit` tool, or `perl -0777` with explicit `\r\n`.**

### 7.3 What a child must do to add a production file

1. Create `QuickFiler/Viewers/<NewFile>.cs` (<= 500 lines).
2. Add exactly one `<Compile Include="Viewers\<NewFile>.cs" />` line **inside the existing F13 block
   (396-411)**, preserving CRLF, touching no other line, changing no property or reference, and
   reordering nothing (`epic.md:606-610`).
3. Append a ledger row for the file in the same change (`epic.md:578-581`), defaulting to `testable`
   at >= 90% line (`epic.md:583-585`).

The mirror obligation for tests: add `<Compile Include="Viewers\<NewFile>Tests.cs" />` to
`QuickFiler.Test/QuickFiler.Test.csproj` (existing breadcrumb block at lines 58-91).

### 7.4 `InternalsVisibleTo` — both questions answered

| Assembly | Grants to `QuickFiler.Test`? | Evidence |
|---|---|---|
| `QuickFiler` | **YES** | `QuickFiler/Properties/AssemblyInfo.cs:5` — `[assembly: InternalsVisibleTo("QuickFiler.Test")]` |
| `UtilitiesCS` | **NO** | `UtilitiesCS/Properties/AssemblyInfo.cs:18-20` grants only `DynamicProxyGenAssembly2`, `UtilitiesCS.Test`, `ToDoModel.Test` |

**Consequence:** `internal` seams inside `QuickFiler` are directly usable — which is why
`BreadcrumbUiDispatcher`, `BreadcrumbPopupUiOperations`, `BreadcrumbDropDownOpenLifetime`,
`BreadcrumbDropDownOpenCoordinator`, `BreadcrumbCollapsedSurfaceController`,
`BreadcrumbWebViewSurfaceFactory` and `BreadcrumbNavigationReadiness` are all `internal` and all
already tested. F13 should keep new seams `internal`, matching the established style
(`.claude/rules/csharp.md`: "Prefer `internal` for non-public APIs").

Any `UtilitiesCS` internal remains unreachable. Per `epic.md:619-631`, build a local seam inside the
F13 assignment rather than editing `UtilitiesCS/Properties/AssemblyInfo.cs`.

---

## 8. Test-harness command reality

### 8.1 `scripts/vscode/Invoke-MSTestWithCoverage.ps1` — actual signature

```powershell
param(
    [string]$SearchRoot,                                        # default '.'  (relative to repo root)
    [string]$Configuration,                                     # default 'Debug'
    [string]$CoverageOutput = "coverage\coverage.cobertura.xml", # relative to repo root
    [switch]$NoExecute
)
```

Behavior (lines 248-344):

- `$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..\..')`.
- Discovers assemblies: `Get-ChildItem -Path $repoRoot/$SearchRoot -Recurse -Filter '*.Test.dll'`
  filtered to `\bin\<Configuration>\`, excluding `\obj\` and `\ref\` (lines 296-302).
- Resolves `vstest.console.exe` via `vswhere.exe` (lines 279-290); throws if absent.
- Requires the global tool `dotnet-coverage` (lines 292-294).
- Runs (lines 70-77):
  `dotnet-coverage collect --output <out> --output-format cobertura --settings <derived coverage.config> -- <vstest.console.exe> <assemblies…> /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:TestCategory!=LiveOutlook`
- Post-processes the Cobertura to workspace-relative paths, injects `<sources>`, and strips
  third-party `<package>` elements (lines 333-343).

Output location: `<repoRoot>/<CoverageOutput>`, default `<repoRoot>/coverage/coverage.cobertura.xml`.

Supporting config:
- `coverage.config` (repo root) excludes only third-party modules (`Deedle`, `FSharp`, `Castle.Core`,
  `FluentAssertions`, `Moq`, `Microsoft.Testing`, `MSTest`) — **no QuickFiler exclusion**.
- `scripts/vscode/TaskMaster.cli.runsettings` sets `MSTest/Parallelize Workers=0 Scope=ClassLevel`.
  **Class-level parallelism is active** — F13 tests must not share mutable static state across test
  classes. (`BreadcrumbUiDispatcher` uses a `[ThreadStatic]` field at `:14-15`; that is per-thread
  and safe, but any new static seam is not.)

### 8.2 Single-assembly runs report a false-low repo-wide number — CONFIRMED MECHANISM

The `<coverage line-rate>` header is computed over **whatever assemblies the run instrumented**.
Restricting `-SearchRoot QuickFiler.Test` instruments a smaller set, so the repo-wide figure is not
comparable to a full-suite figure. #424's own evidence documents exactly this instability
(`coverage-delta.2026-08-07T00-48.md:65`: denominator grew 38.6% between two full-suite runs).

**F13 must run the full `*.Test.dll` set (default `-SearchRoot '.'`) for every repo-wide figure it
cites, and must capture the before/after pair in the same session.**

### 8.3 Known local hazard — stale agent-worktree builds

The discovery filter (lines 296-302) excludes `\obj\` and `\ref\` but **not** `.claude\worktrees\`.
Run from the main repository root, it will pick up `*.Test.dll` from every agent worktree under
`.claude/worktrees/`, producing duplicate/stale assemblies and spurious failures. Run from **this
worktree root** (`$repoRoot` resolves to the worktree), which contains no nested worktrees, or filter
`\.claude\` explicitly.

### 8.4 CI

`.github/workflows/ci.yml:129-131` resolves `vstest.console.exe` via `vswhere` in the same way. CI
does **not** invoke `Invoke-MSTestWithCoverage.ps1` by name; it reproduces the vswhere/vstest
resolution inline. `CLAUDE.md` §CUT3 states the canonical local command as
`vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`. The wrapper script is the mechanism
that actually produces the Cobertura the epic consumes, so cite the wrapper in evidence.

---

## 9. Latent defect candidates (for MCP promotion by the orchestrator — do NOT fix in F13)

Per `epic.md:538-546`, these must become GitHub issues, not feature-folder prose. Listed with a
confidence marker because no test was run to confirm any of them.

| ID | Location | Impact | Confidence |
|---|---|---|---|
| **L1** | `QuickFiler/Viewers/WebView2BreadcrumbHost.cs:49-50` | **Handler-retention leak across pooled `ItemViewer` reuse.** The ctor does `_control.CoreWebView2InitializationCompleted -= OnCoreInitializationCompleted; += OnCoreInitializationCompleted;`. Delegate equality is instance-based, so a *new* host over the *same* Designer control unhooks only its own (absent) handler; every prior `WebView2BreadcrumbHost` instance stays subscribed. The type implements no `IDisposable`. Same pattern for `core.WebMessageReceived` at `:131-132`. Symptom: duplicate `MessageReceived`/`CoreInitialized` fan-out and retained hosts after `ItemViewerQueue` recycles a viewer. | High |
| **L2** | `QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs:95` | **Lock held across an outward call.** `_currentOpenTask = OpenCoreAsync(_generation);` runs inside `lock (_sync)`. `OpenCoreAsync`'s synchronous prologue reaches `BreadcrumbPopupUiOperations.RunAsync` → `BreadcrumbUiDispatcher.DispatchValue`, which executes **inline** when `_executingDispatcher == this` (`BreadcrumbUiDispatcher.cs:166-178`). That inline path runs `BeginOpenCore`, which calls `_host.OpenAsync(...)` at `:195` and thence `BreadcrumbDropDownOpenLifetime.OpenAsync`, which takes a *different* lock (`BreadcrumbDropDownOpenLifetime.cs:53`). Lock order `Coordinator._sync → Lifetime._sync` is therefore established on the inline path; any reverse order elsewhere is a deadlock. | Medium |
| **L3** | `QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.cs:229-230` | Null-forgiving dereference `_host.InstalledControlHost!` / `_host._popupControl!` after `EnsureSurfaceAsync` returned `true` via the `HasInstalledSurface` short-circuit at `:309-310`. A concurrent `BreadcrumbDropDownHost.Reset()` → `DisposeSurfaceAsync` → `TakeOwnedSurface()` (`BreadcrumbDropDownHost.cs:367-383`) nulls both fields. Both paths are dispatcher-serialized today, so this is latent rather than active. | Low–Medium |
| **L4** | `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs:394` and `:457` | **`[ExcludeFromCodeCoverage]` does not suppress instrumentation of lambdas nested inside the attributed member.** Source lines 406, 409 and 471-490 remain instrumented and permanently uncovered (22 of the file's 24 uncovered lines). This silently misstates the exemption boundary for any file using this pattern — a repo-wide measurement concern, not just an F13 one. | High (measured, §3.7) |
| **L5** | `QuickFiler/Viewers/WebView2Messenger.cs:40-48` | Fire-and-forget subscription in the constructor. `_ = _dispatcher.Dispatch(...)`; `Dispatch` catches and merely *reports* (`BreadcrumbUiDispatcher.cs:85-89`). A failed `WebMessageReceived` subscription leaves `_subscribed == false` and `MessageReceived` permanently silent, with no observable failure at the call site. Contradicts `CLAUDE.md` §3 "fail fast and explicitly". | Medium |
| **L6** | `QuickFiler/Viewers/WebView2BreadcrumbHost.cs:129` | `CoreWebView2 core = _control.CoreWebView2;` is dereferenced at `:131` immediately after `e.IsSuccess`. A disposal race between initialization completion and the event callback yields a `NullReferenceException` on a WinForms event handler (unhandled). | Low–Medium |
| **L7** | `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs:349` and `QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.cs:197` | Bare `catch { }` with no rethrow. `:349` has no explanatory comment. `.claude/rules/general-code-change.md` prohibits silent swallowing; Sonar/Meziantou will flag both. Test-policy adjacent, low runtime risk. | High (textual) |
| **L8** | `QuickFiler.Test/Controllers/WebView2CoreInitializerTests.cs` | The test for `QuickFiler/Viewers/WebView2CoreInitializer.cs` lives under `Controllers/`, breaching the mirror-the-production-tree requirement in `.claude/rules/general-unit-test.md` § Test File Location. In-scope for F13 to relocate. | High (textual) |

L8 (and any test-policy breach in existing F13 tests) is **in-scope for F13's own execution**, on the
precedent set for F4 at `epic.md:556-558`. L1-L7 are production behaviour and must be deferred to
issues under the epic's no-behaviour-change NFR.

---

## 10. Sibling-boundary coupling (note only; no edits)

F12-owned and F14-owned files that our files depend on, or that depend on ours:

| Direction | Our file | Their file | Coupling |
|---|---|---|---|
| **we depend on them** | `BreadcrumbPopupUiOperations.cs:401,414,466` | `Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs:355` (`BreadcrumbPopupLifecycleOperations`) and `:337` (`BreadcrumbNavigationSubscription`) — **F12** | `CreateNavigationSurface`, `DisposeTwoResources`, `NavigateWithSubscription`. Tests we write that exercise these credit **F12's** file, not ours. |
| they depend on us | `IBreadcrumbWebHost.cs`, `WebView2BreadcrumbHost.cs` | `Controllers/BreadcrumbBridgeRouter.cs` (3 refs) — **F12** | Router is mocked against `IBreadcrumbWebHost`. Any signature change to that interface breaks F12. |
| they depend on us | `IWebViewMessenger.cs`, `BreadcrumbCollapsedSurfaceController.cs` | `Viewers/BreadcrumbMessengerHub.cs` (18 refs), `Viewers/BreadcrumbBridgeCoordinator.cs` (10 refs) — **F12** | Hub attaches to the collapsed controller's `ReadyMessenger`. |
| they depend on us | `BreadcrumbDropDownHost.cs`, `IBreadcrumbDropDownHost.cs`, `BreadcrumbDropDownOpenCoordinator.cs` | `Viewers/ItemViewer.Breadcrumb.cs` (20 refs) — **F14** | `ConfigureBreadcrumbDropDown` overloads; pinned by `ItemViewerBreadcrumbDropDownContractTests`. |
| they depend on us | our host/messenger types | `Controllers/QfcItemController.cs`, `.Initialization.cs`, `.ViewerSetup.cs` (5 refs) — **F10** | `QfcItemController.InitializeWebViewAsync` and `EnsureBreadcrumbPipeline` are pre-existing method-level exemptions (`coverage-accounting-scope-change.2026-07-21T18-01.md:40-43`). |
| they depend on us | `IWebViewMessenger.cs` | `Controllers/BreadcrumbOutboundQueue.cs` (4 refs) — **F2**; `Controllers/EfcFormController.cs` (3 refs), `Viewers/EfcViewer.cs` (1 ref) — **F9** | Interface consumers only. |

**Rule for F13: treat every `public`/`internal` signature in our 15 files as frozen.** Six sibling
children compile against them.

---

## 11. Recommended shape of F13's work (evidence-based)

Given §3.2, the child is **not** a gap-closure exercise on the eight instrumented files. It is:

1. **Primary — exemption removal on the WebView2 pair.** `WebView2BreadcrumbHost.cs` (143) and
   `WebView2Messenger.cs` (147) start at N/A and must land >= 80% line / >= 75% branch using the
   `FormatterServices.GetUninitializedObject` + fake-`SynchronizationContext` techniques already
   proven in `BreadcrumbPopupUiOperationsDirectAdapterTests.cs`. This is where nearly all the real
   testing effort sits. New test files required (no headroom in existing ones).
2. **Secondary — member-level exemption audit** on `BreadcrumbPopupUiOperations.cs`'s seven
   attributes (§3.4), each argued individually against the three exemption grounds, with the L4
   lambda-instrumentation finding recorded.
3. **Tertiary — irreducible-remainder rationale** for `WebView2CoreInitializer.cs`, plus F1 ledger
   rows for the four `interface-only / not-measured` files with the enum caveat from §6.4.
4. **Retention** — re-measure the eight already-passing files and prove no regression. Branch is the
   tighter of the two gates but is not currently in breach anywhere in scope.
5. **Structural** — relocate `WebView2CoreInitializerTests.cs` to `QuickFiler.Test/Viewers/` (L8),
   and promote L1-L7 as issues.

Do **not** budget effort for: retyping the Designer field (§5.2 forbids it), STA infrastructure
(§5.4 shows it is unnecessary), or a union step for duplicate `<class>` elements (§3.5 shows there
are none).

---

## 12. Corrections to the delegation brief, consolidated

| # | Brief claim | Finding |
|---|---|---|
| 1 | `BreadcrumbPopupUiOperations.cs` is in the "Production (11)" set marked `[X]` in the epic | **Corrected.** No type-level exemption. Seven member-level exemptions at lines 105, 380, 383, 390, 394, 412, 457. The file is instrumented at 92.94% line / 86.88% branch. |
| 2 | "Three WebView2 files are ABSENT from the report" | **Confirmed**, with root cause at `WebView2BreadcrumbHost.cs:29`, `WebView2Messenger.cs:20`, `WebView2CoreInitializer.cs:15`. |
| 3 | "one source file can produce multiple `<class>` elements sharing one `filename` — union them" (epic Directive A) | **Refuted for this report writer.** Exactly one `<class>` per `filename` in both the #424 and #400 reports; multi-type files and lambda closures are already merged. Directive B remains necessary. |
| 4 | "the brief supplied no branch figures at all" | **Supplied.** See §3.2. All eight instrumented files are at 85.90%-100% branch — every one already clears the 75% floor. |
| 5 | Implied premise that F13 is a coverage-gap child | **Refuted for 8 of 11 files.** They clear both gates today. The genuine work is exemption removal on 2-3 files. (Issue #455's own `issue.md:21-27` already states this correctly; the brief's framing is what understates it.) |
| 6 | Epic's "merge-base repository line rate of 70.19%" | **Context needed.** That is #424's merge-base. #424's post-change figure is 85.65% line / 79.00% branch, and its own evidence (`coverage-delta.2026-08-07T00-48.md:65`) warns the two are not like-for-like (+38.6% denominator drift). F13 must measure its own before/after pair in one session. |
| 7 | "at least 20 breadcrumb/WebView test files" | **Confirmed and refined.** 31 files in `QuickFiler.Test/Viewers/` plus 2 in `Controllers/`; 19 are F13-primary, 11 F12-primary, 1 asset-contract. |
| 8 | F1 folder name `quickfiler-coverage-ledger` | **Manifest disagrees.** `epic.md:22-24` names it `quickfiler-coverage-denominator-and-exemption-ledger` (issue placeholder 1001). Neither the folder nor `coverage-ledger.md` exists yet. |
| 9 | Section 4 open-issue scan | **Not performed.** No `gh` tool in this session. Filesystem proxy only; must be re-run. |
