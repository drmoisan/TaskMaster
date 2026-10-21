# Phase 0 — Base Commit for Change-Footprint Measurement

Timestamp: 2026-09-14T08-12

Command: `git merge-base HEAD origin/main`

Execution note: run against this worktree. The Bash invocation supplied the worktree root through an explicit `-C` operand, which is equivalent to running the command from the worktree root.

EXIT_CODE: 0

Output Summary: the merge base of HEAD and the remote-tracking ref `origin/main` is commit `e4a337505af5ce0c53641d3a89343f20c1e2c6c1`. This 40-character hash is the BASE_COMMIT value that P2-T8 substitutes into its two-dot diff. The remote-tracking ref, not the local `main` branch, was used as the second operand, because the local `main` in this clone is stale relative to the `origin/main` commit this branch already merged; anchoring on the stale local ref would enumerate an entire upstream advance belonging to other items.
