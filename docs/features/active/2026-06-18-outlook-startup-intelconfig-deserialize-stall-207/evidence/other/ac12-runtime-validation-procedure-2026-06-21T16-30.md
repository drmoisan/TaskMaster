# AC12 Manual Runtime-Validation Procedure

Timestamp: 2026-06-22T00-00

Procedure: Cold-start Outlook with the rebuilt add-in (clear any warm state; launch Outlook fresh). Observe the emitted `[Startup timing]` log lines, in particular the `Hook start` line and the `Hook complete | startup hook` line produced by `PerformReadinessHookup()` when the coordinator's `DispatcherTimer` poll reaches Completed. Confirm that the three readiness-dependent hookups are established once the store is ready: `ToDoFolder.Items` subscription (`OlToDoItems`), the `OlReminders` capture, and each inbox's `ItemAdd` subscription. Confirm the poll ran (the `Hook complete` line appears after one or more poll ticks rather than synchronously at `Hook start`).

ExpectedPassCondition: The readiness hookup completes without a prolonged STA block and without a `ContextSwitchDeadlock` Managed Debugging Assistant (MDA) being raised during startup. After startup, the inbox subscription is present (new inbox items are auto-processed), confirming the subscription was not silently dropped by a transient not-ready COMException. The `Hook complete` timing line shows the per-operation latencies (`toDoItemsMs`, `remindersMs`, `inboxSubscribeMs`) without a single prolonged blocking interval at `Hook start`.

Performer: maintainer (drmoisan).

NotAUnitTest: This is a runtime capture performed manually against a live Outlook process. It is explicitly NOT part of this plan's QC loop and NOT a unit test. Outlook is NOT launched by the toolchain (CSharpier / MSBuild analyzer / MSBuild nullable / vstest). The QC loop validates only the pure `HookReadinessCoordinator` seam, the toolchain gates, behavior preservation, banned-API absence, file-size compliance, and coverage. End-to-end deadlock resolution is verified here, by the maintainer, at runtime.
