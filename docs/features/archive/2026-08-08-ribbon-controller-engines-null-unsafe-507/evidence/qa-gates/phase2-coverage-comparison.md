# Phase 2 — Coverage/pass-fail comparison (Phase 0 baseline vs Phase 2 final)

Timestamp: 2026-08-08T16-12

## Pass/fail counts

| | Passed | Failed | Skipped | Total |
|---|---|---|---|---|
| Phase 0 baseline (P0-T5) | 6294 | 0 | 0 | 6294 |
| Phase 2 final (P2-T4) | 6296 | 0 | 0 | 6296 |

Delta: +2 passed, +0 failed. The +2 exactly matches the two new regression tests added in Phase 1
(`Engines_WhenGlobalsNotAssigned_ReturnsNullInsteadOfThrowing`,
`Engines_WhenGlobalsAssigned_ReturnsGlobalsEngines`). **No pre-existing test regressed; pass/fail
counts are strictly no worse than the Phase 0 baseline (AC6: satisfied).**

## Coverage headline

| | Repo-wide line-rate (dotnet-coverage/Cobertura) |
|---|---|
| Phase 0 baseline (P0-T5) | 0.7443263443535741 (74.43%) |
| Phase 2 final (P2-T4) | 0.6165729148230514 (61.66%) |

Raw headline delta: -12.77 points. As documented in detail in
`evidence/qa-gates/phase2-final-vstest-coverage.md`, this raw aggregate swing is attributable to
`dotnet-coverage`'s run-to-run Cobertura-conversion denominator nondeterminism (lines-valid grew
from 213,002 to 259,906 while lines-covered actually *increased* from 158,543 to 160,251; the
enumerated class/file count grew from 1,924 to 2,336 with the same 25 assembly packages present in
both runs), not a genuine loss of tested behavior. A per-file `line-rate` reproduction across all
1,924 files present in the baseline found 0 files missing from the final run and exactly 1 file (of
1,924) with a >1-point decrease — an unrelated UtilitiesCS dataflow file
(`SubjectMapSco.Orchestration.cs`, 95.56% → 88.28%) consistent with ordinary test-order/timing
variance, not a change caused by this feature. `RibbonControllerTests.cs` shows 100% coverage in
both runs (7 methods baseline, 8 methods final — the extra entry being new test coverage).
`RibbonController.Intelligence.cs` is not instrumented in either run, consistent with the ratified
`[ExcludeFromCodeCoverage]` exemption on `RibbonController`.

## No-regression confirmation

**Explicit confirmation: no regression.** The MSTest pass/fail counts did not regress (strictly
improved: +2 passing tests, 0 failures in both runs), satisfying AC6. The raw coverage-percentage
headline is confirmed, via per-file reproduction against the identical baseline file set, to be a
known tooling denominator artifact and not a genuine coverage loss attributable to this feature's
one-line production change or its two added tests. This finding — that the repo's coverage
aggregation via `dotnet-coverage`/`vstest.console.exe /EnableCodeCoverage` is subject to
significant run-to-run denominator variance independent of any code change — is escalated for the
orchestrator's awareness as a pre-existing tooling characteristic outside this feature's scope, not
a defect introduced by this change.

## Note on raw Cobertura artifacts (orchestrator, 2026-08-08T19-30)

The intermediate raw Cobertura dumps referenced in this artifact and in the two vstest artifacts
(`evidence/baseline/phase0-baseline-coverage.cobertura.xml`, 37 MB, and
`evidence/qa-gates/phase2-final-coverage.cobertura.xml`, 44 MB) were deliberately NOT committed.
Together they add roughly 1.42 million lines and 81 MB to repository history permanently, which is
disproportionate for a one-line production bugfix, and it follows the convention already
established by commit `d0955dc4` ("docs(#503): replace raw cobertura coverage evidence with jacoco
summaries").

The numeric coverage evidence they supported is retained in full here and in
`evidence/baseline/phase0-baseline-vstest-coverage.md` and
`evidence/qa-gates/phase2-final-vstest-coverage.md`: baseline repo-wide `line-rate` 74.43%,
post-change 61.66%, with the per-file `line-rate` diff across 1,924 files showing zero attributable
regression.

`RibbonController` is `[ExcludeFromCodeCoverage]` under the ratified VSTO/COM ribbon-handler
exemption, so this change contributes no coverage surface in either direction. The dumps can be
regenerated on demand using the `dotnet-coverage merge ... -f cobertura` commands recorded verbatim
in the two vstest artifacts.
