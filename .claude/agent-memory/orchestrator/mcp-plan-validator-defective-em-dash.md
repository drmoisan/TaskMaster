---
name: mcp-plan-validator-defective-em-dash
description: The MCP plan validator's em-dash rejection is INTERMITTENT/version-dependent — it accepted canonical em-dash+LF plans cleanly on 2026-07-10; do not assume it is broken, but keep em-dash and use executor preflight as the authoritative gate
metadata:
  type: reference
---

STATUS UPDATE (2026-07-10, epic swordfish-removal F2 #307): `mcp__drm-copilot__validate_orchestration_artifacts` with `artifact_type: "plan"` **PASSED** cleanly on a canonical `### Phase N — <Title>` (em-dash U+2014), LF-line-ending, 9-phase/51-task plan (`.../2026-07-10-swordfish-collection-stack-lineage-307/plan.2026-07-10T20-14.md`). So the blanket "defective on em-dash" claim is NOT reliable across bundle versions — the behavior is intermittent/version-dependent. Run the validator and read its actual result; do not pre-assume rejection.

Prior observation (kept for contrast): earlier in this repo the same tool was seen to REJECT the canonical `### Phase N — <Title>` heading (em-dash U+2014) with "phase heading must match `### Phase N — <Title>`" / "Plan does not contain any canonical phase headings", and also rejected a plain-hyphen `-` heading. Calibrated against F1's already-MERGED plan (`.../store-disable-service-261/plan.2026-07-07T18-00.md`, PR #275) it failed identically. Possible drivers of the divergence: bundle version, or CRLF vs LF (a CRLF plan fails the validator regardless — see [[mcp-plan-validator-requires-lf]]).

The local `.claude/hooks/validate-planner-output.ps1` uses a hyphen-only regex `^### Phase (?<Phase>\d+)\s+-\s+...` that ALSO fails em-dash, yet did not fatally block the atomic-planner SubagentStop (planner returned normally with em-dash). Em-dash is canonical per the `atomic-plan-contract` skill and every committed plan in the repo.

**Why:** the `.claude` bundle was pushed down from a reference repo; multiple validators diverge from the repo's actual artifacts. See [[orchestrator-state-validator-divergence]] and [[remediation-plan-em-dash-required]].

**How to apply:** KEEP em-dash `### Phase N — <Title>` (repo convention + skill canonical). Treat a MCP plan-validator rejection of em-dash headings as a documented false-negative; do NOT contort the plan to it. The operative plan gate is the atomic-planner structural self-check + an atomic-executor `DIRECTIVE: PREFLIGHT VALIDATION ONLY` pass. Record the determination in the checkpoint. (The MCP validator's orchestrator-state mode with `require_model_routing` DID work correctly this session — the defect is specific to the plan artifact_type's phase-heading regex.)
