# Baseline Per-File Nullable Pragma-Gate Build (SVGControl.csproj)

Timestamp: 2026-07-19T00-30

Command: `msbuild SVGControl/SVGControl.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:TreatWarningsAsErrors=true`
(WITHOUT `/p:Nullable=enable`)

EXIT_CODE: 1

Output Summary:

- **CS86xx count: 0.** Confirmed via `grep -c "CS86" <log>` on the full build log: zero
  occurrences. This matches expectation — the 3 already-`#nullable enable` verify-only files
  (`PathInternal.cs`, `RelativePath.cs`, `ValueStringBuilder.cs`) compile clean, and none of the
  12 hand-authored files carry the pragma yet, so no CS86xx is surfaced anywhere.
- **Overall build result: FAILED (2 Errors, 0 Warnings)**, but both errors are pre-existing,
  unrelated to nullable reference types: `CS0649` ("field is never assigned to, and will always
  have its default value null") on `SvgImageSelector.cs(55,24)` (`_relativeImagePath`) and
  `SvgImageSelector.cs(56,24)` (`_absoluteImagePath`). These are ordinary compiler warnings
  (confirmed present as warnings, not yet promoted, in the P0-T4 analyzer-baseline build which
  did not pass `/p:TreatWarningsAsErrors=true`) that `/p:TreatWarningsAsErrors=true` promotes to
  build errors, independent of any nullable annotation. An isolated `csc.exe` experiment (a
  throwaway scratch file, not committed, no production files touched) confirmed nullable
  annotation (`string?`) does **not** suppress CS0649: the field is genuinely never written
  anywhere in `SvgImageSelector.cs` (consistent with the plan's own documented `ImagePath`
  dead-setter finding, to be resolved in Phase 3). This pre-existing condition is out of scope
  for this annotation-only feature (fixing it would require either assigning the field
  somewhere — a behavior change — or suppressing an unrelated diagnostic, neither of which this
  plan authorizes) and will recur, unchanged, at every subsequent per-batch and final pragma-gate
  build in this plan, independent of the CS86xx signal. Each subsequent gate artifact records the
  CS86xx count explicitly as the AC1 signal, separate from the overall exit code.

**Command-syntax note (environment-mechanical, not a plan-scope change):** the plan's literal
`/p:Platform="Any CPU"` value (with a space), when passed to a *direct* `SVGControl.csproj`
build (bypassing `TaskMaster.sln`), does not match any `PropertyGroup` condition in
`SVGControl.csproj` (which only declares `AnyCPU`, no space, matching MSBuild's standalone-project
convention), and fails immediately with `error : The BaseOutputPath/OutputPath property is not
set`. This is unrelated to nullable content. `/p:Platform=AnyCPU` (no space) is used instead for
all direct `SVGControl/SVGControl.csproj` invocations in this plan's execution; the solution-wide
final gate (`msbuild TaskMaster.sln ...`) uses `/p:Platform="Any CPU"` (with space) as written,
since building through `TaskMaster.sln` maps the solution platform name to each project's own
platform value automatically.
