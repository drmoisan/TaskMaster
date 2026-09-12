# quickfiler-breadcrumb-bridge-coverage

- Date captured: 2026-08-08
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/2026-08-08-quickfiler-breadcrumb-bridge-coverage-495/ (Issue #495)
- Parent epic issue: #136 (https://github.com/drmoisan/TaskMaster/issues/136)
- Parent epic manifest: docs/features/epics/quickfiler-per-file-coverage/epic.md (child F12, wave 1)
- Integration branch: `epic/quickfiler-per-file-coverage-integration`
- Depends on: F1 (#432) — per-file coverage/branch harness and ratified exemption ledger

## Problem / Why

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

## Proposed Behavior

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

## Next Step

- [x] Promote to GitHub issue (feature request template), citing parent epic #136
- [x] Create the active feature folder from the template
