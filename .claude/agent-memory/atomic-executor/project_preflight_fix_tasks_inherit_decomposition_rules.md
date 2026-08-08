---
name: preflight-fix-tasks-inherit-decomposition-rules
description: A task inserted to fix one preflight finding must itself satisfy every rule the other findings established (atomic decomposition, paired csproj task, Decisions-Record enumeration) — check newly-added tasks against the plan's own rules, not just against the finding they resolve
metadata:
  type: project
---

When re-validating a revised atomic plan, audit every **newly inserted** task against the plan's other
ratified rules — not only against the finding it was written to close. A fix for finding X routinely
violates the rule established by finding Y in the same round.

**Why:** #436 preflight iteration 2. R5 required four bundled "measure + split + register csproj" tasks to be
decomposed into three atomic tasks each, and D-14 was amended to state that every file-creating contingency
branch carries its own dedicated `<Compile Include>` task "rather than folding registration into a measurement
or split task". The R2 fix then inserted a brand-new post-format re-measurement task ([P12-T3]) with exactly
the bundled shape R5 had just outlawed — measure, split if over 500, register the companion inline — and
D-14's enumeration of contingency branches was not extended to include it. Every other invariant (IDs,
sequencing, 157 test tasks, evidence paths, TimeProvider, constraint compliance) was clean; the sole finding
was the new task not inheriting the round's own rules.

**How to apply:** After confirming each finding is resolved, diff the new task set against the previous
iteration and run each *newly added* task through the full invariant list independently. Pay particular
attention to: (a) does it create a file without a paired registration task, (b) does it bundle more than one
independent outcome, (c) is it enumerated wherever the Decisions Record enumerates tasks of its class. Also
check whether the new task's scope silently widens a prior constraint — e.g. a split task whose scope covers
production files as well as test files needs a different `.csproj` target and interacts with a
"last production task" decision.

Related: [[project_plan_task_ids_digit_only_forces_renumbering]] — the delta for an inserted mid-phase task
must be phrased as "insert + renumber downstream", and every reference to a renumbered ID must be listed
explicitly in the delta.
