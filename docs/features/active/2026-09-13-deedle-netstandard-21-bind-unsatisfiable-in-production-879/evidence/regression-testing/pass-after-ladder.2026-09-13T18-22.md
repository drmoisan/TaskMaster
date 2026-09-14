# Pass-After Ladder Unit Tests — [P4-T2]

Timestamp: 2026-09-14T11-36

Command:

```
pwsh -NoProfile -Command '
Set-Location -LiteralPath "<repo-root>"
$d = "TestResults/p4-ladder"
foreach ($f in @(Get-ChildItem -LiteralPath $d -Filter "*.trx" -Recurse -ErrorAction SilentlyContinue)) { Remove-Item -LiteralPath $f.FullName -Force }
Write-Output ("PRERUN_TRX_COUNT=" + @(Get-ChildItem -LiteralPath $d -Filter "*.trx" -Recurse -ErrorAction SilentlyContinue).Count)
'
```

```
pwsh -NoProfile -Command '& ([scriptblock]::Create((Get-Content -Raw "<build-lock-root>/acquire.txt"))) -Item "879"'
```

```
pwsh -NoProfile -Command '
Set-Location -LiteralPath "<repo-root>"
$vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"
$vstest = @(& $vswhere -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe")[0]
& $vstest "UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll" /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~UtilitiesCS.Test.Bootstrap" "/Logger:trx;LogFileName=p4-ladder.trx" /ResultsDirectory:TestResults/p4-ladder
$LASTEXITCODE
'
```

```
pwsh -NoProfile -Command '& ([scriptblock]::Create((Get-Content -Raw "<build-lock-root>/release.txt"))) -Item "879"'
```

```
pwsh -NoProfile -Command '
Set-Location -LiteralPath "<repo-root>"
$m = @(Get-ChildItem -LiteralPath "TestResults/p4-ladder" -Filter "p4-ladder.trx" -Recurse)
Write-Output ("TRX_MATCH_COUNT=" + $m.Count)
$trx = $m[0]
$x = [xml](Get-Content -LiteralPath $trx.FullName -Raw)
Write-Output ("RESULT_SUMMARY_OUTCOME=" + $x.TestRun.ResultSummary.outcome)
Write-Output ("COUNTERS_TOTAL=" + $x.TestRun.ResultSummary.Counters.total)
Write-Output ("COUNTERS_PASSED=" + $x.TestRun.ResultSummary.Counters.passed)
Write-Output ("COUNTERS_FAILED=" + $x.TestRun.ResultSummary.Counters.failed)
foreach ($r in @($x.TestRun.Results.UnitTestResult)) { Write-Output ($r.testName + " OUTCOME=" + $r.outcome) }
'
```

The `Set-Location` prefix is mechanically necessary and measurement-neutral: this executor was launched
without worktree isolation, so its inherited working directory is a different checkout and every
repository-relative path in the plan would otherwise resolve into the wrong tree. It is disclosed here
rather than omitted.

EXIT_CODE: 0

ExpectedExitCode: 0

Output Summary:

```
PRERUN_TRX_COUNT=0
TRX_MATCH_COUNT=1
RESULT_SUMMARY_OUTCOME=Completed
COUNTERS_TOTAL=11
COUNTERS_PASSED=11
COUNTERS_FAILED=0
Resolve_WhenAlreadyResolvingTheSameSimpleName_ReturnsNull OUTCOME=Passed
Resolve_WhenNoRungApplies_ReturnsNullWithoutThrowing OUTCOME=Passed
Resolve_WhenDisplayNameRungMisses_LoadsFacadeFromRuntimeDirectory OUTCOME=Passed
Install_ThenLoadOfUnresolvableName_LeavesTheLoadFailingWithoutHandlerThrowing OUTCOME=Passed
Resolve_WhenRequestedTokenIsNull_DoesNotMatchStronglyNamedLoadedAssembly OUTCOME=Passed
Install_CalledTwice_AttachesExactlyOneHandler OUTCOME=Passed
Resolve_WhenNothingLoaded_AsksFullDisplayNameAtFacadeVersion OUTCOME=Passed
Resolve_WhenAlreadyLoadedMatches_ReturnsItWithoutCallingLoadByDisplayName OUTCOME=Passed
Resolve_WhenRequestedTokenIsEmpty_DoesNotMatchStronglyNamedLoadedAssembly OUTCOME=Passed
Resolve_WhenARungThrows_AbsorbsAndContinuesToTheNextRung OUTCOME=Passed
Resolve_WhenLoadedTokenDiffers_DoesNotReturnTheLoadedAssembly OUTCOME=Passed
```

Acceptance Condition: MET. `EXIT_CODE: 0`; `PRERUN_TRX_COUNT=0` taken before the run; `TRX_MATCH_COUNT=1`;
`ResultSummary` `outcome` is `Completed`; `Counters` `failed` is `0`; `Counters` `passed` is 11, which
meets the floor of at least 11.

`[P3-T1]` and `[P3-T2]` take their acceptance from this run. `[P3-T1]`'s ladder is exercised by the eight
rung tests above; `[P3-T2]`'s idempotence obligation is measured by
`Install_CalledTwice_AttachesExactlyOneHandler` and its never-throws obligation by
`Install_ThenLoadOfUnresolvableName_LeavesTheLoadFailingWithoutHandlerThrowing`, both `Passed`.
