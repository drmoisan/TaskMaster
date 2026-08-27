# Cycle 2 partial-revert pass-after evidence

Timestamp: 2026-08-26T22-20

## Build

Command: `& "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`

Result: PASS (`EXIT_CODE: 0`). The solution built with 0 errors and the five previously recorded `System.Reactive` `packages.config` warnings.

## Focused regression run

Command: `& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation "/TestCaseFilter:FullyQualifiedName~EfcSelectionGuardTests|FullyQualifiedName~BreadcrumbBridgeRouterIssue439Tests|FullyQualifiedName~BreadcrumbBridgeRouterIssue614Tests|FullyQualifiedName~BreadcrumbBridgeRouterTests" "/Logger:trx;LogFileName=p2-t5.trx" "/ResultsDirectory:coverage\trx\p2-t5"`

Result: PASS (`EXIT_CODE: 0`). All 60 filtered tests passed with 0 failures. The run included 25 `EfcSelectionGuardTests` and passed each required boundary assertion:

- `IsValidFilingSelection_RootedTargetUnderArchiveRoot_IsRejected`
- `IsValidFilingSelection_ArchiveRootExactTarget_IsRejected`
- `Issue614_GuardAcceptedSelection_DoesNotThrowAtFilingBoundary`
- `IsValidFilingSelection_TwoCharacterRelativeStem_IsAccepted`
- `IsValidFilingSelection_SingleCharacterRelativeStem_IsAccepted`
- `Issue439AlreadyRootedTargetRemainsUnchangedWithCaseInsensitiveArchiveMatch`

The four filtered test classes completed without failures.
