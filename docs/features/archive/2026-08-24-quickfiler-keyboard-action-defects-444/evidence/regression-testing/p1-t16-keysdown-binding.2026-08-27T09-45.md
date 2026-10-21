# [P1-T16] Source-search confirmation of the surviving `Keys.Down` binding

Timestamp: 2026-08-27T09-45
EXIT_CODE: 0

## Command (verbatim)

```powershell
@(Select-String -SimpleMatch -Pattern 'Keys.Down, (k) => SelectNextItemAsync()' -Path QuickFiler\Controllers\QfcCollectionController.cs).Count
```

The asserted literal is `Keys.Down, (k) => SelectNextItemAsync()`, quoted verbatim in the plan's
conventions section. Fixed-string matching is used (`-SimpleMatch`), so the parentheses and the `=>`
are matched literally rather than as regular-expression syntax.

## Observed

```
COUNT = 1
HIT line 1131: new KaKeyAsync("Collection", Keys.Down, (k) => SelectNextItemAsync()),
```

The single occurrence is inside `RegisterAsyncKeyActions`, the live registration reached from
`WireUpAsyncKeyboardHandler`. `Keys.Down` on the QuickFiler collection surface therefore means
`SelectNextItem()`, exactly as the recorded product decision states.

## Acceptance evaluation

- The recorded count is exactly `1`. PASS.

This literal carries a `>` character, so plan-acceptance-gate rules G5 and G6 skip it and the plan
validator performs no tree-resolution check on it. The authoritative assertion for this binding is the
named test at `[P1-T15]`, which passed; this search is corroborating evidence at the source level.

Output Summary: exactly one occurrence of the asserted literal, at line 1131 inside
`RegisterAsyncKeyActions`.
