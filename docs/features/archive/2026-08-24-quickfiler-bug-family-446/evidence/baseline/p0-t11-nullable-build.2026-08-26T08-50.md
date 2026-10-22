# [P0-T11] Baseline Nullable / Type-Check Gate

Timestamp: 2026-08-26T08-50

Task: [P0-T11]
Feature: docs/features/active/quickfiler-bug-family-446

Command: `& $msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU" /p:TreatWarningsAsErrors=true`

Executed through `pwsh -NoProfile`. `$msbuild` was resolved at run time with the plan's vswhere
prelude to the Visual Studio 18 Community full-framework MSBuild.

EXIT_CODE: 0

## Command-Shape Assertions

The recorded command text above contains neither `Nullable=enable` nor `/t:Build`:

- No `/p:Nullable=enable`. Per D-Plan-6 and `.claude/rules/csharp.md`, no project in this
  repository carries a `<Nullable>` element and there is no `Directory.Build.props`, so the
  property is a solution-wide opt-in that CI deliberately omits.
- `/t:Rebuild` is used, not `/t:Build`. MSBuild's up-to-date check does not invalidate on a
  command-line `/p:` change, so a warm `/t:Build` would return exit 0 with `CoreCompile` skipped
  and the gate could not fail.

## MSBuild Summary Counts

- Error count: `0`
- Warning count: `5`

## Output Summary

Baseline nullable / type-check build succeeded with `5 Warning(s)` and `0 Error(s)`, elapsed
00:00:15.77. The five warnings are the same pre-existing
`System.Reactive.PackagesConfigCheck.targets(31,5)` packages.config diagnostic recorded by
`[P0-T10]`; because it is emitted with a bare `warning` category rather than a `CS`/analyzer
code, `/p:TreatWarningsAsErrors=true` did not promote it and the build stayed green.
No `CS86xx` nullable diagnostic was reported. The gate is green at the merge base.
