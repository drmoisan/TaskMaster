# Baseline Per-File Nullable Pragma Gate

Timestamp: 2026-07-19T00-50

## 1. Literal solution-wide command (plan P0-T6)

Command: `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` (WITHOUT `/p:Nullable=enable`)

EXIT_CODE: 1

Output Summary: CS86xx (nullable) diagnostics: 0 (none of the 24 cluster files carry
`#nullable enable` yet at baseline, so none can emit nullable diagnostics). Build FAILED with
2 pre-existing, non-nullable errors: `error CS0649` (field never assigned) x2 in the VENDORED
`SVGControl/SvgImageSelector.cs` (`_relativeImagePath`, `_absoluteImagePath`). This is a
pre-existing vendored-project condition (documented precedent: same finding recorded in the
sibling Wave-0 `utilitiescs-nullable-extensions` feature's `final-nullable-pragma-gate.md`).
Under parallel (`-m`) build, the vendored `SVGControl` compile fails first under
`TreatWarningsAsErrors` and aborts the dependency graph before `UtilitiesCS.csproj` recompiles,
so this literal invocation alone cannot serve as a clean CS86xx proof for the cluster's 24
files. `/p:Nullable=enable` was NOT passed.

## 2. Scoped `UtilitiesCS.csproj` rebuild — definitive CS86xx baseline proof

Command: `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:TreatWarningsAsErrors=true /p:BuildProjectReferences=false` (WITHOUT `/p:Nullable=enable`;
pre-requisite: `SVGControl/SVGControl.csproj` built once normally, without `TreatWarningsAsErrors`,
to produce the `SVGControl.dll` dependency artifact consumed via `BuildProjectReferences=false`)

EXIT_CODE: 1

Output Summary: CS86xx (nullable) diagnostics: 0 across the entire `UtilitiesCS.csproj`
compilation, confirming none of the 24 cluster files (none yet opted in) emit nullable
diagnostics at baseline. Build FAILED with 15 pre-existing, non-nullable errors promoted by
`TreatWarningsAsErrors`, none of which are CS86xx: `CS0618` (obsolete `System.Linq.AsyncEnumerable`
API usage) x13 across `BayesianClassifierGroup.cs`, `BayesianSerializationHelper.cs`,
`ManagerAsyncLazy.cs`, `Triage.cs` (x4 occurrences), `EmailDataMiner.FolderExtraction.cs`,
`EmailFiler.cs`, `SortEmail.cs` (x3 occurrences), `IntelligenceConfig.cs`,
`IAsyncEnumerableExtensions.cs`; and `CS0168` (unused local `OlMail`) x1 in `AutoFile.cs`. Three
of these pre-existing errors (`EmailDataMiner.FolderExtraction.cs`, `EmailFiler.cs`,
`SortEmail.cs`, `AutoFile.cs`) fall inside this feature's 24-file cluster — they are pre-existing,
non-nullable, out-of-scope diagnostics (obsolete-API usage and an unused local) and this
annotation-only feature does not fix them; their presence/count is recorded here as baseline
noise so any change in count during remediation is attributable to this feature, not
pre-existing debt.

## Interpretation

The operative per-file pragma-gate metric for AC1 is the CS86xx count. Baseline CS86xx = 0
(expected, since none of the 24 cluster files are yet opted into `#nullable enable`). Per-batch
and final gates are evaluated on the same CS86xx-count metric (target 0 after each file is
opted in), with the pre-existing non-nullable `CS0618`/`CS0168`/`CS0649` errors noted as
orthogonal, out-of-scope context that this feature must not introduce new instances of but is
not required to fix.
