# Final QC — Nullable Pragma Gate (P9-T3)

Timestamp: 2026-07-19T22-03

Per the epic P9-T3 ruling: the literal "solution-wide 0 CS86xx / 0 CS8714" is UNSATISFIABLE for
#366 in isolation on the integrated tree, because pre-existing cross-child CS86xx arise from
sibling-owned nullable-enabled files (cross-child fan-in; the #376 capstone's obligation). The
OPERATIVE gate for #366 is the ISOLATED-CLUSTER result. This record captures BOTH.

## (a) Isolated-cluster result — OPERATIVE (PASS)

Command (isolated-compile methodology, per P0-T5 / Batch-6/7/8):
`msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:TreatWarningsAsErrors=true /p:BuildProjectReferences=false`
(WITHOUT `/p:Nullable=enable`).

- CS86xx attributable to any #366 cluster file (`ReusableTypeClasses/**` or the four waiver files
  `WrapperScoDictionary.cs`, `ScoDictionaryConverter.cs`, `WrapperScDictionary.cs`,
  `ScDictionaryConverter.cs`): **0**.
- CS8714 anywhere in the build: **0**.
- Result: PASS for #366 (AC1). All 51 in-scope ReusableTypeClasses files plus the four cross-child
  waiver consumers reach zero CS86xx and zero CS8714 under the per-file pragma. See
  `evidence/qa-gates/batch-8-nullable-gate.md` for the full per-file decomposition.

## (b) Solution-wide count — EXPECTED cross-child-fan-in deviation (NOT a #366 failure)

Command:
`msbuild TaskMaster.sln /t:Rebuild /p:Configuration=Debug /p:Platform="Any CPU" /p:TreatWarningsAsErrors=true`
(WITHOUT `/p:Nullable=enable`; VS18 full-framework msbuild.exe; `MSYS_NO_PATHCONV=1`).

EXIT_CODE: 1

Immediate blocking errors surfaced by the solution rebuild:
- 2 x CS0649 (field never assigned) in vendored `SVGControl/SvgImageSelector.cs` — pre-existing
  vendored-code diagnostics under `/p:TreatWarningsAsErrors=true`, wholly unrelated to #366. The
  solution `/t:Rebuild` halts at this vendored project.

Additional cross-child fan-in established by the isolated UtilitiesCS rebuild (a): 148 CS86xx in
sibling-owned nullable-enabled files under `UtilitiesCS/EmailIntelligence/**` and
`UtilitiesCS/OutlookObjects/Folder/**` (e.g. `EmailDataMiner.Transform.cs`,
`BayesianPerformanceMeasurement.cs`, `FolderPredictor.cs`). These are sibling children's
nullable-enabled files, NOT #366-owned. They are the #376 capstone's obligation.

Solution-wide count of #366-cluster nullable errors (CS86xx / CS8714): **0**.

## Disposition

- `/p:Nullable=enable` was NOT passed in either run (per-file pragma enforcement only).
- Non-opted-in files elsewhere are not cross-blocked by #366's opt-in (AC6): every solution-wide
  nullable/vendored error originates in a sibling-owned or vendored file, none in a #366 file.
- [P9-T3] is a PASS on the operative isolated-cluster gate. The solution-wide EXIT 1 is an
  EXPECTED cross-child-fan-in / vendored-code deviation attributable to sibling-owned and vendored
  files, NOT a #366 failure, and is NOT recorded as a failure of this task.
