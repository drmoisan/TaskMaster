# Merge-Base Capture — Cycle 2, Issue #218

Timestamp: 2026-06-28T17-31

Command: `git merge-base HEAD main` ; `git rev-parse main` ; `git rev-parse HEAD`

EXIT_CODE: 0

Resolved merge-base SHA: `1b8536b6e5fb0778aba528caa39853590185bcb7`
`git rev-parse main`: `1b8536b6e5fb0778aba528caa39853590185bcb7`
`git rev-parse HEAD`: `2637e4c1f3b6eae983336f7ad4277f08acdee66c` (maintainer split commit)

Assertion: Merge-base equals the plan anchor `1b8536b6`. CONFIRMED — merge-base = main = `1b8536b6...`. The merge-base `1b8536b6` is the changed-line denominator source for the remainder of the plan.

Output Summary: Merge-base and main both resolve to `1b8536b6e5fb0778aba528caa39853590185bcb7`, matching the anchor. HEAD is at the maintainer split commit `2637e4c1`.
