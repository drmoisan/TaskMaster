# quickfiler-keyboard-actions-coverage (Issue #430)

- Date captured: 2026-08-07
- Author: Dan Moisan
- Status: Promoted -> docs/features/active/quickfiler-keyboard-actions-coverage/ (Issue #430)
- Parent epic issue: [#136](https://github.com/drmoisan/TaskMaster/issues/136)
- Epic: `quickfiler-per-file-coverage` (child F3, wave 1)
- Epic manifest: `docs/features/epics/quickfiler-per-file-coverage/epic.md`
- Integration branch: `epic/quickfiler-per-file-coverage-integration`
- Depends on: F1 `quickfiler-coverage-denominator-and-exemption-ledger` (wave 0)

- Issue: #430
- Issue URL: https://github.com/drmoisan/TaskMaster/issues/430
- Last Updated: 2026-08-08
- Work Mode: full-feature

## Problem / Why

Epic #136 requires every production `.cs` file compiled by `QuickFiler/QuickFiler.csproj` to reach at
least 80% line coverage or to sit on an explicitly ratified exemption ledger. This child owns the
QuickFiler keyboard-handling and mail-item-action cluster: 11 compiled files totalling roughly 1,025
lines.

The cluster's central file, `QuickFiler/Controllers/KeyboardHandler.cs` (414 lines), currently carries
`[ExcludeFromCodeCoverage]` and has no tests at all. Per the epic's ratified policy reconciliation
(Shared Design section 1), the CLAUDE.md COM/VSTO exemption qualifier "without an injectable seam" is
a live obligation rather than a standing permission: an `[ExcludeFromCodeCoverage]` attribute on a
*testable* seam is a Blocking finding. `KeyboardHandler.cs` must therefore be refactored behind seams
and covered, unless F1's ledger ratifies a specific irreducible remainder.

Separately, `.claude/rules/csharp.md` and `CLAUDE.md` § UT2 name `KbdActions<>` explicitly as a
testable seam within an otherwise COM-bound assembly that is **not** exempt and must meet the coverage
floor.

## Proposed Behavior

- Establish actual current per-file line coverage for all 11 in-scope files using F1's per-file
  coverage harness, and target the genuine gaps rather than duplicating the existing test files
  (`KaCharTests.cs`, `KaKeyTests.cs`, `KaStringAsyncTests.cs`, `KbdActionsTests.cs`,
  `KbdActionsRemainingBranchesTests.cs`, `QfcFormKeyHandlerTests.cs`, `MailItemActionsAdapterTests.cs`).
- Extract seams from `KeyboardHandler.cs` following the epic seam hierarchy (interface seam, then
  injectable delegate, then adapter) so its `[ExcludeFromCodeCoverage]` attribute can be removed and
  the file can reach the floor.
- Add MSTest/Moq/FluentAssertions unit tests in `QuickFiler.Test/`, mirroring the production tree,
  covering positive path plus invalid-input, boundary, and error-handling behavior per file.
- Record numeric per-file coverage evidence under the feature's `evidence/qa-gates/` folder.

## Acceptance Criteria (early draft)

- [ ] Every `testable` file in the F3 assignment reaches at least 80% line coverage, verified with
      F1's per-file harness and recorded as numeric evidence under `<FEATURE>/evidence/qa-gates/`.
- [ ] `KeyboardHandler.cs` has its `[ExcludeFromCodeCoverage]` removed and reaches the floor via seam
      extraction, unless F1's ledger ratifies a specific irreducible remainder.
- [ ] No production file in scope exceeds 500 lines.
- [ ] Tests use MSTest, Moq, and FluentAssertions; they are deterministic, isolated, and use no
      temporary files, external services, or live forms.
- [ ] Coverage per file spans the positive path plus invalid-input, boundary, and error-handling
      behavior.
- [ ] The full C# toolchain passes in final form: csharpier, analyzer build, nullable build,
      coverage-enabled vstest.
- [ ] No behavior change to observable QuickFiler keyboard flows.

## Constraints & Risks

- **Cross-child contract risk.** `KeyboardHandler.cs` seam extraction changes an internal contract
  consumed by the QuickFiler form and item controllers, which are owned by sibling children F6
  (`quickfiler-qfc-form-explorer-controller-coverage`) and F10
  (`quickfiler-item-controller-coverage`). The change must remain additive: introduce seams without
  altering the public call shape those controllers use. An unavoidable breaking change must be
  recorded in `spec.md` as a cross-child contract note rather than resolved by editing sibling files.
- **Event-driven surface.** Keyboard handling is event-driven. Tests must never construct live forms,
  never show popups, and never depend on the UI thread.
- **Determinism.** `Thread.Sleep`, `Task.Delay`, and real wall-clock waits are prohibited in tests;
  `KaStringAsync` requires a fake-timer or injected-clock approach.
- **Shared-file isolation.** This child must not modify `coverage.config` or any shared build property
  file; those are owned by F1 and the epic root.
- **Upstream dependency.** F1's ledger is the sole authority on whether any in-scope file is
  `ratified-exempt`, and F1's harness is the per-file coverage evidence mechanism. F1 merges to the
  integration branch before this child executes.

## Test Conditions to Consider

- [ ] Unit coverage areas: `KeyboardHandler` seam behavior, `KbdActions<>` dispatch/registration,
      `KaChar` / `KaKey` / `KaStringAsync` action implementations, `QfcFormKeyHandler` routing,
      `MailItemActionsAdapter` delegation.
- [ ] Invalid-input and boundary scenarios per file (null/empty keys, unregistered actions,
      out-of-range modifiers).
- [ ] Error-handling behavior for action invocation failures.
- [ ] Async ordering behavior for `KaStringAsync` under an injected clock / fake timer.

## Next Step

- [ ] Promote to GitHub issue (feature request template)
- [ ] Create `docs/features/active/quickfiler-keyboard-actions-coverage/` folder from the template
