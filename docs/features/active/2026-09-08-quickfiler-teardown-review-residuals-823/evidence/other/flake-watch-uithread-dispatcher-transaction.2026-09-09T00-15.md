# Flake watch — Transaction_SecondCallerCannotInstallUntilTheFirstRestores

Timestamp: 2026-09-09T00-15

Issue: #823, entry R5. This is an append-only observation log. It records observed outcomes of one
intermittently failing test. It diagnoses nothing and prescribes nothing.

## The test under watch

- **Test:** `Transaction_SecondCallerCannotInstallUntilTheFirstRestores`
- **File:** `QuickFiler.Test/Controllers/QfcItemController.UiThreadDispatcherFixtureTests.cs`
- **Class:** `QfcItemController_UiThreadDispatcherFixtureTests`
- **Fully qualified name:**
  `QuickFiler.Controllers.Tests.QfcItemController_UiThreadDispatcherFixtureTests.Transaction_SecondCallerCannotInstallUntilTheFirstRestores`

The class doc in that file already records, in code, that this test is probabilistic by
construction. This log references that acknowledgement rather than restating it: read the class
summary at `:13-22` for the reason a broken implementation fails it only probabilistically, and the
paragraph at `:23-28` for the record that the fixture uses no sleep, no delay, no wall-clock wait
and no temporary file. Nothing in this log proposes changing any of that. No sleep, retry attribute
or timing tolerance may be introduced to stabilise the test.

## Per-row schema

Each observation row carries exactly these five columns:

SCHEMA: date | command | assembly set | parallelism setting | failure text

- **date** — the `Timestamp:` of the source evidence artifact, in `yyyy-MM-ddTHH-mm`.
- **command** — the runner invocation, abbreviated to its distinguishing arguments.
- **assembly set** — which test assemblies were in the run.
- **parallelism setting** — the `Parallelize` values in force, read from
  `scripts/vscode/TaskMaster.cli.runsettings`.
- **failure text** — the message the runner printed, or `PASSED`, or an explicit statement that the
  text was not captured.

## Observations

OBSERVATIONS: 4

### Row 1 — FAILURE

- **date:** 2026-09-08T09-22
- **command:** `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /Logger:trx /Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None /TestCaseFilter:TestCategory!=LiveOutlook`
- **assembly set:** `QuickFiler.Test` alone
- **parallelism setting:** Workers 0, Scope ClassLevel
- **failure text:** not captured. The source artifact records only the fully qualified test name in
  its `BASELINE-QFT-FAILED-SET` field and no runner message, so no failure text is available for
  this row. It is recorded as absent rather than reconstructed.
- **run totals:** 1380 tests, 1379 passed, 1 failed, exit code 1.

SOURCE: docs/features/active/2026-09-07-quickfiler-teardown-and-dropdown-residuals-810/evidence/baseline/p0-t11-quickfiler-tests.md

### Row 2 — PASS

- **date:** 2026-09-08T09-25
- **command:** `dotnet-coverage collect --output-format cobertura --settings coverage\810-effective-coverage.config -- vstest.console.exe <nine assemblies> /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /Logger:trx /Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None` with the four shell-icon exclusion clauses
- **assembly set:** the nine first-party test assemblies, `QuickFiler.Test` among them
- **parallelism setting:** Workers 0, Scope ClassLevel
- **failure text:** PASSED. The run executed 7153 tests with zero failures and exited 0.

SOURCE: docs/features/active/2026-09-07-quickfiler-teardown-and-dropdown-residuals-810/evidence/baseline/p0-t12-coverage.md

### Row 3 — PASS

- **date:** 2026-09-08T09-50
- **command:** `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /Logger:trx /Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None /TestCaseFilter:TestCategory!=LiveOutlook`
- **assembly set:** `QuickFiler.Test` alone
- **parallelism setting:** Workers 0, Scope ClassLevel
- **failure text:** PASSED. The run executed 1381 tests with zero failures and exited 0. This is the
  same command and the same assembly set as Row 1, on the same tree plus one added test, and the
  case that failed in Row 1 passed here.

SOURCE: docs/features/active/2026-09-07-quickfiler-teardown-and-dropdown-residuals-810/evidence/regression-testing/p1-t10-quickfiler-suite.md

### Row 4 — PASS

- **date:** 2026-09-08T10-23
- **command:** `dotnet-coverage collect --output-format cobertura --settings coverage\810-effective-coverage.config -- vstest.console.exe <nine assemblies> /Settings:scripts\vscode\TaskMaster.cli.runsettings /InIsolation /Logger:trx /Blame:CollectHangDump;TestTimeout=4min;HangDumpType=None` with the four shell-icon exclusion clauses
- **assembly set:** the nine first-party test assemblies, `QuickFiler.Test` among them
- **parallelism setting:** Workers 0, Scope ClassLevel
- **failure text:** PASSED. The run executed 7163 tests with zero failures and exited 0.

SOURCE: docs/features/active/2026-09-07-quickfiler-teardown-and-dropdown-residuals-810/evidence/qa-gates/p7-t5-tests-coverage.md

## Notes on the seeded set

The four rows above are the issue-810 run's complete observation set for this test: one failure and
three passes, on the same tree and on an adjacent unmodified tree. They are enumerated together in
that feature's `policy-audit.2026-09-08T20-15.md`. No correlation between the failure and any of
the five recorded columns is asserted here; four observations do not support one.

## How to append

Add a new `### Row N` section using the same five columns, with a `SOURCE:` line naming the
evidence artifact the row was read from, and raise the `OBSERVATIONS:` count. Do not edit or remove
an existing row: the log is append-only so that the sequence of outcomes stays readable.
