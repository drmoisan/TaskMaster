# ApplicationGlobals Constructor Census After Cycle 3 Changes

Timestamp: 2026-08-27T03-26-00Z

Command: `rg -n "new TaskMaster\.ApplicationGlobals\([^\n]*, true\)" UtilitiesCS.Test --glob "*.cs"`

EXIT_CODE: 1

Output Summary: No single-line eager two-argument call matched. Exit 1 is ripgrep's expected no-match result.

Command: `rg -n -C 20 "Constructor_WithoutLoadBasic_DoesNotMaterializeCollaboratorsUntilForceBasicLoad" TaskMaster.Test/AppGlobals/ApplicationGlobalsTests.cs`

EXIT_CODE: 0

Output Summary: The lazy-force caller uses the injected three-argument constructor with `loadBasic: false`, preserves the explicit force operation, and reads only the pure `C:\OneDrive` test value.

Command: `rg -n "ApplicationGlobals\("` over the eight in-scope test files.

EXIT_CODE: 0

Output Summary: The census found ten eager UtilitiesCS.Test constructor calls across seven files and the separate lazy-force TaskMaster.Test caller. Every affected eager call supplies a pure `OneDriveCommercial` reader, and the lazy-force caller supplies the same reader through the three-argument path. Unrelated non-eager constructor coverage in `ApplicationGlobalsTests.cs` remains unchanged.

## Exact-head failure mapping

- `TaskMaster.Test.AppGlobals.ApplicationGlobalsTests`: 1 failure mapped to the adapted lazy-force caller.
- `UtilitiesCS.Test.ReusableTypeClasses.SmartSerializableLoader_Tests`: 4 failures mapped to five adapted eager calls in the shared class, including the new deterministic regression.
- `UtilitiesCS.Test.NewtonsoftHelpers.ScoDictionaryConverterTests`: 4 failures mapped to its adapted initialization caller.
- `UtilitiesCS.Test.NewtonsoftHelpers.WrapperScoDictionaryTest`: 2 failures mapped to its adapted initialization caller.
- `UtilitiesCS.Test.NewtonsoftHelpers.WrapperScDictionaryTest`: 2 failures mapped to its adapted initialization caller.
- `UtilitiesCS.Test.NewtonsoftHelpers.PeopleScoConverter_Tests`: 3 failures mapped to its adapted initialization caller.
- `UtilitiesCS.Test.NewtonsoftHelpers.ScDictionaryConverter_Tests`: 5 failures mapped to its adapted initialization caller.
- `UtilitiesCS.Test.EmailIntelligence.PeopleScoDictionaryNew_Tests`: 1 failure mapped to its adapted constructor caller.

Mapped failures: 22 of 22. Unadapted affected eager callers: 0.
