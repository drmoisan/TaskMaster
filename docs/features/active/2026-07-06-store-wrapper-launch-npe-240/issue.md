# store-wrapper-launch-npe (Issue #240)

- Date captured: 2026-07-06
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/store-wrapper-launch-npe/ (Issue #240)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #240
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/240
- Last Updated: 2026-07-06
- Work Mode: minor-audit

## Summary

`StoreWrapperController.Launch()` throws an unhandled `System.NullReferenceException` when the Outlook store settings dialog is opened before the store-wrapper model has been initialized. The immediate crash is a missing null guard; the underlying issue is that the ribbon entry point invokes the dialog with no gating on whether async store initialization completed or succeeded.

## Environment

- OS/version: Windows, Outlook desktop (VSTO add-in)
- Assembly: UtilitiesCS
- Command/flags used: Ribbon action -> RibbonController.FolderStoresSettings() -> StoreWrapperController.Launch()
- Data source or fixture: Globals.Ol.StoresWrapper (populated by AppOlObjects.LoadStoresAsync during startup)

## Steps to Reproduce

1. Start Outlook with the TaskMaster add-in; startup queues `_globals.LoadAsync(false)` on the `IdleAsyncQueue`.
2. Invoke the store/junk-folder settings ribbon action before `LoadStoresAsync()` completes, or in a session where `LoadStoresAsync()` did not populate `StoresWrapper` (config-missing branch logs an error and leaves it null, or deserialization returned null).
3. `StoreWrapperController.Launch()` executes.

## Expected Behavior

The dialog either opens with a valid store model, or the command fails gracefully with a clear, user-facing message (and no unhandled exception) when store state is not yet available.

## Actual Behavior

Unhandled exception reaches the user:

```
System.NullReferenceException
  HResult=0x80004003
  Message=Object reference not set to an instance of an object.
  Source=UtilitiesCS
  StackTrace:
   at UtilitiesCS.OutlookObjects.Store.StoreWrapperController.Launch() in ...\UtilitiesCS\OutlookObjects\Store\StoreWrapperController.cs:line 52
```

## Logs / Screenshots

- [x] Attached minimal logs or screenshot
- Snippet: debugger cause analysis shows `this.Model == null`, `Globals != null`, `Globals.Ol != null`, `Globals.Ol.StoresWrapper == null` at the point of failure.

## Impact / Severity

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

## Suspected Cause / Notes

- Immediate defect: `Launch()` (StoreWrapperController.cs lines 50-54) assigns `Model = Globals.Ol.StoresWrapper` and dereferences `Model.Stores.Select(...)` with no null guard. `DisplayName_SelectedValueChanged` (line 89) and `SaveChanges` (line 213) share the same unguarded `Model` dependency.
- Secondary latent risk: even a non-null `StoresWrapper` can transiently have a null `Stores` list. `[OnDeserialized] RewireOlObjects` fires-and-forgets the rewire (`_ = RewireAfterDeserializeWithLoggingAsync()`), and `Stores ??= []` runs only inside that async path. `Launch()` guards neither `Model` nor `Model.Stores`.
- Underlying bug: `StoresWrapper` is populated only by `AppOlObjects.LoadStoresAsync()` during async startup (`ThisAddIn` queues `_globals.LoadAsync(false)` on `IdleAsyncQueue`). `RibbonController.FolderStoresSettings()` invokes `Launch()` with no readiness gate and no recovery when the load has not completed or failed.
- Files to inspect: `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs`, `TaskMaster/Ribbon/RibbonController.cs`, `TaskMaster/AppGlobals/AppOlObjects.cs`, `UtilitiesCS/OutlookObjects/Store/StoresWrapper.cs`.

## Proposed Fix / Validation Ideas

- [x] Unit coverage areas: `Launch()` null-model and null-stores behavior; graceful-failure path.
- [x] Integration scenario to retest: open store settings dialog before/after store load completes.
- [x] Manual verification notes: confirm no unhandled exception when store state is unavailable; confirm normal open when populated.

## Acceptance Criteria

- [x] AC1: `StoreWrapperController.Launch()` does not throw an unhandled `NullReferenceException` when `Globals.Ol.StoresWrapper` (`Model`) is null. It fails gracefully with a clear user-facing message and returns without opening a broken dialog. (Evidence: `evidence/regression-testing/pass-after-240.md`, fix in `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs` P2-T3.)
- [x] AC2: `Launch()` also handles a non-null `Model` whose `Stores` list is null (transient post-deserialize state) without throwing. (Evidence: `evidence/regression-testing/pass-after-240.md`, fix in `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs` P2-T3.)
- [x] AC3: A deterministic MSTest regression test reproduces the pre-fix crash path (fails before the fix, passes after) using Moq for `IApplicationGlobals`/`IOlObjects`; no live Outlook, no temporary files. (Evidence: `evidence/regression-testing/fail-before-240.md` and `evidence/regression-testing/pass-after-240.md`, P1-T3/P2-T5.)
- [x] AC4: The underlying readiness/initialization gap identified by root-cause research is addressed so that invoking the store-settings command when store state is unavailable produces deterministic, non-crashing behavior rather than an unhandled exception. (Evidence: `EvaluateLaunchReadiness()` in `UtilitiesCS/OutlookObjects/Store/StoreWrapperController.cs`, P2-T1/P2-T2/P2-T3.)
- [x] AC5: The full C# toolchain passes in order (csharpier -> .NET analyzers -> nullable/TreatWarningsAsErrors -> MSTest with coverage); coverage on changed lines meets the >= 90% new-code target and repository line coverage remains >= 80% for the testable denominator. (Evidence: `evidence/qa-gates/qa-01-format.md` through `evidence/qa-gates/qa-05-coverage-delta.md`, P3-T1 through P3-T5. Note: the solution-wide nullable gate's raw `EXIT_CODE` is 1 due to a pre-existing, unrelated condition documented in `evidence/qa-gates/qa-03-nullable.md`; the touched files themselves introduce zero new nullable diagnostics. Scope reconciliation (feature-review Finding 4): the ">= 80% repository line coverage" clause is verified for the measured testable denominator of the `UtilitiesCS` assembly (85.88%, unchanged, no regression) and for this change's new/changed lines (100%). A canonical, merged repo-wide C# coverage artifact (`artifacts/csharp/coverage.xml` across all first-party test projects) does not yet exist in-repo; producing it is a pre-existing, maintainer-owned item tracked under `feature/csharp-coverage-uplift` (feature-review Finding 2), not attributable to issue #240's commits.)
- [ ] AC6: All required PR CI checks are green against the PR head SHA.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [x] Move to active fix folder / branch
