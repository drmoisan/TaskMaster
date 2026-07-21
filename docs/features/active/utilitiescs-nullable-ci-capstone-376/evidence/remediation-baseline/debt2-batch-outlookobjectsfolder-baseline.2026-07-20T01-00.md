# Debt 2 — Batch: OutlookObjects/Folder — Baseline

Timestamp: 2026-07-20T01-00
Command: filtered from a fresh isolated rebuild (post-People batch), using the authoritative
per-`(file, line, col, code)` deduped extraction method.

Files under `UtilitiesCS/OutlookObjects/Folder/**` (31 diagnostics, 4 files):

| File | Diagnostics |
|---|---|
| `FolderConverter.cs` | CS8603:2 |
| `FolderPredictor.cs` | CS8603:2 |
| `FolderScorer.cs` | CS8604:16, CS8601:8 |
| `FolderTreeCompatibilityView.cs` | CS8604:1 |

Per-code totals: CS8604:17, CS8603:4, CS8601:8 (31 total, 4 files). This is the final Phase 2
batch (P2-T15/T16); after this batch's remediation, P2-T17's full-solution rebuild gate is
expected to reach `EXIT_CODE: 0`.
