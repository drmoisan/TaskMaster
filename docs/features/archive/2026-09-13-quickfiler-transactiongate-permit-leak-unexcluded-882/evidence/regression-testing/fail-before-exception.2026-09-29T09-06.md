# Fail-Before Exception Dossier, Compile Level (P1-T2, [expect-fail], D5, AC9)

Timestamp: 2026-09-29T09-06
Command: pwsh -NoProfile -Command 'Set-Location "<repo-root>"; $m = & (Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio/Installer/vswhere.exe") -latest -requires Microsoft.Component.MSBuild -find "MSBuild\**\Bin\MSBuild.exe" | Select-Object -First 1; & $m QuickFiler.Test\QuickFiler.Test.csproj /t:Build /m /p:Configuration=Debug /p:Platform=AnyCPU 2>&1 | Tee-Object -FilePath coverage/logs/fail-before-build.log | Out-Null; "EXIT=$LASTEXITCODE"; "CS1501-LINES=" + @(Select-String -LiteralPath coverage/logs/fail-before-build.log -Pattern "error CS1501").Count; Select-String -LiteralPath coverage/logs/fail-before-build.log -Pattern "Build succeeded|Build FAILED" | ForEach-Object { $_.Line.Trim() }'
EXIT_CODE: 1
ExpectedExitCode: 1
Output Summary:
- Build FAILED.
- CS1501-LINES=2 (one diagnostic; MSBuild prints it once inline and once in the closing error summary)
- Quoted diagnostic, location rewritten to the repository-relative path: `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs(418,68): error CS1501: No overload for method 'BeginTransactionAsync' takes 1 arguments [QuickFiler.Test/QuickFiler.Test.csproj]`
- Line 418 column 68 is the `UiThreadDispatcherFixture.BeginTransactionAsync(TimeSpan.Zero)` probe in the new test.
- Log retained at the gitignored path coverage/logs/fail-before-build.log.

WhyFailingRunImpossible: The `TimeSpan` overload of `UiThreadDispatcherFixture.BeginTransactionAsync` does not exist on the pre-fix tree, so the regression test cannot be compiled, loaded or executed before the fix. A runtime failing run is therefore structurally impossible; the compile failure above is the fail-before observation.

## Alternative proof

TEST-INSERTION-FACTS: (measured at 2026-09-29T09-06 immediately after the P1-T1 insertion, with C-7 and C-8 against QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs)
- TimeSpan.Zero = 1
- ThrowAsync<TimeoutException> = 1
- TRANSACTIONGATE_ACQUIRE_TIMEOUT = 2
- NotThrow<SemaphoreFullException> = 1
- BeGreaterThanOrEqualTo( = 1
- [TestMethod] = 8
- [Timeout(GateTimeoutMs)] = 8
- DoNotParallelize = 0
- Thread.Sleep = 0
- Task.Delay = 0
- Stopwatch = 0
- [Retry = 0
- BeginTransactionAsync_ZeroBoundWhileThisTestHoldsThePermit_ThrowsTimeoutExceptionAndReleasesNothing = 1
- LINES-FT=457 (greater than 396, at most 500)
- git diff --numstat 177b6d78e1b2408e5aedbd794cef3aad6b7fb372 -- QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs: `61	0` (61 added, 0 deleted; the seven pre-existing tests are untouched). The plain diff is kept at the gitignored path coverage/logs/p1-t1.diff.

ABSENCE-PROOF: the P0-T15 baseline (evidence/baseline/baseline-source-facts.md) recorded, in QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixture.cs, `BeginTransactionAsync(TimeSpan bound)`=0 and `TransactionGate.WaitAsync()`=1: the bounded overload is absent and the only acquisition is the unbounded parameterless wait.

SearchScope: docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/evidence/regression-testing/
SearchPatterns: fail-before-exception.*.md
SearchResult: fail-before-exception.2026-09-29T09-06.md
