# Phase 1 — test-project wiring build

Timestamp: 2026-08-27T23-48
Task: [P1-T3]
Command: `& "<resolved MSBuild.exe>" "QuickFiler.Test\QuickFiler.Test.csproj" /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU /nologo /v:m` under `pwsh -NoProfile` from the worktree root
EXIT_CODE: 0

**This is an intermediate build, not an analyzer gate and not a nullable gate.** It uses `/t:Build`
deliberately, per decision D3, solely to produce a fresh `QuickFiler.Test.dll` containing the three new
files. No analyzer or nullable conclusion is drawn from it.

## Result

`QuickFiler.Test -> ...\QuickFiler.Test\bin\Debug\QuickFiler.Test.dll` — EXIT_CODE 0. The dependency
chain `Tags`, `ToDoModel`, `TaskVisualization`, `QuickFiler` all built. The only warnings emitted are the
identifier-less `System.Reactive` `packages.config` advisories already recorded in the Phase 0 baseline.

The three files added by `[P1-T1]` and wired by `[P1-T2]` compile: had any of the three `Compile Include`
paths been wrong, MSBuild would have failed with CS2001 rather than exiting 0.

## Recorded deviation — platform argument

`[P1-T3]` states the argument as `"/p:Platform=Any CPU"`. Run against the **project file** rather than the
solution, that value fails before compiling anything:

```
Microsoft.Common.CurrentVersion.targets(843,5): error : The BaseOutputPath/OutputPath property is not
set for project 'QuickFiler.Test.csproj'. ... Configuration='Debug'  Platform='Any CPU'.
```

`QuickFiler.Test.csproj` declares its platform as `AnyCPU` without a space
(`QuickFiler.Test.csproj:12`, `:32`, `:41`), and the `Any CPU` spelling with a space exists only in the
solution file's configuration mapping. The project-level invocation therefore uses `/p:Platform=AnyCPU`.

This substitution applies **only** to project-level intermediate builds. Both solution-level gate
commands — `[P10-T4]` and `[P10-T5]` — keep `"/p:Platform=Any CPU"` verbatim, because they target
`TaskMaster.sln`, where that is the correct value and where it is character-for-character CI's command.

Output Summary: QuickFiler.Test.csproj builds clean, EXIT_CODE 0, producing
QuickFiler.Test\bin\Debug\QuickFiler.Test.dll with the three new test files compiled in. The plan's
`Any CPU` platform spelling is invalid for a project-level build and was replaced with `AnyCPU`; the
solution-level gate commands are unaffected.
