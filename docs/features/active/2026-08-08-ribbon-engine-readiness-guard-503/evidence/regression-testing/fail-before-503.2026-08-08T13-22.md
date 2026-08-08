# Fail-Before (Red) Evidence — Issue #503 (P1-T2) `[expect-fail]`

Timestamp: 2026-08-08T13-22

Command:
```
pwsh -NoProfile -Command "Set-Location 'C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55'; & 'C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe' TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU'; Write-Host \"EXIT_CODE=$LASTEXITCODE\""
```

EXIT_CODE: **1** (non-zero — this is the expected outcome for this `[expect-fail]` task)

## Output Summary — verbatim compiler diagnostics

```
Build FAILED.

C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55\TaskMaster.Test\Ribbon\EngineGatedCommandRunnerTests.cs(46,28): error CS0246: The type or namespace name 'EngineReadinessGate' could not be found (are you missing a using directive or an assembly reference?) [C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55\TaskMaster.Test\TaskMaster.Test.csproj]
C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55\TaskMaster.Test\Ribbon\EngineGatedCommandRunnerTests.cs(47,30): error CS0246: The type or namespace name 'EngineGatedCommandRunner' could not be found (are you missing a using directive or an assembly reference?) [C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55\TaskMaster.Test\TaskMaster.Test.csproj]
C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55\TaskMaster.Test\Ribbon\EngineGatedCommandRunnerTests.cs(81,28): error CS0246: The type or namespace name 'EngineReadinessGate' could not be found (are you missing a using directive or an assembly reference?) [C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55\TaskMaster.Test\TaskMaster.Test.csproj]
C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55\TaskMaster.Test\Ribbon\EngineGatedCommandRunnerTests.cs(82,30): error CS0246: The type or namespace name 'EngineGatedCommandRunner' could not be found (are you missing a using directive or an assembly reference?) [C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55\TaskMaster.Test\TaskMaster.Test.csproj]

    5 Warning(s)
    4 Error(s)
```

## Binary outcome

**PASS (red proven).** The build exits non-zero, and every one of the four errors is a `CS0246` sourced from `TaskMaster.Test\Ribbon\EngineGatedCommandRunnerTests.cs` naming exactly the two guard types that do not exist at the merge-base: `EngineReadinessGate` and `EngineGatedCommandRunner`. The 5 warnings are the pre-existing System.Reactive `packages.config` notices recorded in the P0-T7 baseline. No other error of any kind appears, so the non-zero exit is caused solely by the absence of the guard types.

## Execution note

The first execution of this task's command produced only **one** diagnostic (`CS0246` for `EngineReadinessGate` at the signature of a private helper method), because Roslyn does not bind method bodies once a declaration signature fails to bind, and `EngineGatedCommandRunner` appeared only in method bodies. The helper's signature was therefore changed from `private static EngineReadinessGate CreateGateOver(...)` to `private static Func<IAppItemEngines> CreateEnginesAccessor(...)`, moving both guard-type references into method bodies. Both types now bind in the same phase and both required diagnostics surface. The two `[TestMethod]` members, their names, their arrangements, and their assertions are unchanged by that edit.
