# [P2-T3] csharpier check — Final QC Pass 1

Timestamp: 2026-08-04T19-56

Command: `dotnet tool run csharpier check .`

EXIT_CODE: 0

Output Summary: 0 files need formatting

- Tool output: `Checked 1466 files in 4898ms.` with no per-file "needs formatting" line and exit code 0,
  which is csharpier's clean result. Zero formatting drift remains in `SVGControl/SvgRenderer.cs`,
  `SVGControl/SvgAssemblyProbe.cs`, or under `SVGControl.Test/`.

## Post-formatting line counts for the five in-scope C# files

Measured with `(Get-Content <path>).Count` under `pwsh -NoProfile`, after `[P2-T2]`'s format run.
Limit is 500 lines per `.claude/rules/general-code-change.md`.

| File | Lines | `<= 500` |
|---|---|---|
| `SVGControl/SvgRenderer.cs` | 497 | yes |
| `SVGControl/SvgAssemblyProbe.cs` | 67 | yes |
| `SVGControl.Test/SvgRendererParseContractTests.cs` | 332 | yes |
| `SVGControl.Test/SvgRendererNullToleranceTests.cs` | 143 | yes |
| `SVGControl.Test/SvgAssemblyProbeDirectoryTests.cs` | 187 | yes |

All five files are at or below the 500-line limit. Largest margin consumed: `SVGControl/SvgRenderer.cs`
at 497, three lines of headroom.

Notes on the two production measurements, which are confirming rather than discovering:

- `SVGControl/SvgRenderer.cs` was 495 lines at the close of `[P1-T19]` and is 497 here. The two-line
  growth is `[P2-T1]`'s replacement comment (2 lines -> 4 lines), which was counted against its
  seven-line budget before the `[P2-T1]` rebuild and is unchanged by formatting.
- `SVGControl/SvgAssemblyProbe.cs` is 67 lines, unchanged from `[P1-T19]`'s post-format measurement.

`SVGControl.Test/SvgRendererParseContractTests.cs` is the one test file whose count moved in this
phase: 312 lines at the close of Phase 1, 332 here, the difference being `[P2-T1]`'s single added
`[TestMethod]` (18 lines) plus the 2-line growth of the class-level XML doc. Its `[TestMethod]` count
is 14, above the nine that `[P1-T20]`'s acceptance clause required.
