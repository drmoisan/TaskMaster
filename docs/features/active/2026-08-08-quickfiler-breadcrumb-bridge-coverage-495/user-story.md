# `quickfiler-breadcrumb-bridge-coverage` — User Story

- Issue: #495
- Owner: drmoisan
- Status: Draft
- Last Updated: 2026-08-08T00-32

## Story Statement

- As a ..., I want ..., so that ...
- As a ..., I want ..., so that ...

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


## Personas & Scenarios

- Persona: ...
  - who the user is
  - what they care about
  - their constraints
  - their goals and frustrations
  - their context and motivations
- Scenario: ...
  - A concrete, step-by-step narrative that describes how a user accomplishes a goal in a real-world context using the system.
  - who is acting?
  - what triggered the action?
  - what steps do they take?
  - what obstacles or decisions occur?
  - what outcome do they expect?


## Acceptance Criteria

- [ ] Every one of the five assigned files reaches >= 80% line AND >= 75% branch, verified with
- [ ] F1's harness, recorded as numeric evidence under `<FEATURE>/evidence/qa-gates/`. For a file
- [ ] already above a floor, the bar on that axis is retain-or-improve.
- [ ] `Viewers/BreadcrumbItemViewerLifecycleCoordinator.cs` closes its branch gap to >= 75%.
- [ ] Repository-wide coverage retained or improved, measured as a self-consistent before/after
- [ ] pair on this branch using an identical command and identical post-processing. No imported
- [ ] figure is a valid comparison baseline.
- [ ] No production file exceeds 500 lines; any newly created file reaches >= 90% line coverage
- [ ] and gets a ledger row per the epic's "Mid-Wave File Creation" rules.
- [ ] Tests use MSTest, Moq, and FluentAssertions; deterministic and isolated; no temporary files,
- [ ] no external services, no live forms, no popups, no `Thread.Sleep`/`Task.Delay`/wall-clock
- [ ] waits.
- [ ] Full C# toolchain green in final form.
- [ ] No behavior change to observable QuickFiler flows.


## Non-Goals

Call out what is explicitly excluded from this feature.
