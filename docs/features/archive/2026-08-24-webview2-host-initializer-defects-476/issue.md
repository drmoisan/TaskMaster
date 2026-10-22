# webview2-host-initializer-defects (Issue #476)

- Issue: #476
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/476
- Also closes: #458, #477
- Type: bug
- Work Mode: full-bug
- Epic: quickfiler-bug-family
- Integration branch: epic/quickfiler-bug-family-integration
- Owner: drmoisan
- Last Updated: 2026-08-24T09-50
- Status: Prepared (planning complete; execution deferred to epic-orchestrator)

## Summary

Three pre-existing defect reports in the WebView2 breadcrumb host and the WebView2 core-initializer
seam share one file set, one lifecycle area, and one remediation window. They are delivered as a
single epic child so that the host's subscription lifetime, thread-affinity contract, and
initializer contract are corrected together rather than in three passes over the same code.

| Issue | Title | Severity | Primary file |
| --- | --- | --- | --- |
| #458 | Handler retention across pooled-viewer reuse | Medium | `QuickFiler/Viewers/WebView2BreadcrumbHost.cs` |
| #476 | Unmarshalled SDK call and unsynchronized state publication | High | `QuickFiler/Viewers/WebView2BreadcrumbHost.cs` |
| #477 | `IWebViewCoreInitializer` contract defects | Medium | `QuickFiler/Viewers/IWebViewCoreInitializer.cs`, `QuickFiler/Viewers/WebView2CoreInitializer.cs` |

The authoritative requirement text for each defect is its promoted potential document, which carries
file:line, the offending code block, the root cause, and the suggested fix:

- #458 — `docs/features/potential/promoted/2026-08-07-webview2breadcrumbhost-handler-retention-pooled-viewer.md`
- #476 — `docs/features/potential/promoted/2026-08-07-webview2breadcrumbhost-unmarshalled-sdk-call-and-unsynchronized-state.md`
- #477 — `docs/features/potential/promoted/2026-08-07-iwebviewcoreinitializer-contract-defects.md`

## Defect Summaries

### #458 — constructor-side unhook cannot remove a predecessor's subscription

`WebView2BreadcrumbHost.cs:48-50` performs an unhook-then-hook sequence whose stated intent is
idempotency across pooled-viewer reuse. `OnCoreInitializationCompleted` is an instance method, so
the delegate formed by `-=` in the constructor is bound to the instance under construction, which
has never subscribed. Delegate equality is pairwise `(target, method)`, so the removal matches
nothing. When a second host is constructed over the same Designer-owned `WebView2` control, the
prior host stays subscribed, is retained for the control's lifetime, and initialization completion
is handled more than once. The same pattern recurs at `:131-132` for `core.WebMessageReceived`.

### #476 — unmarshalled SDK access and unsynchronized state publication

`PostMessageJson` (`:72-84`) reads `_control.CoreWebView2` and calls `PostWebMessageAsJson` on
whatever thread invoked it; `NavigateToString` (`:66-69`) has the same shape. WebView2 controls
require the UI (STA) thread, a requirement this same file states at `:105` and honours in
`InitializeAsync`. `IsCoreInitialized` (`:54`) is a plain auto-property written on the UI thread at
`:134` and read from other threads, with no barrier guaranteeing that a reader observes either the
flag or the `core.WebMessageReceived` subscription at `:131-132` that is intended to precede it.

### #477 — false contract claim, hard-coded SDK argument, and absent argument validation

`IWebViewCoreInitializer` documents itself, and `WebView2CoreInitializer` justifies its coverage
exemption, on the claim of a 1:1 forward to the WebView2 SDK. `CreateEnvironmentAsync` drops the
SDK's `browserExecutableFolder` parameter and passes `null` unconditionally, pinning every caller to
the Evergreen runtime; the claim is therefore false. Neither member validates any argument, so a
null `control` produces a bare `NullReferenceException` with no parameter name, against the
convention of every sibling seam and against CLAUDE.md §C#4.

## Scope

Production files this feature owns and may write:

- `QuickFiler/Viewers/WebView2BreadcrumbHost.cs`
- `QuickFiler/Viewers/WebView2CoreInitializer.cs`
- `QuickFiler/Viewers/IWebViewCoreInitializer.cs`

Production files this feature must NOT write (owned by concurrent sibling epic children):

- `QuickFiler/Viewers/WebView2Messenger.cs` and `QuickFiler/Viewers/BreadcrumbMessengerHub.cs` (siblings 501, 488)
- `QuickFiler/Viewers/BreadcrumbPopupUiOperations.cs` and `QuickFiler/Viewers/BreadcrumbBridgeCoordinator.cs` (siblings 501, 488)
- `QuickFiler/Controllers/EfcFormController.cs` (feature 464) — the sole construction site of
  `WebView2BreadcrumbHost`. Any remediation that would require editing it is out of scope and is
  recorded in `spec.md` as a cross-feature note instead.

Test files are writable. `QuickFiler.Test/Controllers/WebView2CoreInitializerTests.cs` already
carries a `Compile Include` entry and is the preferred home for new test methods.

## Acceptance Criteria

For work mode `full-bug` the authoritative acceptance-criteria source is `spec.md`. This section is
a non-authoritative pointer retained for readers who open `issue.md` first.

See `spec.md` § Acceptance Criteria.

## Out of Scope

- Any change to a file on the forbidden list above.
- Adopting a fixed-version WebView2 distribution. Whether that is a product requirement is recorded
  as UNCONFIRMED in the #477 potential document's Next Step and is not resolved by this feature.
- Removing the COM/WinForms coverage exemption from host-bound WebView2 SDK calls that have no
  injectable seam. Any testable seam this feature introduces is not covered by that exemption.

## References

- Issue #455 — F13 breadcrumb drop-down and WebView2 host coverage, where all three defects were found.
- Issue #136 — parent epic of #455; its no-behaviour-change NFR is why these were deferred to their own issues.
- Issue #349 — the change that introduced the WebView2 breadcrumb control.
- Issue #432 — F1 coverage ledger; the ratified exemption rationale referenced by #477.
