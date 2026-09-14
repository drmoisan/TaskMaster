---
name: preflight-cleared-plan-can-still-conflict-with-a-spec-acceptance-clause
description: A two-round preflight-cleared plan still carried a verdict task whose decision rule contradicted the spec's own AC clause (no-expiry = negative result, not a pass); the reviewer correctly ruled the spec authoritative, so read each AC's disqualifying sentence against the plan's verdict tasks BEFORE execution
metadata:
  type: feedback
---

Before launching execution, read every acceptance criterion's DISQUALIFYING sentences (the "FAILS
if", "is not a pass", "escalate rather than infer" clauses) against the plan task that will claim
that criterion, and ask whether the task's own decision rule can ever satisfy the clause. Preflight
does not do this: it checks that tasks are executable and falsifiable, not that a task's verdict
logic is consistent with the spec it will be judged against.

**Why:** issue #743 (parallel run bugs-2026-09-11, 2026-09-13). The plan's P1-T9/P1-T11 decision
rule said "serial contended count 0 and balance test passed -> H-LEAK REJECTED by direct
observation". It cleared preflight in two rounds and executed exactly as written. The reviewer then
ruled AC1 PARTIAL: spec AC1's final sentence says a run with no expiry "is a recorded negative
result, not a pass", and H-LEAK is by the spec's own definition a cascade conditional on an expiry,
so a zero-expiry run could only ever produce contended=0 and the observation discriminated nothing.
The plan author had treated the negative result as something to STATE in the artifact; the spec
treats it as something that BARS the pass. The spec is authoritative over the plan, and the only
remedy is a maintainer ratification of the negative result, which is a human gate no agent can
close. Cost: a full remediation cycle plus a blocked item, after 64/64 tasks and a clean toolchain.

**How to apply:** at the plan-approval step, for each AC build a two-column check: the AC's
disqualifying clause on the left, the plan's claiming task and its decision rule on the right. If a
plausible execution outcome (here: the defect does not reproduce on an idle machine) makes the rule
emit PASS while the clause says not-a-pass, send the plan back with that exact pair as the delta,
or get the spec amended before execution. Do this yourself as orchestrator; neither the planner's
self-review (citation-to-tree) nor the executor's preflight (executability) covers spec-vs-plan
verdict consistency, and the reviewer only sees it after the cost is sunk.

Related: [[preflight-catches-what-the-plan-validator-cannot]] (what preflight does cover),
[[absence-from-failure-list-is-not-a-pass-gate]] (same family: an observation that cannot fail is
not evidence).
