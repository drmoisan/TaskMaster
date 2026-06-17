# Phase 0 Context Read — Remediation Cycle 1 (#177)

- Timestamp: 2026-06-12T16-08 (UTC)
- Task: [P0-T2]
- Executor: atomic-executor

## Files read

- `remediation-inputs.2026-06-12T15-54.md` (authoritative scope source)
- `code-review.2026-06-12T15-43.md`
- `UtilitiesCS/EmailIntelligence/ClassifierGroups/OlFolder/OlFolderClassifierGroup.cs`
- Caller `UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailFiler.cs` (GetFolderPredictorAsync seam at lines 368-372; callers at 374-393)
- Caller `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs` (250-256 / 583-585)
- Caller `UtilitiesCS/OutlookObjects/Folder/FolderScorer.cs` (161-172)
- `UtilitiesCS/EmailIntelligence/ClassifierGroups/ManagerAsyncLazy.cs` (HARD: must not modify)
- `UtilitiesCS/Interfaces/IGlobals/IAppAutoFileObjects.cs` (F1 holder declaration target)
- `TaskMaster/AppGlobals/AppAutoFileObjects.cs` (F1 holder impl target, near Manager at line 609)
- `UtilitiesCS/EmailIntelligence/Bayesian/FolderHierarchyTree.cs` (F2 target, 86.4% strict)
- `UtilitiesCS/EmailIntelligence/Bayesian/LcppnFolderPredictor.cs` (F2 target, 89.1% strict)

## Scope confirmation

Cycle 1 has exactly two in-scope findings, confirmed against the authoritative
`remediation-inputs.2026-06-12T15-54.md`:

- F1 [Major]: the flag-on LCPPN path is unreachable in production because
  `_lcppnPredictor` is per-instance state, while the three production callers
  (`EmailFiler`, `SortEmail`, `FolderScorer`) each construct a fresh
  `new OlFolderClassifierGroup(globals)` per call. The built predictor on the
  registration-site instance is therefore never returned to a caller instance.
  The chosen F1 holder location is `IAppAutoFileObjects.FolderPredictor` (a Folder-only
  nullable `IFolderPredictor` holder on the shared `globals.AF` surface), declared in
  `IAppAutoFileObjects.cs` and implemented in `AppAutoFileObjects.cs`. All three callers
  already read `globals.AF.Manager`, so a holder on `globals.AF` is reachable from every
  fresh per-call instance.
- F2 [Minor]: raise strict new-code line coverage to >= 90% for `FolderHierarchyTree.cs`
  (86.4%) and `LcppnFolderPredictor.cs` (89.1%) with deterministic in-memory MSTest tests.

HARD constraints confirmed: the fix must not retype the shared
`Globals.AF.Manager` value type, must not modify `ManagerAsyncLazy.cs`, and must not
touch `Triage.cs`, `SpamBayes.cs`, `CategoryClassifierGroup.cs`, or `MulticlassEngine.cs`.
Flag-off behavior (AC13) must remain byte-for-byte unchanged.
