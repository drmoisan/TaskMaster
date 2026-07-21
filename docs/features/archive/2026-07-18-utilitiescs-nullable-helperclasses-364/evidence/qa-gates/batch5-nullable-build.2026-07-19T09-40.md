# Batch 5 — Pragma-Only Nullable Build Verification (Issue #364)

- Timestamp: 2026-07-19T09-40
- Task: [P5-T8]

## Opted-in files (5 hand-written) + Designer handling

- `FileSystem/ShellUtilities.cs` — `GetFileIcon` return annotated `Icon?` (returns null on failure); P/Invoke struct string fields required no annotation.
- `FileSystem/ShellUtilitiesStatic.cs` — `GetFileIcon` return `Icon?` (matches existing XML doc).
- `FileSystem/SysImageListHelper.cs` — mutually-exclusive `listView`/`treeView` fields annotated nullable; collection-getter properties return nullable; behavior-preserving `!` on `GetImageIndex`/`AddImageToCollection` derefs.
- `WipUnfinished/ComStreamWrapper.cs` — pragma; `out STATSTG stat` and ctor-assigned fields required no annotation.
- `DvgForm.cs` (hand-written partial) — pragma; only the `object sender` -> `object? sender` event-handler annotation.
- `DvgForm.Designer.cs` — left NON-opted-in (no pragma, byte-unchanged). See `evidence/other/maintainer-flags.2026-07-19T09-40.md`.

## Command (authoritative CS86xx verification)

- Command: `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:BuildProjectReferences=false`
- EXIT_CODE: 0
- `/p:Nullable=enable` NOT passed (pragma-only; isolated build is the authoritative CS86xx signal — see P0-T4).

## Output Summary

- CS86xx warnings (whole UtilitiesCS project): 0
- CS86xx warnings in `HelperClasses/`: 0
- Total warnings: 15 (pre-existing non-nullable CS0618/CS0168, unchanged). No new diagnostics introduced by Batch 5.
- Result: PASS. All 5 Batch-5 opted-in files reach zero CS86xx. `DvgForm.Designer.cs` produces no CS86xx (oblivious) and does not cross-block the opted-in `DvgForm.cs`. Epic-scope Designer conflict flagged.
