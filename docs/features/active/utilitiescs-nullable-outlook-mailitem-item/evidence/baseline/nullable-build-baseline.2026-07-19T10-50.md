# Pragma-Only Nullable Build Baseline (P0-T4)

- Timestamp: 2026-07-19T10-50
- Task: [P0-T4]
- Primary (plan-literal) Command: `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true`
  - Resolved binary: `C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\amd64\MSBuild.exe`
  - `/p:Nullable=enable` was NOT passed (confirmed).
- EXIT_CODE (plan-literal solution command): 1

## Plan-literal command outcome (documented pre-existing out-of-scope halt)

The solution-wide TWAE Rebuild halts (exit 1) at the vendored `SVGControl` project on 2 PRE-EXISTING CS0649 errors that are entirely outside this child's scope (`UtilitiesCS/OutlookObjects/`):

- `SVGControl/SvgImageSelector.cs(56,25): error CS0649: Field '_relativeImagePath' is never assigned` (dates to vendored code, unrelated to #371)
- `SVGControl/SvgImageSelector.cs(57,25): error CS0649: Field '_absoluteImagePath' is never assigned`

`SVGControl` is a UtilitiesCS project dependency, so under `/t:Rebuild` its failure prevents the solution build from reaching `UtilitiesCS`. This is the same pre-existing epic-integration-branch condition documented for the already-merged sibling children #363/#364; it is not a regression introduced by #371 and is not fixable within this cluster-scoped child (SVGControl is not in scope). It is flagged, not fixed.

## Authoritative in-scope CS86xx measure (isolated UtilitiesCS build)

Because the solution build halts on the out-of-scope SVGControl CS0649, the in-scope CS86xx count is measured by the authoritative isolated UtilitiesCS build (SVGControl.dll and dependency DLLs first restored WITHOUT TWAE):

- Command: `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:BuildProjectReferences=false` (NO `/p:Nullable=enable`, NO TWAE)
- EXIT_CODE: 0
- `UtilitiesCS.csproj` has NO `<Nullable>` element, so it is nullable-oblivious by default; CS86xx diagnostics arise ONLY from files carrying a `#nullable enable` pragma. CS86xx severity is warning-vs-error-independent (a `warning CS86xx` here becomes an `error CS86xx` under TWAE), so grepping the non-TWAE build output for `warning CS86` yields the exact TWAE CS86xx error count.

## Output Summary

- Pre-remediation CS86xx count for `UtilitiesCS/OutlookObjects/{MailItem,Item,Conversation,Attachment,Table}/`: **0**.
  - All 30 in-scope files are nullable-oblivious at baseline (no whole-file pragma). `MailItemHelper.Html.cs`'s existing interior `#nullable enable`/`#nullable disable` region (lines 107–144, wrapping only `_emailHeader`) emits 0 CS86xx at baseline.
- Total CS86xx across UtilitiesCS at baseline: 0.
- Remediation adds a per-file `#nullable enable` pragma to each in-scope file, which will SURFACE CS86xx that this feature then drives back to 0 batch by batch.
- Confirmed: NO `/p:Nullable=enable` passed; NO `<Nullable>` element in `UtilitiesCS.csproj`.
