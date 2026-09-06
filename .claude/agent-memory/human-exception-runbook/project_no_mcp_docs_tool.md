---
name: project-no-mcp-docs-tool
description: Repo currently has no callable MCP documentation-retrieval tool; MCP-first sourcing clause is unmet and WebFetch is the sole web-second mechanism
metadata:
  type: project
---

Re-verified 2026-09-06 (previously 2026-08-28, 2026-08-08, 2026-08-04; first recorded 2026-07-06): no `mcp__*`
documentation-retrieval tool wired as a dependency in TaskMaster. The `human-exception-runbook` skill's sourcing rule is MCP-first, then
web-second (`.claude/skills/human-exception-runbook/SKILL.md`), but the "MCP-first" clause is
currently aspirational: there is no MCP tool that can be queried for third-party UI documentation
(e.g., GitHub web UI, Entra admin center). `WebFetch` is the only available sourcing mechanism for
third-party UI steps until such a tool is added.

**Why:** This limitation is explicitly documented in the two-axis-model-selection spec's Out of
Scope section and is not something any individual runbook-authoring task should try to resolve.

**How to apply:** When authoring a human-exception runbook that includes a third-party UI step,
note in the Source and Citation section that MCP-first sourcing could not be satisfied for this
reason, then cite a current `WebFetch`-retrieved vendor documentation page as the web-second source
with a dated capture. Do not treat the missing MCP tool as a defect to fix within the runbook task
itself. Re-check whether an MCP docs tool has been added before repeating this note in future
sessions — this is a snapshot of repo state as of 2026-08-28, not a permanent constraint.

Microsoft Learn pages fetched via `WebFetch` expose an `updated_at` field in their front matter, which
satisfies the skill's dated-capture requirement directly; record both `updated_at` and the retrieval date.
