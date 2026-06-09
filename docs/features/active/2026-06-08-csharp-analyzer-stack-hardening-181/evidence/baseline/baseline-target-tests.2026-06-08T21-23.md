# Baseline Target Tests — [expect-fail] (Cycle 4, Issue #181)

Timestamp: 2026-06-08T21-23

Command: `vstest.console.exe UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll "ToDoModel.Test\bin\Debug\ToDoModel.Test.dll" /EnableCodeCoverage /Tests:FromSeed_ShouldBuildFileNameFromParts,CalcMaxSeedLength_WhenInitialized_ShouldSubtractComponentLengths,People_Deserialize_CanDeserializePatternCorrectly,Consume_WhenSequenceProvided_ReturnsItemsAndReportsProgress`
(A supplemental single-test run of `FromSeed_ShouldBuildFileNameFromParts` was executed to capture its discrete failure message.)

EXIT_CODE: 1

Output Summary (4-test run): Total tests: 4; Passed: 1; Failed: 3; Test Run Failed.

Per-test fail-before status:

1. `FromSeed_ShouldBuildFileNameFromParts` — FAILED.
   - Error: `Expected fph.FolderPath to be "C:\data" with a length of 7, but "C:\" has a length of 3, differs near "\" (index 2).`
   - Root cause (Finding A): terminal `FilePath = Path.Combine(_folderPath, _fileName)` in the seed constructor re-enters the FilePath handler with `_fileName == ""`, combining to the folder (`C:\data`), then the FilePath case splits it back into `_folderPath = "C:\"` and `_fileName = "data"`, corrupting FolderPath.

2. `CalcMaxSeedLength_WhenInitialized_ShouldSubtractComponentLengths` — FAILED.
   - Error: `Expected result to be 239, but found 245 (difference of 6).`
   - Root cause (Finding A): corrupted `FolderPath` length (`C:\` = 3 instead of `C:\output` = 9) shifts the `MAX_PATH - FolderPath.Length - ...` subtraction by 6.

3. `People_Deserialize_CanDeserializePatternCorrectly` — FAILED. (See `Finding B Observed Failure` section below.)

4. `Consume_WhenSequenceProvided_ReturnsItemsAndReportsProgress` — PASSED in this run (non-deterministic / wall-clock-timer dependent). The `System.Threading.Timer` fired at least once within the 1-second SpinWait window during this run, so `tracker.Reports.Count >= 2` was satisfied opportunistically. This test is flaky-by-construction (Finding C) and is not reliably fail-before in a single run; a fail-before exception dossier is recorded under `evidence/regression-testing/fail-before-exception.2026-06-08T21-23.md` per the evidence conventions.

---

## Finding B Observed Failure

Test: `People_Deserialize_CanDeserializePatternCorrectly`
Source: `ToDoModel.Test/Data Model/People/PeopleScoDictionaryNewTests.cs:258`

Observed `Config.Disk.FileName` value at assertion: empty string `""` (string length 0).

Assertion-failure message (MSTest `Assert.AreEqual`):
```
Assert.AreEqual failed. Expected string length 11 but was 0. 'expected' expression: '"pplkey.json"', 'actual' expression: 'people.Config.Disk.FileName'.
Expected: "pplkey.json"
But was:  ""
```

Stack trace:
```
at Microsoft.VisualStudio.TestTools.UnitTesting.Assert.ThrowAssertFailed(String assertionName, String message)
at Microsoft.VisualStudio.TestTools.UnitTesting.Assert.ThrowAssertAreEqualFailed(Object expected, Object actual, String userMessage)
at Microsoft.VisualStudio.TestTools.UnitTesting.Assert.AreEqual[T](T expected, T actual, String message, String expectedExpression, String actualExpression)
at ToDoModel.Tests.Data_Model.People.PeopleScoDictionaryNewTests.People_Deserialize_CanDeserializePatternCorrectly() in C:\Users\DanMoisan\repos\TaskMaster-wt-2026-06-08-12-10\ToDoModel.Test\Data Model\People\PeopleScoDictionaryNewTests.cs:line 258
```

Interpretation: deserialization routes through `FilePathHelperConverter.ReadJson` -> `new FilePathHelper(fileName, folderPath)` with `fileName == "pplkey.json"`. The observed empty `FileName` confirms the direct `(fileName, folderPath)` constructor's terminal `FilePath = Path.Combine(...)` line (or the property-set ordering it triggers) clears/overwrites `_fileName` after it is set. This grounds the [P1-T2] diagnosis in the actual runtime failure rather than the assumed mechanism: the empty-`FileName` outcome must be eliminated by correcting the constructor so `FileName` survives initialization.
