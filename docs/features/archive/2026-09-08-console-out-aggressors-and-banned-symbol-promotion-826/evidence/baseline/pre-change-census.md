# Pre-change census (issue #826, [P0-T5])

Timestamp: 2026-09-09T19-03

Command: two `pwsh -NoProfile -Command` blocks carrying the plan's C2 preamble. The first ran
`$cs = @(git ls-files "*.cs")`, `$so = @(Select-String -LiteralPath $cs -CaseSensitive -SimpleMatch
"Console.SetOut(")`, `$so.Count`, the `HashSet` distinct-path count, the `BannedSymbols.txt` line count
and the two `.editorconfig` token counts. The second ran, per write-set file, the `DebugTextWriter`,
`TestInitialize` and `Console.SetOut(` `-SimpleMatch` counts.

EXIT_CODE: 0

## Headline figures

| Figure | Observed | Plan-stated | Deviation |
|---|---|---|---|
| `Console.SetOut(` occurrences, tracked `*.cs` | 38 | 38 | none |
| distinct files carrying it | 35 | 35 | none |
| `BannedSymbols.txt` line count | 7 | 7 | none |
| `.editorconfig` `#181` count | 3 | 3 | none |
| `.editorconfig` `dotnet_diagnostic.RS0030.severity = suggestion` count | 1 | 1 | none |

No deviation from the first three figures. The Phase 5 and Phase 3 gates stated against them are
therefore capable of failing as written.

## Distinct file list (35)

```
QuickFiler.Test/Controllers/QfcFormControllerSeamTests.cs
QuickFiler.Test/Controllers/QfcFormControllerTests.cs
QuickFiler.Test/Controllers/QfcHomeControllerIterationTests.cs
QuickFiler.Test/Controllers/QfcHomeControllerPropertyTests.cs
QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncTests.cs
QuickFiler.Test/Controllers/QfcHomeControllerTests.cs
TaskMaster.Test/AppGlobals/AppToDoObjectsTests.cs
TaskMaster/ThisAddIn.cs
ToDoModel.Test/Data Model/People/PeopleScoDictionaryNewTests.cs
ToDoModel.Test/Data Model/Tree/TreeNodeTests.cs
ToDoModel.Test/Data Model/Tree/TreeNodeTests_UnfinishedStubs.cs
UtilitiesCS.Test/EmailIntelligence/Bayesian/BayesianClassifierGroupTests.cs
UtilitiesCS.Test/EmailIntelligence/Bayesian/BayesianClassifierSharedTests.cs
UtilitiesCS.Test/EmailIntelligence/Bayesian/BayesianClassifierTests.cs
UtilitiesCS.Test/EmailIntelligence/Bayesian/BayesianClassifierTests_UnfinishedStubs.cs
UtilitiesCS.Test/EmailIntelligence/Bayesian/BayesianPerformanceMeasurement_Tests.cs
UtilitiesCS.Test/EmailIntelligence/Bayesian/BayesianSerializationHelper_Tests.cs
UtilitiesCS.Test/EmailIntelligence/Bayesian/ObsoleteBayesianClassifier_Tests.cs
UtilitiesCS.Test/EmailIntelligence/ClassifierGroups/Triage/Triage_OlLogicTests.cs
UtilitiesCS.Test/EmailIntelligence/EmailDetailsTest.cs
UtilitiesCS.Test/EmailIntelligence/EmailParsingSorting/MailItemHelperTests.cs
UtilitiesCS.Test/EmailIntelligence/EmailParsingSorting/MinedMailInfoTests.cs
UtilitiesCS.Test/Extensions/Frexp_Test.cs
UtilitiesCS.Test/HelperClasses/PrettyPrintTest.cs
UtilitiesCS.Test/NewtonsoftHelpers/FilePathHelperConverterTests.cs
UtilitiesCS.Test/NewtonsoftHelpers/PeopleScoConverter_Tests.cs
UtilitiesCS.Test/NewtonsoftHelpers/ScDictionaryConverter_Tests.cs
UtilitiesCS.Test/NewtonsoftHelpers/ScoDictionaryConverterTests.cs
UtilitiesCS.Test/NewtonsoftHelpers/WrapperPeopleScoDictionaryNew_Tests.cs
UtilitiesCS.Test/NewtonsoftHelpers/WrapperScDictionaryTest.cs
UtilitiesCS.Test/NewtonsoftHelpers/WrapperScoDictionaryTest.cs
UtilitiesCS.Test/OneDriveHelpers/AngleSharpParsedEmailBodyTests.cs
UtilitiesCS.Test/Threading/AppGlobalsConverterTests.cs
UtilitiesCS.Test/Threading/AppGlobalsConverterTests_Unfinished.cs
VBFunctions.Test/ComputerInfo_Test.cs
```

The two files excluded from the 33-file write-set population are `TaskMaster/ThisAddIn.cs` (production,
out of scope) and
`UtilitiesCS.Test/EmailIntelligence/Bayesian/BayesianClassifierTests_UnfinishedStubs.cs` (a commented
call that installs nothing).

## Per-file counts across the 33 write-set files

| File | `DebugTextWriter` | `TestInitialize` | `Console.SetOut(` |
|---|---|---|---|
| `VBFunctions.Test/ComputerInfo_Test.cs` | 1 | 1 | 1 |
| `UtilitiesCS.Test/Threading/AppGlobalsConverterTests.cs` | 1 | 2 | 1 |
| `UtilitiesCS.Test/Threading/AppGlobalsConverterTests_Unfinished.cs` | 1 | 2 | 1 |
| `UtilitiesCS.Test/HelperClasses/PrettyPrintTest.cs` | 1 | 2 | 1 |
| `UtilitiesCS.Test/Extensions/Frexp_Test.cs` | 1 | 2 | 1 |
| `UtilitiesCS.Test/EmailIntelligence/EmailDetailsTest.cs` | 1 | 2 | 1 |
| `UtilitiesCS.Test/EmailIntelligence/EmailParsingSorting/MinedMailInfoTests.cs` | 1 | 2 | 1 |
| `UtilitiesCS.Test/EmailIntelligence/EmailParsingSorting/MailItemHelperTests.cs` | 1 | 2 | 1 |
| `UtilitiesCS.Test/EmailIntelligence/ClassifierGroups/Triage/Triage_OlLogicTests.cs` | 1 | 1 | 1 |
| `UtilitiesCS.Test/EmailIntelligence/Bayesian/ObsoleteBayesianClassifier_Tests.cs` | 2 | 4 | 2 |
| `UtilitiesCS.Test/EmailIntelligence/Bayesian/BayesianSerializationHelper_Tests.cs` | 1 | 2 | 1 |
| `UtilitiesCS.Test/EmailIntelligence/Bayesian/BayesianPerformanceMeasurement_Tests.cs` | 1 | 2 | 1 |
| `UtilitiesCS.Test/EmailIntelligence/Bayesian/BayesianClassifierTests.cs` | 1 | 2 | 1 |
| `UtilitiesCS.Test/EmailIntelligence/Bayesian/BayesianClassifierSharedTests.cs` | 1 | 2 | 1 |
| `UtilitiesCS.Test/EmailIntelligence/Bayesian/BayesianClassifierGroupTests.cs` | 1 | 2 | 1 |
| `UtilitiesCS.Test/OneDriveHelpers/AngleSharpParsedEmailBodyTests.cs` | 1 | 2 | 1 |
| `UtilitiesCS.Test/NewtonsoftHelpers/WrapperScoDictionaryTest.cs` | 1 | 2 | 1 |
| `UtilitiesCS.Test/NewtonsoftHelpers/WrapperScDictionaryTest.cs` | 1 | 2 | 1 |
| `UtilitiesCS.Test/NewtonsoftHelpers/WrapperPeopleScoDictionaryNew_Tests.cs` | 1 | 2 | 1 |
| `UtilitiesCS.Test/NewtonsoftHelpers/ScoDictionaryConverterTests.cs` | 1 | 2 | 1 |
| `UtilitiesCS.Test/NewtonsoftHelpers/ScDictionaryConverter_Tests.cs` | 1 | 2 | 1 |
| `UtilitiesCS.Test/NewtonsoftHelpers/PeopleScoConverter_Tests.cs` | 1 | 2 | 1 |
| `UtilitiesCS.Test/NewtonsoftHelpers/FilePathHelperConverterTests.cs` | 1 | 2 | 1 |
| `TaskMaster.Test/AppGlobals/AppToDoObjectsTests.cs` | 1 | 2 | 1 |
| `QuickFiler.Test/Controllers/QfcHomeControllerTests.cs` | 1 | 1 | 1 |
| `QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncTests.cs` | 1 | 1 | 1 |
| `QuickFiler.Test/Controllers/QfcHomeControllerPropertyTests.cs` | 1 | 1 | 1 |
| `QuickFiler.Test/Controllers/QfcHomeControllerIterationTests.cs` | 1 | 1 | 1 |
| `QuickFiler.Test/Controllers/QfcFormControllerTests.cs` | 1 | 1 | 1 |
| `QuickFiler.Test/Controllers/QfcFormControllerSeamTests.cs` | 1 | 1 | 1 |
| `ToDoModel.Test/Data Model/People/PeopleScoDictionaryNewTests.cs` | 1 | 1 | 1 |
| `ToDoModel.Test/Data Model/Tree/TreeNodeTests.cs` | 3 | 2 | 2 |
| `ToDoModel.Test/Data Model/Tree/TreeNodeTests_UnfinishedStubs.cs` | 3 | 2 | 2 |

Every one of the 33 files carries a non-zero `DebugTextWriter` count, so AC2's per-file zero gate is
capable of failing. Every one of the ten AC4 files carries a non-zero `TestInitialize` count, so AC4's
per-file zero gate is capable of failing. A `TestInitialize` count is recorded for all 33 files, not
only the ten, because the P5-T1 and P5-T6 gates assert the count is unchanged from this census and
their files are not among the ten.

Arithmetic reconciliation with plan decision D9: the 33 write-set files carry 36 `Console.SetOut(`
occurrences (30 files at 1, three files at 2). Adding `TaskMaster/ThisAddIn.cs` at 1 and
`BayesianClassifierTests_UnfinishedStubs.cs` at 1 gives the repository-wide 38. Subtracting the two
commented occurrences inside the commented-out `[ClassInitialize]` blocks of the two `TreeNode` files
gives the 34 live install statements D9 states.

Output Summary: all five headline figures match the plan exactly; every write-set file carries a
non-zero `DebugTextWriter` count and every AC4 file a non-zero `TestInitialize` count. No deviation to
report to the orchestrator before Phase 5.

---

Timestamp correction, recorded rather than absorbed: the `Timestamp:` value in this artifact was initially written as an extrapolated value rather than read from the clock. At 2026-09-09T19-10 the executor read the real clock, observed the discrepancy, and replaced the value with this artifact's own observed filesystem write time. The corrected value is an observation; the superseded value was not.
