# Batch F0 Nullable Gate (P1-T3)

Timestamp: 2026-07-19T11-20

## Format

Command: `dotnet tool run csharpier format .`
EXIT_CODE: 0 — Formatted 1406 files, clean.

## Full-solution compile (non-TWAE, confirms all projects still build)

Command: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /m`
EXIT_CODE: 0 — 0 errors.

## Nullable pragma gate (scoped UtilitiesCS Rebuild; see baseline-nullable-pragma-gate.md for methodology)

Command: `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:TreatWarningsAsErrors=true /p:BuildProjectReferences=false`

EXIT_CODE: 1

Output Summary: **Zero CS86xx** for the 14 Batch F0 files (AC1). The build's only errors are the 15
pre-existing non-CS86xx warnings-as-errors (CS0618 obsolete-API x28 raw, CS0168 unused-variable x2 raw)
in non-Folder/Store files (Triage.cs, SortEmail.cs, etc.) — identical to the P0-T5 baseline, so no new
diagnostic was introduced by adding the F0 pragmas and annotations.

## Files remediated (14)

`IDeadlineClock.cs`, `IDispatcherYield.cs`, `IFolderHandleResolver.cs`, `IFolderHierarchyProvider.cs`,
`IFolderSearchHandler.cs`, `IOutlookFolderHierarchyReader.cs`, `IOutlookFolderNotificationSink.cs`,
`IOutlookFolderTreeService.cs`, `FolderTreeRefreshReason.cs`, `FolderRow.cs`, `FolderScore.cs`,
`FolderBreadcrumbSegment.cs`, `FolderTreeSnapshotChangedEventArgs.cs`, `OutlookFolderHierarchyRecord.cs`.

## Key annotation decisions (shape contracts for later batches)

- `IFolderHandleResolver.TryResolve(FolderTreeSnapshotNode? node, out object? folder)` — both node and out
  folder nullable, matching the F5 `OutlookFolderHandleResolver`/`FakeFolderHandleResolver` null-check pattern.
- `IFolderHierarchyProvider.ResolveLeafKeyAsync` returns `Task<FolderTreeNodeKey?>` (impl returns `match?.Key`).
- `IFolderSearchHandler.FindFolder` optional params `List<string>? emailSearchRoots = null` and
  `IEnumerable<...>? exclusions = null` (default-null); `FolderArray`/`FolderRowArray`/`Suggestions` kept
  non-null, matching `FolderPredictor`'s non-null returns (Batch F3).
- No post-condition attributes added; no `record`/`init` introduced.
