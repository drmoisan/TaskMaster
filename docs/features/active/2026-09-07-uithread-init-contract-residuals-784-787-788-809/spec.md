# 2026-09-07-uithread-init-contract-residuals-784-787-788 (Spec)

- **Issue:** #809
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-09-07T20-14
- **Status:** Draft
- **Version:** 0.1

## Context
Consolidates three findings on one file, `UtilitiesCS/Threading/UiThread.cs`, that were filed separately as #784, #787, and #788 after the #781 and #782 reviews. (1) `Init()` accepts a non-STA caller and installs that worker's non-pumping dispatcher and context into set-once process-global state (#787). (2) `Init()` consumes its single-shot latch before `Initialize()` runs, so a failed first attempt can never be retried, and the naive re-arm was measured to regress in #782 (#788). (3) `SynchronizationContextAwaiter.IsCompleted` compares contexts by reference, so any context captured inside a WPF dispatcher operation always posts instead of continuing inline on the UI thread (#784). All three touch the same initialization and awaiter code and should ship as one change with one test suite.

Environment:
- OS/version: Windows 11 Pro 10.0.26200
- Runtime: .NET Framework 4.8 VSTO add-in hosted by Outlook desktop; `main` at `04a54e68`
- Command/flags used: `vstest.console.exe <test assemblies> /InIsolation`; runtime probe in the #781 feature folder (`evidence/other/dispatcher-synccontext-probe.2026-09-05T10-40.md`)
- Data source or fixture: `QuickFiler.Test/Controllers/QfcHomeControllerRunAsyncTests.cs:329` (MTA caller of `UiThread.Init(false)`)

Impact / Severity:
- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

Medium, carried from #787: in production `ThisAddIn.cs:35-40` is the only `Init()` caller and runs on the Outlook STA, so the hazards are reachable today only from test code, but a worker-thread read of the lazy accessors before startup completes would poison the process. #784 and #788 are Low individually.


## Repro & Evidence
Steps to Reproduce:
1. #787: call `UiThread.Init(false)` from an MTA thread (the in-repo instance is the test at `QfcHomeControllerRunAsyncTests.cs:329`). It returns normally and every later `UiThread.Dispatcher` / `UiSyncContext` / `UiThreadId` read marshals onto a thread with no message loop.
2. #788: arrange for `Initialize()` to throw (headless or non-STA), call `Init()`, fix the condition, call `Init()` again. The second call is a no-op because `_loaded.CheckAndSetFirstCall` at `UiThread.cs:36` was consumed before `Initialize()` ran.
3. #784: construct an `ItemViewer` through `ItemViewerQueue.Dequeue` (inside `UiThread.Dispatcher.Invoke`, so `UiSyncContext` is a `DispatcherSynchronizationContext`), then on the UI thread evaluate `viewer.UiSyncContext.GetAwaiter().IsCompleted`. It is `false`, so the continuation posts instead of running inline.

Expected:
- `Init()` rejects a non-STA caller with a named `InvalidOperationException` before capturing anything.
- A failed `Initialize()` leaves the latch re-armed so a later `Init()` retries, without reintroducing the regression #782 measured.
- `IsCompleted` is true when the caller already runs on the owning UI thread, regardless of which `SynchronizationContext` instance is ambient.

Actual:
See the three reproduction steps. #787 succeeds silently and poisons the globals for the process lifetime; #788 leaves `UiThread.Dispatcher` throwing an exception that names `Init()` as the remedy while `Init()` is a no-op; #784 adds one queued hop per await and changes ordering relative to already-queued UI work.

Logs / Screenshots:
- [x] Attached minimal logs or screenshot
- Snippet: `UtilitiesCS/Threading/UiThread.cs` line 100 (verified 2026-09-05): `public bool IsCompleted => _context == SynchronizationContext.Current;` (reference comparison). Probe result: `Invoke ctx == outer ambient : False` on .NET Framework 4.8 STA. #787 and #788 are missing-precondition and ordering defects with no diagnostic output.


## Scope & Non-Goals
- In scope:
- Out of scope / non-goals:
- Explicitly excluded systems, integrations, or datasets:

## Root Cause Analysis
- #787: no `Thread.CurrentThread.GetApartmentState() == ApartmentState.STA` check in `Init()` or `Initialize()`; `CaptureUiVariables()` reads `SynchronizationContext.Current`, `AutoScaleFactor`, `Dispatcher.CurrentDispatcher`, and the managed thread id from the caller unconditionally.
- #788: latch consumed at `UiThread.cs:36` before `Initialize()`; the naive fix (re-arm on throw) was applied and withdrawn in #782 after measuring a reproducible regression. Read the #782 feature folder before choosing the fix.
- #784: reference equality on `SynchronizationContext`; the correct predicate is owning-thread identity (`UiThread.UiThreadId == Thread.CurrentThread.ManagedThreadId`), the same ownership test #781 adopted for the breadcrumb UI-boundary guard.
- Superseded issues: #784, #787, #788 (close with a pointer to this issue).


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

Acceptance criteria:

- [ ] AC1: `Init()` throws a named `InvalidOperationException` when called from a non-STA thread, before any global is captured; the MTA test caller at `QfcHomeControllerRunAsyncTests.cs:329` is corrected or given an STA host.
- [ ] AC2: A failed `Initialize()` does not consume the latch; a subsequent `Init()` retries and succeeds. The #782 regression scenario is reproduced as a test and passes with the chosen design.
- [ ] AC3: `SynchronizationContextAwaiter.IsCompleted` returns true on the owning UI thread regardless of ambient context instance, and false elsewhere; ordering-sensitive callers in `ItemViewer` and `EfcFormController` still pass their existing tests.
- [ ] AC4: Unit tests cover STA/MTA rejection, latch re-arm after throw, and awaiter inline-vs-post decisions with a fake dispatcher seam; no real Outlook host.

Validation:

- [ ] Unit coverage areas: `UiThread.Init`, `Initialize`, `CaptureUiVariables`, `SynchronizationContextAwaiter`.
- [ ] Integration scenario to retest: full nine-assembly `/InIsolation` run; QuickFiler launch, item load, and breadcrumb open on a live host.
- [ ] Manual verification notes: no change in observable UI behavior; verify no new keyboard-focus regressions after #677/#796.

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
