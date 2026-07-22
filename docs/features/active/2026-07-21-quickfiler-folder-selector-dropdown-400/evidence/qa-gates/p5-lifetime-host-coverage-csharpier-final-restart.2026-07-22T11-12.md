# P5 Lifetime and Host Coverage CSharpier Final Restart

Timestamp: 2026-07-22T11:12:49Z

Command: `$file=(Resolve-Path 'QuickFiler.Test/Viewers/BreadcrumbDropDownLifecycleCoverageTests.cs').Path; $tool='C:\Users\DanMoisan\.dotnet\tools\csharpier.exe'; & $tool format $file --log-level Information; $first=(Get-FileHash -Algorithm SHA256 -LiteralPath $file).Hash; $firstLines=(Get-Content -LiteralPath $file).Count; & $tool format $file --log-level Information; $second=(Get-FileHash -Algorithm SHA256 -LiteralPath $file).Hash; $secondLines=(Get-Content -LiteralPath $file).Count; & $tool check $file --log-level Information`

EXIT_CODE: 0

Output Summary: PASS. The exact-filter investigation replaced an unobservable inner `Task.FromException` fault with a deterministic synchronous failure from the injected `Func<Task>` delegate. An orphaned isolated test host from the superseded wait was identified and stopped before the required P5-T142 restart. CSharpier then formatted only `BreadcrumbDropDownLifecycleCoverageTests.cs`; both write passes retained SHA-256 `70D700C6F4EF145B106FDDA5058FDCAEA99471CE229D43448DC9917923F2B9D3` at 468 physical lines, and the final check returned exit code 0.
