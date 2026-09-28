# P5-T7 — Whole-assembly test run, fourth step of the final QC loop

Timestamp: 2026-09-13T16-54
Command: vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook" "/Logger:trx;LogFileName=vstest-run.trx" /ResultsDirectory:TestResults\P5-T7
EXIT_CODE: 0
LoopPass: 1

The test platform executable was resolved through the Visual Studio locator to
Microsoft Visual Studio 18 Community, Common7, IDE, Extensions, TestPlatform, vstest.console.exe. The
command was run while this item held the shared cross-item build lock, which was released immediately
after it returned.

## Counters read from the trx ResultSummary element

```
trx=vstest-run.trx
total=1423 executed=1423 passed=1423 failed=0
```

TestTotal: 1423
TestExecuted: 1423
TestPassed: 1423
TestFailed: 0

The counters element is the assertion target rather than a console phrase, because a green run of this
runner prints no failed line and no skipped line at all, so a console search for a failure phrase
returns nothing on a green run and nothing on a run that was never executed.

## Console summary, verbatim

```
Test Run Successful.
Total tests: 1423
     Passed: 1423
 Total time: 12.6286 Seconds
```

## Movement against BASELINE_TEST_TOTAL

BaselineTestTotal: 1395
NewSuiteCases: 28
ExpectedTotal: 1423
ObservedTotal: 1423

BASELINE_TEST_TOTAL, recorded by P0-T11 against the post-merge anchor, is 1395. The regression suite
this item added contributes 28 cases, the figure P4-T24 established from the class-scoped count. The sum
is 1423 and the observed whole-assembly total is 1423, so the arithmetic identity holds and no case was
lost, duplicated or silently filtered by the final format pass.

## Failures outside this item's authorship

None. Every one of the 1423 cases passed. No test in any file outside this item's Write Set failed, so
there is no side effect from a concurrently running item to report.

## Scope of the run

The run names exactly one test assembly. A whole-solution local run on this host pulls in four
shell-icon test classes in another assembly that stall the runner; that is an environmental property of
this machine rather than a regression, and continuous integration covers those classes. The
`/InIsolation` switch is retained because the assembly-load behaviour of this test project differs
between an in-process and an isolated host. The `LiveOutlook` category filter excludes tests that
require a live Outlook process, which is a manual human gate and is not entered here; Outlook was
confirmed closed for the duration of this phase.

Output Summary: The whole QuickFiler test assembly ran to 1423 of 1423 passed with `failed=0` and exit
code 0, matching BASELINE_TEST_TOTAL of 1395 plus the 28 cases the new suite contributes. No test failed,
including every test this item did not author. This is the fourth and final step of loop pass 1.
Acceptance met.
