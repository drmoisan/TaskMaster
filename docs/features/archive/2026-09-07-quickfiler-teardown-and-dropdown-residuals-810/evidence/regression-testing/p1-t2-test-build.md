# [P1-T2] Test-Assembly Build

Timestamp: 2026-09-08T09-44
Command: `msbuild QuickFiler.Test\QuickFiler.Test.csproj /t:Rebuild /m /p:Configuration=Debug /p:Platform=AnyCPU`
EXIT_CODE: 0
Output Summary: The direct-project rebuild of `QuickFiler.Test.csproj` succeeded with zero warnings and zero errors and produced `QuickFiler.Test\bin\Debug\QuickFiler.Test.dll`, which is the assembly every later vstest task in this plan runs against. The assembly contains the [P1-T1] regression test `ActionCancelAsync_SelfInflictedByOwnPopup_StillCancelsEverySelector`, so [P1-T3] can execute its fail-before run.

Verbatim `Build succeeded.` block:

```
Build succeeded.
    0 Warning(s)
    0 Error(s)

Time Elapsed 00:00:15.98
```

## Platform token (D20)

The command uses `/p:Platform=AnyCPU`, one word, no space and no quotes around the switch, as D20 requires for a direct-project build. `QuickFiler.Test/QuickFiler.Test.csproj` declares `Debug|AnyCPU` at `:32` carrying `<OutputPath>bin\Debug\</OutputPath>` at `:36`, and declares no `Debug|Any CPU` group; the spaced spelling is a solution-level platform name that MSBuild maps onto this project's GUID only when `TaskMaster.sln` is the entry point. The observed output path is `QuickFiler.Test\bin\Debug\`, which is what every later vstest task in this plan addresses.

The solution-scoped commands of this plan are unaffected and continue to use `"/p:Platform=Any CPU"` unchanged.

## D5 file-lock check

The build output was searched for `MSB3061` and `MSB3021`. Match count: 0. No Outlook or test-host process was holding the build output, so the D5 stop condition did not fire and no process was terminated.

## Prior blocked record

An earlier run of this task recorded a BLOCKED state, because the command as originally written used the solution-level `"/p:Platform=Any CPU"` token and failed in `_CheckForInvalidConfigurationAndPlatform` before reaching `CoreCompile`. That record is superseded by this one following the plan revision that introduced D20 and corrected the command.
