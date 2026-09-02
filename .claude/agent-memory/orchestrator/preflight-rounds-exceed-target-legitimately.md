---
name: preflight-rounds-exceed-target-legitimately
description: Four preflight rounds on a one-file change was not round inflation — diagnose the cause before treating an over-target round count as a process failure
metadata:
  type: project
---

Issue #648 was a single-test-file change and still took 4 preflight rounds against the
`atomic-plan-contract` target of 2. Defect counts per round: 13, 10, 1, 0 — monotonic convergence, every
pass exhaustive. So it was NOT the failure mode the two-round target exists to prevent (single-defect
reporting, where each round rediscovers what the same pass could have reported).

Three causes actually accounted for it, and they are worth separating because only one is a process defect:

1. **Orchestrator-seeded.** My delegation prompt reported an unqualified "repository-wide" literal count
   that I had in fact derived over `*.cs` only (5 vs 19 across all tracked files). That produced a round-1
   defect directly. Qualify the SCOPE of every count handed to a planner.
2. **Bootstrap surface, not change surface.** Most round-2 findings were conditions over the C# Phase 0
   bootstrap (SDK install, NuGet restore, coverage collector, msbuild resolution) that an agent worktree
   lacks — not over the one-line change. A one-file C# change in a fresh worktree carries a large gate
   surface regardless of how small the diff is. Budget rounds against the GATE surface, not the diff size.
3. **Wrong corrections consumed two rounds.** Round 1 gave line citations off by one to two lines, and the
   planner asserted a correction that was itself false (see
   [[subagent-self-reported-correction-can-be-false]]).

**What worked:** requiring each reviewer to BOUND ITS OWN EDIT SET with `git diff -U0 <prev> <cur>` before
reviewing. Round 4 did this (4 hunks, 6 insertions, 5 deletions), which let it confine the pass to the
changed surface plus siblings and clear in one narrow pass instead of re-reviewing 49 tasks.

**How to apply:** when a round count runs over, state the diagnosis rather than either excusing it or
recording it as generic process failure — the three causes have different remedies. And on any confirming
round, hand the reviewer the exact prior commit SHA so it can diff rather than re-read.
