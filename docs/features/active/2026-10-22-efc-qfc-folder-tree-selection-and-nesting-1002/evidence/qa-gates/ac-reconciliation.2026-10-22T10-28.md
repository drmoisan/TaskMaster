# QA Gate: Acceptance-Criteria Reconciliation (#1002)

Timestamp: 2026-10-22T10-28
Command: grep -cE '^- \[x\] AC[0-9]+ ' spec.md; grep -cE '^- \[ \] AC[0-9]+ ' spec.md; git diff HEAD -- <FEATURE>/spec.md | grep -E '^[-+]- \[' | sed -E 's/^[-+]- \[.\] //' | sort | uniq -c
EXIT_CODE: 0
Output Summary:
- Checked AC boxes: 26; unchecked: 0.
- Text check against `spec.md` at `HEAD` (the merge base has no `spec.md`): the diff changes exactly 26 lines, and after the checkbox marker is normalized each of the 26 criterion texts appears exactly twice (once removed, once added), so every criterion text is unchanged and only the box changed.
- Each AC was checked off by its own task (P9-T7 to P9-T32), one edit per criterion, citing the evidence named in the plan.

### Acceptance Criteria Status
- Source: `docs/features/active/2026-10-22-efc-qfc-folder-tree-selection-and-nesting-1002/spec.md`
- Total AC items: 26
- Checked off (delivered): 26
- Remaining (unchecked): 0
- Items remaining: none

Notes carried into the self-review:
- AC18: the merge-base diff also lists three orchestrator-owned paths committed before execution (two task-researcher agent-memory files and the promotion record); none is a spec-excluded file. See `scope-fences`.
- AC26 is satisfied by the pending-human record; the manual Outlook check itself has not been performed.
