# utilitiescs-test-cs2002-duplicate-compile-entry (Issue #394)

- Date captured: 2026-07-20
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/utilitiescs-test-cs2002-duplicate-compile-entry/ (Issue #394)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #394
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/394
- Last Updated: 2026-07-21
## Summary

`UtilitiesCS.Test.csproj` contains two identical `<Compile Include="OutlookObjects\Folder\PercentageFormatterTests.cs" />` items (lines 288 and 338), producing compiler warning CS2002 ("Source file ... specified multiple times") on every build of the test project.

## Environment

- OS/version: Windows (any); also reproduces on the `windows-latest` CI runner
- Toolchain: MSBuild, TaskMaster.sln, Configuration=Debug, Platform=Any CPU
- Command/flags used: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"`
- Data source or fixture: none

## Steps to Reproduce

1. Build the solution: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"`.
2. Observe the compiler output for `UtilitiesCS.Test.csproj`.

## Expected Behavior

The build completes without CS2002; each source file appears exactly once in the project's `<Compile>` item group.

## Actual Behavior

`CSC : warning CS2002: Source file 'C:\...\UtilitiesCS.Test\OutlookObjects\Folder\PercentageFormatterTests.cs' specified multiple times [UtilitiesCS.Test.csproj]`

## Logs / Screenshots

- [x] Attached minimal logs or screenshot
- Snippet: observed in local toolchain runs on 2026-07-20 during PR #391 verification (analyzer build and TreatWarningsAsErrors rebuild).

## Impact / Severity

- [ ] Blocker
- [ ] High
- [ ] Medium
- [x] Low

Build warning noise only; the duplicate entry does not currently fail any gate because CS2002 is not promoted to an error in the affected configuration. It risks masking real warnings and could break the build if warning promotion rules change.

## Suspected Cause / Notes

Likely a merge artifact: two `<ItemGroup>` sections in `UtilitiesCS.Test.csproj` each include the same file (lines 288 and 338 as of commit 443a1a52).

## Proposed Fix / Validation Ideas

- [ ] Remove one of the duplicate `<Compile>` items from `UtilitiesCS.Test.csproj`.
- [ ] Rebuild and confirm CS2002 no longer appears.
- [ ] Confirm `PercentageFormatterTests` still runs (test count unchanged) via vstest.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
