# Batch G — Per-File Nullable Pragma Gate

Timestamp: 2026-07-19T05-40

Command: `msbuild UtilitiesCS/UtilitiesCS.csproj -t:Rebuild -p:Configuration=Debug -p:Platform=AnyCPU -p:TreatWarningsAsErrors=true -p:WarningsNotAsErrors=CS0649;CS0618;CS0168` (WITHOUT `/p:Nullable=enable`)

EXIT_CODE: 0

Output Summary: Build succeeded. 0 Error(s). CS86xx count: 0 for the full Batch A–G pragma-enabled set (Performance/BayesianMetricTypes.cs, BayesianSerializationHelper.cs, BayesianPerformanceMeasurement.cs plus Batches A–F). AC1 satisfied through Batch G. `Performance/` was confirmed in scope at P0-T6. `/p:Nullable=enable` not passed.
