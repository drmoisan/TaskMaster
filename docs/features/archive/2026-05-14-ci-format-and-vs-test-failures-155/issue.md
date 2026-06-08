# ci-format-and-vs-test-failures (Issue #155)

- Date captured: 2026-05-14
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/ci-format-and-vs-test-failures/ (Issue #155)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #155
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/155
- Last Updated: 2026-05-14
- Work Mode: full-bug

## Summary

CI run #156 fails at the "Verify formatting" step because committed git merge-conflict
artifact files are invalid, and `TaskMaster.csproj` lacks a trailing newline. Separately,
several MSTest unit tests fail under the Visual Studio / `vstest.console.exe` runner
(including CI) while passing under the VS Code runner.

## Environment

- OS/version: Windows 11 Pro 10.0.26200; CI runner windows-latest (Windows Server 2025)
- Toolchain: .NET Framework / VSTO classic projects; CSharpier (latest); MSBuild; vstest.console.exe
- Command/flags used: `csharpier check .` (CI "Verify formatting"); MSTest run via Visual Studio and vstest.console.exe
- Data source or fixture: TaskMaster.sln solution; UtilitiesCS.Test, ToDoModel.Test, and related test assemblies

## Steps to Reproduce

1. Run the CI workflow `.github/workflows/ci.yml` (or `csharpier check .` locally) — the "Verify formatting" step exits with code 1.
2. Open `TaskMaster.sln` in Visual Studio and run the full MSTest suite, or run `vstest.console.exe` against the built test assemblies.
3. Observe the failing tests listed under Actual Behavior, which pass when run from the VS Code test runner.

## Expected Behavior

`csharpier check .` passes with exit code 0, and the full MSTest suite passes identically
under the Visual Studio runner, `vstest.console.exe` (CI), and the VS Code runner.

## Actual Behavior

CI #156 "Verify formatting" output:

```
Warning The csproj at ...\TaskMaster\TaskMaster_BACKUP_1250.csproj failed to load with the
  following exception Name cannot begin with the '<' character, hexadecimal value 0x3C.
  Line 471, position 2.
Warning .\TaskMaster\TaskMaster_BACKUP_1250.csproj - Appeared to be invalid xml so was not formatted.
Error .\TaskMaster\TaskMaster.csproj - Was not formatted.
  The file did not end with a single newline.
Checked 1054 files in 25617ms.
Error: Process completed with exit code 1.
```

Visual-Studio-runner test failures (pass under VS Code):

- `TraceExtensions_Tests.GetCallerByName_ReturnsMatchingMethodAndHandlesEmptySpecialAndMissingNames` — `Expected object not to be <null>`.
- `TraceUtility_Tests.GetMethodCallLogString_ShouldIncludeCallerMethodAndParameters` — produced string contains `mscorlib.dll.InvokeMethod` instead of `CaptureMethodCallLogString`.
- `TraceUtility_Tests.GetMethodTraceString_ShouldIncludeCurrentCallChain` — produced string does not contain `CaptureMethodTraceString`.
- `AngleSharpParsedEmailBodyTests.ExtractLinks_StateUnderTest_ExpectedBehavior` and `FilterLinksByDomain_StateUnderTest_ExpectedBehavior` — `System.IO.FileLoadException: Could not load file or assembly 'System.Text.Encoding.CodePages, Version=10.0.0.7'`.
- `SCODictionary_Additional_Tests.Deserialize_WhenInvalidJsonSimpleOverloadAndPromptDisabled_SerializesCurrentState` — `Expected boolean to be True, but found False`.

## Logs / Screenshots

- [x] Attached minimal logs or screenshot
- Snippet: see Actual Behavior above; CI run #156 (databaseId 25676969961) step "Verify formatting".

## Impact / Severity

- [x] Blocker
- [ ] High
- [ ] Medium
- [ ] Low

CI is red on the `development` branch; the formatting gate blocks all downstream build,
analyzer, nullable, and test steps. The runner-dependent test failures undermine confidence
in the local Visual Studio test signal.

## Suspected Cause / Notes

- `TaskMaster/` contains four committed git mergetool artifacts: `TaskMaster_BACKUP_1250.csproj`,
  `TaskMaster_BASE_1250.csproj`, `TaskMaster_LOCAL_1250.csproj`, `TaskMaster_REMOTE_1250.csproj`
  (the `_BACKUP_` file still contains raw `<<<<<<< / ======= / >>>>>>>` markers). They were
  committed by a "WIP" commit and are not part of any solution.
- `TaskMaster/TaskMaster.csproj` does not end with a single newline.
- Additional stray, unreferenced backup files: `ToDoModel/ToDoItem_Backup.bkp`,
  `ToDoModel/Data Model/People/PeopleScoDictionaryNewBackup.cs`,
  `UtilitiesCS/EmailIntelligence/People/PeopleScoDictionaryNewBackup.cs` (none referenced by any `.csproj`).
- AngleSharp `FileLoadException`: `UtilitiesCS.csproj` references `System.Text.Encoding.CodePages`
  Version 10.0.0.7 (package 10.0.7) while `UtilitiesCS.Test.csproj` still references Version 10.0.0.5
  (package 10.0.5) — the recent "Upgrade Nuget Packages" commit missed the test project.
- Trace tests assert on the runtime call stack, which differs between the VS test host and the
  VS Code runner (reflection `InvokeMethod` frame, method inlining).
- SCODictionary deserialize test: root cause under investigation (test isolation vs. product defect).

## Proposed Fix / Validation Ideas

- [ ] Remove the four merge-conflict `.csproj` artifacts and the three stray backup files; confirm no `.csproj` references them.
- [ ] Add a trailing newline to `TaskMaster.csproj`; re-run `csharpier check .` to green.
- [ ] Align every project consuming `UtilitiesCS` to `System.Text.Encoding.CodePages` 10.0.7 (packages.config, csproj Reference + HintPath, app.config binding redirect).
- [ ] Make Trace caller-resolution and/or its tests runner-independent; add deterministic regression coverage.
- [ ] Diagnose and fix the SCODictionary deserialize runner dependency; add regression coverage.
- [ ] Validation: `csharpier check .` green; full MSTest suite green under `vstest.console.exe`.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [x] Move to active fix folder / branch