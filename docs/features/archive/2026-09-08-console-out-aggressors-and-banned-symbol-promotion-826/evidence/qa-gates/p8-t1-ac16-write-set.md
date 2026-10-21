# AC16 scope gate, measured (issue #826, [P8-T1])

Timestamp: 2026-09-09T19-46

Command, run as one `pwsh -NoProfile -Command` block carrying the plan's C2 preamble branch guard:

```
git add --intent-to-add "UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensionsTimeoutDiagnosticsTests.cs"
git diff --name-only $Base -- . ":(exclude).claude"
git status --porcelain --untracked-files=all -- . ":(exclude).claude"
```

The intent-to-add is what lets the name-listing diff see the file this feature creates; an anchored
`git diff --name-only` enumerates tracked changes only and would otherwise be blind to it. The porcelain
companion covers the untracked evidence artifacts, which the diff also cannot see. The `:(exclude).claude`
pathspec is applied per plan decision D17, because `.claude/agent-memory` is tracked and may be written
mid-run by this executor or by sibling agents.

EXIT_CODE: 0 (`git add --intent-to-add` exited 0)

## Span sizes

| Span | Entries |
|---|---|
| anchored `git diff --name-only` | 39 |
| `git status --porcelain --untracked-files=all` | 81 |
| union of both, de-duplicated | 81 |

## Set difference — observed minus allow-list

**Empty. 0 entries.**

The allow-list is the 38 write-set paths plus `<FEATURE>/spec.md`,
`<FEATURE>/plan.2026-09-08T23-52.md` and any path beginning `<FEATURE>/evidence/`.

The 81 observed paths break down as:

| Category | Count |
|---|---|
| write-set paths outside the feature folder | 38 |
| this feature's plan file | 1 |
| paths under `<FEATURE>/evidence/` | 42 |
| **total** | **81** |

The 38 non-feature paths are exactly the spec's write set:

```
.editorconfig
BannedSymbols.txt
QuickFiler.Test/Controllers/QfcFormControllerSeamTests.cs
QuickFiler.Test/Controllers/QfcFormControllerTests.cs
QuickFiler.Test/Controllers/QfcHomeControllerIterationTests.cs
QuickFiler.Test/Controllers/QfcHomeControllerPropertyTests.cs
QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncTests.cs
QuickFiler.Test/Controllers/QfcHomeControllerTests.cs
TaskMaster.Test/AppGlobals/AppToDoObjectsTests.cs
ToDoModel.Test/Data Model/People/PeopleScoDictionaryNewTests.cs
ToDoModel.Test/Data Model/Tree/TreeNodeTests.cs
ToDoModel.Test/Data Model/Tree/TreeNodeTests_UnfinishedStubs.cs
UtilitiesCS.Test/EmailIntelligence/Bayesian/BayesianClassifierGroupTests.cs
UtilitiesCS.Test/EmailIntelligence/Bayesian/BayesianClassifierSharedTests.cs
UtilitiesCS.Test/EmailIntelligence/Bayesian/BayesianClassifierTests.cs
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
UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensionsTimeoutDiagnosticsTests.cs
UtilitiesCS.Test/Threading/AppGlobalsConverterTests.cs
UtilitiesCS.Test/Threading/AppGlobalsConverterTests_Unfinished.cs
UtilitiesCS.Test/UtilitiesCS.Test.csproj
UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs
VBFunctions.Test/ComputerInfo_Test.cs
```

That is 33 item-1 test files, the table-access production file, the new test file, the project file,
`BannedSymbols.txt` and `.editorconfig` — 38 paths, neither more nor fewer.

## Set difference — allow-list minus observed (recorded, not asserted)

1 entry: `docs/features/active/2026-09-08-console-out-aggressors-and-banned-symbol-promotion-826/spec.md`.

`spec.md` is on the allow-list but has not been modified yet, because the acceptance-criteria check-offs
are [P8-T2] through [P8-T17], which run after this task. It will appear in the [P8-T20] span over the
committed tree.

## Explicit absence checks

| Path or prefix | Present in the observed set |
|---|---|
| `CLAUDE.md` | no |
| `<FEATURE>/issue.md` | no |
| any path under `<FEATURE>/research/` | no (0 matches) |
| any path under `.claude/rules/` | no (0 matches) |
| any path under `.github/instructions/` | no (0 matches) |
| any path under `docs/features/epics/` | no (0 matches) |
| `UtilitiesCS/Threading/TimeOutTask.cs` | no |
| `QuickFiler.Test/Controllers/QfcHomeControllerCleanupTests.cs` | no |

The last two are the sibling-owned paths the spec singles out under "Explicitly excluded systems". None
of the other sibling-owned paths the spec enumerates appears either; every one of them would have shown
in the 38-path non-feature list above, and none does.

No workflow file under `.github/workflows/` appears, and no file under `coverage/` appears, because
`.gitignore` excludes that directory and the raw msbuild logs, SARIF documents, TRX files, `.coverage`
files and Cobertura documents all live there.

Output Summary: the set difference "observed minus allow-list" is empty. The observed footprint is
exactly the 38 write-set paths plus this feature's own plan file and 42 evidence artifacts. Every named
out-of-scope path is absent. AC16 is satisfied at working-tree scope; [P8-T20] re-confirms it over the
committed tree.
