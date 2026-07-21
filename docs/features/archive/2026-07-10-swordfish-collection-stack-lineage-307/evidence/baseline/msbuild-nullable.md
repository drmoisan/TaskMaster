# Phase 0 — Baseline MSBuild Nullable Build (P0-T5)

Timestamp: 2026-07-11T03-07
Command: `MSBuild.exe TaskMaster.sln -t:Build -p:Configuration=Debug -p:Platform="Any CPU" -p:Nullable=enable -p:TreatWarningsAsErrors=true`
EXIT_CODE: 1

Output Summary: Build fails at baseline with 84 nullable errors, ALL confined to vendored /
third-party projects that are outside F2 scope and F5-reserved:

- `UtilitiesSwordfish.NET.General.csproj` — 50 errors
- `SVGControl.csproj` — 34 errors

Distinct codes: CS8625 (26), CS8618 (26), CS8603 (9), CS8600 (8), CS8602 (6), CS8601 (5),
CS0649 (2), CS8619 (1), CS8604 (1).

First-party project (UtilitiesCS, TaskMaster, QuickFiler, TaskVisualization, and their .Test
projects) nullable error count at baseline: 0. The vendored projects build first under the
solution graph and their failures prevent the first-party projects from completing a nullable
compile; consequently the `TreatWarningsAsErrors` nullable gate is pre-existingly RED in this
repository due to vendored debt only.

No-regression obligation for F2: the full baseline error set is preserved verbatim in
`nullable-baseline-errors.txt` (84 lines). After the F2 migration, the nullable build must
produce the SAME vendored-only error set with ZERO new first-party nullable diagnostics. Per
CLAUDE.md/csharp.md the operative first-party type-safety gate is the analyzer build
(P0-T4/P8-T2), which compiles all first-party projects and is green (0 errors) at baseline.
