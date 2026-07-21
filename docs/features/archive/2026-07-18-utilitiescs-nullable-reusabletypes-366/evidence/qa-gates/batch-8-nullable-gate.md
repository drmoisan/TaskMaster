# Batch 8 — Nullable Pragma Gate (P8-T2 / P8-T3)

Timestamp: 2026-07-19T22-03

Supersedes the prior 2026-07-19T24-30 STOP-state record. The epic layer subsequently
ratified the FOUR-file cross-child waiver (Option A''), clearing the STOP. This record
captures the result AFTER the three deferred constraint lines were applied.

## Commands

1. `dotnet tool run csharpier format .` — EXIT_CODE 0 (formatted 1406 files; the three
   Batch-8 constraint edits were the only source-diff changes, no unrelated churn — verified
   via `git diff --stat`).
2. Pragma gate (isolated-compile methodology per P0-T5 / Batch-6 / Batch-7):
   `msbuild UtilitiesCS/UtilitiesCS.csproj /t:Rebuild /p:Configuration=Debug /p:Platform=AnyCPU /p:TreatWarningsAsErrors=true /p:BuildProjectReferences=false`
   (WITHOUT `/p:Nullable=enable`; `MSYS_NO_PATHCONV=1`; VS18 full-framework msbuild.exe).

EXIT_CODE: 1 (whole-assembly build; the nonzero exit is caused entirely by (a) PRE-EXISTING,
out-of-scope non-nullable warnings-as-errors documented at baseline P0-T5, and (b) cross-child
fan-in CS86xx from sibling-owned nullable-enabled files under `UtilitiesCS/EmailIntelligence/**`
and `UtilitiesCS/OutlookObjects/**` — NOT #366 cluster files. Decomposed below.)

## Output Summary — #366 opted-in cluster (OPERATIVE gate)

Batch 8 (7 files: `SerializableList`, `ScBag`, `ScoDictionaryStatic`, `ScoDictionaryNew`,
`SloLinkedList`, `SloStack`, `ScDictionary`) plus the four cross-child NewtonsoftHelpers waiver
consumers cluster diagnostics:

- CS86xx (nullable) count attributed to any #366 cluster file (`ReusableTypeClasses/**` or the four
  waiver files `WrapperScoDictionary.cs`, `ScoDictionaryConverter.cs`, `WrapperScDictionary.cs`,
  `ScDictionaryConverter.cs`): **0** (AC1 for Batch 8).
- CS8714 count anywhere in the whole build: **0**. Applying the three additive `where TKey : notnull`
  constraint lines (`ScDictionary`, `WrapperScDictionary`, `ScDictionaryConverter`) cleared the +4
  CS8714 that `ScDictionaryConverter.cs` would otherwise emit at
  `(15,40)/(30,85)/(31,50)/(43,61)`. No FIFTH cross-child CS8714 consumer surfaced; the closed
  four-consumer enumeration holds.

## Whole-assembly error decomposition (all pre-existing or cross-child; ZERO originate in a #366 cluster file)

Nullable (CS8xxx) totals across the whole UtilitiesCS assembly:
- CS8600: 2, CS8601: 16, CS8602: 50, CS8603: 9, CS8604: 55, CS8619: 4, CS8620: 3, CS8625: 9
  (total 148 CS86xx). CS8714: 0. CS8766: 0.

Every CS8xxx error originates in a sibling-owned, out-of-#366-scope file. Distinct emitting files
(all under `UtilitiesCS/EmailIntelligence/**` or `UtilitiesCS/OutlookObjects/Folder/**`):
BayesianPerformanceMeasurement.cs, BayesianSerializationHelper.cs, ActionableClassifierGroup.cs,
CategoryClassifierGroup.cs, ClassifierGroupUtilities.cs, ManagerAsyncLazy.cs, MulticlassEngine.cs,
OlFolderClassifierGroup.cs, SpamBayes.Classify.cs, SpamBayes.Conditions.cs, Triage_OlLogic.cs,
AutoFile.cs, EmailDataMiner.FolderExtraction.cs, EmailDataMiner.Serialization.cs,
EmailDataMiner.Transform.cs, EmailFiler.cs, EmailFilerConfig.cs, SortEmail.cs,
FolderPredictorEvaluator.cs, FlagClassNoItem.cs, IntelligenceConfig.cs,
FilterOlFoldersController.cs, PeopleScoDictionaryNew.cs, SubjectMapEncoder.cs, FolderConverter.cs,
FolderPredictor.cs, FolderScorer.cs, FolderTreeCompatibilityView.cs.

These 148 CS86xx are the cross-child fan-in described by the epic P9-T3 ruling (sibling-owned
nullable-enabled files on the integrated tree; the #376 capstone's obligation). They are NOT a
#366 failure.

Non-nullable pre-existing errors (unchanged from P0-T5 baseline; all out of scope):
- CS0618 (obsolete-API usage): 14 occurrences — pre-existing non-cluster files.
- CS0168 (unused variable): 1 occurrence — pre-existing non-cluster file.

## Constraint placement (ratified `where TKey : notnull`, per [P6-T2] + four-file waiver)

- APPLIED to `ScoDictionaryNew<TKey, TValue>` (Batch 6/prior commit) — clean.
- APPLIED to `ScDictionary<TKey, TValue>` (this task) — clean. Its cross-child cascade lands entirely
  in `WrapperScDictionary.cs` + `ScDictionaryConverter.cs` (both now constrained under the four-file
  waiver). Zero residual CS8714.
- APPLIED to `WrapperScDictionary<TDerived, TKey, TValue>` and
  `ScDictionaryConverter<TDerived, TKey, TValue>` (this task, third+fourth waiver consumers under
  Option A'').
- NOT APPLIED to `ScoDictionaryStatic` — non-generic `public static class` of `Type` extension methods;
  no `TKey` to constrain (mechanically inapplicable; the plan "four generic bases" wording is
  inaccurate for this file).
- NOT APPLIED to `ScBag` — `ConcurrentBag<T>`-based; takes `T`; no `notnull` requirement.

## Scope compliance

- No `System.Diagnostics.CodeAnalysis` post-condition attribute added; no polyfill declared.
- No `record` / `init` / `record struct` conversion.
- `SerializableList.cs` (575, pre-existing >500) remains a single file.
- No NewtonsoftHelpers file other than the four waiver consumers was modified.
- `/p:Nullable=enable` was NOT passed.
