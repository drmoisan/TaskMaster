---
name: qfc-item-controller-f10-init-viewersetup-453
description: "Epic #136 F10 (#453) research on QfcItemController.cs / .Initialization.cs / .ViewerSetup.cs — 3 of 7 Initialization exemptions are on dead members; every exemption comment cites an already-defeated barrier; ViewerSetup fails branch (56%) worse than line."
metadata:
  type: project
---

Findings from the F10 per-file research on the three partials `QfcItemController.cs`,
`QfcItemController.Initialization.cs`, `QfcItemController.ViewerSetup.cs` (2026-08-07). Complements
[[qfc-item-controller-f10-coverage-453]] (FocusAndTheme/MailActions/build mechanics) and
[[cobertura-exemption-and-branchrate-gotchas]] (measurement arithmetic); no overlap intended.

## Exemptions

- **3 of the 7 `[ExcludeFromCodeCoverage]` members in `Initialization.cs` are unreachable dead
  code**: the private 9-arg `Initialize` (`:138`), `CreateAsync` (`:403`), `CreateSequentialAsync`
  (`:436`). A repo-wide grep finds no call site for any of them (`GetItemSummary`,
  `ViewerSetup.cs:423`, is likewise dead). Dead code fails the epic's irreducible-remainder standard,
  so the policy-consistent disposition is deletion — which also drops the file from 466 to ~402
  against the 500 limit.
- **Every exemption comment in both files cites "not unit-reachable without a live ItemViewer"** —
  a barrier already defeated *in the same test project* by `ViewerSetupTests.cs:379-405` and the
  reusable `ViewerScope` at `QfcItemControllerBreadcrumbDropDownTests.cs:365-383`, both plain
  `[TestClass]`, no STA. `ResolveControlGroups` was de-exempted on exactly that basis and says so
  in-file (`ViewerSetup.cs:204`); the other comments were never updated. Defensible boundary after
  analysis: Initialization 7 -> 1, ViewerSetup 3 -> 1.
- **The real residual barrier is `await _itemViewer.UiSyncContext`, and it is defeasible test-side.**
  `ItemViewer.cs:25-26` runs `InitializeComponent()` and *then* captures
  `_context = SynchronizationContext.Current`, so `UiSyncContext` is a `WindowsFormsSynchronizationContext`
  that needs a message loop. Reflection-setting the headless viewer's private `_context` to a plain
  `SynchronizationContext` (the technique already used for `Theme._uiDispatcher` and
  `UiThread._dispatcher` in `QfcItemController.TestSupport.cs`) removes it with no production change.
  What stays irreducible is `ViewerSetup.cs:76`'s direct `.CoreWebView2` dereference — that needs the
  Edge runtime, an external process dependency.

## Coverage shape

- `QfcItemController.cs` is **100% line / 78.6% branch**; the only gap is `TopFolderScore` (`:254`,
  1 of 4 conditions). `_folderHandler` is already an interface — zero seams needed.
- `Initialization.cs` **passes both gates today only because 7 exempt bodies are outside the
  denominator**; removing them adds ~63 lines at zero hits and drops it from ~92% to ~62%. Remove an
  attribute and land its test in the *same* atomic task, never separately.
- `ViewerSetup.cs` **fails branch (56% vs a 75% floor) worse than line (72.5% vs 80%)**, and the
  brief flagged only the line failure. One 8-line pure static, `ResolveImageMimeType` (`:194-202`),
  holds 12 of the 24 uncovered conditions — covering it alone clears the branch floor. Line coverage
  cannot reach 80% by tests alone (plateaus at 78.8%); a host-neutral extraction of the
  `WebResourceRequested` lambda's logic is required.

## Latent trap

`Cleanup()` (`ViewerSetup.cs:392-421`) nulls `_mailItem` but not `_mailActions`, while
`SaveParameters` uses `??=` (`Initialization.cs:392`), so a re-initialized controller would keep a
`MailItemActionsAdapter` bound to the *previous* mail item. Not live today (production always
constructs a fresh controller), but `Cleanup()` advertises a reuse contract the seam defaults do not
honour. Same method also nulls `_emailIsReadTimer` (`:420`) without disposing it, while the sibling
path `Navigation.cs:211-214` disposes correctly.

**How to apply:** before accepting any `[ExcludeFromCodeCoverage]` in this family, grep for the
member's call sites (several are dead) and check the stated barrier against the headless-`ItemViewer`
precedent rather than believing the comment. See
[[feedback-exemption-audit-check-proven-techniques]] and [[qfc-item-controller-227-r2-denial]].
