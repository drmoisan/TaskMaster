# Batch E Nullable Gate

Timestamp: 2026-07-19T04-30

Batch E files (4): AsyncSerialization.cs, DfMLNet.cs, DfDeedle.cs, DfDeedle.FrameUtilities.cs (DfDeedle.cs + DfDeedle.FrameUtilities.cs are one partial class, remediated together; both carry the pragma).

Commands:
1. `dotnet tool run csharpier format UtilitiesCS/Extensions/` -> EXIT 0.
2. `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:TreatWarningsAsErrors=true /p:BuildProjectReferences=false` (per-file pragma gate; WITHOUT /p:Nullable=enable)

EXIT_CODE: 1 (overall build FAILED only on pre-existing non-nullable warnings)

Output Summary:
- CS86xx (nullable) diagnostics: 0 (after one fix pass clearing 3 flow-state sites). All 4 Batch E files carry `#nullable enable` and compile with zero nullable diagnostics (AC1 satisfied for Batch E).
- Non-nullable warnings-as-errors: CS0168 x2, CS0618 x28 — unchanged from baseline.
- Annotation notes:
  - DfMLNet.cs: `GetFirstNonNull` -> `object?` (param `object[]?`); local `object? T`. Consumes Batch C `CastNullSafe`/`ToStringArray` without re-touching Batch C.
  - DfDeedle.cs: logger `GetCurrentMethod()!.DeclaringType!`; `DescribeSynchronizationContext(SynchronizationContext?)`; `LogDfTiming(..., string? details)`; `DfDeedle.EmailRecord` kept a plain `private struct` with `= default!` on the five `string` fields (no `record`/`init`); one justified `!` for currentFolder at the AddQfcColumnsAsync call.
  - DfDeedle.FrameUtilities.cs: `GetFirstNonNull` -> `object?`; `FromArray2D` -> `Frame<int,string>?` (params `object[,]?`/`Dictionary<string,int>?`); `FromDefaultFolderAsync` -> `Task<Frame<int,string>?>`; `FromDefaultFolder(Store,...)` -> `Frame<int,string>?`; `FromDefaultFolder(Stores,...)` local `Frame<string,string>? df` (return stays non-null). Consumes Batch C `To2D` without re-touching it.
  - AsyncSerialization.cs: pragma; two justified `progress!.Report(...)` to preserve the original unconditional progress reporting past defensive `?.`/null-comparison flow states.
- `DfDeedle.EmailRecord` remains a plain `private struct` (no `record`/`record struct`/`init`, which fail CS0518 on net481). No Batch C file was re-edited. No post-condition attribute added.
