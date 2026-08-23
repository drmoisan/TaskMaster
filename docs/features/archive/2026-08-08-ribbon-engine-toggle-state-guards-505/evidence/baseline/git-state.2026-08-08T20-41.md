# P0-T4 — Git State (audit record)

Timestamp: 2026-08-08T20-41

Command:

```
pwsh -NoProfile -Command "Set-Location '<REPO>'; git rev-parse --abbrev-ref HEAD; git rev-parse HEAD; git rev-parse f910ff2f; git merge-base HEAD origin/main; git status --porcelain"
```

EXIT_CODE: 0

Output Summary:

- Branch: `bug/ribbon-engine-toggle-state-guards-505`
- HEAD SHA (audit record only; never used as a later equality gate):
  `f910ff2f21c67a03cf8eebcb340727d5415d8e08`
- `<MERGE_BASE>` — the full 40-character expansion of `f910ff2f`:
  **`f910ff2f21c67a03cf8eebcb340727d5415d8e08`**. This exact value is used verbatim in every
  later `<MERGE_BASE>..HEAD` diff command.
- `git merge-base HEAD origin/main` returns the same SHA, confirming the branch is currently at
  the merge-base. P0-T12 advances HEAD so the Phase 3/4/5 diff gates are non-vacuous.

Verbatim porcelain output:

```
 M .claude/agent-memory/atomic-executor/MEMORY.md
 M .claude/agent-memory/atomic-executor/project_incremental_build_vacuous_baseline.md
 M .claude/agent-memory/atomic-executor/project_missing_vsto_runtime_breaks_baseline_gates.md
 M .claude/agent-memory/atomic-executor/project_vs18_build_toolchain_paths.md
 M .claude/agent-memory/atomic-planner/MEMORY.md
 M .claude/agent-memory/prd-feature/MEMORY.md
 M .claude/agent-memory/task-researcher/MEMORY.md
?? .claude/agent-memory/atomic-executor/project_koverage_cobertura_postprocessing_shape.md
?? .claude/agent-memory/atomic-planner/project_505_toggle_state_guards_plan_seams.md
?? .claude/agent-memory/prd-feature/project_522_nullable_typecheck_deviation.md
?? .claude/agent-memory/task-researcher/project_ribbon_toggle_state_guards_505.md
?? docs/features/active/2026-08-08-ribbon-engine-toggle-state-guards-505/
```

Binary outcome: **PASS**. No `.cs`, `.csproj`, `.xml`, or `.sln` path appears in the porcelain
output. Every entry lies under `.claude/agent-memory/` (tracked agent memory, dirty at branch
head) or `docs/features/` (this feature folder), both of which the plan declares expected.
