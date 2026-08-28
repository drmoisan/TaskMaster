# P3-T2 — QuickFiler.Test project build after the Phase 3 RED tests

Timestamp: 2026-08-28T00-40
Command: msbuild QuickFiler.Test\QuickFiler.Test.csproj /t:Rebuild /m /p:Configuration=Debug /p:Platform=AnyCPU
EXIT_CODE: 0
ExpectedExitCode: 0

## Acceptance

`EXIT_CODE: 0`. `Build succeeded.` with `3 Warning(s)` and `0 Error(s)`. All three warnings are the
pre-existing `System.Reactive` `packages.config` advisory raised by
`packages\System.Reactive.7.0.0\build\System.Reactive.PackagesConfigCheck.targets(31,5)`, one each
for `UtilitiesCS`, `ToDoModel` and `QuickFiler` — the same advisory recorded at the Phase 0 analyzer
and nullable baselines and at P1-T5. A count of lines matching `: (warning|error) CS[0-9]+` over the
full build output returns **0**, so there is no `CS` diagnostic of any kind.

## The platform spelling had to be corrected for a project-level invocation

PlanCommandAsWritten: msbuild QuickFiler.Test\QuickFiler.Test.csproj /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU"
PlanCommandAsWrittenExitCode: 1

This is the same carry-forward defect P1-T5 recorded and corrected; P3-T2 prints the identical
project-level command. Run exactly as the plan prints it, the build fails before compiling anything:

```
Microsoft.Common.CurrentVersion.targets(843,5): error : The BaseOutputPath/OutputPath property is
not set for project 'QuickFiler.Test.csproj'. Please check to make sure that you have specified a
valid combination of Configuration and Platform for this project. Configuration='Debug'
Platform='Any CPU'. You may be seeing this message because you are trying to build a project without
a solution file, and have specified a non-default Configuration or Platform that doesn't exist for
this project.
```

It reported `0 Warning(s)` and `1 Error(s)` in 0.08 seconds, having invoked no compiler.
`QuickFiler.Test/QuickFiler.Test.csproj` declares its platform as `AnyCPU` with no space: `:12`
defaults `<Platform>` to `AnyCPU`, and the `Debug` output path is guarded by
`Condition=" '$(Configuration)|$(Platform)' == 'Debug|AnyCPU' "` at `:32`. The spaced form
`Any CPU` is the **solution**-level spelling, normalised to `AnyCPU` only when MSBuild is driving a
`.sln`; a project-level invocation receives the literal string and matches no configuration.

The correction is confined to that one token. The project path, `/t:Rebuild`, `/m` and
`/p:Configuration=Debug` are unchanged, no property was added, and `/p:Nullable=enable` was not
introduced. Both runs are recorded above so the substitution is auditable. The remaining
project-level command that prints the same defect is P5-T4, later in this batch; the solution-level
builds at P4-T5 and P6-T4 are unaffected and keep the spaced form verbatim.

## What was compiled

The two Phase 3 RED tests appended by P3-T1 compile:

- `QuickFiler.Test/Viewers/ItemViewerBreadcrumbDropDownContractTests.cs` — 2 appended `[TestMethod]`
  members, `ItemViewer_DeclaresNoParentChangedHandler` and
  `ItemViewerExpanded_DeclaresNoParentChangedHandler`, taking the file from 7 to 9 `[TestMethod]`
  attributes at 231 lines.

Both compile against the production surface as it currently stands, which still declares the
handlers at `QuickFiler/Viewers/ItemViewer.cs:166` and `QuickFiler/Viewers/ItemViewerExpanded.cs:154`
— that is why they are expected to fail at P3-T3 and not to fail to build.

Output Summary: The `QuickFiler.Test` project rebuilds at `EXIT_CODE: 0` with `Build succeeded.`,
`3 Warning(s)` and `0 Error(s)`; all three are the pre-existing `System.Reactive` `packages.config`
advisory and there is no `CS` diagnostic. The plan's printed command carries the same solution-level
platform spelling defect corrected at P1-T5: run verbatim it exits `1` at
`Microsoft.Common.CurrentVersion.targets(843,5)` with `BaseOutputPath/OutputPath property is not set`
and compiles nothing, because the project declares `AnyCPU` at `:12` and guards `Debug` output on
`Debug|AnyCPU` at `:32`. Both exit codes are recorded. The single-token substitution to
`/p:Platform=AnyCPU` makes the build succeed and the two new `[TestMethod]` members compile.
