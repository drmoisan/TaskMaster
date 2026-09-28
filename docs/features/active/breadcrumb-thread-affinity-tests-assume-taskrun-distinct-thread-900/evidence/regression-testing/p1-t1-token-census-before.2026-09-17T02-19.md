# P1-T1 — Pre-Edit Token Census

Timestamp: 2026-09-17T02-19

Command: `CMD-CENSUS` over
`QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs`. The census reads the file once
and counts case-sensitive, non-overlapping occurrences with
`[regex]::Matches($content, [regex]::Escape($token)).Count`. Counts are occurrences, not lines.
`Select-String` is not used, because it is case-insensitive by default and counts lines.

EXIT_CODE: 0

CHANNEL: COMMAND

## Output Summary

    TOKEN Task.Run( = 3
    TOKEN .GetAwaiter() = 3
    TOKEN Throw<InvalidOperationException>( = 2
    TOKEN error.Message.Contains( = 2
    TOKEN Contains( = 2
    TOKEN NotBeOfType<ObjectDisposedException>() = 2
    TOKEN ClearViewerDispatcher(scope.Viewer); = 1
    TOKEN [TestMethod] = 7
    TOKEN vacuously = 1
    TOKEN RunOnDedicatedWorkerThread( = 0
    TOKEN Exception captured = RunOnDedicatedWorkerThread( = 0
    TOKEN new Thread( = 0
    TOKEN IsBackground = true = 0
    TOKEN thread.Join(); = 0
    TOKEN Join( = 0
    TOKEN Join() = 0
    TOKEN UiDispatcher.CheckAccess() = 0
    TOKEN isOwnerThread = 0
    TOKEN BeOfType<InvalidOperationException>() = 0
    TOKEN captured.Message.Should().Contain( = 0
    TOKEN #900 = 0
    TOKEN action(); = 0
    TOKEN Thread.Sleep = 0
    TOKEN Task.Delay = 0
    TOKEN [Timeout = 0
    TOKEN DoNotParallelize = 0
    LINES = 419
    SHA256 = CE87F6C29F590A1CDFD6AC95B9B77BBCB1E334B9A017B13699B3A6AFE30AA1CA

## Acceptance

Every `TOKEN` value equals the pre-edit column of the plan's Token Census Expectations table. The
four values the acceptance condition names explicitly all hold: `Task.Run(` 3, `.GetAwaiter()` 3,
`RunOnDedicatedWorkerThread(` 0, `UiDispatcher.CheckAccess()` 0, `vacuously` 1, `#900` 0,
`action();` 0.

`LINES = 419`, as required. The file ends with a newline; a viewer that renders a 420th empty row is
displaying the terminator, not a line of content. The count above is
`@(Get-Content -LiteralPath $path).Count`, which does not include the terminator.

`SHA256` equals the `PRE-EDIT-HASH:` recorded by P0-T4,
`CE87F6C29F590A1CDFD6AC95B9B77BBCB1E334B9A017B13699B3A6AFE30AA1CA`. Both values are the `Hash`
property of `Get-FileHash -Algorithm SHA256 -LiteralPath`, so the comparison is between values
produced the same way. The file is therefore byte-identical to its state before any task in this run
touched anything, which also re-confirms P0-T3's finding that the Write Set file was not already
dirty on this branch.

`TREE DIVERGED FROM PLAN` was not reached.

## Role of these figures

These are the positive controls every later count gate is measured against. Their value is that each
transition gate has a recorded before-value on the unfixed tree, so a post-edit count of, for
example, `RunOnDedicatedWorkerThread(` equal to 3 is a measured change from 0 rather than an
unanchored assertion. The three tokens that stay constant across the whole plan
(`NotBeOfType<ObjectDisposedException>()` at 2, `[TestMethod]` at 7,
`ClearViewerDispatcher(scope.Viewer);` at 1 outside the mutation window) are invariants rather than
transitions, and are gated as such.

The word `captured` appears twice in the file's prose at this point, which is why every gate on the
captured exception uses the longer literal `Exception captured = RunOnDedicatedWorkerThread(` or
`captured.Message.Should().Contain(` rather than the bare word.
