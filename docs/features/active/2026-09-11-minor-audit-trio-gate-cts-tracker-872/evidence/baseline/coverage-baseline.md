# Phase 0 — Repository-Wide Coverage Baseline (FAILED, PRE-EXISTING DEFECT)

Timestamp: 2026-09-13T05-12
Task: [P0-T10]

Command: pwsh -File scripts/vscode/Invoke-MSTestWithCoverage.ps1 -SearchRoot . -Configuration Debug -CoverageOutput TestResults/coverage/coverage-baseline.cobertura.xml
EXIT_CODE: 1

Command: pwsh -Command '"TransientXml: " + (Test-Path -LiteralPath "TestResults/coverage/coverage-baseline.cobertura.xml" -PathType Leaf); "FeatureFolderXml: " + @(Get-ChildItem -LiteralPath "docs/features/active/2026-09-11-minor-audit-trio-gate-cts-tracker-872" -Recurse -File -Filter "coverage-baseline.cobertura.xml").Count'
EXIT_CODE: 0

Output Summary: the coverage runner FAILED. It ran 7221 tests, of which 7218 passed and 3 failed, printed
`Test Run Failed.`, and then threw from its own line 236 with the message
`MSTest with coverage failed with exit code 1`. Repository-wide raw figures read from the root coverage
element of the emitted document are a first-party line figure of 71.15 percent (59585 of 83751 lines) and
a first-party branch figure of 59.91 percent (14644 of 24445 branches). The runner's own
`First-party coverage: ` line and its terminating `Done. Coverage artifact: ` line were NOT printed,
because it threw before its post-processing step, so the two figure sets cannot be reconciled as the task
requires and the figures above come from the raw document alone.

This task's acceptance is NOT met. It demands `EXIT_CODE: 0` on the runner and the two printed first-party
lines; the observed exit code is 1 and neither line was printed. P0-T14 records this as the failing row.

## Numeric Values Read From The Root Coverage Element

LineRate: 0.7114541915917422
LinesCovered: 59585
LinesValid: 83751
BranchRate: 0.5990591122929024
BranchesCovered: 14644
BranchesValid: 24445
TestsPassed: 7218

Supporting counters from the run's printed totals: `Total tests: 7221`, `Passed: 7218`, `Failed: 3`. The
class-element count in the emitted document is 3298. No value above is a placeholder; each was read from
the emitted document or from the run's printed output.

## Second Span Assertions, Both Satisfied

```
TransientXml: True
FeatureFolderXml: 0
```

`TransientXml: True` establishes that the raw Cobertura document exists at the git-ignored transient path.
`FeatureFolderXml: 0` establishes that no file of that name exists anywhere under this feature folder, so
no raw coverage XML has entered the tracked tree. The second span is a filesystem name enumeration by the
PowerShell `-Filter` wildcard and involves no regular-expression engine.

## Cause: A Pre-Existing, Already-Diagnosed Tooling Defect

The three failures are all in one QuickFiler test class and are the defect D14 documents. The failing test
names, as printed:

```
Failed InitEmailQueue_ZeroBatchSize_ReturnsEmptyListWithoutThrowing
Failed InitEmailQueue_ZeroBatchSize_StillStartsBackgroundWorker
Failed InitEmailQueue_PositiveBatchSize_RetainsExistingProjectionAndFrameDrop
```

All three belong to `QuickFiler.Controllers.Tests.QfcInitEmailQueueZeroBatchTests` and all three fail with
the same exception, quoted from the captured run log:

```
System.TypeInitializationException: The type initializer for 'Deedle.Reflection' threw an exception.
 ---> System.TypeInitializationException: The type initializer for '<StartupCode$Deedle>.$FrameUtils' threw an exception.
 ---> System.IO.FileNotFoundException: Could not load file or assembly 'netstandard, Version=2.1.0.0, Culture=neutral, PublicKeyToken=cc7b13ffcd2ddd51' or one of its dependencies.
```

The failing frame is the test class's own two-row frame builder, reached from each test method.

This is the pre-existing defect recorded against the runner and restated in D14, not a regression
introduced by this delivery and not a property of the base tree's test code. The mechanism is already
diagnosed: the runner appends the MSTest runsettings file at its line 76 and resolves that path internally,
exposing no override parameter. That runsettings file's entire content is an MSTest Parallelize block with
ClassLevel scope and a worker count of zero, which runs test classes concurrently across every logical
processor. One race during concurrent class initialization poisons the Deedle reflection type, and the CLR
caches a failed static initializer for the process lifetime, so repeated runs reproduce it and the failure
reads as deterministic.

The same runner also hard-codes a LiveOutlook-only test filter at that line, so it cannot exclude the four
shell-icon test classes that stall vstest on this workstation. That is why its population of 7221 is wider
than the D5 per-assembly population and why D6 records the difference as intended.

The corroborating control is P0-T9: the identical QuickFiler assembly, run without that runsettings file,
passed 1394 of 1394 at exit 0 minutes earlier. The failure is therefore attributable to the runsettings
file the runner forces and to nothing in this delivery.

## What Was Deliberately Not Done

- `scripts/vscode/Invoke-MSTestWithCoverage.ps1` was not edited. It is outside the Write Set.
- `scripts/vscode/TaskMaster.cli.runsettings` was not edited. It is outside the Write Set.
- The task was not dropped, narrowed or substituted with a run over a narrower population.
- The defect was not re-diagnosed. It is already diagnosed and D14 records it.

The runner was invoked twice. The first invocation produced exit code 1 and the same throw, but its piped
output was discarded, so a second invocation captured the full output to the git-ignored transient log in
order to record which tests failed and with which exception. Capturing that output is what makes this
artifact a record of the observed state rather than a bare exit code; it is not a re-diagnosis and no
remedy was attempted. Both invocations produced the identical result.

## Consequence For The Emitted Document

Because the runner threw before its post-processing step, the document at the transient path is the raw
dotnet-coverage output rather than the post-processed form. P0-T11 reads that document, and its artifact
records the effect of the raw shape on the per-file derivation.

## Transient Output, Per D10

The raw Cobertura document and the captured run log both remain under the git-ignored results directory.
Neither is committed. Absolute host paths appear in both and none is transcribed into this artifact; the
test source is named repository-relatively as `QuickFiler.Test/Controllers/QfcInitEmailQueueZeroBatchTests.cs`.

## D7 Timing

D7 allows ten minutes before a stall is declared. Neither invocation stalled: the first ran from
2026-09-13T05:09:47 to 2026-09-13T05:10:55 and the second from 2026-09-13T05:11:25 to 2026-09-13T05:12:10,
so each terminated in about one minute. The failure is an explicit non-zero exit, not a non-termination.

## Build Lock

The cross-item build lock was held across each runner invocation only and released immediately after each
returned. The artifact reads and the second span ran outside the lock.
