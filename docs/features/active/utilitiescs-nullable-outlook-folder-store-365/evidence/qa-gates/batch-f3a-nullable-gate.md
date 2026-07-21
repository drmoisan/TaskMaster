# Batch F3a Nullable Gate (P4-T3)

Timestamp: 2026-07-19T12-25

## Format / Build / Gate
- `dotnet tool run csharpier format .` — EXIT 0, clean.
- `msbuild TaskMaster.sln /t:Build ... /m` — EXIT 0, 0 errors.
- Scoped gate `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Platform=AnyCPU /p:TreatWarningsAsErrors=true /p:BuildProjectReferences=false` — **zero CS86xx** for FolderConverter.cs, FolderNavigator.cs, FolderMinimalWrapper.cs, and "FolderWrapper .cs" (AC1). (FolderTree.cs, FolderScorer.cs, FolderPredictor.cs are pragma'd in P4-T1 but annotated in later P4 sub-tasks T5/T8/T11; their diagnostics are addressed there and confirmed zero at P4-T12.)

## Files remediated in P4-T2 (4 of the 8 pragma'd in P4-T1)
FolderConverter.cs, FolderNavigator.cs, FolderMinimalWrapper.cs, "FolderWrapper .cs" (531 lines, not split, not renamed).

## Key annotation decisions
- FolderConverter: the two IApplicationGlobals `ToFsFolderpath` overloads return `string?` (they `return null`).
- FolderNavigator: `GetOutlookFolder` returns `Folder?`; `OlFolderlist_GetAllRet` local nullable; the root lookup
  forgiven with `!` (COM root always resolves).
- FolderMinimalWrapper / FolderWrapper: MAPIFolder/Outlook.Folder fields+props nullable; Name/RelativePath `string?`;
  Load*/ToRelativePath returns `string?`; `AsyncLazy<IItemInfo[]>? ItemHelpers`; `IApplicationGlobals? Globals`
  (nullable-by-design, guarded); `PropertyChangedEventHandler? PropertyChanged` and `object? sender` handlers.
- The nullable string Lazy fields use `new Lazy<string?>(...)` because #363's `ToLazy<T>() where T : class` rejects
  `string?` (CS8634); this is behavior-equivalent to `ToLazy` for the nullable case (a real net481/#363 API
  constraint). Value-type lazies (`ToLazyValue`) are unchanged.
- COM traversal derefs (FolderWrapper LoadFolderSize/LoadItemHelpers/ResetLazy; FolderMinimalWrapper
  RestoreFromRelativePath) use justified `!` — no new runtime guard statements added (AC4).
- No post-condition attributes; no record/init; "FolderWrapper .cs" not split/renamed.
