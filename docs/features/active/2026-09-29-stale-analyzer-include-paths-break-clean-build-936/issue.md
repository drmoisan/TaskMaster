# stale-analyzer-include-paths-break-clean-build (Issue #936)

- Date captured: 2026-09-29
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/stale-analyzer-include-paths-break-clean-build/ (Issue #936)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #936
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/936
- Last Updated: 2026-09-29
- Work Mode: minor-audit

## Summary

`main` does not compile after a clean package restore. Dependabot PR #921 (merged 2026-09-26) bumped Meziantou.Analyzer to 3.0.290 and MSTest.Analyzers to 4.4.1 in every `packages.config`, but left the `<Analyzer Include>` paths in the project files pointing at the previous version folders. A clean restore does not create those folders, so the compiler fails with CS0006. CI stays green because its `actions/cache` `restore-keys` fallback restores an older `packages/` cache that still contains the stale folders.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Python version: not applicable (.NET Framework 4.8.1 packages.config solution)
- Command/flags used: `msbuild TaskMaster.sln /t:Restore /p:RestorePackagesConfig=true`, then `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU"`
- Data source or fixture: a fresh detached worktree at `main` `dcce3c816` with an empty `packages/` folder

## Steps to Reproduce

1. Create a fresh worktree at `main`.
2. Restore packages into its empty `packages/` folder.
3. Rebuild the solution.

## Expected Behavior

The solution builds from a clean restore, and CI fails whenever it would not.

## Actual Behavior

The build fails with:
- `CSC : error CS0006: Metadata file '..\packages\Meziantou.Analyzer.3.0.235\analyzers\dotnet\roslyn5.0\cs\Meziantou.Analyzer.dll' could not be found`, in 16 project files. The manifests declare 3.0.290.
- `CSC : error CS0006: Metadata file '..\packages\MSTest.Analyzers.4.4.0\analyzers\dotnet\cs\MSTest.Analyzers.dll' could not be found`, plus `MSTest.Analyzers.CodeFixes.dll`, in 9 project files. The manifests declare 4.4.1.

A scan of every `..\packages\<dir>\` reference against the clean restore finds exactly these two stale directories. It also finds `altcover.8.6.45`, which is guarded by `Exists()` and tracked by #929.

## Logs / Screenshots

- [x] Attached minimal logs or screenshot
- Snippet: see Actual Behavior. The fix commit records the before and after rebuild output.

## Impact / Severity

- [x] Blocker
- [ ] High
- [ ] Medium
- [ ] Low

Blocker: the maintainer cannot build the tool from a clean checkout. Every fresh worktree used by agent runs is affected as well.

## Suspected Cause / Notes

- The Dependabot grouped update rewrites `packages.config` versions but does not rewrite `<Analyzer Include>` paths. This is the same defect class #898 fixed (PR #913) for the previous Meziantou bump.
- Four workflows (`_build-analyzers.yml`, `_build-nullable.yml`, `_format-check.yml`, `_mstest-coverage.yml`) key the `packages/` cache on `hashFiles('**/packages.config')` with a `restore-keys` prefix fallback. On a miss, the fallback cache carries the old version folders, so stale paths resolve in CI and the defect is invisible to every required check.

## Proposed Fix / Validation Ideas

- [ ] Repoint all `<Analyzer Include>` paths to the versions declared in each project's `packages.config`: Meziantou.Analyzer 3.0.290 and MSTest.Analyzers 4.4.1.
- [ ] Remove the `restore-keys` fallback from the `packages/` cache in the four workflows, so CI restores exactly what the manifests declare.
- [ ] Validation: a clean-restore rebuild of the whole solution passes locally on a fresh worktree, and a scan of `..\packages\<dir>\` references finds no missing directory other than the `Exists()`-guarded altcover import.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
