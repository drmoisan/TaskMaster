---
name: removing-a-halt-requires-branch-propagation
description: Converting a plan's mid-plan HALT into a recorded-blocker continuation strands every downstream task whose acceptance still demands the blocked artifact; relay the delta as "propagate to all consumers", not just "remove the halt"
metadata:
  type: feedback
---

When a preflight delta removes a mid-plan `halt` and replaces it with a
record-the-blocker-and-continue branch, the edit is only half the fix. Every
downstream task that was previously *unreachable* on that branch becomes
reachable, and any of them whose acceptance criteria demand the artifact the
blocked step could not produce (typically a created GitHub issue number) is now
unsatisfiable.

**Why:** On #494 preparation, delta D-5 converted `P5-T1` from a halt on
`potential_to_issue` availability into a recorded-blocker continuation. The
planner applied it to `P5-T1` and `P5-T16` only. Preflight iteration 2 found
`P5-T2`, `P5-T3`, `P5-T4`, `P5-T17` and `P6-T7` newly reachable with acceptance
text still requiring a created issue number — trading a clean stop at task 79 of
97 for five silent failures at tasks 80-97. It took a whole extra
planner+preflight round trip (six R-deltas) to close.

**How to apply:** When relaying a halt-removal delta to `atomic-planner`, state
the delta as "remove the halt AND propagate the branch to every downstream
consumer", and enumerate the consumers yourself if you can. Then, in the next
preflight prompt, make branch-completeness the explicit substantive check:
"walk every task reachable under <BLOCKER STATE> and name any lacking a
satisfiable outcome". Also require that each added branch stay non-vacuous — a
branch permitting `remediation-required` must still name the artifact content
proving that outcome, or the fix for an unsatisfiable gate produces a gate that
verifies nothing.

Related: [[preflight-catches-vacuous-gates]],
[[mcp-tools-available-to-orchestrator]] (the orchestrator session often *does*
expose the promotion MCP surface the executor subagent lacks, so the right
branch is "defer to the orchestrator", never "halt" and never "file by another
route").
