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

## Phase 1 — IFolderPredictor seam (reconciliation of committed code)

### Reconciliation context

- Timestamp: 2026-06-12T10-33 (UTC)
- Phase 1 was already implemented and committed (`d674b81b`) before the plan checkboxes were updated. The executor reconciled the committed code against each task's stated acceptance criteria rather than re-implementing.

### [P1-T1] Compile registration verification

- `UtilitiesCS/UtilitiesCS.csproj:563` — `<Compile Include="EmailIntelligence\Bayesian\IFolderPredictor.cs" />`.
- `UtilitiesCS.Test/UtilitiesCS.Test.csproj:114` — `<Compile Include="EmailIntelligence\Bayesian\IFolderPredictor_Tests.cs" />`.
- `UtilitiesCS.Test/UtilitiesCS.Test.csproj:115` — `<Compile Include="EmailIntelligence\Bayesian\BayesianClassifierGroup_FlatPathUnchanged_Tests.cs" />`.
- Verified by successful compilation in both msbuild steps and test discovery.

### [P1-T2] IFolderPredictor interface signature verification

- `UtilitiesCS/EmailIntelligence/Bayesian/IFolderPredictor.cs` declares the four members. Signatures matched exactly against `BayesianClassifierGroup`:
  - `void Train(string tag, IEnumerable<string> matchTokens, int emailCount)` — matches `BayesianClassifierGroup.cs:148`.
  - `void UnTrain(string tag, IEnumerable<string> matchTokens, int emailCount)` — matches `BayesianClassifierGroup.cs:93`.
  - `OrderedParallelQuery<Prediction<string>> Classify(string[] tokens)` — matches `BayesianClassifierGroup.cs:231`.
  - `void Serialize()` — inherited from `SmartSerializable<T>`.

### [P1-T3] BayesianClassifierGroup conformance verification

- `BayesianClassifierGroup.cs:15-18` declares `: SmartSerializable<BayesianClassifierGroup>, IFolderPredictor` additively. No method bodies changed (all four members pre-existed; the interface was satisfied by the existing public surface).

### [P1-T4] / [P1-T5] Tests verification

- `IFolderPredictor_Tests.cs` (MSTest + FluentAssertions): assignability (`BeAssignableTo<IFolderPredictor>`), Train/UnTrain/Classify dispatch through the interface, Serialize callable.
- `BayesianClassifierGroup_FlatPathUnchanged_Tests.cs` (MSTest + FluentAssertions): fixed-corpus ordering descending, determinism across identical instances, base-class vs interface path equality, repeated-call equality. No temp files, no external services.

### [P1-T6] Full C# toolchain gate

- Timestamp: 2026-06-12T10-33 (UTC). Single clean pass:
  1. `dotnet tool run csharpier format .` — EXIT 0; "Formatted 1063 files"; `git status --porcelain` empty (no changes).
  2. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` — EXIT 0; 0 Error(s), 62 Warning(s) (all pre-existing CS0618/CS8632/MSTEST0032/CS0067 in unrelated files; none in Phase 1 files).
  3. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true` — EXIT 0; 0 Warning(s), 0 Error(s).
  4. `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll /EnableCodeCoverage /InIsolation` (filtered to Phase 1 classes) — 9/9 passed.
- Result: Phase 1 acceptance fully satisfied by committed code; no acceptance gap. P1-T1..P1-T6 checked off.
- AC note: AC14 (shared `IFolderPredictor` seam) is partially delivered (flat side proven). Per the AC traceability table, AC14 also depends on P6-T7 (both predictors reachable); AC14 will be checked off after Phase 6.

## Phase 2 — Folder hierarchy model (pure path parsing)

### [P2-T1] Compile registration

- Timestamp: 2026-06-12T10-38 (UTC)
- Added `<Compile Include="EmailIntelligence\Bayesian\FolderHierarchyNode.cs" />` and `<Compile Include="EmailIntelligence\Bayesian\FolderHierarchyTree.cs" />` to `UtilitiesCS/UtilitiesCS.csproj`; added `<Compile Include="EmailIntelligence\Bayesian\FolderHierarchyTree_Tests.cs" />` to `UtilitiesCS.Test/UtilitiesCS.Test.csproj`. Verified by compilation and test discovery at P2-T8.

### [P2-T2..T6] Implementation

- `FolderHierarchyNode.cs` — immutable `sealed record` with `string NodeKey`, `string[] Children`, `[JsonConstructor]` for Newtonsoft, null guards. (P2-T2)
- `FolderHierarchyTree.cs` — pure logic, no Outlook COM / filesystem. `Build(IEnumerable<string>, StringComparer)` parses each path on backslash into root-keyed (empty string) parent->child edges (P2-T3); single-segment paths register one root edge and a zero-child leaf (P2-T4); per-parent `ChildSet` (ordered list + HashSet on the configured comparer) deduplicates children so duplicate paths are idempotent (P2-T5); `AddLeaf(parentKey, childSegment)` adds to one parent only (P2-T6). Default comparer is `StringComparer.Ordinal`; `OrdinalIgnoreCase` is configurable.

### [P2-T7] Tests

- `FolderHierarchyTree_Tests.cs` (MSTest + FluentAssertions, 12 tests): multi-depth construction (AC1), single-segment leaf (AC2), duplicate-path idempotence (AC3), new-leaf locality with pre/post child-set comparison of all other nodes (AC4), empty collection, null/empty entry skipping, ordinal vs OrdinalIgnoreCase comparison, GetNode snapshot, and two fail-fast guards. Deterministic, in-memory, no temp files.

### [P2-T8] Full C# toolchain gate

- Timestamp: 2026-06-12T10-38 (UTC).
  1. `dotnet tool run csharpier format .` — first run reformatted `FolderHierarchyTree.cs` (collapsed `IsLeaf`); restarted from step 1; second run stable (no further changes).
  2. `msbuild ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` — EXIT 0; 0 Error(s), 60 Warning(s) (all pre-existing; none in new files).
  3. `msbuild ... /p:Nullable=enable /p:TreatWarningsAsErrors=true` — EXIT 0; 0 Warning(s), 0 Error(s).
  4. `vstest.console.exe ... /EnableCodeCoverage /InIsolation` (filtered to `FolderHierarchyTree_Tests`) — 12/12 passed.
- Result: all four steps pass in a single final pass. AC1-AC4 checked off in `user-story.md`. New files `FolderHierarchyNode.cs` and `FolderHierarchyTree.cs` are fully exercised by the 12 tests (every public method and record member).

## Phase 3 — PerParentClassifier (shrinkage blend + cold-start fallback)

### [P3-T1] Compile registration

- Timestamp: 2026-06-12T10-42 (UTC)
- Added `<Compile Include="EmailIntelligence\Bayesian\PerParentClassifier.cs" />` to `UtilitiesCS/UtilitiesCS.csproj`; added `<Compile Include="EmailIntelligence\Bayesian\PerParentClassifier_Tests.cs" />` to `UtilitiesCS.Test/UtilitiesCS.Test.csproj`.

### [P3-T2..T6] Implementation

- `PerParentClassifier.cs` — wraps one `BayesianClassifierGroup` whose `Classifiers` are keyed by direct child segment and whose `SharedTokenBase` is the parent-scoped `Corpus` (P3-T2; reuses `BayesianClassifierGroup`/`BayesianClassifierShared`/`Corpus` unmodified).
- Per-child scoring `ScoreChildren` computes a numerically stable softmax over per-child log-scores. Each per-token estimate is the shrinkage blend `λ·P_leaf(t|c) + (1-λ)·P_parent(t|p)` with Laplace add-one smoothing; `P_leaf` uses the child `Match.TokenFrequency`, `P_parent` uses the parent `SharedTokenBase.TokenFrequency` (P3-T3). `ShrinkageLambda` is supplied at construction.
- Cold-start fallback: when `TotalExamples < MinColdStartExamples`, scoring drops the parent term and uses the leaf-only estimate (unsmoothed NB) (P3-T4).
- Count-based `Train(childSegment, tokens, count)` / `UnTrain(...)` delegate to the wrapped group's `Train`/`UnTrain`, which use `GetOrAdd` for new-child registration and per-tag match counts (P3-T5).
- Construction validates `0 <= ShrinkageLambda <= 1` (and rejects NaN) and `MinColdStartExamples >= 0`, throwing `ArgumentOutOfRangeException` with a clear message (P3-T6).

### [P3-T7] Tests

- `PerParentClassifier_Tests.cs` (MSTest + FluentAssertions, 14 tests incl. a 3-row DataTestMethod): blend ranks the matching child (AC9); lambda controls the blend (leaf-only λ=1 vs λ=0.5 differ); cold-start toggles at the threshold and uses leaf-only scoring (AC10); new-child training leaves siblings unchanged; untrain decrements; empty parent returns empty; all scores in [0,1] and sum to 1; invalid lambda (-0.1, 1.1, NaN), negative cold-start, empty child segment, and null tokens all fail fast. Deterministic, in-memory, no temp files, no Outlook COM.

### [P3-T8] Full C# toolchain gate

- Timestamp: 2026-06-12T10-42 (UTC).
  1. `dotnet tool run csharpier format .` — first run reformatted both new files; restarted; second run stable.
  2. `msbuild ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` — EXIT 0; 0 Error(s), 60 Warning(s) (all pre-existing).
  3. `msbuild ... /p:Nullable=enable /p:TreatWarningsAsErrors=true` — EXIT 0; 0 Warning(s), 0 Error(s).
  4. `vstest.console.exe ... /EnableCodeCoverage /InIsolation` (filtered to `PerParentClassifier_Tests`) — 14/14 passed.
- Result: all four steps pass in a single final pass. AC9, AC10 checked off in `user-story.md`. `PerParentClassifier.cs` is exercised across blend, cold-start, train/untrain, normalization, and every fail-fast guard.

## Phase 4 — LcppnFolderPredictor (config, beam search, abstention, incremental dispatch)

### [P4-T1] Compile registration

- Timestamp: 2026-06-12T10-48 (UTC)
- Added `<Compile Include="EmailIntelligence\Bayesian\LcppnFolderPredictorConfig.cs" />` and `<Compile Include="EmailIntelligence\Bayesian\LcppnFolderPredictor.cs" />` to `UtilitiesCS/UtilitiesCS.csproj`; added `<Compile Include="EmailIntelligence\Bayesian\LcppnFolderPredictor_Tests.cs" />` to `UtilitiesCS.Test/UtilitiesCS.Test.csproj`.

### [P4-T2..T3] Config

- `LcppnFolderPredictorConfig.cs` — five keys with documented defaults: `UseLcppnPredictor` (false), `BeamWidth` (3), `MinimumPathProbability` (0.5), `ShrinkageLambda` (0.7), `MinColdStartExamples` (5); serializable. `Validate()` (invoked by the `Create` helper) enforces `BeamWidth >= 1`, `0 < MinimumPathProbability < 1`, `0 <= ShrinkageLambda <= 1`, `MinColdStartExamples >= 0`, throwing `ArgumentOutOfRangeException` (AC6/AC9 validation).

### [P4-T4..T11] Predictor

- `LcppnFolderPredictor.cs` — `sealed class : SmartSerializable<LcppnFolderPredictor>, IFolderPredictor`. Holds `Dictionary<string, PerParentClassifier> Nodes` keyed by full parent path (empty string root) and a `FolderHierarchyTree Tree` (P4-T4). File is 348 lines, under the 500 limit.
- `Build(IEnumerable<MinedMailInfo>, config)` constructs the tree and per-parent classifiers from `FolderInfo.RelativePath` + `Tokens` (P4-T5).
- `Classify` runs beam-search descent (`DescendBeam`): each frontier entry is a partial path with cumulative log-probability; children are scored via `PerParentClassifier.ScoreChildren`; the top `BeamWidth` partials are retained per step until leaves are reached (P4-T6). The returned `Probability` is `exp(sum of per-step log conditional probabilities)` = the path product (P4-T7), as an `OrderedParallelQuery<Prediction<string>>` ordered descending.
- Abstention: if the top leaf's path product < `MinimumPathProbability`, returns empty; an empty root (no children) returns empty (root abstention) (P4-T8).
- Localized `Train`/`UnTrain` walk the root-to-leaf path and update only the per-parent classifiers on that path (P4-T9/T10). New-leaf handling registers a new child on the target parent only via `Tree.AddLeaf` + per-parent `Train` (P4-T11).

### [P4-T12] Tests

- `LcppnFolderPredictor_Tests.cs` (MSTest + FluentAssertions, 19 tests): path-product probability equality recomputed from node scorers (AC5), descending order; `BeamWidth >= 1`, `MinimumPathProbability` strict (0,1), `ShrinkageLambda` [0,1], negative cold-start, and default values for config (AC6/AC9 validation); wider beam recovers a branch greedy width-1 discards, with determinism check (AC6); below-threshold abstention and root abstention (AC7); localized Train/UnTrain leaving off-path nodes unchanged (AC11); new-leaf modifies only the target parent's child set with all other parents byte-for-byte unchanged (AC12); assignability to `IFolderPredictor`; null-corpus fail-fast. Deterministic, in-memory, no temp files, no Outlook COM.

### [P4-T13] Full C# toolchain gate

- Timestamp: 2026-06-12T10-48 (UTC).
  1. `dotnet tool run csharpier format .` — reformatted the two new files on first run; restarted; stable thereafter (incl. after adding the config-validation tests).
  2. `msbuild ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` — EXIT 0; 0 Error(s); no new warnings.
  3. `msbuild ... /p:Nullable=enable /p:TreatWarningsAsErrors=true` — EXIT 0; 0 Warning(s), 0 Error(s).
  4. `vstest.console.exe ... /EnableCodeCoverage /InIsolation` (filtered to `LcppnFolderPredictor_Tests`) — 19/19 passed.
- Result: all four steps pass in a single final pass. AC5, AC6, AC7, AC11, AC12 checked off in `user-story.md`.

## Phase 5 — Serialization (separate file, inline Corpus, round-trip)

### [P5-T1] Compile registration

- Timestamp: 2026-06-12T10-55 (UTC)
- Added `<Compile Include="EmailIntelligence\Bayesian\LcppnFolderPredictor_Serialization_Tests.cs" />` to `UtilitiesCS.Test/UtilitiesCS.Test.csproj`.

### [P5-T2..T4] Serialization shape

- `LcppnFolderPredictor` serializes via `SmartSerializable<LcppnFolderPredictor>` (Newtonsoft, TypeNameHandling.Auto). `[JsonProperty]` exposes `Version`, `BeamWidth`, `MinimumPathProbability`, `ShrinkageLambda`, `MinColdStartExamples`, and the `Nodes` map keyed by full parent path; each node's `BayesianClassifierGroup` subtree (with inline `Corpus` via `SharedTokenBase`) serializes within the single document (P5-T2/T3). `Version` defaults to 1 (P5-T4).
- Two production adjustments required for a working round-trip (minimal, in-scope):
  - `PerParentClassifier` made serializable: added a private `[JsonConstructor]`, `[JsonProperty]` on `ShrinkageLambda`/`MinColdStartExamples`/`Group`, and an `OnDeserialized` hook that re-applies the construction invariants. Validation was factored into a shared `ValidateInvariants` helper (Phase 3 fail-fast tests still pass; parameter names unchanged).
  - `LcppnFolderPredictor` parameterless ctor now sets `base._parent = this` (the same pattern `BayesianClassifierGroup` uses), because `SmartSerializable.SerializeToStream` serializes its `_parent` reference. The `Tree` is `[JsonIgnore]` and rebuilt from `Nodes` in `OnDeserialized`/`RebuildTree`, avoiding a redundant second copy of the structure.

### [P5-T5] Tests

- `LcppnFolderPredictor_Serialization_Tests.cs` (MSTest + FluentAssertions, 6 tests): in-memory round-trip preserves Version + top-level scalars; preserves per-parent tree (node keys + child segments) and the rebuilt derived tree; preserves counts and reproduces identical classification; the serialized JSON contains `Nodes`/`Version`/`SharedTokenBase` and no `CorpusInherit`; empty-tree round-trips cleanly; serialized JSON parses. All in-memory, no temp files (AC15).
- Serialization settings: `TypeNameHandling.Auto` + `PreserveReferencesHandling.Objects` (the established Bayesian convention, required because the reused `BayesianClassifierShared` subtree holds a parent back-reference).

### [P5-T6] Full C# toolchain gate

- Timestamp: 2026-06-12T10-55 (UTC).
  1. `dotnet tool run csharpier format .` — restarted once after the production serialization adjustment; stable thereafter.
  2. `msbuild ... /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` — EXIT 0; 0 Error(s); no new warnings.
  3. `msbuild ... /p:Nullable=enable /p:TreatWarningsAsErrors=true` — EXIT 0; 0 Warning(s), 0 Error(s).
  4. `vstest.console.exe ... /EnableCodeCoverage /InIsolation` — serialization 6/6 passed; full Phase 1-5 feature suite 60/60 passed (no regression from the serialization adjustments).
- Result: all four steps pass in a single final pass. AC15 checked off in `user-story.md`.

## Phase 6 — BLOCKED (scope-change finding; reported to orchestrator)

- Timestamp: 2026-06-12T11-00 (UTC)
- P6-T1 (register `FolderPredictorSeam_Tests.cs`) was prepared then reverted to keep the test project compiling, because the phase cannot complete: P6-T3's required change is not localizable to the plan's named scope.

### Finding: the `ManagerAsyncLazy` type-parameter change cascades beyond the plan's scope

- P6-T3 directs changing `ManagerAsyncLazy : ConcurrentObservableDictionary<string, AsyncLazy<BayesianClassifierGroup>>` to use `IFolderPredictor`. The plan's named P6 scope is `OlFolderClassifierGroup.cs`, `ManagerAsyncLazy.cs`, `EmailFiler.cs`, `SortEmail.cs`.
- `ManagerAsyncLazy` (`Globals.AF.Manager`) is a single shared dictionary used by **all** classifier subsystems, not just `Manager["Folder"]`. `AsyncLazy<T>` is a `sealed` (invariant) class, so `AsyncLazy<BayesianClassifierGroup>` is not assignable to `AsyncLazy<IFolderPredictor>`.
- Changing the dictionary value type therefore breaks these out-of-scope sites (verified by grep):
  - `Triage.cs:45` — `ClassifierGroup = await Globals.AF.Manager[GroupName];` assigns the awaited value to a `BayesianClassifierGroup` field (read-side break), plus assignment sites at `Triage.cs:149,177,302`.
  - `SpamBayes.cs:222` — `Manager[GroupName] = ClassifierGroup.ToAsyncLazy();` (produces `AsyncLazy<BayesianClassifierGroup>`).
  - `CategoryClassifierGroup.cs:150` — same `ToAsyncLazy()` assignment.
  - `MulticlassEngine.cs:173` — same `ToAsyncLazy()` assignment.
  - `OlFolderClassifierGroup.cs:211` (in scope) and `OlFolderClassifierGroup.cs:234` (`Manager["Spam"]`).
  - `ManagerAsyncLazy.cs` `GetAsyncLazyClassifierLoader`/`GetAltLoader` return concrete `BayesianClassifierGroup` and subscribe `classifier.PropertyChanged += Config_PropertyChanged` — `IFolderPredictor` exposes no `PropertyChanged`, `Config`, `Static.DeserializeAsync`, or `ToAsyncLazy()`.
- Making P6-T3 compile requires edits to `Triage.cs`, `SpamBayes.cs`, `CategoryClassifierGroup.cs`, and `MulticlassEngine.cs` — files not in the plan's P6 scope and not in the orchestrator's HARD-constraint file list. `csharp.md` Prohibited Behaviors forbids "Broad refactors across unrelated projects or files."
- Per the orchestrator directive ("stop and report a scope-change finding rather than widening scope"), Phase 6 is halted at its boundary. Phases 1-5 are complete and the tree is green (Debug build 0 errors; incremental nullable gate 0/0; 60/60 feature tests pass).

### Note on the nullable gate

- The nullable `TreatWarningsAsErrors=true` step passes as an incremental build (0/0), which is the established gate behavior. A from-scratch full recompile of `UtilitiesCS.Test` under `Nullable=enable` surfaces pre-existing CS8625 in many unrelated test files; this occurs identically on clean HEAD (verified via `git stash -u`) and is not introduced by this feature's changes.

### Suggested plan delta for atomic-planner (not applied)

- Expand P6 scope explicitly to include the cascading `ToAsyncLazy()`/await-read sites in `Triage.cs`, `SpamBayes.cs`, `CategoryClassifierGroup.cs`, `MulticlassEngine.cs`, OR adopt a narrower seam that avoids changing the shared dictionary value type — for example: keep `Manager` typed as `AsyncLazy<BayesianClassifierGroup>` and introduce a separate `Manager["Folder"]`-specific `IFolderPredictor` accessor/holder, so the LCPPN predictor is stored and retrieved through a Folder-only seam without altering the shared multiclass manager. The planner should choose and specify one approach with the full file list before P6 resumes.
