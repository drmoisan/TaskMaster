# [P3-T2] CSharpier read-only check

Timestamp: 2026-09-07T07-51

Command: dotnet tool run csharpier check .

EXIT_CODE: 0

ExpectedExitCode: 0

## Verbatim printed line

```
Checked 1601 files in 6748ms.
```

FINAL-CSHARPIER-CHECKED-FILES: 1601

## Delta against the [P0-T8] baseline

BASELINE-CSHARPIER-CHECKED-FILES: 1593
FINAL-CSHARPIER-CHECKED-FILES: 1601
DELTA: 8

The expected observation is a delta of 8, and 8 is what was observed. The eight new `.cs` files this plan adds are
three production files —
`UtilitiesCS/OutlookObjects/Folder/ArchiveStemProjection.cs`,
`UtilitiesCS/OutlookObjects/Folder/ArchiveChainProjection.cs` and
`QuickFiler/Controllers/QfcItemController.BreadcrumbWiring.cs` — and five test files —
`UtilitiesCS.Test/OutlookObjects/Folder/ArchiveStemProjectionTests.cs`,
`UtilitiesCS.Test/OutlookObjects/Folder/ArchiveChainProjectionTests.cs`,
`UtilitiesCS.Test/OutlookObjects/Folder/OutlookFolderHierarchyProviderTrimTests.cs`,
`UtilitiesCS.Test/OutlookObjects/Folder/FolderPredictorRecentsProjectionTests.cs` and
`QuickFiler.Test/Controllers/BreadcrumbBridgeRouterScoreJoinTests.cs`.
No other value needed explanation. The four modified project files are excluded from the checked set by
`.csharpierignore` line 12, and every evidence artifact this plan writes is excluded by its line 4, so neither
category contributes to the count.

Output Summary: The read-only check exited 0 and printed the single success-case line `Checked 1601 files in
6748ms.` with no drift entry, so the tree is formatting-clean after the [P3-T1] pass. The exit code is the gate
here because `check` is read-only and returns non-zero on drift. The checked-file count is the [P0-T8] baseline of
1593 plus exactly the eight new `.cs` files this plan adds.

## Path hygiene (R3)

No absolute host path, host account name, or machine name appears in this artifact.
