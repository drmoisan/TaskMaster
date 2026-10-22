# P1-T9 — Split invariants

Timestamp: 2026-09-13T15-21
Command: a line-oriented literal search of each of the three paths for the tokens `#nullable`, `#region` and `#endregion`, counting matching lines
EXIT_CODE: 0
Output Summary: Neither new partial part carries a nullable pragma, and in all three files the
`#region` count equals the `#endregion` count, so every region opens and closes on the same side of
the split.

## Nullable-pragma invariant

The search counts lines containing the literal token `#nullable`.

- QuickFiler/Controllers/QfcQueue.Tlp.cs: 0 matches
- QuickFiler/Controllers/QfcQueue.UiIdle.cs: 0 matches

Both are zero, which is this task's acceptance condition. QuickFiler/Controllers/QfcQueue.cs also
measures 0, recorded here for completeness although this clause names only the two new files.

## Region-balance invariant

| Path | `#region` count | `#endregion` count | Equal |
|---|---|---|---|
| QuickFiler/Controllers/QfcQueue.cs | 3 | 3 | yes |
| QuickFiler/Controllers/QfcQueue.Tlp.cs | 1 | 1 | yes |
| QuickFiler/Controllers/QfcQueue.UiIdle.cs | 1 | 1 | yes |

The base part retained three of its five regions — Constructors and Private Members, Queue Functions
and INotify — and the two relocated ones account for the single balanced pair in each new file.

Note on the counting method: `#endregion` lines were counted with their own literal rather than by
subtracting, and the `#region` count is taken over the bare `#region` token, which is not a substring
of `#endregion`, so the two counts are independent measurements rather than one derived from the other.
