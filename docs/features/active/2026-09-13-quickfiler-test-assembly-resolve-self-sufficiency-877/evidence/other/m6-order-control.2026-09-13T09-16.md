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
