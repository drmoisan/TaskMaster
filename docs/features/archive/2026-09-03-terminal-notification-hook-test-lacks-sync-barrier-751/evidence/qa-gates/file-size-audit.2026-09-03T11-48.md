# P4-T7 — File Size Audit After Formatting (Issue #751)

Timestamp: 2026-09-03T14-44

Commands:

```powershell
(Get-Content -LiteralPath 'TaskMaster.Test\AppGlobals\AppOlObjectsFolderTreeServiceTests.cs').Count
(Get-Content -LiteralPath 'TaskMaster.Test\AppGlobals\AppOlObjectsFolderTreeServiceLifecycleTests.cs').Count
```

EXIT_CODE: 0

This audit runs **after** P4-T1, because the formatter is the last actor that can change a line count.

## Output Summary

| File | P0-T10 pre-change | P4-T7 post-change | Delta | Cap | Headroom remaining |
|---|---|---|---|---|---|
| `TaskMaster.Test/AppGlobals/AppOlObjectsFolderTreeServiceTests.cs` | 492 | **493** | **+1** | 500 | 7 |
| `TaskMaster.Test/AppGlobals/AppOlObjectsFolderTreeServiceLifecycleTests.cs` | 490 | **490** | **0** | 500 | 10 |

## Acceptance

| Required | Observed | Result |
|---|---|---|
| Both post-change integers are less than or equal to 500 | 493 and 490 | PASS |
| Growth of `AppOlObjectsFolderTreeServiceTests.cs` relative to its P0-T10 value is at least 1 and at most 2 | +1 | PASS |
| Count of `AppOlObjectsFolderTreeServiceLifecycleTests.cs` is unchanged from its P0-T10 value | 490, unchanged | PASS |

## Interpretation

CSharpier did **not** reflow the inserted statement across two physical lines. The upper bound of 2 in the
growth band exists to tolerate that reflow; the observed growth is the minimum of 1, so the inserted
assertion `(await GetExceptionAsync(await run.Terminal)).Should().BeSameAs(fault);` fits on a single
physical line at CSharpier's configured print width.

The zero delta on the lifecycle file confirms the P2-T3 edit was a one-for-one line replacement:
`InvokedTerminalHookCount++;` became `Interlocked.Increment(ref InvokedTerminalHookCount);` with no net line
change.

Both files remain under the 500-line cap defined in `.claude/rules/general-code-change.md` § "File Size
Limit" and `CLAUDE.md` § 4, with 7 and 10 lines of headroom respectively.
