---
name: epic-status-needs-its-own-followup-pr
description: epic-status.md's final regeneration can never ride in the integration PR — plan a docs-only follow-up PR off main, and expect the final PR to be the epic's only real CI gate
metadata:
  type: feedback
---

The final regeneration of `docs/features/epics/<slug>/epic-status.md` cannot be part of the
integration-to-`main` pull request. Land it as a separate docs-only follow-up PR branched off
`origin/main` after the integration PR merges.

**Why:** the values that regeneration must record — the integration PR's `merge_commit_sha`,
`merged_at`, its CI run id and per-check results, and the post-merge issue states — do not exist
until *after* that PR has merged. Completion requirement 3 ("`epic-status.md` reflects the completed
state") is therefore structurally unreachable inside the PR it describes. Attempting to pre-write
those fields would mean asserting a merge that has not happened.

**How to apply:**

- Budget for a second PR at the end of every epic. On 2026-08-22 that was PR #596 (merged
  `d15f9510`) following integration PR #595 (merged `20462ed7`).
- Do NOT reuse the session checkout for it — it is routinely mid-work on another branch with
  uncommitted changes. Create a dedicated worktree off `origin/main`; unlike the child worktrees it
  is not framework-locked, so `git worktree remove` succeeds cleanly afterwards.
- Supersede stale notes *in place* with the reason and the outcome that overtook them, rather than
  deleting them. Two recur every time: the kickoff-era instruction about which issues the final PR
  must close (overtaken whenever a child is descoped), and any "re-check X against the integrated
  tree" instruction (discharged by the final PR's CI).

**Corollary worth knowing before you plan the epic:** because `ci.yml` triggers `pull_request` only
on `[main, development]`, the integration PR is the *first and only* pull request in the entire epic
that receives real GitHub Actions CI. Every child PR based on the integration branch got zero
checks. Treat the final PR's run as the epic's actual CI gate, verify its `headSha` equals the PR's
`headRefOid` before accepting it, and note that `scripts/orchestration/Invoke-CiGateParser.ps1`
does not exist here — evaluate the gate from `gh pr checks` plus `gh run view` and report the parser
as absent rather than skipping silently.

Related: [[project_child_pr_ci_gap_integration_base]],
[[project_require_complete_launch_binding_gate_unsatisfiable]],
[[feedback_merged_child_worktree_still_locked_defer_removal]].

## Update (quickfiler-bug-family, 2026-08-28): the follow-up PR is itself gate-blocked

Confirmed again — the regeneration must come after the integration PR merges, on a branch cut from the
resulting `main`, because the document asserts that the integration PR merged.

**New finding: you will not be able to merge that follow-up.** `EPIC_MERGE_GATE_BLOCKED` offers three
paths and none fits an epic-documentation follow-up:
- per-feature `epic_mode == true` — false; `epic_mode` marks a *child feature* targeting the
  integration branch, not the orchestrator's own docs PR targeting `main`;
- epic checkpoint with a **matching `pr_number`** — `epic_merge_pr` is the *integration* PR (673 here),
  and repointing it at the docs PR would falsify which PR integrated the epic;
- parallel route — not applicable.

Do **not** set `epic_mode` or repoint `epic_merge_pr` to pass it. Both are false statements written
solely to satisfy a control, and the gate's actual purpose — a verified CI gate — was already met
(run green on a head SHA equal to the PR head). Open the PR, verify its gate, record the blockage, and
hand it to the maintainer for a manual merge or an upstream fix. Same shape as
[[merged-child-worktree-still-locked-defer-removal]]: a push-down-owned hook with no jurisdiction path.
