---
name: parallel-run-execution-playbook
description: TaskMaster-specific mechanics for running /parallel-run — kickoff artifact lives on the plan-home branch, no poetry, no status template, plan-home worktree separate from the session worktree
metadata:
  type: project
---

Mechanics that the `parallel-orchestrate` skill assumes but that TaskMaster does not supply.

**Why:** TaskMaster is a C# repository whose `.claude` tree is push-down-owned from
drm-copilot with zero templating, so parts of the parallel surface that the skill treats as
present were never ported. Discovering each gap mid-run costs a stall.

**How to apply:**

- **The kickoff artifact is not in the session worktree.** `/parallel-plan` commits
  `docs/features/parallel/<slug>/parallel-kickoff.md` and `parallel.md` to the plan-home
  branch `parallel/<slug>-plan`, which is NOT the branch the session worktree is on. Read
  them with `git show parallel/<slug>-plan:<path>`. Their absence from the working tree is
  not the STOP condition the `parallel-run` skill describes — check the branch before
  concluding the run was never planned.
- **Use a dedicated plan-home worktree.** Check `parallel/<slug>-plan` out at
  `TaskMaster-wt/parallel-<slug>-plan` and write `parallel-status.md` there. Never check the
  plan-home branch out in the session worktree. Keep the checkpoint
  (`artifacts/orchestration/parallel-orchestrator-state.json`, gitignored) in the session cwd,
  because the hooks read it relative to the hook process cwd.
- **The cohort table is NOT in the manifest.** `parallel.md` frontmatter carries only
  `parallel`, `mode`, `max_concurrency`, `created_at`, and `items[]` — no `cohorts` key and no
  `conflict_edges` key. Both live solely in `artifacts/orchestration/parallel-planner-state.json`
  (which is in the session worktree, not on the plan-home branch). The manifest body's cohort
  column is prose. Read the planner state for `cohorts[]`, `conflict_edges[]`, and
  `recolor_generation`, and copy `blast_radius` verbatim from its `items[]` rather than
  re-transcribing from the manifest.
- **The checkpoint is a single file, so a new run OVERWRITES the previous run's state.** Before
  seeding, read `parallel_slug` and `next_step` from the existing
  `parallel-orchestrator-state.json` and confirm the prior run reads `COMPLETE` with every item
  terminal. Seed the new run only after that check.
- **`git show <ref>:<path>` needs `MSYS_NO_PATHCONV=1`.** With it the operand survives intact and
  the plan-home artifacts read correctly; without it the Bash tool mangles it (see
  [[issue-merge-and-removal-commands-bare]]). The same variable makes
  `git rev-parse <branch>:<plan-path>` usable for verifying the kickoff Integrity table's
  plan-hash column, which is a plain git blob SHA.
- **There is no poetry and no pyproject.toml.** Every `poetry run python -m ...` fallback the
  skill names is unavailable. Validate exclusively through
  `mcp__drm-copilot__validate_orchestration_artifacts`. The bash entry points
  (`validate-parallel-manifest.sh`, `compute-cohorts.sh`,
  `compute-concurrency-batches.sh`) DO work and need no interpreter.
- **`docs/features/templates/parallel/parallel-status.md` does not exist.** Generate the
  status doc from the documented section list instead: a `## Run` header block, `## Items`,
  item lifecycle timestamps, `## Cohorts`, and the three read-only projections
  `## Conflict Edges`, `## Mutations`, `## Drift Events` (empty renders as an empty section,
  never an omitted one).
- **Committing the status doc trips the pre-implementation gate** unless the pathspec form is
  right; see [[preimplementation-gate-scope]].
- **Free the item branches before launching**; see [[free-item-branches-by-detaching]].
- **`main` is unprotected**, so same-cohort merges need no `gh pr update-branch` re-green
  cycle and may merge in any order.
