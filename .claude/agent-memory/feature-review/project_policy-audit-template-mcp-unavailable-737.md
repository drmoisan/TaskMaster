---
name: policy-audit-template-mcp-unavailable-737
description: mcp__drm-copilot__resolve_policy_audit_template_asset and validate_orchestration_artifacts are absent from the feature-review agent's tool set; hand-author the audit preserving the 12 canonical headings instead
metadata:
  type: project
---

At #737's review, no `mcp__*` tools were exposed to the feature-review agent session at all (same absence pattern as [[pr-context-mcp-unavailable-manual-fallback]], but for the `policy-audit-template-usage` skill's `mcp__drm-copilot__resolve_policy_audit_template_asset` / `mcp__drm-copilot__validate_orchestration_artifacts` tools specifically, not the PR-context tool).

**Why:** the skill's own fallback path only covers this ("If MCP asset resolution fails, create a minimal policy audit artifact marked BLOCKED and document the missing template resolution") but a fully-BLOCKED artifact is unnecessarily weak when the skill also publishes the exact required heading list (`## Executive Summary` through `## Appendix B`).

**How to apply:** when these MCP tools are absent from the tool list (not merely erroring), hand-author `policy-audit.<ts>.md` preserving all 12 canonical major headings verbatim from `policy-audit-template-usage/SKILL.md` §5, and record a `## Template Resolution Deviation` section at the top stating the tools were unavailable rather than marking the whole artifact BLOCKED — the review can still be evidence-complete even without the MCP-templated scaffold.
