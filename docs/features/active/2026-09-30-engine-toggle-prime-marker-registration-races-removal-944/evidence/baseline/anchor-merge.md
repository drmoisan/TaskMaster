# Anchor Merge (P0-T5)

Timestamp: 2026-09-30T13-19
Command: git rev-parse origin/main; git merge-base origin/main HEAD; git diff --cached --name-only; git diff --name-only HEAD origin/main; git status --porcelain --untracked-files=all; (merge not run: first two outputs equal); git rev-parse origin/main; git merge-base origin/main HEAD; git merge-base --is-ancestor origin/main HEAD; git rev-parse HEAD; git diff --name-status ANCHOR-SHA HEAD; git status --porcelain --untracked-files=all
EXIT_CODE: 0
Output Summary: merge-base equals origin/main (b305903e275b8abf58e8e65831c189f517568fe4), so MERGE: NOT NEEDED. Index empty before the merge decision. UPSTREAM-OVERLAP: NONE. Ancestor check exited 0. INHERITED-COMMITTED lists only feature-folder paths and the promotion record. No porcelain line names TaskMaster/ or TaskMaster.Test/.

## Pre-merge probes

- git rev-parse origin/main: b305903e275b8abf58e8e65831c189f517568fe4
- git merge-base origin/main HEAD: b305903e275b8abf58e8e65831c189f517568fe4
- git diff --cached --name-only: (no output; index clean)
- git diff --name-only HEAD origin/main: seven feature-folder paths plus docs/features/potential/promoted/2026-09-30-engine-toggle-prime-marker-registration-races-removal.md
- Porcelain (before the merge decision): three .claude/agent-memory modified paths, three .claude/agent-memory untracked paths, the plan file (modified, feature folder) and evidence/baseline/pre-merge-docs-commit.md (untracked, feature folder)

UPSTREAM-OVERLAP: NONE (after excluding feature-folder paths, the upstream diff lists only the promotion record, which has no porcelain line; no .claude/agent-memory path is in the upstream diff)

MERGE: NOT NEEDED (merge-base equals origin/main; git merge was not run)

## Post-merge probes

- git rev-parse origin/main: b305903e275b8abf58e8e65831c189f517568fe4
- git merge-base origin/main HEAD: b305903e275b8abf58e8e65831c189f517568fe4 (equal)
- git merge-base --is-ancestor origin/main HEAD: exit 0

ANCHOR-SHA: b305903e275b8abf58e8e65831c189f517568fe4

HEAD-SHA: 9c6290d8b13a4503555c58debad248a2d9730180

INHERITED-COMMITTED:

```
A	docs/features/active/2026-09-30-engine-toggle-prime-marker-registration-races-removal-944/evidence/baseline/phase0-instructions-read.md
A	docs/features/active/2026-09-30-engine-toggle-prime-marker-registration-races-removal-944/evidence/baseline/scope-and-anchor.md
A	docs/features/active/2026-09-30-engine-toggle-prime-marker-registration-races-removal-944/evidence/baseline/upstream-942-check.md
A	docs/features/active/2026-09-30-engine-toggle-prime-marker-registration-races-removal-944/issue.md
A	docs/features/active/2026-09-30-engine-toggle-prime-marker-registration-races-removal-944/plan.2026-09-30T07-20.md
A	docs/features/active/2026-09-30-engine-toggle-prime-marker-registration-races-removal-944/research/2026-09-30T08-00-engine-toggle-prime-marker-registration-research.md
A	docs/features/active/2026-09-30-engine-toggle-prime-marker-registration-races-removal-944/spec.md
A	docs/features/potential/promoted/2026-09-30-engine-toggle-prime-marker-registration-races-removal.md
```

Every listed path is under the feature folder or is exactly the promotion record.

PRE-EXISTING-WORKTREE-PATHS:

```
 M .claude/agent-memory/atomic-planner/MEMORY.md
 M .claude/agent-memory/orchestrator/MEMORY.md
 M .claude/agent-memory/task-researcher/MEMORY.md
 M docs/features/active/2026-09-30-engine-toggle-prime-marker-registration-races-removal-944/plan.2026-09-30T07-20.md
?? .claude/agent-memory/atomic-planner/project_944_prime_marker_registration_plan_seams.md
?? .claude/agent-memory/orchestrator/isolated-child-liveness-wait-and-delegation-target-lines.md
?? .claude/agent-memory/task-researcher/project_prime_marker_register_before_start_944.md
?? docs/features/active/2026-09-30-engine-toggle-prime-marker-registration-races-removal-944/evidence/baseline/pre-merge-docs-commit.md
```

No porcelain line names a path under TaskMaster/ or TaskMaster.Test/. The .claude/agent-memory paths are left by earlier subagents and are never staged by this plan.

The re-anchoring AC1 requires is satisfied by the branch already sitting on origin/main b305903e2 (no merge commit); it precedes every code change of this plan.
