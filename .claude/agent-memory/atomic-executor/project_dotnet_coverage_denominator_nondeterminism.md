---
name: dotnet-coverage-denominator-nondeterminism
description: Invoke-MSTestWithCoverage repo-wide line-rate is nondeterministic across runs due to dotnet-coverage double-counting the instrumented denominator; re-baseline via git-stash for a trustworthy delta
metadata:
  type: project
---

The repo coverage path `scripts/vscode/Invoke-MSTestWithCoverage.ps1` (dotnet-coverage collect wrapping vstest over all 7 `*.Test.dll`, Workers=0) can emit a WILDLY different repo-wide `line-rate` between runs of the SAME code, because dotnet-coverage instruments all runtime-loaded modules and its cross-assembly merge is order/parallelism-sensitive and sometimes DOUBLE-COUNTS lines.

Concrete #261 F1 observation: one baseline run reported 47.16% with `lines-valid=180246` (UtilitiesCS package showed an implausible 141,188 valid lines); a clean re-measure of the exact same pre-change tree reported 81.02% with `lines-valid=97933`. The ~98k denominator is the correct de-duplicated value; the 180k run was the double-count anomaly.

**Second confirmed instance (#464, 2026-08-28), with a new correlate.** Phase 0 baseline: `line-rate=0.7032`, `lines-valid=82070`, 18 MB Cobertura, and the run had **15 failing tests** (WinFormsPumpHost/dispatcher-fixture timeouts at ~60 s each). Final QC run of the SAME command on the same 9 assemblies: `line-rate=0.8525`, `lines-valid=64124`, 10.7 MB Cobertura, **0 failures**. A 17,946-line denominator swing against a diff that adds ~150 production lines.

The correlate worth checking next time: **the run with timing-out test hosts produced the LARGER denominator and the LOWER rate.** A killed/timed-out testhost appears to leave a different set of loaded modules in the merge. So a baseline captured during a flaky run is the deflated one, and a later clean run will look like a large phantom improvement.

Practical consequence when you cannot re-baseline (the baseline Cobertura was deleted, and checking out the baseline commit is forbidden): assert only the direction the gate requires ("post-change line rate is not lower"), state both `lines-valid` figures side by side, and say explicitly that the delta is **not** claimed as a coverage improvement the feature delivered. Also record positively what the delivered file contains — package count, no duplicate package names, no test packages — so a reader can see the delivered denominator is complete even if the baseline's composition is unrecoverable.

**Third instance (#644, 2026-08-29) — the NUMERATOR moves while the denominator holds.** Three runs of the same command, `lines-valid` byte-identical at 64221 every time, `lines-covered` = 54800 (pre-change) / 54805 (fix applied) / 54793 (fix applied, +2 condensed comment blocks in an `[ExcludeFromCodeCoverage]` class). That is a ±12-line numerator spread, or ±0.01 percentage points at two decimal places, under a 24-worker ClassLevel-parallel run. Note the series is **non-monotonic with respect to source state**: the changed state measured ABOVE the pre-change baseline on one run and below it on another. Non-monotonicity is the cheapest available proof that a small delta is measurement noise rather than a source-attributable regression — say so explicitly, since you usually cannot diff against the baseline document (it has been overwritten).

**Plan-authoring consequence — flag this at preflight.** A plan clause of the form "post-change percentage must be >= baseline percentage" against this harness is a **flaky gate**: it fails on noise alone at the second decimal place, and it fails even when the change's only production file carries `[ExcludeFromCodeCoverage]` and appears in 0 classes of the coverage document, i.e. when the change provably cannot move the figure. An executor hitting it has no honest move except the authorized REMEDIATION-REQUIRED branch — re-running until a run clears the bar is selecting the evidence you are judged against, which is the unfalsifiable-acceptance defect class. Prefer a tolerance band (e.g. "not more than 0.1 points below baseline") or a per-class assertion over the touched files.

**The mechanism, re-derived in the script (2026-09-02).** In the POST-PROCESSED document the root `lines-valid`/`line-rate` are not dotnet-coverage's own numbers: `ConvertTo-KoverageCoberturaXml` first removes every `<package>` whose name is not in `Get-KoverageProjectAllowlist`, then recomputes and re-sets `line-rate`, `lines-valid`, `lines-covered` and the branch trio from the survivors (`Invoke-MSTestWithCoverage.Helpers.ps1:417-421` and `:441-447`). Two consequences worth knowing at plan-authoring time:
- The allowlist **excludes test projects by design** (`Helpers.ps1:21-24`, keyed on the `.Test` assembly-name suffix), so adding N new test files cannot inflate the processed denominator. A plan clause that compares two processed `lines-valid` figures is therefore NOT auto-breached by the plan's own new tests.
- The residual variance is purely which *production* assemblies happened to be loaded and instrumented at run time (`Invoke-MSTestWithCoverage.ps1:336-337` states dotnet-coverage instruments all loaded DLLs). That makes `lines-valid` genuinely independent of the run's exit code — two runs that both exit 0 can report different denominators — so a plan that models degraded-run states from exit codes still needs a separate comparability axis measured from the two documents' own numbers.

**Why:** dotnet-coverage merge nondeterminism inflates the denominator, halving the apparent coverage. The per-CLASS line-rate for touched files stays stable and correct regardless.

**How to apply:**
- Never trust a single repo-wide coverage number for a no-regression delta. Run coverage at least twice and confirm the denominator (`lines-valid`) reproduces.
- For an apples-to-apples baseline-vs-postchange delta, `git stash push -u` the code changes (NOT the plan/evidence .md), rebuild, re-run coverage for a clean baseline, then `git stash pop` and rebuild. This gives both measurements under the same (correct) denominator.
- Rely on per-class coverage (parse the Cobertura `<class>` line hits with a small Python/awk script) for new-code >=90% and no-regression proof — it is stable when the overall percentage is not.
- Related: [[project_qfc227_coverage_tooling]], [[project_coverage_firstparty_denominator_method]], [[project_utilitiescs_test_parallelism_flakiness]].
