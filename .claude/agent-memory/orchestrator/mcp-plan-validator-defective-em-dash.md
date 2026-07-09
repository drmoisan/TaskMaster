---
name: mcp-plan-validator-defective-em-dash
description: The MCP plan validator rejects the canonical em-dash phase headings used by every repo plan (incl. merged ones) — it is not a usable plan gate here
metadata:
  type: reference
---

`mcp__drm-copilot__validate_orchestration_artifacts` with `artifact_type: "plan"` is DEFECTIVE in the worktree bundle: it rejects the canonical `### Phase N — <Title>` heading (em-dash U+2014) with "phase heading must match `### Phase N — <Title>`" / "Plan does not contain any canonical phase headings", and also rejects a plain-hyphen `-` heading. Calibrated against F1's already-MERGED plan (`.../store-disable-service-261/plan.2026-07-07T18-00.md`, PR #275): it fails identically. So it rejects known-good shipped plans and cannot be the gate.

The local `.claude/hooks/validate-planner-output.ps1` uses a hyphen-only regex `^### Phase (?<Phase>\d+)\s+-\s+...` that ALSO fails em-dash, yet did not fatally block the atomic-planner SubagentStop (planner returned normally with em-dash). Em-dash is canonical per the `atomic-plan-contract` skill and every committed plan in the repo.

**Why:** the `.claude` bundle was pushed down from a reference repo; multiple validators diverge from the repo's actual artifacts. See [[orchestrator-state-validator-divergence]] and [[remediation-plan-em-dash-required]].

**How to apply:** KEEP em-dash `### Phase N — <Title>` (repo convention + skill canonical). Treat a MCP plan-validator rejection of em-dash headings as a documented false-negative; do NOT contort the plan to it. The operative plan gate is the atomic-planner structural self-check + an atomic-executor `DIRECTIVE: PREFLIGHT VALIDATION ONLY` pass. Record the determination in the checkpoint. (The MCP validator's orchestrator-state mode with `require_model_routing` DID work correctly this session — the defect is specific to the plan artifact_type's phase-heading regex.)
