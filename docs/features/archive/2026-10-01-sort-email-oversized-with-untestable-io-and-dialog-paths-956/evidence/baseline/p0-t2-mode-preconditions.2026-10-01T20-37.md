# P0-T2 Full-bug mode preconditions

Timestamp: 2026-10-01T20-37
Command: read-only verification with the Read, Grep and Glob tools (no shell command)
EXIT_CODE: 0
Output Summary:
- WORK-MODE-LINE: FEATURE/issue.md line 12 is exactly `- Work Mode: full-bug` (PASS)
- AC-HEADING: FEATURE/spec.md line 256 is exactly `## Acceptance Criteria` (PASS)
- UNCHECKED-AC-LINES: regex `^- \[ \] AC([1-9]|1[0-7])\. ` matches 17 lines of FEATURE/spec.md (lines 257 to 273) (PASS)
- CHECKED-AC-LINES: regex `^- \[x\] AC` matches 0 lines of FEATURE/spec.md (PASS)
- USER-STORY: FEATURE/user-story.md does not exist (Glob of FEATURE/** lists no user-story.md) (PASS)
- AC15-LITERALS: FEATURE/spec.md line 271 (AC15) contains `ninety percent` and `System.IO.Directory.CreateDirectory(path)` (PASS)
- AC12-LITERAL: FEATURE/spec.md line 268 (AC12) contains `fail to compile` (PASS)
- PRE-EXISTING-EVIDENCE: docs/features/active/2026-10-01-sort-email-oversized-with-untestable-io-and-dialog-paths-956/evidence/other/preflight-clearance.2026-10-01T20-25.md; docs/features/active/2026-10-01-sort-email-oversized-with-untestable-io-and-dialog-paths-956/evidence/baseline/phase0-instructions-read.md (written by P0-T1 of this run)
- RESULT: all five preconditions hold; no stop string applies.
