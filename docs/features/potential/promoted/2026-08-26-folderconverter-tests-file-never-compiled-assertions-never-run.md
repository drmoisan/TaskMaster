# folderconverter-tests-file-never-compiled-assertions-never-run (Issue #627)

- Date captured: 2026-08-26
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/folderconverter-tests-file-never-compiled-assertions-never-run/ (Issue #627)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #627
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/627
- Last Updated: 2026-08-26
## Summary

`UtilitiesCS.Test/OutlookExtensions/FolderConverter_Tests.cs` exists on disk but has no
`<Compile Include>` entry in `UtilitiesCS.Test/UtilitiesCS.Test.csproj`. Because every project in
this repository is non-SDK `packages.config` style with no wildcard globbing, a source file without
an explicit include is simply not part of the compilation. The file's tests are therefore never
built and never run. The test project builds green, the suite reports a healthy pass count, and the
assertions in this file contribute nothing to it.

This is the silent-exclusion failure mode of explicit-include projects. There is no diagnostic: the
build does not warn, the test runner does not report the file as skipped, and a reader browsing the
repository sees what appears to be an active test file. The only way to notice is to check the
project file, or to observe that a `/TestCaseFilter` naming the class matches zero tests.

It was found exactly that way. During issue #614 work a filter alternation
`FullyQualifiedName~FolderConverter_Tests` was added to a verification step and matched zero tests.
Verified directly: `grep -c "FolderConverter_Tests.cs" UtilitiesCS.Test/UtilitiesCS.Test.csproj`
returns `0` while the file is present on disk.

The immediate risk is bounded. The assertions in the orphaned file are duplicated by a compiled and
currently green test, so no behavior is presently unverified because of it. The durable risk is not
bounded: any future edit to that file is a no-op that looks like work, any future assertion added to
it provides false assurance, and the same class of mistake silently disables any other test file
added without a project entry. A test file that cannot fail is worse than no test file, because it
is mistaken for coverage.

Recommended fix is to add the `<Compile Include>` entry, then run the class and fix whatever it
reports — the file has never been compiled against current production code, so it may not build or
pass without adjustment. If the duplication is judged redundant, delete the file outright rather
than leaving it in place uncompiled. Either resolution is acceptable; leaving it as-is is not.

Worth pairing with a guard: a check that every `.cs` file under a project directory appears in that
project's include list would catch this class of defect for both test and production code. The same
explicit-include fragility underlies issue #615, where `<Analyzer Include>` paths drifted from
`packages.config`.

## Environment

- OS/version: Windows 11 Pro 10.0.26200; .NET Framework 4.8.1; Visual Studio 18 Community MSBuild.
- Python version: Not applicable; this is C# / MSBuild project configuration.
- Command/flags used: `grep -c "FolderConverter_Tests.cs" UtilitiesCS.Test/UtilitiesCS.Test.csproj`
  and a `vstest.console.exe` run with `/TestCaseFilter:"FullyQualifiedName~FolderConverter_Tests"`.
- Data source or fixture: Repository source on the issue #614 branch.

## Steps to Reproduce

1. Confirm `UtilitiesCS.Test/OutlookExtensions/FolderConverter_Tests.cs` exists.
2. Search `UtilitiesCS.Test/UtilitiesCS.Test.csproj` for `FolderConverter_Tests.cs`; it returns zero
   hits.
3. Build the solution; it succeeds with no warning about the excluded file.
4. Run vstest with `/TestCaseFilter:"FullyQualifiedName~FolderConverter_Tests"`; zero tests match.

## Expected Behavior

Every `.cs` file under a project directory is either compiled by that project or absent from the
repository. A test file present in the tree runs in the suite, and a filter naming its class matches
at least one test.

## Actual Behavior

The file is silently excluded from the compilation. It builds nothing, runs nothing, and reports
nothing, while appearing to be an active test file.

## Logs / Screenshots

- [x] Attached minimal logs or screenshot
- Snippet: `grep -c "FolderConverter_Tests.cs" UtilitiesCS.Test/UtilitiesCS.Test.csproj` returns `0`;
  the file is present at `UtilitiesCS.Test/OutlookExtensions/FolderConverter_Tests.cs`.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

No behavior is currently unverified, because a compiled test duplicates the assertions. Severity is
Medium rather than Low because the file is indistinguishable from working coverage and will absorb
future edits that silently do nothing.

## Suspected Cause / Notes

- Missing `<Compile Include="OutlookExtensions\FolderConverter_Tests.cs" />` in
  `UtilitiesCS.Test/UtilitiesCS.Test.csproj`.
- Most likely a file added or moved without the corresponding project-file entry, which non-SDK
  projects do not detect.
- Same explicit-include fragility class as issue #615.

## Proposed Fix / Validation Ideas

- [ ] Add the `<Compile Include>` entry, build, run the class, and fix whatever it reports — it has
      never been compiled against current production code.
- [ ] Alternatively delete the file if its assertions are judged redundant with the compiled test.
- [ ] Add a repository guard that fails when a `.cs` file under a project directory is absent from
      that project's include list; this would also have caught issue #615.
- [ ] Unit coverage areas: whatever the file's tests cover once compiled.
- [ ] Integration scenario to retest: full suite; confirm the total test count increases by the
      number of tests the file contributes.
- [ ] Manual verification notes: re-run the `FullyQualifiedName~FolderConverter_Tests` filter and
      confirm it now matches a non-zero count.

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
