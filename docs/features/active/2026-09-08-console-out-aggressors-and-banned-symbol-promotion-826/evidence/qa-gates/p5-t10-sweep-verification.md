# Item-1 sweep verification at repository scope (issue #826, [P5-T10])

Timestamp: 2026-09-09T19-40

Command, run as `pwsh -NoProfile -Command` blocks carrying the plan's C2 preamble branch guard:

```
$cs = @(git ls-files "*.cs")
$so = @(Select-String -LiteralPath $cs -CaseSensitive -SimpleMatch "Console.SetOut(")
$so.Count
$so.Path
```

followed by per-file `DebugTextWriter` counts over the 33 write-set files and per-file `TestInitialize`
counts over the ten AC4 files.

EXIT_CODE: 0

## Repository-wide `Console.SetOut(` result

Occurrence count: **2**. Distinct file list:

```
TaskMaster/ThisAddIn.cs
UtilitiesCS.Test/EmailIntelligence/Bayesian/BayesianClassifierTests_UnfinishedStubs.cs
```

This is exactly the two-file list the acceptance condition requires. Before the change the same search
returned 38 occurrences across 35 files, recorded in
`<FEATURE>/evidence/baseline/pre-change-census.md`, so the gate is capable of failing and did not pass
vacuously.

Neither surviving file is a test file modified by this feature: `TaskMaster/ThisAddIn.cs` is production
code and out of scope, and the occurrence in
`BayesianClassifierTests_UnfinishedStubs.cs` is a commented-out line that installs nothing.

## `DebugTextWriter` count is 0 in every one of the 33 write-set files

| File | Count |
|---|---|
| `VBFunctions.Test/ComputerInfo_Test.cs` | 0 |
| `UtilitiesCS.Test/Threading/AppGlobalsConverterTests.cs` | 0 |
| `UtilitiesCS.Test/Threading/AppGlobalsConverterTests_Unfinished.cs` | 0 |
| `UtilitiesCS.Test/HelperClasses/PrettyPrintTest.cs` | 0 |
| `UtilitiesCS.Test/Extensions/Frexp_Test.cs` | 0 |
| `UtilitiesCS.Test/EmailIntelligence/EmailDetailsTest.cs` | 0 |
| `UtilitiesCS.Test/EmailIntelligence/EmailParsingSorting/MinedMailInfoTests.cs` | 0 |
| `UtilitiesCS.Test/EmailIntelligence/EmailParsingSorting/MailItemHelperTests.cs` | 0 |
| `UtilitiesCS.Test/EmailIntelligence/ClassifierGroups/Triage/Triage_OlLogicTests.cs` | 0 |
| `UtilitiesCS.Test/EmailIntelligence/Bayesian/ObsoleteBayesianClassifier_Tests.cs` | 0 |
| `UtilitiesCS.Test/EmailIntelligence/Bayesian/BayesianSerializationHelper_Tests.cs` | 0 |
| `UtilitiesCS.Test/EmailIntelligence/Bayesian/BayesianPerformanceMeasurement_Tests.cs` | 0 |
| `UtilitiesCS.Test/EmailIntelligence/Bayesian/BayesianClassifierTests.cs` | 0 |
| `UtilitiesCS.Test/EmailIntelligence/Bayesian/BayesianClassifierSharedTests.cs` | 0 |
| `UtilitiesCS.Test/EmailIntelligence/Bayesian/BayesianClassifierGroupTests.cs` | 0 |
| `UtilitiesCS.Test/OneDriveHelpers/AngleSharpParsedEmailBodyTests.cs` | 0 |
| `UtilitiesCS.Test/NewtonsoftHelpers/WrapperScoDictionaryTest.cs` | 0 |
| `UtilitiesCS.Test/NewtonsoftHelpers/WrapperScDictionaryTest.cs` | 0 |
| `UtilitiesCS.Test/NewtonsoftHelpers/WrapperPeopleScoDictionaryNew_Tests.cs` | 0 |
| `UtilitiesCS.Test/NewtonsoftHelpers/ScoDictionaryConverterTests.cs` | 0 |
| `UtilitiesCS.Test/NewtonsoftHelpers/ScDictionaryConverter_Tests.cs` | 0 |
| `UtilitiesCS.Test/NewtonsoftHelpers/PeopleScoConverter_Tests.cs` | 0 |
| `UtilitiesCS.Test/NewtonsoftHelpers/FilePathHelperConverterTests.cs` | 0 |
| `TaskMaster.Test/AppGlobals/AppToDoObjectsTests.cs` | 0 |
| `QuickFiler.Test/Controllers/QfcHomeControllerTests.cs` | 0 |
| `QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncTests.cs` | 0 |
| `QuickFiler.Test/Controllers/QfcHomeControllerPropertyTests.cs` | 0 |
| `QuickFiler.Test/Controllers/QfcHomeControllerIterationTests.cs` | 0 |
| `QuickFiler.Test/Controllers/QfcFormControllerTests.cs` | 0 |
| `QuickFiler.Test/Controllers/QfcFormControllerSeamTests.cs` | 0 |
| `ToDoModel.Test/Data Model/People/PeopleScoDictionaryNewTests.cs` | 0 |
| `ToDoModel.Test/Data Model/Tree/TreeNodeTests.cs` | 0 |
| `ToDoModel.Test/Data Model/Tree/TreeNodeTests_UnfinishedStubs.cs` | 0 |

Write-set file count: 33. Files with a non-zero count: 0. Every one of these files carried a non-zero
count at the [P0-T5] census (31 files at 1, `ObsoleteBayesianClassifier_Tests.cs` at 2, and the two
`TreeNode` files at 3 each), so this gate is capable of failing.

## `TestInitialize` count is 0 in every one of the ten AC4 files

| File | Count | Census |
|---|---|---|
| `VBFunctions.Test/ComputerInfo_Test.cs` | 0 | 1 |
| `UtilitiesCS.Test/HelperClasses/PrettyPrintTest.cs` | 0 | 2 |
| `UtilitiesCS.Test/Extensions/Frexp_Test.cs` | 0 | 2 |
| `UtilitiesCS.Test/EmailIntelligence/EmailDetailsTest.cs` | 0 | 2 |
| `UtilitiesCS.Test/NewtonsoftHelpers/WrapperPeopleScoDictionaryNew_Tests.cs` | 0 | 2 |
| `TaskMaster.Test/AppGlobals/AppToDoObjectsTests.cs` | 0 | 2 |
| `UtilitiesCS.Test/EmailIntelligence/Bayesian/ObsoleteBayesianClassifier_Tests.cs` | 0 | 4 |
| `UtilitiesCS.Test/OneDriveHelpers/AngleSharpParsedEmailBodyTests.cs` | 0 | 2 |
| `UtilitiesCS.Test/EmailIntelligence/Bayesian/BayesianClassifierGroupTests.cs` | 0 | 2 |
| `UtilitiesCS.Test/EmailIntelligence/Bayesian/BayesianClassifierSharedTests.cs` | 0 | 2 |

Every one of the ten dropped from a non-zero census value to 0.

## Expected retentions, recorded and not asserted to be zero

Five files legitimately retain the `DebugTextWriter` token and are out of scope for AC2:

- `UtilitiesCS/HelperClasses/Logging/DebugTextWriter.cs` — the type definition itself
- `UtilitiesCS.Test/HelperClasses/DebugTextLogger_Tests.cs`
- `UtilitiesCS.Test/DeedleTests.cs`
- `UtilitiesCS.Test/Extensions/DeedleTests.cs`
- `TaskMaster/ThisAddIn.cs`

Output Summary: the repository-wide `Console.SetOut(` occurrence count is 2, across exactly
`TaskMaster/ThisAddIn.cs` and
`UtilitiesCS.Test/EmailIntelligence/Bayesian/BayesianClassifierTests_UnfinishedStubs.cs`. The
`DebugTextWriter` count is 0 in all 33 write-set files and the `TestInitialize` count is 0 in all ten AC4
files. AC1, AC2 and AC4 are satisfied.
