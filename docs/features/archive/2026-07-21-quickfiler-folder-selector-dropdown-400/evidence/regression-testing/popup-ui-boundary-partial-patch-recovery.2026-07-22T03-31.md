# Popup UI-boundary interrupted patch recovery

Timestamp: 2026-07-22T03:31:01.9879087Z

Command: `& { $paths = @('QuickFiler/Viewers/BreadcrumbUiDispatcher.cs','QuickFiler/Viewers/BreadcrumbWebViewSurfaceFactory.cs','QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs','QuickFiler/Viewers/BreadcrumbDropDownHost.cs','QuickFiler/Viewers/ItemViewer.Breadcrumb.cs','QuickFiler/Viewers/BreadcrumbDropDownOpenLifetime.cs','QuickFiler.Test/Viewers/BreadcrumbPopupControlDispatchTests.cs','QuickFiler.Test/Viewers/BreadcrumbUiThreadDispatchTests.cs','QuickFiler.Test/Viewers/BreadcrumbDropDownReadinessTests.cs','QuickFiler.Test/Viewers/BreadcrumbSelectorToggleUiBoundaryTests.cs','QuickFiler.Test/Viewers/BreadcrumbSelectorOpenRetryTests.cs','QuickFiler/QuickFiler.csproj'); 'HEAD=' + (git rev-parse HEAD); 'STATUS'; git status --short -- $paths; 'DIFF_NUMSTAT'; git diff --numstat -- $paths; 'DIFF_CHECK'; git diff --check -- $paths; 'FILES'; foreach ($path in $paths) { if (Test-Path -LiteralPath $path -PathType Leaf) { $hash=(Get-FileHash -Algorithm SHA256 -LiteralPath $path).Hash; $lines=(Get-Content -LiteralPath $path).Count; '{0}|LINES={1}|SHA256={2}' -f $path,$lines,$hash } else { '{0}|ABSENT' -f $path } }; 'COMPILE_INVENTORY'; $project=Get-Content -LiteralPath 'QuickFiler/QuickFiler.csproj'; $popup=@($project | Select-String -SimpleMatch '<Compile Include="Viewers\BreadcrumbPopupUiOperations.cs" />'); $lifetime=@($project | Select-String -SimpleMatch '<Compile Include="Viewers\BreadcrumbDropDownOpenLifetime.cs" />'); 'BreadcrumbPopupUiOperations.cs|COUNT=' + $popup.Count; $popup | ForEach-Object { 'LINE=' + $_.LineNumber + '|' + $_.Line.Trim() }; 'BreadcrumbDropDownOpenLifetime.cs|COUNT=' + $lifetime.Count; $lifetime | ForEach-Object { 'LINE=' + $_.LineNumber + '|' + $_.Line.Trim() } }`

EXIT_CODE: 0

Output Summary: The deterministic recovery inventory captured interrupted work at HEAD `dfb202fc5dbc50638a9519c66b64005bcb5de116` without resetting or discarding any edit. `git diff --check` returned zero scoped whitespace errors. The recovered state matches the revised plan: `BreadcrumbDropDownHost.cs` is 663 lines, `BreadcrumbPopupUiOperations.cs` is 491 lines, `ItemViewer.Breadcrumb.cs` is 498 lines, `BreadcrumbDropDownOpenLifetime.cs` is absent, `BreadcrumbPopupUiOperations.cs` has exactly one legacy-project compile include at line 392, and the lifetime helper has no include yet. The oversized host/ItemViewer composition remains interrupted P5-T18 work; it is not accepted by this recovery task.

## Recovered file inventory

| File | Lines | SHA-256 / state |
|---|---:|---|
| `BreadcrumbUiDispatcher.cs` | 270 | `64B341920E94238F894BB885D251420E7E2CB4263F827E3B0EEAFF1863519B42` |
| `BreadcrumbWebViewSurfaceFactory.cs` | 253 | `D0E8A20F353040A75ECAD3440D11DC8738A7134A3305D33F8D3A6F7F54B259A0` |
| `BreadcrumbPopupUiOperations.cs` | 491 | `37E8C305C7099282D28AAC8BA4351097962639483F7407828E3E1F75B76E5A74` |
| `BreadcrumbDropDownHost.cs` | 663 | `3F666817F223FB16027E71CA9002F66E05AAF837A1EE49476621266B4C7111CC` |
| `ItemViewer.Breadcrumb.cs` | 498 | `A3C616F3D6BFFEF76D1C375E53BC6AA8001566A5CCC6E498B3F03002DAA6B14C` |
| `BreadcrumbDropDownOpenLifetime.cs` | n/a | absent |
| `BreadcrumbPopupControlDispatchTests.cs` | 472 | `F3118B41594D016D058B65A3275A06204B647760A4C8E9BEB47EF2B91674B710` |
| `BreadcrumbUiThreadDispatchTests.cs` | 480 | `E4BD60150636A83CE977681249E03C63A2FC7CA96C32C5F8EF5BBB760926E62E` |
| `BreadcrumbDropDownReadinessTests.cs` | 498 | `6C910ED246150F2E27BAA6C1EC422B64E5638FB81EFEB3F8B333B37D8B9AF32E` |
| `BreadcrumbSelectorToggleUiBoundaryTests.cs` | 321 | `FA9CA66F2B03DE21311580F0A988D6DCDA9756361294A65E80E18E3B585D36B4` |
| `BreadcrumbSelectorOpenRetryTests.cs` | 268 | `282F5254FA1254AD25366002A8F1EEF238CB32091527DB86C05B1A108B8AA3F4` |

## Retained core obligations

- `BreadcrumbPopupControlDispatchTests.Readiness_DisposeFromAmbientNullWorker_DispatchesHandlerDetachment` retains deterministic dispatched handler-detachment proof.
- `SurfaceFactory_InitializationFailure_ReportsOnceAndCleansUp`, `DisposeSurfaceAsync_MessengerFailure_StillDisposesControlAndReportsOnce`, and the invalid-navigation data cases retain primary-error preservation, all-resource cleanup attempts, exactly-once disposal, and one null-navigation observation.
- `BreadcrumbDropDownReadinessTests.SurfaceFactory_InvalidArgumentsFailBeforeUiContextCapture` retains validation-before-capture proof.
- `BreadcrumbUiThreadDispatchTests.DispatchValue_SchedulingFailure_ReportsOnceAndFaultsReturnedTask` and the related value-dispatch cases retain per-method scheduling and error-observation coverage.

All completed P0-P4 and P5-T1-P5-T11 artifacts remain historical evidence. No artifact or source was reset, removed, or overwritten during recovery.
