# P2-T7 — AC1 diff confinement, before formatting

Timestamp: 2026-09-13T23-29

Command:

```
git -C . diff refs/issue816/base -- UtilitiesCS/Threading/UiThread.cs > coverage\logs\p2-t7-uithread.diff
```

followed by `Select-String` counts against that diff file and the four exit-shape counts from
P0-T18 against the source.

EXIT_CODE: 0

Output Summary:

## Condition 1 — the four exit-shape counts are unchanged

| # | Pattern | Post-change | P0-T18 baseline |
|---|---|---|---|
| 1 | `^\s*return ` | **9** | 9 |
| 2 | `^\s*return true;\s*$` | **2** | 2 |
| 3 | `^\s*return false;\s*$` | **2** | 2 |
| 4 | `^\s*return _context is DispatcherSynchronizationContext` | **1** | 1 |

All four are identical to the baseline. That is the mechanical proof that the accessor still has
exactly five exits in the same source order: no exit was added, removed, or converted from one
shape to another.

## Condition 2 — the removed-line set has exactly two members

`(Select-String -Path coverage\logs\p2-t7-uithread.diff -Pattern '^-[^-]').Count` is **2**, and the
two members, by their trimmed text, are:

```
// The persistent UI context captured at Init() time.
if (ReferenceEquals(_context, _uiSyncContext))
```

These are exactly the comment and the condition line the plan names. The diff is therefore confined
to the `_uiSyncContext` condition and its explanatory comment.

## Condition 3 — no removed line touches any other exit or the initializer

The count of removed lines containing any of the six tokens is **0**:

| Token | Removed lines containing it |
|---|---|
| `ReferenceEquals(_context, ambient)` | 0 |
| `ambient is null` | 0 |
| `_uiThreadId == -1` | 0 |
| `DispatcherSynchronizationContext` | 0 |
| `lock (InitLock)` | 0 |
| `_initialized` | 0 |

This proves the diff altered neither the ambient-identity exit, the null-ambient exit, the thread-id
guard, the dispatcher exit, nor `UiThread.Init` and its initialization flag. The last two tokens are
the evidence AC8 cites for finding 4A: the initializer is untouched by this delivery.

None of the three conditions failed.

## The change itself, for the record

The single added hunk replaces the one-line condition with a three-conjunct condition and replaces
the one-line comment with one stating why the second proof is required:

```csharp
if (
    ReferenceEquals(_context, _uiSyncContext)
    && _dispatcher is not null
    && ReferenceEquals(
        System.Windows.Threading.Dispatcher.FromThread(Thread.CurrentThread),
        _dispatcher
    )
)
```

The dispatcher type is spelled fully qualified, matching the existing spelling in the dispatcher
exit below it, because inside the nested struct the simple name `Dispatcher` also names the
enclosing type's static property.

This check is repeated against the post-format tree by P4-T15, because the formatter may reflow the
file and a pre-format result does not describe the tree a reviewer will see.
