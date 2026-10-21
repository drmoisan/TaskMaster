# [P0-T14] Pre-fix `Digits` read count inside `UnregisterNavigation`

Timestamp: 2026-08-27T09-45
EXIT_CODE: 0

## Command (verbatim, run from `WS` under `pwsh -NoProfile`)

```powershell
$lines = Get-Content QuickFiler\Controllers\QfcCollectionController.cs
$start = (@($lines | Select-String -SimpleMatch 'public void UnregisterNavigation()') |
  Select-Object -First 1).LineNumber
$end = (@($lines | Select-String -SimpleMatch 'internal void RegisterNavigationAsyncAction') |
  Select-Object -First 1).LineNumber
$body = $lines[($start - 1)..($end - 2)]
@($body | Select-String -CaseSensitive -Pattern '\bDigits\b').Count
```

The word-boundary pattern `\bDigits\b` is required by decision D-P3: the field this feature adds is
named `_registeredDigits`, which contains `Digits` as a substring, so a substring search could not
express "zero reads of the `Digits` property". `-CaseSensitive` is required because `Select-String`
matches case-insensitively by default and an uncased `\bDigits\b` would also match a local variable
named `digits`.

## Observed slice bounds

| Measure | Value |
| --- | --- |
| `$start` (`public void UnregisterNavigation()`) | 1180 |
| `$end` (`internal void RegisterNavigationAsyncAction`) | 1195 |
| `$start` strictly less than `$end` | True |

The forward ordering confirms the slice is the `UnregisterNavigation` body and not a reversed range
produced by a post-#468 member reordering.

## Extracted body (verbatim)

```csharp
        public void UnregisterNavigation()
        {
            for (int i = 0; i < _itemGroups.Count; i++)
            {
                if (Digits == 1)
                {
                    _kbdHandler.StringActionsAsync.Remove("Collection", (i + 1).ToString());
                }
                else
                {
                    _kbdHandler.StringActionsAsync.Remove("Collection", (i + 1).ToString("00"));
                }
            }
        }
```

## Baseline figure

```
BaselineDigitsReads = 1
```

The single read is the per-iteration `if (Digits == 1)` inside the loop — exactly the defect #472
files. The read is inside the loop bound by `_itemGroups.Count`, so it is re-evaluated once per
iteration at run time even though it appears once in source.

## Acceptance evaluation

- `$start` (1180) is strictly less than `$end` (1195). PASS.
- `BaselineDigitsReads` is recorded and is greater than `0` (it is `1`). PASS.

This is the false-before half of the `[P2-T8]` gate. A zero baseline would have made that gate vacuous
and would itself have been a failure of this task.

Output Summary: slice bounds 1180..1194 (forward); `BaselineDigitsReads = 1`; both acceptance
conditions met; the `[P2-T8]` zero-read gate is therefore non-vacuous.
