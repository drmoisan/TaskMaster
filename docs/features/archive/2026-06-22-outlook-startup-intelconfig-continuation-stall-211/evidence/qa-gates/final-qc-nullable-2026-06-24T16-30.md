# Final QC — Nullable / TreatWarningsAsErrors Build (issue #211, Phase 3.6)

Timestamp: 2026-06-24T16-30
Command: `msbuild TaskMaster.sln -t:Build -p:Configuration=Debug -p:Platform="Any CPU" -p:Nullable=enable -p:TreatWarningsAsErrors=true`
EXIT_CODE: 0

Output Summary:
- Solution-level `-t:Build` nullable/TWAE: Build succeeded, 0 Warning(s), 0 Error(s).
- Touched-code verification: a forced `-t:Rebuild` of `UtilitiesCS.csproj` and `TaskMaster.csproj`
  under nullable/TWAE surfaces 84 pre-existing errors, ALL confined to the vendored projects
  `SVGControl` and `UtilitiesSwordfish.NET.General` (files: `Collections\*.cs`, `Svg*.cs`,
  `DropDownEditor.cs`). This matches the documented repo baseline (vendored projects carry ~84
  pre-existing nullable errors only surfaced under `-t:Rebuild`).
- NONE of the touched/new first-party files produced a nullable error. Grep for
  `StoreWrapperInitClock`, `StoreWrapperInitProbe`, `StoreWrapper.cs`, `ApplicationGlobals.cs`,
  `StartupDiagnosticsProbe.cs` in the error set returned no matches. The Phase 3.6 production code
  is nullable-clean.
- After the forced rebuild, a plain `-t:Build -p:Configuration=Debug` restored the Debug test DLLs
  (UtilitiesCS.Test.dll, TaskMaster.Test.dll present) before the test step.
- Final nullable/TWAE state for touched code: PASS. No files changed by the build (no loop restart).
