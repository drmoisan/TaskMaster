# P1-T5 — QuickFiler.Test project build after the Phase 1 RED tests

Timestamp: 2026-08-28T00-24
Command: msbuild QuickFiler.Test\QuickFiler.Test.csproj /t:Rebuild /m /p:Configuration=Debug /p:Platform=AnyCPU
EXIT_CODE: 0
ExpectedExitCode: 0

## Acceptance

`EXIT_CODE: 0`. `Build succeeded.` with `3 Warning(s)` and `0 Error(s)`. All three warnings are the
pre-existing `System.Reactive` `packages.config` advisory raised by
`packages\System.Reactive.7.0.0\build\System.Reactive.PackagesConfigCheck.targets`, one each for
`UtilitiesCS`, `ToDoModel` and `QuickFiler` — the same advisory recorded at the Phase 0 analyzer
and nullable baselines. There is no `CS` diagnostic of any kind.

## The platform spelling had to be corrected for a project-level invocation

PlanCommandAsWritten: msbuild QuickFiler.Test\QuickFiler.Test.csproj /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU"
PlanCommandAsWrittenExitCode: 1

The command exactly as the plan prints it **cannot** succeed and fails before compiling anything:

```
Microsoft.Common.CurrentVersion.targets(843,5): error : The BaseOutputPath/OutputPath property is
not set for project 'QuickFiler.Test.csproj'. ... Configuration='Debug' Platform='Any CPU'. You may
be seeing this message because you are trying to build a project without a solution file, and have
specified a non-default Configuration or Platform that doesn't exist for this project.
```

`QuickFiler.Test/QuickFiler.Test.csproj` declares its platform as `AnyCPU` with no space — `:12`
defaults `<Platform>` to `AnyCPU`, and the only `Debug` output path is guarded by
`Condition=" '$(Configuration)|$(Platform)' == 'Debug|AnyCPU' "` at `:32`/`:36`. The spaced form
`Any CPU` is the **solution**-level spelling; MSBuild normalises it to `AnyCPU` only when it is
driving a `.sln`, which is why the repository's four policy toolchain commands, all of which target
`TaskMaster.sln`, correctly use `"/p:Platform=Any CPU"`. A project-level invocation receives the
literal string and matches no configuration.

The correction is confined to that one token. Everything else in the command — the project path,
`/t:Rebuild`, `/m`, `/p:Configuration=Debug` — is unchanged, no property was added, and
`/p:Nullable=enable` was not introduced. Both runs are recorded above so the substitution is
auditable. The same substitution will be required at P3-T2, P5-T4 and P7-T8, which print the same
project-level command; the solution-level builds at P2-T7, P4-T5, P6-T4, P8-T9, P9-T7, P11-T4 and
P11-T6 are unaffected and keep the spaced form verbatim.

## What was compiled

The Phase 1 RED and pin tests all compile:

- `QuickFiler.Test/Viewers/ToolStripMenuItemCbTests.cs` (new, 5 `[TestMethod]`)
- `QuickFiler.Test/Viewers/ItemViewerBreadcrumbDropDownContractTests.cs` (2 appended `[TestMethod]`, now 7)
- `QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.Part2.cs` (new, 2 `[TestMethod]`)
- `QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.cs` (`partial` modifier added)

The two-file partial split compiles cleanly, which confirms the `[TestClass]` attribute correctly
sits on the parent declaration only.

Output Summary: The `QuickFiler.Test` project rebuilds at `EXIT_CODE: 0` with `Build succeeded.`,
`3 Warning(s)` and `0 Error(s)`; all three warnings are the pre-existing `System.Reactive`
`packages.config` advisory and there is no `CS` diagnostic. The plan's printed command uses the
solution-level platform spelling `"/p:Platform=Any CPU"`, which a project-level invocation cannot
match because `QuickFiler.Test.csproj` declares `AnyCPU` at `:12` and guards its `Debug` output path
on `Debug|AnyCPU` at `:32`; run verbatim it fails at `Microsoft.Common.CurrentVersion.targets(843,5)`
with `EXIT_CODE: 1` and compiles nothing. The single-token substitution to `/p:Platform=AnyCPU` is
recorded here with both exit codes, and the five new `[TestMethod]` members plus the partial-class
split compile.
