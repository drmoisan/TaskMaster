---
name: project-136-wave1-nonhalting-f1-dependency
description: epic #136 wave-1 children depend on F1's ledger+harness which is absent on disk during planning — write it as an execution-time read, never as a preflight-evaluable gate
metadata:
  type: project
---

Every wave-1 child of epic `quickfiler-per-file-coverage` (#136) depends on child **F1** (`quickfiler-coverage-ledger`, wave 0), which delivers `docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.md` and the per-file coverage harness. Neither exists on disk while the wave-1 children are being planned; F1 is prepared concurrently and merges to the epic integration branch before any wave-1 child executes.

**Why:** a plan whose Phase 0 gate reads "F1's ledger exists" is evaluated by preflight *today*, against a worktree where it does not exist, and returns REVISIONS REQUIRED for a condition that is correct-by-design. Issue #136's caller brief for F7 (#433) called this out explicitly and required a preflight run today to return `PREFLIGHT: ALL CLEAR`.

**How to apply:** word the ledger/harness tasks as normal execution-time dependency consumption — "Read F1's ledger and record the classification row verbatim" with acceptance on the *artifact produced*, not on the upstream file's existence. Add one sentence noting that genuine absence at execution time is an epic-orchestrator sequencing failure raised then. Put a short "Upstream Dependency Handling — Non-Blocking" section above the phases so a reviewer sees the intent without reading every task. Same treatment applies to the per-file coverage verification tasks, which cite the harness command captured in Phase 0.

Related: [[project-433-f7-qfchomecontroller-plan-seams]], [[project-planner-mcp-validator-not-in-tool-surface]].
