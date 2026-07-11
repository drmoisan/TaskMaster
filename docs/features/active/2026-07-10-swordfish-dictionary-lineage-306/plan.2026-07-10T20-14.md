# swordfish-dictionary-lineage — Atomic Implementation Plan (Issue #306)

- **Issue:** #306
- **Issue URL:** https://github.com/drmoisan/TaskMaster/issues/306
- **Epic:** swordfish-removal (child F1, wave 0)
- **Integration branch:** epic/swordfish-removal-integration
- **Owner:** drmoisan
- **Work Mode:** full-feature
- **Last Updated:** 2026-07-10T20-14
- **Plan file (continuity target):** docs/features/active/2026-07-10-swordfish-dictionary-lineage-306/plan.2026-07-10T20-14.md

## Objective

Re-point every production consumer of the vendored Swordfish-based `ScoDictionary<TKey,TValue>` (`UtilitiesCS\ReusableTypeClasses\Serializable\Concurrent\SCO\SCODictionary.cs`) to the Swordfish-free `UtilitiesCS.ReusableTypeClasses.ScoDictionaryNew<TKey,TValue>` (`UtilitiesCS\ReusableTypeClasses\SerializableNew\Concurrent\Observable\ScoDictionaryNew.cs`), migrate affected tests, and preserve on-disk JSON serialization compatibility for every persisted dictionary. Authoritative acceptance criteria live in `spec.md` (`## Acceptance Criteria`); serialization behavior, consumer inventory, and construction reconciliation are governed by `research/research-dictionary-lineage.2026-07-10T20-16.md`.

## Research-Grounded Invariants (binding on all phases)

1. On-disk production payloads are NOT type-name-embedded (flat `{"key": value}` object; write-path `TypeNameHandling` is `None`, and the default `SmartSerializable` `Auto` path emits no `$type` because declared type equals runtime type). A bare type swap preserves on-disk compatibility WITHOUT a serialization binder or converter, CONDITIONAL on not using the globals-based `GetSettingsJson<T>(globals)` path.
2. The globals path (`ScoDictionaryNew<...>.GetSettingsJson<T>(globals)` / `Static.GetSettingsJson<T>(globals)`) registers `ScoDictionaryConverter` + `PreserveReferencesHandling.All` and emits an incompatible `{ "$id", "CoDictionary", "RemainingObject" }` wrapper. None of the four persisted dictionaries may use it.
3. Persisted dictionaries requiring on-disk round-trip compatibility tests: `DictRemap` (AppToDoObjects), `FilteredFolderScraping` (AppToDoObjects), `FolderRemap` (AppToDoObjects), SubjectMap `Encoder`. In-memory-only (pure type swap, no on-disk test): SubjectMap `Decoder`, `FolderScorer._folderNameScores`.
4. `ScoDictionaryNew` has no self-loading `(filename, folderpath)` constructor; persisted instances load via `ScoDictionaryNew<...>.Static.Deserialize(fileName, folderPath)` (factory returns a new instance) and persist via plain `Serialize()` / `SerializeToString()`. Writes are deferred via a 3-second `ITimerWrapper`; tests must drive `SerializeToString()` / `SerializeThreadSafe(path)` directly or inject the `TimerFactory` seam, never wall-clock elapse.
5. `PeopleScoDictionary.cs` is entirely block-commented (inert) — confirm-only, no code change.

## Scope Boundary (do NOT plan any of this)

- Do NOT delete `UtilitiesSwordfish`, remove any `ProjectReference`, or touch `TaskMaster.sln` (child F5).
- Do NOT migrate `IScoCollection` / `IScoCollection2` (child F5).
- Do NOT touch collection/stack types (child F2) or `ScoSortedDictionary` (child F3).
- Do NOT switch any persisted dictionary to the globals-based `GetSettingsJson` / `ScoDictionaryConverter` / `PreserveReferencesHandling` path.
- `SCODictionary.cs` deletion is OPTIONAL (Phase 8), not a goal of F1.

## Evidence Location Compliance

All evidence resolves under `docs/features/active/2026-07-10-swordfish-dictionary-lineage-306/evidence/<kind>/` per `evidence-and-timestamp-conventions`:
- Baseline command-step artifacts: `evidence/baseline/`
- Final-QC command-step artifacts and coverage delta: `evidence/qa-gates/`
- Any fail-before / expect-fail dossiers: `evidence/regression-testing/`

No `artifacts/` evidence path is used. The caller-supplied paths are already canonical; no `EVIDENCE_LOCATION_OVERRIDE_REJECTED` correction was required.

## Scope-Lock (files this plan may create or modify)

Production source (MODIFY):
- `UtilitiesCS\Interfaces\IGlobals\IToDoObjects.cs`
- `UtilitiesCS\Interfaces\IToDo\ISubjectMapEncoder.cs`
- `UtilitiesCS\Interfaces\IOutlookObjects\IEmailDetailsWrapper.cs`
- `TaskMaster\AppGlobals\AppToDoObjects.cs`
- `UtilitiesCS\EmailIntelligence\SubjectMap\SubjectMapEncoder.cs`
- `UtilitiesCS\OutlookObjects\Folder\FolderScorer.cs`
- `UtilitiesCS\OutlookObjects\MailItem\EmailDetails.cs`
- `UtilitiesCS\OutlookObjects\MailItem\EmailDetailsWrapper.cs`
- `UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.cs`

Test source (CREATE / MODIFY):
- CREATE `UtilitiesCS.Test\ReusableTypeClasses\SerializableNew\ScoDictionaryNew_OnDiskCompatibility_Tests.cs`
- MODIFY `UtilitiesCS.Test\UtilitiesCS.Test.csproj` (explicit `<Compile Include>` wiring for the new test file — this project is a legacy packages.config project with explicit `<Compile Include>` items and NO glob; a new `.cs` will not compile without wiring)
- MODIFY `UtilitiesCS.Test\EmailIntelligence\FilterOlFoldersController_Tests.cs`
- MODIFY `UtilitiesCS.Test\EmailIntelligence\FilterOlFoldersViewer_Tests.cs`
- MODIFY `UtilitiesCS.Test\EmailIntelligence\SubjectMapSco_Orchestration_Tests.cs`
- MODIFY `UtilitiesCS.Test\EmailIntelligence\OlFolderClassifierGroup_Tests.cs`
- MODIFY `UtilitiesCS.Test\EmailIntelligence\SubjectMapSco_Tests.cs`
- MODIFY `UtilitiesCS.Test\OutlookObjects\MailItem\EmailDetailsTests.cs`
- MODIFY `UtilitiesCS.Test\OutlookObjects\MailItem\EmailDetailsWrapperTests.cs`
- MODIFY `UtilitiesCS.Test\EmailIntelligence\EmailDataMiner_TestSupport.cs`
- MODIFY `UtilitiesCS.Test\EmailIntelligence\EmailDataMiner_Tests.cs`
- MODIFY `UtilitiesCS.Test\EmailIntelligence\EmailDataMiner_Additional_Tests.cs`
- MODIFY `UtilitiesCS.Test\EmailIntelligence\EmailDataMiner_FolderExtractionCoverage_Tests.cs`
- MODIFY `UtilitiesCS.Test\EmailIntelligence\FolderRemapController_Tests.cs`
- MODIFY `UtilitiesCS.Test\EmailIntelligence\FolderRemapController_Tests2.cs`

Optional (Phase 8 only, if legacy deletion is pursued):
- DELETE `UtilitiesCS\ReusableTypeClasses\Serializable\Concurrent\SCO\SCODictionary.cs`
- DELETE `UtilitiesCS.Test\ReusableTypeClasses\SCODictionary_Tests.cs`, `UtilitiesCS.Test\ReusableTypeClasses\SCODictionary_Additional_Tests.cs`
- MODIFY `UtilitiesCS.Test\ReusableTypeClasses\SmartSerializableStatic_Tests.cs`, `SmartSerializableNonTyped_Tests.cs`, `SmartSerializableBase_Tests.cs`
- MODIFY `UtilitiesCS\UtilitiesCS.csproj`, `UtilitiesCS.Test\UtilitiesCS.Test.csproj` (remove `<Compile Include>` entries for deleted files)

## Implementation Plan (Atomic Tasks)

### Phase 0 — Baseline Capture and Policy Compliance

- [x] [P0-T1] Read the four policy documents in compliance order — `CLAUDE.md`, `.claude/rules/general-code-change.md`, `.claude/rules/general-unit-test.md`, `.claude/rules/csharp.md` — and write `docs/features/active/2026-07-10-swordfish-dictionary-lineage-306/evidence/baseline/phase0-instructions-read.md`.
  - Acceptance: Artifact exists and contains `Timestamp:`, `Policy Order:` (the four files in order), and an explicit `Files Read:` list naming all four absolute paths. No policy document is modified.
- [x] [P0-T2] Capture the CSharpier formatting baseline by running `dotnet tool run csharpier --check .` (or `csharpier --check .`) at the worktree root and write `docs/features/active/2026-07-10-swordfish-dictionary-lineage-306/evidence/baseline/baseline-csharpier.md`.
  - Acceptance: Artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (pass/fail and count of files needing formatting, if any).
- [x] [P0-T3] Capture the .NET analyzer baseline by running `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` and write `docs/features/active/2026-07-10-swordfish-dictionary-lineage-306/evidence/baseline/baseline-analyzers.md`.
  - Acceptance: Artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (build succeeded/failed, warning/error counts).
- [x] [P0-T4] Capture the nullable / TreatWarningsAsErrors type-check baseline by running `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true` and write `docs/features/active/2026-07-10-swordfish-dictionary-lineage-306/evidence/baseline/baseline-nullable.md`.
  - Acceptance: Artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` (build succeeded/failed, warning/error counts).
- [x] [P0-T5] Capture the MSTest + coverage baseline by running `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /EnableCodeCoverage` and write `docs/features/active/2026-07-10-swordfish-dictionary-lineage-306/evidence/baseline/baseline-tests-coverage.md`.
  - Acceptance: Artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with numeric passed/failed/total test counts AND numeric coverage headline values (repo-wide line coverage % and, where obtainable, the coverage % for the files listed in the scope-lock). Placeholder values such as `UNVERIFIED` are not acceptable.

### Phase 1 — Cross-Module Interface Contract Re-point

- [x] [P1-T1] In `UtilitiesCS\Interfaces\IGlobals\IToDoObjects.cs`, change the `FilteredFolderScraping` member type to `ScoDictionaryNew<string,int>`, the `FolderRemap` member type to `ScoDictionaryNew<string,string>`, and the `DictRemap` member type to `IScoDictionaryNew<string,string>`; add the required `using` for the `SerializableNew` namespace.
  - Acceptance: The three members declare the new lineage types; no `ScoDictionary<>` / `IScoDictionary<>` reference remains in this file. Verified by grep.
- [x] [P1-T2] In `UtilitiesCS\Interfaces\IToDo\ISubjectMapEncoder.cs`, change the `Encoder` return type to `IScoDictionaryNew<string,int>`; add the required `using`.
  - Acceptance: `Encoder` returns `IScoDictionaryNew<string,int>`; no legacy `IScoDictionary<>` reference remains in this file. Verified by grep.
- [x] [P1-T3] In `UtilitiesCS\OutlookObjects\MailItem\EmailDetails.cs`, change every `IScoDictionary<string,string> dictRemap` parameter (lines ~32, ~71, ~308) to `IScoDictionaryNew<string,string> dictRemap`; add the required `using`.
  - Acceptance: No `IScoDictionary<string,string>` parameter remains in this file; all `dictRemap` parameters use `IScoDictionaryNew<string,string>`. Verified by grep.
- [x] [P1-T4] In `UtilitiesCS\OutlookObjects\MailItem\EmailDetailsWrapper.cs`, change the `IScoDictionary<string,string> dictRemap` parameter (line ~17) to `IScoDictionaryNew<string,string> dictRemap`; add the required `using`.
  - Acceptance: No `IScoDictionary<string,string>` parameter remains in this file; the `dictRemap` parameter uses `IScoDictionaryNew<string,string>`. Verified by grep.
- [x] [P1-T5] In `UtilitiesCS\Interfaces\IOutlookObjects\IEmailDetailsWrapper.cs`, change the `Details(...)` method's `IScoDictionary<string,string> dictRemap = null` parameter (line ~13) to `IScoDictionaryNew<string,string> dictRemap = null`, keeping the default value; add the required `using` for the `SerializableNew` namespace.
  - Acceptance: The `Details` method declares `IScoDictionaryNew<string,string> dictRemap = null`; no legacy `IScoDictionary<string,string>` reference remains in this file; `EmailDetailsWrapper` still implements `IEmailDetailsWrapper` (no CS0535). Verified by grep and by the Phase 5 build.

### Phase 2 — AppToDoObjects Persisted-Dictionary Re-point

- [x] [P2-T1] In `TaskMaster\AppGlobals\AppToDoObjects.cs`, change the `_dictRemap` field and `DictRemap` property to the new lineage (`ScoDictionaryNew<string,string>` / `IScoDictionaryNew<string,string>` matching the interface), and replace the self-loading `new ScoDictionary<string,string>(filename: FnameDictRemap, folderpath: pythonStaging)` construction (lines ~299-313) with `ScoDictionaryNew<string,string>.Static.Deserialize(FnameDictRemap, pythonStaging)`. Preserve the existing `Initialized` / `Initializer.GetOrLoad` lazy pattern.
  - Acceptance: `_dictRemap` and `DictRemap` use the new lineage; construction uses `Static.Deserialize`; the globals-based `GetSettingsJson` path is NOT used. Verified by grep for `ScoDictionary<` (absent for DictRemap) and `Static.Deserialize`.
- [x] [P2-T2] In `TaskMaster\AppGlobals\AppToDoObjects.cs`, change the `_filteredFolderScraping` field and `FilteredFolderScraping` property to `ScoDictionaryNew<string,int>`, and replace the self-loading `new ScoDictionary<string,int>(_defaults.FileName_FilteredFolderScraping, pythonStaging)` construction (lines ~427-441) with `ScoDictionaryNew<string,int>.Static.Deserialize(_defaults.FileName_FilteredFolderScraping, pythonStaging)`. Preserve the lazy pattern.
  - Acceptance: `FilteredFolderScraping` uses the new lineage and `Static.Deserialize`; no globals path. Verified by grep.
- [x] [P2-T3] In `TaskMaster\AppGlobals\AppToDoObjects.cs`, change the `_folderRemap` field and `FolderRemap` property to `ScoDictionaryNew<string,string>`, and replace the self-loading `new ScoDictionary<string,string>(_defaults.FileName_FolderRemap, pythonStaging)` construction (lines ~455-469) with `ScoDictionaryNew<string,string>.Static.Deserialize(_defaults.FileName_FolderRemap, pythonStaging)`. Preserve the lazy pattern.
  - Acceptance: `FolderRemap` uses the new lineage and `Static.Deserialize`; no globals path. Verified by grep. No `ScoDictionary<` construction remains anywhere in `AppToDoObjects.cs`.

### Phase 3 — SubjectMapEncoder Construction and Persistence Reconciliation

- [x] [P3-T1] In `UtilitiesCS\EmailIntelligence\SubjectMap\SubjectMapEncoder.cs`, change the `_encoder` field type to `IScoDictionaryNew<string,int>` and replace the self-loading `new ScoDictionary<string,int>(filename, folderpath)` constructions in the constructor (line ~22) and in the `Encoder` getter (lines ~90-94) with `ScoDictionaryNew<string,int>.Static.Deserialize(filename, folderpath)`.
  - Acceptance: `_encoder` is `IScoDictionaryNew<string,int>`; both construction sites use `Static.Deserialize`; no globals path. Verified by grep.
- [x] [P3-T2] In `UtilitiesCS\EmailIntelligence\SubjectMap\SubjectMapEncoder.cs`, rewrite the no-arg legacy `_encoder.Deserialize()` call site (line ~40) to the new-lineage factory-load equivalent (`ScoDictionaryNew<string,int>.Static.Deserialize(filename, folderpath)` assignment), preserving the missing-file create-empty-and-write behavior.
  - Acceptance: No no-arg `Deserialize()` call remains on `_encoder`; missing-file behavior is preserved (create empty and write). Verified by grep for `.Deserialize()` on `_encoder`.
- [x] [P3-T3] In `UtilitiesCS\EmailIntelligence\SubjectMap\SubjectMapEncoder.cs`, rewrite the no-arg legacy `_encoder.ToDictionary()` call site (line ~131) to a new-lineage equivalent (`new Dictionary<string,int>(_encoder)` or direct iteration), preserving the duplicate-key rebuild path (lines ~47-81) and the immediate re-serialize.
  - Acceptance: No no-arg `ToDictionary()` call remains on `_encoder`; duplicate-key rebuild path is preserved and still calls `Serialize()`. Verified by grep.
- [x] [P3-T4] In `UtilitiesCS\EmailIntelligence\SubjectMap\SubjectMapEncoder.cs`, change the `_decoder` field type to `IScoDictionaryNew<int,string>` as a pure in-memory type swap (built from the in-memory `IEnumerable<KeyValuePair<>>`; never assigned a filename/folderpath; never serialized).
  - Acceptance: `_decoder` is `IScoDictionaryNew<int,string>`; no filename/folderpath or serialize call is introduced for `_decoder`. Verified by grep. No `ScoDictionary<` reference remains anywhere in `SubjectMapEncoder.cs`.
- [x] [P3-T5] In `UtilitiesCS\EmailIntelligence\EmailParsingSorting\SortEmail.cs`, migrate the two `await appGlobals.AF.Encoder.Encoder.SerializeAsync();` call sites (lines ~205 and ~440) to the new lineage, which exposes no async serialize. Replace each with the new-lineage synchronous persistence call (`appGlobals.AF.Encoder.Encoder.Serialize();`, removing the `await`), preserving the existing persist-on-completion behavior and the surrounding `async` method signature.
  - Acceptance: No `.SerializeAsync()` call remains on `AF.Encoder.Encoder` in this file (verified by grep); the file compiles against the `ISubjectMapEncoder.Encoder : IScoDictionaryNew<string,int>` contract; verified by the Phase 5 build.

### Phase 4 — FolderScorer In-Memory Re-point

- [x] [P4-T1] In `UtilitiesCS\OutlookObjects\Folder\FolderScorer.cs`, change `_folderNameScores` (line ~27) from `ScoDictionary<string,long>` to `ScoDictionaryNew<string,long>` as a pure in-memory type swap (`= new()`), preserving the existing `.Clear()`, `.Count`, `.TryAdd`, indexer, and LINQ usage; add the required `using`.
  - Acceptance: `_folderNameScores` is `ScoDictionaryNew<string,long>`; no filename or serialize/deserialize is introduced; no `ScoDictionary<` reference remains in `FolderScorer.cs`. Verified by grep.

### Phase 5 — Production Build and Analyzer Verification

- [x] [P5-T1] Run `dotnet tool run csharpier .` (write mode) at the worktree root to format the modified production files, then run `dotnet tool run csharpier --check .` and confirm a clean check.
  - Acceptance: `csharpier --check .` exits 0 (no files need formatting). If write mode changed files, that is acceptable within this task; the final `--check` must be clean.
- [x] [P5-T2] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` and confirm the full solution compiles with the interface contract change (all implementers and callers of `IToDoObjects`, `ISubjectMapEncoder`, `EmailDetails`, `EmailDetailsWrapper` build).
  - Acceptance: Build exits 0; no analyzer errors introduced relative to the Phase 0 analyzer baseline. Cross-module contract change compiles across `UtilitiesCS`, `TaskMaster`, and all referencing projects.
- [x] [P5-T3] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true` and confirm no new nullable/type warnings-as-errors were introduced.
  - Acceptance: Build exits 0; no new nullable warnings on touched code paths relative to the Phase 0 nullable baseline.

### Phase 6 — Persisted-Dictionary On-Disk Compatibility Tests

- [x] [P6-T1] Create `UtilitiesCS.Test\ReusableTypeClasses\SerializableNew\ScoDictionaryNew_OnDiskCompatibility_Tests.cs` and wire it into `UtilitiesCS.Test\UtilitiesCS.Test.csproj` with an explicit `<Compile Include="ReusableTypeClasses\SerializableNew\ScoDictionaryNew_OnDiskCompatibility_Tests.cs" />` item. Add a `[TestClass]` skeleton using MSTest + Moq + FluentAssertions with the injected `ReadAllText` / `DiskExists` / `CreateStreamWriter` / `TimerFactory` seams (no temporary files, no wall-clock waits).
  - Acceptance: The file exists AND is wired into the csproj so it compiles into `UtilitiesCS.Test.dll` (single binary outcome). `msbuild` on the test project produces the assembly containing this class. No temp-file or `Thread.Sleep`/`Task.Delay`/wall-clock API is used.
- [x] [P6-T2] Add a round-trip compatibility test for `DictRemap` (`ScoDictionaryNew<string,string>`): supply a representative flat `{"key":"value"}` payload as an embedded string via the injected read seam, load it through `ScoDictionaryNew<string,string>.Static.Deserialize` (or the instance `Deserialize` with the injected seam), assert entry fidelity, re-serialize via `SerializeToString()`, and assert the output is a flat object with NO `$type`/`$id`/`CoDictionary`/`RemainingObject` tokens.
  - Acceptance: The `[TestMethod]` exists, uses an embedded string payload (no temp file), asserts entry fidelity and absence of the four wrapper tokens, and passes under vstest.
- [x] [P6-T3] Add a round-trip compatibility test for `FilteredFolderScraping` (`ScoDictionaryNew<string,int>`) following the P6-T2 pattern with an embedded flat `{"key":int}` payload.
  - Acceptance: The `[TestMethod]` exists, uses an embedded payload, asserts entry fidelity and absence of `$type`/`$id`/`CoDictionary`/`RemainingObject`, and passes under vstest.
- [x] [P6-T4] Add a round-trip compatibility test for `FolderRemap` (`ScoDictionaryNew<string,string>`) following the P6-T2 pattern with an embedded flat `{"key":"value"}` payload.
  - Acceptance: The `[TestMethod]` exists, uses an embedded payload, asserts entry fidelity and absence of the four wrapper tokens, and passes under vstest.
- [x] [P6-T5] Add a round-trip compatibility test for the SubjectMap `Encoder` (`ScoDictionaryNew<string,int>`) following the P6-T2 pattern with an embedded flat `{"key":int}` payload representative of the SubjectMap encoder file.
  - Acceptance: The `[TestMethod]` exists, uses an embedded payload, asserts entry fidelity and absence of the four wrapper tokens, and passes under vstest.
- [x] [P6-T6] Add an assertion (within the new test file) that the migrated persisted dictionaries do NOT register the globals path: verify that a default `SerializeToString()` output for each of the four persisted types is a flat object and never emits `$id`/`CoDictionary`/`RemainingObject` (the `GetSettingsJson<T>(globals)` / `ScoDictionaryConverter` / `PreserveReferencesHandling.All` wrapper shape).
  - Acceptance: A `[TestMethod]` asserts the default-path output for the persisted types is wrapper-free, encoding the globals-path prohibition (spec AC on the globals-converter constraint). Passes under vstest.

### Phase 7 — Consumer-Coupled Test Fixture Migration

- [x] [P7-T1] In `UtilitiesCS.Test\EmailIntelligence\FilterOlFoldersController_Tests.cs`, migrate every `new ScoDictionary<string,int>()` FilteredFolderScraping fixture (lines ~28, 71, 90, 114, 134, 151, 172, 226) to `ScoDictionaryNew<string,int>` / `IScoDictionaryNew<string,int>`; add the required `using`.
  - Acceptance: No `ScoDictionary<` construction remains in this file; fixtures use the new lineage. Verified by grep.
- [x] [P7-T2] In `UtilitiesCS.Test\EmailIntelligence\SubjectMapSco_Orchestration_Tests.cs`, migrate the FolderRemap / FilteredFolderScraping fixtures (lines ~272, 279, 285) to the new lineage; add the required `using`.
  - Acceptance: No `ScoDictionary<` construction remains in this file; fixtures use the new lineage. Verified by grep.
- [x] [P7-T3] In `UtilitiesCS.Test\EmailIntelligence\OlFolderClassifierGroup_Tests.cs`, migrate the `ScoDictionary` fixture (line ~93) to the new lineage; add the required `using`.
  - Acceptance: No `ScoDictionary<` construction remains in this file; fixture uses the new lineage. Verified by grep.
- [x] [P7-T4] In `UtilitiesCS.Test\EmailIntelligence\SubjectMapSco_Tests.cs`, migrate the `IScoDictionary<string,int> Encoder` fixture (line ~352) to `IScoDictionaryNew<string,int>`; add the required `using`.
  - Acceptance: No `IScoDictionary<string,int>` remains in this file; the Encoder fixture uses `IScoDictionaryNew<string,int>`. Verified by grep.
- [x] [P7-T5] In `UtilitiesCS.Test\OutlookObjects\MailItem\EmailDetailsTests.cs`, migrate the `new ScoDictionary<string,string>(...)` dictRemap fixture (line ~144) to `ScoDictionaryNew<string,string>` / `IScoDictionaryNew<string,string>`; add the required `using`.
  - Acceptance: No `ScoDictionary<` construction remains in this file; dictRemap fixture uses the new lineage. Verified by grep.
- [x] [P7-T6] In `UtilitiesCS.Test\OutlookObjects\MailItem\EmailDetailsWrapperTests.cs`, migrate the `new ScoDictionary<string,string>(...)` dictRemap fixture (line ~152) to the new lineage; add the required `using`.
  - Acceptance: No `ScoDictionary<` construction remains in this file; dictRemap fixture uses the new lineage. Verified by grep.
- [x] [P7-T7] In `UtilitiesCS.Test\EmailIntelligence\EmailDataMiner_TestSupport.cs`, migrate the hand-written `StubToDoObjects : IToDoObjects` double: change `FilteredFolderScraping` (line ~125) to `ScoDictionaryNew<string,int>`, `FolderRemap` (line ~127) to `ScoDictionaryNew<string,string>`, `DictRemap` (line ~109) to `IScoDictionaryNew<string,string>`, and the constructor parameters (lines ~97-98) and their `??` defaults (lines ~101-102) to the new lineage; add the required `using`.
  - Acceptance: StubToDoObjects compiles against the changed `IToDoObjects`; no `ScoDictionary<`/`IScoDictionary<` remains in this file (verified by grep).
- [x] [P7-T8] In `UtilitiesCS.Test\EmailIntelligence\EmailDataMiner_Tests.cs`, migrate the `StubToDoObjects` constructor-argument fixtures (`filteredFolderScraping: new ScoDictionary<string,int>` line ~171, `folderRemap: new ScoDictionary<string,string>` line ~175) to `ScoDictionaryNew<>`; add the required `using`.
  - Acceptance: No `ScoDictionary<` construction remains (verified by grep).
- [x] [P7-T9] In `UtilitiesCS.Test\EmailIntelligence\EmailDataMiner_Additional_Tests.cs`, migrate the `new ScoDictionary<string,int>` fixture (line ~109) fed to `Mock<IToDoObjects>.SetupGet(x => x.FilteredFolderScraping)` (line ~111) to `ScoDictionaryNew<string,int>`; add the required `using`.
  - Acceptance: No `ScoDictionary<` construction remains (verified by grep).
- [x] [P7-T10] In `UtilitiesCS.Test\EmailIntelligence\EmailDataMiner_FolderExtractionCoverage_Tests.cs`, migrate the `Mock<IToDoObjects>.Returns(new ScoDictionary<string,int>())` fixtures (lines ~57, ~81) to `ScoDictionaryNew<string,int>`; add the required `using`.
  - Acceptance: No `ScoDictionary<` construction remains (verified by grep).
- [x] [P7-T11] In `UtilitiesCS.Test\EmailIntelligence\FolderRemapController_Tests.cs`, migrate the five `Mock<IToDoObjects>` `FolderRemap` fixtures (`.Returns(new ScoDictionary<string,string>(...))` at lines ~169, ~250, ~275, ~300, ~331) to `ScoDictionaryNew<string,string>`; add the required `using`.
  - Acceptance: No `ScoDictionary<` construction remains (verified by grep).
- [x] [P7-T12] In `UtilitiesCS.Test\EmailIntelligence\FolderRemapController_Tests2.cs`, migrate the `Mock<IToDoObjects>.SetupGet(x => x.FolderRemap).Returns(new ScoDictionary<string,string>())` fixture (line ~390) to `ScoDictionaryNew<string,string>`; add the required `using`.
  - Acceptance: No `ScoDictionary<` construction remains (verified by grep).
- [x] [P7-T13] In `UtilitiesCS.Test\EmailIntelligence\FilterOlFoldersViewer_Tests.cs`, migrate the `Mock<IToDoObjects>.SetupGet(td => td.FilteredFolderScraping).Returns(new ScoDictionary<string,int>())` fixture (mock at line ~194, `.Returns` at line ~197) to `ScoDictionaryNew<string,int>`; add the required `using`.
  - Acceptance: No `ScoDictionary<` construction remains in this file; the mock fixture uses the new lineage. Verified by grep.

### Phase 8 — PeopleScoDictionary Confirmation and Optional Legacy Deletion

- [x] [P8-T1] Confirm `ToDoModel\Data Model\People\PeopleScoDictionary.cs` is entirely block-commented (inert) and requires no F1 change; record the confirmation in `docs/features/active/2026-07-10-swordfish-dictionary-lineage-306/evidence/other/peoplescodictionary-inert-confirmation.md`.
  - Acceptance: Artifact records `Timestamp:` and the verification that the whole file is commented out (no live `ScoDictionary` reference), including the `public class PeopleScoDictionary : ScoDictionary<string,string>` declaration. No source change is made to `PeopleScoDictionary.cs`.
- [x] [P8-T2] Verify whether the legacy concrete `ScoDictionary<>` class is production-unreferenced after Phases 1-4 by grepping the production tree (excluding `SCODictionary.cs` itself and comment-only hits) for `ScoDictionary<`; record the result in `docs/features/active/2026-07-10-swordfish-dictionary-lineage-306/evidence/other/scodictionary-reference-census.md`.
  - Acceptance: Artifact records `Timestamp:`, the grep command, and the list of remaining live production references (expected: none). This gates whether P8-T3 optional deletion is eligible.
- [x] [P8-T3] SKIPPED: deletion not elected (optional per spec Non-Goals). P8-T2 confirmed zero live production references, so deletion is technically eligible, but the plan's Scope Boundary marks `SCODictionary.cs` deletion OPTIONAL and not an F1 goal. Deletion would require migrating the legacy-class test files (`SCODictionary_Tests.cs`, `SCODictionary_Additional_Tests.cs`, and the three `SmartSerializable*_Tests.cs` sample-type usages) and editing two csproj files — out-of-goal churn with regression risk. Left the class in place. OPTIONAL (execute only if P8-T2 confirms zero live production references AND deletion is elected within budget): delete `UtilitiesCS\ReusableTypeClasses\Serializable\Concurrent\SCO\SCODictionary.cs`, delete its two direct test files (`SCODictionary_Tests.cs`, `SCODictionary_Additional_Tests.cs`), remove the corresponding `<Compile Include>` items from `UtilitiesCS\UtilitiesCS.csproj` and `UtilitiesCS.Test\UtilitiesCS.Test.csproj`, and substitute a non-`ISmartSerializable` sample type in `SmartSerializableStatic_Tests.cs`, `SmartSerializableNonTyped_Tests.cs`, and `SmartSerializableBase_Tests.cs` (which use `ScoDictionary<string,int>` only as a convenient non-`ISmartSerializable` sample). If deletion is not elected, mark this task `[x]` with an explicit `SKIPPED: deletion not elected (optional per spec Non-Goals)` note.
  - Acceptance: EITHER the class, its two direct tests, and all csproj `<Compile Include>` entries are removed AND the three SmartSerializable negative-sample tests compile against a substitute non-`ISmartSerializable` type with no remaining `ScoDictionary` reference in the solution; OR the task is explicitly skipped with the note above. The skip branch is authorized here because spec Non-Goals mark deletion optional.

### Phase 9 — Final QC Toolchain and Coverage Verification

- [x] [P9-T1] Run `dotnet tool run csharpier .` (write mode) then `dotnet tool run csharpier --check .` and write `docs/features/active/2026-07-10-swordfish-dictionary-lineage-306/evidence/qa-gates/final-csharpier.md`.
  - Acceptance: Artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; final `--check` exits 0 (clean). If write mode changed files, restart the loop from this task.
- [x] [P9-T2] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` and write `docs/features/active/2026-07-10-swordfish-dictionary-lineage-306/evidence/qa-gates/final-analyzers.md`.
  - Acceptance: Artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; build exits 0 with no new analyzer errors.
- [x] [P9-T3] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true` and write `docs/features/active/2026-07-10-swordfish-dictionary-lineage-306/evidence/qa-gates/final-nullable.md`.
  - Acceptance: Artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`; build exits 0 with no new nullable warnings.
- [x] [P9-T4] Run `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll TaskMaster.Test\bin\Debug\TaskMaster.Test.dll /EnableCodeCoverage` and write `docs/features/active/2026-07-10-swordfish-dictionary-lineage-306/evidence/qa-gates/final-tests-coverage.md`.
  - Acceptance: Artifact contains `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` with numeric passed/failed/total counts AND numeric post-change coverage headline values (repo-wide line % and coverage % for the scope-lock files). All tests pass. If this step changed files or failed, restart the loop from P9-T1.
- [x] [P9-T5] Verify coverage thresholds by comparing the Phase 0 baseline (`evidence/baseline/baseline-tests-coverage.md`) against the Phase 9 post-change values (`evidence/qa-gates/final-tests-coverage.md`) and write `docs/features/active/2026-07-10-swordfish-dictionary-lineage-306/evidence/qa-gates/coverage-delta.md`.
  - Acceptance: Artifact records baseline coverage, post-change coverage, and new/changed-code coverage; confirms repo-wide line coverage `>= 80%`, new-code coverage `>= 90%`, and no regression on changed lines. If any threshold is not met, the plan outcome is remediation-required (not PASS).

## Traceability — Spec Acceptance Criteria to Tasks

- Re-point all production consumers to `ScoDictionaryNew` → P1-T1..T4, P2-T1..T3, P3-T1..T4, P4-T1.
- `IToDoObjects` contract change compiles across modules (incl. `EmailDetails`/`EmailDetailsWrapper` ripple, `ISubjectMapEncoder.Encoder`, `SortEmail` serialize ripple) → P1-T1..T5, P3-T5, P5-T2.
- Per-persisted-dictionary on-disk round-trip test (DictRemap, FilteredFolderScraping, FolderRemap, SubjectMap Encoder) → P6-T2..T5.
- In-memory-only fields as pure type swap (Decoder, FolderScorer scores) → P3-T4, P4-T1.
- Globals-converter-path prohibition respected → P2/P3 acceptance criteria + P6-T6.
- SubjectMapEncoder construction/persistence reconciliation (self-loading ctor, no-arg `Deserialize()`/`ToDictionary()`, missing-file and duplicate-key paths, `SortEmail` serialize ripple) → P3-T1..T5.
- Affected tests migrated (incl. EmailDetails/Wrapper fixtures) → P7-T1..T13.
- `PeopleScoDictionary.cs` confirmed inert, no change → P8-T1.
- Optional legacy deletion with SmartSerializable negative-sample substitution → P8-T3.
- Full C# toolchain green + coverage thresholds → P9-T1..T5.

## Test Plan

- Unit / compatibility: four persisted-dictionary on-disk round-trip tests (P6-T2..T5) using embedded string payloads and injected read/timer seams (no temp files, no wall-clock waits), plus a globals-path-prohibition assertion (P6-T6).
- Migrated fixtures: FilterOlFoldersController, SubjectMapSco orchestration, OlFolderClassifierGroup, SubjectMapSco, EmailDetails, EmailDetailsWrapper (P7).
- Coverage evidence: baseline `evidence/baseline/baseline-tests-coverage.md`; post-change `evidence/qa-gates/final-tests-coverage.md`; comparison `evidence/qa-gates/coverage-delta.md`.

## Open Questions / Notes

- MCP validator `mcp__drm-copilot__validate_orchestration_artifacts` availability in this checkout is uncertain (historically the tool belongs to the drm-copilot/mix-calculator repo, not TaskMaster). Phase headings intentionally use the canonical em-dash (U+2014) form per the atomic-plan-contract skill and repo convention; if the validator flags only the em-dash headings, that is a known validator limitation and the headings are retained.
- Deferred-write timer: confirm the `SmartSerializable.RequestSerialization` 3-second timer does not change observable startup ordering for the `LoadParallelAsync` tasks in `AppToDoObjects` (design note from research §10; no behavior change expected).

PREFLIGHT: ALL CLEAR
