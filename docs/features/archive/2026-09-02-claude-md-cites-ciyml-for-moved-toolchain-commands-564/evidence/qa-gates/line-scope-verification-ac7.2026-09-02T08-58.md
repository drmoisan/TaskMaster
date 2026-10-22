---
timestamp: 2026-09-02T20-52
plan: docs/features/active/claude-md-cites-ciyml-for-moved-toolchain-commands-564/plan.2026-09-02T08-58.md
task: P3-T7
ac: AC7
---

# AC7 Verification: Command Text Unchanged, Only Citation Token Replaced

Timestamp: 2026-09-02T20-52

Command: `git diff origin/main...HEAD -- CLAUDE.md`

EXIT_CODE: 0

Output Summary: Diff shows exactly 3 removed lines and 3 added lines. Each removed line contains `.github/workflows/ci.yml` and each added line contains the corresponding reusable workflow file (same position, same surrounding text otherwise). No line other than 194, 202, and 210 changed. Command text and argument text on surrounding lines is unchanged.

## Diff Analysis

| Line | Change | Old Citation | New Citation | Command Text Status |
|---|---|---|---|---|
| 194 | Replaced | `.github/workflows/ci.yml` | `.github/workflows/_format-check.yml` | UNCHANGED (dotnet tool run ... manifest-pinned version ...) |
| 202 | Replaced | `.github/workflows/ci.yml` | `.github/workflows/_build-analyzers.yml` | UNCHANGED (/t:Rebuild not /t:Build ... /t:Build /m for analyzer step ...) |
| 210 | Replaced | `.github/workflows/ci.yml` | `.github/workflows/_build-nullable.yml` | UNCHANGED (character-for-character the command in ... /p:TreatWarningsAsErrors ...) |

## Diff Line Count

- Removed lines (marked with `-`): 3 (lines 194, 202, 210 old text)
- Added lines (marked with `+`): 3 (lines 194, 202, 210 new text)
- Total diff hunks: 3 (one per citation)
- Lines outside the citation tokens: UNCHANGED

## Verification Result

- Only citation token changed on each line: YES
- Command text preserved on each line: YES
- No edits to surrounding lines: YES
- No edits to lines outside 190–215 range: YES

---

**AC7 Status: PASS** — The command text in all three cited bullets is unchanged (only the file citation is edited).
