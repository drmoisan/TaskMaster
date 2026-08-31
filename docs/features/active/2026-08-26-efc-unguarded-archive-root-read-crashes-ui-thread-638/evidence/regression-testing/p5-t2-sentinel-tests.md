# [P5-T2] Ordering-sentinel tests (Issue 638)

Timestamp: 2026-08-29T12-35

Command:

```
$vs = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe" | Select-Object -First 1
& $vs QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /Logger:trx /ResultsDirectory:TestResults\p5-t2 /TestCaseFilter:"FullyQualifiedName~OpenFolderMethods_DelegateToDataModelWithoutExternalServices|FullyQualifiedName~HandleMoveResult_WhenMoveFails_RoutesMessageThroughInjectedAction"
```

Same executable resolution and assembly as [P3-T15]. The `vstest.console.exe` path is
recorded unresolved because the resolved path is absolute.

EXIT_CODE: 0

Output Summary:

```
  Passed HandleMoveResult_WhenMoveFails_RoutesMessageThroughInjectedAction [133 ms]
  Passed OpenFolderMethods_DelegateToDataModelWithoutExternalServices [65 ms]
Total tests: 2
     Passed: 2
```

No `Failed:` summary line was emitted, so Failed: 0.

Both sentinels pass unmodified. Their significance:

- `OpenFolderMethods_DelegateToDataModelWithoutExternalServices`
  (`QuickFiler.Test/Controllers/EfcHomeControllerLifecycleTests.cs:207-218`) asserts
  `probe.FileSystem.SpecialFoldersAccessCount.Should().Be(2)` at `:217`. It still passes,
  which proves the archive-root guard was placed strictly **after** the OneDrive
  `SpecialFolders` read in both folder-open methods. Placing the guard first would have
  dropped that counter from 2 to 0 and additionally raised `NullReferenceException` from
  the probe's null `Ol` at `:388`, which a `catch (InvalidOperationException)` does not
  absorb.
- `HandleMoveResult_WhenMoveFails_RoutesMessageThroughInjectedAction`
  (`QuickFiler.Test/Controllers/EfcHomeControllerExecuteMovesTests.cs:160-175`) asserts the
  exact string `Cannot move to folderpath Archive/Target` at `:174`. It still passes,
  which proves the new `UserDiagnosticAction` seam did not disturb the existing
  `MoveFailureMessageAction` routing it is modelled on.
