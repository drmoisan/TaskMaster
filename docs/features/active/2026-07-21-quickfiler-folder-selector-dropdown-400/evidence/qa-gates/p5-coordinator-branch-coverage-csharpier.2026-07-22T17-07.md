# P5-T189 — CSharpier format + scoped check (batch N1)

Timestamp: 2026-07-22T17-07Z

Command: `$file=(Resolve-Path 'QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part2.cs').Path; $tool='C:\Users\DanMoisan\.dotnet\tools\csharpier.exe'; & $tool format $file --log-level Information; $first=(Get-FileHash -Algorithm SHA256 -LiteralPath $file).Hash; & $tool format $file --log-level Information; $second=(Get-FileHash -Algorithm SHA256 -LiteralPath $file).Hash; & $tool check $file --log-level Information; $code=$LASTEXITCODE`

EXIT_CODE: 0

## Output Summary

Mutating `csharpier format` was run on disk against exactly one file,
`QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part2.cs`. `csharpier pipe-files` was not used as a
formatting or verification gate. The formatter reported `Formatted 1 files` on both passes and the file hash was
identical after the first and second pass (`6EC48542768E3D195E2B6B844349DE40D8E100FFFEE78D24A29FDA48D2032FB5`),
so formatting is stable with no further change. The authoritative scoped `csharpier check` reported
`Checked 1 files` and exited **0**.

Post-format physical line count: **341** lines, which is at or below the 480-line bound (139 lines of remaining
headroom). No new file was created and no plan replanning trigger was reached.
