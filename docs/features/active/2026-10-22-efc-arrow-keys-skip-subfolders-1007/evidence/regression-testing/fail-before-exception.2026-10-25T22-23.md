# Fail-Before Exception: UnhandledRightArrow_RaisesUnhandledArrowRight (Issue #1007, P12-T3)

Timestamp: 2026-10-25T22-23
Command: vstest.console.exe QuickFiler.Test.dll /Settings:scripts/vscode/TaskMaster.cli.runsettings /InIsolation "/TestCaseFilter:FullyQualifiedName~BreadcrumbBridgeCoordinatorIssue1007Tests|FullyQualifiedName~QuickFiler.Test.Viewers.BreadcrumbBridgeCoordinatorTests|FullyQualifiedName~BreadcrumbSelectorCoordinatorTests" (the P12-T3 run; results under SCRATCH)
EXIT_CODE: 1
ExpectedExitCode: 1
Output Summary: in the P12-T3 fail-before run, the updated test `QuickFiler.Test.Viewers.BreadcrumbBridgeCoordinatorTests.UnhandledRightArrow_RaisesUnhandledArrowRight` passed, while P12-T3 lists it among the tests expected to fail. A failing run of this test is not obtainable, for the reasons below.

Scope: one test. The other three P12-T2 updated tests and all five G6 non-guard tests failed in the same run (qfc-closed-fail-before.2026-10-25T22-23.md).

WhyFailingRunImpossible: P12-T2 changes this test only by opening the selector before the Right arrow, so that the AC10 closed-list "Right opens the list" change of Phase 13 does not alter what it observes. An open-list Right on a leaf raises `UnhandledArrow(Right)` exactly once in every relevant code state, so there is no code state between P12 and P13 in which the updated test can fail. The behaviour it asserts is the D-Q21 open-list pop-out, which Phase 11 (P11-T6) already delivered. Before #1007 the same observation also held, through the router's unhandled-arrow path.

## Alternative proof (static, absence of a distinguishing state)

The test asserts one observation: after `OpenSelector()` and an inbound `arrowKey` Right on a single leaf row without children, `UnhandledArrow` is raised exactly once with `Right`. Each code state produces that observation:

1. Pre-#1007 code (the plan base): the coordinator had no open-list arrow decision, so the arrow took `DispatchAsync`. `FolderBreadcrumbBridgeRouter.ArrowAsync` called `BreadcrumbStateModel.RightArrow()` on the committed row. A leaf without children cannot expand, so the router returned an `UnhandledArrowMessage(Right)` and `PublishRouterOutputs` raised `UnhandledArrow(Right)` once. The observation holds.
2. After Phase 11 (the state at P12-T3): `TryDispatchOpenArrowAsync` (in `BreadcrumbBridgeCoordinator.Keyboard.cs`) classifies the arrow through `ClassifyPendingArrow`. The pending entry has no children, so the result is `CancelAndPopOut`: `CancelSelector()`, then one `UnhandledArrow(Right)`. The observation holds.
3. After Phase 13: the closed-list half adds only closed-state branches, so the open-state path of item 2 is unchanged. The observation holds.

The test therefore characterizes behaviour that holds in every state. It functions as a guard that keeps the AC6 pop-out observable once closed Right opens the list. The failing evidence for the AC6 behaviour itself is `OpenRight_OnLeaf_CancelsThenRaisesUnhandledArrowRight` in qfc-open-fail-before.2026-10-25T22-08.md, which failed before P11 because it also asserts the close-before-pop-out ordering.

Detached-worktree run: not performed. Item 1 shows that the plan-base code produces the asserted observation, so a run of the updated test against that code would pass and could not serve as fail-before evidence.

SearchScope: docs/features/active/2026-10-22-efc-arrow-keys-skip-subfolders-1007/evidence/regression-testing/
SearchPatterns: fail-before-exception.*.md, qfc-closed-fail-before.*.md
SearchResult: this dossier and qfc-closed-fail-before.2026-10-25T22-23.md
