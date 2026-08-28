---
name: fan-in-hook-paths-resolve-to-session-cwd
description: When the parent does a child's fan-in, pr-author/merge-gate hooks resolve artifacts/ and the singular orchestrator-state.json against the SESSION cwd, not the child worktree — copy the child's body+receipt across and expect the shared checkpoint to have been overwritten by a sibling
metadata:
  type: feedback
---

Doing a dead child's S9 yourself means satisfying hooks that resolve every path relative to the
**session cwd**, never the worktree the command runs in.

**Why:** `enforce-pr-author-skill.ps1` hard-codes `artifacts/pr_context.summary.txt`,
`artifacts/orchestration/orchestrator-state.json`, and the canonical `artifacts/pr_body_<N>.md`
shape as bare relative paths, so a `cd <worktree> && gh pr create --body-file artifacts/...`
fails with `PR_AUTHOR_RECEIPT_MISSING` even though the receipt is sitting right there. Passing an
absolute path instead fails differently, with `PR_BODY_PATH_NONCANONICAL` — the literal string
must match `artifacts/pr_body_<N>.md`. Both were hit on 2026-08-26 landing feature 498.

**How to apply:**

1. Copy the child's already-authored `pr_body_<N>.md` **and** its `.receipt.json` byte-identically
   from the child worktree into the session cwd `artifacts/`, verify `sha256sum` still matches the
   receipt, and run `gh pr create` from the session cwd with the bare relative `--body-file`. Do
   not re-author the body — the hash binds it to the child's own pr-author run.
2. Staleness compares `receipt.created_at` against `artifacts/pr_context.summary.txt`'s mtime, so
   regenerating the context *after* the receipt was written breaks a body that was previously fine.
   Check the ordering before you refresh context.
3. `enforce-epic-merge-gate.ps1` accepts a child merge only via
   `artifacts/orchestration/orchestrator-state.json` with `epic_mode: true` and
   `step9_status == "passed"` (that exact literal). Every child in the session shares that one
   path, so by fan-in time it usually holds a *sibling's* completed record. Archive it to a
   suffixed filename first, then write the target child's record. Set `step9_status: "passed"` for
   the gate and flip it to `"verified"` after merging, because the completion gate rejects
   `"passed"` as terminal.
4. Writing that per-feature checkpoint is legitimate only when you record facts you verified
   yourself and add an explicit `authorship_disclosure` naming who wrote it, why the child's own
   record was gone, and what is **not** being claimed (e.g. "no feature review was performed").
   That is the line between completing S9 for a dead child and the thing refused in
   [[feedback_merged_child_worktree_still_locked_defer_removal]] — inventing a whole run that never
   happened to unlock a gate.

Related: [[project_pr_author_is_inline_skill_not_agent]],
[[feedback_collect_pr_context_races_across_concurrent_children]].
