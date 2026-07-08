# PeopleScoDictionaryNewTests Class After Fix — Finding B no-regression (Cycle 5, Issue #181)

Timestamp: 2026-06-08T21-53

Command: `vstest.console.exe "ToDoModel.Test\bin\Debug\ToDoModel.Test.dll" /InIsolation /TestCaseFilter:"FullyQualifiedName~PeopleScoDictionaryNewTests"`
(VS18 vstest.console.exe; MSYS_NO_PATHCONV=1. `/InIsolation` required per the people-deserialize-after-fix note to apply Moq/STTE binding redirects.)

EXIT_CODE: 0

Output Summary:
- Total tests: 2. Passed: 2. Failed: 0.
- The class has exactly two active `[TestMethod]` members (the remaining methods are commented out in source): `People_Deserialize_CanDeserializePatternCorrectly` and `People_DeserializeShortcut_CanDeserializePatternCorrectly`. Both PASS.
- No other serialization consumer in this class is regressed by the `WrapperScoDictionary.ToDerived()` change. The fix is additive (a fallback branch that only runs when the existing reflective Config lookup returns null), so the typed-RemainingObject and shortcut paths are unaffected.
