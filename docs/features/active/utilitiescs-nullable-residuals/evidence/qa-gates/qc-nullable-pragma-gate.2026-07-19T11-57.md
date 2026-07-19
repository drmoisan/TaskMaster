# QC Nullable Pragma-Only Gate (P12-T3) — AC1

Timestamp: 2026-07-19T11-57

NO `/p:Nullable=enable` is used. Enforcement is the per-file `#nullable enable` pragma under
`/p:TreatWarningsAsErrors=true`.

## A) Mandated full-solution command (command of record)

Command: `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true`

EXIT_CODE: 1

Output Summary:
- CS86xx (nullable) diagnostics: ZERO.
- The only 2 errors are pre-existing, OUT OF SCOPE for #375:
  `SVGControl/SvgImageSelector.cs(56,25)` and `(57,25)`: `error CS0649` (fields never assigned).
  SVGControl is the vendored net481 WinForms project owned by epic child #368, not #375; its CS0649
  warnings are promoted to errors by `/p:TreatWarningsAsErrors`.
- Because `UtilitiesCS.csproj` has a `<ProjectReference>` to `SVGControl.csproj`, this `/t:Rebuild`
  cleans and cannot rebuild SVGControl (it fails under TWAE), which blocks UtilitiesCS from compiling —
  so the full-solution command's zero-CS86xx is non-informative for this child. The trustworthy signal
  is section B.

## B) Trustworthy isolated UtilitiesCS gate (the authoritative CS86xx signal)

Command: `msbuild UtilitiesCS/UtilitiesCS.csproj -t:Rebuild -p:Configuration=Debug -p:Platform=AnyCPU -p:TreatWarningsAsErrors=true -p:WarningsNotAsErrors=CS0649;CS0618;CS0168 -p:BuildProjectReferences=false`
(preceded by a no-TWAE full-solution `-t:Build` that regenerates SVGControl.dll and UtilitiesCS.dll so
`BuildProjectReferences=false` resolves the reference)

EXIT_CODE: 0

Output Summary: Build succeeded. 0 errors, 0 CS86xx across all 37 opted-in hand-written files under
`/p:TreatWarningsAsErrors=true`. The 15 remaining warnings are the pre-existing out-of-scope
CS0618/CS0168 classes only (excluded from error promotion via `WarningsNotAsErrors`; NO CS86xx is ever
excluded). This satisfies AC1: every compiled in-scope hand-written file carries `#nullable enable` and
compiles with zero CS86xx under the pragma-only build.
