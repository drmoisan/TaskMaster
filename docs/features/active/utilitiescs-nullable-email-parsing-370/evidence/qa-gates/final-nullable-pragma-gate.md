# Final Per-File Nullable Pragma Gate (AC1)

Timestamp: 2026-07-19T06-55

## 1. Literal solution-wide command

Command: `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` (WITHOUT `/p:Nullable=enable`)

EXIT_CODE: 1

Output Summary: CS86xx (nullable) diagnostics: 0. Build FAILED with 2 pre-existing, non-nullable
errors: `CS0649` x2 in the vendored `SVGControl/SvgImageSelector.cs`, identical to the baseline
(P0-T6) and every batch's finding. Under parallel (`-m`) build the vendored SVGControl compile
fails first and aborts the graph before `UtilitiesCS.csproj` recompiles. `/p:Nullable=enable`
was NOT passed.

## 2. Scoped `UtilitiesCS.csproj` rebuild — definitive AC1 proof across all 24 files

Command: `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:TreatWarningsAsErrors=true /p:BuildProjectReferences=false` (WITHOUT `/p:Nullable=enable`; pre-requisite: `SVGControl/SVGControl.csproj` built once normally to produce the dependency DLL, same technique as baseline P0-T6)

EXIT_CODE: 1

Output Summary: CS86xx (nullable) diagnostics: 0 across the entire `UtilitiesCS.csproj`
compilation — confirming all 24 remediation-target cluster files (`EmailParsingSorting/` x14,
`SubjectMap/` x7 minus the excluded Designer file, `Ctf/` x4) emit zero nullable diagnostics
under their per-file `#nullable enable` pragma with `TreatWarningsAsErrors` (**AC1 SATISFIED**).
Build FAILED with the same 14 pre-existing, non-nullable errors as baseline (`CS0618` x13,
`CS0168` x1 in `AutoFile.cs`), unchanged in kind and count from the P0-T6 baseline scoped-gate
run.

## Conclusion

AC1 is met: every one of the 24 cluster files carries `#nullable enable` and compiles with zero
CS86xx diagnostics under the per-file pragma with `TreatWarningsAsErrors`. The literal
solution-wide gate's failure is attributable only to a pre-existing, out-of-scope, non-nullable
condition in a vendored project (`SVGControl`), unchanged from baseline. `/p:Nullable=enable`
was not passed in either command.
