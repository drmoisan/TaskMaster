## `QuickFiler/Controllers/BayesianPerformanceController.cs` (156 lines)

- **Epic child:** F15 (`quickfiler-form-viewers-bayesian-coverage`, issue #496), parent epic #136.
- **Measured baseline (epic manifest, indicative, not this branch's authority):** 66.0% line / 57.1% branch, 97 Cobertura-visible lines.
- **Classification (F1 ledger rules, applied directly — F1's ledger file does not exist on disk yet):** `testable`. No `[ExcludeFromCodeCoverage]` attribute present today (verified: no match for `ExcludeFromCodeCoverage` in this file). None of the three CLAUDE.md exemption grounds nor the epic's fourth (prohibited-to-execute adapter) ground apply — the file is a plain, non-Form, non-VSTO-lifecycle class with an injectable-seam path available for every member.

### Current structure

`public class BayesianPerformanceController`, not sealed, not partial. Members:

- Constructor `BayesianPerformanceController(IApplicationGlobals globals)` — trivial field assignment, no branch.
- `Globals`, `Errors`, `ActiveOutcome`, `ActiveError` — plain auto-backed properties via private fields, no branch.
- `internal virtual BayesianPerformanceViewer Viewer { get; private set; }` (backed by `protected BayesianPerformanceViewer _viewer`) — the seam already used by the existing `RunWithViewer` harness (`SetField(controller, "_viewer", viewer)` via reflection, not via the property, because the setter is `private`).
- `public async Task InvestigatePerformance()` (lines 52-70) — the only member with zero existing test coverage. Two null-coalescing branches (`Serialization ??=`, `Errors ??=`), then unconditionally: builds a `BayesianSerializationHelper`, deserializes `Errors`, constructs a `ProgressPackage` and calls `InitializeAsync(...)` **without awaiting or storing the result usefully** (see Latent Defect below), constructs a **real, shown** `BayesianPerformanceViewer` via `new BayesianPerformanceViewer(this).Init()`, populates it, and calls `Viewer.Show()`.
- `public void AssignFormValues(ClassificationErrors error)` — straight-line, no branch. Already covered (`AssignFormValues_WithClassificationError_MapsMetricsAndVerboseOutcomes`).
- `internal void OlvVerboseDetails_SelectionChanged()` — outer guard `if ((objects is not null) && (objects.Count != 0))`, inner `if (!outcome.Drivers.IsNullOrEmpty()) {...} else {...}`.
- `internal void OlvDrivers_SelectionChanged()` — single `if (objects is not null) && (objects.Count != 0) {...} else {...}`.
- `internal void ClassSelector_SelectedIndexChanged()` — `if (ActiveError is not null) { AssignFormValues(ActiveError); }`.
- `internal void ReSortItem()` — resolves a `MailItem` via COM (`Globals.Ol.NamespaceMAPI.GetItemFromID(...)`), then `if (item is not null) { new EfcHomeController(_globals, () => { }, item).Run(); }`.

Dependencies: `IApplicationGlobals` (already an interface seam, injected via constructor), `Microsoft.Office.Interop.Outlook.MailItem` (COM, only in `ReSortItem`), `BayesianPerformanceViewer` (WinForms `Form`, constructed directly — not injected), `BayesianSerializationHelper` and `ProgressPackage` (concrete `UtilitiesCS` types, constructed directly), `EfcHomeController` (concrete controller, constructed directly).

### What is already tested vs. the coverage gap

`QuickFiler.Test/Controllers/BayesianPerformanceControllerTests.cs` (verified by reading it) has 6 `[TestMethod]`s, all routed through the existing `BayesianPerformanceControllerTestSupport.RunWithViewer` harness (STA thread, unshown `BayesianPerformanceViewer`, reflection-seeded `_viewer` field). Covered:

- `AssignFormValues` (positive path).
- `ClassSelector_SelectedIndexChanged` — only the **true** branch of `if (ActiveError is not null)` (a known class is selected).
- `OlvVerboseDetails_SelectionChanged` — both branches of the **inner** `if (!outcome.Drivers.IsNullOrEmpty())`, but only the **true** branch of the **outer** guard (`objects is not null && objects.Count != 0`); no test exercises "nothing selected."
- `OlvDrivers_SelectionChanged` — both branches of its single guard (`WithSelectedToken` = true, `WithoutSelection` = false).

**Untested branches (the coverage gap, matching the epic's "untaken-guard" framing):**

1. `ClassSelector_SelectedIndexChanged` — the **false** branch of `if (ActiveError is not null)` (selecting a class string that does not match any `Errors[].Class`, so `FirstOrDefault` returns `null` and `AssignFormValues` is skipped).
2. `OlvVerboseDetails_SelectionChanged` — the **false** branch of the outer guard (`objects is null`, or `objects.Count == 0` — e.g. no row selected in the `ObjectListView`).
3. `InvestigatePerformance()` — entirely untested, including both `??=` branches (already-populated `Serialization`/`Errors` vs. not) and the unconditional live-Form-construction-and-`Show()` path.
4. `ReSortItem()` — entirely untested, including both branches of `if (item is not null)` and the COM `GetItemFromID` call.

### Latent defect found (flag for promotion, do not fix)

`InvestigatePerformance()` line 58: `var ppkg = (new ProgressPackage()).InitializeAsync(cancelSource: ..., cancel: ..., progressTrackerPane: ...);` returns a `Task` (or `Task<ProgressPackage>`) that is **never awaited and never read** — `ppkg` is not referenced again anywhere in the method. This is an unobserved-task / discarded-initialization defect: any exception during `ProgressPackage` initialization is silently swallowed, and the constructed package is never wired to anything. This matches the epic's "Latent Defect Promotion" section — it must be routed through the MCP promotion lifecycle at execution time, not fixed here (fixing it would be a behavior change forbidden by the epic's no-behavior-change NFR and by AC-7-equivalent constraints in `issue.md`). Do not write a test that "fixes" this by awaiting `ppkg`; a regression test should assert the current (silently-discarded) behavior if `InvestigatePerformance` is covered at all.

### Proposed seams (interface seam > injectable delegate > adapter)

`BayesianPerformanceController` is not sealed and has no interface of its own, and no sibling consumes it through an interface, so introducing a new interface seam here would be a "seam nobody asked for." The injectable-delegate tier is the right level, following the **existing in-repo precedent** at `QuickFiler/Controllers/EfcHomeController.cs:294`: `internal Action<EfcViewer> ViewerShowAction { get; set; } = viewer => viewer.Show();`. Recommended, in order of what each unblocks:

| Seam | Shape | Unblocks |
| --- | --- | --- |
| **Viewer-show delegate** | `internal Action<BayesianPerformanceViewer> ViewerShowAction { get; set; } = viewer => viewer.Show();`, called at line 69 as `ViewerShowAction(Viewer);` instead of `Viewer.Show();` | Lets a test substitute a no-op recorder so `InvestigatePerformance()` can be driven to completion without a live/visible Form. Direct precedent already merged in this assembly (`EfcHomeController`). |
| **Viewer factory delegate** | `internal Func<BayesianPerformanceController, BayesianPerformanceViewer> ViewerFactory { get; set; } = c => new BayesianPerformanceViewer(c).Init();`, called at line 63 as `Viewer = ViewerFactory(this);` | Lets a test substitute a viewer built via the existing `RunWithViewer`-style STA construction (or a lighter stand-in) without editing the `Viewer` property's `private set`, which is otherwise only reachable by reflection (as the current harness already does). |
| **Serialization + package factories (lower priority)** | `internal Func<IApplicationGlobals, BayesianSerializationHelper> SerializationFactory` and an equivalent for the `ProgressPackage` construction | Only needed if the plan chooses to drive `InvestigatePerformance()` end-to-end rather than stopping short of the discarded `ppkg` line; given the latent-defect finding above, the simplest compliant test skips asserting anything about `ppkg` and only needs the two seams above plus a pre-populated `Errors`/`Serialization` to skip the `??=` branches' true side. |
| **No new seam for `ReSortItem`** | Route through the existing `IApplicationGlobals` mock already used everywhere else in this file's tests (`Globals.Ol.NamespaceMAPI.GetItemFromID` is already mockable via the interop mock precedent this repo uses elsewhere) plus one of the two viewer/factory seams above if `EfcHomeController` construction needs to be avoided — but `EfcHomeController`'s constructor here is F8-owned and out of F15's file assignment; **do not edit it**. If `EfcHomeController(_globals, () => {}, item)` proves unmockable/uninjectable from F15's side, the fallback is testing only the `item is null` (false) branch of `ReSortItem`, which needs no interop mock beyond returning `null` from `GetItemFromID`, and documenting the `item is not null` branch as a residual gap routed to F1's ledger rather than reaching into F8's file. |

No STA / DEC-1 last-resort clause is needed for this file's **own** tests beyond what the existing `RunWithViewer` harness already provides (it already runs on an STA thread because it must construct `BayesianPerformanceViewer`, a `Form`). `BayesianPerformanceController` itself is not Form-derived.

### Zero-branch caveat

Not applicable — this file has real branch points (at least 7 identified above: 2 `??=`, 2 in `OlvVerboseDetails_SelectionChanged`, 1 in `OlvDrivers_SelectionChanged`, 1 in `ClassSelector_SelectedIndexChanged`, 1 in `ReSortItem`), so it reports a real percentage, not N/A.
