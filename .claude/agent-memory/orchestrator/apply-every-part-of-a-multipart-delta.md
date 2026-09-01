---
name: apply-every-part-of-a-multipart-delta
description: A preflight defect often names 2-3 edits across task text, sibling task, and reading-guide prose; applying only the task edits leaves the document self-contradictory and costs a whole extra round
metadata:
  type: feedback
---

When applying a preflight delta, apply EVERY sub-edit the defect names, then
sweep the document for other places that state the same rule.

**Why:** on issue #663 (2026-09-01) I did this wrong twice in one round, and
round 2 spent two of its five findings on my errors rather than on the plan.

- Round 1's D3 named three edits: `[P0-T13]`, `[P4-T6]`, and a reading-guide
  paragraph. I applied the two task edits and missed the paragraph, so the guide
  still described one baseline failure set for every later run while `[P4-T6]`
  compared against a second one. An executor reading the guide as the general
  rule would have gated the instrumented run on the uninstrumented baseline.
- Round 1's D2 scoped a `git diff` to `-- '*.cs'` in the PLAN, but the matching
  spec acceptance criterion carried the same unscoped command. Left unscoped it
  reported twenty prose matches and could never pass.

Both are the same shape: a rule stated in more than one place, corrected in one.

**How to apply:** after applying a delta, grep for the identifiers and command
forms it touched and check every hit against the run or task that consumes it.
For issue #663 that meant enumerating all eight `BASELINE_*_FAILURE_SET`
references and confirming each uninstrumented run reads the uninstrumented set,
and sweeping the spec for the unscoped diff form. Do the sweep BEFORE launching
the confirming round; a round spent rediscovering your own partial application
teaches nothing about the plan. A delta that also propagates into `spec.md` is
common whenever the spec's verification column quotes a command, because the
plan and the spec then hold two copies of one assertion.
Related: [[preflight-catches-vacuous-gates]],
[[multi-location-fact-residuals-drive-preflight-rounds]].
