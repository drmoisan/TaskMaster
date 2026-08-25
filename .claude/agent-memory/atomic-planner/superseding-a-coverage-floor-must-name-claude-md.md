---
name: superseding-a-coverage-floor-must-name-claude-md
description: A plan clause that supersedes the repo-wide coverage floor must enumerate CLAUDE.md alongside the .claude/rules files, because CLAUDE.md carries the same floor independently at authority rank 1 - an exhaustive-reading enumeration that omits it implies its floor survives
metadata:
  type: feedback
---

When a plan records a settled scope decision that supersedes the repository-wide coverage floor for its change set, the enumeration of superseded sources must include `CLAUDE.md` (§ General Unit Test Policy, UT2), not only `.claude/rules/csharp.md`, `.claude/rules/general-unit-test.md` and `.claude/rules/quality-tiers.md`.

**Why:** `CLAUDE.md:303` carries its own repository-wide `>= 80%` floor independently of the rule files. It is authority rank 1 in the policy-compliance order and is read by the Phase 0 policy-read task, so the executor holds it in context. Because such a clause reads as an exhaustive enumeration, omitting `CLAUDE.md` positively implies its floor SURVIVES the supersession — a worse outcome than writing no clause at all, since CLAUDE.md's halt-on-conflicting-instructions rule then fires mid-execution on the one source the plan cannot dismiss. Caught as a blocking finding on the #446 family plan at preflight round 3 (2026-08-25); the neighbouring bullet in [[project_446_quickfiler_bug_family_plan_seams]] had already flagged the `.claude/rules/csharp.md` half of the same trap at round 2 without noticing CLAUDE.md.

**How to apply:** any time a plan demotes a repo-wide gate to record-and-report, or otherwise declares a policy floor superseded. Grep the floor's numeral across `CLAUDE.md` and `.claude/rules/` and enumerate every carrier. Related: [[single-numeral-gates-must-name-the-role]], [[project_coverage_threshold_conflict_claude_md_vs_general_unit_test]].
