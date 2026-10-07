---
name: restart-loop-reenters-precommit-census-and-closed-classification-lists
description: Preflight checks that late rounds missed on #968 r4/r5 - a restart rule that loops back to a pre-commit census task, a closed classification list missing a census line, and a rewrite exemption keyed on the current pass only
metadata:
  type: project
---

Two preflight defect classes that survived three rounds on #968 (found in round 4, 2026-10-03):

1. **Restart loop re-enters a pre-commit census task.** D-13 sent a Phase 8 failure back to P6-T1
   (format, census, build, runs, commit). The P6-T2 census sits before the P6-T9 commit and gated
   `git diff --numstat HEAD` rows and an exact porcelain set with `??` lines. On the restart path the
   commit is already in HEAD, so the HEAD-anchored rows are empty and the new files are tracked:
   the gate cannot pass. Fix shape: on that restart, anchor the task's diffs at BASE and replace the
   exact porcelain set with a containment rule.
2. **Closed classification list vs census output.** A task that says "classify every PRIMARY line as
   A, B, ... or INVOCATION (stop)" must have a category for every printed line. Round 3 caught the
   method-group lines; round 4 caught the `#region` / `#endregion` lines of the search pattern's own
   region-name alternative. Run the census and map every line to a named category before clearing.

3. **Exemption keyed on the current pass only (round 5).** P6-T2 let LINES/SPAN values differ from
   their pre-loop (Phase 1-5) records "only if `REWRITTEN:` named the file", and `REWRITTEN:` is the
   current P6-T1 pass's hash diff. On any second pass, a file the first pass reformatted is already
   formatted, so this pass does not name it, yet its values still differ from the pre-loop record.
   The same applies to the file the restart correction edited. Fix shape: also exempt paths named by
   any earlier pass (carried forward as `PRIOR-PASS-REWRITTEN:`) and by the restart correction
   (`RESTART-CORRECTED:`).

4. **Round 6 (confirming, ALL CLEAR).** With the round-5 fix in place, the one remaining path that
   escapes every label is the P8-T1 format restart (commit the rewrite, restart at P8-T1, P6 not
   re-run). That breaks the P8-T8 "LINES equal to the P6-T2 value" equality. It is reachable only if
   CSharpier is non-idempotent, because P6-T1 already formatted the same files with the same config,
   so it was reported as an observation, not a defect. Test reachability before calling a
   restart-path gap blocking.

**Why:** reading a task in isolation shows none of these; only following the restart arrow to the earlier
task, or running the census and mapping each line, exposes them.

**How to apply:** at preflight, for every restart rule, re-read the target task's acceptance in the
post-commit state AND on a second pass (what did pass 1 already change?); for every closed category list, run the command and classify each line.
Related: [[midplan-commit-breaks-deletion-staging-and-porcelain-spans]].
