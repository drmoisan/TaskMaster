# P4-T7 — New-module line coverage (AC13)

Timestamp: 2026-09-01T20-17
Command: the inline PowerShell expression reproduced verbatim below, evaluated against `docs/features/active/2026-08-28-qfc-initializewebviewasync-fault-is-unobserved-670/evidence/qa-gates/postchange.cobertura.xml`
EXIT_CODE: 0

## Expression, verbatim

    [xml]$c = Get-Content -LiteralPath 'docs/features/active/2026-08-28-qfc-initializewebviewasync-fault-is-unobserved-670/evidence/qa-gates/postchange.cobertura.xml'
    $rows = $c.SelectNodes('//class/lines/line') | Where-Object { $_.ParentNode.ParentNode.GetAttribute('filename') -like '*QfcItemController.WebViewFaultBoundary.cs' } | ForEach-Object { [pscustomobject]@{ Num = [int]$_.GetAttribute('number'); Hits = [int]$_.GetAttribute('hits') } }
    $g = $rows | Group-Object Num
    $valid = $g.Count
    $covered = ($g | Where-Object { ($_.Group | Measure-Object -Property Hits -Maximum).Maximum -gt 0 }).Count
    '{0} covered / {1} valid = {2:N4}%' -f $covered, $valid, (100 * $covered / $valid)

Printed result:

    12 covered / 13 valid = 92.3077%

## Derived values — AC13 PASSES

    NEWFILE_LINES_COVERED = 12
    NEWFILE_LINES_VALID   = 13
    NEWFILE_LINE_PERCENT  = 92.3077

**92.3077% is at or above the required 90%, so AC13 passes.**

The denominator is non-zero, which the task requires as a precondition: a zero denominator would mean the file was never instrumented and the percentage would be meaningless rather than passing. The file is recorded in the document under the repository-relative filename `QuickFiler\Controllers\QfcItemController.WebViewFaultBoundary.cs`, and P4-T6 separately confirmed the transition from 0 `class` nodes at baseline to 1 post-change.

## Covered and uncovered lines

    COVERED_LINES   = 17, 25, 27, 28, 30, 31, 34, 35, 36, 37, 38, 39
    UNCOVERED_LINES = 29

Exactly one line is uncovered. It is line 29:

    26            try
    27            {
    28                await InitializeWebViewAsync();
    29            }            <-- uncovered
    30            catch (OperationCanceledException)
    31            {

Line 29 is the closing brace of the `try` block, which is the **normal-completion path** of `await InitializeWebViewAsync()` — the sequence point reached only when that call returns without throwing.

That path is not reachable in a unit test, and the spec states this plainly rather than promising it away. A successful `InitializeWebViewAsync` requires a live CoreWebView2 runtime, which is an external process barred by the unit-test policy. Under the mocked `IWebViewCoreInitializer` the seam raises `WebViewSentinelException` at its first call, so execution always leaves the `try` block through a `catch` arm and never falls off its end. Even a mock returning completed tasks would not reach line 29: execution would proceed to `((ItemViewer)_itemViewer).L0v2h2_WebView2.CoreWebView2`, which is null without the real runtime, producing a `NullReferenceException` that the guard itself catches.

This is a genuine, stated limitation of the measurement, not a disposition applied to make a threshold pass. The threshold passes with the limitation included in the denominator: no line was excluded from measurement, and no `[ExcludeFromCodeCoverage]` attribute was added to this file or to any member of it.

## What covers the remaining 12 lines

- **Line 17** is the sink's default initializer lambda, covered by `WebViewInitializationErrorSink_DefaultDelegate_InvokesWithoutThrowing`, which exercises the default delegate body rather than substituting a test double.
- **Lines 25, 27, 28** are the guard's entry and the awaited call, reached by every test that invokes the guard.
- **Lines 30, 31, 34** are the `catch (OperationCanceledException)` arm, covered by `InitializeWebViewGuardedAsync_WhenTheTokenIsAlreadyCanceled_DoesNotInvokeTheSink`. This is the test the plan elected specifically because without it that arm would be the only uncovered region and AC13 would not be reachable; the measurement confirms the election was load-bearing.
- **Lines 35 through 39** are the `catch (Exception ex)` arm and the sink invocation, covered by `InitializeWebViewGuardedAsync_WhenTheWebViewSeamFaults_ReportsToTheSinkAndDoesNotFault` and by the pump-hosted `InitializeBool_WhenTheWebViewSeamFaults_ObservesTheFaultThroughTheSink`.

Both catch arms of the guard are therefore covered, which is the substantive requirement behind the percentage.

Base-ref note: this task states no `git` command. The re-anchored base used throughout this delivery run is `988d35a8f8eb7436cc46a9f6424db917ed93807a`, replacing the plan-pinned `2b85134b42872e405602e6064e02dc9cda6c319b`, which is a stale ancestor rather than the current merge base.
