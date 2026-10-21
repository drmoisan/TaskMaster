# P1-T3 — Pure-move Relocation Is Behaviour-Preserving (Issue #797)

Timestamp: 2026-09-07T09-25

Commands:

1. `msbuild TaskMaster.sln /t:Build /m /p:Configuration=Debug "/p:Platform=Any CPU"` — exit 0, every
   project produced its output assembly, including UtilitiesCS, UtilitiesCS.Test and TaskMaster.Test.
2. `pwsh -NoProfile -File coverage/plan797-helpers.ps1 -Mode Test -FilterName ControllerScope -ResultsDirectory coverage/plan797-trx/p1-t3`

EXIT_CODE: 0

ExpectedExitCode: 0

The declared expectation equals the exit code this run actually produced. The exit code is 0, so the
alternative acceptance branch — exit code 1 with every failing test a member of the
`BASELINE-CONTROLLER-SCOPE:` failing set recorded in the P0-T9 artifact — is not entered. That failing
set is empty, so any failure at all would have been a gate failure attributable to the relocation.

## Counts, read from the results file

- Total: 82
- Passed: 82
- Failed: 0
- Skipped: 0 (total minus executed)

The passed count of 82 is at or above the passed count recorded under `BASELINE-CONTROLLER-SCOPE:` in
the P0-T9 artifact, which is also 82. Both figures were obtained with the identical filter expression
recorded there.

## Filter expression used

```text
(FullyQualifiedName~StoreWrapperController_Tests&TestCategory!=LiveOutlook&FullyQualifiedName!~HelperClasses.ShellUtilities_Tests&FullyQualifiedName!~HelperClasses.ShellUtilitiesStatic_Tests&FullyQualifiedName!~HelperClasses.SysImageListHelperTests&FullyQualifiedName!~EmailIntelligence.OSBrowser_Tests)|(FullyQualifiedName~StoreWrapperControllerTests&TestCategory!=LiveOutlook)|(FullyQualifiedName~StoreWrapperTests&TestCategory!=LiveOutlook)|(FullyQualifiedName~StoreWrapperViewerTests&TestCategory!=LiveOutlook)
```

The filter selects `StoreWrapperController_Tests`, `StoreWrapperControllerTests`, `StoreWrapperTests`
and `StoreWrapperViewerTests`, with the `TestCategory!=LiveOutlook` clause repeated on every disjunct
because `&` binds tighter than `|` in a vstest filter expression.

Four shell-icon test classes — `HelperClasses.ShellUtilities_Tests`,
`HelperClasses.ShellUtilitiesStatic_Tests`, `HelperClasses.SysImageListHelperTests` and
`EmailIntelligence.OSBrowser_Tests` — are excluded from every local run in this plan. CI covers them.
Their exclusion clauses bind to the first disjunct alone, which is inert here because no shell-icon
test name matches any selector in this expression.

Output Summary: The relocation of `PopulateWithCurrent`, `BindExcludeStoreCheckbox` and
`GetRelativeFsPath` into the new display partial is behaviour-preserving. The solution builds and the
82 tests in the controller and store scope all pass, matching the baseline exactly. No new test
existed at the time of this run.
