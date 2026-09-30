# tracked-csproj-bak-files-carry-stale-project-content (Issue #951)

- Date captured: 2026-09-30
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/tracked-csproj-bak-files-carry-stale-project-content/ (Issue #951)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #951
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/951
- Last Updated: 2026-09-30
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

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
