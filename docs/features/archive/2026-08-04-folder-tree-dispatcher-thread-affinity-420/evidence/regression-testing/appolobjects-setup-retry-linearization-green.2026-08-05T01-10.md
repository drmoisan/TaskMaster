# P5-T24 green evidence

Timestamp: 2026-08-05T01:10:00-04:00 (derived from the artifact filename)
Command: `vstest.console.exe TaskMaster.Test\\bin\\Debug\\TaskMaster.Test.dll /Tests:AppOlObjectsFolderTreeServiceLifecycleTests`
EXIT_CODE: 0
Output Summary: The recorded lifecycle suite passed 12/12, preserving exact setup failures, enabling a sequential retry, and verifying controlled disposal/publication interleavings.

- Command: `vstest.console.exe TaskMaster.Test\\bin\\Debug\\TaskMaster.Test.dll /Tests:AppOlObjectsFolderTreeServiceLifecycleTests`
- Result: passed, 12 tests, 0 failures, 0.2829 seconds.
- Setup factory and dispatcher-thread-check failures preserve the exact controlled exception, terminalize the owned initialization, and allow a direct sequential retry to publish one live service.
- The task-signal controls passed for disposal before the completion linearization point, two coalesced callers receiving one service, and disposal after a getter returns. No control uses a timeout, polling loop, live UI, reflection, or process-global dispatcher mutation.
- `AppOlObjectsFolderTreeServiceTests.cs`: 263 lines. `AppOlObjectsFolderTreeServiceLifecycleTests.cs`: 427 lines.
- `TaskMaster.Test.csproj` retains exactly one compile entry for each AppOl test source. `GetFolderTreeServiceGate`, `UiDispatcherScope`, and `UiThread._dispatcher` reflection are absent from the AppOl test sources.
