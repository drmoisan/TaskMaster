# Phase 0 — Baseline Test Run (Issue #797)

Timestamp: 2026-09-07T09-19

Command: `pwsh -NoProfile -File coverage/plan797-helpers.ps1 -Mode Test -FilterName All -ResultsDirectory coverage/plan797-trx/baseline`

The helper resolves vstest.console.exe through vswhere and invokes it over the two explicitly named
test assemblies UtilitiesCS.Test/bin/Debug/UtilitiesCS.Test.dll and
TaskMaster.Test/bin/Debug/TaskMaster.Test.dll, never by directory discovery, with `/InIsolation`, the
off-root CLI runsettings, and a TRX logger writing under coverage/plan797-trx/baseline.

EXIT_CODE: 0

ExpectedExitCode: 0

## Output Summary

Counts read from the results file, not from console text:

- Total: 5237
- Passed: 5237
- Failed: 0
- Skipped: 0

The skipped count is derived from the results-file counters as total minus executed (5237 - 5237 = 0),
not from console text. A green run prints no `Skipped` line and the results file writes its
not-executed counter as zero, so a console-derived figure would be unreadable on exactly this run.

Exact filter expression used:

```text
TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests
```

Four shell-icon test classes are excluded from this run for environmental reasons unrelated to this
change: `HelperClasses.ShellUtilities_Tests`, `HelperClasses.ShellUtilitiesStatic_Tests`,
`HelperClasses.SysImageListHelperTests` and `EmailIntelligence.OSBrowser_Tests`. They stall
vstest on this workstation. CI covers them.

The results file itself is not committed; it is written to the git-ignored coverage directory because
a test results file carries `runUser` and `computerName` attributes. Only the sanitized counts above
are recorded here.

BASELINE-FAILING-TESTS: NONE

The run is green, so Phase 1 and Phase 5 subtract an empty set. Any test failing in a later run is
therefore attributable to this change unless it is separately identified.

Note on a known intermittent failure recorded by the caller: the test
`UtilitiesCS.Test.Extensions.DfDeedle_COM_Tests.GetEmailDataInViewAsync_SeparatesTableSnapshotFromDataFrameTransform`
is tracked as a nondeterministic timing race under issue 803 and is outside this item's Write Set. It
passed in this baseline run, so it is not a member of `BASELINE-FAILING-TESTS:`. If it fails in a
later run it is handled as a known flake: the scoped invocation is re-run once, both attempts are
recorded, and issue 803 is named.

BASELINE-CONTROLLER-SCOPE:

Derived from this same results file by selecting the four named test classes, so no second run was
performed.

- Total: 82
- Passed: 82
- Failed: 0
- Failing test names within this narrower scope: none.

Exact text of the narrower filter expression P1-T3 uses:

```text
(FullyQualifiedName~StoreWrapperController_Tests&TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests)|(FullyQualifiedName~StoreWrapperControllerTests&TestCategory!=LiveOutlook)|(FullyQualifiedName~StoreWrapperTests&TestCategory!=LiveOutlook)|(FullyQualifiedName~StoreWrapperViewerTests&TestCategory!=LiveOutlook)
```

Per rule R6 the `TestCategory!=LiveOutlook` clause is repeated on every disjunct because `&` binds
tighter than `|` in a vstest filter expression. The four shell-icon exclusion clauses are conjunctive
and therefore bind to the first disjunct alone; that is inert here because no shell-icon test name
matches any selector used in the scoped runs.

Output Summary: The baseline test run is green. 5237 total, 5237 passed, 0 failed, 0 skipped, exit
code 0. The narrower controller scope holds 82 tests, all passing. `BASELINE-FAILING-TESTS: NONE`.
