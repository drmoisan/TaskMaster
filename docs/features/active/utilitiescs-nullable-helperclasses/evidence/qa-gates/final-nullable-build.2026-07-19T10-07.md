# Final QC — Pragma-Only Nullable / TreatWarningsAsErrors Type-Check Gate (Issue #364)

- Timestamp: 2026-07-19T10-07
- Task: [P9-T3]
- `/p:Nullable=enable` was NOT passed (pragma-only, per the critical deviation).

## 1. Authoritative isolated CS86xx verification (all opted-in HelperClasses)

- Command: `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:BuildProjectReferences=false`
- EXIT_CODE: 0
- CS86xx warnings (whole UtilitiesCS project): 0
- CS86xx warnings in `HelperClasses/`: 0
- `#nullable enable` pragma count under `UtilitiesCS/HelperClasses/`: 42 of 43 files (the 43rd, `DvgForm.Designer.cs`, is intentionally non-opted-in per the Designer rule).

Because CS86xx nullable diagnostics are emitted identically whether reported as warnings or (under TreatWarningsAsErrors) errors, and arise only from `#nullable enable` files (project default is oblivious — `UtilitiesCS.csproj` has no `<Nullable>` element), zero CS86xx warnings here is a valid proof of "zero CS86xx across all opted-in HelperClasses files under `/p:TreatWarningsAsErrors=true`".

## 2. Plan-literal full-solution TreatWarningsAsErrors gate

- Command: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true`
- EXIT_CODE: 1
- Errors: 4 x `CS0649` — all in the vendored `SVGControl/SvgImageSelector.cs` (fields `_relativeImagePath`/`_absoluteImagePath` never assigned; 2 unique, counted per-project + summary).
- CS86xx errors: 0. Errors in `UtilitiesCS/HelperClasses/`: 0.

The full-solution gate exit code (1) is identical in cause to the P0-T4 baseline: the pre-existing, out-of-scope vendored `SVGControl` CS0649 (unfixable within the #364 HelperClasses scope; flagged in `evidence/other/`). The HelperClasses annotation work introduced ZERO new diagnostics — no CS86xx and no errors in any opted-in file. See `evidence/baseline/nullable-build-baseline.2026-07-19T08-51.md` and the maintainer flags.

## Output Summary

- Result for #364 scope: PASS — zero CS86xx across all 42 opted-in HelperClasses files under the pragma-only type-check.
- Full-solution gate remains blocked only by the pre-existing vendored SVGControl CS0649 (out of scope, flagged, unchanged from baseline). No new diagnostics elsewhere.
- No files changed by this step; the toolchain loop proceeds without restart.
