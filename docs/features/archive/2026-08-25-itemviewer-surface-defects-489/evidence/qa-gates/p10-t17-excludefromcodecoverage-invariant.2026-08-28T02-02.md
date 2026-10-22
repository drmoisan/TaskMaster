# P10-T17 — Coverage-exemption invariant on the viewer partials

Timestamp: 2026-08-28T02-02
Command: sed -n '20p' QuickFiler/Viewers/ItemViewer.cs ; git show cecd78130a489fcfdc2ddac7970f344256f4a75a:QuickFiler/Viewers/ItemViewer.cs | sed -n '20p' ; git diff cecd78130a489fcfdc2ddac7970f344256f4a75a -- QuickFiler/ QuickFiler.Test/ | grep for added and removed lines matching ExcludeFromCodeCoverage ; (git grep -n -E "\[(System\.Diagnostics\.CodeAnalysis\.)?ExcludeFromCodeCoverage\]" -- "*.cs" | Measure-Object).Count
EXIT_CODE: 0

`BASELINE_SHA` is `cecd78130a489fcfdc2ddac7970f344256f4a75a`.

## `QuickFiler/Viewers/ItemViewer.cs:20` still carries `[ExcludeFromCodeCoverage]`, unchanged

Working tree, lines 18 through 22:

```
namespace QuickFiler
{
    [ExcludeFromCodeCoverage]
    public partial class ItemViewer : UserControl, IItemViewer, IContainerControlLocal
    {
```

Line 20 of the baseline blob reads `    [ExcludeFromCodeCoverage]` — byte-identical to line 20 of the
working tree.

The citation is **not** a stale-shift risk. `ItemViewer.cs:20` sits above every deletion this plan
makes in that file — P2-T4 deletes at `:171-187` and `:205`, P4-T1 at `:166-169`, P6-T3 at `:27` and
`:65-69`, all of them below `:20` in file order — so its line number is fixed for the whole plan. The
plan's § Fact base names this as the single stated exception to its "no printed line number is
asserted" rule, precisely because it is stable.

## No new coverage-exclusion attribute anywhere, in either spelling

| Direction | Search over `git diff <BASELINE_SHA> -- QuickFiler/ QuickFiler.Test/` | Result |
|---|---|---|
| Added lines (`^+`) matching `ExcludeFromCodeCoverage` | zero matches | **No attribute added** |
| Removed lines (`^-`) matching `ExcludeFromCodeCoverage` | zero matches | **No attribute removed** |

The search matches the bare substring `ExcludeFromCodeCoverage`, so it catches **both** spellings: the
unqualified `[ExcludeFromCodeCoverage]` and the fully-qualified
`[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]` that dominates
`QuickFiler/Controllers/QfcItemController.EventHandlers.cs`, the file P2-T5 edits. Neither appears in
an added or a removed line anywhere in the 25-path diff.

In particular:

| Type | Exclusion attributes present |
|---|---|
| `QuickFiler/Viewers/ItemViewerExpanded.cs` | **0** |
| `QuickFiler/Viewers/ToolStripMenuItemCb.cs` | **0** |

Neither received one, as the task requires.

## Two-spelling numeric proof

The alternation pattern P0-T16 established is
`git grep -n -E "\[(System\.Diagnostics\.CodeAnalysis\.)?ExcludeFromCodeCoverage\]" -- "*.cs"`, which
matches both spellings. Its baseline reading is recorded in
`FEATURE/evidence/baseline/phase0-excludefromcodecoverage-count.2026-08-27T23-31.md` as
`BaselineExcludeAttributeCount: 261`.

| Reading | Count |
|---|---|
| P0-T16 baseline | **261** |
| This measurement | **261** |

Unchanged. A fixed-string count of the unqualified spelling alone would understate the true total and
could not observe a fully-qualified addition, which is why the alternation form is used.

**P11-T10 is the authoritative recount** using this same alternation pattern, and this artifact cites
it as the plan directs. The reading above is a Phase 10 confirmation, not a substitute for it.

## Why per-defect proof is a named test, not a coverage delta

Every `ItemViewer*.cs` change this feature makes lands inside a type carrying
`[ExcludeFromCodeCoverage]` at `ItemViewer.cs:20`, or inside `ItemViewerExpanded` and
`ItemViewer.Designer.cs`, which are WinForms Designer and form-derived surfaces. Lines inside an
exempt type are removed from the coverage denominator, so a coverage figure for them is constant
whatever the executor writes.

A coverage-delta claim over an exempt type would therefore be an **acceptance condition that cannot
fail** — the defect `.claude/rules/plan-acceptance-gates.md` exists to reject. For that reason the
per-defect proof for every `ItemViewer*.cs` change in this feature is a **named test**, not a coverage
figure:

| Defect | `ItemViewer*` change | Proof |
|---|---|---|
| #486 D1 | `ToolStripMenuItemCb` shadowed `Checked` setter | `Checked_WhenSetTrue_AssignsCheckedCheckBoxImage`, `Checked_WhenSetFalse_AssignsNullImage`, `Checked_WhenSetTrue_RaisesShadowedCheckedChangedExactlyOnce` |
| #486 D2 | dead twin handler removed from `ItemViewerExpanded.cs` | `ItemViewerExpanded_DeclaresNoMenuItemCheckedChangedHandler`, `ItemViewer_DeclaresNoMoveOptionsMenuClickHandler` |
| #487 D1 | dead `ParentChanged` handlers removed | `ItemViewer_DeclaresNoParentChangedHandler`, `ItemViewerExpanded_DeclaresNoParentChangedHandler` |
| #489 D4 | `UiScheduler` seam deleted | `IItemViewer_DeclaresNoUiSchedulerMember`, `IItemViewer_StillDeclaresUiDispatcher`, `IItemViewer_StillDeclaresUiSyncContext` |
| #490 D1 | `SetFolderItems` renamed to `AddFolderItems` in `ItemViewer.FolderSearch.cs` | `IItemViewer_DeclaresAddFolderItemsAndNotSetFolderItems` |
| #490 D2 | `FocusSearch()` made a bare forward in `ItemViewer.FolderSearch.cs` | fail-before exception dossier plus the P9-T1 = 1 / P9-T8 = 0 `TxtboxSearch.Invoke` count pair, and `JumpToSearchTextbox_TogglesKeyboardAndFocusesSearch` staying green |
| #490 D3 | `FocusSubject()` returns `bool` in `ItemViewer.DisplayState.cs` | `IItemViewer_FocusSubjectReturnsBool`, `Expand_WhenFocusSubjectReturnsFalse_StillEnumeratesConversation` |

Every one is a reflection or interaction assertion that fails if the corresponding source change is
absent, so each is genuinely falsifiable in a way a coverage percentage over an exempt type is not.

## Acceptance

| P10-T17 condition | Result |
|---|---|
| `QuickFiler/Viewers/ItemViewer.cs:20` still carries `[ExcludeFromCodeCoverage]` unchanged | Met — byte-identical to the baseline blob |
| No new coverage-exclusion attribute anywhere, in either spelling | Met — zero added lines matching `ExcludeFromCodeCoverage` across the 25-path diff |
| None added to `ItemViewerExpanded` or `ToolStripMenuItemCb` | Met — both carry zero |
| The two-spelling numeric proof cites P11-T10's recount using P0-T16's alternation pattern | Met — cited above; a Phase 10 confirmation reading of **261** matches the **261** baseline |
| The summary states that per-defect proof for every `ItemViewer*.cs` change is a named test | Met — with the per-defect table above |

Output Summary: The coverage-exemption invariant **holds**.
`QuickFiler/Viewers/ItemViewer.cs:20` still carries `[ExcludeFromCodeCoverage]`, byte-identical to the
baseline blob, and its line number is stable because it sits above every deletion this plan makes in
that file. **No new coverage-exclusion attribute was added anywhere in either spelling**: a search of
the added lines of the whole 25-path diff for the substring `ExcludeFromCodeCoverage` — which catches
both the unqualified form and the fully-qualified
`[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]` used in
`QfcItemController.EventHandlers.cs` — returns zero, and none was removed either.
`ItemViewerExpanded` and `ToolStripMenuItemCb` each carry zero. The two-spelling alternation count
that P0-T16 established reads **261**, unchanged from its **261** baseline; **P11-T10's recount using
that same pattern is the authoritative proof** and is cited here. Per-defect proof for every
`ItemViewer*.cs` change is a **named test**, not a coverage delta, because a coverage claim over a
type carrying `[ExcludeFromCodeCoverage]` would be an acceptance condition that cannot fail.
