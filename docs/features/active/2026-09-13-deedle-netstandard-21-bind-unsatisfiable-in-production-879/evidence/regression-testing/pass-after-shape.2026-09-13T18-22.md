# Pass-After Add-In Eager-Install Shape Tests — [P4-T7]

Timestamp: 2026-09-14T11-40

Command:

```
pwsh -NoProfile -Command '
Set-Location -LiteralPath "<repo-root>"
$d = "TestResults/p4-shape"
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
& $vstest "TaskMaster.Test/bin/Debug/TaskMaster.Test.dll" /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~AddInEagerInstallShapeTests" "/Logger:trx;LogFileName=p4-shape.trx" /ResultsDirectory:TestResults/p4-shape
$LASTEXITCODE
'
```

```
pwsh -NoProfile -Command '& ([scriptblock]::Create((Get-Content -Raw "<build-lock-root>/release.txt"))) -Item "879"'
```

```
pwsh -NoProfile -Command '
Set-Location -LiteralPath "<repo-root>"
$m = @(Get-ChildItem -LiteralPath "TestResults/p4-shape" -Filter "p4-shape.trx" -Recurse)
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
COUNTERS_TOTAL=2
COUNTERS_PASSED=2
COUNTERS_FAILED=0
ThisAddIn_HasExplicitStaticConstructor OUTCOME=Passed
AppConfig_DeclaresNetstandardRedirect OUTCOME=Passed
```

`AppConfig_DeclaresNetstandardRedirect` reads the deployed image `TaskMaster.dll.config` from the parent
domain's base directory, which is `TaskMaster.Test/bin/Debug`. Its pass is therefore evidence that the
`[P3-T4]` hardening reached the build output and not only the source `TaskMaster/app.config`.

Acceptance Condition: MET. `EXIT_CODE: 0`; `PRERUN_TRX_COUNT=0` taken before the run;
`TRX_MATCH_COUNT=1`; `ThisAddIn_HasExplicitStaticConstructor OUTCOME=Passed` and
`AppConfig_DeclaresNetstandardRedirect OUTCOME=Passed`.
