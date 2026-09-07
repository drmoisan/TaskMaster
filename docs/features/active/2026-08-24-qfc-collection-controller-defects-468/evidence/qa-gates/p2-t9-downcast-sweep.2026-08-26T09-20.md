# [P2-T9] AC-14 search half — the `(QfcFormController)_parent` downcast is gone

Timestamp: 2026-08-26T09-20

Command:

```
grep -c -F '(QfcFormController)_parent' QuickFiler/Controllers/QfcCollectionController.cs
```

`-F` selects fixed-string matching so the parentheses are literal rather than regular-expression
grouping. The search is scoped to the single owned file, per this plan's
`### Literals asserted by acceptance conditions` convention: the identifier legitimately appears in
`docs/features/**` prose (including this plan and the P0-T15 baseline artifact), so a
repository-wide zero-hit gate would be unsatisfiable by construction.

EXIT_CODE: 1

`grep -c` exits 1 when the match count is zero. That non-zero exit **is** the passing signal for this
gate; the measured hit count is `0`.

## Output Summary

```
0
```

### Acceptance verification — baseline contrast

| File | Literal | P0-T15 baseline count | Post-P2-T7 count |
|---|---|---|---|
| `QuickFiler/Controllers/QfcCollectionController.cs` | `(QfcFormController)_parent` | **1**, at `:1232` | **0** |

The P0-T15 baseline artifact records the single occurrence verbatim at section
`### 4. (QfcFormController)_parent hit count`:

```
1232:                    await ((QfcFormController)_parent).SkipGroupAsync();
```

P1-T2's dead-code removal renumbered the file, moving that statement to `:1025`. P2-T7 replaced it
with `await _parent.SkipGroupAsync();`, which is now a direct interface call: `_parent` is declared
`IQfcFormController` and `QuickFiler.Controllers.IQfcFormController` declares
`Task SkipGroupAsync();` at `QuickFiler/Controllers/IQfcFormController.cs:38`. The runtime downcast
to the concrete `QfcFormController` has been replaced by a compile-time constraint.

The baseline count is non-zero and the post-fix count is zero, so this gate is discriminating: it
would have failed had P2-T7 not been applied.

Result: PASS.
