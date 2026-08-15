# qfc-item-controller-init-tests-flaky-window-handle (Issue #571)

- Date captured: 2026-08-15
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/qfc-item-controller-init-tests-flaky-window-handle/ (Issue #571)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #571
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/571
- Last Updated: 2026-08-15
## Summary

Two tests in `QuickFiler.Controllers.Tests.QfcItemController_InitializationTests`
fail intermittently during a full-suite run with
`InvalidOperationException: Invoke or BeginInvoke cannot be called on a control
until the window handle has been created`, but pass every time when the class is
run in isolation. The tests exercise a real WinForms `Control.Invoke` path with
no seam, so they depend on whether the control's window handle happens to exist
when the test reaches it.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Python version: n/a (C# / .NET Framework 4.8.1, MSTest)
- Command/flags used: `vstest.console.exe <9 test assemblies> /EnableCodeCoverage /InIsolation /Logger:trx /TestCaseFilter:"TestCategory!=LiveOutlook"`
- Data source or fixture: none; the failure is in test-host state, not data

## Steps to Reproduce

1. Build the solution in Debug.
2. Run the full suite across all nine `*.Test.dll` assemblies with the command
   above.
3. Repeat. The two tests below fail on some runs and pass on others.

Observed on 2026-08-15: run 1 passed both tests, run 2 failed both, run 3 (full
suite) passed both. Running only
`/TestCaseFilter:"FullyQualifiedName~QfcItemController_InitializationTests"`
passed 9 of 9 on every attempt.

## Expected Behavior

Per `.claude/rules/general-unit-test.md`, tests are deterministic: identical
inputs and environment produce identical results, and the suite does not depend
on ordering or on ambient UI state.

## Actual Behavior

Both of the following fail non-deterministically:

- `InitializeNineArgOverload_ThroughThePumpHost_SavesParametersAndDelegates`
- `InitializeBool_ThroughThePumpHost_CompletesAndInitializesState`

```
System.InvalidOperationException: Invoke or BeginInvoke cannot be called on a
control until the window handle has been created.
   at System.Windows.Forms.Control.MarshaledInvoke(...)
   at System.Windows.Forms.Control.Invoke(Delegate method, Object[] args)
   at QuickFiler.ItemViewer.QuickFiler.IItemViewer.Invoke(Delegate method)
   at QuickFiler.Controllers.QfcItemController.InvokeBeginInvoke(Boolean async, Action action)
      in QuickFiler\Controllers\QfcItemController.FocusAndTheme.cs:line 256
   at QuickFiler.Controllers.QfcItemController.ToggleTips(Boolean async, ToggleState desiredState)
      in QuickFiler\Controllers\QfcItemController.FocusAndTheme.cs:line 204
```

## Logs / Screenshots

- [x] Attached minimal logs or screenshot
- Snippet: stack trace above, extracted from the TRX of the failing run on
  2026-08-15.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

The failure is intermittent and does not indicate a production defect, but a
flaky test in a protected gate is corrosive: it trains reviewers to re-run
rather than investigate, and it can fail an otherwise-green CI run at random.

## Suspected Cause / Notes

- `QfcItemController.InvokeBeginInvoke` at
  `QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs:256` calls
  `IItemViewer.Invoke` directly. `Control.Invoke` throws unless the control's
  native window handle already exists.
- The tests reach this through `ToggleTips` at
  `QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs:204`.
- Handle creation depends on ambient WinForms state in the shared test host,
  which differs between a full-suite run and a single-class run. This is exactly
  the mutable-global-state dependency the unit-test policy prohibits.
- A sibling test in the same file already documents this hazard and works around
  it with a headless `ProgressTrackerPane` built via
  `FormatterServices.GetUninitializedObject` (see the comment block in
  `UtilitiesCS.Test/Extensions/AsyncSerialization_Tests.cs`), which indicates the
  repository already has a pattern for avoiding a live message pump in tests.
- Preferred fix per `.claude/rules/csharp.md` "DI Seams": introduce an interface
  or injectable-delegate seam for the invoke path so the test supplies a
  synchronous no-op marshaller and never touches a real window handle.
  Adding a sleep, a retry, or a handle-forcing call would violate the
  "Prohibited Behaviors" section of the same rule.

## Proposed Fix / Validation Ideas

- [ ] Unit coverage areas: `QfcItemController` initialization overloads and
      `ToggleTips`, exercised through the new seam rather than a live control.
- [ ] Integration scenario to retest: full nine-assembly suite run repeatedly
      (at least 5 consecutive runs) to demonstrate the flakiness is gone.
- [ ] Manual verification notes: confirm the seam's production default still
      marshals through the real control so runtime behavior is unchanged.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
