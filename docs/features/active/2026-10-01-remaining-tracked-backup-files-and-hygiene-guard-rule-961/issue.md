# remaining-tracked-backup-files-and-hygiene-guard-rule (Issue #961)

- Date captured: 2026-10-01
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/remaining-tracked-backup-files-and-hygiene-guard-rule/ (Issue #961)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #961
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/961
- Last Updated: 2026-10-01
- Work Mode: minor-audit

## Summary

#951 (PR #960) removed the eight tracked `*.csproj.bak` files and added an ignore rule. Two gaps remain:
- three other tracked backup files on `main`;
- no CI rule preventing a forced add from reintroducing them.

An ignore rule does not stop `git add -f`.

## Environment

- OS/version: n/a
- Python version: n/a
- Command/flags used: `git ls-tree -r --name-only origin/main | grep -E "\.bak$"`
- Data source or fixture: repository tree at `9a3d2dd3e` or later

## Steps to Reproduce

1. List tracked `.bak` files on `main`:
   - `TaskMaster.sln.bak`
   - `TaskTree/TaskTree.vbproj.bak`
   - `TaskVisualization/TaskVisualization.vbproj.bak`
2. Force-add any `*.csproj.bak`. The hygiene guard reports nothing.

## Expected Behavior

- No tracked backup files.
- The repository hygiene guard (#927, `scripts/hygiene/`) fails CI when any tracked `*.bak` exists.

## Actual Behavior

Three backups remain, and the guard has no backup-file rule.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: #951 executor follow-ups 1 and 2 (PR #960).

## Impact / Severity

- [ ] Blocker
- [ ] High
- [ ] Medium
- [x] Low

## Suspected Cause / Notes

#951 was scoped to `*.csproj.bak` only. Confirm that no build step reads the `.sln.bak` or `.vbproj.bak` files before deleting them.

Pre-plan findings recorded by the preparation run (verified against the worktree at origin/main 860d67bf4):

- Exactly three tracked backup files exist and no other backup-family file (`.orig`, `.rej`, `.old`, `.tmp`, `.swp`) is present on disk or tracked. The #951 audit and a repository-wide glob agree.
- No build step, project file, workflow or script reads `TaskMaster.sln.bak`, `TaskTree/TaskTree.vbproj.bak` or `TaskVisualization/TaskVisualization.vbproj.bak`. Matches outside `docs/` and `.claude/` for the `.bak` token are limited to `.gitignore` and one test string literal in `UtilitiesCS.Test/HelperClasses/PhysicalFileSystemAdapters_Tests.cs`.
- `.gitignore` already carries `*.rptproj.bak` and `*.csproj.bak` (lines 257 and 258) and does not cover `*.sln.bak` or `*.vbproj.bak`. The plan adds a general `*.bak` rule.
- Rule scope decision: the guard matches the `.bak` extension only, case-insensitively, on the final path segment. A broader family is not adopted because the issue scopes `*.bak`, the tree holds no other backup-family file to justify it, and `.old`, `.tmp` and `.orig` are legitimate fixture or tool-output suffixes in some trees, which would create false positives.
- The guard keeps its single governance-directory exclusion (`.claude/`); the backup rule applies to every other tracked path and adds no exemption mechanism.

## Acceptance Criteria

- [x] AC-1: No tracked backup file remains. `git ls-files -- "*.bak"` prints nothing, and the three files `TaskMaster.sln.bak`, `TaskTree/TaskTree.vbproj.bak` and `TaskVisualization/TaskVisualization.vbproj.bak` are deleted from the index and the worktree.
- [x] AC-2: `.gitignore` carries a `*.bak` rule, so `git check-ignore -q TaskMaster.sln.bak` exits 0 for a re-created file of that name.
- [x] AC-3: The repository hygiene guard reports `HYGIENE backup-file <path>` for every tracked path whose extension is `.bak` (case-insensitive), counts each as a finding, and exits 1; paths under the `.claude/` governance directory stay excluded as for the existing rules.
- [x] AC-4: Pester tests under `tests/scripts/hygiene/` prove the rule: a negative control seeds a tracked `.bak` path and asserts the finding line and exit code 1, and positive cases assert zero findings for `.bak`-lookalike names and for a clean listing.
- [x] AC-5: The hygiene guard run against the final tree prints `HYGIENE Findings=0` and exits 0.
- [x] AC-6: PoshQC format, analyze and test pass for the changed PowerShell files. The PoshQC test run over `tests/scripts/hygiene` reports `errors="0" failures="0"` in `artifacts/pester/pester-junit.xml` with a test count above the 31-test baseline. Line coverage of `scripts/hygiene` is measured by CI (`_pester.yml`), because the PoshQC coverage document does not include `scripts/hygiene` in its denominator; locally, every added statement is exercised by a named test.
- [x] AC-7: `.github/workflows/README.md` describes the backup-file rule in the `_hygiene.yml` row.

## Proposed Fix / Validation Ideas

- [ ] Delete the three files, and broaden the ignore rule to `*.bak` if appropriate.
- [ ] Add a guard rule that flags tracked `*.bak` paths, with Pester tests and a negative control proving the rule fails on a seeded path. Run it through PoshQC, with coverage from CI.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
