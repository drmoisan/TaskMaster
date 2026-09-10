---
name: isolated-worktrees-cut-from-main-not-session-head
description: Agent(isolation:worktree) cuts the child worktree from the primary checkout's HEAD (main), NOT from the epic-planner session worktree's integration branch — so a child told its folder is "already committed" may find it absent; tell every child to verify and fast-forward
metadata:
  type: reference
---

Measured 2026-09-09 on the review-residuals-2026-09-08 resume. Eight children were launched in one
message from a session worktree checked out on `epic/review-residuals-2026-09-08-integration` at
`d9eade59`. Their worktree bases were **inconsistent**: some were cut at `d9eade59`, others at
`6f08302a`, which was `main` and the tip of the primary checkout `C:/Users/DanMoisan/repos/TaskMaster`.

So `isolation: "worktree"` resolves its base from the repository's primary checkout, not from the
calling agent's session worktree, and the result can race a merge the caller just made. The
`epic-plan` skill's phrasing "branching each worktree from `origin/epic/<slug>-integration`" is an
intent, not a guarantee the harness provides.

**Consequence.** A delegation prompt that says "your feature folder is already present because its
predecessor committed it and I merged it" can be false in the child's worktree, and a child that
believes it will either report BLOCKED or, worse, regenerate the document.

**How to apply.** Put this in every child prompt:

> Your worktree may have been cut from `main` rather than from the integration branch. Before
> Step 0, run `git log --oneline -1` and `git merge-base --is-ancestor <integration-sha> HEAD`. If
> HEAD does not contain `<integration-sha>`, confirm HEAD is a strict ancestor of it and
> fast-forward to `<integration-sha>`; do not create, rename or delete a branch, and report what
> you did.

Name the exact integration SHA in the prompt. One child on this run did the whole recovery
unprompted and reported it; do not rely on that.

At fan-in a stale-base child is still safe to merge — files added on the integration side after the
merge base are added-on-one-side-only and are preserved — but verify it anyway, because
[[verify-branch-family-additions-only-vs-main]] describes the shape where it is not.

Related: [[child-hooks-fail-closed-on-session-cwd]],
[[recover-dead-prep-child-by-committing-then-relaunching]].
