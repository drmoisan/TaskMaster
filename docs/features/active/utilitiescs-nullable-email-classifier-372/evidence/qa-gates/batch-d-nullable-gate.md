# Batch D — Per-File Nullable Pragma Gate

Timestamp: 2026-07-19T03-00

Command: `msbuild UtilitiesCS/UtilitiesCS.csproj -t:Rebuild -p:Configuration=Debug -p:Platform=AnyCPU -p:TreatWarningsAsErrors=true -p:WarningsNotAsErrors=CS0649;CS0618;CS0168` (WITHOUT `/p:Nullable=enable`)

EXIT_CODE: 0

Output Summary: Build succeeded. 0 Error(s). CS86xx count: 0 for the Batch A–D pragma-enabled files (TristateEngine.cs, ConditionalItemEngine.cs, MulticlassEngine.cs, ManagerAsyncLazy.cs, ClassifierGroupUtilities.cs plus Batches A/B/C). AC1 satisfied through Batch D. `/p:Nullable=enable` not passed.
