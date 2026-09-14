# P5-T5 — Post-change coverage for the QuickFiler test assembly

Timestamp: 2026-09-13T16-51
Command: pwsh -NoProfile -File .\scripts\vscode\Invoke-MSTestWithCoverage.ps1 -SearchRoot QuickFiler.Test -Configuration Debug -CoverageOutput docs\features\active\2026-09-11-qfcqueue-enqueue-path-lacks-injectable-seams-871\evidence\qa-gates\coverage-postchange.2026-09-12T10-25.cobertura.xml
EXIT_CODE: 1
ExpectedExitCode: 1
ThresholdAssertion: THREW
LoopPass: 1

The command string is identical to the one P0-T12 ran apart from the output path, so the two
measurements are directly comparable. The command was run while this item held the shared cross-item
build lock, which was released immediately after it returned.

## Tokens CMD-COVERAGE printed

```
RUNNER-EXIT: 1
COVERAGE-ARTIFACT-WRITTEN
THRESHOLD-ASSERTION: THREW
```

RUNNER-TERMINATED: Cobertura line coverage 24.2744% is below the required 80% threshold.

The observed pair is `THREW` with exit code 1. The task text designates only two pairs a failure of this
task — `THREW` with exit code 0, and `PASSED` with a non-zero exit code. This is neither, and it is the
same pair P0-T12 observed. The runner post-processed and wrote the document, then evaluated its own
document-level assertion, which threw and set the non-zero exit code. That assertion is a
repository-wide gate applied to a denominator spanning all six first-party packages while this run's
numerator comes from a single test assembly; it is not this task's gate either way, and the
`ExpectedExitCode: 1` line declares the expectation so the gate normalizes to a pass. The
repository-wide figure is handled by the projection in P6-T4.

## Post-processed discriminator, checked explicitly

An artifact-exists check is not sufficient evidence that a measurement occurred. The runner writes the
raw Cobertura document at an earlier line than the post-processing call, so `COVERAGE-ARTIFACT-WRITTEN`
can print over a raw document produced by a run that terminated before any post-processing, and a
threshold assertion can then read as satisfied although nothing was measured. The discriminating
property is the form of the class `filename` attributes: the post-processor rewrites them to
backslash-separated repository-relative paths, while a raw document carries absolute host paths.

```
DISCRIMINATOR: POST-PROCESSED
CLASS-COUNT: 537
QFCQUEUE-FILENAME-ATTRIBUTES:
QuickFiler\Controllers\QfcQueue.cs
QuickFiler\Controllers\QfcQueue.Enqueue.cs
QuickFiler\Controllers\QfcQueue.Tlp.cs
QuickFiler\Controllers\QfcQueue.UiIdle.cs
PACKAGE-ELEMENTS: QuickFiler, UtilitiesCS, TaskVisualization, SVGControl, ToDoModel, Tags
ABSOLUTE-HOST-PATH-OCCURRENCES: 0
SOURCE-ELEMENT: .
DOCUMENT-BYTES: 12151131
```

All four filename attributes required by the acceptance condition occur in the backslash-separated
repository-relative form. Three further observations corroborate the post-processed reading. The class
count is 537, against 535 in the P0-T12 document and against 3166 in the raw document an earlier failed
run produced; the rise of two is exactly the two new production partial parts this item created that
carry executable statements. The package set is the same six first-party assemblies. And a scan of all
12,151,131 bytes for a drive-letter path form returns zero matches, with the document's single `source`
element carrying the relative token shown above.

## Document-level figures

DocumentLineRate: 0.242744
DocumentLinesCovered: 15022
DocumentLinesValid: 61884

These three are read directly from the coverage root element, which is the only element in a Cobertura
document that carries lines-covered and lines-valid as attributes.

Against the P0-T12 baseline of 0.241477 over 14938 of 61861, the covered count rose by 84 and the valid
count rose by 23.

## QuickFiler package figures, from Get-CoberturaPackageLineSummary

PackageLinesCovered: 10314
PackageLinesValid: 12626
PackageLineRate: 0.816886

Obtained by dot-sourcing the coverage helpers file under the vscode scripts directory and calling
`Get-CoberturaPackageLineSummary` on the package element whose name attribute is QuickFiler. The
helper-returned `LineRate` agrees exactly with that package element's own line-rate attribute, which
independently confirms the package element was recomputed by the post-processor. Against the P0-T12
baseline of 10215 covered over 12603 valid at 0.810521, the package gained 99 covered lines and 23 valid
lines.

## Property-name mapping for every class-level figure below

The acceptance condition asks for "the `LineRate`, `LinesCovered` and `LinesValid` from
`Get-CoberturaClassLineSummary`". That helper emits none of those three names. Its returned object
carries `LineMap`, `TotalLines`, `CoveredLines`, `TotalBranches` and `CoveredBranches`, and emits no rate
property at all; the count naming is inverted relative to the words the plan uses. The package-level
helper does emit `LineRate`, `LinesCovered` and `LinesValid` under exactly those names, so the divergence
is confined to the class-level helper and the package figures above are recorded under the helper's own
labels.

This artifact therefore records the semantically corresponding figures and states the mapping rather
than reporting a figure under a label its source does not use. The mapping is the one P0-T12 established
and is reproduced here in full so that a reviewer meets the same explanation at this site:

- `LinesCovered` below is the helper's `CoveredLines`.
- `LinesValid` below is the helper's `TotalLines`.
- `LineRate` below is read from the class element's own line-rate attribute, which the plan's evidence
  conventions section already states class elements carry.

Each rate was cross-checked by recomputing `CoveredLines` divided by `TotalLines` to six places and
comparing it with the attribute. The two agree exactly for all four files, as the recomputation column
below records. The divergence is recorded, not resolved; the plan is not edited.

## Per-file class figures, from Get-CoberturaClassLineSummary

Each of the four filename attributes resolves to exactly one class element in this document, so no
cross-element accumulation was required and no ambiguity arises about which element a figure came from.
The element-count observation is recorded per file below.

For `QuickFiler/Controllers/QfcQueue.cs`, class element name QuickFiler.Controllers.QfcQueue,
element count 1:

QfcQueueLineRate: 0.703226
QfcQueueLinesCovered: 109
QfcQueueLinesValid: 155
QfcQueueRecomputedRate: 0.703226
QfcQueueBranchesCovered: 32
QfcQueueBranchesValid: 48

For `QuickFiler/Controllers/QfcQueue.Enqueue.cs`, class element name QuickFiler.Controllers.QfcQueue,
element count 1:

QfcQueueEnqueueLineRate: 1
QfcQueueEnqueueLinesCovered: 85
QfcQueueEnqueueLinesValid: 85
QfcQueueEnqueueRecomputedRate: 1
QfcQueueEnqueueBranchesCovered: 16
QfcQueueEnqueueBranchesValid: 16

For `QuickFiler/Controllers/QfcQueue.Tlp.cs`, class element name QuickFiler.Controllers.QfcQueue,
element count 1:

QfcQueueTlpLineRate: 0.443709
QfcQueueTlpLinesCovered: 67
QfcQueueTlpLinesValid: 151
QfcQueueTlpRecomputedRate: 0.443709
QfcQueueTlpBranchesCovered: 23
QfcQueueTlpBranchesValid: 40

For `QuickFiler/Controllers/QfcQueue.UiIdle.cs`, class element name QuickFiler.Controllers.QfcQueue,
element count 1:

QfcQueueUiIdleLineRate: 0.172414
QfcQueueUiIdleLinesCovered: 5
QfcQueueUiIdleLinesValid: 29
QfcQueueUiIdleRecomputedRate: 0.172414
QfcQueueUiIdleBranchesCovered: 4
QfcQueueUiIdleBranchesValid: 4

## Test outcome under the coverage runner

```
Test Run Successful.
Total tests: 1423
     Passed: 1423
 Total time: 12.7791 Seconds
```

All 1423 cases passed at `failed=0`. That total is BASELINE_TEST_TOTAL of 1395 plus the 28 cases the new
regression suite contributes. No test failed, and no test outside this item's Write Set failed.

## Artifact hygiene

The Cobertura document was written to the path the plan states, as
coverage-postchange.2026-09-12T10-25.cobertura.xml in the qa-gates evidence directory, so that every
acceptance condition reading it is satisfiable. It is not staged and is not committed: repository
evidence-hygiene policy recorded under issue 671 forbids committing a raw Cobertura document. It is
deleted after P6-T4, its last consumer, and that deletion is recorded in the P6-T4 artifact. No raw trx
is committed either; the trx from this run remains under the ignored TestResults directory.

Output Summary: The coverage runner ran the QuickFiler test assembly to 1423 of 1423 passed with
`failed=0`, post-processed and wrote the Cobertura document, then its own repository-wide document-level
80 percent assertion threw at 24.2744 percent and set exit code 1; that pair is the declared expectation
and is not this task's gate. The document is confirmed post-processed: all four required filename
attributes appear in backslash-separated repository-relative form, the class count is 537 against 3166 in
the earlier raw document, only six first-party packages are present, and a scan of all 12,151,131 bytes
finds no absolute host path. Document rate 0.242744 over 15022 of 61884; QuickFiler package rate 0.816886
over 10314 of 12626; QfcQueue.cs 0.703226 over 109 of 155; QfcQueue.Enqueue.cs 1 over 85 of 85;
QfcQueue.Tlp.cs 0.443709 over 67 of 151; QfcQueue.UiIdle.cs 0.172414 over 5 of 29. The class-level helper
property-name mapping is reproduced in full and every rate was cross-checked against its attribute.
Acceptance met.
