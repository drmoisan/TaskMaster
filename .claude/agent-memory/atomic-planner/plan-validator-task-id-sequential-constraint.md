---
name: plan-validator-task-id-sequential-constraint
description: Planner-output validator requires digit-only task IDs and sequential-by-appearance numbering; mid-phase insertion forces renumbering all later tasks in that phase
metadata:
  type: feedback
---

`.claude/hooks/validate-planner-output.ps1` (the atomic-planner SubagentStop hook) enforces task lines with the regex `^- \[(?<State>[ xX])\] \[P(?<Phase>\d+)-T(?<Task>\d+)\] (?<Text>.+)$` and checks numbering with `$expectedTaskNumber = tasksByPhase[phase].Count + 1` (sequential by order of appearance).

**Why:** A letter-suffixed insertion ID like `P4-T3a` fails the task-line regex (digits only after `T`), and any inserted task that is not numbered as the next integer in appearance order fails the sequential check.

**How to apply:** When a caller asks to "insert a task before P#-T#" (even offering a suffix like `P4-T3a`), you cannot use a suffix and cannot leave gaps. You MUST renumber every later task in that phase (`T4→T5, T5→T6, ...`), preserving each task's checkbox state and text. Then update every cross-reference to the renumbered IDs: the Status header, the Traceability section ranges, any Notes-prose pointers, and any earlier-phase task that points forward to a Phase-4 task by ID (e.g. a `[x]` P2 task's parenthetical "(P4-T6)"). Renumbering a completed task's ID token and fixing pointers to it is a mechanical consequence of a validator-forced renumber, not a re-opening of the work. Related: [[plan-validator-phase-heading-constraint]].
