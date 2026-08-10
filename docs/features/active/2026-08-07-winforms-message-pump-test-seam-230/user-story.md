# `2026-08-07-winforms-message-pump-test-seam` — User Story

- Issue: #230
- Owner: drmoisan
- Status: Draft
- Last Updated: 2026-08-07T21-30

## Story Statement

- As the repository maintainer, I want the ratified `QfcItemController` coverage-exemption
  boundary reduced from 19 members to 11 by real, deterministic unit tests, so that the exemption
  list I ratify reflects only genuine external-dependency barriers (WebView2 runtime, live
  Outlook) rather than missing test infrastructure.
- As a future contributor writing tests for code that awaits `UiSyncContext` or posts through a
  `WindowsFormsSynchronizationContext`, I want a documented, reusable WinForms message-pump test
  seam with the same shape as the existing WPF dispatcher hosts, so that I can await those
  continuations deterministically instead of hitting the known indefinite-hang hazard or adding a
  new `[ExcludeFromCodeCoverage]` exemption.

## Problem / Why

Issue #227 reduced `QfcItemController`'s exemption boundary from 103 to 19 members. The 9
remaining controller members are all blocked by one gap: their continuations are posted through a
`WindowsFormsSynchronizationContext`, and no thread in an MSTest run pumps WinForms messages, so
awaiting them hangs. The repository already solved the equivalent WPF problem
(`StaDispatcherHost`, `StartRunningDispatcher()`); the WinForms analogue was explicitly deferred
from #227 cycle 5 into this issue. Until it exists, these members stay exempt for an
infrastructure reason rather than a genuine testability barrier, and every future contributor who
touches WinForms-context async code faces the same hang hazard with no sanctioned tool.

## Personas & Scenarios

- Persona: Repository maintainer (drmoisan)
  - Owns the ratified coverage-exemption boundary and re-ratifies changes to it.
  - Cares about: the exemption list staying minimal and honest; the coverage denominator growing
    only alongside real covering tests; unattended CI never hanging.
  - Constraints: net481 non-SDK projects; MSTest/Moq/FluentAssertions only; no temporary files;
    no external processes in unit tests.
  - Frustration: 9 members are exempt not because they are untestable but because a known,
    already-designed piece of test infrastructure has not been built.

- Persona: Future contributor
  - Adds or modifies tests around `QuickFiler` controllers and viewers.
  - Cares about: a copy-able, precedented pattern for pumping WinForms continuations; tests that
    fail with a clear exception instead of timing out in CI.
  - Constraints: may not know the `AsyncSerialization_Tests` hang hazard exists; needs the seam's
    usage contract (one host per test, dispose in `finally`, construct controls via the host) to
    be visible from the seam's own tests.

- Scenario: De-exempting a pump-blocked member
  - The maintainer schedules #230 after #227's merge. The executor builds `WinFormsPumpHost` in
    `QuickFiler.Test`, proves it with self-tests (including the WPF-dispatcher interop smoke
    test), then works through the 8 target members: construct a headless `ItemViewer` on the pump
    via `host.InvokeAsync`, wire the established harness with mocked seams, await the member,
    assert observable state, and remove that member's exemption attribute in the same change.
  - Obstacle: the two static factories have no injection point, so the executor applies the
    scoped, non-breaking optional-parameter extension defined in spec.md rather than letting the
    factories reach the real WebView2 runtime.
  - Outcome: the maintainer reviews a census diff showing 19 → 11 exemption sites, coverage
    evidence showing no regression, and re-ratifies the reduced boundary in the PR.

- Scenario: A contributor tests new WinForms-context async code a year later
  - The contributor's new test hangs locally when awaiting a viewer continuation. They find
    `WinFormsPumpHost` and its self-tests, copy the `using (var host = ...)` pattern, and their
    test completes deterministically; a bug in their code surfaces as a faulted awaited task, not
    a CI timeout.

## Acceptance Criteria

- [x] A contributor can deterministically await a `WindowsFormsSynchronizationContext`
      continuation in a `QuickFiler.Test` unit test using `WinFormsPumpHost`, without touching
      the MSTest thread's `SynchronizationContext` and without any sleep, delay, or polling.
      <br/>Evidence: `evidence/other/pump-host-selftests.2026-08-07T22-05.md` (P1-T7) —
      `AwaitingSyncContext_FromTheTestThread_ResumesOnThePumpThread` passing; no
      `SetSynchronizationContext` call on the MSTest thread and no sleep/delay/polling in the seam
      or its tests.
- [x] The seam's own test file demonstrates the complete usage contract a future contributor
      needs: construction, running work on the pump, observing failures as faulted tasks or
      `StopAsync` errors, and disposal — so the pattern can be adopted by example without reading
      the implementation.
      <br/>Evidence: `evidence/other/pump-host-selftests.2026-08-07T22-05.md` (P1-T7) — the 13
      self-tests in `QuickFiler.Test/TestSupport/WinFormsPumpHostTests.cs` show construction,
      `using`/`finally` release, all four posting members, direct context await, both fault
      channels, `StopAsync` fault surfacing, post-after-stop, and double-`Dispose`.
- [x] The `QfcItemController` exemption boundary shrinks from 19 to 11 sites, and the evidence
      trail (pre- and post-change census plus coverage comparison) is sufficient for the
      maintainer to re-ratify the reduced boundary during PR review without re-deriving it.
      <br/>Evidence: `evidence/baseline/exclusion-census-pre.2026-08-07T21-50.md` (19 sites) and
      `evidence/qa-gates/exclusion-census-post.2026-08-07T23-30.md` (11 sites, each removal mapped
      to its phase and covering test), plus
      `evidence/qa-gates/coverage-delta.2026-08-08T00-15.md` (no-regression comparison and
      per-member coverage for all 8 de-exempted members).
- [x] Every exemption that remains in the controller partials after this feature carries a
      justification tied to a genuine external dependency or a tracked follow-up issue — none
      remains blocked solely on missing WinForms pump infrastructure.
      <br/>Evidence: `evidence/qa-gates/exclusion-census-post.2026-08-07T23-30.md` (P7-T1) — the
      remaining 11 sites are 9 ratified non-pump categories (test seam, thin async-void event
      shells, `TlpCellSnapShot`-bound toggles), `InitializeWebViewAsync` (CoreWebView2 runtime, an
      external process, with the concrete-accessor barrier tracked per #230), and
      `EnsureBreadcrumbPipeline` (post-ratification #351, out of scope).
- [x] An unattended CI run of the full test suite with the new tests completes without hangs,
      dialogs, or external processes (no live Outlook, no WebView2 runtime initialization).
      <br/>Evidence: `evidence/qa-gates/final-test-coverage.2026-08-08T00-05.md` (P8-T5,
      iteration 2) — 6293/6293 passing, EXIT_CODE 0, unattended, every new test inside its
      `[Timeout]` bound; all Outlook types are Moq'd COM interfaces and every
      `IWebViewCoreInitializer` is mocked so no WebView2 runtime initializes. The same artifact
      records the iteration-1 failure and the test-isolation fix that resolved it.
- [x] Existing consumers of `CreateAsync`/`CreateSequentialAsync` observe no behavior change; the
      factory extension is invisible to callers that do not pass the new optional parameters.
      <br/>Evidence: `evidence/other/factory-seam-verification.2026-08-07T23-00.md` (P5-T2 —
      call-site enumeration shows zero in-repo callers; the seams are assigned before
      `SaveParameters`, so its `??=` defaults still apply when a parameter is omitted) and
      `evidence/other/factory-tests.2026-08-07T23-15.md` (P5-T6). Full re-confirmation comes from
      the Phase 8 full-suite run (`evidence/qa-gates/final-test-coverage.*.md`).

## Non-Goals

- Covering `InitializeWebViewAsync` end-to-end. Its residual barrier is the CoreWebView2/WebView2
  runtime (an external process barred by the unit-test policy), not the message pump; it remains
  exempt with an updated justification, and its concrete-`ItemViewer` accessor barrier is tracked
  separately per issue #230. The achievable outcome is 8 of the 9 residual members.
- Resolving `EnsureBreadcrumbPipeline` (added by #351 after the 2026-07-02 ratification); the
  census documents it as out-of-scope.
- Creating a shared test-support project or refactoring production initialization flow; see
  spec.md Non-Goals for rationale.
