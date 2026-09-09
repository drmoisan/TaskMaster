# Final file-size audit (Issue #824, task P5-T12)

Timestamp: 2026-09-09T16-13

Command: `pwsh -NoProfile -Command 'Set-Location "<worktree-root>"; foreach ($p in @("UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILGlobals.cs","UtilitiesCS/NewtonsoftHelpers/SDIL Reader/MethodBodyReader.cs","UtilitiesCS.Test/NewtonsoftHelpers/SDILReader/ILGlobals_Tests.cs","UtilitiesCS.Test/NewtonsoftHelpers/SDILReader/MethodBodyReader_Tests.cs")) { Write-Output ($p + " = " + (Get-Content -LiteralPath $p).Count) }'`

This is the P0-T14 command run again, after the P5-T1 format pass.

EXIT_CODE: 0

Output Summary:

| File | Baseline | Final | Required | Holds | Headroom to 500 |
|---|---|---|---|---|---|
| `UtilitiesCS/NewtonsoftHelpers/SDIL Reader/ILGlobals.cs` | 197 | **228** | below 500 | yes | 272 |
| `UtilitiesCS.Test/NewtonsoftHelpers/SDILReader/ILGlobals_Tests.cs` | 133 | **270** | below 500 | yes | 230 |
| `UtilitiesCS/NewtonsoftHelpers/SDIL Reader/MethodBodyReader.cs` | 299 | **299** | exactly 299 | yes | 201 |
| `UtilitiesCS.Test/NewtonsoftHelpers/SDILReader/MethodBodyReader_Tests.cs` | 489 | **489** | exactly 489 | yes | 11 |

All four conditions hold.

The two modified files grew: `ILGlobals.cs` by 31 lines and `ILGlobals_Tests.cs` by 137 lines. Both
remain well below the 500-line cap in `.claude/rules/general-code-change.md`, so no file needed to be
split and no content was displaced into another file.

The two owned-but-unmodified files are exactly at their baseline counts. The 489-line figure for
`MethodBodyReader_Tests.cs` is the one that matters most: it has 11 lines of headroom, and the plan
placed all four new tests in `ILGlobals_Tests.cs` precisely so that this file would not absorb any of
them. It did not.

These counts were taken after the format pass, so they reflect CSharpier's final layout rather than
the hand-written one. That ordering is deliberate: the formatter's line wrapping is what determines
the committed line count, and a size audit taken before it would measure a layout that never reaches
the repository.
