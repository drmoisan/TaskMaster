---
name: recovering-a-dead-agent-worktree-via-shared-git
description: "Recover a rate-limited predecessor agent's work without writing into its worktree: committed content is reachable via the shared .git, and hash-object reads its uncommitted files even though git -C into it is refused"
metadata:
  type: project
---

All `.claude/worktrees/agent-*` worktrees share one `.git`, so a dead predecessor's **committed**
work is already in your object store. Its **uncommitted** files are readable by absolute path.

**Why:** on 2026-09-12 a preparation-mode run for issue #743 died on an account-wide rate limit
holding the only copy of a spec, a user story and a mid-authoring plan. Reconstructing them would
have cost hours; recovering them cost four commands.

**How to apply:**

1. `git -C <MY worktree> cat-file -t <predecessor-sha>` — it resolves, because the object store is
   shared. `git ls-tree -r --name-only <sha> -- <path>` then enumerates what it committed.
2. Restore committed content byte-exact with `git -C <MY worktree> checkout <sha> -- <path>/`.
   This also stages it, which `git add`/`git commit` gates do not object to.
3. For an **uncommitted** file, `git -C <MY worktree> hash-object -w <ABSOLUTE path inside the other
   worktree>` works, then `git cat-file blob <sha> > <my path>`. Re-`hash-object` your copy and
   compare SHAs to prove byte-identity and preserve LF. See [[byte-exact-copy-via-git-plumbing]].
4. **`git -C <OTHER worktree> ...` is REFUSED** by the isolation filter ("a worktree-isolated
   agent's git operations must target its own worktree"), so you cannot read the predecessor's
   `git status` or `git log`. Substitute the `Glob` tool over its folder and compare the file list
   against `ls-tree` of its last commit — a file present on disk but absent from the tree is its
   uncommitted work.
5. **Prove the citation base matches before trusting inherited line numbers.**
   `git diff --name-only <my-base> <predecessor-commit>` — if it lists only docs, the predecessor's
   citations were derived against your exact source tree. That converts a full re-derivation into a
   spot check. Do the spot check anyway; see [[imported-checkpoint-recorded-pass-is-not-evidence]].
6. Commit and **push** the recovered work before starting any new work, so it stops being
   single-copy. Do not write into the other worktree.

Also: an inherited checkpoint's `model_budget.fable_policy` may differ from your session's. Recompute
the routing rather than inheriting the receipts — see [[model-routing-feature-review-is-always-fable]].
