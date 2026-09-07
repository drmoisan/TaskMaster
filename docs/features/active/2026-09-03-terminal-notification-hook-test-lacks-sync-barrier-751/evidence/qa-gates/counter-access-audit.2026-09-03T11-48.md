# P2-T4 — Counter Access Audit (Issue #751)

Timestamp: 2026-09-03T14-41

Command:

```powershell
Select-String -Path 'TaskMaster.Test\AppGlobals\*.cs','TaskMaster\AppGlobals\*.cs' -SimpleMatch 'InvokedTerminalHookCount'
```

EXIT_CODE: 0

## Output Summary — full match list

```
AppOlObjectsFolderTreeServiceLifecycleTests.cs:158: internal int InvokedTerminalHookCount,
AppOlObjectsFolderTreeServiceLifecycleTests.cs:200: Interlocked.Increment(ref InvokedTerminalHookCount);
AppOlObjectsFolderTreeServiceTests.cs:115: Volatile.Read(ref sut.InvokedTerminalHookCount).Should().Be(1);
```

Match count: **3**

## Acceptance

| Required | Observed | Result |
|---|---|---|
| The match list contains exactly three lines | 3 | PASS |
| The field declaration in `AppOlObjectsFolderTreeServiceLifecycleTests.cs` | line 158, `internal int InvokedTerminalHookCount,` | PASS |
| The `Interlocked.Increment` write in `AppOlObjectsFolderTreeServiceLifecycleTests.cs` | line 200 | PASS |
| The `Volatile.Read` read in `AppOlObjectsFolderTreeServiceTests.cs` | line 115 | PASS |
| No line carrying a bare `sut.InvokedTerminalHookCount.Should()` read | 0 such lines | PASS |
| No line carrying `InvokedTerminalHookCount++` | 0 such lines | PASS |

The two negative conditions were evaluated mechanically over the same match set rather than by inspection:
the bare-read filter `sut\.InvokedTerminalHookCount\.Should\(\)` returned 0 matches, and the non-atomic-write
filter `InvokedTerminalHookCount\+\+` returned 0 matches.

## Interpretation

Every remaining access to the counter is synchronised. The single write uses `Interlocked.Increment` and the
single cross-thread read uses `Volatile.Read`, matching the `_loadCount` precedent already present in the
same fixture class (`private int _loadCount;` at `:137`, `internal int LoadCount => Volatile.Read(ref _loadCount);`
at `:160`, `Interlocked.Increment(ref _loadCount)` at `:186`, per research §2.5).

`InvokedTerminalHookCount` was, before this change, the sole cross-thread counter in this fixture that was
neither atomically written nor volatilely read. It no longer is.

The read moved from line 114 to line 115 because P2-T1 inserted one line above it. The line-count effect of
that insertion is audited by P4-T7.

Scope note, carried from P1-T2 claim 1: this audit is scoped to the two globs
`TaskMaster.Test\AppGlobals\*.cs` and `TaskMaster\AppGlobals\*.cs` by construction. The same identifier also
appears in this feature folder's Markdown documents, which the globs exclude.
