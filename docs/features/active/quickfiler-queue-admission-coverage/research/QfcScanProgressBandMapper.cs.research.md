# Research: `QuickFiler/Controllers/QfcScanProgressBandMapper.cs`

- Parent epic: #136 (`quickfiler-per-file-coverage`)
- Child feature: #431 F2 (`quickfiler-queue-admission-coverage`)
- File under research: `QuickFiler/Controllers/QfcScanProgressBandMapper.cs` (79 lines, verified by direct read)
- Evidence basis: direct read of the file; direct read of
  `QuickFiler.Test/Controllers/QfcScanProgressBandMapperTests.cs`; grep for `ExcludeFromCodeCoverage`
  across `QuickFiler/Controllers` (the only match in this file is inside an XML `<remarks>` comment,
  not an actual attribute); the #424 feature audit's independently re-verified coverage figure for
  this class (100% line and branch, `coverage-final.cobertura.xml`).

## Correction to the epic's file-level `[X]` marker

The epic (`docs/features/epics/quickfiler-per-file-coverage/epic.md`, F2 section) marks this file
`[X]`, implying it currently carries `[ExcludeFromCodeCoverage]`. **This is stale/incorrect as of the
current worktree.** A direct read and a targeted grep across every `.cs` file in
`QuickFiler/Controllers` confirm that `QfcScanProgressBandMapper.cs` contains no
`ExcludeFromCodeCoverage` attribute anywhere; the file's own doc comment merely *mentions* the phrase
`[ExcludeFromCodeCoverage]` in prose, describing why the type was extracted out of `QfcDatamodel`
(which does carry the attribute) — the grep match is a documentation string, not an attribute
application. This file was created by issue #424 specifically so this mapping logic would be testable
outside the excluded `QfcDatamodel`; it was already delivered with a 12-test suite and, per the #424
feature-review's independent re-computation from the committed Cobertura report, already sits at 100%
line and branch coverage.

## Current structure

- `internal sealed class QfcScanProgressBandMapper` — no public surface; internal collaborator between
  the confidence gate's progress callback and `QfcHomeController.RunAsync` (F7's file, not F2's).
- Constructor-injected: `Action<double, string> report` (validated non-null, `ArgumentNullException`
  otherwise) — this is the file's only dependency, already an injected delegate.
- One behavior method: `Report(int scanned, int accepted, int quantity)`. Pure arithmetic/clamping logic
  with one piece of instance state: `private double _lastValue` (monotonic floor across calls).
- No dependency on `Microsoft.Office.Interop.Outlook.*` at all. No I/O, no UI, no threads (per the
  file's own doc comment, which is accurate).
- No concurrency constructs — single-threaded, stateful-but-simple (the "never travel backwards" clamp
  against `_lastValue`).
- No wall-clock or RNG usage.

## Existing test coverage

`QfcScanProgressBandMapperTests.cs` (12 tests): `Constructor_NullReport_ThrowsArgumentNullException`,
`Constructor_NonNullReport_DoesNotThrow`, `Report_QuantityZero_MapsToZero`,
`Report_QuantityNegative_MapsToZero`, `Report_ZeroAccepted_MapsToZeroWithScanningLabel`,
`Report_MidBand_MapsProportionallyIntoTheBand`, `Report_AcceptedEqualsQuantity_MapsToBandCeiling`,
`Report_AcceptedExceedsQuantity_ClampsToBandCeiling`, `Report_NegativeAcceptedCount_ClampsToZero`,
`Report_WhenComputedValueWouldDecrease_HoldsThePreviousValue`,
`Report_AcrossARisingSequence_IsMonotonicAndStaysInsideTheBand`,
`Report_LabelFormat_CarriesScannedAndAcceptedCounts`.

This suite already exercises every branch in `Report`: the `quantity <= 0` guard, the two clamps (`>
BandCeiling`, `< 0`), the monotonic-hold clamp against `_lastValue`, the proportional mid-band
computation, both boundary values (`accepted == quantity`, `accepted > quantity`), a negative `accepted`
count, a full rising sequence for monotonicity, and the exact label format. Plus both constructor paths
(null-guard and success).

## Coverage gap

None identified by direct comparison of every branch in `Report`/the constructor against the 12 tests
above. This file is already at (or effectively at) the required >= 80% floor — the #424 audit's
independently re-verified figure of 100% line and branch coverage for this class is consistent with
this file-by-file read. No new test cases are required to close a gap; F2's plan should record this file
as **already compliant**, requiring no work beyond confirming the coverage figure holds under F1's
per-file harness once it exists.

## `[ExcludeFromCodeCoverage]` disposition

Not applicable — the attribute is not present on this file (see correction above). There is nothing to
remove and nothing to justify to F1's ledger; this file should not appear as an open exemption item in
F1's ledger at all.

## Seam requirements

None. The file's only dependency is already an injected delegate.

## Candidate test cases

None required. If the atomic-planner wants a defensive re-verification task (rather than a new test),
the appropriate action is a single task that runs F1's per-file coverage harness against this file and
records the numeric result as evidence under `evidence/qa-gates/`, not a new test-authoring task.

## Determinism constraints

Not applicable — no clock, RNG, or concurrency surface exists in this file.
