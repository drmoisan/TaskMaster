# P5 Hub and Attachment Coverage CSharpier Gate

Timestamp: 2026-07-22T11:30:51Z

Command: `$file=(Resolve-Path 'QuickFiler.Test/Viewers/BreadcrumbMessengerHubCoverageTests.cs').Path; $tool='C:\Users\DanMoisan\.dotnet\tools\csharpier.exe'; & $tool format $file --log-level Information; $firstHash=(Get-FileHash -Algorithm SHA256 -LiteralPath $file).Hash; $firstLines=(Get-Content -LiteralPath $file).Count; & $tool format $file --log-level Information; $secondHash=(Get-FileHash -Algorithm SHA256 -LiteralPath $file).Hash; $secondLines=(Get-Content -LiteralPath $file).Count; & $tool check $file --log-level Information`

EXIT_CODE: 0

Output Summary: PASS. CSharpier formatted only `BreadcrumbMessengerHubCoverageTests.cs`. Both write passes retained SHA-256 `4387E3B3F98CE0FA5DB06488D117DBFFE214DC7212E2518D721A0134FC631EB3` at exactly 478 physical lines, within the 480-line hard cap, and the final check returned exit code 0.
