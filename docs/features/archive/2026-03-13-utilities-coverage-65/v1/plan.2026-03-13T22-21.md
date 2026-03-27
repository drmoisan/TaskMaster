# 2026-03-13-utilities-coverage — Plan

- **Issue:** #65
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-03-13T23-30
- **Status:** Draft
- **Version:** 1.1
- **Work Mode:** full-feature
- **Budget:** large path (125+ test files, 0 production files)

## Overview

Increase UtilitiesCS unit test coverage from ~13% (14.5% line-rate, 13.1% branch-rate) to ≥80% per testable file by adding ~125 new or expanded MSTest test files in UtilitiesCS.Test, organized in 8 phases by testability and ROI. No production code changes. Framework: MSTest + Moq + FluentAssertions on .NET Framework 4.8.1.

## Required References

- [`.github/copilot-instructions.md`](../../../../.github/copilot-instructions.md)
- [`.github/instructions/general-code-change.instructions.md`](../../../../.github/instructions/general-code-change.instructions.md)
- [`.github/instructions/general-unit-test.instructions.md`](../../../../.github/instructions/general-unit-test.instructions.md)
- [`.github/instructions/csharp-code-change.instructions.md`](../../../../.github/instructions/csharp-code-change.instructions.md)
- [`.github/instructions/csharp-unit-test.instructions.md`](../../../../.github/instructions/csharp-unit-test.instructions.md)

**All work must comply with these policies; do not duplicate their content here.**

## Requirements Sources

- **Binding execution inputs:** issue.md, spec.md, user-story.md
- **Informational input:** research.md
- **Conflict rule:** If issue.md, spec.md, and user-story.md disagree, stop for plan revision before execution.

## Toolchain Commands (C#)

| Step | Command |
|------|---------|
| Format | `dotnet format TaskMaster.sln --verify-no-changes --no-restore` |
| Analyzers | `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` |
| Nullable | `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true` |
| Test | `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTest.ps1 -SearchRoot . -Configuration Debug` |
| Test+Coverage | `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug` |

## Implementation Acceptance Standard (Phases 1–7)

Every implementation task shares a **single binary acceptance gate**:

1. Test file exists at the stated path (≤500 lines per file; split with topic suffixes if exceeded)
2. Solution builds: `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU"`
3. All tests in the named `*_Tests` class pass with 0 failures

These three conditions constitute one verifiable outcome: **the test file is valid (buildable and passing)**. Each task below states the test class name, file path, and scenario scope; the acceptance standard above applies to all.

## Coverage Exclusion List

The following UtilitiesCS source files are **excluded** from the ≥80% per-file line-rate target. Every non-excluded file must meet ≥80%.

| Category | Pattern / Files | Justification |
|----------|----------------|---------------|
| Interfaces | All files under `Interfaces/` | Pure interface definitions; no executable logic |
| Designer-generated | All `*.Designer.cs` files | Auto-generated WinForms code |
| Deprecated | All files under `To Depricate/` | Marked for deprecation |
| Obsolete | All files under `Bayesian/Obsolete/` | Superseded implementations |
| UI-heavy | Files under `Viewers/`, `Controls/`; `Form`/`Dialog` classes requiring WinForms runtime | Require live WinForms runtime |
| COM-heavy | `FolderWrapper.cs`, `FolderWrapperData.cs`, `OutlookItem*.cs`, `MailItemHelper.cs`, `OutlookCOM*.cs`, and files whose primary purpose is Outlook COM interop | Require live Outlook COM runtime |
| Misc untestable | `WindowsAPI.cs`, files under `Examples/`, `SDILReader/` | No testable logic |

## Implementation Plan (Atomic Tasks)

---

### Phase 0 — Baseline Capture

- [x] [P0-T1] Read all repo policy files in compliance order: (1) `.github/copilot-instructions.md`, (2) `general-code-change.instructions.md`, (3) `general-unit-test.instructions.md`, (4) `csharp-code-change.instructions.md`, (5) `csharp-unit-test.instructions.md`
  - Acceptance: Evidence artifact `evidence/baseline/phase0-instructions-read.md` exists with `Timestamp:`, `Policy Order:`, and explicit list of 5 files read

- [x] [P0-T2] Capture baseline format state by running `dotnet format TaskMaster.sln --verify-no-changes --no-restore`
  - Acceptance: Evidence artifact `evidence/baseline/baseline-format.{ISO-8601 yyyy-MM-ddTHH-mm}.md` exists with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`

- [x] [P0-T3] Capture baseline MSBuild analyzer state by running `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
  - Acceptance: Evidence artifact `evidence/baseline/baseline-analyzers.{ISO-8601 yyyy-MM-ddTHH-mm}.md` exists with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`

- [x] [P0-T4] Capture baseline MSBuild nullable state by running `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
  - Acceptance: Evidence artifact `evidence/baseline/baseline-nullable.{ISO-8601 yyyy-MM-ddTHH-mm}.md` exists with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:`

- [x] [P0-T5] Capture baseline test and coverage state by running `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug`
  - Acceptance: Evidence artifact `evidence/baseline/baseline-test-coverage.{ISO-8601 yyyy-MM-ddTHH-mm}.md` exists with `Timestamp:`, `Command:`, `EXIT_CODE:`, `Output Summary:` including numeric total pass/fail counts and numeric UtilitiesCS package line-rate baseline percentage (14.5%)

- [x] [P0-T6] Record per-file coverage baseline for all UtilitiesCS source files from `coverage/coverage.cobertura.xml`
  - Acceptance: Evidence artifact `evidence/baseline/baseline-utilitiescs-per-file-coverage.{ISO-8601 yyyy-MM-ddTHH-mm}.md` exists listing each UtilitiesCS source file with its current line-rate percentage

### Phase 1 — P1 Pure Logic Extension Tests

All test files placed in `UtilitiesCS.Test/Extensions/`. Each file uses `[TestClass]`, `[TestMethod]`, FluentAssertions `.Should()`, Arrange–Act–Assert pattern. Each file ≤500 lines. If a file would exceed 500 lines, split into multiple files with topic suffixes.

- [x] [P1-T1] Create `ArrayExtensions_Tests.cs` covering the public API surface of `ArrayExtensions`: null input, empty array, single-element, typical, boundary, type variations
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/Extensions/ArrayExtensions_Tests.cs` | class: `ArrayExtensions_Tests`

- [x] [P1-T2] Create `StringExtensions_Tests.cs` covering the public API surface of `StringExtensions`: null/empty string, whitespace, single char, typical, unicode, boundary lengths
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/Extensions/StringExtensions_Tests.cs` | class: `StringExtensions_Tests`

- [x] [P1-T3] Expand `DictionaryExtensions_Tests.cs` covering the public API surface of `DictionaryExtensions`: null dict, empty dict, single entry, duplicate keys, missing keys
  - Preconditions: Existing test file may exist — expand or replace as needed
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/Extensions/DictionaryExtensions_Tests.cs` | class: `DictionaryExtensions_Tests`

- [x] [P1-T4] Create `IEnumerableExtensions_Tests.cs` covering the public API surface of `IEnumerableExtensions`: null input, empty sequence, single-element, large sequence, deferred execution
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/Extensions/IEnumerableExtensions_Tests.cs` | class: `IEnumerableExtensions_Tests`

- [x] [P1-T5] Create `IListExtensions_Tests.cs` covering the public API surface of `IListExtensions`: null list, empty list, single item, boundary indices, type variations
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/Extensions/IListExtensions_Tests.cs` | class: `IListExtensions_Tests`

- [x] [P1-T6] Create `EnumExtensions_Tests.cs` covering the public API surface of `EnumExtensions`: valid enum values, invalid cast, flags, default values
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/Extensions/EnumExtensions_Tests.cs` | class: `EnumExtensions_Tests`

- [x] [P1-T7] Create `NullExtensions_Tests.cs` covering the public API surface of `NullExtensions`: null reference, non-null reference, value types, nullable value types
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/Extensions/NullExtensions_Tests.cs` | class: `NullExtensions_Tests`

- [x] [P1-T8] Create `ExceptionExtensions_Tests.cs` covering the public API surface of `ExceptionExtensions`: null exception, simple exception, nested inner exceptions, AggregateException
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/Extensions/ExceptionExtensions_Tests.cs` | class: `ExceptionExtensions_Tests`

- [x] [P1-T9] Create `QueueExtensions_Tests.cs` covering the public API surface of `QueueExtensions`: null queue, empty queue, single item, multiple items, boundary
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/Extensions/QueueExtensions_Tests.cs` | class: `QueueExtensions_Tests`

- [x] [P1-T10] Create `LazyExtension_Tests.cs` covering the public API surface of `LazyExtension`: uninitialized Lazy, initialized Lazy, null factory, value type, reference type
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/Extensions/LazyExtension_Tests.cs` | class: `LazyExtension_Tests`

- [x] [P1-T11] Create `ExtToChar_Tests.cs` covering the public API surface of `ExtToChar`: valid conversions, boundary chars, invalid inputs, unicode
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/Extensions/ExtToChar_Tests.cs` | class: `ExtToChar_Tests`

- [x] [P1-T12] Create `TraceExtensions_Tests.cs` covering the public API surface of `TraceExtensions`: typical trace output, null arguments, empty strings
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/Extensions/TraceExtensions_Tests.cs` | class: `TraceExtensions_Tests`

- [x] [P1-T13] Create `CompilerServicesExtensions_Tests.cs` covering the public API surface of `CompilerServicesExtensions`: caller member name, file path, line number scenarios
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/Extensions/CompilerServicesExtensions_Tests.cs` | class: `CompilerServicesExtensions_Tests`

- [x] [P1-T14] Create `JsonExtensions_Tests.cs` covering the public API surface of `JsonExtensions`: valid JSON, malformed JSON, null input, empty object, nested structures, type handling
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/Extensions/JsonExtensions_Tests.cs` | class: `JsonExtensions_Tests`

- [x] [P1-T15] Create `JsonSerializerExtensions_Tests.cs` covering the public API surface of `JsonSerializerExtensions`: serializer settings, null serializer, round-trip scenarios, custom converters
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/Extensions/JsonSerializerExtensions_Tests.cs` | class: `JsonSerializerExtensions_Tests`

### Phase 2 — P1 Helper Classes Tests

All test files placed in `UtilitiesCS.Test/HelperClasses/`. Same conventions as Phase 1.

- [x] [P2-T1] Create `MergeSortImplementations_Tests.cs` covering the public API surface of `MergeSortImplementations`: empty array, single element, pre-sorted, reverse-sorted, duplicates, large array, different types
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/HelperClasses/MergeSortImplementations_Tests.cs` | class: `MergeSortImplementations_Tests`

- [x] [P2-T2] Create `ParamArray_Tests.cs` covering the public API surface of `ParamArray`: empty params, single param, multiple params, null, type variations
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/HelperClasses/ParamArray_Tests.cs` | class: `ParamArray_Tests`

- [x] [P2-T3] Create `ObjectSize_Tests.cs` covering the public API surface of `ObjectSize`: primitive types, reference types, complex objects, null, empty objects
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/HelperClasses/ObjectSize_Tests.cs` | class: `ObjectSize_Tests`

- [x] [P2-T4] Create `ReflectionHelper_Tests.cs` covering the public API surface of `ReflectionHelper`: known types, unknown types, null input, generic types, interface types
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/HelperClasses/ReflectionHelper_Tests.cs` | class: `ReflectionHelper_Tests`

- [x] [P2-T5] Create `SegmentStopWatch_Tests.cs` covering the public API surface of `SegmentStopWatch`: start/stop, segment naming, elapsed tracking, reset, multiple segments
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/HelperClasses/SegmentStopWatch_Tests.cs` | class: `SegmentStopWatch_Tests`

- [x] [P2-T6] Create `GenericBitwise_Tests.cs` covering the public API surface of `GenericBitwise`: AND/OR/XOR/NOT, boundary values, zero, max, different integer types
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/HelperClasses/GenericBitwise_Tests.cs` | class: `GenericBitwise_Tests`

- [x] [P2-T7] Create `DeepCompare_Tests.cs` covering the public API surface of `DeepCompare`: equal objects, different objects, null, nested objects, collections, circular references
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/HelperClasses/DeepCompare_Tests.cs` | class: `DeepCompare_Tests`

- [x] [P2-T8] Expand `PrettyPrint_Tests.cs` covering the public API surface of `PrettyPrint`: null input, empty collections, nested objects, primitives, formatting edge cases
  - Preconditions: Existing test file may exist — expand to cover untested methods
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/HelperClasses/PrettyPrint_Tests.cs` | class: `PrettyPrint_Tests`

- [x] [P2-T9] Expand `SimpleRegex_Tests.cs` covering the public API surface of `SimpleRegex`: match patterns, no match, null input, empty pattern, special regex chars, multiple matches
  - Preconditions: Existing test file may exist — expand to cover untested methods
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/HelperClasses/SimpleRegex_Tests.cs` | class: `SimpleRegex_Tests`

- [x] [P2-T10] Expand `Tokenizer_Tests.cs` covering the public API surface of `Tokenizer`: empty input, single token, multiple tokens, delimiters, quoted strings, whitespace
  - Preconditions: Existing test file may exist — expand to cover untested methods
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/HelperClasses/Tokenizer_Tests.cs` | class: `Tokenizer_Tests`

### Phase 3 — P1 Reusable Type Classes Tests

All test files placed in `UtilitiesCS.Test/ReusableTypeClasses/`. Same conventions as Phase 1.

- [x] [P3-T1] Create `AsyncLazy_Tests.cs` covering the public API surface of `AsyncLazy`: lazy initialization, multiple await, exception propagation, value type, reference type, concurrent access
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/ReusableTypeClasses/AsyncLazy_Tests.cs` | class: `AsyncLazy_Tests`

- [x] [P3-T2] Create `LazyTry_Tests.cs` covering the public API surface of `LazyTry`: successful init, failed init with exception capture, retry after failure, null factory
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/ReusableTypeClasses/LazyTry_Tests.cs` | class: `LazyTry_Tests`

- [x] [P3-T3] Create `Matrix_Tests.cs` covering the public API surface of `Matrix`: construction, indexing, bounds checks, arithmetic operations, empty matrix, single-element, identity
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/ReusableTypeClasses/Matrix_Tests.cs` | class: `Matrix_Tests`

- [x] [P3-T4] Create `JaggedMatrix_Tests.cs` covering the public API surface of `JaggedMatrix`: construction, ragged rows, indexing, null rows, operations
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/ReusableTypeClasses/JaggedMatrix_Tests.cs` | class: `JaggedMatrix_Tests`

- [x] [P3-T5] Create `DenMatrix_Tests.cs` covering the public API surface of `DenMatrix`: dense matrix operations, dimension validation, element access, boundary
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/ReusableTypeClasses/DenMatrix_Tests.cs` | class: `DenMatrix_Tests`

- [x] [P3-T6] Create `DataConverter2d_Tests.cs` covering the public API surface of `DataConverter2d`: conversion between formats, null input, empty data, mismatched dimensions
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/ReusableTypeClasses/DataConverter2d_Tests.cs` | class: `DataConverter2d_Tests`

- [x] [P3-T7] Create `StackGeek_Tests.cs` covering the public API surface of `StackGeek`: push, pop, peek, empty stack, overflow, underflow, Count
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/ReusableTypeClasses/StackGeek_Tests.cs` | class: `StackGeek_Tests`

- [x] [P3-T8] Create `StackObjectCS_Tests.cs` covering the public API surface of `StackObjectCS`: push, pop, peek, empty stack, type safety, multiple types
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/ReusableTypeClasses/StackObjectCS_Tests.cs` | class: `StackObjectCS_Tests`

- [x] [P3-T9] Create `TreeNodeOfT_Tests.cs` covering the public API surface of `TreeNodeOfT`: add child, remove child, parent reference, depth, traversal, leaf detection, root detection, null data
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/ReusableTypeClasses/TreeNodeOfT_Tests.cs` | class: `TreeNodeOfT_Tests`

- [x] [P3-T10] Create `SerializableList_Tests.cs` covering the public API surface of `SerializableList`: add, remove, clear, enumerate, indexer, Count, serialization round-trip, empty list, single item
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/ReusableTypeClasses/SerializableList_Tests.cs` | class: `SerializableList_Tests`

- [x] [P3-T11] Create `ScBag_Tests.cs` covering the public API surface of `ScBag`: add, remove, contains, enumerate, Count, concurrent access, empty bag
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/ReusableTypeClasses/ScBag_Tests.cs` | class: `ScBag_Tests`

- [x] [P3-T12] Expand `ScoCollection_Tests.cs` covering the public API surface of `ScoCollection`: add, remove, clear, enumerate, concurrent add/remove, empty collection, single item
  - Preconditions: Existing test file may exist — expand to cover untested methods
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/ReusableTypeClasses/ScoCollection_Tests.cs` | class: `ScoCollection_Tests`

- [x] [P3-T13] Create `SCODictionary_Tests.cs` covering the public API surface of `SCODictionary`: add, remove, TryGetValue, indexer, Keys, Values, Count, concurrent access, missing key, duplicate key
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/ReusableTypeClasses/SCODictionary_Tests.cs` | class: `SCODictionary_Tests`

- [x] [P3-T14] Create `ScoStack_Tests.cs` covering the public API surface of `ScoStack`: push, pop, peek, TryPop, Count, empty stack, concurrent access
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/ReusableTypeClasses/ScoStack_Tests.cs` | class: `ScoStack_Tests`

### Phase 4 — P1 Email Intelligence Logic Tests

All test files placed in `UtilitiesCS.Test/EmailIntelligence/`. Focus on pure logic paths only — no COM, no Outlook.

- [x] [P4-T1] Create `Prediction_Tests.cs` covering the public API surface of `Prediction`: construction, property access, equality, comparison, null folder, zero probability, boundary probability values
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/EmailIntelligence/Prediction_Tests.cs` | class: `Prediction_Tests`

- [x] [P4-T2] Create `DoNotSerializeContractResolver_Tests.cs` covering the public API surface of `DoNotSerializeContractResolver`: resolves properties correctly, excludes decorated properties, handles null type, round-trip JSON serialization with resolver
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/EmailIntelligence/DoNotSerializeContractResolver_Tests.cs` | class: `DoNotSerializeContractResolver_Tests`

- [x] [P4-T3] Expand `CtfIncidence_Tests.cs` covering the public API surface of `CtfIncidence`: construction, property accessors, equality, comparison, merge, zero counts, negative counts, boundary
  - Preconditions: Existing test file at 72% coverage — expand to cover remaining
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/EmailIntelligence/CtfIncidence_Tests.cs` | class: `CtfIncidence_Tests`

- [x] [P4-T4] Expand `CtfIncidenceList_Tests.cs` covering the public API surface of `CtfIncidenceList`: add, remove, lookup, merge, sorting, empty list, single item, duplicate entries
  - Preconditions: Existing test file at 28% coverage — expand significantly
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/EmailIntelligence/CtfIncidenceList_Tests.cs` | class: `CtfIncidenceList_Tests`

- [x] [P4-T5] Expand `CtfMap_Tests.cs` covering the public API surface of `CtfMap`: add entry, lookup, merge maps, empty map, single entry, overwrite existing
  - Preconditions: Existing test file at 58% coverage — expand to cover remaining
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/EmailIntelligence/CtfMap_Tests.cs` | class: `CtfMap_Tests`

- [x] [P4-T6] Expand `EmailTokenizer_Tests.cs` covering the public API surface of `EmailTokenizer`: tokenize typical email body, empty body, HTML body, special characters, URL extraction, token dedup
  - Preconditions: Existing test file may have partial coverage — expand
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/EmailIntelligence/EmailTokenizer_Tests.cs` | class: `EmailTokenizer_Tests`

- [x] [P4-T7] Create `ImageStripper_Tests.cs` covering the public API surface of `ImageStripper`: strip embedded images, no images present, null input, empty body, mixed content
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/EmailIntelligence/ImageStripper_Tests.cs` | class: `ImageStripper_Tests`

- [x] [P4-T8] Expand `MinedMailInfo_Tests.cs` covering the public API surface of `MinedMailInfo`: construction, property defaults, equality, null fields, serialization round-trip
  - Preconditions: Existing test file at 54% coverage — expand
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/EmailIntelligence/MinedMailInfo_Tests.cs` | class: `MinedMailInfo_Tests`

- [x] [P4-T9] Create `MovedMailInfo_Tests.cs` covering the public API surface of `MovedMailInfo`: construction, property defaults, equality, ToString, null handling
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/EmailIntelligence/MovedMailInfo_Tests.cs` | class: `MovedMailInfo_Tests`

- [x] [P4-T10] Expand `FlagParser_Tests.cs` covering the public API surface of `FlagParser`: parse known flags, unknown flags, empty input, null input, delimiter variations, multiple flags
  - Preconditions: Existing test file may have partial coverage — expand
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/EmailIntelligence/FlagParser_Tests.cs` | class: `FlagParser_Tests`

- [x] [P4-T11] Create `FlagClassNoItem_Tests.cs` covering the public API surface of `FlagClassNoItem`: construction, property access, equality, null fields, comparison
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/EmailIntelligence/FlagClassNoItem_Tests.cs` | class: `FlagClassNoItem_Tests`

- [x] [P4-T12] Create `FlagDetails_Tests.cs` covering the public API surface of `FlagDetails`: construction, property access, defaults, equality, null handling
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/EmailIntelligence/FlagDetails_Tests.cs` | class: `FlagDetails_Tests`

- [x] [P4-T13] Expand `CommonWords_Tests.cs` covering the public API surface of `CommonWords`: known common words, unknown words, empty input, null, case insensitivity, boundary
  - Preconditions: Existing test file may have partial coverage — expand
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/EmailIntelligence/CommonWords_Tests.cs` | class: `CommonWords_Tests`

- [x] [P4-T14] Create `SubjectMapEntry_Tests.cs` covering the public API surface of `SubjectMapEntry`: construction, property access, equality, comparison, null subject, empty tokens
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/EmailIntelligence/SubjectMapEntry_Tests.cs` | class: `SubjectMapEntry_Tests`

### Phase 5 — P1 OutlookObjects Logic, NewtonsoftHelpers, and Dialogs Tests

Test files placed in respective `UtilitiesCS.Test/` subdirectories. Focus on comparers, POCOs, binders, and dialog logic — no COM interop.

**OutlookObjects (in `UtilitiesCS.Test/OutlookObjects/`):**

- [x] [P5-T1] Expand `DASLFilterParser_Tests.cs` covering the public API surface of `DASLFilterParser`: parse valid DASL filters, invalid syntax, empty filter, null input, nested conditions
  - Preconditions: Existing coverage at 90.3% — expand to cover remaining edge cases
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/OutlookObjects/DASLFilterParser_Tests.cs` | class: `DASLFilterParser_Tests`

- [x] [P5-T2] Expand `OlItemSummary_Tests.cs` covering the public API surface of `OlItemSummary`: construction, property access, ToString, equality, null properties
  - Preconditions: Existing test file with partial coverage — expand
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/OutlookObjects/OlItemSummary_Tests.cs` | class: `OlItemSummary_Tests`

- [x] [P5-T3] Create `ItemComparer_Tests.cs` covering the public API surface of `ItemComparer`: equal items, different items, null item, same reference, ordering consistency
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/OutlookObjects/ItemComparer_Tests.cs` | class: `ItemComparer_Tests`

- [x] [P5-T4] Create `AttachmentSerializable_Tests.cs` covering the public API surface of `AttachmentSerializable`: construction, property access, serialization round-trip, null fields, defaults
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/OutlookObjects/AttachmentSerializable_Tests.cs` | class: `AttachmentSerializable_Tests`

- [x] [P5-T5] Create `FolderWrapperNameComparer_Tests.cs` covering the public API surface of `FolderWrapperNameComparer`: equal names, different names, null, case sensitivity, special characters
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/OutlookObjects/FolderWrapperNameComparer_Tests.cs` | class: `FolderWrapperNameComparer_Tests`

- [x] [P5-T6] Create `FolderWrapperNameAndParentNameComparer_Tests.cs` covering the public API surface of `FolderWrapperNameAndParentNameComparer`: same name+parent, different names, different parents, null, ordering
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/OutlookObjects/FolderWrapperNameAndParentNameComparer_Tests.cs` | class: `FolderWrapperNameAndParentNameComparer_Tests`

- [x] [P5-T7] Create `FolderWrapperNameCountSizeComparer_Tests.cs` covering the public API surface of `FolderWrapperNameCountSizeComparer`: equal, different count, different size, null, boundary values
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/OutlookObjects/FolderWrapperNameCountSizeComparer_Tests.cs` | class: `FolderWrapperNameCountSizeComparer_Tests`

- [x] [P5-T8] Create `FolderWrapperNodeComparer_Tests.cs` covering the public API surface of `FolderWrapperNodeComparer`: equal nodes, different nodes, null, tree depth variations
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/OutlookObjects/FolderWrapperNodeComparer_Tests.cs` | class: `FolderWrapperNodeComparer_Tests`

- [x] [P5-T9] Create `FolderWrapperNodeContentsComparer_Tests.cs` covering the public API surface of `FolderWrapperNodeContentsComparer`: equal contents, different contents, null, empty nodes
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/OutlookObjects/FolderWrapperNodeContentsComparer_Tests.cs` | class: `FolderWrapperNodeContentsComparer_Tests`

- [x] [P5-T10] Create `ItemInfo_Tests.cs` covering the public API surface of `ItemInfo`: construction, property access, defaults, equality, null handling, ToString
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/OutlookObjects/ItemInfo_Tests.cs` | class: `ItemInfo_Tests`

**NewtonsoftHelpers (in `UtilitiesCS.Test/NewtonsoftHelpers/`):**

- [x] [P5-T11] Create `AllInclusiveBinder_Tests.cs` covering the public API surface of `AllInclusiveBinder`: bind known type, unknown type, null type name, assembly-qualified name, generic types
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/NewtonsoftHelpers/AllInclusiveBinder_Tests.cs` | class: `AllInclusiveBinder_Tests`

- [x] [P5-T12] Create `KnownTypesBinder_Tests.cs` covering the public API surface of `KnownTypesBinder`: bind registered type, unregistered type, null, multiple registered types, serializer integration
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/NewtonsoftHelpers/KnownTypesBinder_Tests.cs` | class: `KnownTypesBinder_Tests`

**Dialogs (in `UtilitiesCS.Test/Dialogs/`):**

- [x] [P5-T13] Create `DelegateButton_Tests.cs` covering the public API surface of `DelegateButton`: construction, delegate invocation, null delegate, property access, click behavior
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/Dialogs/DelegateButton_Tests.cs` | class: `DelegateButton_Tests`

- [x] [P5-T14] Expand `YesNoToAll_Tests.cs` covering the public API surface of `YesNoToAll`: each button result, null prompt, empty prompt, default values, state after selection
  - Preconditions: Existing test file with partial coverage — expand
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/Dialogs/YesNoToAll_Tests.cs` | class: `YesNoToAll_Tests`

### Phase 6 — P2 Threading and Medium-Difficulty Tests

Test files placed in respective `UtilitiesCS.Test/` subdirectories. These require Moq mocking of interfaces and deterministic synchronization patterns (ManualResetEvent, TaskCompletionSource).

**Threading (in `UtilitiesCS.Test/Threading/`):**

- [x] [P6-T1] Create `ThreadSafeFunctions_Tests.cs` covering the public API surface of `ThreadSafeFunctions`: thread-safe invocation, concurrent calls, null function, return value propagation
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/Threading/ThreadSafeFunctions_Tests.cs` | class: `ThreadSafeFunctions_Tests`

- [x] [P6-T2] Create `ThreadSafeSingleShotGuard_Tests.cs` covering the public API surface of `ThreadSafeSingleShotGuard`: first call succeeds, second call blocked, reset behavior, concurrent attempts
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/Threading/ThreadSafeSingleShotGuard_Tests.cs` | class: `ThreadSafeSingleShotGuard_Tests`

- [x] [P6-T3] Create `TaskPriority_Tests.cs` covering the public API surface of `TaskPriority`: priority ordering, equal priority, default priority, boundary values
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/Threading/TaskPriority_Tests.cs` | class: `TaskPriority_Tests`

- [x] [P6-T4] Create `ProgressPackage_Tests.cs` covering the public API surface of `ProgressPackage`: construction, property access, progress percentage, null message, boundary percentages (0%, 100%)
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/Threading/ProgressPackage_Tests.cs` | class: `ProgressPackage_Tests`

- [x] [P6-T5] Create `ProgressTracker_Tests.cs` covering the public API surface of `ProgressTracker`: increment, completion percentage, reset, zero total, single step, multi-step, overflow
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/Threading/ProgressTracker_Tests.cs` | class: `ProgressTracker_Tests`

- [x] [P6-T6] Create `TimeOutTask_Tests.cs` covering the public API surface of `TimeOutTask`: completes before timeout, times out, cancellation, zero timeout, negative timeout, exception propagation
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/Threading/TimeOutTask_Tests.cs` | class: `TimeOutTask_Tests`

**Extensions — async/stream (in `UtilitiesCS.Test/Extensions/`):**

- [x] [P6-T7] Create `StreamExtensions_Tests.cs` covering the public API surface of `StreamExtensions`: read from MemoryStream, empty stream, null stream, copy, position reset
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/Extensions/StreamExtensions_Tests.cs` | class: `StreamExtensions_Tests`

- [x] [P6-T8] Create `IAsyncEnumerableExtensions_Tests.cs` covering the public API surface of `IAsyncEnumerableExtensions`: empty async enumerable, single item, multiple items, cancellation, null source
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/Extensions/IAsyncEnumerableExtensions_Tests.cs` | class: `IAsyncEnumerableExtensions_Tests`

- [x] [P6-T9] Create `AsyncSerialization_Tests.cs` covering the public API surface of `AsyncSerialization`: serialize/deserialize round-trip, null input, empty object, complex graph, cancellation
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/Extensions/AsyncSerialization_Tests.cs` | class: `AsyncSerialization_Tests`

**HelperClasses — with deps (in `UtilitiesCS.Test/HelperClasses/`):**

- [x] [P6-T10] Create `Initializer_Tests.cs` covering the public API surface of `Initializer`: initialize once, already initialized, null action, exception during init, concurrent init
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/HelperClasses/Initializer_Tests.cs` | class: `Initializer_Tests`

- [x] [P6-T11] Create `ObjectCopier_Tests.cs` covering the public API surface of `ObjectCopier`: deep copy simple object, nested object, null, collection, circular reference handling
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/HelperClasses/ObjectCopier_Tests.cs` | class: `ObjectCopier_Tests`

- [x] [P6-T12] Create `DirectoryInfoWrapper_Tests.cs` covering the public API surface of `DirectoryInfoWrapper`: property access via mock, null path handling, equality
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/HelperClasses/DirectoryInfoWrapper_Tests.cs` | class: `DirectoryInfoWrapper_Tests`

- [x] [P6-T13] Create `FileInfoWrapper_Tests.cs` covering the public API surface of `FileInfoWrapper`: property access via mock, null path, equality, extension extraction
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/HelperClasses/FileInfoWrapper_Tests.cs` | class: `FileInfoWrapper_Tests`

- [x] [P6-T14] Create `DebugTextLogger_Tests.cs` covering the public API surface of `DebugTextLogger`: log message, empty message, null message, log level filtering
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/HelperClasses/DebugTextLogger_Tests.cs` | class: `DebugTextLogger_Tests`

- [x] [P6-T15] Create `TraceUtility_Tests.cs` covering the public API surface of `TraceUtility`: trace output capture, null arguments, empty message, format string
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/HelperClasses/TraceUtility_Tests.cs` | class: `TraceUtility_Tests`

- [x] [P6-T16] Create `VerboseLogger_Tests.cs` covering the public API surface of `VerboseLogger`: verbose on/off, log message, null message, level control
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/HelperClasses/VerboseLogger_Tests.cs` | class: `VerboseLogger_Tests`

### Phase 7 — P2 ReusableTypeClasses Medium and Observable Tests

All test files placed in `UtilitiesCS.Test/ReusableTypeClasses/`. These require Moq mocking and deterministic synchronization for observable and concurrent types.

**Observable collections:**

- [x] [P7-T1] Create `ObservableCollectionBatchUpdate_Tests.cs` covering the public API surface of `ObservableCollectionBatchUpdate`: single add, batch add, remove, clear, event firing, empty batch, nested batch
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/ReusableTypeClasses/ObservableCollectionBatchUpdate_Tests.cs` | class: `ObservableCollectionBatchUpdate_Tests`

- [x] [P7-T2] Create `ObservableDictionary_Tests.cs` covering the public API surface of `ObservableDictionary`: add, remove, update, clear, event firing per operation, TryGetValue, indexer, empty dict, duplicate key
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/ReusableTypeClasses/ObservableDictionary_Tests.cs` | class: `ObservableDictionary_Tests`

- [x] [P7-T3] Create `ObserverHelper_Tests.cs` covering the public API surface of `ObserverHelper`: subscribe, unsubscribe, notify, null observer, multiple observers
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/ReusableTypeClasses/ObserverHelper_Tests.cs` | class: `ObserverHelper_Tests`

**Concurrent observable collections:**

- [SKIP] [P7-T4] Create `ConcurrentObservableBag_Tests.cs` covering the public API surface of `ConcurrentObservableBag`: add, remove, concurrent add/remove, event firing, empty bag, Count
  - Skip note: `UtilitiesCS/ReusableTypeClasses/Concurrent/Observable/Bag/ConcurrentObservableBag.cs` is entirely commented out and has no live buildable class
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/ReusableTypeClasses/ConcurrentObservableBag_Tests.cs` | class: `ConcurrentObservableBag_Tests`

- [x] [P7-T5] Create `DictionaryChangedEventArgs_Tests.cs` covering the public API surface of `DictionaryChangedEventArgs`: construction with each change type, property access, null key
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/ReusableTypeClasses/DictionaryChangedEventArgs_Tests.cs` | class: `DictionaryChangedEventArgs_Tests`

- [x] [P7-T6] Create `BagChangedEventArgs_Tests.cs` covering the public API surface of `BagChangedEventArgs`: construction with each change type, property access, null item
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/ReusableTypeClasses/BagChangedEventArgs_Tests.cs` | class: `BagChangedEventArgs_Tests`

**Locking collections:**

- [x] [P7-T7] Create `LockingLinkedList_Tests.cs` covering the public API surface of `LockingLinkedList`: add, remove, find, enumerate, concurrent add/remove, empty list, single node, head/tail
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/ReusableTypeClasses/LockingLinkedList_Tests.cs` | class: `LockingLinkedList_Tests`

- [x] [P7-T8] Create `LockingLinkedListNode_Tests.cs` covering the public API surface of `LockingLinkedListNode`: construction, Value property, Next/Previous references, null value
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/ReusableTypeClasses/LockingLinkedListNode_Tests.cs` | class: `LockingLinkedListNode_Tests`

**Other ReusableTypeClasses:**

- [x] [P7-T9] Create `AsyncQueue_Tests.cs` covering the public API surface of `AsyncQueue`: enqueue, dequeue await, empty queue await, cancellation, concurrent enqueue/dequeue
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/ReusableTypeClasses/AsyncQueue_Tests.cs` | class: `AsyncQueue_Tests`

- [x] [P7-T10] Create `AbstractCloneable_Tests.cs` covering the public API surface of `AbstractCloneable`: clone concrete subclass, verify deep copy, null fields, collection fields
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/ReusableTypeClasses/AbstractCloneable_Tests.cs` | class: `AbstractCloneable_Tests`

**Timed actions:**

- [x] [P7-T11] Create `TimerWrapper_Tests.cs` covering the public API surface of `TimerWrapper`: start, stop, interval, elapsed event, dispose, zero interval
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/ReusableTypeClasses/TimerWrapper_Tests.cs` | class: `TimerWrapper_Tests`

- [x] [P7-T12] Create `TimedBatchAction_Tests.cs` covering the public API surface of `TimedBatchAction`: add item, batch fires after interval, empty batch, dispose, multiple batches
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/ReusableTypeClasses/TimedBatchAction_Tests.cs` | class: `TimedBatchAction_Tests`

- [x] [P7-T13] Create `TimedQueueOfActions_Tests.cs` covering the public API surface of `TimedQueueOfActions`: enqueue action, timed execution, empty queue, dispose, concurrent enqueue
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/ReusableTypeClasses/TimedQueueOfActions_Tests.cs` | class: `TimedQueueOfActions_Tests`

- [x] [P7-T14] Create `TimedAsyncTask_Tests.cs` covering the public API surface of `TimedAsyncTask`: schedule task, cancel before execution, timeout, exception propagation, dispose
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/ReusableTypeClasses/TimedAsyncTask_Tests.cs` | class: `TimedAsyncTask_Tests`

**Serializable New/Concurrent:**

- [x] [P7-T15] Expand `ScoDictionaryNew_Tests.cs` covering the public API surface of `ScoDictionaryNew`: add, remove, TryGetValue, concurrent access, serialization, clear
  - Preconditions: Existing test file with partial coverage — expand
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/ReusableTypeClasses/ScoDictionaryNew_Tests.cs` | class: `ScoDictionaryNew_Tests`

- [x] [P7-T16] Create `ScoSortedDictionary_Tests.cs` covering the public API surface of `ScoSortedDictionary`: add, remove, sorted enumeration, concurrent access, empty, duplicate key
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/ReusableTypeClasses/ScoSortedDictionary_Tests.cs` | class: `ScoSortedDictionary_Tests`

- [x] [P7-T17] Create `SloLinkedList_Tests.cs` covering the public API surface of `SloLinkedList`: add, remove, find, enumerate, concurrent operations, empty list, single node
  - Acceptance: Per acceptance standard — file: `UtilitiesCS.Test/ReusableTypeClasses/SloLinkedList_Tests.cs` | class: `SloLinkedList_Tests`

### Phase 8 — Final QA Loop

Run the full C# toolchain in strict order. If any step fails or changes files, restart from P8-T1 until a clean pass completes.

- [x] [P8-T1] Run `dotnet format TaskMaster.sln --verify-no-changes --no-restore` and record result
  - Acceptance: Command exits with code 0; evidence artifact `evidence/qa-gates/final-format.{ISO-8601 yyyy-MM-ddTHH-mm}.md` exists with `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:`

- [x] [P8-T2] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true` and record result
  - Preconditions: P8-T1 passed
  - Acceptance: Command exits with code 0; evidence artifact `evidence/qa-gates/final-analyzers.{ISO-8601 yyyy-MM-ddTHH-mm}.md` exists with `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:`

- [x] [P8-T3] Run `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true` and record result
  - Preconditions: P8-T2 passed
  - Acceptance: Command exits with code 0; evidence artifact `evidence/qa-gates/final-nullable.{ISO-8601 yyyy-MM-ddTHH-mm}.md` exists with `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:`

- [x] [P8-T4] Run `pwsh -NoProfile -ExecutionPolicy Bypass -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug` and record result
  - Preconditions: P8-T3 passed
  - Acceptance: Command exits with code 0; all tests pass with zero failures; evidence artifact `evidence/qa-gates/final-test-coverage.{ISO-8601 yyyy-MM-ddTHH-mm}.md` exists with `Timestamp:`, `Command:`, `EXIT_CODE: 0`, `Output Summary:` including total pass/fail counts and numeric UtilitiesCS package line-rate percentage

- [ ] [P8-T5] Verify per-file UtilitiesCS coverage ≥80% for all non-excluded source files by inspecting the coverage report against the Coverage Exclusion List
  - Preconditions: P8-T4 passed with coverage data
  - Acceptance: Evidence artifact `evidence/qa-gates/final-per-file-coverage.{ISO-8601 yyyy-MM-ddTHH-mm}.md` exists listing each non-excluded UtilitiesCS source file with its numeric post-change line-rate percentage, every file ≥80%; excluded files listed separately with exclusion category

- [ ] [P8-T6] Record coverage delta: baseline vs. final for UtilitiesCS package-level and per-file metrics
  - Preconditions: P8-T5 passed
  - Acceptance: Evidence artifact `evidence/qa-gates/final-coverage-delta.{ISO-8601 yyyy-MM-ddTHH-mm}.md` exists with numeric baseline line-rate percentage (14.5%), numeric final line-rate percentage, numeric delta, and per-file numeric line-rate improvements

- [ ] [P8-T7] If any of P8-T1 through P8-T6 failed or required file changes, restart from P8-T1 and repeat until a fully clean pass is achieved
  - Acceptance: All of P8-T1 through P8-T6 pass in a single contiguous run with no file modifications or failures; final evidence artifacts reflect a clean pass

---

## Test Plan

~880 new MSTest methods across ~125 test files in UtilitiesCS.Test/. No integration tests (test-only initiative). Coverage target: ≥80% per non-excluded file.

## Open Questions / Notes

- **Existing test expansion:** Tasks marked "expand" add methods to existing test files while preserving all existing passing tests.
- **Mocking patterns:** P2 tasks follow existing Moq patterns (`MockBehavior.Loose` for COM interfaces, `Mock<IApplicationGlobals>`, `Mock<IFileSystemFolderPaths>`).
