Timestamp: 2026-08-27T03-18-00Z
Command: `rg -n "new TaskMaster\.ApplicationGlobals\([^\n]*, true\)" UtilitiesCS.Test --glob "*.cs"`
EXIT_CODE: 0
Output Summary: Ten eager two-argument constructor calls were found across seven UtilitiesCS.Test files. A separate TaskMaster.Test caller uses the one-argument constructor and explicitly forces the lazy basic load.

Eager call sites:

1. `UtilitiesCS.Test/NewtonsoftHelpers/PeopleScoConverter_Tests.cs:26`
2. `UtilitiesCS.Test/NewtonsoftHelpers/ScDictionaryConverter_Tests.cs:24`
3. `UtilitiesCS.Test/NewtonsoftHelpers/ScoDictionaryConverterTests.cs:28`
4. `UtilitiesCS.Test/NewtonsoftHelpers/WrapperScoDictionaryTest.cs:105`
5. `UtilitiesCS.Test/NewtonsoftHelpers/WrapperScDictionaryTest.cs:105`
6. `UtilitiesCS.Test/ReusableTypeClasses/SmartSerializableLoader_Tests.cs:32`
7. `UtilitiesCS.Test/ReusableTypeClasses/SmartSerializableLoader_Tests.cs:98`
8. `UtilitiesCS.Test/ReusableTypeClasses/SmartSerializableLoader_Tests.cs:116`
9. `UtilitiesCS.Test/ReusableTypeClasses/SmartSerializableLoader_Tests.cs:133`
10. `UtilitiesCS.Test/EmailIntelligence/PeopleScoDictionaryNew_Tests.cs:27`

Separate lazy-force caller:

- `TaskMaster.Test/AppGlobals/ApplicationGlobalsTests.cs:45-51` constructs `new ApplicationGlobals(application)`; the test then forces the private lazy-load path by reflection while asserting the before/after collaborator state.

Second command: `rg -n -C 20 "Constructor_WithoutLoadBasic_DoesNotMaterializeCollaboratorsUntilForceBasicLoad" TaskMaster.Test/AppGlobals/ApplicationGlobalsTests.cs`
Second command exit code: 0
