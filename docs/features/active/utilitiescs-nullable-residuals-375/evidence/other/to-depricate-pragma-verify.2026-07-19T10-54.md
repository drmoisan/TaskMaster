# To Depricate Batch Pragma Verification (P10-T3)

Timestamp: 2026-07-19T10-54

Opted-in files (2, deprecation-marked — remediated annotation-only; deletion remains a maintainer
decision recorded in spec.md Maintainer Decisions item 3):
1. UtilitiesCS/To Depricate/FileIO2.cs — `CSV_ReadTxtF` and `CsvRead` returns → `string[]?` (both have
   `return null`); `CsvReadTo2D`/`CsvReadToJagged` locals `string[]? array1D` with justified `!` at the
   `SplitArrayTo2D(array1D!, ...)` and `array1D!.Select(...)` call sites, preserving the current NRE
   behavior (compile-time-only; a null read still throws at the same dereference as before).
2. UtilitiesCS/To Depricate/StringManipulation.cs — pragma only; verify-only clean (returns a non-null
   `Regex.Replace` result).

## Trustworthy isolated CS86xx gate

Command: `msbuild UtilitiesCS/UtilitiesCS.csproj -t:Rebuild -p:Configuration=Debug -p:Platform=AnyCPU -p:TreatWarningsAsErrors=true -p:WarningsNotAsErrors=CS0649;CS0618;CS0168 -p:BuildProjectReferences=false`

EXIT_CODE: 0

Output Summary: Build succeeded. 0 errors, 0 CS86xx, 15 pre-existing out-of-scope warnings. No
behavior change; no new runtime guard.
