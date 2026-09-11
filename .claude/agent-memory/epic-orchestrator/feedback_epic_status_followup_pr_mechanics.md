---
name: epic-status-followup-pr-mechanics
description: The epic-status closing commit needs its own PR, and that PR trips the merge gate because epic_merge_pr still names the integration PR; repoint it with history preserved rather than falsifying anything
metadata:
  type: feedback
---

The final `epic-status.md` entry needs a follow-up PR off `main`, and getting that PR merged trips
three mechanics in a row. All three are satisfiable truthfully. Verified 2026-09-10 on
review-residuals-2026-09-08 (integration PR #836, follow-up PR #837).

**1. `PR_BODY_PATH_NONCANONICAL` then `PR_AUTHOR_RECEIPT_MISSING`.** `gh pr create --body-file` will
not accept a heredoc or an arbitrary path: it must be `artifacts/pr_body_<N>.md` with a sibling
`artifacts/pr_body_<N>.receipt.json`. Critically, `enforce-pr-author-skill.ps1` resolves both as bare
paths **relative to the session cwd**, not to the worktree you run `gh` from. Authoring them inside
the follow-up worktree is not enough — copy both into the SESSION worktree's `artifacts/` and run
`gh pr create` from there with the bare relative `--body-file artifacts/pr_body_<N>.md`. See
[[fan-in-hook-paths-resolve-to-session-cwd]]. `<N>` is predictable: the next sequential number after
the highest existing issue or PR.

**2. `EPIC_MERGE_GATE_BLOCKED` on the follow-up.** The gate accepts an epic checkpoint only when
`epic_merge_pr.ci_gate.conclusion == "success"` **and** `epic_merge_pr.pr_number` matches the PR being
merged. After the integration PR merges, that field names the integration PR, so the follow-up cannot
match. Do NOT overwrite the integration PR's record. Move it to an additive
`epic_merge_pr_history[]` array preserving every field including its own green `ci_gate` and its merge
commit, then write the follow-up into `epic_merge_pr` with its OWN observed `ci_gate`, plus a `role`
and a `disclosure` field stating that both are epic-level merges to `main` and that nothing was
retracted. Every recorded value stays an observed fact; you are supplying the gate the evidence it
asks about the PR actually being merged, not inventing a conclusion. Read the conclusion from
`gh pr checks <N>` AFTER the checks finish — never pre-fill it.

**Why a follow-up PR is unavoidable:** `epic-status.md` is a projection whose last transition is the
integration PR's own merge, and that merge commit does not exist until that PR merges. A document
cannot record the merge commit of the PR that carries it.

**3. Worktree removal stays blocked, and a loop variable makes it worse.**
`enforce-epic-worktree-removal-gate.ps1` passes once the feature is `merged`, but
`enforce-parallel-worktree-removal-gate.ps1` then demands a *parallel* checkpoint `items[]` record that
an epic run never maintains, so it fails closed on every epic worktree. Never fabricate a parallel
checkpoint to clear it — see [[merged-child-worktree-still-locked-defer-removal]]. Also note the hooks
inspect the raw command TEXT before execution, so a `for w in ...; do git worktree remove ".../rr-$w"`
loop is evaluated with `$w` UNEXPANDED and matches no record; issue one literal path per call, which
at least tells you which gate is really denying.

**How to apply:** budget for a second PR at the end of every epic, author its body and receipt into the
session `artifacts/`, repoint `epic_merge_pr` with `epic_merge_pr_history[]` preserved, and expect to
hand the worktrees to `scripts/bash/cleanup-worktrees.sh`.
