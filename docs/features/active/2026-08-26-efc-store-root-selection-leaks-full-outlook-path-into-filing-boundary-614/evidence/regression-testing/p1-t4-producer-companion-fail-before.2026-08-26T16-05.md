# P1-T4 — Producer companion regression test, fail-before evidence (#614, D1)

Timestamp: 2026-08-26T16-14

Command: `& $vstest QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /InIsolation "/TestCaseFilter:FullyQualifiedName~Issue614_SegmentActivate_StoreRootSegment_DoesNotStoreFullOutlookPath" "/Logger:trx;LogFileName=p1-t4.trx" "/ResultsDirectory:coverage\trx\p1-t4"`

(`$vstest` resolved via vswhere to the VS 18 Community `Common7\IDE\Extensions\TestPlatform\vstest.console.exe`.)

EXIT_CODE: 1

ExpectedExitCode: 1

## Output Summary

- Total tests: 1; Failed: 1; Passed: 0. `Test Run Failed.` Total time 1.5865 s.
- Failing test: `QuickFiler.Test.Controllers.BreadcrumbBridgeRouterTests.Issue614_SegmentActivate_StoreRootSegment_DoesNotStoreFullOutlookPath`.
- Recorded failure message (redaction-safe; the mailbox literal is the fabricated
  `mailbox@example.com` placeholder used by the test):

  `Expected _router.SelectedFolderPath not to be "\mailbox@example.com".`

- Interpretation: the D1 verbatim pass-through is observed. The test binds the router through
  the internal `BindRowsAsync(rows, scores, archiveRootPath, ct)` overload with
  `archiveRootPath = @"\mailbox@example.com\Archive"` and a provider ancestor chain whose
  segment 0 is the mailbox store root `@"\mailbox@example.com"`, then sends
  `{"type":"segmentActivate","rowId":"row-0","segmentIndex":0}`. Pre-fix,
  `BreadcrumbBridgeRouter.ToArchiveRelativePath` finds the activated full path is neither equal
  to nor under the bound archive root and returns it unchanged, so `SelectHierarchyPath` stores
  the full Outlook store-root path into `SelectedFolderPath`. The assertion therefore fails,
  which is the expected pre-fix outcome for this `[expect-fail]` task.
- Raw TRX (contains the machine account and host name) stays under the gitignored
  `coverage\trx\p1-t4\` tree and is not copied under `evidence/`.
- No pre-existing test was executed or affected by this scoped filter.
- Provenance note: an initial authoring pass wrote the store-root literals with a single
  leading separator. The literals were corrected to the plan-pinned double-separator store
  form and this command was re-run; the figures, exit code, and message quoted above are
  from that final re-run against the corrected test.
