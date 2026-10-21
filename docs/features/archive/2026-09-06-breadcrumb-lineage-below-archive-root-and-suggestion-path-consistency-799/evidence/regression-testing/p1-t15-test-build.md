# [P1-T15] Test build — the new and retargeted Phase 1 tests compile

Timestamp: 2026-09-07T07-13

Command: `msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`

EXIT_CODE: 0

ExpectedExitCode: 0

## Output Summary

MSBuild summary tail:

```
    0 Warning(s)
    0 Error(s)
```

Build succeeded with 0 warnings and 0 errors, proving every new test compiles against the Phase 1
seams. The five test files added or retargeted by [P1-T6] through [P1-T14] are in the build via the
[P1-T10] and [P1-T12] Compile Include entries.

Files proved to compile by this run:

- UtilitiesCS.Test/OutlookObjects/Folder/ArchiveStemProjectionTests.cs (new)
- UtilitiesCS.Test/OutlookObjects/Folder/ArchiveChainProjectionTests.cs (new)
- UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderHierarchyProviderTrimTests.cs (new)
- UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorRecentsProjectionTests.cs (new)
- QuickFiler.Test/Controllers/BreadcrumbBridgeRouterScoreJoinTests.cs (new)
- UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderHierarchyProviderTests.cs (retargeted)
- QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.Part2.cs (retargeted)

## Language-version observation

Zero errors on QuickFiler.Test also confirms the [P1-T11] C# 7.3 authoring constraint was honoured.
That project declares no `LangVersion` element and targets v4.8.1, so it compiles at the 7.3
default; a construct mirrored from a UtilitiesCS.Test file would have surfaced here as CS8370.

## Command form (R10)

`/t:Build` with no `/p:` gate switches. This build exists to produce test assemblies, not to run
gates. R10 reserves `/t:Rebuild` with the gate switches for [P3-T3] and [P3-T4].

## Environment note

MSBuild is not on this machine's PATH. The invoking shell prepended the Visual Studio 18 Community
MSBuild `Current\Bin\amd64` directory before invoking the command. The command text itself is
character-for-character the form this task specifies.

## Path hygiene (R3)

No absolute host path, host account name, or machine name appears in this artifact. The raw MSBuild
log was written outside the repository to a session scratch location and is not committed.
