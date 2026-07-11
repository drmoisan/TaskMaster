# Baseline — Nullable / Type-Check (P0-T5)

- **Timestamp:** 2026-07-11T12-52
- **Command:** `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true` (MSBuild.exe from VS18 Community; `MSYS_NO_PATHCONV=1`)
- **EXIT_CODE:** 0
- **Output Summary:** `Build succeeded. 0 Warning(s), 0 Error(s).` Under `/p:Nullable=enable` the nullable-annotation context is enabled, which resolves the CS8632 warnings observed in the analyzer baseline. This invocation was incremental (outputs up-to-date from the P0-T4 build; elapsed 0.93s). The genuine nullable/type-check gate is re-exercised at final QC (P5-T3), where the ProjectReference/`.sln`/file deletions invalidate the up-to-date checks and force a real recompile of the affected first-party projects. Baseline state: green (EXIT 0).
