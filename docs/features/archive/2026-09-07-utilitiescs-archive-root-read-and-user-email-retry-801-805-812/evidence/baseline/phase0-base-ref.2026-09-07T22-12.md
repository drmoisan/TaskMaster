# Phase 0 — Diff Base Reference (P0-T7)

Timestamp: 2026-09-08T06-38

Command: `git rev-parse --verify origin/main`

EXIT_CODE: 0

Command: `git merge-base HEAD origin/main`

EXIT_CODE: 0

Output Summary:

- `git rev-parse --verify origin/main` printed `f63a2c4bc396ed43af2354a07891b8ef0bb205ed` and exited 0.
- `git merge-base HEAD origin/main` printed `f63a2c4bc396ed43af2354a07891b8ef0bb205ed` and exited 0.

The two values agree, so `origin/main` is an ancestor of `HEAD` at the time this observation was taken and `origin/main...HEAD` resolves its merge base to that SHA, yielding this branch's own changes only.

Per D8 this plan uses two ref operands and selects between them by whether the change being observed has already been committed: `HEAD` for a task that observes an edit not yet committed, and `origin/main...HEAD` for a task that observes a change already committed by P5-T1 or P5-T11. Neither form is unanchored.

This merge-base SHA is recorded as an observation only. It is deliberately not carried into any later task as an expected value.
