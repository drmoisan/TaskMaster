# bayesian-email-sorter-unit-tests (Issue #248)

- Date captured: 2026-07-06
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/2026-07-06-bayesian-email-sorter-unit-tests-248/ (Issue #248)

- Issue: #248
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/248
- Last Updated: 2026-07-06
- Work Mode: minor-audit

## Problem / Why

`QuickFiler.Controllers.BayesianPerformanceController` and `QuickFiler.Controllers.EmailSorter`
currently lack direct unit coverage. This leaves controller UI-binding behavior, classifier
error selection behavior, and email sort-key behavior less protected than adjacent QuickFiler
controller code.

## Proposed Behavior

Add focused MSTest unit tests for both classes using the existing `QuickFiler.Test`
test conventions. Keep the production behavior unchanged unless a minimal testability
seam is required to exercise existing behavior deterministically.

## Acceptance Criteria

- [x] `EmailSorter` has deterministic unit tests for default/options construction, date key formatting, supported triage sort keys, and unsupported triage error behavior.
- [x] `BayesianPerformanceController` has deterministic unit tests for direct form value assignment and selection-change behavior that can run without Outlook or external services.
- [x] Tests use MSTest and FluentAssertions, follow the repository's existing C# test layout, and do not create temporary files.
- [x] The C# toolchain runs in the required order: CSharpier, analyzer build, nullable build, and MSTest with coverage.

## Constraints & Risks

- `BayesianPerformanceController` currently depends on WinForms viewer controls and Outlook interop paths; tests should cover isolated behavior and avoid live Outlook dependencies.
- Any production seam must be minimal, internal where possible, and limited to the two target classes or their immediate test surface.
- The work is scoped to unit tests and minimal testability changes only.

## Test Conditions to Consider

- [x] `EmailSorter.GetDateKey` returns a stable `yyyyMMddHHmmss` numeric key.
- [x] `EmailSorter.GetSortKey` orders triage classes under the supported option combination.
- [x] `EmailSorter.GetSortKey` propagates `KeyNotFoundException` for unsupported triage values.
- [x] `BayesianPerformanceController.AssignFormValues` maps classification metrics into viewer text fields and verbose outcome objects.
- [x] `BayesianPerformanceController` selection handlers update active state and clear or populate dependent viewer collections.

## Next Step

- [x] Generate the minor-audit plan of record.
- [x] Execute the approved plan and update acceptance criteria as verification completes.
- [ ] Complete reduced minor-audit review.
