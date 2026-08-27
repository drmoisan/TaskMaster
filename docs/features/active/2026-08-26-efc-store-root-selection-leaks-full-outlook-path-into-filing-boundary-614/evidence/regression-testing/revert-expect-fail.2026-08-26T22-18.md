# Partial-Revert Fail-Before Proof — remediation cycle 2

Timestamp: 2026-08-26T22-18

Command: `pwsh -NoProfile -Command '& "C:\Program Files\Microsoft Visual Studio\18\Community\MSBuild\Current\Bin\MSBuild.exe" TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"'`

Test Command: `pwsh -NoProfile -Command '$vstest = Join-Path (& "C:\Program Files (x86)\Microsoft Visual Studio\Installer\vswhere.exe" -latest -property installationPath) "Common7\IDE\Extensions\TestPlatform\vstest.console.exe"; & $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation "/TestCaseFilter:FullyQualifiedName~EfcSelectionGuardTests" "/Logger:trx;LogFileName=p1-t2.trx" "/ResultsDirectory:coverage\trx\p1-t2"; exit $LASTEXITCODE'`

EXIT_CODE: 1

ExpectedExitCode: 1

Output Summary:

- Build exit code: 0; build succeeded with 0 errors and the 5 pre-existing System.Reactive warnings.
- Test result: 30 total, 27 passed, 3 failed.
- The only failed tests were
  `IsValidFilingSelection_RootedTargetUnderArchiveRoot_IsRejected`,
  `IsValidFilingSelection_ArchiveRootExactTarget_IsRejected`, and
  `Issue614_GuardAcceptedSelection_DoesNotThrowAtFilingBoundary`.
- Both inverted guard tests found `true` where strict rooted rejection expects `false`.
- The composition test failed because `EmailFilerConfig.ResolvePaths` propagated the expected
  `ArgumentException` for an accepted rooted `DestinationOlStem` at the D4 boundary.
- Every other `EfcSelectionGuardTests` test passed.

Verdict: expected fail-before result confirmed.
