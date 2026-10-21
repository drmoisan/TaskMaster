# Phase 2 — Write Set line counts after the Site A and Site A' guards

Timestamp: 2026-09-09T13-00
Task: [P2-T3]

Verified immediately, before formatting, because
`QuickFiler/Controllers/QfcHomeController.cs` lands at exactly the 500-line ceiling with no headroom.

Command: the `[P0-T13]` line-count command, re-run unchanged.
EXIT_CODE: 0

Verbatim output:

```text
QuickFiler/Controllers/QfcHomeController.cs 500
QuickFiler/Controllers/EfcHomeController.cs 447
UtilitiesCS/Threading/ProgressViewer.cs 92
UtilitiesCS/Threading/ProgressPane.cs 61
QuickFiler.Test/Controllers/QfcHomeControllerCleanupTests.cs 192
QuickFiler.Test/Controllers/EfcHomeControllerLifecycleTests.cs 492
UtilitiesCS.Test/Threading/ProgressViewer_Tests.cs 352
UtilitiesCS.Test/Threading/ProgressPane_Tests.cs 192
```

## Acceptance check

| File | Baseline | Now | Required | Met |
|---|---|---|---|---|
| `QuickFiler/Controllers/QfcHomeController.cs` | 498 | **500** | exactly 500 | yes |
| `QuickFiler/Controllers/EfcHomeController.cs` | 445 | **447** | exactly 447 | yes |

Both figures are exact, not merely under budget. `QfcHomeController.cs` gained a net two lines — one
statement removed, three added — and now sits at exactly the 500-line ceiling, which
`.claude/rules/general-code-change.md` line 49 permits because the rule forbids *exceeding* 500.
There is **zero headroom** on that file for the remainder of this plan.

The explanatory "why" at both sites is a trailing comment on the local-read statement rather than a
separate comment line, precisely because of that zero headroom. The longest of the three inserted
lines is 94 characters including indentation, below CSharpier's 100-character print width, so the
final formatting pass will not split it and will not push the file to 501. `[P6-T13]` re-verifies
after that pass.

No count above 500 was recorded, so the hard stop this task defines was not reached. No commented-out
`//logger.Debug(...)` line at 41, 273, 316, 331 or 337 was deleted — those five lines are the sole
`DateTime.Now` / `DateTime.UtcNow` / `Random.Shared` / `Thread.Sleep` / `Task.Delay` matches in the
four production files, and the banned-API record depends on them remaining. No code was moved into
`QfcHomeController.Metrics.cs` or `QfcHomeController.Iteration.cs`, which would have falsified AC20.

The other six files are unchanged from their post-Phase-1 counts, confirming this phase touched only
the two production files it was scoped to.

Output Summary: `QfcHomeController.cs` reports exactly 500 and `EfcHomeController.cs` reports exactly
447, both matching the plan's required values. No file exceeds the 500-line ceiling.
