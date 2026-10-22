# QA Gate: Evidence Form (#1002)

Timestamp: 2026-10-22T10-28
Command: pwsh -NoProfile -File SCRATCH/issue-1002-evidenceform.ps1 (git diff --name-status $(git merge-base origin/main HEAD) added paths plus git status --porcelain --untracked-files=all untracked paths; extension and name checks; Get-ChildItem over <FEATURE>/evidence; Select-String for drive-rooted paths and user-profile segments)
EXIT_CODE: 0
Output Summary:
- Added or untracked paths examined: 52. Paths ending in `.trx`, `.coverage`, or `.xml`, or containing `cobertura` or `jacoco`: 0.
- Files under `<FEATURE>/evidence/`: 35 at the time of the check, all Markdown (non-Markdown: 0).
- Search of `<FEATURE>/evidence/` for a drive-rooted path (a drive letter, a colon, and a separator), a user-profile directory segment, the account name, the short profile name, or the per-user application-data folder name: 0 hits (patterns held in SCRATCH/issue-1002-evidenceform.ps1). A re-run after the self-review was written also returned 0 hits once this line was reworded, because the earlier wording quoted the searched tokens and matched itself.
- Raw trx, Cobertura, and JaCoCo outputs remain in the ignored `coverage/` folder and in SCRATCH only.
