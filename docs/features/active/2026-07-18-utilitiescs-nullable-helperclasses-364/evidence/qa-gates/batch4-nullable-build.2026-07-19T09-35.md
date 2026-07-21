# Batch 4 — Pragma-Only Nullable Build Verification (Issue #364)

- Timestamp: 2026-07-19T09-35
- Task: [P4-T9]

## Opted-in files (6, FileSystem wrappers/adapters)

- `FileSystemInfoWrapper.cs` — pragma; clean delegating wrapper (ctor already `?? throw`).
- `DirectoryInfoWrapper.cs` — pragma; delegates to the oblivious inner interface (no `!` needed).
- `FileInfoWrapper.cs` — pragma; delegates to the oblivious inner interface.
- `PhysicalDirectoryInfoAdapter.cs` — behavior-preserving `!` at `Parent` (root boundary); `Root` unchanged.
- `PhysicalFileInfoAdapter.cs` — behavior-preserving `!` at `Directory`/`DirectoryName`; injectable-delegate seam byte-unchanged (verified by `git diff`).
- `MyFileSystemInfo.cs` — `AsDirectory`/`AsFile` return `IDirectoryInfo?`/`IFileInfo?`; `!` on `Length`/`GetFileSystemInfos` derefs; `Equals(MyFileSystemInfo?)`, `Equals(object?)`, and `==`/`!=` operand nullability.

## Command (authoritative CS86xx verification)

- Command: `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:BuildProjectReferences=false`
- EXIT_CODE: 0
- `/p:Nullable=enable` NOT passed (pragma-only; isolated build is the authoritative CS86xx signal — see P0-T4).

## Output Summary

- CS86xx warnings (whole UtilitiesCS project): 0
- CS86xx warnings in `HelperClasses/`: 0
- Total warnings: 15 (pre-existing non-nullable CS0618/CS0168, unchanged). No new diagnostics introduced by Batch 4.
- Result: PASS. All 6 Batch-4 opted-in files reach zero CS86xx. Root-boundary `!` decisions and the latent root-throws flag recorded in `evidence/other/maintainer-flags.2026-07-19T09-35.md`. PhysicalFileInfoAdapter seam preserved exactly.
