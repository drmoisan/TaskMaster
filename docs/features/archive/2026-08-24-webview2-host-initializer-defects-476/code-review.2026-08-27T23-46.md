# Code Review — webview2-host-initializer-defects-476

- **Branch:** `bug/webview2-host-initializer-defects-476-exec` at `d1dcabd6caa960a68899e28ed9a282eaca6ffd5e`
- **Base:** `origin/epic/quickfiler-bug-family-integration` (`69e83171`)
- **Timestamp:** 2026-08-27T23-46
- **Verdict:** 0 Blocking, 1 Non-blocking, 4 Advisory

## Scope Reviewed

Full read of the three production files (`QuickFiler/Viewers/WebView2BreadcrumbHost.cs`, `WebView2CoreInitializer.cs`, `IWebViewCoreInitializer.cs`) and the three test files at HEAD, plus the `QuickFiler.Test.csproj` hunk, against the base diff.

## Design Assessment

The change is well-constructed for its constraints:

- **#458 owner registry.** `ConditionalWeakTable<WebView2, WebView2BreadcrumbHost>` keyed on control identity, with an explicit gate around the compound lookup-detach-replace, correctly replaces the dead constructor-side `-=` (which could never match a predecessor's delegate under pairwise `(target, method)` equality). Detach runs on the predecessor instance — the only instance whose delegate can match. Only `TryGetValue`/`Add`/`Remove` are used, respecting the net481 API-surface caution. Entries are collectible with the control.
- **#476 defect 1 marshalling.** Each of `NavigateToString` and `PostMessageJson` routes its SDK touch through a single fire-and-forget `BreadcrumbUiDispatcher.Dispatch` callback; `PostMessageJson` keeps the `CoreWebView2` read, null guard, log-and-drop, and post in one unit of work so the read cannot be split across dispatch hops. The documented pre-`InitializeAsync` inline window matches spec residual-risk item 2 exactly.
- **#476 defect 2 publication.** `IsCoreInitialized` is an explicit field with `Volatile.Read` acquire / `Volatile.Write` release, and the release store sits strictly after the `core.WebMessageReceived` subscription and strictly before the `CoreInitialized` raise, with an in-code comment forbidding reorder. Correct minimal fix for the visibility defect.
- **#477 contract.** The false 1:1-forward claim is gone from both the interface and the implementation; the Evergreen-only `browserExecutableFolder` decision is documented on the contract with its cost; argument guards run before any SDK call, and the deliberate non-guards (`options`, `environment`) carry evidence-based rationales.
- **Coverage exemptions** moved from class level to exactly six genuinely host-bound members, enforced by reflection tests so drift will fail the suite.

## Findings

| ID | Severity | Location | Finding |
|---|---|---|---|
| CR-1 | Non-blocking | `WebView2BreadcrumbHost.DetachCore` (`:308-321`) and constructor (`:113`) | A detached predecessor's `_control.Disposed += OnControlDisposed` subscription (line 113) is never removed: `DetachCore` unsubscribes `CoreWebView2InitializationCompleted` and (guarded) `core.WebMessageReceived`, but not `Disposed`. A superseded host therefore remains reachable from the control through the `Disposed` invocation list until control disposal. This is behaviorally harmless — `OnControlDisposed` is idempotent and its registry removal is `ReferenceEquals`-guarded — but it retains the predecessor object for the control's lifetime, and spec residual-risk item 3's claim that "the `control -> stale host` edge is removed" is overstated by exactly this edge. In current production wiring the two-hosts-over-one-live-control case does not occur (spec Premise correction 1), which is why this is not blocking. Recommend a one-line fix in a follow-up: add `_control.Disposed -= OnControlDisposed;` to `DetachCore`, with a matching ownership-test assertion. Candidate for defect promotion at epic close. |
| CR-2 | Advisory | `WebView2BreadcrumbHost.InitializeAsync` (`:242-243`) | The `ArgumentNullException` guard for a null `uiSyncContext` is ordinary testable logic with no covering test (the member sits at exactly 90.00%). A negative-flow test (`InitializeAsync(null)` asserting exception type and `ParamName`) is cheap and closes the only non-host-bound uncovered pair in the change. |
| CR-3 | Advisory | `WebView2BreadcrumbHost.LogDispatchFailure` (`:277-280`) | 0% covered (3 lines). It is reachable in-process: install the V1 dispatcher via `InitializeAsync` with a mocked seam, then trigger a dispatched SDK forward that throws (the wrapper throws `InvalidOperationException` on an uninitialized control, as the [P2-T2] red run demonstrated) and assert the error sink logged. Not enumerated by the plan's gate; recommended as a follow-up test. |
| CR-4 | Advisory | `WebView2BreadcrumbHostTests.cs:428`, `BreadcrumbUiThreadDispatchTests.cs:361` | `RecordingSynchronizationContext` is now duplicated as a private nested helper in two test files. Extract one copy to `QuickFiler.Test/TestSupport/` when either file is next touched. |
| CR-5 | Advisory | `evidence/qa-gates/coverage-delta.2026-08-27T23-20.md`, section "Why the four shortfalls were recorded" | The evidence claim that covering every short line "requires the external Evergreen runtime" is overstated for host line 162: the [P2-T2] red run executed `_control.NavigateToString` in-process and received the wrapper's `InvalidOperationException` without any runtime. The conclusion survives (such a test pins third-party wrapper throw behavior and cannot reach the 90% floor because line 163 stays unreachable), but the basis as written is broader than the facts. Correcting the record here per the verify-the-asserted-mechanism practice; no evidence file was modified. |

## Standards Conformance

- Naming, XML documentation, and comment discipline (why-not-what) are consistently strong; the reorder-forbidding comment on the publication sequence and the delegate-equality rationale on the registry are exactly the load-bearing kind.
- Nullable participation is correct per repo convention: `#nullable enable` in the host file with zero CS86xx under the gate; the initializer pair deliberately stays out of pragma scope and expresses nullability through runtime guards, as the spec requires.
- Formatting is CSharpier-clean repo-wide at HEAD (gate evidence, EXIT_CODE 0).
- No public API break: the public two-argument constructor signature and both interface member signatures are unchanged; all eleven pre-existing Moq mock sites pass unmodified (6734/6734 green).
