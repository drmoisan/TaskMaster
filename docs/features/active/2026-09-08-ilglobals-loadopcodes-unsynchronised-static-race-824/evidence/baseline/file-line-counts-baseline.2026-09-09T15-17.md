# Baseline line counts of the four owned files (Issue #824, task P0-T14)

Timestamp: 2026-09-09T15-17

Command: `pwsh -NoProfile -Command 'Set-Location "<worktree-root>"; foreach ($p in @("UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILGlobals.cs","UtilitiesCS/NewtonsoftHelpers/SDIL Reader/MethodBodyReader.cs","UtilitiesCS.Test/NewtonsoftHelpers/SDILReader/ILGlobals_Tests.cs","UtilitiesCS.Test/NewtonsoftHelpers/SDILReader/MethodBodyReader_Tests.cs")) { Write-Output ($p + " = " + (Get-Content -LiteralPath $p).Count) }'`

EXIT_CODE: 0

Output Summary:

| File | `Get-Content .Count` | `Grep ^` count | Plan-expected | Agreement |
|---|---|---|---|---|
| `UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILGlobals.cs` | 197 | 197 | 197 | yes |
| `UtilitiesCS/NewtonsoftHelpers/SDIL Reader/MethodBodyReader.cs` | 299 | 299 | 299 | yes |
| `UtilitiesCS.Test/NewtonsoftHelpers/SDILReader/ILGlobals_Tests.cs` | 133 | 133 | 133 | yes |
| `UtilitiesCS.Test/NewtonsoftHelpers/SDILReader/MethodBodyReader_Tests.cs` | 489 | 489 | 489 | yes |

Both measurements agree on every file and both match the values the plan re-derived while it was
authored, so the tree has not moved and no plan revision is required before editing begins.

The cross-check uses a `Grep` for `^` in `output_mode: count`, which returns the same number whether
or not the file ends with a trailing newline, so it is not an independent restatement of the same
measurement error.

Headroom against the 500-line cap at baseline: `ILGlobals.cs` 303 lines, `MethodBodyReader.cs` 201,
`ILGlobals_Tests.cs` 367, `MethodBodyReader_Tests.cs` 11. The last of these is why no new test may be
placed in `MethodBodyReader_Tests.cs`.
