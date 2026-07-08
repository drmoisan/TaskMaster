# Maintainer Authority-Scoped Exception Decision — Issue #223

- **Date:** 2026-06-29
- **Decision owner:** Dan Moisan (project maintainer)
- **Decision:** Option 1 — ACCEPT the pre-existing repo-wide first-party coverage
  shortfall as an authority-scoped exception for issue #223.
- **Status:** Ratified.

## Context

Feature-review remediation cycle 1 for issue #223 resolved the FAIL on the absent
canonical C# coverage artifact (`artifacts/csharp/coverage.xml` now exists). The
resulting measurement showed repo-wide first-party testable-denominator coverage at
**73.35%–74.11%**, below the repository's `>= 80%` floor.

## Decision and rationale

The maintainer accepts the shortfall as a pre-existing, separately-tracked condition
that is out of scope for issue #223:

1. The shortfall is **not introduced by this change.** New code `QfcFormKeyHandler` is
   100% covered (>= 90% new-code floor); the changed `QfcFormController` type improved
   +12.62pp (39.24% → 51.86%) with no regression. The refactor only adds tests and moves
   Form-bound code under `[ExcludeFromCodeCoverage]`; it cannot lower first-party coverage.
2. All change-scope quality gates pass: csharpier, .NET analyzers, nullable/TWAE, and
   4566/4566 first-party MSTest with coverage.
3. The repo-wide first-party uplift to `>= 80%` is the explicit scope of the separate
   `feature/csharp-coverage-uplift` (#197) initiative. The low-coverage packages
   (QuickFiler, ToDoModel, Tags, TaskMaster, TaskVisualization) are predominantly
   Outlook-Interop-bound code; raising them is a dedicated effort, not part of this
   testability refactor.
4. The measured 73.35% is consistent with #197's known 59–76% baseline.

## Scope and guardrails of this exception

- The `>= 80%` floor is **not weakened** in policy; no `.editorconfig`, `coverage.config`,
  `.claude/rules/**`, or `CLAUDE.md` threshold was altered. No test was weakened or removed.
- This exception applies to issue #223 only. The repo-wide first-party floor remains in
  force and the uplift remains tracked under #197.

## Effect on acceptance criteria

- **AC5** is treated as satisfied-with-documented-exception: the new-code (100%),
  changed-line (no regression), and test-presence sub-claims are fully met; the
  "repo-wide coverage stays >= 80%" sub-claim is dispositioned under this ratified
  authority-scoped exception, with the residual repo-wide uplift owned by #197.

## References

- `evidence/other/repo-wide-floor-escalation-finding.2026-06-28T21-30.md`
- `evidence/qa-gates/repo-wide-coverage-measurement.2026-06-28T21-30.md`
- `evidence/regression-testing/repo-wide-coverage-testable-denominator.2026-06-28T21-30.md`
- `evidence/issue-updates/issue-223-ac5-deferred.2026-06-28T21-30.md`
