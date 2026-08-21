# quickfiler-breadcrumb-bridge-coverage (Promoted)

- Date captured: 2026-08-08
- Author: Dan Moisan
- Status: Promoted -> Issue #495 -> `docs/features/active/2026-08-08-quickfiler-breadcrumb-bridge-coverage-495/`
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/495
- Work Mode: full-feature
- Parent epic: #136 (QuickFiler per-file 80% coverage)
- Epic child: F12 of `quickfiler-per-file-coverage`
- Integration branch: `epic/quickfiler-per-file-coverage-integration`
- Upstream dependency: F1 `quickfiler-coverage-denominator-and-exemption-ledger` (issue #432)

> Audit-trail note: the MCP promotion tool reported this destination path in its receipt but the
> file was not present on disk afterwards, matching the behavior already recorded for sibling F13.
> It is reconstructed here so the promotion lifecycle remains auditable. The authoritative record
> is issue #495 and the active feature folder. Promotion was NOT re-run during the finishing pass —
> re-invoking `potential_to_issue` would have duplicated issue #495.

## Problem / Why

Child F12 of epic #136 owns the QuickFiler breadcrumb bridge surface: five compiled files totalling
roughly 2,183 lines — `Controllers/BreadcrumbBridgeRouter.cs` (450),
`Viewers/BreadcrumbBridgeCoordinator.cs` (487), `Viewers/BreadcrumbCoordinatorUpgradeLifetime.cs`
(309), `Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs` (481), and
`Viewers/BreadcrumbMessengerHub.cs` (456).

All five clear the 80% per-file line floor, which made this child look close to a no-op on the line
column alone. It is not. `BreadcrumbItemViewerLifecycleCoordinator.cs` sits at **66.44% branch
across 146 branch points** against the epic's 75% branch floor — the largest single branch gap in
the epic. Branch coverage, not line coverage, is the binding gate here.

## Proposed Behavior

Raise every file in the F12 assignment above both the 80% line and 75% branch floors, verified by
recomputation from class-level Cobertura `<line>` elements keyed on `filename=` (never from the
emitted `line-rate` / `branch-rate` attributes, per issues #441 and #478). No production `.cs`
change and no observable behavior change to QuickFiler flows: the child is test-only.

## Outcome of Preparation (2026-08-08)

Promotion, five per-file research artifacts (including a 1,204-line lifecycle-coordinator study and
946/956/1,051-line studies of the router, messenger hub, and upgrade lifetime), `issue.md`,
`spec.md` (AC-1..AC-16), `user-story.md` (US-1..US-8), and an 8-phase / 108-task atomic plan were
produced. The plan passes the MCP plan validator and cleared `atomic-executor` preflight on the
fourth round.

Preparation spanned three runs: two were terminated by infrastructure failures (a spend limit, then
an expired login) and their work was salvaged and committed. The third run carried only the
preflight-clearance step to completion.

Preflight surfaced and closed four blocking defects across four iterations: a Phase 2 execution
count that no vstest run could satisfy, an omitted path-scoped `.claude/rules/csharp.md` policy
read, a touched-test-file count that contradicted the task's own scope clause, and an AC-14
confirmation clause that was unsatisfiable against the plan's own Phase 1. A csproj insertion point
was also relocated after direct inspection showed it would have landed outside the breadcrumb block
that a later audit task checks.

Five latent defects were promoted to their own issues rather than left as feature-folder prose, per
the epic's Latent Defect Promotion rule: **#498, #499, #500, #501, #502**.

## Known Conflict Risk

Open issue **#440** (`breadcrumb-left-right-arrow-parent-child-navigation`) names
`BreadcrumbBridgeRouter` and `BreadcrumbBridgeCoordinator` and will rewrite the Left/Right arrow-key
semantics this child's tests pin. Per the epic ruling, **F12's tests pin current behavior, not
corrected behavior**, and cite #440 in an in-code doc comment so a future break is legible. Whoever
schedules #440 should expect to update those tests as part of the fix rather than treat them as a
regression.

## Next Step

- [x] Promote to GitHub issue (#495)
- [x] Create the active feature folder
- [x] Research, feature documents, and atomic plan
- [x] `atomic-executor` preflight — `PREFLIGHT: ALL CLEAR`
- [ ] Atomic execution — deferred to `epic-orchestrator` after F1 (#432) merges
