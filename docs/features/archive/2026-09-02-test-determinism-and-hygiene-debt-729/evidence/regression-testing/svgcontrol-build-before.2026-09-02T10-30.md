# SVGControl.Test pre-deletion build (P3-T3)

Timestamp: 2026-09-02T23-23

Command: `& $msbuild SVGControl.Test\SVGControl.Test.csproj /t:Rebuild /m /p:Configuration=Debug /p:Platform=AnyCPU`

EXIT_CODE: 0

Output Summary:

- `Build succeeded.` with `0 Warning(s)` and `0 Error(s)`.
- `Test-Path SVGControl.Test\bin\Debug\SVGControl.Test.dll` returns `True`.
- Tool resolution used the Block K prelude; `/p:Platform=AnyCPU` (no space) is the
  single-project spelling required by D6.
- This build is the pre-deletion state: `SVGControl.Test/SVGControl.Test.csproj` still compiles
  `Form1.cs`, `Form1.Designer.cs`, `Form2.cs`, and `Form2.Designer.cs`, and the newly added
  `NoLiveFormInTestAssemblyTests.cs` guard is compiled alongside them. The resulting assembly is
  what P3-T4 runs the guard against.
- Re-run note: this artifact records the build performed after revision round 16 rewrote Block E's
  `because` argument. The earlier round-14 build of this same project is superseded by it.
