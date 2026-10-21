# P4-T8 Identifier residual scan (AC9, AC10)

Timestamp: 2026-09-29T20-06
Command: GATE1, GATE2, GATE3, GATE4, GATE9 (Gate command reference); the P4-T8 folder-scoped confirmation payload; the P4-T8 UTF-16 census payload (POST-CLEANUP run, byte-identical to the PRE-CLEANUP run, appending to SCRATCH\utf16-census.txt); the P4-T8 census read-back payload; the P0-T17 legacy-user any-case census
EXIT_CODE: 0
Output Summary:
- GATE1=0
- GATE2=0
- GATE3=0
- GATE4=0
- GATE9=0
- FEATURE-FOLDER-HITS=0
- PLAN-FILE-HITS=0
- CENSUS-FILE-LINES=8
- LEGACY-USER-ANYCASE-FILES=9 (recorded, not gated, per D18: with GATE9=0 the remaining files carry only upper-case or mixed-case user-token forms, which rule 7 leaves as specified; this is the residual P6-T26 records)
- XML-PARSE-CHECK: 0 rewritten, 0 failed (copied from P4-T4: XML-REWRITTEN=0, XML-REPARSE-FAILED=0)
- git grep reads the working tree for modified tracked files, so the uncommitted rewrite is what these gates measured.

PRE-CLEANUP:
- CENSUS| 1 | RX-LENGTH=40
- CENSUS| 2 | UTF16-FILES=1
- CENSUS| 3 | UTF16-PROFILE-FILES=1
- CENSUS| 4 | UTF16-IDENTIFIER-FILES=1

POST-CLEANUP:
- CENSUS| 5 | RX-LENGTH=40
- CENSUS| 6 | UTF16-FILES=1
- CENSUS| 7 | UTF16-PROFILE-FILES=0
- CENSUS| 8 | UTF16-IDENTIFIER-FILES=0

The PRE-CLEANUP record (run after P4-T3 and immediately before the P4-T4 write) reads UTF16-PROFILE-FILES=1, which is what allows the POST-CLEANUP 0 to fail. The one UTF-16 file is the same file P0-T17 counted and is now redacted in place as UTF-16.

Scope: the scan covers every tracked file outside the governance directory (.claude/). This feature folder and the plan file are inside the scope of gates one to four; gate nine alone excludes this feature folder (spec Resolved tensions item 3); the two MCP configuration files (.mcp.json and .codex/config.toml) are excluded from gate one only; and the UTF-16 census covers the one file class that git grep -I omits (tracked files whose index attributes read i/-text and whose first two bytes are a UTF-16 byte-order mark).
