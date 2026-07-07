# 2026-07-07-folder-settings-store-model-null (Spec)

- **Issue:** #262
- **Parent (optional):** Epic #260 (store-lockup-resilience), F2, wave 0, `depends_on: []`
- **Owner:** drmoisan
- **Last Updated:** 2026-07-07T17-41
- **Status:** Draft
- **Version:** 0.2
- **Work Mode:** full-bug

## Context
Opening "TaskMaster -> Settings -> Folder Settings" shows "Store settings are not available yet.
Please try again after startup completes." even though startup completed long ago. The store
settings model (`Globals.Ol.StoresWrapper`) is null for the entire session, so the readiness
guard correctly refuses to open the dialog. The dialog copy implies a timing or startup-notification
problem, but the model is null permanently for the session; retrying after startup can never succeed.

Environment:
- OS/version: Windows, Outlook desktop (VSTO add-in)
- Assembly: UtilitiesCS / TaskMaster
- Command/flags used: Ribbon action -> RibbonController.FolderStoresSettings() -> StoreWrapperController.Launch()
- Data source or fixture: Globals.Ol.StoresWrapper (populated by AppOlObjects.LoadStoresAsync during startup)

Impact / Severity:
- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low


## Repro & Evidence
Steps to Reproduce:
1. Start Outlook with the TaskMaster add-in and allow startup to fully complete.
2. Click TaskMaster -> Settings -> Folder Settings.
3. Observe the "Store settings are not available yet" message despite startup being finished.

Expected:
After startup completes, Folder Settings opens with a populated store model. If the persisted
store configuration is missing or invalid, the add-in rebuilds the model from the live Outlook
stores rather than leaving it null, and any genuine failure is surfaced clearly.

Actual:
`StoreWrapperController.EvaluateLaunchReadiness()` returns `ModelUnavailable`/`StoresUnavailable`
because `Globals.Ol.StoresWrapper` (or its `.Stores`) is null, so `Launch()` shows the
"not available yet" dialog and returns. The message implies a timing/notification problem, but
the model is null permanently for the session.

Logs / Screenshots:
- [ ] Attached minimal logs or screenshot
- Snippet: distinguishing log lines to look for — `"StoresWrapper config not found."`
  (`AppOlObjects.cs:263`), `"Loader for StoresWrapper is null"` (`IntelligenceConfig.cs`), and
  whether `"Finished loading globals"` (`ThisAddIn.cs`) appears.


## Scope & Non-Goals
- In scope: `TaskMaster/AppGlobals/AppOlObjects.cs` store-load pipeline only (extracted into a new
  partial `AppOlObjects.StoreLoading.cs` for file-size compliance); the fresh-build fallback on the
  two recoverable null paths; a bounded failure surface for the genuine-failure path; MSTest
  regression coverage for the three null paths.
- Out of scope / non-goals:
  - Changing the `StoreWrapperController` "not available yet" dialog copy for the genuine-failure
    case (imprecise but not required by any AC; documented follow-up only).
  - Any modification to `StoresWrapper.cs` (owned by F1 #261), `IntelligenceConfig.cs`, or
    `StoreWrapperController.cs`.
  - Adding a second user-facing notification path for the genuine-failure case.
- Explicitly excluded systems, integrations, or datasets: live Outlook/COM dependencies in tests;
  temporary files in tests; any startup-complete event/flag notification mechanism (deliberately
  rejected by #240 in favor of live-state inspection, which is not the defect).

## Root Cause Analysis
Root cause is NOT a missing startup-complete notification. Issue #240 deliberately chose direct
live-state inspection over an event/flag, and the guard reads live state correctly on every click.
The guard is not the defect. The defect is upstream in `TaskMaster/AppGlobals/AppOlObjects.cs`
`LoadStoresAsync` (`:251-265`), which leaves `StoresWrapper` null on recoverable paths. Three null
paths are confirmed:

1. **Path 1 — config missing.** `_globals.IntelRes.Config.TryGetValue("StoresWrapper", out var
   config)` returns `false`; the `else` branch logs a bare `logger.Error("StoresWrapper config not
   found.")` and returns. `StoresWrapper` (auto-property at `:244`, default `null`) is never assigned
   and stays null for the session. This input can also originate one level up:
   `IntelligenceConfig.ReadConfigurationAsync` (`:140`) drops the `"StoresWrapper"` key from `Config`
   entirely when its loader deserializes to null (already logged distinctly and resource-keyed at
   `:118-119`). From `LoadStoresAsync`'s perspective this is indistinguishable from Path 1, and the
   same fallback covers it — no `IntelligenceConfig.cs` change is required.
2. **Path 2 — null deserialize.** `TryGetValue` returns `true`, but
   `SmartSerializable.Deserialize<StoresWrapper, SmartSerializableLoader>(config)` returns `null`.
   There is no null-check on the assignment. `AwaitStoreRewireAsync` (`:246-249`) explicitly tolerates
   a null argument, so no exception is raised and `StoresWrapper` remains null.
3. **Path 3 — exception during load.** The `Deserialize<T,U>(SmartSerializable<U> loader)` overload
   (`SmartSerializableBase.cs:166-187`) can throw `ArgumentNullException` (via `ThrowIfNull` guards)
   or throw on malformed JSON. `LoadStoresAsync` has no try/catch, so the exception propagates to the
   `IdleAsyncQueue.AddEntry` continuation (`IdleAsyncQueue.cs:60-95`), whose generic
   `catch (Exception ex)` only logs `"Failed to execute IdleAsyncQueue.actionAsync"` plus
   `ex.Message` — no attribution to `StoresWrapper` or store settings. This is the bare, unattributed
   failure surface AC3 targets.

`_globals.Ol` is safe to dereference from inside `LoadStoresAsync` at runtime:
`ApplicationGlobals.LoadBasicMethod` (`:99-117`) constructs `_olObjects` and wires `Ol => _olObjects`
(`:420`) before `LoadAsync` ever calls `LoadOlObjectsPhaseAsync()`, so a fresh-build call from inside
`LoadStoresAsync` does not depend on any not-yet-wired collaborator.


## Proposed Fix

### Design summary (what changes where):
Restructure `AppOlObjects.LoadStoresAsync` so that both recoverable null paths (config missing, null
deserialize) fall back to building a fresh model from the live Outlook stores, and wrap the method in
a bounded try/catch that surfaces a genuine, unrecoverable failure with `StoresWrapper`-specific
context. Introduce a new `protected internal virtual` seam `BuildFreshStoresWrapper()`. Extract the
store-loading concern into a new partial file for file-size compliance. No controller change.

### Boundaries and invariants to preserve:
- `StoreWrapperController.EvaluateLaunchReadiness()`/`Launch()` and the "not available yet" dialog
  are unmodified; they already handle a null model correctly.
- `StoresWrapper.Init()`/`CreateAsync()` are reused unchanged.
- On the valid-config path the existing deserialize-then-`AwaitStoreRewireAsync` behavior is
  preserved exactly.
- Both `AppOlObjects.cs` and the new partial end at 500 lines or fewer.

### Dependencies or blocked work:
- F1 (#261) edits `StoresWrapper.cs` independently; no line-level overlap with F2. F3 depends on F2
  (shared `AppOlObjects.cs`). Both F1 and F2 are wave 0, `depends_on: []`.

### Implementation strategy (what changes, not sequencing):

#### Files/modules to change:
- `TaskMaster/AppGlobals/AppOlObjects.cs` — remove the `StoresWrapper` property,
  `AwaitStoreRewireAsync`, `LoadStoresAsync`, and `LoadAsync()` (moved to the new partial) to bring
  the file under the 500-line cap (currently 525 lines).
- `TaskMaster/AppGlobals/AppOlObjects.StoreLoading.cs` (new) — `public partial class AppOlObjects` in
  `namespace TaskMaster` holding `LoadAsync()`, the `StoresWrapper` property, `AwaitStoreRewireAsync`,
  the new `BuildFreshStoresWrapper()` seam, and the restructured `LoadStoresAsync()`. Follows the
  documented `AppOlObjects.JunkFolders.cs` precedent (Issue #207, AC8 file-size relief).

#### Functions/classes/CLI commands impacted:
- New seam:
  ```csharp
  protected internal virtual StoresWrapper BuildFreshStoresWrapper() =>
      new StoresWrapper(_globals).Init();
  ```
  Mirrors the existing `AwaitStoreRewireAsync` convention exactly. Reuses the synchronous live-stores
  build path: `Init()` (`StoresWrapper.cs:35-49`) materializes `GetFilteredStores()` (reads
  `Globals.Ol.NamespaceMAPI.Stores`) and assigns `Stores`. No `AwaitStoreRewireAsync` call is needed
  after `Init()` — it is a complete synchronous build. `new StoresWrapper(_globals)` (not the
  parameterless constructor) is required so `GetFilteredStores()` can read
  `Globals.Ol.NamespaceMAPI.Stores`. `Init()` is preferred over `CreateAsync` only because
  `LoadStoresAsync` is already `async` (avoids an unnecessary `Task.FromResult` wrap); the two are
  equivalent in effect.
- Restructured `LoadStoresAsync`:
  ```csharp
  internal async Task LoadStoresAsync()
  {
      try
      {
          if (_globals.IntelRes.Config.TryGetValue("StoresWrapper", out var config))
          {
              var deserialized =
                  SmartSerializable.Deserialize<StoresWrapper, SmartSerializableLoader>(config);
              if (deserialized is not null)
              {
                  StoresWrapper = deserialized;
                  await AwaitStoreRewireAsync(StoresWrapper);
                  return;
              }
              logger.Warn("StoresWrapper config deserialized to null; rebuilding from live stores.");
          }
          else
          {
              logger.Warn("StoresWrapper config not found; rebuilding from live stores.");
          }

          // Fresh build has no persisted disabled-store state to restore (see F1 note below).
          StoresWrapper = BuildFreshStoresWrapper();
      }
      catch (Exception e)
      {
          logger.Error(
              $"Failed to load StoresWrapper; store settings will remain unavailable until this is resolved. {e.Message}",
              e
          );
      }
  }
  ```

#### Data flow and validation changes:
- Both recoverable branches now fall through to `StoresWrapper = BuildFreshStoresWrapper()`, so
  `StoresWrapper` is never left null on a recoverable path (AC1, AC2). The fresh-build path bypasses
  `AwaitStoreRewireAsync` (only the valid-deserialize path awaits rewire).

#### Error handling and logging updates:
- The two recoverable branches log `logger.Warn` (not `Error`): these are now handled conditions and
  the model still ends up populated, matching the repo's existing use of `Warn` for recovered/
  non-fatal conditions (`AppOlObjects.cs:411`).
- The bounded `try/catch (Exception)` at the method boundary elevates a genuine failure to
  `logger.Error` with the exception object attached (preserving the stack trace, unlike the current
  bare-string log) and explicit `StoresWrapper`/user-consequence context. It is intentional and
  narrowly scoped: `LoadStoresAsync` is one phase inside
  `ApplicationGlobals.LoadParallelAsync`/`LoadSequentialAsync`; an unhandled exception here would
  abort the awaited chain and prevent the ToDo, AutoFile, Engines, and Events phases from running.
  There is **no retry** inside the catch — if `BuildFreshStoresWrapper()` itself throws, the same
  catch reports it once; no second fallback attempt. On the genuine-failure path `StoresWrapper`
  remains null, the readiness guard still reports `ModelUnavailable`, and the existing dialog remains
  the single user-facing surface (AC3).

#### Rollback/feature-flag considerations (if applicable):
- None. The change is behavior-preserving on the valid-config path and additive on the recoverable/
  failure paths. No feature flag is warranted.

### Technical specifications (interfaces/contracts):

#### Inputs/outputs and formats:
- Input: `_globals.IntelRes.Config` (a `ConcurrentDictionary<string, SmartSerializableLoader>`) keyed
  by `"StoresWrapper"`. Output: `StoresWrapper` property set to a populated instance on all
  recoverable paths; left null only on the genuine-failure path.

#### Required configuration keys and defaults:
- None added. The `"StoresWrapper"` config key remains optional; its absence now triggers a fresh
  build rather than a permanent null.

#### Backward-compatibility expectations:
- The `BuildFreshStoresWrapper()` seam is `protected internal virtual` (non-public surface). No
  public API changes. The valid-config path is byte-for-byte equivalent in behavior.

#### Performance constraints (latency/throughput/memory):
- The fresh build runs synchronously via `Init()` only on the recoverable paths, which are the
  paths that previously produced a permanently-null model; there is no added cost on the valid path.

## Assumptions, Constraints, Dependencies
- Assumptions (environment, data, access): at the moment `LoadStoresAsync` runs, `_globals.Ol`
  already resolves to the constructed `AppOlObjects` instance (verified via `LoadBasicMethod` wiring);
  the live `NamespaceMAPI.Stores` chain is available for the fresh build.
- Constraints (budget, performance, compatibility): 1 logical production file touched, split into 2
  for the 500-line cap; net48 constraints honored; no new production types.
- External dependencies (services, libraries, releases): MSTest, Moq, FluentAssertions for tests;
  no new dependencies.

## Data / API / Config Impact
- User-facing or API changes: none beyond the corrected behavior (Folder Settings opens on
  recoverable paths). The "not available yet" dialog is unchanged.
- Data or migration considerations: none.
- Logging/telemetry updates (if any): two recoverable branches log at `Warn`; genuine failure logs at
  `Error` with exception and `StoresWrapper`-specific context (replacing the bare-string `Error`).
- Compatibility notes (CLI flags, config schemas, versioning): none.

## Test Strategy
Seeded from issue:

- [ ] Unit coverage areas: `LoadStoresAsync` builds a fresh `StoresWrapper` from live stores when
      config is missing or deserializes to null; failure is surfaced (not a silent `logger.Error`).
- [ ] Integration scenario to retest: open Folder Settings after startup with (a) missing config,
      (b) null-deserialized config, (c) valid config — dialog opens populated in all recoverable cases.
- [ ] Manual verification notes: confirm no "not available yet" after startup; confirm the model
      is populated and Folder Settings opens.

Test-first (bugfix workflow) — deterministic failing tests must exist before the fix and pass after.
All tests use the existing `StubApplicationGlobals`/`StubIntelligenceConfig`/`TestableAppOlObjects`
seams (MSTest + Moq + FluentAssertions); no live Outlook, no temp files.

- Regression tests to add or update:
  1. **Invert the mis-specified existing test.**
     `TaskMaster.Test/AppGlobals/AppOlObjectsCoverageTests.cs`
     (`LoadStoresAsync_LeavesStoresWrapperNullWhenConfigMissing`, `:75-87`) currently asserts
     `sut.StoresWrapper.Should().BeNull()` — the bug's signature. Replace the assertion (not merely
     rename) to assert the fallback: `StoresWrapper` is non-null and `BuildFreshStoresWrapper()` was
     invoked (via a `TestableAppOlObjects` override that records invocation and returns a sentinel
     instance). Justified under "treat existing unit tests as part of the spec."
  2. **Path 1 (config missing):** empty config dictionary; override `BuildFreshStoresWrapper()` to
     return a sentinel; assert `sut.StoresWrapper` is that sentinel and the override was invoked
     exactly once.
  3. **Path 2 (null deserialize):** config contains the key but deserialize returns
     `(StoresWrapper)null`; assert the same fallback outcome as Path 1 and assert
     `AwaitStoreRewireAsync` was **not** invoked.
  4. **Path 3 (exception during load):** deserialize throws (`ArgumentNullException` or
     `InvalidOperationException`, matching the real throw site); assert `LoadStoresAsync()` completes
     without throwing and `sut.StoresWrapper` remains null (no fallback after a mid-deserialize
     exception). If an injectable log seam already exists in the test class, also assert an
     `Error`-level entry; otherwise "does not throw" plus "stays null" is sufficient, and no new
     log-capture seam should be introduced solely for this assertion.
- Unit tests for the fixed behavior and boundaries: the four items above.
- Edge cases and negative scenarios: config-missing, null-deserialize, and exception-during-load are
  the three negative paths; the valid-config path is the positive path.
- Error handling and logging verification: Path 3 verifies the exception is absorbed at the method
  boundary; log-level assertions per item 4 above where a seam exists.
- Coverage impact and targets for changed lines/modules: new/changed lines in the store-loading
  partial meet the repo coverage targets; existing valid-config tests
  (`LoadAsync_AssignsStoresWrapperFromConfigAndCompletes`,
  `LoadStoresAsync_DoesNotCompleteBeforeStoreRewireTaskFinishes`) require no change and confirm no
  regression on the unchanged branch.
- Toolchain commands to run (format -> lint -> type-check -> test): csharpier -> msbuild analyzers ->
  msbuild nullable/TreatWarningsAsErrors -> vstest.console.exe with `/EnableCodeCoverage`.
- Manual validation steps: open Folder Settings after startup on missing-config, null-deserialize,
  and valid-config sessions; confirm the dialog opens populated in all recoverable cases and no "not
  available yet" appears.


## Acceptance Criteria
- [ ] AC1: When the persisted `StoresWrapper` config is missing, `LoadStoresAsync` builds a fresh
      model from the live Outlook stores (via `BuildFreshStoresWrapper()` ->
      `new StoresWrapper(_globals).Init()`) instead of leaving `StoresWrapper` null. Verified by the
      Path 1 regression test.
- [ ] AC2: When the persisted config deserializes to null, the same fresh-build fallback applies
      rather than being silently tolerated. Verified by the Path 2 regression test, which also asserts
      `AwaitStoreRewireAsync` is not invoked on the fresh-build path.
- [ ] AC3: A genuine, unrecoverable load failure is surfaced — logged at `Error` with the exception
      attached and `StoresWrapper`-specific context — not swallowed as a bare `logger.Error` string
      and not escaping as the generic `IdleAsyncQueue` catch. No retry and no new dialog are added; the
      existing readiness-guard dialog remains the only user-facing surface. Verified by the Path 3
      regression test (completes without throwing; `StoresWrapper` stays null).
- [ ] AC4: After startup completes on a recoverable path, `StoreWrapperController.Launch()` opens the
      dialog with a populated model and no longer shows "not available yet". `StoreWrapperController.cs`
      is unmodified; the guard reports `Ready` because `StoresWrapper` is non-null with populated
      `Stores`.
- [ ] AC5: A deterministic MSTest regression suite reproduces the null-model paths (fails before the
      fix, passes after) using the existing `StubApplicationGlobals`/`StubIntelligenceConfig`/
      `TestableAppOlObjects` seams and Moq; no live Outlook, no temp files. Includes inverting the
      previously mis-specified `LoadStoresAsync_LeavesStoresWrapperNullWhenConfigMissing`.
- [ ] AC6: `AppOlObjects.cs` is brought to 500 lines or fewer by extracting the store-loading concern
      into the new partial `AppOlObjects.StoreLoading.cs` (precedent: `AppOlObjects.JunkFolders.cs`);
      both files end at 500 lines or fewer.
- [ ] AC7: Full C# toolchain passes in order (csharpier -> analyzers -> nullable/TreatWarningsAsErrors
      -> MSTest with coverage); new/changed lines meet coverage targets; no repo-wide regression; net48
      constraints honored.

## Risks & Mitigations
- Technical or operational risks:
  - Cross-feature (F1 #261): F1 adds a persisted disabled-store list to `StoresWrapper.cs`. If the
    fresh-build fallback fires in a session where the persisted config was lost or corrupted (Path
    1/2) and a store had previously been disabled-for-future-sessions, that disablement is not present
    in the rebuilt model (the store reappears enabled until disabled again). This is inherent to "the
    persisted config is unavailable," not a defect introduced by F2 — there is no other source from
    which the previously-persisted disabled list could be recovered once the config blob is confirmed
    missing or null.
  - `BuildFreshStoresWrapper()` could itself throw on a COM error enumerating `Stores`; the bounded
    catch reports it once with no retry.
- Mitigations and rollbacks:
  - Record the F1 interaction in a one-line code comment at the `BuildFreshStoresWrapper` call site so
    it is not later misattributed as a regression in F1 or F5. F2 does not modify `StoresWrapper.cs`.
  - No feature flag needed; the change is behavior-preserving on the valid-config path.

## Rollout & Follow-up
- Release/rollout steps: standard build and add-in deployment; no migration.
- Post-fix monitoring or clean-up tasks: watch for the new `Warn`/`Error` log lines to distinguish
  recoverable rebuilds from genuine failures.
- Follow-up (not required by any AC): consider revising the `StoreWrapperController` "not available
  yet" dialog copy for the genuine-failure case, whose current wording implies a timing issue.
- Links: issue #262; epic #260 (`docs/features/epics/store-lockup-resilience/epic-plan.md`); prior
  art #240 (`docs/features/active/2026-07-06-store-wrapper-launch-npe-240/`).
