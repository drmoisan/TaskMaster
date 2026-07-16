---
name: epic-planner-state-required-fields
description: epic-planner-state checkpoint validator requires top-level max_parallel_features and per-feature research_path, beyond the fields listed in the agent doc
metadata:
  type: feedback
---

The `epic-planner-state` schema enforced by `mcp__drm-copilot__validate_orchestration_artifacts`
(artifact_type `epic-planner-state`) requires fields not called out in
`.claude/agents/epic-planner.md` "Checkpoint Persistence":

- top-level `max_parallel_features` — integer 1 through 8 (set it to the number of concurrent
  preparation delegations launched).
- per-feature `research_path` — every entry in `features[]` must carry it (the research artifact
  path under `<feature-folder>/research/`).

**Why:** A resumed run inherited a prior checkpoint that lacked both fields; the validator failed
until they were added. The agent doc's field list is necessary but not sufficient for the schema.

**How to apply:** Include `max_parallel_features` and per-feature `research_path` in the first
checkpoint write, then validate with the MCP tool before treating the checkpoint as durable.
Read each child's `research_artifact` from its own `orchestrator-state.json`; a child still in
S4 planning may not have recorded it yet, so read the file present under its `research/` dir.
Related: [[epic-plan-tooling-not-vendored]].
