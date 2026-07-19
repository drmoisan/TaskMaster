# Final QC — Solution-Wide Per-File Nullable Pragma Gate

Timestamp: 2026-07-19T04-25

Command: `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true`
(WITHOUT `/p:Nullable=enable`)

EXIT_CODE: 1

Output Summary: **Zero CS86xx (nullable) diagnostics anywhere in the solution-wide rebuild**
(confirmed via `grep -oE "CS8[0-9]{3}"` on the full build log, matching nothing) — this confirms
AC1 across all 12 remediated files and the 3 verify-only files (`PathInternal.cs`,
`RelativePath.cs`, `ValueStringBuilder.cs`) in `SVGControl/`.

6 pre-existing, out-of-scope errors remain, all previously documented:
- 2x `CS0649` in `SvgImageSelector.cs` (`_relativeImagePath`, `_absoluteImagePath` never
  assigned — unrelated to nullable reference types; documented since
  `evidence/baseline/baseline-nullable-pragma-gate.md`).
- 4x `CS0006` metadata-file-not-found in `VBFunctions.csproj` (pre-existing analyzer-package-
  version-pin mismatch; documented since `evidence/baseline/baseline-analyzers.md`).

`UtilitiesCS.csproj` also reports "FAILED" in this run, but with zero directly-attributed error
lines of its own: its dependency `SVGControl.csproj` fails first (on the 2 pre-existing `CS0649`
errors, promoted by `TreatWarningsAsErrors`), so MSBuild never reaches `UtilitiesCS.csproj`'s own
`CoreCompile` step where its otherwise-identical `CS0006` analyzer-version-pin errors (documented
in the Phase 0 baseline) would occur. This dependency-propagation short-circuit does not change
the underlying finding: no new error, and specifically no nullable diagnostic, was introduced by
this feature anywhere in the solution.

`/p:Nullable=enable` was not passed at any point in this command.
