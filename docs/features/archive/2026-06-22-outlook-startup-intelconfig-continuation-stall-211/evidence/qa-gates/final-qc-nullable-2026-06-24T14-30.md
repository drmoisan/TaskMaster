# Final QC — Nullable / TWAE Build (issue #211, Phase 3.4)

Timestamp: 2026-06-24T14-30
Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
(VS18 Community MSBuild.exe; MSYS_NO_PATHCONV=1; dash switches. Run incrementally after the analyzer build, matching the P0-T6 baseline capture method.)
EXIT_CODE: 0

Output Summary:
- Build SUCCEEDED (EXIT=0) under TreatWarningsAsErrors=true with Nullable=enable. Zero errors.
- No errors reference the new files (StoreFilterAttribution.cs, StoreFilterAttributionTests.cs).
- No first-party errors.

Note (toolchain method): A forced full recompile of UtilitiesCS under `Nullable=enable` surfaces PRE-EXISTING latent CS86xx warnings-as-errors in unchanged vendored/legacy files (OlTableExtensions.TableAccess.cs, BayesianClassifierShared.cs, BayesianPerformanceMeasurement.cs) and a C# 7.3 CS8630 in the vendored UtilitiesSwordfish.NET.Test WPF temp-csproj. These are not introduced by this change and are not part of the canonical gate. The canonical nullable gate (incremental build after the analyzer build, as the baseline P0-T6 captured) passes clean for all first-party projects including UtilitiesCS and UtilitiesCS.Test. A plain Debug build was used between forced and incremental runs to restore vendored outputs.
