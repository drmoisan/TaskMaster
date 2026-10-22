Timestamp: 2026-08-27T03-17-00Z
Command: `gh run view 33034033583 --job 98392718650 --log-failed`
EXIT_CODE: 0
Output Summary: Exact-head CI executed 6,586 tests: 6,564 passed and 22 failed. Every failure reached the same OneDrive-root exception through `ResolveOneDriveRoot -> LoadFolders -> AppFileSystemFolderPaths..ctor -> ApplicationGlobals.LoadBasicMethod`.

Redacted failure census:

- `TaskMaster.Test.AppGlobals.ApplicationGlobalsTests`: 1
  - `Constructor_WithoutLoadBasic_DoesNotMaterializeCollaboratorsUntilForceBasicLoad`
- `UtilitiesCS.Test.ReusableTypeClasses.SmartSerializableLoader_Tests`: 4
- `UtilitiesCS.Test.NewtonsoftHelpers.ScoDictionaryConverterTests`: 4
- `UtilitiesCS.Test.NewtonsoftHelpers.WrapperScoDictionaryTest`: 2
- `UtilitiesCS.Test.NewtonsoftHelpers.WrapperScDictionaryTest`: 2
- `UtilitiesCS.Test.NewtonsoftHelpers.PeopleScoConverter_Tests`: 3
- `UtilitiesCS.Test.NewtonsoftHelpers.ScDictionaryConverter_Tests`: 5
- `UtilitiesCS.Test.EmailIntelligence.PeopleScoDictionaryNew_Tests`: 1

Total mapped failures: 22. The log contained 22 `ApplicationGlobals.LoadBasicMethod` stack frames. Host, account, runner-workspace, and user-profile paths were intentionally omitted.
