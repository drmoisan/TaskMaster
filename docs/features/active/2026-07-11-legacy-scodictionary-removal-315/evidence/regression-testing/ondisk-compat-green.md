# Regression — On-Disk JSON Compatibility Tests Green (post-change)

Timestamp: 2026-07-11T12-00
Test class: `UtilitiesCS.Test/ReusableTypeClasses/SerializableNew/ScoDictionaryNew_OnDiskCompatibility_Tests.cs` (out of scope; left unchanged)
Command: `vstest.console.exe "...\UtilitiesCS.Test\bin\Debug\UtilitiesCS.Test.dll" /InIsolation /TestCaseFilter:"FullyQualifiedName~ScoDictionaryNew_OnDiskCompatibility"`
EXIT_CODE: 0

Total tests: 5
Passed: 5
Failed: 0

Per-method results (all Passed):
- DictRemap_FlatOnDiskPayload_RoundTripsWithoutWrapperTokens
- FilteredFolderScraping_FlatOnDiskPayload_RoundTripsWithoutWrapperTokens
- FolderRemap_FlatOnDiskPayload_RoundTripsWithoutWrapperTokens
- SubjectMapEncoder_FlatOnDiskPayload_RoundTripsWithoutWrapperTokens
- DefaultWritePath_ForAllPersistedTypes_NeverEmitsGlobalsWrapperTokens

Conclusion: The authoritative persisted-dictionary compatibility coverage remains green after the ScoDictionary removal and SmartSerializable test retargeting. AC4 satisfied.
