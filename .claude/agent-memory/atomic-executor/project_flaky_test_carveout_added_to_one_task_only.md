---
name: flaky-test-carveout-added-to-one-task-only
description: A known-flaky-test disposition added to one task during a revision round leaves the sibling tasks that run the same suite without it, and can produce a record a later task's pinned count cannot absorb
metadata:
  type: project
---

When a preflight round asks the planner to add a disposition branch for a named
known-intermittent test, check two things before clearing the revision.

1. **Every task that runs that assembly needs the same branch.** A plan usually runs the
   same suite several times (per-phase regression run, final confirming run, coverage run).
   A carve-out applied to only the phase-level run leaves the later runs treating the same
   probabilistic failure as a new regression, which typically routes to a `stop` instruction
   near the end of the plan.
2. **Check where the carve-out's output is meant to land.** If the branch says "record it as
   an observation row on the schema task X defines", and task X runs later with an acceptance
   pinning an exact row count (for example `OBSERVATIONS: 4`), the row cannot be added to that
   log without failing X's own acceptance. The row is then stranded in the earlier task's
   evidence artifact.

**Why:** Observed on the issue #823 plan, round 3. Round 2's coherence repair gave [P3-T9]
a disposition for `Transaction_SecondCallerCannotInstallUntilTheFirstRestores`; [P6-T5] and
[P6-T6] run the same assembly and kept the "stop on a non-baseline failure" rule, and
[P5-T1] pins the flake log at exactly four rows.

**How to apply:** During a confirming preflight round, grep the plan for every task naming
the same test assembly and compare their failed-set dispositions. Neither finding is
blocking on its own — both branches remain executable with a defined outcome — so report
them as observations with an applyable delta rather than as a revision requirement.

Related: [[project_exact_count_gate_vs_remediation_loop]],
[[project_expectedexitcode_declared_from_baseline_not_observed_run]],
[[project_winformspumphost_tests_load_flaky]].
