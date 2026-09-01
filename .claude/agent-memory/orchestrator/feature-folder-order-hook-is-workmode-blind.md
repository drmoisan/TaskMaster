---
name: feature-folder-order-hook-is-workmode-blind
description: enforce-feature-folder-order.ps1 unconditionally demands user-story.md before any docs/features/active/<folder>/plan.md write, which blocks every full-bug plan; the fix is the repo's own plan.<timestamp>.md convention, never creating the stub
metadata:
  type: project
---

`.claude/hooks/enforce-feature-folder-order.ps1` denies a `Write` to
`docs/features/(active|archive)/<folder>/plan.md` unless **all three** of `issue.md`, `spec.md`, and
`user-story.md` exist in that folder. `Get-FeatureFolderMissingFile` hard-codes
`$required = @('issue.md', 'spec.md', 'user-story.md')` and never reads the `- Work Mode:` marker.

**Why this is a defect, not a gate you should satisfy.** `atomic-plan-contract` and
`acceptance-criteria-tracking` both say a `full-bug` item has `spec.md` as its SOLE acceptance-criteria
source and that `user-story.md` should be **absent**. The same contract says validation must "fail
closed when `spec.md` or `user-story.md` exists unexpectedly in the active folder." So creating the
stub to clear the hook actively corrupts work-mode integrity for every downstream executor and
reviewer. The hook and the contract are in direct conflict, and the contract is authoritative.

**The resolution: name the plan `plan.<timestamp>.md`.** `Test-IsFeaturePlanPath` matches only
`(^|/)docs/features/(active|archive)/[^/]+/plan\.md$`, so a timestamped name does not match and the
hook allows the write. This is not evasion — it is already the overwhelming repository convention:
on 2026-09-01, 49 of the ~50 plans under `docs/features/active/` were named `plan.<timestamp>.md`,
`remediation-plan.<timestamp>.md`, and similar. Exactly one used the bare `plan.md`. Verified on
issue #285 (`plan.2026-09-01T00-30.md`), which then passed the MCP plan validator on the first call.

This also restores the planner's write access: once the canonical path is timestamped, `atomic-planner`
can revise it in place across preflight rounds, so you do not have to relay every delta by hand.

**A single timestamped file does not violate plan-path continuity.** The contract forbids timestamped
*sibling* files accumulating across revision rounds. One canonical timestamped file, revised in place,
satisfies it exactly. Say so explicitly in the report if the caller asked for `plan.md`.

**Note `atomic-planner` cannot recover from this itself.** It has no Bash tool, so when the Write is
denied it can only park the finished plan text in scratchpad and return `SELF-REVIEW: BLOCKED`. The
orchestrator then places it with a plain `cp`, which keeps the bytes verbatim and costs no context —
do NOT Read-then-Write a 50 KB plan to move it. There is no `SendMessage` tool, so you cannot ask the
blocked planner to retry under a new name; see [[one-executor-per-worktree]] and
[[agent-tool-cannot-course-correct-running-subagent]].

The real fix is upstream in `drm-copilot`: the hook should read the work-mode marker and drop
`user-story.md` from the required set for `full-bug`. `.claude/**` here is push-down-owned, so do not
patch it in this repository.

Related: [[project_claude_files_are_pushdown_owned_fix_upstream]],
[[atomic-planner-lacks-mcp-validator-tool]], [[mcp-plan-validator-requires-lf]].
