# Debt 2 — Batch: OlFolderTools — Baseline

Timestamp: 2026-07-19T08-25
Command: filtered from a fresh isolated rebuild (post-Evaluation/Flags/IntelligenceConfig/
SubjectMap/Extensions batch), using the authoritative per-`(file, line, col, code)` deduped
extraction method.

Excluded Designer-generated files (confirmed present, zero diagnostics, left null-oblivious per
the epic-wide Designer-file exclusion): `FilterOlFolders/FilterOlFoldersViewer.Designer.cs`,
`FilterOlFolders/FolderInfoViewer.Designer.cs`, `FilterOlFolders/OSBrowser.Designer.cs`,
`FilterOlFolders/OSFolder.Designer.cs`, `FolderRemap/FolderRemapViewer.Designer.cs`,
`FolderRemap/FolderSelector.Designer.cs` (6 files, confirmed via
`find UtilitiesCS/EmailIntelligence/OlFolderTools -iname "*.Designer.cs"`).

Non-excluded files with diagnostics under `UtilitiesCS/EmailIntelligence/OlFolderTools/**`
(2 diagnostics, 1 file):

| File | Diagnostics |
|---|---|
| `FilterOlFolders/FilterOlFoldersController.cs` | CS8604:2 |

Per-code totals: CS8604:2 (2 total, 1 file).
