# efc-controllers-null-guard-and-async-void-boundary-defects (Issue #464)

- Date captured: 2026-08-07
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/efc-controllers-null-guard-and-async-void-boundary-defects/ (Issue #464)
- Work Mode: full-bug

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #464
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/464
- Last Updated: 2026-08-08
## Summary

Across `EfcFormController` and `EfcItemController`, theme and dark-mode property accessors lack the
null guards their already-merged QFC twins have, and several `async void` handlers rethrow from their
catch blocks — which terminates the host process rather than surfacing a handled error.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Runtime: .NET Framework 4.8.1 WinForms VSTO add-in
- UI path: `QuickFiler/Controllers/EfcFormController.cs`, `QuickFiler/Controllers/EfcItemController.cs`
- Data source or fixture: n/a

## Steps to Reproduce

1. Reach the post-`Cleanup` state on either controller, where `_globals` and `_themes` are null.
2. Read `DarkMode` or `ActiveTheme`.
3. Observe `NullReferenceException` / `ArgumentNullException` where the QFC twin returns a default.

For the `async void` path: cause any of the listed button handlers to fault and observe that the
rethrow terminates the host rather than being contained.

## Expected Behavior

- Theme and dark-mode accessors return their cached default when their dependencies are null, matching
  the merged QFC implementations.
- A faulted UI event handler logs and contains the error; it does not terminate the Outlook host.
- Fire-and-forget tasks carry an explicit logged error boundary.

## Actual Behavior

**A — missing null guards on the EFC side.**
`EfcFormController.cs:276-283` passes `_globals.Ol` eagerly as a `params object[]` element, so the
getter throws `NullReferenceException` when `_globals` is null. `EfcFormController.cs:257` uses
`strict: true` with `_themes` as the sole dependency, so the getter throws `ArgumentNullException`
(`Initializer.cs:310-321`) when `_themes` is null. `EfcFormController.cs:269` dereferences a null
`_themes`. The already-merged twin guards all three: `QfcFormController.cs:103-105`
(`_themes is null ? _activeTheme : ...`), `:123` (`if (_themes is not null && _themes.TryGetValue(...))`),
`:134` (`_globals?.Ol is null ? _darkMode : ...`).

The same eager-argument shape appears at `EfcItemController.cs:441-448`: `DarkMode`'s getter passes
`_globals.Ol` as a dependency argument, evaluated **before** `Initializer.DependenciesNotNull` can
inspect it, so the post-`Cleanup` state throws instead of returning the intended `false` default.
`DarkMode_Changed` (`:803`) is compile-time-safe via `nameof`, but `:805` (`_globals.Ol.DarkMode`) is not.

**B — `async void` rethrow terminates the host.**
`EfcFormController.cs:424-428`, `:440-444`, `:456-460`, `:516-520`, `:529-533` each perform
`logger.Error(...); throw;` inside an `async void` method. A rethrow from `async void` is posted to the
synchronization context as an unhandled exception.

**C — unobserved fire-and-forget task.** `EfcFormController.cs:97` and `:117` use
`_ = PopulateFolderCombobox()`. `PopulateFolderCombobox` (`:1024-1038`) has no `try`/`catch`, so a
failure inside `InitFolderHandlerAsync` or `FolderHelper.FolderArray` faults an unobserved `Task` and
the folder list silently stays empty. The sibling fire-and-forget at `:853` does carry a logged
boundary in `InitializeBreadcrumbHostAsync` (`:858-868`).

**D — `async void` lambdas in keyboard actions.** `EfcItemController.cs:741`, `:882`, `:887`, `:704`,
`:711`, `:716`. `CharActions` is `KbdActions<char, KaChar, Action<char>>` (`IQfcKeyboardHandler.cs:21`),
so `async (x) => await JumpToAsync(...)` compiles as an `async void` lambda: any fault is raised on the
thread pool and crashes the process rather than surfacing to the caller.

**E — stack-trace-destroying rethrow.** `EfcItemController.cs:777` does `throw (e.InitializationException)`,
rethrowing a captured exception and resetting its stack trace, from inside a WebView2 event handler on
the UI thread.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Code-read evidence recorded above (verified 2026-08-07 against the working tree).

## Impact / Severity

- [ ] Blocker
- [x] High
- [ ] Medium
- [ ] Low

The `async void` rethrows can terminate the Outlook host process. The missing null guards make
post-cleanup property reads throw where the QFC equivalents return safely.

## Suspected Cause / Notes

The QFC controllers received defensive null guards in earlier work; the EFC twins were not updated at
the same time, so the two sides of an otherwise-parallel design have diverged. Using the merged QFC
implementations as the reference is the lowest-risk route.

The `async void` items are a boundary-design problem rather than a regression: `logger.Error(...); throw;`
looks like correct propagation but has no caller to propagate to.

Related open issue: #451 `Bug: efc-home-controller-metrics-inert-duration` covers a separate EFC-family
defect.

Discovered during preparation of issue #452 (epic #136) per-file coverage research. Out of scope there
under that feature's no-behavior-change constraint.

## Proposed Fix / Validation Ideas

- [ ] Port the `QfcFormController.cs:103-105,123,134` guard shapes to the EFC accessors
- [ ] Replace `logger.Error(...); throw;` in `async void` handlers with log-and-contain
- [ ] Give `PopulateFolderCombobox` an explicit logged error boundary
- [ ] Use `ExceptionDispatchInfo.Capture(...).Throw()` or wrap in `InvalidOperationException` at `EfcItemController.cs:777`
- [ ] Unit coverage: post-cleanup property reads; faulting handler does not escape; faulting fire-and-forget logs
- [ ] Manual verification: induced handler failure logs without terminating Outlook

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
