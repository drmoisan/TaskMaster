# [P3-T20] AC8 asset-path check

Timestamp: 2026-09-07T08-30

Command: `git add --intent-to-add -- 'QuickFiler/Resources'`; `git diff --name-only <BASE-SHA> -- 'QuickFiler/Resources'`; `git status --porcelain --untracked-files=all -- 'QuickFiler/Resources'`

EXIT_CODE: 0

ExpectedExitCode: 0

## Result

AC8-ASSET-PATHS-CHANGED: 0

- Anchored diff (`git diff --name-only <BASE-SHA> -- 'QuickFiler/Resources'`): 0 lines.
- Porcelain companion (`git status --porcelain --untracked-files=all -- 'QuickFiler/Resources'`): 0 lines.
- Tracked files in that directory at the time of the check: 43.

The `--intent-to-add` companion ran first so a newly created asset would be visible to the anchored diff, and the
porcelain companion covers the state in which a change is already committed and the anchored diff is the only
witness. Both returned empty. The directory is non-empty (43 tracked files, including
`QuickFiler/Resources/FolderBreadcrumb.html`, the QuickFiler breadcrumb page [P3-T12] traced), so an empty result
is a real observation rather than the trivially empty result of querying a path that does not exist.

## Why this check is made here and not read off [P3-T11]

R7 scopes the [P3-T11] enumeration to the source pathspec `'*.cs' '*.csproj'`, and the QuickFiler resources
directory contains no `.cs` and no `.csproj` file. An assertion that the directory is absent from that enumeration
would therefore be true for every possible execution and would gate nothing. This task runs its own separately
anchored diff over the resources pathspec, which can fail if an asset is touched.

Output Summary: No file under `QuickFiler/Resources` was changed by this plan. Both the anchored diff and the
porcelain companion returned zero lines against a directory holding 43 tracked files.
`AC8-ASSET-PATHS-CHANGED: 0`. Together with `<FEATURE>/evidence/qa-gates/p3-t12-ac8-verification.md`, which records
the zero-match repository-wide negative search and the five re-verified render-path traces, this establishes that
AC8 is satisfied by a verified finding and that no renderer or asset change was made.

## Path hygiene (R3)

No absolute host path, host account name, or machine name appears in this artifact.
