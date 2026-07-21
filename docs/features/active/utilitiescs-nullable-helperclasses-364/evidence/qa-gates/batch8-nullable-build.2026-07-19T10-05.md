# Batch 8 — Pragma-Only Nullable Build Verification (Issue #364)

- Timestamp: 2026-07-19T10-05
- Task: [P8-T6]

## Opted-in files (3, high-contract finish)

- `Initializer.cs` — deliberate downstream contract: the three dependency-gated `GetOrLoad<T>`/`Load<T>` overloads that `return default(T)` are annotated `T?` (callers must handle a possible null); `EqualityComparer<T>.Default.Equals(variable, default(T)!)` and `StackFrame.GetMethod()!` suppress the internal reflection/default-arg nullability without behavior change.
- `FileSystem/FilePathHelper.cs` — the two-group string-property contract split applied: `FilePath`/`FolderPath`/`FileName` remain non-null (default `""`, transient internal null via `null!`); `FileStemSeed`/`FileStemSuffix`/`FileStem`/`FileExtension` are nullable sentinels; `TryParseFileStem` out params are `string?`; `object? sender`; `Path.GetDirectoryName(...)!`; behavior-preserving `!` on stem derefs guarded by `StemInitialized()`. The Newtonsoft/`ICloneable`/`INotifyPropertyChanged` contract is behavior-compatible.
- `PrettyPrint.cs` — annotation-only null-safety (nullable optional `headers`/`title`/`justifications` params threaded through `GetJaggedColumnCount`/`GetJaggedColumnWidths`/`AppendJaggedTitle`/`AppendJaggedHeaders`; `ToString()!` and guarded `text!` derefs). The file is NOT split (see maintainer-flags).

## Command (authoritative CS86xx verification)

- Command: `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:BuildProjectReferences=false`
- EXIT_CODE: 0
- `/p:Nullable=enable` NOT passed (pragma-only; isolated build is the authoritative CS86xx signal — see P0-T4).

## Output Summary

- CS86xx warnings (whole UtilitiesCS project): 0
- CS86xx warnings in `HelperClasses/`: 0
- Total warnings: 15 (pre-existing non-nullable CS0618/CS0168, unchanged). No new diagnostics introduced by Batch 8.
- Result: PASS. All 3 Batch-8 opted-in files reach zero CS86xx. The `Initializer` generic `T?` returns and the `FilePathHelper` string-property nullability split are recorded as deliberate downstream contracts. File-size breaches (PrettyPrint 680, FilePathHelper 505) flagged in `evidence/other/maintainer-flags.2026-07-19T10-05.md`, not fixed.
