# P5 selector-toggle worker-boundary CSharpier gate

Timestamp: `2026-07-22T09-00`

Command: `$file=(Resolve-Path 'QuickFiler.Test/Viewers/BreadcrumbSelectorToggleUiBoundaryTests.cs').Path; $before=(Get-FileHash -Algorithm SHA256 $file).Hash; @($file) | & 'C:\Users\DanMoisan\.dotnet\tools\csharpier.exe' pipe-files; $code=$LASTEXITCODE; $after=(Get-FileHash -Algorithm SHA256 $file).Hash; "BEFORE=$before"; "AFTER=$after"; "LINES=$((Get-Content $file).Count)"; exit $code`

EXIT_CODE: `0`

Output Summary: `PASS. CSharpier completed on exactly BreadcrumbSelectorToggleUiBoundaryTests.cs and made no change. The file retained SHA-256 98DCF6E455A135C41C0ED5529C3EA0AEFAC50DF64CCE912A7CB14F2211465104 and 480 physical lines.`

## Hash verification

- Before: `98DCF6E455A135C41C0ED5529C3EA0AEFAC50DF64CCE912A7CB14F2211465104`.
- After: `98DCF6E455A135C41C0ED5529C3EA0AEFAC50DF64CCE912A7CB14F2211465104`.
- Result: no formatter change; no repeat was required.
