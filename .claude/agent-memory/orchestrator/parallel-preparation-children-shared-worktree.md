---
name: parallel-preparation-children-shared-worktree
description: Preparation-mode epic children can run concurrently in ONE shared working directory + git index + checkpoint; isolate to survive collisions
metadata:
  type: project
---

Under epic-planner-driven preparation, multiple preparation-mode `Agent(orchestrator)` children can run **concurrently inside the same session working directory**, not in separate per-child worktrees. They therefore collide on three shared resources.

**Observed (epic utilitiescs-nullable-remediation, 2026-07-18):** while preparing child `utilitiescs-nullable-extensions` (#363), the canonical `artifacts/orchestration/orchestrator-state.json` was overwritten mid-run by sibling `utilitiescs-nullable-helperclasses` (#364), and the sibling had already `git add`-staged its feature folder into the shared index.

**Why:** these children share (1) the canonical gitignored checkpoint path, (2) the git index/HEAD, (3) `docs/features/potential/`. A naive `git commit` (no pathspec) would sweep in a sibling's staged files; a naive checkpoint write races the sibling.

**How to apply (safe protocol for a preparation child in a shared worktree):**
- Keep the authoritative checkpoint at a **child-scoped path** `artifacts/orchestration/orchestrator-state.<feature-folder>.json` (still gitignored) and run the MCP validator against that explicit path. The canonical `orchestrator-state.json` is unreliable — do NOT revert a sibling's write to it (the system-reminder marks that write "intentional"). The checkpoint is gitignored/local, so this never affects committed deliverables.
- Commit deliverables with an **explicit pathspec**: `git add docs/features/active/<feature>/` then `git commit -o docs/features/active/<feature>/ -F -`. The `-o`/pathspec form commits only your paths and leaves the sibling's staged files in the index. Verified: 5-file commit excluded the sibling's staged files cleanly.
- Committed deliverables are the feature folder only (uniquely pathed, no collision). Promotion potential/promoted `.md` may not persist on disk (see [[promotion-potential-md-may-not-persist]]); the GitHub issue + committed feature folder are the durable audit trail — recreating potential `.md` in shared `docs/features/potential/` is not required and risks sibling collision.
- Distinct from [[unplanned-epic-child-worktree-mechanics]] and [[parallel-epic-children-name-collisions]], which assume a separate worktree per child. This is the shared-worktree preparation case.
