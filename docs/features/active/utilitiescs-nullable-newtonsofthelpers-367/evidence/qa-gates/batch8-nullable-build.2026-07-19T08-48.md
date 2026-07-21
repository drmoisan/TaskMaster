# Batch 8 Nullable Build Verification (P8-T3)

- Timestamp: 2026-07-19T08-48
- Opted-in file (1): `UtilitiesCS/NewtonsoftHelpers/FilePathHelperConverter.cs`

## Genuine nullable gate

- Command: `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:TreatWarningsAsErrors=true /p:WarningsNotAsErrors=CS0649%3BCS0618%3BCS0168 /m` (NO `/p:Nullable=enable`)
- EXIT_CODE: 0
- Output Summary: Build succeeded, zero errors, zero `CS86xx` in the Batch 8 file. EXIT 0 under a gate where CS86xx is fatal proves nullable-clean.

## Exact plan solution command (invariant, per baseline)

Invariant with P0-T4 (SVGControl-blocked). Executed in full at P9-T3.

## Edits applied (annotation-only; cross-module FilePathHelper serialization contract)

- `#nullable enable` at top.
- `_fileSystemFolders` field -> `= null!` (the public `FilePathHelperConverter(IFileSystemFolderPaths)` ctor sets it from its required non-null dependency; the protected parameterless ctor is for subclassing/testing). The ctor PARAMETER stays non-null (required dependency), per spec.
- `ReadJson` `existingValue` -> `FilePathHelper?`; return stays non-null `FilePathHelper` (body returns `new FilePathHelper(...)`).
- `ExtractFolderPath(Dictionary<string, string> info)` return -> `string?` (has `return null;` paths; the ReadJson call site already uses `?? ""`). The `ExtractFolderPath(string, string)` overload keeps its non-null `string` return (no null path).
- `GetErrorMessage`: tightened `if (reader is JsonTextReader) { var textReader = reader as JsonTextReader; ... }` to the pattern form `if (reader is JsonTextReader textReader)` so `textReader.LineNumber` is a non-null deref (behavior-preserving pattern-match tightening, not a control-flow change).
- `WriteJson` `value` -> `FilePathHelper?` with behavior-preserving `value!.FolderPath` / `value!.FileName` (Newtonsoft invokes WriteJson with a non-null value for a registered converter).
- `TryGetValue(..., out string ...)` sites did not require `out string?` — the BCL `MaybeNullWhen(false)` flow plus the existing `if (!TryGetValue(...)) return ...;` guards yield correct non-null flow, so no CS86xx surfaced there. `reader.Value as string` sites are already `?? ""`- or `ThrowIfNull()`-guarded.
