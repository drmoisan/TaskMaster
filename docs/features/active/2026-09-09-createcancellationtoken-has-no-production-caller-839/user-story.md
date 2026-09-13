# 2026-09-09-createcancellationtoken-has-no-production-caller (User Story)

- **Issue:** #839
- **Owner:** drmoisan
- **Last Updated:** 2026-09-12
- **Work Mode:** full-bug

> This file exists because the item's validation hook requires it and the caller asked for it. Under full-bug work mode, spec.md in this feature folder is the sole authoritative acceptance-criteria source. This file is narrative context only, contains no checkboxes, and must not be used as an acceptance-criteria source. Repository paths in this file are written as plain prose without backticks by design; do not add backticks.

## Who the user is

The user of this change is the QuickFiler maintainer, not an end user. The research verified that the broken path (`QfcHomeController.Init()` reached through the public constructor) has no live entry point: its only production caller, `RibbonController.LoadQuickFiler()`, has zero callers, and both ribbon buttons route through the asynchronous `LaunchAsync` path, which creates the cancellation token source correctly. No end user can currently observe the defect, and this document does not claim otherwise.

## Story 1: the public initializer honours its contract

As the QuickFiler maintainer, I want `QfcHomeController.Init()` to establish the cancellation token source before any loader observes the controller's token, so that a controller built through the public constructor and initialized synchronously behaves the same as one built through `LaunchAsync` and the same as `EfcHomeController`, which already calls its identical factory on every construction path.

- Given a `QfcHomeController` built through its public constructor
- When `Init()` is called
- Then `TokenSource` is a live `CancellationTokenSource`, `Token` is that source's `Token` with `CanBeCanceled` true, and the datamodel loader, the queue loader and the form-controller loader all received that same source and token, so `LoadItems` proceeds past its null-source guard instead of returning silently.

## Story 2: the ordering rule is pinned by a test, not by convention

As the QuickFiler maintainer, I want a regression test that fails if the factory call is placed anywhere other than first in `Init()`, so that a later edit cannot quietly leave the datamodel and queue holding a token that can never be cancelled.

- Given the loaders replaced by lambdas that capture the token arguments they receive
- When `Init()` runs against a fix that inserts the call after the datamodel loader
- Then the test fails on the `CanBeCanceled` assertion for the datamodel token, and when the call is first, all assertions pass.

## Story 3: the decision is auditable and the scope stays narrow

As the QuickFiler maintainer, I want the spec to record why the token source is not made non-nullable (cleanup deliberately nulls it, pinned by an issue #810 test; the file has no nullable directive and adding one would surface roughly 37 diagnostics under a warnings-as-errors build) and why the early-return guards stay (they also protect pre-Init and post-Cleanup calls, and removing them relocates the failure into item construction), so that a reviewer can see the alternatives were considered rather than overlooked.

- Given the production file sits at exactly 500 lines, the repository ceiling
- When the one-statement fix lands
- Then one commented-out dead line is removed from the same file, no other production file changes, no project file changes, and a follow-up is filed to remove the dead synchronous entry path altogether.

## What is known and what is not

- Verified by the research: the zero-caller finding for `LoadQuickFiler()`, the three silent early returns, the EfcHomeController precedent, the 500-line file size, and the existing disposal path through `Cleanup()`.
- Unknown: when `LoadQuickFiler()` lost its last call site. Git history was not available in the research session.
- Inherited debt, declared not introduced: `Init()` constructs a real `QfcFormViewer`, so the regression test does too, exactly as the existing `Init_InitializesCorrectly` already does in the same class.
