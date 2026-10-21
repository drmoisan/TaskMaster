# P5-T7 — File Size and Token Census After Formatting

Timestamp: 2026-09-17T02-36

Command: `CMD-CENSUS` over
`QuickFiler.Test/Viewers/ItemViewerBreadcrumbThreadAffinityTests.cs`.

EXIT_CODE: 0

CHANNEL: COMMAND

This runs after P5-T1 because CSharpier can change line counts.

## Output Summary

    TOKEN Task.Run( = 1
    TOKEN .GetAwaiter() = 1
    TOKEN Throw<InvalidOperationException>( = 0
    TOKEN error.Message.Contains( = 0
    TOKEN Contains( = 0
    TOKEN NotBeOfType<ObjectDisposedException>() = 2
    TOKEN ClearViewerDispatcher(scope.Viewer); = 1
    TOKEN [TestMethod] = 7
    TOKEN vacuously = 3
    TOKEN RunOnDedicatedWorkerThread( = 3
    TOKEN Exception captured = RunOnDedicatedWorkerThread( = 2
    TOKEN new Thread( = 1
    TOKEN IsBackground = true = 1
    TOKEN thread.Join(); = 1
    TOKEN Join( = 4
    TOKEN Join() = 4
    TOKEN UiDispatcher.CheckAccess() = 2
    TOKEN isOwnerThread = 4
    TOKEN BeOfType<InvalidOperationException>() = 2
    TOKEN captured.Message.Should().Contain( = 2
    TOKEN #900 = 3
    TOKEN action(); = 1
    TOKEN Thread.Sleep = 0
    TOKEN Task.Delay = 0
    TOKEN [Timeout = 0
    TOKEN DoNotParallelize = 0
    LINES = 490
    SHA256 = 8EBC19F829536957BEB0DAEC628929CA8DE3F11E10AA819DEA41362C6FCBB164

## Acceptance

All three conditions hold.

- `LINES` is 490, at most 500. `FILE SIZE LIMIT EXCEEDED` was not reached; the file sits 10 lines
  under the repository's 500-line limit. The XML remarks were not shortened to gain headroom, and
  the census confirms why that matters: they carry the pinned `#900` count of 3 and two of the three
  `vacuously` occurrences.
- Every `TOKEN` value equals the "After P2-T1 and P2-T2" column. The two mutation-residue checks the
  acceptance condition names explicitly both hold: `ClearViewerDispatcher(scope.Viewer);` is 1 and
  `action();` is 1. The four banned-API tokens are 0 each.
- `SHA256` equals `FIX-HASH:` from P2-T5,
  `8EBC19F829536957BEB0DAEC628929CA8DE3F11E10AA819DEA41362C6FCBB164`. P5-T1 recorded
  `FORMAT_CHANGED_TREE: False`, so this is the branch the acceptance condition requires, and no
  `POST-FORMAT-HASH:` is recorded.

The file is byte-identical to the state committed by P2-T5, after two mutations, two reverts, a
repository-wide formatter pass and four solution rebuilds.
