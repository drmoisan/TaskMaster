# [P2-T8] `Digits` zero-read gate on the post-fix `UnregisterNavigation` body

Timestamp: 2026-08-27T09-45
EXIT_CODE: 0

## Command (verbatim, the `[P0-T14]` extraction re-run unchanged)

```powershell
$lines = Get-Content QuickFiler\Controllers\QfcCollectionController.cs
$start = (@($lines | Select-String -SimpleMatch 'public void UnregisterNavigation()') |
  Select-Object -First 1).LineNumber
$end = (@($lines | Select-String -SimpleMatch 'internal void RegisterNavigationAsyncAction') |
  Select-Object -First 1).LineNumber
$body = $lines[($start - 1)..($end - 2)]
@($body | Select-String -CaseSensitive -Pattern '\bDigits\b').Count
```

The `-CaseSensitive` switch and the word-boundary pattern `\bDigits\b` are both retained from
`[P0-T14]`, per decision D-P3. The pattern does **not** match `_registeredDigits`, because the
character preceding the `D` is `e`, a word character, so no word boundary exists at that position.
`-CaseSensitive` is required because `Select-String` matches case-insensitively by default and an
uncased `\bDigits\b` would also match a local variable named `digits`.

## Observed slice bounds

| Measure | Value |
| --- | --- |
| `$start` (`public void UnregisterNavigation()`) | 1184 |
| `$end` (`internal void RegisterNavigationAsyncAction`) | 1195 |
| `$start` strictly less than `$end` | True |

## Extracted post-fix body (verbatim)

```csharp
        public void UnregisterNavigation()
        {
            // Issue #472: replay the recorded registration width; re-reading the live width property
            // would remove keys this page never registered. Non-2 means width 1, so a field of 0 does.
            var format = _registeredDigits == 2 ? "00" : "";
            for (int i = 0; i < _itemGroups.Count; i++)
            {
                _kbdHandler.StringActionsAsync.Remove("Collection", (i + 1).ToString(format));
            }
        }
```

## Counts

```
BaselineDigitsReads (from [P0-T14]) = 1
Post-fix count                      = 0
```

The explanatory comment above the `format` declaration deliberately refers to "the live width
property" rather than naming the property, so the comment prose does not register as a match. An
earlier draft of this comment named the property twice and drove this count to 2 while the code itself
read it zero times; the comment was reworded rather than the gate relaxed, because a mention in a
comment is not a read and the gate should measure reads.

## Acceptance evaluation

- The recorded `$start` (1184) is strictly less than the recorded `$end` (1195). PASS.
- The post-fix count is exactly `0`. PASS.
- The recorded baseline value (1) is greater than `0`. PASS.

The gate is non-vacuous: the same command over the same slice returned 1 before the fix and 0 after it.

Output Summary: post-fix `Digits` read count in the `UnregisterNavigation` body is 0 against a baseline
of 1; slice bounds forward at 1184..1194; all three acceptance conditions met.
