# [P4-T26] Phase 4 Commit

Timestamp: 2026-08-26T10-52

Task: [P4-T26]
Feature: docs/features/active/quickfiler-bug-family-446

Command: `git add "docs/features/active/quickfiler-bug-family-446" "docs/features/potential"`
EXIT_CODE: 0

Command: `git commit -m "docs(446): record acceptance-criteria verification for AC1 through AC17, AC20, AC21, AC23 and AC26"`
EXIT_CODE: 0

Command: `git status --porcelain -- "docs/features/active/quickfiler-bug-family-446" "docs/features/potential"`
EXIT_CODE: 0

## Resulting HEAD

- Short sha: f455e2dd
- Full sha: f455e2dd4f418dcff319679c4bb0f8242ac1608f

## Files Committed

- 21 acceptance-criteria verification artifacts under `evidence/qa-gates/` (one per criterion, [P4-T1] through [P4-T21])
- 5 mirrors under `evidence/issue-updates/`: the [P4-T17] closing-keyword constraint record plus the four follow-up issue mirrors from [P4-T22] through [P4-T25]
- `spec.md` with 21 acceptance criteria checked off
- `plan.2026-08-24T09-37.md` with [P4-T1] through [P4-T25] checked off
- 4 promoted records under `docs/features/potential/promoted/`

## Promoted-File Presence

[P4-T26] instructs that an absence of files under `docs/features/potential/` be recorded explicitly
rather than treated as a promotion failure. In this run the files were NOT absent. All four
promotions retained their promoted record:

- `docs/features/potential/promoted/2026-08-26-quickfiler-emailmovemonitor-instances-not-shared.md`
- `docs/features/potential/promoted/2026-08-26-qfcformcontroller-cleanup-disposal-ordering.md`
- `docs/features/potential/promoted/2026-08-26-qfcremainingqueueadmission-dead-scoreloader.md`
- `docs/features/potential/promoted/2026-08-26-quickfiler-500-line-cap-violations.md`

Each pre-promotion original was MOVED out of `docs/features/potential/` into `promoted/`, which is
the documented behavior for a source resolved directly from `docs/features/potential/`. Verified: zero
`2026-08-26-*.md` files remain directly under `docs/features/potential/`, and all four promoted
records exist and are committed.

## Follow-up Issues Filed

| Task | Issue | Type | Title |
| --- | --- | --- | --- |
| [P4-T22] | #620 | bug | three independent EmailMoveMonitor instances |
| [P4-T23] | #621 | bug | QfcFormController.Cleanup() disposal ordering |
| [P4-T24] | #622 | bug | dead scoreLoader parameter |
| [P4-T25] | #623 | feature | pre-existing 500-line cap violations |

Output Summary: Phase 4 committed as f455e2dd. The acceptance condition holds:
`git status --porcelain` over the two staged pathspecs produced zero output lines. 21 of 28
acceptance criteria are checked off in spec.md; AC18, AC19, AC22, AC24, AC25, AC27 and AC28 remain
deferred to Phase 5 by plan design. Four follow-up issues (#620, #621, #622, #623) were filed through
the MCP promotion path.
