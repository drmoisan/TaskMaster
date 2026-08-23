# Production File-Size Gate — 500-line limit (Issue #418, task P1-T19)

Timestamp: 2026-08-04T18-57

Issue: #418
Plan: `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/plan.2026-08-04T14-36.md` (version 0.7)
Task: `[P1-T19]`
Branch: `bug/svg-renderer-null-document-nre-418`

> This artifact was **overwritten in full** at plan version 0.7. It previously recorded the
> `SCOPE_EXCEEDED` blocker captured under plan version 0.6, when `SVGControl/SvgRenderer.cs`
> measured 547 lines after a genuine tightening pass. That blocker is resolved by the
> Design Decision 12 extraction recorded below, and this artifact no longer asserts it.

## Command

```
dotnet tool run csharpier format .
pwsh -NoProfile -Command "(Get-Content 'C:\Users\DanMoisan\repos\TaskMaster\SVGControl\SvgRenderer.cs').Count"
pwsh -NoProfile -Command "(Get-Content 'C:\Users\DanMoisan\repos\TaskMaster\SVGControl\SvgAssemblyProbe.cs').Count"
```

EXIT_CODE: 0

## Output Summary

`dotnet tool run csharpier format .` reported `Formatted 1464 files in 4183ms.` and exited 0.
Formatting did not change the line count of either in-scope production file relative to the
pre-format state, so the extraction as authored is csharpier-stable.

Post-`csharpier` line counts:

| File | Lines | Limit | Verdict |
|---|---|---|---|
| `SVGControl/SvgRenderer.cs` | 495 | 500 | PASS (5 lines of headroom) |
| `SVGControl/SvgAssemblyProbe.cs` | 67 | 500 | PASS (433 lines of headroom) |

Both production files are at or below 500 lines, satisfying the file-size limit in
`.claude/rules/general-code-change.md`.

Extraction accounting: `SVGControl/SvgRenderer.cs` measured 547 lines before this task. The two
relocated helper regions — `TryGetDirectoryFromCodeBase` and `GetProbeDirectories`, together with
their explanatory comments and the trailing separator — occupied a contiguous 52-line span at
lines 165-216, giving 547 - 52 = 495. The plan's projection note stated 50 lines and predicted 497;
the measured span is 52 lines and the measured result is 495. This artifact records the **measured**
value, which is what the task's acceptance clause requires.

No fallback was needed. The `ParseFailed` const and every other renderer-state-free member remain on
`SvgRenderer`, and no second `SCOPE_EXCEEDED` escalation was required.

Moved verbatim: both member bodies, signatures, and explanatory comments were relocated
byte-for-byte. Indentation is identical in both nestings (namespace to class to member, 8 spaces),
so no reflow was required. No tightening, rewording, or behavioral change was applied, per this
task's explicit prohibition — the tightening budget was exhausted under plan version 0.6.

`using` set on the new file: `System` (supplies `Uri`, `UriKind`, `StringComparison`,
`StringComparer`), `System.IO` (supplies `Path.GetInvalidPathChars`, `Path.GetDirectoryName`), and
`System.Collections.Generic` (supplies `List<string>`, `HashSet<string>`, `IReadOnlyList<string>`).
The file opens with `#nullable enable` because `SVGControl` does not enable nullable project-wide;
without it the mandated `string?` annotations would emit `CS8632`. Both
`using System.Collections.Generic` and `using System.IO` remain load-bearing in `SvgRenderer.cs`
after the removal, so the move introduced no unused-using diagnostic in either file.

Compile item added: `SVGControl/SVGControl.csproj`

`<Compile Include="SvgAssemblyProbe.cs" />` was added to the explicit compile `<ItemGroup>`,
immediately before the existing `<Compile Include="SvgOptionsConverter.cs" />` item. `SVGControl` is
a legacy non-SDK project with no compile glob, so without this item the new file would not be
compiled. `git diff --stat` reports `1 file changed, 1 insertion(+)` for that project file, and
`file` confirms it remains `UTF-8 (with BOM)` with `CRLF line terminators`. No other change was made
to it, per the Scope Lock's single-item authorization.

Compilation proof: `SVGControl/bin/Debug/SVGControl.dll` was rebuilt by the build recorded below
(file timestamp 18:56:54) and reflection over the built assembly resolves
`SVGControl.SvgAssemblyProbe` with both static members `TryGetDirectoryFromCodeBase` and
`GetProbeDirectories` present. The new file is therefore genuinely compiled rather than silently
omitted from the compile set.

Call-site requalification: the `[P1-T18]` strategy-3 call site in `ResolveByNameAndKey` now reads
`SvgAssemblyProbe.GetProbeDirectories(` (type-qualified). `ResolveByNameAndKey` itself did not move
and remains on `SvgRenderer`, so AC-8's cited `SVGControl/SvgRenderer.cs:44-104` range is unshifted:
the method occupies lines 47-143, entirely before the removed region.

## Build

Command:

```
pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-VSBuild.ps1 -SolutionPath TaskMaster.sln -Configuration Debug -Platform "Any CPU" -EnableNETAnalyzers -EnforceCodeStyleInBuild
```

EXIT_CODE: 0

Result: `Build succeeded.` with `0 Error(s)` and `6 Warning(s)`.

Baseline artifact compared: `evidence/baseline/analyzer-build.2026-08-04T21-04.md`
(`EXIT_CODE: 0`, 0 errors, 6 warnings).

New diagnostics vs baseline: 0

Warning inventory (all six pre-existing and present in the baseline): five instances of the
`System.Reactive 7.0.0` `packages.config`-unsupported warning from
`System.Reactive.PackagesConfigCheck.targets(31,5)` in `UtilitiesCS`, `ToDoModel`, `QuickFiler`,
`TaskMaster`, and `UtilitiesCS.Test`; plus one `CS2002` duplicate-`Compile`-item warning for
`UtilitiesCS.Test/OutlookObjects/Folder/PercentageFormatterTests.cs`. None originates in
`SVGControl` or `SVGControl.Test`, and no `CS86xx` nullable diagnostic appeared, confirming that the
mandated `string?` annotations on the relocated members plus the new file's `#nullable enable`
directive introduce no new diagnostic.

Environmental note: Microsoft Outlook was confirmed not running before this build, so the two
`MSB3061` CoreClean warnings on `leptonica-1.82.0.dll` and `tesseract50.dll` recorded in both
baseline artifacts did not occur.

## Verdict

PASS. Both in-scope production files are at or below the 500-line limit, the solution analyzer gate
is clean at `EXIT_CODE: 0` with zero new diagnostics against the authoritative `2026-08-04T21-04`
baseline, and the extraction is compiled and verified in the built assembly. Task `[P2-T2]`
re-records both counts after the Phase 2 formatting run.
