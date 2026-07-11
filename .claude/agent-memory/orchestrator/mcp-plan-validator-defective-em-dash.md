---
name: mcp-plan-validator-defective-em-dash
description: MCP plan validator ACCEPTED canonical em-dash headings on 2026-07-10 (F5 #308 and F2 #307 preps); earlier sessions saw rejection, so treat as version-dependent — run it and read the actual result; executor preflight remains a co-gate
metadata:
  type: reference
---

UPDATED 2026-07-10 (epic swordfish-removal, confirmed independently by F5 #308 prep and F2 #307 prep, on the epic-planner bundle after commit "update claude for epic planner"): `mcp__drm-copilot__validate_orchestration_artifacts` with `artifact_type: "plan"` ACCEPTS the canonical `### Phase N — <Title>` heading (em-dash U+2014) on LF-line-ending plans — it returned `ok:true` for a 6-phase/41-task plan (F5) and a 9-phase/51-task plan (F2, `.../2026-07-10-swordfish-collection-stack-lineage-307/plan.2026-07-10T20-14.md`). The MCP plan validator IS usable as the mandated `atomic-plan-contract` gate — run it and read its actual result; do not pre-assume rejection.

Prior observation (kept for contrast): earlier sessions saw the same tool REJECT the canonical em-dash heading ("phase heading must match `### Phase N — <Title>`" / "Plan does not contain any canonical phase headings"), and also reject a plain-hyphen `-` heading, calibrated against F1's already-MERGED plan (`.../store-disable-service-261/plan.2026-07-07T18-00.md`, PR #275). The blanket "defective on em-dash" claim is therefore NOT reliable across bundle versions — behavior is version-dependent. Possible drivers: bundle version, or CRLF vs LF (a CRLF plan fails the validator regardless — see [[mcp-plan-validator-requires-lf]]). If a future checkout regresses, fall back to the executor preflight as the gate.

The remaining em-dash rejecter is the SubagentStop hook `.claude/hooks/validate-planner-output.ps1`, whose regex `^### Phase (?<Phase>\d+)\s+-\s+...` is ASCII-hyphen-only and does not match em-dash — yet it did NOT fatally block the atomic-planner SubagentStop (planner returned normally with an em-dash plan both this session and before). Watch for an atomic-planner that "fixes" this hook (broadens the regex to `[-—]`) plus its own agent-memory: that is an OUT-OF-SCOPE repo change. Revert both (`git checkout -- <hook> <planner-memory>`) so a feature branch carries only its feature folder + plan; the MCP validator + executor preflight are the gates, and the plan keeps canonical em-dash.

**Why:** the `.claude` bundle was pushed down from a reference repo; validators historically diverged from the repo's actual artifacts, and the bundle is being actively updated. See [[orchestrator-state-validator-divergence]] and [[remediation-plan-em-dash-required]].

**How to apply:** KEEP em-dash `### Phase N — <Title>`. Run the MCP plan validator (it works now) AND the atomic-executor `DIRECTIVE: PREFLIGHT VALIDATION ONLY` pass; record both in the checkpoint. The MCP validator's orchestrator-state mode with `require_model_routing` also works (requires the promotion keys — `promotion-type`, `short-name`, `relativeFile`, `long-name`, `issue-num`, `feature-folder`, `work-mode`, `plan-path` — at the checkpoint TOP LEVEL, not nested under `variables`).
