# P0-T2 — Re-derivation of the defect site in UtilitiesCS/Threading/UiThread.cs

Timestamp: 2026-09-03T08-20

Command:
```text
cat -n UtilitiesCS/Threading/UiThread.cs
wc -l UtilitiesCS/Threading/UiThread.cs
```

EXIT_CODE: 0

The field carries a single integer. Both commands this task ran exited 0.

## Output Summary

- Total line count: **163** (`wc -l`, physical newline count; the `cat -n` listing likewise ends at
  line 163).
- Nullable-enable directive: present on **line 1**, spelled `#nullable enable` (the file carries a
  UTF-8 BOM immediately before it).
- `Dispatcher` property: **lines 135-139**.

  ```csharp
          public static Dispatcher Dispatcher
          {
              get => _dispatcher;
              private set => _dispatcher = value;
          }
  ```

- Backing field: **line 140**, verbatim:

  ```csharp
          private static Dispatcher _dispatcher = null!; // set in Initialize() before any access
  ```

  The line contains the null-forgiving suppression `null!`.

- Lazy-initialising sibling properties, recorded for contrast with the `Dispatcher` accessor:
  - `UiSyncContext` — **lines 113-125**; its getter calls `Init()` when `_uiSyncContext is null` and
    then returns `_uiSyncContext!`. Backing field `_uiSyncContext` at line 126.
  - `AutoScaleFactor` — **lines 147-158**; its getter calls `Init()` when `_autoScaleFactor is null`
    and returns `_autoScaleFactor ?? new System.Drawing.SizeF(1f, 1f)`. Backing field
    `_autoScaleFactor` at line 159.

  `Dispatcher` is the only one of the three that neither lazily initialises nor guards: its getter is
  the bare expression body `get => _dispatcher;`, so on a worker thread that reaches it before
  `Initialize()` has run it returns `null` silently, which is the defect this plan fixes.

- Assignment site: `Dispatcher = _syncContextForm.UiDispatcher;` at line 61, inside `Initialize()`.

All five values asserted by P0-T2 match: total line count 163; backing-field line contains `null!`;
property at lines 135-139; field at line 140; nullable-enable directive on line 1. No BLOCKED
condition applies.
