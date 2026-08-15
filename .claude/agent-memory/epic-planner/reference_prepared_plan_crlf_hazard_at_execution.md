---
name: prepared-plan-crlf-hazard-at-execution
description: Prepared atomic plans commit as LF but core.autocrlf=true materializes them CRLF in a fresh worktree, and the MCP plan validator has rejected CRLF plans
metadata:
  type: reference
---

An epic-planner run commits each child's approved atomic plan to the integration branch as an LF
blob. This repository sets `core.autocrlf=true`, so when `epic-orchestrator` later checks the
integration branch out into a fresh child worktree, the plan file materializes as CRLF. The MCP
`validate_orchestration_artifacts` `plan` validator has previously rejected CRLF plans.

The plan passes the validator during preparation (in the child's own worktree, where the file was
just written as LF) and can fail it at execution time in a new worktree, with no change to the
committed blob.

**How to apply:** Carry this as an explicit note in the epic kickoff artifact and in the planning
checkpoint's `execution_notes`, addressed to `epic-orchestrator`: re-normalize each prepared plan
to LF before re-running the plan validator gate in a newly created worktree. Do not treat the
resulting validator failure as a defect in the prepared plan.

Related: [[epic-plan-tooling-not-vendored]].
