Timestamp: 2026-09-01T03-05
Command: pwsh -NoProfile -Command '& "scripts/vscode/Invoke-MSTest.ps1" -SearchRoot . -Configuration Debug *>&1 | Tee-Object -FilePath "coverage/p1-testrun.log"'
EXIT_CODE: 1
ExpectedExitCode: 1
Output Summary: Discovery line: "Discovered 9 test assemblies." Total tests: 6912 (= BASELINE TOTAL 6900 + 12 new [TestMethod] declarations, register entries 1-9, 12, 13, 14). Passed: 6907. Failed: 5. "Test Run Failed." was printed (expected: [expect-fail]). Failed-test lines (five, exactly matching register entries 10-14):
- Launch_WhenStoresWrapperIsNull_ShowsModelUnavailableCopyAndLeavesViewerNull
- Launch_WhenStoresListIsNull_ShowsStoresUnavailableCopyAndLeavesViewerNull
- Launch_WhenStoresWrapperIsNull_ShowsUserMessageAndDoesNotThrowOrOpenViewer
- Launch_WhenStoresListIsNull_ShowsUserMessageAndDoesNotThrowOrOpenViewer
- Launch_ForModelUnavailableAndStoresUnavailable_ShowsDifferentMessages

None of the nine unit tests numbered 1-9 in the test-name register appears in the failed set (confirmed by a zero-hit search for `^  Failed BuildUnavailable`). This is the expected reproduction of the defect: both call sites (StoreWrapperController.cs and DisabledStoresController.cs) still pass the pre-#287 literal message and title arguments to MyBox.ShowDialog, so the copy assertions added in P1-T3 and P1-T4 and the differing-message assertion in the new Launch test cannot hold until Phase 2 rewires the call sites.
