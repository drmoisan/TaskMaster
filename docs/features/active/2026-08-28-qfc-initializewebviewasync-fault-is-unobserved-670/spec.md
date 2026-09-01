# 2026-08-28-qfc-initializewebviewasync-fault-is-unobserved (Spec)

- **Issue:** #670
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-08-31T20-20
- **Status:** Draft
- **Version:** 0.1

## Context
`QfcItemController.InitializeWebViewAsync` (`QuickFiler/Controllers/QfcItemController.ViewerSetup.cs:48`)
returns a `Task` that **three of its four production call sites discard**, so any exception it raises becomes an
unobserved task exception rather than a diagnostic anyone sees. The method is the sole entry point for WebView2
environment creation, core initialization, and — at `ViewerSetup.cs:112` — the call to `EnsureBreadcrumbPipeline()`.
Issue #488's D5 fix makes that path newly capable of throwing `ObjectDisposedException` when the pipeline is built
against a viewer whose teardown has begun, which converts a previously silent leak into a fault that is itself
silently swallowed.

Environment:
- OS/version: Windows 11 Pro 10.0.26200
- Python version: n/a (.NET Framework 4.8.1, VSTO / WinForms)
- Command/flags used: n/a — identified by source reading during issue #488 execution, discharging research §3.5
- Data source or fixture: `QuickFiler/Controllers/QfcItemController.Initialization.cs` call sites

Impact / Severity:
- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

The severity comes from the failure mode rather than the likelihood. A WebView2 initialization failure — a missing
runtime, a locked cache directory, a disposed viewer — produces no diagnostic on three of four paths, so the
breadcrumb surface simply never appears and the cause is unavailable to anyone triaging it.


## Repro & Evidence
Steps to Reproduce:
1. Drive a `QfcItemController` through any of the three fire-and-forget initialization paths listed under
   "Suspected Cause / Notes".
2. Arrange for `InitializeWebViewAsync` to fault — for example by disposing the `ItemViewer` before the posted
   continuation reaches `EnsureBreadcrumbPipeline()`, which after #488's D5 fix throws `ObjectDisposedException`.
3. Observe that no exception surfaces to the caller, no log entry is written by the call site, and initialization
   silently completes as far as any observer can tell.

Expected:
A faulted `InitializeWebViewAsync` should be observed by its caller — awaited, `ContinueWith`-observed, or routed to
the repository's logging pattern — so that a failure during WebView2 initialization is diagnosable rather than
invisible.

Actual:
The task is discarded at three of the four call sites, so the fault is never observed:

| Call site | Form | Observed? |
| --- | --- | --- |
| `QfcItemController.Initialization.cs:192` | `_ = _itemViewer.UiDispatcher.InvokeAsync(InitializeWebViewAsync);` | **no** — discarded, and additionally wrapped in a WPF `DispatcherOperation` |
| `QfcItemController.Initialization.cs:256` | `await InitializeWebViewAsync();` | yes — awaited into the enclosing async method's task |
| `QfcItemController.Initialization.cs:288` | `_ = InitializeWebViewAsync();` | **no** — discarded |
| `QfcItemController.Initialization.cs:324` | `_ = InitializeWebViewAsync();` | **no** — discarded |

On .NET Framework 4.5 and later an unobserved task exception no longer terminates the process by default, so the
fault is finalized away with no observable effect at all.

Logs / Screenshots:
- [ ] Attached minimal logs or screenshot
- Snippet: no captured log — that is the defect. Identified by source reading, recorded in
  `docs/features/active/itemviewer-breadcrumb-lifecycle-defects-488/evidence/qa-gates/d5-faulted-task-observation.md`.


## Scope & Non-Goals
- In scope:
- Out of scope / non-goals:
- Explicitly excluded systems, integrations, or datasets:

## Root Cause Analysis
The three discarding sites were written as deliberate fire-and-forget dispatches, with the comment "Fire and forget
WebView initialization" at `Initialization.cs:191`. The intent — not blocking initialization on a WebView2 round trip
— is sound; discarding the fault is the part that is not.

`QuickFiler/Controllers/QfcItemController.ViewerSetup.cs` and
`QuickFiler/Controllers/QfcItemController.Initialization.cs` are owned by feature
`qfc-item-controller-defects-484`, so this was **not** fixed inside #488. Research §3.5 required that if the task
proves unobserved the correct response is a new issue against `ViewerSetup.cs`, **not** a weakening of D5's guard.
D5's guard is delivered unweakened.

`EfcItemController.cs:97` and `:153` use `Task.Run(() => InitializeWebViewAsync());` against that class's own
same-named method and discard the returned task too; whether that belongs in the same fix is worth evaluating.

Options worth evaluating:

- Attach a continuation at each fire-and-forget site that routes a fault to the project's logging pattern.
- Introduce a small `FireAndForget(Task, ILogger)` helper so the three sites share one observation policy.
- Subscribe `TaskScheduler.UnobservedTaskException` at the add-in boundary as a backstop only.

Files to inspect: `QuickFiler/Controllers/QfcItemController.ViewerSetup.cs`,
`QuickFiler/Controllers/QfcItemController.Initialization.cs`, `QuickFiler/Controllers/EfcItemController.cs`.


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

- [ ] Unit coverage areas: a test that forces `InitializeWebViewAsync` to fault at the mocked web-view seam and asserts the fault is observed and logged rather than discarded
- [ ] Integration scenario to retest: dispose an `ItemViewer` mid-initialization and confirm the resulting `ObjectDisposedException` reaches a log
- [ ] Manual verification notes: confirm the three fire-and-forget sites still do not block initialization after the change

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
