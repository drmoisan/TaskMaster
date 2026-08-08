---
name: crlf-plans-validate-do-not-normalize
description: CRLF plan files pass the MCP plan validator; never add a CRLF-to-LF normalization step or claim the validator requires LF
metadata:
  type: feedback
---

Never write a CRLF-to-LF normalization instruction into a plan, spec, or decision record, and never
assert that the MCP plan validator requires LF line endings.

**Why:** Verified by `epic-planner` on the `quickfiler-per-file-coverage` epic and re-confirmed
during the #432 F1 plan revision (2026-08-07). `core.autocrlf=true` plus `* text=auto` in
`.gitattributes` means committed plans do materialize as **pure CRLF** on a fresh Windows checkout —
that half of the original hazard report is accurate. But the validator accepts them: all six
committed epic-child plans were re-validated in the integration worktree with `artifact_type:
"plan"` and every one returned `ok: true`. The claim is recorded as verified fact in the epic
manifest's `## Verified Toolchain and Tooling Facts` section. A normalization step is pointless
churn against files that already pass, and it produces a whole-file diff.

**How to apply:** When a caller reports a "CRLF hazard" for a plan, cite the manifest section rather
than acting. Edit plans in place with the Edit tool so the existing line endings are preserved; do
not rewrite the whole file with Write unless the file is new. Note this is unrelated to the separate,
real CRLF concern on **legacy non-SDK `.csproj` files** (see
[[project_432_coverage_ledger_plan_seams]]), where `sed -i` genuinely strips CRLF and guarantees a
fan-in conflict — that guidance stays.

Related: [[plan-validator-phase-heading-constraint]],
[[plan-validator-task-id-sequential-constraint]],
[[project_planner_mcp_validator_not_in_tool_surface]].
