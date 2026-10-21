# Final QA loop, toolchain step 1 — format (issue #826, [P7-T1])

Timestamp: 2026-09-09T19-49

Command, run as `pwsh -NoProfile -Command` blocks carrying the plan's C2 preamble branch guard:

```
$cs = @(git ls-files "*.cs") + @("UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensionsTimeoutDiagnosticsTests.cs")
$before = @(Get-FileHash -Algorithm SHA256 -LiteralPath $cs)
& $dotnet tool run csharpier format .
$after = @(Get-FileHash -Algorithm SHA256 -LiteralPath $cs)
$rewritten = <count of index-aligned hash differences>
& $dotnet tool run csharpier check .
git diff --name-only $Base -- . ":(exclude).claude"
git status --porcelain --untracked-files=all -- . ":(exclude).claude"
```

EXIT_CODE: 0 for `format`, 0 for `check`.

## Write-mode observation beyond the exit code

`csharpier format .` rewrites files and still exits 0, and its `Formatted N files` line is a
processed-file count rather than a rewrite count, so the exit code alone cannot distinguish a clean run
from a repairing one. The discriminating observation is the SHA-256 comparison:

| Measure | Value |
|---|---|
| files hashed before and after | 1658 (1657 tracked `*.cs` plus the untracked new test file) |
| `Formatted` line printed by the write-mode command | `Formatted 1624 files in 4919ms.` |
| **`$rewritten` (index-aligned hash differences)** | **0** |
| `Checked ` line printed by the read-only command | `Checked 1624 files in 4766ms.` |

`$rewritten` is 0, so the write-mode command changed nothing on this pass. `Get-FileHash -LiteralPath`
accepts a string array and preserves input order, so the two hash arrays are index-aligned and the loop
compared each file with itself.

The hash set is repository-wide rather than write-set-scoped, because this phase's restart rule is
triggered by a rewrite of **any** file and a write-set-scoped set could not detect one outside it.

The `Checked ` count of 1624 is one higher than the 1623 recorded at the [P0-T6] baseline, which is the
single new test file this feature adds. That corroborates that the read-only check saw the new file
rather than skipping it.

No restart of this phase was triggered, and no rewritten file lay outside the write set, because no file
was rewritten at all. Convention C4's rule that every `.cs`-touching task formats the paths it touched is
what makes a zero `$rewritten` the expected outcome here rather than a surprise.

## Anchored name-listing diff, excluding `.claude`

38 tracked paths: `.editorconfig`, `BannedSymbols.txt`, `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs`,
`UtilitiesCS.Test/UtilitiesCS.Test.csproj`, the 33 item-1 test files, and this feature's plan file.

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
UtilitiesCS.Test/Threading/AppGlobalsConverterTests.cs
UtilitiesCS.Test/Threading/AppGlobalsConverterTests_Unfinished.cs
UtilitiesCS.Test/UtilitiesCS.Test.csproj
UtilitiesCS/OutlookObjects/Table/OlTableExtensions.TableAccess.cs
VBFunctions.Test/ComputerInfo_Test.cs
docs/features/active/2026-09-08-console-out-aggressors-and-banned-symbol-promotion-826/plan.2026-09-08T23-52.md
```

## Porcelain companion span, excluding `.claude`

The companion lists the same 38 tracked paths as ` M`, plus 33 untracked entries: the new test file
`UtilitiesCS.Test/OutlookObjects/Table/OlTableExtensionsTimeoutDiagnosticsTests.cs` and 32 evidence
artifacts, every one of them under
`docs/features/active/2026-09-08-console-out-aggressors-and-banned-symbol-promotion-826/evidence/`.

The two spans are complementary: the anchored diff is blind to the untracked new test file, and the
porcelain status surfaces it. Every path in the union is inside the [P8-T1] allow-list. `spec.md` does
not yet appear, because the acceptance-criteria check-offs are Phase 8 work.

Output Summary: both exit codes are 0, the `check` output carries a line beginning with `Checked `, and
`$rewritten` is 0, proving the write-mode command changed nothing on this pass. Every path in the
name-listing diff and in the porcelain companion is inside the [P8-T1] allow-list. Toolchain step 1
passes; no restart is triggered.
