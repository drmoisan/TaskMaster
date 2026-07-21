# Code Review — store-lockup-detect-notify (F4, Issue #264)

- Timestamp: 2026-07-08T09-19
- Reviewer: feature-review
- Branch: `feature/store-lockup-detect-notify-264`
- Base: `epic/store-lockup-resilience-integration` (merge-base `6a525937`)
- Implementation commit: `e0b58302`

## Executive Summary

The F4 code is well-factored around the repository's coverable-decision / thin-host split. Pure
logic (`LockupStallDecider`, `StoreLockupAttribution`), the host-neutral orchestrator
(`StoreLockupResponder`), and the ambient-context holder (`CurrentStoreContext`) are all COM-free
and unit-tested; host-bound WinForms and watchdog-loop code is isolated behind
`[ExcludeFromCodeCoverage]` shells. The set/clear attribution wraps are minimal, additive, and reuse
already-cached identity strings. All execution-critical invariants supplied by the caller were
verified in code. No blocking findings; three non-blocking observations are recorded for
maintainability.

## Findings Table

| Severity | File | Location | Finding | Recommendation | Rationale | Evidence |
|---|---|---|---|---|---|---|
| Info | UtilitiesCS/Threading/StoreLockupResponder.cs | Line 127 | The Reenable button action is fire-and-forget: `() => _ = _disableService.ReenableAsync(identity)` discards the returned `Task`, so a fault in `ReenableAsync` is unobserved. | Optional: attach a fault continuation that logs on `ReenableAsync` failure, or document the fire-and-forget intent inline. | Consistent with the modeless, non-blocking design (F1 owns rehook orchestration), but a swallowed exception in the reenable path would be silent to the user who clicked the button. | StoreLockupResponder.cs:122–129 |
| Info | UtilitiesCS/Threading/ThreadMonitor.cs | Lines 148, 207 | Two `Thread.Sleep` calls remain in the diagnostic stack-capture path (`PingAndAwaitDiagnosticWindow`, `GetStackTrace`). | No action required. | Pre-existing diagnostic-only code inside `[ExcludeFromCodeCoverage]` host-bound methods, gated behind `delayThreshold` and off the auto-disable/notify path; F4 removed the former polling-loop `Thread.Sleep` in favor of a clock-driven `ITimer`. | `e0b58302~1` vs `e0b58302` diff; ThreadMonitor.cs:135–160, 197–237 |
| Info | UtilitiesCS.Test/UtilitiesCS.Test.csproj | packages.config | Adds test-only dependency `Microsoft.Extensions.TimeProvider.Testing` 9.0.0. | No action required. | Already-approved in-repo test package (mirrors existing `QuickFiler.Test` wiring); production dependency set is unchanged. | UtilitiesCS.Test.csproj:540–542; packages.config diff |

## Design Assessment

### Separation of concerns (PASS)

- `LockupStallDecider` is a pure boundary decision (`elapsedMs >= threshold`) with an explicit
  boundary contract (>= confirms; strictly below does not), following the repository's
  `StartupLifetimeStopDecider` split. Not marked `[ExcludeFromCodeCoverage]` — it is the coverable
  seam (LockupStallDecider.cs:70–82).
- `StoreLockupAttribution.FormatLine` is a pure, culture-invariant formatter with no log4net/COM/clock
  dependency, mirroring `StoreFilterAttribution.FormatLine` (StoreLockupAttribution.cs:25–36).
- `StoreLockupResponder` composes only interface/delegate seams (`IStoreDisableService`,
  `IUiDispatcher`, `StoreLockupNotifier`, `Action<string>` sink), keeping it Moq-testable without
  Outlook (StoreLockupResponder.cs:59–71).
- `ThreadMonitor` keeps its infinite polling loop and stack-capture in thin `[ExcludeFromCodeCoverage]`
  shells and exposes the deterministic `EvaluatePoll` attribution seam (ThreadMonitor.cs:173–194).

### Attribution mechanism (PASS)

- `CurrentStoreContext` is a single-writer/single-reader static `volatile string`, with a documented
  rationale for rejecting `AsyncLocal` (does not flow to the watchdog's independent background
  thread). `Begin` normalizes null/whitespace/`<unavailable>` to "no context" and restores the prior
  value on dispose, tolerating nested and sequential scopes (CurrentStoreContext.cs:21–88).
- The three set/clear sites wrap only the post-`DisplayName` blocking COM chain using the
  already-cached identity string, leaving the `[Startup timing]` / `[loadinboxes]` diagnostic lines
  unchanged (StoreWrapper.cs Init wrap; StoresWrapper.cs RewireOlObjectsAsync wrap; AppOlObjects.cs
  `EmitPerStoreInboxAttribution` wrap). No new COM property read is introduced.

### Auto-disable-then-notify sequence (PASS)

`OnLockupDetected` enforces, in order: no-context guard -> unresolved-identity guard -> already-disabled
guard -> `DisableSessionOnly` -> one WARN line -> `BeginInvoke` of the modeless notify. The notify hop
uses `BeginInvoke` (fire-and-forget), never `Invoke`, never modal `ShowDialog`
(StoreLockupResponder.cs:80–130).

### Modeless notification (PASS)

`MyBoxModeless` constructs the viewer without a `using` block, owns lifetime via a `FormClosed`
disposal handler, and shows through an injectable `showAction` defaulting to `viewer => viewer.Show()`.
The three buttons map to `DisableSessionOnly` / `DisableForFutureSessions` / `ReenableAsync`; F4 makes
no direct F3 call (MyBoxModeless.cs:63–116).

### net48 value-type constraint (PASS)

`LockupAttribution` is a plain `readonly struct` with an ordinary constructor and get-only properties,
with an explicit comment explaining that `init`/`record struct` are avoided because
`IsExternalInit` is unavailable on the net48 target (LockupStallDecider.cs:11–40). Consistent with
the repository's documented net48 constraint.

### Test quality (PASS)

- Deterministic: `FakeTimeProvider` for all timing; synchronous pass-through `IUiDispatcher` mock;
  strict mocks used for the no-context negative path to prove zero downstream calls.
- Clear Arrange-Act-Assert structure and descriptive method names.
- Boundary coverage: threshold reached exactly (fires once), 1 ms below (does not fire), continued
  stall (no duplicate), responsive reset (re-fires). Exact WARN string asserted.
- Button routing asserted by invoking each `ActionButton.Delegate` against Moq without a real window.

## AC-Invariant Cross-Check

| Caller invariant | Verdict | Evidence |
|---|---|---|
| 1. COM/STA safety, cached-identity-only attribution, no new UI-thread COM reads | PASS | CurrentStoreContext volatile holder; wraps reuse cached DisplayName; EvaluatePoll reads in-memory field only |
| 2. Modeless notify: no `using`, dispatched via `IUiDispatcher.BeginInvoke` | PASS | MyBoxModeless.cs:71–83; StoreLockupResponder.cs:122 |
| 3. Calls F1 `IStoreDisableService` (`StoreDisable`), not F3 directly | PASS | ThisAddIn.cs `_globals?.StoreDisable`; responder calls `ReenableAsync` (F1 sequences F3) |
| 4. net48 value types are plain `readonly struct` | PASS | LockupAttribution readonly struct |
| 5. Injected clock, no Thread.Sleep/Task.Delay/temp files in tests | PASS | FakeTimeProvider; banned-API test scan clean |
| 6. Disable-then-notify order; guards; one WARN `[store-lockup]` line | PASS | StoreLockupResponder.OnLockupDetected; StoreLockupResponderTests order/guard/log assertions |
| 7. All files <= 500 lines | PASS | Max 472 (AppOlObjects.cs) |

blocking_count (code-review): 0
