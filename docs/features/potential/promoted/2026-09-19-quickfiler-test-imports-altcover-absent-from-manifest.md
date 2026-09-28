# quickfiler-test-imports-altcover-absent-from-manifest (Issue #912)

- Date captured: 2026-09-19
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/quickfiler-test-imports-altcover-absent-from-manifest/ (Issue #912)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #912
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/912
- Last Updated: 2026-09-19
## Summary

`QuickFiler.Test/QuickFiler.Test.csproj` imports `altcover.8.6.45` build assets, but no
`packages.config` in the repository declares `altcover`, so the package is never restored and the
imports never resolve. The build is unaffected today only because both imports are
`Condition="Exists(...)"`-guarded and no matching `EnsureNuGetPackageBuildImports` `<Error>` element
was generated for them.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Python version: n/a (.NET Framework 4.8.1 VSTO solution)
- Command/flags used: `nuget restore TaskMaster.sln`, then enumeration of every
  `packages\<dir>` token across all `*.csproj`, `*.props` and `*.targets`
- Data source or fixture: a clean worktree cut from `origin/main` at `734112ed2` with a cold restore

## Steps to Reproduce

1. Create a fresh worktree from `origin/main` and run `nuget restore TaskMaster.sln`.
2. Inspect `QuickFiler.Test/QuickFiler.Test.csproj` lines 8 and 514.
3. Search `QuickFiler.Test/packages.config` for `altcover`.
4. Check whether `packages/altcover.8.6.45/` exists.

## Expected Behavior

Every `<Import>` naming a package under `..\packages\` corresponds to an entry in that project's own
`packages.config`, so a restore driven by the manifest materialises everything the project file
references.

## Actual Behavior

- `QuickFiler.Test/QuickFiler.Test.csproj:8` imports `..\packages\altcover.8.6.45\build\netstandard2.0\AltCover.props`.
- `QuickFiler.Test/QuickFiler.Test.csproj:514` imports `..\packages\altcover.8.6.45\build\netstandard2.0\AltCover.targets`.
- `QuickFiler.Test/packages.config` contains no `altcover` entry.
- `packages/altcover.8.6.45/` does not exist after a cold restore, and no restore will ever create
  it because no manifest requests it.

Both imports carry `Condition="Exists(...)"` and the project's `EnsureNuGetPackageBuildImports`
target carries no matching `<Error>` element, so MSBuild silently skips them and the build succeeds.

## Logs / Screenshots

- [x] Attached minimal logs or snippet
- Snippet (measured on a clean worktree at `734112ed2`, cold restore, 2026-09-19):

```
183 distinct "packages\<dir>" references across *.csproj, *.props, *.targets
  -> exactly 2 have no corresponding directory under packages/:
       Meziantou.Analyzer.3.0.203   (tracked separately as issue #898)
       altcover.8.6.45              (this issue)

All 13 distinct EnsureNuGetPackageBuildImports <Error> guard paths resolve.
```

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

No build impact today. The severity is that AltCover's targets are silently not applied: if anything
was ever expected to depend on them, it has been quietly inactive. It is also a latent trap — adding
a matching `<Error>` guard, or removing the `Exists()` condition during any future project-file
regeneration, converts a silent skip into a hard build failure.

## Suspected Cause / Notes

Most likely residue of an AltCover evaluation whose `packages.config` entry was removed while the
project-file imports were left behind. That is the mirror image of issue #898, where a project-file
element was left behind at a version the manifest had moved past.

Both are instances of one invariant failing: **the project file and its own `packages.config` must
agree**. Issue #911 builds a verifier for exactly that invariant. This entry is the second known
live violation and should be one of its test cases, not a hard-coded exception.

## Proposed Fix / Validation Ideas

- [ ] Decide whether AltCover is wanted. If not, delete both `<Import>` elements. If it is, add the
      `altcover` entry to `QuickFiler.Test/packages.config` at the version the imports name and let
      restore materialise it.
- [ ] Either way, the #911 verifier must classify "dependent element whose package is absent from
      the manifest" as a distinct, reported, non-fatal class, with this project as a fixture.
- [ ] Unit coverage: a verifier test asserting the class is detected and reported rather than
      ignored or treated as fatal.
- [ ] Manual verification: confirm a cold restore plus solution rebuild still succeeds afterwards.

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
