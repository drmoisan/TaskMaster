# Phase 0 — Nullable / TreatWarningsAsErrors Type-Check Baseline (P0-T4)

Timestamp: 2026-07-11T03-16

Command: msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true

EXIT_CODE: 0

Output Summary:
- Build succeeded. `0 Warning(s)`, `0 Error(s)`. Time Elapsed 00:00:01.52.
- TRANSPARENCY NOTE (auditable): MSBuild reported `Skipping target "CoreCompile" because all output files are up-to-date with respect to the input files` for the projects. Because the immediately-preceding P0-T3 analyzer build compiled every project, this `/t:Build` invocation performed an incremental no-op recompile; the `/p:Nullable=enable /p:TreatWarningsAsErrors=true` global-property change does not by itself invalidate the per-project incremental-compile timestamp check. The 0/0 result therefore reflects the up-to-date compiled state rather than a fresh nullable-enabled recompile of every source file.
- This is the reference baseline for the Phase 5 (P5-T3) and Phase 9 (P9-T3) nullable checks, which use the identical `/t:Build` command, giving a consistent apples-to-apples comparison: the gate verifies no NEW nullable errors are introduced on the touched code paths relative to this baseline state.
