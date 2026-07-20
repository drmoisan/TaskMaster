---
name: epic-checkpoint-schema-gotchas
description: Non-obvious epic-orchestrator-state.json schema/validator behaviors not documented in the agent/skill §6 field list
metadata:
  type: project
---

Schema/validator behaviors for `artifacts/orchestration/epic-orchestrator-state.json`, derived from the
bundled validator (`validate_epic_orchestrator_state.py` / MCP `validate_orchestration_artifacts`,
artifact_type `epic-orchestrator-state`). Not documented in the agent/skill §6 field list.

- **`max_parallel_features` (int 1..8) is REQUIRED.** Omitting it fails validation. Set it to the
  widest wave's child count (this epic: 6).
- **`waves[]` entries key on `feature_folders`** (array of feature_folder strings), NOT
  `feature_issue_nums`. The waves-vs-wave_number consistency check reads `feature_folders`; a wrong
  key silently skips the check rather than erroring.
- **Wave-barrier check is unconditional on unmerged deps.** `_validate_wave_barrier_ordering` emits
  `EPIC_WAVE_BARRIER_VIOLATION: <f> started before dependency <d> merged` whenever any dependency's
  `merge_status` is not in {merged, worktree_removed} — regardless of whether the dependent has
  started. So a mid-epic checkpoint ALWAYS reports these until deps merge; it is only barrier-clean at
  wave-boundary / completion states. Treat these as expected mid-flight noise, not defects.
- **Python CLI validator is ABSENT in TaskMaster** (`scripts.dev_tools.validate_orchestration_artifacts`
  does not exist here). The epic-orchestrator SubagentStop hook (`validate-orchestrator-output.ps1`,
  wired with `-ArtifactType epic-orchestrator-state`) therefore falls back to the portable
  orchestrator-state completion module. The authoritative epic validator is the drm-copilot MCP tool.
- **`enforce-model-routing-receipt.ps1` excludes `orchestrator`** and reads `orchestrator-state.json`
  (not the epic checkpoint). So `Agent(orchestrator)` child launches are NOT receipt-gated; only
  `Agent(pr-author)` at the final PR is (and it reads orchestrator-state.json — see [[epic-final-gate-pr-author-mechanics]]).
- **`merge_status` VALUE enum is STRICTLY enforced** (unlike additive extra keys, which are tolerated).
  The validator emits `Epic checkpoint feature '<folder>' has invalid merge_status: "<value>"` for any
  value outside {not_started, worktree_created, pr_open, ci_green, merge_conflict,
  blocked_conflict_loop_limit, merged, worktree_removed}. There is NO enum slot for a planned
  human-gate STOP. To record a maintainer-ratification block (e.g. #366 on 2026-07-19), keep
  `merge_status` at the canonical `pr_open` (when a child PR is open) and carry the distinct semantics
  in an ADDITIVE `execution_status: "blocked_pending_maintainer_ratification"` field. Additive keys
  pass; a non-canonical merge_status value does not. On the maintainer decision, `merge_status` STAYS
  `pr_open` (the child PR is still open until the resume merges) and only `execution_status` flips
  (e.g. to `ratified_resuming`); record the decision in an additive `ratification_decision.decision`
  sub-object, then re-delegate `Agent(orchestrator)` for the child (mode
  `epic-child-resume-post-ratification`). The wave-barrier line for that dep persists until the resume
  actually merges — it does NOT clear on the decision alone.

**Why:** These cost a full discovery pass (reading the bundled TS/Py validator + hooks) on the
2026-07-19 utilitiescs-nullable-remediation epic run.
**How to apply:** When seeding or resuming an epic checkpoint, include `max_parallel_features` and use
`feature_folders` in `waves[]`; do not treat mid-flight wave-barrier notices as errors to fix.
