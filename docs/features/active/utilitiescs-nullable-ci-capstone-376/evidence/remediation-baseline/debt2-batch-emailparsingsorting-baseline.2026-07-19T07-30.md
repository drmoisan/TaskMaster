# Debt 2 — Batch: EmailParsingSorting — Baseline

Timestamp: 2026-07-19T07-30
Command: filtered from a fresh isolated rebuild
(`MSBuild.exe UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU
/p:TreatWarningsAsErrors=true`, captured after the Bayesian and ClassifierGroups batches
completed), using a per-`(file, line, col, code)` deduped extraction (the authoritative method
established in P2-T1, since the simple shell-grep dedup double-counts under `/m` doubling).

Files under `UtilitiesCS/EmailIntelligence/EmailParsingSorting/**` (34 diagnostics, 7 files):

| File | Diagnostics |
|---|---|
| `AutoFile.cs` | CS0168:1, CS8604:1 |
| `EmailDataMiner.FolderExtraction.cs` | CS8619:1, CS8602:2, CS8604:1, CS0618:1 |
| `EmailDataMiner.Serialization.cs` | CS8625:4 |
| `EmailDataMiner.Transform.cs` | CS8602:5, CS8600:1, CS8604:1, CS8620:1 |
| `EmailFiler.cs` | CS8602:1, CS0618:1, CS8604:1 |
| `EmailFilerConfig.cs` | CS8604:4 |
| `SortEmail.cs` | CS8602:2, CS0618:4, CS8604:3, CS8601:1 |

Per-code totals: CS8604:11, CS8602:10, CS0618:6, CS8625:4, CS8620:1, CS8619:1, CS8600:1,
CS8601:1, CS0168:1 (34 total).
