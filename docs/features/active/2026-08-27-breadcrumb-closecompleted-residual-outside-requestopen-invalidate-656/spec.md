# 2026-08-27-breadcrumb-closecompleted-residual-outside-requestopen-invalidate (Spec)

- **Issue:** #656
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-08-31T20-10
- **Status:** Draft
- **Version:** 0.1

## Context
`BreadcrumbDropDownOpenCoordinator._closeCompleted` stays `true` when the drop-down host is reopened by
a path that reaches neither `RequestOpen` nor `Invalidate`, so a subsequent close is wrongly suppressed.
This is the known residual of the SR-4 two-flag close fix shipped for #462 under #501, recorded against
the host paths owned by feature #488.

Environment:
- OS/version: Windows 11, Outlook VSTO add-in host
- Python version: n/a (C#, .NET Framework 4.8)
- Command/flags used: `msbuild TaskMaster.sln /t:Rebuild /m /p:Configuration=Debug "/p:Platform=Any CPU"`
- Data source or fixture: `QuickFiler.Test/Viewers/BreadcrumbDropDownOpenCoordinatorTests.Part2.cs` harness

Impact / Severity:
- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

Medium: it requires a reopen path that bypasses both entry points, which the currently exercised UI
flows do not take. It is a latent correctness gap rather than an observed user-facing failure.


## Repro & Evidence
Steps to Reproduce:
1. Open the breadcrumb drop-down host and close it through `CloseCore`, so `_closeCompleted` becomes `true`.
2. Reopen the host through a path that reaches neither `RequestOpen` nor `Invalidate`.
3. Request a close.

Expected:
The close request reaches `_host.Close`, because the host is genuinely open again.

Actual:
The coordinator still treats the host as already closed and suppresses the close. `_closeCompleted` was
never cleared, because it is cleared only on the `RequestOpen` and `Invalidate` paths.

Logs / Screenshots:
- [ ] Attached minimal logs or screenshot
- Snippet: no runtime log; the residual is established by source inspection of the flag-clearing paths.


## Scope & Non-Goals
- In scope:
- Out of scope / non-goals:
- Explicitly excluded systems, integrations, or datasets:

## Root Cause Analysis
#462 was fixed by replacing the single `_closePending` flag with two flags, `_closeInFlight` and
`_closeCompleted`. `_closeCompleted` is cleared on `RequestOpen` and `Invalidate` only.

The two-flag form was chosen deliberately. The naive alternative, clearing the close flag on the
successful-close path, makes two existing must-pass tests fail by letting a second `CloseCore` reach
`_host.Close`: `PendingToggleClose_HostOwnershipSuppressesFallbackAndRepeatedClose` and
`SelectorStateTransitions_RequestOpenThenCloseOnlyWhenRequired`. Both encode the repeated-close
suppression contract. The two-flag form passes all three must-pass tests with no test edit, so it was
shipped and this residual recorded rather than traded for a regression.

This belongs to feature #488, not #501: the reopen paths that bypass `RequestOpen` and `Invalidate`
live in the ItemViewer breadcrumb lifecycle host surface. #501 was not permitted to write
`BreadcrumbItemViewerLifecycleCoordinator.cs`, `BreadcrumbDropDownHost.cs` or `ItemViewer.Breadcrumb.cs`.


## Proposed Fix

### Design summary (what changes where):

### Boundaries and invariants to preserve:

### Dependencies or blocked work:

### Implementation strategy (what changes, not sequencing):
	
#### Files/modules to change:

#### Functions/classes/CLI commands impacted:

#### Data flow and validation changes:

#### Error handling and logging updates:

#### Rollback/feature-flag considerations (if applicable):

### Technical specifications (interfaces/contracts):

#### Inputs/outputs and formats:

#### Required configuration keys and defaults:

#### Backward-compatibility expectations:

#### Performance constraints (latency/throughput/memory):

## Assumptions, Constraints, Dependencies
- Assumptions (environment, data, access):
- Constraints (budget, performance, compatibility):
- External dependencies (services, libraries, releases):

## Data / API / Config Impact
- User-facing or API changes:
- Data or migration considerations:
- Logging/telemetry updates (if any):
- Compatibility notes (CLI flags, config schemas, versioning):

## Test Strategy
Seeded from issue:

- [ ] Enumerate every path that reopens the drop-down host
- [ ] For any path reaching neither `RequestOpen` nor `Invalidate`, route it through one of them or clear `_closeCompleted` explicitly
- [ ] Add a regression test driving that path, keeping the three must-pass tests unedited

- Regression tests to add or update:
- Unit tests (pytest) for the fixed behavior and boundaries:
- Edge cases and negative scenarios (invalid inputs, missing data, boundary values):
- Error handling and logging verification:
- Coverage impact and targets for changed lines/modules:
- Toolchain commands to run (format → lint → type-check → test):
- Manual validation steps (if required):


## Acceptance Criteria
- [ ] Repro steps now produce the expected behavior in all documented environments.
- [ ] Regression test(s) added and passing (list file path and test name).
- [ ] Edge cases and invalid inputs are handled with correct errors or fallbacks.
- [ ] No unintended behavior changes outside the defined scope.
- [ ] Required logs/telemetry updated and validated (if applicable).
- [ ] Performance constraints met or explicitly waived with rationale.
- [ ] Full toolchain pass completed (format → lint → type-check → test).
- [ ] Docs/config references updated to match the new behavior.

## Risks & Mitigations
- Technical or operational risks:
- Mitigations and rollbacks:

## Rollout & Follow-up
- Release/rollout steps:
- Post-fix monitoring or clean-up tasks:
- Links: issue, PRs, related docs
