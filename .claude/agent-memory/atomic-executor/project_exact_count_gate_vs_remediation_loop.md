---
name: exact-count-gate-vs-remediation-loop
description: A plan that pins an exact test TotalCount/PassedCount and also has a remediation path that adds tests is self-contradictory; check every add-tests-and-restart clause against every downstream count gate.
metadata:
  type: project
---

When a plan hard-pins a test gate to an exact figure (`PassedCount` = `TotalCount` = **19**) and
elsewhere carries a remediation path that says "add further unit tests, then restart the QA loop
from P#-T#", the two clauses contradict each other the moment the remediation fires: the restarted
gate can only be satisfied by recording a false number.

**Why:** Exact counts are added to close the zero-discovery hole (`FailedCount = 0` is also true of
a run that discovered nothing — see [[project-poshqc-pester-mcp-exit-minus1]]). Remediation paths
are added to close a coverage-shortfall hole. Both fixes are correct in isolation; the collision is
only visible when you read them together. Found on the #441 Cobertura plan at preflight iteration 3,
where the fix for SF-6 (coverage remediation) collided with the fix for SF-3 (TotalCount pinning)
introduced in the same round.

**How to apply:** The safe formulation is `PassedCount` = `TotalCount` = **B + N**, where B is the
baseline figure and N is the number of tests added under the named remediation path (N = 0 on the
first pass), with the in-force N recorded in the artifact, plus "a `TotalCount` below B fails this
gate". That keeps the zero-discovery guard intact without forbidding the remediation. Whenever a
preflight round adds a remediation-restart clause, re-scan every downstream numeric acceptance the
restart replays — the executor cannot reconcile a numeric mismatch at runtime without replanning.

Related: [[project-preflight-selfderived-gate-thresholds-are-blind]],
[[project-418-plan-rationale-clauses-are-evidence]].
