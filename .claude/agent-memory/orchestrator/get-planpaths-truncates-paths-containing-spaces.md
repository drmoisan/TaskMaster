---
name: get-planpaths-truncates-paths-containing-spaces
description: Get-PlanPaths splits a backticked path on whitespace, so UtilitiesCS/To Depricate/FileIO2.cs enters the derived blast radius as Depricate/FileIO2.cs; plan-to-plan conflicts still fire, plan-to-DECLARED-radius does not
metadata:
  type: reference
---

`Get-PlanPaths` extracts only backtick-delimited code spans
(see [[blast-radius-extractor-mechanics]] in the parallel-planner memory), and it additionally
**tokenizes each span on whitespace and keeps only the last token**. A repository path containing a
space is therefore silently truncated.

**Measured 2026-08-29** against `.claude/lib/blast-radius/BlastRadius.psm1` in a worktree at
`origin/main` `ecdb1c84`, feeding five backticked paths through `Get-PlanPaths`:

| plan code span | extracted |
| --- | --- |
| `` `UtilitiesCS/To Depricate/FileIO2.cs` `` | **`Depricate/FileIO2.cs`** (truncated) |
| `` `QuickFiler/Controllers/QfcHomeController.Metrics.cs` `` | correct |
| `` `TaskMaster/AppGlobals/AppOlObjects.cs` `` | correct |
| `` `UtilitiesCS.Test/HelperClasses/FileIO2_Tests.cs` `` | correct |
| `` `QuickFiler.Test/Controllers/QfcHomeControllerMetricsTests.cs` `` | correct |

`Get-BlastRadius` then records the truncated token verbatim:
`RADIUS_A_PATHS=Depricate/FileIO2.cs | docs/features/active/item-a/**`.

**What this does and does not break** (all measured with `Test-BlastRadiusConflict`, which needs
`-Config` as well as `-RadiusA`/`-RadiusB`):

- Two plan-derived radii that both cite the spaced path **do** conflict (`True`). Both truncate
  identically, so plan-to-plan contention on that file is still detected. This is the common case
  and it is safe.
- The spaced path does **not** conflict with a `UtilitiesCS/**` span (`False`) — but neither does the
  space-free control `UtilitiesCS/Threading/TimeOutTask.cs` (`False`). So the truncation is **not** a
  differential risk against subtree globs; that is a separate property of glob handling.
- The genuine exposure is **plan-derived versus manifest-declared**. A declared radius written by
  hand carries the true `UtilitiesCS/To Depricate/FileIO2.cs`, which is not equal to the derived
  `Depricate/FileIO2.cs`, so the two do not match and the pair fails open.

**How to apply:** this repository has exactly one space-containing source directory of consequence,
`UtilitiesCS/To Depricate/`. When an item touches a file under it, still spell the path with its
space in the plan (it is the true path, and plan-to-plan detection works), but tell the parallel
planner or epic planner so the item's radius can be augmented by hand if a declared radius is also
in play. Do not invent a space-free substitute path in the plan to game the extractor — that
declares a file the diff does not touch.

Fixing the tokenizer belongs upstream in drm-copilot; `.claude/**` here is push-down-owned and
overwritten wholesale.

Related: [[parallel-epic-children-name-collisions]], [[preflight-catches-vacuous-gates]].
