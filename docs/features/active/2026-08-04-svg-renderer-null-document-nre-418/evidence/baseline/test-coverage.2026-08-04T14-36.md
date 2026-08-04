# Baseline — MSTest Run with Coverage (Issue #418)

Task: `[P0-T9]`
Feature: `docs/features/active/2026-08-04-svg-renderer-null-document-nre-418`

Timestamp: 2026-08-04T15-02

Command: `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug`

Working directory: repository root (`c:\Users\DanMoisan\source\repos\drmoisan\TaskMaster`)

Coverage artifact read: `coverage/coverage.cobertura.xml`

EXIT_CODE: 0

Output Summary: `Test Run Successful.` Total tests: **896**; passed: **896**; failed: **0**;
skipped: **0**. Total time `1.3459 Minutes`. Repository-wide coverage read from the
Cobertura root element: **line-rate 25.5305%** (`lines-covered 24628` of
`lines-valid 96465`) and **branch-rate 20.6824%** (`branches-covered 4910` of
`branches-valid 23740`). The `SVGControl` package element reports **line-rate 16.1047%** and
branch-rate 7.3630%. The `SVGControl.SvgRenderer` class, which this change modifies, reports
line-rate **62.5592%** and branch-rate 43.3333%. Six test assemblies were discovered and run;
`UtilitiesCS.Test` and `TaskMaster.Test` were **not** present on disk and therefore did not
participate — see the denominator caveat below.

## Cobertura Root Element (verbatim)

```xml
<coverage line-rate="0.255305" branch-rate="0.206824" complexity="22869" version="1.9"
          timestamp="1785873845" lines-covered="24628" lines-valid="96465"
          branches-covered="4910" branches-valid="23740">
```

| Metric | Raw value | Percentage |
| --- | --- | --- |
| Repository-wide line-rate | `0.255305` | **25.5305%** |
| Repository-wide branch-rate | `0.206824` | **20.6824%** |
| Lines covered / valid | `24628` / `96465` | — |
| Branches covered / valid | `4910` / `23740` | — |

## Per-Package Coverage

| Package | line-rate | Line % | branch-rate | Branch % |
| --- | --- | --- | --- | --- |
| `QuickFiler` | `0.7228238519533927` | 72.2824% | `0.6232394366197183` | 62.3239% |
| `UtilitiesCS` | `0.10701234567901234` | 10.7012% | `0.09149741138988451` | 9.1497% |
| **`SVGControl`** | `0.16104651162790698` | **16.1047%** | `0.07363013698630137` | **7.3630%** |
| `TaskVisualization` | `0.8984326018808777` | 89.8433% | `0.8325` | 83.2500% |
| `Tags` | `0.9268929503916449` | 92.6893% | `0.9157894736842105` | 91.5789% |
| `TaskTree` | `0.9548387096774194` | 95.4839% | `0.9215686274509803` | 92.1569% |
| `ToDoModel` | `0.567769477054429` | 56.7769% | `0.4881889763779528` | 48.8189% |
| `VBFunctions` | `1` | 100.0000% | `1` | 100.0000% |

## Per-Class Coverage Inside the `SVGControl` Package

The class this change modifies is `SVGControl.SvgRenderer`.

| Class | line-rate | Line % | branch-rate | Branch % |
| --- | --- | --- | --- | --- |
| **`SVGControl.SvgRenderer`** | `0.6255924170616114` | **62.5592%** | `0.43333333333333335` | **43.3333%** |
| `SVGControl.SvgImageSelector` | `0.423077` | 42.3077% | `0.276596` | 27.6596% |
| `SVGControl.PictureBoxSVG` | `0.96` / `0.7857142857142857` | 96.0000% / 78.5714% | `1` / `0.5` | 100.0000% / 50.0000% |
| `SVGControl.ButtonSVG` | `0.7857142857142857` / `0.631578947368421` | 78.5714% / 63.1579% | `0.5` / `0` | 50.0000% / 0.0000% |
| `SVGControl.SvgResource` | `0.16666666666666666` | 16.6667% | `1` | 100.0000% |
| `SVGControl.SVGParser` | `0` | 0.0000% | `0` | 0.0000% |
| `SVGControl.RelativePath` | `0` | 0.0000% | `0` | 0.0000% |
| `SVGControl.DropDownEditor` | `0` | 0.0000% | `0` | 0.0000% |
| `SVGControl.SvgOptionsConverter` | `0` | 0.0000% | `0` | 0.0000% |
| `SVGControl.SvgOptionsConverter1` | `0` | 0.0000% | `0` | 0.0000% |
| `SVGControl.SvgFileNameEditor` | `0` | 0.0000% | `0` | 0.0000% |
| `SVGControl.SvgResourceConverter` | `0` | 0.0000% | `0` | 0.0000% |
| `SVGControl.ToggleSwitch` | `0` | 0.0000% | `0` | 0.0000% |
| `SVGControl.ValueStringBuilder` | `0` | 0.0000% | `0` | 0.0000% |

`PictureBoxSVG`, `ButtonSVG`, and `ToggleSwitch` each appear as two `<class>` elements
(partial types split across a code-behind file and a Designer file); both rows are reported
above rather than merged, so no value is synthesized.

The measured `SVGControl` coverage comes entirely from incidental execution by other test
assemblies. `SVGControl.Test` is not in the solution, was not built, and contributed nothing
to this run.

## Test Assemblies in the Run

`Discovered 6 test assemblies.`

| # | Assembly |
| --- | --- |
| 1 | `QuickFiler.Test/bin/Debug/QuickFiler.Test.dll` |
| 2 | `Tags.Test/bin/Debug/Tags.Test.dll` |
| 3 | `TaskTree.Test/bin/Debug/TaskTree.Test.dll` |
| 4 | `TaskVisualization.Test/bin/Debug/TaskVisualization.Test.dll` |
| 5 | `ToDoModel.Test/bin/Debug/ToDoModel.Test.dll` |
| 6 | `VBFunctions.Test/bin/Debug/VBFunctions.Test.dll` |

## Denominator Caveat (material; carried to the `[P2-T8]` comparison)

Two test projects that exist in the repository produced **no `bin/Debug` output** at the time
of this run and therefore did not participate:

- `UtilitiesCS.Test` — no DLL under `UtilitiesCS.Test/bin/Debug/`
- `TaskMaster.Test` — no DLL under `TaskMaster.Test/bin/Debug/`

This follows directly from the `[P0-T7]` analyzer build failing on
`TaskMaster/TaskMaster.csproj` (four `CS0234` errors caused by the unresolved VSTO runtime
assemblies `Microsoft.Office.Tools.Outlook.v4.0.Utilities` and
`Microsoft.Office.Tools.Common.v4.0.Utilities`, both reported as `MSB3245`). It is the
principal reason the repository-wide line-rate reads 25.5305% rather than a figure consistent
with prior sessions, and it is why the `UtilitiesCS` package reads 10.7012%.

`SVGControl.Test` likewise contributed nothing, but for a different and expected reason: it is
absent from `TaskMaster.sln` and its packages are not restored (recorded under `[P0-T10]`).

Consequence for the coverage-delta task `[P2-T8]`: the post-change run must be compared
against this baseline only when the participating-assembly set is stated alongside the
numbers. If the post-change run includes `UtilitiesCS.Test`, `TaskMaster.Test`, or
`SVGControl.Test`, the denominator differs and the repository-wide delta is not a like-for-like
comparison. `[P2-T8]` already carries a denominator-change decision rule and a
`COVERAGE_DENOMINATOR_CHANGE` report path for the `SVGControl.Test` case; the two additional
absent assemblies recorded here widen that same caveat.

## Post-Processing Note

The script post-processed the Cobertura XML for Koverage compatibility (workspace-relative
paths, injected `<sources><source>.</source></sources>`, and removal of `<package>` elements
for third-party assemblies). The values above were read from the post-processed artifact at
`coverage/coverage.cobertura.xml`, which is the artifact the plan names.
