---
name: sta-last-resort-control-identity-pattern
description: Epic #295 WinForms testability — plan pattern for measuring control-identity WinForms partials via STA last-resort instead of file-level ExcludeFromCodeCoverage
metadata:
  type: project
---

For epic winforms-testability-refactor (#295) children, when a controller partial's
logic keys on real WinForms control identity/parenting (dictionaries keyed by `Label`
instances, `TipsController` requiring a parented `TableLayoutPanel`/`Panel`,
`.Visible`/`.BackColor` toggles, `Button.PerformClick`), do NOT plan a blanket
file-level `[ExcludeFromCodeCoverage]`. Apply the maintainer-ratified STA last-resort
refinement (epic manifest Shared Design Pattern item 4).

**Why:** The maintainer ratified (2026-07-09) measuring such logic against real,
never-shown, in-memory controls on an STA thread rather than waiving coverage. See
[[feedback-sta-controls-last-resort-ratified]] (orchestrator memory).

**How to apply (verified while revising #297 plan/spec, 2026-07-09):**
- Expose the control-identity members the partial reads off the concrete viewer on a
  dedicated companion interface (real `Label`/`Control` types, NOT primitives) — e.g.
  `ITaskViewerControls` — keeping the primitive facade interface (`ITaskViewer`)
  clean. The controller reads control identity through a `(ITaskViewerControls)_viewer`
  accessor; only the irreducible `.Handle`/`PostMessage` residue stays on the concrete
  `(TaskViewer)` cast.
- STA tests supply REAL in-memory controls (a `Mock<ITaskViewerControls>` returning
  real controls, or a non-`Form` fake) parented in real `TableLayoutPanel`/`Panel`.
  Never construct `TaskViewer` / any `Form`-derived type, even unshown.
- All STA-bound tests go in dedicated `*.StaTests.cs` files using
  `[STATestClass]`/`[STATestMethod]`. MSTest 4.2.2 (in `TaskVisualization.Test` /
  `UtilitiesCS.Test`) HAS these attributes; already used in
  `UtilitiesCS.Test/HelperClasses/WindowsForms/WinFormsLayoutTests.cs`. Global STA is
  intentionally disabled via `UtilitiesCS.Test/test.runsettings`; opt-in per class/method.
  Fallback (not needed) is assembly-scoped `.runsettings`
  `<ExecutionThreadApartmentState>STA` at the cost of assembly-wide STA + lost parallelism.
- STA assertions must target reliable state (`.BackColor`, `.Text`, `.Checked`,
  returned tuples/dictionaries), NOT the parent-dependent `.Visible` getter; dispose
  every control per test; no `Show()`/`ShowDialog()`, no `PostMessage` round-trip
  assertions, no `DoEvents`, no timers.
- Residue that genuinely needs a live window handle or the message pump (`PostMessage`,
  `.Handle`, focus traversal, paint) stays exempt at method/branch level with a named
  dependency; extract it into a small `[ExcludeFromCodeCoverage]` helper so the rest of
  a mixed method stays measured. Each STA-covered region records why no logic-isolating
  seam is feasible (condition a).

Relevant for sibling #298 (depends on #297) and the other WinForms children #293/#296.
