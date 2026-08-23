# [P0-T9] Baseline Test and Coverage State — re-capture on VSTO-enabled host

Timestamp: 2026-08-04T21-04

Issue: #418
Plan: `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418/plan.2026-08-04T14-36.md`
Task: `[P0-T9]`
Branch: `bug/svg-renderer-null-document-nre-418`
HEAD: `a5695656e711f98a8ae6ad334115c0f8666c509f`
Base: `ce0c91e6` (PR #419 repository-wide NuGet package update)
vstest.console: `C:\Program Files\Microsoft Visual Studio\18\Community\Common7\IDE\Extensions\TestPlatform\vstest.console.exe`, VSTest 18.8.0 (x64)
dotnet-coverage: `18.5.2.0 [win-x64 - .NET 10.0.10]`

## Command

```
pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug
```

EXIT_CODE: 0

Coverage artifact read: `coverage/coverage.cobertura.xml`
(`C:\Users\DanMoisan\repos\TaskMaster\coverage\coverage.cobertura.xml`, Cobertura version 1.9,
timestamp 1785878330).

## Output Summary

**Test Run Successful.**

| Metric | Value |
|---|---|
| Test assemblies discovered | **9** |
| Total tests | **6112** |
| Passed | **6112** |
| Failed | **0** |
| Skipped | **0** |
| Total time | 58.0824 seconds |

### Test assemblies discovered (9)

`QuickFiler.Test`, `SVGControl.Test`, `Tags.Test`, `TaskMaster.Test`, `TaskTree.Test`,
`TaskVisualization.Test`, `ToDoModel.Test`, `UtilitiesCS.Test`, `VBFunctions.Test` — each from
`<project>\bin\Debug\<project>.dll`.

This is the full set. The `2026-08-04T14-36` capture on the originating host discovered only 6 of 8
because `TaskMaster.Test` and `UtilitiesCS.Test` could not produce build output without the VSTO
utility assemblies. Both build and run here. The ninth assembly is `SVGControl.Test`, which entered
the solution in commit `0162567d` (task P1-T1) and now builds and runs. No assembly was discovered
from `.claude/worktrees/`.

### Repository-wide coverage headline (from `coverage/coverage.cobertura.xml` root element)

| Metric | Numerator / Denominator | Percentage |
|---|---|---|
| Line coverage (`line-rate` = `0.85355`) | **93252 / 109252** | **85.3550%** |
| Branch coverage (`branch-rate` = `0.785353`) | **21448 / 27310** | **78.5353%** |

Root element also records `complexity = 24314`.

Floor check against `.claude/rules/general-unit-test.md` (line >= 85%, branch >= 75%): line
**85.3550% — PASS**; branch **78.5353% — PASS**. Recorded as observation only; no gate is asserted by
this Phase 0 task.

The root numerator and denominator were independently reconciled by summing per-`<line>` elements
across all nine deduped `<package>` elements: 93252 covered of 109252 total lines and 21448 covered
of 27310 total branch conditions — an exact match to the root attributes.

### Per-package coverage (all nine packages)

| Package | Lines covered / valid | Line % | Branches covered / valid | Branch % |
|---|---|---|---|---|
| UtilitiesCS | 68379 / 76065 | 89.8955% | 15828 / 18980 | 83.3930% |
| QuickFiler | 13993 / 17158 | 81.5538% | 2964 / 3982 | 74.4350% |
| TaskMaster | 2762 / 4244 | 65.0801% | 557 / 942 | 59.1295% |
| ToDoModel | 2032 / 3442 | 59.0354% | 468 / 928 | 50.4310% |
| **SVGControl** | **1412 / 3266** | **43.2333%** | **460 / 1140** | **40.3509%** |
| TaskVisualization | 2736 / 3012 | 90.8367% | 649 / 768 | 84.5052% |
| Tags | 1374 / 1480 | 92.8378% | 342 / 374 | 91.4439% |
| TaskTree | 556 / 577 | 96.3605% | 180 / 196 | 91.8367% |
| VBFunctions | 8 / 8 | 100.0000% | 0 / 0 | n/a |

### `SVGControl` package numeric line coverage (required by the task)

**`SVGControl` line coverage: 1412 / 3266 = 43.2333%.** Branch coverage: 460 / 1140 = 40.3509%.

The `<package name="SVGControl">` element's own `line-rate` attribute reads `0.42707728065078443`
(42.7077%) and `branch-rate` reads `0.398972602739726` (39.8973%). The attribute values are computed
by dotnet-coverage from its internal block model and differ slightly from the per-`<line>` count
above. Both readings are recorded; the per-`<line>` figure is the one that reconciles with the root
totals and is used as the baseline for the task P2-T8 comparison.

### `SVGControl` class-level baseline (per-`<line>` counts)

| Class | Source file | Covered / total | Line % |
|---|---|---|---|
| `SVGControl.SvgRenderer` | `SVGControl\SvgRenderer.cs` | **264 / 422** | **62.559%** |
| `SVGControl.RelativePath` | `SVGControl\RelativePath.cs` | 790 / 1392 | 56.753% |
| `SVGControl.SvgImageSelector` | `SVGControl\SvgImageSelector.cs` | 136 / 312 | 43.590% |
| `SVGControl.ValueStringBuilder` | `SVGControl\ValueStringBuilder.cs` | 80 / 414 | 19.324% |
| `SVGControl.PictureBoxSVG` | `SVGControl\PictureBoxSVG.cs` | 48 / 50 | 96.000% |
| `SVGControl.PictureBoxSVG` | `SVGControl\PictureBoxSVG.Designer.cs` | 22 / 28 | 78.571% |
| `SVGControl.ButtonSVG` | `SVGControl\ButtonSVG.cs` | 48 / 76 | 63.158% |
| `SVGControl.ButtonSVG` | `SVGControl\ButtonSVG.Designer.cs` | 22 / 28 | 78.571% |
| `SVGControl.SvgResource` | `SVGControl\ISvgResource.cs` | 2 / 12 | 16.667% |
| `SVGControl.DropDownEditor` | `SVGControl\DropDownEditor.cs` | 0 / 99 | 0.000% |
| `SVGControl.SvgOptionsConverter1` | `SVGControl\SvgOptionsConverter.cs` | 0 / 48 | 0.000% |
| `SVGControl.SvgOptionsConverter` | `SVGControl\SvgOptionsConverter2.cs` | 0 / 48 | 0.000% |
| `SVGControl.SvgResourceConverter` | `SVGControl\SvgResourceConverter.cs` | 0 / 26 | 0.000% |
| `SVGControl.ToggleSwitch` | `SVGControl\ToggleSwitch.cs` | 0 / 62 | 0.000% |
| `SVGControl.ToggleSwitch` | `SVGControl\ToggleSwitch.Designer.cs` | 0 / 23 | 0.000% |
| `SVGControl.SvgFileNameEditor` | `SVGControl\SvgFileNameEditor.cs` | 0 / 104 | 0.000% |
| `SVGControl.SVGParser` | `SVGControl\SVGParser.cs` | 0 / 122 | 0.000% |

`SVGControl.SvgRenderer` at **264 / 422 = 62.559%** is the specific baseline that task P2-T8 must
compare against for the members changed by this feature.

### Denominator note

`SVGControl` production code is measured in this baseline because `SVGControl.Test` is a solution
member and produces a discoverable test assembly. The task P2-T8 "denominator change" scenario has
therefore **already occurred before this baseline was taken**, and the repository-wide line rate
still clears the 85% floor at 85.3550%. Task P2-T8 should compare against these figures rather than
against the `2026-08-04T14-36` figures (repo line rate 25.5305%), which were produced by a run in
which only 6 assemblies executed.

No placeholder values appear in this artifact. Every figure was read from
`coverage/coverage.cobertura.xml` produced by the command above.
