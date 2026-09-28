# P4-T2 — Measured Pass-After Run of the Rewritten Pair (AC6)

Timestamp: 2026-09-17T02-26

Command: `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /InIsolation "/TestCaseFilter:FullyQualifiedName~InitializeBreadcrumbPipeline_WorkerThread_ThrowsBoundaryDiagnostic|FullyQualifiedName~ConfigureBreadcrumbDropDown_WorkerThread_ThrowsBoundaryDiagnostic" "/ResultsDirectory:TestResults\900\p4-t2" "/Logger:trx;LogFileName=p4-t2.trx"`
(vstest.console.exe resolved through vswhere); plus
`Get-FileHash -Algorithm SHA256 -LiteralPath scripts/vscode/TaskMaster.cli.runsettings`.

EXIT_CODE: 0

CHANNEL: COMMAND

## Output Summary

    COUNTERS total=2 executed=2 passed=2 failed=0
    RESULT InitializeBreadcrumbPipeline_WorkerThread_ThrowsBoundaryDiagnostic = Passed
    RESULT ConfigureBreadcrumbDropDown_WorkerThread_ThrowsBoundaryDiagnostic = Passed

RUNSETTINGS-HASH-NOW: 98EF03A8D3B0EBB2ED7A765E3B5E1B58E774D20202DF2F294C03A7260B9CEF57

## Acceptance

All four conditions hold.

- `EXIT_CODE: 0`.
- `total` 2, `passed` 2, `failed` 0.
- Both `RESULT` lines read `Passed`.
- `RUNSETTINGS-HASH-NOW:` equals `RUNSETTINGS-HASH:` from P0-T4,
  `98EF03A8D3B0EBB2ED7A765E3B5E1B58E774D20202DF2F294C03A7260B9CEF57`, proving the run used the
  unchanged CLI runsettings.

## Why the runsettings hash is part of this gate

The run settings file configures `Workers=0` and `Scope=ClassLevel`, that is, full class-level
parallel execution with one worker per logical processor. AC6 requires the rewritten tests to pass
under those settings rather than serially, so a result obtained after quietly relaxing the settings
would satisfy the letter of a pass-after run while removing the condition the item exists to survive.
Comparing the file's hash against the anchor taken before any task edited anything is what makes the
settings a measured input rather than an assumption. The hash was taken with the same
`Get-FileHash -Algorithm SHA256 -LiteralPath` form as the anchor.

No test in this file is serialised, carries `[DoNotParallelize]`, is retried, or is given a timing
tolerance; the census gates `Thread.Sleep`, `Task.Delay`, `[Timeout` and `DoNotParallelize` at 0.

## Agreement with the confirming run

P2-T4 ran the same pair against the pre-mutation build and recorded the same result: `total` 2,
`passed` 2, both `Passed`. There is no test that passed there and fails here, so the `BLOCKED`
disagreement branch was not reached. The two runs differ in the binary they loaded: P2-T4 ran the
P2-T3 build, and this run loaded the P4-T1 rebuild made from the source after both mutation reverts.
Agreement between them is the evidence that the reverts restored the tested behaviour and not merely
the file bytes.

This is the measured run for AC6. P4-T3 confirms it over the whole class.

## Build lock

This task ran inside a held shared build lock for item 900, acquired before the test run and
released immediately after it completed.
