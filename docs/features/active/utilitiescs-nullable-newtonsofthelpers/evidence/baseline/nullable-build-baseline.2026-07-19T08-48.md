# Pragma-Only Nullable Build Baseline (P0-T4)

- Timestamp: 2026-07-19T08-48

## Primary — exact plan command (for the record)

- Command: `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` (VS18 MSBuild.exe, `/m`). NO `/p:Nullable=enable` was passed (confirmed).
- EXIT_CODE: 1
- Output Summary: The solution-level pragma-only build fails on exactly 2 distinct PRE-EXISTING errors, both `CS0649` (field never assigned) in the vendored `SVGControl/SvgImageSelector.cs` (lines 55, 56), promoted from warning to error by `/p:TreatWarningsAsErrors=true`. Zero `CS86xx` diagnostics appear. These 2 SVGControl CS0649 errors are present identically on `origin/main` (the CI nullable gate, commit 20d163ac, is expected to be red on pre-existing debt per its own commit message) and are unrelated to nullable annotation or to `NewtonsoftHelpers/`.

### Structural limitation of the plan command (documented, not a defect in my work)

`UtilitiesCS` (which contains `NewtonsoftHelpers/`) has a build-dependency on the vendored `SVGControl` project. Because SVGControl fails first under `/p:TreatWarningsAsErrors=true`, the solution build never reaches or compiles `UtilitiesCS`, so the solution-level plan command cannot, by itself, exercise the `NewtonsoftHelpers/` nullable state. In addition, `UtilitiesCS` itself carries pre-existing NON-nullable warnings that TreatWarningsAsErrors promotes to errors: 14x `CS0618` (obsolete `System.Linq.AsyncEnumerable` overloads) and 1x `CS0168` (unused variable `OlMail` in `AutoFile.cs`). None are `CS86xx` and none are in `NewtonsoftHelpers/`.

## Genuine NewtonsoftHelpers nullable gate (used for per-batch verification)

Because the plan command is structurally blocked by pre-existing vendored/obsolete debt that TreatWarningsAsErrors promotes, the genuine nullable verification for the in-scope files compiles `UtilitiesCS` under the same pragma-only settings (NO `/p:Nullable=enable`) while exempting ONLY the pre-existing non-nullable warning codes, so that any `CS86xx` (nullable) diagnostic introduced by a `#nullable enable` pragma still errors:

- Command: `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:TreatWarningsAsErrors=true /p:WarningsNotAsErrors=CS0649%3BCS0618%3BCS0168 /m`
- EXIT_CODE: 0 (baseline GREEN — `UtilitiesCS -> bin/Debug/UtilitiesCS.dll` produced)
- The exempted codes (`CS0649`, `CS0618`, `CS0168`) are exactly the pre-existing non-nullable warnings that TreatWarningsAsErrors would otherwise promote; `CS86xx` remains fatal, so this gate turns RED if any opted-in `NewtonsoftHelpers/` file emits a nullable diagnostic. This modifies no tracked file — the exemptions are command-line only and do NOT edit `.claude/rules/*` or any `.csproj`, and do NOT add `/p:Nullable=enable`.

## Direct CS86xx measurement in NewtonsoftHelpers (baseline)

- Command: `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild ... /p:EnableNETAnalyzers=true` (warnings-as-warnings) then `grep "NewtonsoftHelpers" | grep -oE "warning CS86[0-9]+"`
- Result: ZERO `CS86xx` diagnostics originate from any `NewtonsoftHelpers/` file at baseline (no file carries a top-of-file `#nullable enable` yet; `NonRecursiveConverter.cs` has only a mid-file pragma). This is the pre-opt-in CS86xx count = 0, matching the plan's expectation for the in-scope tree.

## Verification protocol for Phases 1-8 and P9-T3

For each batch and the final gate:
1. Run the exact plan command (solution `/t:Rebuild /p:TreatWarningsAsErrors=true`) for the record; confirm the result matches this baseline (only the 2 pre-existing SVGControl CS0649; no new CS86xx introduced).
2. Run the genuine gate (`UtilitiesCS.csproj` TWAE with `WarningsNotAsErrors=CS0649%3BCS0618%3BCS0168`) and require EXIT 0; and grep the UtilitiesCS compile output for `CS86` in `NewtonsoftHelpers/` paths, requiring ZERO for the opted-in files.
