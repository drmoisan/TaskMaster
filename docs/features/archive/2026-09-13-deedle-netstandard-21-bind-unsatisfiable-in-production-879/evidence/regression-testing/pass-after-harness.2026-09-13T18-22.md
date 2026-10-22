# Pass-After Child-Domain Harness — [P4-T3]

Timestamp: 2026-09-14T11-37

Command:

```
pwsh -NoProfile -Command '
Set-Location -LiteralPath "<repo-root>"
$d = "TestResults/p4-harness"
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
& $vstest "TaskMaster.Test/bin/Debug/TaskMaster.Test.dll" /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~TaskMaster.Test.Bootstrap" "/Logger:trx;LogFileName=p4-harness.trx" /ResultsDirectory:TestResults/p4-harness
$LASTEXITCODE
'
```

```
pwsh -NoProfile -Command '& ([scriptblock]::Create((Get-Content -Raw "<build-lock-root>/release.txt"))) -Item "879"'
```

```
pwsh -NoProfile -Command '
Set-Location -LiteralPath "<repo-root>"
$m = @(Get-ChildItem -LiteralPath "TestResults/p4-harness" -Filter "p4-harness.trx" -Recurse)
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
AfterInstall_BothNetstandardVersionsBind OUTCOME=Passed
NegativeControl_WithoutInstall_Netstandard21Throws OUTCOME=Passed
ThisAddIn_HasExplicitStaticConstructor OUTCOME=Passed
ChildDomain_IsRootedAtTheQuickFilerTestOutputDirectory OUTCOME=Passed
ChildDomain_ConfigurationFileDeclaresNoNetstandardRedirect OUTCOME=Passed
AppConfig_DeclaresNetstandardRedirect OUTCOME=Passed
AfterInstall_DeedleTypeInitializerSucceeds OUTCOME=Passed
NegativeControl_HasNoUtilitiesCsAssemblyLoaded OUTCOME=Passed
ChildDomain_HasNoSvgControlAssemblyLoaded OUTCOME=Passed
NegativeControl_Netstandard20Observation_IsRecorded OUTCOME=Passed
ChildDomain_HasNoAssemblyResolveHandlerBeforeInstall OUTCOME=Passed
```

The run's `/TestCaseFilter:"FullyQualifiedName~TaskMaster.Test.Bootstrap"` also discovers
`AddInEagerInstallShapeTests`, which `[P2-T8]` declares in that namespace, so the counters read 11 rather
than the harness class's own nine. All nine harness method names named in `[P2-T6]` are present above with
`OUTCOME=Passed`, including the Revision R5 addition
`ChildDomain_IsRootedAtTheQuickFilerTestOutputDirectory`.

Acceptance Condition: MET. `EXIT_CODE: 0`; `PRERUN_TRX_COUNT=0` taken before the run;
`TRX_MATCH_COUNT=1`; `Counters` `failed` is `0`; all nine `[P2-T6]` method names record `OUTCOME=Passed`.

## Deedle Record Conversion Outcome:

Appended by `[P4-T6]` at 2026-09-14T11-38. `[P4-T6]` deliberately carries no pre-run TRX removal span,
because it reads the TRX that `[P4-T3]` wrote earlier in this same phase.

Command:

```
pwsh -NoProfile -Command '
Set-Location -LiteralPath "<repo-root>"
$m = @(Get-ChildItem -LiteralPath "TestResults/p4-harness" -Filter "p4-harness.trx" -Recurse)
Write-Output ("TRX_MATCH_COUNT=" + $m.Count)
$trx = $m[0]
$x = [xml](Get-Content -LiteralPath $trx.FullName -Raw)
foreach ($r in @($x.TestRun.Results.UnitTestResult)) {
if ($r.testName -eq "AfterInstall_DeedleTypeInitializerSucceeds") {
foreach ($line in @(($r.Output.StdOut -split "`r?`n"))) {
if ($line.StartsWith("DEEDLE_RECORD_CONVERSION_OUTCOME=")) { Write-Output $line } } } }
'
```

EXIT_CODE: 0

```
TRX_MATCH_COUNT=1
DEEDLE_RECORD_CONVERSION_OUTCOME=INVOKED-NO-EXCEPTION
```

Acceptance Condition: MET. `TRX_MATCH_COUNT=1`; the `[P4-T3]` outcome list above records
`AfterInstall_DeedleTypeInitializerSucceeds OUTCOME=Passed` and
`ChildDomain_IsRootedAtTheQuickFilerTestOutputDirectory OUTCOME=Passed`; the recorded output above carries
exactly one outcome line and its value is exactly `INVOKED-NO-EXCEPTION`. The test outcome is neither
`NotExecuted` nor `Inconclusive`, and the value does not begin with `OTHER-FAILURE:`.

### Fail-before / pass-after pair for AC10

The fail-before reading is at line 91 of
`evidence/regression-testing/expect-fail-run.2026-09-13T18-22.md`, against a build carrying no fix, and its
value there is `NETSTANDARD-BIND-FAILURE:TypeInitializationException`. The pass-after reading is the single
outcome line recorded above, against the build produced by `[P4-T1]` from the Phase 3 fix, and its value is
the completion token.

Both readings come from a child domain rooted at the `QuickFiler.Test` build output directory. That is what
makes them comparable, and it is asserted in both runs by
`ChildDomain_IsRootedAtTheQuickFilerTestOutputDirectory`, which records `OUTCOME=Passed` in the `[P2-T11]`
artifact at line 85 and in this artifact above.
