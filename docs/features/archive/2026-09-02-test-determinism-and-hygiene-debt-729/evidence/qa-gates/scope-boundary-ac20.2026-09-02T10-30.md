# Scope boundary AC20 — no push-down-owned path changed (P7-T4)

Timestamp: 2026-09-03T00-08

EXIT_CODE: 0

## Base re-derivation (D11)

```
$base = (git merge-base origin/main HEAD).Trim()
```

Observed `$base`: `8be5a6aac3b5a82c86241fbbf989fd9118602c56`, equal to the `BaseRef:` recorded by
P0-T14.

## Command 1 — anchored diff

```
git diff --name-only $base HEAD -- .claude .codex .agents config/blast-radius.json config/orchestration-routing.json
```

Output:

```
(empty)
```

This is the unconditional half of the acceptance and it passes: no task in this plan commits any
push-down-owned path, so nothing under `.claude`, `.codex`, `.agents`,
`config/blast-radius.json`, or `config/orchestration-routing.json` is reachable from `HEAD` on
this branch.

## Command 2 — working-tree status

```
git status --porcelain -- .claude .codex .agents config/blast-radius.json config/orchestration-routing.json
```

Output:

```
 M .claude/agent-memory/atomic-executor/project_doubled_backslash_dedoubles_bash_to_native_exe.md
 M .claude/agent-memory/atomic-planner/MEMORY.md
 M .claude/agent-memory/task-researcher/MEMORY.md
?? .claude/agent-memory/atomic-planner/project_729_dirty_tree_and_host_leak_plan_seams.md
?? .claude/agent-memory/task-researcher/project_test_determinism_debt_729.md
```

## Allowance covering each reported path

| Path | Listed in the P0-T15 `PreExistingPaths:` set | Under `.claude/agent-memory/` |
|---|---|---|
| `.claude/agent-memory/atomic-executor/project_doubled_backslash_dedoubles_bash_to_native_exe.md` | yes | yes |
| `.claude/agent-memory/atomic-planner/MEMORY.md` | yes | yes |
| `.claude/agent-memory/task-researcher/MEMORY.md` | yes | yes |
| `.claude/agent-memory/atomic-planner/project_729_dirty_tree_and_host_leak_plan_seams.md` | yes | yes |
| `.claude/agent-memory/task-researcher/project_test_determinism_debt_729.md` | yes | yes |

Every reported path is covered by both allowances. Each holds per-agent scratch memory written by
the persistent-memory system of a delegated agent, not repository policy or configuration content,
and none is written or staged by any task in this plan.

No path under `.claude/` outside `.claude/agent-memory/` is reported, and neither
`config/blast-radius.json` nor `config/orchestration-routing.json` is reported. `.codex` and
`.agents` produce no entries.

Output Summary: The anchored diff returns empty output, so no push-down-owned path is committed on
this branch. The porcelain status reports five paths, every one of them under
`.claude/agent-memory/` and every one of them also listed in the P0-T15 `PreExistingPaths:` set,
so both stated allowances cover each. AC20 holds.
