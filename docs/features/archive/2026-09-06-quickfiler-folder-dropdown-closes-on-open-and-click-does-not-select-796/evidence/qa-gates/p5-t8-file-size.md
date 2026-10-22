# P5-T8 — File-size gate for Phase 5

Timestamp: 2026-09-07T14-31
Task: [P5-T8]
Issue: #796
Channel used: A

Command:

```
pwsh -NoProfile -Command '@("QuickFiler\Viewers\BreadcrumbDropDownHost.cs","QuickFiler\Viewers\BreadcrumbDropDownHost.Open.cs","QuickFiler\Viewers\BreadcrumbDropDownHost.Diagnostics.cs","QuickFiler\Viewers\BreadcrumbDropDownOpenCoordinator.cs","QuickFiler.Test\Viewers\BreadcrumbDropDownCloseOrderingTests.cs","QuickFiler.Test\Viewers\BreadcrumbPendingOpenCloseTests.cs") | ForEach-Object { $_ + " " + (Get-Content -LiteralPath $_).Count }'
```

EXIT_CODE: 0

LINE-COUNT-IDIOM: (Get-Content -LiteralPath $_).Count

This is the idiom recorded on the `LINE-COUNT-IDIOM:` line of
evidence/baseline/p0-t12-file-size-baseline.md and no other. The prohibited
`Measure-Object -Line` idiom was not used.

## Measured physical line counts

| Path | Physical lines | Ceiling | Verdict |
|---|---|---|---|
| QuickFiler/Viewers/BreadcrumbDropDownHost.cs | 496 | 500 | within |
| QuickFiler/Viewers/BreadcrumbDropDownHost.Open.cs | 131 | 500 | within |
| QuickFiler/Viewers/BreadcrumbDropDownHost.Diagnostics.cs | 79 | 500 | within |
| QuickFiler/Viewers/BreadcrumbDropDownOpenCoordinator.cs | 395 | 500 | within |
| QuickFiler.Test/Viewers/BreadcrumbDropDownCloseOrderingTests.cs | 253 | 500 | within |
| QuickFiler.Test/Viewers/BreadcrumbPendingOpenCloseTests.cs | 413 | 500 | within |

Every recorded physical count is at most 500.

The main host part stands at 496 against the 500 ceiling, four lines of headroom. That is the file
executed task P1-T2 relieved by relocating `OnDropDownClosed` into the diagnostics part; the two
AC3 edits this phase made to it — the latch term inside `FinishClose` and the latch producer inside
`Close` — consumed eleven of the fifteen lines that relocation bought. The coordinator is unchanged
at its baseline 395, which is the observable form of the HOST enforcement-site decision.

Output Summary: six paths measured with the recorded idiom; all at most 500; the main host part at
496 with four lines of headroom.
