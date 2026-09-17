# P4-T3 — Full Class Pass-After Run (AC7 sibling clause)

Timestamp: 2026-09-17T02-27

Command: `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll "/Settings:scripts\vscode\TaskMaster.cli.runsettings" /InIsolation "/TestCaseFilter:FullyQualifiedName~ItemViewerBreadcrumbThreadAffinityTests" "/ResultsDirectory:TestResults\900\p4-t3" "/Logger:trx;LogFileName=p4-t3.trx"`
(vstest.console.exe resolved through vswhere); plus
`Get-FileHash -Algorithm SHA256 -LiteralPath scripts/vscode/TaskMaster.cli.runsettings`.

EXIT_CODE: 0

CHANNEL: COMMAND

## Output Summary

    COUNTERS total=7 executed=7 passed=7 failed=0
    RESULT ConfigureBreadcrumbDropDown_WorkerThread_ThrowsBoundaryDiagnostic = Passed
    RESULT InitializeBreadcrumbPipeline_OwningThreadDifferentPlainContext_DoesNotThrow = Passed
    RESULT ConfigureBreadcrumbDropDown_OwningThreadInsideDispatcherOperation_DoesNotThrow = Passed
    RESULT InitializeBreadcrumbPipeline_NullOwningDispatcher_DoesNotThrow = Passed
    RESULT InitializeBreadcrumbPipeline_ConstructedInsideDispatcherOperation_SucceedsUnderDifferentAmbientContext = Passed
    RESULT InitializeBreadcrumbPipeline_WorkerThread_ThrowsBoundaryDiagnostic = Passed
    RESULT InitializeBreadcrumbPipeline_OwningThreadNullAmbientContext_DoesNotThrow = Passed

RUNSETTINGS-HASH-NOW: 98EF03A8D3B0EBB2ED7A765E3B5E1B58E774D20202DF2F294C03A7260B9CEF57

## Acceptance

All four conditions hold.

- `EXIT_CODE: 0`.
- `total` 7, `passed` 7, `failed` 0.
- The seven `RESULT` names are exactly the seven `[TestMethod]` names recorded in the plan's fact 1,
  declared at pre-edit lines 39, 89, 129, 168, 204, 237 and 280. The set matches the seven names
  P0-T9 observed on the unfixed tree, so no test was added, removed or renamed by this change; the
  census independently holds `[TestMethod]` at 7.
- `RUNSETTINGS-HASH-NOW:` equals `RUNSETTINGS-HASH:` from P0-T4.

## Sibling regression check

The five tests other than the two under repair are the sibling population for AC7. All five pass:
the two owning-thread `DoesNotThrow` cases, the dispatcher-operation construction case, the
different-plain-context case, and the null-owning-dispatcher case that remains out of scope and
still uses the original `Task.Run` shape. The change added a private static helper and rewrote two
method bodies; it altered no shared helper contract, which is why no sibling assumption was
invalidated.

The run order above is the TRX order, which differs between runs under class-level parallel
execution and is not expected to match declaration order.

## Build lock

This task ran inside a held shared build lock for item 900, acquired before the test run and
released immediately after it completed.
