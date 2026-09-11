---
name: exclusive-diff-clause-vs-task-own-evidence-artifact
description: An acceptance clause saying "the diff for this task touches no file other than <X>" is unsatisfiable when the same task must write or append its own evidence artifact; check for a production/feature-folder qualifier.
metadata:
  type: project
---

An acceptance condition of the form "the diff for this task touches no file other than
`<source path>`" is self-contradictory whenever the same task is also required to write or
append an evidence artifact, because that artifact is itself a file in the diff. The defect
hides well: it reads as a tight scope gate, and every sibling task in the same plan usually
carries the qualifier that makes it satisfiable while the defective one does not.

Observed on issue #796, plan task P4-T11: the clause read "the diff for this task touches no
file other than `QuickFiler/Controllers/QfcFormController.Deactivate.cs` and adds, removes and
modifies no line that is not a `///` comment line", while the same task's acceptance also
required appending measurements to two artifacts under the feature folder. The sibling tasks
were correct: P6-T5 said "no **production** file other than that one", and P1-T14 said "either
inside the **feature folder** or is one of these eight write-set paths".

**Why:** it makes the task's instruction impossible to follow correctly, which is a blocking
class of preflight defect, not prose imprecision. An executor either skips the mandated
artifact or fails its own acceptance.

**How to apply:** when a preflight pass reads any exclusive-diff or exclusive-file clause,
immediately check the same task for an artifact-writing obligation. If one exists, the clause
needs a `production`, `outside the feature folder`, or explicit-carve-out qualifier. Compare
against sibling tasks in the same plan — the qualifier is usually already present elsewhere
and can be lifted verbatim. Related: [[scope-gate-cannot-list-artifacts-written-after-it]],
[[sanitisation-task-cannot-sweep-its-own-record]].
