# Implementation-Strategy Research: folder-settings-store-model-null (Issue #262)

- Timestamp: 2026-07-07
- Feature: `docs/features/active/2026-07-07-folder-settings-store-model-null-262`
- Epic: #260 (store-lockup-resilience), wave 0, `depends_on: []`
- Scope: research only; no production code modified
- Diagnosis status: confirmed per epic manifest and #240 research (do not relitigate). This
  document deepens the fix design in `AppOlObjects.LoadStoresAsync` only.

## 1. Current-State Analysis (verified)

### 1.1 The three null paths, verified at the cited lines

`TaskMaster/AppGlobals/AppOlObjects.cs:251-265`:

```csharp
internal async Task LoadStoresAsync()
{
    if (_globals.IntelRes.Config.TryGetValue("StoresWrapper", out var config))
    {
        StoresWrapper = SmartSerializable.Deserialize<StoresWrapper, SmartSerializableLoader>(config);
        await AwaitStoreRewireAsync(StoresWrapper);
    }
    else
    {
        logger.Error("StoresWrapper config not found.");
    }
}
```

- **Path 1 — config missing.** `TryGetValue` returns `false`; the `else` branch logs a bare
  `logger.Error` and returns. `StoresWrapper` (a plain auto-property, `AppOlObjects.cs:244`,
  default `null`) is never touched, so it stays `null` for the session.
- **Path 2 — null deserialize.** `TryGetValue` returns `true`, but
  `SmartSerializable.Deserialize<StoresWrapper, SmartSerializableLoader>(config)` returns `null`
  (no null-check on the assignment). `AwaitStoreRewireAsync` (`:246-249`) explicitly tolerates a
  null argument (`storesWrapper is null ? Task.CompletedTask : ...`), so no exception is raised
  and `StoresWrapper` remains `null`.
- **Path 3 — exception during load.** Verified in
  `UtilitiesCS/ReusableTypeClasses/NewSmartSerializable/SmartSerializableBase.cs:166-187`, the
  `Deserialize<T,U>(SmartSerializable<U> loader)` overload actually invoked here
  (`Deserialize<StoresWrapper, SmartSerializableLoader>`) calls
  `loader.ThrowIfNull().Config.ThrowIfNull().Disk.ThrowIfNull()` and re-throws
  `ArgumentNullException` after logging, and `DeserializeJson<T>` can itself throw on malformed
  JSON (uncaught in this overload). `LoadStoresAsync` has no try/catch, so such an exception
  propagates out of `LoadStoresAsync()` → `LoadAsync()` → (production)
  `ApplicationGlobals.LoadOlObjectsPhaseAsync()`. Verified sink:
  `UtilitiesCS/Threading/IdleAsyncQueue.cs:60-95` — the `IdleAsyncQueue.AddEntry` continuation
  that runs `await _globals.LoadAsync(false)` (`TaskMaster/ThisAddIn.cs:59-69`) is wrapped in a
  generic `try/catch (Exception ex)` that only logs `"Failed to execute
  IdleAsyncQueue.actionAsync"` plus `ex.Message` — no attribution to "StoresWrapper" or "store
  settings." This is the bare, unattributed failure surface AC3 targets. It also means the two
  statements after the awaited load in `ThisAddIn.cs` (`logger.Debug("Finished loading
  globals")`, `_startupPostLoadReached = true`) never execute on this path — a secondary,
  out-of-scope symptom not required for AC1-AC4.

### 1.2 Why the "config missing" input can also originate one level up

`UtilitiesCS/EmailIntelligence/IntelligenceConfig.cs:97-141` (`ReadConfigurationAsync`) builds
the `Config` dictionary that `AppOlObjects.LoadStoresAsync` reads. If the **loader wrapper**
(`SmartSerializableLoader`, not the eventual `StoresWrapper`) for the `"StoresWrapper"` resource
key deserializes to `null`, line 140's `.Where(kvp => kvp.Value is not null)` drops that key from
`Config` entirely — already logged distinctly at `:118-119`
(`logger.Error($"... Loader for {kvp.Key} is null")`, resource-keyed and already actionable).
From `AppOlObjects.LoadStoresAsync`'s perspective this is indistinguishable from Path 1
(`TryGetValue` returns `false`). **No change to `IntelligenceConfig.cs` is needed for F2** — the
config-missing fallback in `AppOlObjects` already covers this upstream cause; `IntelligenceConfig`
already logs an actionable, resource-keyed error for its own part of the failure.

### 1.3 The existing fresh-build path is already implemented and already unit-tested

`UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs`:

- `Init()` (`:35-49`, `public virtual`): synchronously materializes `GetFilteredStores()`
  (`:129-134`, reads `Globals.Ol.NamespaceMAPI.Stores`) and assigns `Stores` from it. No async
  rewire step is needed afterward — `Init()` is a complete, synchronous build.
- `CreateAsync(IApplicationGlobals globals, CancellationToken cancel)` (`:51-58`): the existing
  public factory, `Task.FromResult(new StoresWrapper(globals).Init())`. **Already covered** by
  `UtilitiesCS.Test/OutlookObjects/Store/StoresWrapperTests.cs` (`CreateAsync_WhenInputsValid_
  ReturnsInitializedStoresWrapper`, line ~47), which proves the COM-mocking pattern needed: a
  `Mock<Stores>` exposing `IEnumerable.GetEnumerator()`, a `Mock<NameSpace>` whose `Stores`
  returns it, a `Mock<IOlObjects>` whose `NamespaceMAPI` returns that, and a
  `Mock<IApplicationGlobals>` whose `Ol` returns that (`CreateGlobalsWithStores`, line 420-439).
  This is the proof that a "build fresh from live stores" call is fully unit-testable today
  without live Outlook.
- The parameterless constructor (`:23-27`) leaves `Globals` `null`; the fresh-build call **must**
  use `new StoresWrapper(_globals)` (or `CreateAsync(_globals, ...)`), not `new StoresWrapper()`,
  or `GetFilteredStores()`'s `Globals.Ol...` read throws `NullReferenceException`.

### 1.4 Why `_globals.Ol` is safe to dereference from inside `LoadStoresAsync` at runtime

`TaskMaster/AppGlobals/ApplicationGlobals.cs:99-117` (`LoadBasicMethod`) constructs
`_olObjects = new AppOlObjects(_outlookApp, this)` and assigns `Ol => _olObjects` (`:420`)
**before** `LoadAsync` ever calls `LoadOlObjectsPhaseAsync() => _olObjects.LoadAsync()` (`:394`,
invoked from `:125` parallel or `:183` sequential). So at the moment `LoadStoresAsync` runs in
production, `_globals.Ol` already resolves to the same `AppOlObjects` instance (`this`) whose
`NamespaceMAPI` property is directly available. A fresh-build call from inside `LoadStoresAsync`
therefore does not depend on any not-yet-wired collaborator.

### 1.5 Existing test doubles and the one test that encodes the bug as "correct"

Two test files carry `AppOlObjects` seams:

- `TaskMaster.Test/AppGlobals/AppOlObjectsCoverageTests.cs` — `StubApplicationGlobals`
  (implements `IApplicationGlobals` directly, `Ol` throws `NotSupportedException`) and
  `StubIntelligenceConfig : IntelligenceConfig` (sets `Config` via the protected setter from a
  derived class, since `IntelligenceConfig.Config` is `public virtual { get; protected set; }`).
  **`LoadStoresAsync_LeavesStoresWrapperNullWhenConfigMissing` (line 75-87) currently asserts
  `sut.StoresWrapper.Should().BeNull()` after a config-missing run — this test encodes the bug
  as intended behavior and must be corrected as part of the bugfix (see §4).**
- `TaskMaster.Test/AppGlobals/AppOlObjectsTests.cs` — a second, near-identical
  `StubApplicationGlobals`/`StubIntelligenceConfig`/`TestableAppOlObjects` triad, plus
  `LoadStoresAsync_DoesNotCompleteBeforeStoreRewireTaskFinishes` (line 181-235), which exercises
  the **valid-config** path (deserialize returns a non-null `StoresWrapper`, then awaits rewire).
  This test is orthogonal to F2 and needs no change.
- Both files override `AppOlObjects.AwaitStoreRewireAsync` (`protected internal virtual`,
  `AppOlObjects.cs:246-249`) via a `TestableAppOlObjects : AppOlObjects` subclass — this is the
  established seam pattern for controlling the store-load pipeline without live Outlook, and the
  natural template for the new `BuildFreshStoresWrapper` seam (§2).

### 1.6 File-size constraint already at the limit

`TaskMaster/AppGlobals/AppOlObjects.cs` is **525 lines** — already over the repo's 500-line cap
(`.claude/rules/general-code-change.md`, "File Size Limit"). The file already carries a
documented precedent for splitting under this exact pressure:
`TaskMaster/AppGlobals/AppOlObjects.JunkFolders.cs:12-17` states it was "extracted from
`AppOlObjects.cs` to bring that file under the 500-line cap" for "Issue #207, AC8 file-size
relief," as a behavior-preserving move into a `public partial class AppOlObjects` file in the
same `namespace TaskMaster`. Any addition to `LoadStoresAsync` (a fallback branch, a seam method,
a try/catch) makes the size violation worse unless the store-loading code is extracted the same
way. This is addressed in §5.

## 2. Design: fresh-build fallback (AC1, AC2)

### 2.1 Recommended shape

Add one new `protected internal virtual` seam, following the exact convention of
`AwaitStoreRewireAsync`, and restructure `LoadStoresAsync` around it:

```csharp
protected internal virtual StoresWrapper BuildFreshStoresWrapper() =>
    new StoresWrapper(_globals).Init();

internal async Task LoadStoresAsync()
{
    try
    {
        if (_globals.IntelRes.Config.TryGetValue("StoresWrapper", out var config))
        {
            var deserialized = SmartSerializable.Deserialize<StoresWrapper, SmartSerializableLoader>(config);
            if (deserialized is not null)
            {
                StoresWrapper = deserialized;
                await AwaitStoreRewireAsync(StoresWrapper);
                return;
            }
            logger.Warn(
                "StoresWrapper config deserialized to null; rebuilding from live stores."
            );
        }
        else
        {
            logger.Warn("StoresWrapper config not found; rebuilding from live stores.");
        }

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

Rationale for each element:

- **Seam, not inline `new StoresWrapper(_globals).Init()`.** Mirrors `AwaitStoreRewireAsync`
  exactly (`protected internal virtual`, same class, same override pattern already proven by
  `TestableAppOlObjects` in two existing test files). Lets an `AppOlObjects`-level test assert
  "the fallback path was taken and its result was assigned" without re-building the full
  `Mock<Stores>`/`Mock<NameSpace>`/`Mock<IOlObjects>` COM chain — that chain is already the
  responsibility of, and already covered by, `StoresWrapperTests.cs` (§1.3). This division of
  test responsibility avoids duplicating COM-mock setup across two test classes.
- **`Init()` over `CreateAsync`.** `LoadStoresAsync` is already `async`; `Init()` is synchronous
  and matches how `CreateAsync` itself implements the factory
  (`Task.FromResult(new StoresWrapper(globals).Init())`). Calling `Init()` directly avoids an
  unnecessary `Task.FromResult` wrap-and-unwrap and keeps the seam's return type a plain
  `StoresWrapper` (simpler to stub in tests than `Task<StoresWrapper>`).
  `new StoresWrapper(_globals)` (not the parameterless constructor) is required so
  `GetFilteredStores()` can read `Globals.Ol.NamespaceMAPI.Stores` (§1.3).
  No `AwaitStoreRewireAsync` call is needed after `Init()` — `Init()` is a complete synchronous
  build, unlike the deserialize-then-rewire two-step path.
  **Note on identity naming:** this recommendation predates and is independent of F1's disabled-
  store identity work; it reuses `Init()` unchanged (see §3).
- **`logger.Warn`, not `logger.Error`, for the two recoverable branches.** These are now
  *handled* conditions — the model still ends up populated — so `Error` (reserved for conditions
  that leave the system in a degraded, actionable state) is no longer proportionate; `Warn`
  signals "notable, but the code recovered," consistent with the repo's existing use of `Warn`
  for recovered/non-fatal conditions (`AppOlObjects.cs:411`,
  `ResolveCurrentUserEmailAddress`'s COMException catch).
- **`try/catch (Exception)` at this method boundary is intentional and bounded (AC3).**
  `LoadStoresAsync` is one phase among several inside `ApplicationGlobals.LoadParallelAsync`
  (`Task.WhenAll(_toDoObjects.LoadAsync(), _autoFileObjects.LoadAsync(), _olObjects.LoadAsync())`,
  `:119-129`) and, more importantly, `LoadSequentialAsync` (`:131-224`), where an unhandled
  exception from `LoadOlObjectsPhaseAsync()` would abort the awaited chain and prevent the ToDo,
  AutoFile, Engines, and Events phases from ever running. Catching here isolates the store-load
  failure to the store-load feature, matching the general policy's "fail fast and explicitly...
  do not silently ignore errors... unless you immediately re-raise or propagate with added
  context" — the added context here is the message identifying `StoresWrapper` specifically,
  which the current bare `IdleAsyncQueue` catch (§1.1, Path 3) does not provide. This is a
  narrowly-scoped boundary catch, not a broad suppress-and-ignore.
- **No retry-inside-catch.** If `BuildFreshStoresWrapper()` itself throws (e.g., a COM error
  enumerating `Stores`), the same catch reports it once; there is no second fallback attempt,
  which avoids masking a genuine, unrecoverable failure behind an infinite or hidden retry loop.

### 2.2 Interaction with the existing readiness guard (no controller change)

`StoreWrapperController.EvaluateLaunchReadiness()` (`StoreWrapperController.cs:108-125`) and its
"Store settings are not available yet" dialog (`:130-142`) are unmodified by this design. On the
two recoverable paths, `StoresWrapper` is non-null with `Stores` populated after `LoadStoresAsync`
returns, so the guard reports `Ready` and `Launch()` opens normally — this directly satisfies
AC4. On the genuine-failure path (§2.3), `StoresWrapper` remains `null` and the guard still
reports `ModelUnavailable`, so the existing dialog still fires. Its current wording implies a
timing issue ("try again after startup completes"), which is imprecise for a genuine failure, but
the task instructions direct keeping "consistent with the existing readiness-guard UX" — changing
that dialog's copy is out of scope for this fix and is not required by any AC. Recording this as
a documented, not-required follow-up avoids scope creep into the controller (also consistent with
the #240 research's explicit rejection of touching `AppOlObjects.cs`/controller wording together
with a controller-level fix).

### 2.3 Surfacing genuine failures (AC3)

No new user-facing dialog is introduced. The catch block above elevates the failure to
`logger.Error` with the exception object attached (so the stack trace is preserved, unlike the
current bare-string `logger.Error("StoresWrapper config not found.")`) and an explicit mention of
`StoresWrapper` and its user-visible consequence ("store settings will remain unavailable"). This
is strictly more actionable than the status quo on two counts: (1) it fires from inside
`AppOlObjects`, attributing the failure to the store-load pipeline specifically, rather than only
surfacing as `IdleAsyncQueue`'s generic `"Failed to execute IdleAsyncQueue.actionAsync"`; and (2)
because the catch prevents the exception from escaping `LoadStoresAsync`, the existing
`StoreWrapperController` dialog remains the single, already-proven user-facing surface for "the
model is not available" — satisfying AC3's "logged at an actionable level and/or a clear
user-facing message" without adding a second, redundant notification path.

## 3. Interaction with F1 (#261, `store-disable-service`) — wave-0 sibling, no shared dependency

Both F1 and F2 are wave 0 (`depends_on: []` for each in `epic-plan.md`) and are expected to be
implemented in parallel worktrees off the same integration branch.

- **F1's touch points** (per `docs/features/active/2026-07-07-store-disable-service-261/spec.md`):
  a new persisted `[JsonProperty]` list on `StoresWrapper` "beside the existing exclusion lists"
  (i.e., near `StoresWrapper.cs:313-333`), plus integration into the include/exclude decision via
  `ShouldIncludeStore` / `StoreIsIncluded` / `StoreFilterAttribution.Decide`
  (`StoresWrapper.cs:145-309`).
- **F2's touch points** (this design): `TaskMaster/AppGlobals/AppOlObjects.cs` only (or its new
  partial file per §5). F2 does **not** modify `StoresWrapper.cs` — it calls `Init()` unchanged.
- **Line-level overlap: none verified.** F1's changes land in `StoresWrapper.cs` lines ~192-333;
  F2 does not edit that file. No merge conflict is expected between the two features on shared
  lines.
- **Semantic interaction worth flagging (not a line conflict).** `Init()`/`GetFilteredStores()`
  builds a `StoresWrapper` purely from live COM `Stores` plus the substring-exclusion properties
  and (after F1 lands) the new disabled-store list's *default* value (empty, since a freshly
  constructed `StoresWrapper` has nothing to deserialize). Consequently, if the fresh-build
  fallback fires in a session where the persisted config was lost or corrupted (Path 1/2) **and**
  a store had previously been disabled-for-future-sessions, that disablement is not present in
  the rebuilt model — the store reappears enabled until the operator disables it again. This is
  an inherent consequence of "the persisted config is unavailable," not a defect introduced by
  F2: there is no other source from which the previously-persisted disabled list could be
  recovered once the config blob itself is confirmed missing or null. Recommend documenting this
  in a one-line code comment at the `BuildFreshStoresWrapper` call site (e.g., "fresh build has no
  persisted disabled-store state to restore") so it is not later misattributed as a regression in
  F1 or F5 (`disabled-stores-settings-ui`). No code change is required in F2 to compensate.
- **Sequencing recommendation.** Because there is no line-level overlap, F1 and F2 do not need to
  be serialized relative to each other. The one dependency to verify at whichever PR merges
  second (or at the wave-0 barrier before promoting to the integration branch) is that `Init()`'s
  synchronous, side-effect-free contract (build-from-live-stores, return populated instance) is
  unchanged by F1's edits — F1's spec only adds a new property and extends the filter predicate
  bodies, it does not restructure `Init()` itself, so this check is expected to pass without
  rework. No shared file lock or explicit hand-off artifact is needed beyond the existing
  wave-0-barrier review already defined in the epic plan.

## 4. Bugfix workflow: failing regression test first (AC5)

Per the repo's bugfix workflow, a deterministic failing test must exist before the fix and pass
after. Two actions are required, not one, because an existing test currently encodes the bug as
correct behavior:

1. **Correct the mis-specified existing test.**
   `TaskMaster.Test/AppGlobals/AppOlObjectsCoverageTests.cs:75-87`
   (`LoadStoresAsync_LeavesStoresWrapperNullWhenConfigMissing`) currently asserts
   `sut.StoresWrapper.Should().BeNull()`. This assertion is the bug's signature and must be
   replaced (not merely renamed) to assert the fallback behavior instead — e.g. that
   `StoresWrapper` is non-null and that `BuildFreshStoresWrapper()` was invoked (via a
   `TestableAppOlObjects` override that records invocation and returns a sentinel instance,
   avoiding any live COM dependency). Under the general policy's "treat existing unit tests as
   part of the spec," this change is justified because the test itself specified the defect being
   fixed; the bugfix workflow explicitly expects such a test to flip from pass (encoding the bug)
   to fail (once the assertion is corrected to the intended behavior) to pass (after the fix).

2. **Add new regression tests for each of the three null paths**, using the seams already proven
   in this codebase (§1.3, §1.5) — no live Outlook, no temp files:
   - **Path 1 (config missing):** `StubApplicationGlobals` + `StubIntelligenceConfig` with an
     empty `ConcurrentDictionary<string, SmartSerializableLoader>`. Override
     `BuildFreshStoresWrapper()` in a `TestableAppOlObjects` subclass to return a sentinel
     `StoresWrapper` instance; assert `sut.StoresWrapper` is that sentinel and that the override
     was invoked exactly once (e.g., a captured invocation counter), matching the existing
     `TestableAppOlObjects` pattern that already overrides `AwaitStoreRewireAsync`.
   - **Path 2 (null deserialize):** Same globals/config setup as the existing
     `AppOlObjectsCoverageTests` valid-path test, but
     `Mock<ISmartSerializableNonTyped>.Setup(x => x.Deserialize<StoresWrapper,
     SmartSerializableLoader>(...)).Returns((StoresWrapper)null)`. Assert the same fallback
     outcome as Path 1, and assert `AwaitStoreRewireAsync` was **not** invoked (the fresh-build
     path bypasses rewire entirely, per §2.1).
   - **Path 3 (exception during load):** `Mock<ISmartSerializableNonTyped>.Setup(x =>
     x.Deserialize<StoresWrapper, SmartSerializableLoader>(...)).Throws<InvalidOperationException>()`
     (or `ArgumentNullException`, matching the real throw site in `SmartSerializableBase.cs:182-186`).
     Assert `LoadStoresAsync()` completes without throwing (the catch absorbs it),
     `sut.StoresWrapper` remains `null` (no fallback is attempted after an exception mid-deserialize,
     per §2.1's "no retry inside catch"), and — if a log-sink seam is available/added — that an
     `Error`-level entry was emitted. If no injectable log seam exists in this test class today,
     asserting "does not throw" plus "`StoresWrapper` stays null" is sufficient to cover AC3's
     behavioral contract; adding a log4net `IAppender`-based capture seam is optional and should
     not be introduced solely for this assertion if it is not already a repo convention (a repo
     grep for existing log-capture test patterns should precede adding one).
   - **Positive/no-regression path:** the existing
     `LoadAsync_AssignsStoresWrapperFromConfigAndCompletes` (`AppOlObjectsCoverageTests.cs:20-72`)
     and `LoadStoresAsync_DoesNotCompleteBeforeStoreRewireTaskFinishes`
     (`AppOlObjectsTests.cs:181-235`) already cover the valid-config path end-to-end and require
     no change; re-running them after the fix confirms no regression on the unchanged branch.
   - Each new test failing before the fix (because today `StoresWrapper` stays `null` in Paths
     1/2, and today `LoadStoresAsync` currently cannot even reach a "does not throw" assertion in
     the *pre*-Path-3 code because there is no wrapping try/catch to absorb the exception seeded
     by the mock) and passing after satisfies the bugfix-workflow requirement.

## 5. File-by-file change list and production-file estimate

| File | Change | Rationale |
|---|---|---|
| `TaskMaster/AppGlobals/AppOlObjects.cs` | Remove `StoresWrapper` property, `AwaitStoreRewireAsync`, `LoadStoresAsync`, and `LoadAsync()` (move to new partial file, §5.1) | Bring the file under the repo's 500-line cap; it is already at 525 lines before this change |
| `TaskMaster/AppGlobals/AppOlObjects.StoreLoading.cs` (new) | Add `LoadAsync()`, `StoresWrapper` property, `AwaitStoreRewireAsync`, new `BuildFreshStoresWrapper()` seam, restructured `LoadStoresAsync()` per §2.1 | New cohesive partial file following the `AppOlObjects.JunkFolders.cs` precedent (Issue #207, AC8) for the same file-size reason |
| `TaskMaster.Test/AppGlobals/AppOlObjectsCoverageTests.cs` | Correct `LoadStoresAsync_LeavesStoresWrapperNullWhenConfigMissing` (assertion inversion) + add Path 2/Path 3 regression tests | Bugfix-workflow requirement (§4); test file, not counted against production budget |
| `TaskMaster.Test/AppGlobals/AppOlObjectsTests.cs` | No change expected | Covers only the valid-config path, orthogonal to this fix |
| `UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs` | No change | `Init()`/`CreateAsync()` are reused unchanged; F1 edits this file independently (§3) |
| `UtilitiesCS/EmailIntelligence/IntelligenceConfig.cs` | No change | Its own null-loader path already drops the key (equivalent to Path 1) and already logs a resource-keyed `Error` (§1.2) |
| `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs` | No change | Readiness guard and dialog already handle a null model correctly (§2.2); no new user-facing surface is introduced |

**Production file count: 1 file touched, split into 2 files for size compliance** (net: one
existing partial file trimmed, one new partial file added; no new production types). This stays
within the small-path budget the #240 research established for adjacent work in the same class.

## 6. Automation feasibility

- **Format:** `dotnet tool run csharpier .` (file-based; unaffected by the partial-file split).
- **Analyze:** `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`.
- **Type-check:** `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`.
- **Test:** `vstest.console.exe <TaskMaster.Test assembly> /EnableCodeCoverage` — MSTest + Moq +
  FluentAssertions, using only the seams verified in §1.3/§1.5/§4; no live Outlook process, no
  temporary files.

## Rejected alternatives (brief)

- **Fix in `StoreWrapperController`/`RibbonController` instead of `AppOlObjects`.** Rejected: the
  #240 research already established the controller-level guard is the correct place for the
  *symptom* (crash-free dialog), and explicitly deferred the *root cause* (permanently-null
  `StoresWrapper`) as out of scope for that issue. F2's mandate, confirmed in the epic manifest's
  "Root-Cause Note," is exactly this deferred fix; re-doing it at the controller layer again would
  only mask the model staying null forever, not repair it.
- **Retry the fresh build inside the catch block if it also throws.** Rejected: would either loop
  indefinitely or require an ad hoc retry cap with no clear termination signal, and would obscure
  a genuine COM-level failure behind repeated attempts. A single, clearly logged failure with no
  retry is simpler and keeps the existing readiness guard as the sole recovery-signaling surface.
- **Add a new user-facing dialog specifically for the genuine-failure path.** Rejected per task
  guidance to keep the UX consistent with the existing readiness guard; the existing "not
  available yet" dialog already fires whenever `StoresWrapper` is null, so a second dialog would
  be redundant and would expand scope into `StoreWrapperController.cs`, which does not need to
  change for this fix.
- **Route the fresh build through `StoresWrapper.CreateAsync` instead of `Init()` directly.**
  Considered equivalent in effect (`CreateAsync` is `Task.FromResult(new
  StoresWrapper(globals).Init())`); `Init()` was preferred only because `LoadStoresAsync` is
  already `async` and calling the synchronous method directly avoids an unnecessary `Task` wrap,
  not because of any behavioral difference.

## File references

- `TaskMaster/AppGlobals/AppOlObjects.cs` (`StoresWrapper` property `:244`; `AwaitStoreRewireAsync`
  `:246-249`; `LoadStoresAsync` `:251-265`; `LoadAsync` `:34-38`; file length 525 lines)
- `TaskMaster/AppGlobals/AppOlObjects.JunkFolders.cs` (`:12-17`, partial-file precedent for the
  500-line cap, Issue #207 AC8)
- `UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs` (`Init()` `:35-49`; `CreateAsync` `:51-58`;
  `RewireOlObjectsAsync` `:83-127`; `GetFilteredStores` `:129-134`; `[JsonProperty]` region
  `:313-333`; `ShouldIncludeStore`/`StoreIsIncluded` `:145-309`)
- `UtilitiesCS/EmailIntelligence/IntelligenceConfig.cs` (`ReadConfigurationAsync` `:76-156`;
  null-loader drop `:140`; resource-keyed error log `:118-119`)
- `UtilitiesCS/ReusableTypeClasses/NewSmartSerializable/SmartSerializableBase.cs`
  (`Deserialize<T,U>(SmartSerializable<U> loader)` `:166-187`, the throw site behind Path 3)
- `UtilitiesCS/Threading/IdleAsyncQueue.cs` (`:60-95`, the current bare, unattributed catch
  wrapping the whole startup-load continuation)
- `TaskMaster/ThisAddIn.cs` (`Application_Startup` `:33-78`; `IdleAsyncQueue.AddEntry` continuation
  `:59-69`)
- `TaskMaster/AppGlobals/ApplicationGlobals.cs` (`LoadBasicMethod` `:99-117`; `Ol` property `:420`;
  `LoadOlObjectsPhaseAsync` `:394`; `LoadParallelAsync` `:119-129`; `LoadSequentialAsync`
  `:131-224`)
- `UtilitiesCS/Interfaces/IGlobals/IApplicationGlobals.cs`, `UtilitiesCS/Interfaces/IGlobals/IOlObjects.cs`
  (interface shapes used by every test double referenced above)
- `TaskMaster.Test/AppGlobals/AppOlObjectsCoverageTests.cs` (`:20-144`, seam patterns and the
  mis-specified test at `:75-87`)
- `TaskMaster.Test/AppGlobals/AppOlObjectsTests.cs` (`:181-260`, `AwaitStoreRewireAsync` seam
  usage via `TestableAppOlObjects`/`BaseAwaitingAppOlObjects`)
- `UtilitiesCS.Test/OutlookObjects/Store/StoresWrapperTests.cs` (`:24-90`, `:420-439`,
  `CreateGlobalsWithStores` — proven COM-mocking pattern for `Init()`/`CreateAsync`)
- `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs` (`EvaluateLaunchReadiness`
  `:108-125`; `Launch` dialog `:130-142` — unmodified by this design)
- `docs/features/active/2026-07-06-store-wrapper-launch-npe-240/research/2026-07-06T00-00-store-wrapper-launch-npe-240-research.md`
  (prior-art root-cause ranking and the explicit deferral of this exact fix as "optional
  follow-up")
- `docs/features/active/2026-07-07-store-disable-service-261/spec.md` (F1 scope used for the §3
  overlap analysis)
- `docs/features/epics/store-lockup-resilience/epic-plan.md` (wave/dependency DAG, confirmed
  root-cause note)
