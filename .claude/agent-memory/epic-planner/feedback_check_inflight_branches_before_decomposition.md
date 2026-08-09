---
name: check-inflight-branches-before-decomposition
description: Before computing an epic decomposition, diff each candidate feature's target files against every unmerged local branch and worktree, not just origin/main
metadata:
  type: feedback
---

Before deciding a decomposition or wave layering, enumerate unmerged work and check whether any
candidate feature's target files are being moved or rewritten there:

```
git worktree list --porcelain
git branch -a
git diff --stat origin/main...<branch> -- '*.cs'
git show <branch>:<path> | grep -n <symbol>
```

Also read that branch's `artifacts/orchestration/orchestrator-state.json` (`next_step`,
`step9_status`) from its own worktree to judge how far it is from merging. A branch at
`step9_status: pending` with no PR is not about to land.

**Why:** Bugs promoted out of another feature's review are usually located in the exact code that
feature is refactoring. In the 505/506/507/508 planning run, all four issues were spawned by the
in-flight `bug/ribbon-engine-readiness-guard-503` branch, and that branch relocated the four
callback methods named by #505 and #506 out of `RibbonViewer.cs` into a new
`RibbonViewer.EngineCommands.cs` while carrying both defects forward unchanged. An integration
branch cut from `origin/main` could not even have run preparation for those two issues: the
atomic-planner would have targeted a path the parent branch deletes, guaranteeing a modify/delete
conflict at fan-in. Checking only `origin/main` would have missed this entirely.

**How to apply:** Run this check during the epic-worthiness gate, before authoring the manifest or
creating the integration branch. When an unmerged branch relocates or rewrites a candidate's target
file, that candidate is sequencing-blocked behind it; say so rather than adding a `depends_on` edge
(the blocker is outside the epic's DAG and cannot be modeled as a wave). Split the objective into
the conflict-free subset that can proceed now and the blocked remainder.

Two related decomposition rules this run confirmed:
- Two issues whose fixes edit adjacent lines of the same methods cannot be separate parallel
  features; merge them into one feature (one PR closing both issues) rather than serializing them
  with a dependency edge, which costs a wave and still risks conflict.
- A "contract" coupling (feature A consumes a property feature B makes null-safe) is only a real
  `depends_on` edge if A cannot be written correctly under B's current contract. Prefer encoding
  the constraint in A's prepared plan over adding an edge, since preparation happens with full
  knowledge of both.

Related: [[epic-planner-state-required-fields]], [[epic-plan-tooling-not-vendored]].
