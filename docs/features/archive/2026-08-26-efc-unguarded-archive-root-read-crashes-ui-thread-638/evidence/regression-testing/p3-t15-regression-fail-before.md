# [P3-T15] `[expect-fail]` regression run before the fix (Issue 638)

Timestamp: 2026-08-29T12-32

Command:

```
$vs = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1
& $vs QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /Logger:trx /ResultsDirectory:TestResults\p3-t15 /TestCaseFilter:"FullyQualifiedName~EfcDataModelArchiveRootTests"
```

The `vstest.console.exe` path is recorded unresolved, as the vswhere expression, because
the resolved path is absolute. `QuickFiler.Test\bin\Debug\QuickFiler.Test.dll` is the
Debug|AnyCPU `<OutputPath>` at `QuickFiler.Test/QuickFiler.Test.csproj:36`. The run has its
own `/ResultsDirectory` so its TRX cannot collide with the Phase 5 and Phase 6 runs.

EXIT_CODE: 1

ExpectedExitCode: 1

Output Summary:

This is the `[expect-fail]` task. A failing run is the expected and required outcome here,
because the archive-root guard has not been written yet; Phase 4 adds it and [P5-T1]
re-runs the same eleven tests and requires them all to pass.

## Counts

```
Total tests: 11
     Passed: 6
     Failed: 5
```

## The five failing tests

Exactly the five the plan names, and no others:

1. `MoveToFolderAsync_WhenArchiveRootIsUnresolvable_ReturnsFalseInsteadOfThrowing`
2. `MoveToFolderAsync_WhenArchiveRootIsCrossStoreUnresolvable_ReturnsFalseInsteadOfThrowing`
3. `OpenOlFolderAsync_WhenArchiveRootIsUnresolvable_ReportsAndReturns`
4. `OpenFsFolderAsync_WhenArchiveRootIsUnresolvable_ReportsAndReturns`
5. `ArchiveRootFailureDiagnostic_DoesNotContainTheArchivePathOrMailboxAddress`

Each of the five failure messages names `InvalidOperationException`. A
`Select-String -SimpleMatch 'InvalidOperationException'` over the run output returned
exactly **5** matches, one per failing test. Each message takes the form:

```
Test method QuickFiler.Test.Controllers.EfcDataModelArchiveRootTests.<test name> threw exception: System.InvalidOperationException: ...
```

The failures are therefore genuine runtime failures caused by the unguarded archive-root
read escaping the method, not build failures — [P3-T14] recorded `0 Error(s)` immediately
before this run. Stack traces are deliberately not reproduced: their frames carry absolute
source paths, which no artifact this plan writes may contain.

## The six tests that already pass

These pin behavior that is correct before the fix and are expected to pass in this run,
which they did:

- `MoveToFolderAsync_WhenArchiveRootResolves_StillReadsItOnce`
- `MoveToFolderAsync_WhenMailInfoIsNull_ReturnsFalseWithoutReadingArchiveRoot`
- `MoveToFolderAsync_WhenOneDriveIsMissing_ReturnsFalseWithoutReadingArchiveRoot`
- `MoveToFolderAsync_WhenArchiveRootThrowsComException_StillPropagates`
- `OpenOlFolderAsync_WhenOneDriveIsMissing_ReturnsWithoutReadingArchiveRoot`
- `OpenFsFolderAsync_WhenOneDriveIsMissing_ReturnsWithoutReadingArchiveRoot`
