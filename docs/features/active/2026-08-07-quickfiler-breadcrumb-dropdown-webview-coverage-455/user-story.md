# `quickfiler-breadcrumb-dropdown-webview-coverage` — User Story

- Issue: #455
- Parent: epic #136 `quickfiler-per-file-coverage`, child F13
- Owner: drmoisan
- Status: Draft
- Last Updated: 2026-08-07
- Work Mode: full-feature (this file and `spec.md` are the authoritative acceptance-criteria sources)

## Story Statement

- As the **maintainer of QuickFiler**, I want the breadcrumb drop-down and WebView2 host surface to be
  measurably covered rather than administratively excluded, so that an autonomous agent can change
  that surface and get a truthful signal about whether it broke something.
- As the **maintainer**, I want every `[ExcludeFromCodeCoverage]` in this surface to be either
  withdrawn or backed by a written, ratified ground, so that the exemption boundary is auditable
  instead of self-asserted by the file it exempts.
- As the **epic capstone reviewer (F16)**, I want each file's target expressed against its true
  reachable ceiling, so that I can distinguish an unmet gate from a structurally unreachable outcome
  without re-deriving the analysis.

## Problem / Why

Child F13 owns 15 compiled files (~3,111 lines) under `QuickFiler/Viewers/`. Three problems make this
surface expensive to maintain today, and none of them is "the coverage number is low".

**1. Three files are invisible, not uncovered.** `WebView2BreadcrumbHost.cs`, `WebView2Messenger.cs`,
and `WebView2CoreInitializer.cs` carry class-level `[ExcludeFromCodeCoverage]` and are therefore
absent from instrumentation entirely — they produce no `filename=` entry in any coverage report. Two
of them have **zero test references anywhere** in `QuickFiler.Test/`. Their exemptions are justified
in their own doc comments as "1:1 SDK forwarding", and for two of the three that claim is false:
`WebView2Messenger` has only five SDK statements among roughly seventy coverable lines, the rest
being disposal gating, race guards, null guards, and two independent payload fallbacks;
`WebView2BreadcrumbHost.InitializeAsync` is fully testable today behind a seam that is already
injected into its own constructor. A maintainer editing either file gets no coverage signal at all.

**2. The exemption boundary does not do what it says.** `[ExcludeFromCodeCoverage]` on a *method* does
not suppress instrumentation of lambdas lifted out of that method. In
`BreadcrumbPopupUiOperations.cs`, 23 of 24 uncovered lines are exactly this defect: the attribute
claims to exempt code that the report still counts as uncovered production. The file also sits at
494 of a 500-line limit, so it cannot absorb even a doc-comment addition.

**3. The stated grounds do not cover the files that cite them.** `CLAUDE.md` §UT2 names three
exemption grounds — VSTO lifecycle, WinForms form-derived/Designer-generated, and Outlook Interop
without a seam. **None applies to any WebView2 file in this scope.** Three attributes rest on a ground
that does not textually exist, and one further attribute
(`BreadcrumbPopupUiOperations.DisposeProductionSurface`) sits on a member that touches no SDK type at
all and that existing tests already execute.

What is *not* a problem: the eight instrumented coordinator files already clear both the 80% line
floor and the 75% branch floor, with the lowest branch figure at 85.71%. Treating them as
under-covered would spend effort against a shortfall that does not exist.

## Personas & Scenarios

**Persona — the repository maintainer directing autonomous agents.**
Cares about whether a coverage number can be trusted as a proxy for regression risk. Constrained by a
no-behavior-change mandate on a VSTO/WinForms codebase in the middle of a long-term migration away
from VSTO, and by fifteen sibling epic children editing two shared `.csproj` files concurrently.
Frustrated by exemptions that are self-justifying, by ceilings that get rediscovered by every
reviewer, and by "100% or fail" gates on code where 100% is provably unreachable.

**Scenario — an agent is asked to fix the breadcrumb popup's initialization-failure path.**
Today the agent opens `WebView2BreadcrumbHost.cs`, sees a class-level exemption, finds no test file,
and has no way to tell whether its change to the failure branch is exercised by anything. It either
writes nothing and hopes, or it removes the attribute and discovers the file starts at zero. After
this feature, the same agent finds a non-exempt type with a fake control surface it can drive,
a failure branch pinned by named tests, and — for the residual five statements that genuinely require
a live browser process — one small adapter type with a single attribute and a written ground.

**Scenario — the capstone reviewer checks F13 against issue #136 AC1.**
Today the reviewer reads `98.97% line / 85.71% branch` on `BreadcrumbCollapsedSurfaceController.cs`
and has to decide whether the missing 14.29% of branches is neglect. After this feature the ledger
records that this file's branch ceiling is **95.24%**, with the proof that two operands of one guard
are unreachable because a single method is the sole atomic writer of both fields it tests. The
reviewer confirms the number against the ceiling instead of re-deriving it.

## Value Delivered

- **Autonomous agentic maintenance.** The two highest-risk files in the surface become measurable and
  test-driven, so a coverage delta on a future change is meaningful rather than vacuous.
- **Regression-escape reduction.** The drop-down open/close lifetime, WebView2 initialization
  success/failure, message round-trip, disposal races, and popup placement boundaries are pinned by
  deterministic tests with no timing dependence, so a reordering or guard-inversion regression fails a
  test instead of reaching a user.
- **An auditable exemption boundary.** Ten attributes become six, all consolidated onto types whose
  every member is a single SDK statement with zero branches and zero state, each backed by a ground
  that F1 has ratified in writing. The exemption stops hiding testable logic and, for the first time,
  actually removes the lines it claims to remove.

## Acceptance Criteria

These are the user-observable outcomes. The numeric and mechanical detail behind each lives in
`spec.md` §12.

- [ ] **US-1.** A maintainer can open the committed per-file coverage report and see a line and branch
      figure for **every** production file in this surface, including
      `WebView2BreadcrumbHost.cs` and `WebView2Messenger.cs`, which report no figure today.
- [ ] **US-2.** `WebView2BreadcrumbHost.cs` and `WebView2Messenger.cs` are no longer excluded from
      coverage, and each measures **at least 90% line and 80% branch**.
- [ ] **US-3.** Every file that already met both gates still meets them: no file in the surface
      regresses on either line or branch coverage against its recorded baseline.
- [ ] **US-4.** Every remaining `[ExcludeFromCodeCoverage]` in the surface is on a type whose members
      are single SDK statements with no branches and no state, and each is backed by an exemption
      ground written down in the epic coverage ledger — not asserted in the exempt file's own doc
      comment.
- [ ] **US-5.** The exemption on `BreadcrumbPopupUiOperations.DisposeProductionSurface` — a member
      that touches no WebView2 type and that existing tests already execute — is removed, and the
      member's previously unmeasured branches are covered.
- [ ] **US-6.** Exempting a file no longer silently leaves its lambda bodies in the denominator: the
      lines that the exemption claims to remove are actually absent from the coverage report.
- [ ] **US-7.** A reader can find, in one committed record, every outcome in this surface that is
      provably unreachable, with the reason, so that no future reviewer treats a ceiling below 100% as
      a defect.
- [ ] **US-8.** No user-visible QuickFiler behavior changes. The breadcrumb drop-down opens, closes,
      navigates, and disposes exactly as before, and the complete pre-existing test suite passes with
      no assertion weakened or removed.
- [ ] **US-9.** The test suite added by this work runs deterministically with no sleeps, no timers, no
      temporary files, no shown windows, and no dependency on the WebView2 runtime being installed, so
      it produces the same result on a developer workstation and on a CI runner.
- [ ] **US-10.** The full C# toolchain (format, analyzers, nullable/type-check, tests) passes in a
      single clean final pass, and repository-wide coverage measured before and after in the same
      session is retained or improved.
- [ ] **US-11.** Every file in this surface — existing, created, exempt, and interface-only — has a
      row in the epic coverage ledger, so the capstone can reconcile the whole surface without
      re-deriving any classification.

## Non-Goals

- Any change to observable QuickFiler behavior, including the latent defects found during research
  (handler-retention across pooled viewer reuse, cross-thread SDK access, missing argument validation,
  silent-degradation fallbacks). These are tracked as separate GitHub issues.
- Any edit to files owned by sibling children F12 (`BreadcrumbBridgeRouter`,
  `BreadcrumbBridgeCoordinator`, `BreadcrumbCoordinatorUpgradeLifetime`,
  `BreadcrumbItemViewerLifecycleCoordinator`, `BreadcrumbMessengerHub`) or F14
  (`ItemViewer.Breadcrumb.cs`, the `ItemViewer` Designer family).
- Retyping the Designer-owned `_l0vhBreadcrumb_WebView2` field or any Designer-backed property. This
  approach is known-broken and is pinned by a live, passing reflection test.
- Converging the two WebView2 hosting paths (`IBreadcrumbWebHost` and `IWebViewMessenger`) into one
  seam. Recorded as a post-epic candidate.
- Reaching 100% coverage on any file. Several outcomes in this surface are provably unreachable and
  the targets respect those ceilings.
- Changing any repository-wide coverage threshold, adding an assembly-level coverage exclusion, or
  widening `InternalsVisibleTo`.
