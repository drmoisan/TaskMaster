# [P6-T8] AC7 Registry Cases — Pass-After

Timestamp: 2026-09-08T10-17
Command: `& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll '/Settings:scripts\vscode\TaskMaster.cli.runsettings' '/InIsolation' '/Logger:trx' '/ResultsDirectory:TestResults\810-p6-t8' '/TestCaseFilter:FullyQualifiedName~BreadcrumbPopupOwnerRegistryTests'`, with `$vstest` re-bound per D3 from the two `vswhere` lines pinned by [P0-T7]
EXIT_CODE: 0
Output Summary: All six cases over the extracted `BreadcrumbPopupOwnerRegistry` pass. The six selected match the six `[TestMethod]` attributes [P6-T1] asserted, so every case authored is a case that ran.

TOTAL: 6
PASSED: 6
FAILED: 0

## Outcome per case

```
Passed AnyOpen_WithNoRegistration_ReportsFalse
Passed AnyOpen_SingleOwnerReportingClosed_ReportsFalse
Passed AnyOpen_SingleOwnerReportingOpen_ReportsTrue
Passed AnyOpen_TwoOwnersOneReportingOpen_ReportsTrue
Passed Register_SameControlTwice_ReplacesRatherThanAppends
Passed Register_NullControlOrNullPredicate_IsIgnored
```

## The load-bearing polarity

`AnyOpen_WithNoRegistration_ReportsFalse` is the genuine case named by [P6-T1]. Its passing establishes that a form with no registered popup owner reports false, which is what keeps the issue-677 deactivation contract in force for every deactivation that is not self-inflicted. Had the extraction inverted the polarity, this case would fail while the three positive cases still passed, so it is the one that pins the direction.

`Register_SameControlTwice_ReplacesRatherThanAppends` pins the replace-not-append behaviour twice over: the observable `AnyOpen` result is false, and the Moq verification records that the superseded predicate was never invoked after replacement. An appending registry would have reported true and invoked the superseded predicate.

## Together with [P6-T3]

[P6-T3] recorded the fail-before as the compiler's own report, six CS0246 diagnostics naming the type that did not yet exist. This run is the pass-after. A runtime-red run was structurally impossible for AC7 because the type under test did not exist, so the compile-red / green-run pair is the fail-before / pass-after evidence for this criterion.

## D13

No TRX content is reproduced here. The counters were parsed from the run's `ResultSummary/Counters` element and the outcome list from its `UnitTestResult` elements.
