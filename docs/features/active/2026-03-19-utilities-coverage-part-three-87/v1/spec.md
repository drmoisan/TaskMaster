# 2026-03-19-utilities-coverage-part-three — Spec

- **Issue:** #87
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-03-19T21-49
- **Status:** Draft
- **Version:** 0.1

## Overview

The UtilitiesCS library has 292 classes tracked by coverage tooling, but 196 of them (67%) are below the repository-wide 80% line-coverage floor mandated by general-unit-test.instructions.md. Many files sit at 0% coverage — including helpers, extension methods, threading utilities, serialization infrastructure, email intelligence modules, and Newtonsoft JSON converters. This gap means regressions in core shared code go undetected and the library cannot pass the repo-wide >=80% coverage gate.

Previous feature work (issue #82, utilities-coverage-part-two) raised OutlookObjects/Folder to >=80%. This third part extends coverage to **every remaining file and subfolder** in UtilitiesCS.


## Behavior

Add or extend MSTest unit tests in UtilitiesCS.Test so that every production .cs file compiled by UtilitiesCS.csproj reaches at least 80% line coverage. Tests must follow the repo's general and C#-specific unit test policies (MSTest + Moq + FluentAssertions, Arrange-Act-Assert, deterministic, no external dependencies, no temp files).

Coverage categories requiring uplift include:
- Extensions (StringExtensions, ArrayExtensions, IEnumerableExtensions, ImageExtensions, AsyncSerialization, WinFormsExtensions, DfDeedle, DfMLNet, DrawingExtensions, etc.)
- HelperClasses (PrettyPrint, Tokenizer, Initializer, DeepCompare, DispatchUtility, FilePathHelper, FileInfoWrapper, DirectoryInfoWrapper, ThemeHelpers, Logging, etc.)
- ReusableTypeClasses (LockingLinkedList, SerializableList, AsyncLazy, LazyTry, Matrices, TimedActions, SmartSerializable, SCO collections, Observable collections, etc.)
- Threading (TimeOutTask, ThreadSafeFunctions, ProgressTracker, AsyncMultiTasker, IdleActionQueue, UiThread, etc.)
- NewtonsoftHelpers (converters, binders, wrappers, SDIL reader, MonoExtension)
- EmailIntelligence (Bayesian, Ctf, Flags, SubjectMap, EmailParsingSorting, ClassifierGroups, OlFolderTools, People, Recents, etc.)
- OutlookObjects (Item, MailItem, Store, Table, Recipient, Attachment, Calendar, Category, Conversation, etc.)
- Dialogs (ActionButton, DelegateButton, MyBox, InputBox, YesNoToAll, FolderNotFound, etc.)
- OneDriveHelpers, Interfaces with implementation, WindowsAPI


## Inputs / Outputs

- **Inputs:**
  - `coverage/coverage.cobertura.xml` — Cobertura XML from the most recent `Invoke-MSTestWithCoverage.ps1` run; used to identify files below the 80% line-rate gate and to measure uplift after each phase.
  - `UtilitiesCS/UtilitiesCS.csproj` — explicit `<Compile Include>` entries define the canonical set of production files that must be covered. Files not in the csproj (e.g., the orphaned `OutlookObjects/MailResolution.cs`) are excluded from scope.
  - `UtilitiesCS.Test/UtilitiesCS.Test.csproj` — explicit `<Compile Include>` entries; every new test `.cs` file must be registered here or it will silently not compile.
  - Existing test files in `UtilitiesCS.Test/` — ~120+ files providing established mocking patterns, namespace conventions, and AAA scaffolding to extend.

- **Outputs:**
  - New and updated MSTest `.cs` files under `UtilitiesCS.Test/` (one test class per production file, namespace mirroring subfolder).
  - Updated `UtilitiesCS.Test.csproj` with `<Compile Include>` entries for every new test file.
  - Updated `coverage/coverage.cobertura.xml` after the final test-with-coverage run, showing all production files at ≥80% line-rate.
  - TRX test-result logs under `TestResults/` from `vstest.console.exe` runs.

- Config keys and defaults: None — no runtime configuration is introduced.
- Versioning or backward-compatibility constraints: No public API changes; test-only additions.

## API / CLI Surface

List commands, flags, request/response shapes, and examples.
- Example invocations with expected outputs (concise):
- Contracts and validation rules:

## Data & State

Data flow, storage, or state changes introduced by this feature.
- Data transformations and invariants:
- Caching or persistence details:
- Migration or backfill requirements (if any):

## Constraints & Risks

- Many classes have deep Outlook COM interop dependencies requiring extensive Moq seams
- WinForms UI classes (Designer.cs, viewers, dialogs) may require special treatment for testability
- Some files may be dead code or commented stubs (e.g., ObservableDictionary.cs, ConcurrentObservableBag.cs in UtilitiesCS are stubs; live implementations are in UtilitiesSwordfish)
- Serialization classes have complex generic type constraints
- EmailIntelligence modules depend on domain-specific types and may require significant mock scaffolding
- The large number of files (196 below 80%) means this work should be phased carefully


## Implementation Strategy

### Phased Approach (Easy → Medium → Hard → Skip Evaluation)

**Phase 1 — Quick Wins (~45 files, Easy difficulty)**
1. *Close-to-80% files* (small delta): ArrayExtensions (77.7%), IEnumerableExtensions (70.6%), ConcurrentObservableDictionary (77.4%), AbstractCloneable (77.8%), TreeNodeOfT (76.8%), StackGeek (72.2%), StackObjectCS (72%), WrapperScDictionary (70.7%), WrapperScoDictionary (76%), MyFileSystemInfo (71%), FilePathHelperConverter (72%), EmailTokenizer (74.8%).
2. *Pure-logic helpers*: PrettyPrint, DeepCompare, Initializer, DebugTextWriter, TraceUtility, SmithWaterman, StringManipulation.
3. *Data classes at 0%*: FilterEntry, BayesianMetricTypes, EmailFilerConfig, NConsoleTraceWriter, PropertyStore.
4. *Simple extensions*: IAsyncEnumerableExtensions, AsyncSerialization.
5. *Data structures*: LockingLinkedList/Node, TimedQueueOfActions, ThreadSafeFunctions, TimeOutTask, AsyncLazy, SimpleActionBagObserver/SimpleActionLockingLinkedListObserver.
6. *EmailIntelligence data*: CtfIncidenceList, CtfMap, SubjectMapEntry, MovedMailInfo, ImageStripper, DedicatedToken.

**Phase 2 — Medium Difficulty (~55 files)**
1. *Newtonsoft converters*: ScDictionaryConverter, NonRecursiveConverter, MonoExtension, PeopleScoConverter, PeopleScoRemainingObjectConverter, WrapperPeopleScoDictionaryNew, DerivedCompositionConverter_ConcurrentDictionary.
2. *Serializable collections* (SCO family): ScBag, ScoCollection, SCODictionary, ScoSortedDictionary, ScoStack, SerializableList, ScoDictionaryNew, SloLinkedList, ScDictionary.
3. *SmartSerializable framework*: SmartSerializable, SmartSerializableBase, SmartSerializableLoader, SmartSerializableStatic, NewSmartSerializableConfig.
4. *Bayesian core*: BayesianClassifierShared, BayesianClassifierGroup, Corpus, BayesianClassifierExtensions.
5. *OutlookObjects (mocked COM)*: Extend coverage for AttachmentHelper, AttachmentSerializable, CreateCategory, StoreWrapper, OutlookItem*, RecipientStatic, UserDefinedFields.
6. *Threading*: ProgressTracker, ProgressTrackerAsync, AsyncMultiTasker, ThreadMonitor.
7. *EmailIntelligence domain*: FlagTranslator, IntelligenceConfig, PeopleScoDictionaryNew, SubjectMapEncoder, SubjectMapSco, RecentsList.

**Phase 3 — Hard Files (~55 files)**
1. *Outlook COM-heavy*: ConversationHelper, MailItemHelper, StoreWrapperController, OlTableExtensions, OlToDoTable.
2. *ClassifierGroups* (all 0%, depend on IApplicationGlobals + COM): may need facade extraction.
3. *WinForms dialogs*: InputBox, MyBox, YesNoToAll — extract testable logic from code-behind.
4. *WinForms helpers*: ControlPosition, ControlResizer, ImageHelper, MouseDownFilter, Theme, ThemeControlGroup, TipsController, OlvExtension, ScreenHelper, TableLayoutHelper.
5. *WinForms viewers*: FilterOlFoldersViewer, FolderInfoViewer, OSBrowser, FolderRemapViewer, ConfigViewer, ProgressViewer, ProgressPane, SubjectMapMetrics.
6. *Other hard*: DispatchUtility (COM dispatch), ComStreamWrapper (WIP), OneDriveDownloader (Graph API), ShellUtilities (Shell32), IdleActionQueue/IdleAsyncQueue.

**Phase 4 — Evaluate Skips**
Review whether the following should be excluded from the coverage gate, given minimal smoke tests, or removed from the project:
- ~16 Designer.cs auto-generated files (provide no testable logic; coverage via parent form instantiation only).
- ~4 commented-out stubs with zero executable lines (ObservableDictionary.cs, ConcurrentObservableBag.cs, StackObjectVB.cs, FlattenArray.cs).
- ~3 "To Depricate" files (CSVDictUtilities, FileIO2, StringManipulation) — candidates for removal rather than testing.
- ~40+ pure-interface files with no executable code.

### Seam Patterns for COM / WinForms Mocking

- **COM interop (Outlook):** Use `Moq` to mock `Microsoft.Office.Interop.Outlook` interfaces (e.g., `Mock<Outlook.MailItem>`, `Mock<Outlook.MAPIFolder>`). Follow existing patterns in `OutlookItemTests`, `FolderWrapperStateTests`.
- **WinForms UI:** For Forms/UserControls, extract testable logic into non-UI helper classes. Where extraction is impractical, create control instances under `[STAThread]` context. Avoid cross-thread access by testing on the creating thread.
- **File-system serialization:** Replace actual file I/O with `MemoryStream`/`StringWriter` injection; never create temp files per repo policy.
- **IApplicationGlobals dependency:** Mock via `Moq` interface mock to isolate EmailIntelligence classifier groups from the full application context.

### Explicit csproj Registration Requirement

Every new test `.cs` file **must** be added as a `<Compile Include="...">` entry in `UtilitiesCS.Test.csproj`. This is a non-negotiable requirement due to the old-style project format — files not registered silently fail to compile.

- Dependency changes (new/removed packages) and rationale: None expected. All required test packages (MSTest, Moq, FluentAssertions) are already present.
- Logging/telemetry additions: None.
- Rollout plan: Incremental — each phase is merged independently after passing the full C# toolchain loop.

## Definition of Done

- [ ] Every `.cs` file compiled by `UtilitiesCS.csproj` reaches ≥80% line coverage as reported by Cobertura XML, or is explicitly documented as a skip candidate (Designer.cs, commented stub, pure interface)
- [ ] No pre-existing tests are broken or removed
- [ ] All new tests follow MSTest + Moq + FluentAssertions conventions (AAA pattern, deterministic, isolated, no external dependencies, no temp files)
- [ ] All new test files are registered in `UtilitiesCS.Test.csproj` via `<Compile Include>`
- [ ] Repository-wide line coverage does not regress below the pre-work baseline
- [ ] C# toolchain loop passes clean in a single pass: `dotnet format` → analyzer build (`/p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`) → nullable build (`/p:Nullable=enable /p:TreatWarningsAsErrors=true`) → `vstest.console.exe` test run with `/EnableCodeCoverage`
- [ ] Coverage report reviewed: skip candidates (Designer.cs, stubs) documented with rationale
- [ ] Docs updated (feature folder status, change-plan.md if applicable)

## Seeded Test Conditions (from potential)
- [ ] Unit coverage for each of the 196 files currently below 80%
- [ ] Positive and negative flows for extension methods
- [ ] Edge cases and boundary conditions for collection/threading utilities
- [ ] Error-handling paths in serialization helpers
- [ ] Mocked COM interop for Outlook-dependent classes
- [ ] Thread-safety verification for concurrent collections and threading utilities
