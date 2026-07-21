# Final Nullable Pragma Gate (P12-T3)

Timestamp: 2026-07-19T16-40

## Plan exact command (full solution)
Command: `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` (WITHOUT `/p:Nullable=enable`)
EXIT_CODE: 1
Output Summary: **Zero CS86xx and zero CS87xx nullable diagnostics** across the whole solution. The only 2
errors are the pre-existing `CS0649` in the vendored `SVGControl` project (from sibling feature #368, documented
at baseline P0-T5) — not nullable diagnostics and outside this feature's `Folder/`+`Store/` scope.

## Scoped UtilitiesCS gate (authoritative CS86xx/CS87xx signal for this cluster)
Command: `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:TreatWarningsAsErrors=true /p:BuildProjectReferences=false`
EXIT_CODE: 1
Output Summary: **Zero nullable diagnostics across the full CS8xxx range** (CS86xx AND CS87xx) for all 63
remediated files and the 18 verify-only Folder/Store files under the per-file pragma (AC1). The only errors are
the 15 pre-existing non-nullable CS0618 (obsolete-API) and CS0168 (unused-variable) warnings-as-errors in
non-Folder/Store files (Triage.cs, SortEmail.cs, etc.), unchanged from the P0-T5 baseline. `/p:Nullable=enable`
was not passed.

## Coverage-gap-audit note
The per-batch gates filtered on `CS86xx`; this final comprehensive gate widened the filter to the full `CS8xxx`
nullable range and caught one `CS8766` (nullable return-type mismatch on the nested `IOutlookStoreAdapter`
interface in F5's OutlookFolderHierarchyReader.cs), which was fixed (commit widening the interface to
`IOutlookFolderAdapter? GetRootFolder()`). After that fix the cluster is clean of ALL nullable diagnostics.
