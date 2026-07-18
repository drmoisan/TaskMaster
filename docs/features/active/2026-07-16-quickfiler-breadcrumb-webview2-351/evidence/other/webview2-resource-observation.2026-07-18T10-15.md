# Second-WebView2-Per-Pooled-Viewer Resource Observation (P6-T4) — STRUCTURAL-IMPOSSIBILITY DOSSIER

Timestamp: 2026-07-18T10-15

WhyRuntimeCaptureImpossible: Memory-delta and breadcrumb-init-latency figures across the
`QuickFiler/Helper Classes/ItemViewerQueue.cs` pool require a live Outlook process hosting the
add-in with real Chromium-backed WebView2 instances; none exists in this execution environment.
No observed figures are recorded; none are fabricated.

Observed figures: NONE (structurally impossible in this environment).

Cost-mitigation facts pinned in code (relevant to the eventual observation):
- Environment reuse (G7): the breadcrumb WebView2 is initialized through the SAME injected
  `IWebViewCoreInitializer` and the SAME `CoreWebView2Environment` + options object already
  created for the message-body pane (`QfcItemController.ViewerSetup.cs`, breadcrumb block after
  the cid-image handler), so both controls share one user-data folder and browser process pool —
  no second environment negotiation, mitigating per-viewer process count.
- The host-neutral pipeline (`ItemViewer.Breadcrumb.cs` `InitializeBreadcrumbPipeline` +
  `BreadcrumbMessengerRelay`) is created without any WebView2 resources; population, selection,
  and filing correctness never wait on the breadcrumb WebView2 init (buffered posts flush on
  attach), so init latency does not sit on the filing critical path.
- The page is a single static in-memory `NavigateToString` string (`Resources/FolderBreadcrumb.html`,
  244 lines, no external fetches), minimizing per-init work.

DECISION: EAGER-RETAINED-PENDING-RUNTIME-OBSERVATION — the implementation keeps the plan's
eager breadcrumb initialization inside `InitializeWebViewAsync` (which is itself invoked
fire-and-forget off the UI init path). If the maintainer's runtime observation finds the memory
delta or init latency prohibitive, the sanctioned response is to file the lazy-initialization
fallback (initialize the breadcrumb WebView2 only when a pooled viewer becomes active) as a
follow-up issue rather than widening this feature's scope, exactly as the spec's Constraints &
Risks section prescribes.

MANUAL-VERIFICATION-REQUIRED: yes — the maintainer must observe, in the live add-in: (1) the
per-pooled-viewer memory delta with the second WebView2 attached, and (2) the breadcrumb init
latency across the ItemViewerQueue pool, then either accept the eager decision or file the
lazy-initialization follow-up issue.
