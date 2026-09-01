---
name: efc-464-family-shipped-issues-left-open
description: Issues 460, 461, 463, 465, 466, 467 are delivered on main under the #464 feature but still OPEN — the second confirmed family of this bookkeeping pattern; #460, #461 and #465 verified guard-site by guard-site
metadata:
  type: project
---

EIGHT EfcFormController/EfcItemController defect issues — #459, #460, #461, #463, #464, #465,
#466, #467 — were fixed and merged under the SINGLE feature `efc-controller-surface-defects-464`
(PR #661, merge `ee7d0ec4`). Only #459 and #464 are CLOSED. The other six are still OPEN
purely as bookkeeping debt: every fix commit carries a `fix(efc-464):` subject, which names the
FEATURE slug rather than the issue and carries no GitHub closing keyword, so nothing auto-closed
them.

Delivering commits on `main`, one per issue: `ace22477` (#460 A/B/C, as RC1+RC2), `b8a7200b`
(#461), `abb825d9` (#463), `ace22477` + `93629f69` (#465), `345f6848` + `5d567e7e` (#466),
`28d244e5` (#467).

**The merge commit BODY names all eight issues explicitly** — `fix(efc): close eight EFC
controller-surface defects (#459, #460, #461, #463, #464, #465, #466, #467)` — even though every
constituent commit subject says only `fix(efc-464):`. So `git log origin/main --grep="<N>"` on a
bare number returns the MERGE commit for any member of this family. A lone merge-commit hit is
therefore a strong family signal and is worth one `git log -1 --format=%B <merge>` before
anything else; that single command named #460 as delivered in one step.

**#465 is verified to guard-site depth** (2026-08-29, against `origin/main`). All four of its
defects are gone from `QuickFiler/Controllers/EfcFormController.cs`:

- A — `Cleanup()` at `:192` is now idempotent and partial-construction safe (`globals?.Ol`,
  `_parentCleanup` captured and cleared before invoke). Delivered as RC1 by `ace22477`.
- B — `RefreshSuggestionsAsync` at `:877` reads `_formViewer.SearchText.Text` into a local as its
  first statement; no member access on `_formViewer` remains inside either `Task.Run` lambda.
- C — `BindFolderRows` no longer writes back to `_folderRows`; retention moved to
  `BindSourceFolderRows` (`:967`), which the delete path does not call. `_folderRows` is assigned
  in exactly two method bodies plus its declaration initializer, and `WithTrashRow` (`:787`) is
  idempotent.
- D — single classification owner `IsBannerRow` (`:1143`) uses `StartsWith` over
  `BreadcrumbRowBuilder.BannerPrefix` (`"===="`), never `Substring`; both `IsValidSelection` and
  `ActionOkAsync` route through it. Zero `Substring(0, 3)` occurrences remain in the file.

**#460 is verified to guard-site depth** (2026-08-31, against `origin/main`, on `/parallel-add
460`). All three of its defects are gone from `QuickFiler/Controllers/EfcItemController.cs`:

- A — `Cleanup()` (`:227`) captures `_buttons` into a local and guards `is not null` before
  iterating (`:231-232`), then assigns `_buttons = null` (`:240`), which it never did. The
  duplicate `_itemViewer = null` is gone: exactly one assignment remains, at `:248`.
- B — `_timer?.Dispose()` (`:263`) precedes `_timer = null` (`:264`), with an inline comment
  giving the thread-pool retention reason.
- C — `Subject` (`:611-615`) reads `_itemInfo?.Subject`, uniform with `Sender` (`:598`) and `To`
  (`:626`), carrying a `#464 A` comment.

Residual scope is closed too: all SEVEN of #460's acceptance criteria are `[x]` in
`docs/features/active/efc-controller-surface-defects-464/spec.md` on `main`, and
`QuickFiler.Test/Controllers/EfcItemController.CleanupTests.cs` exists there with 8 `[TestMethod]`
bodies. The whole pre-check ran in six tool calls with no preparation delegated.

**#461 is verified to guard-site depth** (2026-08-31, against `origin/main`, on `/parallel-add
461`). Its remedy was DELETION, which changes the shape of the check: the guard site is an
ABSENCE, so the confirming evidence is a zero count plus a live-route positive.

- `ConversationResolverPropertyChanged` has 0 occurrences in
  `QuickFiler/Controllers/EfcItemController.cs`; the handler and its `:666-669` subscription are
  gone. The only surviving `PropertyChanged +=` (`:671`) is `_globals.Ol.PropertyChanged +=
  DarkMode_Changed`, an unrelated globals subscription.
- The guard token `nameof(_dataModel.ConversationResolver.ConversationInfo.Expanded)` has 0
  occurrences, so it was not retargeted. **A substring grep for `ConversationInfo.Expanded` still
  returns one hit (`:1032`), and it is a false positive** — an ordinary `.ForEach(item =>
  item.ToggleDark(...))` iteration over the `Expanded` collection in the dark-mode path. Count the
  full `nameof(...)` token, not the property path, or a delivered deletion reads as outstanding.
- The live route the deletion relies on is present: `PopulateConversation` assigns
  `_dataModel.ConversationResolver.UpdateUI = SetTopicThread` (`:301`), `SetTopicThread` is
  declared at `:341`, and two named tests in
  `QuickFiler.Test/Controllers/EfcItemControllerTests.cs` pin both the removal and that route.

Residual scope is closed: all FOUR of #461's acceptance criteria are `[x]` in
`docs/features/active/efc-controller-surface-defects-464/spec.md` on `main`. The whole pre-check
ran in six tool calls with no preparation delegated.

**#463, #466, #467 are NOT verified to that depth** — only their delivering commits were
identified. Do the guard-site read before rejecting one of those.

**Why:** This is the SECOND confirmed family of the same pattern, after
[[qfc-collection-468-family-shipped-issues-left-open]]. Two independent occurrences make it a
repository-wide property rather than a one-off: a multi-issue feature folder closes its OWN issue
and leaves every sibling open. Admitting one of these into a parallel run spends a full
preparation cycle and yields an empty branch.

**How to apply:** Treat any of #460, #461, #463, #465, #466, #467 as presumptively delivered and run
the delivery pre-check in [[verify-delivery-before-preparing-an-admission]] before preparing.
The generalized signal: when `git log origin/main --grep="<N>"` returns a commit whose subject
scope is a DIFFERENT issue's feature slug, that is the shape of this pattern, not a coincidence —
widen to the bare number and read the guard sites. Re-verify before relying on this; the memory
goes stale the moment someone closes them (`gh issue view <N> --json state`).
