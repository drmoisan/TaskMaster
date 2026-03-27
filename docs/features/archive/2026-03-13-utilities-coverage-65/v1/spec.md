# 2026-03-13-utilities-coverage — Spec

- **Issue:** #65
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-03-13T22-21
- **Status:** Draft
- **Version:** 0.1

## Overview

The `UtilitiesCS` library is a shared dependency used across the TaskMaster solution for extension methods, helper classes, threading primitives, serialization helpers, Outlook-adjacent logic, and reusable collection/data-structure code. Repository research shows 359 production `.cs` files in `UtilitiesCS/`, 74 existing test files in `UtilitiesCS.Test/`, and a baseline package line-rate of roughly 14.5%, which leaves many commonly reused APIs effectively unguarded.

This feature adds or expands MSTest coverage in `UtilitiesCS.Test/` for the testable portions of `UtilitiesCS`, with the goal of driving each non-excluded production file to at least 80% line coverage while keeping tests deterministic, isolated, and consistent with repository tooling and conventions.


## Behavior

Add or expand unit tests in `UtilitiesCS.Test/` so the test project mirrors the production directory structure and documents the observable contract of each targeted class. The main path is: identify a testable production file, place its tests in the matching `UtilitiesCS.Test/<area>/` folder, add coverage for happy-path, invalid-input, boundary, and error-handling behavior, then validate with the repository C# toolchain and coverage report.

Alternative paths are handled explicitly rather than silently skipped. Pure logic files are tested directly; classes with interface-based dependencies are isolated with Moq; files that are designer-generated, interface-only, UI-heavy, deprecated, obsolete, or dependent on live Outlook COM/WinForms runtime are excluded from the 80% target and must be listed as exclusions rather than treated as hidden misses. The feature does not introduce any production API changes; it strengthens confidence in existing APIs by making their current behavior executable and reviewable.


## Inputs / Outputs

**Inputs:**
- Production source files under `UtilitiesCS/`, especially these high-value areas:
   - `Extensions/`
   - `HelperClasses/`
   - `Threading/`
   - `ReusableTypeClasses/`
   - `NewtonsoftHelpers/`
   - testable logic in `EmailIntelligence/`, `OutlookObjects/`, and `Dialogs/`
- Existing test project layout and project file wiring in `UtilitiesCS.Test/` and `UtilitiesCS.Test/UtilitiesCS.Test.csproj`
- Baseline coverage data from `coverage/coverage.cobertura.xml` (package line-rate ≈ 0.145, branch-rate ≈ 0.131)
- Repository policy/instruction set governing tests and C# validation
- Existing test conventions already present in the repo: MSTest attributes, Moq for interfaces, FluentAssertions for expressive assertions

**Outputs:**
- New or expanded test files under `UtilitiesCS.Test/`, placed in folders matching the production area they cover
- Updated explicit `<Compile Include=...>` entries in `UtilitiesCS.Test/UtilitiesCS.Test.csproj` for each newly created test file
- Updated coverage and test execution artifacts, including:
   - `coverage/coverage.cobertura.xml`
   - `.trx` results under `TestResults/`
   - baseline and QA evidence under `evidence/baseline/` and `evidence/qa-gates/`
- A documented list of excluded production files/categories when 80% per-file coverage is not applicable

**Constraints:**
- Each test file must be ≤500 lines; larger surfaces must be split by topic while keeping naming and directory placement predictable
- Target framework: .NET Framework 4.8.1
- No new package dependencies required; MSTest, Moq, FluentAssertions already present in `UtilitiesCS.Test.csproj`
- Tests must avoid file I/O, network access, live Outlook COM objects, and live UI runtime dependencies unless explicitly excluded from scope

## API / CLI Surface

No new public runtime API is introduced. The deliverable is test code, test project wiring, and supporting evidence. The effective validation surface is the repository C# toolchain, run in strict order:

1. **Format:** `dotnet format TaskMaster.sln --verify-no-changes --no-restore`
   - Verifies code style compliance; fails if formatting changes are needed.
2. **MSBuild Analyzers:** `msbuild TaskMaster.sln /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
   - Runs .NET analyzers and enforces code style rules at build time.
3. **MSBuild Nullable:** `msbuild TaskMaster.sln /p:Nullable=enable /p:TreatWarningsAsErrors=true`
   - Enables nullable reference type analysis and treats all warnings as errors.
4. **Test Execution:** `vstest.console.exe <discovered *.Test.dll assemblies> /EnableCodeCoverage /InIsolation /Logger:trx`
   - Runs all test assemblies with code coverage collection; emits `.trx` results.

**Contracts and validation rules:**
- All validation steps must pass without errors in a single pass before the work is considered ready.
- If any step fails or produces auto-fixes, the entire toolchain restarts from step 1.
- Per-file line coverage is verified against `coverage.cobertura.xml` for all non-excluded `UtilitiesCS` source files.
- Test placement is part of the contract: each new test file must live under the matching `UtilitiesCS.Test/` subtree and be included in `UtilitiesCS.Test.csproj`.

## Data & State

This feature introduces no production data model or persistence changes. It is a test-only initiative, but it does rely on repository-managed coverage and test result artifacts to measure success.

- **Data sources:** Production C# source files in `UtilitiesCS/`, existing tests in `UtilitiesCS.Test/`, and generated coverage/test output from the repo validation commands.
- **Transformations:** Coverage tooling maps executed test lines to source files in `coverage/coverage.cobertura.xml`; review artifacts summarize package-level and per-file line-rate changes.
- **Persistence:** No application data is persisted. Only test/coverage outputs and feature evidence markdown files are written.
- **Caching assumptions:** None. Tests are expected to be deterministic and not depend on prior test runs or mutable local state.
- **Migration or backfill requirements:** None.

## Constraints & Risks

- **Coverage target size:** Research identified roughly 204 testable production files out of 359 total in `UtilitiesCS/`; reaching ≥80% per file requires disciplined scoping, batching, and evidence capture rather than a single opportunistic sweep.
- **COM interop and Outlook-adjacent code:** Many classes under `OutlookObjects/` and `EmailIntelligence/` expose only limited logic that can be exercised without a live Outlook runtime. These areas must either be mocked behind existing interfaces or explicitly excluded when they are runtime-bound.
- **UI and WinForms dependencies:** Files requiring live form creation, UI thread interaction, designer-generated code, or rendering behavior are poor unit-test candidates and should be excluded from the target metric.
- **Commented-out or effectively dead code:** Some files exist in source but are not live buildable implementations; these must be called out explicitly so coverage reporting does not pretend they were fully validated.
- **.NET Framework 4.8.1 constraints:** The target framework limits available modern testing patterns and can surface binding/version quirks in async or serialization helpers.
- **Operational risk:** Because the test project uses explicit `<Compile Include>` entries, missing a project-file update can make a new test file invisible to the build even when the file exists on disk.
- **Review risk:** Package-level coverage can improve materially while still failing the per-file 80% gate, so success must be judged on file-level evidence, not just the overall percentage.


## Implementation Strategy

**Implementation scope:** Add ~125 new or expanded MSTest test files under `UtilitiesCS.Test/`, organized in three phases by testability and ROI. No production code changes. No new dependencies.

### Phase 1 — Pure Logic, High-Value (P1): ~70 test files, ~500 test methods

Focus on files with zero external dependencies (pure functions, extension methods, data structures):

| Area | Files | Examples |
|------|-------|---------|
| Extensions/ | 15 files | ArrayExtensions, StringExtensions, DictionaryExtensions, IEnumerableExtensions, IListExtensions, EnumExtensions, NullExtensions, ExceptionExtensions, QueueExtensions, LazyExtension, ExtToChar, TraceExtensions, JsonExtensions, JsonSerializerExtensions, CompilerServicesExtensions |
| HelperClasses/ pure logic | 10 files | GenericBitwise, MergeSortImplementations, ParamArray, ObjectSize, DeepCompare, SegmentStopWatch, ReflectionHelper, PrettyPrint (expand), SimpleRegex (expand), Tokenizer (expand) |
| ReusableTypeClasses/ data structures | 12 files | AsyncLazy, LazyTry, Matrix, JaggedMatrix, DenMatrix, DataConverter2d, StackGeek, StackObjectCS, TreeNodeOfT, SerializableList, ScBag, ScoCollection (expand), SCODictionary, ScoStack |
| Dialogs/ logic | 2 files | DelegateButton, YesNoToAll (expand) |
| EmailIntelligence/ pure logic | 10+ files | Prediction, DoNotSerializeContractResolver, CtfIncidence/CtfIncidenceList/CtfMap (expand), FlagParser (expand), FlagClassNoItem, FlagDetails, CommonWords (expand), SubjectMapEntry, EmailTokenizer (expand), ImageStripper, MinedMailInfo (expand), MovedMailInfo |
| OutlookObjects/ comparers & POCOs | 8 files | FolderWrapperNameComparer, FolderWrapperNameAndParentNameComparer, FolderWrapperNameCountSizeComparer, FolderWrapperNodeComparer, FolderWrapperNodeContentsComparer, ItemComparer, AttachmentSerializable, OlItemSummary (expand), ItemInfo |
| NewtonsoftHelpers/ binders | 2 files | AllInclusiveBinder, KnownTypesBinder |
| Threading/ pure logic | 3 files | ThreadSafeSingleShotGuard, ThreadSafeFunctions, TaskPriority, ProgressPackage |

### Phase 2 — Medium Complexity (P2): ~40 test files, ~300 test methods

Files requiring Moq mocking of interfaces (`IApplicationGlobals`, `IFileSystemFolderPaths`, `JsonReader`, COM interfaces from `Interfaces/IOutlookObjects/`):

| Area | Files | Examples |
|------|-------|---------|
| Extensions/ async/stream | 3 files | AsyncSerialization, StreamExtensions, IAsyncEnumerableExtensions |
| HelperClasses/ with deps | 8 files | Initializer, ObjectCopier, FilePathHelper, DirectoryInfoWrapper, FileInfoWrapper, FileSystemInfoWrapper, logging classes |
| ReusableTypeClasses/ observable/concurrent | 12 files | ObservableDictionary, ConcurrentObservableBag, LockingLinkedList, TimerWrapper, TimedBatchAction, TimedQueueOfActions, TimedAsyncTask |
| Threading/ with deps | 8 files | TimeOutTask, ProgressTracker, ProgressTrackerAsync, ApplicationIdleTimer, AsyncMultiTasker, IdleActionQueue, IdleAsyncQueue, AsyncIdleQueue1 |
| NewtonsoftHelpers/ converters | 5 files | NonRecursiveConverter, PeopleScoConverter, PeopleScoRemainingObjectConverter, NConsoleTraceWriter, MonoExtension |
| EmailIntelligence/ medium | 10 files | Corpus, BayesianClassifierGroup (expand), SmithWaterman, SubjectMapEncoder, FolderConverter, EmailFilerConfig, FlagConsolidator, FlagTranslator |
| OutlookObjects/ mocked | 5 files | RecipientInfo, EmailDetails (expand), FolderScorer, MAPIFields, StoresWrapper (expand) |

### Phase 3 — Hard / Low-Value (P3): ~15 test files, ~80 test methods

Files with heavy external dependencies; limited ROI unless refactored:

- EmailIntelligence Outlook-dependent classifiers — test extractable logic only
- OutlookObjects COM wrappers — test non-COM logic paths only
- ReusableTypeClasses SmartSerializable — test serialization logic via mocks
- NewtonsoftHelpers complex wrappers (WrapperPeopleScoDictionaryNew)

### Test Approach by Category

| Category | Strategy |
|----------|----------|
| Extension methods | Test each method: null input, empty, single, typical, boundary. Separate test file per extension class. |
| Data structures | CRUD operations, enumeration, boundary (empty, single, large), concurrency for concurrent types. |
| Comparers | Equals/GetHashCode: same, different, null, edge cases. |
| Newtonsoft converters | Round-trip: serialize → deserialize → verify equality. Malformed JSON. |
| Bayesian classifiers | Test double subclasses (existing pattern: BayesianClassifierSub, CorpusSub). Train → classify cycles. |
| Flags/parsers | Known input → expected output for each parsing path. |
| Threading | ManualResetEvent/TaskCompletionSource for deterministic synchronization. |
| COM-dependent code | Mock via interfaces in `Interfaces/IOutlookObjects/`; test logic only. |

### Skipped Files (~155 production files)

| Category | Count | Justification |
|----------|-------|---------------|
| Interfaces/ | 63 | Pure interface definitions; no logic |
| Designer-generated | ~20 | Auto-generated WinForms code |
| Deprecated (To Depricate/) | 5 | Marked for deprecation |
| Obsolete (Bayesian/Obsolete/) | 6 | Superseded implementations |
| UI-heavy (Viewers, Forms, Controls) | ~35 | Require WinForms runtime |
| COM-heavy (OutlookItem*, FolderWrapper) | ~20 | Deep COM interop; require live Outlook |
| WindowsAPI, Examples, SDIL Reader | 5 | No testable logic |

**Dependency changes:** None. MSTest, Moq, FluentAssertions already present.

**Logging/telemetry:** None added. Tests use `Console.SetOut(new DebugTextWriter())` per existing convention.

**Rollout:** No feature flags or staged deployment. Changes are test-only and merged per phase via standard PR process.

## Definition of Done

- [ ] Acceptance criteria are documented in this spec/user story and trace to named test areas in `UtilitiesCS.Test/`
- [ ] New or expanded MSTest files exist for each in-scope production area, and each new file is included in `UtilitiesCS.Test/UtilitiesCS.Test.csproj`
- [ ] Tests cover positive paths, invalid input handling, boundary cases, and error/concurrency behavior where relevant
- [ ] Excluded files/categories are listed explicitly rather than implied by missing tests
- [ ] Coverage evidence is generated from `coverage/coverage.cobertura.xml` and shows each non-excluded target file at or above 80% line coverage
- [ ] Repository evidence artifacts are updated under `evidence/baseline/` and `evidence/qa-gates/` for the validation run
- [ ] Full C# toolchain passes in order: format → analyzer build → nullable build → coverage-enabled test run
- [ ] Review/audit docs in `docs/features/active/2026-03-13-utilities-coverage-65/` reflect whether the work fully meets, partially meets, or misses the coverage objective

## Seeded Test Conditions (from potential)
- [ ] Extension methods: null inputs, empty collections, single-element, large collections, type mismatches
- [ ] Helper classes: boundary values, invalid arguments, concurrent access patterns
- [ ] Threading utilities: thread-safety verification, timeout behavior, cancellation
- [ ] Serialization converters: round-trip serialize/deserialize, malformed JSON, missing properties, type discrimination
- [ ] Reusable collections: add/remove/clear/enumerate, concurrent modification, serialization persistence
- [ ] Bayesian classifiers: training with empty corpus, single token, edge probability values
