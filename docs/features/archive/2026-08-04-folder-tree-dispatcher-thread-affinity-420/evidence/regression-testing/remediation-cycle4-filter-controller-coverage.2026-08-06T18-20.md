# P5-T44 refreshed FilterOlFolders controller coverage evidence

Timestamp: 2026-08-06T18-20

The controller fixture passed 25/25. The deterministic local fakes cover successful and faulted viewer creation/disposal, synchronous and `InvokeRequired` close paths including invoke-fault containment, queued dispose-before-initialization and queued dispose-before-refresh, archive-root disposal callbacks, request/view/commit and subscription branches, refresh fault observation, and post-dispose mutation suppression. The tests use no real viewer, message loop, reflection, global mutable state, timer, network resource, temporary file, or live Outlook.

`FilterOlFoldersControllerRefreshDisposalTests.Coverage.cs` is 499 lines and retains exactly one adjacent `Compile` entry. The pre-authorized `FilterOlFoldersControllerRefreshDisposalTests.LifecycleRaces.cs` is 296 lines. The exact P5-T46 report measures the changed controller source at 101/102 and lifecycle source at 334/335; each exceeds the 95% P5 margin. The remaining unhit source lines are unchanged controller line 81 and unchanged lifecycle line 81.
