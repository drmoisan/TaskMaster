# qfcformcontroller-eventhandlers-coverage-uplift (Potential)

- Date captured: 2026-09-11
- Author: Dan Moisan
- Status: Draft

## Problem / Why

`QuickFiler/Controllers/QfcFormController.EventHandlers.cs` sits at 49.41% line coverage, below the modified-file floor. It was 45.38% before item #633 touched it; #633 covered all of its own changed lines and improved the file by 4 points but did not close the gap. The remainder is Outlook-interop and WinForms handler code with no seam. Deferred out of the 2026-09-11 consolidated bug run as refactor work (issue #727 sub-finding 1). Source: item #633 review, PR #717.

## Proposed Behavior

Raise the file to at least the 80% line floor settled under #563 by introducing the smallest seams that let the handler bodies execute under MSTest without a live Outlook process: an injectable delegate for each Interop call site and a synchronous pass-through for UI marshalling, following the `ItemControllerFactory` pattern in `QfcQueue.Enqueue.cs`. Where a handler is genuinely an Outlook Interop event handler with no possible seam, classify it under CLAUDE.md UT2 exemption class (c) with a comment naming the class. The maintainer decision of 2026-09-11 prefers seams over exemptions; the exemption is the fallback, not the first choice.

## Acceptance Criteria (early draft)

- [ ] `QfcFormController.EventHandlers.cs` line coverage is at or above 80% in the committed projection.
- [ ] Every member left under 80% carries an `[ExcludeFromCodeCoverage]` with a comment citing UT2 class (c) and stating why no seam is possible.
- [ ] No production behavior change; every production default reproduces the previous call exactly.
- [ ] The file remains under 500 lines (439 today).
- [ ] Full C# toolchain passes.

## Constraints & Risks

- Must be scheduled after the 2026-09-11 bug run merges; item #792 and item #743 touch neighbouring QuickFiler controller files.
- Seams that change constructor signatures affect `QfcFormControllerLoader`; keep them settable-property or optional-parameter seams.

## Test Conditions to Consider

- [ ] Unit coverage areas: each handler's guard branches and dispatch, with Moq on the new delegates.
- [ ] Integration scenarios: none; manual QuickFiler smoke after merge.
- [ ] CLI/API examples: not applicable.

## Next Step

- [ ] Promote to GitHub issue (feature request template)
- [ ] Create `docs/features/active/qfcformcontroller-eventhandlers-coverage-uplift/` folder from the template
