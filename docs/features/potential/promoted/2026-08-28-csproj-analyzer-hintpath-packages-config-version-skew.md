# csproj-analyzer-hintpath-packages-config-version-skew (Issue #682)

- Date captured: 2026-08-28
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/csproj-analyzer-hintpath-packages-config-version-skew/ (Issue #682)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #682
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/682
- Last Updated: 2026-08-28
## Summary

16 first-party `.csproj` files carry `<Analyzer Include>` HintPaths naming older Meziantou.Analyzer/Roslynator.Analyzers versions than the versions declared in their sibling `packages.config` files, so a clean-worktree NuGet restore followed by the repo's analyzer-baseline build fails with `CS0006` (metadata file not found) for every affected project.

## Environment

- OS/version: Windows, .NET Framework / VSTO toolchain
- Command/flags used: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` in a fresh worktree with no pre-populated `packages/` directory
- Data source or fixture: N/A

## Steps to Reproduce

1. Create a new git worktree/clone of the repository with no `packages/` directory present.
2. Run `dotnet tool restore` and the repo's standard NuGet restore for the solution.
3. Run the analyzer-baseline msbuild command above.

## Expected Behavior

The analyzer-baseline build succeeds against whatever `Meziantou.Analyzer`/`Roslynator.Analyzers` versions are declared in each project's `packages.config`.

## Actual Behavior

10 `CS0006` errors occur: the affected `.csproj` files' `<Analyzer Include>` items hard-code HintPaths to `Meziantou.Analyzer.3.0.156` and `Roslynator.Analyzers.4.16.0`, while the corresponding `packages.config` entries declare `3.0.174` and `4.16.1` respectively, so NuGet restores the newer package directories and the older, csproj-referenced paths never exist on disk.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: `CS0006: Metadata file 'packages\Meziantou.Analyzer.3.0.156\analyzers\...\Meziantou.Analyzer.dll' could not be found` (and the Roslynator.Analyzers 4.16.0 equivalent), repeated across 16 first-party projects.

## Impact / Severity

- [ ] Blocker
- [x] Medium
- [ ] Low

## Suspected Cause / Notes

Discovered during feature review of issue #677 (`docs/features/active/2026-08-28-quickfiler-keyboard-hook-leaks-to-outlook-677/policy-audit.2026-08-28T12-31.md`, section 8, Deviation D-1). The executor worked around this by provisioning the two missing package versions into the gitignored `packages/` directory (no tracked file changed) so the #677 fix's own baseline could proceed; that workaround is per-worktree and does not fix the underlying skew. Confirmed via `git status --porcelain` that no `packages.config` or `.csproj` file was altered as part of that workaround. This blocks a clean analyzer baseline for every future contributor starting from a fresh worktree/clone until the 16 `.csproj` files' `<Analyzer Include>` HintPaths are updated to match their `packages.config` versions (or vice versa).

## Proposed Fix / Validation Ideas

- [ ] Unit coverage areas: N/A (build-configuration fix, no runtime code)
- [ ] Integration scenario to retest: fresh clean-worktree `dotnet tool restore` + NuGet restore + analyzer-baseline msbuild command succeeds with zero `CS0006` errors
- [ ] Manual verification notes: confirm the 16 affected `.csproj` files' `<Analyzer Include>` HintPaths match their `packages.config` versions after the fix

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
