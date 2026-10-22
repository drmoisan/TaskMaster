# Reduced-Audit Handoff (P3-T34)

Timestamp: 2026-09-30T15-23

## Pointers

- evidence/qa-gates/toolchain-final-pass.md (loop closure; pass 2 is the clean pass)
- evidence/qa-gates/coverage-summary.md (P3-T8 top-level fields and PASS-2: section; COMPARISON: section from P3-T10)
- evidence/qa-gates/footprint-scope.md (P3-T12 raw-document check and P3-T14 change footprint)
- evidence/regression-testing/prime-registration-fail-before.md (P1-T4 fail-before run)
- evidence/regression-testing/prime-registration-pass-after.md (P2-T4 pass-after run, FINAL-FIXTURE-RUN: and PASS-2: sections)
- evidence/other/ac-status-summary.md (18 of 18 acceptance criteria met)

## Anchors and route

- ANCHOR-SHA: b305903e275b8abf58e8e65831c189f517568fe4
- MAIN-MERGE-SHA: 66afa6372fd82fc1ffd7c81f85a1ad65eebc5817 (merged before pass 2 by merge commit 7190a4bcddab8c519933d98b12ede739d4afede3; used by P3-T12 and P3-T14 only)
- COVERAGE-ROUTE: DIRECT (STALL-PROBE: REPRODUCES in P0-T16)

## Statements

- The throwing-sink hazard and the post-fault log volume are out of scope and are recorded only in the spec's Rollout section, so no potential entry was written by this run.
- The committed test evidence is projections only: test-result summaries derived from the trx documents, the one-line first-party coverage summaries and the package-level JaCoCo projections. No raw trx or Cobertura document is committed (RAW-DOCS-COMMITTED: 0).
- The Phase 3 loop ran twice: pass 1 stopped at P3-T8 with a first-attempt failure set confined to QuickFiler.Controllers.Tests.QfcDatamodelLivenessTests; the single pass-2 restart admitted by the revision round 3 coordinator extension completed with zero failures.

PRE-FINAL-COMMIT-HEAD: 594c3eb9b0ee9ce288df9afdc97f7aa85bdd5b83
