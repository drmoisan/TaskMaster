---
name: parallel-surface-partial-port
description: The parallel surface is now structurally present in TaskMaster (PR #544) but config/blast-radius.json was pushed down verbatim and is unfit for the C#/VSTO layout, so parallel-plan yields zero parallelism
metadata:
  type: project
---

Status as of 2026-08-11. Supersedes the 2026-08-10 assessment, which was wrong on one hard blocker.
Verify against the repo before relying on this.

**Structurally present now.** PR #544 ("(chore): push down claude parallel orchestrator", merged to
`main` as `2073f717`) landed the governance payload from [[drm-copilot-is-claude-governance-upstream]]:
`.claude/rules/parallel-orchestration.md`, `config/blast-radius.json`, `route_id: parallel` in
`config/orchestration-routing.json`, and `.claude/lib/bash/compute-cohorts.sh` +
`parallel-cohorts.sh`. Already present before that: `.claude/lib/blast-radius/*.psm1`,
`.claude/hooks/enforce-parallel-*.ps1`, the six `parallel-*` skills, both `parallel-*` agents. MCP
`parallel-planner-state` and `parallel-kickoff` dispatch correctly.

**Cohort computation is NOT missing — the earlier verdict was a false negative.** `compute_cohorts`
exists upstream at `scripts/dev_tools/parallel_cohort_computation.py` (commit `663d71ee`, issue #445)
with `compute_concurrency_batches`, and TaskMaster now has the bash entry point
`.claude/lib/bash/compute-cohorts.sh` (no Python or Poetry required; emits compact JSON identical to
the Python authority). The 2026-08-10 "absent from both repos" finding came from
`git grep -in "compute_cohorts|welsh"` **without `-E`** — git grep defaults to basic regex, so the
`|` was matched literally and the search could never hit. Always pass `-E` when using alternation.

**The real remaining blocker: `config/blast-radius.json` is unfit for this repo.** It was pushed
down verbatim and describes the governance payload's own layout, not TaskMaster's. It lists modules
`.claude/**`, `config/**`, `docs/**`, `tests/**` and shared surfaces `.claude/settings.json`,
`config/orchestration-routing.json`, `config/blast-radius.json`. Verified consequences (probed
directly through `Get-BlastRadius` / `Test-BlastRadiusConflict`):

1. **Zero parallelism.** `Get-BlastRadius` always appends the feature-folder glob
   `docs/features/active/<name>/**`, and module `docs` maps to `docs/**`, so *every* item carries
   module `docs` and every pair conflicts with reason `module_overlap`. The conflict graph is a
   complete graph, Welsh-Powell yields one cohort per item, and an N-item run is fully serial.
2. **Fail-open on real collisions.** TaskMaster's actual root shared surfaces (`TaskMaster.sln`,
   `Directory.Build.targets`, `.editorconfig`, `coverage.config`, `.github/workflows/**`) are not in
   `shared_surfaces`. Per the F1a rule, a separator-free root token is admitted only as an exact
   member of that list, so a plan editing `coverage.config` or `Directory.Build.targets` produces a
   radius that does not mention them at all. Two items editing the same build config are reported
   non-conflicting once the `docs` edge above is removed.
3. **C# projects attribute to no module.** None of the 9 production or 9 test project directories
   (`QuickFiler/`, `UtilitiesCS/`, `ToDoModel/`, ...) appear in `modules`; `tests/` in TaskMaster
   holds only `scripts/` (PowerShell), not the C# test projects.

**How to apply:** treat `/parallel-plan` as unable to produce a *useful* run until
`config/blast-radius.json` is authored for TaskMaster (enumerate the real `.csproj` module set and
the real root shared surfaces, and keep the feature-folder glob from collapsing the graph). Fixing it
is a design decision about which surfaces are shared in a C#/VSTO repo, so promote it as an issue
rather than editing the just-merged config inside a planner run. Separately, the parallel schema
prohibits `depends_on` and `wave`, so any requirement of the form "these items must land in a given
order" belongs to `/epic-plan`, not to this surface — see [[parallel-surface-cannot-express-ordering]].
