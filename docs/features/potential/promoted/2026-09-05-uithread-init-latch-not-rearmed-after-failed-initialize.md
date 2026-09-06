# uithread-init-latch-not-rearmed-after-failed-initialize (Issue #788)

- Date captured: 2026-09-05
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/uithread-init-latch-not-rearmed-after-failed-initialize/ (Issue #788)

> Automation note: Keep the section headings below unchanged; the promotion tooling maps each of them into the GitHub bug issue template.

- Issue: #788
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/788
- Last Updated: 2026-09-06
## Summary

`UtilitiesCS.UiThread.Init()` consumes its single-shot latch before `Initialize()` runs, so an `Initialize()` that throws leaves the latch permanently consumed and no later caller can retry. The remedy the exception message names, calling `Init()`, is unreachable once the first attempt has failed.

The obvious fix is unsound as written and must not be applied naively. Issue #782 attempted it, measured a reproducible regression, and withdrew it. This entry carries the finding forward together with the measurement that rules out the naive form, so a future attempt does not repeat it.

## Environment

- OS/version: Windows 11 Pro 10.0.26200
- Runtime: .NET Framework 4.8, VSTO add-in hosted by Outlook desktop
- Command/flags used: `vstest.console.exe <test assemblies> /InIsolation`
- Data source or fixture: `UtilitiesCS.Test`, `TaskMaster.Test`

## Steps to Reproduce

1. Arrange for `UiThread.Initialize()` to throw on its first invocation. In a headless or non-STA context this happens naturally, because `Initialize()` constructs a WinForms `SyncContextForm` and calls `Show()` on it.
2. Call `UtilitiesCS.UiThread.Init()`. Observe the exception propagate.
3. Correct the condition that made `Initialize()` fail, then call `UiThread.Init()` again.

## Expected Behavior

The second call retries `Initialize()` and succeeds, because the first attempt never completed and therefore should not have counted as the single shot.

## Actual Behavior

The second call is a no-op. `UiThread.cs:36` reads `if (_loaded.CheckAndSetFirstCall)`, which consumes the latch before `Initialize()` is attempted, so the failed first call has already spent it. Every later `Init()` returns without doing anything, and `UiThread.Dispatcher` continues to throw its not-initialized exception naming `Init()` as the remedy.

## Logs / Screenshots

- [ ] Attached minimal logs or screenshot
- Snippet: not applicable. The defect is an ordering property of the latch read, established by reading `UiThread.cs:36-51`.

## Impact / Severity

- [ ] Blocker
- [ ] High
- [ ] Medium
- [x] Low

Low because no production path currently fails `Initialize()`: `TaskMaster/ThisAddIn.cs:35-40` is the only direct production caller and runs on the Outlook STA during startup, where the WinForms construction succeeds. The defect is a latent recoverability gap rather than an observed production failure.

## Suspected Cause / Notes

- `UtilitiesCS/Threading/UiThread.cs:36` consumes the latch with `CheckAndSetFirstCall` before the guarded body runs.
- `UtilitiesCS/Threading/UiThread.cs:59-90` is the guarded body. It constructs a `SyncContextForm`, calls `Show()`, captures the UI variables, and hides the form again.

**The naive fix is measurably unsound. Do not simply re-arm the latch in a catch.**

Issue #782 implemented exactly that — a `catch` around `Initialize()` that assigns a fresh `ThreadSafeSingleShotGuard` to `_loaded` and rethrows — and it caused a reproducible test regression. `UtilitiesCS.Test.Extensions.DictionaryExtensions_Tests.TryAddValuesAsync_UpdatesExistingValue` failed with a 21-second duration. The failure was bisected to the single re-arm line: with it, `UtilitiesCS.Test` plus `TaskMaster.Test` returned 5179/5180; without it, 5180/5180. The branch base returned 6992/6992 both before and after the failing runs, so this was not the pre-existing flake tracked as issue #780.

The mechanism is the interaction with the two lazy accessors. `UiThread.cs:128-131` (`UiSyncContext`) and `UiThread.cs:194-197` (`AutoScaleFactor`) each call `Init()` when their backing field is null. Without the re-arm, the latch stays consumed after a first failure and every later `Init()` is a cheap no-op. With the re-arm, every subsequent read of either accessor retries the WinForms construction and throws again, which starves the thread pool and defeats the 500 ms `CancelAfter` at `UtilitiesCS/Extensions/DictionaryExtensions.cs:177`. `TaskMaster/AppGlobals/AppOlObjects.cs:367` and `TaskMaster/ThisAddIn.cs:114` are the readers that make `TaskMaster.Test` the assembly where this surfaces.

The file's own documentation already records the underlying tension: the XML `<remarks>` on `UiThread.Dispatcher` states that initialization has UI-thread affinity and that a lazy `Init()` from an arbitrary reader is deliberately avoided for that property, while the two sibling accessors still self-heal.

## Proposed Fix / Validation Ideas

- [x] Unit coverage areas: latch state after a failed `Initialize()`, retry success on a subsequent call, and the cost of a repeated failed `Init()` from each lazy accessor.
- [x] Integration scenario to retest: the full nine-assembly suite, specifically `DictionaryExtensions_Tests.TryAddValuesAsync_UpdatesExistingValue`, which is the canary for thread-pool starvation.
- [ ] Manual verification notes: none required.

A sound fix must make repeated failure cheap, not merely retryable. Candidate approaches, none yet chosen:

- Remove the implicit `Init()` from the `UiSyncContext` and `AutoScaleFactor` getters so that only an explicit caller can trigger initialization, which is the contract `UiThread.Dispatcher` already has. This is the largest change and the most likely to be correct.
- Make `Initialize()` cheap on the failure path, so that a retry does not reconstruct and show a WinForms form before discovering it cannot.
- Separate the retry affordance from the accessors: keep the latch single-shot for the lazy paths and expose an explicit reset that only host startup calls.

Whichever is chosen, the acceptance criteria must include a full-suite run and an explicit assertion on `TryAddValuesAsync_UpdatesExistingValue`, because that test is what detected the regression and a scoped run over `UtilitiesCS.Test` alone did not.

Related: `uithread-init-accepts-non-sta-callers`, promoted as issue #787, proposes checking apartment state before the latch is consumed. That check is compatible with this entry and does not resolve it: rejecting a non-STA caller before the latch is read is a different property from re-arming the latch after `Initialize()` itself fails.

## Next Step

- [ ] Promote to GitHub issue (bug-report template)
- [ ] Move to active fix folder / branch
