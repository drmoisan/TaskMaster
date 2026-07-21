# Batch 3 — Nullable Pragma Gate (P3-T3)

Timestamp: 2026-07-19T09-25

## Commands

1. `dotnet tool run csharpier format .` — EXIT_CODE 0 (clean).
2. Pragma gate: `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:TreatWarningsAsErrors=true` (WITHOUT `/p:Nullable=enable`), isolated-compile methodology per P0-T5.

## Output Summary

Batch 3 (3 files: Matrix, DenMatrix, JaggedMatrix/JagMatrix) cluster diagnostics:
- CS86xx count attributed to `ReusableTypeClasses/`: 0 (AC1 for Batch 3)
- CS8714 count: 0
- Pre-existing non-cluster UtilitiesCS TWAE errors: 14 (unchanged; out of scope)

All three matrices constrain `T : struct, IComparable<T>` (value-type element), so no
reference-nullable annotation is needed on the element type. The only remediation was declaring
the reference-type backing array field nullable (`T[,]?` / `T[]?` / `T[][]?`) because an empty or
disposed matrix legitimately leaves it null (each `IsEmpty` getter tests `== null`, and `Dispose`
sets it to null). The existing null guards in `Get`/`Set` already narrow the field before element
access, so no new guard or `!` was required. No post-condition attribute added.
