# Maintainer Decision — Issue #227 (Reduced Exemption Boundary Ratification)

- **Date:** 2026-07-02
- **Decision owner:** Dan Moisan (project maintainer)
- **Decision:** RATIFIED. The 19-member `[ExcludeFromCodeCoverage]` boundary
  (`evidence/other/exemption-boundary.2026-07-02T17-00.md`) is accepted.
- **Status:** Ratified.

## Context

Across five remediation cycles, the `QfcItemController` coverage-exemption boundary was reduced from
103 (cycle-1, denied ratification 2026-07-01) to 41 (cycle-2, seams) to 24 (cycle-3, targeted
reduction after the maintainer questioned the original ~6-8 estimate) to 19 (cycle-5, after the
maintainer directly questioned whether 24 was genuinely the floor). Cycle-4 resolved a test-honesty
gap on 2 of the 24 without changing the count. Each round of maintainer skepticism found further
genuine reduction was possible — this is documented explicitly so the pattern is visible to future
reviewers, not smoothed over.

## Decision and rationale

The maintainer ratifies the 19-member residual boundary as of cycle 5. The residual composition is:

- **9 members** blocked by an unbuilt WinForms message-pump test seam (the `Application.Run()`-on-
  background-thread analogue of the WPF `Dispatcher.Run()` pump this repo already has for
  `IUiDispatcher`) — a materially larger, distinct test-infrastructure investment, not a missed
  application of an existing pattern. Tracked as a separate follow-up issue (see below), not folded
  into this remediation.
- **6 members** — `async void` WinForms-event-handler signature shells whose substantive logic is
  already extracted and tested via `*Core` methods; testing the shell itself would require new
  `SynchronizationContext`-capture test infrastructure this repo does not have.
- **3 members** — deliberate `virtual` test-seam methods (the override point IS the test seam by
  design; the base body is intentionally unexercised directly).
- **1 member** — `WebView2CoreInitializer`'s adapter body, a genuine external-process dependency
  (the WebView2 Runtime), barred by this repo's own External Dependencies unit-test rule.

## Follow-up work authorized

Issue #230 tracks the WinForms message-pump test-infrastructure gap (9 members), analogous to how
#197 tracks the repo-wide coverage uplift. This is exploratory future work, not committed to any
timeline, and is explicitly NOT a condition of merging #227.

## Effect on acceptance criteria

- **AC8** and **AC10** are satisfied. The 19-member boundary is individually justified, minimized
  (no member reducible via an already-established technique retains an exemption — verified across
  five rounds of increasingly rigorous re-audit), and documented for maintainer ratification, which
  this decision provides.

## References

- `evidence/other/exemption-boundary.2026-07-02T17-00.md` (ratified boundary)
- `evidence/other/exemption-boundary.2026-07-02T15-05.md`, `evidence/other/exemption-boundary.2026-06-29T12-40.md` (superseded prior boundaries)
- `artifacts/research/2026-07-02T16-15-qfc-item-controller-headless-itemviewer-research.md` (cycle-5 design source)
- `docs/features/active/2026-06-28-qfc-form-viewer-testability-223/maintainer-decision.2026-06-29.md` (authority-scoped exception precedent)
