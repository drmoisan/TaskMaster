# Debt 2 — Batch: Evaluation/Flags/IntelligenceConfig/SubjectMap/Extensions — Baseline

Timestamp: 2026-07-19T08-00
Command: filtered from a fresh isolated rebuild (post-EmailParsingSorting-batch), using the
authoritative per-`(file, line, col, code)` deduped extraction method.

Per-subdirectory file list (16 diagnostics, 5 files):

| Subdirectory | File | Diagnostics |
|---|---|---|
| `UtilitiesCS/EmailIntelligence/Evaluation/**` | `FolderPredictorEvaluator.cs` | CS8604:1 |
| `UtilitiesCS/EmailIntelligence/Flags/**` | `FlagClassNoItem.cs` | CS8603:5 |
| `UtilitiesCS/EmailIntelligence/IntelligenceConfig/**` (actually a single file `IntelligenceConfig.cs` directly at the `EmailIntelligence` root — see P2-T1's plan-vs-reality note; no `IntelligenceConfig/` subdirectory exists) | `IntelligenceConfig.cs` | CS0618:1, CS8619:1, CS8604:1 |
| `UtilitiesCS/EmailIntelligence/SubjectMap/**` | `SubjectMapEncoder.cs` | CS8604:6 |
| `UtilitiesCS/EmailIntelligence/Extensions/**` (actually `UtilitiesCS/Extensions/` at the `UtilitiesCS` root — see P2-T1's plan-vs-reality note; no `UtilitiesCS/EmailIntelligence/Extensions/` folder exists) | `IAsyncEnumerableExtensions.cs` | CS0618:1 |

Per-code totals: CS8604:9, CS8603:5, CS0618:2, CS8619:1 (16 total, 5 files).
