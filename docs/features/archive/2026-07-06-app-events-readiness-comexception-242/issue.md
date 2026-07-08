# app-events-readiness-comexception (Issue #242)

- Date captured: 2026-07-06
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/2026-07-06-app-events-readiness-comexception-242/ (Issue #242)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #242
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/242
- Last Updated: 2026-07-06
- Work Mode: minor-audit

## Summary

TaskMaster can throw an unhandled `System.Runtime.InteropServices.COMException`
from the `AppEvents.Hook()` readiness `DispatcherTimer` callback during Outlook
startup. The reported HRESULT is `0x90740111`, which is not currently classified
as a transient readiness error.

## Environment

- OS/version: Windows / Outlook VSTO add-in host
- .NET target: TaskMaster C# solution
- Runtime path: `TaskMaster.AppEvents.Hook()` readiness timer
- Data source or fixture: Outlook COM startup state

## Steps to Reproduce

1. Start TaskMaster as an Outlook add-in while Outlook is still initializing its
   store-backed objects.
2. Allow `AppEvents.Hook()` to create the readiness `DispatcherTimer`.
3. When the timer callback calls `HookReadinessCoordinator.Tick()`, observe a
   COM failure with HRESULT `0x90740111`.

## Expected Behavior

Transient Outlook readiness COM failures during the hook readiness callback are
treated as retryable. The timer should continue polling instead of surfacing an
unhandled dispatcher exception.

## Actual Behavior

The dispatcher surfaces an unhandled COM exception:

```text
System.Runtime.InteropServices.COMException
HResult=0x90740111
Message=Exception from HRESULT: 0x90740111
```

The first confirmed app-owned frame is the lambda registered in
`TaskMaster.AppEvents.Hook()`. `OutlookReadinessGate.IsReady()` already catches
all `COMException` values, so the unhandled exception is consistent with
`PerformReadinessHookup()` throwing a COM exception that
`OutlookReadinessGate.IsTransientError()` does not classify as retryable.

## Logs / Screenshots

- [x] Attached minimal logs or screenshot
- Snippet:

```text
System.Runtime.InteropServices.COMException
  HResult=0x90740111
  Message=Exception from HRESULT: 0x90740111
  Source=<Cannot evaluate the exception source>
  StackTrace:
<Cannot evaluate the exception stack trace>
```

## Impact / Severity

- [ ] Blocker
- [ ] High
- [x] Medium
- [ ] Low

## Suspected Cause / Notes

`OutlookReadinessGate` currently treats only `0xDAC40111` and `0x8E640111` as
transient Outlook not-ready HRESULTs. The reported `0x90740111` has the same
low readiness/status code suffix (`0x0111`) and appears in the same timer-driven
startup readiness path, but it is absent from the transient classifier. Because
the coordinator rethrows non-transient COM exceptions by design, the missing
classification allows this startup readiness variant to escape the
`DispatcherTimer` callback.

## Proposed Fix / Validation Ideas

- [ ] Add a focused regression test showing a `0x90740111` COM exception from
      the readiness hookup returns `ContinuePolling` and does not mark the
      coordinator complete.
- [ ] Add `0x90740111` to the `OutlookReadinessGate` transient readiness
      classifier.
- [ ] Verify the existing non-transient propagation test still passes so the
      classifier remains narrow.
- [ ] Run the C# formatter, analyzer build, nullable build, and MSTest command
      in the repository-required order.

## Acceptance Criteria

- [x] `OutlookReadinessGate.IsTransientError()` classifies HRESULT `0x90740111`
      as a transient Outlook readiness error.
- [x] A focused regression test proves a `0x90740111` COM exception thrown from
      readiness hookup returns `ContinuePolling` and leaves the coordinator
      incomplete for retry.
- [x] Existing non-transient COM exception behavior remains unchanged.
- [x] The required C# format, analyzer, nullable, and MSTest verification
      commands pass in the repository-required order.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [x] Move to active fix folder / branch
