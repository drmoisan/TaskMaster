# Remediation Inputs — Cycle 1 (Issue #292)

- **Entry timestamp:** 2026-07-09T16-05
- **Trigger:** S9 CI green gate — required check FAILED against PR #294 head `9ae5c0e3952f9ff29febd825b8def21a1981caff`.
- **Failing required check:** `Format, build, analyze, and test` (workflow CI)
- **Failing job URL:** https://github.com/drmoisan/TaskMaster/actions/runs/29046195330/job/86215357832
- **Canonical issue number:** 292

## Finding 1 — BLOCKING — Test-isolation regression: shared process-global `CurrentStoreContext._current` polluted by the new enumeration-phase scope under class-level parallel test execution

**Severity: Blocking** (required CI check FAILURE; 10 tests fail deterministically-under-parallelism)

### Evidence

CI (`vstest.console.exe` over all `*.Test.dll`, `/InIsolation`, run 29046195330) reports `Total tests: 5141 / Failed: 10`. All 10 failures are in `UtilitiesCS.Test`:

- `UtilitiesCS.Test/Threading/CurrentStoreContextTests.cs`: `Begin_SetsCurrent_ReadableInsideScope` (line 23), `Dispose_RestoresPreviousValue` (line 42), `NestedScopes_RestoreInnerThenOuter`, `SequentialScopes_EachRestoreToNull`, `Begin_NormalizesUnavailableIdentity_ToNoContext` (4 data cases: null, "", "   ", "<unavailable>"), `Begin_NormalizedInnerScope_RestoresRealOuterValue`.
- `UtilitiesCS.Test/Threading/ThreadMonitorTests.cs`: `EvaluatePoll_NoContext_CarriesNullIdentity`.

Representative assertion messages:

```
Expected CurrentStoreContext.Current to be <null> because no scope is open before the test, but found "<Stores-enumeration>".
Expected CurrentStoreContext.Current to be <null>, but found "<Stores-enumeration>".
Expected fired[0].StoreIdentity to be <null>, but found "<Stores-enumeration>".
```

### Root cause

`CurrentStoreContext` is a process-global `static volatile string _current` (by design — the #260 watchdog reads it from an independent background thread, so it deliberately is NOT `AsyncLocal`; the production type must stay a process-global static).

This PR added `MaterializeFilteredStores()` in `UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs`, which now opens `using (CurrentStoreContext.Begin(CurrentStoreContext.StoresEnumerationPhaseIdentity))` (`"<Stores-enumeration>"`) around the enumeration in both `Init()` and `RewireOlObjectsAsync()`.

`UtilitiesCS.Test` runs with `[assembly: Parallelize(Workers = 0, Scope = ClassLevel)]` (`UtilitiesCS.Test/Properties/AssemblyInfo.cs:18-20`) — class-level parallelism across all cores. That same assembly contains many `StoresWrapper` test classes (`StoresWrapperTests.cs`, `StoresWrapperRehookTests.cs`, `StoresWrapperDisableTests.cs`, etc.) that call `StoresWrapper.Init()`/enumeration. Those classes now hold `_current == "<Stores-enumeration>"` for the duration of each enumeration, running in parallel with the `[DoNotParallelize]` reader classes (`CurrentStoreContextTests`, `ThreadMonitorTests`) that assert `Current == null` at the start/around their body. `[DoNotParallelize]` on the reader class does not prevent concurrent writers in other parallelizable classes of the same assembly, so the reader observes the polluted global value.

Run `8d515463` (pre-memory-commit, identical test binaries) passed only because the parallel windows did not overlap on that scheduler pass — this is a real, recurring nondeterministic race, not a one-off flake, and it is introduced/aggravated by this PR's new writer.

### Required remediation (design constraints)

- Do NOT change the production `CurrentStoreContext` to `AsyncLocal`/`ThreadStatic` or otherwise weaken the process-global static — the watchdog cross-thread read requires it (research §1.2/§3.1).
- Do NOT weaken or delete the reader assertions (`Current == null` baseline is their contract) and do NOT remove the enumeration-phase attribution scope (it is the feature).
- The fix must restore deterministic isolation between the process-global-static reader tests and every test that opens a `CurrentStoreContext` scope, under `UtilitiesCS.Test`'s class-level parallelization. Candidate approaches for the planner to evaluate and choose the minimal correct one: (a) coordinate the reader classes and all `CurrentStoreContext`-scope-opening classes onto a shared non-parallel/serialized execution group (for example a common `[DoNotParallelize]` grouping or a shared lock/collection so they never overlap); (b) reduce the assembly parallelization scope where it is unsafe; or (c) another isolation mechanism that provably removes the overlap. The chosen fix must make the full `*.Test.dll` suite pass deterministically under the CI `vstest ... /InIsolation` invocation, with no new sleeps/retries/timing hacks and no coverage regression.
- Re-run of the failing job alone is NOT an acceptable resolution: the race is causal to this PR's new writer and will recur.
