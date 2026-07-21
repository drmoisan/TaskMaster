# outlook-crash-async-void-sectiongroupname (Issue #270)

- Date captured: 2026-07-07
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/outlook-crash-async-void-sectiongroupname/ (Issue #270)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #270
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/270
- Last Updated: 2026-07-08
- Work Mode: minor-audit

## Summary

The TaskMaster VSTO Outlook add-in terminates `outlook.exe` with an unhandled `System.ArgumentException` ("The parameter 'sectionGroupName' is invalid."). The crash is produced by two `async void` Outlook COM event handlers whose `catch (Exception) { throw; }` reschedules a recoverable settings/config fault onto the ThreadPool with no captured `SynchronizationContext`, terminating the process.

## Environment

- OS/version: Windows; host process `outlook.exe`, CLR v4.0.30319 (.NET Framework)
- Python version: N/A (C# VSTO add-in)
- Command/flags used: Normal add-in operation (Outlook item events on inbox / to-do items)
- Data source or fixture: Live Outlook mailbox; deployed `outlook.exe.config` / per-user `user.config`

## Steps to Reproduce

1. Run Outlook with the TaskMaster add-in loaded.
2. Trigger an inbox `ItemAdd` or to-do `ItemChange` event whose handler path performs a `Settings.Default` access or `Settings.Default.Save()` that raises the framework `sectionGroupName` `ArgumentException` (dependent on the deployed/per-user config state).
3. Observe `outlook.exe` termination with the unhandled exception.

Note: the exact configuration state that produces the invalid `sectionGroupName` cannot be reproduced from repository evidence (it depends on the deployed `outlook.exe.config` / `user.config`, which are not in the repo). The process-termination mechanism, however, is reproducible in a unit test via seams.

## Expected Behavior

A fault raised inside an Outlook item-event handler is logged and contained; it must not terminate `outlook.exe`. The add-in remains running and the exception is recorded in the log for diagnosis.

## Actual Behavior

The exception escapes the `async void` handler and is rethrown on a ThreadPool worker, terminating the process:

```
System.ArgumentException
  HResult=0x80070057
  Message=The parameter 'sectionGroupName' is invalid.
Parameter name: sectionGroupName
```

## Logs / Screenshots

- [x] Attached minimal logs or screenshot
- Snippet (crash-time managed stack, CLR rethrow signature only):

```
ExceptionDispatchInfo.Throw()
  ExecutionContext.RunInternal
  ExecutionContext.Run
  QueueUserWorkItemCallback.ExecuteWorkItem
  ThreadPoolWorkQueue.Dispatch
```

## Impact / Severity

- [x] Blocker
- [ ] High
- [ ] Medium
- [ ] Low

Rationale: an unhandled exception on a routine Outlook item event terminates the host process.

## Suspected Cause / Notes

Confirmed by research (`docs/research/2026-07-08-sectiongroupname-argumentexception-crash.md`) and independently verified:

- Proximate cause: `System.Configuration` `ArgumentException("sectionGroupName")` thrown inside framework code, reached only via `ApplicationSettingsBase` (`Settings.Default` / `.Save()`). Exact invalid value depends on deployed config not in the repo; not statically reproducible.
- Systemic cause (actionable): `TaskMaster/AppGlobals/AppEvents.ReadinessHookup.cs:63-73` (`OlToDoItems_ItemChange`) and `:75-85` (`OlInboxItems_ItemAdd`) use `catch (System.Exception) { throw; }` in `async void` handlers, forcing a ThreadPool rethrow that terminates the process.

Follow-ups (out of scope for this fix): capture the crashing machine's `user.config` / `outlook.exe.config` to identify the proximate config trigger; the ~40 unguarded `RibbonViewer` `async void *_Click` handlers are additional process-crash vectors.

## Proposed Fix / Validation Ideas

- [x] Unit coverage areas: extend `TaskMaster.Test/AppGlobals/AppEventsTests.cs` to drive the item-event handler with an injected collaborator that throws, asserting the handler logs and contains (does not rethrow).
- [ ] Integration scenario to retest: N/A (COM event path not unit-testable without seams).
- [x] Manual verification notes: after the fix, the next occurrence records the full exception + stack in the log instead of crashing Outlook.

Fix: in the two `async void` handlers, replace `catch (Exception) { throw; }` with a catch that logs the full exception via the existing `logger` and does not rethrow.

## Acceptance Criteria

- [x] AC1: `OlToDoItems_ItemChange` no longer contains `catch (System.Exception) { throw; }`; a fault from the awaited `ToDoEvents.OlToDoItems_ItemChange` call is logged (full exception, via the existing `logger`) and contained, with no exception escaping the `async void` method.
- [x] AC2: `OlInboxItems_ItemAdd` no longer contains `catch (System.Exception) { throw; }`; a fault from the awaited `ProcessMailItemAsync` call is logged (full exception, via the existing `logger`) and contained, with no exception escaping the `async void` method.
- [x] AC3: The logged output preserves the original exception object (message and stack), so a previously-lost `sectionGroupName` `ArgumentException` becomes observable in the log rather than being silently swallowed or rethrown.
- [x] AC4: A deterministic MSTest regression test (Moq + FluentAssertions, no COM/network/temp files) drives each handler path with an injected collaborator that throws a synthetic exception and asserts the handler contains and logs it (does not throw). The test fails against the pre-fix `catch { throw; }` and passes after the fix.
- [x] AC5: The full C# toolchain passes in order (CSharpier -> .NET analyzers -> nullable/type-check -> MSTest) with no new warnings, and coverage on changed lines does not regress.
- [x] AC6: No scope creep — only `TaskMaster/AppGlobals/AppEvents.ReadinessHookup.cs` (production) and `TaskMaster.Test/AppGlobals/AppEventsTests.cs` (test) are changed for the fix. The proximate config trigger and the `RibbonViewer` async-void handlers remain documented follow-ups.

## Next Step

- [x] Promote to GitHub issue (bug-report template)
- [x] Move to active fix folder / branch
