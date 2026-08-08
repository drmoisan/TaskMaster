---
name: qfc-breadcrumb-bridge-router-495
description: "#495/epic #136 F12 BreadcrumbBridgeRouter.cs: open #440 will rewrite its arrow-key semantics (don't pin them); two same-named router types make the wrong Cobertura class emit the right-looking branch-rate"
metadata:
  type: project
---

`QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` (F12, issue #495) research, 2026-08-08.

**Open issue #440 (`breadcrumb-left-right-arrow-parent-child-navigation`) names this file by name**
and will change `HandleArrowKeyAsync` Left/Right semantics from collapse/expand to parent/child tree
navigation.

**Why:** any new coverage test that asserts the *current* Left/Right behaviour will have to be
rewritten when #440 lands. Six existing tests in `BreadcrumbBridgeRouterTests.cs` already pin it.
#440 is NOT in the epic's "Known Conflict Risks" list (which names only #400, #424, #426).

**How to apply:** when planning coverage tests for breadcrumb arrow handling, target guard behaviour
that survives the re-interpretation (non-suggestion no-op, stale-row focus hand-back), not the
happy-path arrow semantics. Re-check whether #440 is still open before relying on this.

---

**Measurement trap specific to this file pair.** `QuickFiler.Controllers.BreadcrumbBridgeRouter` and
`UtilitiesCS.OutlookObjects.Folder.FolderBreadcrumbBridgeRouter` are different types in different
assemblies. A plain grep for the shorter name substring-matches the longer one (71 hits / 14 files
vs the true 3 test files). Worse: the **UtilitiesCS** class's emitted `branch-rate="0.922222"` equals
the **QuickFiler** class's correctly-recomputed 83/90 = 92.22% to six digits, while the QuickFiler
class's own emitted `branch-rate="0.926471"` (63/68) is wrong. Reading the wrong element's attribute
produces the right answer by coincidence — a reviewer cannot tell the two apart by inspection.

**How to apply:** select Cobertura `<class>` by the `filename` attribute, recompute from the
class-level `<lines>` block, and state the disambiguation explicitly. See
[[committed-cobertura-baselines]] and [[cobertura-perfile-attribution-contract]].

Also verified: an out-of-range `segmentIndex` is accepted by `BreadcrumbMessageCodec` (it validates
type and presence only), reaches `BreadcrumbRow.CollapseAfter`, throws `ArgumentOutOfRangeException`,
and escapes `async void OnHostMessageReceived` (whose catch is `BreadcrumbMessageException`-only) —
a host-process crash path. Recommended for promotion; not yet an issue as of 2026-08-08.
