---
name: orchestrator-subagent-not-registered
description: Runtime gap — the 'orchestrator' and 'pr-author' subagent types are granted to epic-orchestrator in frontmatter but are NOT in the runtime launchable Agent set (verified 2026-07-10); spawns fail with "Agent type 'orchestrator' not found"
metadata:
  type: project
---

The epic-orchestrator agent definition grants `Agent(orchestrator)` and `Agent(pr-author)` as tools, and `.claude/agents/orchestrator.md` reportedly exists. Despite this, the runtime launchable subagent set exposed to the Agent tool does NOT include `orchestrator` or `pr-author`.

Verified 2026-07-10T06:08Z on epic #295: `Agent(subagent_type="orchestrator", model="opus")` returned verbatim:
`Agent type 'orchestrator' not found. Available agents: atomic-executor, atomic-planner, commit-message, csharp-typed-engineer, epic-orchestrator, epic-review, feature-review, human-exception-runbook, powershell-typed-engineer, prd-feature, python-typed-engineer, staged-review, status-updater, task-researcher, typescript-engineer.`

`pr-author` is absent from that same list, so the final integration→main PR delegation hits the same failure.

**Why it matters:** Combined with the prohibition on inline child execution ([[inline-child-lifecycle-prohibited]]), this gap blocks both wave-N child completion and the final epic PR. The epic cannot progress until these subagent types are registered in the runtime, or the maintainer grants explicit inline-execution authorization.

**How to apply:** This is a moving target the maintainer is actively managing — VERIFY before relying on it. On each epic-orchestrator run, attempt the spawn (do not pre-emptively assume it will fail); only record a block after a real, verbatim failure. If a future session sees `orchestrator`/`pr-author` in the available-agents list, this memory is stale — update it. Note `atomic-executor`, `atomic-planner`, `commit-message`, `feature-review` ARE available, which is why prior sessions were tempted to drive lifecycles inline (now prohibited).
