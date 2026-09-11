---
name: task-researcher-filename-regex-is-strict
description: The task-researcher SubagentStop hook enforces an exact research filename regex and a repo-relative research-path token, so a filename suggested in the delegation prompt can trap the agent.
metadata:
  type: project
---

`.claude/hooks/validate-task-researcher-output.ps1` blocks task-researcher termination unless its final
output carries a `research-path` token whose value is REPO-RELATIVE (the root check is
`StartsWith('docs/features/')` or `StartsWith('docs/research/')`, so an absolute Windows path fails) and
whose filename matches exactly:

`^\d{4}-\d{2}-\d{2}T\d{2}-\d{2}-[A-Za-z0-9][A-Za-z0-9-]*-research\.md$`

That is `<timestamp>-<slug>-research.md`, e.g. `2026-09-08T23-55-etl-deadline-mechanics-research.md`.
The trailing-timestamp form `etl-deadline-mechanics-research.2026-09-08T23-55.md` does NOT match.

**Why:** the orchestrator naturally writes the timestamp last, matching the `plan.<ts>.md` and
`code-review.<ts>.md` conventions used everywhere else in this repo. Research is the one artifact class
that puts the timestamp FIRST, and naming it in the delegation prompt overrides the agent definition's
own correct convention.

**How to apply:** do not name the research file in the delegation prompt at all — `.claude/agents/task-researcher.md`
already states the convention. Ask only for the directory, and ask the agent to report
`research-path: docs/features/.../research/<file>` repo-relative. If a non-conforming file lands anyway,
rename it with `git add <old>` then `git mv <old> <new>` (an untracked file cannot be `git mv`d, and only
`git *` is allowlisted for Bash). Related: [[prd-feature-stop-hooks-are-workmode-blind]], which needs a
`## Numeric Derivation Evidence` section appended to this same research file afterwards.
