---
name: mcp-plan-validator-defective-em-dash
description: MCP plan validator now ACCEPTS canonical em-dash headings (as of 2026-07-10 bundle); the remaining em-dash rejecter is the SubagentStop planner hook's ASCII-hyphen regex
metadata:
  type: reference
---

UPDATED 2026-07-10 (swordfish-removal F5 prep, epic-planner bundle after commit "update claude for epic planner"): `mcp__drm-copilot__validate_orchestration_artifacts` with `artifact_type: "plan"` now ACCEPTS the canonical `### Phase N — <Title>` heading (em-dash U+2014) and returned `ok:true` for a 6-phase/41-task em-dash plan. So the MCP plan validator IS usable as the mandated `atomic-plan-contract` gate now — run it. (Earlier sessions, calibrated against F1's merged plan under PR #275, saw it reject em-dash; that behavior appears fixed in the current bundle. If a future checkout regresses, fall back to the executor preflight as the gate.)

The remaining em-dash rejecter is the SubagentStop hook `.claude/hooks/validate-planner-output.ps1`, whose regex `^### Phase (?<Phase>\d+)\s+-\s+...` is ASCII-hyphen-only and does not match em-dash — yet it did NOT fatally block the atomic-planner SubagentStop (planner returned normally with an em-dash plan both this session and before). Watch for an atomic-planner that "fixes" this hook (broadens the regex to `[-—]`) plus its own agent-memory: that is an OUT-OF-SCOPE repo change. Revert both (`git checkout -- <hook> <planner-memory>`) so a feature branch carries only its feature folder + plan; the MCP validator + executor preflight are the gates, and the plan keeps canonical em-dash.

**Why:** the `.claude` bundle was pushed down from a reference repo; validators historically diverged from the repo's actual artifacts, and the bundle is being actively updated. See [[orchestrator-state-validator-divergence]] and [[remediation-plan-em-dash-required]].

**How to apply:** KEEP em-dash `### Phase N — <Title>`. Run the MCP plan validator (it works now) AND the atomic-executor `DIRECTIVE: PREFLIGHT VALIDATION ONLY` pass; record both in the checkpoint. The MCP validator's orchestrator-state mode with `require_model_routing` also works (requires the promotion keys — `promotion-type`, `short-name`, `relativeFile`, `long-name`, `issue-num`, `feature-folder`, `work-mode`, `plan-path` — at the checkpoint TOP LEVEL, not nested under `variables`).
