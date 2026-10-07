---
name: project-964-r4-post-merge-footprint-reanchor-with-negative-controls
description: #964 R4 (2026-10-02) - after a mid-plan origin/main merge, only the whole-tree footprint gate moves to MERGE-SHA; scoped `-- TaskMaster TaskMaster.Test` anchors and `merge-base BASE HEAD` stay valid; the re-anchored gate needs two negative controls; a planner without Bash reads git facts from .git/worktrees/<id>/logs/HEAD and cannot produce hash-object
metadata:
  type: project
---

Post-merge re-anchor pattern applied on #964 version 1.4 (orchestrator-supplied delta, applied verbatim):

1. Only the gate that diffs the WHOLE tree (`git diff --name-only BASE`) breaks when origin/main is merged into the branch mid-plan; every diff scoped with `-- TaskMaster TaskMaster.Test` and every `merge-base BASE HEAD` check (BASE remains an ancestor) stays valid on BASE-SHA. Re-anchor only the whole-tree gate.
2. The re-anchored gate carries two negative controls, both over committed ranges so they are deterministic: `diff --name-only BASE MERGE` must be non-empty and contain one named out-of-scope path (proves the path-set check can still fire: `FOOTPRINT CONTROL INERT`), and `diff --name-only BASE MERGE -- <item dirs>` must be empty (proves the anchor move hides no item code: `MERGE TOUCHED ITEM CODE`).
3. Residual drift the delta did not touch (reported, not edited): the task title and a design-decision sentence ("every footprint gate compares against BASE-SHA") still name BASE-SHA; a Base-bullet "Exception (version N)" sentence is the orchestrator's chosen reconciliation.
4. Tool-surface fact: this agent had no Bash tool in that session. Git facts were taken from `.git/worktrees/<id>/logs/HEAD` (reflog line `merge origin/main: Merge made by the 'ort' strategy.`), the branch ref file and `refs/remotes/origin/main`; `git hash-object` of the edited plan could not be produced and was handed back to the caller.

**Why:** a whole-tree footprint gate would have failed with FOOTPRINT EXCEEDS WRITE SET for merged files unrelated to the item.
**How to apply:** on any "re-anchor after merge" delta, first classify every ref-bearing command as whole-tree or scoped; move only the whole-tree ones; add the two controls; grep the new SHA afterwards to confirm it lands only where the delta names. Related: [[project-940-r3-per-file-coverage-rule-and-post-merge-reanchor-seams]], [[project-929-r3-post-merge-ci-sourced-gates-seams]], [[project-964-r3-glob-backslash-and-hit-attribution-seams]].
