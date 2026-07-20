# Batch 7 — Nullable Pragma Gate (P7-T2 / P7-T3)

Timestamp: 2026-07-19T23-40

## Commands

1. `dotnet tool run csharpier check .` — EXIT_CODE 1 initially (prior-agent
   formatting residue in the 7 in-flight Batch 7 files), then
   `dotnet tool run csharpier format .` — EXIT_CODE 0 (formatted; only the 7
   Batch 7 source files reformatted, no unrelated churn), then
   `dotnet tool run csharpier check .` — EXIT_CODE 0 (clean second pass).
2. Pragma gate (isolated-compile methodology per P0-T5 / Batch-6):
   `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:TreatWarningsAsErrors=true /p:BuildProjectReferences=false`
   (WITHOUT `/p:Nullable=enable`).

EXIT_CODE: 1 (whole-assembly build; nonzero is caused entirely by the same PRE-EXISTING,
out-of-scope non-nullable warnings-as-errors documented at baseline P0-T5 and Batch-6,
decomposed below).

## Output Summary

Batch 7 (7 files: `NewSmartSerializableConfig`, `SmartSerializableBase`, `SmartSerializable`,
`SmartSerializableStatic`, `SmartSerializableNonTyped`, `SmartSerializableLoader`,
`ConfigController`) cluster diagnostics:
- CS86xx (nullable) count attributed to the 7 Batch 7 files: 0 (AC1 for Batch 7)
- CS87xx (nullable, incl. CS8714 / CS8766) count attributed to the 7 Batch 7 files: 0

One nullability mismatch (CS8766) was present in the inherited working-tree state at
`SmartSerializable.cs` line 406: the prior agent had annotated
`SmartSerializable<T>.DeserializeObject` with a `T?` return, which conflicted with the
null-oblivious out-of-scope interface `ISmartSerializable<T>.DeserializeObject` (declared `T`
in `UtilitiesCS/Interfaces/IReusableTypeClasses/ISmartSerializable.cs`, not one of the 7
Batch 7 files). Resolved IN SCOPE by conforming the implementation return type to the
interface's `T` and returning the genuinely-nullable local with a justified `!` plus a
`// why` comment. No out-of-scope interface file was edited.

Whole-assembly error decomposition (unchanged from P0-T5 baseline and Batch-6; all pre-existing /
out of scope; ZERO originate in a Batch 7 file):
- `error CS0618` (obsolete-API usage): 28 occurrences — pre-existing non-cluster files
  (Triage.cs, SortEmail.cs, ManagerAsyncLazy.cs, IntelligenceConfig.cs, BayesianClassifierGroup.cs,
  BayesianSerializationHelper.cs, EmailFiler.cs, FolderExtraction.cs, AutoFile.cs,
  IAsyncEnumerableExtensions.cs).
- `error CS0168` (unused variable): 2 occurrences — pre-existing non-cluster files.
- Zero errors and zero warnings originate in any of the 7 Batch 7 files.
- No `System.Diagnostics.CodeAnalysis` post-condition attribute was added; no polyfill declared.
- No `record` / `init` / `record struct` conversion.
- No `NewtonsoftHelpers` file was touched; the three exempt WinForms files
  (`ConfigViewer.Designer.cs`, `ConfigViewer.cs`, `ConfigGroupBox.cs`) carry no `#nullable enable`
  and were not modified.
- `SmartSerializable.cs` and `SmartSerializableBase.cs` (both pre-existing >500) remain single files.
- `/p:Nullable=enable` was NOT passed.
