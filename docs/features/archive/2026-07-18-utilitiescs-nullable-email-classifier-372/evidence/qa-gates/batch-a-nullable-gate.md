# Batch A — Per-File Nullable Pragma Gate

Timestamp: 2026-07-19T01-00

Command: `msbuild UtilitiesCS/UtilitiesCS.csproj -t:Rebuild -p:Configuration=Debug -p:Platform=AnyCPU -p:TreatWarningsAsErrors=true -p:WarningsNotAsErrors=CS0649;CS0618;CS0168` (WITHOUT `/p:Nullable=enable`)

Command adaptation note: `Platform=AnyCPU` (standalone legacy-project OutputPath token) and `WarningsNotAsErrors=CS0649;CS0618;CS0168` (three pre-existing, non-CS86xx, out-of-scope warnings — vendored SVGControl CS0649 and UtilitiesCS obsolete-usage CS0618 / unused-local CS0168) are the established scoped-gate parameters from the P0-T5 baseline. None is a CS86xx code, so the nullable measurement is fully preserved: any CS86xx from a pragma-enabled file is still enforced as an error.

EXIT_CODE: 0

Output Summary: Build succeeded. 0 Error(s). CS86xx count: 0 for the Batch A pragma-enabled files (Prediction.cs, FolderHierarchyNode.cs, LcppnFolderPredictorConfig.cs, DoNotSerializeContractResolver.cs, BayesianClassifierExtensions.cs). AC1 satisfied for Batch A. `/p:Nullable=enable` not passed.
