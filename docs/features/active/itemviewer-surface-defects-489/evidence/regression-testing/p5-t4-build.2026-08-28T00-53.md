# P5-T4 — QuickFiler.Test project build after the Phase 5 RED tests

Timestamp: 2026-08-28T00-53
Command: msbuild QuickFiler.Test\QuickFiler.Test.csproj /t:Rebuild /m /p:Configuration=Debug /p:Platform=AnyCPU
EXIT_CODE: 0
ExpectedExitCode: 0

## Acceptance

`EXIT_CODE: 0`. `Build succeeded.` with `3 Warning(s)` and `0 Error(s)` in 7.53 seconds. All three
warnings are the pre-existing `System.Reactive` `packages.config` advisory raised by
`packages\System.Reactive.7.0.0\build\System.Reactive.PackagesConfigCheck.targets(31,5)`, one each
for `UtilitiesCS`, `ToDoModel` and `QuickFiler`. A count of lines matching
`: (warning|error) CS[0-9]+` over the full build output returns **0**, so there is no `CS`
diagnostic of any kind.

## The platform spelling had to be corrected for a project-level invocation

PlanCommandAsWritten: msbuild QuickFiler.Test\QuickFiler.Test.csproj /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU"
PlanCommandAsWrittenExitCode: 1

This is the third and final occurrence in this batch of the carry-forward defect first recorded and
corrected at P1-T5 and repeated at P3-T2. Run exactly as the plan prints it, the build fails in 0.08
seconds having invoked no compiler:

```
Microsoft.Common.CurrentVersion.targets(843,5): error : The BaseOutputPath/OutputPath property is
not set for project 'QuickFiler.Test.csproj'. Please check to make sure that you have specified a
valid combination of Configuration and Platform for this project. Configuration='Debug'
Platform='Any CPU'.
```

It reported `0 Warning(s)` and `1 Error(s)`. `QuickFiler.Test/QuickFiler.Test.csproj` declares its
platform as `AnyCPU` with no space at `:12` and guards its `Debug` output path on
`Condition=" '$(Configuration)|$(Platform)' == 'Debug|AnyCPU' "` at `:32`. The spaced form
`Any CPU` is the solution-level spelling, which MSBuild normalises to `AnyCPU` only when driving a
`.sln`.

The correction is confined to that one token; no property was added or removed and
`/p:Nullable=enable` was not introduced. Both runs are recorded above so the substitution is
auditable. The solution-level build at P6-T4 keeps the spaced form verbatim.

## What was compiled

The Phase 5 RED and pin tests all compile:

- `QuickFiler.Test/Controllers/QfcItemController.ThemeMarshallingTests.cs` (new, 126 lines, 3
  `[TestMethod]`: the two #489 D2 REDs plus the not-required pin), newly referenced by the
  `<Compile Include>` entry P5-T2 appended to the project file.
- `QuickFiler.Test/Viewers/ItemViewerBreadcrumbDropDownContractTests.cs` (3 appended `[TestMethod]`,
  now 12 at 279 lines): the #489 D4 RED `IItemViewer_DeclaresNoUiSchedulerMember` and the two
  over-deletion pins `IItemViewer_StillDeclaresUiDispatcher` and
  `IItemViewer_StillDeclaresUiSyncContext`.

That the new theme-marshalling file compiles confirms the `<Compile Include>` entry resolves and that
its consumption of the 493-owned `QfcItemControllerTestSupport.BuildSyncDispatcher()` and
`HarnessController` binds correctly without editing `QfcItemController.TestSupport.cs`, which remains
unmodified against `BASELINE_SHA`.

Output Summary: The `QuickFiler.Test` project rebuilds at `EXIT_CODE: 0` with `Build succeeded.`,
`3 Warning(s)` and `0 Error(s)`; all three are the pre-existing `System.Reactive` `packages.config`
advisory and there is no `CS` diagnostic. The plan's printed command carries the same solution-level
platform spelling defect corrected at P1-T5 and P3-T2: run verbatim it exits `1` at
`Microsoft.Common.CurrentVersion.targets(843,5)` and compiles nothing. Both exit codes are recorded.
With the single-token substitution to `/p:Platform=AnyCPU` the build succeeds and all six new
`[TestMethod]` members across the two Phase 5 files compile.
