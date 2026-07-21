# Coverage Delta Verification — P4-T5

- **Timestamp:** 2026-07-16T00-45

## Per-package Cobertura line-rate (touched first-party assemblies)

| Package     | Baseline (P0-T10) | Final (P4-T4) | Delta     |
|-------------|-------------------|---------------|-----------|
| UtilitiesCS | 88.40%            | 88.45%        | +0.05 pt  |
| QuickFiler  | 72.52%            | 72.27%        | -0.25 pt  |

## Repository-wide testable-denominator line-rate (first-party production packages only)

Computed by summing per-line hit/valid counts across the first-party production packages
(`UtilitiesCS`, `QuickFiler`, `Tags`, `TaskVisualization`, `TaskMaster`, `ToDoModel`) in both
Cobertura XMLs — excluding all `*.Test` packages and vendored/third-party packages (`Mono.Reflection`,
`System.Interactive`, `log4net`, `FluentAssertions`, `System.Linq.Async`, `Deedle`, `FSharp.Core`,
`SVGControl`), per the repo's "testable denominator" coverage-scope convention.

- **Baseline: 40745 / 53886 lines-covered/valid = 75.6133%**
- **Final: 40815 / 53971 lines-covered/valid = 75.6239%**
- **Delta: +0.0106 pt (no regression).**

## `CidImageResolver.cs` new-code coverage

- `UtilitiesCS.CidImageResolver` (the static class body): line-rate 93.33% (28/30 lines), branch-rate
  100%.
- `UtilitiesCS.CidImageResolver.<>c__DisplayClass3_0` (the `MatchEvaluator` lambda's compiler-generated
  closure class): line-rate 100% (8/8 lines), branch-rate 100%.
- Combined: 36/38 lines covered = **94.7% line coverage, 100% branch coverage.**

## PASS/FAIL statement

- **(a) No regression on repository-wide testable-denominator line/branch coverage versus baseline:
  PASS.** 75.6133% -> 75.6239% (+0.0106 pt). Branch-rate is 1 (100%) in both baseline and final
  Cobertura root output for all measured packages (this repo's coverage tool records branch-rate=1
  uniformly across packages in this configuration, consistent with the P0-T10/P4-T4 raw XML).
- **(b) `CidImageResolver.cs` new-code coverage is at or above the applicable threshold: PASS.** 94.7%
  line / 100% branch, exceeding both the repo-wide >=85%/>=75% floor and CLAUDE.md's >=90% new-module
  target.

## Observed package-level dip in `QuickFiler` (-0.25 pt) — root cause and disposition

The `QuickFiler` package's own line-rate fell slightly because this feature added two new
lines-bearing constructs to `QfcItemController.ViewerSetup.cs` that are not covered by any unit test:

1. `ResolveImageMimeType(string)` — a small, pure, stateless extension-to-MIME-type helper method
   (line-rate 0%, per-method Cobertura data). This method has no COM/WebView2 dependency and is, in
   principle, unit-testable; the plan (P2-T9) did not include a task to add a unit test for it, and no
   such test was added. This is recorded here as an observed gap, not a task requirement of this plan.
2. The `WebResourceRequested` lambda handler body (compiler-generated method
   `<InitializeWebViewAsync>b__121_0`, line-rate 0%) — this closure is nested inside
   `InitializeWebViewAsync`, which carries the pre-existing `[ExcludeFromCodeCoverage]` attribute
   (P2-T9's task text explicitly treats this handler as host-bound glue that "cannot be exercised
   without a live `CoreWebView2` instance"). The Cobertura/`dotnet-coverage` tooling in this repo does
   not propagate the containing method's `[ExcludeFromCodeCoverage]` exclusion to the
   compiler-generated lambda-closure method, so it is still counted in the raw per-class coverage data
   even though it falls within the intended, already-ratified exemption scope for this method.

Because the repository-wide testable-denominator coverage did not regress (item (a) above, PASS), and
`QfcItemController.ViewerSetup.cs`'s `InitializeWebViewAsync` extension is the repo's ratified
host-bound WebView2/COM exemption (per spec.md's Test Strategy coverage-impact section), this
package-level dip does not constitute a coverage-policy violation. The `ResolveImageMimeType` gap is
flagged as a follow-up observation for a future change, not a blocking finding for this plan's
completion.
