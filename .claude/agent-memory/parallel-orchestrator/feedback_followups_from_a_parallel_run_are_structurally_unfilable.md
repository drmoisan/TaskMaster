---
name: followups-from-a-parallel-run-are-structurally-unfilable
description: Neither the parent nor an item child can file a follow-up issue found during a parallel run — three independent blocks close every path, so enumerate them in the PR body and hand them to the operator instead of promising to file them
metadata:
  type: feedback
---

Never state, or let a child state, that a follow-up issue discovered during a parallel run will be
filed. Enumerate follow-ups in the PR body and in the committed `code-review` artifact, then hand
them to the operator as an explicit action in the final report.

**Why:** three independent blocks close every filing path, and they were hit twice in one run on
`bugs-2026-09-06` (2026-09-07).

1. **The parent has no promotion tool.** The `parallel-orchestrator` tool set carries
   `collect_pr_context` and `validate_orchestration_artifacts` and nothing else from the MCP
   surface. On item 797 I told the operator I would promote finding CR-1 once the item merged, then
   discovered after merging that I never had the capability.
2. **The child cannot use the command-line path.** `gh issue create` is denied project-wide with
   `PROMOTION_MCP_ONLY_BLOCKED`.
3. **The child cannot use the MCP path either, once its scope gate has passed.** Promotion writes a
   file under `docs/features/potential/`. That path is outside the item's declared write set, so it
   falsifies a scope gate that has already passed, and committing it pushes a new head that cancels
   the in-flight green CI run and supersedes a result that was already green. Item 796 hit this with
   five follow-ups and correctly declined to file any of them.

The consequence worth remembering is the shape, not the individual blocks: **the window in which an
item's own run can file a follow-up closes at its scope gate, and the parent never had the
capability at any point.** So there is no actor in a parallel run who can file a late follow-up.

**How to apply:**

- **Check the tool set before stating any plan that depends on a capability.** The promotion case is
  only where this bit; the general rule is what matters. A promise to do something later is a claim
  about a capability, and it is cheap to verify before making it and expensive to retract after.
- **Late follow-ups are a reporting obligation, not a filing one.** Confirm they reached both the PR
  body's Follow-ups section and the committed `code-review` artifact — those survive the merge — and
  then name them in the final report as work only the operator can start.
- **Do not route around the block from the session worktree.** The promotion hooks are project-wide
  and match on the command string rather than on the working directory, so relocating the command
  changes nothing. See [[preimplementation-gate-scope]] for the sibling case where a `cd` prefix
  defeats static command analysis, and
  [[parallel-run-execution-playbook]] for the related family in which merely
  QUOTING a promotion tool name in unrelated prose trips the same gate.
- Related: [[close-delivered-but-open-issues-from-parallel-add]] covers the opposite direction —
  closing an issue that is already delivered — which IS within reach because it needs only `gh`.
