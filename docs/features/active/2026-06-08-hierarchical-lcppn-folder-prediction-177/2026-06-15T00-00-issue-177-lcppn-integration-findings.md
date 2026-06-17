# Issue #177 — LCPPN Hierarchical Folder Prediction: Integration & Migration Findings

**Date:** 2026-06-15
**Branch:** `TaskMaster-wt-2026-06-08-12-06` (head `31cbb12e`)
**Scope:** Five integration/migration questions. No code changes proposed.

---

## Q1. Which classifiers use this algorithm?

**Short answer:** `LcppnFolderPredictor` is wired exclusively for folder prediction. Spam, triage, actionable, and category classification are not touched.

### Evidence

Two types implement `IFolderPredictor`:

| Type | File | Role |
|------|------|------|
| `BayesianClassifierGroup` | `UtilitiesCS/EmailIntelligence/Bayesian/BayesianClassifierGroup.cs:15-17` | Flat per-leaf Bayesian classifier; existing implementation; now also satisfies the seam interface |
| `LcppnFolderPredictor` | `UtilitiesCS/EmailIntelligence/Bayesian/LcppnFolderPredictor.cs:22-24` | New hierarchy-aware predictor; flag-gated |

`IFolderPredictor` is declared at `UtilitiesCS/EmailIntelligence/Bayesian/IFolderPredictor.cs:13`. Its four members (`Train`, `UnTrain`, `Classify`, `Serialize`) exactly match the surface area used by the three production callers:

| Caller | File:line | How it routes |
|--------|-----------|---------------|
| `EmailFiler` | `EmailFiler.cs:371-372` | `new OlFolderClassifierGroup(Globals).GetFolderPredictorAsync()` |
| `SortEmail` | `SortEmail.cs:251`, `584` | `new OlFolderClassifierGroup(appGlobals).GetFolderPredictorAsync()` |
| `FolderScorer` | `FolderScorer.cs:162`, `169` | `new OlFolderClassifierGroup(globals).GetFolderPredictorAsync()` |

All three callers route through `OlFolderClassifierGroup.GetFolderPredictorAsync()` and receive an `IFolderPredictor`; they do not call `BayesianClassifierGroup` directly.

**Other classifier tasks are not touched.** `Manager["Actionable"]` (`EmailFiler.cs:377`, `406`) and the spam / triage subsystems (`SpamBayes.cs`, `Triage.cs`, `CategoryClassifierGroup.cs`, `MulticlassEngine.cs`) all remain `BayesianClassifierGroup` values in the shared `Manager` dictionary and are unchanged. The cycle-2 audit confirms zero diff in those files (`code-review.2026-06-12T17-14.md`, containment section).

`FolderPredictorEvaluator` (`UtilitiesCS/EmailIntelligence/Evaluation/FolderPredictorEvaluator.cs:54`) accepts `Func<IReadOnlyList<MinedMailInfo>, IFolderPredictor>`, so it works with either implementation but is an offline evaluation harness, not a runtime classifier.

---

## Q2. Migration/rebuild plan for existing classifiers

**Short answer:** LCPPN runs _alongside_ the flat predictor, selected by a boolean flag. The flat `Manager["Folder"]` registration is _always_ updated at build time regardless of the flag. The old Bayesian folder-prediction path is intentionally retained and is the default; LCPPN is additive, not a replacement.

### Feature flag

`LcppnFolderPredictorConfig.UseLcppnPredictor` (bool, default `false`).

- Declared: `LcppnFolderPredictorConfig.cs:20`
- Default set on `OlFolderClassifierGroup`: `OlFolderClassifierGroup.cs:40-41`
- Checked at build time: `OlFolderClassifierGroup.cs:279`
- Checked at prediction time: `OlFolderClassifierGroup.cs:82-84`

### Flag-off behavior

`GetFolderPredictorAsync` returns `await Globals.AF.Manager["Folder"]` — the existing `BayesianClassifierGroup` (`OlFolderClassifierGroup.cs:90`). No behavior change.

### Flag-on behavior

`BuildClassifiersAsync` (`OlFolderClassifierGroup.cs:207-292`) always:

1. Removes the old `Manager["Folder"]` entry (`cs:209`).
2. Rebuilds the flat `BayesianClassifierGroup` from the mined corpus.
3. Serializes it and stores it back in `Manager["Folder"]` (`cs:273`).
4. If the flag is on, additionally calls `BuildLcppnPredictorAsync(collection)` and assigns the result to `Globals.AF.FolderPredictor` (`cs:279-281`).

`GetFolderPredictorAsync` then returns `Globals.AF.FolderPredictor` (the LCPPN predictor) when both conditions are true: flag is on AND `Globals.AF.FolderPredictor is not null` (`cs:82-87`). If the flag is on but no predictor has been built yet, it falls back to flat (`FolderPredictorSeam_Tests.cs:209-223`).

### What remains to do before the flat path can be retired

The flat `Folder.json` is still built and serialized on every `BuildClassifiersAsync` call even when the flag is on. The LCPPN predictor has no file-path registration wired into `Manager.Configuration`; the spec notes it serializes to "a separate file from `Folder.json`" but the file name and load path are not yet wired into the Manager's loader configuration. Concretely:

- No load-on-startup path exists for `LcppnFolderPredictor`: `AppAutoFileObjects.LoadAsync` loads the `ManagerAsyncLazy` (including `Manager["Folder"]`) but has no corresponding load step for `Globals.AF.FolderPredictor`. After an application restart with the flag on, `Globals.AF.FolderPredictor` will be null until `BuildClassifiersAsync` is called again, causing the accessor to silently fall back to flat.
- The flat path cannot be retired until: (a) the LCPPN predictor has a registered load path so it survives restart, and (b) the "also rebuild flat" step in `BuildClassifiersAsync` is made conditional or removed.

---

## Q3. Is LcppnFolderPredictor still a SmartSerializable object?

**Short answer:** Yes. Both `LcppnFolderPredictor` and its predecessor `BayesianClassifierGroup` derive from `SmartSerializable<T>`. They persist differently: `BayesianClassifierGroup` is in the `Manager` dictionary whose configuration is stored externally; `LcppnFolderPredictor` uses `SmartSerializable<T>` for in-memory and to-string serialization but has no registered path in `Manager.Configuration`.

### Evidence

```
LcppnFolderPredictor : SmartSerializable<LcppnFolderPredictor>, IFolderPredictor
   — LcppnFolderPredictor.cs:22-24

BayesianClassifierGroup : SmartSerializable<BayesianClassifierGroup>, IFolderPredictor
   — BayesianClassifierGroup.cs:15-17
```

`LcppnFolderPredictor` stores its complete state in `Nodes` (a `Dictionary<string, PerParentClassifier>` with inline `Corpus`), which is serialized with `[JsonProperty]`. The `Tree` (`FolderHierarchyTree`) is `[JsonIgnore]` and is rebuilt from `Nodes` on deserialization via the `[OnDeserialized]` callback (`LcppnFolderPredictor.cs:76-79`).

The serialization tests confirm (`LcppnFolderPredictor_Serialization_Tests.cs`):
- Round-trips use `SmartSerializable<T>.SerializeToString()` / `DeserializeObject()` (`cs:51-53`).
- No `CorpusInherit` side files are produced; `Corpus` is inline (`cs:133-151`).
- A `Version` field is present for forward migration (`cs:29-30`).

**Serialization gap vs. `BayesianClassifierGroup`:** `BayesianClassifierGroup` instances are loaded at startup via `Manager.LoadAsync` because they are registered in `Manager.Configuration`. `LcppnFolderPredictor` uses the same `SmartSerializable` mechanism for round-tripping but is currently _not_ registered in any load path. Its `Serialize()` method exists (satisfying `IFolderPredictor.Serialize()`), but there is no code path that calls it with a configured file name and then reloads it at startup.

---

## Q4. Do pre-existing folder-scrape / build-from-scratch functions co-exist?

**Short answer:** Yes. The existing `BuildClassifiersAsync` / `BuildFolderClassifiersAsync` routines are unchanged. The LCPPN path reuses their output (the same `MinedMailInfo[]` corpus array) rather than duplicating the scrape. Both paths can run in the same session.

### Evidence

`BuildClassifiersAsync` (`OlFolderClassifierGroup.cs:207`) is the entry point for the full rebuild flow. It:

1. Instantiates `EmailDataMiner(Globals)` (`cs:210`) to access `GetOlFolderTree` and `QueryOlFolderInfo` — the COM-backed folder enumeration routines (`EmailDataMiner.cs:140`, `170`).
2. Loads the mined corpus with `EmailDataMiner.Load<MinedMailInfo[]>(folderPath)` (`cs:228`).
3. Calls `GetOrCreateClassifierGroupAsync` → `CreateClassifierGroupAsync` → `BuildFolderClassifiersAsync` — the existing flat build routines (`cs:247`, `264`).
4. After the flat build, conditionally calls `BuildLcppnPredictorAsync(collection)` with the same `collection` array (`cs:281`).

`BuildLcppnPredictorAsync` (`cs:51-56`) simply calls `LcppnFolderPredictor.Build(collection, FolderPredictorConfig)` in a `Task.Run`. `LcppnFolderPredictor.Build` iterates the same corpus and calls `predictor.Train` per item (`LcppnFolderPredictor.cs:136-145`).

Neither path duplicates the folder scrape or the corpus load; they share the single `collection` array. `GetOlFolderTree` / `QueryOlFolderInfo` are called once. Both classifiers are built in one `BuildClassifiersAsync` call when the flag is on.

**No duplication of folder-enumeration code was introduced.** The LCPPN Build factory takes a corpus that was already assembled by the existing pipeline.

---

## Q5. How to use this — entry points and call path

**Short answer:** Set `FolderPredictorConfig.UseLcppnPredictor = true` on the `OlFolderClassifierGroup` instance before calling `BuildClassifiersAsync`, then call `GetFolderPredictorAsync()` on any fresh `OlFolderClassifierGroup` instance with the same flag set to get an `IFolderPredictor` that dispatches to `LcppnFolderPredictor.Classify`.

### Build-time call path (one-time, admin action)

```
OlFolderClassifierGroup.BuildClassifiersAsync()          OlFolderClassifierGroup.cs:207
  └─ EmailDataMiner.Load<MinedMailInfo[]>(folderPath)       cs:228   (loads mined corpus)
  └─ GetOrCreateClassifierGroupAsync / BuildFolderClassifiersAsync  (builds flat group)
  └─ Globals.AF.Manager["Folder"] = classifierGroup.ToAsyncLazy()   cs:273
  └─ if (FolderPredictorConfig.UseLcppnPredictor == true)           cs:279
       └─ BuildLcppnPredictorAsync(collection)                       cs:56
            └─ LcppnFolderPredictor.Build(collection, config)        LcppnFolderPredictor.cs:112
                 └─ predictor.Train(relativePath, tokens, 1)  per mail item  cs:144
       └─ Globals.AF.FolderPredictor = <built predictor>             cs:281
```

### Prediction-time call path (per email action)

```
EmailFiler / SortEmail / FolderScorer
  └─ new OlFolderClassifierGroup(globals)   (fresh per-call instance)
       must have FolderPredictorConfig.UseLcppnPredictor = true
  └─ GetFolderPredictorAsync()              OlFolderClassifierGroup.cs:80
       condition: UseLcppnPredictor == true
                  && Globals.AF.FolderPredictor is not null    cs:82-84
       → returns Globals.AF.FolderPredictor (the LcppnFolderPredictor)  cs:87
  └─ IFolderPredictor.Classify(tokens)
       → LcppnFolderPredictor.Classify(tokens)                LcppnFolderPredictor.cs:210
            └─ DescendBeam(tokens)                             cs:236
                 └─ PerParentClassifier.ScoreChildren per node
            └─ sort by LogProbability, apply MinimumPathProbability threshold
            └─ returns OrderedParallelQuery<Prediction<string>>
```

### Required flag setting

`FolderPredictorConfig` is a property on `OlFolderClassifierGroup` (`cs:40`). Production callers construct a fresh `OlFolderClassifierGroup(globals)` per call without setting this property, so they get the default `UseLcppnPredictor = false`. For the LCPPN path to activate in production callers (`EmailFiler`, `SortEmail`, `FolderScorer`), the flag must be injected before or at construction — no production injection site exists yet.

The shared holder (`Globals.AF.FolderPredictor`) was introduced exactly to allow fresh per-call instances to resolve the same predictor without carrying a per-instance field (`FolderPredictorSeam_Tests.cs:226-260`), but those instances must still receive a `FolderPredictorConfig` with `UseLcppnPredictor = true`.

---

## Migration Gaps / Open Items

Items that must be addressed before LCPPN can fully replace the flat folder predictor:

1. **No load-on-startup path.** `LcppnFolderPredictor` is not registered in `Manager.Configuration`; `AppAutoFileObjects.LoadAsync` does not set `Globals.AF.FolderPredictor`. After an application restart with the flag on, the accessor falls back to flat until `BuildClassifiersAsync` is manually re-run.

2. **No production flag injection.** The three callers (`EmailFiler`, `SortEmail`, `FolderScorer`) construct `OlFolderClassifierGroup(globals)` with default `FolderPredictorConfig` (flag off). There is no code path that sets `UseLcppnPredictor = true` on per-call instances unless the caller is explicitly written to do so. The shared `Globals.AF.FolderPredictor` holder is wired, but the per-call config flag is not.

3. **LCPPN serialization file path not registered.** `LcppnFolderPredictor.Serialize()` satisfies the `IFolderPredictor` contract, but no `Config.FileName` / `Config.FolderPath` is assigned before the call in the production build path. `EmailFiler.SerializeFolderManagerAsync` (`EmailFiler.cs:374-378`) calls `(await GetFolderPredictorAsync()).Serialize()`, which with flag on would call `LcppnFolderPredictor.Serialize()` — but with an unconfigured `SmartSerializable.Config`, this would either no-op or fail depending on the base class behavior.

4. **Flat build still always runs.** Even with the flag on, `BuildClassifiersAsync` always rebuilds and serializes `Manager["Folder"]` (the flat group). For a final retirement of the flat path, this build step should be made conditional or removed.

5. **Reparenting requires full rebuild.** The spec documents this explicitly (`spec.md` "Reparented folder" section): if a user moves a folder subtree, incremental update cannot handle it; a full `BuildClassifiersAsync` call is required. This is a known design limitation, not a gap in the current implementation.

6. **Pre-existing over-cap production files.** `BayesianClassifierGroup.cs` (515 lines), `FolderScorer.cs` (608 lines), `SortEmail.cs` (1406 lines) were already over the 500-line policy cap before this feature. These are accepted out-of-scope items for separate refactor work.
