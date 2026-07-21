# Baseline Nullable Pragma-Gate Build (P0-T5)

Timestamp: 2026-07-19T10-59

## Plan gate command (exact, full solution)

Command: `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` (WITHOUT `/p:Nullable=enable`, via VS18 `amd64/MSBuild.exe`, `/m`)

EXIT_CODE: 1

Output Summary: The full-solution gate exits 1, but **zero CS86xx** diagnostics are emitted. The 2 errors
are pre-existing `CS0649` ("field is never assigned, always default null") in the **vendored `SVGControl`**
project (`SVGControl/SvgImageSelector.cs` lines 56-57), promoted to errors by `TreatWarningsAsErrors`. These
fields originate from sibling epic child `#368` (commit `c194362d`, "feat(368): remediate nullable-reference
debt in SVGControl/ via per-file opt-in"); the source comments at lines 82-88 explicitly document that the
fields are intentionally never assigned. There is no `NoWarn` for CS0649 anywhere. Because `UtilitiesCS`
has a `ProjectReference` to `SVGControl`, the parallel build fails at `SVGControl` before `UtilitiesCS`
compiles, so the full-solution command alone cannot surface `UtilitiesCS` diagnostics.

This is a **pre-existing, out-of-scope, non-CS86xx** condition on the epic integration branch tip
(`dffadd5a`) from a sibling feature. Per the epic's incremental per-file gate model, the full-solution
gate is not expected to exit 0 until all sibling children land. This feature (`#365`) must not edit
`SVGControl` (outside the `Folder/`+`Store/` scope). The feature's obligation is that Folder/Store files
contribute **zero CS86xx**.

## Scoped UtilitiesCS gate (actual CS86xx signal for this cluster)

To obtain a valid CS86xx signal for the Folder/Store cluster (which lives in `UtilitiesCS`), the same
`TreatWarningsAsErrors` recompile was run scoped to `UtilitiesCS`, against pre-built dependency DLLs:

Command: `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:TreatWarningsAsErrors=true /p:BuildProjectReferences=false`

EXIT_CODE: 1

Output Summary: **Zero CS86xx** diagnostics from `UtilitiesCS`. The 15 errors are all pre-existing
`CS0618` (obsolete-API use) and `CS0168` (unused variable) promoted by `TreatWarningsAsErrors`, located
in non-Folder/Store files (`Triage.cs`, `SortEmail.cs`, `AutoFile.cs`, `BayesianClassifierGroup.cs`,
`BayesianSerializationHelper.cs`, `EmailDataMiner.FolderExtraction.cs`, `EmailFiler.cs`,
`IAsyncEnumerableExtensions.cs`, `IntelligenceConfig.cs`, `ManagerAsyncLazy.cs`). **None are in
`OutlookObjects/Folder/` or `OutlookObjects/Store/`.** Roslyn reports all diagnostics in one pass, so the
absence of any CS86xx confirms the 18 already-`#nullable enable` Folder/Store files are nullable-clean and
the 63 opt-in-target files are still null-oblivious (no pragma) and therefore emit no pragma-driven CS86xx
at this baseline.

## Baseline CS86xx count for the cluster: 0

- 18 already-enabled Folder/Store files: 0 CS86xx (verify-only, clean).
- 63 opt-in-target files: still null-oblivious at baseline (no `#nullable enable` pragma yet), so they emit
  no pragma-driven CS86xx under this gate.

## Per-batch gate methodology (used for P1-T3 .. P11-T3)

Because the pre-existing SVGControl CS0649 and the pre-existing UtilitiesCS CS0618/CS0168 warning debt make
the literal full-solution `/p:TreatWarningsAsErrors=true` command exit non-zero for reasons unrelated to
this feature, each batch's nullable-gate verification uses the **scoped UtilitiesCS Rebuild** above as the
CS86xx signal (grep `CS86xx` count, which must remain 0), preceded by a full-solution non-TWAE build
(`/t:Build`) confirming all projects (including `UtilitiesCS.Test`) still compile with the annotation
changes. The plan's exact full-solution gate command is re-run at final QC (P12-T3) for the record.
