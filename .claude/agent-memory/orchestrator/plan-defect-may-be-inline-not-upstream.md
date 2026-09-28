---
name: plan-defect-may-be-inline-not-upstream
description: Before accepting that a bad command switch needs an unreachable atomic-planner revision, check whether the plan's own spans pass it INLINE; if so the fix is in your Write Set and needs no delegation, no hook and no maintainer ruling
metadata:
  type: feedback
---

When a plan's command span carries a defective switch, classify the exposure before concluding you
cannot fix it. There are two cases and they have opposite remedies:

- **Runner-mediated.** The switch is appended by a script the plan merely invokes, e.g.
  `scripts/vscode/Invoke-MSTestWithCoverage.ps1` line 76 appending
  `/Settings:<scripts/vscode/TaskMaster.cli.runsettings>` internally. That script is outside every
  item's Write Set, so the item genuinely cannot fix it and the repair belongs to a separate issue.
- **Direct.** The plan text itself passes the switch inline in its own command spans. Then the fix is
  an edit to your own plan file, inside your own Write Set: **no planner delegation, no hook traversal,
  no maintainer ruling.**

**Why:** on item 839 (2026-09-13) both I and the coordinator modelled every exposure as
runner-mediated and recorded the item as BLOCKED with `blocked_reason: delegation_launch_failed`,
because the `atomic-planner` revision we thought was required is denied `PRD_FEATURE_BLOCKED` on the
parallel surface (see [[prd-feature-hook-parses-prompt-paths.md]]). A sibling item disproved the model.
Item 839 was the direct case: its `CMD-TEST-SCOPED`, `CMD-TEST-FULL` and `CMD-COVERAGE` each spelled
`/Settings:...` inline, and its own Decision D5 already stated the runner's entry point was not used —
the script was dot-sourced only, for one helper function. The block was real but its stated cause was
false, and the item shipped after a three-span edit that touched nothing outside the feature folder.

**How to apply:** when a plan gate fails on a command-line defect, grep the plan for the literal switch
before reaching for a planner. If it appears in the plan's own spans, correct it in place in the same
plan file (the plan-path continuity contract forbids a new timestamped sibling), commit the correction
ALONE and separately from execution work, and then resume.

Two discipline points that cost real rounds elsewhere in the same run:

1. **Re-derive the span count; never apply a supplied list.** The coordinator gave me "three spans" and
   warned that a sibling had re-derived a count it was given and found a different number because one
   line carried two spans. Mine did total three, one occurrence per line, but only because I measured
   it. Search case-sensitively for the literal and confirm zero matches afterwards.
2. **Rewrite the prose the removal falsifies, in the same commit.** A citation line asserting
   "no logger element, so no TRX is produced by any run in this plan" was grounded in the file being
   passed; once it is not passed, that inference is unsupported even though the conclusion still holds
   for an independent reason. Re-ground it on the decision that actually carries it, and record the
   correction as a new numbered decision. Leaving a false decision standing is worse than the switch.

**The boundary to hold.** Authorisation to remove a switch is NOT authorisation to change an acceptance
condition, assertion, threshold or task ordering. Verify the switch's real effect first: on 839 the
runsettings' entire content was an MSTest parallelisation element with no test filter, no logger and no
data collector, so the test population, the assertions and the coverage instrumentation were all
untouched and no AC changed what it was verified by. Had the file carried a filter or a logger, the
population or the artifact set would have moved and the right move would have been to stop and report.
One more check worth making: whether the affected baseline has already been captured. On 839 it had
not, so both sides of every before-and-after gate were measured under the one corrected method and no
method skew was introduced.

See [[repo-coverage-runner-parallelism-poisons-deedle]] for the underlying repository defect and
[[blocked-reason-enum-cannot-express-substantive-halt]] for recording a halt whose real cause the enum
cannot express.
