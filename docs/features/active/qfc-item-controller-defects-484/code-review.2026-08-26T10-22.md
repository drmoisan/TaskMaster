# Code Review — qfc-item-controller-defects (Issue #484)

- Reviewer: feature-review agent
- Timestamp: 2026-08-26T10-22
- Branch: `bug/qfc-item-controller-defects-484` @ `4f2b55f1`, reviewed against merge-base `61edc19b`
- Scope: four production partials of `QfcItemController` and five test files (full branch diff)

## Review emphasis verification (concurrency, ordering, lifecycle)

Each item below was verified directly in the delivered source, not from evidence prose.

1. **Teardown ordering in `Cleanup()`** (`ViewerSetup.cs:447-482`) — correct.
   Order as delivered: breadcrumb `BreadcrumbUnhandledArrow -=` before `_breadcrumbViewer = null`;
   `UnwireEvents()` before `_itemViewer = null` (first nulling at `:460`) and before
   `_kbdHandler = null` (`:472`); `_emailIsReadTimer?.Dispose()` at `:478` strictly before
   `_emailIsReadTimer = null` at `:479`; `_mailActions = null` last. No dispose-after-null and no
   unwire-after-release exists.

2. **Wire/unwire symmetry** — exact.
   `WireIntentEvents()` attaches 16 subscriptions; `UnwireIntentEvents()` detaches the same 16
   events with delegates equal by target+method (re-formed wrappers for `SearchTextChanged`,
   `FolderKeyDown`, `ConversationItemSelectionChanged` compare equal to the wrappers attached, so
   each `-=` removes). `WireControlTreeEvents()` makes 6 subscription statements
   (`PreviewKeyDown`/`KeyDown` per control via `ForAllControls`, `MouseEnter`/`MouseLeave` on
   `Buttons` and on `MenuItems`); `UnwireControlTreeEvents()` mirrors all 6 and passes the identical
   `ForAllControls` exclusion list (`L0vhBreadcrumb_WebView2`). The 17th subscription
   (`WebResourceRequested`, made outside the wire methods) is detached via the captured
   field pair in `DetachWebResourceRequestedHandler()`, using the same delegate instance that was
   attached, so no re-formed-delegate miss is possible. Counted sets match exactly; no leak found.

3. **Cancellation placement (#483)** — correct.
   `Token.ThrowIfCancellationRequested()` is the first executable statement of `MoveMailAsync`
   (outside the `try`, so the broad catch cannot swallow or re-wrap cancellation), of
   `FlagAsTaskAsync`, and of `EnumerateConversationAsync`. No other `Token` use exists inside the
   `try`, so an `OperationCanceledException` cannot be converted into the wrapped
   `InvalidOperationException`. The checks precede all cancellable work (packaging, factory call,
   enqueue, dispatch), so cancellation is observable where it matters.

4. **#483 error path propagates** — correct.
   The catch logs at error level, invokes `NotifyMoveFailure`, then throws
   `InvalidOperationException` with the original fault as `InnerException`; it cannot return
   normally. The caller `QfcCollectionController.TryMoveEmailByGroupAsync` (unmodified) already
   catches and continues, so the bulk loop is preserved while per-item failure is no longer
   reported as success.

5. **Notifier dispatcher marshalling cannot self-deadlock** — verified.
   `NotifyMoveFailure` snapshots both the notifier and `_uiDispatcher` into locals before use
   (race-safe against concurrent `Cleanup`). The production dispatcher (`WpfUiDispatcher.Invoke`
   forwarding to WPF `Dispatcher.Invoke`) executes inline when already on the dispatcher thread, so
   a same-thread call cannot deadlock. A deadlock would require the UI thread to block synchronously
   on `MoveMailAsync`'s task; the in-repo call path is fully async (`await` throughout). See F3 for
   the residual observation.

6. **No real WinForms message pump in tests; no banned wait APIs** — verified.
   Exactly one new test constructs a real `ItemViewer`; it starts no pump, calls no `Show()`, and
   restores `SynchronizationContext` in `finally`. Regex scan of all added test lines for
   `Thread.Sleep`, `Task.Delay`, `DateTime.Now`, `Stopwatch`, `DoEvents`, `Application.Run`, and
   temp-file APIs: zero matches.

7. **Public surface and interfaces** — verified. No public member added or removed on the four
   partials (additions are `internal`/`private`); `IQfcItemController.cs` and `IItemViewer.cs` are
   not in the diff (byte-identical); the retained `ToggleNavigation(bool)` and its
   `(bool, ToggleState)` overload are unchanged in signature.

8. **No new `[ExcludeFromCodeCoverage]`** — verified; zero occurrences in the branch diff.

## Findings

| ID | Severity | Blocking | File:Line | Finding |
|---|---|---|---|---|
| F1 | Major | No | `QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs:321-336` | Residual TOCTOU race in `ApplyReadEmailFormat`. The #484 guard checks `ItemHelper`, `_themes`, `_activeTheme`, `_mailActions` for null and then re-reads the same fields to dereference them. `Timer.Dispose()` (parameterless) does not wait for an in-flight callback, so `Cleanup()` on another thread can null these fields between the guard and the use, leaving a narrow window for the same NRE class this fix targets — and an unhandled exception in a `System.Threading.Timer` callback terminates the host process. Recommendation: snapshot the four fields into locals, guard the locals, and use only the locals (or dispose via `Timer.Dispose(WaitHandle)`). Non-blocking: the spec AC required exactly the early-return guard delivered, the window is orders of magnitude narrower than the unconditional pre-fix NRE, and T2 covers the primary post-`Cleanup` path. |
| F2 | Minor | No | `QuickFiler/Controllers/QfcItemController.EventWiring.cs:404-419` | `UnwireControlTreeEvents` detaches keyboard handlers by re-forming delegates from the *current* `_kbdHandler`. If `_kbdHandler` were ever reassigned between wire and unwire, every `-=` would silently miss (delegate target differs) and the walk would leak all control-tree keyboard subscriptions. Today `_kbdHandler` is assigned once per controller lifetime, so this is latent, not live. Recommendation: capture the handler delegates into fields at wire time (the pattern already used for `_webResourceRequestedHandler`) if `_kbdHandler` ever becomes reassignable. |
| F3 | Info | No | `QuickFiler/Controllers/QfcItemController.MailActions.cs:36-46` | `NotifyMoveFailure` uses synchronous `IUiDispatcher.Invoke`, so a background move-failure blocks its worker thread for the full lifetime of the modal `MessageBox` in production. This preserves the pre-existing synchronous `MessageBox.Show` behavior and is intentional; noted so a future throughput change is deliberate. |
| F4 | Info | No | `QuickFiler/Controllers/QfcItemController.MailActions.cs:117-124` | The OneDrive-missing path in `MoveMailAsync` still returns normally after a debug-level log — a silent per-item skip that reports success to the bulk loop. Pre-existing behavior, outside the five in-scope defects (#483 covered the catch block, not this branch). Candidate for promotion to a follow-up issue if silent skips matter operationally. |
| F5 | Info | No | `QuickFiler/Controllers/QfcItemController.FocusAndTheme.cs:333` | `_themes[_activeTheme]` can throw `KeyNotFoundException` if the active theme key is absent from the dictionary. Pre-existing (unchanged by this feature); the new null guard does not claim to cover it. |

Blocking findings: **0**.

## Code quality assessment

- **Design**: The three fixes that needed seams got real seams (`MoveFailureNotifier`,
  `TryResolveCidResource`, captured handler fields) rather than test hooks bolted onto framework
  types. The pure decision half of the WebView2 handler is now fully unit-tested with plain values,
  which is the correct separation under the repository's I/O-isolation rule.
- **Deliberate asymmetry of the unwire guards** is well-reasoned and documented in-code: wiring runs
  only on the fully initialized path, teardown must tolerate partial construction. The
  `Cleanup_WithNullKeyboardHandlerAndNonItemViewerViewer_DoesNotThrow` test pins this contract.
- **Comments** consistently explain why (issue numbers, race rationale, UriKind choice, delegate
  identity), matching policy.
- **Tests**: AAA throughout, one behavior per test, `VerifyRemove` with `Times.Once()` for each of
  the 16 intent detachments, DataRow-per-case for the URI guards, and helper extraction into
  `TestSupport` keeps every file under the 500-line cap (max 499). The C2 rule 6 namespace hazard in
  `MailActionsTests` (Outlook `Action`/`Exception` shadowing) is respected — all `System` types are
  fully qualified there.
- **File-size pressure**: `ViewerSetup.cs` (499), `EventWiringTests.cs` (499), `ViewerSetupTests.cs`
  (498), `MailActionsTests.cs` (498) are at or near the cap. Any future addition to these files will
  require extraction first; flagged for planning awareness, not a defect.

## Verdict

Zero blocking findings. One Major non-blocking robustness finding (F1) with a concrete
recommendation; remaining findings are Minor/Info. The delivered code is merge-ready from a code
quality standpoint.
