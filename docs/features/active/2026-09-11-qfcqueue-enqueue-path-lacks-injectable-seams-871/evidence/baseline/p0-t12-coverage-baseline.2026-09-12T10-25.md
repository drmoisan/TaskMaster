# P0-T12 — Coverage baseline for the QuickFiler test assembly

**Status: acceptance met.** The blocker that stopped the previous attempt at this task is fixed
upstream and merged into this branch; this run reached post-processing and produced the document the
acceptance condition requires.

Timestamp: 2026-09-13T15-00
Command: pwsh -NoProfile -File .\scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot QuickFiler.Test -Configuration Debug -CoverageOutput docs\features\active\2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871\evidence\baseline\coverage-baseline.2026-09-12T10-25.cobertura.xml
EXIT_CODE: 1
ExpectedExitCode: 1
ThresholdAssertion: THREW

## Tokens CMD-COVERAGE printed

```
RUNNER-EXIT: 1
COVERAGE-ARTIFACT-WRITTEN
THRESHOLD-ASSERTION: THREW
```

RUNNER-TERMINATED: Cobertura line coverage 24.1477% is below the required 80% threshold.

The observed pair is `THREW` with exit code 1. The task text designates only two pairs a failure —
`THREW` with exit code 0, and `PASSED` with a non-zero exit code. This is neither. The runner
post-processed and wrote the document, then evaluated its own repository-wide document-level assertion,
which threw and set the non-zero exit code. Under the plan that assertion is a repository-wide gate and
is not this task's gate either way; this task's gate is the artifact content recorded below, and the
`ExpectedExitCode: 1` line above declares the expectation so the gate normalizes to a pass.

## Post-processed discriminator, checked explicitly

An artifact-exists check is not sufficient evidence that a measurement occurred. The runner writes the
raw document before it post-processes, so `COVERAGE-ARTIFACT-WRITTEN` can print over a raw document
produced by a run that terminated before any post-processing. The discriminating property is the form
of the class `filename` attributes: the post-processor rewrites them to backslash-separated
repository-relative paths, while the raw document carries absolute host paths.

```
DISCRIMINATOR: POST-PROCESSED
CLASS-COUNT: 535
QFCQUEUE-FILENAME-ATTRIBUTES:
QuickFiler\Controllers\QfcQueue.cs
QuickFiler\Controllers\QfcQueue.Enqueue.cs
PACKAGE-ELEMENTS: QuickFiler, UtilitiesCS, TaskVisualization, SVGControl, ToDoModel, Tags
ABSOLUTE-HOST-PATH-OCCURRENCES: 0
SOURCE-ELEMENT: .
```

Both required filename attributes occur in the backslash-separated repository-relative form the
acceptance condition names. Three further observations corroborate that the document is the
post-processed one and not the raw one. The class count is 535, where the previous failed attempt
recorded 3166 classes in its raw document. The package set is six first-party assemblies, where the raw
document carried eleven packages including log4net, Mono.Reflection, Microsoft.IO.RecyclableMemoryStream,
System.Linq.Async and System.Interactive. And the document contains no absolute host path at all: its
single `source` element is the relative token shown above, and a scan for a drive-letter path form
returns zero matches across all 12,140,123 bytes of the file.

## Document-level figures

DocumentLineRate: 0.241477
DocumentLinesCovered: 14938
DocumentLinesValid: 61861

These three are read directly from the coverage root element, which is the only element that carries
lines-covered and lines-valid as attributes.

The document-level rate is low because this run is scoped to a single test assembly by the plan's
`-SearchRoot QuickFiler.Test` while the denominator spans all six first-party packages. The five
packages that no test in this assembly exercises contribute their whole line count to the denominator
and almost nothing to the numerator. That is the measurement the plan asked for and the reason the plan
declines to predict which branch the runner's own assertion would take; the repository-wide figure is
handled by the projection in P6-T4 rather than by this number.

## QuickFiler package figures, from Get-CoberturaPackageLineSummary

PackageLinesCovered: 10215
PackageLinesValid: 12603
PackageLineRate: 0.810521

Obtained by dot-sourcing the coverage helpers file and the package-rate file under the vscode scripts
directory and calling `Get-CoberturaPackageLineSummary` on the package element whose name attribute is
QuickFiler. The helper-returned `LineRate` agrees exactly with the package element's own line-rate
attribute, which independently confirms the package element was recomputed by the post-processor.

## Per-file class figures, from Get-CoberturaClassLineSummary

For `QuickFiler/Controllers/QfcQueue.cs`, class element name QuickFiler.Controllers.QfcQueue:

QfcQueueLineRate: 0.496795
QfcQueueLinesCovered: 155
QfcQueueLinesValid: 312

For `QuickFiler/Controllers/QfcQueue.Enqueue.cs`, class element name QuickFiler.Controllers.QfcQueue:

QfcQueueEnqueueLineRate: 0.152941
QfcQueueEnqueueLinesCovered: 13
QfcQueueEnqueueLinesValid: 85

Each filename attribute resolves to exactly one class element in this document, so no cross-element
accumulation was required and no ambiguity arises about which element the figures came from.

### Divergence between the plan's property names and the helper's actual output

The acceptance condition asks for "the `LineRate`, `LinesCovered` and `LinesValid` from
`Get-CoberturaClassLineSummary`". That helper does not emit properties under those three names. Its
returned object carries `LineMap`, `TotalLines`, `CoveredLines`, `TotalBranches` and `CoveredBranches`,
and emits no rate property at all. The package-level helper does emit `LineRate`, `LinesCovered` and
`LinesValid` under exactly those names, so the divergence is confined to the class-level helper.

This artifact therefore records the semantically corresponding figures and states the mapping rather
than reporting a figure under a label its source does not use:

- `LinesCovered` above is the helper's `CoveredLines`.
- `LinesValid` above is the helper's `TotalLines`.
- `LineRate` above is read from the class element's own line-rate attribute, which the plan's evidence
  conventions section already states that class elements carry. It was cross-checked against the
  quotient of the helper's `CoveredLines` over its `TotalLines`, rounded to six places, and the two
  agree exactly for both files: 155 over 312 is 0.496795 and 13 over 85 is 0.152941.

The divergence is recorded here rather than resolved, and is reported to the caller. It also affects
P5-T5, P6-T1 and P6-T2, each of which cites the same three property names against the same class-level
helper.

## Test outcome under the coverage runner, and the fixed blocker

```
Test Run Successful.
Total tests: 1395
     Passed: 1395
 Total time: 14.1367 Seconds
```

All 1395 cases passed under the coverage runner, at `failed=0`. The three cases that failed under this
runner on the previous attempt at this task all pass now:

- `InitEmailQueue_ZeroBatchSize_ReturnsEmptyListWithoutThrowing` — PASSED
- `InitEmailQueue_ZeroBatchSize_StillStartsBackgroundWorker` — PASSED
- `InitEmailQueue_PositiveBatchSize_RetainsExistingProjectionAndFrameDrop` — PASSED

The fix is upstream, in the change merged into this branch as commit
8213826f695439e86e3ed34faa575de493a11ec7. It installs a process-wide `AssemblyResolve` fallback in the
QuickFiler test assembly's own assembly initializer through the new shared source file
TestSupport/TestAssemblyResolver.cs. No file in this item's Write Set was involved in the fix and this
item made no change to obtain it.

The superseded record in this artifact attributed the three failures to the class-level parallelism the
runner's own settings file imposes. That attribution was wrong and is withdrawn. The binding failed on a
reference to netstandard version 2.1.0.0 that enters through an FSharp.Core redirect, an assembly that
exists nowhere on this host and that no configuration file redirects; the bind was satisfiable only by a
process-global resolver that the QuickFiler test assembly did not install and had been borrowing by
accident from whichever earlier test class happened to touch the SVG renderer. Whether a run passed
therefore depended on class execution order, and parallelism changed that order without being the cause.

## Because the runner reached post-processing, the earlier mechanism no longer applies

The previous attempt terminated at the runner's throw for a non-zero inner test exit code, which
precedes the post-processing call, so the document on disk was raw. This run's inner test command
returned zero, so the runner proceeded through post-processing — its progress line
"Post-processing coverage XML for Koverage compatibility..." appears in the captured output immediately
before the assertion — and only then evaluated its document-level threshold. The throw observed here
originates in the threshold script rather than in the runner's inner-exit check, which is why the
document on disk is the post-processed one.

## Artifact hygiene

The Cobertura document is written to the path the plan states, as
coverage-baseline.2026-09-12T10-25.cobertura.xml in this directory, and is committed because this plan
names it as a required artifact of this task. Before staging it was scanned for host-identifying
strings: the host account name occurs zero times, the user-directory path prefix occurs zero times, and
no drive-letter path form occurs anywhere in the file. No redaction was necessary and none was applied,
so the document is committed byte-for-byte as the post-processor wrote it. No raw trx is committed; the
trx from this run remains under the ignored TestResults directory.

Output Summary: The coverage runner ran the QuickFiler test assembly to 1395 of 1395 passed with
`failed=0`, including the three cases that failed here before the upstream resolver fix was merged. It
post-processed and wrote the Cobertura document, then its own repository-wide document-level 80 percent
assertion threw at 24.1477 percent and set exit code 1; that pair is the declared expectation and is not
this task's gate. The document is confirmed post-processed by the discriminating check: both required
filename attributes appear in backslash-separated repository-relative form, the class count is 535
against 3166 in the earlier raw document, only six first-party packages are present, and the file
contains no absolute host path. Document-level rate 0.241477 over 14938 of 61861 lines; QuickFiler
package rate 0.810521 over 10215 of 12603 lines; QfcQueue.cs 0.496795 over 155 of 312; QfcQueue.Enqueue.cs
0.152941 over 13 of 85. One divergence is reported: the class-level helper emits `CoveredLines` and
`TotalLines` and no rate, not the three property names the acceptance condition asks for. Acceptance met.
