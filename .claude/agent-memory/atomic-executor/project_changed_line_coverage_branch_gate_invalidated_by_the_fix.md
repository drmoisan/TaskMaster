---
name: changed-line-coverage-branch-gate-invalidated-by-the-fix
description: A plan gate that keys its expected changed-line coverage off the BASELINE measurement is unsatisfiable when the fix itself makes the line analyzable for the first time.
metadata:
  type: project
---

A two-branch acceptance gate of the form "if the baseline recorded the changed line as COVERED, assert COVERED; if it recorded NOT REPORTED, assert NOT REPORTED; any other combination is a stop-and-report" can be driven into its own stop-and-report clause by a correct fix.

**Why:** Pester's JaCoCo writer emits a `<line nr="N">` counter only for lines that carry an *analyzable command*. In issue #752 the pre-fix line 301 held only an operand of an `-and` chain begun three lines earlier, so the baseline XML had a `sourcefile` node for the file and no node at all for line 301 (`LINE301 NODE COUNT=0`). The fix replaced that operand with a `[System.IO.Path]::GetRelativePath(...)` call, which *is* an analyzable command: analyzed commands went 802 -> 803, a line-301 node appeared, and it was covered. Baseline `false`, post-change `COVERED` — exactly the combination the plan classified as a stop-and-report, reached by the change working as designed.

**How to apply:** When a plan's changed-line coverage branch keys off a baseline non-observation, expect the third combination and do not force the measurement into a branch. Writing `NOT REPORTED` would have required also writing the branch's mandated note asserting that no per-line counter exists — false against the post-change XML. Record the measurement truthfully, add a clearly-labelled divergence section giving the analyzed-command delta as the mechanism, leave that task's checkbox unchecked, and escalate at completion; the blocking condition (post-change >= baseline) is what actually matters and it held. Related: [[project_coverage_delta_reproduce_baseline_counting_method]], [[feedback_never_predict_an_observation_into_an_artifact]].

At authoring time, the fix is to make such a gate assert `post-change >= baseline` on the file percentage plus "the changed line is covered OR carried no counter at baseline", rather than demanding parity with the baseline's reporting state.
