---
name: project-epic-295-winforms-testability
description: Epic #295 (winforms-testability-refactor) is design-phase only until maintainer signal; children #293/#296/#297/#298; #298 depends on #297
metadata:
  type: project
---

Epic #295 `winforms-testability-refactor` (created 2026-07-09) covers testability
refactors of Tags (#293), TaskTree (#296), TaskVisualization core (#297), and
TaskVisualization secondary (#298, depends_on #297 — same csproj/test project).
Manifest: `docs/features/epics/winforms-testability-refactor/epic.md`. Design
branch: `epic/winforms-testability-refactor-295-design`.

**Why:** The user pivoted #293 (originally a standalone TagController test task)
into this epic mid-orchestration and gave an explicit design-only mandate: create
and promote all potentials via MCP, research each child, author specs/user-stories
(user-story N/A for refactors)/atomic plans, obtain `PREFLIGHT: ALL CLEAR` per
child, then STOP and wait for the maintainer's signal. Execution (worktrees,
integration branch, PRs via epic-orchestrator) is explicitly deferred.

**How to apply:** If resuming this epic, check
`artifacts/orchestration/orchestrator-state.json` (gitignored) `children[]`
design_status. Do not begin execution without the maintainer's go signal. Shared
pattern for every child: viewer interface derived from
`UtilitiesCS.Interfaces.IWinForm.IForm`, <=500-line files, COM/logic separation,
seams (interface > delegate > adapter) instead of live forms; popups in tests are
a policy violation; COM-on-UI-thread is production-only last resort. TaskTree
needs a brand-new TaskTree.Test project wired into TaskMaster.sln.
