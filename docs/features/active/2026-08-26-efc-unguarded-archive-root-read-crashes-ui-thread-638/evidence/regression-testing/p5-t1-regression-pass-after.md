# [P5-T1] Regression run after the fix (Issue 638)

Timestamp: 2026-08-29T12-35

Command:

```
$vs = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1
& $vs QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /Logger:trx /ResultsDirectory:TestResults\p5-t1 /TestCaseFilter:"FullyQualifiedName~EfcDataModelArchiveRootTests"
```

Identical to [P3-T15] in executable resolution, assembly and filter; only the
`/ResultsDirectory` differs, so the TRX cannot collide with the [P3-T15] or Phase 6 runs.
The `vstest.console.exe` path is recorded unresolved because the resolved path is absolute.

EXIT_CODE: 0

Output Summary:

## Counts

```
Total tests: 11
     Passed: 11
```

No `Failed:` summary line was emitted, so Failed: 0.

## The eleven test names, all passing

1. `MoveToFolderAsync_WhenArchiveRootIsUnresolvable_ReturnsFalseInsteadOfThrowing`
2. `MoveToFolderAsync_WhenArchiveRootIsCrossStoreUnresolvable_ReturnsFalseInsteadOfThrowing`
3. `OpenOlFolderAsync_WhenArchiveRootIsUnresolvable_ReportsAndReturns`
4. `OpenFsFolderAsync_WhenArchiveRootIsUnresolvable_ReportsAndReturns`
5. `ArchiveRootFailureDiagnostic_DoesNotContainTheArchivePathOrMailboxAddress`
6. `MoveToFolderAsync_WhenArchiveRootResolves_StillReadsItOnce`
7. `MoveToFolderAsync_WhenMailInfoIsNull_ReturnsFalseWithoutReadingArchiveRoot`
8. `MoveToFolderAsync_WhenOneDriveIsMissing_ReturnsFalseWithoutReadingArchiveRoot`
9. `MoveToFolderAsync_WhenArchiveRootThrowsComException_StillPropagates`
10. `OpenOlFolderAsync_WhenOneDriveIsMissing_ReturnsWithoutReadingArchiveRoot`
11. `OpenFsFolderAsync_WhenOneDriveIsMissing_ReturnsWithoutReadingArchiveRoot`

The five tests that [P3-T15] recorded failing with `InvalidOperationException` now pass,
and the six that already passed before the fix still pass. Together with
`p3-t15-regression-fail-before.md` this is the fail-before / pass-after pair.
