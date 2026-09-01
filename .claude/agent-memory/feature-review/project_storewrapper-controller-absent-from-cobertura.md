---
name: storewrapper-controller-absent-from-cobertura
description: StoreWrapperController class is entirely missing from Cobertura coverage XML (both baseline and post-change) despite only 2 of its members carrying [ExcludeFromCodeCoverage] — pre-existing tooling gap, not a per-PR regression
metadata:
  type: project
---

During review of #287 (`bug/storewrapper-dialog-imprecise-for-genuine-failure-287`), parsing both
`coverage/baseline.cobertura.xml` and `coverage/post-change.cobertura.xml` directly (Python
`xml.etree`) found zero `<class>` elements named
`UtilitiesCS.OutlookObjects.Store.StoreWrapperController` in either file, even though the class is
`public` and only 2 of its members (`Launch()` and one other method) carry
`[ExcludeFromCodeCoverage]` — the rest of the class should be instrumented and measurable. Sibling
classes in the same namespace (`DisabledStoresController`, `StoreLaunchReadinessEvaluator`,
`StoreWrapper`, etc.) all appear normally with real line/branch rates.

**Why:** This is identical in both baseline and post-change XML, so it predates #287 and was not
caused or fixed by it — do not attribute it to whichever branch you're reviewing when you notice it.
It also means any coverage shortfall inside `StoreWrapperController`'s non-excluded members is
currently invisible to the repo-wide metric rather than counted as a gap, which could mask real
undercoverage in future reviews of this class.

**How to apply:** If a future PR touches `StoreWrapperController` and you're checking coverage
regression/no-regression on it, first confirm the class actually appears in the Cobertura XML for
that run before trusting a "no regression" read from its absence — an absent class is not evidence
of either full coverage or full exclusion. Consider filing a dedicated investigation issue (root
cause: assembly-load timing, Koverage post-processing filter, or a class-level instrumentation gap)
independent of whatever functional PR surfaces it next. See also
[[csharp-canonical-jacoco-includes-uninstrumented-assemblies]] and
[[TestResults coverage XML cross-module check]] for related coverage-artifact interpretation traps
in this repo.
