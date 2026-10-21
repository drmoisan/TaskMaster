# P0-T11 — Test baseline for the QuickFiler test assembly

Timestamp: 2026-09-13T14-57
ReAnchoredAt: 2026-09-13T14-57
ReAnchorReason: merge commit 8213826f brought origin/main into this branch after this baseline was
first captured. The merge added one new test method to the QuickFiler test assembly, so the case total
this baseline records — the figure P1-T7, P2-T9, P3-T11 and P4-T24 each compare against — is a property
of the post-merge tree rather than of the tree the superseded capture measured. The baseline is
therefore re-measured, and BASELINE_TEST_TOTAL is re-derived.
Command: vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation /TestCaseFilter:"TestCategory!=LiveOutlook" "/Logger:trx;LogFileName=vstest-run.trx" /ResultsDirectory:TestResults\p0-t11-reanchor
EXIT_CODE: 0

Test platform resolved through vswhere: Visual Studio 18 Community.

## Console tail, verbatim

```
Results File: <worktree-root>\TestResults\p0-t11-reanchor\vstest-run.trx

Test Run Successful.
Total tests: 1395
     Passed: 1395
 Total time: 12.4377 Seconds
VSTEST-EXIT: 0
```

## Counters element of the trx, the assertion target

Command: CMD-TRXCOUNTERS over the newest trx under the results directory for this task

```
total=1395 executed=1395 passed=1395 failed=0
```

TrxTotal: 1395
TrxExecuted: 1395
TrxPassed: 1395
TrxFailed: 0

BASELINE_TEST_TOTAL: 1395

The counters element rather than a console phrase is the assertion target, because a green run of this
runner prints no failed line and no skipped line at all, so an assertion phrased over console text
would have nothing to read on the passing case.

## Movement against the superseded capture, and its cause

SupersededBaselineTestTotal: 1394
BaselineTestTotal: 1395
Delta: +1

The delta is accounted for exactly. An anchored diff of QuickFiler.Test/Controllers/QfcHomeControllerTests.cs
between the superseded anchor and the merge commit shows a single added block of 71 lines carrying
exactly one `[TestMethod]`, named `Init_CreatesTokenSourceBeforeAnyLoaderObservesIt`, a regression test
for issue 839. One new test method yields one new case, so 1394 plus 1 is 1395. No case was removed and
no case was renamed.

Every later `total` comparison in this plan — P1-T7, P2-T9, P3-T11 and the arithmetic identity in
P4-T24 — reads 1395 and not 1394 from this point forward.

## Relationship to the resolver change the merge carried

The same merge installs a process-wide `AssemblyResolve` fallback in the QuickFiler test assembly's own
`[AssemblyInitialize]`, through the new shared TestSupport/TestAssemblyResolver.cs. That change is
invisible in this particular measurement, because this command passes no settings file and the assembly
was already fully green in that configuration before the merge. Its effect is measured by P0-T12, which
runs the same assembly through the coverage runner, the configuration in which three cases previously
failed.

## Notes on the command shape

The run names exactly one test assembly, as the catalogue requires. A whole-solution local run pulls in
four shell-icon test classes in another assembly that stall the runner on this host; that is an
environmental property of this machine rather than a regression, and the repository pipeline covers
those classes. This command passes no settings file, which is the same configuration the repository
pipeline uses.

The results directory lies under the repository TestResults directory, which is matched by the
repository ignore file. The trx itself is therefore never a candidate for staging, which is consistent
with the repository rule that only projections of a test run may be committed. The directory is named
for this task with a re-anchor suffix, so the superseded run's results remain distinguishable from this
one's.

Output Summary: 1395 of 1395 tests passed, 0 failed, runner exit code 0. BASELINE_TEST_TOTAL is
re-anchored to 1395, superseding 1394; the one-case delta is the single regression test method the merge
added to the QuickFiler home-controller test file. Both clauses of the acceptance condition are met: the
exit code is 0 and the trx counters element reports `failed=0`. The command was run while this item held
the shared build lock, which was released immediately after it returned.
