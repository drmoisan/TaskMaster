# Batch B — Per-File Nullable Pragma Gate

Timestamp: 2026-07-19T01-30

Command: `msbuild UtilitiesCS/UtilitiesCS.csproj -t:Rebuild -p:Configuration=Debug -p:Platform=AnyCPU -p:TreatWarningsAsErrors=true -p:WarningsNotAsErrors=CS0649;CS0618;CS0168` (WITHOUT `/p:Nullable=enable`; scoped-gate parameters per the P0-T5 baseline — the three exempted codes are pre-existing, non-CS86xx, out-of-scope)

EXIT_CODE: 0

Output Summary: Build succeeded. 0 Error(s). CS86xx count: 0 for the Batch A+B pragma-enabled files (Corpus.cs, CorpusInherit.cs plus the Batch A set). AC1 satisfied through Batch B. `/p:Nullable=enable` not passed.
