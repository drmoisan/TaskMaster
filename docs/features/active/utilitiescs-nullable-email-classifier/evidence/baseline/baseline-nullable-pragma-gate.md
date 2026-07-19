# Baseline — Per-File Nullable Pragma Gate (Clean Pre-State)

Timestamp: 2026-07-19T00-25

## Solution-wide form (plan P0-T5 literal command)

Command: `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` (WITHOUT `/p:Nullable=enable`)

EXIT_CODE: 1

Output Summary: Build FAILED with exactly 2 Error(s), both pre-existing and out of scope:
- `SVGControl/SvgImageSelector.cs(56,25): error CS0649` — `_relativeImagePath` never assigned.
- `SVGControl/SvgImageSelector.cs(57,25): error CS0649` — `_absoluteImagePath` never assigned.
**CS86xx count: 0.** No nullable diagnostic exists in the pre-state; no in-scope file has opted into `#nullable enable`, so non-opted files remain null-oblivious. The only solution-wide TWAE failure is the pre-existing vendored `SVGControl` CS0649 (never-assigned field), which predates this feature, is unrelated to nullable pragma work, and is not in the `UtilitiesCS/EmailIntelligence` remediation scope. This establishes the pre-state: zero CS86xx solution-wide; `/p:Nullable=enable` was not passed.

## Scoped per-batch gate form (authoritative for AC1 measurement)

The per-batch gate is scoped to `UtilitiesCS/UtilitiesCS.csproj`. Two mechanically-necessary adaptations are required to run the plan's per-file pragma gate against a standalone legacy (packages.config, non-SDK) project without letting pre-existing out-of-scope diagnostics mask the CS86xx signal:

1. `-p:Platform=AnyCPU` (not `"Any CPU"`). A standalone legacy project resolves its OutputPath on the literal platform token `AnyCPU`; the solution maps the solution platform "Any CPU" to the project platform "AnyCPU". Passing `"Any CPU"` to a standalone project build produces `error : BaseOutputPath/OutputPath is not set`.
2. `-p:WarningsNotAsErrors=CS0649;CS0618;CS0168`. Under `/t:Rebuild`, the UtilitiesCS build cascades into the vendored `SVGControl` project (a ProjectReference) which emits the pre-existing CS0649, and UtilitiesCS itself emits pre-existing CS0618 (28x, obsolete-member usage) and CS0168 (2x, unused local). All three are pre-existing, non-nullable, out-of-scope warnings that TWAE would otherwise promote to errors and abort the gate. None is a CS86xx code, so exempting them leaves the nullable measurement fully intact: any CS86xx from a pragma-enabled in-scope file is still enforced as an error.

Baseline scoped gate result (no in-scope pragmas applied):
`msbuild UtilitiesCS/UtilitiesCS.csproj -t:Rebuild -p:Configuration=Debug -p:Platform=AnyCPU -p:TreatWarningsAsErrors=true -p:WarningsNotAsErrors=CS0649;CS0618;CS0168`
EXIT_CODE: 0 — Build succeeded. 0 Error(s), 17 Warning(s). CS86xx count: 0.

This scoped gate is the authoritative per-batch AC1 measurement. It confirms zero CS86xx in the pre-state and provides a clean baseline so any CS86xx introduced by a batch pragma is attributable to that batch. `/p:Nullable=enable` is not passed in either form.
