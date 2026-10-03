# remaining-tracked-backup-files-and-hygiene-guard-rule (Issue #961)

- Date captured: 2026-10-01
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/remaining-tracked-backup-files-and-hygiene-guard-rule/ (Issue #961)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #961
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/961
- Last Updated: 2026-10-01
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

## Proposed Fix / Validation Ideas

- [ ] Delete the three files, and broaden the ignore rule to `*.bak` if appropriate.
- [ ] Add a guard rule that flags tracked `*.bak` paths, with Pester tests and a negative control proving the rule fails on a seeded path. Run it through PoshQC, with coverage from CI.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
