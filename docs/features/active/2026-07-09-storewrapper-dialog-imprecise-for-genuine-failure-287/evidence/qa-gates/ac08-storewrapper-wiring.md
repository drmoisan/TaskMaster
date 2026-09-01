Timestamp: 2026-09-01T06-08
Command: (verification against recorded evidence, not a new command run)
EXIT_CODE: 0
Output Summary: All three names confirmed present in the P1-T6 failed set (evidence/regression-testing/fail-before-wiring-tests.md) and absent from the P3-T5 failed set (evidence/qa-gates/final-coverage-test-run.md, empty failed set):
- Launch_WhenStoresWrapperIsNull_ShowsUserMessageAndDoesNotThrowOrOpenViewer: failed in P1-T6, passed in P3-T5
- Launch_WhenStoresListIsNull_ShowsUserMessageAndDoesNotThrowOrOpenViewer: failed in P1-T6, passed in P3-T5
- Launch_ForModelUnavailableAndStoresUnavailable_ShowsDifferentMessages: failed in P1-T6, passed in P3-T5

All three behave as stated across the two runs. AC8 satisfied.
