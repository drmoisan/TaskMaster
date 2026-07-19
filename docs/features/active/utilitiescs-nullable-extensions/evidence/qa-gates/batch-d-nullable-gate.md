# Batch D Nullable Gate

Timestamp: 2026-07-19T03-55

Batch D files (3): EnumExtensions.cs, TraceExtensions.cs, WinFormsExtensions.cs

Commands:
1. `dotnet tool run csharpier format UtilitiesCS/Extensions/` -> EXIT 0.
2. `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:TreatWarningsAsErrors=true /p:BuildProjectReferences=false` (per-file pragma gate; WITHOUT /p:Nullable=enable)

EXIT_CODE: 1 (overall build FAILED only on pre-existing non-nullable warnings)

Output Summary:
- CS86xx (nullable) diagnostics: 0 (after one fix pass clearing a CS8604 overload-resolution issue). All 3 Batch D files carry `#nullable enable` and compile with zero nullable diagnostics (AC1 satisfied for Batch D).
- Non-nullable warnings-as-errors: CS0168 x2, CS0618 x28 — unchanged from baseline.
- Downstream contract (#374 dialogs-misc consumes WinFormsExtensions.cs `Clone<T>()`):
  - The three public `Clone<T>` overloads keep their exact signatures (`T Clone<T>(this T, ...)`) and remain non-null returns; `Clone(this RowStyle)`/`Clone(this ColumnStyle)` unchanged. Internal reflection copy helpers use justified `!` (commented) on `Activator.CreateInstance`, `PropertyInfo.GetValue`, and `Type.FullName` to preserve the original behavior.
  - `GetAncestor<T>` (both overloads) now return `T?` (honestly returns null when no ancestor found); `control.Parent!` preserves the original parentless-control NRE behavior.
  - `IsRegistered(this EventHandler? handler, ...)` reflects the existing null guard; `GetEventHandlerList` keeps its non-null `(EventHandlerList, object)` tuple via justified `!` on the reflected field/property.
- TraceExtensions.cs: reflection returns annotated nullable per plan — `GetCallerByName` -> `MethodBase?`, `GetParameterName`/`TryGetParameterName` -> `string?`, `GetParameterNames` -> `string?[]`; logger field uses `GetCurrentMethod()!.DeclaringType!`. The `string.IsNullOrEmpty(methodName)` change resolves an extension-overload ambiguity (a nullable string binds to the `IEnumerable<char>` overload) with identical behavior.
- EnumExtensions.cs: pragma only; `Enum`-constrained value-type generics were NOT given reference-nullable annotations, per plan.
- No post-condition attribute added.
