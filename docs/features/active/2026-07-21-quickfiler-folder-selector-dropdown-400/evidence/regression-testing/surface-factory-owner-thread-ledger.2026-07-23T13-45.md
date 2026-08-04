# Surface factory owner-thread pre-edit ledger

- Timestamp: `2026-07-23T13-45Z`
- Command: `Get-FileHash QuickFiler.Test/Viewers/BreadcrumbPopupControlDispatchTests.cs; inspect MSTest attributes, DataRow attributes, FluentAssertions calls, static members, production-seam tokens, physical lines, the StringComparer.OrdinalIgnoreCase issue path set, and protected-file hashes`
- EXIT_CODE: `0`
- Output Summary: `13 cases, 52 Should calls across 44 assertion lines, 479 physical lines, six outer static helper members, no static fixture state, 62 authorized C# paths, and all protected hashes matched.`

## Source baseline

| Measurement | Value |
|---|---|
| File | `QuickFiler.Test/Viewers/BreadcrumbPopupControlDispatchTests.cs` |
| SHA-256 | `A59DDA03D17572E9597B9146AD1E84AF8FE7A919DE5A7B611DBEDB38E9B9B356` |
| Physical lines | `479` |
| `[TestMethod]` methods | `10` |
| `[DataTestMethod]` methods | `1` |
| `[DataRow]` rows | `3` |
| Expected discovered cases | `13` |
| `.Should()` calls | `52` |
| Assertion lines | `44` |
| Ordered test-name SHA-256 | `DFCD8BB714DB88473F702E9E8122F15BCF4EB8B749F5A0CE9F36321DD2266981` |
| Ordered assertion-line SHA-256 | `0FA3A31B15FE6825B716DEB28E0CFAE58CE8014891AA6BA901FDD0ABD2034BEC` |
| Sorted production-seam-token SHA-256 | `48DA4538877099D3B0D59D7CD26BE2E9CAC24F905D3D6E34E4F06BD79DA34D82` |

The ordered test methods are:

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

The production-seam tokens are `BreadcrumbNavigationReadiness`,
`BreadcrumbPopupControlDispatchTests`, `BreadcrumbPopupUiOperations`,
`BreadcrumbUiDispatcher`, and `BreadcrumbWebViewSurfaceFactory`.

## Static-member inventory

`SurfaceFactoryFixture` has no static members. Its five state properties are instance
properties. The outer test class contains these six stateless helper members:

- `Operations`
- `Factory`
- `VerifyCreateAndInstallCleanupAsync`
- `CaptureFailure`
- `Uninitialized`
- `NewCompletionSource`

The correction will move fixture-specific behavior to instance methods and will not
introduce mutable static state.

## Authorized and protected scope

| Item | Value |
|---|---|
| HEAD | `1dd7e44aaa3689fb2b74326cec397157302585d8` |
| Merge base | `df5ad49c909f6b739edef45d0336151f44e827a6` |
| Authorized issue-#400 C# paths | `62` |
| `StringComparer.OrdinalIgnoreCase` LF-joined path-set SHA-256 | `E2439D9F8A28D97A05EA3EEFB3201587904CC784FCB9EF7200632F6BEED3EBCD` |
| `coverage.config` SHA-256 | `B9CD80356C6BDBE03807A0B8CB106AE03D24EFBDBB2515097FBF003099050943` |
| `.csharpierignore` SHA-256 | `362211A82C6C6887E023B1B1936408715D6C35CC42E0D91B46E8E76A29318C25` |
| `SpamBayes.Actions.cs` SHA-256 | `99AFDEB968CD88ED657807E17CD1EE804D0043AEF3879E4D30C2259ED73945DA` |

The authorized implementation scope is exactly the existing test file. No production,
project, package, configuration, runsettings, filter, threshold, or exclusion change is
authorized by this batch.
