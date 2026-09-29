# QA Acceptance Criteria Status (P4-T21)

Timestamp: 2026-09-29T09-20
Command: pwsh -NoProfile -Command 'Set-Location "<repo-root>"; @(Select-String -LiteralPath docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/spec.md -SimpleMatch -Pattern "- [x] AC").Count'
EXIT_CODE: 0

### Acceptance Criteria Status
- Source: docs/features/active/2026-09-13-quickfiler-transactiongate-permit-leak-unexcluded-882/spec.md
- Total AC items: 12
- Checked off (delivered): 12
- Remaining (unchecked): 0
- Items remaining: none
- (Counts updated by P4-T23 at 2026-09-29T09-21, re-measured against spec.md: 12 lines `- [x] AC`, 0 lines `- [ ] AC`.)

Per-criterion status:
- AC1: MET (fixture-structure-gates.md: TransactionGate.WaitAsync() 0, TransactionGate.WaitAsync(bound) 1, bool acquired = await 1, if (!acquired) 1)
- AC2: MET (throw new TimeoutException( 1, TRANSACTIONGATE_ACQUIRE_TIMEOUT 1, bound.TotalMilliseconds 1; throw line 181 < acquisitions increment 188 < construction 189; the new test Passed in qa-coverage-test-run.md)
- AC3: MET (internal const int TransactionGateAcquireTimeoutMs = 120000; 1, TimeSpan bound 1, signature heads 2; contended increment 175 < wait 178; 181 < 188 < 189)
- AC4: MET (test-structure-gates.md: [Timeout(GateTimeoutMs)] 8, DoNotParallelize 0, TimeSpan.Zero 1, ThrowAsync<TimeoutException> 1, TRANSACTIONGATE_ACQUIRE_TIMEOUT 2, Thread.Sleep 0, Task.Delay 0, Stopwatch 0, [Retry 0; no .Install( call in the new method body, which begins at line 407 of the test file while the last .Install( call is at line 334; the new test Passed)
- AC5: MET (NotThrow<SemaphoreFullException> 1, UiThreadDispatcherFixture.TransactionReleases 2, roundTrip.Dispose(); 2; the new test Passed)
- AC6: MET (numstat 62 added, 0 deleted; the seven pre-existing names each 1 and each TEST-OUTCOME Passed; no other QuickFiler.Test/ path in FOOTPRINT)
- AC7: MET (project-file numstat empty; every FOOTPRINT path is a Write Set path or excluded agent-memory path; LINES-FT 458. The four qa-gates artifacts written at or after P4-T9 (qa-footprint-scope.md, this artifact, qa-hygiene-scan.md, qa-post-commit-verification.md) could not appear in the pre-commit union; P4-T25 re-checks the committed set.)
- AC8: MET (plan contains `contributes nothing to the duration of a passing run` 2 times and `Clause (ii):` 2 times)
- AC9: MET (exactly one dossier, fail-before-exception.2026-09-29T09-06.md, carrying WhyFailingRunImpossible:, ExpectedExitCode: 1 and EXIT_CODE: 1)
- AC10: MET (LOOP: CLEAN PASS in 1 iteration; csharpier check EXIT_CODE 0, DRIFT-FILES: NONE; analyzer rebuild 0 Warning(s) 0 Error(s); nullable rebuild 0 Warning(s) 0 Error(s); coverage run EXIT_CODE 0, TOTAL 1469 = BASELINE-TOTAL 1468 + 1, FAILED 0; three qa-gates projections present)
- AC11: MET (every FOOTPRINT path lies under QuickFiler.Test/ or the feature folder; agent-memory paths excluded and never staged)
- AC12: MET (qa-footprint-scope.md: no FOOTPRINT path ends in .trx, .coverage, .coveragexml, .cobertura.xml or .jacoco.xml, and none is named coverage.xml; qa-hygiene-scan.md: all five PATTERN-n-HITS 0 with POSITIVE-CONTROL 2941 and POSITIVE-CONTROL-SHAPE 2939)
