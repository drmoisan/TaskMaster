---
name: project-959-r3-checkoff-evidence-precedes-edit-and-line-anchored-field-check-seams
description: Issue #959 preflight round 3 seams - an AC check-off task must run its own gate before the Edit (forward-record the output into a later artifact), a `Contains` field check matches prefixed labels (anchor it with `(?m)^[^\w\r\n]*` plus a printed negative control), and task-text row enumerations silently drop the plain `EXIT_CODE:` row
metadata:
  type: project
---

Three seams that cost preflight round 3 on #959 (plan revision 1.2 -> 1.3, 2026-10-02).

1. **A check-off's precondition must be evidence that already exists when the Edit runs.** AC26 (evidence fields) was checked off at P6-T41 on the P6-T13 field-check counts, but artifacts P6-T14, P6-T15 and P6-T21 were written between them; the "second run" at P6-T43 came after the box was ticked. Fix shape: the check-off task issues the read-only gate itself over every artifact written so far, and its printed lines are forward-recorded into a later task's artifact (the plan already did this for `AC7-READ: HOLDS` into the P6-T43 inventory) under a named heading. The final run becomes a post-check-off confirmation whose only remedy is adding a field, never reverting the box.
   **Why:** an executor who ticks on stale counts cannot be told apart from one who verified; the reviewer reads the ordering, not the intent.
   **How to apply:** for every check-off task, ask "which run produced the number this precondition cites, and what was written after it?" If anything was, the task must run the gate itself.

2. **`$c.Contains("EXIT_CODE:")` is satisfied by `FORMAT_EXIT_CODE:`.** Every prefixed payload label (`FORMAT_`, `CHECK_`, `MSBUILD_`, `VSTEST_`, `COLLECT_`, `RESTORE_`, `PASS-AFTER-VSTEST_`) contains the bare field name, so a substring field check can never report a missing field in any artifact that copies a payload label. Fix: `[regex]::IsMatch($c, "(?m)^[^\w\r\n]*" + [regex]::Escape($label))` (label at line start after optional list/emphasis punctuation, never after a word character) plus a printed control `FIELD-CHECK-CONTROL: ` = `IsMatch("FORMAT_EXIT_CODE: 0", "(?m)^[^\w\r\n]*EXIT_CODE:")` gated `False`.
   **Why:** the gate had a zero count that proved nothing; the control makes the discrimination observable on every run.
   **How to apply:** any "artifact carries field X" gate over files that also carry `PREFIX_X` labels; pair the anchored match with a control that feeds the prefixed form and must print False.

3. **A task's row enumeration overrides the convention in the executor's hands.** The Artifact-fields convention said every artifact carries `Timestamp:`, `Command:`, `EXIT_CODE:`, but tasks such as P1-T4 enumerated "BEFORE/AFTER hashes, `FORMAT_EXIT_CODE:`, `CHECK_EXIT_CODE:`" and nothing else; once the field check was line-anchored those artifacts would fail it. The audit found 24 tasks whose enumeration lacked the plain `EXIT_CODE:` (and two fixed-name baselines lacking `Timestamp:`). Fix: name the invocation the `EXIT_CODE:` row is scoped to in each task (the printed `*_EXIT_CODE:` label, or the process exit code for payloads such as a backup copy that print no label), state in the convention that an enumeration or "every printed line" is additional to the three rows, and give a default scope (last invocation before the write) for tasks that name none.
   **Why:** an executor copying the listed rows verbatim is following the plan; the convention is read once, the task text is read at the moment of writing.
   **How to apply:** whenever a payload prints a prefixed exit label, the task that records it must also say where the plain `EXIT_CODE:` comes from.

Related: the noncanonical-subfolder count was already satisfiable because `$files` was rooted at `FEATURE\evidence` (the plan, spec.md, issue.md and research/ sit outside it); the round only asked for per-file `NONCANONICAL:` rows and a stop string (`NONCANONICAL EVIDENCE FILE`, the executor never moves a file it did not write). See [[project-959-r2-pre-existing-evidence-ordering-and-no-git-channel-seams]] for the INHERITED subtraction this check depends on and [[self-referential-evidence-enumeration]] for why the final run cannot check its own artifact.
