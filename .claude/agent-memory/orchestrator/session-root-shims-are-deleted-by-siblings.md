---
name: session-root-shims-are-deleted-by-siblings
description: PRD_FEATURE_BLOCKED fires because hooks resolve the feature folder against the SESSION cwd, which is on a different branch; the seeded shim also gets deleted mid-run by concurrent siblings
metadata:
  type: project
---

When an epic child orchestrator runs with its session cwd on one branch and the feature worktree elsewhere, `Agent(atomic-planner)` and `Agent(prd-feature)` delegations fail with:

`PRD_FEATURE_BLOCKED: ... work mode could not be determined from '<feature>/issue.md' (the '- Work Mode:' marker is absent, unreadable, or unrecognized)`

**Why:** the hook resolves the feature folder against the **session** working directory. The session checkout is on a different branch that does not carry the feature folder at all, so the marker is "absent" from the hook's point of view even though the authoritative worktree copy is correct. Verify the worktree copy before touching anything — on 489 it read `- Work Mode: full-bug` at `issue.md:6` the whole time.

**How to apply:** seed byte-identical copies of `issue.md` and `spec.md` from the feature worktree into the session checkout at the same relative path, verify with `cmp`, then retry. This is the sanctioned "seed a truthful record and retry" remedy, not evasion — sibling 442 had already done the same, leaving its folder untracked (`??`) in the session checkout.

**The part that surprises you the second time:** the seeded directory gets **deleted mid-run** by a concurrent sibling cleaning the shared session checkout. On 489 it vanished between the plan-amendment delegation and the remediation-plan delegation. Do not assume it survives — **re-seed immediately before every delegation**, alongside the checkpoint reinstall.

Related: [[shared-checkpoint-read-modify-write-corrupts]], [[model-routing-hook-reads-canonical-path-only]], [[parent-session-can-commit-into-child-worktree]].
