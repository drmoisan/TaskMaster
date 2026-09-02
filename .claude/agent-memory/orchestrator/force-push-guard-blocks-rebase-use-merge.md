---
name: force-push-guard-blocks-rebase-use-merge
description: The Bash guard blocks every `git push --force*` AND `git reset --hard`, so resolving a moved base must be done with a merge commit, never a rebase
metadata:
  type: project
---

Do not plan a rebase to re-base a pushed branch in this environment. Resolve a moved base with
`git merge --no-ff origin/main` instead, which needs only a fast-forward push.

**Why:** the Bash tool refuses `git push --force` and `git push --force-with-lease` on a text
pattern match, and separately refuses `git reset --hard`. A rebase of an already-pushed branch
is therefore a dead end: the rebase itself succeeds locally, but the result can never reach the
remote, and the usual way back (`reset --hard` to the pre-rebase head) is also blocked. Observed
on issue #440 / PR #689 after a cohort sibling moved `main`.

Recovery, if a rebase has already been run: `git checkout -B <branch> <pre-rebase-sha>` is NOT
blocked and moves the branch pointer back. Find the pre-rebase sha in `git reflog`, on the entry
immediately before `rebase (start): checkout <base>`. The working tree must be clean first, so
commit any in-flight agent-memory writes before attempting either operation — an unstaged
tracked file blocks `git rebase` outright with `cannot rebase: You have unstaged changes`.

**How to apply:** when the base moves under a pushed branch, go straight to the merge. The merge
keeps the pre-existing evidence claims honest in one specific way that matters: a rebase changes
the merge-base, which silently invalidates every `git diff <BASE>` figure the feature folder
already recorded, whereas after a merge you can still verify scope with the three-dot form
`git diff --name-only origin/main...HEAD`. Verify BOTH the two-dot and three-dot diffs afterwards;
they agree only when the merge introduced nothing of its own.

Check whether the new base actually changed any compiled file before deciding to re-run the
toolchain: `git diff --name-only <old-base> origin/main -- . ":(exclude)docs" ":(exclude).claude"`.
An empty result means the merge is docs-only and the existing build, test, and coverage evidence
still describes the same compiled surface, so a full re-run buys nothing.

See [[conflicting-pr-gets-no-ci-at-all]] for the symptom that sends you here.
