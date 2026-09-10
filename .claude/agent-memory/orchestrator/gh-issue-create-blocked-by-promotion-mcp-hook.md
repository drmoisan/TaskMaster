---
name: gh-issue-create-blocked-by-promotion-mcp-hook
description: gh issue create is refused by PROMOTION_MCP_ONLY_BLOCKED even for the orchestrator, so a post-merge follow-up obligation has no file-free discharge path
metadata:
  type: feedback
---

`gh issue create` is denied by a PreToolUse hook with `PROMOTION_MCP_ONLY_BLOCKED`, which insists on the MCP chain `new_potential_entry` -> `potential_to_issue` -> `new_active_feature_folder`. Having `Bash(gh *)` in your tool surface does **not** mean you can file an issue.

**Why:** this bites specifically when a plan hands you an out-of-plan promotion obligation *after* the feature's PR has merged. The standing user rule is that out-of-scope findings must become real issues, and I reached for `gh issue create` as the file-free route that would not touch the branch footprint. The hook closes that route. The only permitted route writes a tracked record under `docs/features/potential/promoted/`, which needs a commit — and on a just-merged epic child there is no in-scope commit target, because the AC20 footprint gate is already discharged and the run forbids creating a branch.

**How to apply:** when a plan's "out-of-plan obligations" name promotions, resolve where they will be committed *before* the PR merges, not after. Either fold the promotion into the feature's own footprint while the branch is still open (only if its footprint AC permits it), or record the obligation in the checkpoint under a named key for the parent. Do not assume `gh` gives you an escape hatch. See [[executor-blocked-ac-may-be-orchestrator-dischargeable]] — that memory is right that you should check your own tool surface, but this is the counter-case where the surface exists and a hook still denies it.

The findings are not lost if the spec carries them: an AC that requires the findings to survive in `spec.md` with file-and-line citations gives a durable record that reaches main with the fix. That is what makes recording-instead-of-forcing an acceptable outcome rather than a dropped obligation. Related: [[footprint-ac-forbids-onbranch-followup-promotion]], [[promote-latent-defects-to-issues]].
