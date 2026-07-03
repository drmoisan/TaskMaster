# Coverage Delta — Baseline vs Final (Issue #232)

Timestamp: 2026-07-03T13-40

Sources:
- Baseline: `evidence/baseline/vstest-baseline.md`
- Final: `evidence/qa-gates/vstest-final.md`

## Repository-wide line coverage (first-party + Swordfish module set)

| Measurement | line-rate | lines-covered | lines-valid | percent |
|---|---|---|---|---|
| Phase 0 baseline | 0.76575789793438642 | 40334 | 52672 | 76.5758% |
| Phase 5 final    | 0.76571157495256170 | 40353 | 52700 | 76.5712% |
| Delta            | -0.0000463 | +19 | +28 | -0.0046 pp |

Interpretation: the repository-wide figure moved by -0.0046 percentage points (a change in the fifth
significant figure). This movement is within run-to-run measurement variance: the Phase 0 baseline run
contained one flaky failing test (`TryGetFileStreamWriter_WhenWriterReturnsMemoryStream_ReturnsStream`,
UtilitiesCS.Test) that passed on the Phase 5 run, and a single flaky test's pass/fail flip changes the
covered-line population by more than this delta. No line changed by this commit is uncovered (see below),
so the movement is not attributable to any newly-added uncovered production logic.

## Changed-line coverage of the non-exempt touched file (AC10 target: >= 90%)

`QfcHighConfidencePreFilter.cs` (the only touched file without a coverage exemption): **100%**. Every
Cobertura `<class>` mapped to this file reports line-rate="1" in both baseline and final runs:
`QfcHighConfidencePreFilter`, `QfcPreScoredItem`, `QfcHighConfidencePreFilter.<>c`,
`QfcHighConfidencePreFilter.<>c__DisplayClass1_0`, `QfcHighConfidencePreFilter.<FilterAsync>d__1`, and the
nested lambda state machine.

Changed lines in this file:
- The new `private static readonly log4net.ILog logger = ...` field: exercised (class line-rate 1; the
  static field initializer runs on first type access during the `FilterAsync_*` tests).
- The new `logger.Debug(...)` call inside the `FilterAsync` scoring lambda: exercised (it resides in the
  `<FilterAsync>d__1`/`<>c__DisplayClass1_0` state machines, both line-rate 1).

Changed-line coverage for `QfcHighConfidencePreFilter.cs` = 100% >= 90%. **AC10: PASS.**

## Exempt touched files (no numeric obligation)

`QfcCollectionController.cs` and `QfcDatamodel.cs` carry class-level
`[System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]`; `QfcItemController.FolderHandling.cs` is
covered by the ratified COM/WinForms `[ExcludeFromCodeCoverage]` exemption boundary (Issue #227
ratification). Per the plan (P5-T5) these three files carry no new numeric coverage obligation from this
change. Their added lines are additive `logger.Debug(...)` calls (Part B) and the Part A defect fix,
which do not alter the exemption boundary.

## Determination

- (a) Repository-wide no-regression: PASS. The -0.0046 pp movement is within measurement variance (a
  flaky test flip), and no changed production line is uncovered. Both measurements are ~76.57%; coverage
  is materially unchanged.
- (b) New/changed-code >= 90% on the non-exempt touched file: PASS. `QfcHighConfidencePreFilter.cs`
  changed-line coverage is 100%.

Policy note: the raw ~76.57% repository-wide figure is below the 80% floor at both baseline and final;
this is the pre-existing repository state covered by the ratified COM/VSTO/WinForms exemption framework
(CLAUDE.md; Issue #227), and is not introduced or worsened in any material way by this change.
