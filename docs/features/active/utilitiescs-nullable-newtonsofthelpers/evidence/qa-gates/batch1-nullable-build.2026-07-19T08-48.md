# Batch 1 Nullable Build Verification (P1-T4)

- Timestamp: 2026-07-19T08-48
- Opted-in files (2): `UtilitiesCS/NewtonsoftHelpers/AllInclusiveBinder.cs`, `UtilitiesCS/NewtonsoftHelpers/MonoExtension/MonoExtension.cs`

## Genuine nullable gate (authoritative — actually compiles NewtonsoftHelpers)

- Command: `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:TreatWarningsAsErrors=true /p:WarningsNotAsErrors=CS0649%3BCS0618%3BCS0168 /m` (NO `/p:Nullable=enable`)
- EXIT_CODE: 0
- Output Summary: Build succeeded (`UtilitiesCS -> bin/Debug/UtilitiesCS.dll`). Zero `CS86xx` diagnostics in `NewtonsoftHelpers/` (grep count 0). Zero errors. The only remaining diagnostics are the pre-existing exempted `CS0618`/`CS0168` warnings unrelated to this feature. Because `CS86xx` remains fatal under this gate, EXIT 0 proves both Batch 1 files are nullable-clean under their `#nullable enable` pragmas.

## Exact plan solution command (invariant, per baseline)

- Command: `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` (NO `/p:Nullable=enable`)
- Result: Invariant with the P0-T4 baseline — fails only on the 2 pre-existing vendored `SVGControl` `CS0649` errors, which block the solution build before `UtilitiesCS` compiles. The Batch 1 edits touch only two `NewtonsoftHelpers/` files (pragma + one annotation), which the solution command never reaches; therefore its result cannot differ from the P0-T4 baseline and NO new `CS86xx` is introduced. The exact solution command is executed in full at the final gate (P9-T3). See `evidence/baseline/nullable-build-baseline.2026-07-19T08-48.md` for the full structural explanation.

## Edits applied (annotation-only)

- `AllInclusiveBinder.cs`: added top-of-file `#nullable enable`; changed `GetAssemblies()` return `Assembly[]` -> `Assembly[]?` (deliberate contract: unused stub whose body returns null; plain class, no interface constraint) with a `// why` comment.
- `MonoExtension.cs`: added top-of-file `#nullable enable`; no annotation changes required — the Mono.Reflection `Instruction.Operand` casts operate on the library's oblivious `object` and the existing `is`-pattern branches already narrow, so zero CS86xx surface. Namespace unchanged.
