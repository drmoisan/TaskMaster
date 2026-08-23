# P5 Lifetime and Host Coverage CSharpier Gate

Timestamp: 2026-07-22T11:06:17Z

Command: `$file=(Resolve-Path 'QuickFiler.Test/Viewers/BreadcrumbDropDownLifecycleCoverageTests.cs').Path; $tool='C:\Users\DanMoisan\.dotnet\tools\csharpier.exe'; & $tool format $file --log-level Information; $first=(Get-FileHash -Algorithm SHA256 -LiteralPath $file).Hash; $firstLines=(Get-Content -LiteralPath $file).Count; & $tool format $file --log-level Information; $second=(Get-FileHash -Algorithm SHA256 -LiteralPath $file).Hash; $secondLines=(Get-Content -LiteralPath $file).Count; & $tool check $file --log-level Information`

EXIT_CODE: 0

Output Summary: PASS. CSharpier formatted only `BreadcrumbDropDownLifecycleCoverageTests.cs`. The first pass produced SHA-256 `0A2D1BC26A823A04E00A8E9CD5190183E453B641560FE106F30C43573E410DC3` at 466 physical lines. The second pass retained the same hash and line count, and the final CSharpier check returned exit code 0. A preliminary `pipe-files` invocation was not accepted as a gate because a subsequent check correctly reported the file unformatted; the explicit file-scoped `format` command above corrected it before this evidence was recorded.
