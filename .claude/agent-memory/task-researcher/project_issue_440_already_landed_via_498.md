---
name: issue-440-already-landed-via-498
description: Issue #440 (breadcrumb Left/Right tree nav) was largely IMPLEMENTED as a secondary payload of feature #498 and is on main; only a one-line Qfc gate remains. Also: the two "breadcrumb coverage r2" branches are pre-#439/#498 and will silently revert it.
metadata:
  type: project
---

Research 2026-08-29 for `docs/features/active/2026-08-07-breadcrumb-left-right-arrow-parent-child-navigation-440/`,
read at `origin/main` = `b56400ab`.

**1. A feature folder's `issue.md` can be months stale even when the folder was created today.**
The #440 folder was seeded 2026-08-29 but its `issue.md`/`spec.md` carry a 2026-08-07 code-read whose
eight source citations are ALL wrong. `docs/features/active/breadcrumb-router-navigation-defects-498/spec.md:4`
says "**Also closes:** #440, #499" — #498 shipped the #440 Left/Right tree transitions on BOTH surfaces
with ACs 15-18, 23, 24, 28 checked `[x]`.
**Why:** an orchestrator promoting a queued potential re-seeds the spec from the original potential file,
not from the tree. **How to apply:** before researching any bug whose potential is more than a few weeks
old, grep the repo for the bare issue number in `*.cs` FIRST. Seven files carried `440` markers; that
single grep reframed the whole task in one call.

**2. The residual defect is a single boolean clause.** `BreadcrumbStateModel.cs` Qfc `LeftArrow()` gates
the parent-select on `activeIndex.Value == row.Chain.Count - 1` (leaf-anchored), so Left walks up exactly
ONE level and the second Left falls through to `UnhandledArrowMessage` -> `SetFolderDroppedDown(false)`,
which CLOSES the QuickFiler drop-down. The Efc twin in `BreadcrumbBridgeRouter.Arrows.cs` has no such
clause and walks to the root. The `_selectedSubfolderIndex < 0` clause in the same `if` must be RETAINED
(a different test depends on it). The one-step limit is codified in a test comment at
`FolderBreadcrumbBridgeRouterTests.cs:370-371`, which is how to prove it rather than infer it.

**3. Efc and Qfc do NOT share `BreadcrumbRow`,** contrary to what #440's own spec asserts. Efc uses
`BreadcrumbRow` (`BreadcrumbRow.cs`); Qfc uses `BreadcrumbStateRow` (`BreadcrumbStateModel.Row.cs`).
Parallel members, different guards (`BreadcrumbRow.ActivateSegment` additionally requires an attached
`_segmentKeys` entry). Only `FolderBreadcrumbSegment`, `FolderTreeNodeKey`, `IFolderHierarchyProvider`
are genuinely shared. So "share the transition logic between the two routers" is a refactor, not a fix.

**4. `feature/quickfiler-breadcrumb-bridge-coverage-r2` (#495) and
`feature/quickfiler-per-file-coverage-capstone-r2` (#497) are pre-#439/#498/#614.** Both are checked out
in live worktrees (`agent-aca320624821a4ad1`, `agent-a24c84de174a27784`), so their files can be READ
directly when no shell tool is available. Proof of staleness without `git diff`: `BreadcrumbBridgeRouter.Arrows.cs`
absent, zero `440` markers, five main-side test files missing, and `2026-07-21-...-400` still in `active/`.
#495 targets the pre-split 450-line `BreadcrumbBridgeRouter.cs` for 100% coverage; merging it as-is would
silently revert the #440 Efc work. **How to apply:** never merge; require a rebuild on main.

**5. Both #440 boundary decisions were already ratified** by #498 D2 / AC-23 / AC-24 — Efc silent no-op,
Qfc fall-through retained. The `#400 AC-9 supersession record` already exists at #498 `spec.md:304-311`.
Cite it; do not author a second supersession.

See also [[breadcrumb-navigation-defects-439-440-498-499]] (its item 2 — "#440 contradicts landed #400
AC-9" — is now RESOLVED by that record; its items 1, 3, 4 remain accurate),
[[qfc-breadcrumb-webview2-351]], [[efcviewer-breadcrumb-webview2-349]].
