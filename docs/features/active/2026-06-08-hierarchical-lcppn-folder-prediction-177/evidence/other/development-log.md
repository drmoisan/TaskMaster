# Development Log — hierarchical-lcppn-folder-prediction (#177)

Plan: `plan.2026-06-08T09-23.md`
Executor: atomic-executor

## Phase 0 — Compliance & Context

### [P0-T1] General policy reads

- Timestamp: 2026-06-10T12-31 (UTC)
- Files read before any code change:
  - `CLAUDE.md` — General Code Change Policy section, General Unit Test Policy section
  - `.claude/rules/general-code-change.md`
  - `.claude/rules/general-unit-test.md`

### [P0-T2] C# policy reads

- Timestamp: 2026-06-10T12-31 (UTC)
- Files read:
  - `CLAUDE.md` — C# Code Change Policy section, C# Unit Test Policy section
  - `.claude/rules/csharp.md`
  - `.claude/skills/csharp-qa-gate/SKILL.md`
- Toolchain commands restated exactly as written in policy (run in this exact order; restart from step 1 on any failure or auto-fix):
  1. `dotnet tool run csharpier .`
  2. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  3. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
  4. `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`

### [P0-T3] Tonality policy read

- Timestamp: 2026-06-10T12-31 (UTC)
- File read: `.claude/rules/tonality.md`
- Confirmation: all plan-execution output will use neutral professional tone; no humor, hyperbole, or decorative metaphor.

### [P0-T4] Baseline toolchain verification (clean tree)

- Timestamp: 2026-06-10T12-38 (UTC)
- All four toolchain steps completed on the unchanged tree. Evidence: `evidence/baseline/2026-06-10T12-31/step1-csharpier.md`, `step2-analyzers.md`, `step3-nullable.md`, `step4-vstest-coverage.md`, `vstest-run.log`.
- Note: `nuget restore TaskMaster.sln` was required once (fresh worktree); 168 packages restored before the msbuild steps.

### [P0-T5] Canonical pre-change coverage baseline

- Timestamp: 2026-06-10T12-38 (UTC)
- Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage /InIsolation`, converted via `Microsoft.CodeCoverage.Console.exe merge ... -f xml`.
- Canonical baseline: `artifacts/csharp/coverage.xml`; copy at `evidence/baseline/2026-06-10T12-31/coverage.xml`.
- Baseline repository line coverage (production assembly in scope, `UtilitiesCS.dll`): **85.31%** strict-covered (35047/41083); 87.49% including partially covered lines. Test run: 3814/3814 passed.

### [P0-T6] Baseline flat folder path behavior and seam points

- Timestamp: 2026-06-10T12-42 (UTC)
- Verified seam points (file:line, current tree):
  - `UtilitiesCS/EmailIntelligence/Bayesian/BayesianClassifierGroup.cs:15` — `class BayesianClassifierGroup : SmartSerializable<BayesianClassifierGroup>`; `UnTrain(string, IEnumerable<string>, int)` at line 91; `Train(string, IEnumerable<string>, int)` at line 146; caller-used `Classify(string[])` overload at line 229 returning `OrderedParallelQuery<Prediction<string>>`.
  - `UtilitiesCS/ReusableTypeClasses/NewSmartSerializable/SmartSerializable.cs:426` — `public void Serialize()` (inherited by `BayesianClassifierGroup`; deferred-write via `RequestSerialization`).
  - `UtilitiesCS/EmailIntelligence/ClassifierGroups/OlFolder/OlFolderClassifierGroup.cs:211` — `Globals.AF.Manager["Folder"] = classifierGroup.ToAsyncLazy();` inside `BuildClassifiersAsync` (registration site for P6-T3); `BuildFolderClassifiersAsync` at line 112 (unchanged by plan).
  - `UtilitiesCS/EmailIntelligence/ClassifierGroups/ManagerAsyncLazy.cs:27-28` — `ManagerAsyncLazy : ConcurrentObservableDictionary<string, AsyncLazy<BayesianClassifierGroup>>`; type-parameter surface: `GetAsyncLazyClassifierLoader` (line 242, returns `AsyncLazy<BayesianClassifierGroup>`, calls `BayesianClassifierGroup.Static.DeserializeAsync` at line 283), `GetAltLoader` (line 293, returns `Func<BayesianClassifierGroup>`).
  - `UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailFiler.cs` — `SerializeFolderManagerAsync` line 369 (`.Serialize()`), `UnTrainFolderAsync` line 375 (`.UnTrain(...)`), `TrainFolderAsync` line 381 (`.Train(...)`); no literal `(BayesianClassifierGroup)` casts; type flows from `await Globals.AF.Manager["Folder"]`.
  - `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs:250` — `.Train(...)`; `SortEmail.cs:582` — `.UnTrain(...)`; no literal casts.
- Additional caller discovered (not listed in the plan's seam tasks): `UtilitiesCS/OutlookObjects/Folder/FolderScorer.cs:161,168` — `(await globals.AF.Manager["Folder"]).Classify(mailInfo.Tokens)`. This resolves through the `Classify(string[])` overload declared on `IFolderPredictor`, so the P6-T3 type change should compile here without modification; will be verified during Phase 6.
- Flat-path baseline behavior: `Train` adds via `Classifiers.GetOrAdd(tag, ...)` then per-classifier `Train(matchFrequency, emailCount)`; `UnTrain` decrements and removes the tag when `MatchEmailCount <= 0`; `Classify` scores all classifiers in parallel via `Chi2SpamProb`, filters by `MinimumProbability`, orders descending; `Serialize()` writes via the deferred timer when `Config.Disk.FilePath != ""`.
