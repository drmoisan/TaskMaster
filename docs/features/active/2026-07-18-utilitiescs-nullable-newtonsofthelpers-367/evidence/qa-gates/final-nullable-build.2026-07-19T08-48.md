# Final Pragma-Only Nullable / TreatWarningsAsErrors Gate (P9-T3)

- Timestamp: 2026-07-19T08-48

## Exact plan command (solution, run in full)

- Command: `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` (VS18 MSBuild.exe, `/m`). `/p:Nullable=enable` was NOT passed (confirmed).
- EXIT_CODE: 1
- Output Summary: The ONLY errors are the 2 pre-existing vendored `SVGControl/SvgImageSelector.cs` `CS0649` (field never assigned), promoted from warning by TreatWarningsAsErrors. ZERO `CS86xx` diagnostics anywhere in the solution; ZERO `CS86xx` in `NewtonsoftHelpers/`. This is IDENTICAL to the P0-T4 baseline — the feature introduced no new nullable diagnostics. The SVGControl CS0649 errors are pre-existing on `origin/main` and out of scope (vendored, not nullable, unrelated to #367).

## Genuine NewtonsoftHelpers nullable gate (authoritative — actually compiles the opted-in files)

- Command: `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:TreatWarningsAsErrors=true /p:WarningsNotAsErrors=CS0649%3BCS0618%3BCS0168 /m` (NO `/p:Nullable=enable`)
- EXIT_CODE: 0
- Output Summary: Build succeeded (`UtilitiesCS -> bin/Debug/UtilitiesCS.dll`), zero errors, zero `CS86xx` in `NewtonsoftHelpers/`. Since `CS86xx` is fatal under this gate (only the pre-existing non-nullable `CS0649`/`CS0618`/`CS0168` are exempted), EXIT 0 proves all 19 opted-in `NewtonsoftHelpers/` files compile with zero nullable diagnostics under their per-file `#nullable enable` pragmas.

No files were changed by this step, so the loop proceeds to P9-T4.
