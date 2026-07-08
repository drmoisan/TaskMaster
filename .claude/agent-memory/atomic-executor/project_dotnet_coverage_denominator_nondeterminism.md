---
name: dotnet-coverage-denominator-nondeterminism
description: Invoke-MSTestWithCoverage repo-wide line-rate is nondeterministic across runs due to dotnet-coverage double-counting the instrumented denominator; re-baseline via git-stash for a trustworthy delta
metadata:
  type: project
---

The repo coverage path `scripts/vscode/Invoke-MSTestWithCoverage.ps1` (dotnet-coverage collect wrapping vstest over all 7 `*.Test.dll`, Workers=0) can emit a WILDLY different repo-wide `line-rate` between runs of the SAME code, because dotnet-coverage instruments all runtime-loaded modules and its cross-assembly merge is order/parallelism-sensitive and sometimes DOUBLE-COUNTS lines.

Concrete #261 F1 observation: one baseline run reported 47.16% with `lines-valid=180246` (UtilitiesCS package showed an implausible 141,188 valid lines); a clean re-measure of the exact same pre-change tree reported 81.02% with `lines-valid=97933`. The ~98k denominator is the correct de-duplicated value; the 180k run was the double-count anomaly.

**Why:** dotnet-coverage merge nondeterminism inflates the denominator, halving the apparent coverage. The per-CLASS line-rate for touched files stays stable and correct regardless.

**How to apply:**
- Never trust a single repo-wide coverage number for a no-regression delta. Run coverage at least twice and confirm the denominator (`lines-valid`) reproduces.
- For an apples-to-apples baseline-vs-postchange delta, `git stash push -u` the code changes (NOT the plan/evidence .md), rebuild, re-run coverage for a clean baseline, then `git stash pop` and rebuild. This gives both measurements under the same (correct) denominator.
- Rely on per-class coverage (parse the Cobertura `<class>` line hits with a small Python/awk script) for new-code >=90% and no-regression proof — it is stable when the overall percentage is not.
- Related: [[project_qfc227_coverage_tooling]], [[project_coverage_firstparty_denominator_method]], [[project_utilitiescs_test_parallelism_flakiness]].
