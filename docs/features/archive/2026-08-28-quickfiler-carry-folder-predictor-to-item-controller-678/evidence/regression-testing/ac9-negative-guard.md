# P1-T8 — AC9 negative guard test

Timestamp: 2026-09-01T23-08

Test added: `LoadFolderHandlerAsync_WhenCarriedHandlerPresentAndVarListProvided_InvokesPredictorFactory`
in `QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.Part2.cs`.

## Preceding build (Derivation D7)

Command: `msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"`
EXIT_CODE: 0

## Scoped run (Derivation D7)

Command:

```
vstest.console.exe QuickFiler.Test/bin/Debug/QuickFiler.Test.dll
  /Settings:scripts/vscode/TaskMaster.cli.runsettings
  /InIsolation
  /TestCaseFilter:TestCategory!=LiveOutlook&FullyQualifiedName~LoadFolderHandlerAsync_WhenCarriedHandlerPresentAndVarListProvided_InvokesPredictorFactory
  /Logger:trx
  /ResultsDirectory:TestResults\p1-t8
```

EXIT_CODE: 0

Output Summary:

```
  Passed LoadFolderHandlerAsync_WhenCarriedHandlerPresentAndVarListProvided_InvokesPredictorFactory [187 ms]
Test Run Successful.
Total tests: 1
     Passed: 1
 Total time: 1.2570 Seconds
```

## Acceptance conditions

### 1. The test arranges both a carried handler and a non-null `varList`, and asserts the sentinel-throwing `_folderPredictorFactory` IS invoked

Arrange, in order:

- a `FolderController` harness instance and a mocked `IApplicationGlobals` in `_globals`;
- the sentinel-throwing Moq delegate mock injected into `_folderPredictorFactory` by reflection,
  built by the shared `BuildThrowingPredictorFactoryMock` helper this file already used for the AC16
  test, so the two tests exercise the same seam through the same mechanism;
- a `Mock<IFolderSearchHandler>().Object` injected into `_carriedFolderHandler` — the carried handler
  IS present;
- `object varList = new[] { "search-term" }` — non-null.

Act: `controller.LoadFolderHandlerAsync(CancellationToken.None, varList)`.

Assert, two ways so the test cannot pass vacuously:

- `await act.Should().ThrowAsync<InvalidOperationException>()` — the sentinel fired, which is only
  possible if the factory was invoked;
- `VerifyFactoryTimes(factory, Times.Once(), ...)` — a Moq `Times.Once()` verification on the same
  delegate mock. This is the exact mirror of the AC16 test's `Times.Never()`, so the pair
  distinguishes the two branches rather than merely observing one of them.

### 2. Exactly 1 test discovered and 1 passed

`Total tests: 1`, `Passed: 1`, recorded above. The single-discovery figure is the control that
distinguishes a real pass from a filter that matched nothing.

### 3. MSTest, Moq and FluentAssertions; no temporary file; no live Outlook COM

- MSTest: `[TestMethod]` from `Microsoft.VisualStudio.TestTools.UnitTesting`.
- Moq: `Mock<Func<...>>` for the predictor-construction seam, `Mock<IApplicationGlobals>` and
  `Mock<IFolderSearchHandler>`.
- FluentAssertions: `act.Should().ThrowAsync<InvalidOperationException>(...)`.
- No file of any kind is created or read by the test.
- No Outlook COM object is constructed. The run carries `/TestCaseFilter:TestCategory!=LiveOutlook`,
  and the test declares no such category, so it is in the headless set by construction.

## Why this test is the right negative guard for AC9

AC9 requires that the `FromArrayOrString` branches stay unchanged and that a carried handler is
never adopted on a `FromArrayOrString` call. A test that only asserted the branch's behaviour with
no carried handler present would pass against an implementation that adopts unconditionally, because
there would be nothing to adopt. This test supplies a carried handler and then requires the factory
to be invoked anyway, so an adoption placed before the `varList is null` test — the plausible
implementation error — fails it.

## TRX handling

The TRX was written under `TestResults\p1-t8\`, which is git-ignored (`.gitignore:39`), and is
referenced here by results directory only. No absolute host path, account name or machine name is
recorded in this artifact.
