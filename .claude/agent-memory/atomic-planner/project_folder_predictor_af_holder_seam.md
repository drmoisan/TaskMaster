---
name: folder-predictor-af-holder-seam
description: F1 fix for #177 routes the flag-on LCPPN predictor through a Folder-only holder on IAppAutoFileObjects (globals.AF), not per-instance OlFolderClassifierGroup state
metadata:
  type: project
---

The flag-on LCPPN path in #177 was unreachable in production because `_lcppnPredictor`/`FolderPredictorConfig` were per-instance state on `OlFolderClassifierGroup`, while callers (`EmailFiler.cs`, `SortEmail.cs`, `FolderScorer.cs`) construct a fresh `new OlFolderClassifierGroup(globals)` per call.

**Why:** All three callers share the same `globals` and already read `globals.AF.Manager`. A holder on the `IAppAutoFileObjects` (AF) surface is reachable by every fresh per-call instance, so the built predictor is shared without retyping `Manager` or touching `ManagerAsyncLazy.cs`. The AF interface is in `UtilitiesCS/Interfaces/IGlobals/IAppAutoFileObjects.cs`; the concrete impl is `TaskMaster/AppGlobals/AppAutoFileObjects.cs` (Manager property ~line 609).

**How to apply:** For a Folder-only seam that must be shared across per-call instances, add a nullable `IFolderPredictor FolderPredictor { get; set; }` on `IAppAutoFileObjects`, set it at the `BuildClassifiersAsync` registration site on a flag-on build, and resolve it in `GetFolderPredictorAsync`. This is a separate Folder holder distinct from `Manager["Folder"]` and does not violate the shared-dictionary constraint in [[manager-asynclazy-shared-seam]].
