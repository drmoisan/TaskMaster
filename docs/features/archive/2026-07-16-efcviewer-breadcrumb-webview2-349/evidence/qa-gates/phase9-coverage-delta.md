# Phase 9 — Coverage Delta / Threshold Verification (P9-T5)

Timestamp: 2026-07-18T12-50
Inputs: baseline `evidence/baseline/phase0-baseline-tests-coverage.md` (P0-T5) vs post-change `evidence/qa-gates/phase9-final-tests-coverage.md` (P9-T4); identical runsettings and test-assembly set for both runs.

## Baseline vs post-change

| Scope | Baseline (P0-T5) | Post-change (P9-T4) | Delta |
|---|---|---|---|
| OVERALL line (all instrumented modules) | 58.74% (42623/72557) | 59.15% (43377/73328) | +0.41 pp |
| OVERALL branch | 46.33% (9560/20635) | 46.94% (9821/20923) | +0.61 pp |
| UtilitiesCS line | 88.55% | 88.65% | +0.10 pp |
| UtilitiesCS branch | 82.22% | 82.36% | +0.14 pp |
| QuickFiler line | 72.32% | 73.34% | +1.02 pp |
| QuickFiler branch | 62.32% | 64.30% | +1.98 pp |
| Tests | 4838 passed | 4935 passed | +97 tests, 0 failures |

## New-code coverage per module (threshold >= 90% line)

| New non-exempt module | Line | Branch | >= 90%? |
|---|---|---|---|
| BreadcrumbSegment | 100% | n/a (guard-only branches 50% of 4) | YES |
| BreadcrumbRow | 98.02% | 88.46% | YES |
| BreadcrumbRowBuilder | 100% | 100% | YES |
| BreadcrumbMessages (5 types) | 100% each | guard-only branches | YES |
| BreadcrumbMessageCodec (+exception) | 95.56% / 100% | 97.37% | YES |
| BreadcrumbDocumentAssets | constant-only (no executable lines) | n/a | exempt-by-shape (type-only/constant module clarification) |
| BreadcrumbHtmlRenderer | 96.90% | 89.47% | YES |
| BreadcrumbOutboundQueue | 95.83% | 100% | YES |
| BreadcrumbBridgeRouter (incl. async state machines) | 97.87% | 92.22% | YES |

## Changed-line coverage

- All new non-exempt code is in the modules above (>= 95% line each), so no changed non-exempt line lost coverage.
- Remaining modified files are coverage-exempt by policy: `EfcViewer.cs`/`EfcViewer.Designer.cs`/`EfcViewer3.Designer.cs` (Form/Designer, `[ExcludeFromCodeCoverage]`), `EfcFormController.cs` (wholly `[ExcludeFromCodeCoverage]`, wiring-only, net -36 lines), `WebView2BreadcrumbHost.cs` (`[ExcludeFromCodeCoverage]` with in-code justification), plus test files and `<Compile Include>` csproj wiring.

## Verdict

- Every new non-exempt module meets the >= 90% line threshold: PASS.
- Repository floor not regressed: both feature packages and the overall figure IMPROVED vs the P0-T5 baseline (UtilitiesCS 88.65% line / 82.36% branch exceeds the 85%/75% spec bars; the like-for-like overall and QuickFiler figures rose; the sub-floor overall headline is the known uninstrumented-assembly artifact documented in the baseline and unchanged in kind): PASS (no regression).
- No changed line lost coverage: PASS.

Outcome: PASS — all coverage thresholds of the plan are met.
