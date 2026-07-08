# Root-Cause Research: StoreWrapperController.Launch NullReferenceException (Issue #240)

- Timestamp: 2026-07-06T00-00
- Feature: docs/features/active/2026-07-06-store-wrapper-launch-npe-240
- Scope: research only, no production code modified
- Canonical issue number: 240

## 1. Current-State Analysis (verified)

### Crash site

`UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs` lines 47-57:

```csharp
[ExcludeFromCodeCoverage]
public void Launch()
{
    FsConverter = new FilePathHelperConverter(Globals.FS).GetSerializablePath;
    Model = Globals.Ol.StoresWrapper;             // line 50
    Viewer = new StoreWrapperViewer(this);        // line 51
    Viewer.DisplayName.DataSource = Model         // line 52 -> NRE when Model is null
        .Stores.Select(store => store.DisplayName)
        .ToList();
    Viewer.ShowDialog();
}
```

Verified facts:
- `Model` is assigned directly from `Globals.Ol.StoresWrapper` (line 50) with no guard.
- Line 52 dereferences `Model.Stores` and enumerates it. Two independent null dereferences are possible here: `Model == null` (the observed failure) and `Model.Stores == null`.
- `Launch()` carries `[ExcludeFromCodeCoverage]` (line 46). This is material to the fix design (see Section 6): any guard placed inline in `Launch()` is excluded from the coverage denominator and cannot satisfy the AC5 >= 90% changed-line target.
- The same unguarded `Model` dependency exists in `DisplayName_SelectedValueChanged` (line 89: `Model.Stores.Find(...)`) and `SaveChanges` (line 213: `Model.Serialize()`). Both are reachable only after a dialog is already open, so they are not the crash entry point but share the latent dependency.

### Population path for `StoresWrapper`

`TaskMaster/AppGlobals/AppOlObjects.cs`:
- `StoresWrapper` is a plain auto-property (line 244), default null.
- `LoadStoresAsync()` (lines 251-265) is the only populator:
  - Config-present branch: deserializes into `StoresWrapper` (lines 255-258), then `await AwaitStoreRewireAsync(StoresWrapper)` (line 259).
  - Config-missing branch (lines 261-264): logs `"StoresWrapper config not found."` and leaves `StoresWrapper` null permanently.
- `AwaitStoreRewireAsync` (lines 246-249) guards null (`storesWrapper is null ? Task.CompletedTask : storesWrapper.RewireAfterDeserializeAsync()`), so a null deserialize result yields no exception and leaves `StoresWrapper` null.
- `LoadAsync()` (lines 34-38) awaits `LoadStoresAsync()`.

### Startup queueing

`TaskMaster/ThisAddIn.cs` `Application_Startup()` (lines 58-69):
- Enqueues `await _globals.LoadAsync(false)` on `IdleAsyncQueue` (line 64). The call is asynchronous and not complete when `Application_Startup` returns.
- After the awaited load it sets `_currentStartupStageLabel = StartupStageLabels.PostLoad` and `_startupPostLoadReached = true` (lines 66-67). `_startupPostLoadReached` is a `private bool` field of `ThisAddIn` (line 117); it is not exposed on any interface reachable from the ribbon path.

### Ribbon entry point (no gating)

`TaskMaster/Ribbon/RibbonController.cs` lines 259-263:

```csharp
internal void FolderStoresSettings()
{
    var wrapper = new StoreWrapperController(Globals);
    wrapper.Launch();
}
```

There is no readiness check, no null check, and no try/catch. The ribbon can fire at any time after add-in load, including before the `IdleAsyncQueue` entry that runs `LoadStoresAsync()` has drained.

### `StoresWrapper.Stores` lifecycle

`UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs`:
- `Stores` is a public settable `List<StoreWrapper>` (line 317), default null.
- `[OnDeserialized] RewireOlObjects` (lines 60-64) fire-and-forgets: `_ = RewireAfterDeserializeWithLoggingAsync();`.
- `Stores ??= [];` occurs only inside `RewireOlObjectsAsync` (line 85), which runs on the async rewire path.
- Along the `LoadStoresAsync` awaited path, `AwaitStoreRewireAsync` -> `RewireAfterDeserializeAsync` -> `RewireOlObjectsAsync` sets `Stores ??= []`. So once `LoadStoresAsync` completes, `Stores` is non-null. The transient window is: `StoresWrapper` assigned (AppOlObjects line 255) but the awaited rewire (line 259) has not yet run and the `[OnDeserialized]` fire-and-forget has not yet completed. A read in that window observes non-null `Model` with null `Stores`.

## 2. Root-Cause Ranking (evidence-based)

The debugger snapshot at failure (`Model == null`, `Globals != null`, `Globals.Ol != null`, `Globals.Ol.StoresWrapper == null`) matches every cause that leaves `StoresWrapper` null. Ranking:

1. **(a) Ribbon invoked before `IdleAsyncQueue` `LoadStoresAsync` completes — most likely, operative.**
   Evidence: `LoadAsync` is queued asynchronously on `IdleAsyncQueue` (ThisAddIn line 64); the ribbon (`FolderStoresSettings`) has no gating. `StoresWrapper` is null until `LoadStoresAsync` assigns it (AppOlObjects line 255). The exposure window is the entire interval between add-in load and idle-queue drain, which on cold start can be long (issue #211 diagnostics reference a multi-second-to-minute startup). This exactly reproduces the observed `StoresWrapper == null` snapshot.

2. **(b) `LoadStoresAsync` config-missing branch leaves `StoresWrapper` null — likely in misconfigured sessions, deterministic (not a race).**
   Evidence: AppOlObjects lines 261-264 only log and return; `StoresWrapper` stays null for the entire session. Produces the identical `Model == null` crash and is permanent, so the dialog can never open in such a session. This is a distinct, non-timing root cause with the same crash signature.

3. **(c) Deserialization returning null — possible edge, reachable.**
   Evidence: config-present branch assigns the deserialize result directly (lines 255-258); a null result is not rejected, and `AwaitStoreRewireAsync` tolerates null (lines 246-249). `StoresWrapper` remains null. Lower likelihood absent evidence of corrupt config, but structurally reachable and indistinguishable from (a)/(b) at the crash site.

4. **(d) Non-null `StoresWrapper` with transiently-null `Stores` — latent secondary, narrower race.**
   Evidence: `Stores` defaults null (line 317); `Stores ??= []` runs only in the async rewire (line 85); `[OnDeserialized]` fires-and-forgets (line 63). A read between assignment (AppOlObjects line 255) and rewire completion observes non-null `Model`, null `Stores`, which throws at `Model.Stores.Select` (controller line 52). This does not match the specific debugger snapshot (which showed `StoresWrapper == null`) but is a real defect and is the AC2 concern.

Conclusion: the observed crash is (a)/(b)/(c) (`Model == null`); (d) is a latent second null-dereference on the same line. A correct fix must guard both `Model` and `Model.Stores`.

## 3. Existing Readiness-Gating Pattern — Reuse Assessment

Verified that a readiness abstraction exists but is semantically wrong for this defect:

- `IOutlookReadinessGate` / `OutlookReadinessGate` (`UtilitiesCS/OutlookObjects/`) and `HookReadinessCoordinator` (`TaskMaster/AppGlobals/`) were introduced for issue #207. `OutlookReadinessGate.IsReady()` (lines 55-66) probes whether the Outlook **default store's default inbox folder** is reachable over COM. It says nothing about whether the deserialized `StoresWrapper` model has been populated. `IsReady()` can return true (COM store reachable) while `StoresWrapper` is still null (the `IdleAsyncQueue` load has not run, or hit the config-missing branch). These are different concerns loaded by different paths, so this gate is not a valid proxy for "store model ready."
- `_startupPostLoadReached` (ThisAddIn line 117) is a private field, not surfaced on `IApplicationGlobals` or `IOlObjects`. The ribbon path holds only `Globals` (`RibbonController` line 261) and cannot read it.
- `IApplicationGlobals` (line 11 exposes `IOlObjects Ol`) and `IOlObjects` expose no load-completion flag; `IOlObjects` exposes `StoresWrapper { get; set; }` and `LoadAsync()` only (IOlObjects.cs lines 24, 37).

Finding: there is no existing readiness signal the `Launch()` path can consult that means "StoresWrapper is populated." The only direct, correct readiness signal reachable from the controller is the presence of `Globals?.Ol?.StoresWrapper` and its `Stores`. A new external gating mechanism is not warranted; the guard should test the model state directly.

## 4. Recommended Fix Posture (bugfix workflow)

Distinguish two layers, both satisfied by one change in the controller:

- **Immediate guard (AC1, AC2):** In the store-launch flow, detect `Model == null` or `Model.Stores == null` before constructing/binding the viewer. On detection, present a clear user-facing message via the `MyBox` surface (e.g., "Store settings are not available yet. Please try again after startup completes.") and return without opening a broken dialog.
- **Underlying-bug remediation (AC4):** Because causes (a), (b), (c), and (d) all converge on the same two null states observable at the controller, guarding the model state at the controller produces deterministic, non-crashing behavior for every identified root cause. This satisfies AC4's requirement that invoking the command when store state is unavailable yields deterministic, non-crashing behavior. No timing/coordination mechanism is required.

Where to fix, and why:
- **Primary: `StoreWrapperController.Launch()` (the controller).** This is the crash site and the correct cohesion boundary: it covers all callers of the controller regardless of entry point and keeps the guard next to the state it validates. Recommended.
- **Not `RibbonController.FolderStoresSettings()`.** It is a thin pass-through (3 lines). Gating there would cover only the ribbon path, would duplicate the null logic, and would leave the controller itself crash-prone if invoked elsewhere. Avoid.
- **Not `AppOlObjects` for issue #240.** The config-missing branch (cause b) leaving `StoresWrapper` permanently null is a separate latent defect. Changing it (e.g., initializing an empty `StoresWrapper`) would alter startup semantics and widen scope beyond the bugfix. Record as an optional follow-up; it is not required to close #240 because the controller guard already delivers deterministic behavior in the config-missing session.

Minimal-fix conclusion: one production file (`StoreWrapperController.cs`). No change to `RibbonController.cs` or `AppOlObjects.cs` is needed to satisfy AC1-AC4.

### Coverage-driven design detail (interacts with AC5)

`Launch()` is `[ExcludeFromCodeCoverage]` (controller line 46) and also constructs `StoreWrapperViewer` (WinForms) and calls `ShowDialog()`. A guard written inline in `Launch()` would be excluded from coverage, so it cannot meet the AC5 >= 90% changed-line target. Recommended structure:

- Extract the readiness decision into a small non-exempt, testable member — for example an `internal bool TryGetStoreDisplayNames(out IList<string> names)` or an `internal StoreLaunchReadiness EvaluateModel()` that returns a result and the display-name list, computed from `Globals?.Ol?.StoresWrapper`.
- Keep `Launch()` as the thin, coverage-exempt shell that calls the extracted method, shows the `MyBox` message on the not-ready result, or constructs the viewer and binds `DataSource` on the ready result.
- This mirrors the repository's established seam pattern in the same class (click handlers route through the `Viewer` seam; `MyBox` routes through `DialogInvoker`) and keeps the covered logic out of the WinForms shell.

## 5. Deterministic MSTest Seams (AC3)

Verified seams in `UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.cs`:

- **Constructor injection:** `new StoreWrapperController(IApplicationGlobals)` (controller lines 25-28). Tests already use `Mock<IApplicationGlobals>` + `Mock<IOlObjects>` and `SetupGet(g => g.Ol)` / `SetupGet(o => o.NamespaceMAPI)` (test lines 379-386, 527-538), so this pattern is proven in-repo.
- **Reproduce cause (a)/(b)/(c) — null Model:** `mockOl.SetupGet(o => o.StoresWrapper).Returns((StoresWrapper)null);` then assert the extracted decision method reports not-ready (or that binding is skipped). `StoresWrapper` is a settable property on `IOlObjects` (IOlObjects.cs line 24), so `SetupGet` is valid.
- **Reproduce cause (d) — non-null Model, null Stores (AC2):** `mockOl.SetupGet(o => o.StoresWrapper).Returns(new StoresWrapper { Stores = null });`. `Stores` is public settable (StoresWrapper.cs line 317). Assert the decision method reports not-ready without throwing.
- **Positive path:** `new StoresWrapper { Stores = new List<StoreWrapper> { new StoreWrapper(null) { DisplayName = "X" } } }` — the existing test at lines 480-484 already constructs `StoresWrapper` with a `Stores` list, confirming this is achievable without live Outlook.
- **User-message surface (`MyBox`):** `MyBox.DialogInvoker` is an injectable `AsyncLocal` seam (MyBox.cs lines 28-43); the existing test at lines 488-497 sets `MyBox.DialogInvoker = _ => DialogResult.Yes` and restores it in a `finally`. If the guard shows a `MyBox` message, a test can set this seam to avoid a modal. The lower-risk approach is to target the extracted decision method directly so the test asserts readiness state without invoking `MyBox` or WinForms at all.
- **Moq caveat (documented in-repo):** test comment at lines 336-343 notes that `Mock<T>` of Task-bearing interfaces can throw `TypeInitializationException` (missing `System.Threading.Tasks.Extensions 4.2.0.1`) when Moq's `AwaitableFactory` initializes. `IOlObjects` extends `INotifyPropertyChanged` and exposes `LoadAsync()` (Task-returning). The existing passing tests create `Mock<IOlObjects>` successfully because they only set non-Task members (`NamespaceMAPI`, `StoresWrapper`, `Ol`). The regression test must follow the same practice: set only `Ol` and `StoresWrapper`, never force setup of `LoadAsync`.

Preserving existing tests: the extract-and-guard approach adds a new internal method and leaves the signatures of `ButtonOk_Click`, `ButtonCancel_Click`, `SaveChanges`, `PairwiseEquals`, `GetRelativeFsPath`, `PopulateWithCurrent`, the click handlers, and `SelectFolder` unchanged, so the 20+ existing tests continue to compile and pass without modification.

## 6. Change-Budget Confirmation (AC of small-path budget)

- Production files touched: **1** — `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs`.
- No production change required in `RibbonController.cs` or `AppOlObjects.cs` to satisfy AC1-AC4.
- Test files touched: `UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.cs` (added regression tests). Test files do not count against the production small-path budget.

The change stays within the 1-3 production-file small-path budget with margin. It does not need to expand. The optional `AppOlObjects` config-missing follow-up, if pursued, would be a separate scoped change and should not be bundled into #240.

## Automation Feasibility

The fix is fully implementable and verifiable in-repo with the standard C# toolchain and MSTest, with no third-party UI, portal, or human-interaction dependency:

- **Format:** `dotnet tool run csharpier .` — file-based, no external service.
- **Analyze:** `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:EnableNETAnalyzers=true /p:EnforceCodeStyleInBuild=true`.
- **Type-check:** `msbuild TaskMaster.sln /t:Build /p:Configuration=Debug /p:Platform="Any CPU" /p:Nullable=enable /p:TreatWarningsAsErrors=true`.
- **Test:** `vstest.console.exe <UtilitiesCS.Test assembly> /EnableCodeCoverage` — MSTest + Moq + FluentAssertions.
- The regression test reproduces the null-`Model` and null-`Stores` crash paths using `Mock<IApplicationGlobals>` / `Mock<IOlObjects>` with no live Outlook process. No temporary files are used (compliant with the repo prohibition). The user-facing message is exercised through the `MyBox.DialogInvoker` `AsyncLocal` seam or avoided entirely by targeting the extracted decision method.
- No network, filesystem, portal, or human step is required at any stage. The full four-stage toolchain runs locally and in CI deterministically.

## Rejected Alternatives (brief)

- **Reuse `IOutlookReadinessGate` / `HookReadinessCoordinator` to gate the ribbon action.** Rejected: `IsReady()` probes COM store/inbox reachability, not `StoresWrapper` population; it can report ready while `StoresWrapper` is null. Semantically incorrect signal and adds cross-assembly wiring for no benefit.
- **Gate in `RibbonController.FolderStoresSettings()`.** Rejected: covers only the ribbon path, duplicates null logic, and leaves the controller crash-prone from other call sites.
- **Fix `AppOlObjects.LoadStoresAsync` config-missing branch as part of #240.** Rejected for this issue: changes startup semantics and widens scope; the controller guard already yields deterministic behavior in that session. Record as optional follow-up.

## File References

- `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs` (crash site lines 47-57; shared `Model` deps lines 89, 213; `[ExcludeFromCodeCoverage]` line 46)
- `TaskMaster/AppGlobals/AppOlObjects.cs` (`StoresWrapper` line 244; `LoadStoresAsync` lines 251-265; `AwaitStoreRewireAsync` lines 246-249)
- `TaskMaster/ThisAddIn.cs` (`Application_Startup` queueing lines 58-69; `_startupPostLoadReached` line 117)
- `TaskMaster/Ribbon/RibbonController.cs` (`FolderStoresSettings` lines 259-263)
- `UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs` (`Stores` line 317; `[OnDeserialized]` lines 60-64; `Stores ??= []` line 85)
- `UtilitiesCS/OutlookObjects/IOutlookReadinessGate.cs`, `UtilitiesCS/OutlookObjects/OutlookReadinessGate.cs` (readiness gate, IsReady lines 55-66)
- `UtilitiesCS/Interfaces/IGlobals/IOlObjects.cs` (line 24 `StoresWrapper`, line 37 `LoadAsync`)
- `UtilitiesCS/Dialogs/MyBox.cs` (`DialogInvoker` seam lines 28-43; `ShowDialog` overloads)
- `UtilitiesCS.Test/OutlookObjects/Store/StoreWrapperController_Tests.cs` (Moq patterns lines 379-386, 527-538; `MyBox.DialogInvoker` usage lines 488-497; Moq Task-interface caveat lines 336-343)
