# quickfiler-breadcrumb-dropdown-webview-coverage (Issue #455)

- Date captured: 2026-08-07
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/quickfiler-breadcrumb-dropdown-webview-coverage/ (Issue #455)
- Parent epic: #136 (QuickFiler per-file 80% coverage)
- Epic child: F13 of `quickfiler-per-file-coverage`
- Integration branch: `epic/quickfiler-per-file-coverage-integration`
- Upstream dependency: F1 `quickfiler-coverage-ledger` (per-file coverage harness + classification ledger)

- Issue: #455
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/455
- Last Updated: 2026-08-08
- Work Mode: full-feature

## Problem / Why

Child F13 of epic #136 owns the QuickFiler breadcrumb drop-down surface and the WebView2 host
(15 compiled files, ~3,111 lines under `QuickFiler/Viewers/`). The file set has an unusual shape:

- Eight non-exempt coordinator files already measure 92.9%-100% line coverage, so the remaining
  work for them is branch coverage against the 75% floor plus explicit error-path and
  cancellation-path pinning — not new happy-path tests.
- Three WebView2 files (`WebView2BreadcrumbHost.cs`, `WebView2Messenger.cs`,
  `WebView2CoreInitializer.cs`) carry class-level `[ExcludeFromCodeCoverage]` and are therefore
  absent from instrumentation entirely. They are unmeasured, not covered, and will start near zero
  once their exemptions are reconsidered. This is where the genuine coverage work is concentrated.
- `BreadcrumbPopupUiOperations.cs` carries method-level `[ExcludeFromCodeCoverage]` attributes on
  individual UI-bound members rather than a single file-level attribute.
- Four files are interface-only declarations with no executable IL and belong in F1's
  `interface-only / not-measured` bucket.

## Verified Findings and Documented Deviations (research, 2026-08-07)

Twelve per-file research artifacts under `research/` supersede several premises above and in the
epic manifest. Per-file coverage was recomputed independently from
`docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml`
by summing class-level `<line>` children, deduplicating by line number with max(hits), and keying on
`filename` rather than `<class name>`. `<class>` `line-rate`/`branch-rate` attributes are **not**
trustworthy (issue #441).

### D1. The branch-coverage premise is refuted

All eight instrumented files already clear both the 80% line floor and the 75% branch floor:

| File | Line | Branch | Uncovered lines |
| --- | --- | --- | --- |
| `BreadcrumbDropDownHost.cs` | 99.42% | 91.49% | 335, 377 |
| `BreadcrumbDropDownOpenLifetime.cs` | 99.13% | 91.86% | 197, 238, 359 |
| `BreadcrumbDropDownOpenCoordinator.cs` | 98.25% | 92.05% | 93, 107, 187, 242 |
| `BreadcrumbCollapsedSurfaceController.cs` | 98.97% | 85.71% | 198, 199 |
| `BreadcrumbUiDispatcher.cs` | 100% | 97.22% | none |
| `BreadcrumbWebViewSurfaceFactory.cs` | 99.29% | 97.62% | 222 |
| `BreadcrumbPopupPlacement.cs` | 100% | 100% | none |
| `BreadcrumbPopupUiOperations.cs` | 90.7% | 88.3% | 58, 325, 406, 409, 471-490 |

The acceptance bar for these eight is therefore **retain-or-improve**, not gap closure.

### D2. The exemption count is understated

`BreadcrumbPopupUiOperations.cs` carries **seven** method-level attributes (lines 105, 380, 383,
390, 394, 412, 457), not one file-level attribute. The in-scope total is **3 class-level + 7
method-level = 10**, not four.

### D3. `[ExcludeFromCodeCoverage]` does not propagate to nested lambdas (issue #457)

Method-level attributes leak nested lambda bodies into the denominator as permanently-uncovered
lines; **class-level attributes do not**. 22 of `BreadcrumbPopupUiOperations.cs`'s 24 uncovered
lines are this defect. The convention for this child is therefore **class-level-exempt adapter
types**, deviating from the method-level precedent in that same file.

### D4. One exemption in scope is unjustified and is a Blocking finding

`BreadcrumbPopupUiOperations.DisposeProductionSurface` (line 412) touches no WebView2 type — its
signature is `(Control?, IWebViewMessenger?)` — and its lambda bodies at lines 415-416 already
report `hits="1"`. Existing tests execute it. Per epic.md:223 the attribute must be removed.

### D5. `CLAUDE.md` §UT2's three exemption grounds do not cover any WebView2 file

None is a VSTO lifecycle class, none is form-derived or Designer-generated, and none imports an
Outlook Interop type. All three current attributes rest on a ground that does not textually exist.
**F1 (#432) must ratify a narrow fourth ground or classify them testable with a documented
exception. F13 cannot ratify this itself.**

### D6. Per-file exemption verdicts

- `WebView2Messenger.cs` — **remove exemption**. Only ~5 of ~70 coverable lines are SDK statements.
- `WebView2BreadcrumbHost.cs` — **remove exemption**. `InitializeAsync` is already testable today
  behind the `IWebViewCoreInitializer` seam already injected into its constructor.
- `WebView2CoreInitializer.cs` — **retain**, genuinely irreducible. Executing either member is
  *prohibited*, not merely hard: it creates a user-data folder on disk (§UT4 bans temp files,
  approved exceptions none) and requires the Evergreen runtime (external process). Its stated
  "1:1 forwarding" rationale is false and must be restated (issue #477).

### D7. No injected clock or fake timers are required

There is no `DateTime`, `Stopwatch`, `Timer`, `Task.Delay`, `Thread.Sleep`, or `TimeProvider`
anywhere in the drop-down lifetime files. Determinism here is **scheduler** control, not clock
control. The vehicle already exists: a manually-pumped fake `SynchronizationContext` with explicit
`Drain()`, green at `BreadcrumbPopupUiOperationsDirectAdapterTests.cs:274-300`.

### D8. No STA is required anywhere in this child

`BreadcrumbPopupControlDispatchTests.cs` is a plain `[TestClass]` already constructing `Panel`,
`ToolStripDropDown`, and `ToolStripControlHost` in memory. No proposed test constructs a WinForms
control.

### D9. Provably unreachable outcomes — the plan must not target 100%

- `BreadcrumbCollapsedSurfaceController.cs:245-246` — ceiling 95.24% branch.
- `BreadcrumbUiDispatcher.cs:276` — unreachable across all 24 construction sites; ceiling 97.22%.
- `BreadcrumbWebViewSurfaceFactory.cs:221-222` — Roslyn `catch { await …; throw; }` artifact.
- `BreadcrumbDropDownOpenLifetime.cs:359` — ceiling 99.13% line.
- `BreadcrumbPopupUiOperations.cs:325`, `:324` — `await` inside `catch` (issue-457 class).

### D10. Conflict risk #400 is resolved

#400 merged as PR #416 on 2026-08-04; commit `294132b4` is an ancestor of HEAD. It authored all 15
F13 files, and its committed report matches #424's byte-for-byte per file. Open issue **#440**
(breadcrumb arrow-key navigation) is a live behavior bug in adjacent territory, in no active folder.

### D11. New harness directive for F1 (stronger than epic Directive B)

Key on `filename=`, never `<class name=>`, and sum **class-level** `<lines>` children only. Proven
twice: `BreadcrumbPopupPlacement.cs`'s only `<class>` is named `…BreadcrumbPopupPlacementResult` and
its `<methods>` block covers only the struct constructor (undercount 91.7%);
`BreadcrumbWebViewSurfaceFactory.cs`'s only `<class>` is named `…BreadcrumbNavigationReadiness` and
omits the static factory type entirely. `BreadcrumbDropDownOpenLifetime.cs` reports as
`BreadcrumbDropDownOpenLease`. Epic Directive A (union multiple `<class>` per filename) is a no-op
for this writer.

### D12. Latent defects promoted (not fixed here)

#457, #458, #462, #475, #476, #477. All are behavior changes barred by the no-behavior-change NFR.

### D13. In-scope items beyond coverage

- Remove the unjustified `DisposeProductionSurface` exemption (D4).
- Relocate `QuickFiler.Test/Controllers/WebView2CoreInitializerTests.cs` to `Viewers/` to satisfy
  the mirror-layout rule.
- `BreadcrumbPopupPlacementTests.cs:138-155` anchors reflection on `typeof(BreadcrumbBridgeCoordinator)`,
  an **F12-owned** type used only as an assembly handle. This is a cross-child compile coupling
  invisible to a file-set disjointness check and should be re-anchored on an F13-owned type.

## Proposed Behavior

Raise every `testable` file in the F13 assignment to at least 80% line and 75% branch coverage,
verified with F1's per-file harness, and either remove the `[ExcludeFromCodeCoverage]` attributes
with the code genuinely covered or retain only a file-specific irreducible remainder argued against
the three exemption grounds in the epic's Shared Design section 1. No observable behavior change to
QuickFiler flows.

## Acceptance Criteria (early draft)

- [ ] Every `testable` file in scope reaches >= 80% line and >= 75% branch coverage, measured with
      F1's harness and recorded under `<FEATURE>/evidence/qa-gates/`.
- [ ] For files already above the line floor, line coverage is retained or improved and the branch
      floor is met.
- [ ] `[ExcludeFromCodeCoverage]` attributes in scope are removed with the code covered, or retained
      only where a file-specific irreducible-remainder rationale is recorded and F1's ledger
      ratifies it.
- [ ] The four interface-only files are classified `interface-only / not-measured`, receive no
      `[ExcludeFromCodeCoverage]`, and are reported N/A rather than 0%.
- [ ] No production file in scope exceeds 500 lines; any newly created file reaches >= 90% line
      coverage and appends its own ledger row plus `<Compile Include>` entry.
- [ ] Tests use MSTest, Moq, and FluentAssertions; deterministic and isolated; no temporary files,
      external services, live forms, popups, `Thread.Sleep`, `Task.Delay`, or wall-clock waits.
- [ ] Full C# toolchain green in final form; repository-wide coverage retained or improved.
- [ ] No behavior change to observable QuickFiler flows.

## Constraints & Risks

- In-flight conflict risk with issue #400
  (`docs/features/active/2026-07-21-quickfiler-folder-selector-dropdown-400`), which targets the
  same breadcrumb folder-selector drop-down and carries its own committed Cobertura evidence.
- Drop-down open/close lifetime and WebView2 initialization carry concurrency and ordering
  invariants. Cancellation, double-open, open-during-close, and initialization-failure paths must be
  covered explicitly with an injected clock and fake timers.
- Sibling boundary: `BreadcrumbBridgeRouter`, `BreadcrumbBridgeCoordinator`,
  `BreadcrumbCoordinatorUpgradeLifetime`, `BreadcrumbItemViewerLifecycleCoordinator`, and
  `BreadcrumbMessengerHub` belong to F12. `ItemViewer.Breadcrumb.cs` belongs to F14.
- Seam hierarchy: interface seam, then injectable delegate, then adapter. STA constructions are a
  last resort confined to dedicated `*.StaTests.cs` files.
- Known prior art: a retyped Designer field breaks reflection-injected tests; injecting a router is
  the working approach for the breadcrumb WebView2 surface.

## Test Conditions to Consider

- [ ] Drop-down open/close lifetime: cancellation, double-open, open-during-close, dispose-during-open.
- [ ] WebView2 core initialization: success, failure, re-entrancy, disposal before completion.
- [ ] WebView2 message round-trip: malformed payload, unknown message type, post-after-dispose.
- [ ] Popup placement boundary arithmetic at screen edges.
- [ ] UI dispatcher marshalling when already on and off the target thread.

## Next Step

- [ ] Promote to GitHub issue (feature request template)
- [ ] Create `docs/features/active/quickfiler-breadcrumb-dropdown-webview-coverage/` folder from the template
