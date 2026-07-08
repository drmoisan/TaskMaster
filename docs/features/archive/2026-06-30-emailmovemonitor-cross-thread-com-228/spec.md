# emailmovemonitor-cross-thread-com (Spec)

- **Issue:** #228
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-06-30T22-55
- **Status:** Implemented (pending review/merge)
- **Version:** 0.2

## Context
`EmailMoveMonitor.UnhookItem(MailItem)` accesses thread-affine Outlook COM objects (`mail.Parent`, `Folder.EntryID`) from a ThreadPool thread because `QfcDatamodel.DequeueNextItemGroupAsync` invokes the unhook path inside `await Task.Run(...)`. Cross-thread access to STA-bound Outlook interop objects throws `System.Runtime.InteropServices.COMException: "The operation failed."`.

Environment:
- OS/version: Windows, Outlook VSTO add-in host
- Python version: N/A (C# / .NET Framework VSTO add-in)
- Command/flags used: Triggered during QuickFiler queue processing (dequeue/unhook)
- Data source or fixture: Live Outlook mail items hooked via `EmailMoveMonitor.HookItem`

Impact / Severity:
- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low


## Repro & Evidence
Steps to Reproduce:
1. Run QuickFiler with items hooked into the move monitor.
2. Trigger queue dequeue via `QfcDatamodel.DequeueNextItemGroupAsync`, which runs `TryUnhookOrReplace` -> `_moveMonitor.UnhookItem(node)` inside `await Task.Run(...)`.
3. Observe `COMException` ("The operation failed.") surfaced via `ExceptionDispatchInfo.Throw()` from the Outlook interop call on the background thread.

Expected:
Unhooking items from the move monitor completes without COM exceptions; all Outlook COM access remains on the owning/Outlook STA thread.

Actual:
`COMException: "The operation failed."` is thrown when `mail.Parent`/`Folder.EntryID` are evaluated on a ThreadPool thread. The displayed `ExceptionDispatchInfo.Throw()` frame is only a rethrow of the original background-thread interop failure.

Logs / Screenshots:
- [x] Attached minimal logs or screenshot
- Snippet: `System.Runtime.InteropServices.COMException: The operation failed.` rethrown via `System.Runtime.ExceptionServices.ExceptionDispatchInfo.Throw()`.


## Scope & Non-Goals
- In scope:
  - Eliminate cross-thread Outlook COM access in the `EmailMoveMonitor` hook/unhook path so all COM member access (`mail.Parent`, `Folder.EntryID`, `BeforeItemMove +=/-=`) executes on the captured Outlook STA thread.
  - Introduce a narrow `IEmailMoveMonitor` interface and an injectable marshal-to-STA delegate seam so the hook/unhook bookkeeping is deterministically unit-testable without a live Outlook process.
  - Remove the redundant `Task.Run` wrapper in `QfcDatamodel.DequeueNextItemGroupAsync` once the unhook path self-marshals.
  - Add MSTest + Moq + FluentAssertions unit coverage for the bookkeeping logic to meet the >=90% new/changed-code floor.
- Out of scope / non-goals:
  - Determining the original product intent of `EmailMoveMonitor` (the standing TODO). The fix corrects the threading defect; it does not redesign the feature.
  - Restoring the dead/commented-out `UnhookItemAsync` call path in `QfcDatamodel.QueueProcessing.cs`. `GetParentFolderAsync`/`UnhookItemAsync` are dormant (no active callers); they are hardened only insofar as the same seam is applied, but are not re-wired.
  - Broad refactors of `QfcQueue`, `QfcDatamodel`, or `QfcCollectionController` beyond the minimal changes needed to consume `IEmailMoveMonitor` and remove the `Task.Run` unhook wrapper.
- Explicitly excluded systems, integrations, or datasets: live Outlook/MAPI sessions in unit tests; WPF `Dispatcher`/STA message-pump behavior (exercised only via the injectable marshal delegate substitute).

## Root Cause Analysis
Cross-thread access to STA/thread-affine Outlook COM objects. `EmailMoveMonitor` is live and consumed by four production files: `QfcQueue.cs`, `QfcDatamodel.cs`, `QfcDatamodel.QueueProcessing.cs`, `QfcCollectionController.cs`. Files to inspect:
- `QuickFiler\Helper Classes\EmailMoveMonitor.cs` (failing line 48-50)
- `QuickFiler\Controllers\QfcDatamodel.QueueProcessing.cs` (`Task.Run` unhook path, lines 33, 70-105)
- `UnhookItemAsync` and `GetParentFolderAsync` also wrap COM access in `Task.Run` and are not safe alternatives.


## Proposed Fix

### Design summary (what changes where):
Adopt the research-recommended combination of approach (c) + (a), with (b) as secondary hardening:
- **(c) Self-marshal inside `EmailMoveMonitor`** — route all Outlook COM access (`HookItem`, `UnhookItem`, `UnhookAll`, and the dormant `UnhookItemAsync`/`GetParentFolderAsync`) through an injectable marshal-to-STA delegate that defaults to the existing `UiThread` seam (`UiThread.Dispatcher.Invoke` / `UiSyncContext`), captured at add-in startup in `ThisAddIn.cs:28`. This makes the class correct regardless of the caller's thread.
- **(a) Remove the redundant `Task.Run`** wrapping the unhook loop in `QfcDatamodel.DequeueNextItemGroupAsync` (`QfcDatamodel.QueueProcessing.cs:70-105`); wrapping an already-marshaled call in `Task.Run` adds a hop without value.
- **(b) Cache stable EntryID strings at hook time** (on the STA thread, in `EmailMoveAction`) to reduce repeated live-COM property gets during unhook comparisons, mirroring the `MailItemHelper` lazy-EntryID precedent.

### Boundaries and invariants to preserve:
- The `lock (_hookedItems)` bookkeeping invariant (subscribe `BeforeItemMove` only for the first item per folder; unsubscribe only when the last item for that folder is removed).
- No deadlock between a marshaled synchronous call awaiting the STA thread and the STA thread's own event-dispatch reentrancy (`BeforeItemMove` is raised by Outlook on the STA thread).
- `BeforeItemMove` handler stays STA-bound by Outlook contract; do not re-marshal the handler body.
- Public/observable behavior of `DequeueNextItemGroupAsync` (returns the dequeued node list) is unchanged.

### Dependencies or blocked work:
- Relies on `UiThread.Init(...)` already running at startup (`ThisAddIn.cs:28`) — confirmed present. No new infrastructure required.

### Implementation strategy (what changes, not sequencing):

#### Files/modules to change:
- `QuickFiler\Helper Classes\EmailMoveMonitor.cs` — add `IEmailMoveMonitor`, injectable marshal delegate, cached EntryID in `EmailMoveAction`, marshal all COM access.
- `QuickFiler\Controllers\QfcDatamodel.QueueProcessing.cs` — remove the `Task.Run` unhook wrapper; keep the per-item retry/replace bookkeeping.
- `QuickFiler\Controllers\QfcDatamodel.cs`, `QfcQueue.cs`, `QfcCollectionController.cs` — change field/type to `IEmailMoveMonitor` (minimal; construction stays `new EmailMoveMonitor(...)` with default marshal delegate).
- New test file(s) under `QuickFiler.Test` for `EmailMoveMonitor` bookkeeping.

#### Functions/classes/CLI commands impacted:
`EmailMoveMonitor.HookItem/UnhookItem/UnhookAll`, `EmailMoveAction`, `QfcDatamodel.TryUnhookOrReplace`, `QfcDatamodel.DequeueNextItemGroupAsync`.

#### Data flow and validation changes:
`EmailMoveAction` records `Mail.EntryID` and `Folder.EntryID` as strings captured on the STA thread at hook time; unhook comparisons prefer cached IDs over live COM re-reads. Null-guard behavior of `UnhookItem(null)` preserved.

#### Error handling and logging updates:
Preserve existing log4net logging in `TryUnhookOrReplace`. Marshaling failures must fail fast with context rather than being silently swallowed; do not broaden catch scope.

#### Rollback/feature-flag considerations (if applicable):
None required; change is behavior-preserving except for thread affinity. The default marshal delegate keeps production behavior identical to a correctly-threaded call.

### Technical specifications (interfaces/contracts):

#### Inputs/outputs and formats:
`IEmailMoveMonitor` exposes the existing signatures (`HookItem(MailItem, Action<MailItem>)`, `UnhookItem(MailItem)`, `UnhookAll()`). Constructor accepts an optional `Action<Action>` marshal delegate defaulting to the `UiThread` STA dispatcher.

#### Required configuration keys and defaults:
None.

#### Backward-compatibility expectations:
No public-API breakage outside the QuickFiler assembly (`EmailMoveMonitor` is `internal`). Callers migrate from concrete type to `IEmailMoveMonitor`.

#### Performance constraints (latency/throughput/memory):
Marshaling adds one STA-dispatch hop per hook/unhook call; COM property gets are fast. Caching EntryIDs reduces COM round-trips. No throughput regression expected.

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

- [ ] Unit coverage areas: unhook bookkeeping logic separated from COM access via a seam, tested with Moq + FluentAssertions (MSTest).
- [ ] Integration scenario to retest: queue dequeue/unhook on the Outlook thread without COMException.
- [ ] Manual verification notes: confirm no COM access executes on a ThreadPool thread.

- Regression tests to add or update: MSTest tests asserting that `HookItem`/`UnhookItem`/`UnhookAll` invoke COM access only through the injected marshal delegate (verify the delegate is called; substitute a synchronous pass-through and a thread-id-capturing fake to prove COM access does not occur on a foreign thread).
- Unit tests (MSTest + Moq + FluentAssertions) for the fixed behavior and boundaries: hook bookkeeping (subscribe only on first item per folder), unhook bookkeeping (unsubscribe only on last item per folder), `UnhookItem(null)` no-op, cached-EntryID comparison path, `UnhookAll` clears state.
- Edge cases and negative scenarios: null `MailItem`; item not currently hooked; multiple items sharing one folder; duplicate hook of the same item.
- Error handling and logging verification: `TryUnhookOrReplace` retry/replace loop still logs via log4net on failure; marshaling exceptions propagate with context.
- Coverage impact and targets for changed lines/modules: `EmailMoveMonitor` bookkeeping is new/changed testable code and must reach >=90%; repo-wide floor stays >=80% (testable denominator). Live COM event subscription and live STA dispatcher behavior remain COM-host-bound and exemption-eligible only where no seam reaches them.
- Toolchain commands to run (format → lint → type-check → test): `csharpier .`; `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`; `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`; `vstest.console.exe <QuickFiler.Test assembly> /EnableCodeCoverage`.
- Manual validation steps (if required): run QuickFiler queue dequeue with hooked items in a live Outlook session; confirm no `COMException` and correct unhook behavior.


## Acceptance Criteria
- [x] AC1: No Outlook COM member access (`mail.Parent`, `Folder.EntryID`, `BeforeItemMove +=/-=`) in `EmailMoveMonitor` executes on a ThreadPool/background thread; all such access is marshaled to the captured Outlook STA thread. Evidence: all COM access in `HookItem`/`UnhookItem`/`UnhookAll` (`QuickFiler\Helper Classes\EmailMoveMonitor.cs`) is wrapped in `_marshalToSta(...)`; regression test `UnhookItem_InvokedFromThreadPoolThread_RunsComAccessOnMarshalTargetThread` (`QuickFiler.Test\Helper Classes\EmailMoveMonitorTests.cs`) proves the COM-access body runs on the marshal-target thread, not the invoking ThreadPool thread.
- [x] AC2: The redundant `Task.Run` wrapper around the unhook loop in `QfcDatamodel.DequeueNextItemGroupAsync` is removed; the method's returned-node behavior is unchanged. Evidence: `QuickFiler\Controllers\QfcDatamodel.QueueProcessing.cs` — the `await Task.Run(...)` wrapper is removed, the `for` loop calling `TryUnhookOrReplace` runs directly inside the preserved `try/catch` that logs and rethrows; `return nodes;` unchanged.
- [x] AC3: `EmailMoveMonitor` is consumed through `IEmailMoveMonitor` with an injectable marshal-to-STA delegate that defaults to the existing `UiThread` seam; tests substitute a deterministic pass-through. Evidence: `QuickFiler\Interfaces\IEmailMoveMonitor.cs`; `EmailMoveMonitor(Action<System.Action> marshalToSta = null)` defaults to `action => UiThread.Dispatcher.Invoke(action)`; `_moveMonitor` fields in `QfcDatamodel.cs`, `QfcQueue.cs`, `QfcCollectionController.cs` are typed `IEmailMoveMonitor`; tests use `a => a()` pass-through.
- [x] AC4: Regression/unit tests added and passing. Evidence: `QuickFiler.Test\Helper Classes\EmailMoveMonitorTests.cs` (8 tests, all passing): `HookItem_FirstItemOfFolder_SubscribesBeforeItemMoveOnce_AndSharedFolderDoesNotResubscribe`, `UnhookItem_RemovesLastItemForFolder_UnsubscribesBeforeItemMoveOnlyOnLastItem`, `UnhookItem_Null_IsNoOp_NoComAccessNoMarshalInvocation`, `UnhookItem_UsesCachedEntryIds_RemovesExactlyTheMatchingEntry`, `AllComAccess_FlowsThroughInjectedMarshalDelegate`, `UnhookAll_UnsubscribesEveryFolder_AndClearsState`, `DuplicateHookOfSameItem_AndUnhookNeverHookedItem_DoNotThrowOrSpuriouslyUnsubscribe`, `UnhookItem_InvokedFromThreadPoolThread_RunsComAccessOnMarshalTargetThread`.
- [x] AC5: Changed/new `EmailMoveMonitor` bookkeeping code reaches >=90% line coverage; repo-wide coverage no-regression on changed lines (testable denominator); COM-host-bound exemption documented and scoped. Evidence: `evidence/qa-gates/coverage-delta.2026-06-30T18-10.md` — changed/new bookkeeping = 96.92% (63/65); QuickFiler first-party package coverage rose 32.94% -> 33.74% (no changed-line regression); exempt-vs-non-exempt boundary documented (BeforeItemMove handler body and dormant async members exemption-eligible; marshaled bookkeeping NOT exempt and meets the floor).
- [x] AC6: No banned-API regressions; existing `TimeProvider.Delay` preserved. Evidence: `evidence/qa-gates/qa-analyzers.2026-06-30T18-10.md` — BannedApiAnalyzers produced no diagnostics for changed files; no `DateTime.Now/UtcNow`/`Random.Shared`/`Thread.Sleep`/`Task.Delay` introduced; `TimeProvider.Delay` in `QfcDatamodel.QueueProcessing.cs` `WaitForQueue` unchanged.
- [x] AC7: No unintended behavior changes outside the defined scope; existing log4net logging in `TryUnhookOrReplace` preserved. Evidence: `TryUnhookOrReplace` body unchanged (retry/replace + log4net); the `DequeueNextItemGroupAsync` `try/catch` logging `"Error unhooking items from move monitor"` is preserved; the commented-out `UnhookItemAsync` path remains commented out.
- [x] AC8: Full toolchain pass completed in order with no failures in the final pass. Evidence: `evidence/qa-gates/` — qa-csharpier (EXIT 0), qa-analyzers (EXIT 0), qa-nullable (EXIT 0), qa-tests-coverage (EXIT 0, 209/209 passed).
- [x] AC9: Spec/issue references updated to reflect the implemented behavior. Evidence: this spec (Status -> Implemented, AC1–AC9 checked); issue-update mirror `evidence/issue-updates/issue-228.2026-06-30T18-10.md`.

## Risks & Mitigations
- Technical or operational risks:
- Mitigations and rollbacks:

## Rollout & Follow-up
- Release/rollout steps:
- Post-fix monitoring or clean-up tasks:
- Links: issue, PRs, related docs
