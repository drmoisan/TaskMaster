---
name: amend-ac-text-when-a-measurement-rule-is-ratified
description: When I ratify a planner's measurement exemption for an AC, amend the spec AC text to state it verbatim before preflight clears; ratification alone does not satisfy an AC worded otherwise
metadata:
  type: feedback
---

When a planner introduces a measurement rule that relaxes how an AC is checked (e.g. #956 PD-7: subtract one closure-filter artifact line from the AC15 uncovered-line delta), my ratification in the checkpoint is not enough. The executor's preflight flagged it as a defect because acceptance-criteria-tracking rule 4 forbids checking off an AC whose text says otherwise.

**Why:** #956 round 1 (2026-10-01). Fixed by a one-line in-place amendment to spec.md AC15 (same line number, so other line citations stayed valid), plus a P0-T2 literal check that the amended text is present. Round 2 then cleared.

**How to apply:** when ratifying any planner-proposed exemption or adjusted gate, edit the spec AC line yourself as a single-line replacement, verify with `git diff --numstat` that it is 1/1, record it under `orchestrator_spec_amendments`, and tell the planner to make the plan's AC restatements consistent. Related: [[preflight-catches-vacuous-gates]].
