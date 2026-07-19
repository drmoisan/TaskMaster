# Batch 2 — Nullable Pragma Gate (P2-T3)

Timestamp: 2026-07-19T09-22

## Commands

1. `dotnet tool run csharpier format .` — EXIT_CODE 0 (clean).
2. Pragma gate: `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:TreatWarningsAsErrors=true` (WITHOUT `/p:Nullable=enable`), measured via the isolated-compile methodology from P0-T5 (SVGControl pre-built clean, `/p:BuildProjectReferences=false`).

## Output Summary

Batch 2 (7 files: AsyncQueue, AsyncLazy, LazyTry, StackGeek, StackObjectCS, TreeNodeOfT,
DataConverter2d) cluster diagnostics under the per-file pragma:
- CS86xx count attributed to `ReusableTypeClasses/`: 0 (AC1 for Batch 2)
- CS8714 count: 0
- Pre-existing non-cluster UtilitiesCS TWAE errors: 14 (unchanged; out of scope)

Annotations applied (annotation/null-safety only, no behavior change):
- `AsyncLazy.cs` (DataBoundValues sample): `PropertyChangedEventHandler?` event, `string?`
  CallerMemberName param, nullable local handler.
- `LazyTry.cs`: 3x `return default(T)!` on the try/catch factory fallback (LazyTry's documented
  null sentinel for reference T; justified `!` with comment).
- `StackObjectCS.cs`: 4x `out T? result` on TryPeek/TryPop (default on failure path) — the plan's
  prescribed `out TValue?` form, no post-condition attribute.
- `StackGeek.cs` (GFG sample): nullable DLLNode `prev/next/head/mid` fields; justified `!` at
  derefs the count-guarded algorithm proves non-null.
- `TreeNodeOfT.cs`: nullable `Parent`/`_parent` (root has no parent), `_value = default!` backing
  field, nullable returns on `FirstAncestor`/`FindByDelegate`/`FindSequentialNode`/`FindNode`/
  `GetNextLevel`/`GetPreviousLevel`, `Select(x => x.Parent!)` (Where-guarded), `!` at
  non-null-input GetNextLevel/GetPreviousLevel call sites.

No `System.Diagnostics.CodeAnalysis` post-condition attribute was added.
