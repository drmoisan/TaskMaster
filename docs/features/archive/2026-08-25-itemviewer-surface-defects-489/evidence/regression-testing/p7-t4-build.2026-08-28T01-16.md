# P7-T4 — QuickFiler.Test project build after the Phase 7 runtime RED tests

Timestamp: 2026-08-28T01-16
Command: msbuild QuickFiler.Test\QuickFiler.Test.csproj /t:Rebuild /m /p:Configuration=Debug /p:Platform=AnyCPU
EXIT_CODE: 0
ExpectedExitCode: 0

## Acceptance

`EXIT_CODE: 0`. `Build succeeded.` with `3 Warning(s)` and `0 Error(s)` in 6.93 seconds. All three
warnings are the pre-existing `System.Reactive` `packages.config` advisory raised by
`packages\System.Reactive.7.0.0\build\System.Reactive.PackagesConfigCheck.targets(31,5)`, one each
for `UtilitiesCS`, `ToDoModel` and `QuickFiler`. A count of lines matching
`: (warning|error) CS[0-9]+` over the full build output returns **0**, so there is no `CS`
diagnostic of any kind.

## The platform spelling had to be corrected for a project-level invocation

PlanCommandAsWritten: msbuild QuickFiler.Test\QuickFiler.Test.csproj /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU"
PlanCommandAsWrittenExitCode: 1

This is the fourth occurrence of the carry-forward defect first recorded and corrected at P1-T5 and
repeated at P3-T2 and P5-T4. Run exactly as the plan prints it, the build fails in 0.18 seconds
having invoked no compiler:

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
`.sln`. Both runs are recorded so the substitution is auditable; the correction is confined to that
one token, no property was added or removed and `/p:Nullable=enable` was not introduced.

## What this build proves compiles

- `QuickFiler.Test/Controllers/QfcItemController.MailActionsTests.Part2.cs` (new, 107 lines, 2
  `[TestMethod]`: the two #490 D4 REDs), newly referenced by the fourth and last `<Compile Include>`
  entry this plan appends, `Controllers\QfcItemController.MailActionsTests.Part2.cs`.
- The parent `QuickFiler.Test/Controllers/QfcItemController.MailActionsTests.cs` still compiles with
  its declaration changed in place to `public partial class QfcItemController_MailActionsTests`, so
  the two files bind as one class and the Part2 members reach the parent's private `MailController`
  nested type and its private `SetField` helper directly.
- `QuickFiler.Test/Viewers/ItemViewerBreadcrumbDropDownContractTests.cs` (2 appended `[TestMethod]`,
  now 14 at 325 lines): the #490 D1 RED `IItemViewer_DeclaresAddFolderItemsAndNotSetFolderItems` and
  the #490 D3 RED `IItemViewer_FocusSubjectReturnsBool`. Both are reflection assertions over
  `typeof(IItemViewer)`, so they compile against the current interface and fail at run time; the
  compile-time RED for #490 D3 is P7-T7 and P7-T8, which follow.

Output Summary: The `QuickFiler.Test` project rebuilds at `EXIT_CODE: 0` with `Build succeeded.`,
`3 Warning(s)` and `0 Error(s)`; all three are the pre-existing `System.Reactive` `packages.config`
advisory and there is no `CS` diagnostic. The plan's printed command carries the same solution-level
platform spelling defect corrected at P1-T5, P3-T2 and P5-T4: run verbatim it exits `1` at
`Microsoft.Common.CurrentVersion.targets(843,5)` and compiles nothing. Both exit codes are recorded.
With the single-token substitution to `/p:Platform=AnyCPU` the build succeeds and all four new
`[TestMethod]` members across the two Phase 7 files compile.
