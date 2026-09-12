---
name: plan-aggregate-claims-must-be-rederived-after-deltas
description: After any plan delta, re-derive every plan-level aggregate claim (branch inventories, file counts, universally-worded standing criteria) and check each conditional branch's acceptance is satisfiable in ALL arms
metadata:
  type: feedback
---

Two defect classes cost a full preflight iteration on #435 F6 (531-task plan). Both are invisible to the structural validator, which only checks IDs, headings, and counts.

**1. A conditional branch's acceptance clause must be satisfiable in every arm.**
[P0-T12] ran `csharpier check .` at repo root, defined a non-zero-count fallback arm that scopes [P11-T1]/[P11-T2] to the child's diff set, and *then* stated "acceptance is that the count is recorded numerically **and equals zero**." The executor entering the non-zero arm could never check the task off. Fix: acceptance became "recorded numerically together with an explicit `repo-root` or `diff-set` scoping verdict; a non-zero count is not an acceptance failure, it selects the diff-set scope."

**2. Aggregate claims silently rot when a later delta adds tasks.**
Three separate counts went stale in one round: the "exactly four authorized conditional branches" inventory missed a fifth branch spanning three tasks ([P0-T12]/[P11-T1]/[P11-T2]); the Test Plan said "eleven `.PartN.cs`" and "eleven base `[TestClass]`" after a sizing delta made it 17 and 12; the Notes said "split nine fixtures into twenty-six files" when it was twelve into twenty-nine. Derived downstream figures (31 total, 31 csproj lines) were already right, which is what hid the errors.

**Why:** The plan validator counts task IDs and phase headings; it never cross-checks prose against the task list. A stale inventory or an unsatisfiable acceptance clause routes the executor into a state with no legal exit, which surfaces only at preflight — one full revision cycle late.

**How to apply:** After writing or revising any plan, before returning it for preflight:
- Grep the plan for every number-word and digit that quantifies tasks/files/branches (`four`, `eleven`, `nine`, `twenty-six`) and re-derive each from the actual task list rather than trusting the prior revision.
- A branch that spans multiple tasks still counts as **one** branch in the inventory, and the inventory sentence must name every task it spans.
- For each conditional task, read the acceptance clause against both arms; if either arm cannot satisfy it, rewrite acceptance to describe the *recording* obligation, not one arm's outcome.
- Treat universally-quantified standing criteria ("Every size measurement in Phases 1-5 is taken after the format task") as claims to verify task-by-task; scope them explicitly when exceptions exist rather than leaving the universal wording.

Related: [[per-phase-size-gates-need-scoped-csharpier]], [[csharpier-format-not-pipe-files-gate]], [[plan-validator-task-id-sequential-constraint]].
