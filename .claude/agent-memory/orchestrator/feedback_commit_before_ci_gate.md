---
name: feedback-commit-before-ci-gate
description: Make the LAST branch commit before running the S9 CI gate; any post-gate commit (even docs/memory) moves the head SHA and forces an S9 re-run
metadata:
  type: feedback
---

Stage and commit ALL branch-bound changes before running the S9 CI green gate. Do not push a new commit to the PR branch after S9 has recorded `ci_gate` against a head SHA.

**Why:** the PR Creation Gate condition 6 requires `ci_gate.head_sha == current head SHA of the PR branch`. In the #254 / PR #258 run I recorded a green `ci_gate` against head `7427b2dc`, then committed orchestrator agent-memory notes to satisfy the "clean worktree / commit all evidence" rule ([[feedback_commit_all_evidence_clean_worktree]]). That push advanced the head to `8e32da03`, invalidating the recorded gate and forcing a full S9 re-watch on the new head before DONE. Even a docs/markdown-only commit triggers the CI run and breaks the SHA match.

**How to apply:** sequence the tail of the workflow as (1) finish ALL commits — code, evidence, audit artifacts, and any memory-note updates — and push; (2) THEN resolve the live head SHA and run S9 once against it; (3) do not commit again. If a late change is unavoidable after S9, treat it as a new head: re-run S9 and update `ci_gate.head_sha`/`last_verified_ci_sha` before writing DONE. `artifacts/orchestration/orchestrator-state.json` and `artifacts/pr_body_*` are git-ignored, so updating the checkpoint itself does not move the head — only tracked-file commits do.
