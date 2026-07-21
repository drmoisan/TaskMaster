# Misc Batch — Nullable Pragma Gate

- Timestamp: 2026-07-19T12-25
- Task: [P6-T3]
- Misc files: `UtilitiesCS/WindowsAPI/ExtraDeclarations.cs`, `UtilitiesCS/Properties/AssemblyInfo.cs`

## Step 1 — CSharpier format

- Command: `dotnet tool run csharpier format .`
- EXIT_CODE: 0
- Output Summary: `Formatted 1406 files`. Only the 2 misc files changed.

## Step 2 — Authoritative CS86xx detector (scoped isolated UtilitiesCS build)

- Command: `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:TreatWarningsAsErrors=true /p:WarningsNotAsErrors=CS0649%3BCS0618%3BCS0168 /p:BuildProjectReferences=false`
- EXIT_CODE: 0
- CS86xx count: 0
- Output Summary: `0 Error(s)`. Zero CS86xx for both misc files.

## Step 3 — Plan-mandated solution-wide command (for the record)

- Command: `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true`
- EXIT_CODE: 1
- CS86xx count: 0
- Output Summary: Aborts on pre-existing vendored SVGControl CS0649 (invariant); zero CS86xx.

## Verify-only outcome (P6-T2)

Both misc files reached zero CS86xx under the pragma with NO annotation edits (pragma add only), as
research predicted: `ExtraDeclarations.cs` is entirely commented out (only usings + an empty
`namespace Windows.Win32` body), and `AssemblyInfo.cs` contains only assembly-level attributes. No
source line beyond the `#nullable enable` pragma was changed in either file; no post-condition
attribute added.

Result: AC1 satisfied for the misc batch.
