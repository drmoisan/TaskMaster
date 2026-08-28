---
name: project-464-efc-controller-plan-seams
description: Preflight seams from epic child #464 (EFC controller surface defects) rounds 1-4 — additive-only file-size gates, comment-bearing literal counts, guard tasks that must exist before a green run, and counts that disagree with an earlier phase's deletions
metadata:
  type: project
---

Three preflight defects recurred on the #464 plan and generalise beyond it.

**A file that only receives additions cannot carry a "fewer lines than baseline" gate.**
`EfcFormController.cs` (1084 lines at merge base) received ~+108 mandated additions against ~21
available deletions, yet 12 tasks asserted `fewer than 1084 lines` and splitting the file was out of
scope. The gate fired at the first production edit. Remedy: replace the shrink assertion with an
explicit ceiling equal to baseline plus a budgeted net delta (`at most 1204`), and make the final
file-size task itemise the delivered net delta **per remedy** so the ceiling is not a blank cheque.

**Why:** a size gate on an additive change is unsatisfiable for a correct implementation, and the
plan had no member-removal task that could pay for the additions.

**How to apply:** before writing any `strictly fewer than <baseline>` clause on a pre-existing
oversized file, sum the plan's own mandated additions against its mandated deletions in that file.
If the sum is positive, the gate must be a budgeted ceiling, not a shrink. The spec criterion and
the C2 table cell must both be reworded in the same pass — see [[feedback_spec_corrections_sweep_sibling_sections]].

**An "appears exactly once" literal gate must be scoped to non-comment occurrences.**
`Trash to Delete` occurred three times in the file: two comments (one retained by an explicit
decision record) and the code literal the task replaces. The exactly-once clause failed for a
correct implementation and contradicted its own sibling clause, since a comment is not a statement.
Scope such clauses with `outside a comment` and state the pre-change **non-comment** count.

**A red test needs a task that turns it green.** `ApplyReadEmailFormat_AfterCleanup_DoesNotThrow`
was added and observed red, but no task guarded `ApplyReadEmailFormat`, so the phase's
`failed count 0` run over 13 results was unreachable. Inserting the missing production task forces a
full renumber of the phase's tail — see [[plan-validator-task-id-sequential-constraint]]. A guard
task is production, not a test, so the plan's new-test reconciliation figure stays unchanged;
re-derive it explicitly rather than assuming.

**A count asserted in phase N must be recomputed against what phases 1..N-1 delete.** `[P5-T11]`
asserted that "the two `new System.Threading.Timer(ApplyReadEmailFormat)` arming sites still bind",
which was true at the merge base (`:875`, `:953`) but false by Phase 5: `[P1-T6]` deletes the method
enclosing `:875`. An executor counting literally records a mismatch and leaves the task unchecked
under the fail-closed rule, stalling the phase.

**Why:** the natural authoring move is to read the count off the merge-base source, but a
survival clause is evaluated at the task's own point in the plan, not at the merge base.

**How to apply:** for every clause of the form "the N occurrences of X still ..." over a file the
plan edits, grep the merge-base file for X, then check each occurrence's enclosing member against
every earlier deletion task. Restate as a baseline-relative claim — state the pre-change count and
sites, name the deleting task, and assert the surviving count — so the clause stays falsifiable
rather than being weakened to "at least one". Assertions that already separate "pre-change count"
from "delivered count" (for example a `_folderRows`-assignment or `throw;` count) are safe; bare
present-tense counts are the risk.

**A "this artifact is refreshed later" clause needs the refreshing task to say so.** Appending
"`[P9-T7]` re-reads and appends to this artifact" to an early evidence task creates an obligation
`[P9-T7]` does not carry. Add the matching clause to the consumer in the same edit — see
[[thread-granted-discharges-through-consumers]].
