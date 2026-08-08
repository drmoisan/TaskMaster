# quickfiler-breadcrumb-bridge-coverage (Issue #495)

- Date captured: 2026-08-08
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/quickfiler-breadcrumb-bridge-coverage/ (Issue #495)
- Parent epic: #136 (QuickFiler Per-File 80% Coverage), child F12
- Epic manifest: `docs/features/epics/quickfiler-per-file-coverage/epic.md`
- Integration branch: `epic/quickfiler-per-file-coverage-integration`
- Depends on: F1 (#432) — per-file coverage/branch harness and ratified exemption ledger

- Issue: #495
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/495
- Last Updated: 2026-08-08
- Work Mode: full-feature

## Problem / Why

Child F12 of epic #136 owns the QuickFiler breadcrumb bridge, messenger, and lifecycle
coordination cluster — five production files totalling roughly 2,183 lines:

| File | Lines | Line % | Branch % |
| --- | --- | --- | --- |
| `Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs` | 318 | 90.6% | **66.4%** (146 branch points) |
| `Viewers/BreadcrumbBridgeCoordinator.cs` | 280 | 100.0% | 87.4% |
| `Viewers/BreadcrumbMessengerHub.cs` | 294 | 100.0% | 96.6% |
| `Viewers/BreadcrumbCoordinatorUpgradeLifetime.cs` | 204 | 99.0% | 92.6% |
| `Controllers/BreadcrumbBridgeRouter.cs` | 282 | 97.9% | 92.2% |

(Coverage figures are the epic's corrected, indicative baseline; they are not acceptance evidence.
F1's harness run on this branch is the authority.)

Every file clears the 80% per-file line floor, which caused an earlier assessment to treat this
child as a near-no-op. That assessment was wrong. `BreadcrumbItemViewerLifecycleCoordinator.cs`
sits at approximately **66.4% branch against the 75% branch floor across 146 branch points** — the
largest single branch gap in the epic. Line coverage and branch coverage are independent gates
(epic ruling, "Coverage-Target Reconciliation"), and this child fails the branch gate today.

None of the five files carries an `[ExcludeFromCodeCoverage]` attribute, so there is no exemption
disposition work here; the work is branch-gap closure plus retain-or-improve on the other four.

## Proposed Behavior

Raise per-file coverage for the five assigned files to at least 80% line and at least 75% branch,
verified with F1's harness, with no observable behavior change to QuickFiler flows.

Branch-gap closure in this cluster is specifically about the untaken sides of guard clauses,
cancellation paths, double-invoke guards, disposal guards, and out-of-order state transitions —
not additional happy-path tests. Bridge, messenger, and lifecycle coordination carry concurrency
and ordering invariants, which is exactly why branch coverage lags line coverage here.

## Acceptance Criteria (early draft)

- [ ] Every one of the five assigned files reaches >= 80% line AND >= 75% branch, verified with
      F1's harness, recorded as numeric evidence under `<FEATURE>/evidence/qa-gates/`. For a file
      already above a floor, the bar on that axis is retain-or-improve.
- [ ] `Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs` closes its branch gap to >= 75%.
- [ ] Repository-wide coverage retained or improved, measured as a self-consistent before/after
      pair on this branch using an identical command and identical post-processing. No imported
      figure is a valid comparison baseline.
- [ ] No production file exceeds 500 lines; any newly created file reaches >= 90% line coverage
      and gets a ledger row per the epic's "Mid-Wave File Creation" rules.
- [ ] Tests use MSTest, Moq, and FluentAssertions; deterministic and isolated; no temporary files,
      no external services, no live forms, no popups, no `Thread.Sleep`/`Task.Delay`/wall-clock
      waits.
- [ ] Full C# toolchain green in final form.
- [ ] No behavior change to observable QuickFiler flows.

## Constraints & Risks

- **Determinism.** ~~Use an injected clock and fake timers.~~ **STRUCK 2026-08-08 — refuted by
  research on all five files.** Zero occurrences of `DateTime`, `Stopwatch`, `Timer`, `Task.Delay`,
  `Thread.Sleep`, or `TimeProvider` anywhere in this cluster; there is no time dependency to control,
  and an injected clock would be a seam with nothing behind it. Determinism here is **scheduler and
  completion-source control**, adopting sibling F13's ratified ruling
  (`.../455/spec.md:381-390` §8.1). See `spec.md` §5. `Thread.Sleep`, `Task.Delay`, and real
  wall-clock waits remain prohibited in tests.
- **Sibling boundaries.** The drop-down surface and WebView2 host files belong to F13 (#455);
  `ItemViewer.Breadcrumb.cs` belongs to F14. Neither may be edited by this child.
- **#457 trap.** A method-level `[ExcludeFromCodeCoverage]` does not suppress nested lambdas; the
  compiler lifts them into a generated closure type the attribute never marks. Any thin-forwarder
  adapter introduced here must be a class-level-exempt adapter **type** that is `sealed` and **not
  `partial`**.
- **Shared csproj files.** `QuickFiler/QuickFiler.csproj` and `QuickFiler.Test/QuickFiler.Test.csproj`
  are non-SDK projects with explicit `<Compile Include>` entries and no globbing. Own entries only,
  minimal adjacent hunks, preserve CRLF. Additive fan-in conflicts are expected.
- **Tooling.** `csharpier` is pinned at 1.2.6 and requires a subcommand:
  `dotnet tool run csharpier format .`, not the bare `csharpier .` form in `CLAUDE.md`.
- **Baseline accuracy.** The coverage figures above are indicative and were recomputed once
  already. If research disproves any figure in this brief, the correction is recorded as a
  documented deviation in `spec.md` and the plan is written against reality.

## Research Outcome (2026-08-08)

Preparation research produced one artifact per production file under `research/`, per issue #136.
All five coverage rows in the table above were **confirmed** by independent recomputation — F12 is
the first child in the epic whose brief figures survived re-measurement unchanged. The corrections
found were structural, and are recorded in `spec.md` §7. Headline results:

- **No production edit is required on any of the five files.** No csproj edit, no ledger row, and the
  #457 trap does not engage.
- **49 untaken branch outcomes** on `BreadcrumbItemViewerLifecycleCoordinator.cs`; 46 reachable,
  6 structurally unreachable across the cluster with proofs recorded in `spec.md` §4.1.
- **Five latent defects promoted** via the MCP lifecycle: **#498**, **#499**, **#500**, **#501**,
  **#502**. A verified correction plus two adjacent defects were posted to existing **#488**.
- **Open #440** will rewrite arrow-key semantics in two of these files and is now registered in the
  epic's Known Conflict Risks. Tests pin current behavior.

Acceptance criteria for this child are the numbered `AC-1 .. AC-16` in `spec.md` and `US-1 .. US-8`
in `user-story.md`; the draft list above is superseded by them.

## Test Conditions to Consider

- [ ] Guard-clause untaken branches across all five files
- [ ] Cancellation and cancelled-token paths — **N/A** for `BreadcrumbMessengerHub.cs` and
      `BreadcrumbItemViewerLifecycleCoordinator.cs` (no `CancellationToken` in either)
- [ ] Double-invoke and re-entrancy guards
- [ ] Out-of-order and unexpected state transitions
- [ ] Disposal and post-disposal invocation paths — **N/A** for `Controllers/BreadcrumbBridgeRouter.cs`
      (implements no `IDisposable`, holds no disposable resource, has no disposal flag)
- [ ] Error/exception paths in message routing and bridge upgrade
- [ ] ~~Deterministic time-dependent behavior via injected clock and fake timers~~ — **STRUCK**; no
      time-dependent behavior exists in this cluster. Replaced by deterministic scheduler and
      completion-source control.

## Next Step

- [x] Promote to GitHub issue (feature request template), citing parent epic #136 — issue #495
- [x] Create the active feature folder from the template
- [x] Per-file research (5 artifacts under `research/`)
- [x] Feature documents (`spec.md`, `user-story.md`)
- [ ] Atomic plan and executor preflight clearance
- [ ] Execution, deferred to `epic-orchestrator`
