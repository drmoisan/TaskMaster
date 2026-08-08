# MSBuild Nullable / Type-Check Gate Baseline — Issue #503 (P0-T8)

Timestamp: 2026-08-08T13-10

Command:
```
pwsh -NoProfile -Command "Set-Location 'C:\Users\DanMoisan\repos\TaskMaster-wt\2026-08-08T11-55'; & 'C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe' TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:Nullable=enable /p:TreatWarningsAsErrors=true; Write-Host \"EXIT_CODE=$LASTEXITCODE\""
```

EXIT_CODE: 0

Output Summary:

- Result: `Build succeeded.`
- Error count: **0**
- Warning count: **5**
- Elapsed: 00:00:01.65

The 5 warnings are the identical pre-existing System.Reactive 7.0.0 `packages.config` packaging notices recorded in the P0-T7 baseline. They are emitted by an MSBuild `.targets` file rather than the compiler, so `/p:TreatWarningsAsErrors=true` does not promote them. Zero `CS86xx` nullable-flow diagnostics appear.

Measured value matches the plan's expected merge-base value of EXIT 0. This is the comparison basis for P6-T5.
