# Batch E — Per-File Nullable Pragma Gate

Timestamp: 2026-07-19T04-00

Command: `msbuild UtilitiesCS/UtilitiesCS.csproj -t:Rebuild -p:Configuration=Debug -p:Platform=AnyCPU -p:TreatWarningsAsErrors=true -p:WarningsNotAsErrors=CS0649;CS0618;CS0168` (WITHOUT `/p:Nullable=enable`)

EXIT_CODE: 0

Output Summary: Build succeeded. 0 Error(s). CS86xx count: 0 for the Batch A–E pragma-enabled files (the 12 Batch E files — SpamBayes 4-file partial set, Triage 2-file partial set, ActionableClassifierGroup, CategoryClassifierGroup, LcppnFolderPredictor, LcppnFolderPredictorStore, OlFolderClassifierGroup, SpamInitTimingProbe — plus Batches A/B/C/D). AC1 satisfied through Batch E. `/p:Nullable=enable` not passed.
