---
name: mcp-plan-validator-requires-lf
description: The MCP validate_orchestration_artifacts plan validator rejects CRLF line endings; normalize the plan to LF before the mandatory validator gate
metadata:
  type: feedback
---

The `mcp__drm-copilot__validate_orchestration_artifacts` plan validator (`artifact_type: "plan"`) requires LF line endings. A plan committed with CRLF (Windows default) fails with every `### Phase N — <Title>` heading and every `- [ ] [P#-T#]` task line flagged plus "Plan does not contain any canonical phase headings", even though the em-dash and task format are correct.

**Why:** The executor's textual PREFLIGHT (`PREFLIGHT: ALL CLEAR`) tolerates CRLF, so a plan can be genuinely "preflight-cleared" and committed yet still fail the separate MCP validator gate that `atomic-plan-contract` requires before treating a plan as approved. Confirmed empirically 2026-07-08 (#262): identical minimal plan passed with LF, failed with CRLF; hyphen-vs-em-dash was ruled out (em-dash is correct and required — see [[remediation-plan-em-dash-required]]).

**How to apply:** Before running the MCP plan validator, if the plan fails on headings/tasks that look canonical, check line endings (`file <plan>`). Normalize CRLF->LF with `tr -d '\r'` — this is content-preserving and is NOT re-planning or regenerating, so it is allowed even under an implementation-only mandate. Note git may re-apply CRLF on checkout via autocrlf, but the working-tree file the validator reads is what matters, and the committed blob content is unaffected. Related: [[orchestrator-state-validator-divergence]].
