---
name: preflight-conjunctive-criterion-citation-gap
description: A "one evidence artifact per check-off task" plan rule silently breaks every AC whose text is a conjunction of facts produced by different command steps; sweep for the second conjunct
metadata:
  type: project
---

When a plan declares "each check-off task flips exactly one checkbox and cites exactly ONE evidence
artifact", every acceptance criterion whose text is a **conjunction of facts produced by different
command steps** becomes uncheckable-with-evidence. The rule reads as tidy discipline and is actually
the defect generator.

**Why:** on #488 round 3 a systematic sweep of all 54 check-off tasks found 8 defects, every one of
this shape, after rounds 1 and 2 (13 then 3 defects) had missed them. Round 2 had found exactly one
instance ([P9-T9]) and treated it as a one-off rather than as a class.

**How to apply:** for each check-off task, read the criterion and split it on `and` / `, and` /
`additionally` / a second sentence. Then ask of the ONE cited artifact: can it contain the second
conjunct, given the producing task's own acceptance list? Recurring second conjuncts that no
artifact carries:

- **"passes unmodified" / "is byte-identical"** — a `Passed` outcome does not establish it (a red
  test can be edited into passing) and a line count does not establish it (a file can be edited
  without changing its line count). Only `git diff --name-only <BASE> -- <file>` returning empty does.
- **"anywhere by this feature"** — scans scoped to the owned PRODUCTION files miss the owned TEST
  files, which are equally part of the feature.
- **"the spec, the change description, AND the test's own documentation each state X"** — a
  change-description artifact carries one third of it.
- **"does not lower EITHER figure"** — needs a baseline for BOTH denominators; plans routinely
  capture only the raw baseline and then compare a testable-denominator figure against nothing.
- **"pass/fail counts AND the pre-change coverage figures"** — two Phase 0 command steps, so the
  test-run artifact cannot carry the coverage half.
- **"a new issue is opened"** vs the plan delivering a `docs/features/potential/` entry. Check
  whether a SIBLING criterion says "a potential entry or GitHub issue" — if one criterion offers the
  choice and another names an issue specifically, the narrower one governs its own branch.

**Preferred fix, in order:** widen the PRODUCING task's acceptance so the single cited artifact
carries both conjuncts. Only when the conjuncts come from genuinely different command steps, and no
single artifact could hold both, authorize a named two-artifact citation and enumerate it
exhaustively in the decision that states the one-artifact rule.

Also watch for the mirror defect: **two criteria that constrain the same code position
incompatibly**. #488 had one criterion requiring a guard be the "first statement" of a member and
another requiring a throw be its "first action" in the same member. Neither the plan nor either
check-off task reconciled them, so both were flipped against a source that cannot literally satisfy
both. The fix is a decision entry stating the reading ("statement" vs "action" — a precondition
check that returns without effect has performed no action), not a source change.

Related: [[project_418_plan_rationale_clauses_are_evidence]],
[[project_preflight_ac_checkoff_and_tooloutput_paths]],
[[feedback_confirmatory_preflight_proportionate_bar]].
