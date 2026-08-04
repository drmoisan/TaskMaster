# Phase 9 Formatter-Stabilization Pre-Edit Ledger

- Timestamp: `2026-07-23T11:58:41Z`
- Command: `$base=(git merge-base HEAD origin/main).Trim(); $patterns=@('QuickFiler/**/*.cs','QuickFiler.Test/**/*.cs','UtilitiesCS/**/*.cs','UtilitiesCS.Test/**/*.cs'); $spam='UtilitiesCS/EmailIntelligence/ClassifierGroups/SpamBayes/SpamBayes.Actions.cs'; $authorized=@(@(git diff --name-only --diff-filter=ACMR $base -- $patterns)+@(git ls-files --others --exclude-standard -- $patterns)|Sort-Object -Unique|Where-Object {$_ -ne $spam}); hash the ordinal LF-joined authorized paths; Get-FileHash -Algorithm SHA256 coverage.config,.csharpierignore,$spam,QuickFiler.Test/Viewers/BreadcrumbPopupControlDispatchTests.cs; inspect physical lines, MSTest attributes, test names, FluentAssertions lines, Breadcrumb production-seam tokens, ExceptionRecorder references, and Tuple<string,SynchronizationContext> references`
- EXIT_CODE: `0`
- Output Summary: `P8_T20_LEDGER_OK authorized=62 path_hash=E2439D9F8A28D97A05EA3EEFB3201587904CC784FCB9EF7200632F6BEED3EBCD test_file_lines=480 test_methods=11 data_rows=3 discovered_cases=13 assertion_lines=44 production_seam_tokens=5 protected_changes=0`

## Authorized and Protected Scope

| Item | Value |
|---|---|
| Live merge base | `df5ad49c909f6b739edef45d0336151f44e827a6` |
| Authorized issue-#400 C# paths | `62` |
| Ordinally sorted LF-joined path-set SHA-256 | `E2439D9F8A28D97A05EA3EEFB3201587904CC784FCB9EF7200632F6BEED3EBCD` |
| `coverage.config` SHA-256 | `B9CD80356C6BDBE03807A0B8CB106AE03D24EFBDBB2515097FBF003099050943` |
| `.csharpierignore` SHA-256 | `362211A82C6C6887E023B1B1936408715D6C35CC42E0D91B46E8E76A29318C25` |
| Unrelated committed `SpamBayes.Actions.cs` SHA-256 | `99AFDEB968CD88ED657807E17CD1EE804D0043AEF3879E4D30C2259ED73945DA` |

All required values exactly match the authorized plan. No protected file changed.

## Test-File Semantic Ledger

| Measurement | Pre-edit value |
|---|---|
| File | `QuickFiler.Test/Viewers/BreadcrumbPopupControlDispatchTests.cs` |
| SHA-256 | `F21541FDB8F60D2F9123A6D4D471B2B5DB97FD55DA975BD326942F40EB294991` |
| Physical lines | `480` |
| `[TestMethod]` methods | `10` |
| `[DataTestMethod]` methods | `1` |
| `[DataRow]` rows | `3` |
| Expected discovered cases | `13` |
| Ordered test-name SHA-256 | `DFCD8BB714DB88473F702E9E8122F15BCF4EB8B749F5A0CE9F36321DD2266981` |
| Trimmed `.Should()` assertion-line count | `44` |
| Ordered trimmed assertion-line SHA-256 | `0FA3A31B15FE6825B716DEB28E0CFAE58CE8014891AA6BA901FDD0ABD2034BEC` |
| Unique `Breadcrumb*` production-seam token count | `5` |
| Sorted production-seam-token SHA-256 | `48DA4538877099D3B0D59D7CD26BE2E9CAC24F905D3D6E34E4F06BD79DA34D82` |
| `ExceptionRecorder` references | `7` |
| `Tuple<string, SynchronizationContext>` references | `3` |

The eleven ordered test methods are:

1. `SurfaceFactory_WorkerCompletion_DispatchesEveryStageAndCleanup`
2. `SurfaceFactory_InitializationFailure_ReportsOnceAndCleansUp`
3. `SurfaceFactory_NavigationActionFailure_ReportsOnceAndCleansUp`
4. `SurfaceFactory_ReadinessFailure_ReportsOnceThenDisposesSurface`
5. `Readiness_DisposeFromAmbientNullWorker_DispatchesHandlerDetachment`
6. `Readiness_DetachSchedulingFailure_ReportsOnceWithoutDirectDetach`
7. `DisposeSurfaceAsync_MessengerFailure_StillDisposesControlAndReportsOnce`
8. `CreateAndInstall_CancellationCleanupFailure_RetriesOnlyFailedResource`
9. `CreateAndInstall_StaleHostCleanup_DoesNotDisposeOwnedControlDirectly`
10. `DirectAdapters_CreateGuardAndReportThroughOwnedBoundary`
11. `SurfaceFactory_InvalidNavigationResult_ReportsOnceAndCleansUp`

The five production-seam tokens are `BreadcrumbNavigationReadiness`, `BreadcrumbPopupControlDispatchTests`, `BreadcrumbPopupUiOperations`, `BreadcrumbUiDispatcher`, and `BreadcrumbWebViewSurfaceFactory`. The test-class token is retained in the token hash so the exact pre/post lexical inventory can be compared without a special-case filter.

## Authorized Transformation

Only the following representation-preserving substitutions are permitted:

- Replace all seven empty-wrapper type/construction references with `ConcurrentQueue<Exception>`.
- Remove the empty `ExceptionRecorder : ConcurrentQueue<Exception>` declaration.
- Add `using OperationEntry = System.Tuple<string, System.Threading.SynchronizationContext>;`.
- Replace the three exact `Tuple<string, SynchronizationContext>` operation-record type references with `OperationEntry`.

The alias denotes the identical constructed `Tuple` type. The empty queue subclass has no members and is not referenced outside this file. CSharpier 1.3.0 read-only simulation predicts 479 physical lines after this exact transformation. No test, assertion, production seam, project include, public API, dependency, synchronization operation, or exception observation is authorized to change.
