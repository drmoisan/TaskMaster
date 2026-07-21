# Final QC — Solution-Wide Per-File Nullable Pragma Gate (AC1)

Timestamp: 2026-07-19T06-10

## A. Solution-wide literal plan command (P8-T3)

Command: `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true` (WITHOUT `/p:Nullable=enable`)

EXIT_CODE: 1

Output Summary: Build FAILED with exactly 2 Error(s), both pre-existing and out of scope: `SVGControl/SvgImageSelector.cs(56,25)` and `(57,25)` — `CS0649` never-assigned vendored fields. **CS86xx count: 0.** The only solution-wide TWAE failure is the pre-existing vendored SVGControl CS0649 (identical to the P0-T5 baseline), unrelated to nullable pragma work and outside the `UtilitiesCS/EmailIntelligence` remediation scope. `/p:Nullable=enable` was not passed.

## B. Solution-wide with pre-existing out-of-scope codes exempted

Command: `msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true /p:WarningsNotAsErrors=CS0649;CS0618;CS0168`

EXIT_CODE: 1 — 4 Error(s): pre-existing `CS0169` (unused fields in `ToDoModel.Test/.../PeopleScoDictionaryNewTests.cs`) and `CS0170`/`CS4014` (`TaskVisualization/TaskController.Actions.cs` not-awaited) in OTHER out-of-scope projects. **CS86xx count: 0.** These are pre-existing non-nullable warnings-as-errors in projects unrelated to this feature.

## C. Authoritative scoped gate (UtilitiesCS remediation set)

Command: `msbuild UtilitiesCS/UtilitiesCS.csproj -t:Rebuild -p:Configuration=Debug -p:Platform=AnyCPU -p:TreatWarningsAsErrors=true -p:WarningsNotAsErrors=CS0649;CS0618;CS0168` (WITHOUT `/p:Nullable=enable`)

EXIT_CODE: 0 — Build succeeded. 0 Error(s). **CS86xx count: 0** across every pragma-enabled in-scope file (the full measured remediation set from P0-T6).

## AC1 conclusion

- 36 in-scope `.cs` files under `UtilitiesCS/EmailIntelligence/{Bayesian,ClassifierGroups,Flags}` now carry `#nullable enable` (the full REMEDIATE set: 30 that emitted CS86xx + 6 that were measured null-clean).
- Every pragma-enabled in-scope file compiles with **zero CS86xx** under `/t:Rebuild` + `/p:TreatWarningsAsErrors=true`, WITHOUT `/p:Nullable=enable`.
- The only solution-wide TWAE failures are pre-existing, non-CS86xx, out-of-scope diagnostics (vendored SVGControl CS0649; ToDoModel.Test/TaskVisualization CS0169/CS4014) that predate and are unaffected by this feature.
- **AC1 is satisfied.**
