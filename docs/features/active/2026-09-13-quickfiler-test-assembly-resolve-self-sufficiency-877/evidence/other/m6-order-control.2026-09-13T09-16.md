# M6 order control: zero-batch class plus an SVG-bearing class, no runsettings

Timestamp: 2026-09-13T09-16
Command: `vstest.console.exe QuickFiler.Test\bin\Debug\QuickFiler.Test.dll /TestCaseFilter:"FullyQualifiedName~QfcInitEmailQueueZeroBatchTests|FullyQualifiedName~BreadcrumbDropDownIntegrationTests" /InIsolation /ResultsDirectory:<scratch>/m7-pre /Logger:"trx;LogFileName=m7-pre.trx"`
EXIT_CODE: 1
ExpectedExitCode: 1
Output Summary: Test Run Failed. Total tests: 13, Passed: 10, Failed: 3. The three failures are the `QfcInitEmailQueueZeroBatchTests` tests, with the same `netstandard, Version=2.1.0.0` bind failure as M3. The zero-batch class executed first in this sequential run; the SVG-bearing `BreadcrumbDropDownIntegrationTests` class ran afterwards and passed. Head: c9590a8b7, before any fix.

## Significance

Both classes were in the same run and the same process. The outcome for the zero-batch class was
decided by which class the runner started first, not by which classes were present. With no runsettings
file, MSTest does not parallelize, so the classes ran sequentially and the zero-batch class was reached
before any SVG-bearing control had been constructed.

This is the intra-assembly ordering dependence in isolated form, and it is the same dependence that
makes the full-suite result unreliable as a repro.

## Findings recorded for the production-risk issue

The CLR caches a failed type initializer for the lifetime of the process. In this run the resolver was
installed later, by `BreadcrumbDropDownIntegrationTests`, and the already-failed `Deedle.Reflection`
initializer was not retried. That caching behaviour is the reason the production risk tracked as issue
879 would be permanent for the life of an affected Outlook session rather than transient.

## Prior-session observation, not the same run shape

- A prior session observed Total 9, 9 passed, 0 failed, with a zero exit.
- The headline row of this artifact records Total 13, 10 passed, 3 FAILED, with a non-zero exit.
- The totals differ, 9 against 13. The two runs therefore did not select the same set of tests, and they
  are NOT directly comparable.
- M6 is not a same-command flip. The two observations are two different run shapes, so no conclusion
  about run-to-run stability may be drawn from placing them side by side.
- The value of this artifact is unchanged by the prior-session observation: it remains the single-run
  demonstration that the zero-batch class fails when the runner schedules it first, even though an
  SVG-bearing class is present later in the same run.
- The demonstration that a suite-shaped run can produce different outcomes on identical input is carried
  by the M2 artifact at `evidence/baseline/m2-suite-before.2026-09-13T09-15.md`, not by this one, and
  that demonstration is on its own sufficient.
