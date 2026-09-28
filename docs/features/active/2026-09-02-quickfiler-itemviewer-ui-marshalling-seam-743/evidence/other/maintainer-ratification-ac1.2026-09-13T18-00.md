# Maintainer ratification of the AC1 negative result (issue 743)

Timestamp: 2026-09-13T18-00
Command: none (this artifact records a human decision, not a tool run)
EXIT_CODE: 0
Output Summary: THE MAINTAINER ratified the AC1 recorded negative result on 2026-09-13, subject to four conditions, all four of which are discharged and evidenced below. The ratification accepts the item DESPITE a negative result. It is not a finding that the result was positive. AC1's checkbox in `spec.md` remains `- [ ]` and was verified unchecked before, during and after this record was written.

## Attribution

- **Decision made by: THE MAINTAINER.** Date of ruling: 2026-09-13.
- Not made by the parallel-run coordinator, and not made by any agent. The coordinator relayed the ruling; the orchestrator transcribed it. Neither originated it, and neither is empowered to.
- Review finding under which the decision was requested: **R-2**, raised in `feature-audit.2026-09-13T15-30.md` and `code-review.2026-09-13T15-30.md`, and scoped for remediation as F-2 and F-4 in `remediation/2026-09-13T15-45/remediation-inputs.md`.

## What was ratified, stated so it cannot be misread in either direction

AC1 required that the mechanism be identified by measurement rather than inference, and its final sentence provides: "If the instrumented run produces no expiry at all, that is a recorded negative result, not a pass."

The instrumented runs produced no expiry. Both the SERIAL and the PARALLEL regime recorded `timeout=0`. AC1's own terms therefore classify the outcome as a recorded negative result, and no agent action can convert it into a pass.

The maintainer has ratified that negative result: the item is accepted for merge notwithstanding that AC1 was not satisfied. Two statements are therefore true simultaneously, and both belong in the record:

1. **The measurement produced a negative result.** AC1 is not satisfied. Its checkbox is `- [ ]`.
2. **The maintainer accepts the item anyway.** The ratification is recorded here.

These are different statements. The checkbox reflects what was measured; the ratification reflects the maintainer's acceptance. Marking the checkbox `- [x]` would destroy exactly the distinction the ratification rests on, by asserting that the measurement succeeded when it did not. The checkbox is deliberately left unchecked and must stay unchecked.

## The four conditions and their discharge

### Condition 1 — AC1 stays unchecked

**Discharged.** `spec.md` line 413 reads `- [ ] **AC1 — Mechanism identified by measurement, not inference.**` It was verified to be `- [ ]` before this artifact was written and was not modified. The other four criteria remain `- [x]`. The stale `evidence/other/acceptance-status.2026-09-12T19-30.md` row that still read `PASS` for AC1 — a residual of commit 9170499b4, which unchecked the box in `spec.md` without updating that artifact — has been corrected to `RATIFIED NEGATIVE RESULT — NOT A PASS`, with the superseded summary retained.

### Condition 2 — the verdict wording is corrected, with both prior errors visible

**Discharged** in `evidence/baseline/ac1-mechanism-verdict.2026-09-12T17-00.md`. That artifact has now been wrong twice and both superseded texts are retained verbatim under a `## Correction history` heading so a reader can see both corrections:

- The original Output Summary claimed H-LEAK "is REJECTED by direct observation OF THE PRE-DECLARED COUNTER OBSERVABLE".
- Correction 1, applied earlier on 2026-09-13, identified the right defect and then defended the wrong claim, asserting that the rejection "rests entirely on the counter observable, which is a legitimate basis".

Both claims are withdrawn. The accurate statement now recorded is that the instrumented runs recorded `timeout=0`, no expiry occurred, and therefore **neither H-COST nor H-LEAK was discriminated**: there was no expiry event in which to observe whether the one-permit `TransactionGate` was held with no live holder. **A hypothesis cannot be rejected by the absence of observations.**

The mechanism of the error is recorded in the artifact and is worth restating. Section 4.2 of `spec.md` defines H-LEAK as a cascade conditional on a prior expiry. With `timeout=0`, no test was abandoned, so no leak could have occurred under *either* hypothesis. The pre-declared observable therefore read `contended=0` for a reason wholly independent of whether H-LEAK is true of this codebase. The reading was predetermined, and a predetermined reading carries no information about the hypothesis it was declared to test. The observable was declared honestly and in advance; what failed is that the run never entered the regime in which it becomes discriminating.

Section (iii) of the verdict artifact is marked SUPERSEDED rather than deleted, and the escalation paragraph has been updated from "escalated and unresolved" to record that the maintainer has now ruled.

### Condition 3 — the AC3(b) statistical caveat is complete

**Discharged.** State before this run, reported precisely because the two artifacts differed:

- `evidence/regression-testing/ac3b-consecutive-runs.2026-09-12T19-00.md`, which is the artifact spec AC3(b) governs and whose content AC3's own text mandates: **PRESENT and complete.** Its line 25 already carried both the single-failure provenance and the interval.
- `evidence/other/orchestrator-run-count-derivation.2026-09-12T14-15.md`, the preparation-mode derivation: **PARTIAL.** Its Sensitivity note carried the qualitative wide-uncertainty language but neither the single-failure provenance nor the interval.

The derivation artifact has been completed. It now states that the 4.8 percent base rate is a point estimate from a SINGLE observed failure, gives the exact (Clopper-Pearson) 95 percent interval of approximately **[0.0012, 0.2382]**, and states the consequence plainly.

Both endpoints were re-derived here rather than copied from the request:

- Lower, solving `1 - (1 - p)^21 = 0.025`: `(1 - p) = 0.975^(1/21) = exp(-0.0253178 / 21) = 0.9987951`, so `p = 0.0012049`.
- Upper, solving `(1-p)^21 + 21p(1-p)^20 = 0.025`: at `p = 0.2382`, `(0.7618)^21 = 0.0032993` and `21 x 0.2382 x 0.004330 = 0.021660`, summing to `0.024959`.

Consequence: the interval spans a factor of roughly 200. If the true rate sits near the low end, the expected number of failures in 62 runs is about 0.07, so a clean 62-run streak is the overwhelmingly likely outcome even if the fix did nothing, and the streak establishes little. Reaching alpha 0.05 against the low endpoint would need approximately 2,500 runs. The record must not imply more confidence than the measurement supports: AC3's blocking weight rests on component (a), the deterministic single-run assertion, which has no base rate at all.

### Condition 4 — a separate issue is filed for H-LEAK before this item's pull request merges

**Discharged. The new issue is https://github.com/drmoisan/TaskMaster/issues/882**, filed 2026-09-13 through the MCP promotion lifecycle (`new_potential_bug_entry` then `potential_to_issue`, `full-bug` mode). No active feature folder was created for it, as directed.

Its scope is to determine whether QuickFiler's one-permit `TransactionGate` can leak or late-release a permit. It carries correction C2 of this item's own spec as its load-bearing evidence — verified against the spec before being asserted — namely that `TransactionGate` remains a `SemaphoreSlim(1,1)`, still awaited without timeout or cancellation token, still held from acquisition to disposal, so **issue 493 changed the OWNER of the serialization, not its SHAPE**. It states plainly that **H-LEAK was never excluded, only never observed**, and that this item's seam routes the affected tests around the question rather than answering it: if the mechanism was H-LEAK, that defect still exists and the seam has avoided it rather than fixed it.

Cross-references run both ways. Issue 882 names 743 (eleven occurrences in its body). This item names 882 here, and its pull-request body names 882.

Fidelity note: the bug-report issue template carries no `Suspected Cause / Notes`, `Proposed Fix / Validation Ideas` or `Next Step` section, so the promotion tool dropped those three sections from the promoted document. The dropped content — which includes the C2 quotation and the "do not treat a clean run as evidence of absence" trap warning — was verified missing from the issue body and reposted as a comment: https://github.com/drmoisan/TaskMaster/issues/882#issuecomment-5656393710. The remaining seven template sections mapped through with zero `(not provided in potential file)` placeholders.

## Standing of the four conditions

The conditions are not optional because the ratification was granted. The ratification is conditional on them, and condition 4 in particular is the condition it depends on: without issue 882, merging 743 would retire the symptom and lose the open question. All four are discharged above.

## Related records

- `evidence/baseline/ac1-mechanism-verdict.2026-09-12T17-00.md` — Correction 2.
- `evidence/other/acceptance-status.2026-09-12T19-30.md` — corrected AC1 row.
- `evidence/other/orchestrator-run-count-derivation.2026-09-12T14-15.md` — completed sensitivity note.
- `evidence/regression-testing/ac3b-consecutive-runs.2026-09-12T19-00.md` — the caveat as originally and correctly recorded.
- `remediation/2026-09-13T15-45/remediation-inputs.md` — F-2 and F-4, which scoped this work before the ruling.
