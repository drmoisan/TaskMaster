# `2026-08-08-quickfiler-form-viewers-bayesian-coverage` — User Story

- Issue: #496
- Owner: drmoisan
- Status: Draft
- Last Updated: 2026-08-08

## Story Statement

- As the maintainer of the QuickFiler add-in, I want the small viewer classes
  (`QfcFormViewer`, `QfcItemViewerExpanded`, `BayesianPerformanceViewer`, `ToolStripMenuItemCb`) and
  the `BayesianPerformanceController` covered by deterministic unit tests, so that regressions in
  these files are caught before merge instead of discovered in a live Outlook session.
- As an autonomous coding agent working elsewhere in the QuickFiler codebase, I want every compiled
  file in `QuickFiler.csproj` to be either measured-and-passing or explicitly and correctly
  classified (ratified-exempt, measured-not-gated, or interface-only/not-measured), so that I can
  trust a green coverage gate as a real signal rather than a gate silently blinded by unratified
  `[ExcludeFromCodeCoverage]` attributes.
- As the epic #136 capstone reviewer (F16), I want F15's per-file evidence and generated-file
  classifications to be internally consistent with the epic's DEC-1/DEC-5 rulings, so that the
  capstone's repository-wide reconciliation does not have to re-litigate F15's dispositions.

## Problem / Why

Epic #136 exists because QuickFiler's per-file coverage is uneven and several files carry
`[ExcludeFromCodeCoverage]` attributes that were never checked against the repository's own
exemption grounds — a gate that looks green can be hiding untested logic. F15 is the child that
owns the leftover small viewers and the generated `Properties/` remainder: three files
(`BayesianPerformanceController.cs`, `BayesianPerformanceViewer.cs`, `ToolStripMenuItemCb.cs`) sit
below both coverage floors, and two files (`QfcFormViewer.cs`, `QfcItemViewerExpanded.cs`) carry an
attribute that pre-dates any ledger judgment. Without this feature, the epic's capstone cannot
close, because these five files and their seven generated companions would remain either
under-floor or ambiguously classified, and a future maintainer or agent touching
`BayesianPerformanceController.InvestigatePerformance()` or `QfcFormViewer`'s keyboard handling
would have no regression safety net.

## Personas & Scenarios

- Persona: QuickFiler maintainer (Dan Moisan / repo owner)
  - who the user is: the person accountable for QuickFiler's correctness and for deciding whether an
    `[ExcludeFromCodeCoverage]` attribute is a legitimate exemption or a testability gap left
    unaddressed.
  - what they care about: that coverage gates reflect real testable-vs-exempt status, that no
    observable Outlook/QuickFiler behavior changes as a side effect of a "just add tests" feature,
    and that frozen contracts (like `IQfcFormViewer`) consumed by other children are not disturbed.
  - their constraints: cannot run a full interactive Outlook session for every verification pass;
    relies on deterministic, off-screen unit tests and Cobertura evidence instead.
  - their goals and frustrations: wants the epic to finish without a long tail of unratified
    exemptions; is frustrated by silent-wrong-answer coverage tooling (e.g. the `line-rate` /
    `branch-rate` distortions documented elsewhere in the epic) that makes a passing gate untrustworthy.
  - their context and motivations: this is one of fifteen parallel children in epic #136; consistency
    with the epic's DEC-1 (unshown-STA-Form-construction), DEC-5 (`measured-not-gated` generated
    files), and `#457` (lambda-suppression) rulings matters more than any local shortcut.
- Persona: autonomous coding agent (future orchestrator/executor in this repository)
  - who the user is: a Claude-based agent later asked to modify `QuickFiler/Viewers/**` or
    `QuickFiler/Controllers/BayesianPerformanceController.cs`.
  - what they care about: a reliable regression harness so a change can be verified without human
    interaction; correct file classification so it does not waste effort trying to "fix" a
    `measured-not-gated` generated file's branch percentage.
  - their constraints: must not show a live Form, pump a message loop, or depend on Outlook Interop
    in a unit test; must follow the STA-last-resort clause exactly when a Form must be constructed.
  - their goals and frustrations: wants a clear, already-established seam (the `RunWithViewer`
    harness, the `ViewerShowAction`/`ViewerFactory` delegate pattern) to extend rather than a
    from-scratch testability investigation each time.
  - their context and motivations: consistency with epic-wide precedent (e.g. `EfcHomeController`'s
    `ViewerShowAction`) reduces the chance of introducing a second, divergent seam shape.
- Scenario: F16 capstone verification run
  - who is acting: the F16 capstone reviewer (agent or maintainer) closing out epic #136.
  - what triggered the action: all wave-1 children, including F15, have merged to the integration
    branch and F16 re-derives the coverage denominator from `QuickFiler.csproj`.
  - what steps do they take: F16 reads F15's evidence under `<FEATURE>/evidence/qa-gates/`, checks
    that `BayesianPerformanceController.cs`, `BayesianPerformanceViewer.cs`, and
    `ToolStripMenuItemCb.cs` clear 80%/75%, confirms both `[ExcludeFromCodeCoverage]` attributes in
    F15's set are gone, and confirms the seven generated files are correctly split between
    `measured-not-gated` and `interface-only / not-measured`.
  - what obstacles or decisions occur: if `AssemblyInfo.cs` were mis-recorded as
    `measured-not-gated` instead of `interface-only / not-measured`, F16 would expect a Cobertura
    `<class>` element that does not exist and would need to reconcile the discrepancy — this feature
    prevents that by recording the correct bucket up front.
  - what outcome do they expect: F15's row in the capstone's reconciliation table closes with no
    follow-up questions, and the two promoted latent-defect issues (the unawaited-task defect and the
    dead `QfcItemViewerExpanded` finding) are visible as tracked GitHub issues, not lost prose.

## Acceptance Criteria

- [ ] `Controllers/BayesianPerformanceController.cs`, `Viewers/BayesianPerformanceViewer.cs`, and
      `Viewers/ToolStripMenuItemCb.cs` each reach >= 80% line and >= 75% branch coverage.
- [ ] `Viewers/QfcFormViewer.cs` and `Viewers/QfcItemViewerExpanded.cs` have their
      `[ExcludeFromCodeCoverage]` attributes removed and are covered (line >= 80%; branch >= 75%
      where `branches-valid > 0`, N/A for `QfcItemViewerExpanded.cs` which has zero branch points).
- [ ] `IQfcFormViewer` gains no new, removed, or renamed member as a result of this work.
- [ ] The seven generated/`Properties/` files are classified correctly: six `measured-not-gated`,
      one (`AssemblyInfo.cs`) `interface-only / not-measured`.
- [ ] Repository-wide coverage is retained or improved on this branch (self-consistent
      before/after pair).
- [ ] The full C# toolchain (csharpier, analyzers, nullable, MSTest with coverage) passes cleanly.
- [ ] No observable QuickFiler behavior changes; the unawaited-task defect at
      `BayesianPerformanceController.cs:58` is left unmodified and promoted as a tracked GitHub
      issue rather than fixed inline.
- [ ] The dead `QfcItemViewerExpanded` finding is promoted as a tracked GitHub issue recommending a
      future, separately-scoped removal; it is not deleted in this feature.

## Non-Goals

- No end-user-visible change to QuickFiler's filing, keyboard-handling, or Bayesian-performance
  review flows.
- No deletion of `QfcItemViewerExpanded` (dead code) — recommended as a follow-up issue only.
- No fix for the `BayesianPerformanceController.cs:58` unawaited-task defect — promoted as a
  follow-up issue only.
- No change to `IQfcFormViewer` or any other frozen interface's member set.
- No repository-wide coverage threshold changes.
- No work on any other epic #136 child's file assignment (e.g. `EfcHomeController.cs` stays F8-owned
  even though this feature follows its `ViewerShowAction` precedent).
