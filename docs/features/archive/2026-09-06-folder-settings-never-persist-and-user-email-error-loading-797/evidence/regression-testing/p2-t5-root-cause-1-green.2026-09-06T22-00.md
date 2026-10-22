# P2-T5 — Root Cause 1 Automated Criteria Are Green (Issue #797, AC1, AC2, AC4)

Timestamp: 2026-09-07T09-43

Commands:

1. `msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"` — exit 0.
2. `pwsh -NoProfile -File coverage/plan797-helpers.ps1 -Mode Test -FilterName RootCause1 -ResultsDirectory coverage/plan797-trx/p2`

EXIT_CODE: 0

Failed count: 0.

## PASS-AFTER correspondence

One line per test name that appeared as a `FAIL-BEFORE:` entry for AC1, AC2 or AC4 in the P1-T18
artifact. All four are now passing.

- PASS-AFTER: TaskMaster.Test.AppGlobals.AppOlObjectsCoverageTests.LoadStoresAsync_WhenConfigDeserializesToNull_FreshWrapperAdoptsLoaderDiskConfiguration
- PASS-AFTER: UtilitiesCS.Test.ReusableTypeClasses.SmartSerializableSerializeGuardTests.Serialize_WithEmptyDiskPath_LogsErrorAndArmsNoTimer
- PASS-AFTER: UtilitiesCS.Test.ReusableTypeClasses.SmartSerializableSerializeGuardTests.Serialize_WithNullDiskPath_LogsErrorAndArmsNoTimer
- PASS-AFTER: UtilitiesCS.Test.ReusableTypeClasses.SmartSerializableSerializeGuardTests.SerializeNow_WithConfiguredPath_WritesWithoutFiringTimer

## Run totals, recorded as non-asserted observations

Total tests 11, passed 11, failed 0, elapsed 4.73 seconds. The scope selects the two classes that
carry the AC1, AC2 and AC4 tests, so it also re-runs the pre-existing tests in those classes; all of
them passed, including the AC1 key-absent negative case, the AC4 deferred-path-unchanged case, and the
five pre-existing store-loading tests.

## Filter expression used

```text
(FullyQualifiedName~AppOlObjectsCoverageTests&TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests)|(FullyQualifiedName~SmartSerializableSerializeGuardTests&TestCategory!=LiveOutlook)
```

The four shell-icon test classes named in that expression are excluded from every local run in this
plan for environmental reasons unrelated to this change. CI covers them.

## Implementation recorded by this evidence

- AC1 lands only in TaskMaster/AppGlobals/AppOlObjects.StoreLoading.cs: the fresh-build branch now
  applies the already-resolved loader configuration to the freshly built wrapper with a deep copy.
  The shared deserialize overload in
  UtilitiesCS/ReusableTypeClasses/NewSmartSerializable/SmartSerializable.cs is unchanged, and
  UtilitiesCS/ReusableTypeClasses/NewSmartSerializable/SmartSerializableBase.cs is untouched.
- AC2 replaces the empty-string comparison with a null-or-empty check that logs at error level,
  naming the serialized item type reported by `typeof` over the type parameter together with the
  rejected path, and arms no timer on the rejecting path.
- AC4 replaces the placeholder body of the explicit-save entry point with the guarded synchronous
  flush. The AC2 guard is evaluated first, so the fix does not substitute one silent failure for
  another; only a non-empty, non-null path reaches the existing thread-safe write method.

Output Summary: The root-cause-1 scope is green. Exit code 0, 11 of 11 passing, and all four AC1, AC2
and AC4 fail-before tests now pass.
