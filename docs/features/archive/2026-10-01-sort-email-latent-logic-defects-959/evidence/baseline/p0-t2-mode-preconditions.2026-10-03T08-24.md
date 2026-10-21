# P0-T2 Full-Bug Mode Preconditions

Timestamp: 2026-10-03T08-24
Command: CMD-SPEC-CHECK (STAGE base), run as pwsh -NoProfile -Command with Set-Location to the item worktree (read-only counts over FEATURE/issue.md, FEATURE/spec.md and the #956 spec.md)
EXIT_CODE: 0
Output Summary: all seven acceptance conditions hold; work mode full-bug, 27 unchecked AC, 0 checked, no user-story.md, the three CR-1 anchors present once and the three replacement texts absent, the #956 spec carries 17 checked and 0 unchecked AC lines.

Printed lines:
- WORKMODE-LINES: 1
- AC-HEADING-LINES: 1
- AC-UNCHECKED: 27
- AC-CHECKED: 0
- AC6-UNCHECKED: 1
- AC27-UNCHECKED: 1
- AC-ANY-UNCHECKED: 27
- USERSTORY-EXISTS: False
- AC15-SEAM-STEP: 3
- AC6-RESET-LITERAL: 2
- AC25-NINETY: 2
- S956-OLD-99: 1
- S956-NEW-99: 0
- S956-149: 1
- S956-NEW-149: 0
- S956-OLD-155: 1
- S956-NEW-155: 0
- S956-AC-CHECKED: 17
- S956-AC-UNCHECKED: 0
- S956-LINES: 313

Recorded values:
- S956-AC-CHECKED-BASE: 17
- S956-AC-UNCHECKED-BASE: 0
- S956-LINES-BASE: 313
- PRE-EXISTING-EVIDENCE: docs/features/active/2026-10-01-sort-email-latent-logic-defects-959/evidence/other/preflight-clearance.2026-10-03T00-28.md

Acceptance check:
- WORKMODE-LINES: 1 (holds)
- AC-HEADING-LINES: 1, AC-UNCHECKED: 27, AC-CHECKED: 0 (holds)
- USERSTORY-EXISTS: False (holds)
- AC15-SEAM-STEP 3, AC6-RESET-LITERAL 2, AC25-NINETY 2, each at least 1 (holds)
- S956-OLD-99 1, S956-149 1, S956-OLD-155 1 (holds)
- S956-NEW-99 0, S956-NEW-149 0, S956-NEW-155 0 (holds)
- S956-AC-CHECKED-BASE plus S956-AC-UNCHECKED-BASE equals 17 (holds)
