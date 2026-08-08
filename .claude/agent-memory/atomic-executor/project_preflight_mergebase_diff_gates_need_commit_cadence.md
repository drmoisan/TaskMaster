---
name: preflight-mergebase-diff-gates-need-commit-cadence
description: A plan whose gates read `git diff <MERGE_BASE>..HEAD` is vacuous while HEAD == merge-base; preflight must require explicit commit tasks and must allow docs/agent-memory paths in the diff
metadata:
  type: project
---

When preflighting a plan whose verification gates are expressed as `git diff --numstat|--name-only <MERGE_BASE>..HEAD` (zero-line-diff / scope-lock / file-size audits), check two things mechanically before signing off:

1. **Does HEAD actually differ from the merge-base by the time the gate runs?** On a freshly branched worktree `git rev-parse HEAD` equals `<MERGE_BASE>`, so every such gate returns an empty diff and passes for the wrong reason. The plan needs explicit commit tasks in the cadence — one after Phase 0 (so planning/baseline artifacts land and HEAD advances), one after the last source-editing phase (so the diff gates observe the real change set), and one at the end (so evidence is committed and the final clean-worktree gate is satisfiable).
2. **Does the gate's binary outcome tolerate the non-source paths that the commit cadence necessarily introduces?** Committing planning artifacts puts `docs/features/**` and `.claude/agent-memory/**` into the diff. A gate worded "no path outside the scope lock appears" becomes unsatisfiable unless it is narrowed to `.cs`/`.csproj`/`.xml`/`.sln` or explicitly exempts the documentation/evidence trees. Same for a Phase 0 gate demanding a clean `git status --porcelain` when planning artifacts are legitimately uncommitted at that point.

Related consequence: a gate that runs on the *committed* diff after a later mutating step (for example a CSharpier format pass in the final QC phase) measures pre-mutation content. That is acceptable when the mutating step's own argument list is scope-locked, but the plan should say so rather than claim the gate is "post-format".

**Why:** #503 preflight pass 1 rejected the plan for exactly this class of defect (vacuous diff gates plus unsatisfiable clean-worktree gates); pass 2 cleared after commit tasks `P0-T13`, `P4-T7`, `P7-T32` were added and the diff-gate wording was widened.

**How to apply:** During preflight of any plan with merge-base diff gates, run `git rev-parse HEAD` and `git status --porcelain` in the target worktree, compare against the plan's stated preconditions, and require the commit cadence as a plan delta rather than assuming the executor will commit opportunistically. See [[project_preflight_selfderived_gate_thresholds_are_blind]] for the sibling failure mode (gates that validate against numbers derived from the run being validated).
