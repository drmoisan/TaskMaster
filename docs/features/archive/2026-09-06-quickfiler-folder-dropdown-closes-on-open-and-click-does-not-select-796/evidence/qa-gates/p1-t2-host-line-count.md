# P1-T2 — BreadcrumbDropDownHost.cs physical line count after the handler move

Timestamp: 2026-09-07T14-17
Task: [P1-T2]
Issue: #796
Channel used: A

Command:

```
pwsh -NoProfile -Command '(Get-Content -LiteralPath QuickFiler\Viewers\BreadcrumbDropDownHost.cs).Count'
```

Idiom used: `(Get-Content -LiteralPath $_).Count`, which is the idiom recorded on the
`LINE-COUNT-IDIOM:` line of evidence/baseline/p0-t12-file-size-baseline.md and no
other.

EXIT_CODE: 0

MEASURED-PHYSICAL-LINES: 485

## Band evaluation

| Quantity | Value |
|---|---|
| Baseline physical lines (P0-T12) | 498 |
| Handler physical lines removed (426 through 437 inclusive) | 12 |
| Separating blank line removed (438) | 1 |
| Expected physical result | 485 |
| Admitted band | 480 through 486 inclusive |
| Measured | 485 |
| Verdict | inside the band, and equal to the expected value |

The band must not be evaluated against a blank-line-omitting count, which would
report roughly 441 here and fail a correct move. The idiom recorded above counts
physical lines including blank lines.

## Corroborating observations

- `DropDown.Closed += OnDropDownClosed;` is still present in the main part: 1 match.
  The subscription stays put and continues to bind after the move, because both parts
  declare the same `sealed partial class BreadcrumbDropDownHost`.
- `private void OnDropDownClosed` no longer appears in the main part: 0 matches.
- `FinishClose` was not touched by this edit; the removal ended immediately before its
  declaration.

This is a pure move. The removed body is byte-identical to the body relocated in
P1-T1 apart from the log statement P1-T1 added ahead of it.

Output Summary: BreadcrumbDropDownHost.cs measures 485 physical lines after the move,
equal to the expected value and inside the 480-486 band. Subscription preserved,
handler declaration absent from the main part.
