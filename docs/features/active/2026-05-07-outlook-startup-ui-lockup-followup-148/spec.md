# 2026-05-07-outlook-startup-ui-lockup-followup (Spec)

- **Issue:** #148
- **Parent (optional):** none
- **Owner:** drmoisan
- **Last Updated:** 2026-05-07T19-34
- **Status:** Draft
- **Version:** 0.1

## Context
Outlook and TaskMaster still lock up for an extended period during startup and during first email interactions while initial loading work continues without updating the UI. This follow-up bug remains unresolved after issue `#141` and should use the timing approach from issue `#139` to isolate remaining UI-thread-heavy startup and mail-selection paths.

Environment:
- OS/version: Windows 10/11 with Outlook desktop and the TaskMaster VSTO add-in enabled
- Python version: Not applicable; this path is a .NET Framework Outlook add-in startup and Outlook interaction path
- Command/flags used: Standard Outlook launch into `ThisAddIn.Application_Startup()` with no special flags
- Data source or fixture: Live Outlook profile with the normal TaskMaster startup data/configuration and enough mailbox content to click messages during the initial load window

Impact / Severity:
- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low


## Repro & Evidence
Steps to Reproduce:
1. Start Outlook with the TaskMaster add-in enabled.
2. During the initial startup load window, observe the Outlook window while TaskMaster startup work is still running.
3. Click one or more emails before startup processing has fully completed.
4. Observe whether the Outlook and TaskMaster UI stops repainting or accepting interaction while background and startup operations continue.

Expected:
There should be no perceivable latency during Outlook startup or the first email interactions after launch. Startup coordination, data loading, and selection-driven updates should use `async`/`await`, keep UI-thread work minimal, and leave the Outlook window responsive and repainting throughout.

Actual:
The UI still locks up for an extended period during startup and when clicking emails during the initial load window. The UI does not visibly update while operations continue, which indicates that too much work is still being performed on the UI thread or resumed there too aggressively.

Logs / Screenshots:
- [ ] Attached minimal logs or screenshot
- Snippet: No new log snippet is attached in this follow-up entry; existing startup timing context is already documented in issues `#141` and `#139`, and additional targeted instrumentation may still be required to isolate the remaining root cause.


## Scope & Non-Goals
- In scope:
- Add targeted startup and first-selection instrumentation in the active startup and QuickFiler paths so the remaining lock-up can be attributed to specific UI-thread segments instead of a single undifferentiated startup delay.
- Treat the likely implementation scope as `TaskMaster/AppGlobals/AppEvents.cs`, `QuickFiler/Controllers/EfcHomeController.cs`, `QuickFiler/Controllers/EfcDataModel.cs`, `QuickFiler/Helper Classes/ConversationResolver.cs`, `UtilitiesCS/Extensions/DfDeedle.cs`, `UtilitiesCS/OutlookObjects/Conversation/ConversationHelper.cs`, `UtilitiesCS/OutlookObjects/MailItem/MailItemHelper.cs`, and `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.cs`.
- Preserve the Outlook STA/UI-thread ownership of COM-affine work while moving only snapshot-safe transforms, frame construction, ranking, tokenization, and other pure computation behind explicit snapshot boundaries and `async`/`await` stages.
- Clarify whether `AppEvents.ProcessNewInboxItemsAsync()` should be batched, deferred, or left in place with tighter timing visibility if it still dominates the startup freeze window.
- Reduce first-email interaction blocking by separating selection/conversation/table acquisition from background-safe dataframe and model transforms, and by publishing UI updates in coarse stages rather than repeated fine-grained dispatcher hops.
- Out of scope / non-goals:
- Broad startup architecture replacement, new feature flags, new user-facing startup controls, or redesign of the Outlook add-in lifecycle outside the lock-up fix.
- Treating Outlook COM access as background-safe by wrapping additional object-model calls in `Task.Run`.
- Touching `TaskMaster/AppGlobals/ApplicationGlobals.cs`, `TaskMaster/AppGlobals/AppOlObjects.cs`, `UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs`, `UtilitiesCS/OutlookObjects/Store/StoreWrapper.cs`, or `UtilitiesCS/OutlookObjects/Folder/FolderMinimalWrapper.cs` unless the new instrumentation shows one of those files still owns the dominant remaining UI-thread stall.
- Authoring a `user-story.md`; this active item is the full-bug artifact for issue `#148`.
- Explicitly excluded systems, integrations, or datasets:
- Non-Outlook workflows, non-startup UI surfaces, unrelated TaskMaster modules, and any persisted-data schema or configuration migration.
- Synthetic or external datasets beyond the live Outlook profile and normal persisted TaskMaster state already required to reproduce the bug.

## Root Cause Analysis
This is a follow-up to unresolved issue `#141`, informed by the startup timing instrumentation work in issue `#139`. The remaining problem appears to be that startup and early email-selection flows still perform too much work on the UI thread, or resume to it too often, so Outlook cannot repaint or reflect progress while the operations continue.

The research is sufficient to complete this spec. It identifies two overlapping pipelines that can lock the Outlook window during the same launch session: the startup pipeline (`ThisAddIn.Application_Startup()` -> `IdleAsyncQueue` -> `ApplicationGlobals.LoadSequentialAsync()` -> `AppEvents.LoadAsync()`) and the first-email pipeline (`EfcHomeController` -> `EfcDataModel` -> `ConversationResolver` -> `ConversationHelper` / `DfDeedle` / `OlTableExtensions` / `MailItemHelper`). The prior `#141` fix reduced store-rewire blocking but intentionally left `AppEvents` startup inbox processing and the QuickFiler conversation path outside the minimal fix scope, which matches the remaining symptom reported here.

The strongest repository-grounded risk is the current pattern of wrapping Outlook COM-backed work in `Task.Run` instead of creating an explicit snapshot on the Outlook STA thread and then transforming that snapshot off-thread. Research shows that `ConversationHelper`, `OlTableExtensions`, `DfDeedle`, and `MailItemHelper` still combine or disguise COM-heavy work with asynchronous method names, while `AppEvents.ProcessNewInboxItemsAsync()` still performs restricted inbox enumeration, `MailItem` materialization, and mail processing during startup. That means startup work and first-selection work can both compete for the same UI/STA thread.

For this bug, COM-affine work that must remain on the Outlook/UI thread includes: `ActiveExplorer().Selection`, `CurrentFolder`, `CurrentView`, conversation acquisition, `Conversation.GetTable()`, `MAPIFolder.GetTable()`, row/value extraction from `Outlook.Table`, inbox `Restrict`, and eager materialization of mail-item properties backed by Outlook COM. Background-safe work begins only after those values have been copied into immutable snapshots such as DTOs, arrays, column maps, or dataframe-ready structures. The remaining lock-up is therefore not just “startup is slow”; it is that the code still performs conversation acquisition, row extraction, and mail-item projection in long UI-owned segments and then resumes UI work too frequently while startup is still active.

Related docs:
- `docs/features/active/2026-05-05-outlook-startup-ui-thread-deblock-141/issue.md`
- `docs/features/active/2026-04-21-outlook-startup-store-rewire-ui-lock-instrumentation-139/issue.md`


## Proposed Fix

### Design summary (what changes where):
- Keep Outlook COM acquisition on the Outlook STA/UI thread, but refactor the remaining hotspot paths into an explicit `UI-thread snapshot -> background transform -> single UI-thread publish` pattern.
- Add startup and first-selection timing envelopes that log phase name, thread identity, synchronization-context identity, selected-item count where relevant, and whether startup is still active, so the follow-up implementation can prove which segments remain UI-owned.
- Treat `AppEvents.LoadAsync()` and `ProcessNewInboxItemsAsync()` as the first startup follow-up target because research shows they still perform inbox restriction, row enumeration, and mail-item processing during startup with no direct regression coverage today.
- Treat `EfcHomeController`, `EfcDataModel`, `ConversationResolver`, `ConversationHelper`, `DfDeedle`, `OlTableExtensions`, and `MailItemHelper` as the first-click follow-up target because they currently overlap live selection/conversation COM work with dataframe/model initialization.
- Observe `ApplicationGlobals`, `AppOlObjects`, `StoresWrapper`, `StoreWrapper`, and `FolderMinimalWrapper`, but do not commit to changing them unless instrumentation proves they still dominate the remaining freeze after the `#141` startup de-blocking changes.

### Boundaries and invariants to preserve:
- All Outlook object-model access remains on the main Outlook STA/UI thread. That includes selection reads, folder/view reads, conversation and table acquisition, inbox `Restrict`, row extraction, and any `MailItem` property reads backed by COM.
- Background-only helpers may accept only immutable snapshots or pure data. They must not accept live Outlook COM objects, COM-derived wrappers that still lazily dereference Outlook state, or UI controls.
- `ConfigureAwait(false)` is allowed only in helper/library segments that operate exclusively on snapshots, arrays, DTOs, or dataframes and do not touch Outlook or UI state afterward.
- Startup behavior remains functionally equivalent: globals still load, events still hook, existing persisted state remains readable, and the first-email workflow still produces the same model/UI result once work completes.
- Logging must remain compatible with the existing startup timing pattern established by issue `#139` so before/after evidence remains comparable.

### Dependencies or blocked work:
- The implementation depends on targeted instrumentation to prove whether `AppEvents` startup inbox processing or the QuickFiler first-selection path is the dominant remaining stall on the repro machine.
- Any change that reaches `ApplicationGlobals`, `AppOlObjects`, `StoresWrapper`, `StoreWrapper`, or `FolderMinimalWrapper` is contingent work and should occur only if the new timings show that the remaining delay still lives there after the primary-file changes.
- Manual Outlook verification on a representative mailbox/profile remains required because the final responsiveness outcome depends on real Outlook STA behavior that unit tests cannot fully simulate.

### Implementation strategy (what changes, not sequencing):
	
#### Files/modules to change:
- `TaskMaster/AppGlobals/AppEvents.cs` — instrument startup hook and inbox-processing phases, then batch or defer startup inbox work if it still creates a long uninterrupted UI-owned segment.
- `QuickFiler/Controllers/EfcHomeController.cs` — time the first-selection envelope from selection capture through model initialization completion and avoid repeated UI-thread re-entry while startup is still active.
- `QuickFiler/Controllers/EfcDataModel.cs` — separate synchronous conversation/dataframe loading from background-safe initialization and make the constructor/factory boundary explicit in logs and control flow.
- `QuickFiler/Helper Classes/ConversationResolver.cs` — snapshot conversation data before background transforms and reduce repeated dispatcher publication during resolver initialization.
- `UtilitiesCS/Extensions/DfDeedle.cs` — split Outlook-table acquisition and column extraction from dataframe conversion, filtering, and other pure transforms.
- `UtilitiesCS/OutlookObjects/Conversation/ConversationHelper.cs` — keep conversation and table acquisition on the UI thread, but time and isolate snapshot creation from downstream dataframe work.
- `UtilitiesCS/OutlookObjects/MailItem/MailItemHelper.cs` — make eager COM-backed materialization visible in timings and move only post-snapshot tokenization or projection work off-thread.
- `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.cs` — replace `Task.Run`-style COM wrappers with explicit STA acquisition plus snapshot-returning helpers, then run ETL over the snapshots off-thread.

#### Functions/classes/CLI commands impacted:
- `AppEvents.LoadAsync()`, `Hook()`, `ProcessNewInboxItemsAsync()`, and any per-mail startup processing helper it calls.
- `EfcHomeController` selection/change handling that currently reads `globals.Ol.App.ActiveExplorer().Selection` and kicks off `EfcDataModel.CreateAsync(...)`.
- `EfcDataModel` constructor and async factory paths that currently trigger synchronous dataframe/conversation loading.
- `ConversationResolver.LoadDfAsync(...)`, `BackgroundInitInfoItemsAsync(...)`, and the resolver UI publication path.
- `ConversationHelper` conversation/table/dataframe helpers.
- `DfDeedle.GetEmailDataInViewAsync(...)` and related helpers that currently mix Outlook explorer/table access with dataframe transforms.
- `MailItemHelper.FromMailItemAsync(...)` and `MaterializeTokenizationDependencies()`.
- `OlTableExtensions.GetTableInViewAsync(...)`, `GetTableAsync(...)`, `EtlAsync(...)`, and related row-extraction helpers.

#### Data flow and validation changes:
- Selection, conversation, inbox, and table access must produce immutable snapshots on the Outlook thread before any heavy filtering, ranking, dataframe conversion, or tokenization begins.
- Snapshot payloads may include copied `object[,]` table data, column maps, mailbox/item metadata DTOs, and other pure values needed for downstream processing.
- Validation for the refactor is thread-ownership-based rather than schema-based: code review, tests, and instrumentation must show that background stages no longer dereference Outlook COM state after snapshot capture.
- If startup inbox work remains expensive, the acceptable behavior change is to batch or defer that work so Outlook can repaint and accept input before the full startup mail-processing backlog completes.

#### Error handling and logging updates:
- Continue using `log4net` and the existing startup timing conventions from issue `#139`.
- Add timing around startup phases, first-selection start/end, conversation acquisition, table acquisition, table row extraction, dataframe conversion, tokenization dependency materialization, and final UI publish.
- Include enough context in each timing segment to correlate overlap between startup and first-selection work: thread ID, synchronization-context type, selected item count, and whether startup is still active.
- Preserve failure visibility. Refactoring must not hide COM exceptions, timeout behavior, or task failures behind fire-and-forget work.

#### Rollback/feature-flag considerations (if applicable):
- No runtime feature flag is required.
- Rollback is source-level: revert the snapshotting/defer/batching changes if they regress correctness or Outlook COM affinity.
- Because this bug touches startup and first-selection ordering, rollback validation must compare both responsiveness and behavioral parity for inbox processing and QuickFiler model initialization.

### Technical specifications (interfaces/contracts):
- Any new helper introduced by this fix must encode thread ownership in its contract: UI-thread helpers may touch Outlook COM and must return pure snapshots; background helpers must consume only pure snapshots and must not reacquire Outlook state.
- Async method names in the affected area must reflect actual asynchronous boundaries. A method that still performs all heavy COM-backed work synchronously before returning a completed task is not an acceptable end state for this bug.
- UI publication contracts should prefer one publish per meaningful stage rather than frequent fine-grained dispatcher churn that recreates responsiveness problems after the data work moves off-thread.

#### Inputs/outputs and formats:
- Input trigger: standard Outlook startup and the first email selection that occurs while startup work is still active.
- Input data: live Outlook profile state, selected mail items, conversation/table rows, and existing persisted TaskMaster state already used by startup and QuickFiler.
- Output behavior: startup and first-email interactions remain responsive; logs clearly separate startup timing from first-selection timing; no new external file, CLI, or config formats are introduced.
- Output artifacts for validation are timing log lines and existing MSTest/manual-verification evidence, not new persisted data formats.

#### Required configuration keys and defaults:
- No new configuration keys or defaults are required.
- Existing startup, QuickFiler, and persisted-state configuration must remain readable without migration.

#### Backward-compatibility expectations:
- No user-facing workflow, command surface, configuration schema, or persisted-data format change.
- The first-email interaction must continue to resolve the same conversation/message data and publish the same effective UI result after refactoring.
- Prior COM-safety fixes from issues `#124`, `#126`, `#128`, `#139`, and `#141` must remain intact.

#### Performance constraints (latency/throughput/memory):
- Primary requirement: Outlook must continue repainting and accepting input during startup and during the first email interaction while startup is still active.
- Instrumentation must show that long-running CPU/dataframe/tokenization work occurs in background-timed segments after snapshot capture, not as one long contiguous UI-thread-owned segment.
- Startup and first-click paths may still consume noticeable wall-clock time overall, but the implementation must eliminate the current extended visible lock-up and replace it with staged progress that returns control to the Outlook window between unavoidable COM-bound steps.
- Snapshotting must stay targeted; the fix must not duplicate large mailbox datasets solely for convenience if a smaller immutable snapshot is sufficient.

## Assumptions, Constraints, Dependencies
- Assumptions (environment, data, access):
- Outlook desktop on Windows 10/11 is running the TaskMaster VSTO add-in with a live profile and enough mailbox content to reproduce a click during the startup window.
- The research artifact for issue `#148` and the prior issues `#141` and `#139` are sufficient to define the current bug scope and the likely primary files.
- Existing startup timing infrastructure and `UiThread`/`IdleAsyncQueue` plumbing remain available for additional instrumentation.
- Constraints (budget, performance, compatibility):
- Outlook COM thread affinity is non-negotiable; the fix must not move object-model access to worker threads.
- The desired implementation is a targeted follow-up, not a broad startup-system rewrite.
- Compatibility with the existing .NET Framework Outlook add-in host and current QuickFiler behavior must be preserved.
- External dependencies (services, libraries, releases):
- Outlook desktop/VSTO runtime behavior and the repository’s current threading helpers are the only relevant runtime dependencies.
- No new third-party libraries or external services are required for this bug fix.

## Data / API / Config Impact
- User-facing or API changes:
- No new user-facing commands, controls, or configuration knobs.
- User-visible change is limited to improved Outlook responsiveness during startup and the first email interaction.
- Data or migration considerations:
- No schema or persisted-data migration is planned.
- Any new snapshot types introduced for background transforms are in-memory only.
- Logging/telemetry updates (if any):
- Expand the existing startup timing logs to include first-selection timings and explicit segment boundaries for conversation/table/mail-item snapshotting versus background transforms.
- Keep timing labels stable enough that future regressions can compare `#148` evidence back to the `#139`/`#141` baseline.
- Compatibility notes (CLI flags, config schemas, versioning):
- No CLI flags, no config-schema changes, and no versioning changes are required.

## Test Strategy
Seeded from issue:

- [x] Add targeted instrumentation for startup phases and email-selection/update paths so the remaining UI-bound segments can be measured separately.
- [x] Add or extend regression and unit coverage around startup coordination, initial selection handling, and shared data-loading helpers that can safely move off the UI thread.
- [x] Manually verify Outlook responsiveness during startup and first email clicks, confirming there is no perceivable latency and that the UI continues repainting while work completes.

- Regression tests to add or update:
- `TaskMaster.Test/AppGlobals/ApplicationGlobalsTests.cs` — update only if startup-state exposure or coordination hooks need coverage for the new instrumentation correlation points.
- `TaskMaster.Test/AppGlobals/AppOlObjectsTests.cs` — update only if observe-only store/startup components become in-scope after instrumentation.
- `TaskMaster.Test/AppGlobals/AppEventsTests.cs` — add this MSTest home if direct coverage is needed for startup inbox-processing batching/defer behavior, timing envelope emission, or phase-completion semantics.
- `QuickFiler.Test/Controllers/EfcHomeControllerTests.cs` — verify first-selection handling does not block on full resolver/model work before returning control to the caller-visible stage boundary.
- `QuickFiler.Test/Helper Classes/ConversationResolverTests.cs` — verify resolver initialization consumes snapshots, reduces repeated UI publication, and preserves fallback/cancellation behavior.
- `UtilitiesCS.Test/OutlookObjects/Conversation/ConversationHelper_ExtendedTests.cs` — verify conversation helpers keep COM acquisition isolated from background-safe transforms.
- `UtilitiesCS.Test/OutlookObjects/MailItem/MailItemHelperCoreTests.cs` — verify `FromMailItemAsync(...)` no longer disguises heavy synchronous work as async and that post-snapshot transforms stay pure.
- `UtilitiesCS.Test/Extensions/DfDeedle_Tests.cs` and `UtilitiesCS.Test/Extensions/DfDeedle_COM_Tests.cs` — verify dataframe conversion and COM-bound table acquisition remain separated.
- `UtilitiesCS.Test/OutlookObjects/Table/*` existing MSTest homes, if present, should be extended for snapshot/ETL boundaries in `OlTableExtensions`.
- Unit tests (pytest) for the fixed behavior and boundaries:
- Not applicable. This path is C#/.NET Framework Outlook add-in code; unit coverage must use MSTest with Moq and FluentAssertions where seams allow deterministic verification.
- Edge cases and negative scenarios (invalid inputs, missing data, boundary values):
- Startup still active when the first email is selected.
- Empty or multi-item selections, missing conversation/table data, and cancellation or timeout during conversation/table acquisition.
- Mail items that require heavy recipient, attachment, or body materialization.
- Profiles where startup inbox processing has many unprocessed items and risks monopolizing the UI thread.
- Error handling and logging verification:
- Verify timing logs emit separate segments for startup and first-selection work and include enough correlation data to attribute overlap.
- Verify COM/time-out failures continue to surface through existing error paths and are not swallowed by background helpers.
- Verify any batching or defer logic logs completion in the correct order and does not report startup work complete before the batch truly ends.
- Coverage impact and targets for changed lines/modules:
- Maintain or improve coverage on all touched C# modules.
- Any new helper that separates COM snapshotting from background transforms should receive direct MSTest coverage because that seam defines the bug fix boundary.
- Toolchain commands to run (format → lint → type-check → test):
- `csharpier .`
- `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`
- `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`
- `vstest.console.exe <test-assembly-paths> /EnableCodeCoverage`
- Manual validation steps (if required):
- Launch Outlook with the TaskMaster add-in enabled on a representative live profile.
- Capture startup timing logs, click one or more emails before startup completes, and confirm the new logs separate startup work from first-selection work.
- Observe the Outlook window during both startup and the first selection and confirm repaint/input continue while background-safe work completes.
- Compare resulting timings against the prior `#139`/`#141` baseline to confirm the dominant remaining stall moved out of the UI-thread path or was reduced to short unavoidable COM segments.


## Acceptance Criteria
- [x] Startup and first-selection instrumentation emit distinct timing segments for `AppEvents`, selection capture, conversation/table acquisition, dataframe conversion, mail-item materialization, and final UI publication, with enough context to correlate overlap during the repro path.
- [x] The implementation preserves Outlook STA/UI-thread ownership for COM-affine work in `AppEvents`, `EfcHomeController`, `ConversationHelper`, `MailItemHelper`, and `OlTableExtensions`; background stages consume only immutable snapshots or other pure data.
- [x] `TaskMaster/AppGlobals/AppEvents.cs`, `QuickFiler/Controllers/EfcHomeController.cs`, `QuickFiler/Controllers/EfcDataModel.cs`, `QuickFiler/Helper Classes/ConversationResolver.cs`, `UtilitiesCS/Extensions/DfDeedle.cs`, `UtilitiesCS/OutlookObjects/Conversation/ConversationHelper.cs`, `UtilitiesCS/OutlookObjects/MailItem/MailItemHelper.cs`, and `UtilitiesCS/OutlookObjects/Table/OlTableExtensions.cs` are treated as the primary follow-up scope unless instrumentation proves a contingent file still owns the dominant stall.
- [ ] During the repro path, Outlook continues repainting and accepting input while startup work is active and while the first email interaction completes; the prior extended visible lock-up is no longer observed.
- [x] Startup inbox processing is either batched/deferred or instrumented and refactored enough that it no longer monopolizes the UI thread in one long uninterrupted startup segment.
- [x] The first-email interaction no longer performs conversation acquisition, table extraction, dataframe construction, tokenization dependency materialization, and UI publication as one contiguous UI-thread-owned block; only the unavoidable COM snapshot and final publish remain UI-affine.
- [x] MSTest regression coverage is added or updated in the identified test homes, including a direct home for `AppEvents` if that path is changed, and the affected tests pass.
- [x] No new configuration schema, persisted-data format, feature flag, or user-facing command/control is introduced outside the defined scope.

## Risks & Mitigations
- Technical or operational risks:
- Instrumentation may show the remaining stall still sits in observe-only store/folder startup code, which would expand the implementation scope beyond the preferred primary files.
- Replacing `Task.Run`-wrapped COM calls with explicit snapshot boundaries can expose hidden assumptions in callers that currently rely on lazy COM-backed objects.
- Over-eager UI publication from `ConversationResolver` or controller code can continue to degrade responsiveness even after heavy transforms move off-thread.
- Manual Outlook validation remains necessary because unit tests cannot fully reproduce live Outlook STA contention and repaint behavior.
- Mitigations and rollbacks:
- Use instrumentation first to confirm the dominant remaining hotspot before touching contingent files.
- Keep the refactor narrow: preserve COM-affine acquisition, introduce small snapshot DTO/array seams, and move only proven background-safe transforms.
- Add MSTest coverage around every new seam that separates COM work from background work so rollback decisions can focus on specific boundaries.
- If responsiveness or correctness regresses, revert the snapshot/defer changes and compare the timing logs to identify the last safe boundary.

## Rollout & Follow-up
- Release/rollout steps:
- Implement the targeted fix in the primary-file scope, then expand only if instrumentation proves a contingent file remains dominant.
- Run the C# format/build/type-check/test loop and capture updated timing evidence before release.
- Validate on the live Outlook repro profile before broader distribution.
- Post-fix monitoring or clean-up tasks:
- Retain or trim only the instrumentation needed for future regression diagnosis after the dominant stall is confirmed resolved.
- If timings still show a major startup stall in `ApplicationGlobals`, `AppOlObjects`, `StoresWrapper`, `StoreWrapper`, or `FolderMinimalWrapper`, open the next follow-up with the new evidence rather than widening this bug without proof.
- Review whether any remaining dispatcher churn or startup inbox backlog needs an additional follow-up once the main lock-up is removed.
- Links: issue, PRs, related docs
- Issue: `docs/features/active/2026-05-07-outlook-startup-ui-lockup-followup-148/issue.md`
- Research: `artifacts/research/20260507-outlook-startup-ui-lockup-followup-148-research.md`
- Related prior issue: `docs/features/active/2026-05-05-outlook-startup-ui-thread-deblock-141/issue.md`
- Related prior issue: `docs/features/active/2026-04-21-outlook-startup-store-rewire-ui-lock-instrumentation-139/issue.md`
