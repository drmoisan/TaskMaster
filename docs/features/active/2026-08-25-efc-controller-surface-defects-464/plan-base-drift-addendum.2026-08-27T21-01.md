# Plan base-drift addendum — efc-controller-surface-defects-464

- **Authored:** 2026-08-27T21-01 (UTC, `date -u` immediately before this write)
- **Authored by:** orchestrator (child of epic `quickfiler-bug-family`)
- **Applies to:** `docs/features/active/efc-controller-surface-defects-464/plan.2026-08-25T07-01.md`
- **Actual execution base:** `bug/efc-controller-surface-defects-464` branched from
  `origin/epic/quickfiler-bug-family-integration` at **`69e8317152c0a9ee6ee6e65db0ef81f6906189b1`**

## Authority of this addendum

The approved plan and `spec.md` are unchanged and remain authoritative for **intent, remedy class, and
acceptance criteria**. This addendum corrects **pre-change file:line locators and two factual premises**
that the plan recorded against commit `49199503` / `036a205d` / `2300becf`, none of which is the base this
run executes on. Where this addendum and the plan disagree about **where a thing is on disk**, this
addendum wins, because it was re-read from the actual base. Where they disagree about **what to do**, the
plan wins.

`spec.md` criterion text is unmodifiable (plan `[P11-T15]`). This addendum modifies no criterion.

## Premise correction 1 — the base DOES carry #484 and #444

The plan §`Dependency status — corrected` and `spec.md` §`Dependencies or blocked work` both assert that
the branch point carries neither feature. **That is false for this run.** Verified on the base:

- `TryResolveCidResource` (a #484 member) is present in `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs`.
- `MoveFailureNotifier` (a #444 member) is present in `QuickFiler/Controllers/QfcItemController.MailActions.cs`.

#484 merged as PR #619 (`363bfcdd`) and #444 as PR #654 (`69e83171`), both into this integration branch.

Consequences:

1. `[P4-T5]` lands on the **post-#484** text of `QfcItemController.ViewerSetup.cs`, not the pre-#484 text.
   The file is now **499 lines**, not 430.
2. The two edits are **sequential, not concurrent**. The textual-conflict risk the plan rated "materially
   higher" is **reduced**, not raised: #484's edit is already in the tree and this edit applies on top of it.
3. Nothing else changes. The plan already states that no task depends on a #484 or #444 symbol existing.

## Premise correction 2 — RC7's two call sites were centralized upstream by #614

This is the one substantive change to how a remedy is delivered.

At the plan's authoring commit, `EfcFormController` classified banner rows two ways:
`Substring(0, 3) == "==="` in `IsValidSelection` and `StartsWith("====")` in `ActionOkAsync`. **Both are
gone on this base.** Feature #614 (commits `cee78979`, `cbad2da2`, `98b7a5e1`) replaced them with
delegations to a new shared owner, `QuickFiler/Controllers/EfcSelectionGuard.cs`:

- `EfcFormController.IsValidSelection` (now **`:1038`**) is `EfcSelectionGuard.IsValidCreationSelection(SelectedFolder)`.
- `ActionOkAsync`'s guard (now **`:706`**) is `!EfcSelectionGuard.IsValidFilingSelection(selectedFolder)`.
- `EfcSelectionGuard` carries `private const string BannerPrefix = "==="` (**three** `=`) and tests it with
  `StartsWith(BannerPrefix, StringComparison.Ordinal)`. The `Substring` throw hazard is already removed.

Two parts of RC7 are therefore **already delivered** and must not be re-broken:

- The two EFC sites no longer diverge in arity; a three-`=` row and a four-`=` row already classify
  identically at both sites (both rejected).
- Neither site uses `Substring`.

One part of RC7 is **not** delivered, and is the residue this feature owns: `EfcSelectionGuard`'s
three-character prefix **disagrees with the row producers**, which both use four characters
(`UtilitiesCS/OutlookObjects/Folder/BreadcrumbRowBuilder.cs:19` and `FolderSuggestionTree.cs:16`). #614
resolved "the two guards must agree" by **widening** the classification rather than by adopting the
producer's contract, and #614's own code comment at `EfcFormController.cs:289-290` describes the behavior
as a `"===="` rejection, which the code does not implement.

### Required delivery shape for RC7 — additive, non-regressing

Deliver the four `#465 D` criteria **without editing `QuickFiler/Controllers/EfcSelectionGuard.cs`**:

```csharp
// EfcFormController.cs — RC7, the single classification owner
internal static bool IsBannerRow(string row) =>
    row is not null
    && row.StartsWith(BreadcrumbRowBuilder.BannerPrefix, StringComparison.Ordinal);

internal static bool IsSelectableFolder(string selectedFolder) =>
    !IsBannerRow(selectedFolder)
    && EfcSelectionGuard.IsValidCreationSelection(selectedFolder);
```

- `IsValidSelection` becomes `IsSelectableFolder(SelectedFolder)`.
- `ActionOkAsync`'s guard becomes a composition that classifies through `IsBannerRow` **and** retains
  `EfcSelectionGuard.IsValidFilingSelection`, so #614's rooted-path rejection
  (`ArchiveStemContract.IsFullOutlookPath`) survives intact.

**Why not change `EfcSelectionGuard.BannerPrefix` to four characters.** It would *widen* what #614
deliberately rejects at a filing boundary (a three-`=` row would become filable) in a file this feature
does not own, on a merged sibling's behavior, to gain nothing a user can observe — the producers emit no
three-`=` row. `EfcSelectionGuardTests.cs` asserts only on `"==== SUGGESTIONS ===="`, so the change would
pass tests while silently relaxing a merged guard. That is the failure mode recorded against #614's own
remediation and it must not be repeated in the opposite direction.

**Residual, to be reported and promoted, not absorbed:** `EfcSelectionGuard.BannerPrefix` remains a third
arity variant, and the `EfcFormController.cs:289-290` comment remains inaccurate. Both are outside this
feature's owned set. Report them; do not fix them here.

`spec.md` criterion 4 for #465 D asserts only that **`IsBannerRow`'s** prefix agrees with
`BreadcrumbRowBuilder.BannerPrefix`. Under the shape above that is honestly checkable. Do **not** claim
that every banner-classification site in the repository now shares one arity — that is false and the
criterion does not require it.

## Corrected locator table

`EfcItemController.cs` (1170 lines) and `EfcViewer.cs` (162 lines) are **byte-identical to the plan's
authoring commit**; every citation the plan makes against those two files is correct as written and needs
no adjustment.

`EfcFormController.cs` is **1073 lines**, not 1084. Verified unchanged-and-correct anchors:

| Anchor | Plan cite | On this base |
|---|---|---|
| `private EfcFormController() { }` | `:77` | `:77` — correct |
| `_ = PopulateFolderCombobox()` | `:95`, `:115` | `:95`, `:115` — correct |
| `_parentCleanup` field | `:128` | `:128` — correct |
| `_folderRows` declaration initializer | `:134` | `:134` — correct |
| `Cleanup()` | `:187-194`, deref `:189`, invoke `:193` | correct |
| `ActiveTheme` / its `strict: true` | `:253` / `:255` | correct |
| `LoadTheme` | `:264` | `:264` — correct |
| `DarkMode` getter / eager arg | `:274-281` / `:280` | `:272-282` / `:277` |
| Edit Filters subscription | `:398` | `:398` — correct |
| five `throw;` in `async void` | `:425`, `:441`, `:457`, `:517`, `:530` | all five correct |
| `EditFiltersMenuItem_Click` target | `:559-564` | `:559` — correct |
| `ActionOkAsync` | `:700` | `:700` — correct |
| `ActionDeleteAsync` | `:740-748`, read `:745` | correct |
| `RefreshSuggestionsAsync` | `:795-804`, lambda `:799` | correct |
| `new WebView2BreadcrumbHost(` (#476 invariant) | `:834-837` | `:834` — correct, **must not move** |
| `BindFolderRows` signature | `:871` | `:871` — correct |
| `BindFolderRows` write-back (RC9 target) | `:879` | `:879` — correct |
| `BindFolderRows` read handed to bind | `:880` | `:880` — correct |
| `PopulateFolderCombobox` | `:1022-1036` | `:1022` — correct |

Corrected anchors:

| Anchor | Plan cite | **On this base** |
|---|---|---|
| `IsValidSelection` | `:1035-1047` | **`:1038-1039`**, expression-bodied, delegating to `EfcSelectionGuard` |
| `ActionOkAsync` banner guard | `:706` (`StartsWith("====")`) | **`:706`**, now `!EfcSelectionGuard.IsValidFilingSelection(...)` |
| incognito literal, QFC path | `ViewerSetup.cs:55` | **`ViewerSetup.cs:61`** |
| `QfcItemController.ViewerSetup.cs` size | 430 lines | **499 lines** |
| `EfcFormController.cs` size | 1084 lines | **1073 lines** |

`QuickFiler.Test/QuickFiler.Test.csproj` — the `Efc*` cluster moved. Plan `[P1]` says "insert after line
112 (`Controllers\EfcHomeControllerTests.cs`), before line 113 (`Controllers\EmailSorterTests.cs`)".
**On this base `Controllers\EfcHomeControllerTests.cs` is at `:117`.** The contiguous `Efc*` block is
`:109-117`, and a separate `Controllers\EfcSelectionGuardTests.cs` entry sits far above at `:63`.
Insert the three new entries **immediately after `:117`**, keeping them contiguous and alphabetical within
the `Efc*` cluster. Do not disturb `:63`, and do not reorder any existing entry.

## Size-gate correction

`spec.md` cross-cutting criterion states `EfcFormController.cs` must end at "at most 1204 lines — its
1084-line merge-base count plus at most 120 net lines". The real merge-base count is **1073**. Hold the
**stricter** derived gate of **1193** so both readings of the criterion pass, and record the true 1073
merge-base figure in the file-size evidence artifact alongside the delivered net delta per remedy.

`EfcItemController.cs` must end **strictly below 1170**. That figure is correct as written.

## Ownership guard rails for this run — three siblings are LIVE

Beyond the plan's C1 list, three siblings are executing concurrently against this same integration branch.
Their `<Compile Include>` regions in `QuickFiler.csproj` and `QuickFiler.Test.csproj` are forbidden:

| Region | Owner | Status |
|---|---|---|
| `Viewers\Breadcrumb*` | #501 | LIVE |
| `Viewers\WebView2*` | #476 | LIVE |
| `Controllers\QfcItemController*`, `Viewers\ToolStrip*` | #489 | LIVE |

**This feature owns the `Efc*` prefix only.** All three new test entries
(`Controllers\EfcItemControllerTests.cs`, `Controllers\EfcItemController.CleanupTests.cs`,
`Controllers\EfcViewerTests.cs`) are inside it. `QuickFiler/QuickFiler.csproj` stays untouched, exactly as
the plan requires.

The one-line `QfcItemController.ViewerSetup.cs:61` source edit **adds no project-file entry** and is
therefore not a region breach. It is carved out by `issue.md:212-214` and `spec.md` §RC5, and this feature
is the only owner of that defect. If it conflicts at fan-in, **keep both edits** (`spec.md` §RC5); never
drop it.

If execution pushes toward any `<Compile Include>` entry outside `Efc*`, **stop and report the overlap**.
Do not edit it.

## Standing obligations restated

- `/t:Rebuild` always, never `/t:Build`, for the analyzer and nullable gates; prove non-vacuity by
  confirming **zero** `Skipping target "CoreCompile"` lines in the log.
- Run `date -u` immediately before every evidence or checkpoint write. Never carry a timestamp forward.
- Evidence only under `docs/features/active/efc-controller-surface-defects-464/evidence/<kind>/`.
- No helper `.ps1`/`.py` script may be retained under `evidence/`; a retained script forces a coverage FAIL.
- Check a `spec.md` box only against real, recorded evidence. Leave unmet criteria unchecked and report them.
