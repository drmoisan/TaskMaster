---
name: resume-brief-worktree-contents-premise-can-be-false
description: A resume brief asserting the predecessor's committed artifacts are "already present in your worktree" is a claim about a DIFFERENT worktree; verify by Glob and repair with git merge --ff-only, which creates no branch
metadata:
  type: feedback
---

When a resume brief says a killed predecessor's work "is already present in your worktree", verify it
by Glob before doing anything else, and if it is absent check whether your branch is simply an
ancestor of the branch that carries it.

**Why:** on preparation child #815 (2026-09-09) the brief stated that `issue.md`, `spec.md` and the
research artifact were already in the worktree because the predecessor's commits had been merged into
the epic integration branch. They were merged — but this *agent worktree* had been cut at `6f08302a`,
two commits before the epic commits that carried them, so `Glob` over the feature folder returned
nothing. The brief's premise was true of the repository and false of my checkout. Reporting BLOCKED
(which the brief instructed on a missing artifact) would have halted a run whose inputs existed and
were reachable.

**How to apply:**

1. `git rev-parse --show-toplevel` for your real root, then `git status --short --branch`. An agent
   worktree is usually on its own `worktree-agent-<hash>` branch, NOT on the branch the session-start
   `gitStatus` reminder names — that reminder describes the SESSION ROOT checkout, which is a
   different worktree on a different branch. Do not assume they match.
2. `git ls-tree -r --name-only <expected-branch> -- <path>` to confirm the artifacts exist somewhere.
3. `git merge-base --is-ancestor HEAD <that-branch>` to prove a fast-forward is possible.
4. `git merge --ff-only <that-branch>`. This creates, renames, switches and deletes no branch and
   rewrites no history, so it does not violate a "do not touch any branch" constraint. Record the
   decision and its rationale in the checkpoint.
5. Then confirm the predecessor branch holds nothing extra: `git log <new-head>..<predecessor-branch>`
   must be empty. If it is not, the fast-forward did not capture everything and you must not proceed
   as if it had.

Also note `git worktree list` is the cheap way to see every sibling's branch and commit at once, and
it reveals whether the integration branch is checked out elsewhere (it usually is, in the session
root), which is why you commit on your own branch and let the parent fan it in.

See [[preparation-child-cwd-is-session-root-not-item-worktree]] and
[[stale-base-anchor-passes-ancestry-vacuously]] — the ancestry check here is the benign direction of
the same mechanism that makes a stale base anchor dangerous.
