# Maintainer Decision — Issue #230 (Reduced Exemption Boundary Ratification)

- **Date:** 2026-08-08
- **Decision owner:** Dan Moisan (project maintainer)
- **Decision:** RATIFIED. The 11-member `[ExcludeFromCodeCoverage]` boundary
  (`evidence/qa-gates/exclusion-census-post.2026-08-07T23-30.md`) is accepted.
- **Status:** Ratified.
- **Supersedes:** the 19-member boundary ratified 2026-07-02 under issue #227
  (`docs/features/archive/2026-06-29-qfc-item-controller-testability-227/maintainer-decision.2026-07-02.md`).
- **Delivered by:** PR #479, head `8d670022b06e30156b5a4f92bc3f6ee581d36802`.

## Context

The 2026-07-02 ratification under #227 accepted a 19-member residual boundary, of which **9 members**
were blocked solely by the absence of a WinForms `Application.Run()`-on-background-thread test seam —
the analogue of the WPF `Dispatcher.Run()` pump the repository already had for `IUiDispatcher`. That
decision recorded the 9 as an infrastructure gap rather than a genuine testability barrier, and
deferred the seam to a separate initiative. Issue #230 is that initiative.

PR #479 builds the seam (`QuickFiler.Test/TestSupport/WinFormsPumpHost.cs`) and applies it, reducing
the boundary from 19 to 11.

## Decision and rationale

The maintainer ratifies the 11-member residual boundary. Composition of the reduction:

- **8 of the 9** infrastructure-blocked members are de-exempted and now covered by real, deterministic
  tests running against a live WinForms message pump. Each attribute was removed in the same change as
  the test that covers it. Per-member coverage ranges 83.33%–100.00%, aggregate 159/171 = 92.98%.
- **1 of the 9 — `InitializeWebViewAsync` — remains exempt.** It cannot be de-exempted by the pump seam
  alone: after the mocked `IWebViewCoreInitializer` calls,
  `((ItemViewer)_itemViewer).L0v2h2_WebView2.CoreWebView2` is null without the real WebView2 Runtime,
  which this repository's External Dependencies unit-test rule bars. Issue #230 itself flagged this
  member's residual concrete-`ItemViewer` accessor barrier as tracked separately. This is the same
  category of genuine external-process dependency as the `WebView2CoreInitializer` adapter body already
  accepted in the 2026-07-02 decision.

Residual composition of the ratified 11:

- 6 members — `async void` WinForms-event-handler signature shells whose substantive logic is extracted
  and tested via `*Core` methods (unchanged from the 2026-07-02 rationale).
- 3 members — deliberate `virtual` test-seam methods where the override point IS the test seam by design.
- 1 member — `WebView2CoreInitializer`'s adapter body (genuine external-process dependency).
- 1 member — `InitializeWebViewAsync`, per the rationale above.

## Census drift disclosed at ratification

The controller partials carried **19** `[ExcludeFromCodeCoverage]` sites at the time of this work, but
the 2026-07-02 decision ratified an **18**-member boundary. The discrepancy is `EnsureBreadcrumbPipeline`
(`QuickFiler/Controllers/QfcItemController.ViewerSetup.cs`), added by issue #351 *after* the 2026-07-02
ratification and therefore never covered by it.

This is disclosed rather than smoothed over: the ratified count of 11 is measured against the actual
19 sites present on the branch, not against the historical 18. `EnsureBreadcrumbPipeline` was out of
scope for #230 and is unchanged by PR #479. It remains within the ratified 11.

## Structural coverage limits accepted

`CreateAsync` and `InitializeAsync` have a terminal statement that is structurally unreachable under
unit-test conditions (the tail after `InitializeWebViewAsync`, which faults behind the mocked WebView2
seam). Their per-member coverage is therefore partial by construction rather than incomplete by neglect.
The maintainer accepts this; the applicable gate is "> 0%", not 100%.

## Verification supporting this decision

| Gate | Result |
|---|---|
| Exemption census | 19 → 11 (`evidence/qa-gates/exclusion-census-post.2026-08-07T23-30.md`) |
| Full suite | 6293/6293 passing (6272 → 6293) |
| Line coverage | 85.6453% → 85.8333% raw; 85.8223% denominator-adjusted |
| Branch coverage | 79.0039% → 79.2226% |
| Changed executable lines | 6/6 = 100% |
| Feature review | PASS, 0 blocking findings |
| CI | `success` on head `8d670022`, head-SHA parity confirmed |

## Follow-ups arising

Recorded as issues so they survive this feature folder's merge:

- **#492** — the prescribed nullable gate uses `/t:Build`, so MSBuild's incremental up-to-date check
  skips recompiling `UtilitiesCS` and the gate reports `EXIT 0` without evaluating nullable at all.
  A forced `/t:Rebuild` yields 195 errors, all in `UtilitiesCS.csproj`.
- **#493** — `QfcItemControllerTestSupport.EnsureUiThreadDispatcher` mutates the process-wide static
  `UiThread._dispatcher` without restoring it. This caused a real deadlock during PR #479's Phase 8;
  the fix in that PR is local to its own fixture and the shared helper is unchanged.
- **#494** — `CLAUDE.md` and `.claude/rules/` state conflicting coverage thresholds (80/90 vs 85/75)
  and contradict each other on coverage-exclusion policy.

Not filed as new issues:

- `CS2002` duplicate `<Compile>` entry in `UtilitiesCS.Test.csproj` — already tracked
  (`docs/features/potential/promoted/2026-07-20-utilitiescs-test-cs2002-duplicate-compile-entry.md`).
- Promotion of `WinFormsPumpHost` to a shared test-support project — conditional on `UtilitiesCS.Test`
  gaining a consumer; captured as `spec.md` Non-Goal 3.
