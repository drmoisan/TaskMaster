# QA Gate — Post-Merge Base Reconciliation (resumed run)

Timestamp: 2026-08-27T23-31

Command: `git fetch origin epic/quickfiler-bug-family-integration; git merge origin/epic/quickfiler-bug-family-integration --no-edit; git rev-list --left-right --count HEAD...origin/epic/quickfiler-bug-family-integration`

EXIT_CODE: 0

Output Summary: the integration tip moved to `69e83171` while this branch was down, adding merged
features 493 (PR #653) and 444 (PR #654). The branch was 1 ahead / 28 behind before the merge and is
**0 behind** after it. The merge produced no conflicts and is recorded as a real merge commit.

## Pure-deletion audit

Command: `git diff --numstat origin/epic/quickfiler-bug-family-integration..HEAD | awk '$1==0 && $2>0'`

Exactly one file is a pure deletion:

```
0	50	QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs
```

This is NOT content the base gained. Proof, in two independent steps:

1. The base never touched the file. `git log --oneline <merge-base>..origin/epic/quickfiler-bug-family-integration -- QuickFiler/Viewers/Breadcrumb*.cs` returns **zero commits**, so no merged sibling wrote any `Viewers/Breadcrumb*` file.
2. The 50 removed lines are the SR-1 partial split. They are `SetSuggestions`, `SuggestionsUpgrade`,
   `PopulateSuggestionsAsync` and `AddItems`, all of which now live in
   `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.Suggestions.cs`. The split pair's COMBINED coverage
   is 295/295 lines, so no line was lost in the move.

The invariant "no file may lose content the BASE gained" therefore holds.

## Project-file region ownership

`git diff origin/epic/quickfiler-bug-family-integration..HEAD -- QuickFiler/QuickFiler.csproj QuickFiler.Test/QuickFiler.Test.csproj`
shows **exactly two added lines and zero removed lines**:

| Project file | Added line | Region |
| --- | --- | --- |
| `QuickFiler/QuickFiler.csproj` | `<Compile Include="Viewers\BreadcrumbBridgeCoordinator.Suggestions.cs" />` | `Viewers\Breadcrumb*` (owned) |
| `QuickFiler.Test/QuickFiler.Test.csproj` | `<Compile Include="Viewers\BreadcrumbBridgeCoordinatorSupersessionTests.cs" />` | `Viewers\Breadcrumb*` (owned) |

Merged siblings' entries are preserved verbatim and unmoved:

- sibling 493: `Controllers\QfcItemController.UiThreadDispatcherFixture.cs` and
  `Controllers\QfcItemController.UiThreadDispatcherFixtureTests.cs` both present.
- sibling 444: 8 `Controllers\QfcCollectionController*` entries in `QuickFiler.Test.csproj` and 2 in
  `QuickFiler.csproj`, all present.

No entry was reordered, replaced, or dropped. No line outside `Viewers\Breadcrumb*` was written.
