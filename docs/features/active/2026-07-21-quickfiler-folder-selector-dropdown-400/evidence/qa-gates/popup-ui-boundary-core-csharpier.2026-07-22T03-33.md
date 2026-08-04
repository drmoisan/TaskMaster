# Popup UI-boundary core CSharpier gate after compatibility correction

Timestamp: 2026-07-22T03:33:50.9307348Z

Command: `$files=@('QuickFiler/Viewers/BreadcrumbUiDispatcher.cs','QuickFiler/Viewers/BreadcrumbWebViewSurfaceFactory.cs','QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs','QuickFiler.Test/Viewers/BreadcrumbPopupControlDispatchTests.cs','QuickFiler.Test/Viewers/BreadcrumbUiThreadDispatchTests.cs','QuickFiler.Test/Viewers/BreadcrumbDropDownReadinessTests.cs'); $before=@{}; foreach($f in $files){$before[$f]=(Get-FileHash -Algorithm SHA256 -LiteralPath $f).Hash}; csharpier format @files; $exit=$LASTEXITCODE; 'EXIT_CODE=' + $exit; foreach($f in $files){$after=(Get-FileHash -Algorithm SHA256 -LiteralPath $f).Hash; '{0}|CHANGED={1}|LINES={2}|BEFORE={3}|AFTER={4}' -f $f,($before[$f] -ne $after),(Get-Content -LiteralPath $f).Count,$before[$f],$after}; exit $exit`

EXIT_CODE: 0

Output Summary: The analyzer restart exposed that the interrupted patch had removed the existing `CaptureCurrentOrTests` compatibility seam while ItemViewer still called it. The seam was restored inside the authorized core helper. The first formatter pass changed that helper, so P5-T13 was restarted. This final pass formatted all six files in 1.625 seconds with identical before/after hashes. Every file remains at most 500 lines. This artifact supersedes the 03:32 recovery formatter artifact.

| File | Lines | Stable SHA-256 |
|---|---:|---|
| `BreadcrumbUiDispatcher.cs` | 270 | `64B341920E94238F894BB885D251420E7E2CB4263F827E3B0EEAFF1863519B42` |
| `BreadcrumbWebViewSurfaceFactory.cs` | 253 | `D0E8A20F353040A75ECAD3440D11DC8738A7134A3305D33F8D3A6F7F54B259A0` |
| `BreadcrumbPopupUiOperations.cs` | 497 | `65D31C7901D2602B897A684D87C173B016223FFBF9101C3EBB6E37B181650056` |
| `BreadcrumbPopupControlDispatchTests.cs` | 472 | `F3118B41594D016D058B65A3275A06204B647760A4C8E9BEB47EF2B91674B710` |
| `BreadcrumbUiThreadDispatchTests.cs` | 480 | `E4BD60150636A83CE977681249E03C63A2FC7CA96C32C5F8EF5BBB760926E62E` |
| `BreadcrumbDropDownReadinessTests.cs` | 498 | `6C910ED246150F2E27BAA6C1EC422B64E5638FB81EFEB3F8B333B37D8B9AF32E` |
