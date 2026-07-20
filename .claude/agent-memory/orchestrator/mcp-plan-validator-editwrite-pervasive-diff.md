---
name: mcp-plan-validator-editwrite-pervasive-diff
description: When the MCP plan validator rejects an LF plan with "no canonical phase headings" after an Edit/Write, the tool write introduced a pervasive difference; restore via git-bash cp/sed/perl
metadata:
  type: feedback
---

When `mcp__drm-copilot__validate_orchestration_artifacts` (artifact_type=plan) rejects EVERY phase heading and task line and ends with "Plan does not contain any canonical phase headings", but the file is LF (0 CRLF), has no BOM, and the heading em-dashes hex as `e2 80 94`, the defect is NOT in your visible markdown.

**Why:** On 2026-07-19, editing an already-approved partially-executed plan (#366) via the Edit/Write tool produced a file that failed the validator, while a byte-goal-identical LF copy produced by git-bash `cp`/`sed`/`perl` PASSED. `diff` reported a whole-file change (`1,218c1,219`) between the two LF/no-BOM files, i.e. the Edit/Write write path introduced some pervasive difference the validator chokes on. The failure was also cumulative: removing either of two individually-well-formed prose blocks (my added Scope-Invariants bullet OR a pre-existing very long `- RATIFIED:` sub-bullet) made the same file pass — a genuine parser fragility, not a content defect.

**How to apply:** (1) Prefer restoring/normalizing plan text with git-bash file ops (`cp` a known-passing variant, `sed -i`, `perl -0pi`) rather than the Edit/Write tool when the MCP plan validator is being fragile; those writes validated cleanly. (2) Fold oversized single-bullet prose out of the plan (into the checkpoint `epic_decisions` or a task body) to dodge the cumulative quirk. (3) Remember the executor's textual preflight — not the MCP validator — is the substantive planner->executor gate (see [[mcp-plan-validator-requires-lf]], [[mcp-plan-validator-defective-em-dash]]); a structurally-correct plan that trips only this quirk is still executable. (4) The committed HEAD plan passing while your working copy fails is a fast way to confirm the defect is in your edit path, not the plan structure.
