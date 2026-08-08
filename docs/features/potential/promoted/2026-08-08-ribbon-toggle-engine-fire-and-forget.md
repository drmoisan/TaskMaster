# ribbon-toggle-engine-fire-and-forget (Issue #506)

- Date captured: 2026-08-08
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/ribbon-toggle-engine-fire-and-forget/ (Issue #506)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #506
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/506
- Last Updated: 2026-08-08
## Summary

`RibbonViewer.SpamBayesEnabled_Click` and `RibbonViewer.TriageEnabled_Click` are `void` methods whose body is an unawaited call to `Controller.Engines.ToggleEngineAsync(...)`. The returned `Task` is discarded, so the toggle completes asynchronously with no ordering guarantee and any exception it raises is swallowed into an unobserved task.

## Environment

- OS/version: Windows 11, Outlook desktop (VSTO add-in host)
- Runtime: .NET Framework 4.8.1, TaskMaster VSTO add-in
- Command/flags used: Outlook Explorer ribbon, Spam Manager and Triage configuration menus
- Data source or fixture: Live Outlook profile

## Steps to Reproduce

1. Open Outlook with the TaskMaster add-in loaded.
2. Click the "SpamBayes Enabled" toggle button (or "Triage Enabled").
3. Observe that the caller returns before `ToggleEngineAsync` has awaited `Globals.AF.Manager.Configuration` and flipped `ClassifierActivated`.
4. Induce a failure inside the configuration load and observe that no error is surfaced.

## Expected Behavior

The toggle either completes before the handler returns, or the handler is `async void` with an explicit `await` and a boundary `try/catch` that reports the failure through the project logging pattern. Either way an exception is observed and logged rather than discarded.

## Actual Behavior

```csharp
public void SpamBayesEnabled_Click(Office.IRibbonControl control, bool pressed) =>
    Controller.Engines.ToggleEngineAsync(SpamBayes.GroupName);

public void TriageEnabled_Click(Office.IRibbonControl control, bool pressed) =>
    Controller.Engines.ToggleEngineAsync("Triage");
```

(`TaskMaster/Ribbon/RibbonViewer.cs`, Spam Config and Triage Config regions.)

`ToggleEngineAsync` awaits `Globals.AF.Manager.Configuration` before mutating `loader.Config.ClassifierActivated`, so the state change is genuinely deferred. The discarded `Task` means a faulted toggle is silently lost; the user sees no error and the setting simply does not change.

The sibling handlers in the same regions (`SpamSaveNetwork_Click`, `SpamSaveLocal_Click`, `TriageSaveNetwork_Click`, `TriageSaveLocal_Click`) are correctly written as `async void` with `await`, which makes these two an inconsistency rather than a deliberate pattern.

## Logs / Screenshots

- [x] Attached minimal logs or snippet
- Snippet: see the source excerpt above. No log entry is produced on failure, which is the defect.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [ ] Medium
- [x] Low

The happy path usually works because the configuration task is typically already complete by the time a user reaches the configuration menu. The defect is the swallowed failure and the absent ordering guarantee, not a routinely-observed break.

## Suspected Cause / Notes

Two handlers were written as expression-bodied `void` members while their siblings were written as `async void` with `await`. The compiler does not warn here because the discarded value is a `Task` returned from an expression-bodied member.

Note that `.claude/rules/general-code-change.md` requires failing fast and explicitly and prohibits silently ignoring errors, which this violates.

Found while researching issue #503 (ribbon engine readiness guard); out of scope for that fix.

## Proposed Fix / Validation Ideas

- [x] Unit coverage areas: extract the toggle invocation behind a testable seam and assert the returned task is awaited and that a faulted task is reported through the logging sink rather than discarded.
- [x] Integration scenario to retest: toggle each engine in live Outlook and confirm the setting changes and a failure is surfaced.
- [x] Manual verification notes: confirm the handler does not block the Outlook UI thread.

Consider whether an analyzer rule (for example the existing AsyncFixer package listed in `.claude/rules/csharp.md`) can be raised to catch discarded tasks across the codebase rather than fixing only these two sites.

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
