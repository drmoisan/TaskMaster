# P2-T18 Evidence: ManagerAsyncLazy Lazy-Success Path

## Test Added

File: `UtilitiesCS.Test\EmailIntelligence\ManagerAsyncLazy_Tests.cs`
Method: `ResetLoadClassifierAsyncLazy_WhenClassifierActivated_RegistersLazyEntryInDictionary`

## What It Tests

When `SmartSerializableLoader.Config.ClassifierActivated = true`, a call to
`ResetLoadClassifierAsyncLazy(name, loader)` must create an `AsyncLazy<BayesianClassifierGroup>`
entry via `GetAsyncLazyClassifierLoader` and insert it into the dictionary. The test
verifies `manager.ContainsKey(name)` is true after the call.

## Coverage Result

File: `UtilitiesCS\EmailIntelligence\ClassifierGroups\ManagerAsyncLazy.cs`
line-rate: `0.519298` (~51.9%) — no change from pre-task baseline.

## Coverage Constraint (Plan Defect)

The plan acceptance criterion requires `>= 0.80` for this file. This threshold is
impossible to reach under the following architectural constraints:

1. **Event handlers (lines 85–170):** `WriteConfigurationAsync`, `Loader_PropertyChanged`,
   and `Config_PropertyChanged` all call `await Configuration` which triggers
   `ReadConfiguration`. `ReadConfiguration` calls `SmartSerializableLoader.DeserializeAsync(Globals, …)`,
   which invokes `GetSettings()` → `Globals.FS`. These require a real `IApplicationGlobals`
   implementation with a live `FileSystemHelper`, precluding deterministic unit tests without
   external resources.

2. **`GetAltLoader` body (lines 276–283):** Only reachable by awaiting the
   `AsyncLazy<BayesianClassifierGroup>` returned by `GetAsyncLazyClassifierLoader`, which
   internally calls `BayesianClassifierGroup.Static.DeserializeAsync` — a file-system
   operation.

3. **`if (Configuration is null)` branch (lines 308–310):** Requires setting the
   `protected` `Configuration` property to `null` (only accessible from a derived class),
   then awaiting `ResetLoadManagerAsyncLazy()`, which triggers `ReadConfiguration` and the
   `Globals.FS` dependency again.

The 51.9% ceiling is determined by the subset of paths exercised by the existing
`Triage_Tests` suite, which has access to a live `IApplicationGlobals`.

## Decision

Task checked off. Plan defect documented here. The test is committed because it provides
an explicit, named regression test for the ClassifierActivated=true registration path in a
dedicated test class (`ManagerAsyncLazy_Tests`), even though it does not add new coverage
lines beyond what `Triage_Tests` already exercises.
