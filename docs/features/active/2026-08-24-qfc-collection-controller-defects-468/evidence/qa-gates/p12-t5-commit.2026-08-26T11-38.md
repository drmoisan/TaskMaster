# [P12-T5] Commit of the issue #469 defect 4 documentation change

Timestamp: 2026-08-26T11-38

Command:

```
git add -- QuickFiler/Interfaces/IQfcCollectionController.cs            QuickFiler/Controllers/QfcCollectionController.cs            QuickFiler.Test/Controllers/QfcCollectionControllerDefects468MoveTests.cs
git commit -m "docs(469): document the retained stackMovedItems contract and consume the parameter"
git show --name-only HEAD
```

EXIT_CODE: 0

ExpectedExitCode: 0

## Output Summary

Commit `613e88c3bf59d83c3539df163318b97bcb8bf892` —
`docs(469): document the retained stackMovedItems contract and consume the parameter`.

`git show --name-only HEAD` path list, verbatim and complete:

```
QuickFiler.Test/Controllers/QfcCollectionControllerDefects468MoveTests.cs
QuickFiler/Controllers/QfcCollectionController.cs
QuickFiler/Interfaces/IQfcCollectionController.cs
```

Exactly three paths: the interface, `<CTRL>`, and the move test file. **No other path.** The task's
acceptance is met literally — this commit deliberately excludes the plan, the AC source, and the
Phase 12 evidence artifacts, which would otherwise have violated the "no other path" clause. Those
files stay uncommitted and are absorbed by the Phase 13 fix commit (P13-T8), consistent with the
plan's commit cadence; no extra unplanned commit was created for them.

## What changed, by file

| File | Change |
|---|---|
| `QuickFiler/Interfaces/IQfcCollectionController.cs` | one XML doc block on `MoveEmailsAsync`. Signature and parameter list unchanged and byte-identical to the base commit; no member added or removed. |
| `QuickFiler/Controllers/QfcCollectionController.cs` | the mirrored XML doc block plus a single `_ = stackMovedItems;` discard. Signature unchanged. |
| `QuickFiler.Test/Controllers/QfcCollectionControllerDefects468MoveTests.cs` | one test, `MoveEmailsAsync_WithNullStack_BehavesIdenticallyToAnEmptyStack`, plus the `SloStack` namespace import and one shared reason constant. |

Per D11 the parameter was **not** removed and
`QuickFiler/Controllers/QfcFormController.EventHandlers.cs` was **not** edited. Both files named as
must-not-touch by the scope lock — `KbdActions.cs` and `EfcFormController.cs` — are likewise absent.

## Staging hygiene

The `git add` used an explicit three-path pathspec. `.claude/agent-memory/**` and
`.claude/state/**` remain unstaged and uncommitted.
