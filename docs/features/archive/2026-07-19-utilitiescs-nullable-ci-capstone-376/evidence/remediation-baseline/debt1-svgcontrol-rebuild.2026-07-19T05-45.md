# Debt 1 — SVGControl Isolated Rebuild (Post-Fix)

Timestamp: 2026-07-19T05-45
Command: `MSBuild.exe SVGControl/SVGControl.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:TreatWarningsAsErrors=true`

Note: `/p:Platform=AnyCPU` (no space) is required for a single-project isolated build; the
project's `PropertyGroup` conditions key on the literal `'Debug|AnyCPU'` combination
(confirmed via `grep -A1 "PropertyGroup Condition" SVGControl/SVGControl.csproj`). Passing
`"/p:Platform=Any CPU"` (with a space, as used against the full `.sln`) to a standalone
`.csproj` invocation fails with "The BaseOutputPath/OutputPath property is not set for project"
because the solution-level platform-mapping indirection that resolves `Any CPU` -> `AnyCPU` is
not present outside a `.sln`-scoped build.

EXIT_CODE: 0
Output Summary: Build succeeded. 0 Warning(s), 0 Error(s). CS0649 no longer appears for
`SvgImageSelector.cs`; no new diagnostic is introduced by the pragma edit.
