# ExcludeFromCodeCoverage Confirmation (issue #742, [P0-T10])

Timestamp: 2026-09-14T02-05

Command: `git grep -c -F '[ExcludeFromCodeCoverage]' -- QuickFiler/Controllers/QfcCollectionController.cs QuickFiler/Controllers/EfcItemController.cs`

EXIT_CODE: 0

Output Summary:

```
QuickFiler/Controllers/EfcItemController.cs:1
QuickFiler/Controllers/QfcCollectionController.cs:1
```

Acceptance: the command prints exactly one matching line for each of the two files and `EXIT_CODE`
is 0 — satisfied. `git grep` orders its output by pathspec resolution rather than by argument
order, so `EfcItemController.cs` precedes `QfcCollectionController.cs` here; both required lines are
present with a count of 1 each.

This confirms the coverage-signal framing used by [P0-T8] and [P5-T4]: both classes carry a
type-level `[ExcludeFromCodeCoverage]` attribute, are therefore absent from the Cobertura report,
and their quality signal is the named-test pass/fail state rather than a coverage percentage. This
change does not add, remove, or modify either attribute.
