---
name: pester-filtered-total-counts-notrun
description: Pester 5.6.1 TotalCount includes filtered-out tests as NotRun, so an exact-Total assertion on a Filter.FullName run is invariant under the filter and cannot fail
metadata:
  type: project
---

`$r.TotalCount` on a Pester 5.6.1 run with `$c.Filter.FullName` set counts the **whole
discovered file**, not the filtered population: filtered-out tests land in `NotRunCount` and
are still summed into `TotalCount`. Measured on #911 P5-T5: a `*AC21-*` filter over a
13-`It` file printed `Passed=0 Failed=1 Skipped=0 Total=13`, with `NotRunCount=12`.

**Why:** plans routinely write `Filter.FullName = "*AC<N>-*"` and then assert `Total=4`
exactly, with the stated rationale "a filter that matched nothing would fail this, and one
that over-matched would break the equality". Neither property holds for `TotalCount` — it
reads the same 13 whichever way the filter goes, so the gate cannot fail. The quantity that
does carry both properties is the executed population, `PassedCount + FailedCount +
SkippedCount`, which is 0 when the filter matches nothing and >1 when it over-matches.

**How to apply:** when a plan asserts an exact filtered `Total`, emit and record BOTH
`Total=$($r.TotalCount)` (the mandated CMD-PESTER-ALL text) and
`EXECUTED = Passed + Failed + Skipped` plus `NotRun`, and evaluate the exact clause against
EXECUTED, saying so in the artifact and reporting it as a plan discrepancy. Do not silently
substitute. Lower-bound clauses (`Total at least N`) are satisfied on both readings, which
is why the defect stays hidden until the first exact-equality filtered task — and why it is
also invisible when the filtered file happens to contain only that criterion's cases.

Related: [[project_plan_checkoff_fixpoint_breaks_terminal_clean_tree_gate]],
[[feedback_gates_can_pass_for_reasons_unrelated_to_correctness]].
