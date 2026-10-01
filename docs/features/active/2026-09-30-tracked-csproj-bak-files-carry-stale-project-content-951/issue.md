# tracked-csproj-bak-files-carry-stale-project-content (Issue #951)

- Date captured: 2026-09-30
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/tracked-csproj-bak-files-carry-stale-project-content/ (Issue #951)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #951
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/951
- Last Updated: 2026-09-30
- Work Mode: minor-audit

## Summary

Eight `*.csproj.bak` backup files are tracked in git on `main`. They carry superseded project content: two still contain the altcover import that #929 removed from the live project file. They confuse repository-wide searches and consistency checks, and they are stale copies of build configuration.

## Environment

- OS/version: n/a
- Python version: n/a
- Command/flags used: `git ls-tree -r --name-only origin/main`
- Data source or fixture: repository tree

## Steps to Reproduce

1. `git ls-tree -r --name-only origin/main | grep "\.csproj\.bak$"` lists:
   - `QuickFiler.Test/QuickFiler.Test.csproj.bak`
   - `QuickFiler/QuickFiler.csproj.bak`
   - `Tags/Tags.csproj.bak`
   - `TaskTree/TaskTree.csproj.bak`
   - `TaskVisualization.Test/TaskVisualization.Test.csproj.bak`
   - `TaskVisualization/TaskVisualization.csproj.bak`
   - `ToDoModel.Test/ToDoModel.Test.csproj.bak`
   - `ToDoModel/ToDoModel.csproj.bak`
2. `git grep -l -i altcover origin/main -- "*.csproj.bak"` matches `QuickFiler.Test.csproj.bak` and `QuickFiler.csproj.bak`.

## Expected Behavior

Backup copies of project files are not tracked, and `.gitignore` excludes `*.bak`.

## Actual Behavior

Eight backups are tracked, and two contain tokens that the live project files no longer have.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: reported as a follow-up by #929 (PR #949).

## Impact / Severity

- [ ] Blocker
- [ ] High
- [ ] Medium
- [x] Low

## Suspected Cause / Notes

Probably Visual Studio or NuGet migration backups that were committed by accident. Confirm that no build step or script reads them before deleting them.

## Proposed Fix / Validation Ideas

- [ ] Delete the eight files, and add `*.csproj.bak` (or `*.bak`) to `.gitignore`.
- [ ] Optionally extend the #927 repository hygiene guard to flag tracked backup files, with a negative control.

## Acceptance Criteria

- [ ] AC-1: No `*.csproj.bak` file is tracked. `git ls-files -- "*.csproj.bak"` prints nothing, and the eight listed paths are deleted from the index and the working tree.
- [ ] AC-2: `.gitignore` excludes backup copies of project files by the exact line `*.csproj.bak`, and `git check-ignore -v` reports that rule for each of the eight deleted paths.
- [ ] AC-3: The new ignore rule does not shadow any tracked file. `git ls-files -ci --exclude-standard -- "*.bak"` prints nothing, and the other three tracked backup files (`TaskMaster.sln.bak`, `TaskTree/TaskTree.vbproj.bak`, `TaskVisualization/TaskVisualization.vbproj.bak`) remain tracked and unmodified.
- [ ] AC-4: Nothing in the repository reads the deleted files. A fixed-string search of `scripts/`, `.github/`, `.claude/lib/`, `*.csproj`, `*.sln`, and `*.targets` for `.csproj.bak` returns no match on the end-state tree.
- [ ] AC-5: The change set against `origin/main` consists only of the eight deletions, the `.gitignore` edit, and files under `docs/features/`.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
