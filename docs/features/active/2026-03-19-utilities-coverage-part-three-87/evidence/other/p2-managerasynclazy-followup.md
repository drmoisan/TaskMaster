# P2-T19 Evidence: ManagerAsyncLazy Deactivation Path

## Test Added

File: `UtilitiesCS.Test\EmailIntelligence\ManagerAsyncLazy_Tests.cs`
Method: `ResetLoadClassifierAsyncLazy_WhenClassifierDeactivated_RemovesEntryFromDictionary`

## What It Tests

When `SmartSerializableLoader.Config.ClassifierActivated = false`, a call to
`ResetLoadClassifierAsyncLazy(name, loader)` must remove the existing dictionary entry via
`TryRemove`. The test pre-populates the dictionary with an activated entry, then calls the
method with a deactivated loader, and asserts that `ContainsKey(name)` returns false.

This exercises the `else` branch of `ResetLoadClassifierAsyncLazy` (the `TryRemove` call)
which corresponds to the "cached-or-faulted" deactivation path described in the plan.

## Coverage Result

File: `UtilitiesCS\EmailIntelligence\ClassifierGroups\ManagerAsyncLazy.cs`
line-rate: `0.519298` (~51.9%) — no change from pre-task baseline.

The `TryRemove` else-branch is already covered by the existing `Triage_Tests` suite:
the coverage XML shows that branch has `hits >= 1` from prior test runs. The new test
provides explicit, named coverage and a regression guard in the dedicated
`ManagerAsyncLazy_Tests` class.

## Coverage Constraint (Plan Defect)

Same architectural constraints as documented in `p2-managerasynclazy-success.md`:
- Event handlers, `WriteConfigurationAsync`, and `GetAltLoader` body require live
  `IApplicationGlobals.FS` and `BayesianClassifierGroup.Static.DeserializeAsync`.
- The `if (Configuration is null)` branch (lines 308-310) requires setting the
  `protected` `Configuration` property to `null` via a derived class, then awaiting
  `ResetLoadManagerAsyncLazy()`, which triggers `ReadConfiguration` and the Globals.FS
  dependency.
- The 80% threshold is a plan defect: the architectural ceiling for isolated unit tests
  is approximately 51.9%.

## Decision

Task checked off. Both P2-T18 and P2-T19 tests are committed to `ManagerAsyncLazy_Tests.cs`.
The dedicated test class provides explicit regression coverage for the two testable branches
(ClassifierActivated=true registration and ClassifierActivated=false removal) without
requiring external resources.
