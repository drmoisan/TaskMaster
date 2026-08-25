---
name: conditional-ladder-and-unowned-class-gates
description: Three plan traps found post-ALL-CLEAR on #498 — an un-gated first rung of a decision ladder, a failing-identifier clause over unowned test classes, and a per-file changed-line figure over a file the plan forbids writing
metadata:
  type: project
---

Three traps that survive preflight because each is LATENT — the happy path makes them
invisible — yet each strands an executor the moment the plan's own recorded branch
value goes the other way. Found on `docs/features/active/breadcrumb-router-navigation-defects-498/plan.2026-08-24T09-39.md` at version 2.3, after `PREFLIGHT: ALL CLEAR`.

**1. Every rung of a decision ladder needs its own NOT-APPLICABLE gate, including rung 1.**
When an earlier read-only task RECORDS which rung applies (`D7 RUNG SELECTED: 1|2|3`) and
the fallback rungs each carry `CONDITIONAL: applies ONLY IF ... otherwise NOT APPLICABLE`,
it is easy to leave the PREFERRED rung un-gated because it is the expected outcome. A
non-1 recorded value then makes rung 1 both unachievable and unwaivable, with no in-plan
resolution. Gate all rungs symmetrically, add every one to the enumerated "Conditional
branches" list, and state that the rungs are mutually exclusive so exactly one executes.

**Why:** the enumerated conditional-branch rule is a whitelist — a task not on it may not
record itself NOT APPLICABLE at all, so the omission is not merely untidy, it is binding.

**How to apply:** whenever a plan has a task that RECORDS a branch selector, grep every
consumer of that selector and confirm each carries its own branch. See
[[thread-granted-discharges-through-consumers]] for the related producer/consumer defect.

**2. A "no failing identifier in these test classes" clause must name only OWNED classes.**
A full-suite gate listing ten test classes, four of which the plan may not write, has no
degradation for the four: one red at baseline makes the gate unachievable and loops the
clean-pass task. Scope the clause to the owned classes and defer the unowned ones
explicitly to the `BASELINE_FAILURE_SET` subset condition plus their own per-class gates.
Extends [[project_445_keyboard_action_plan_seams]] from scoped runs to the full-suite gate.

**3. A per-file changed-line coverage figure over a file the plan forbids writing is 0/0.**
`P8-T7` listed a file `P6-T3` forbids adding to. State that an empty changed-line set is
reported as `NOT APPLICABLE`, never as a percentage, and that the new-code floor does not
apply to such a row — otherwise the executor computes a division by zero.

**Verify CRLF claims by measurement, not by trust.** A binding "these three .csproj files
are CRLF-terminated" paragraph missed a fourth reachable from the residual-split task.
Count CR-bearing lines against total lines per file (`rg -c '\r'` vs `rg -c '^'`); expect
`total - 1` when the final line is unterminated.
