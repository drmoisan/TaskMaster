---
name: shared-checkpoint-read-modify-write-corrupts
description: Never read-modify-write the shared session-root orchestrator-state.json; a sibling can own it between your read and your write. Guard every write with an issue-num assert, and move to a child-scoped archive the moment your PR is open.
metadata:
  type: feedback
---

The canonical path `artifacts/orchestration/orchestrator-state.json` in a shared session
directory is rotated among live siblings, sometimes minutes apart. Any "load, mutate, dump"
against it reads whichever sibling owns it at that instant and writes YOUR keys on top of
THEIR identity.

**Why:** two separate incidents.

*2026-08-27, epic child.* Between writing the checkpoint and the PR hook reading it seconds
later, sibling 489 swapped the file in; the write produced a 489/476 hybrid and PR creation
was denied with `ORCHESTRATOR_STATE_PREFLIGHT_FAILED: Checkpoint missing required key:
relativeFile`. The missing keys were mine; the file was theirs.

*2026-08-29, parallel child 638.* The damage came AFTER the work was done. 638 finished, its
PR merged, and the parent launched the 644 child, which seeded its own checkpoint at the same
path. A later bookkeeping write by 638 injected `completion_gate`, `followup_issues` and
`worktrees_for_parent_cleanup` into 644's live checkpoint. The completion gate then reported
every step as pending, which reads exactly like your own state was lost — it was not; you were
looking at somebody else's file.

**How to apply:**

- **Assert identity inside the same process as the write.** `assert s["issue-num"] == MY_ISSUE`
  between the load and the dump. This is the only check that closes the window, because a
  separate verification command re-opens it.
- **Stop writing the canonical path once your PR is open.** Nothing after that point needs the
  shared file: the model-routing hook only reads it before a delegation, and the PR hook only
  reads it before `gh`. Write your final record to a child-scoped path such as
  `artifacts/orchestration/orchestrator-state.<issue>.json`, which
  `Test-OrchestratorStateCompletionReadiness -CheckpointPath` accepts happily.
- **If you do corrupt a sibling, diff before you "clean".** On 2026-08-29 the instinct to strip
  everything that looked like 638 evidence removed nine keys the 644 child had authored itself:
  empty stubs `ci_gate {}`, `merge_outcome {}`, `pr_author_receipt {}`, empty `skill_receipts`
  and `mcp_call_receipts`, and its own `head_sha`. Snapshot what you remove, inspect it, and
  restore anything whose content is not demonstrably yours. An empty stub is a sibling's
  scaffolding, not your leftover.

**Correction to an earlier version of this memory.** It claimed `delegation_receipts` accepts
only the `promotion` key and that per-agent receipts belong under a top-level `agent_receipts`
object. That is no longer true, at least on this repo state: `delegation_receipts.agents[]` was
accepted by BOTH the MCP validator and the portable gate on 2026-08-29. Each entry needs
`agent_name`, `step`, `agent_id`, `skill_source`, `started_at`, `completed_at`, `result_signal`
and `artifact_paths`. Verify against the current validator rather than trusting either shape.

Related: [[child-orchestrator-pr-hook-reads-session-root]],
[[model-routing-hook-reads-canonical-path-only]],
[[orchestrator-state-flat-keys-and-enum]],
[[run-orchestration-hook-gates-locally]],
[[parent-session-can-commit-into-child-worktree]]
