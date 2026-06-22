---
name: plan-validator-phase-heading-constraint
description: The MCP plan validator rejects any token between `Phase N` and the em-dash; phase headings must be exactly `### Phase N — <Title>`. The plan H1 title line is exempt (not a phase heading).
metadata:
  type: feedback
---

`mcp__drm-copilot__validate_orchestration_artifacts` with `artifact_type: "plan"` rejects phase headings that place any token (including `(continued)` or `(...)`) between `Phase N` and the em-dash. The only passing form is `### Phase N — <Title>`. Error: `phase heading must match \`### Phase N — <Title>\`` then `task appears before a canonical phase heading`.

**Why:** Confirmed via orchestrator memory [[remediation-plan-em-dash-required]] (issue #25, 2026-05-28). A parenthetical sub-qualifier in a phase line failed the validator.

**How to apply:** When authoring/revising a plan, keep all `### Phase N` headings canonical with an em-dash and no parenthetical qualifiers. The document H1 (e.g. `# <slug> — Increment 2 (Plan)`) is NOT a phase heading and may contain parentheticals safely. If the `mcp__drm-copilot__validate_orchestration_artifacts` tool is unavailable in the session, do a structural self-check (canonical phase headings, sequential `[P#-T#]` IDs, evidence paths under `<FEATURE>/evidence/{baseline,qa-gates,regression-testing}/`, no forbidden `artifacts/` evidence paths) and report the validator as NOT RUN rather than claiming a pass.
