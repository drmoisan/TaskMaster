# P5 Lifetime and Host Coverage CSharpier Restart

Timestamp: 2026-07-22T11:07:16Z

Command: `$file=(Resolve-Path 'QuickFiler.Test/Viewers/BreadcrumbDropDownLifecycleCoverageTests.cs').Path; $tool='C:\Users\DanMoisan\.dotnet\tools\csharpier.exe'; & $tool format $file --log-level Information; $first=(Get-FileHash -Algorithm SHA256 -LiteralPath $file).Hash; $firstLines=(Get-Content -LiteralPath $file).Count; & $tool format $file --log-level Information; $second=(Get-FileHash -Algorithm SHA256 -LiteralPath $file).Hash; $secondLines=(Get-Content -LiteralPath $file).Count; & $tool check $file --log-level Information`

EXIT_CODE: 0

Output Summary: PASS. The first analyzer build identified that the new test subscribed to a nonexistent `ToolStripControlHost.SizeChanged` event. Only the new test was corrected to subscribe to its injected popup control's `SizeChanged` event, which deterministically invalidates the lease after control placement. P5-T142 then restarted. CSharpier formatted only `BreadcrumbDropDownLifecycleCoverageTests.cs`; both write passes retained SHA-256 `3EB0042A662B3DB8BDCD2BA83E1A04C13D9D6E0778054676DAB4B246E139177A` at 465 physical lines, and the final check returned exit code 0.
