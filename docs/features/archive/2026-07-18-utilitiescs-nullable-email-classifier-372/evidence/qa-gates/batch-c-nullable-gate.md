# Batch C — Per-File Nullable Pragma Gate

Timestamp: 2026-07-19T02-00

Command: `msbuild UtilitiesCS/UtilitiesCS.csproj -t:Rebuild -p:Configuration=Debug -p:Platform=AnyCPU -p:TreatWarningsAsErrors=true -p:WarningsNotAsErrors=CS0649;CS0618;CS0168` (WITHOUT `/p:Nullable=enable`)

EXIT_CODE: 0

Output Summary: Build succeeded. 0 Error(s). CS86xx count: 0 for the Batch A+B+C pragma-enabled files (BayesianClassifierShared.cs, BayesianClassifierGroup.cs, PerParentClassifier.cs, FolderHierarchyTree.cs plus Batches A/B). AC1 satisfied through Batch C. `/p:Nullable=enable` not passed.
