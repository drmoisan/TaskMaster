# [P6-T3] Final QA — Analyzers

- **Issue:** #438
- **Task:** [P6-T3]
- **Timestamp:** 2026-08-08T11-41

## Command

`pwsh -NoProfile -Command "& msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform='Any CPU' /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true ; exit $LASTEXITCODE"`

(`/v:m` appended for a readable log; verbosity does not alter diagnostics.)

- **EXIT_CODE:** 0

## Diagnostics

- **Errors: 0**
- **Warnings: 6**, all pre-existing and none attributable to this change:

| Count | Warning | Projects | Status |
|---|---|---|---|
| 5 | `The project contains a packages.config file, which is not supported by System.Reactive v7.0 or later.` | `UtilitiesCS`, `ToDoModel`, `QuickFiler`, `TaskMaster`, `UtilitiesCS.Test` | identical to the P0-T5 baseline (5 warnings) |
| 1 | `CS2002: Source file '...\UtilitiesCS.Test\OutlookObjects\Folder\PercentageFormatterTests.cs' specified multiple times` | `UtilitiesCS.Test` | **pre-existing, out of scope** — see below |

### CS2002 provenance (not introduced by #438)

The duplicate `<Compile Include>` exists in the committed project file at baseline HEAD `904b4c38dba0f9f41707c3c0f077e123c78de59c`:

```
$ git show 904b4c38...:UtilitiesCS.Test/UtilitiesCS.Test.csproj | Select-String "PercentageFormatterTests"
<Compile Include="OutlookObjects\Folder\PercentageFormatterTests.cs" />
<Compile Include="OutlookObjects\Folder\PercentageFormatterTests.cs" />
```

Two occurrences at HEAD, and the same two occurrences in the working copy (lines 304 and 356). The full diff of `UtilitiesCS.Test.csproj` under this change adds exactly two lines:

```diff
+    <Compile Include="OutlookObjects\Folder\FolderBreadcrumbBridgeRouterReplaceItemsTests.cs" />
+    <Compile Include="OutlookObjects\Folder\BreadcrumbSelectionSessionHighlightTests.cs" />
```

Neither is a duplicate. The warning did not appear in the P0-T5 baseline capture only because that incremental build did not recompile `UtilitiesCS.Test`; the final build does, so the latent warning surfaces. It is a pre-existing project-file defect, unrelated to the folder-search path, and is **not** repaired here because doing so is outside this plan's scope. It is recorded for follow-up promotion as its own issue.

## Result

- **Output Summary:** Solution-wide analyzer build succeeded with EXIT_CODE 0 and **zero errors**. Six warnings were emitted: the five pre-existing System.Reactive packages.config advisories carried over unchanged from the baseline, plus one pre-existing `CS2002` duplicate-`Compile` warning in `UtilitiesCS.Test.csproj` proven present at baseline HEAD and untouched by this change. No warning originates in any file added or modified by #438. Accept criteria met.
