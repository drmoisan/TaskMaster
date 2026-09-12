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
# quickfiler-breadcrumb-bridge-coverage

- Status: Promoted -> docs/features/active/2026-08-08-quickfiler-breadcrumb-bridge-coverage-495/ (Issue #495)
- Parent epic issue: #136 (https://github.com/drmoisan/TaskMaster/issues/136)
- Parent epic manifest: docs/features/epics/quickfiler-per-file-coverage/epic.md (child F12, wave 1)
- Depends on: F1 (#432) — per-file coverage/branch harness and ratified exemption ledger


Epic #136 requires every testable production file compiled by `QuickFiler/QuickFiler.csproj` to reach
at least 80% line coverage measured **per production file**, and (per
`.claude/rules/general-unit-test.md`) at least 75% branch coverage, or to be placed on the ratified
exemption ledger delivered by child F1. Child F12 owns the breadcrumb bridge, messenger, and
lifecycle coordination cluster — five production files totalling roughly 2,183 lines:

| File | Lines | Coverable lines | Line % | Branch points | Branch % |
| --- | --- | --- | --- | --- | --- |
| `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs` | 481 | 318 | 90.57% | 146 | **66.44%** |
| `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs` | 487 | 280 | 100.00% | 87 | 87.36% |
| `QuickFiler/Viewers/BreadcrumbMessengerHub.cs` | 456 | 294 | 100.00% | 118 | 96.61% |
| `QuickFiler/Viewers/BreadcrumbCoordinatorUpgradeLifetime.cs` | 309 | 204 | 99.02% | 54 | 92.59% |
| `QuickFiler/Controllers/BreadcrumbBridgeRouter.cs` | 450 | 282 | 97.87% | 90 | 92.22% |

These coverage figures were recomputed directly from
`docs/features/active/2026-08-06-quickfiler-high-confidence-queue-init-stall-424/evidence/qa-gates/coverage-final.cobertura.xml`
using the epic's corrected reading rules (class-level `<lines>` block only, union by `filename`, max
hits per line, `branch="True"` with `condition-coverage`). They are indicative, not acceptance
evidence; F1's harness run on this branch is the authority.

Every file clears the 80% per-file line floor, which caused an earlier assessment to treat this child
as a near-no-op. That assessment was wrong. `BreadcrumbItemViewerLifecycleCoordinator.cs` sits at
**66.44% branch against the 75% branch floor across 146 branch points** — the largest single branch
gap in the epic. Line coverage and branch coverage are independent gates (epic ruling,
"Coverage-Target Reconciliation"), and this child fails the branch gate today.

None of the five files carries an `[ExcludeFromCodeCoverage]` attribute and no type in any of them is
`partial`, so there is no exemption disposition work and no inherited partial-type suppression. The
work is branch-gap closure on the lifecycle coordinator plus retain-or-improve and residual
error-path pinning on the other four.


Raise per-file coverage for the five assigned files to at least 80% line and at least 75% branch,
verified with F1's harness, with no observable behavior change to QuickFiler flows.

Branch-gap closure in this cluster is specifically about the untaken sides of guard clauses,
cancellation paths, double-invoke guards, disposal guards, and out-of-order state transitions — not
additional happy-path tests. Bridge, messenger, and lifecycle coordination carry concurrency and
ordering invariants, which is exactly why branch coverage lags line coverage here.

## Acceptance Criteria (early draft)

- [ ] Every one of the five assigned files reaches >= 80% line AND >= 75% branch, verified with F1's
      harness, recorded as numeric evidence under `<FEATURE>/evidence/qa-gates/`. For a file already
      above a floor, the bar on that axis is retain-or-improve.
- [ ] `QuickFiler/Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs` closes its branch gap to >= 75%.
- [ ] Repository-wide coverage retained or improved, measured as a self-consistent before/after pair
      on this branch using an identical command and identical post-processing. No imported figure is
      a valid comparison baseline.
- [ ] No production file exceeds 500 lines; any newly created file reaches >= 90% line coverage and
      gets a ledger row per the epic's "Mid-Wave File Creation" rules.
- [ ] Tests use MSTest, Moq, and FluentAssertions; deterministic and isolated; no temporary files, no
      external services, no live forms, no popups, no `Thread.Sleep`/`Task.Delay`/wall-clock waits.
- [ ] Full C# toolchain green in final form.
- [ ] No behavior change to observable QuickFiler flows.

## Constraints & Risks

- **Upstream.** F1 (#432) is merged as documentation only; its harness
  (`scripts/vscode/Get-PerFileCoverage.ps1`) and ledger
  (`docs/features/epics/quickfiler-per-file-coverage/coverage-ledger.json`) do not exist on disk. A
  Phase 0 halt gate on F1's deliverables is required, with the documented fallback established by
  sibling F13.
- **Determinism.** There is no clock dependency anywhere in these five files — zero
  `DateTime`/`Stopwatch`/`Timer`/`Task.Delay`/`Thread.Sleep`/`TimeProvider` usages. Determinism is
  achieved by scheduler control (a manually-pumped fake `SynchronizationContext` with an explicit
  drain) and `Task`/`TaskCompletionSource` sequencing, not by an injected clock.
- **500-line pressure.** `BreadcrumbBridgeCoordinator.cs` is at 487/500 and
  `BreadcrumbItemViewerLifecycleCoordinator.cs` at 481/500. Test files
  `BreadcrumbCoordinatorLifecycleTests.cs` (489) and `BreadcrumbBridgeCoordinatorTests.cs` (488) are
  effectively full; new tests need `.Part2.cs` companions per existing repo precedent.
- **Sibling boundaries.** The drop-down surface and WebView2 host files belong to F13 (#455);
  `ItemViewer.Breadcrumb.cs` belongs to F14. Neither may be edited by this child. Note that
  `BreadcrumbPopupLifecycleOperations` and `BreadcrumbNavigationSubscription` are declared inside
  F12's `BreadcrumbItemViewerLifecycleCoordinator.cs` and are called from F13's
  `BreadcrumbPopupUiOperations.cs`; a pure file move is source-compatible, a type reorganisation is
  not.
- **#457 trap.** A method-level `[ExcludeFromCodeCoverage]` does not suppress nested lambdas. Any
  thin-forwarder adapter introduced here must be a class-level-exempt adapter **type** that is
  `sealed` and **not `partial`**.
- **#441 measurement defect.** The Cobertura `<class>` `branch-rate` attribute is inflated. It reads
  0.688073 for the lifecycle coordinator where the correct figure is 0.6644. Never read
  `line-rate`/`branch-rate` attributes and never use a `.//lines/line` descendant axis.
- **Shared csproj files.** `QuickFiler/QuickFiler.csproj` and `QuickFiler.Test/QuickFiler.Test.csproj`
  are non-SDK projects with explicit `<Compile Include>` entries and no globbing. Own entries only,
  minimal adjacent hunks, preserve CRLF. Additive fan-in conflicts are expected.
- **Tooling.** `csharpier` is pinned at 1.2.6 and requires a subcommand:
  `dotnet tool run csharpier format .`, not the bare `csharpier .` form in `CLAUDE.md`.

## Test Conditions to Consider

- [ ] Guard-clause untaken branches across all five files
- [ ] Cancellation and cancelled-token paths, including `OperationCanceledException` exception filters
- [ ] Double-invoke, re-attach, and re-entrancy guards
- [ ] Stale-generation and out-of-order state transitions
- [ ] Disposal and post-disposal invocation paths
- [ ] Error/exception paths in message routing and bridge upgrade


- [x] Promote to GitHub issue (feature request template), citing parent epic #136
- [x] Create the active feature folder from the template
