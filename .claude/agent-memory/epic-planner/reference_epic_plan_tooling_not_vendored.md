---
name: epic-plan-tooling-not-vendored
description: The epic-plan/epic-orchestrate skills reference epic_wave_computation.py and an epic-manifest validator that are NOT present in this repo; verify DAG manually
metadata:
  type: reference
---

The `epic-plan` and `epic-orchestrate` skills cite
`scripts/dev_tools/epic_wave_computation.py` as the tested wave-assignment reference and imply an
epic-manifest schema validator. In the TaskMaster repo (worktrees under
`<repo-root>-wt/`) neither is vendored: `find scripts -name "*epic*"`
returns nothing, and the MCP validator's `artifact_type` enum has no epic-manifest type (only
`epic-planner-state`, `epic-orchestrator-state`, `epic-kickoff`, `plan`, etc.).

**How to apply:** For the epic manifest (`docs/features/epics/<slug>/epic.md`), verify the DAG
manually — every `depends_on` entry resolves to a feature in the set, and no node sits in its own
dependency chain (cycle-free) — and compute waves by longest-path layering by hand
(`wave=0` when no deps, else `1 + max(wave(dep))`). Do NOT try to run the named script or pass the
manifest to the `plan` validator (that type is for atomic plans and will misparse). Validate the
planner checkpoint and kickoff artifact with their real MCP artifact types.
Related: [[epic-planner-state-required-fields]].
