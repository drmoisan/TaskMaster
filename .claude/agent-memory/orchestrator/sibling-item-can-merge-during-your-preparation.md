---
name: sibling-item-can-merge-during-your-preparation
description: A parallel item's "in-flight sibling that will merge before you execute" can merge DURING your preparation run, silently invalidating a subagent already reading the tree
metadata:
  type: feedback
---

When a parallel/epic parent hands you an item and warns that a sibling item is in flight and "will merge before this one executes", treat that merge as something that can land **mid-run**, not as a future event you plan around.

**Why:** On issue #646 (2026-08-31) the parent described sibling #647 as pending and instructed me to author anchors that would survive its future rewrite of the same method. I cut the branch at `origin/main` = `9b6aff2e` and launched `task-researcher`. While the researcher was reading, #647 merged as PR #712 and `origin/main` advanced to `2b85134b`. The researcher's citations therefore described a superseded tree: it recorded the delegate as returning `Task` and the writer call as a single-line `await`, when the merged tree already had `Task<bool>` and a multi-line assignment plus a failure-logging branch.

The failure is quiet in a specific way. `git rev-parse HEAD` and `git rev-parse origin/main` agreed at branch-creation time and `git status` stayed clean, so nothing looked wrong. The divergence only surfaced when I compared `git rev-parse HEAD:<file>` against `git rev-parse origin/main:<file>` and got different blobs. A plain `git status` cannot show this, because the working tree genuinely matches the HEAD you pinned — it is the remote ref that moved underneath you.

**How to apply:**
- Re-run `git fetch` and re-compare `HEAD` to `origin/main` at every phase boundary of a preparation run, not once at the start. Do it specifically before delegating planning and before committing.
- When a sibling is named as touching your exact call site, check whether it has ALREADY merged before you write the plan: `git merge-base --is-ancestor origin/<sibling-branch> origin/main`. On #646 that returned true, which is what exposed the whole problem. Also note `git diff origin/main...origin/<sibling>` returning empty means merged, not means "no changes".
- A branch with no commits of its own can be re-anchored with `git merge --ff-only origin/main`. Do NOT reach for `git reset --hard`; it is blocked by a dangerous-command hook in this repo.
- You cannot course-correct a running subagent. There is no `SendMessage` tool in the orchestrator's surface, and a fresh `Agent(...)` call starts a SECOND agent rather than redirecting the first (see [[agent-tool-cannot-course-correct-running-subagent]]). The workable recovery is to let it finish, re-derive its load-bearing citations yourself against the corrected tree, and commit a separate correction artifact alongside its output rather than editing its file. That keeps the researcher's artifact honest as-authored and makes the supersession auditable.
- Fold the correction into the planner's prompt explicitly, naming which document wins where the two disagree. The planner will otherwise cite the stale artifact, and preflight will spend a round on it.

Related: [[stale-base-anchor-passes-ancestry-vacuously]], [[external-actor-can-merge-your-child-pr-midrun]], [[epic-child-stale-local-integration-ref]], [[reconcile-plan-numbers-against-your-own-measurements]].
