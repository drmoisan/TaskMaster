# Baseline Target Tests — [expect-fail] (Cycle 5, Issue #181)

Timestamp: 2026-06-08T21-53

Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll "ToDoModel.Test\bin\Debug\ToDoModel.Test.dll" /EnableCodeCoverage /Tests:FromSeed_ShouldBuildFileNameFromParts,CalcMaxSeedLength_WhenInitialized_ShouldSubtractComponentLengths,People_Deserialize_CanDeserializePatternCorrectly,Consume_WhenSequenceProvided_ReturnsItemsAndReportsProgress`
(Invoked via VS18 `vstest.console.exe`; `MSYS_NO_PATHCONV=1` for path args; a `/Logger:trx` rerun produced `trx-target/baseline-target.trx`.)

EXIT_CODE: 1

Output Summary (fail-before evidence):
- Total tests: 4. Passed: 1. Failed: 3.
- FAILED `FromSeed_ShouldBuildFileNameFromParts` (Finding A): `Expected fph.FolderPath to be "C:\data" with a length of 7, but "C:\" has a length of 3, differs near "\" (index 2).` Confirms the seed-constructor terminal `FilePath = Path.Combine(...)` re-entry corrupting FolderPath from `C:\data` to `C:\`.
- FAILED `CalcMaxSeedLength_WhenInitialized_ShouldSubtractComponentLengths` (Finding A): `Expected result to be 239, but found 245 (difference of 6).` The 6-char difference equals the lost `data` (4) plus separator collapse; the corrupted shorter FolderPath (`C:\` length 3 vs `C:\output` length 9 expected) inflates `MAX_PATH - FolderPath.Length - ...`. Same root cause as test 1.
- FAILED `People_Deserialize_CanDeserializePatternCorrectly` (Finding B): see `Finding B Observed Failure` section below.
- PASSED `Consume_WhenSequenceProvided_ReturnsItemsAndReportsProgress` (Finding C) in this run AND in a second confirming run. This test is flaky-by-construction (wall-clock `System.Threading.Timer` dependent) and passes under isolated/light load; it does NOT reliably fail in a single run. A fail-before-exception dossier is recorded at `evidence/regression-testing/fail-before-exception.2026-06-08T21-53.md` per evidence conventions.

## Finding B Observed Failure

Test: `People_Deserialize_CanDeserializePatternCorrectly`
(`ToDoModel.Test/Data Model/People/PeopleScoDictionaryNewTests.cs` line 258)

Observed `Config.Disk.FileName` value at assertion failure: empty string `""` (string length 0).

Assertion-failure message (MSTest Assert.AreEqual):
```
Assert.AreEqual failed. Expected string length 11 but was 0. 'expected' expression: '"pplkey.json"', 'actual' expression: 'people.Config.Disk.FileName'.
Expected: "pplkey.json"
But was:  ""
-----------^
```

Stack (top frames):
```
Microsoft.VisualStudio.TestTools.UnitTesting.Assert.ThrowAssertAreEqualFailed(...)
Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual[T](...)
ToDoModel.Tests.Data_Model.People.PeopleScoDictionaryNewTests.People_Deserialize_CanDeserializePatternCorrectly()
  in ...\ToDoModel.Test\Data Model\People\PeopleScoDictionaryNewTests.cs:line 258
```

Interpretation: `people` and `people.Config` are non-null (the two prior `Assert.IsNotNull` checks pass), but `people.Config.Disk.FileName` is the empty default. This is consistent with the Finding B diagnosis: under `TypeNameHandling.None`, `WrapperScoDictionary.RemainingObject` binds to an untyped `JObject`, `WrapperScoDictionary.ToDerived()`'s reflective `Config` lookup returns null, and `derivedInstance.Config` is left at its default `new NewSmartSerializableConfig()` whose `Disk.FileName == ""`. The serialization layer, not `FilePathHelper`, is the defect site.

Coverage attachment for this run: `trx-target/3286cd60-1b8a-42ee-91ec-5000f55372a4/...coverage` (binary; full repo-wide coverage headline captured in `baseline-full-suite.2026-06-08T21-53.md`).
