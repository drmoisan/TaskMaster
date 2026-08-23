# Popup UI-boundary core CSharpier gate after recovery

Timestamp: 2026-07-22T03:32:28.3430765Z

Command: `$files=@('QuickFiler/Viewers/BreadcrumbUiDispatcher.cs','QuickFiler/Viewers/BreadcrumbWebViewSurfaceFactory.cs','QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs','QuickFiler.Test/Viewers/BreadcrumbPopupControlDispatchTests.cs','QuickFiler.Test/Viewers/BreadcrumbUiThreadDispatchTests.cs','QuickFiler.Test/Viewers/BreadcrumbDropDownReadinessTests.cs'); $before=@{}; foreach($f in $files){$before[$f]=(Get-FileHash -Algorithm SHA256 -LiteralPath $f).Hash}; csharpier format @files; $exit=$LASTEXITCODE; 'EXIT_CODE=' + $exit; foreach($f in $files){$after=(Get-FileHash -Algorithm SHA256 -LiteralPath $f).Hash; '{0}|CHANGED={1}|LINES={2}|BEFORE={3}|AFTER={4}' -f $f,($before[$f] -ne $after),(Get-Content -LiteralPath $f).Count,$before[$f],$after}; exit $exit`

EXIT_CODE: 0

Output Summary: CSharpier formatted the six authorized P5 core files in 1.611 seconds. Every before/after SHA-256 value was identical, so the pass was stable and did not require a restart. All six files remain readable and at most 500 lines.

| File | Lines | Stable SHA-256 |
|---|---:|---|
| `BreadcrumbUiDispatcher.cs` | 270 | `64B341920E94238F894BB885D251420E7E2CB4263F827E3B0EEAFF1863519B42` |
| `BreadcrumbWebViewSurfaceFactory.cs` | 253 | `D0E8A20F353040A75ECAD3440D11DC8738A7134A3305D33F8D3A6F7F54B259A0` |
| `BreadcrumbPopupUiOperations.cs` | 491 | `37E8C305C7099282D28AAC8BA4351097962639483F7407828E3E1F75B76E5A74` |
| `BreadcrumbPopupControlDispatchTests.cs` | 472 | `F3118B41594D016D058B65A3275A06204B647760A4C8E9BEB47EF2B91674B710` |
| `BreadcrumbUiThreadDispatchTests.cs` | 480 | `E4BD60150636A83CE977681249E03C63A2FC7CA96C32C5F8EF5BBB760926E62E` |
| `BreadcrumbDropDownReadinessTests.cs` | 498 | `6C910ED246150F2E27BAA6C1EC422B64E5638FB81EFEB3F8B333B37D8B9AF32E` |

This artifact supersedes earlier P5 core formatter artifacts for the recovered current tree.
