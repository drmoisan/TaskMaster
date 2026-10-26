# Preflight Fixes (Issue #1007, remediation cycle 1)

Timestamp: 2026-10-25T23-50
Plan: docs/features/active/2026-10-22-efc-arrow-keys-skip-subfolders-1007/remediation-plan.2026-10-25T23-50.md
Validator: `validate_orchestration_artifacts` (plan) returned ok with no warnings before and after the fix.

## Fixes

1. [P5-T15] | literal scoping (trivial) | The task counted "AC boxes" in `spec.md` and `issue.md` without naming the section. `issue.md` also carries unchecked checkboxes under `## Logs / Screenshots`, `## Impact / Severity`, `## Proposed Fix / Validation Ideas` and `## Next Step`, so a whole-file count could not show 0 unchecked. The task now counts the `## Acceptance Criteria` section of each file. The asserted value (14 checked, 0 unchecked) is unchanged.

## Citations re-derived during preflight (no defect found)

- `QuickFiler/Resources/FolderBreadcrumb.html`: one `<style>` block; the `.sub.selected` rule uses `var(--bc-sel-back)` and `var(--bc-sel-fore)`; there is no `.sub.active` rule.
- `QuickFiler/Resources/FolderBreadcrumb.js` `makeSubfolders`: assigns the `"sub active"` class; the script contains no `style>` text.
- `QuickFiler/Viewers/FolderBreadcrumbDocument.cs` `Html`: the HTML resource with the script spliced in.
- `QuickFiler.Test/Viewers/FolderBreadcrumbKeysIssue1007Tests.cs`: 8 tests, no `#nullable` directive.
- `BreadcrumbBridgeRouter.HandleLeafToggleAsync`, `FolderBreadcrumbBridgeRouter.ToggleAsync`, `BreadcrumbStateModel.Expand`, and `BreadcrumbBridgeCoordinator.PublishRouterOutputs` / `RaiseSyntheticArrowKey` (`UnhandledArrowMessage` arms): present as described in N1-LINES.
- `FolderBreadcrumbBridgeRouter.ClassifyPendingArrow` returns `Route` for Left on an expanded pending entry. The router's `UnhandledArrowMessage` case echoes the message (D3).
- Helpers `ThreeSegmentAsync`, `DoubleClickAsync`, `ExpandAsync`, `VerifyFetched`, `RouterAsync`, `ExpandAlphaAsync`, `Key`, `ModelWithSuggestion`, and the coordinator `Harness` (`commit`, `open`, `expandAlpha`, `Events`, `KeyDowns`): present.
- File sizes: `BreadcrumbBridgeCoordinatorIssue1007Tests.cs` 477 lines and `FolderBreadcrumbBridgeRouterIssue1007Tests.cs` 499 lines (D4).
- The six D5 old names occur once each, at their declarations; none of the new names exists yet.
- The AC4 tests named in P5-T13 exist in `UtilitiesCS.Test` and `QuickFiler.Test`.
- `artifacts/csharp/coverage.xml` and the `coverage/` outputs are git-ignored. `<FEATURE>/evidence/` contains no drive-rooted or `Users` path.
