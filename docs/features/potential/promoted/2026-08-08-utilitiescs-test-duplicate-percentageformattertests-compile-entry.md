# utilitiescs-test-duplicate-percentageformattertests-compile-entry (Issue #510)

- Date captured: 2026-08-08
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/utilitiescs-test-duplicate-percentageformattertests-compile-entry/ (Issue #510)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #510
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/510
- Last Updated: 2026-08-08
## Summary

`UtilitiesCS.Test/UtilitiesCS.Test.csproj` lists `OutlookObjects\Folder\PercentageFormatterTests.cs` twice, producing MSBuild warning **CS2002** ("Source file specified multiple times"). The duplicate is long-standing and unrelated to any single feature branch.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Runtime: .NET Framework 4.8.1, legacy non-SDK project (`ToolsVersion="15.0"`, `packages.config`)
- Command/flags used: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
- Data source or fixture: `UtilitiesCS.Test/UtilitiesCS.Test.csproj`

## Steps to Reproduce

1. From the repository root, run the analyzer build command above.
2. Search the build output for `CS2002`.
3. Inspect `UtilitiesCS.Test/UtilitiesCS.Test.csproj` and search for `PercentageFormatterTests.cs`.

## Expected Behavior

Each source file appears exactly once in the project's `<Compile Include>` item group, and the build emits no CS2002 warning.

## Actual Behavior

The file is included twice. Verified on 2026-08-08 on branch `bug/quickfiler-search-keystroke-focus-steal-438`:

```
304:    <Compile Include="OutlookObjects\Folder\PercentageFormatterTests.cs" />
356:    <Compile Include="OutlookObjects\Folder\PercentageFormatterTests.cs" />
```

The same duplicate is present at the merge-base commit `003c5715` (at lines 302 and 354 there), confirming it predates that branch.

## Logs / Screenshots

- [x] Attached minimal logs or screenshot
- Snippet: the two `<Compile Include>` lines above, from `grep -n "PercentageFormatterTests" UtilitiesCS.Test/UtilitiesCS.Test.csproj`.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [ ] Medium
- [x] Low

The build succeeds and tests run, so there is no functional defect today. The cost is a persistent warning in every build of the solution, which erodes the signal value of build output. It also carries a latent risk: the repository's type-check stage runs with `/p:TreatWarningsAsErrors=true`, and any future change that promotes CS2002 to error severity would break that protected gate.

## Suspected Cause / Notes

Observed during orchestration of issue #438 on 2026-08-08 and independently confirmed at merge-base.

- Legacy non-SDK projects enumerate every file explicitly, so a file added by two separate edits (or a merge that resolved by keeping both sides) yields a duplicate entry with no automatic deduplication.
- A related potential entry already exists at `docs/features/potential/promoted/2026-07-20-utilitiescs-test-cs2002-duplicate-compile-entry.md`. Confirm whether that entry covers this same duplicate before opening new work, and consolidate rather than duplicating the tracking.

## Proposed Fix / Validation Ideas

- [ ] Unit coverage areas: not applicable — this is a project-file correction with no runtime behavior.
- [ ] Integration scenario to retest: run the analyzer build and confirm zero CS2002 occurrences in the output; run the `UtilitiesCS.Test` suite and confirm the `PercentageFormatterTests` test count is unchanged.
- [ ] Manual verification notes: remove the second `<Compile Include>` entry only. Do not reorder or otherwise reformat the item group, so the diff stays reviewable. Consider a small repository check that fails when any `.csproj` lists the same `Include` path more than once, since this class of defect recurs in legacy non-SDK projects.

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
