---
name: efc-464-family-shipped-issues-left-open
description: Issues 461, 463, 465, 466, 467 are delivered on main under the #464 feature but still OPEN — the second confirmed family of this bookkeeping pattern; #465 verified guard-site by guard-site
metadata:
  type: project
---

Seven EfcFormController/EfcItemController defect issues — #459, #461, #463, #464, #465, #466,
#467 — were fixed and merged under the SINGLE feature `efc-controller-surface-defects-464`
(PR #661, merge `ee7d0ec4`). Only #459 and #464 are CLOSED. The other five are still OPEN
purely as bookkeeping debt: every fix commit carries a `fix(efc-464):` subject, which names the
FEATURE slug rather than the issue and carries no GitHub closing keyword, so nothing auto-closed
them.

Delivering commits on `main`, one per issue: `b8a7200b` (#461), `abb825d9` (#463),
`ace22477` + `93629f69` (#465), `345f6848` + `5d567e7e` (#466), `28d244e5` (#467).

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

**#461, #463, #466, #467 are NOT verified to that depth** — only their delivering commits were
identified. Do the guard-site read before rejecting one of those.

**Why:** This is the SECOND confirmed family of the same pattern, after
[[qfc-collection-468-family-shipped-issues-left-open]]. Two independent occurrences make it a
repository-wide property rather than a one-off: a multi-issue feature folder closes its OWN issue
and leaves every sibling open. Admitting one of these into a parallel run spends a full
preparation cycle and yields an empty branch.

**How to apply:** Treat any of #461, #463, #465, #466, #467 as presumptively delivered and run
the delivery pre-check in [[verify-delivery-before-preparing-an-admission]] before preparing.
The generalized signal: when `git log origin/main --grep="<N>"` returns a commit whose subject
scope is a DIFFERENT issue's feature slug, that is the shape of this pattern, not a coincidence —
widen to the bare number and read the guard sites. Re-verify before relying on this; the memory
goes stale the moment someone closes them (`gh issue view <N> --json state`).
