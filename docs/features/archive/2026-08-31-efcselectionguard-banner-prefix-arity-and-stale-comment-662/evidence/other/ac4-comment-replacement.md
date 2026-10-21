# AC4 — SelectedFolder Comment Replacement (P1-T5)

Timestamp: 2026-09-01T15-54

Command: `git grep -n -F -- 'IsValidSelection keeps its "====" rejection' -- QuickFiler/Controllers/EfcFormController.cs`

EXIT_CODE: 1

ExpectedExitCode: 1

This command asserts an absence, so exit 1 is the passing outcome. It returned
no output: the stale claim is gone from the file.

Command: `git grep -c -F -- 'three-character rejection' -- QuickFiler/Controllers/EfcFormController.cs`

EXIT_CODE: 0

Output Summary: the command reported a count of 1 —

```
QuickFiler/Controllers/EfcFormController.cs:1
```

Both figures hold, and together they are one pass/fail outcome.

## The comment as it stood before the edit (`:318-320`)

```csharp
            // Derived from the bridge router's selection tracking. The router never selects
            // "===="-banner rows, and IsValidSelection keeps its "====" rejection as a second
            // guard, so banner rows remain invalid filing targets.
```

## Full replacement comment text (`:318-320`)

```csharp
            // Derived from the bridge router's selection tracking. IsValidSelection routes to
            // IsSelectableFolder, which composes IsBannerRow, matching the producers' "===="
            // prefix, with the guard's deliberately broader three-character rejection.
```

The replacement describes the composition the code implements: `IsBannerRow`
matching the producers' four-character prefix, combined with the guard's
deliberately broader three-character rejection. It no longer asserts that
`IsValidSelection` keeps a four-character rejection. The token
`three-character rejection` sits on a single comment line, which is what the
count assertion reads.

## Scope containment

The change is comment-only. The `get => _router?.SelectedFolderPath;` accessor at
`:321` and everything outside `:318-320` are byte-identical. The replacement
occupies exactly three comment lines, the same count as the text it replaced, so
the file remains 1189 lines. `EfcFormController.cs` already exceeds the 500-line
limit in `.claude/rules/general-code-change.md`; this task did not widen that
condition.
