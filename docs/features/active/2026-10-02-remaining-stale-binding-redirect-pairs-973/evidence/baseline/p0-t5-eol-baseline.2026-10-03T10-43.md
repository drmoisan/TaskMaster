# P0-T5 line-ending baseline (issue #973; read-only)

Timestamp: 2026-10-03T10-43
Command: Grep tool pattern `^` and pattern `\r$` (count mode) over glob */app.config, over the five packages.config and five csproj of section 5, and over tests/scripts/dependencies/BindingRedirectVerification.Tests.ps1 (CMD-LINECOUNT, CMD-CRCOUNT); git -C <execution-worktree-root> ls-files --eol -- <28 paths> (CMD-EOL); pwsh -NoProfile -Command 'Set-Location -LiteralPath "<execution-worktree-root>"; foreach ($p in @(<17 app.config paths>)) { "BOM-BYTES " + $p + "=" + ((Get-Content -LiteralPath $p -AsByteStream -TotalCount 3) -join [char]44) }' (CMD-BOM)
EXIT_CODE: 0
Output Summary: every app.config CRCOUNT equals LINECOUNT with w/crlf and a UTF-8 BOM (239,187,191); the 15 Write Set configs equal section 8 column L; every packages.config and csproj has CRCOUNT equal to LINECOUNT minus 1 with the fact 6 figures and w/crlf; the test file reads 335/335 and w/crlf. No EOL-DRIFT.

## app.config (LINECOUNT / CRCOUNT / EOL / BOM)

- Tags/app.config 239 / 239 / w/crlf / BOM-BYTES Tags/app.config=239,187,191
- TaskTree/app.config 239 / 239 / w/crlf / BOM-BYTES TaskTree/app.config=239,187,191
- TaskVisualization/app.config 239 / 239 / w/crlf / BOM-BYTES TaskVisualization/app.config=239,187,191
- QuickFiler/app.config 243 / 243 / w/crlf / BOM-BYTES QuickFiler/app.config=239,187,191
- TaskMaster/app.config 430 / 430 / w/crlf / BOM-BYTES TaskMaster/app.config=239,187,191
- ToDoModel/app.config 323 / 323 / w/crlf / BOM-BYTES ToDoModel/app.config=239,187,191
- UtilitiesCS/app.config 293 / 293 / w/crlf / BOM-BYTES UtilitiesCS/app.config=239,187,191
- VBFunctions.Test/app.config 351 / 351 / w/crlf / BOM-BYTES VBFunctions.Test/app.config=239,187,191
- Tags.Test/app.config 343 / 343 / w/crlf / BOM-BYTES Tags.Test/app.config=239,187,191
- TaskTree.Test/app.config 343 / 343 / w/crlf / BOM-BYTES TaskTree.Test/app.config=239,187,191
- QuickFiler.Test/app.config 367 / 367 / w/crlf / BOM-BYTES QuickFiler.Test/app.config=239,187,191
- TaskMaster.Test/app.config 359 / 359 / w/crlf / BOM-BYTES TaskMaster.Test/app.config=239,187,191
- TaskVisualization.Test/app.config 363 / 363 / w/crlf / BOM-BYTES TaskVisualization.Test/app.config=239,187,191
- ToDoModel.Test/app.config 363 / 363 / w/crlf / BOM-BYTES ToDoModel.Test/app.config=239,187,191
- UtilitiesCS.Test/app.config 383 / 383 / w/crlf / BOM-BYTES UtilitiesCS.Test/app.config=239,187,191
- SVGControl/app.config 23 / 23 / w/crlf / BOM-BYTES SVGControl/app.config=239,187,191
- SVGControl.Test/app.config 227 / 227 / w/crlf / BOM-BYTES SVGControl.Test/app.config=239,187,191

## packages.config and csproj (LINECOUNT / CRCOUNT / EOL)

- UtilitiesCS/packages.config 146 / 145 / w/crlf
- QuickFiler/packages.config 82 / 81 / w/crlf
- ToDoModel/packages.config 27 / 26 / w/crlf
- TaskMaster/packages.config 78 / 77 / w/crlf
- UtilitiesCS.Test/packages.config 109 / 108 / w/crlf
- UtilitiesCS/UtilitiesCS.csproj 1335 / 1334 / w/crlf
- QuickFiler/QuickFiler.csproj 620 / 619 / w/crlf
- ToDoModel/ToDoModel.csproj 201 / 200 / w/crlf
- TaskMaster/TaskMaster.csproj 587 / 586 / w/crlf
- UtilitiesCS.Test/UtilitiesCS.Test.csproj 1023 / 1022 / w/crlf

## Test file

- tests/scripts/dependencies/BindingRedirectVerification.Tests.ps1 335 / 335 / w/crlf

CMD-EOL raw rows all read `i/lf    w/crlf  attr/text=auto` (the `* text=auto` attribute normalises the index; the w/ column is the gated field).
