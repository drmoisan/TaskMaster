# Repo Baseline (P0-T2)

Timestamp: 2026-08-28T15-41
Command: `git rev-parse --abbrev-ref HEAD && git rev-parse HEAD && git status --porcelain`
EXIT_CODE: 0

## Output Summary

- Branch: `bug/quickfiler-keyboard-hook-leaks-to-outlook-677`
- BASELINE_SHA: `361a49b884a4e3fe192bf04bae05151c598398fa`
- Working tree is NOT clean at baseline. Pre-existing dirty paths enumerated verbatim below.

### `git status --porcelain` (verbatim)

```
 M .claude/agent-memory/atomic-executor/MEMORY.md
 M .claude/agent-memory/atomic-executor/feedback_confirmatory_preflight_proportionate_bar.md
 M .claude/agent-memory/atomic-planner/MEMORY.md
 M .claude/agent-memory/task-researcher/MEMORY.md
?? .claude/agent-memory/atomic-executor/project_inline_dispatch_harness_citation_makes_execution_time_test_vacuous.md
?? .claude/agent-memory/atomic-executor/project_preflight_drain_scope_optimization_note_makes_test_vacuous.md
?? .claude/agent-memory/atomic-planner/project_677_keyboard_focus_leak_plan_seams.md
?? .claude/agent-memory/task-researcher/project_qfc677_webview2_focus_hold_outlook_keyboard.md
?? docs/features/active/2026-08-28-quickfiler-keyboard-hook-leaks-to-outlook-677/
?? docs/features/potential/promoted/2026-08-28-quickfiler-keyboard-hook-leaks-to-outlook.md
?? docs/features/potential/promoted/2026-08-28-quickfiler-search-box-loses-focus-on-dropdown-expand.md
```

### Deviation note from the task's parenthetical expectation

The task text expects "none or feature-folder/plan files only". The observed set additionally
contains agent-memory files under `.claude/agent-memory/` (written by the research and planning
agents that produced this feature's artifacts) and two `docs/features/potential/promoted/`
entries produced by the promotion lifecycle for this issue and a sibling entry. No source file
(`*.cs`, `*.csproj`, `*.props`, `*.targets`) and no policy document is dirty, so no production or
test baseline is affected. P7-T3 commits all changes, which subsumes these paths.

### Scoped source-tree cleanliness

`git status --porcelain -- '*.cs' '*.csproj'` is empty at baseline: no C# source or project file
is modified or untracked. This is the condition later gates (P1-T5, P3-T10, P4-T3, P4-T4, P5-T7)
measure against.
