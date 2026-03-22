# utilities-coverage-part-three (Potential)

- Date captured: 2026-03-19
- Author: Dan Moisan
- Status: Draft

## Problem / Why

The UtilitiesCS library has 292 classes tracked by coverage tooling, but 196 of them (67%) are below the repository-wide 80% line-coverage floor mandated by general-unit-test.instructions.md. Many files sit at 0% coverage — including helpers, extension methods, threading utilities, serialization infrastructure, email intelligence modules, and Newtonsoft JSON converters. This gap means regressions in core shared code go undetected and the library cannot pass the repo-wide >=80% coverage gate.

Previous feature work (issue #82, utilities-coverage-part-two) raised OutlookObjects/Folder to >=80%. This third part extends coverage to **every remaining file and subfolder** in UtilitiesCS.

## Proposed Behavior

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

## Acceptance Criteria (early draft)

- [ ] Every .cs file compiled by UtilitiesCS.csproj has >=80% line coverage as reported by Cobertura
- [ ] No pre-existing tests are broken or removed
- [ ] All new tests follow MSTest + Moq + FluentAssertions conventions
- [ ] All new tests are deterministic, isolated, and use no external dependencies
- [ ] All new test files are registered in UtilitiesCS.Test.csproj (explicit Compile Include)
- [ ] The C# toolchain loop passes clean: format, analyzer build, nullable build, test run
- [ ] Repository-wide line coverage does not regress below the pre-work baseline

## Constraints & Risks

- Many classes have deep Outlook COM interop dependencies requiring extensive Moq seams
- WinForms UI classes (Designer.cs, viewers, dialogs) may require special treatment for testability
- Some files may be dead code or commented stubs (e.g., ObservableDictionary.cs, ConcurrentObservableBag.cs in UtilitiesCS are stubs; live implementations are in UtilitiesSwordfish)
- Serialization classes have complex generic type constraints
- EmailIntelligence modules depend on domain-specific types and may require significant mock scaffolding
- The large number of files (196 below 80%) means this work should be phased carefully

## Test Conditions to Consider

- [ ] Unit coverage for each of the 196 files currently below 80%
- [ ] Positive and negative flows for extension methods
- [ ] Edge cases and boundary conditions for collection/threading utilities
- [ ] Error-handling paths in serialization helpers
- [ ] Mocked COM interop for Outlook-dependent classes
- [ ] Thread-safety verification for concurrent collections and threading utilities

## Next Step

- [ ] Promote to GitHub issue (feature request template)
- [ ] Create `docs/features/active/utilities-coverage-part-three/` folder from the template

