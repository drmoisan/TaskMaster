# Verify-Only: Pre-Enabled Files (IAsyncEnumerableExtensions.cs, NullExtensions.cs)

Timestamp: 2026-07-19T01-30

Command: `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:TreatWarningsAsErrors=true /p:BuildProjectReferences=false` (per-file pragma gate; WITHOUT /p:Nullable=enable)

EXIT_CODE: 1 (build overall FAILED only on pre-existing non-nullable CS0168/CS0618; see below)

Output Summary:
- CS86xx (nullable) diagnostics attributable to the two already-`#nullable enable` files IAsyncEnumerableExtensions.cs and NullExtensions.cs: 0. Total CS86xx across the whole UtilitiesCS compilation at baseline: 0.
- No edits were made to either file; both remain unmodified (git status shows no changes to these two files).
- The non-zero exit is due solely to pre-existing non-nullable warnings promoted by TreatWarningsAsErrors (CS0168 x2, CS0618 x28) in unrelated UtilitiesCS production code, not to any nullable diagnostic in the verify-only files.

Conclusion: Both pre-enabled files pass the per-file pragma gate with zero CS86xx and require no changes (AC1 already satisfied for these two files at baseline).
