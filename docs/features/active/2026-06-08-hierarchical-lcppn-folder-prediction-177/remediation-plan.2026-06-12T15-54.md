# Remediation Plan (Cycle 1): hierarchical-lcppn-folder-prediction (#177)

- Plan timestamp: 2026-06-12T15-54 (UTC)
- Authored by: atomic-planner
- Cycle: 1
- Feature folder: `docs/features/active/2026-06-08-hierarchical-lcppn-folder-prediction-177`
- Base: `main` (merge-base `742d4f1656367ddb1d43ea66e1bdd59776f1a287`)
- Head: `TaskMaster-wt-2026-06-08-12-06` (`d06f5c00`)
- Scope source (authoritative): `remediation-inputs.2026-06-12T15-54.md` — exactly two in-scope findings: F1 and F2. No scope widening.

## Scope

In-scope:
- F1 [Major, REQUIRED] — flag-on LCPPN path unreachable in production.
- F2 [Minor, REQUIRED] — raise strict new-code coverage to >= 90% for `FolderHierarchyTree.cs` and `LcppnFolderPredictor.cs`.

Out-of-scope (recorded, not remediated): `FolderHierarchyNode.cs` strict shortfall (auto-generated record members, inclusive 100%); `BayesianClassifierGroup.cs` over-cap (pre-existing, +2 interface declaration only); pre-existing over-cap `SortEmail.cs`/`FolderScorer.cs`; out-of-scope classifier subsystems (`Triage.cs`, `SpamBayes.cs`, `CategoryClassifierGroup.cs`, `MulticlassEngine.cs`).

## F1 mechanism selection (chosen + justification)

Chosen mechanism: add a Folder-only shared holder on the `IAppAutoFileObjects` (AF) surface — a nullable `IFolderPredictor FolderPredictor { get; set; }` property on `IAppAutoFileObjects` (declared in `UtilitiesCS/Interfaces/IGlobals/IAppAutoFileObjects.cs`, implemented in `TaskMaster/AppGlobals/AppAutoFileObjects.cs`). The registration site `OlFolderClassifierGroup.BuildClassifiersAsync` (line ~281) sets `Globals.AF.FolderPredictor` after a flag-on build; `GetFolderPredictorAsync` resolves the held LCPPN predictor from `Globals.AF.FolderPredictor` instead of the per-instance `_lcppnPredictor` field.

Justification: all three production callers (`EmailFiler.cs`, `SortEmail.cs`, `FolderScorer.cs`) construct `new OlFolderClassifierGroup(globals)` per call but pass the same shared `globals`, and all already read `globals.AF.Manager`. A holder on `globals.AF` is therefore reachable by every fresh per-call instance, closing the wiring gap with the smallest seam. This mechanism does NOT retype the shared `Globals.AF.Manager` (`ConcurrentObservableDictionary<string, AsyncLazy<BayesianClassifierGroup>>`) value type, does NOT modify `ManagerAsyncLazy.cs`, and does NOT touch any out-of-scope classifier subsystem. It only adds one Folder-specific member to the AF surface (a separate holder keyed for Folder, distinct from `Manager["Folder"]`), consistent with the recorded Manager-shared-seam constraint. Flag-off behavior is unchanged: when `UseLcppnPredictor` is false the accessor still awaits the unchanged `Manager["Folder"]` entry (AC13 preserved).

Evidence location note: all evidence in this plan resolves to canonical `<FEATURE>/evidence/<kind>/` paths. Canonical post-change coverage XML is mirrored at `artifacts/csharp/coverage.xml` per the C# coverage gate convention.

---

### Phase 0 — Compliance read and remediation baseline

- [x] [P0-T1] Read `CLAUDE.md`, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, and `.claude/rules/csharp.md` in the policy-compliance order; write `docs/features/active/2026-06-08-hierarchical-lcppn-folder-prediction-177/evidence/baseline/phase0-instructions-read.2026-06-12T15-54.md` with `Timestamp:`, `Policy Order:`, and the explicit list of files read. Acceptance: the artifact exists and lists all four policy files in order.
- [x] [P0-T2] Read `remediation-inputs.2026-06-12T15-54.md`, `code-review.2026-06-12T15-43.md`, `OlFolderClassifierGroup.cs`, the three caller sites (`EmailFiler.cs` 371-393, `SortEmail.cs` 250-256/583-585, `FolderScorer.cs` 161-172), `ManagerAsyncLazy.cs`, `IAppAutoFileObjects.cs`, `AppAutoFileObjects.cs`, `FolderHierarchyTree.cs`, and `LcppnFolderPredictor.cs`; record a one-paragraph scope-confirmation note in `evidence/baseline/phase0-context-read.2026-06-12T15-54.md`. Acceptance: the artifact confirms F1/F2 scope and the F1 holder location (`IAppAutoFileObjects.FolderPredictor`).
- [x] [P0-T3] Run CSharpier check on the repo and capture `evidence/baseline/phase0-baseline-csharpier.2026-06-12T15-54.md` with `Timestamp:`, `Command:` (`dotnet tool run csharpier --check .`), `EXIT_CODE:`, `Output Summary:`. Acceptance: artifact records exit code and pass/fail summary.
- [x] [P0-T4] Run the analyzer build and capture `evidence/baseline/phase0-baseline-analyzers.2026-06-12T15-54.md` with `Timestamp:`, `Command:` (`msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`), `EXIT_CODE:`, `Output Summary:`. Acceptance: artifact records exit code and warning/error counts.
- [x] [P0-T5] Run the nullable/TreatWarningsAsErrors build and capture `evidence/baseline/phase0-baseline-nullable.2026-06-12T15-54.md` with `Timestamp:`, `Command:` (`msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`), `EXIT_CODE:`, `Output Summary:`. Acceptance: artifact records exit code; pre-existing unrelated CS8625 in other test files noted as out of scope.
- [x] [P0-T6] Run the UtilitiesCS.Test assembly under coverage and capture `evidence/baseline/phase0-baseline-tests.2026-06-12T15-54.md` with `Timestamp:`, `Command:` (`vstest.console.exe <UtilitiesCS.Test.dll> /EnableCodeCoverage`), `EXIT_CODE:`, `Output Summary:` including the baseline strict per-type coverage for `FolderHierarchyTree` (86.4%) and `LcppnFolderPredictor` (89.1%) and the repo-wide strict total (85.40%). Acceptance: artifact records numeric baseline coverage headline values for both target types.

#### End-of-phase gate (Phase 0)

- [x] [P0-T7] Confirm all Phase 0 baseline artifacts exist with required fields (`Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`) and numeric coverage values present in P0-T6. Acceptance: all six baseline artifacts are present and field-complete.

---

### Phase 1 — F1: shared Folder predictor holder seam

- [x] [P1-T1] Add a nullable Folder-only holder property `IFolderPredictor FolderPredictor { get; set; }` to the `IAppAutoFileObjects` interface in `UtilitiesCS/Interfaces/IGlobals/IAppAutoFileObjects.cs`, with an XML doc comment stating it holds the flag-on LCPPN predictor and is null when the flat path is active. Acceptance: the interface declares `IFolderPredictor FolderPredictor { get; set; }` and the file compiles.
- [x] [P1-T2] Implement the `FolderPredictor` property (default `null`) on the concrete `AppAutoFileObjects` class in `TaskMaster/AppGlobals/AppAutoFileObjects.cs` near the existing `Manager` property (line ~609). Acceptance: `AppAutoFileObjects` defines `public IFolderPredictor FolderPredictor { get; set; }` and the file compiles.
- [x] [P1-T3] In `OlFolderClassifierGroup.GetFolderPredictorAsync` (`OlFolderClassifierGroup.cs` lines 83-91), change the flag-on branch to resolve the held predictor from `Globals.AF.FolderPredictor` instead of the per-instance `_lcppnPredictor` field: when `FolderPredictorConfig?.UseLcppnPredictor == true && Globals.AF.FolderPredictor is not null` return `Globals.AF.FolderPredictor`; otherwise await and return `Globals.AF.Manager["Folder"]`. Acceptance: the accessor reads `Globals.AF.FolderPredictor` and no longer reads `_lcppnPredictor`.
- [x] [P1-T4] In `OlFolderClassifierGroup.BuildClassifiersAsync` (`OlFolderClassifierGroup.cs` line ~279-282), set `Globals.AF.FolderPredictor = await BuildLcppnPredictorAsync(collection)` inside the existing `UseLcppnPredictor == true` block (replacing the assignment to the instance field). Acceptance: the flag-on build assigns the built predictor to `Globals.AF.FolderPredictor`.
- [x] [P1-T5] Update `SetLcppnPredictor` (`OlFolderClassifierGroup.cs` lines 70-73) to set `Globals.AF.FolderPredictor = predictor` so the existing seam-test entry point routes through the shared holder; remove the now-unused `_lcppnPredictor` field (line 38) and its comment. Acceptance: `SetLcppnPredictor` writes to `Globals.AF.FolderPredictor` and the `_lcppnPredictor` field no longer exists.
- [x] [P1-T6] Update the existing seam tests in `UtilitiesCS.Test/EmailIntelligence/FolderPredictorSeam_Tests.cs` so `CreateMockGlobalsWithFolder` also sets up `mockAf.SetupProperty(x => x.FolderPredictor)` (a real backing store on the mock), keeping AC13/AC14 assertions intact. Acceptance: the four existing seam tests compile and assert against the shared-holder seam.
- [x] [P1-T7] Add a regression test `GetFolderPredictorAsync_FlagOn_ReachableThroughFreshPerCallInstance` to `FolderPredictorSeam_Tests.cs` that sets `Globals.AF.FolderPredictor` to a built LCPPN predictor on shared mock globals, then constructs two separate `new OlFolderClassifierGroup(globals)` instances (mirroring the production per-call pattern) and asserts both return the same LCPPN predictor via `GetFolderPredictorAsync`. Acceptance: the test proves the flag-on predictor is reachable from a fresh per-call instance (not only the build-time instance) and passes.
- [x] [P1-T8] Add a regression test `GetFolderPredictorAsync_FlagOff_FreshPerCallInstance_ReturnsFlat` to `FolderPredictorSeam_Tests.cs` asserting that with `UseLcppnPredictor` off and `Globals.AF.FolderPredictor` null, a fresh per-call instance returns the flat `Manager["Folder"]` group byte-for-byte (AC13 preserved). Acceptance: the test passes and confirms flag-off behavior is unchanged.

#### End-of-phase gate (Phase 1)

- [x] [P1-T9] Run the full C# toolchain in order — (1) `dotnet tool run csharpier .`, (2) analyzer msbuild, (3) nullable/TreatWarningsAsErrors msbuild, (4) `vstest.console.exe <UtilitiesCS.Test.dll> /EnableCodeCoverage` — restarting from CSharpier if any step changes files or fails; use the incremental nullable gate convention (pre-existing unrelated CS8625 in other test files out of scope). Capture one artifact per step under `evidence/qa-gates/2026-06-12T15-54/` (`p1-csharpier.md`, `p1-analyzers.md`, `p1-nullable.md`, `p1-tests.md`), each with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. Acceptance: all four steps pass in a single final pass and the new F1 regression tests are green.

---

### Phase 2 — F2: strict new-code coverage to >= 90%

- [x] [P2-T1] Add targeted tests to the existing `UtilitiesCS.Test/EmailIntelligence/Bayesian/FolderHierarchyTree_Tests.cs` covering the uncovered `FolderHierarchyTree` members/branches: `GetChildren` null-key and unknown-node early returns (empty array), `NodeKeys` accessor on a populated tree, `GetNode` null/missing-key null return, `IsLeaf` false branches (non-existent node and node with children), and `ContainsNode` null/false branches. Acceptance: tests are deterministic MSTest (Moq/FluentAssertions as needed), in-memory, no temp files/COM, and they exercise `GetChildren`/`NodeKeys` plus the named branches.
- [x] [P2-T2] Add targeted tests to the existing `UtilitiesCS.Test/EmailIntelligence/Bayesian/LcppnFolderPredictor_Tests.cs` covering the uncovered descent/abstention branches: terminal-leaf emission when a frontier node has no classifier (`partial.NodeKey.Length > 0` branch in `DescendBeam`), the `scores.Count == 0` terminal branch, the `Math.Exp(top.LogProbability) < MinimumPathProbability` abstention path returning `Empty()`, the beam-trim branch (`next.Count > BeamWidth`), and `UnTrain` on a tag whose intermediate parent key is absent from `Nodes` (TryGetValue miss). Acceptance: tests are deterministic MSTest, in-memory, no temp files/COM, and they exercise the named descent/abstention/untrain branches.
- [x] [P2-T3] If any new test file was created in P2-T1/P2-T2 (rather than extending the existing registered files), add the corresponding `<Compile Include="..." />` entry to `UtilitiesCS.Test/UtilitiesCS.Test.csproj` in the EmailIntelligence test region. Acceptance: every new test `.cs` file has a matching Compile Include; if no new file was created, this task records "no new file — N/A". (Note: the targeted F2 test files already have Compile Include entries at lines 116 and 118.)

#### End-of-phase gate (Phase 2)

- [x] [P2-T4] Run the full C# toolchain in order (CSharpier -> analyzer msbuild -> nullable/TreatWarningsAsErrors msbuild -> `vstest.console.exe <UtilitiesCS.Test.dll> /EnableCodeCoverage`), restarting from CSharpier on any file change or failure. Capture one artifact per step under `evidence/qa-gates/2026-06-12T15-54/` (`p2-csharpier.md`, `p2-analyzers.md`, `p2-nullable.md`, `p2-tests.md`), each with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` including post-change strict per-type coverage for `FolderHierarchyTree` and `LcppnFolderPredictor`. Acceptance: all four steps pass in a single final pass and both target types report >= 90% strict line coverage.

---

### Phase 3 — Final QA loop and coverage verification

- [x] [P3-T1] Run the full C# toolchain end-to-end one final time in order — (1) `dotnet tool run csharpier .`, (2) `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`, (3) `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`, (4) `vstest.console.exe <UtilitiesCS.Test.dll> /EnableCodeCoverage` — restarting from CSharpier if any step changes files or fails. Capture one artifact per step under `evidence/qa-gates/2026-06-12T15-54/` (`final-csharpier.md`, `final-analyzers.md`, `final-nullable.md`, `final-tests.md`), each with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`. Acceptance: all four steps pass in a single final pass.
- [x] [P3-T2] Merge the post-change coverage to XML, write it to the canonical `artifacts/csharp/coverage.xml`, and record `evidence/qa-gates/2026-06-12T15-54/coverage-comparison.md` with baseline strict, post-change strict, and new/changed-code strict per-type values for `FolderHierarchyTree` and `LcppnFolderPredictor`, plus the repo-wide strict total. Acceptance: the comparison shows both target types at >= 90% strict, repo-wide strict >= 80% with no regression vs the 85.40% baseline, and `artifacts/csharp/coverage.xml` is updated.
- [x] [P3-T3] Record `evidence/regression-testing/f1-flag-on-reachability.2026-06-12T15-54.md` linking the P1-T7 fresh-per-call regression test name, its run result, and the AC13 flag-off preservation result from P1-T8. Acceptance: the artifact names both regression tests and records PASS for the flag-on reachability and flag-off-unchanged outcomes.
- [x] [P3-T4] Confirm the HARD constraints held: `git diff` shows zero changes to `ManagerAsyncLazy.cs`, no change to the `Manager` dictionary value type, and no changes to `Triage.cs`/`SpamBayes.cs`/`CategoryClassifierGroup.cs`/`MulticlassEngine.cs`. Record `evidence/other/f1-containment-check.2026-06-12T15-54.md` listing the touched files. Acceptance: the artifact confirms only `IAppAutoFileObjects.cs`, `AppAutoFileObjects.cs`, `OlFolderClassifierGroup.cs`, and the two/three test files were changed, and the prohibited files are untouched.
