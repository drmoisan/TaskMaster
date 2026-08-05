# [P2-T2] csharpier format — Final QC Pass 1

Timestamp: 2026-08-04T19-56

Command: `dotnet tool run csharpier format .`

EXIT_CODE: 0

Output Summary:

- Tool output: `Formatted 1466 files in 1257ms.` That figure is the number of files csharpier
  **processed**, not the number it rewrote.
- **Files reformatted: 0.** Verified by content comparison rather than by the tool's summary line:
  the two files this feature touched in `[P2-T1]` are byte-identical before and after the format run.
  - `SVGControl/SvgRenderer.cs` — 497 lines before format, 497 lines after; the replaced comment at
    `:397-400` is unchanged.
  - `SVGControl.Test/SvgRendererParseContractTests.cs` — 332 lines before format, 332 lines after; the
    new `GetSvgDocumentOrThrow_WithTheBuiltInDefaultImage_ReturnsADocument` method body and the
    class-level XML doc are unchanged.
  - `git status --porcelain -- '*.cs'` lists exactly those two files as modified relative to `82badeba`,
    which is the `[P2-T1]` change set, with no additional `.cs` file touched by the formatter.
- This is consistent with the inherited state, in which `dotnet tool run csharpier check .` already
  exited 0 across 1466 files, and it satisfies the `[P2-T8]` premise that a single consecutive clean
  pass is reachable without a loop restart.
- Coverage of scope: the run was invoked from the repository root, so it covered
  `SVGControl/SvgRenderer.cs`, `SVGControl/SvgAssemblyProbe.cs`, and every file under `SVGControl.Test/`.
