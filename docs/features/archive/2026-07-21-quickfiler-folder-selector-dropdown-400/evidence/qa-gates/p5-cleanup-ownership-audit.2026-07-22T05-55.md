# P5 cleanup ownership audit

Timestamp: 2026-07-22T05:55:59.9995947Z

Command: `& { $production='C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\QuickFiler\Viewers\BreadcrumbPopupUiOperations.cs'; $test='C:\Users\DanMoisan\repos\TaskMaster-wt\2026-07-21T10-25\QuickFiler.Test\Viewers\BreadcrumbPopupControlDispatchTests.cs'; Write-Output 'OWNERSHIP_AND_RETRY'; & 'C:\Users\DanMoisan\AppData\Roaming\npm\node_modules\@openai\codex\node_modules\@openai\codex-win32-x64\vendor\x86_64-pc-windows-msvc\codex-path\rg.exe' -n 'DisposeHostedSurfaceAsync|DisposeHostedSurfaceAfterFailureAsync|DisposeWithRetryAsync|DisposeSurfaceAsync|CompleteAll|completed\[|host == null|Items.Remove|messenger as IDisposable|CreateAndInstall_CancellationCleanupFailure|CreateAndInstall_StaleHostCleanup|MessengerFailure|DisposeCount|errors.Should' $production $test; Write-Output 'LINES_AND_HASHES'; foreach($path in @($production,$test)){ $hash=(Get-FileHash -Algorithm SHA256 -LiteralPath $path).Hash; $lines=(Get-Content -LiteralPath $path).Count; Write-Output "$path|LINES=$lines|SHA256=$hash" } }`

EXIT_CODE: 0

Output Summary: The source audit found explicit cleanup actions for drop-down membership, the host wrapper, an unhosted control, and the messenger; a per-action completion array; first-failure preservation; and ownership transfer before cleanup. The production source is SHA-256 `6D28CD1C10C81993101E989CCE473E20E44D4DEB412DD86244E728EC59378B26` at 479 physical lines. The test source is SHA-256 `3FF0BA998C3727C7E1E68AD33F10B6ADAFE354C21A29869217AE0228E295E979` at 500 physical lines; its separate structural-headroom work remains assigned to P5-T56 through P5-T62.

## Cleanup ownership matrix

| State or resource | Sole cleanup owner | Retry and deterministic proof |
|---|---|---|
| Drop-down membership | The first action supplied by `DisposeHostedSurfaceAsync` removes the host from `dropDown.Items`. | `DisposeWithRetryAsync` records successful completion by action index and skips it on the second pass. `CreateAndInstall_StaleHostCleanup_DoesNotDisposeOwnedControlDirectly` passes through this stale installation path. |
| Hosted control | `ToolStripControlHost.Dispose` is the sole owner while `host` exists. | Direct control disposal is guarded by `host == null`; therefore a failed host action retries the host without invoking a second control owner. The stale-host test asserts one control disposal. |
| Unhosted control | The direct-control action owns the control only before a host exists. | Cancellation cleanup supplies the unhosted control as its own action. `CreateAndInstall_CancellationCleanupFailure_RetriesOnlyFailedResource` proves the successful control action is not repeated when the messenger alone fails transiently. |
| Messenger | The messenger `IDisposable.Dispose` action is independent of host/control ownership. | Its completion index is set only after a successful dispatched action. The cancellation test proves a first messenger failure is retried exactly once while the completed control remains at one disposal. |
| Legacy non-host surface | `DisposeProductionSurface` uses `CompleteAll` to attempt messenger and control once in order. | `DisposeSurfaceAsync_MessengerFailure_StillDisposesControlAndReportsOnce` proves the control attempt continues, the original messenger failure remains the thrown instance, and the error sink sees it once. |
| Failure rollback | `DisposeHostedSurfaceAfterFailureAsync` invokes hosted cleanup, while `IgnoreFailureAsync` prevents a cleanup exception from replacing the initiating exception. | Factory initialization, navigation, readiness, and invalid-navigation tests all pass with the initiating failure retained and a single expected observation. |

## Retry and primary-error conclusions

- Each cleanup action has a dedicated `completed[index]` flag. Only successful actions set it, and every later attempt skips completed indexes. A completed resource cannot be double-disposed by retry.
- Each pending resource executes through its own `RunAsync` call, so one secondary failure does not stop attempts for the remaining resources. Only first-attempt failures are reported; retry failures are not reported again.
- `failure ??= exception` retains the first cleanup failure. In a primary operation catch, rollback cleanup is awaited and suppressed before the original `throw`, so a cleanup secondary cannot replace the initiating exception.
- Before cancellation or stale cleanup is awaited, local ownership is transferred by clearing `created` and, where applicable, `host`. The outer catch cannot perform a second cleanup of the transferred resources.
- `BreadcrumbPopupUiOperations` has a focused class summary, behavior-oriented method names, separate placement/creation/observation/cleanup sections, and 479 physical lines. It is readable, documented, CSharpier-stable, and below the P5-T35 maximum of 480 lines.
