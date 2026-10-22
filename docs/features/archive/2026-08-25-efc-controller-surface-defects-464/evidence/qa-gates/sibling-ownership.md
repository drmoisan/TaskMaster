# [P9-T3] Sibling-owned path set is untouched

Timestamp: 2026-08-28T01-49
Task: [P9-T3]
EXIT_CODE: 0

## Bases used, and why two are recorded

The plan names `BASELINE_SHA` = `002335989830ba9f3ad802858ef0b794f6281750`. As `changed-file-set.md`
records in full, the branch took a mandated integration merge (`25924673`) after Phase 4 whose second
parent is `38f097898639b054428188c9c5e266e54972c259`, so `BASELINE_SHA..HEAD` now contains two merged
siblings' diffs as well as this feature's. Both bases are run and both results are recorded. The
acceptance condition is evaluated against `38f09789`, which `git merge-base HEAD 38f09789` confirms is
an ancestor of `HEAD` and is the base that isolates this feature's own changes.

## Command 1 — the explicit sibling-owned path list

```
git diff --name-only <BASE> -- QuickFiler/Controllers/EfcHomeController.cs \
  QuickFiler/Controllers/EfcHomeController.ExecuteMoves.cs QuickFiler/Controllers/BreadcrumbBridgeRouter.cs \
  QuickFiler/Controllers/KeyboardHandler.cs QuickFiler/Controllers/KbdActions.cs \
  QuickFiler/Controllers/QfcFormKeyHandler.cs QuickFiler/Controllers/QfcCollectionController.cs \
  QuickFiler/Viewers/QfcFormViewer.cs QuickFiler/Viewers/BreadcrumbMessengerHub.cs \
  QuickFiler/Viewers/WebView2BreadcrumbHost.cs QuickFiler/Viewers/WebView2CoreInitializer.cs \
  QuickFiler/Interfaces QuickFiler.Test/Controllers/QfcCollectionControllerTests.cs \
  QuickFiler.Test/Controllers/KbdActionsTests.cs QuickFiler.Test/Controllers/KbdActionsRemainingBranchesTests.cs \
  UtilitiesCS/OutlookObjects/Folder/BreadcrumbRowBuilder.cs UtilitiesCS/HelperClasses/Initializer.cs \
  QuickFiler/QuickFiler.csproj QuickFiler/Viewers/IItemViewer.cs
```

| Base | Output lines | Paths |
|---|---|---|
| `38f09789` (**evaluated**) | **0** | (none) |
| `002335989830...` (as written) | 4 | `QuickFiler/QuickFiler.csproj`, `QuickFiler/Viewers/BreadcrumbMessengerHub.cs`, `QuickFiler/Viewers/WebView2BreadcrumbHost.cs`, `QuickFiler/Viewers/WebView2CoreInitializer.cs` |

All four paths in the as-written result are changes made by merged siblings **#476** and **#501** and
carried into this branch by the integration merge. None was written by this feature; the evaluated
98-path result of `[P9-T2]` contains none of them.

## Command 2 — the glob-and-exclude form

```
git diff --name-only <BASE> -- "QuickFiler/Viewers/BreadcrumbBridgeCoordinator*.cs" \
  "QuickFiler/Viewers/ItemViewer*.cs" "QuickFiler/Controllers/EfcHomeController.*.cs" \
  "QuickFiler.Test/Controllers/QfcItemController.*Tests.cs" "QuickFiler/Controllers/QfcItemController.*.cs" \
  ":(exclude)QuickFiler/Controllers/QfcItemController.ViewerSetup.cs" \
  "QuickFiler/Controllers/EfcHomeController*.cs" QuickFiler/Controllers/QfcItemController.cs
```

| Base | Output lines | Paths |
|---|---|---|
| `38f09789` (**evaluated**) | **0** | (none) |
| `002335989830...` (as written) | 2 | `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.Suggestions.cs`, `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs` |

Both as-written paths are merged sibling **#501**'s files.

## Acceptance

Under the evaluated base both commands produce **no output lines**, so the intersection of this
feature's diff with the sibling-owned path set is **empty**. The `QfcItemController.ViewerSetup.cs`
carve-out did not mask anything: the exclude pathspec removes exactly that one file, and the appended
literal `QuickFiler/Controllers/QfcItemController.cs` — which the dotted glob cannot match — is still
gated and reports nothing.

Sibling **#489** is LIVE on `Controllers\QfcItemController*` and `Viewers\ToolStrip*`. `[P9-T2]` shows
this feature wrote to neither, and the `QfcItemController.*` clauses of command 2 independently return
zero.

## Additional gates this artifact carries for [P9-T9], [P9-T10] and [P9-T12]

These three criterion check-offs cite this artifact, so their measurements are recorded here.

### `KbdActions` (criterion at `spec.md:926`)

| Measure | Result |
|---|---|
| `git diff --name-only 38f09789 -- . ":(exclude).claude/agent-memory"` filtered case-insensitively for `KbdActions` | **0 lines** |
| Same filter over the as-written `BASELINE_SHA` diff | **0 lines** |

This half of the criterion holds under **both** bases, so the base-drift deviation does not bear on it.
The other half of the criterion — that the indexer-setter contract and the `overwriteDuplicates` truth
table are documented in this spec's §RC4 — is satisfied in `spec.md`: §RC4 opens at `spec.md:307`; the
indexer `set` row at `spec.md:315` reads "assign-if-present. **A missing key is a silent no-op, never an
insert.**"; and the `overwriteDuplicates` truth table is at `spec.md:320-323`, concluding that
`overwriteDuplicates: false` registers nothing and `true` overwrites but never inserts.

Per the upstream-constraints briefing, feature #444 froze `KbdActions.Remove` and promoted the
discarded-`bool` question as a separate follow-up. This feature absorbed neither; the zero-hit path
search above is the proof.

### `BreadcrumbRowBuilder` (criterion at `spec.md:979`)

| Measure | Result |
|---|---|
| `git diff --name-only 38f09789 -- . ":(exclude).claude/agent-memory"` filtered case-insensitively for `BreadcrumbRowBuilder` | **0 lines** |
| Same filter over the as-written `BASELINE_SHA` diff | **0 lines** |

Zero under both bases. The first half of the criterion — that the prefix of `IsBannerRow` **agrees with**
`BreadcrumbRowBuilder.BannerPrefix` — is satisfied by direct symbol reference rather than by a duplicated
literal. Delivered source:

- `UtilitiesCS/OutlookObjects/Folder/BreadcrumbRowBuilder.cs:19` declares
  `public const string BannerPrefix = "====";`
- `QuickFiler/Controllers/EfcFormController.cs:1143-1148` — `IsBannerRow` calls
  `row.StartsWith(UtilitiesCS.OutlookObjects.Folder.BreadcrumbRowBuilder.BannerPrefix, StringComparison.Ordinal)`.

Because `IsBannerRow` consumes the producer's constant itself, agreement is structural and cannot drift.

**Residual explicitly NOT claimed and NOT fixed (RC7).** `QuickFiler/Controllers/EfcSelectionGuard.cs`
declares its own `BannerPrefix` as three `=` characters — a third arity variant — and the comment near
`QuickFiler/Controllers/EfcFormController.cs:325` still describes a four-`=` rejection that
`EfcSelectionGuard` does not implement. `EfcSelectionGuard.cs` belongs to merged sibling #614 and is
outside the owned set of this feature; `[P9-T2]` confirms the diff of this feature does not contain it.
This artifact does **not** claim that every banner-classification site in the repository shares one
arity — that is false, and criterion `spec.md:979` does not require it. The residual is carried into the
Phase 11 handoff for promotion as a follow-up issue.

### The QFC twin (criterion at `spec.md:1000`)

| Measure | Result |
|---|---|
| `git diff --name-only 38f09789 -- QuickFiler/Viewers/QfcFormViewer.cs QuickFiler/Controllers/QfcFormKeyHandler.cs` | **0 lines** |
| Same command against the as-written `BASELINE_SHA` | **0 lines** |

Zero under both bases; the QFC twin is unchanged.

Output Summary: PASS. Under the evaluated base `38f09789` both `[P9-T3]` commands produce zero output
lines, so the diff of this feature intersects the sibling-owned path set in the empty set. Under the
as-written `BASELINE_SHA` the two commands report 4 and 2 paths respectively, every one of them a merged
sibling (#476, #501) change carried in by the mandated integration merge, not a write by this feature.
The three additional gates this artifact carries all return zero under **both** bases: no path matching
`KbdActions`, none matching `BreadcrumbRowBuilder`, and no entry for either QFC twin file. `IsBannerRow`
agrees with `BreadcrumbRowBuilder.BannerPrefix` by direct symbol reference. The RC7 three-`=`
`EfcSelectionGuard.BannerPrefix` residual is recorded as unfixed and outside the ownership of this
feature.
