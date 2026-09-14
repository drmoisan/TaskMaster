# Phase 2 — Fail-Before Harness Run (expect-fail)

Timestamp: 2026-09-14T11-26

This artifact was overwritten by the Revision R5 re-run of `[P2-T11]`, as that task directs. The
precondition was checked before the run: `[P1-T5]`'s copy of the superseded version 1.0 artifact exists
at
`docs/features/active/2026-09-13-deedle-netstandard-21-bind-unsatisfiable-in-production-879/evidence/regression-testing/expect-fail-run-superseded-probe-surface.2026-09-13T18-22.md`
(`P1T5_SUPERSEDED_COPY_PRESENT=True`), so the record of the vacuous pass that produced Defect 1 is not
destroyed. The Revision R2 run's own artifact, which recorded
`DEEDLE_RECORD_CONVERSION_OUTCOME=INVOKED-NO-EXCEPTION` and `Acceptance Condition: NOT MET`, was the
previous content of this path and is overwritten here; its observation is reproduced in the plan's
`## R6.1`, so no measurement is lost.

The build lock was acquired for item `879` before the spans below and released after them.

Command:

Span 1, pre-run TRX removal:

```
pwsh -NoProfile -Command '
$d = "TestResults/p2-expect-fail"
foreach ($f in @(Get-ChildItem -LiteralPath $d -Filter "*.trx" -Recurse -ErrorAction SilentlyContinue)) { Remove-Item -LiteralPath $f.FullName -Force }
Write-Output ("PRERUN_TRX_COUNT=" + @(Get-ChildItem -LiteralPath $d -Filter "*.trx" -Recurse -ErrorAction SilentlyContinue).Count)
'
```

Span 2, the harness run:

```
pwsh -NoProfile -Command '
$vswhere = Join-Path ${env:ProgramFiles(x86)} "Microsoft Visual Studio\Installer\vswhere.exe"
$vstest = @(& $vswhere -latest -products * -find "Common7\IDE\Extensions\TestPlatform\vstest.console.exe")[0]
& $vstest "TaskMaster.Test/bin/Debug/TaskMaster.Test.dll" /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation /TestCaseFilter:"FullyQualifiedName~TaskMaster.Test.Bootstrap" "/Logger:trx;LogFileName=p2-expect-fail.trx" /ResultsDirectory:TestResults/p2-expect-fail
$LASTEXITCODE
'
```

Span 3, per-test outcomes:

```
pwsh -NoProfile -Command '
$m = @(Get-ChildItem -LiteralPath "TestResults/p2-expect-fail" -Filter "p2-expect-fail.trx" -Recurse)
Write-Output ("TRX_MATCH_COUNT=" + $m.Count)
$trx = $m[0]
$x = [xml](Get-Content -LiteralPath $trx.FullName -Raw)
foreach ($r in @($x.TestRun.Results.UnitTestResult)) { Write-Output ($r.testName + " OUTCOME=" + $r.outcome) }
'
```

Span 4, failure class:

```
pwsh -NoProfile -Command '
$m = @(Get-ChildItem -LiteralPath "TestResults/p2-expect-fail" -Filter "p2-expect-fail.trx" -Recurse)
Write-Output ("TRX_MATCH_COUNT=" + $m.Count)
$trx = $m[0]
$x = [xml](Get-Content -LiteralPath $trx.FullName -Raw)
foreach ($r in @($x.TestRun.Results.UnitTestResult)) {
if ($r.testName -eq "AfterInstall_DeedleTypeInitializerSucceeds") {
foreach ($line in @(($r.Output.StdOut -split "`r?`n"))) {
if ($line.StartsWith("DEEDLE_RECORD_CONVERSION_OUTCOME=")) { Write-Output $line } } } }
'
```

Every payload additionally carries a leading `Set-Location` to the item worktree, because the executor
was launched without worktree isolation and pwsh would otherwise resolve every repository-relative path
in the coordinator session worktree. That statement changes no measurement.

EXIT_CODE: 1

ExpectedExitCode: 1

Output Summary:

```
PRERUN_TRX_COUNT=0
TRX_MATCH_COUNT=1
ChildDomain_HasNoAssemblyResolveHandlerBeforeInstall OUTCOME=Passed
AfterInstall_DeedleTypeInitializerSucceeds OUTCOME=Failed
AfterInstall_BothNetstandardVersionsBind OUTCOME=Failed
NegativeControl_HasNoUtilitiesCsAssemblyLoaded OUTCOME=Passed
NegativeControl_WithoutInstall_Netstandard21Throws OUTCOME=Passed
ChildDomain_IsRootedAtTheQuickFilerTestOutputDirectory OUTCOME=Passed
AppConfig_DeclaresNetstandardRedirect OUTCOME=Failed
ChildDomain_HasNoSvgControlAssemblyLoaded OUTCOME=Passed
NegativeControl_Netstandard20Observation_IsRecorded OUTCOME=Passed
ThisAddIn_HasExplicitStaticConstructor OUTCOME=Failed
ChildDomain_ConfigurationFileDeclaresNoNetstandardRedirect OUTCOME=Passed
DEEDLE_RECORD_CONVERSION_OUTCOME=NETSTANDARD-BIND-FAILURE:TypeInitializationException
```

The vstest console summary for the same run reads `Total tests: 11`, `Passed: 7`, `Failed: 4`, matching
the eleven `OUTCOME=` lines above.

Acceptance Condition: MET.

- `PRERUN_TRX_COUNT=0`, taken before the run. The results directory previously held two superseded TRX
  files, one of which already bore the pinned logger name and carried the vacuous outcome. Emptying the
  directory first is what makes the `TRX_MATCH_COUNT=1` below a measurement of this run rather than of a
  residue.
- `TRX_MATCH_COUNT=1`.
- `ChildDomain_IsRootedAtTheQuickFilerTestOutputDirectory OUTCOME=Passed`. This is the Revision R5
  addition and it is evaluated first. The child domain is rooted at the `QuickFiler.Test` build output
  directory, so the bind under test is reachable in the domain the other three observations describe.
  Two earlier fail-before attempts produced a success token against an unfixed build precisely because
  that was not true and nothing measured it.
- `AfterInstall_BothNetstandardVersionsBind OUTCOME=Failed`. The console message records
  `outcome21` as `FileNotFoundException` against an expected `LOADED`.
- `AfterInstall_DeedleTypeInitializerSucceeds OUTCOME=Failed`.
- Exactly one `DEEDLE_RECORD_CONVERSION_OUTCOME=` line, and its value is
  `NETSTANDARD-BIND-FAILURE:TypeInitializationException`, which begins with the
  `NETSTANDARD-BIND-FAILURE:` prefix. This is what makes the two `OUTCOME=Failed` conditions above it
  non-vacuous: the Deedle line failed because the `netstandard` bind is unsatisfiable, not because of an
  unrelated exception. Had the value begun with `OTHER-FAILURE:` this task would have halted and reported
  blocked.

The run's own exit code is `1`, recorded against `ExpectedExitCode: 1` and not itself a gate.

These are fail-before observations. They fail at runtime against the behaviour-empty installer supplied
by `[P2-T1]`, not at compile time: `[P2-T10]` recorded a clean build of the same tree.

Two further failures in this run, `ThisAddIn_HasExplicitStaticConstructor OUTCOME=Failed` and
`AppConfig_DeclaresNetstandardRedirect OUTCOME=Failed`, belong to the sibling class
`AddInEagerInstallShapeTests` created by `[P2-T8]`. They are discovered by this task's
`FullyQualifiedName~TaskMaster.Test.Bootstrap` filter and are recorded here for completeness. They are
also fail-before observations: the explicit static constructor is added by `[P3-T3]` and the
`netstandard` redirect by `[P3-T4]`, neither of which exists yet. `[P4-T7]` is the task that runs that
class after the fix. They are outside this task's acceptance condition and were not treated as evidence
about the bind.
