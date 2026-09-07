# [P2-T9] Format-selection formulation gate

Timestamp: 2026-08-27T09-45
EXIT_CODE: 0

## Command (verbatim)

```powershell
@(Select-String -SimpleMatch -Pattern '_registeredDigits == 2 ? "00" : ""' -Path QuickFiler\Controllers\QfcCollectionController.cs).Count
```

The asserted literal is `_registeredDigits == 2 ? "00" : ""`, quoted verbatim in the plan's
conventions section. `-SimpleMatch` is used so the `?`, the quotes, and the `:` are matched literally
rather than as regular-expression syntax.

## Observed

```
COUNT = 1
line 1188: var format = _registeredDigits == 2 ? "00" : "";
```

The single occurrence is a complete statement on one physical line, hoisted above the removal loop.
Writing it as a standalone local declaration rather than inline in the `Remove(...)` argument is
load-bearing: inline, the line would exceed CSharpier's 100-column print width, CSharpier would wrap
the conditional across lines, and this single-line literal would cease to exist after `[P2-T13]`
formats the file. `[P2-T13]` re-asserts this count after the mutating format pass for exactly that
reason.

## Why this formulation and not `== 1 ? "" : "00"`

Tests build the controller with `FormatterServices.GetUninitializedObject`, which bypasses field
initialisers, so a test-built controller sees `_registeredDigits == 0`. Treating anything that is not
2 as single-digit means such a controller behaves as width 1, which is what the four pre-existing
navigation tests in `QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs` assume. The inverted
formulation would have made a 0-valued field behave as width 2 and would have broken them.

## Acceptance evaluation

- The recorded count is exactly `1`. PASS.

Output Summary: exactly one occurrence of the asserted literal, at line 1188, as a complete
single-line statement.
