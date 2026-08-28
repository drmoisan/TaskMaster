# [P2-T12] — Publication Order of the Initialization Flag

Timestamp: 2026-08-27T20-34

Command:
```
$p = 'QuickFiler\Viewers\WebView2BreadcrumbHost.cs'
foreach ($t in @('core.WebMessageReceived +=', 'Volatile.Write(ref _isCoreInitialized', 'CoreInitialized?.Invoke')) {
    Select-String -SimpleMatch -Pattern $t -Path $p | ForEach-Object { $_.LineNumber; $_.Line }
}
```
(run through `pwsh -NoProfile` from the workspace root; the line number is read from the
`LineNumber` **property** of each returned `MatchInfo` — `Select-String` has no `-LineNumber` switch)

EXIT_CODE: 0

## Captured line numbers

| Statement | Line number | Line text |
| --- | --- | --- |
| `core.WebMessageReceived += OnWebMessageReceived;` | **311** | `core.WebMessageReceived += OnWebMessageReceived;` |
| `Volatile.Write(ref _isCoreInitialized, true);` | **316** | `Volatile.Write(ref _isCoreInitialized, true);` |
| `CoreInitialized?.Invoke(this, EventArgs.Empty);` | **317** | `CoreInitialized?.Invoke(this, EventArgs.Empty);` |

Each search returned exactly one match.

## Output Summary

The required strict ordering holds:

```
311 < 316 < 317
```

- The `core.WebMessageReceived` subscription (311) is strictly before the `Volatile.Write` (316).
- The `Volatile.Write` (316) is strictly before `CoreInitialized?.Invoke` (317).

`Volatile.Write` is a release store, so a reader that observes the flag through the acquire load in
`IsCoreInitialized` is guaranteed to observe the preceding subscription. The five lines between 311
and 316 are the comment recording that the pairing is load-bearing and that the three statements must
not be reordered. No statement was moved by this feature; the write replaced the plain assignment
in place.
