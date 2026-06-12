# hierarchical-lcppn-folder-prediction - Plan

- **Issue:** #177
- **Parent (optional):** none
- **Owner:** TBD
- **Last Updated:** 2026-06-10T12-31
- **Status:** In Progress
- **Version:** 1.4

## Introduction

![Status: In Progress](https://img.shields.io/badge/status-In%20Progress-yellow)

This plan delivers the hierarchy-aware LCPPN folder predictor described in `spec.md` and
`user-story.md` (Issue #177). It is C#/.NET work targeting `UtilitiesCS` and
`UtilitiesCS.Test`. Status is **In Progress**: preflight returned ALL CLEAR and
execution has begun at Phase 0.

## Required References

This is C#/.NET work. All work must comply with the following policies; do not duplicate their content here.

- Policy authority and reading order: [`CLAUDE.md`](../../../../CLAUDE.md) — General Code Change Policy, General Unit Test Policy, C# Code Change Policy, C# Unit Test Policy (apply in that order).
- General Code Change Policy: [`.claude/rules/general-code-change.md`](../../../../.claude/rules/general-code-change.md)
- General Unit Test Policy: [`.claude/rules/general-unit-test.md`](../../../../.claude/rules/general-unit-test.md)
- C# Code Standards (toolchain, DI seams, deterministic test rules): [`.claude/rules/csharp.md`](../../../../.claude/rules/csharp.md)
- Tonality Policy: [`.claude/rules/tonality.md`](../../../../.claude/rules/tonality.md)
- C# QA Gate (final toolchain and zero-regression deltas): [`.claude/skills/csharp-qa-gate/SKILL.md`](../../../../.claude/skills/csharp-qa-gate/SKILL.md)

### C# toolchain order (run on every implementation phase; restart from step 1 on any failure or auto-fix)

1. `dotnet tool run csharpier .`
2. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
3. `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
4. `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`

Tests use **MSTest** + **Moq** + **FluentAssertions**. New modules/classes must reach **>= 90%** line coverage; repository-wide line coverage must remain **>= 80%**; coverage on changed lines must not regress. No temporary files in tests; deterministic tests only; pure prediction/evaluation logic must be testable without Outlook COM.

### Target files (verified against the codebase)

- Production namespace `UtilitiesCS.EmailIntelligence.Bayesian` under `UtilitiesCS/EmailIntelligence/Bayesian/`.
- Production namespace `UtilitiesCS.EmailIntelligence.Evaluation` under `UtilitiesCS/EmailIntelligence/Evaluation/` (new folder).
- Build wiring in `UtilitiesCS/EmailIntelligence/ClassifierGroups/OlFolder/OlFolderClassifierGroup.cs`.
- Caller seam in `UtilitiesCS/EmailIntelligence/EmailParsingSorting/EmailFiler.cs` and `UtilitiesCS/EmailIntelligence/EmailParsingSorting/SortEmail.cs`.
- Manager type in `UtilitiesCS/EmailIntelligence/ClassifierGroups/ManagerAsyncLazy.cs`.
- Tests under `UtilitiesCS.Test/EmailIntelligence/Bayesian/` and `UtilitiesCS.Test/EmailIntelligence/Evaluation/` (new folder); test project `UtilitiesCS.Test/UtilitiesCS.Test.csproj`.

### Non-SDK project compile registration (HARD constraint)

`UtilitiesCS/UtilitiesCS.csproj` and `UtilitiesCS.Test/UtilitiesCS.Test.csproj` are **legacy non-SDK MSBuild projects** (`ToolsVersion="15.0"`, `xmlns="http://schemas.microsoft.com/developer/msbuild/2003"`, no `Microsoft.NET.Sdk` import, no `EnableDefaultCompileItems`, no wildcard `<Compile Include="**\*.cs"/>`). These projects use **explicit `<Compile Include="..."/>` lists**. A new `.cs` file placed on disk does **not** compile until it is registered with an explicit `<Compile Include="..."/>` entry in the correct `.csproj`.

Therefore EVERY new `.cs` file created by this plan — production and test — MUST be registered with an explicit `<Compile Include="..."/>` entry in the correct `.csproj` (production files in `UtilitiesCS/UtilitiesCS.csproj`; test files in `UtilitiesCS.Test/UtilitiesCS.Test.csproj`) before the file will compile into its assembly. Each phase that introduces new files registers those files as its first task (`[P#-T1]`) so the per-phase "all four toolchain steps pass" acceptance can be met. Use the project file's existing relative-path and folder convention (for example `EmailIntelligence\Bayesian\IFolderPredictor.cs`) for each `<Compile Include>` path.

Production files to register in `UtilitiesCS/UtilitiesCS.csproj`: `EmailIntelligence\Bayesian\IFolderPredictor.cs`, `EmailIntelligence\Bayesian\FolderHierarchyNode.cs`, `EmailIntelligence\Bayesian\FolderHierarchyTree.cs`, `EmailIntelligence\Bayesian\PerParentClassifier.cs`, `EmailIntelligence\Bayesian\LcppnFolderPredictorConfig.cs`, `EmailIntelligence\Bayesian\LcppnFolderPredictor.cs`, `EmailIntelligence\Evaluation\EvaluationResult.cs`, `EmailIntelligence\Evaluation\FolderPredictorEvaluator.cs`.

Test files to register in `UtilitiesCS.Test/UtilitiesCS.Test.csproj`: `EmailIntelligence\Bayesian\IFolderPredictor_Tests.cs`, `EmailIntelligence\Bayesian\BayesianClassifierGroup_FlatPathUnchanged_Tests.cs`, `EmailIntelligence\Bayesian\FolderHierarchyTree_Tests.cs`, `EmailIntelligence\Bayesian\PerParentClassifier_Tests.cs`, `EmailIntelligence\Bayesian\LcppnFolderPredictor_Tests.cs`, `EmailIntelligence\Bayesian\LcppnFolderPredictor_Serialization_Tests.cs`, `EmailIntelligence\FolderPredictorSeam_Tests.cs`, `EmailIntelligence\Evaluation\FolderPredictorEvaluator_Tests.cs`.

### Evidence locations (canonical; not overridable)

All evidence artifacts produced by this plan live under the feature folder per `evidence-and-timestamp-conventions`, except the one canonical C# coverage baseline that repository policy fixes at `artifacts/csharp/coverage.xml`.

- Feature evidence root: `docs/features/active/2026-06-08-hierarchical-lcppn-folder-prediction-177/evidence/`
- Baseline evidence: `docs/features/active/2026-06-08-hierarchical-lcppn-folder-prediction-177/evidence/baseline/<ISO-8601-UTC>/`
- QA-gate evidence (including post-change coverage and coverage-comparison artifacts): `docs/features/active/2026-06-08-hierarchical-lcppn-folder-prediction-177/evidence/qa-gates/<ISO-8601-UTC>/`
- Canonical C# coverage baseline artifact (required before the first feature-review): `artifacts/csharp/coverage.xml`

> Canonical evidence sub-paths are limited to `baseline/`, `regression-testing/`, `qa-gates/`, `issue-updates/`, `other/`, and `remediation-baseline/` per `evidence-and-timestamp-conventions`. Coverage and coverage-comparison artifacts are stored under `qa-gates/`; there is no `evidence/coverage/` sub-path. The only evidence artifact outside the feature folder is the policy-mandated C# coverage baseline at `artifacts/csharp/coverage.xml`.

If a caller instruction specifies a non-canonical evidence path (for example `artifacts/baselines/`, `artifacts/qa/`, `artifacts/coverage/`), this plan ignores it, writes to the canonical path above, and records `EVIDENCE_LOCATION_OVERRIDE_REJECTED: <supplied path> replaced with <canonical path>`. The sole exception is the canonical C# coverage baseline at `artifacts/csharp/coverage.xml`, which is mandated by repository policy.

## Implementation Plan (Atomic Tasks)

> Each task is binary (one verifiable outcome) and sized at 5-30 minutes. Test-creation tasks live inside the phase that implements the code (self-validating phases). The full C# toolchain (CSharpier -> .NET analyzers -> nullable -> MSTest with coverage) is run at the end of every implementation phase and restarted from step 1 on any failure or auto-fix.

### Phase 0 — Compliance & Context

- [x] [P0-T1] Read the General Code Change Policy and General Unit Test Policy sections of `CLAUDE.md`, plus `.claude/rules/general-code-change.md` and `.claude/rules/general-unit-test.md`, before any code change
  - Acceptance: Development log records the read with an ISO-8601 UTC timestamp prior to any Phase 1 edit
- [x] [P0-T2] Read the C# Code Change Policy and C# Unit Test Policy sections of `CLAUDE.md`, plus `.claude/rules/csharp.md` and `.claude/skills/csharp-qa-gate/SKILL.md`
  - Acceptance: Development log records the read with an ISO-8601 UTC timestamp; the four toolchain commands are restated in the log exactly as written in policy
- [x] [P0-T3] Read the Tonality Policy (`.claude/rules/tonality.md`) and confirm all plan-execution output uses neutral professional tone
  - Acceptance: Development log records the read with an ISO-8601 UTC timestamp
- [x] [P0-T4] Verify the build and test environment by running `dotnet tool run csharpier .`, both `msbuild` commands, and `vstest.console.exe` against the current `UtilitiesCS.Test` assemblies on a clean tree
  - Acceptance: All four toolchain steps complete on the unchanged tree; the pass/fail status, analyzer findings, nullable diagnostics, and MSTest results are captured under `docs/features/active/2026-06-08-hierarchical-lcppn-folder-prediction-177/evidence/baseline/<ISO-8601-UTC>/`
- [x] [P0-T5] Generate the canonical pre-change C# coverage baseline by running `vstest.console.exe <UtilitiesCS.Test assemblies> /EnableCodeCoverage` and exporting the result to Cobertura/XML
  - Acceptance: The baseline coverage XML exists at `artifacts/csharp/coverage.xml` and a copy plus the run log are stored under `docs/features/active/2026-06-08-hierarchical-lcppn-folder-prediction-177/evidence/baseline/<ISO-8601-UTC>/`; the overall repository line-coverage percentage is recorded for later comparison
- [x] [P0-T6] Record the baseline behavior of the flat folder path: capture the current `BayesianClassifierGroup` `Train` / `UnTrain` / `Classify` / `Serialize` behavior and the `Manager["Folder"]` type and caller cast sites in `EmailFiler.cs` and `SortEmail.cs`
  - Acceptance: Development log lists the verified file:line seam points (`OlFolderClassifierGroup.cs`, `EmailFiler.cs`, `SortEmail.cs`, `ManagerAsyncLazy.cs`) that Phase 6 will modify

### Phase 1 — IFolderPredictor seam (additive, no flat behavior change)

- [ ] [P1-T1] Register this phase's new files in the non-SDK projects: add `<Compile Include="EmailIntelligence\Bayesian\IFolderPredictor.cs"/>` to `UtilitiesCS/UtilitiesCS.csproj`, and `<Compile Include="EmailIntelligence\Bayesian\IFolderPredictor_Tests.cs"/>` plus `<Compile Include="EmailIntelligence\Bayesian\BayesianClassifierGroup_FlatPathUnchanged_Tests.cs"/>` to `UtilitiesCS.Test/UtilitiesCS.Test.csproj`
  - Acceptance: `UtilitiesCS.csproj` and `UtilitiesCS.Test.csproj` each contain explicit `<Compile Include>` entries for the listed files; the files compile into their respective assemblies (verified once the files exist in this phase)
- [ ] [P1-T2] Create `IFolderPredictor` interface in `UtilitiesCS/EmailIntelligence/Bayesian/IFolderPredictor.cs` declaring the members the callers use, with signatures matching `BayesianClassifierGroup` exactly: `void Train(string tag, IEnumerable<string> matchTokens, int emailCount)` (matches `BayesianClassifierGroup.cs:146`), `void UnTrain(string tag, IEnumerable<string> matchTokens, int emailCount)` (matches `BayesianClassifierGroup.cs:91`), the caller-used `Classify` overload `OrderedParallelQuery<Prediction<string>> Classify(string[] tokens)` (matches `BayesianClassifierGroup.cs:229`), and `void Serialize()` (inherited from `SmartSerializable<T>` at `SmartSerializable.cs:425`; both `BayesianClassifierGroup` and `LcppnFolderPredictor` satisfy it through that base, so the interface member is satisfied without adding a new method body)
  - Acceptance: Interface compiles; `Train`, `UnTrain`, the `string[]`-overload `Classify`, and `Serialize()` member signatures are identical to the `BayesianClassifierGroup` members used by callers (verified against `BayesianClassifierGroup.cs:91,146,229` and `SmartSerializable.cs:425`)
- [ ] [P1-T3] Update `BayesianClassifierGroup` (`UtilitiesCS/EmailIntelligence/Bayesian/BayesianClassifierGroup.cs`) to declare `: IFolderPredictor` additively, with no change to existing method bodies
  - Preconditions: P1-T2 complete
  - Acceptance: `BayesianClassifierGroup` compiles as an `IFolderPredictor`; no existing method body is modified
- [ ] [P1-T4] Add `IFolderPredictor` interface-conformance tests in `UtilitiesCS.Test/EmailIntelligence/Bayesian/IFolderPredictor_Tests.cs` asserting that a `BayesianClassifierGroup` instance is assignable to `IFolderPredictor` and that calls through the interface dispatch to the flat methods
  - Acceptance: Tests use MSTest + FluentAssertions and pass; assignability and dispatch are asserted
- [ ] [P1-T5] Add a flat-path regression test in `UtilitiesCS.Test/EmailIntelligence/Bayesian/BayesianClassifierGroup_FlatPathUnchanged_Tests.cs` that trains a small fixed corpus and asserts `Classify` output ordering and probabilities are identical before and after the `IFolderPredictor` declaration (proving AC13 behavior is preserved)
  - Acceptance: Test passes deterministically with no temp files and no external services
- [ ] [P1-T6] Run the full C# toolchain for Phase 1 and resolve any failure or auto-fix by restarting from CSharpier
  - Acceptance: CSharpier, both `msbuild` runs, and `vstest.console.exe /EnableCodeCoverage` all pass in a single final pass; new `IFolderPredictor.cs` reaches >= 90% coverage via P1-T4/P1-T5

### Phase 2 — Folder hierarchy model (pure path parsing)

- [ ] [P2-T1] Register this phase's new files in the non-SDK projects: add `<Compile Include="EmailIntelligence\Bayesian\FolderHierarchyNode.cs"/>` and `<Compile Include="EmailIntelligence\Bayesian\FolderHierarchyTree.cs"/>` to `UtilitiesCS/UtilitiesCS.csproj`, and `<Compile Include="EmailIntelligence\Bayesian\FolderHierarchyTree_Tests.cs"/>` to `UtilitiesCS.Test/UtilitiesCS.Test.csproj`
  - Acceptance: `UtilitiesCS.csproj` and `UtilitiesCS.Test.csproj` each contain explicit `<Compile Include>` entries for the listed files; the files compile into their respective assemblies
- [ ] [P2-T2] Create `FolderHierarchyNode` immutable record in `UtilitiesCS/EmailIntelligence/Bayesian/FolderHierarchyNode.cs` with `string NodeKey` and `string[] Children`, serializable, with XML doc on the contract
  - Acceptance: Record compiles; `NodeKey` and `Children` are exposed; type is serializable by Newtonsoft.Json
- [ ] [P2-T3] Create `FolderHierarchyTree` in `UtilitiesCS/EmailIntelligence/Bayesian/FolderHierarchyTree.cs` that builds and holds `Dictionary<string, FolderHierarchyNode>` from a `RelativePath[]`, parsing each path on backslash and recording each adjacent segment pair as a parent->child edge with empty-string root key (pure logic, no I/O), with a configurable ordinal vs `OrdinalIgnoreCase` comparer option
  - Preconditions: P2-T2 complete
  - Acceptance: File is under 500 lines; class has no Outlook COM or filesystem dependency; comparer option defaults to ordinal
- [ ] [P2-T4] Implement single-segment handling in `FolderHierarchyTree` so a one-segment path (e.g., `"Inbox"`) yields exactly one edge `root -> "Inbox"` and registers that node as both a root child and a zero-child leaf
  - Acceptance: A leaf-detection accessor reports zero children for the single-segment node
- [ ] [P2-T5] Implement idempotent/deduplicated child registration in `FolderHierarchyTree` using a per-parent child set so duplicate paths produce no duplicate children
  - Acceptance: Building from a list with duplicates yields the same node/children sets as the distinct list
- [ ] [P2-T6] Implement incremental `AddLeaf(parentKey, childSegment)` (or equivalent) in `FolderHierarchyTree` that adds the child to one parent's child set only, leaving all other parents unchanged
  - Acceptance: After adding `parent\NewLeaf`, only `parent`'s child set changes (verified by comparing all other nodes' child sets to the pre-add state)
- [ ] [P2-T7] Create `FolderHierarchyTree_Tests.cs` in `UtilitiesCS.Test/EmailIntelligence/Bayesian/` covering: multi-depth construction (AC1), single-segment edge case (AC2), duplicate-path idempotence (AC3), new-leaf locality (AC4), empty collection, and case-variant comparison
  - Acceptance: All listed scenarios are asserted with FluentAssertions; tests are deterministic with no temp files
- [ ] [P2-T8] Run the full C# toolchain for Phase 2 and restart from CSharpier on any failure or auto-fix
  - Acceptance: All four steps pass in a single final pass; `FolderHierarchyNode.cs` and `FolderHierarchyTree.cs` each reach >= 90% coverage

### Phase 3 — PerParentClassifier (shrinkage blend + cold-start fallback)

- [ ] [P3-T1] Register this phase's new files in the non-SDK projects: add `<Compile Include="EmailIntelligence\Bayesian\PerParentClassifier.cs"/>` to `UtilitiesCS/UtilitiesCS.csproj`, and `<Compile Include="EmailIntelligence\Bayesian\PerParentClassifier_Tests.cs"/>` to `UtilitiesCS.Test/UtilitiesCS.Test.csproj`
  - Acceptance: `UtilitiesCS.csproj` and `UtilitiesCS.Test.csproj` each contain explicit `<Compile Include>` entries for the listed files; the files compile into their respective assemblies
- [ ] [P3-T2] Create `PerParentClassifier` in `UtilitiesCS/EmailIntelligence/Bayesian/PerParentClassifier.cs` wrapping one `BayesianClassifierGroup` whose `Classifiers` are keyed by direct child segment and whose `SharedTokenBase` is the parent-scoped `Corpus`
  - Acceptance: File is under 500 lines; reuses `BayesianClassifierGroup` / `BayesianClassifierShared` / `Corpus` without modifying them
- [ ] [P3-T3] Implement per-child scoring in `PerParentClassifier` that returns `P(child | parent, tokens)` using the shrinkage blend `λ·P_leaf(t|c) + (1-λ)·P_parent(t|p)` with `ShrinkageLambda` supplied from config
  - Preconditions: P3-T2 complete
  - Acceptance: Blend uses the configured lambda; a fixed numeric example reproduces the documented blend value (AC9)
- [ ] [P3-T4] Implement the cold-start fallback in `PerParentClassifier`: when total examples under the parent are fewer than `MinColdStartExamples`, per-child scoring uses unsmoothed Naive Bayes (existing `BayesianClassifierShared` behavior) instead of the blend
  - Acceptance: A parent under the threshold scores via unsmoothed NB; at/above the threshold it scores via the blend (AC10)
- [ ] [P3-T5] Implement count-based `Train(childSegment, tokens, count)` and `UnTrain(childSegment, tokens, count)` on `PerParentClassifier` delegating to the wrapped group's per-tag match counts and shared corpus, with new-child registration via `GetOrAdd`
  - Acceptance: Training a new child registers it without affecting sibling counts; untraining decrements the same counts
- [ ] [P3-T6] Validate construction invariant `0 <= ShrinkageLambda <= 1` and `MinColdStartExamples >= 0` in `PerParentClassifier` (or its config), failing fast with an explicit exception
  - Acceptance: Out-of-range lambda or negative cold-start count throws at construction with a clear message
- [ ] [P3-T7] Create `PerParentClassifier_Tests.cs` covering: blend correctness at a fixed lambda (AC9), cold-start fallback boundary at `MinColdStartExamples` (AC10), incremental train/untrain count changes, sibling isolation, probability sanity bounds, and invalid-lambda fail-fast
  - Acceptance: All scenarios asserted with FluentAssertions; deterministic; no temp files
- [ ] [P3-T8] Run the full C# toolchain for Phase 3 and restart from CSharpier on any failure or auto-fix
  - Acceptance: All four steps pass in a single final pass; `PerParentClassifier.cs` reaches >= 90% coverage

### Phase 4 — LcppnFolderPredictor (config, beam search, abstention, incremental dispatch)

- [ ] [P4-T1] Register this phase's new files in the non-SDK projects: add `<Compile Include="EmailIntelligence\Bayesian\LcppnFolderPredictorConfig.cs"/>` and `<Compile Include="EmailIntelligence\Bayesian\LcppnFolderPredictor.cs"/>` to `UtilitiesCS/UtilitiesCS.csproj`, and `<Compile Include="EmailIntelligence\Bayesian\LcppnFolderPredictor_Tests.cs"/>` to `UtilitiesCS.Test/UtilitiesCS.Test.csproj`
  - Acceptance: `UtilitiesCS.csproj` and `UtilitiesCS.Test.csproj` each contain explicit `<Compile Include>` entries for the listed files; the files compile into their respective assemblies
- [ ] [P4-T2] Create `LcppnFolderPredictorConfig` in `UtilitiesCS/EmailIntelligence/Bayesian/LcppnFolderPredictorConfig.cs` with `UseLcppnPredictor` (default false), `BeamWidth` (default 3), `MinimumPathProbability` (default 0.5), `ShrinkageLambda` (default 0.7), `MinColdStartExamples` (default 5), serializable
  - Acceptance: All five keys present with the documented defaults; type is serializable
- [ ] [P4-T3] Implement construction-time validation in `LcppnFolderPredictorConfig`: `BeamWidth >= 1`, `0 < MinimumPathProbability < 1`, `0 <= ShrinkageLambda <= 1`, `MinColdStartExamples >= 0`, failing fast on violation
  - Preconditions: P4-T2 complete
  - Acceptance: Each invalid value throws an explicit exception with a clear message (AC6 validation, AC9 validation)
- [ ] [P4-T4] Create `LcppnFolderPredictor` in `UtilitiesCS/EmailIntelligence/Bayesian/LcppnFolderPredictor.cs` extending `SmartSerializable<LcppnFolderPredictor>`, implementing `IFolderPredictor`, holding `Dictionary<string, PerParentClassifier>` keyed by full parent path and a `FolderHierarchyTree`
  - Acceptance: File estimated 350-450 lines and under the 500-line limit; compiles as an `IFolderPredictor`
- [ ] [P4-T5] Implement a builder on `LcppnFolderPredictor` (or `OlFolderClassifierGroup` helper invoked here) that constructs the tree and per-parent classifiers from a `MinedMailInfo[]` corpus using `FolderInfo.RelativePath`
  - Preconditions: P4-T4, Phase 2, Phase 3 complete
  - Acceptance: Building from a fixed corpus produces one `PerParentClassifier` per internal node with child-keyed classifiers
- [ ] [P4-T6] Implement beam-search descent in `LcppnFolderPredictor.Classify` that descends from the root, scores each frontier node's children, and retains the top `BeamWidth` partial paths by cumulative path log-probability until frontier entries reach leaves
  - Acceptance: Descent terminates at leaves; retained-path count never exceeds `BeamWidth` (AC5)
- [ ] [P4-T7] Implement path-product probability and result assembly so `Classify` returns the top leaf with `Probability` equal to the product of per-step conditional probabilities along its root-to-leaf path, plus an ordered list of alternative `Prediction<string>` entries
  - Acceptance: For a constructed corpus with path `Projects\Alpha\2024`, top `Class` is the full path and `Probability` equals `P(Projects|root)·P(Alpha|Projects)·P(2024|Alpha)` within numeric tolerance (AC5)
- [ ] [P4-T8] Implement abstention in `Classify`: if the top leaf's path-product probability is below `MinimumPathProbability`, return an empty result; if no root-level child clears the threshold, return an empty result (root abstention allowed)
  - Acceptance: An input whose best path product is below the threshold returns empty; a root-level all-below case returns empty (AC7)
- [ ] [P4-T9] Implement localized incremental `Train(tag, tokens, count)` on `LcppnFolderPredictor` that parses the leaf tag into its root-to-leaf path and calls `Train(childSegment, tokens, count)` on each per-parent classifier on that path only
  - Acceptance: Training leaf `L` updates only path classifiers; nodes off the path have unchanged counts and probabilities (AC11)
- [ ] [P4-T10] Implement localized incremental `UnTrain(tag, tokens, count)` on `LcppnFolderPredictor` that applies `UnTrain` along the prior leaf path only
  - Acceptance: Untraining a prior leaf decrements only that path's classifiers; other nodes unchanged (AC11)
- [ ] [P4-T11] Implement new-leaf handling so a previously unseen `parent\NewLeaf` registers the child on `parent`'s `PerParentClassifier` only (via `FolderHierarchyTree.AddLeaf` and child registration), leaving all other per-parent classifiers unchanged
  - Acceptance: Registering a new leaf modifies only the target parent's classifier; all other classifiers are byte-for-byte unchanged in counts (AC12)
- [ ] [P4-T12] Create `LcppnFolderPredictor_Tests.cs` covering: beam-search returns correct leaf and path-product probability (AC5), configurable beam width recovers a branch width-1 would discard and `BeamWidth >= 1` validation (AC6), abstention and root abstention (AC7), localized Train/UnTrain (AC11), and local new-leaf addition (AC12)
  - Acceptance: All scenarios asserted with FluentAssertions; deterministic; no temp files; no Outlook COM
- [ ] [P4-T13] Run the full C# toolchain for Phase 4 and restart from CSharpier on any failure or auto-fix
  - Acceptance: All four steps pass in a single final pass; `LcppnFolderPredictorConfig.cs` and `LcppnFolderPredictor.cs` each reach >= 90% coverage

### Phase 5 — Serialization (separate file, inline Corpus, round-trip)

- [ ] [P5-T1] Register this phase's new test file in the non-SDK test project: add `<Compile Include="EmailIntelligence\Bayesian\LcppnFolderPredictor_Serialization_Tests.cs"/>` to `UtilitiesCS.Test/UtilitiesCS.Test.csproj`
  - Acceptance: `UtilitiesCS.Test.csproj` contains an explicit `<Compile Include>` entry for the serialization test file; the file compiles into the test assembly
- [ ] [P5-T2] Configure `LcppnFolderPredictor` serialization via `SmartSerializable<LcppnFolderPredictor>` (Newtonsoft.Json, `TypeNameHandling.Auto`) to emit a `Nodes` dictionary keyed by full parent path (empty string for root), each holding the node's children and its `BayesianClassifierGroup` subtree, plus top-level `Version`, `BeamWidth`, and `MinimumPathProbability`
  - Acceptance: Serialized JSON contains `Version`, `BeamWidth`, `MinimumPathProbability`, and a `Nodes` map; the file is distinct from `Folder.json`
- [ ] [P5-T3] Ensure each `PerParentClassifier` shared token base uses `Corpus` serialized inline within the predictor JSON (not `CorpusInherit`), so no per-node separate JSON files are produced
  - Preconditions: P5-T2 complete
  - Acceptance: Serialization produces a single predictor JSON document with inline `Corpus`; no `CorpusInherit` side files are written
- [ ] [P5-T4] Add a `Version` field (default 1) to `LcppnFolderPredictor` for forward migration and include it in the serialized shape
  - Acceptance: `Version` is present and preserved through round-trip
- [ ] [P5-T5] Create `LcppnFolderPredictor_Serialization_Tests.cs` asserting in-memory JSON round-trip losslessly preserves `Version`, the per-parent tree, and counts, and that an empty tree serializes and deserializes cleanly
  - Acceptance: Round-trip equality on tree, counts, and `Version`; empty-tree case passes; all in-memory, no temp files (AC15)
- [ ] [P5-T6] Run the full C# toolchain for Phase 5 and restart from CSharpier on any failure or auto-fix
  - Acceptance: All four steps pass in a single final pass; serialization paths in `LcppnFolderPredictor.cs` reach >= 90% coverage

### Phase 6 — Wiring and backward compatibility (flag-gated seam)

- [ ] [P6-T1] Register this phase's new test file in the non-SDK test project: add `<Compile Include="EmailIntelligence\FolderPredictorSeam_Tests.cs"/>` to `UtilitiesCS.Test/UtilitiesCS.Test.csproj` (the production files modified in this phase — `OlFolderClassifierGroup.cs`, `ManagerAsyncLazy.cs`, `EmailFiler.cs`, `SortEmail.cs` — already exist and are already registered)
  - Acceptance: `UtilitiesCS.Test.csproj` contains an explicit `<Compile Include>` entry for the seam test file; the file compiles into the test assembly
- [ ] [P6-T2] Add `BuildLcppnPredictorAsync` to `UtilitiesCS/EmailIntelligence/ClassifierGroups/OlFolder/OlFolderClassifierGroup.cs` that builds an `LcppnFolderPredictor` from the mined corpus, leaving `BuildFolderClassifiersAsync` unchanged
  - Acceptance: New method compiles; existing `BuildFolderClassifiersAsync` body is unmodified
- [ ] [P6-T3] Change the `Manager["Folder"]` registration type parameter to `IFolderPredictor`. The registration occurs in `OlFolderClassifierGroup.BuildClassifiersAsync` (around line 211), not in `BuildFolderClassifiersAsync`; update that registration site to select `LcppnFolderPredictor` when `UseLcppnPredictor = true` and the flat `BayesianClassifierGroup` otherwise. In `ManagerAsyncLazy.cs`, the manager is `ConcurrentObservableDictionary<string, AsyncLazy<BayesianClassifierGroup>>`; the `BayesianClassifierGroup` type parameter surface spans multiple members (return types, the `DeserializeAsync` path, and the `GetAltLoader`/loader members), all of which must change to `IFolderPredictor`
  - Preconditions: P6-T2, Phase 1 complete
  - Acceptance: The `ManagerAsyncLazy` type parameter is `IFolderPredictor` across all affected members (return types, `DeserializeAsync` path, loader members); the registration in `OlFolderClassifierGroup.BuildClassifiersAsync` registers the flat predictor when `UseLcppnPredictor = false` and the LCPPN predictor when `true`; both satisfy `IFolderPredictor`
- [ ] [P6-T4] Confirm and adjust the `(await Manager["Folder"]).Train` / `.UnTrain` / `.Serialize` call sites in `EmailFiler.cs` (`TrainFolderAsync`, `UnTrainFolderAsync`, `Serialize`) so they resolve through `IFolderPredictor` once the manager type parameter changes in P6-T3. There is no literal `(BayesianClassifierGroup)` cast at these sites; the type flows from awaiting `Manager["Folder"]`. Adjust only if a member is referenced that is not on `IFolderPredictor`
  - Acceptance: `EmailFiler` compiles with `Manager["Folder"]` typed as `IFolderPredictor`; the `Train` / `UnTrain` / `Serialize` calls are unchanged in arguments and resolve through `IFolderPredictor`
- [ ] [P6-T5] Confirm and adjust both `(await Manager["Folder"])` call sites in `SortEmail.cs` — the `.Train` call at `SortEmail.cs:250` and the `.UnTrain` call at `SortEmail.cs:582` — so each resolves through `IFolderPredictor` once the manager type parameter changes in P6-T3. There is no literal `(BayesianClassifierGroup)` cast at either site; the type flows from awaiting `Manager["Folder"]`
  - Acceptance: `SortEmail` compiles with `Manager["Folder"]` typed as `IFolderPredictor`; both the `Train` (line 250) and `UnTrain` (line 582) calls are unchanged in arguments and resolve through `IFolderPredictor`
- [ ] [P6-T6] Verify `Folder.json` is neither read nor written by the LCPPN path: confirm `LcppnFolderPredictor` serializes to a distinct file and that `UseLcppnPredictor = false` loads and writes `Folder.json` exactly as before
  - Acceptance: Flat path I/O targets `Folder.json`; LCPPN path targets its own file (AC13)
- [ ] [P6-T7] Create `FolderPredictorSeam_Tests.cs` in `UtilitiesCS.Test/EmailIntelligence/` (Moq for COM/manager boundaries) asserting: with flag off the flat `BayesianClassifierGroup` is selected and its behavior is unchanged (AC13), and both predictors are reachable through `IFolderPredictor` by `EmailFiler`/`SortEmail` (AC14)
  - Acceptance: Tests use Moq + FluentAssertions; deterministic; no Outlook COM instances; no temp files
- [ ] [P6-T8] Run the full C# toolchain for Phase 6 and restart from CSharpier on any failure or auto-fix
  - Acceptance: All four steps pass in a single final pass; changed lines in `OlFolderClassifierGroup.cs`, `EmailFiler.cs`, `SortEmail.cs`, and `ManagerAsyncLazy.cs` do not regress coverage

### Phase 7 — Evaluation harness (deterministic time-sliced F1)

- [ ] [P7-T1] Register this phase's new files in the non-SDK projects: add `<Compile Include="EmailIntelligence\Evaluation\EvaluationResult.cs"/>` and `<Compile Include="EmailIntelligence\Evaluation\FolderPredictorEvaluator.cs"/>` to `UtilitiesCS/UtilitiesCS.csproj`, and `<Compile Include="EmailIntelligence\Evaluation\FolderPredictorEvaluator_Tests.cs"/>` to `UtilitiesCS.Test/UtilitiesCS.Test.csproj`
  - Acceptance: `UtilitiesCS.csproj` and `UtilitiesCS.Test.csproj` each contain explicit `<Compile Include>` entries for the listed files; the files compile into their respective assemblies
- [ ] [P7-T2] Create `EvaluationResult` value record in `UtilitiesCS/EmailIntelligence/Evaluation/EvaluationResult.cs` carrying per-leaf precision/recall/F1, macro F1, and abstention rate
  - Acceptance: Record compiles; all four metric groups are exposed
- [ ] [P7-T3] Create `FolderPredictorEvaluator` in `UtilitiesCS/EmailIntelligence/Evaluation/FolderPredictorEvaluator.cs` accepting `IFolderPredictor`, `MinedMailInfo[]`, and an evaluation config; pure logic, no Outlook COM
  - Acceptance: File under 500 lines (estimated 150-200); no Outlook COM or filesystem dependency
- [ ] [P7-T4] Implement the deterministic time-sliced split in `FolderPredictorEvaluator` using the corpus-index proxy. `MinedMailInfo` has no timestamp/received-date field (verified members: Categories, Tokens, FolderInfo, ToRecipients, CcRecipients, Sender, ConversationId, EntryId, StoreId, Subject, Actionable, GroupingKey), so the split must use the input array's stable corpus index as the deterministic time proxy: take the first `TrainFraction` of the array (by index order) as train and the remainder as test, build the predictor from the train slice, evaluate the test slice
  - Preconditions: P7-T3 complete
  - Acceptance: The split is computed from the array's corpus index (no timestamp dependency); the same input yields the same split and result across runs (AC16)
- [ ] [P7-T5] Implement per-leaf precision/recall/F1, macro F1, and abstention-rate computation in `FolderPredictorEvaluator`, returning an `EvaluationResult`
  - Acceptance: Metrics match hand-computed values on a fixed small corpus
- [ ] [P7-T6] Implement abstention accounting so an abstained test example counts as a false negative for its true class and a true negative for all other classes, never incrementing a false positive
  - Acceptance: A constructed abstained example lowers recall for its true class without inflating any class's false positives (AC8)
- [ ] [P7-T7] Create `FolderPredictorEvaluator_Tests.cs` in `UtilitiesCS.Test/EmailIntelligence/Evaluation/` covering: deterministic split reproducibility (AC16), precision/recall/macro-F1 correctness, and abstention-as-false-negative accounting (AC8); no Outlook COM, no external services, no temp files
  - Acceptance: All scenarios asserted with FluentAssertions; deterministic
- [ ] [P7-T8] Run the full C# toolchain for Phase 7 and restart from CSharpier on any failure or auto-fix
  - Acceptance: All four steps pass in a single final pass; `EvaluationResult.cs` and `FolderPredictorEvaluator.cs` each reach >= 90% coverage

### Phase 8 — Final QA, coverage comparison, and acceptance check-off

- [ ] [P8-T1] Run the full C# toolchain end to end on the complete change set (CSharpier -> .NET analyzers -> nullable -> MSTest with coverage), restarting from step 1 on any failure or auto-fix, per `.claude/skills/csharp-qa-gate/SKILL.md`
  - Acceptance: All four steps complete in a single final pass with zero new analyzer findings, zero new nullable diagnostics, and zero failing tests; outputs stored under `docs/features/active/2026-06-08-hierarchical-lcppn-folder-prediction-177/evidence/qa-gates/<ISO-8601-UTC>/`
- [ ] [P8-T2] Export the post-change coverage report and store it as the final-QA coverage artifact
  - Acceptance: Post-change coverage XML is stored at `docs/features/active/2026-06-08-hierarchical-lcppn-folder-prediction-177/evidence/qa-gates/<ISO-8601-UTC>/coverage.xml`
- [ ] [P8-T3] Generate a coverage-comparison artifact comparing the post-change coverage to the Phase 0 baseline (`artifacts/csharp/coverage.xml`)
  - Acceptance: Comparison artifact at `docs/features/active/2026-06-08-hierarchical-lcppn-folder-prediction-177/evidence/qa-gates/<ISO-8601-UTC>/coverage-comparison.md` shows repository-wide line coverage >= 80%, each new module/class >= 90%, and no coverage regression on changed lines (AC18)
- [ ] [P8-T4] Verify the file-size and separation constraints: confirm no new production, test, or reusable script file exceeds 500 lines, and that all new prediction/evaluation logic compiles and tests without any Outlook COM reference
  - Acceptance: Line counts for all new files are recorded and all are <= 500; new prediction/evaluation namespaces contain no Outlook COM types (AC20)
- [ ] [P8-T5] Verify the test-stack and isolation constraint: confirm all new tests use MSTest + Moq + FluentAssertions, are independent and deterministic, create no temp files, and depend on no external services
  - Acceptance: New test files are audited against the General Unit Test Policy; the audit result is recorded in the QA-gate evidence folder (AC17)
- [ ] [P8-T6] Complete the AC traceability check-off: mark each of AC1-AC20 against its verifying task/test and confirm every AC maps to at least one passing test or verification task
  - Acceptance: The AC Traceability table below is filled with passing references; if any required baseline, QA, or coverage-comparison artifact is missing, the verdict is recorded as BLOCKED or INCOMPLETE, never PASS

## Test Plan

- **Unit (MSTest + Moq + FluentAssertions):**
  - `IFolderPredictor_Tests`, `BayesianClassifierGroup_FlatPathUnchanged_Tests` (Phase 1)
  - `FolderHierarchyTree_Tests` (Phase 2)
  - `PerParentClassifier_Tests` (Phase 3)
  - `LcppnFolderPredictor_Tests` (Phase 4)
  - `LcppnFolderPredictor_Serialization_Tests` (Phase 5)
  - `FolderPredictorSeam_Tests` (Phase 6)
  - `FolderPredictorEvaluator_Tests` (Phase 7)
- **Integration / seam:** `FolderPredictorSeam_Tests` exercises the `Manager["Folder"]` -> `IFolderPredictor` selection through Moq seams (no Outlook COM).
- **Manual/CLI:** None. All behavior is verified by automated tests; no temporary files; deterministic.
- **Coverage evidence:**
  - Baseline coverage artifact (canonical, required before first feature-review): `artifacts/csharp/coverage.xml` (Phase 0, P0-T5), copy under `docs/features/active/2026-06-08-hierarchical-lcppn-folder-prediction-177/evidence/baseline/<ISO-8601-UTC>/`
  - Post-change coverage artifact: `docs/features/active/2026-06-08-hierarchical-lcppn-folder-prediction-177/evidence/qa-gates/<ISO-8601-UTC>/coverage.xml` (Phase 8, P8-T2)
  - Coverage-comparison artifact: `docs/features/active/2026-06-08-hierarchical-lcppn-folder-prediction-177/evidence/qa-gates/<ISO-8601-UTC>/coverage-comparison.md` (Phase 8, P8-T3)
- **Gate rule:** If any of the three coverage artifacts above is missing, the audit verdict must be BLOCKED or INCOMPLETE, never PASS.

## AC Traceability

| AC | Description | Verifying task(s) / test(s) |
|---|---|---|
| AC1 | Hierarchy construction from RelativePath | P2-T3, P2-T7 (`FolderHierarchyTree_Tests` multi-depth) |
| AC2 | Single-segment path edge case | P2-T4, P2-T7 (single-segment case) |
| AC3 | Idempotent / duplicate-path construction | P2-T5, P2-T7 (duplicate-path case) |
| AC4 | New-leaf construction is local | P2-T6, P2-T7 (new-leaf locality) |
| AC5 | Beam-search descent returns leaf with path-product probability | P4-T6, P4-T7, P4-T12 (`LcppnFolderPredictor_Tests`) |
| AC6 | Configurable beam width; `BeamWidth >= 1` validation | P4-T3, P4-T6, P4-T12 (beam-width recovery case) |
| AC7 | Abstention semantics incl. root abstention | P4-T8, P4-T12 (abstention cases) |
| AC8 | F1 accounting for abstention | P7-T6, P7-T7 (abstention-as-false-negative) |
| AC9 | Shrinkage smoothing with configurable lambda | P3-T3, P3-T6, P3-T7 (blend case + validation) |
| AC10 | Cold-start fallback | P3-T4, P3-T7 (cold-start boundary) |
| AC11 | Localized incremental update | P4-T9, P4-T10, P4-T12 (Train/UnTrain locality) |
| AC12 | New-leaf addition is local | P4-T11, P4-T12 (new-leaf locality) |
| AC13 | Backward compatibility (flat predictor, `Folder.json`) | P1-T5, P6-T6, P6-T7 (flat-path unchanged) |
| AC14 | Shared `IFolderPredictor` seam | P1-T2, P1-T3, P1-T4, P6-T7 (seam reachability) |
| AC15 | Serialization round-trip | P5-T2, P5-T3, P5-T4, P5-T5 (`LcppnFolderPredictor_Serialization_Tests`) |
| AC16 | Deterministic evaluation harness | P7-T4, P7-T5, P7-T7 (deterministic split) |
| AC17 | Test stack and isolation | P8-T5 (test-stack audit) plus all per-phase test tasks |
| AC18 | Coverage (>= 90% new, >= 80% repo, no regression) | P0-T5, P8-T2, P8-T3 (coverage comparison) |
| AC19 | Full C# toolchain passes in order | P1-T6, P2-T8, P3-T8, P4-T13, P5-T6, P6-T8, P7-T8, P8-T1 |
| AC20 | File-size and separation constraints | P8-T4 (file-size + COM-free audit) |

## Open Questions / Notes

- Smoothing coefficient `λ` is fixed and configurable via `ShrinkageLambda` (default 0.7), per research §9 recommendation; per-node estimation is out of scope.
- Root abstention is allowed (research §9): if no root-level child clears `MinimumPathProbability`, `Classify` returns an empty result.
- Per-parent shared token base uses `Corpus` serialized inline (research §9), not `CorpusInherit`, to avoid O(nodes) separate JSON files.
- The evaluation harness split is resolved: `MinedMailInfo` exposes no timestamp/received-date field (verified members: Categories, Tokens, FolderInfo, ToRecipients, CcRecipients, Sender, ConversationId, EntryId, StoreId, Subject, Actionable, GroupingKey), so P7-T4 mandates the deterministic corpus-index proxy for the time-sliced split. The timestamp option is removed; it is not available.
- The `Manager["Folder"]` type parameter change (research §9 open question 5) affects multiple `ManagerAsyncLazy` members. The manager is `ConcurrentObservableDictionary<string, AsyncLazy<BayesianClassifierGroup>>`; the `BayesianClassifierGroup` type parameter surface spans return types, the `DeserializeAsync` path, and the `GetAltLoader`/loader members, all changed to `IFolderPredictor` in P6-T3. The registration itself occurs in `OlFolderClassifierGroup.BuildClassifiersAsync` (around line 211), not `BuildFolderClassifiersAsync`.
- Reparenting is handled by full rebuild, not incremental update (spec Constraints & Risks); no task implements incremental reparenting.
