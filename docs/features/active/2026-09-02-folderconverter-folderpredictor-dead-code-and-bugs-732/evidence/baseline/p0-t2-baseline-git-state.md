# P0-T2: Baseline Git State

Timestamp: 2026-09-03T11-22

Command: git rev-parse HEAD
Command: git status --porcelain
EXIT_CODE: 0

Output Summary:
BASELINE_SHA = b24b62fd15b4956ca8ffa9358f57c90ea3e35413

Verbatim `git status --porcelain` output at capture time (captured after P0-T1's
check-off edit to this plan file and after this feature folder's evidence/ directory
was created, both of which are in this plan's own Write Set / evidence-producing scope):

```
 M .claude/agent-memory/orchestrator/MEMORY.md
 M docs/features/active/2026-09-02-folderconverter-folderpredictor-dead-code-and-bugs-732/plan.2026-09-02T12-01.md
?? .claude/agent-memory/orchestrator/dead-code-resurrection-check-type-name-collision.md
?? .claude/agent-memory/orchestrator/pwsh-plain-command-entirely-refused-in-worktree.md
?? docs/features/active/2026-09-02-folderconverter-folderpredictor-dead-code-and-bugs-732/evidence/
```

The `.claude/agent-memory/orchestrator/*` entries are pre-existing, out-of-scope orchestrator
memory changes not owned by this plan's Write Set; this plan will not commit them (per the
delegation prompt's instruction not to commit anything under `.claude/agent-memory/`). The
plan.md and evidence/ entries are this plan's own in-progress Phase 0 work.
