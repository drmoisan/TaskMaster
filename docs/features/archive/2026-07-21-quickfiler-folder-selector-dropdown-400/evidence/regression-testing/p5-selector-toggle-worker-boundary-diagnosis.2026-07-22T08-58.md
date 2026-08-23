# P5 selector-toggle worker-boundary diagnosis

Timestamp: `2026-07-22T08-58`

Command: `$test='QuickFiler.Test/Viewers/BreadcrumbSelectorToggleUiBoundaryTests.cs'; $inspect=@($test,'QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs','QuickFiler/Viewers/BreadcrumbUiDispatcher.cs'); $production=@('QuickFiler/Viewers/BreadcrumbUiDispatcher.cs','QuickFiler/Viewers/BreadcrumbWebViewSurfaceFactory.cs','QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs','QuickFiler/Viewers/BreadcrumbDropDownHost.cs','QuickFiler/Viewers/ItemViewer.Breadcrumb.cs','QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.cs','QuickFiler/Viewers/BreadcrumbMessengerHub.cs','QuickFiler/Viewers/BreadcrumbCollapsedSurfaceController.cs'); foreach($path in ($inspect+$production | Select-Object -Unique)){"$path|$((Get-Content $path).Count)|$((Get-FileHash -Algorithm SHA256 $path).Hash)"}; Select-String -Path $test -Pattern '^\s*public void (\w+)\('; Select-String -Path $test -Pattern 'Task\.Run|GetAwaiter|GetResult|postsBeforeToggle|PostCount|WaitForPost|InvokeAmbientNull|lock \(_sync\)' -Context 2,2; Select-String -Path 'QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs','QuickFiler/Viewers/BreadcrumbUiDispatcher.cs' -Pattern 'selectorToggle|DispatchAsync|IsCurrentBoundary|_ownerThreadId|SynchronizationContext\.Current|_context\.Post' -Context 2,2; git diff --check -- $test; git status --short -- $test`

EXIT_CODE: `0`

Output Summary: `PASS. The failure is a deterministic-test proof defect exposed by coverage timing. Immediate synchronous waiting can inline the queued Task.Run operation on the owning waiter; BreadcrumbUiDispatcher then legitimately accepts its captured owner thread and executes without a second SynchronizationContext.Post. No production correction is required. Exactly one existing test source is authorized.`

## Root cause

The selector-toggle test currently queues `messenger.Receive(...)` with `Task.Run` and immediately calls `GetAwaiter().GetResult()` on that task. A synchronous task wait may ask the default scheduler to inline queued work on the waiting thread. Under the 08-44 coverage timing, the receive operation therefore ran on the owning test thread before another worker executed it.

`BreadcrumbBridgeCoordinator` dispatches the selector-toggle callback through `BreadcrumbUiDispatcher`. `BreadcrumbUiDispatcher.IsCurrentBoundary` accepts any of these valid ownership indicators:

- its executing-dispatcher marker;
- the captured `SynchronizationContext`; or
- the captured `_ownerThreadId`.

When the queued receive is inlined by the owner-thread waiter, `_ownerThreadId` correctly identifies the current boundary. The dispatcher therefore executes the callback directly and does not issue a second `SynchronizationContext.Post`. Coverage instrumentation supplied the timing condition that exposed this test-proof defect; it does not show a production dispatch behavior change.

Separately, `CapturingSynchronizationContext.Post` updates `PostCount` while holding `_sync`, but the current auto-property getter reads the value without that lock. The corrected test observation must use the existing locked read boundary.

## Inspected files

| File | Physical lines | SHA-256 |
|---|---:|---|
| `QuickFiler.Test/Viewers/BreadcrumbSelectorToggleUiBoundaryTests.cs` | 480 | `5FD7983359427300F589C0D6A2E80FC00F028DB07613F8948465EB675E1D9AFC` |
| `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs` | 455 | `224D5614B8A293665EC22B563A9C2D7421CA1E0046A369AB4D56A728347BD391` |
| `QuickFiler/Viewers/BreadcrumbUiDispatcher.cs` | 270 | `64B341920E94238F894BB885D251420E7E2CB4263F827E3B0EEAFF1863519B42` |

The authorized test source contains exactly four MSTest methods:

1. `WorkerProviderAndSelectorToggle_MarshalPostsAndCallbackEntryToOwningBoundary`
2. `PopupHost_WorkerCompletions_RunOnlyWhenCreatorThreadDrainsBoundary`
3. `PopupHost_FocusFailureAfterShow_NativeClosesThenRetriesClosedSession`
4. `PopupHost_FirstSchedulingFailure_SettlesFalseThenRetriesAndObservesLifecycle`

## P5-T82 production immutability ledger

| Production source | Physical lines | Current SHA-256 | P5-T82 result |
|---|---:|---|---|
| `BreadcrumbUiDispatcher.cs` | 270 | `64B341920E94238F894BB885D251420E7E2CB4263F827E3B0EEAFF1863519B42` | Preserved |
| `BreadcrumbWebViewSurfaceFactory.cs` | 253 | `D0E8A20F353040A75ECAD3440D11DC8738A7134A3305D33F8D3A6F7F54B259A0` | Preserved |
| `BreadcrumbPopupUiOperations.cs` | 480 | `A5CCA5E401E3612DE406464F4F03C11B3BBD6B1CD76D86FA5AD31AF2C2D5A396` | Preserved |
| `BreadcrumbDropDownHost.cs` | 470 | `7B0A2981918DB95A83EEB077AE860EA62B28C8713CDD537EED5C0BECD9BD6F28` | Preserved |
| `ItemViewer.Breadcrumb.cs` | 480 | `4AF910250A32B3A037E1ECDAE71EECA10DA3E4432379A4D7F84202DDF27436A0` | Preserved |
| `BreadcrumbDropDownOpenLifetime.cs` | 437 | `E53DE9BE76CB7AC3F69B43C12088A7B4B6DA6F3F2455DCF7C6C10F5A010C53F1` | Preserved |
| `BreadcrumbMessengerHub.cs` | 456 | `AE307D76F01FB5C50289E9F50B6FC5F05C770A81EA4827BA010C00336A1006B2` | Preserved |
| `BreadcrumbCollapsedSurfaceController.cs` | 308 | `92B24E477A20C49ADBD372B42E7A6F22AC7870276789139AA42700BF8AE5FBDE` | Preserved |

## Scope decision

- Production corrections required: `0`.
- Authorized correction: exactly `QuickFiler.Test/Viewers/BreadcrumbSelectorToggleUiBoundaryTests.cs`.
- Additional source, helper, test, project, package, runsettings, coverage configuration, exclusion, or designer changes required: `0`.
- `git diff --check` reported no error for the authorized test source.
