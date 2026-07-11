---
name: mcp-plan-validator-defective-em-dash
description: The MCP plan validator's em-dash rejection is version-dependent — it PASSED em-dash+LF plans on 2026-07-10; verify behavior in the current bundle before trusting or distrusting it
metadata:
  type: reference
---

UPDATE 2026-07-10 (worktree agent-a0bb15bdb226acc2c, swordfish epic F1 prep): `mcp__drm-copilot__validate_orchestration_artifacts` with `artifact_type: "plan"` ACCEPTED the canonical `### Phase N — <Title>` em-dash headings (U+2014) with LF line endings — returned `ok:true` on all four preflight-revision passes of `plan.2026-07-10T20-14.md`. This directly contradicts the earlier "rejects em-dash" finding below. The bundle appears to have been fixed/updated between sessions. Treat the em-dash-rejection claim as version-dependent: run the validator and observe the actual result rather than assuming a defect. Also note both `atomic-planner` and `atomic-executor` subagents reported the MCP validator tool as "not available" — that is expected (it is exposed to the orchestrator, not to those subagents); run it yourself from the main thread. See [[mcp-tools-available-to-orchestrator]].

PRIOR FINDING (earlier session, may be a superseded bundle): it rejected the canonical `### Phase N — <Title>` heading (em-dash U+2014) with "phase heading must match `### Phase N — <Title>`" / "Plan does not contain any canonical phase headings", and also rejected a plain-hyphen `-` heading. Calibrated against F1's already-MERGED plan (`.../store-disable-service-261/plan.2026-07-07T18-00.md`, PR #275): it failed identically.

The local `.claude/hooks/validate-planner-output.ps1` uses a hyphen-only regex `^### Phase (?<Phase>\d+)\s+-\s+...` that ALSO fails em-dash, yet did not fatally block the atomic-planner SubagentStop (planner returned normally with em-dash). Em-dash is canonical per the `atomic-plan-contract` skill and every committed plan in the repo.

**Why:** the `.claude` bundle was pushed down from a reference repo; multiple validators diverge from the repo's actual artifacts. See [[orchestrator-state-validator-divergence]] and [[remediation-plan-em-dash-required]].

**How to apply:** KEEP em-dash `### Phase N — <Title>` (repo convention + skill canonical). Treat a MCP plan-validator rejection of em-dash headings as a documented false-negative; do NOT contort the plan to it. The operative plan gate is the atomic-planner structural self-check + an atomic-executor `DIRECTIVE: PREFLIGHT VALIDATION ONLY` pass. Record the determination in the checkpoint. (The MCP validator's orchestrator-state mode with `require_model_routing` DID work correctly this session — the defect is specific to the plan artifact_type's phase-heading regex.)
