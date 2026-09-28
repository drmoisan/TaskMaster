# P4-T15 — AC1 diff confinement, re-verified after the final formatting pass

Timestamp: 2026-09-13T23-51

Command:

```
git -C . diff refs/issue816/base -- UtilitiesCS/Threading/UiThread.cs > coverage\logs\p4-t15-uithread.diff
```

followed by the same `Select-String` counts P2-T7 used, evaluated against the post-format tree.

EXIT_CODE: 0

Output Summary:

This repetition is required because the formatter may reflow the hardened condition or any other
region of the file, and a pre-format confinement result does not describe the tree a reviewer will
see. It was run after P4-T2 and P4-T3.

## Condition 1 — the four exit-shape counts

| # | Pattern | Post-format | Required |
|---|---|---|---|
| 1 | `^\s*return ` | **9** | 9 |
| 2 | `^\s*return true;\s*$` | **2** | 2 |
| 3 | `^\s*return false;\s*$` | **2** | 2 |
| 4 | `^\s*return _context is DispatcherSynchronizationContext` | **1** | 1 |

All four match the required values, so the first FAIL condition is not met.

## Condition 2 — the removed-line set

`(Select-String -Path coverage\logs\p4-t15-uithread.diff -Pattern '^-[^-]').Count` is **2**, and the
two members, by trimmed text, are:

```
// The persistent UI context captured at Init() time.
if (ReferenceEquals(_context, _uiSyncContext))
```

## Condition 3 — no removed line touches any other exit or the initializer

The count of removed lines containing any of the six tokens
`ReferenceEquals(_context, ambient)`, `ambient is null`, `_uiThreadId == -1`,
`DispatcherSynchronizationContext`, `lock (InitLock)` and `_initialized` is **0**, so the second
FAIL condition is not met.

## Whether the removed-line set differs from the one P2-T7 recorded

**It does not differ.** The removed-line set is the same two lines, in the same order, with the same
trimmed text.

The comparison was made over the whole diff document rather than only the removed lines:
`Compare-Object` over the two files' lines reports no differences, and both documents are 28 lines
long. The formatter therefore reflowed nothing in this file, which is consistent with the equal
before-and-after content hash P4-T2 recorded for it.

(The two diff files' SHA-256 hashes are not equal, because P2-T7's redirect merged standard error
into the file and this one did not. That is a difference in how the redirect was written, not in the
diff content; the line-by-line comparison above is the authoritative check and it reports identical
content.)

No FAIL condition is met, and AC1 holds against the tree as it will be reviewed.
