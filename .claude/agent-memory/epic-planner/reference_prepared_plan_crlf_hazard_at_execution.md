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

## CORRECTION 2026-08-22: a CRLF plan passes the validator

Measured directly, not inferred. The #445 prepared plan in the
`quickfiler-suite-determinism-foundation` epic is CRLF — `file(1)` reports "with CRLF line
terminators" — and `mcp__drm-copilot__validate_orchestration_artifacts` with
`artifact_type: "plan"` returned `ok` against it. In the same epic the #491 plan is LF (its child's
Edit operations converted it) and also passes. **Both line endings validate.**

So the rejection claim above is not reproducible against the current validator. Treat
re-normalization as PRECAUTIONARY, not required: it is harmless to do and cheap, but a CRLF plan is
not by itself a reason to expect a gate failure, and an execution-time validator failure should be
diagnosed on its actual message rather than attributed to line endings.

Keep the underlying mechanism in mind, which is still real: `core.autocrlf=true` means a plan
committed as LF materializes as CRLF in a fresh worktree, so the bytes on disk at execution time
genuinely differ from the bytes committed. That is worth knowing for any byte-exact comparison; it
is just not a validator failure.
