# [P0-T3] Git Baseline

- **Issue:** #438
- **Task:** [P0-T3]
- **Timestamp:** 2026-08-08T11-41

## Command

`pwsh -NoProfile -Command "git rev-parse HEAD; git status --porcelain ; exit $LASTEXITCODE"`

- **EXIT_CODE:** 0

## Baseline HEAD

```
904b4c38dba0f9f41707c3c0f077e123c78de59c
```

This sha is `<P0-T3-sha>` and is consumed by the P6-T2 post-format file-size audit.

Branch (`git rev-parse --abbrev-ref HEAD`): `bug/quickfiler-search-keystroke-focus-steal-438` (EXIT_CODE 0). No branch switch performed.

## `git status --porcelain`

```
 M .claude/agent-memory/atomic-executor/MEMORY.md
 M .claude/agent-memory/atomic-planner/MEMORY.md
 M .claude/agent-memory/prd-feature/project_promotion_scaffold_metadata_defects.md
 M .claude/agent-memory/task-researcher/MEMORY.md
 D docs/features/potential/promoted/2026-08-07-quickfiler-search-keystroke-focus-steal.md
?? .claude/agent-memory/atomic-executor/feedback_verify_line_citations_with_numbered_output.md
?? .claude/agent-memory/atomic-planner/project_438_search_focus_plan_seams.md
?? .claude/agent-memory/task-researcher/project_qfc438_search_focus_steal.md
?? docs/features/active/2026-08-07-quickfiler-search-keystroke-focus-steal-438/
```

## Allowlist reconciliation

| Entry | Allowed bucket |
|---|---|
| `.claude/agent-memory/atomic-executor/MEMORY.md` | `.claude/agent-memory/` |
| `.claude/agent-memory/atomic-planner/MEMORY.md` | `.claude/agent-memory/` |
| `.claude/agent-memory/prd-feature/project_promotion_scaffold_metadata_defects.md` | `.claude/agent-memory/` |
| `.claude/agent-memory/task-researcher/MEMORY.md` | `.claude/agent-memory/` |
| `docs/features/potential/promoted/2026-08-07-quickfiler-search-keystroke-focus-steal.md` (deleted) | `docs/features/potential/promoted/` — expected and pre-existing; the promotion lifecycle consumed the file into the feature folder. Not restored, not deleted further. |
| `.claude/agent-memory/atomic-executor/feedback_verify_line_citations_with_numbered_output.md` | `.claude/agent-memory/` |
| `.claude/agent-memory/atomic-planner/project_438_search_focus_plan_seams.md` | `.claude/agent-memory/` |
| `.claude/agent-memory/task-researcher/project_qfc438_search_focus_steal.md` | `.claude/agent-memory/` |
| `docs/features/active/2026-08-07-quickfiler-search-keystroke-focus-steal-438/` | `<FEATURE>/` |

- Entries outside the allowlist: **0**
- `.cs` / `.csproj` / `packages.config` / `app.config` entries: **0**

## Result

- **Output Summary:** HEAD recorded as `904b4c38dba0f9f41707c3c0f077e123c78de59c`. Working tree contains only allowlisted paths (agent memory, the expected promotion-lifecycle deletion, and the feature folder). Zero source or project-file modifications exist prior to implementation. Accept criteria met.
