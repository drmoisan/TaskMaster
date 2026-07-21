# Debt 2 — Batch: OlFolderTools — Remediated

Timestamp: 2026-07-20T00-35
Command: `MSBuild.exe UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:TreatWarningsAsErrors=true`
EXIT_CODE: 1 (solution-wide count still non-zero — remaining errors are entirely in
not-yet-remediated later batches. Zero errors remain for
`UtilitiesCS/EmailIntelligence/OlFolderTools/**`, confirmed by targeted grep returning no matches
after remediation.)

## Before/after

`FilterOlFoldersController.cs`: CS8604:2 -> 0. Total remaining solution-wide error count after
this batch: 35 (down from 37 after the prior batch).

## Remediation approach

- `Save()`: `.Select(info => info.RelativePath!)` — `RelativePath` (a `string?` property on the
  folder-tree-snapshot node type) is null-forgiven at the LINQ projection point, covering both
  downstream uses (`.Contains(x)` and `.TryAdd(x, 1)`) with a single fix.
- `CreateFolderTreeRequest()`: `FolderTreeRequest.ForStore(storeId!, ...)` — `storeId` (from
  `_globals.Ol.ArchiveRoot?.StoreID`) is confirmed non-null/non-whitespace by the preceding
  `string.IsNullOrWhiteSpace(storeId)` check's `else` branch, but the compiler cannot propagate
  that guarantee through the negated `IsNullOrWhiteSpace` call without a `[NotNullWhen]`
  post-condition attribute (unavailable on this net481 target without a polyfill).

## Designer-file exclusion confirmation

The 6 `OlFolderTools` Designer-generated files (`FilterOlFoldersViewer.Designer.cs`,
`FolderInfoViewer.Designer.cs`, `OSBrowser.Designer.cs`, `OSFolder.Designer.cs`,
`FolderRemapViewer.Designer.cs`, `FolderSelector.Designer.cs`) remain untouched by this batch —
`git diff` confirms empty for all 6 files, consistent with the epic-wide Designer-file exclusion.

## Behavior-preservation confirmation

`git diff --stat` shows 2 insertions / 2 deletions in exactly 1 file — both null-forgiving
operator additions; no removed or altered method signatures, no altered control flow.
