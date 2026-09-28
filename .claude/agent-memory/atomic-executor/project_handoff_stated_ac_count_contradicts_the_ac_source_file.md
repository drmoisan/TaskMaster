---
name: handoff-stated-ac-count-contradicts-the-ac-source-file
description: A predecessor delegation's closing "N of M criteria checked" is a claim, not evidence — an earlier phase may already have checked several off per the tracking skill; measure the AC source file before executing any check-off phase
metadata:
  type: project
---

Before executing an acceptance-criteria check-off phase, measure the `## Acceptance Criteria` section
of the AC source file yourself. Never take the starting count from a predecessor delegation's closing
summary or from the coordinator's prompt.

**Why:** On 2026-09-13 (issue #871, Phase 7) the previous delegation's closing summary reported 0 of
22 criteria checked. The file actually read **15 checked, 7 unchecked, 22 total**: Phase 4 had checked
15 off as its verification tasks passed, which is exactly what `acceptance-criteria-tracking` directs
("check off AC items as soon as the corresponding plan task passes verification — do not defer all AC
updates to the end"). So the discrepancy was not an error by either party; it is structural. A phase
that checks off criteria mid-run and a later phase whose tasks are each written as "Check off ACn"
will always disagree about the starting state, and the disagreement grows with the number of
mid-run check-offs. The coordinator caught it here only by measuring the file directly.

**How to apply:**
- Grep the AC source file for `^- \[[ x]\] AC` with `-n` before task 1 of the check-off phase, and
  report the measured triple (checked / unchecked / total) at the start and at the end.
- For an already-checked criterion, do **not** uncheck and recheck it. Rule 3 of the tracking skill
  permits only `- [ ]` -> `- [x]`, and a recheck writes a spurious diff hunk into a scope-locked spec.
- The citation half of the task is still owed for **all** criteria. Each check-off task's acceptance
  usually has two clauses — the box reads checked, and the completion record cites that criterion's
  evidence. An already-ticked box satisfies only the first. Do not skip the task.
- Verify each cited artifact exists on disk *before* writing its pointer into the record, not after.
  The reconciliation task at the end of such a phase typically requires exactly that, and a pointer
  written first and checked later turns a check-off into a finding.
- Where the check-off tasks cite an artifact a *later* task writes (the completion record), build that
  record incrementally, one row per task, so each task's own acceptance is verifiable when it runs.
  See [[project_preflight_checkoff_cites_later_task_artifact]].
