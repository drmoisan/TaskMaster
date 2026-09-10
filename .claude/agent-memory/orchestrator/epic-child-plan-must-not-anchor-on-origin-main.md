---
name: epic-child-plan-must-not-anchor-on-origin-main
description: A plan that anchors its git diffs on `git merge-base HEAD origin/main` is wrong for an epic child; the merge base sits behind every already-merged sibling and bills their paths to your feature
metadata:
  type: project
---

An atomic plan authored for an epic child must anchor every `git diff` on the **integration base** the child branched from, never on `git merge-base HEAD origin/main`.

**Why:** an epic child branches from the integration branch, which by mid-epic carries several merged siblings that are *not* on `origin/main`. The merge base with `origin/main` therefore resolves to a commit *behind* all of them. Every diff anchored on it reports the siblings' work as part of your footprint. Measured on F824 of `review-residuals-2026-09-08` (2026-09-09): the anchor resolved to `6f08302a` while the true branch base was `553f874a`, and the inherited listing carried 304 paths of which 277 were outside the feature's Owned Write Set. The correct anchor produced exactly the Owned Write Set.

This is the mirror image of [[stale-base-anchor-passes-ancestry-vacuously]], which says to compare against `origin/main`. That advice is for a *standalone* feature. For an epic child `origin/main` is the wrong ref, so read the two together and pick by topology, not by habit.

**How to apply:**

- When preparing or preflighting a plan for an epic child, grep the plan for `origin/main` before approving it. Substitute the integration branch ref, or the literal base commit resolved at execution time.
- If it slips through and an inertness task fails on it, the correct executor behaviour is to **leave that task unchecked** and record why, not to tick it and not to halt the whole run. `atomic-plan-contract` requires checklist state to match evidence on disk, and one honestly-unchecked task is a truthful artifact. F824 shipped 68/69 that way.
- Do not accept the executor's fallback of a *symbolic* `HEAD` anchor as sufficient. `git diff HEAD` is ambient state: it is only meaningful before the run commits, and it passes vacuously afterwards. Re-measure yourself with an explicit ref, `git diff --name-status <integration-base> HEAD`, which survives the commits. Related: [[three-dot-diff-degenerates-on-ancestor-base]].
- The defect belongs upstream in the planning surface. Fixing the plan file in-repo fixes one child; the next epic reproduces it.
