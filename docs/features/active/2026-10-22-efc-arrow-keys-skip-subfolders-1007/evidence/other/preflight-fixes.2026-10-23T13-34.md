# Preflight Fixes — Issue #1007

Timestamp: 2026-10-23T13-34
Plan: docs/features/active/2026-10-22-efc-arrow-keys-skip-subfolders-1007/plan.2026-10-22T13-03.md
Directive: PREFLIGHT VALIDATION ONLY (single pass; task text edited only; no task added, removed, or reordered)

## Fixes

1. File Sets, EXCLUDED bullet | count and citation correction
   - Before: "the four not-to-be-extended 1002/439/Queue/provider test files the spec lists under Boundaries".
   - After: "the five not-to-be-extended test files the spec lists under Boundaries", followed by the five repository-relative paths.
   - Reason: spec.md (Boundaries and invariants, 500-line limit bullet) lists five such files: BreadcrumbBridgeRouterIssue1002Tests.cs, BreadcrumbRowDescendantsIssue1002Tests.cs, BreadcrumbBridgeRouterQueueTests.cs, BreadcrumbBridgeRouterIssue439Tests.cs, OutlookFolderHierarchyProviderTests.cs. Each exists in the tree. The assertion (these files stay out of the diff) is unchanged.

2. [P10-T3] | validator-blind compile-ordering clarification
   - Added: page keys with a size are delivered as a `selectorKey` JSON message carrying `pageSize`, because the `pageSize` parameter of `HandleSelectorKey` lands in P11-T6.
   - Reason: `BreadcrumbBridgeCoordinator.HandleSelectorKey(BreadcrumbSelectorKey key)` currently takes no page size, and Phase 1 does not declare one. A direct two-argument call in `SelectorKeyPageDown_Open_MovesPendingByPageSize` would not compile at P10-T6, which contradicts R10. The JSON route compiles and fails at run time as R10 intends. The asserted behavior is unchanged.

3. [P10-T6] | class-name correction
   - Before: "the five edited classes".
   - After: "the four edited classes `BreadcrumbStateModelTests` (whose partials include `BreadcrumbStateModelSequenceTests.cs`), `BreadcrumbSelectionSessionTests`, `BreadcrumbSelectorMessagesTests`, and `FolderBreadcrumbBridgeRouterTests`".
   - Reason: `BreadcrumbStateModelSequenceTests.cs` declares `partial class BreadcrumbStateModelTests`, so the five edited files form four test classes; a class filter named `BreadcrumbStateModelSequenceTests` would match nothing. The run set is unchanged.

4. [P18-T1] | validator-blind merge-base residue (scope gate)
   - Added `docs/features/potential/promoted/2026-10-22-efc-arrow-keys-skip-subfolders.md` (added by the promotion commit before Phase 0) to the permitted path set.
   - Reason: `git diff --name-status` from the merge base of `origin/main` and `HEAD` already lists this promotion file (status A) before any plan task runs, so without it the gate could not pass. The fence still covers every file the plan's tasks change.

## Checks of the fixes against their own rules

- No fix cites a line number or a commit SHA (G11, G12).
- No fix adds, removes, or reorders a task, and none changes what an acceptance condition asserts.
- The fix prose uses neutral wording that follows `.claude/rules/tonality.md`.
