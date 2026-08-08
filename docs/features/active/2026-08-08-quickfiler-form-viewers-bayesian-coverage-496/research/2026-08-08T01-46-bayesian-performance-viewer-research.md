## `QuickFiler/Viewers/BayesianPerformanceViewer.cs` (67 lines) — a `Form`

- **Epic child:** F15 (`quickfiler-form-viewers-bayesian-coverage`, issue #496), parent epic #136.
- **Measured baseline (epic manifest, indicative):** 54.3% line / 12.5% branch on the `.cs` file, 35 Cobertura-visible lines; the paired `BayesianPerformanceViewer.Designer.cs` is separately at 99.1% line / 50.0% branch, 498 lines.
- **Classification (F1 ledger rules applied directly):** `testable`. No `[ExcludeFromCodeCoverage]` attribute today (verified). It is a Form-derived class, so the CLAUDE.md WinForms exemption ground is textually available, but the epic's Shared Design §1 reads that ground as a live obligation, not a standing permission: "if a seam can be introduced, the exemption does not apply." A seam already exists and is already in production use (`BayesianPerformanceController.TestSupport.cs`), so exemption is not warranted here — this file is `testable`, not `ratified-exempt`.

### Current structure

`public partial class BayesianPerformanceViewer : Form`. Members:

- `BayesianPerformanceViewer()` — parameterless, calls `InitializeComponent()` only. No branch.
- `BayesianPerformanceViewer(BayesianPerformanceController controller)` — calls `InitializeComponent()`, sets `Controller`. No branch. **This is the constructor the existing `RunWithViewer` harness already uses.**
- `Init()` — sets `PredictedClass.GroupKeyGetter = GroupKeyGetter;` and returns `this`. No branch. **Already exercised** — every existing test in `BayesianPerformanceControllerTests.cs` goes through `RunWithViewer`, which calls `new BayesianPerformanceViewer(controller).Init()`.
- `Controller` property (`virtual`, `internal set`) — plain, no branch.
- `GroupKeyGetter(object rowObject)` — the file's **only branch point**: `try { return ((KeyValuePair<...>)rowObject).Key.Actual; } catch (Exception) { return "unknown"; }`. This is exactly the "untaken-guard / error-path" gap the epic and `issue.md` call out: nothing in the existing test suite calls `GroupKeyGetter` with an object that is *not* a `KeyValuePair<VerboseTestOutcome, string>`, so the `catch` branch (12.5% branch figure) has never executed. `GroupKeyGetter` is `internal`, directly reachable from `QuickFiler.Test` because `[assembly: InternalsVisibleTo("QuickFiler.Test")]` is compiled twice in this project (`Properties/AssemblyInfo.cs:5`, `Controllers/QfcHomeController.cs:18`).
- Four private event-forwarding handlers (`OlvVerboseDetails_SelectionChanged`, `OlvDrivers_SelectionChanged`, `ClassSelector_SelectedIndexChanged`, `button1_Click`) — each is a one-line `Controller?.Xxx();` null-conditional forward. Each has exactly one branch point (`Controller` null vs. not-null). **None of these four forwarders is exercised as a standalone unit today** — the existing tests call the *controller's* methods directly (e.g. `controller.OlvVerboseDetails_SelectionChanged()`), not the viewer's private forwarding handlers. Because they are `private`, exercising them requires either (a) invoking them as real WinForms event handlers by raising the underlying control event (`OlvVerboseDetails.SelectedIndexChanged` etc.), which is possible without showing the form since the control tree exists after construction, or (b) reflection.

### What is already tested vs. the gap

Tested indirectly (constructor + `Init()` execute as setup in every existing test via `RunWithViewer`), but the **branch-bearing logic is untested**:

1. `GroupKeyGetter`'s `catch` path (a non-`KeyValuePair` row object) — never exercised. This alone explains most of the 12.5% branch figure, since it is the file's only branch with a well-defined true/false (try-succeeds / catch-fires) split and the "false"/exceptional side has zero coverage.
2. Each of the four `Controller?.` null-conditional forwarders' **null-`Controller`** side — every existing test constructs the viewer with a controller (`new BayesianPerformanceViewer(controller)`), so `Controller` is always non-null in every existing scenario. The parameterless constructor path (`Controller == null`) that would exercise the "no-op" side of each `?.` is never used.
3. The four forwarders' **non-null** side is only exercised transitively when `RunWithViewer`'s `action` calls the controller's public method directly, which does not actually invoke the viewer's private handler at all — so, strictly, none of the four `private void Xxx_SelectionChanged/Click(object, EventArgs)` methods is ever invoked as a unit, whether or not `Controller` is null. This is a coverage gap independent of the branch-count discussion above: the method bodies themselves (not just their forwarding branch) are unreached.

### Proposed seams

No new interface or delegate seam is needed — `Controller` is already a `virtual` property with an `internal set`, which is exactly the seam the existing harness already uses to substitute a controller. The gap is a **test-writing** gap, not a **testability** gap:

1. **`GroupKeyGetter` catch path** — call `viewer.GroupKeyGetter(new object())` (or any non-`KeyValuePair` value) directly; `internal` visibility plus the `InternalsVisibleTo` grant make this a zero-seam unit test.
2. **Forwarder methods, `Controller != null` side** — the four private handlers can be invoked without reflection by raising the underlying WinForms control event directly (e.g. `viewer.OlvVerboseDetails.SelectedIndexChanged += ...` is already wired by the Designer; a test can call the public event-raising path on the control, such as setting `SelectedObject` and letting the real event fire, or — if the control does not raise deterministically off-screen — falling back to a single `MethodInfo.Invoke` per handler as a last resort, consistent with the existing `SetField` reflection helper already used in `BayesianPerformanceControllerTestSupport`). Prefer the event-raising path first; it needs no new production code.
3. **Forwarder methods, `Controller == null` side** — construct via the parameterless `BayesianPerformanceViewer()` constructor (still inside `RunWithViewer`'s STA/unshown-dispose shape) and invoke each handler the same way; assert no exception (the null-conditional short-circuits).

No production seam addition is required for this file; the existing `virtual`/`internal set` `Controller` property and the existing `internal` visibility of `GroupKeyGetter` are already sufficient. This is a case where "no seam is needed" is the correct disposition: the private handlers are pure one-line forwards with no COM, no I/O, and no un-mockable dependency — the only obstacle is that no test has yet targeted them directly.

### STA / DEC-1 last-resort clause

**Required, and already available.** `BayesianPerformanceViewer` is a `Form`. Its constructor and `InitializeComponent()` (in the Designer partial) can only be exercised via the epic's ratified DEC-1 Approach A: unshown construction on an STA thread, disposed in `finally`. This is **already implemented and merged** as `QuickFiler.Test/Controllers/BayesianPerformanceController.TestSupport.cs`'s `RunWithViewer` — no new STA harness is needed for this file; every new test proposed above should be written as an additional `action` lambda passed into the existing `RunWithViewer`, or, if a parameterless-constructor variant is needed for the `Controller == null` scenario, a second overload of `RunWithViewer` (or a sibling method in the same `TestSupport.cs` file) following the identical STA/dispose/`SynchronizationContext`/`ExceptionDispatchInfo` shape. New tests do not need a dedicated `*.StaTests.cs` file because they reuse the existing harness rather than hand-rolling a new STA construction; the epic's dedicated-file rule targets *new* STA-bound test infrastructure, and none is being newly authored here.

### Disposition (this file is not a `[ExcludeFromCodeCoverage]` candidate)

Not applicable — this file carries no attribute today. No removal action is needed on `BayesianPerformanceViewer.cs` itself; only new test coverage against the existing seams.

### Zero-branch caveat

Not applicable to `BayesianPerformanceViewer.cs` — it has 5 real branch points (`GroupKeyGetter`'s try/catch plus the four `?.` null-conditional forwarders), so a genuine, non-N/A branch percentage applies. (The paired `BayesianPerformanceViewer.Designer.cs` **is** a near-zero-branch generated file — see the separate generated-files research artifact.)
