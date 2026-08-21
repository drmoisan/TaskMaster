# efc-home-controller-metrics-inert-duration (Issue #451)

- Date captured: 2026-08-07
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/efc-home-controller-metrics-inert-duration/ (Issue #451)
- Discovered by: research for [#437](https://github.com/drmoisan/TaskMaster/issues/437) (`quickfiler-efc-home-controller-coverage`, epic child F8 of [#136](https://github.com/drmoisan/TaskMaster/issues/136))
- Severity: Low-Medium (silent data-quality defect in a metrics CSV; no user-visible crash)

- Issue: #451
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/451
- Last Updated: 2026-08-08
## Summary

Six latent defects were verified by direct source inspection in the `EfcHomeController` metrics and
move-execution paths. None was fixed in #437 because the parent epic's NFR forbids behavior change
in a coverage-only child. They are recorded here so they survive as tracked work rather than as
prose inside a feature folder.

## Defect 1 — the QuickFile duration metric is permanently zero (primary)

`EfcHomeController._stopWatch` is constructed but **never started**:

- `QuickFiler/Controllers/EfcHomeController.cs` L76 — `_stopWatch = new Stopwatch();`
- `QuickFiler/Controllers/EfcHomeController.cs` L225 — `_stopWatch = new Stopwatch();`
- No `_stopWatch.Start()` call exists anywhere in the `EfcHomeController` family.

Contrast the sibling controller, which does start its stopwatch:

- `QuickFiler/Controllers/QfcHomeController.cs` L267-268 — `_stopWatch = new Stopwatch(); _stopWatch.Start();`

Consequently `QuickFiler/Controllers/EfcHomeController.Metrics.cs` L23 always evaluates
`_stopWatch.Elapsed.Seconds` as `0`, so every EFC-path metrics row records a duration of zero. The
metric is inert and any analysis built on it is measuring nothing.

**Verification:** `grep -rn "_stopWatch" QuickFiler/Controllers/` returns construction and read
sites for `EfcHomeController` but no `Start()`; the `QfcHomeController` sites show the intended
pattern.

## Defect 2 — `.Seconds` instead of `.TotalSeconds` truncates durations past one minute

`EfcHomeController.Metrics.cs` L23 passes `_stopWatch.Elapsed.Seconds`, which is the 0-59 second
*component* of the interval, not its total length. Any elapsed time of 1m05s is reported as 5
seconds. This is latent behind Defect 1 today but becomes active the moment the stopwatch is
started, so the two must be fixed together.

## Defect 3 — non-atomic check-then-set in `TryBeginExecuteMoves`

`QuickFiler/Controllers/EfcHomeController.ExecuteMoves.cs` guards re-entrancy with a `volatile`
field, but `volatile` only constrains memory ordering; it does not make the read-then-write atomic.
Two callers can both observe the "not executing" state and both proceed. `Interlocked.CompareExchange`
is the correct primitive.

## Defect 4 — missing CSV field separator between two columns

The metrics line interpolation omits a separator between `ToRecipientsName` and `SenderName`, so the
two values are emitted concatenated. This defect is currently **pinned by an existing assertion**
expecting the concatenated form `"RecipientSender"` at
`QuickFiler.Test/Controllers/EfcHomeControllerMetricsTests.cs` L59. Any fix must update that
assertion deliberately; #437 preserves it verbatim precisely because changing it would be a behavior
change.

## Defect 5 — incomplete CSV sanitization

`QfcCollectionController.xComma(...)` is applied only to `Subject` in
`EfcHomeController.Metrics.cs`, while three other interpolated fields
(`ToRecipientsName`, `SenderName`, and the folder value) are written unsanitized. A comma in any of
them corrupts the CSV row shape.

## Defect 6 — `QuickFileMetrics_WRITE(string filename)` throws `NotImplementedException`

`EfcHomeController.Metrics.cs` L26-29 declares a public single-argument overload whose entire body is
`throw new NotImplementedException();`. It is public API surface that cannot be called successfully.
Either implement it or remove it.

## Proposed Fix (outline)

1. Start the stopwatch where the EFC session begins, mirroring `QfcHomeController` L267-268.
2. Switch the duration read to `.TotalSeconds` and widen the parameter type accordingly.
3. Replace the `volatile` check-then-set with `Interlocked.CompareExchange`.
4. Add the missing CSV separator and update the pinning assertion in the same commit.
5. Apply `xComma` to every interpolated CSV field.
6. Implement or remove the `NotImplementedException` overload.

Items 1, 2 and 4 change the content of an emitted metrics file, so they need a deliberate decision
about backward compatibility with any existing downstream consumer of that CSV.

## Acceptance Criteria (early draft)

- [ ] The EFC QuickFile duration metric records a real non-zero elapsed time.
- [ ] Durations beyond one minute are reported in full, not truncated to the seconds component.
- [ ] Re-entrancy protection in `TryBeginExecuteMoves` is atomic.
- [ ] Every interpolated CSV field is comma-sanitized and correctly separated.
- [ ] No public method body remains a bare `NotImplementedException`.
- [ ] Regression tests cover each fix; the metrics-format change is asserted explicitly rather than
      pinned to the defective output.

## Constraints & Risks

- Changing emitted CSV content is a behavior change with potential downstream consumers.
- `EfcHomeControllerMetricsTests.cs` L59 currently asserts the defective concatenation and must be
  updated in the same change.
- Coverage work for these files is tracked separately under #437; this issue is behavior-only.

## Next Step

- [ ] Promote to GitHub issue (bug template)
- [ ] Create the active feature folder from the template
