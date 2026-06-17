---
name: manager-asynclazy-shared-seam
description: Globals.AF.Manager is a single shared dictionary used by all classifier subsystems; do not plan to retype its value parameter for one key
metadata:
  type: project
---

`Globals.AF.Manager` is `ConcurrentObservableDictionary<string, AsyncLazy<BayesianClassifierGroup>>` (declared in `UtilitiesCS/EmailIntelligence/ClassifierGroups/ManagerAsyncLazy.cs`). It is shared by ALL classifier subsystems keyed by string (`"Folder"`, `"Spam"`, `"Actionable"`, Triage `GroupName`, Category prefixes, multiclass `EngineName`), not just `"Folder"`.

**Why:** `AsyncLazy<T>` is `sealed` and invariant, so the value type cannot be widened (e.g., to `IFolderPredictor`) for one key without retyping the whole dictionary. Retyping breaks out-of-scope writers/readers: `Triage.cs` (~149/177/302 writes, ~45 read into a `BayesianClassifierGroup` field), `SpamBayes.cs:222`, `CategoryClassifierGroup.cs:150`, `MulticlassEngine.cs:173`, plus the `ManagerAsyncLazy` loader (`GetAsyncLazyClassifierLoader`/`GetAltLoader` call `BayesianClassifierGroup.Static.DeserializeAsync` and subscribe `PropertyChanged`, which `IFolderPredictor` lacks). That is a broad cross-subsystem refactor that `.claude/rules/csharp.md` Prohibited Behaviors forbids.

**How to apply:** When planning a seam that needs a single Manager key to return a wider/different type, prefer a key-specific accessor/adapter (e.g., a Folder-only accessor returning `Task<IFolderPredictor>`) that resolves over the existing `AsyncLazy<BayesianClassifierGroup>` entry, rather than retyping the shared dictionary. The Folder read sites (`EmailFiler.cs` Serialize/Train/UnTrain, `SortEmail.cs` Train/UnTrain, `FolderScorer.cs` Classify) use only `IFolderPredictor` members, so a Folder-only seam is sufficient. See [[manager-asynclazy-shared-seam]].
