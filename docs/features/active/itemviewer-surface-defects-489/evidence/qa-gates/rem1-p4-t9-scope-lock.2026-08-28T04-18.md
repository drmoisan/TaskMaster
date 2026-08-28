# P4-T9 — Scope-lock re-verification (Phase 4, loop iteration 1)

Timestamp: 2026-08-28T04-18
Task: [P4-T9]
LoopIteration: 1
Command: git diff --name-only cecd7813..HEAD (filtered to .cs .csproj .props .targets .config) and git diff d77ac212..HEAD -- "*.csproj"
EXIT_CODE: 0

## (a) The set is unchanged — still exactly 25 paths, no new source or project path

`git diff --name-only cecd7813..HEAD`, filtered to paths ending in `.cs`, `.csproj`, `.props`,
`.targets` or `.config`, returns **25** paths. Compared line-for-line against the `ScopeLockPaths:`
list recorded in
`FEATURE/evidence/remediation-baseline/rem1-phase0-repo-state.2026-08-28T03-40.md`, the two sets are
**identical**: zero additions, zero removals.

```
QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.Part2.cs
QuickFiler.Test/Controllers/QfcItemController.EventWiringTests.cs
QuickFiler.Test/Controllers/QfcItemController.FolderHandlingTests.cs
QuickFiler.Test/Controllers/QfcItemController.FolderSuggestionsTests.cs
QuickFiler.Test/Controllers/QfcItemController.MailActionsTests.Part2.cs
QuickFiler.Test/Controllers/QfcItemController.MailActionsTests.cs
QuickFiler.Test/Controllers/QfcItemController.SeamDispatcherTests.cs
QuickFiler.Test/Controllers/QfcItemController.ThemeMarshallingTests.cs
QuickFiler.Test/QuickFiler.Test.csproj
QuickFiler.Test/Viewers/BreadcrumbDropDownIntegrationTests.cs
QuickFiler.Test/Viewers/BreadcrumbSelectorOpenRetryTests.cs
QuickFiler.Test/Viewers/ItemViewerBreadcrumbDropDownContractTests.cs
QuickFiler.Test/Viewers/ToolStripMenuItemCbTests.cs
QuickFiler/Controllers/QfcItemController.EventHandlers.cs
QuickFiler/Controllers/QfcItemController.EventWiring.cs
QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs
QuickFiler/Controllers/QfcItemController.FolderHandling.cs
QuickFiler/Controllers/QfcItemController.MailActions.cs
QuickFiler/Viewers/IItemViewer.cs
QuickFiler/Viewers/ItemViewer.Designer.cs
QuickFiler/Viewers/ItemViewer.DisplayState.cs
QuickFiler/Viewers/ItemViewer.FolderSearch.cs
QuickFiler/Viewers/ItemViewer.cs
QuickFiler/Viewers/ItemViewerExpanded.Designer.cs
QuickFiler/Viewers/ItemViewerExpanded.cs
```

**No new source path entered the diff.** Both files this remediation edited were already inside the
set before it began — `QfcItemController.EventWiring.cs` at row 15 and
`QfcItemController.EventWiringTests.Part2.cs` at row 1 — so the remediation deepened two existing
entries rather than widening the set. That is the strongest form of this gate: the count and the
membership are both unchanged, not merely the count.

## (b) None of the named off-limits paths appears

| Path | Exact matches in the set |
|---|---:|
| `QuickFiler/Viewers/ItemViewer.Breadcrumb.cs` (488-owned) | **0** |
| `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs` (501-owned) | **0** |
| `QuickFiler/Controllers/QfcItemController.Navigation.cs` (444-owned) | **0** |
| `QuickFiler.Test/Controllers/QfcItemController.TestSupport.cs` (493-owned) | **0** |
| Any `UtilitiesCS/` path | **0** |

The P0-T2 set contained no `UtilitiesCS/` path, so the "no `UtilitiesCS/` path beyond those already in
the P0-T2 set" condition has an empty allowance and reduces to "no `UtilitiesCS/` path at all". There
is none.

## (c) No project file was touched by this remediation

```
git diff d77ac212..HEAD -- "*.csproj"
```

Output: **empty**. Not one byte of any `.csproj` changed since REM_BASE. In particular no analyzer
`<Analyzer Include>` entry and no `packages.config` reference was altered, so the pre-existing
repo-wide stale analyzer HintPath skew recorded as out-of-scope finding E1 is untouched and remains
out of scope, exactly as the plan requires.

`QuickFiler.Test/QuickFiler.Test.csproj` does appear in the 25-path **feature** set at row 9, because
the feature added `<Compile Include>` entries for its new continuation files earlier in its life. That
is pre-existing feature history from before REM_BASE, not a change made by this remediation, and the
REM_BASE-scoped diff above proves the distinction: the file is unchanged since REM_BASE. The new test
required no `.csproj` edit because `Controllers\QfcItemController.EventWiringTests.Part2.cs` was
already a `<Compile Include>` entry at `QuickFiler.Test.csproj:172`.

## Acceptance

| P4-T9 condition | Result |
|---|---|
| (a) the set equals the P0-T2 set exactly — same 25 paths, no new source or project path | **Yes** — 25 paths, sets identical, 0 added, 0 removed |
| (b) no entry for the four named paths, and no `UtilitiesCS/` path beyond the P0-T2 set | **Yes** — 0 for each of the four, 0 `UtilitiesCS/` paths |
| (c) `git diff REM_BASE..HEAD -- "*.csproj"` is empty | **Yes** — empty |

Output Summary: The scope lock holds. `git diff --name-only cecd7813..HEAD` filtered to code and
project extensions still returns **exactly 25** paths, and a line-for-line comparison against the
P0-T2 `ScopeLockPaths:` list shows the two sets are **identical** — zero additions, zero removals — so
this remediation added **no new source or project path** and merely deepened two entries already in
the set. None of the four off-limits sibling-owned paths appears (0 matches each), and no `UtilitiesCS/`
path appears at all. `git diff REM_BASE..HEAD -- "*.csproj"` is **empty**: no project file, and in
particular no analyzer entry, was touched by this remediation. `EXIT_CODE: 0`.
